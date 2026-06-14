[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$StageEncounterPath = "",
    [string]$DronePath = "",
    [string]$FeedbackSystemPath = ""
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
$stageEncounterSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $StageEncounterPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.StageEncounter.cs"
$droneSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $DronePath -RelativePath "Assets\Scripts\Runtime\Gameplay\BossPhaseTwoDroneDummy.cs"
$feedbackSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FeedbackSystemPath -RelativePath "Assets\Scripts\Runtime\Systems\FeedbackSystem.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherBossIdentityProductionChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$stageEncounterText = Read-SourceText -Path $stageEncounterSourcePath
$droneText = Read-SourceText -Path $droneSourcePath
$feedbackText = Read-SourceText -Path $feedbackSourcePath

$missingRuntimeMarkers = [System.Collections.Generic.List[string]]::new()
foreach ($needle in @(
    "BossSentinel",
    "JUSTICE SENTINEL DOWN",
    "ConfigureBossShieldPylons",
    "UpdateBossShieldState",
    "UpdateBossBreakWindow",
    "CORE EXPOSED",
    "DRONE SWARM BROKEN",
    "PHASE 2 DRONE SWEEP",
    "ExecuteStageBossPressurePulse",
    "BossClearCascadeRoutine"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $stageEncounterText -Needle $needle
}

foreach ($needle in @(
    "BossPhaseTwoDroneDummy",
    "SetRespawnPreview",
    "DRONE DOWN",
    "Restore"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $droneText -Needle $needle
}

foreach ($needle in @(
    "bossWarningClip",
    "bossBreakClip",
    "bossDownClip",
    "PlayBossDownFeedback",
    "PlayDroneBreakFeedback",
    "PlayHudWarningFeedback"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $feedbackText -Needle $needle
}

$assetCatalog = @(
    [pscustomobject]@{ Priority = "P0"; Category = "Silhouette"; Asset = "BOSS_Sentinel_Body_Kit"; RuntimeMoment = "Justice Sentinel main target"; ReadabilityTarget = "clearly taller/heavier than normal large buildings"; Folder = "Assets/Art/Boss/Sentinel/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Silhouette"; Asset = "BOSS_Shield_Pylon_Kit"; RuntimeMoment = "shield blockers before core exposure"; ReadabilityTarget = "small countable blockers that point back to boss core"; Folder = "Assets/Art/Boss/Sentinel/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Silhouette"; Asset = "BOSS_Phase2_Drone_Kit"; RuntimeMoment = "phase 2 drone swarm"; ReadabilityTarget = "airborne/hovering shape distinct from street props"; Folder = "Assets/Art/Boss/Sentinel/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Material"; Asset = "MAT_Boss_Sentinel_Armor"; RuntimeMoment = "default boss state"; ReadabilityTarget = "fortified civic/defense look, not market building color"; Folder = "Assets/Art/Materials/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Material"; Asset = "MAT_Boss_Shield_Pylon"; RuntimeMoment = "active shield pylon"; ReadabilityTarget = "reads as protected objective before damage target"; Folder = "Assets/Art/Materials/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Material"; Asset = "MAT_Boss_Core_Exposed"; RuntimeMoment = "break window"; ReadabilityTarget = "opposite of shield state and visibly burstable"; Folder = "Assets/Art/Materials/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "VFX"; Asset = "VFX_Boss_Warning_Ring"; RuntimeMoment = "boss threat pulse / pressure warning"; ReadabilityTarget = "announces danger radius without hiding route markers"; Folder = "Assets/Art/VFX/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "VFX"; Asset = "VFX_Boss_Core_Expose_Burst"; RuntimeMoment = "pylons broken or drones down"; ReadabilityTarget = "sharp open-window punctuation"; Folder = "Assets/Art/VFX/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "VFX"; Asset = "VFX_Boss_Defeat_Cascade"; RuntimeMoment = "Sentinel down and stage clear cascade"; ReadabilityTarget = "climax release bigger than normal collapse"; Folder = "Assets/Art/VFX/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Audio"; Asset = "SFX_Boss_Warning"; RuntimeMoment = "threat pulse / drone return / shield pressure"; ReadabilityTarget = "low warning beat, not normal route warning"; Folder = "Assets/Audio/SFX/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Audio"; Asset = "SFX_Boss_Break"; RuntimeMoment = "pylon/drone/core break"; ReadabilityTarget = "clear open-window hit"; Folder = "Assets/Audio/SFX/Boss/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Audio"; Asset = "SFX_Boss_Down"; RuntimeMoment = "Justice Sentinel defeat"; ReadabilityTarget = "final downbeat and release"; Folder = "Assets/Audio/SFX/Boss/" },
    [pscustomobject]@{ Priority = "P1"; Category = "UI"; Asset = "Icon_Boss_Sentinel"; RuntimeMoment = "HUD/result boss state"; ReadabilityTarget = "boss status recognized faster than text"; Folder = "Assets/Resources/UI/Icons/" },
    [pscustomobject]@{ Priority = "P1"; Category = "UI"; Asset = "Badge_Boss_Clear"; RuntimeMoment = "result and stage clear"; ReadabilityTarget = "separates boss victory from normal clear"; Folder = "Assets/Resources/UI/Badges/" }
)

$productionBatches = @(
    [pscustomobject]@{
        Batch = "A. Boss silhouette hierarchy"
        Goal = "separate Sentinel body, shield pylons, and phase 2 drones from ordinary city targets"
        Targets = @("BOSS_Sentinel_Body_Kit", "BOSS_Shield_Pylon_Kit", "BOSS_Phase2_Drone_Kit")
        Acceptance = "Stage 4 screenshots make main boss, blockers, and drones countable before reading HUD text"
    },
    [pscustomobject]@{
        Batch = "B. Shield and core state readability"
        Goal = "make protected, exposed, and broken states visually opposite"
        Targets = @("MAT_Boss_Sentinel_Armor", "MAT_Boss_Shield_Pylon", "MAT_Boss_Core_Exposed", "VFX_Boss_Core_Expose_Burst")
        Acceptance = "players can identify when to stop hitting shields and when to burst the core"
    },
    [pscustomobject]@{
        Batch = "C. Climax feedback package"
        Goal = "punctuate warning, break, and down beats without changing boss timing"
        Targets = @("VFX_Boss_Warning_Ring", "VFX_Boss_Defeat_Cascade", "SFX_Boss_Warning", "SFX_Boss_Break", "SFX_Boss_Down")
        Acceptance = "boss warning, break window, and defeat release have distinct audio/visual weight"
    }
)

$beatRows = @(
    [pscustomobject]@{ Beat = "Breathe"; RuntimeSignal = "shield pylons, drone recovery, or post-break recovery"; ProductionRule = "reduce warning density and make the next actionable target obvious" },
    [pscustomobject]@{ Beat = "Burst"; RuntimeSignal = "CORE EXPOSED, DRONE SWARM BROKEN, break window"; ProductionRule = "use the brightest core material and boss break sound here" },
    [pscustomobject]@{ Beat = "Punish"; RuntimeSignal = "pressure pulse, drone sweep, shield return"; ProductionRule = "warning ring must appear before the gameplay penalty lands" },
    [pscustomobject]@{ Beat = "Release"; RuntimeSignal = "JUSTICE SENTINEL DOWN, finish shot, clear cascade"; ProductionRule = "larger visual/audio release than any route bonus" }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Boss Identity Production Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`' -f $stageEncounterSourcePath, $droneSourcePath, $feedbackSourcePath))
$lines.Add("")
$lines.Add("Purpose: turn the existing Justice Sentinel boss logic into a production checklist for climax readability, rhythm, and asset assignment.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime markers missing: $(if ($missingRuntimeMarkers.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingRuntimeMarkers) })")
$lines.Add("- Checklist rows: $($assetCatalog.Count)")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Make the Sentinel body, shield pylons, and phase 2 drones visually distinct from normal buildings and props.")
$lines.Add("2. Assign boss armor, shield pylon, and exposed-core materials before tuning boss timings.")
$lines.Add("3. Add warning ring, core-expose burst, and defeat cascade VFX so the encounter reads as breathe -> burst -> punish -> release.")
$lines.Add("4. Assign boss warning, break, and down audio clips to the current `FeedbackSystem` boss slots.")
$lines.Add("5. Verify Stage 4 and Stage 7 in a real sweep before changing boss window timing.")
$lines.Add("")
$lines.Add("## Production Batches")
$lines.Add("| Batch | Goal | Targets | Acceptance check |")
$lines.Add("|---|---|---|---|")
foreach ($batch in $productionBatches) {
    $targetNames = @($batch.Targets | ForEach-Object { "``$_``" })
    $lines.Add(('| {0} | {1} | {2} | {3} |' -f $batch.Batch, $batch.Goal, ([string]::Join(", ", $targetNames)), $batch.Acceptance))
}

$lines.Add("")
$lines.Add("## Boss Rhythm Contract")
$lines.Add("| Beat | Runtime signal | Production rule | Done? |")
$lines.Add("|---|---|---|---|")
foreach ($beat in $beatRows) {
    $lines.Add(("| {0} | {1} | {2} | [ ] |" -f $beat.Beat, $beat.RuntimeSignal, $beat.ProductionRule))
}

$lines.Add("")
$lines.Add("## Current Boss Identity Targets")
$lines.Add("| Priority | Category | Asset | Runtime moment | Readability target | Folder | Done? |")
$lines.Add("|---|---|---|---|---|---|---|")
foreach ($asset in $assetCatalog) {
    $lines.Add(("| {0} | {1} | `{2}` | {3} | {4} | `{5}` | [ ] |" -f $asset.Priority, $asset.Category, $asset.Asset, $asset.RuntimeMoment, $asset.ReadabilityTarget, $asset.Folder))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Treat boss identity as a climax-readability task, not only an art task.")
$lines.Add('- Do not tune `bossBreakWindowDuration`, `bossPressurePulseInterval`, or shield regen until real Stage 1-7 evidence identifies a boss rhythm problem.')
$lines.Add("- Shielded and exposed states must be visually opposite at mobile distance.")
$lines.Add("- Drone respawn preview should warn the player without looking like a live target.")
$lines.Add("- Defeat cascade must feel like the final release after pressure, not another normal destruction burst.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Boss identity production checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
