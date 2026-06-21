[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$UiFlowPath = "",
    [string]$UpgradeUiPath = "",
    [string]$StageEncounterPath = "",
    [string]$MetaProgressionPath = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Resolve-ProjectPath {
    param([string]$ProjectRoot, [string]$OverridePath, [string]$RelativePath)

    if (-not [string]::IsNullOrWhiteSpace($OverridePath)) {
        if ([System.IO.Path]::IsPathRooted($OverridePath)) {
            return $OverridePath
        }

        return Join-Path $ProjectRoot $OverridePath
    }

    return Join-Path $ProjectRoot $RelativePath
}

function Read-SourceText {
    param([string]$Path)

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        throw "Required source not found: $Path"
    }

    return Get-Content -Path $Path -Raw
}

function Add-MissingMarker {
    param(
        [System.Collections.Generic.List[string]]$Missing,
        [string]$Source,
        [string]$Needle
    )

    if (-not $Source.Contains($Needle)) {
        $Missing.Add($Needle)
    }
}

$projectRoot = Resolve-ProjectRoot
$uiFlowSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $UiFlowPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.UIFlow.cs"
$upgradeUiSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $UpgradeUiPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.UpgradeUI.cs"
$stageEncounterSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $StageEncounterPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.StageEncounter.cs"
$metaProgressionSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $MetaProgressionPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.MetaProgression.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherUiIconStatusChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$uiFlowText = Read-SourceText -Path $uiFlowSourcePath
$upgradeUiText = Read-SourceText -Path $upgradeUiSourcePath
$stageEncounterText = Read-SourceText -Path $stageEncounterSourcePath
$metaProgressionText = Read-SourceText -Path $metaProgressionSourcePath

