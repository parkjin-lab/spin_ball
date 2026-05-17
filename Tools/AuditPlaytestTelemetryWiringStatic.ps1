[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [string]$TelemetrySourcePath = "",
    [string]$SummaryScriptPath = "",
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

function Read-RequiredText {
    param(
        [string]$Path,
        [string]$Label,
        [System.Collections.Generic.List[string]]$Errors
    )

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        $Errors.Add("$Label file not found: $Path")
        return ""
    }

    return Get-Content -Path $Path -Raw
}

$projectRoot = Resolve-ProjectRoot
$resolvedTelemetrySourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $TelemetrySourcePath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.PlaytestTelemetry.cs"
$resolvedSummaryScriptPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $SummaryScriptPath -RelativePath "Tools\GeneratePlaytestTelemetrySummary.ps1"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetryWiringStaticAudit.log"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()
$telemetrySourceText = Read-RequiredText -Path $resolvedTelemetrySourcePath -Label "Telemetry source" -Errors $errors
$summaryScriptText = Read-RequiredText -Path $resolvedSummaryScriptPath -Label "Summary script" -Errors $errors

$expectedEvents = @(
    "SWEEP_START",
    "STAGE_START",
    "ROUTE_OPEN",
    "ROUTE_HOLD_CLEAR",
    "ROUTE_BONUS",
    "FORWARD_SMASH",
    "STAGE_END",
    "SWEEP_END"
)

$emittedEvents = @(
    [regex]::Matches($telemetrySourceText, 'EmitPlaytestTelemetry\(\s*"(?<event>[A-Z_]+)"') |
        ForEach-Object { $_.Groups["event"].Value } |
        Sort-Object -Unique
)

foreach ($eventName in $expectedEvents) {
    if ($emittedEvents -notcontains $eventName) {
        $errors.Add("Telemetry source does not emit expected event: $eventName")
    }

    if ($summaryScriptText -notmatch [regex]::Escape("'$eventName'") -and $summaryScriptText -notmatch [regex]::Escape("`"$eventName`"")) {
        $errors.Add("Summary script does not reference expected event: $eventName")
    }
}

foreach ($eventName in $emittedEvents) {
    if ($expectedEvents -notcontains $eventName) {
        $warnings.Add("Telemetry source emits event not covered by the static contract: $eventName")
    }
}

if ($telemetrySourceText -notmatch '\[AlienCrusher\]\[Playtest\]\s+time=') {
    $errors.Add("Telemetry source does not contain the expected [AlienCrusher][Playtest] time= log prefix.")
}

if (-not $summaryScriptText.Contains('\[AlienCrusher\]\[Playtest\] time=')) {
    $errors.Add("Summary parser does not contain the expected [AlienCrusher][Playtest] time= pattern.")
}

if ($telemetrySourceText -notmatch 'Logs",\s*"AlienCrusherPlaytestTelemetry\.log"') {
    $warnings.Add("Editor telemetry log path no longer clearly targets Logs\AlienCrusherPlaytestTelemetry.log.")
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][PlaytestTelemetryWiringStaticAudit] Playtest telemetry wiring audit")
$lines.Add("Telemetry source: $resolvedTelemetrySourcePath")
$lines.Add("Summary script: $resolvedSummaryScriptPath")
$lines.Add("Expected events: $([string]::Join(', ', $expectedEvents))")
$lines.Add("Emitted events: $([string]::Join(', ', $emittedEvents))")

foreach ($errorMessage in $errors) {
    $lines.Add("ERROR: $errorMessage")
}

foreach ($warningMessage in $warnings) {
    $lines.Add("WARN: $warningMessage")
}

$lines.Add("Result: $($errors.Count) error(s), $($warnings.Count) warning(s)")

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output $report

if ($errors.Count -gt 0 -or ($FailOnWarnings -and $warnings.Count -gt 0)) {
    exit 1
}

exit 0
