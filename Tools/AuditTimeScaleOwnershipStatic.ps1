[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectRoot = Split-Path -Parent $PSScriptRoot
$systemsRoot = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems"
$ownerPath = Join-Path $systemsRoot "DummyFlowController.TimeControl.cs"
$lifecyclePath = Join-Path $systemsRoot "DummyFlowController.Lifecycle.cs"
$bossPath = Join-Path $systemsRoot "DummyFlowController.StageEncounter.cs"
$validatorPath = Join-Path $projectRoot "Assets\Scripts\Editor\TimeScaleChannelOverlapValidator.cs"
$batchChecksPath = Join-Path $projectRoot "Tools\RunUnityBatchChecks.ps1"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherTimeScaleOwnershipStaticAudit.log"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

if (-not (Test-Path -Path $ownerPath -PathType Leaf)) {
    $errors.Add("Missing time-scale owner: $ownerPath")
}
else {
    $ownerText = Get-Content -Path $ownerPath -Raw
    foreach ($needle in @(
        'private enum TimeScaleChannel',
        'private void SetTimeScaleChannel',
        'private void ApplyTimeScaleChannels',
        'private static float ResolveTargetTimeScale',
        'private static float ResolveFixedDeltaTime',
        'public static Vector2 ResolveTimeStateForValidation',
        'private void ResetTimeControl',
        'stagePaused = false',
        'TimeScaleChannel.Pause',
        'TimeScaleChannel.OverdrivePulse',
        'TimeScaleChannel.BossFinish'
    )) {
        if (-not $ownerText.Contains($needle)) {
            $errors.Add("Time-scale owner missing contract text: $needle")
        }
    }
}

if (-not (Test-Path -Path $lifecyclePath -PathType Leaf) -or
    -not ([string](Get-Content -Path $lifecyclePath -Raw)).Contains('StopOverdriveTimePulse();') -or
    -not ([string](Get-Content -Path $lifecyclePath -Raw)).Contains('StopBossFinishSlowMotion();')) {
    $errors.Add("Lifecycle must explicitly stop both time-effect coroutines before reset")
}

if (-not (Test-Path -Path $bossPath -PathType Leaf) -or
    -not ([string](Get-Content -Path $bossPath -Raw)).Contains('private void StopBossFinishSlowMotion()')) {
    $errors.Add("Boss finish time channel is missing an explicit stop helper")
}

if (-not (Test-Path -Path $validatorPath -PathType Leaf)) {
    $errors.Add("Missing time-scale overlap validator: $validatorPath")
}
else {
    $validatorText = Get-Content -Path $validatorPath -Raw
    foreach ($needle in @(
        'public static void ValidateBatch()',
        'Slower active channel wins',
        'Pause overrides both effects',
        'Releasing pause preserves both effects',
        'Releasing boss preserves overdrive',
        'Requested scales are clamped',
        'AlienCrusherTimeScaleChannelOverlapValidation.log'
    )) {
        if (-not $validatorText.Contains($needle)) {
            $errors.Add("Time-scale overlap validator missing contract text: $needle")
        }
    }
}

if (-not (Test-Path -Path $batchChecksPath -PathType Leaf)) {
    $errors.Add("Missing Unity batch check runner: $batchChecksPath")
}
else {
    $batchChecksText = Get-Content -Path $batchChecksPath -Raw
    foreach ($needle in @(
        'AlienCrusher.EditorTools.TimeScaleChannelOverlapValidator.ValidateBatch',
        'AlienCrusherTimeScaleChannelOverlapValidationEditor.log',
        'AlienCrusherTimeScaleChannelOverlapValidation.log'
    )) {
        if (-not $batchChecksText.Contains($needle)) {
            $errors.Add("Unity batch check runner missing time overlap wiring: $needle")
        }
    }
}
$controllerFiles = Get-ChildItem -Path $systemsRoot -Filter "DummyFlowController*.cs" -File
foreach ($file in $controllerFiles) {
    if ($file.FullName -eq $ownerPath) {
        continue
    }

    $matches = Select-String -Path $file.FullName -Pattern 'Time\.(timeScale|fixedDeltaTime)\s*='
    foreach ($match in $matches) {
        $errors.Add("Direct global time write outside owner: $($file.Name):$($match.LineNumber)")
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][TimeScaleOwnershipStaticAudit] Global time ownership audit")
$lines.Add("Owner: $ownerPath")
$lines.Add("Overlap validator: $validatorPath")
$lines.Add("Contract: pause, overdrive pulse, and boss finish slow motion submit independent requests; only the owner writes Unity global time state.")
foreach ($errorMessage in $errors) {
    $lines.Add("ERROR: $errorMessage")
}
foreach ($warningMessage in $warnings) {
    $lines.Add("WARN: $warningMessage")
}
$lines.Add("Result: $($errors.Count) error(s), $($warnings.Count) warning(s)")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output $report

if ($errors.Count -gt 0 -or ($FailOnWarnings -and $warnings.Count -gt 0)) {
    exit 1
}

exit 0
