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
$audioChecklistScriptPath = Join-Path $PSScriptRoot "GenerateAudioResourceAssignmentChecklist.ps1"
$formChecklistScriptPath = Join-Path $PSScriptRoot "GenerateFormIdentityProductionChecklist.ps1"
$destructionChecklistScriptPath = Join-Path $PSScriptRoot "GenerateDestructionReadabilityChecklist.ps1"

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

if (-not (Test-Path -Path $audioChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Audio resource checklist generator not found: $audioChecklistScriptPath")
}

if (-not (Test-Path -Path $formChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Form identity checklist generator not found: $formChecklistScriptPath")
}

if (-not (Test-Path -Path $destructionChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Destruction readability checklist generator not found: $destructionChecklistScriptPath")
}

$tempId = [guid]::NewGuid().ToString("N")
$tempChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherStagePlaytestChecklistReadiness-{0}.md" -f $tempId)
$tempSummaryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestTelemetrySummaryReadiness-{0}.md" -f $tempId)
$tempAudioChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherAudioResourceAssignmentChecklistReadiness-{0}.md" -f $tempId)
$tempFormChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherFormIdentityProductionChecklistReadiness-{0}.md" -f $tempId)
$tempDestructionChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherDestructionReadabilityChecklistReadiness-{0}.md" -f $tempId)
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

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $audioChecklistScriptPath `
        -ReportPath $tempAudioChecklistPath |
        Out-Null

    $audioChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($audioChecklistExitCode -ne 0) {
        $errors.Add("Audio resource checklist generator exited with code $audioChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempAudioChecklistPath -PathType Leaf)) {
        $errors.Add("Audio resource checklist generator did not create expected report: $tempAudioChecklistPath")
    }
    else {
        $audioChecklistText = Get-Content -Path $tempAudioChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle "## Assignment Pass Order" -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle "## Current FeedbackSystem Audio Slots" -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle 'failureWarningClip' -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle 'failureBossClip' -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle 'SFX_Route_Open' -Label "Audio checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $formChecklistScriptPath `
        -ReportPath $tempFormChecklistPath |
        Out-Null

    $formChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($formChecklistExitCode -ne 0) {
        $errors.Add("Form identity checklist generator exited with code $formChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempFormChecklistPath -PathType Leaf)) {
        $errors.Add("Form identity checklist generator did not create expected report: $tempFormChecklistPath")
    }
    else {
        $formChecklistText = Get-Content -Path $tempFormChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle "## Production Pass Order" -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle "## Current Runtime Form Identity Targets" -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Sphere' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Spike' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Ram' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Saucer' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Crusher' -Label "Form checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $destructionChecklistScriptPath `
        -ReportPath $tempDestructionChecklistPath |
        Out-Null

    $destructionChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($destructionChecklistExitCode -ne 0) {
        $errors.Add("Destruction readability checklist generator exited with code $destructionChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempDestructionChecklistPath -PathType Leaf)) {
        $errors.Add("Destruction readability checklist generator did not create expected report: $tempDestructionChecklistPath")
    }
    else {
        $destructionChecklistText = Get-Content -Path $tempDestructionChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle "## Production Pass Order" -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle "## Current Destruction Readability Targets" -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'MAT_WeakPoint_Glow' -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'MAT_Shielded_Pylon' -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'MAT_Exposed_Core' -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'SFX_Break_LargeCollapse' -Label "Destruction checklist"
    }
}

foreach ($tempFilePath in @($tempChecklistPath, $tempSummaryPath, $tempAudioChecklistPath, $tempFormChecklistPath, $tempDestructionChecklistPath, $tempMissingTelemetryPath)) {
    if (Test-Path -Path $tempFilePath -PathType Leaf) {
        Remove-Item -Path $tempFilePath -Force
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][ReadinessReportsRegression] Readiness report generator regression")
$lines.Add("Checklist script: $checklistScriptPath")
$lines.Add("Summary script: $summaryScriptPath")
$lines.Add("Audio checklist script: $audioChecklistScriptPath")
$lines.Add("Form checklist script: $formChecklistScriptPath")
$lines.Add("Destruction checklist script: $destructionChecklistScriptPath")
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