$missingRuntimeMarkers = [System.Collections.Generic.List[string]]::new()
foreach ($needle in @(
    "FormatHudRouteIndicatorText",
    "hudRouteIndicatorText",
    "hudRouteArrowText",
    "hudStageGoalText",
    "hudProgressText",
    "lobbyMissionText",
    "resultBreakdownText",
    "OVERDRIVE",
    "BREAK WINDOW"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $uiFlowText -Needle $needle
}

foreach ($needle in @(
    "AppendUpgradeLabel",
    "SEISMIC",
    "CHAOS",
    "FRENZY"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $upgradeUiText -Needle $needle
}

foreach ($needle in @(
    "SHIELD",
    "CORE EXPOSED",
    "PHASE 2"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $stageEncounterText -Needle $needle
}

foreach ($needle in @(
    "lastRecommendedFormUnlock",
    "ApplyRecommendedFormButtonStyle",
    "GetRecommendedFormUnlock"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $metaProgressionText -Needle $needle
}

$iconCatalog = @(
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_DP"; RuntimeUse = "DP balance, rewards, meta purchases"; Shape = "cracked diamond or alien currency pip"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Stage"; RuntimeUse = "stage select and result stage identity"; Shape = "stacked district blocks with number badge space"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_NextStep"; RuntimeUse = "lobby/result next action recommendation"; Shape = "forward arrow with small target notch"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Route"; RuntimeUse = "ROUTE OPEN / ROUTE HOLD target"; Shape = "path line ending in beacon marker"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_BreakWindow"; RuntimeUse = "boss break window and burst timing"; Shape = "open cracked ring"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Shield"; RuntimeUse = "Sentinel shield/pylon blocking state"; Shape = "shield plate with pylon tick marks"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_WeakPoint"; RuntimeUse = "elite weak point and exposed core target"; Shape = "small bullseye with glow dot"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Overdrive"; RuntimeUse = "overdrive status/upgrade"; Shape = "speed ring with flame notch"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Panic"; RuntimeUse = "traffic panic / chaos upgrades"; Shape = "car with radial warning lines"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Seismic"; RuntimeUse = "seismic burst upgrade/status"; Shape = "ground crack wave"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Retail"; RuntimeUse = "retail frenzy and market chain state"; Shape = "shop awning with burst star"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Traffic"; RuntimeUse = "traffic density and vehicle state"; Shape = "road lane plus small car"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Icon_Boss"; RuntimeUse = "Justice Sentinel boss state"; Shape = "sentinel eye inside heavy frame"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Badge_Result_Clear"; RuntimeUse = "result success state"; Shape = "wide badge with upward shard"; Folder = "Assets/Resources/UI/Badges/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Badge_Result_Failure"; RuntimeUse = "result failure bucket state"; Shape = "wide badge with broken route notch"; Folder = "Assets/Resources/UI/Badges/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Badge_Locked"; RuntimeUse = "locked form/meta state"; Shape = "small lock plate"; Folder = "Assets/Resources/UI/Badges/" },
    [pscustomobject]@{ Priority = "P1"; Asset = "Badge_Recommended"; RuntimeUse = "recommended next upgrade/form"; Shape = "small focus chevron"; Folder = "Assets/Resources/UI/Badges/" }
)

$productionBatches = @(
    [pscustomobject]@{
        Batch = "A. Run essentials"
        Goal = "replace repeated HUD/lobby/result reading with fast universal symbols"
        Targets = @("Icon_DP", "Icon_Stage", "Icon_NextStep", "Icon_Route")
        Acceptance = "player can identify money, current stage, next action, and route state before reading text"
    },
    [pscustomobject]@{
        Batch = "B. Route and boss readability"
        Goal = "make high-pressure targets and boss states scannable on mobile"
        Targets = @("Icon_BreakWindow", "Icon_Shield", "Icon_WeakPoint", "Icon_Boss")
        Acceptance = "route target, shield, weak point, break window, and boss state do not share silhouettes"
    },
    [pscustomobject]@{
        Batch = "C. Upgrade and chaos status"
        Goal = "compress upgrade/status words into recognizable gameplay mode symbols"
        Targets = @("Icon_Overdrive", "Icon_Panic", "Icon_Seismic", "Icon_Retail", "Icon_Traffic")
        Acceptance = "upgrade/status icons remain readable beside short HUD labels without crowding"
    },
    [pscustomobject]@{
        Batch = "D. Result and recommendation badges"
        Goal = "make clear, failure, locked, and recommended states visible before reading breakdown copy"
        Targets = @("Badge_Result_Clear", "Badge_Result_Failure", "Badge_Locked", "Badge_Recommended")
        Acceptance = "result/lobby cards show success, failure, lock, and recommendation states at a glance"
    }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher UI Icon And Status Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`, `{3}`' -f $uiFlowSourcePath, $upgradeUiSourcePath, $stageEncounterSourcePath, $metaProgressionSourcePath))
$lines.Add("")
$lines.Add("Purpose: convert the current text-heavy HUD, lobby, result, upgrade, route, and boss states into a concrete icon/status production list.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime markers missing: $(if ($missingRuntimeMarkers.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingRuntimeMarkers) })")
$lines.Add("- Checklist rows: $($iconCatalog.Count)")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Create route, next-step, stage, and DP icons first because they support every run.")
$lines.Add("2. Create weak point, shield, break window, and boss icons for Stage 4+ readability.")
$lines.Add("3. Create overdrive, panic, seismic, retail, and traffic icons for upgrade/status scanning.")
$lines.Add("4. Create result and recommendation badges after the icon silhouettes are stable.")
$lines.Add("5. Verify mobile HUD readability with icons present, not only text best-fit.")
$lines.Add("")
$lines.Add("## Production Batches")
$lines.Add("| Batch | Goal | Targets | Acceptance check |")
$lines.Add("|---|---|---|---|")
foreach ($batch in $productionBatches) {
    $targetNames = @($batch.Targets | ForEach-Object { "``$_``" })
    $lines.Add(('| {0} | {1} | {2} | {3} |' -f $batch.Batch, $batch.Goal, ([string]::Join(", ", $targetNames)), $batch.Acceptance))
}

$lines.Add("")
$lines.Add("## Next UI Icon Batch Task Card")
if ($productionBatches.Count -eq 0) {
    $lines.Add("- none")
}
else {
    $nextBatch = $productionBatches[0]
    $targetNames = @($nextBatch.Targets | ForEach-Object { "``$_``" })
    $runtimeUses = @()
    foreach ($targetName in $nextBatch.Targets) {
        $icon = $iconCatalog | Where-Object { $_.Asset -eq $targetName } | Select-Object -First 1
        if ($null -ne $icon) {
            $runtimeUses += "$($icon.Asset): $($icon.RuntimeUse)"
        }
    }

    $lines.Add("- Batch: $($nextBatch.Batch)")
    $lines.Add("- Goal: $($nextBatch.Goal)")
    $lines.Add("- Targets: $([string]::Join(', ', $targetNames))")
    $lines.Add("- Runtime uses: $([string]::Join('; ', $runtimeUses))")
    $lines.Add("- Acceptance: $($nextBatch.Acceptance)")
    $lines.Add("- Done means: DP, stage, next-step, and route icons have draft silhouettes, mobile readability notes, or explicit placeholder rules before HUD copy changes.")
}

$lines.Add("")
$lines.Add("## Current UI Icon And Status Targets")
$lines.Add("| Priority | Asset | Runtime use | Shape target | Folder | Done? |")
$lines.Add("|---|---|---|---|---|---|")
foreach ($icon in $iconCatalog) {
    $lines.Add(("| {0} | `{1}` | {2} | {3} | `{4}` | [ ] |" -f $icon.Priority, $icon.Asset, $icon.RuntimeUse, $icon.Shape, $icon.Folder))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Icons should reduce reading load, not add extra HUD clutter.")
$lines.Add("- Route and boss icons must be distinguishable in peripheral vision.")
$lines.Add("- Upgrade/status icons should use the same visual grammar as result badges.")
$lines.Add("- Keep icons simple enough to read on a mobile-sized viewport.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "UI icon/status checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
