[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$RuntimeMapPath = "",
    [string]$TrafficBootstrapPath = "",
    [string]$TrafficSpawningPath = ""
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
$trafficBootstrapSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $TrafficBootstrapPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.TrafficBootstrap.cs"
$trafficSpawningSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $TrafficSpawningPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.TrafficSpawning.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherStreetPropVarietyChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$runtimeMapText = Read-SourceText -Path $runtimeMapSourcePath
$trafficBootstrapText = Read-SourceText -Path $trafficBootstrapSourcePath
$trafficSpawningText = Read-SourceText -Path $trafficSpawningSourcePath

$missingRuntimeMarkers = [System.Collections.Generic.List[string]]::new()
foreach ($needle in @(
    "EnsureTrafficVehicleRuntime",
    "EnsureStreetLampRuntime",
    "EnsureStreetTreeRuntime",
    "EnsureExplosiveBarrelRuntime",
    "EnsureTransformerRuntime",
    "EnsureCommercialKioskRuntime",
    "EnsureCommercialBenchRuntime",
    "EnsureCommercialBusStopRuntime",
    "EnsureCommercialVendingRuntime",
    "EnsureResidentialFenceRuntime",
    "EnsureResidentialMailboxRuntime",
    "EnsureResidentialShedRuntime",
    "DummyStreetPropReactive.PropKind.Lamp",
    "DummyStreetPropReactive.PropKind.Tree",
    "DummyStreetPropReactive.PropKind.ChainBarrel",
    "DummyStreetPropReactive.PropKind.Transformer"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $runtimeMapText -Needle $needle
}

foreach ($needle in @(
    "RegisterTrafficVehicle",
    "EnsureStreetPropReactiveRuntime",
    "DummyStreetPropReactive.PropKind.Vehicle"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $trafficBootstrapText -Needle $needle
}

foreach ($needle in @(
    "TrySpawnTrafficVehicle",
    "PopulateDefaultTrafficSpawnPoints",
    "GetRandomTrafficBodyColor",
    "IsTrafficSpawnBlockedByObstacle"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $trafficSpawningText -Needle $needle
}

$assetCatalog = @(
    [pscustomobject]@{ Priority = "P0"; Category = "Traffic"; Asset = "PROP_Car_Compact_A"; RuntimeHook = "EnsureTrafficVehicleRuntime"; GameplayUse = "moving panic target"; Folder = "Assets/Art/Props/Traffic/"; Target = "low, fast, clearly crushable car silhouette" },
    [pscustomobject]@{ Priority = "P0"; Category = "Traffic"; Asset = "PROP_Car_Compact_B"; RuntimeHook = "EnsureTrafficVehicleRuntime"; GameplayUse = "traffic variety"; Folder = "Assets/Art/Props/Traffic/"; Target = "same footprint, different roof/body read" },
    [pscustomobject]@{ Priority = "P0"; Category = "Traffic"; Asset = "PROP_Van_Bus"; RuntimeHook = "EnsureTrafficVehicleRuntime"; GameplayUse = "larger traffic payoff"; Folder = "Assets/Art/Props/Traffic/"; Target = "longer blocky silhouette, readable from chase camera" },
    [pscustomobject]@{ Priority = "P0"; Category = "Street"; Asset = "PROP_StreetLamp"; RuntimeHook = "EnsureStreetLampRuntime"; GameplayUse = "small break rhythm"; Folder = "Assets/Art/Props/Street/"; Target = "thin vertical post with visible cap" },
    [pscustomobject]@{ Priority = "P0"; Category = "Street"; Asset = "PROP_TrafficLight"; RuntimeHook = "EnsureStreetLampRuntime"; GameplayUse = "roadside city identity"; Folder = "Assets/Art/Props/Street/"; Target = "lamp variant with signal head color blocks" },
    [pscustomobject]@{ Priority = "P0"; Category = "Street"; Asset = "PROP_RoadsideTree"; RuntimeHook = "EnsureStreetTreeRuntime"; GameplayUse = "park/roadside soft target"; Folder = "Assets/Art/Props/Street/"; Target = "trunk plus low-poly canopy cluster" },
    [pscustomobject]@{ Priority = "P0"; Category = "Commercial"; Asset = "PROP_Kiosk"; RuntimeHook = "EnsureCommercialKioskRuntime"; GameplayUse = "market route chain"; Folder = "Assets/Art/Props/Street/"; Target = "small stacked shop shape with roof/counter" },
    [pscustomobject]@{ Priority = "P0"; Category = "Commercial"; Asset = "PROP_Vending"; RuntimeHook = "EnsureCommercialVendingRuntime"; GameplayUse = "market density target"; Folder = "Assets/Art/Props/Street/"; Target = "upright box with readable face panel" },
    [pscustomobject]@{ Priority = "P0"; Category = "Commercial"; Asset = "PROP_Bench"; RuntimeHook = "EnsureCommercialBenchRuntime"; GameplayUse = "park/market light target"; Folder = "Assets/Art/Props/Street/"; Target = "seat/back silhouette, low height" },
    [pscustomobject]@{ Priority = "P0"; Category = "Commercial"; Asset = "PROP_BusStop"; RuntimeHook = "EnsureCommercialBusStopRuntime"; GameplayUse = "landmark roadside shape"; Folder = "Assets/Art/Props/Street/"; Target = "roof, panel, and posts separated" },
    [pscustomobject]@{ Priority = "P0"; Category = "Utility"; Asset = "PROP_Transformer"; RuntimeHook = "EnsureTransformerRuntime"; GameplayUse = "power block chain hazard"; Folder = "Assets/Art/Props/Utilities/"; Target = "box plus pipe/cap silhouette, hazard colors" },
    [pscustomobject]@{ Priority = "P0"; Category = "Utility"; Asset = "PROP_ExplosiveBarrel"; RuntimeHook = "EnsureExplosiveBarrelRuntime"; GameplayUse = "route reward explosion chain"; Folder = "Assets/Art/Props/Utilities/"; Target = "cylinder with strong color band" },
    [pscustomobject]@{ Priority = "P1"; Category = "Residential"; Asset = "PROP_Fence"; RuntimeHook = "EnsureResidentialFenceRuntime"; GameplayUse = "low clutter lane edge"; Folder = "Assets/Art/Props/Street/"; Target = "thin rails that do not hide route pips" },
    [pscustomobject]@{ Priority = "P1"; Category = "Residential"; Asset = "PROP_Mailbox"; RuntimeHook = "EnsureResidentialMailboxRuntime"; GameplayUse = "suburb identity prop"; Folder = "Assets/Art/Props/Street/"; Target = "tiny post-and-box read" },
    [pscustomobject]@{ Priority = "P1"; Category = "Residential"; Asset = "PROP_Shed"; RuntimeHook = "EnsureResidentialShedRuntime"; GameplayUse = "small durable backyard target"; Folder = "Assets/Art/Props/Street/"; Target = "box with roof, larger than mailbox smaller than building" }
)

$productionBatches = @(
    [pscustomobject]@{
        Batch = "A. Traffic silhouette set"
        Goal = "make moving/parked vehicles readable as crushable rhythm targets"
        Targets = "PROP_Car_Compact_A, PROP_Car_Compact_B, PROP_Van_Bus"
        Acceptance = "traffic reads while moving and while parked in panic clusters from chase camera distance"
    },
    [pscustomobject]@{
        Batch = "B. Light roadside rhythm props"
        Goal = "create quick-break street beats without hiding route pips"
        Targets = "PROP_StreetLamp, PROP_TrafficLight, PROP_RoadsideTree, PROP_Bench"
        Acceptance = "light props add small break cadence and never block Target_A/Target_B or HOLD trail pips"
    },
    [pscustomobject]@{
        Batch = "C. Market and utility payoff props"
        Goal = "support market chains, transformer risk, and explosive route reward clusters"
        Targets = "PROP_Kiosk, PROP_Vending, PROP_BusStop, PROP_Transformer, PROP_ExplosiveBarrel"
        Acceptance = "commercial/utility props signal chain density and payoff danger before impact"
    },
    [pscustomobject]@{
        Batch = "D. Residential filler extras"
        Goal = "add starter/suburb texture after early route readability is stable"
        Targets = "PROP_Fence, PROP_Mailbox, PROP_Shed"
        Acceptance = "residential props add identity without making Stage 1 starter lane visually noisy"
    }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Street Prop Variety Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`' -f $runtimeMapSourcePath, $trafficBootstrapSourcePath, $trafficSpawningSourcePath))
$lines.Add("")
$lines.Add("Purpose: turn the existing runtime traffic, commercial, utility, and roadside prop hooks into a concrete prop production list.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime markers missing: $(if ($missingRuntimeMarkers.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingRuntimeMarkers) })")
$lines.Add("- Checklist rows: $($assetCatalog.Count)")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Produce three vehicle silhouettes first: two compact cars and one larger van/bus read.")
$lines.Add("2. Produce five light roadside/commercial props: lamp, tree, kiosk, vending, bench.")
$lines.Add("3. Produce utility payoff props: transformer and explosive barrel.")
$lines.Add("4. Add residential extras only after early-stage density reads well.")
$lines.Add("5. Verify Stage 1 / 3 / 6 that props add crush rhythm without hiding route markers.")
$lines.Add("")
$lines.Add("## Production Batches")
$lines.Add("| Batch | Goal | Targets | Acceptance |")
$lines.Add("|---|---|---|---|")
foreach ($batch in $productionBatches) {
    $lines.Add(("| {0} | {1} | `{2}` | {3} |" -f $batch.Batch, $batch.Goal, $batch.Targets, $batch.Acceptance))
}

$lines.Add("")
$lines.Add("## Current Street Prop Variety Targets")
$lines.Add("| Priority | Category | Asset | Runtime hook | Gameplay use | Readability target | Folder | Done? |")
$lines.Add("|---|---|---|---|---|---|---|---|")
foreach ($asset in $assetCatalog) {
    $lines.Add(("| {0} | {1} | `{2}` | `{3}` | {4} | {5} | `{6}` | [ ] |" -f $asset.Priority, $asset.Category, $asset.Asset, $asset.RuntimeHook, $asset.GameplayUse, $asset.Target, $asset.Folder))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Props should create rhythm density, not visual noise.")
$lines.Add("- Vehicles must stay readable while moving and while parked in panic clusters.")
$lines.Add("- Transformers and barrels are payoff props; their color language should warn before they explode or chain.")
$lines.Add("- Light props should break quickly and never block the main route marker silhouette.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Street prop variety checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
