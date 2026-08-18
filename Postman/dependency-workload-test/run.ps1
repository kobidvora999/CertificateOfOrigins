<#
.SYNOPSIS
  dependency-workload-test engine - prove every EXTERNAL dependency the module calls
  through its (REAL, non-mock) proxies is alive and functioning. Generic across
  CustomsCloud .NET 10 services. NO code coverage - pure liveness.

.DESCRIPTION
  Points the Postman CLI at an ALREADY-RUNNING service (pre-prod) whose proxies are REAL.
  Each external service is its OWN v3 collection folder under -CollectionsDir; this runner
  launches N parallel `postman collection run` processes (one per collection) so all
  dependencies are exercised concurrently. Aggregates per-collection exit codes; overall
  success = every dependency answered 2xx.

  This runner does NOT build or launch the service and does NOT enable real proxies -
  that is the infrastructure's job for the target environment.

.EXAMPLE
  ./run.ps1 -CollectionsDir C:/Repos/PreRulings/Postman/dependency-workload-test/collections `
            -BaseUrl https://preprod-host:443 `
            -Environment C:/Repos/PreRulings/Postman/dependency-workload-test/PreRulings-preprod.environment.yaml `
            -OutDir C:/Repos/PreRulings/Postman/dependency-workload-test/reports
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $CollectionsDir, # dir of per-service v3 collection folders
    [Parameter(Mandatory = $true)] [string] $BaseUrl,        # live pre-prod base URL (overrides collection var)
    [string]   $Environment = "",                            # optional -e environment file (.environment.yaml)
    [string]   $OutDir = "./reports",                        # where per-collection json + summary land
    [int]      $Parallel = 8,                                # max concurrent postman processes
    [string]   $Prefix = "",                                 # only run collections whose folder name starts with this
    [string[]] $Collections = @()                            # explicit collection-dir list; default = every folder under -CollectionsDir
)

$ErrorActionPreference = "Stop"
function Info($m) { Write-Host "[dep] $m" -ForegroundColor Cyan }
function Warn($m) { Write-Host "[dep] $m" -ForegroundColor Yellow }
function Die($m)  { Write-Host "[dep] $m" -ForegroundColor Red; exit 1 }

# ---- resolve paths -------------------------------------------------------
if (-not (Test-Path $CollectionsDir)) { Die "CollectionsDir not found: $CollectionsDir" }
$CollectionsDir = (Resolve-Path $CollectionsDir).Path
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
$OutDir = (Resolve-Path $OutDir).Path
# Resolve -Environment to an ABSOLUTE path too: it is passed to the parallel Start-Jobs, whose runspace CWD
# defaults to the user profile (~\Documents) — a relative env path there fails with a confusing ENOENT.
if ($Environment -ne "") {
    if (-not (Test-Path $Environment)) { Die "Environment file not found: $Environment" }
    $Environment = (Resolve-Path $Environment).Path
}

# ---- tooling (Postman CLI, not newman) -----------------------------------
$postmanCmd = Get-Command "postman" -ErrorAction SilentlyContinue
if (-not $postmanCmd) { Die "Postman CLI not on PATH. Install it: https://learning.postman.com/docs/postman-cli/postman-cli-installation/" }
$PostmanExe = $postmanCmd.Source
$PostmanDir = Split-Path $PostmanExe

# ---- enumerate collection folders (one per external service) -------------
# A v3 collection = a directory containing .resources/definition.yaml.
if ($Collections.Count -eq 0) {
    $Collections = @(Get-ChildItem -Path $CollectionsDir -Directory |
        Where-Object { Test-Path (Join-Path $_.FullName ".resources/definition.yaml") } |
        Where-Object { $Prefix -eq "" -or $_.Name.StartsWith($Prefix) } |
        ForEach-Object { $_.FullName })
}
if ($Collections.Count -eq 0) { Die "no v3 collections found under $CollectionsDir (prefix='$Prefix') - nothing to run" }
Info "dependencies to probe ($($Collections.Count)): $(($Collections | Split-Path -Leaf) -join ', ')"
Info "parallelism: $Parallel | base_URL: $BaseUrl"

# ---- launch N parallel `postman collection run` (one per collection) -----
function Safe($s) { ($s -replace '[^\w\.-]', '_') }
$runScript = {
    param($Col, $BaseUrl, $EnvFile, $Rep, $Out, $PostmanExe, $PostmanDir)
    $env:PATH = "$env:PATH;$PostmanDir"
    # v3 collections are classified "multi-protocol" by the Postman CLI, which only supports the 'cli' reporter.
    $a = @("collection", "run", $Col, "--env-var", "base_URL=$BaseUrl", "-r", "cli")
    if ($EnvFile -ne '') { $a += @("-e", $EnvFile) }
    & $PostmanExe @a *>$Out
    [pscustomobject]@{ Exit = $LASTEXITCODE }   # last output object carries the CLI's exit code
}
$jobs = @()
$throttle = [Math]::Max(1, $Parallel)
$queue = [System.Collections.Generic.Queue[string]]::new()
$Collections | ForEach-Object { $queue.Enqueue($_) }
$running = @{}

while ($queue.Count -gt 0 -or $running.Count -gt 0) {
    while ($queue.Count -gt 0 -and $running.Count -lt $throttle) {
        $col  = $queue.Dequeue()
        $name = Split-Path $col -Leaf
        $rep  = Join-Path $OutDir ("postman-{0}.json" -f (Safe $name))
        $out  = Join-Path $OutDir ("postman-{0}.out.log" -f (Safe $name))
        Info "-> probing '$name'"
        $job = Start-Job -ScriptBlock $runScript -ArgumentList $col,$BaseUrl,$Environment,$rep,$out,$PostmanExe,$PostmanDir
        $running[$job.Id] = @{ Dependency = $name; Job = $job }
    }
    Start-Sleep -Milliseconds 400
    foreach ($id in @($running.Keys)) {
        $job = $running[$id].Job
        if ($job.State -ne 'Running') {
            $dep = $running[$id].Dependency
            $rcv = @(Receive-Job $job -ErrorAction SilentlyContinue)
            Remove-Job $job -Force -ErrorAction SilentlyContinue
            $exitObj = $rcv | Where-Object { $_.PSObject.Properties.Name -contains 'Exit' } | Select-Object -Last 1
            $code = if ($exitObj) { [int]$exitObj.Exit } elseif ($job.State -eq 'Completed') { 0 } else { 1 }
            if ($code -eq 0) { Info "[OK] '$dep' alive" } else { Warn "[X] '$dep' FAILED (exit $code)" }
            $jobs += [pscustomobject]@{ Dependency = $dep; Exit = $code }
            $running.Remove($id)
        }
    }
}

# ---- aggregate -----------------------------------------------------------
$failed = @($jobs | Where-Object { $_.Exit -ne 0 })
$summary = Join-Path $OutDir "last-run.json"
$result = [pscustomobject]@{
    collectionsDir = $CollectionsDir
    baseUrl        = $BaseUrl
    total          = $jobs.Count
    passed         = ($jobs.Count - $failed.Count)
    failed         = $failed.Count
    failedNames    = @($failed.Dependency)
    dependencies   = $jobs
}
$result | ConvertTo-Json -Depth 6 | Set-Content -Path $summary -Encoding utf8

Info "----- dependency liveness -----"
$jobs | ForEach-Object { Write-Host ("  {0,-40} {1}" -f $_.Dependency, ($(if ($_.Exit -eq 0){'ALIVE'}else{'FAILED'}))) }
Info "summary: $summary  ($($result.passed)/$($result.total) alive)"

if ($failed.Count -gt 0) { Die "$($failed.Count) dependency probe(s) failed: $($failed.Dependency -join ', ')" }
Info "all dependencies alive OK"
exit 0
