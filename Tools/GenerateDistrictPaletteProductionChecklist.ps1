[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$RuntimeMapPath = "",
    [string]$ControllerPath = "",
    [string]$UiFlowPath = "",
    [string]$SystemBootstrapPath = ""
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
$runtimeMapSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $RuntimeMapPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.RuntimeMapFallback.cs"
$controllerSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ControllerPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.cs"
$uiFlowSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $UiFlowPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.UIFlow.cs"
$systemBootstrapSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $SystemBootstrapPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.SystemBootstrap.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherDistrictPaletteProductionChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$runtimeMapText = Read-SourceText -Path $runtimeMapSourcePath
$controllerText = Read-SourceText -Path $controllerSourcePath
$uiFlowText = Read-SourceText -Path $uiFlowSourcePath
$systemBootstrapText = Read-SourceText -Path $systemBootstrapSourcePath

$missingRuntimeMarkers = [System.Collections.Generic.List[string]]::new()
foreach ($needle in @(
    "RuntimeCityThemeProfile",
    "DenseCore",
    "IndustrialHarbor",
    "GardenResidential"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $controllerText -Needle $needle
}

foreach ($needle in @(
    "ResolveFallbackCityTheme",
    "EnsureRuntimeDistrictLandmarks",
    "EnsurePocketParkLandmarkRuntime",
    "EnsureMarketLandmarkRuntime",
    "EnsureSentinelApproachLandmarkRuntime",
    "EnsureConstructionLandmarkRuntime",
    "EnsurePowerLandmarkRuntime",
    "EnsureSkylineLandmarkRuntime",
    "Landmark_PocketPark_Pad",
    "Landmark_Market_Pad",
    "Landmark_Sentinel_CheckpointPad",
    "YardPad",
    "PowerPad",
    "Landmark_Skyline_Plaza",
    "Target_A",
    "Target_B"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $runtimeMapText -Needle $needle
}

foreach ($needle in @(
    "DescribeStageDistrict",
    "low-rise suburb",
    "mixed commercial strip",
    "dense core",
    "fortified skyline"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $uiFlowText -Needle $needle
}

foreach ($needle in @(
    "RenderSettings.ambientLight"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $systemBootstrapText -Needle $needle
}

$paletteCatalog = @(
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 1"; District = "Starter Residential"; RuntimeAnchor = "GardenResidential / starter cluster"; Asset = "PAL_District_StarterResidential"; PaletteNeed = "clean road, low-rise blocks, friendly starter contrast"; Folder = "Assets/Art/Palettes/Districts/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 2"; District = "Pocket Park"; RuntimeAnchor = "Stage02_PocketPark"; Asset = "PAL_District_PocketPark"; PaletteNeed = "green breathing-space pad, trees, benches, soft route cut"; Folder = "Assets/Art/Palettes/Districts/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 3"; District = "Market Plaza"; RuntimeAnchor = "Stage03_MarketPlaza"; Asset = "PAL_District_MarketPlaza"; PaletteNeed = "signage, kiosk color, sidewalk contrast, chain density"; Folder = "Assets/Art/Palettes/Districts/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 4"; District = "Sentinel Checkpoint"; RuntimeAnchor = "Stage04_SentinelCheckpoint"; Asset = "PAL_District_SentinelCheckpoint"; PaletteNeed = "defense-front warning palette before boss rules dominate"; Folder = "Assets/Art/Palettes/Districts/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 5"; District = "Construction Yard"; RuntimeAnchor = "Stage05_ConstructionYard"; Asset = "PAL_District_ConstructionYard"; PaletteNeed = "hazard stripes, containers, barrels, wider blast payoff"; Folder = "Assets/Art/Palettes/Districts/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 6"; District = "Power Block"; RuntimeAnchor = "Stage06_PowerBlock"; Asset = "PAL_District_PowerBlock"; PaletteNeed = "transformer risk, electric route line, utility warning language"; Folder = "Assets/Art/Palettes/Districts/" },
    [pscustomobject]@{ Priority = "P0"; StageBand = "Stage 7"; District = "Skyline Block"; RuntimeAnchor = "Stage07_SkylineBlock"; Asset = "PAL_District_SkylineBlock"; PaletteNeed = "late anchor towers, plaza route line, high-value crown accent"; Folder = "Assets/Art/Palettes/Districts/" },
    [pscustomobject]@{ Priority = "P1"; StageBand = "Global"; District = "Route Marker Tint Set"; RuntimeAnchor = "Target_A / Target_B / routeColor"; Asset = "PAL_RouteMarker_Tints"; PaletteNeed = "route markers must stay readable over every district palette"; Folder = "Assets/Art/Palettes/UI/" },
    [pscustomobject]@{ Priority = "P1"; StageBand = "Global"; District = "Ambient/Fog Set"; RuntimeAnchor = "RenderSettings.ambientLight"; Asset = "PAL_Ambient_StageBands"; PaletteNeed = "subtle progression tone without hiding gameplay silhouettes"; Folder = "Assets/Art/Palettes/Lighting/" }
)

$productionBatches = @(
    [pscustomobject]@{
        Batch = "A. Route tint readability"
        Goal = "protect Target_A/Target_B, routeColor, and HOLD trail pips across every district"
        Targets = @("PAL_RouteMarker_Tints", "Target_A", "Target_B", "routeColor")
        Acceptance = "route markers remain the highest-contrast navigation signal in Stage 1, 4, and 7 screenshots"
    },
    [pscustomobject]@{
        Batch = "B. Core rhythm palettes"
        Goal = "separate opener, density pivot, boss approach, and late climax first"
        Targets = @("PAL_District_StarterResidential", "PAL_District_MarketPlaza", "PAL_District_SentinelCheckpoint", "PAL_District_SkylineBlock")
        Acceptance = "Stage 1/3/4/7 screenshots do not look interchangeable before reading HUD text"
    },
    [pscustomobject]@{
        Batch = "C. Secondary variation palettes"
        Goal = "give breathing-space, explosive payoff, and utility risk their own color language"
        Targets = @("PAL_District_PocketPark", "PAL_District_ConstructionYard", "PAL_District_PowerBlock")
        Acceptance = "Stage 2/5/6 palettes signal calmer route cut, blast payoff, and transformer risk respectively"
    },
    [pscustomobject]@{
        Batch = "D. Ambient stage bands"
        Goal = "add broad progression tone only after target and prop readability survives"
        Targets = @("PAL_Ambient_StageBands", "RenderSettings.ambientLight")
        Acceptance = "ambient changes never reduce building tier, route target, or payoff prop readability"
    }
)

$stageRhythmRows = @(
    [pscustomobject]@{ StageBand = "Stage 1"; RhythmRole = "opener"; PaletteRule = "low clutter, clear starter lane, high contrast easy targets" },
    [pscustomobject]@{ StageBand = "Stage 2"; RhythmRole = "breathing variation"; PaletteRule = "park greens should feel calmer than the starter street" },
    [pscustomobject]@{ StageBand = "Stage 3"; RhythmRole = "chain-density pivot"; PaletteRule = "market color can be busier, but route tint must still win" },
    [pscustomobject]@{ StageBand = "Stage 4"; RhythmRole = "boss-approach warning"; PaletteRule = "Sentinel colors must foreshadow boss danger before combat" },
    [pscustomobject]@{ StageBand = "Stage 5"; RhythmRole = "explosive payoff"; PaletteRule = "construction hazard colors should point to barrels and wide blast lanes" },
    [pscustomobject]@{ StageBand = "Stage 6"; RhythmRole = "utility risk"; PaletteRule = "electric accents should guide transformer chains, not become decoration" },
    [pscustomobject]@{ StageBand = "Stage 7"; RhythmRole = "late climax"; PaletteRule = "skyline accents should make the high-value anchor obvious at distance" }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher District Palette Production Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`, `{3}`' -f $runtimeMapSourcePath, $controllerSourcePath, $uiFlowSourcePath, $systemBootstrapSourcePath))
$lines.Add("")
$lines.Add("Purpose: turn the current stage-gated runtime districts into concrete palette, material, route tint, and ambient-tone production targets.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime markers missing: $(if ($missingRuntimeMarkers.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingRuntimeMarkers) })")
$lines.Add("- Checklist rows: $($paletteCatalog.Count)")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Lock route marker tint contrast first so Target_A/Target_B and ROUTE HOLD remain readable over every palette.")
$lines.Add("2. Produce Stage 1, Stage 3, Stage 4, and Stage 7 palettes first because they cover opener, density, boss approach, and climax.")
$lines.Add("3. Add Stage 2/5/6 palette variants to make breathing-space, explosive-payoff, and utility-risk beats feel different.")
$lines.Add("4. Add ambient/fog presets only after ground/building/prop contrast is readable.")
$lines.Add("5. Verify screenshots from Stage 1 / 4 / 7 do not look interchangeable before tuning stage rhythm values.")
$lines.Add("")
$lines.Add("## Production Batches")
$lines.Add("| Batch | Goal | Targets | Acceptance check |")
$lines.Add("|---|---|---|---|")
foreach ($batch in $productionBatches) {
    $targetNames = @($batch.Targets | ForEach-Object { "``$_``" })
    $lines.Add(('| {0} | {1} | {2} | {3} |' -f $batch.Batch, $batch.Goal, ([string]::Join(", ", $targetNames)), $batch.Acceptance))
}

$lines.Add("")
$lines.Add("## Next District Palette Batch Task Card")
if ($productionBatches.Count -eq 0) {
    $lines.Add("- none")
}
else {
    $nextBatch = $productionBatches[0]
    $targetNames = @($nextBatch.Targets | ForEach-Object { "``$_``" })
    $paletteNeeds = @()
    foreach ($targetName in $nextBatch.Targets) {
        $palette = $paletteCatalog | Where-Object { $_.Asset -eq $targetName -or $_.RuntimeAnchor -match [regex]::Escape($targetName) } | Select-Object -First 1
        if ($null -ne $palette) {
            $paletteNeeds += "$($palette.Asset): $($palette.PaletteNeed)"
        }
    }
    $paletteNeeds = @($paletteNeeds | Sort-Object -Unique)

    $lines.Add("- Batch: $($nextBatch.Batch)")
    $lines.Add("- Goal: $($nextBatch.Goal)")
    $lines.Add("- Targets: $([string]::Join(', ', $targetNames))")
    $lines.Add("- Palette needs: $([string]::Join('; ', $paletteNeeds))")
    $lines.Add("- Acceptance: $($nextBatch.Acceptance)")
    $lines.Add("- Done means: route marker tint, target contrast, and HOLD trail readability have screenshot notes or explicit placeholder rules before district palette expansion.")
}

$lines.Add("")
$lines.Add("## District Rhythm Palette Contract")
$lines.Add("| Stage band | Rhythm role | Palette rule | Done? |")
$lines.Add("|---|---|---|---|")
foreach ($row in $stageRhythmRows) {
    $lines.Add(("| {0} | {1} | {2} | [ ] |" -f $row.StageBand, $row.RhythmRole, $row.PaletteRule))
}

$lines.Add("")
$lines.Add("## Current District Palette Targets")
$lines.Add("| Priority | Stage band | District | Runtime anchor | Asset | Palette need | Folder | Done? |")
$lines.Add("|---|---|---|---|---|---|---|---|")
foreach ($palette in $paletteCatalog) {
    $lines.Add(("| {0} | {1} | {2} | `{3}` | `{4}` | {5} | `{6}` | [ ] |" -f $palette.Priority, $palette.StageBand, $palette.District, $palette.RuntimeAnchor, $palette.Asset, $palette.PaletteNeed, $palette.Folder))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Palette variation is rhythm design here: each district should make the player read a different kind of route problem.")
$lines.Add("- Do not let market, construction, or skyline color density bury route markers or HOLD trail pips.")
$lines.Add("- Stage 4 must read as a boss approach, not merely a continuation of the market palette.")
$lines.Add("- Ambient/fog changes should be subtle until mobile screenshots prove target and prop silhouettes survive.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "District palette production checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
