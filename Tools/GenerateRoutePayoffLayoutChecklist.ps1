[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$ProgressionCorePath = "",
    [string]$RuntimeMapPath = "",
    [string]$UiFlowPath = "",
    [string]$StageChecklistPath = "",
    [string]$ClusterMarkerVfxPath = "",
    [string]$HoldSuccessVfxPath = "",
    [string]$RouteOpenTrailVfxPath = "",
    [string]$LaneBreakResidualVfxPath = ""
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
$progressionCoreSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ProgressionCorePath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.ProgressionCore.cs"
$runtimeMapSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $RuntimeMapPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.RuntimeMapFallback.cs"
$uiFlowSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $UiFlowPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.UIFlow.cs"
$stageChecklistSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $StageChecklistPath -RelativePath "Tools\GenerateStagePlaytestChecklist.ps1"
$clusterMarkerVfxSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ClusterMarkerVfxPath -RelativePath "Assets\Scripts\Runtime\Systems\RouteClusterMarkerVfx.cs"
$holdSuccessVfxSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $HoldSuccessVfxPath -RelativePath "Assets\Scripts\Runtime\Systems\RouteHoldSuccessVfx.cs"
$routeOpenTrailVfxSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $RouteOpenTrailVfxPath -RelativePath "Assets\Scripts\Runtime\Systems\RouteOpenTrailVfx.cs"
$laneBreakResidualVfxSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $LaneBreakResidualVfxPath -RelativePath "Assets\Scripts\Runtime\Systems\LaneBreakResidualVfx.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherRoutePayoffLayoutChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$progressionCoreText = Read-SourceText -Path $progressionCoreSourcePath
$runtimeMapText = Read-SourceText -Path $runtimeMapSourcePath
$uiFlowText = Read-SourceText -Path $uiFlowSourcePath
$stageChecklistText = Read-SourceText -Path $stageChecklistSourcePath
$clusterMarkerVfxText = Read-SourceText -Path $clusterMarkerVfxSourcePath
$holdSuccessVfxText = Read-SourceText -Path $holdSuccessVfxSourcePath
$routeOpenTrailVfxText = Read-SourceText -Path $routeOpenTrailVfxSourcePath
$laneBreakResidualVfxText = Read-SourceText -Path $laneBreakResidualVfxSourcePath
$allRoutePayoffHookText = $progressionCoreText + $runtimeMapText + $uiFlowText + $clusterMarkerVfxText + $holdSuccessVfxText + $routeOpenTrailVfxText + $laneBreakResidualVfxText

$missingRuntimeMarkers = [System.Collections.Generic.List[string]]::new()
foreach ($needle in @(
    "EvaluateStageAdvanceRouteReward",
    "ROUTE BONUS ->",
    "GetRouteDistrictPayoffLabel",
    "GetRouteDistrictPayoffColor",
    "GetRouteDistrictPayoffLayoutId",
    "PAYOFF_ParkCut_Layout",
    "PAYOFF_MarketChain_Layout",
    "PAYOFF_YardBlast_Layout",
    "PAYOFF_PowerSurge_Layout",
    "PAYOFF_SkylineBreach_Layout",
    "GetDistrictPayoffClusterOffset",
    "SpawnStageAdvanceRewardProps",
    "PreviewStageAdvanceFollowupTarget",
    "SpawnForwardSmashRewardCluster",
    "SpawnSkylineRouteClusterAnchor",
    "EvaluateForwardSmashBonus",
    "EvaluateRouteHoldBonus",
    "RouteHoldSuccessVfx",
    "RouteOpenTrailVfx",
    "LaneBreakResidualVfx",
    "FORWARD SMASH +",
    "RouteClusterMarker",
    "routeRewardClusterRadius",
    "routeRewardClusterPropCount"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $progressionCoreText -Needle $needle
}

foreach ($needle in @(
    "VFX_RouteCluster_Marker",
    "RouteClusterMarker",
    "EnsureRouteClusterMarker"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $clusterMarkerVfxText -Needle $needle
}

foreach ($needle in @(
    "VFX_RouteHold_Success",
    "RouteHoldSuccessVfx"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $holdSuccessVfxText -Needle $needle
}

foreach ($needle in @(
    "VFX_RouteOpen_Trail",
    "RouteOpenTrailVfx"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $routeOpenTrailVfxText -Needle $needle
}

foreach ($needle in @(
    "VFX_LaneBreak_Residual",
    "LaneBreakResidualVfx"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $laneBreakResidualVfxText -Needle $needle
}

foreach ($needle in @(
    "EnsureCommercialBenchRuntime",
    "EnsureStreetTreeRuntime",
    "EnsureCommercialKioskRuntime",
    "EnsureCommercialVendingRuntime",
    "EnsureExplosiveBarrelRuntime",
    "EnsureTransformerRuntime",
    "Stage02_PocketPark",
    "Stage03_MarketPlaza",
    "Stage05_ConstructionYard",
    "Stage06_PowerBlock",
    "Stage07_SkylineBlock"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $runtimeMapText -Needle $needle
}

foreach ($needle in @(
    "Smash opened cluster",
    "FORWARD TARGET",
    "SMASH"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $uiFlowText -Needle $needle
}

foreach ($needle in @(
    "Resolve-RoutePayoff",
    "Route reward opens about",
    "Route payoff identity matches the expected district"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $stageChecklistText -Needle $needle
}

$layoutCatalog = @(
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 1"; Payoff = "Starter Cluster"; RuntimeLabel = "CLUSTER OPEN"; LayoutRule = "small clear cluster close to route target"; Asset = "PAYOFF_StarterCluster_Layout"; Folder = "Assets/Art/Layouts/RoutePayoff/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 2"; Payoff = "Park Cut"; RuntimeLabel = "PARK CUT OPEN"; LayoutRule = "bench/tree/barrel recovery cut with low clutter"; Asset = "PAYOFF_ParkCut_Layout"; Folder = "Assets/Art/Layouts/RoutePayoff/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 3-4"; Payoff = "Market Chain"; RuntimeLabel = "MARKET CHAIN OPEN"; LayoutRule = "tight kiosk/vending/barrel chainable cluster"; Asset = "PAYOFF_MarketChain_Layout"; Folder = "Assets/Art/Layouts/RoutePayoff/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 5"; Payoff = "Yard Blast"; RuntimeLabel = "YARD BLAST OPEN"; LayoutRule = "wider barrel-heavy blast spacing plus one utility target"; Asset = "PAYOFF_YardBlast_Layout"; Folder = "Assets/Art/Layouts/RoutePayoff/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 6"; Payoff = "Power Surge"; RuntimeLabel = "POWER SURGE OPEN"; LayoutRule = "transformer corridor with one barrel punctuation"; Asset = "PAYOFF_PowerSurge_Layout"; Folder = "Assets/Art/Layouts/RoutePayoff/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 7"; Payoff = "Skyline Breach"; RuntimeLabel = "SKYLINE BREACH OPEN"; LayoutRule = "asymmetric anchor-first tower plus transformer/barrel ring"; Asset = "PAYOFF_SkylineBreach_Layout"; Folder = "Assets/Art/Layouts/RoutePayoff/" },
    [pscustomobject]@{ Priority = "P1"; StageBand = "Global"; Payoff = "Route Cluster Marker"; RuntimeLabel = "RouteClusterMarker"; LayoutRule = "floor read that frames the opened cluster without hiding props"; Asset = "VFX_RouteCluster_Marker"; Folder = "Assets/Art/VFX/Route/" },
    [pscustomobject]@{ Priority = "P1"; StageBand = "Global"; Payoff = "Forward Smash Confirmation"; RuntimeLabel = "FORWARD SMASH"; LayoutRule = "impact ring and camera beat stronger than route open, weaker than boss down"; Asset = "VFX_ForwardSmash_Confirm"; Folder = "Assets/Art/VFX/Route/" },
    [pscustomobject]@{ Priority = "P1"; StageBand = "Global"; Payoff = "HOLD Success Pulse"; RuntimeLabel = "ROUTE HOLD"; LayoutRule = "gold-cyan lock ring at HOLD clear that aims at ROUTE BONUS without using smash-star or combo ticks"; Asset = "VFX_RouteHold_Success"; Folder = "Assets/Art/VFX/Route/" },
    [pscustomobject]@{ Priority = "P1"; StageBand = "Global"; Payoff = "ROUTE OPEN Trail Pulse"; RuntimeLabel = "ROUTE OPEN"; LayoutRule = "magenta path dashes that race toward the beacon when LANE BREAK flips to ROUTE OPEN"; Asset = "VFX_RouteOpen_Trail"; Folder = "Assets/Art/VFX/Route/" },
    [pscustomobject]@{ Priority = "P1"; StageBand = "Global"; Payoff = "LANE BREAK Residual Flash"; RuntimeLabel = "LANE BREAK"; LayoutRule = "tiny ivory-ash residual crack at the wreck that completed LANE BREAK"; Asset = "VFX_LaneBreak_Residual"; Folder = "Assets/Art/VFX/Route/" }
)

$productionBatches = @(
    [pscustomobject]@{
        Batch = "A. District payoff layouts"
        Goal = "make each ROUTE BONUS cluster read as a district-specific reward"
        Targets = @("PAYOFF_ParkCut_Layout", "PAYOFF_MarketChain_Layout", "PAYOFF_YardBlast_Layout", "PAYOFF_PowerSurge_Layout", "PAYOFF_SkylineBreach_Layout")
        Acceptance = "Stage 2/3-4/5/6/7 payoff screenshots differ by spacing, silhouette, and chase direction"
    },
    [pscustomobject]@{
        Batch = "B. Cluster marker readability"
        Goal = "frame the opened cluster without hiding route targets or HOLD pips"
        Targets = @("VFX_RouteCluster_Marker", "RouteClusterMarker")
        Acceptance = "Target_A/Target_B, the next SMASH target, and route trail pips remain readable over the marker"
    },
    [pscustomobject]@{
        Batch = "C. Forward Smash confirmation"
        Goal = "make the cash-out beat feel earned after route reading"
        Targets = @("VFX_ForwardSmash_Confirm", "SFX_Route_Bonus")
        Acceptance = "Forward Smash feels bigger than a normal break and shorter than boss down or stage clear"
    },
    [pscustomobject]@{
        Batch = "D. HOLD success pulse"
        Goal = "make ROUTE HOLD clear read as a distinct flip into ROUTE BONUS / Forward Smash"
        Targets = @("VFX_RouteHold_Success")
        Acceptance = "HOLD success shows a gold-cyan lock ring that is not combo ticks, Overdrive chevrons, or the smash star"
    },
    [pscustomobject]@{
        Batch = "E. ROUTE OPEN trail pulse"
        Goal = "make LANE BREAK -> ROUTE OPEN read as a path opening toward the beacon"
        Targets = @("VFX_RouteOpen_Trail")
        Acceptance = "ROUTE OPEN shows magenta path dashes that are not HOLD lock ring, smash star, combo ticks, or Overdrive chevrons"
    },
    [pscustomobject]@{
        Batch = "F. LANE BREAK residual flash"
        Goal = "leave a tiny residual impact mark on the wreck that completed LANE BREAK"
        Targets = @("VFX_LaneBreak_Residual")
        Acceptance = "LANE BREAK shows a short ivory-ash crack flash that is not OPEN dashes, HOLD lock ring, smash star, combo ticks, or Overdrive chevrons"
    }
)

$rhythmRows = @(
    [pscustomobject]@{ Beat = "Open"; RuntimeSignal = "LANE BREAK -> ROUTE OPEN"; ProductionRule = "show the next target before reward props appear" },
    [pscustomobject]@{ Beat = "Hold"; RuntimeSignal = "ROUTE HOLD meter and beacon"; ProductionRule = "cluster should not steal attention until the hold succeeds" },
    [pscustomobject]@{ Beat = "Reveal"; RuntimeSignal = "ROUTE BONUS -> district label"; ProductionRule = "spawn/mark the district payoff cluster as a visible reward" },
    [pscustomobject]@{ Beat = "Chase"; RuntimeSignal = "SMASH / FORWARD TARGET"; ProductionRule = "make the opened cluster pull the player forward, not sideways into noise" },
    [pscustomobject]@{ Beat = "Cash Out"; RuntimeSignal = "FORWARD SMASH + score"; ProductionRule = "confirmation must feel bigger than normal destruction but shorter than stage clear" }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Route Payoff Layout Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`, `{3}`' -f $progressionCoreSourcePath, $runtimeMapSourcePath, $uiFlowSourcePath, $stageChecklistSourcePath))
$lines.Add("")
$lines.Add("Purpose: turn ROUTE BONUS, district payoff props, cluster markers, and Forward Smash into concrete layout/readability production targets.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime markers missing: $(if ($missingRuntimeMarkers.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingRuntimeMarkers) })")
$lines.Add("- Checklist rows: $($layoutCatalog.Count)")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Preserve target and beacon readability first; route payoff props should appear after ROUTE HOLD succeeds.")
$lines.Add("2. Produce Stage 2, Stage 3-4, Stage 5, Stage 6, and Stage 7 payoff layout prefabs or layout rules from the current runtime prop mix.")
$lines.Add("3. Add a subtle route cluster floor marker so the opened cluster reads as intentional.")
$lines.Add("4. Add Forward Smash confirmation VFX/SFX after the cluster is visually readable.")
$lines.Add('5. Verify Stage 1 / 4 / 7 screenshots before changing `routeRewardClusterRadius` or `routeRewardClusterPropCount`.')
$lines.Add("")
$lines.Add("## Production Batches")
$lines.Add("| Batch | Goal | Targets | Acceptance check |")
$lines.Add("|---|---|---|---|")
foreach ($batch in $productionBatches) {
    $targetNames = @($batch.Targets | ForEach-Object { "``$_``" })
    $lines.Add(('| {0} | {1} | {2} | {3} |' -f $batch.Batch, $batch.Goal, ([string]::Join(", ", $targetNames)), $batch.Acceptance))
}

$lines.Add("")
$lines.Add("## Next Route Payoff Batch Task Card")
if ($productionBatches.Count -eq 0) {
    $lines.Add("- none")
}
else {
    $nextBatch = $productionBatches[0]
    $targetNames = @($nextBatch.Targets | ForEach-Object { "``$_``" })
    $stageBands = @()
    foreach ($targetName in $nextBatch.Targets) {
        $layout = $layoutCatalog | Where-Object { $_.Asset -eq $targetName } | Select-Object -First 1
        if ($null -ne $layout) {
            $stageBands += "$($layout.StageBand): $($layout.Payoff)"
        }
    }

    $lines.Add("- Batch: $($nextBatch.Batch)")
    $lines.Add("- Goal: $($nextBatch.Goal)")
    $lines.Add("- Targets: $([string]::Join(', ', $targetNames))")
    $lines.Add("- Stage bands: $([string]::Join('; ', $stageBands))")
    $lines.Add("- Acceptance: $($nextBatch.Acceptance)")
    $lines.Add("- Done means: every district payoff target has a draft layout, screenshot note, or explicit placeholder rule before reward cluster tuning changes.")
}

$lines.Add("")
$lines.Add("## Route Payoff Rhythm Contract")
$lines.Add("| Beat | Runtime signal | Production rule | Done? |")
$lines.Add("|---|---|---|---|")
foreach ($row in $rhythmRows) {
    $lines.Add(("| {0} | {1} | {2} | [ ] |" -f $row.Beat, $row.RuntimeSignal, $row.ProductionRule))
}

$lines.Add("")
$lines.Add("## Current Route Payoff Layout Targets")
$lines.Add("| Priority | Stage band | Payoff | Runtime label | Layout rule | Asset | Folder | Done? |")
$lines.Add("|---|---|---|---|---|---|---|---|")
foreach ($layout in $layoutCatalog) {
    $doneMark = if ($allRoutePayoffHookText.Contains($layout.Asset)) { "[x]" } else { "[ ]" }
    $lines.Add(("| {0} | {1} | {2} | `{3}` | {4} | `{5}` | `{6}` | {7} |" -f $layout.Priority, $layout.StageBand, $layout.Payoff, $layout.RuntimeLabel, $layout.LayoutRule, $layout.Asset, $layout.Folder, $doneMark))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- ROUTE BONUS must read as the city opening up, not just a floating score number.")
$lines.Add("- Market, construction, power, and skyline payoff layouts should differ by spacing, not only by prop type.")
$lines.Add("- Do not increase cluster radius/count until playtest evidence says the current cluster is invisible or cramped.")
$lines.Add("- Route cluster VFX must not hide Target_A/Target_B, HOLD trail pips, or the next Forward Smash target.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Route payoff layout checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
