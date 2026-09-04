<#
.SYNOPSIS
  Second coverage pass for the ONE branch that no request header can reach: the issue-by-worker fork in
  PublishAttachments.

.DESCRIPTION
  PublishAttachments forks on parametersUtil.Get<bool>("IssueCertificateOfOriginByWorker")
  (CertificateOfOriginsBl.cs:1313). That is a row in Infrastructure.Parameters - service-wide, not per-request -
  so it cannot be flipped by an x-mock-feature header or by anything in a request body, and it is False in every
  ordinary run. SendCertificateToIssueQueue therefore sat at 0/25.

  It also cannot simply be switched on for the whole suite: with the flag True, EVERY publish takes the queue
  path and the inline-template path (PrintCertificateOfOriginAndSaveAttachments) goes dark. So this is a second
  pass over one small collection, merged into the main cobertura afterwards.

  Steps: back up the parameter -> set True -> run.ps1 over the "CertificateOfOrigins Param IssueByWorker"
  collection under its own coverage session -> restore the parameter (ALWAYS, even on failure) -> merge this
  pass's cobertura into the main one.

  Run the ordinary suite FIRST (run.ps1), then this. Running this alone leaves you with a merged report that is
  missing everything else.

.EXAMPLE
  ./run.ps1 -Project ... -CollectionsDir ... -Prefix "CertificateOfOrigins Internal Workload -" -BaseUrl http://localhost:9034 -OutDir ./reports
  ./run-issue-by-worker.ps1
#>
[CmdletBinding()]
param(
    [string] $Project        = "$PSScriptRoot/../../API/CustomsCloud.CRM.CertificateOfOrigins.WebApi/CustomsCloud.CRM.CertificateOfOrigins.WebApi.csproj",
    [string] $CollectionsDir = "$PSScriptRoot/../postman/collections",
    [string] $Collection     = "CertificateOfOrigins Param IssueByWorker",
    [string] $BaseUrl        = "http://localhost:9034",
    [string] $OutDir         = "$PSScriptRoot/reports",
    [string] $ConnectionString = 'Data Source=localhost;Initial Catalog=CertificateOfOrigins;User Id=sa;Password=CustomsDev123!;Encrypt=False',
    [string] $ParameterName  = 'IssueCertificateOfOriginByWorker',
    # This pass's own cobertura + HTML go OUTSIDE the repo by default. Only the merged report is a deliverable,
    # and reportgenerator emits ~880 files per run - the .gitignore block only covers files sitting directly in
    # a reports/ folder, so a nested one would show up as 880 untracked files.
    [string] $PassDir        = (Join-Path ([System.IO.Path]::GetTempPath()) 'coo-issue-by-worker'),
    [switch] $NoBuild,
    [switch] $NoMerge
)

$ErrorActionPreference = "Stop"
function Info($m) { Write-Host "[ibw] $m" -ForegroundColor Cyan }
function Warn($m) { Write-Host "[ibw] $m" -ForegroundColor Yellow }
function Die($m)  { Write-Host "[ibw] $m" -ForegroundColor Red; exit 1 }

$passDir   = $PassDir
$mainCob   = Join-Path $OutDir "coverage.cobertura.xml"
$passCob   = Join-Path $passDir "coverage.cobertura.xml"
$collPath  = Join-Path $CollectionsDir $Collection
if (-not (Test-Path $collPath)) { Die "collection not found: $collPath" }

# ---- parameter helpers (System.Data.SqlClient: Microsoft.Data.SqlClient throws under PS 5.1) --------------
function Get-Param {
    $c = New-Object System.Data.SqlClient.SqlConnection($ConnectionString); $c.Open()
    try {
        $cmd = $c.CreateCommand()
        $cmd.CommandText = "SELECT [Value] FROM [Infrastructure].[Parameters] WHERE [Name] = @n"
        [void]$cmd.Parameters.AddWithValue("@n", $ParameterName)
        return $cmd.ExecuteScalar()
    } finally { $c.Close() }
}
function Set-Param($value) {
    $c = New-Object System.Data.SqlClient.SqlConnection($ConnectionString); $c.Open()
    try {
        $cmd = $c.CreateCommand()
        $cmd.CommandText = "UPDATE [Infrastructure].[Parameters] SET [Value] = @v, [UpdateDate] = GETDATE() WHERE [Name] = @n"
        [void]$cmd.Parameters.AddWithValue("@v", $value)
        [void]$cmd.Parameters.AddWithValue("@n", $ParameterName)
        $n = $cmd.ExecuteNonQuery()
        if ($n -ne 1) { throw "expected to update exactly 1 row for '$ParameterName', updated $n" }
    } finally { $c.Close() }
}

$original = Get-Param
if ($null -eq $original) {
    # A from-zero database that ran 'API_20260716 - add params.sql' BEFORE the trailing-tab fix has the row
    # under the name "IssueCertificateOfOriginByWorker<TAB>", which IParametersUtil never matches.
    Die "parameter '$ParameterName' not found. Check Infrastructure.Parameters for a name with a trailing tab."
}
Info "parameter '$ParameterName' is currently '$original'"

$exit = 0
try {
    Info "setting '$ParameterName' = True for this pass ..."
    Set-Param 'True'

    # Hashtable splat, not an array one. Two reasons: $args is a PowerShell automatic variable, and an ARRAY
    # splat binds like a command line - run.ps1's -Collections is [string[]], so it greedily swallowed the
    # following tokens and the BaseUrl ended up on ReadyTimeoutSec ("cannot convert http://localhost:9034 to
    # System.Int32"). A hashtable binds strictly by name.
    $runArgs = @{
        Project        = $Project
        CollectionsDir = $CollectionsDir
        Collections    = @($collPath)
        BaseUrl        = $BaseUrl
        OutDir         = $passDir
        SessionId      = 'ibw-cov'
        Parallel       = 1
    }
    if ($NoBuild) { $runArgs['NoBuild'] = $true }
    Info "running the issue-by-worker pass ..."
    & (Join-Path $PSScriptRoot 'run.ps1') @runArgs
    $exit = $LASTEXITCODE
}
finally {
    # Restore FIRST, report second: leaving the parameter True would silently change every later run.
    Info "restoring '$ParameterName' = '$original' ..."
    Set-Param $original
    $now = Get-Param
    if ($now -ne $original) { Warn "RESTORE FAILED - parameter is '$now', expected '$original'. Fix it before any other run." }
    else { Info "parameter restored." }
}

if (-not $NoMerge) {
    if ((Test-Path $mainCob) -and (Test-Path $passCob)) {
        $merged = Join-Path $OutDir "coverage.merged.cobertura.xml"
        Info "merging both passes -> $merged"
        dotnet-coverage merge $mainCob $passCob -o $merged -f cobertura | Out-Host
        Info "merged report: $merged  (the two passes separately: $mainCob , $passCob)"
    } else {
        Warn "nothing to merge - run run.ps1 first so $mainCob exists."
    }
}

exit $exit
