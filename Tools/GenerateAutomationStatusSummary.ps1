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

function Get-LastResultLine {
    param(
        [string]$Path
    )

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        return "missing"
    }

    $result = Select-String -Path $Path -Pattern "^Result:" | Select-Object -Last 1
    if ($null -eq $result) {
        return "no Result line"
    }

    return $result.Line
}

function Get-FirstResultLineMatching {
    param(
        [string]$Path,
        [string]$Pattern
    )

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        return "missing"
    }

    $result = Select-String -Path $Path -Pattern $Pattern | Select-Object -First 1
    if ($null -eq $result) {
        return "no matching Result line"
    }

    return $result.Line
}

function Get-FirstMatchingLines {
    param(
        [string]$Path,
        [string]$Pattern,
        [int]$Count = 5
    )

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        return @()
    }

    return @(Select-String -Path $Path -Pattern $Pattern | Select-Object -First $Count | ForEach-Object { $_.Line })
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
$resolvedReportPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ReportPath -RelativePath "Logs\AlienCrusherAutomationStatusSummary.md"
$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$staticAuditsPath = Join-Path $projectRoot "Logs\AlienCrusherStaticAudits.log"
$readinessPrepPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestReadinessPrep.log"
$evidenceGatePath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestEvidenceGate.log"
$autonomousBacklogPath = Join-Path $projectRoot "Logs\AlienCrusherAutonomousWorkBacklog.md"
$resourceBacklogPath = Join-Path $projectRoot "Logs\AlienCrusherResourceProductionBacklog.md"
$architecturePlanPath = Join-Path $projectRoot "Logs\AlienCrusherArchitectureExtractionPlan.md"
$telemetryLogPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetry.log"
$stageNotesPath = Join-Path $projectRoot "Docs\AlienCrusherStagePlaytestNotes.md"
$markdownTick = [char]96

$realTelemetryStatus = if (Test-Path -Path $telemetryLogPath -PathType Leaf) { "present" } else { "missing" }
$stageNotesText = if (Test-Path -Path $stageNotesPath -PathType Leaf) { Get-Content -Path $stageNotesPath -Raw } else { "" }
$requiredNoteFields = @("Readability", "Route pressure", "Map identity", "Rhythm identity")
$requiredStageNoteCount = 7 * $requiredNoteFields.Count
$meaningfulStageNoteCount = 0
foreach ($stageNumber in 1..7) {
    foreach ($fieldName in $requiredNoteFields) {
        if (Test-StageMeaningfulNoteField -NotesText $stageNotesText -StageNumber $stageNumber -FieldName $fieldName) {
            $meaningfulStageNoteCount++
        }
    }
}

$stageNoteStatus = if ($meaningfulStageNoteCount -eq 0) {
    "missing"
}
elseif ($meaningfulStageNoteCount -lt $requiredStageNoteCount) {
    "partial"
}
else {
    "complete"
}

$saveSmokeStatus = if ($stageNotesText -match "(?m)^- \[x\] Save/load result:\s*\S") { "present" } else { "missing" }
$resourceItemResult = Get-FirstResultLineMatching -Path $resourceBacklogPath -Pattern "^Result: resource production backlog generated"
$resourceBatchResult = Get-FirstResultLineMatching -Path $resourceBacklogPath -Pattern "^Result: production batch focus generated"
$resourceBatchOrderResult = Get-FirstResultLineMatching -Path $resourceBacklogPath -Pattern "^Result: recommended production batch order generated"
$productionChecklistScripts = @(
    "GenerateAudioResourceAssignmentChecklist.ps1",
    "GenerateFormIdentityProductionChecklist.ps1",
    "GenerateDestructionReadabilityChecklist.ps1",
    "GenerateStreetPropVarietyChecklist.ps1",
    "GenerateUiIconStatusChecklist.ps1",
    "GenerateBossIdentityProductionChecklist.ps1",
    "GenerateDistrictPaletteProductionChecklist.ps1",
    "GenerateOutgameProgressionChecklist.ps1",
    "GenerateRoutePayoffLayoutChecklist.ps1"
)
$productionChecklistCardCount = 0
foreach ($scriptName in $productionChecklistScripts) {
    $scriptPath = Join-Path $PSScriptRoot $scriptName
    if ((Test-Path -Path $scriptPath -PathType Leaf) -and (Get-Content -Path $scriptPath -Raw).Contains("Batch Task Card")) {
        $productionChecklistCardCount++
    }
}
$productionChecklistCardStatus = "$productionChecklistCardCount / $($productionChecklistScripts.Count) checklist generators include next-batch task cards"
$architectureResult = Get-LastResultLine -Path $architecturePlanPath
$evidenceResult = Get-LastResultLine -Path $evidenceGatePath
$prepResult = Get-LastResultLine -Path $readinessPrepPath

