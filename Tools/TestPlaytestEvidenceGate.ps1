[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [string]$TelemetryLogPath = "",
    [string]$SummaryPath = "",
    [string]$NotesPath = "",
    [string]$ReportPath = "",
    [switch]$RequireDecision,
    [switch]$ReportOnly
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

function Add-Error {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$Message
    )

    $Errors.Add($Message)
}

function Add-Warning {
    param(
        [System.Collections.Generic.List[string]]$Warnings,
        [string]$Message
    )

    $Warnings.Add($Message)
}

function Parse-TelemetryLine {
    param([string]$Line)

    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $null
    }

    $match = [regex]::Match($Line, '^\[AlienCrusher\]\[Playtest\] time=(?<time>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) stage=(?<stage>\d+) event=(?<event>[A-Z_]+)(?<detail>.*)$')
    if (-not $match.Success) {
        return $null
    }

    return [pscustomobject]@{
        Time = [datetime]::ParseExact($match.Groups["time"].Value, "yyyy-MM-dd HH:mm:ss", [System.Globalization.CultureInfo]::InvariantCulture)
        Stage = [int]$match.Groups["stage"].Value
        Event = $match.Groups["event"].Value
        Detail = $match.Groups["detail"].Value.Trim()
    }
}

function Test-StageNoteField {
    param(
        [string]$NotesText,
        [int]$Stage,
        [string]$Field,
        [int]$MinLength = 12
    )

    $stageLabel = "Stage {0:00}" -f $Stage
    $stagePattern = "(?ms)^### $([regex]::Escape($stageLabel))\s*(?<body>.*?)(?=^### Stage \d{2}\s*|^## |\z)"
    $stageMatch = [regex]::Match($NotesText, $stagePattern)
    if (-not $stageMatch.Success) {
        return $false
    }

    $body = $stageMatch.Groups["body"].Value
    $fieldPattern = "(?m)^- $([regex]::Escape($Field)):[ \t]*(?<value>[^\r\n]*)\r?$"
    $fieldMatch = [regex]::Match($body, $fieldPattern)
    if (-not $fieldMatch.Success) {
        return $false
    }

    $value = $fieldMatch.Groups["value"].Value.Trim()
    return $value.Length -ge $MinLength
}

function Test-DecisionField {
    param(
        [string]$NotesText,
        [string]$Field
    )

    $decisionMatch = [regex]::Match($NotesText, '(?ms)^## Post-Sweep Decision\s*(?<body>.*?)(?=^## |\z)')
    if (-not $decisionMatch.Success) {
        return $false
    }

    $body = $decisionMatch.Groups["body"].Value
    $fieldPattern = "(?m)^- $([regex]::Escape($Field)):[ \t]*(?<value>[^\r\n]*)\r?$"
    $fieldMatch = [regex]::Match($body, $fieldPattern)
    if (-not $fieldMatch.Success) {
        return $false
    }

    $value = $fieldMatch.Groups["value"].Value.Trim()
    return -not [string]::IsNullOrWhiteSpace($value)
}

function Test-ProgressionSaveSmokeField {
    param(
        [string]$NotesText,
        [string]$Field,
        [int]$MinLength = 12
    )

    $saveSmokeMatch = [regex]::Match($NotesText, '(?ms)^## Progression Save Smoke Pass\s*(?<body>.*?)(?=^## |\z)')
    if (-not $saveSmokeMatch.Success) {
        return $false
    }

    $body = $saveSmokeMatch.Groups["body"].Value
    $fieldPattern = "(?m)^- \[[ xX]\] $([regex]::Escape($Field)):[ \t]*(?<value>[^\r\n]*)\r?$"
    $fieldMatch = [regex]::Match($body, $fieldPattern)
    if (-not $fieldMatch.Success) {
        return $false
    }

    $value = $fieldMatch.Groups["value"].Value.Trim()
    return $value.Length -ge $MinLength
}

$projectRoot = Resolve-ProjectRoot
$resolvedTelemetryLogPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $TelemetryLogPath -RelativePath "Logs\AlienCrusherPlaytestTelemetry.log"
$resolvedSummaryPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $SummaryPath -RelativePath "Logs\AlienCrusherPlaytestTelemetrySummary.md"
$resolvedNotesPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $NotesPath -RelativePath "Docs\AlienCrusherStagePlaytestNotes.md"
$resolvedReportPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ReportPath -RelativePath "Logs\AlienCrusherPlaytestEvidenceGate.log"

$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()
$lines = [System.Collections.Generic.List[string]]::new()

$lines.Add("[AlienCrusher][PlaytestEvidenceGate] Stage 1-$MaxStage evidence gate")
$lines.Add("Telemetry log: $resolvedTelemetryLogPath")
$lines.Add("Telemetry summary: $resolvedSummaryPath")
$lines.Add("Playtest notes: $resolvedNotesPath")
$lines.Add("Mode: $(if ($ReportOnly) { 'report-only' } else { 'blocking' })")

