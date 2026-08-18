[CmdletBinding()]
param(
    [int]$MaxStage = 2,
    [int]$MaxGrowthStage = 10,
    [string]$ReportPath = "",
    [string]$FixturePath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Resolve-ProjectPath {
    param(
        [string]$ProjectRoot,
        [string]$OverridePath,
        [string]$RelativePath
    )

    if (-not [string]::IsNullOrWhiteSpace($OverridePath)) {
        if ([System.IO.Path]::IsPathRooted($OverridePath)) {
            return $OverridePath
        }

        return Join-Path $ProjectRoot $OverridePath
    }

    return Join-Path $ProjectRoot $RelativePath
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
$resolvedFixturePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FixturePath -RelativePath "Tools\TestData\PlaytestTelemetrySummary\basic-sweep.txt"
$summaryScriptPath = Join-Path $PSScriptRoot "GeneratePlaytestTelemetrySummary.ps1"
$evidenceGateScriptPath = Join-Path $PSScriptRoot "TestPlaytestEvidenceGate.ps1"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestEvidenceGateRegression.log"
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
$fixtureMaxStage = 2

if (-not (Test-Path -Path $resolvedFixturePath -PathType Leaf)) {
    $errors.Add("Fixture log not found: $resolvedFixturePath")
}

if (-not (Test-Path -Path $summaryScriptPath -PathType Leaf)) {
    $errors.Add("Summary script not found: $summaryScriptPath")
}

if (-not (Test-Path -Path $evidenceGateScriptPath -PathType Leaf)) {
    $errors.Add("Evidence gate script not found: $evidenceGateScriptPath")
}

$tempId = [guid]::NewGuid().ToString("N")
$tempSummaryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestEvidenceGateSummary-{0}.md" -f $tempId)
$tempNotesPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestEvidenceGateNotes-{0}.md" -f $tempId)
$tempShallowNotesPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestEvidenceGateShallowNotes-{0}.md" -f $tempId)
$tempGateReportPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestEvidenceGateReport-{0}.log" -f $tempId)
$tempShallowGateReportPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestEvidenceGateShallowReport-{0}.log" -f $tempId)
$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

if ($errors.Count -eq 0) {
    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $summaryScriptPath `
        -TelemetryLogPath $resolvedFixturePath `
        -ReportPath $tempSummaryPath |
        Out-Null

    $summaryExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($summaryExitCode -ne 0) {
        $errors.Add("Summary generator exited with code $summaryExitCode")
    }
    elseif (-not (Test-Path -Path $tempSummaryPath -PathType Leaf)) {
        $errors.Add("Summary generator did not create expected report: $tempSummaryPath")
    }
}

if ($errors.Count -eq 0) {
    $notesLines = [System.Collections.Generic.List[string]]::new()
    $notesLines.Add("# Alien Crusher Stage Playtest Notes")
    $notesLines.Add("")
    $notesLines.Add("Last updated: 2026-05-25")
    $notesLines.Add("")
    $notesLines.Add("## Progression Save Smoke Pass")
    $notesLines.Add("")
    $notesLines.Add("- [x] Starting DP / selected form / highest unlocked stage / previewed meta upgrade noted.")
    $notesLines.Add("- [x] DP or stage progress changes after a clear.")
    $notesLines.Add("- [x] Affordable form unlock or meta upgrade purchase persists, or locked/need-DP state is readable when not affordable.")
    $notesLines.Add("- [x] Exit and re-enter play mode keeps DP, selected form, stage unlock, and meta upgrade state.")
    $notesLines.Add("- [x] Edited/restored save data with impossible values is repaired and does not reappear after reload.")
    $notesLines.Add("- [x] Save/load result: fixture save smoke pass")
    $notesLines.Add("")
    $notesLines.Add("## Stage Notes")
    $notesLines.Add("")
    for ($stage = 1; $stage -le $fixtureMaxStage; $stage++) {
        $notesLines.Add(("### Stage {0:00}" -f $stage))
        $notesLines.Add("")
        $notesLines.Add("- Readability: fixture readability note has enough detail")
        $notesLines.Add("- Route pressure: fixture route pressure note has enough detail")
        $notesLines.Add("- Map identity: fixture map identity note has enough detail")
        $notesLines.Add("- Rhythm identity: fixture rhythm identity note has enough detail")
        $notesLines.Add("- Screenshot/video reference: fixture capture")
        $notesLines.Add("")
    }
    $notesLines.Add("## Post-Sweep Decision")
    $notesLines.Add("")
    $notesLines.Add("- Primary bottleneck / dominant broken beat: Opening Clarity")
    $notesLines.Add("- Tune these fields first / one variable family to change next: opener density")
    $notesLines.Add('- Current values copied from `Tune Next`: fixture values')
    $notesLines.Add("- Chosen first-pass experiment: fixture experiment")
    $notesLines.Add("- Retest stage(s): Stage 01-02")
    $notesLines.Add("- Do not touch yet: boss windows")

    Set-Content -Path $tempNotesPath -Value ([string]::Join([Environment]::NewLine, $notesLines) + [Environment]::NewLine) -Encoding UTF8

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $evidenceGateScriptPath `
        -MaxStage $fixtureMaxStage `
        -TelemetryLogPath $resolvedFixturePath `
        -SummaryPath $tempSummaryPath `
        -NotesPath $tempNotesPath `
        -ReportPath $tempGateReportPath `
        -RequireDecision |
        Out-Null

    $gateExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($gateExitCode -ne 0) {
        $errors.Add("Evidence gate exited with code $gateExitCode")
        if (Test-Path -Path $tempGateReportPath -PathType Leaf) {
            $gateReportText = Get-Content -Path $tempGateReportPath -Raw
            $errors.Add("Evidence gate report: $($gateReportText.Trim())")
        }
        if (Test-Path -Path $tempNotesPath -PathType Leaf) {
            $notesText = Get-Content -Path $tempNotesPath -Raw
            $errors.Add("Fixture notes: $($notesText.Trim())")
        }
    }
    elseif (-not (Test-Path -Path $tempGateReportPath -PathType Leaf)) {
        $errors.Add("Evidence gate did not create expected report: $tempGateReportPath")
    }
    else {
        $gateReportText = Get-Content -Path $tempGateReportPath -Raw
        Add-Check -Errors $errors -ReportText $gateReportText -Needle "[AlienCrusher][PlaytestEvidenceGate] Stage 1-$fixtureMaxStage evidence gate" -Label "Gate report"
        Add-Check -Errors $errors -ReportText $gateReportText -Needle "Mode: blocking" -Label "Gate report"
        Add-Check -Errors $errors -ReportText $gateReportText -Needle "Result: 0 error(s), 0 warning(s)" -Label "Gate report"
    }
}

if ($errors.Count -eq 0) {
    $shallowLines = [System.Collections.Generic.List[string]]::new()
    $shallowLines.Add("# Alien Crusher Stage Playtest Notes")
    $shallowLines.Add("")
    $shallowLines.Add("## Progression Save Smoke Pass")
    $shallowLines.Add("")
    $shallowLines.Add("- [x] Save/load result: ok")
    $shallowLines.Add("")
    $shallowLines.Add("## Stage Notes")
    $shallowLines.Add("")
    for ($stage = 1; $stage -le $fixtureMaxStage; $stage++) {
        $shallowLines.Add(("### Stage {0:00}" -f $stage))
        $shallowLines.Add("")
        $shallowLines.Add("- Readability: ok")
        $shallowLines.Add("- Route pressure: ok")
        $shallowLines.Add("- Map identity: ok")
        $shallowLines.Add("- Rhythm identity: ok")
        $shallowLines.Add("- Screenshot/video reference: ok")
        $shallowLines.Add("")
    }

    Set-Content -Path $tempShallowNotesPath -Value ([string]::Join([Environment]::NewLine, $shallowLines) + [Environment]::NewLine) -Encoding UTF8

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $evidenceGateScriptPath `
        -MaxStage $fixtureMaxStage `
        -TelemetryLogPath $resolvedFixturePath `
        -SummaryPath $tempSummaryPath `
        -NotesPath $tempShallowNotesPath `
        -ReportPath $tempShallowGateReportPath |
        Out-Null

    $shallowGateExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($shallowGateExitCode -eq 0) {
        $errors.Add("Evidence gate accepted shallow playtest notes.")
    }
    elseif (-not (Test-Path -Path $tempShallowGateReportPath -PathType Leaf)) {
        $errors.Add("Evidence gate did not create expected shallow-note report: $tempShallowGateReportPath")
    }
    else {
        $shallowGateReportText = Get-Content -Path $tempShallowGateReportPath -Raw
        Add-Check -Errors $errors -ReportText $shallowGateReportText -Needle "missing meaningful note field" -Label "Shallow-note gate report"
        Add-Check -Errors $errors -ReportText $shallowGateReportText -Needle "missing meaningful Save/load result" -Label "Shallow-note gate report"
    }
}

foreach ($tempFilePath in @($tempSummaryPath, $tempNotesPath, $tempShallowNotesPath, $tempGateReportPath, $tempShallowGateReportPath)) {
    if (Test-Path -Path $tempFilePath -PathType Leaf) {
        Remove-Item -Path $tempFilePath -Force
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][PlaytestEvidenceGateRegression] Playtest evidence gate regression")
$lines.Add("Fixture: $resolvedFixturePath")
$lines.Add("Summary script: $summaryScriptPath")
$lines.Add("Evidence gate script: $evidenceGateScriptPath")
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