$safeWorkLines = Get-FirstMatchingLines -Path $autonomousBacklogPath -Pattern "^\d+\. " -Count 6
$resourceOrderLines = Get-FirstMatchingLines -Path $resourceBacklogPath -Pattern "^\d+\. " -Count 5
$architectureOrderLines = Get-FirstMatchingLines -Path $architecturePlanPath -Pattern "^\d+\. " -Count 6

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Automation Status Summary")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$lines.Add("Project: $projectRoot")
$lines.Add("")
$lines.Add("## Progress")
$lines.Add("- Core loop direction is implemented around Stage Start -> LANE BREAK -> ROUTE OPEN -> ROUTE HOLD -> ROUTE BONUS / Forward Smash -> result growth.")
$lines.Add("- Autonomous readiness now generates stage checklist, autonomous work backlog, resource production backlog, architecture extraction plan, telemetry summary readiness, and Evidence Gate readiness.")
$lines.Add("- Resource planning is consolidated: $resourceItemResult")
$lines.Add("- Resource production batches are consolidated: $resourceBatchResult")
$lines.Add("- Resource production batch order is consolidated: $resourceBatchOrderResult")
$lines.Add("- Production checklist task cards are consolidated: $productionChecklistCardStatus")
$lines.Add("- Architecture planning is consolidated: $architectureResult")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Readiness prep: $prepResult")
$lines.Add("- Evidence gate readiness: $evidenceResult")
$lines.Add("- Real playtest telemetry: $realTelemetryStatus")
$lines.Add("- Meaningful stage notes: $stageNoteStatus ($meaningfulStageNoteCount / $requiredStageNoteCount fields)")
$lines.Add("- Progression save smoke result: $saveSmokeStatus")
$lines.Add("- Static audits should remain the commit gate for tooling/documentation changes.")
$lines.Add("")
$lines.Add("## Current Blocker")
$lines.Add("- Evidence Green is not ready until a real editor/development `F10` Stage 1-7 sweep, meaningful stage notes, and a concrete save/load smoke result exist.")
$lines.Add("- Do not tune route timing, payoff counts, target placement, stage rhythm presets, or boss pressure before Evidence Green.")
$lines.Add("")
$lines.Add("## Next To-Do")
$lines.Add("1. Run `Tools/RunPlaytestReadinessPrep.ps1 -IncludeProductionChecklists` before any resource-focused unattended work.")
$lines.Add("2. Use ${markdownTick}Logs/AlienCrusherResourceProductionBacklog.md${markdownTick} ${markdownTick}## Recommended Production Batch Order${markdownTick} to pick the next complete production batch.")
$lines.Add("3. Use `Logs/AlienCrusherArchitectureExtractionPlan.md` for extraction planning only; do not refactor behavior before evidence.")
$lines.Add("4. When the creator is available, run the real `F10` Stage 1-7 sweep and fill `Docs/AlienCrusherStagePlaytestNotes.md`.")
$lines.Add("")
$lines.Add("## Safe Autonomous Queue")
if ($safeWorkLines.Count -eq 0) {
    $lines.Add("- missing autonomous queue; regenerate `Tools/GenerateAutonomousWorkBacklog.ps1`.")
}
else {
    foreach ($line in $safeWorkLines) {
        $lines.Add("- $line")
    }
}
$lines.Add("")
$lines.Add("## Resource Order")
if ($resourceOrderLines.Count -eq 0) {
    $lines.Add("- missing resource order; regenerate `Tools/GenerateResourceProductionBacklog.ps1`.")
}
else {
    foreach ($line in $resourceOrderLines) {
        $lines.Add("- $line")
    }
}
$lines.Add("")
$lines.Add("## Architecture Order")
if ($architectureOrderLines.Count -eq 0) {
    $lines.Add("- missing architecture order; regenerate `Tools/GenerateArchitectureExtractionPlan.ps1`.")
}
else {
    foreach ($line in $architectureOrderLines) {
        $lines.Add("- $line")
    }
}
$lines.Add("")
$lines.Add("## Result")
$lines.Add("Result: automation status summary generated")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
Write-Output $report

exit 0
