[CmdletBinding()]
param(
    [int]$MaxStage = 10,
    [int]$MaxGrowthStage = 10,
    [string]$ReportPath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Add-MissingTextCheck {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ($Text.Contains($Needle)) {
        return
    }

    $Errors.Add("$Label missing required text: $Needle")
}

$projectRoot = Resolve-ProjectRoot
$feedbackPath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\FeedbackSystem.cs"
$requirementsPath = Join-Path $projectRoot "Docs\CURRENT_STAGE_RESOURCE_REQUIREMENTS.md"
$productionBoardPath = Join-Path $projectRoot "Docs\CURRENT_STAGE_RESOURCE_PRODUCTION_BOARD.md"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherResourceSlotDocsStaticAudit.log"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

foreach ($path in @($feedbackPath, $requirementsPath, $productionBoardPath)) {
    if (-not (Test-Path -Path $path -PathType Leaf)) {
        $errors.Add("Required source not found: $path")
    }
}

$clipFields = @()
$requirementsText = ""
$productionBoardText = ""
if ($errors.Count -eq 0) {
    $feedbackText = Get-Content -Path $feedbackPath -Raw
    $requirementsText = Get-Content -Path $requirementsPath -Raw
    $productionBoardText = Get-Content -Path $productionBoardPath -Raw

    $clipMatches = [regex]::Matches($feedbackText, "\[SerializeField\][^\r\n]*private\s+AudioClip\s+([A-Za-z0-9_]+)")
    foreach ($match in $clipMatches) {
        $clipFields += $match.Groups[1].Value
    }

    if ($clipFields.Count -lt 10) {
        $errors.Add("FeedbackSystem audio clip surface looks too small: $($clipFields.Count) field(s)")
    }

    foreach ($field in $clipFields | Sort-Object -Unique) {
        Add-MissingTextCheck -Errors $errors -Text $requirementsText -Needle $field -Label "Resource requirements"
        Add-MissingTextCheck -Errors $errors -Text $productionBoardText -Needle $field -Label "Resource production board"
    }

    foreach ($needle in @(
        "SFX_Hit_Light",
        "SFX_Break_LargeCollapse",
        "SFX_Route_Open",
        "SFX_Route_HoldWarning",
        "SFX_Route_Bonus",
        "SFX_Boss_Down",
        "SFX_Failure_Warning",
        "SFX_Failure_Boss",
        "PAYOFF_ParkCut_Layout",
        "PAYOFF_MarketChain_Layout",
        "PAYOFF_YardBlast_Layout",
        "PAYOFF_PowerSurge_Layout",
        "PAYOFF_SkylineBreach_Layout",
        "VFX_RouteCluster_Marker",
        "VFX_ForwardSmash_Confirm",
        "VFX_RouteHold_Success",
        "VFX_Combo_Rise_Pulse",
        "VFX_Overdrive_Pulse",
        "BOSS_Sentinel_Body_Kit",
        "BOSS_Shield_Pylon_Kit",
        "BOSS_Phase2_Drone_Kit",
        "VFX_Boss_Warning_Ring",
        "VFX_Boss_Defeat_Cascade",
        "SFX_Boss_Warning",
        "SFX_Boss_Break",
        "PAL_RouteMarker_Tints",
        "PAL_District_StarterResidential",
        "PAL_District_PocketPark",
        "PAL_District_MarketPlaza",
        "PAL_District_SentinelCheckpoint",
        "PAL_District_ConstructionYard",
        "PAL_District_PowerBlock",
        "PAL_District_SkylineBlock",
        "PAL_Ambient_StageBands",
        "Icon_DP",
        "Icon_Stage",
        "Icon_NextStep",
        "Icon_Route",
        "Icon_BreakWindow",
        "Icon_Shield",
        "Icon_WeakPoint",
        "Icon_Boss",
        "Icon_Overdrive",
        "Icon_Panic",
        "Icon_Seismic",
        "Icon_Retail",
        "Icon_Traffic",
        "Badge_Result_Clear",
        "Badge_Result_Failure",
        "Badge_Locked",
        "Badge_Recommended",
        "UI_DP_GainBurst",
        "SFX_Progression_Locked",
        "SFX_Progression_Confirm",
        "UI_FormCard_StateSet",
        "UI_MetaNode_SizeCore",
        "UI_MetaNode_ImpactCore",
        "UI_MetaNode_DpAmplifier",
        "Badge_FormReady",
        "Badge_MetaReady",
        "Banner_StageUnlocked",
        "Toast_ProgressionSaved",
        "FORM_Sphere_Body_Kit",
        "Icon_Form_Sphere",
        "Icon_Skill_SpherePulse",
        "FORM_Ram_Body_Kit",
        "FORM_Saucer_Body_Kit",
        "Icon_Form_Ram",
        "Icon_Form_Saucer",
        "FORM_Spike_Body_Kit",
        "FORM_Crusher_Body_Kit",
        "Icon_Form_Spike",
        "Icon_Form_Crusher",
        "MAT_Building_Small",
        "MAT_Building_Mid",
        "MAT_Building_Large",
        "MAT_Boss_Structure",
        "MAT_Damage_CrackOverlay",
        "MAT_WeakPoint_Glow",
        "MAT_Shielded_Pylon",
        "MAT_Exposed_Core",
        "VFX_Debris_Light",
        "VFX_Debris_Heavy",
        "VFX_Smoke_Damage",
        "VFX_WeakPoint_Hit",
        "PROP_Car_Compact_A",
        "PROP_Car_Compact_B",
        "PROP_Van_Bus",
        "PROP_StreetLamp",
        "PROP_TrafficLight",
        "PROP_RoadsideTree",
        "PROP_Bench",
        "PROP_Kiosk",
        "PROP_Vending",
        "PROP_BusStop",
        "PROP_Transformer",
        "PROP_ExplosiveBarrel",
        "PROP_Fence",
        "PROP_Mailbox",
        "PROP_Shed"
    )) {
        Add-MissingTextCheck -Errors $errors -Text $requirementsText -Needle $needle -Label "Resource requirements SFX naming"
    }

    foreach ($needle in @(
        "Assets/Audio/SFX/Impact/",
        "Assets/Audio/SFX/Skills/",
        "Assets/Audio/SFX/Boss/",
        "Assets/Audio/SFX/Failure/",
        "Assets/Audio/SFX/UI/",
        'every current `FeedbackSystem` slot is assigned',
        "PAYOFF_ParkCut_Layout",
        "PAYOFF_MarketChain_Layout",
        "PAYOFF_YardBlast_Layout",
        "PAYOFF_PowerSurge_Layout",
        "PAYOFF_SkylineBreach_Layout",
        "VFX_RouteCluster_Marker",
        "VFX_ForwardSmash_Confirm",
        "VFX_RouteHold_Success",
        "VFX_Combo_Rise_Pulse",
        "VFX_Overdrive_Pulse",
        "BOSS_Sentinel_Body_Kit",
        "BOSS_Shield_Pylon_Kit",
        "BOSS_Phase2_Drone_Kit",
        "VFX_Boss_Warning_Ring",
        "VFX_Boss_Defeat_Cascade",
        "SFX_Boss_Warning",
        "SFX_Boss_Break",
        "PAL_RouteMarker_Tints",
        "PAL_District_StarterResidential",
        "PAL_District_PocketPark",
        "PAL_District_MarketPlaza",
        "PAL_District_SentinelCheckpoint",
        "PAL_District_ConstructionYard",
        "PAL_District_PowerBlock",
        "PAL_District_SkylineBlock",
        "PAL_Ambient_StageBands",
        "Icon_DP",
        "Icon_Stage",
        "Icon_NextStep",
        "Icon_Route",
        "Icon_BreakWindow",
        "Icon_Shield",
        "Icon_WeakPoint",
        "Icon_Boss",
        "Icon_Overdrive",
        "Icon_Panic",
        "Icon_Seismic",
        "Icon_Retail",
        "Icon_Traffic",
        "Badge_Result_Clear",
        "Badge_Result_Failure",
        "Badge_Locked",
        "Badge_Recommended",
        "UI_DP_GainBurst",
        "SFX_Progression_Locked",
        "SFX_Progression_Confirm",
        "UI_FormCard_StateSet",
        "UI_MetaNode_SizeCore",
        "UI_MetaNode_ImpactCore",
        "UI_MetaNode_DpAmplifier",
        "Badge_FormReady",
        "Badge_MetaReady",
        "Banner_StageUnlocked",
        "Toast_ProgressionSaved",
        "FORM_Sphere_Body_Kit",
        "Icon_Form_Sphere",
        "Icon_Skill_SpherePulse",
        "FORM_Ram_Body_Kit",
        "FORM_Saucer_Body_Kit",
        "Icon_Form_Ram",
        "Icon_Form_Saucer",
        "FORM_Spike_Body_Kit",
        "FORM_Crusher_Body_Kit",
        "Icon_Form_Spike",
        "Icon_Form_Crusher",
        "MAT_Building_Small",
        "MAT_Building_Mid",
        "MAT_Building_Large",
        "MAT_Boss_Structure",
        "MAT_Damage_CrackOverlay",
        "MAT_WeakPoint_Glow",
        "MAT_Shielded_Pylon",
        "MAT_Exposed_Core",
        "VFX_Debris_Light",
        "VFX_Debris_Heavy",
        "VFX_Smoke_Damage",
        "VFX_WeakPoint_Hit",
        "PROP_Car_Compact_A",
        "PROP_Car_Compact_B",
        "PROP_Van_Bus",
        "PROP_StreetLamp",
        "PROP_TrafficLight",
        "PROP_RoadsideTree",
        "PROP_Bench",
        "PROP_Kiosk",
        "PROP_Vending",
        "PROP_BusStop",
        "PROP_Transformer",
        "PROP_ExplosiveBarrel",
        "PROP_Fence",
        "PROP_Mailbox",
        "PROP_Shed",
        "Assets/Resources/UI/Icons/",
        "Assets/Resources/UI/Badges/"
    )) {
        Add-MissingTextCheck -Errors $errors -Text $productionBoardText -Needle $needle -Label "Resource production board output contract"
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][ResourceSlotDocsStaticAudit] Resource slot documentation audit")
$lines.Add("Feedback source: $feedbackPath")
$lines.Add("Requirements doc: $requirementsPath")
$lines.Add("Production board: $productionBoardPath")
$lines.Add("Audio clip fields: $([string]::Join(', ', ($clipFields | Sort-Object -Unique)))")

foreach ($errorMessage in $errors) {
    $lines.Add("ERROR: $errorMessage")
}

foreach ($warningMessage in $warnings) {
    $lines.Add("WARN: $warningMessage")
}

$lines.Add("Result: $($errors.Count) error(s), $($warnings.Count) warning(s)")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output $report

if ($errors.Count -gt 0 -or ($FailOnWarnings -and $warnings.Count -gt 0)) {
    exit 1
}

exit 0
