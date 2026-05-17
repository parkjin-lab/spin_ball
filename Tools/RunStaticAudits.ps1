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
        [int]$MaxGrowthStage,
        [string]$PowerShellExecutable
    )

    Write-Host ""
    Write-Host "== $Label =="

    $startedAt = Get-Date
    & $PowerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $ScriptPath `
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

    if ($exitCode -ne 0) {
        return $exitCode
    }

    if (-not (Test-Path -Path $ReportPath -PathType Leaf)) {
        Write-Host "FAIL: $Label did not create expected report $ReportPath"
        return 1
    }

    $reportItem = Get-Item -Path $ReportPath
    if ($reportItem.LastWriteTime -lt $startedAt.AddSeconds(-1)) {
        Write-Host "FAIL: $Label report was not refreshed: $ReportPath"
        Write-Host ("Report timestamp: {0:yyyy-MM-dd HH:mm:ss K}; audit started: {1:yyyy-MM-dd HH:mm:ss K}" -f $reportItem.LastWriteTime, $startedAt)
        return 1
    }

    Write-Host ("Fresh report: {0} ({1:yyyy-MM-dd HH:mm:ss K})" -f $ReportPath, $reportItem.LastWriteTime)
    return 0
}

$projectRoot = Resolve-ProjectRoot
$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

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
    },
    [pscustomobject]@{
        Label = "Playtest telemetry wiring"
        ScriptPath = Join-Path $PSScriptRoot "AuditPlaytestTelemetryWiringStatic.ps1"
        ReportPath = Join-Path $ReportDirectory "AlienCrusherPlaytestTelemetryWiringStaticAudit.log"
    }
)

$failed = 0
Write-Output "[AlienCrusher][StaticAudits] Running Unity-free audits"
Write-Output "Project: $projectRoot"
Write-Output "Reports: $ReportDirectory"
Write-Output "PowerShell: $powerShellExecutable"

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
        -MaxGrowthStage $MaxGrowthStage `
        -PowerShellExecutable $powerShellExecutable

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