$entries = @()
if (-not (Test-Path -Path $resolvedTelemetryLogPath -PathType Leaf)) {
    Add-Error -Errors $errors -Message "Missing telemetry log. Run a real editor/development F10 sweep first."
}
else {
    $rawLines = Get-Content -Path $resolvedTelemetryLogPath
    $entries = @($rawLines | ForEach-Object { Parse-TelemetryLine -Line $_ } | Where-Object { $null -ne $_ })
    if ($entries.Count -eq 0) {
        Add-Error -Errors $errors -Message "Telemetry log exists but contains no parseable [AlienCrusher][Playtest] lines."
    }
}

if (-not (Test-Path -Path $resolvedSummaryPath -PathType Leaf)) {
    Add-Error -Errors $errors -Message "Missing telemetry summary. Run Tools/GeneratePlaytestTelemetrySummary.ps1 after the sweep."
}
elseif (Test-Path -Path $resolvedTelemetryLogPath -PathType Leaf) {
    $logTime = (Get-Item -Path $resolvedTelemetryLogPath).LastWriteTimeUtc
    $summaryTime = (Get-Item -Path $resolvedSummaryPath).LastWriteTimeUtc
    if ($summaryTime -lt $logTime) {
        Add-Error -Errors $errors -Message "Telemetry summary is older than telemetry log. Regenerate the summary after the sweep."
    }
}

if ($entries.Count -gt 0) {
    $sweepStartCount = @($entries | Where-Object { $_.Event -eq "SWEEP_START" }).Count
    $sweepEndCount = @($entries | Where-Object { $_.Event -eq "SWEEP_END" }).Count

    if ($sweepStartCount -lt 1) {
        Add-Error -Errors $errors -Message "Missing SWEEP_START marker."
    }

    if ($sweepEndCount -lt 1) {
        Add-Error -Errors $errors -Message "Missing SWEEP_END marker."
    }

    for ($stage = 1; $stage -le $MaxStage; $stage++) {
        $stageEntries = @($entries | Where-Object { $_.Stage -eq $stage })
        $stageStartCount = @($stageEntries | Where-Object { $_.Event -eq "STAGE_START" }).Count
        $stageEndCount = @($stageEntries | Where-Object { $_.Event -eq "STAGE_END" }).Count

        if ($stageStartCount -lt 1) {
            Add-Error -Errors $errors -Message ("Stage {0:00} missing STAGE_START marker." -f $stage)
        }

        if ($stageEndCount -lt 1) {
            Add-Error -Errors $errors -Message ("Stage {0:00} missing STAGE_END marker." -f $stage)
        }
    }
}

if (-not (Test-Path -Path $resolvedNotesPath -PathType Leaf)) {
    Add-Error -Errors $errors -Message "Missing tracked playtest notes document."
}
else {
    $notesText = Get-Content -Path $resolvedNotesPath -Raw
    foreach ($stage in 1..$MaxStage) {
        foreach ($field in @("Readability", "Route pressure", "Map identity", "Rhythm identity")) {
            if (-not (Test-StageNoteField -NotesText $notesText -Stage $stage -Field $field)) {
                Add-Error -Errors $errors -Message ("Stage {0:00} missing meaningful note field: {1}." -f $stage, $field)
            }
        }
    }

    foreach ($stage in @(1, 4, 7)) {
        if ($stage -le $MaxStage -and -not (Test-StageNoteField -NotesText $notesText -Stage $stage -Field "Screenshot/video reference")) {
            Add-Warning -Warnings $warnings -Message ("Stage {0:00} has no screenshot/video reference." -f $stage)
        }
    }

    if (-not (Test-ProgressionSaveSmokeField -NotesText $notesText -Field "Save/load result")) {
        Add-Error -Errors $errors -Message "Progression Save Smoke Pass missing meaningful Save/load result."
    }

    if ($RequireDecision) {
        foreach ($field in @(
            'Primary bottleneck / dominant broken beat',
            'Tune these fields first / one variable family to change next',
            'Current values copied from `Tune Next`',
            'Chosen first-pass experiment',
            'Retest stage(s)',
            'Do not touch yet'
        )) {
            if (-not (Test-DecisionField -NotesText $notesText -Field $field)) {
                Add-Error -Errors $errors -Message "Post-Sweep Decision missing field: $field."
            }
        }
    }
}

foreach ($errorMessage in $errors) {
    $lines.Add("ERROR: $errorMessage")
}

foreach ($warningMessage in $warnings) {
    $lines.Add("WARN: $warningMessage")
}

$lines.Add("Result: $($errors.Count) error(s), $($warnings.Count) warning(s)")

if ($ReportOnly -and $errors.Count -gt 0) {
    $lines.Add("ReportOnly: evidence is not ready, but this run exits successfully for readiness tracking.")
}

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
Write-Output $report

if ($errors.Count -gt 0 -and -not $ReportOnly) {
    exit 1
}

exit 0
