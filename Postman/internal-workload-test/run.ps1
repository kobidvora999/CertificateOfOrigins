<#
.SYNOPSIS
  internal-workload-test engine - run the internal-workload v3 collections against a live .NET service
  under a SINGLE dotnet-coverage session, with N `postman collection run` processes in PARALLEL (one per
  collection folder), then emit a cobertura report. Generic across CustomsCloud .NET 10 services.

.DESCRIPTION
  1. Ensures tooling (dotnet-coverage, reportgenerator, Postman CLI).
  2. Builds the WebApi (Debug, full symbols).
  3. Launches the built dll under ONE named dotnet-coverage session (background).
  4. Waits for the HTTP port.
  5. Runs the Postman CLI in PARALLEL - one process per v3 collection folder under -CollectionsDir (filtered
     by -Prefix), each writing its own postman-<name>.json. All hit the same live API; the single coverage
     session captures everything.
  6. Gracefully shuts the session down so coverage flushes to cobertura.
  7. Generates an HTML + TextSummary report and a merged last-run.json verdict.

  Extends the postman-coverage engine with parallel collection execution. Runs in the TEST environment.

.EXAMPLE
  ./run.ps1 -Project C:/Repos/PreRulings/API/CustomsCloud.CRM.PreRulings.WebApi/CustomsCloud.CRM.PreRulings.WebApi.csproj `
            -CollectionsDir C:/Repos/PreRulings/Postman/postman/collections -Prefix "PreRulings Internal Workload -" `
            -BaseUrl http://localhost:60942 -OutDir C:/Repos/PreRulings/Postman/internal-workload-test/reports
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Project,        # path to WebApi .csproj
    [Parameter(Mandatory = $true)] [string] $CollectionsDir, # dir of v3 collection folders (the pipeline workspace)
    [string]   $Prefix = "",                                 # only run collections whose folder name starts with this
    [string]   $Environment = "",                            # optional -e environment file (.environment.yaml)
    [string]   $BaseUrl = "http://localhost:60942",          # live service base URL (overrides collection var)
    [string]   $OutDir = "./reports",                        # where cobertura + html + per-collection json land
    [string]   $SessionId = "iwt-cov",                       # dotnet-coverage session id
    [int]      $ReadyTimeoutSec = 90,
    [int]      $Parallel = 6,                                 # max concurrent postman processes
    [string]   $Configuration = "Debug",
    [string[]] $Include = @(),
    [string[]] $Collections = @(),                           # explicit collection-dir list; default = every folder under -CollectionsDir
    [switch]   $NoBuild
)

$ErrorActionPreference = "Stop"
function Info($m) { Write-Host "[iwt] $m" -ForegroundColor Cyan }
function Warn($m) { Write-Host "[iwt] $m" -ForegroundColor Yellow }
function Die($m)  { Write-Host "[iwt] $m" -ForegroundColor Red; exit 1 }
function Safe($s) { ($s -replace '[^\w\.-]', '_') }

# ---- resolve paths -------------------------------------------------------
$Project = (Resolve-Path $Project).Path
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
$Cobertura = Join-Path $OutDir "coverage.cobertura.xml"
$port = ([Uri]$BaseUrl).Port

# ---- 1. tooling (Postman CLI, not newman) --------------------------------
function Ensure-DotnetTool($pkg, $cmd) {
    if (Get-Command $cmd -ErrorAction SilentlyContinue) { return }
    Info "installing global tool $pkg ..."
    dotnet tool install --global $pkg | Out-Host
    $env:PATH = "$env:PATH;$HOME\.dotnet\tools"
}
Ensure-DotnetTool "dotnet-coverage" "dotnet-coverage"
Ensure-DotnetTool "dotnet-reportgenerator-globaltool" "reportgenerator"
$postmanCmd = Get-Command "postman" -ErrorAction SilentlyContinue
if (-not $postmanCmd) { Die "Postman CLI not on PATH. Install: https://learning.postman.com/docs/postman-cli/postman-cli-installation/" }
$PostmanExe = $postmanCmd.Source; $PostmanDir = Split-Path $PostmanExe
foreach ($c in @("dotnet-coverage","reportgenerator")) {
    if (-not (Get-Command $c -ErrorAction SilentlyContinue)) { Die "required tool '$c' not on PATH" }
}

