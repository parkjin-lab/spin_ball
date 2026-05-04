[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportDirectory = "",
    [switch]$KeepGoing
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Invoke-AuditScript {
    param(
        [string]$Label,
        [string]$ScriptPath,
        [string]$ReportPath,
        [int]$MaxStage,
        [int]$MaxGrowthStage
    )

    Write-Host ""
    Write-Host "== $Label =="

    & powershell -NoProfile -ExecutionPolicy Bypass -File $ScriptPath `
        -MaxStage $MaxStage `
        -MaxGrowthStage $MaxGrowthStage `
        -ReportPath $ReportPath `
        -FailOnWarnings |
        ForEach-Object { Write-Host $_ }

    $exitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($exitCode -eq 0) {
        Write-Host "PASS: $Label"
    }
    else {
        Write-Host "FAIL: $Label exited with code $exitCode"
    }

    return $exitCode
}

$projectRoot = Resolve-ProjectRoot
if ([string]::IsNullOrWhiteSpace($ReportDirectory)) {
    $ReportDirectory = Join-Path $projectRoot "Logs"
}

New-Item -ItemType Directory -Path $ReportDirectory -Force | Out-Null

$audits = @(
    [pscustomobject]@{
        Label = "Scene essentials"
        ScriptPath = Join-Path $PSScriptRoot "AuditSceneEssentialsStatic.ps1"
        ReportPath = Join-Path $ReportDirectory "AlienCrusherSceneEssentialsStaticAudit.log"
    },
    [pscustomobject]@{
        Label = "Runtime map layout"
        ScriptPath = Join-Path $PSScriptRoot "AuditRuntimeMapLayoutStatic.ps1"
        ReportPath = Join-Path $ReportDirectory "AlienCrusherMapLayoutStaticAudit.log"
    },
    [pscustomobject]@{
        Label = "ROUTE HOLD tuning"
        ScriptPath = Join-Path $PSScriptRoot "AuditRouteHoldTuningStatic.ps1"
        ReportPath = Join-Path $ReportDirectory "AlienCrusherRouteHoldStaticAudit.log"
    }
)

$failed = 0
Write-Output "[AlienCrusher][StaticAudits] Running Unity-free audits"
Write-Output "Project: $projectRoot"
Write-Output "Reports: $ReportDirectory"

foreach ($audit in $audits) {
    if (-not (Test-Path $audit.ScriptPath)) {
        $failed++
        Write-Output "FAIL: Missing audit script $($audit.ScriptPath)"
        if (-not $KeepGoing) {
            break
        }
        continue
    }

    $exitCode = Invoke-AuditScript `
        -Label $audit.Label `
        -ScriptPath $audit.ScriptPath `
        -ReportPath $audit.ReportPath `
        -MaxStage $MaxStage `
        -MaxGrowthStage $MaxGrowthStage

    if ($exitCode -ne 0) {
        $failed++
        if (-not $KeepGoing) {
            break
        }
    }
}

if ($failed -gt 0) {
    Write-Output ""
    Write-Output "Result: $failed audit(s) failed"
    exit 1
}

Write-Output ""
Write-Output "Result: all static audits passed"
exit 0
