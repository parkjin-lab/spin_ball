[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [string]$TelemetrySourcePath = "",
    [string]$SummaryScriptPath = "",
    [string]$RuntimeSystemsPath = "",
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
$resolvedRuntimeSystemsPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $RuntimeSystemsPath -RelativePath "Assets\Scripts\Runtime\Systems"

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

$eventContracts = @(
    [pscustomobject]@{ Event = "SWEEP_START"; Helper = "LogPlaytestSweepStart"; Caller = "DummyFlowController.Lifecycle.cs" },
    [pscustomobject]@{ Event = "STAGE_START"; Helper = "LogPlaytestStageStart"; Caller = "DummyFlowController.StageFlow.cs" },
    [pscustomobject]@{ Event = "ROUTE_OPEN"; Helper = "LogPlaytestRouteOpen"; Caller = "DummyFlowController.ProgressionCore.cs" },
    [pscustomobject]@{ Event = "ROUTE_HOLD_CLEAR"; Helper = "LogPlaytestRouteHoldSuccess"; Caller = "DummyFlowController.ProgressionCore.cs" },
    [pscustomobject]@{ Event = "ROUTE_BONUS"; Helper = "LogPlaytestRouteBonusClaim"; Caller = "DummyFlowController.ProgressionCore.cs" },
    [pscustomobject]@{ Event = "FORWARD_SMASH"; Helper = "LogPlaytestForwardSmash"; Caller = "DummyFlowController.ProgressionCore.cs" },
    [pscustomobject]@{ Event = "STAGE_END"; Helper = "LogPlaytestStageEnd"; Caller = "DummyFlowController.StageFlow.cs" },
    [pscustomobject]@{ Event = "SWEEP_END"; Helper = "LogPlaytestSweepEnd"; Caller = "DummyFlowController.Lifecycle.cs" }
)
$expectedEvents = @($eventContracts | ForEach-Object { $_.Event })

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

foreach ($contract in $eventContracts) {
    $helperPattern = "(?s)private\s+void\s+$([regex]::Escape($contract.Helper))\s*\([^)]*\).*?EmitPlaytestTelemetry\(\s*`"$([regex]::Escape($contract.Event))`""
    if ($telemetrySourceText -notmatch $helperPattern) {
        $errors.Add("Telemetry helper '$($contract.Helper)' does not clearly emit expected event '$($contract.Event)'.")
    }

    $callerPath = Join-Path $resolvedRuntimeSystemsPath $contract.Caller
    $callerText = Read-RequiredText -Path $callerPath -Label "Telemetry caller $($contract.Caller)" -Errors $errors
    if (-not [string]::IsNullOrWhiteSpace($callerText) -and $callerText -notmatch "$([regex]::Escape($contract.Helper))\s*\(") {
        $errors.Add("Telemetry helper '$($contract.Helper)' is not called in expected file: $callerPath")
    }
}

if (Test-Path -Path $resolvedRuntimeSystemsPath -PathType Container) {
    $telemetrySourceFullPath = [System.IO.Path]::GetFullPath($resolvedTelemetrySourcePath)
    Get-ChildItem -Path $resolvedRuntimeSystemsPath -Filter "DummyFlowController*.cs" -File | ForEach-Object {
        $candidatePath = [System.IO.Path]::GetFullPath($_.FullName)
        if ([string]::Equals($candidatePath, $telemetrySourceFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            return
        }

        $candidateText = Get-Content -Path $_.FullName -Raw
        if ($candidateText -match 'EmitPlaytestTelemetry\s*\(') {
            $errors.Add("Direct EmitPlaytestTelemetry call found outside telemetry helper file: $($_.FullName)")
        }
    }
}
else {
    $errors.Add("Runtime systems directory not found: $resolvedRuntimeSystemsPath")
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
$lines.Add("Runtime systems: $resolvedRuntimeSystemsPath")
$lines.Add("Expected events: $([string]::Join(', ', $expectedEvents))")
$lines.Add("Emitted events: $([string]::Join(', ', $emittedEvents))")
$lines.Add("Helper contracts: $([string]::Join(', ', @($eventContracts | ForEach-Object { "$($_.Event)->$($_.Helper)@$($_.Caller)" })))")

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