# ---- idempotency ---------------------------------------------------------
try { dotnet-coverage shutdown $SessionId 2>$null | Out-Null } catch {}

# ---- 2. build ------------------------------------------------------------
if (-not $NoBuild) {
    Info "building $Project ($Configuration) ..."
    dotnet build $Project -c $Configuration | Out-Host
    if ($LASTEXITCODE -ne 0) { Die "build failed" }
}
$projDir = Split-Path $Project -Parent
$asmName = [System.IO.Path]::GetFileNameWithoutExtension($Project)
$dll = Get-ChildItem -Path (Join-Path $projDir "bin\$Configuration") -Recurse -Filter "$asmName.dll" -ErrorAction SilentlyContinue |
       Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $dll) { Die "could not locate built dll for $asmName under bin\$Configuration" }
Info "entry dll: $($dll.FullName)"

# ---- 3. launch under coverage (background) -------------------------------
$includeArgs = @(); foreach ($i in $Include) { $includeArgs += @("--include-files", $i) }
$collectArgs = @("collect","--session-id",$SessionId,"--output",$Cobertura,"--output-format","cobertura") +
               $includeArgs + @("--","dotnet",$dll.FullName)
Info "starting instrumented service (session=$SessionId) ..."
$svcOut = Join-Path $OutDir "service.out.log"; $svcErr = Join-Path $OutDir "service.err.log"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = $BaseUrl
$collector = Start-Process -FilePath "dotnet-coverage" -ArgumentList $collectArgs `
                 -RedirectStandardOutput $svcOut -RedirectStandardError $svcErr -PassThru -NoNewWindow

# ---- 4. wait for readiness ----------------------------------------------
Info "waiting for $BaseUrl (timeout ${ReadyTimeoutSec}s) ..."
$ready = $false; $deadline = (Get-Date).AddSeconds($ReadyTimeoutSec)
while ((Get-Date) -lt $deadline) {
    if ($collector.HasExited) {
        Warn "collector exited early (code $($collector.ExitCode)). Service log tail:"
        if (Test-Path $svcErr) { Get-Content $svcErr -Tail 40 | Out-Host }
        Die "service failed to start under coverage"
    }
    try { $tcp = New-Object System.Net.Sockets.TcpClient; $tcp.Connect("localhost",$port)
          if ($tcp.Connected) { $ready = $true; $tcp.Close(); break } } catch { Start-Sleep -Milliseconds 800 }
}
if (-not $ready) { try { dotnet-coverage shutdown $SessionId | Out-Null } catch {}; Die "service not ready within ${ReadyTimeoutSec}s" }
Info "service is up."

# ---- 5. run Postman CLI in PARALLEL (one process per v3 collection folder) -
# A v3 collection = a directory containing .resources/definition.yaml.
if ($Collections.Count -eq 0) {
    $Collections = @(Get-ChildItem -Path $CollectionsDir -Directory |
        Where-Object { Test-Path (Join-Path $_.FullName ".resources/definition.yaml") } |
        Where-Object { $Prefix -eq "" -or $_.Name.StartsWith($Prefix) } |
        ForEach-Object { $_.FullName })
}
if ($Collections.Count -eq 0) { try { dotnet-coverage shutdown $SessionId | Out-Null } catch {}; Die "no v3 collections under $CollectionsDir (prefix='$Prefix')" }
Info "scenario collections ($($Collections.Count)), parallelism $Parallel : $(($Collections | Split-Path -Leaf) -join ', ')"

# Parallel `postman collection run` via Start-Job (handles spaces in folder names; sets PATH for the exe).
$runScript = {
    param($Col, $BaseUrl, $EnvFile, $Rep, $Out, $PostmanExe, $PostmanDir)
    $env:PATH = "$env:PATH;$PostmanDir"
    # NOTE: migrated v3 collections are classified "multi-protocol" by the Postman CLI, which only supports the
    # 'cli' reporter (the 'json' reporter aborts the run). Verdict/last-run.json is built from exit codes, so cli-only is fine.
    $a = @("collection", "run", $Col, "--env-var", "base_URL=$BaseUrl", "-r", "cli")
    if ($EnvFile -ne '') { $a += @("-e", $EnvFile) }
    & $PostmanExe @a *>$Out
    [pscustomobject]@{ Exit = $LASTEXITCODE }   # last output object carries the CLI's exit code
}
$throttle = [Math]::Max(1, $Parallel)
$queue = [System.Collections.Generic.Queue[string]]::new()
$Collections | ForEach-Object { $queue.Enqueue($_) }
$running = @{}; $results = @()
while ($queue.Count -gt 0 -or $running.Count -gt 0) {
    while ($queue.Count -gt 0 -and $running.Count -lt $throttle) {
        $col   = $queue.Dequeue()
        $label = Split-Path $col -Leaf
        $rep = Join-Path $OutDir ("postman-{0}.json" -f (Safe $label))
        $out = Join-Path $OutDir ("postman-{0}.out.log" -f (Safe $label))
        Info "-> '$label'"
        $job = Start-Job -ScriptBlock $runScript -ArgumentList $col,$BaseUrl,$Environment,$rep,$out,$PostmanExe,$PostmanDir
        $running[$job.Id] = @{ Label = $label; Job = $job }
    }
    Start-Sleep -Milliseconds 400
    foreach ($id in @($running.Keys)) {
        $job = $running[$id].Job
        if ($job.State -ne 'Running') {
            $label = $running[$id].Label
            $rcv = @(Receive-Job $job -ErrorAction SilentlyContinue)
            Remove-Job $job -Force -ErrorAction SilentlyContinue
            $exitObj = $rcv | Where-Object { $_.PSObject.Properties.Name -contains 'Exit' } | Select-Object -Last 1
            $code = if ($exitObj) { [int]$exitObj.Exit } elseif ($job.State -eq 'Completed') { 0 } else { 1 }
            if ($code -eq 0) { Info "[OK] '$label'" } else { Warn "[X] '$label' (exit $code)" }
            $results += [pscustomobject]@{ Group = $label; Exit = $code }
            $running.Remove($id)
        }
    }
}
$runFailed = @($results | Where-Object { $_.Exit -ne 0 })

# ---- 6. graceful shutdown (flush coverage) -------------------------------
Info "shutting down session $SessionId ..."
dotnet-coverage shutdown $SessionId | Out-Host
$null = $collector.WaitForExit(30000)
if (-not $collector.HasExited) { Warn "collector still running; killing"; $collector.Kill() }
if (-not (Test-Path $Cobertura)) { Die "cobertura report was not produced at $Cobertura" }

# ---- 7. report -----------------------------------------------------------
Info "generating coverage report ..."
reportgenerator -reports:$Cobertura -targetdir:$OutDir "-reporttypes:Html;TextSummary" | Out-Host
$summary = Join-Path $OutDir "Summary.txt"
if (Test-Path $summary) { Info "----- coverage summary -----"; Get-Content $summary | Out-Host }

$verdict = [pscustomobject]@{
    collectionsDir = $CollectionsDir
    baseUrl      = $BaseUrl
    groups       = $results.Count
    passed       = ($results.Count - $runFailed.Count)
    failed       = $runFailed.Count
    failedGroups = @($runFailed.Group)
    cobertura    = $Cobertura
}
$verdict | ConvertTo-Json -Depth 6 | Set-Content -Path (Join-Path $OutDir "last-run.json") -Encoding utf8

Info "cobertura : $Cobertura"
Info "html      : $(Join-Path $OutDir 'index.html')"
Info "verdict   : $(Join-Path $OutDir 'last-run.json')  ($($verdict.passed)/$($verdict.groups) groups passed)"
if ($runFailed.Count -gt 0) { Die "$($runFailed.Count) scenario group(s) failed: $($runFailed.Group -join ', ')" }
exit 0
