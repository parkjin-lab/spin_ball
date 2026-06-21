[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$DestructiblePath = "",
    [string]$FeedbackSystemPath = "",
    [string]$StageEncounterPath = ""
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
$destructibleSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $DestructiblePath -RelativePath "Assets\Scripts\Runtime\Gameplay\DummyDestructibleBlock.cs"
$feedbackSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FeedbackSystemPath -RelativePath "Assets\Scripts\Runtime\Systems\FeedbackSystem.cs"
$stageEncounterSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $StageEncounterPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.StageEncounter.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherDestructionReadabilityChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$destructibleText = Read-SourceText -Path $destructibleSourcePath
$feedbackText = Read-SourceText -Path $feedbackSourcePath
$stageEncounterText = Read-SourceText -Path $stageEncounterSourcePath

$missingRuntimeMarkers = [System.Collections.Generic.List[string]]::new()
foreach ($needle in @(
    "UpdateCrackPieces",
    "UpdateSmokeFromDamage",
    "EmitDebris",
    "EnsureWeakPointSetup",
    "SetWeakPointVisible",
    "StageEncounterRole.EliteWeakPoint",
    "StageEncounterRole.BossSentinel",
    "SetBossCoreExposure",
    "PlayWeakPointCriticalFeedback"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $destructibleText -Needle $needle
}

foreach ($needle in @(
    "PlayDestroyFeedback",
    "PlayWeakPointCriticalFeedback",
    "breakSmallClip",
    "breakLargeClip"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $feedbackText -Needle $needle
}

foreach ($needle in @(
    "bossShieldPylonColor",
    "CORE EXPOSED",
    "SHIELD",
    "ConfigureBossShieldPylons"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $stageEncounterText -Needle $needle
}

$assetCatalog = @(
    [pscustomobject]@{ Priority = "P0"; Category = "Material"; Asset = "MAT_Building_Small"; RuntimeMeaning = "easy target"; Target = "low height, low contrast risk, quick break read"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Material"; Asset = "MAT_Building_Mid"; RuntimeMeaning = "standard route filler"; Target = "clear body color separate from ground and props"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Material"; Asset = "MAT_Building_Large"; RuntimeMeaning = "high-value durable target"; Target = "larger mass, darker base, stronger damage read"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Material"; Asset = "MAT_Boss_Structure"; RuntimeMeaning = "Sentinel or boss-related structure"; Target = "distinct from ordinary large buildings"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "State"; Asset = "MAT_Damage_CrackOverlay"; RuntimeMeaning = "damaged or near-break state"; Target = "visible cracks at medium distance"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "State"; Asset = "MAT_WeakPoint_Glow"; RuntimeMeaning = "elite weak point"; Target = "small but bright target with pulse-friendly color"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "State"; Asset = "MAT_Shielded_Pylon"; RuntimeMeaning = "boss shield pylon"; Target = "reads as blocking core damage"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "State"; Asset = "MAT_Exposed_Core"; RuntimeMeaning = "boss core open window"; Target = "contrasts shield state and invites burst damage"; Folder = "Assets/Art/Materials/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "VFX"; Asset = "VFX_Debris_Light"; RuntimeMeaning = "small hit or prop break"; Target = "short, cheap, non-blocking burst"; Folder = "Assets/Art/VFX/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "VFX"; Asset = "VFX_Debris_Heavy"; RuntimeMeaning = "large break or collapse"; Target = "heavier burst without hiding route marker"; Folder = "Assets/Art/VFX/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "VFX"; Asset = "VFX_Smoke_Damage"; RuntimeMeaning = "near-destroyed structure"; Target = "damage readable before final hit"; Folder = "Assets/Art/VFX/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "VFX"; Asset = "VFX_WeakPoint_Hit"; RuntimeMeaning = "critical weak point hit"; Target = "sharp confirmation distinct from normal hit"; Folder = "Assets/Art/VFX/Destruction/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Audio"; Asset = "SFX_Break_Small"; RuntimeMeaning = "small break"; Target = "short dry break"; Folder = "Assets/Audio/SFX/Impact/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Audio"; Asset = "SFX_Break_LargeCollapse"; RuntimeMeaning = "large collapse"; Target = "wide, heavy, longer tail"; Folder = "Assets/Audio/SFX/Impact/" }
)

$productionBatches = @(
    [pscustomobject]@{
        Batch = "A. Building tier materials"
        Goal = "make small, mid, large, and boss structures readable before damage state polish"
        Targets = @("MAT_Building_Small", "MAT_Building_Mid", "MAT_Building_Large", "MAT_Boss_Structure")
        Acceptance = "players can tell easy targets, standard fillers, durable targets, and boss structures apart at route speed"
    },
    [pscustomobject]@{
        Batch = "B. Combat state materials"
        Goal = "separate damaged, weak-point, shielded, and exposed-core states"
        Targets = @("MAT_Damage_CrackOverlay", "MAT_WeakPoint_Glow", "MAT_Shielded_Pylon", "MAT_Exposed_Core")
        Acceptance = "weak point, shielded pylon, and exposed core never read as the same state"
    },
    [pscustomobject]@{
        Batch = "C. Break feedback package"
        Goal = "make small hit, heavy break, near-break, and critical hit feedback distinct"
        Targets = @("VFX_Debris_Light", "VFX_Debris_Heavy", "VFX_Smoke_Damage", "VFX_WeakPoint_Hit")
        Acceptance = "feedback confirms impact scale without hiding route targets or HOLD pips"
    },
    [pscustomobject]@{
        Batch = "D. Collapse audio pairing"
        Goal = "match visual break size with current FeedbackSystem break slots"
        Targets = @("SFX_Break_Small", "SFX_Break_LargeCollapse", "breakSmallClip", "breakLargeClip")
        Acceptance = "small break and large collapse are separable by ear before final audio polish"
    }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Destruction Readability Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`' -f $destructibleSourcePath, $feedbackSourcePath, $stageEncounterSourcePath))
$lines.Add("")
$lines.Add("Purpose: turn the existing destruction, weak-point, shield, crack, smoke, debris, and collapse code paths into concrete readability assets.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime markers missing: $(if ($missingRuntimeMarkers.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingRuntimeMarkers) })")
$lines.Add("- Checklist rows: $($assetCatalog.Count)")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Assign small/mid/large/boss material tiers first.")
$lines.Add("2. Add weak point, shielded pylon, and exposed-core state materials.")
$lines.Add("3. Add damage crack/smoke/debris VFX variants.")
$lines.Add("4. Pair small break and large collapse audio with the current `FeedbackSystem` break slots.")
$lines.Add("5. Verify in Stage 1 / 4 / 7 that targets, weak points, and boss states remain readable behind route HUD and feedback.")
$lines.Add("")
$lines.Add("## Production Batches")
$lines.Add("| Batch | Goal | Targets | Acceptance check |")
$lines.Add("|---|---|---|---|")
foreach ($batch in $productionBatches) {
    $targetNames = @($batch.Targets | ForEach-Object { "``$_``" })
    $lines.Add(('| {0} | {1} | {2} | {3} |' -f $batch.Batch, $batch.Goal, ([string]::Join(", ", $targetNames)), $batch.Acceptance))
}

$lines.Add("")
$lines.Add("## Next Destruction Readability Batch Task Card")
if ($productionBatches.Count -eq 0) {
    $lines.Add("- none")
}
else {
    $nextBatch = $productionBatches[0]
    $targetNames = @($nextBatch.Targets | ForEach-Object { "``$_``" })
    $readabilityTargets = @()
    foreach ($targetName in $nextBatch.Targets) {
        $asset = $assetCatalog | Where-Object { $_.Asset -eq $targetName } | Select-Object -First 1
        if ($null -ne $asset) {
            $readabilityTargets += "$($asset.Asset): $($asset.Target)"
        }
    }

    $lines.Add("- Batch: $($nextBatch.Batch)")
    $lines.Add("- Goal: $($nextBatch.Goal)")
    $lines.Add("- Targets: $([string]::Join(', ', $targetNames))")
    $lines.Add("- Readability targets: $([string]::Join('; ', $readabilityTargets))")
    $lines.Add("- Acceptance: $($nextBatch.Acceptance)")
    $lines.Add("- Done means: small, mid, large, and boss structure tiers have draft material directions, screenshot notes, or explicit placeholder rules before damage-state polish.")
}

$lines.Add("")
$lines.Add("## Current Destruction Readability Targets")
$lines.Add("| Priority | Category | Asset | Runtime meaning | Readability target | Folder | Done? |")
$lines.Add("|---|---|---|---|---|---|---|")
foreach ($asset in $assetCatalog) {
    $lines.Add(("| {0} | {1} | `{2}` | {3} | {4} | `{5}` | [ ] |" -f $asset.Priority, $asset.Category, $asset.Asset, $asset.RuntimeMeaning, $asset.Target, $asset.Folder))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Do not let heavy collapse VFX hide Target_A/Target_B or ROUTE HOLD trail pips.")
$lines.Add("- Weak point glow should win over building color but lose to route objective markers.")
$lines.Add("- Shielded pylon and exposed core must be visually opposite states.")
$lines.Add("- Damage readability is a gameplay signal here, not surface polish.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Destruction readability checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
