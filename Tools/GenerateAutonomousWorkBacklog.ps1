[CmdletBinding()]
param(
    [string]$ReportPath = ""
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

function Test-StageMeaningfulNoteField {
    param(
        [string]$NotesText,
        [int]$StageNumber,
        [string]$FieldName
    )

    $stagePattern = "(?ms)^### Stage {0:00}\s*(?<section>.*?)(?=^### Stage |\z)" -f $StageNumber
    $stageMatch = [regex]::Match($NotesText, $stagePattern)
    if (-not $stageMatch.Success) {
        return $false
    }

    $match = [regex]::Match($stageMatch.Groups["section"].Value, "(?m)^- $([regex]::Escape($FieldName)):[ \t]*(.+)$")
    return $match.Success -and $match.Groups[1].Value.Trim().Length -ge 12
}

$projectRoot = Resolve-ProjectRoot
$resolvedReportPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ReportPath -RelativePath "Logs\AlienCrusherAutonomousWorkBacklog.md"
$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$telemetryLogPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetry.log"
$telemetrySummaryPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetrySummary.md"
$stageNotesPath = Join-Path $projectRoot "Docs\AlienCrusherStagePlaytestNotes.md"
$readinessPrepPath = Join-Path $projectRoot "Tools\RunPlaytestReadinessPrep.ps1"
$staticAuditsPath = Join-Path $projectRoot "Tools\RunStaticAudits.ps1"
$resourceBacklogPath = Join-Path $projectRoot "Logs\AlienCrusherResourceProductionBacklog.md"
$architecturePlanPath = Join-Path $projectRoot "Logs\AlienCrusherArchitectureExtractionPlan.md"
$roadmapPath = Join-Path $projectRoot "Docs\GAME_UPDATE_ROADMAP.md"
$contextPath = Join-Path $projectRoot "Docs\NEXT_SESSION_CONTEXT_PACKET.md"
$policyPath = Join-Path $projectRoot "Docs\GAME_DESIGN_GAP_POLICY.md"

$notesText = if (Test-Path -Path $stageNotesPath -PathType Leaf) {
    Get-Content -Path $stageNotesPath -Raw
}
else {
    ""
}

$hasStageNotes = $false
foreach ($stageNumber in 1..7) {
    foreach ($fieldName in @("Readability", "Route pressure", "Map identity", "Rhythm identity")) {
        if (Test-StageMeaningfulNoteField -NotesText $notesText -StageNumber $stageNumber -FieldName $fieldName) {
            $hasStageNotes = $true
            break
        }
    }

    if ($hasStageNotes) {
        break
    }
}

$saveSmokeComplete = $notesText -match "(?m)^- \[x\] Exit and re-enter play mode keeps" -and $notesText -match "(?m)^- \[x\] Edited/restored save data" -and $notesText -match "(?m)^- \[x\] Save/load result:\s*\S"

$productionReports = @(
    "AlienCrusherAudioResourceAssignmentChecklist.md",
    "AlienCrusherFormIdentityProductionChecklist.md",
    "AlienCrusherDestructionReadabilityChecklist.md",
    "AlienCrusherStreetPropVarietyChecklist.md",
    "AlienCrusherUiIconStatusChecklist.md",
    "AlienCrusherBossIdentityProductionChecklist.md",
    "AlienCrusherDistrictPaletteProductionChecklist.md",
    "AlienCrusherOutgameProgressionChecklist.md",
    "AlienCrusherRoutePayoffLayoutChecklist.md"
)

$missingProductionReports = [System.Collections.Generic.List[string]]::new()
foreach ($reportName in $productionReports) {
    $path = Join-Path $projectRoot "Logs\$reportName"
    if (-not (Test-Path -Path $path -PathType Leaf)) {
        $missingProductionReports.Add($reportName)
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Autonomous Work Backlog")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$lines.Add("Project: $projectRoot")
$lines.Add("")
$lines.Add("## Current Blocking State")
$lines.Add("- Real telemetry log: $(if (Test-Path -Path $telemetryLogPath -PathType Leaf) { "present" } else { "missing" })")
$lines.Add("- Telemetry summary: $(if (Test-Path -Path $telemetrySummaryPath -PathType Leaf) { "present" } else { "missing" })")
$lines.Add("- Meaningful stage notes: $(if ($hasStageNotes) { "partially present" } else { "missing" })")
$lines.Add("- Progression save smoke result: $(if ($saveSmokeComplete) { "complete" } else { "missing" })")
$lines.Add("- Rhythm/payoff/boss tuning: locked until Evidence Green")
$lines.Add("")
$lines.Add("## Safe Autonomous Work Queue")
$lines.Add("1. Run ``$readinessPrepPath`` with ``-IncludeProductionChecklists`` and inspect generated checklist gaps.")
$lines.Add("2. Keep ``$staticAuditsPath`` green after every tooling or documentation change.")
$lines.Add("3. Improve checklist/report wording when the next human action is buried or ambiguous.")
$lines.Add("4. Update ``$contextPath`` and ``$roadmapPath`` whenever the next safe unattended task changes.")
$lines.Add("5. Inspect ``$resourceBacklogPath`` for audio, HUD/status icon, boss identity, district palette, and route payoff priorities.")
$lines.Add("6. Inspect ``$architecturePlanPath`` for ROUTE HOLD / stage route / telemetry ownership, but do not refactor gameplay behavior before evidence.")
$lines.Add("")
$lines.Add("## Missing Production Checklist Outputs")
if ($missingProductionReports.Count -eq 0) {
    $lines.Add("- none")
}
else {
    foreach ($reportName in $missingProductionReports) {
        $lines.Add("- $reportName")
    }
}
$lines.Add("")
$lines.Add("## Game Fun Policy For Agents")
$lines.Add("- Treat fun as rhythm: opener -> pivot -> sustain -> payoff -> climax.")
$lines.Add("- Before Evidence Green, improve the player's future ability to observe rhythm; do not tune rhythm numbers.")
$lines.Add("- If an autonomous task does not help the player understand what to smash, what to chase, what changed, or what they earned, defer it.")
$lines.Add("- Use ``$policyPath`` as the decision guardrail.")
$lines.Add("")
$lines.Add("## Result")
$lines.Add("Result: autonomous work backlog generated")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
Write-Output $report

exit 0
