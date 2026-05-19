[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Add-Check {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$ReportText,
        [string]$Needle,
        [string]$Label
    )

    if ($ReportText.Contains($Needle)) {
        return
    }

    $Errors.Add("$Label missing expected marker: $Needle")
}

$projectRoot = Resolve-ProjectRoot
$checklistScriptPath = Join-Path $PSScriptRoot "GenerateStagePlaytestChecklist.ps1"
$summaryScriptPath = Join-Path $PSScriptRoot "GeneratePlaytestTelemetrySummary.ps1"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherReadinessReportsRegression.log"
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

if (-not (Test-Path -Path $checklistScriptPath -PathType Leaf)) {
    $errors.Add("Checklist generator not found: $checklistScriptPath")
}

if (-not (Test-Path -Path $summaryScriptPath -PathType Leaf)) {
    $errors.Add("Telemetry summary generator not found: $summaryScriptPath")
}

$tempId = [guid]::NewGuid().ToString("N")
$tempChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherStagePlaytestChecklistReadiness-{0}.md" -f $tempId)
$tempSummaryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestTelemetrySummaryReadiness-{0}.md" -f $tempId)
$tempMissingTelemetryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherMissingTelemetry-{0}.log" -f $tempId)
$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

if ($errors.Count -eq 0) {
    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $checklistScriptPath `
        -MaxStage $MaxStage `
        -MaxGrowthStage $MaxGrowthStage `
        -ReportPath $tempChecklistPath |
        Out-Null

    $checklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($checklistExitCode -ne 0) {
        $errors.Add("Checklist generator exited with code $checklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempChecklistPath -PathType Leaf)) {
        $errors.Add("Checklist generator did not create expected report: $tempChecklistPath")
    }
    else {
        $checklistText = Get-Content -Path $tempChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## Validation Gate" -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle 'Manual notes file: `Docs/AlienCrusherStagePlaytestNotes.md`' -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## First F10 Sweep Run Sheet" -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## Rhythm Pass" -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## Post-Sweep Decision" -Label "Checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $summaryScriptPath `
        -TelemetryLogPath $tempMissingTelemetryPath `
        -ReportPath $tempSummaryPath |
        Out-Null

    $summaryExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($summaryExitCode -ne 0) {
        $errors.Add("Telemetry summary generator exited with code $summaryExitCode")
    }
    elseif (-not (Test-Path -Path $tempSummaryPath -PathType Leaf)) {
        $errors.Add("Telemetry summary generator did not create expected report: $tempSummaryPath")
    }
    else {
        $summaryText = Get-Content -Path $tempSummaryPath -Raw
        Add-Check -Errors $errors -ReportText $summaryText -Needle "No playtest telemetry log exists yet." -Label "No-log summary"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "Manual gate required:" -Label "No-log summary"
        Add-Check -Errors $errors -ReportText $summaryText -Needle 'Expected `Tune Next` fields after real telemetry:' -Label "No-log summary"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "Do not tune stage rhythm presets, payoff layouts, boss windows, or route timing until this log exists." -Label "No-log summary"
    }
}

foreach ($tempFilePath in @($tempChecklistPath, $tempSummaryPath, $tempMissingTelemetryPath)) {
    if (Test-Path -Path $tempFilePath -PathType Leaf) {
        Remove-Item -Path $tempFilePath -Force
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][ReadinessReportsRegression] Readiness report generator regression")
$lines.Add("Checklist script: $checklistScriptPath")
$lines.Add("Summary script: $summaryScriptPath")
$lines.Add("PowerShell: $powerShellExecutable")

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
