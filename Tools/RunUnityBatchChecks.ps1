[CmdletBinding()]
param(
    [string]$UnityPath = "D:\Unity\6000.3.8f1\Editor\Unity.exe",
    [string]$ProjectPath = "",
    [int]$TimeoutSeconds = 900,
    [switch]$UseGraphics,
    [switch]$ClearStaleUnityLock,
    [switch]$KeepGoing
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    param([string]$OverridePath)

    if (-not [string]::IsNullOrWhiteSpace($OverridePath)) {
        return (Resolve-Path -Path $OverridePath).Path
    }

    return Split-Path -Parent $PSScriptRoot
}

function Invoke-UnityBatchCheck {
    param(
        [string]$Label,
        [string]$ExecuteMethod,
        [string]$EditorLogPath,
        [string]$ExpectedReportPath,
        [string]$UnityPath,
        [string]$ProjectRoot,
        [int]$TimeoutSeconds,
        [bool]$UseGraphics,
        [bool]$ClearStaleUnityLock
    )

    Write-Host ""
    Write-Host "== $Label =="

    $arguments = @(
        "-UnityPath", $UnityPath,
        "-ProjectPath", $ProjectRoot,
        "-ExecuteMethod", $ExecuteMethod,
        "-EditorLogPath", $EditorLogPath,
        "-ExpectedReportPath", $ExpectedReportPath,
        "-TimeoutSeconds", $TimeoutSeconds
    )

    if ($UseGraphics) {
        $arguments += "-UseGraphics"
    }

    if ($ClearStaleUnityLock) {
        $arguments += "-ClearStaleUnityLock"
    }

    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "InvokeUnityBatch.ps1") @arguments |
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

$projectRoot = Resolve-ProjectRoot -OverridePath $ProjectPath
$logsPath = Join-Path $projectRoot "Logs"
New-Item -ItemType Directory -Path $logsPath -Force | Out-Null

$checks = @(
    [pscustomobject]@{
        Label = "Scene validation"
        ExecuteMethod = "AlienCrusher.EditorTools.AlienCrusherSceneValidator.ValidateCurrentSceneBatch"
        EditorLogPath = Join-Path $logsPath "AlienCrusherBatchValidationEditor.log"
        ExpectedReportPath = Join-Path $logsPath "AlienCrusherSceneValidation.log"
    },
    [pscustomobject]@{
        Label = "Runtime map layout audit"
        ExecuteMethod = "AlienCrusher.EditorTools.AlienCrusherMapLayoutAuditor.AuditRuntimeMapLayoutBatch"
        EditorLogPath = Join-Path $logsPath "AlienCrusherMapLayoutAuditEditor.log"
        ExpectedReportPath = Join-Path $logsPath "AlienCrusherMapLayoutAudit.log"
    }
)

$failed = 0
Write-Output "[AlienCrusher][UnityBatchChecks] Running Unity batch checks"
Write-Output "Project: $projectRoot"
Write-Output "Logs: $logsPath"

foreach ($check in $checks) {
    $exitCode = Invoke-UnityBatchCheck `
        -Label $check.Label `
        -ExecuteMethod $check.ExecuteMethod `
        -EditorLogPath $check.EditorLogPath `
        -ExpectedReportPath $check.ExpectedReportPath `
        -UnityPath $UnityPath `
        -ProjectRoot $projectRoot `
        -TimeoutSeconds $TimeoutSeconds `
        -UseGraphics ([bool]$UseGraphics) `
        -ClearStaleUnityLock ([bool]$ClearStaleUnityLock)

    if ($exitCode -ne 0) {
        $failed++
        if (-not $KeepGoing) {
            break
        }
    }
}

if ($failed -gt 0) {
    Write-Output ""
    Write-Output "Result: $failed Unity batch check(s) failed"
    exit 1
}

Write-Output ""
Write-Output "Result: all Unity batch checks passed"
exit 0
