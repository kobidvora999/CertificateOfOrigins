<#
  Emits every "needs human review" marker as a REAL MSBuild warning.

  MSBuild parses any line printed in its canonical diagnostic format
  ("file(line): warning CODE: text") straight out of an Exec and turns it into a
  first-class warning - so these show up in the build log, in the IDE Error List,
  and in the warning count, exactly like a compiler warning.

  Why this exists: Sonar S1135 only sees `// TODO` in C#. Everything else that a
  developer must review before go-live - production values in .sql seeds, Planar
  .yml placeholders, CHANGE-ME hosts in Postman - produced no signal at all and
  was therefore easy to ship unreviewed.

  Codes (stable - do not renumber, they are referenced in the gap reports):
    REVIEW001  TODO(...blocking...)      - blocks Mock->real or go-live
    REVIEW002  TODO(confirm ...)         - value/route/enum needs confirming
    REVIEW003  TODO(infra ...)           - no source in infrastructure yet
    REVIEW004  TODO / FIXME / HACK / XXX - generic follow-up
    REVIEW005  placeholder value         - CHANGE-ME / production value / TBD
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $Root,
    [string] $Extensions = '.cs,.sql,.yml,.yaml,.props,.targets,.csproj',
    # comma-separated folder names to skip anywhere in the path
    [string] $Exclude = 'bin,obj,.git,.vs,node_modules,packages,reports,TestResults'
)

if (-not (Test-Path $Root)) { exit 0 }

$exts = $Extensions.Split(',')  | ForEach-Object { $_.Trim() } | Where-Object { $_ }
$skip = $Exclude.Split(',')     | ForEach-Object { $_.Trim() } | Where-Object { $_ }

# ordered: first match wins, so a TODO(blocking) is never downgraded to REVIEW004
$rules = @(
    @{ Code = 'REVIEW001'; Pattern = 'TODO\([^)]*blocking'                       },
    @{ Code = 'REVIEW002'; Pattern = 'TODO[\(:]?\s*confirm'                      },
    @{ Code = 'REVIEW003'; Pattern = 'TODO\(\s*infra'                            },
    @{ Code = 'REVIEW004'; Pattern = '(//|--|#)\s*(TODO|FIXME|HACK|XXX)\b|TODO\(' },
    @{ Code = 'REVIEW005'; Pattern = 'CHANGE-ME|CHANGEME|<TBD>|PRODUCTION VALUE|CONFIRM AGAINST' }
)

Get-ChildItem -Path $Root -Recurse -File -ErrorAction SilentlyContinue |
  Where-Object {
      if ($exts -notcontains $_.Extension.ToLower()) { return $false }
      foreach ($s in $skip) { if ($_.FullName -like "*\$s\*") { return $false } }
      return $true
  } |
  ForEach-Object {
      $file = $_.FullName
      $n = 0
      foreach ($line in [System.IO.File]::ReadLines($file)) {
          $n++
          foreach ($rule in $rules) {
              if ($line -match $rule.Pattern) {
                  # collapse to one line, strip comment noise, cap length
                  $msg = ($line -replace '[^ -~]', '' -replace '^\s*(//|--|#|/\*)\s*', '' -replace '\s+', ' ').Trim()
                  if ($msg.Length -gt 160) { $msg = $msg.Substring(0, 160) + '...' }
                  # canonical MSBuild diagnostic format -> becomes a real warning
                  Write-Output ("{0}({1}): warning {2}: {3}" -f $file, $n, $rule.Code, $msg)
                  break
              }
          }
      }
  }
exit 0
