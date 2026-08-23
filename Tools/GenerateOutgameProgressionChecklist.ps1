[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$MetaProgressionPath = "",
    [string]$FormFlowPath = "",
    [string]$StageFlowPath = "",
    [string]$FormUnlockSystemPath = "",
    [string]$ProgressionSaveSystemPath = "",
    [string]$DpEconomyHookPath = "",
    [string]$ProgressionVisualsHookPath = "",
    [string]$FormEquipConfirmPath = "",
    [string]$SpendChangeReadyPath = "",
    [string]$StageSelectConfirmPath = "",
    [string]$UiFlowPath = "",
    [string]$MetaUpgradeConfirmPath = "",
    [string]$DpGainResidualPath = "",
    [string]$NextActionReadyPath = "",
    [string]$StageUnlockReadyPath = "",
    [string]$FormUnlockConfirmPath = ""
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
$metaProgressionSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $MetaProgressionPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.MetaProgression.cs"
$formFlowSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FormFlowPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.FormFlow.cs"
$stageFlowSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $StageFlowPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.StageFlow.cs"
$formUnlockSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FormUnlockSystemPath -RelativePath "Assets\Scripts\Runtime\Systems\FormUnlockSystem.cs"
$progressionSaveSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ProgressionSaveSystemPath -RelativePath "Assets\Scripts\Runtime\Systems\ProgressionSaveSystem.cs"
$dpEconomyHookSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $DpEconomyHookPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.OutgameDpEconomy.cs"
$progressionVisualsHookSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ProgressionVisualsHookPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.OutgameProgressionVisuals.cs"
$formEquipConfirmSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FormEquipConfirmPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.FormEquipConfirm.cs"
$spendChangeReadySourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $SpendChangeReadyPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.SpendChangeReady.cs"
$stageSelectConfirmSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $StageSelectConfirmPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.StageSelectConfirm.cs"
$uiFlowSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $UiFlowPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.UIFlow.cs"
$metaUpgradeConfirmSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $MetaUpgradeConfirmPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.MetaUpgradeConfirm.cs"
$dpGainResidualSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $DpGainResidualPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.DpGainResidual.cs"
$nextActionReadySourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $NextActionReadyPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.NextActionReady.cs"
$stageUnlockReadySourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $StageUnlockReadyPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.StageUnlockReady.cs"
$formUnlockConfirmSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FormUnlockConfirmPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.FormUnlockConfirm.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherOutgameProgressionChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$metaProgressionText = Read-SourceText -Path $metaProgressionSourcePath
$formFlowText = Read-SourceText -Path $formFlowSourcePath
$stageFlowText = Read-SourceText -Path $stageFlowSourcePath
$formUnlockText = Read-SourceText -Path $formUnlockSourcePath
$progressionSaveText = Read-SourceText -Path $progressionSaveSourcePath
$dpEconomyHookText = Read-SourceText -Path $dpEconomyHookSourcePath
$progressionVisualsHookText = Read-SourceText -Path $progressionVisualsHookSourcePath
$formEquipConfirmText = Read-SourceText -Path $formEquipConfirmSourcePath
$spendChangeReadyText = Read-SourceText -Path $spendChangeReadySourcePath
$stageSelectConfirmText = Read-SourceText -Path $stageSelectConfirmSourcePath
$uiFlowText = Read-SourceText -Path $uiFlowSourcePath
$metaUpgradeConfirmText = Read-SourceText -Path $metaUpgradeConfirmSourcePath
$dpGainResidualText = Read-SourceText -Path $dpGainResidualSourcePath
$nextActionReadyText = Read-SourceText -Path $nextActionReadySourcePath
$stageUnlockReadyText = Read-SourceText -Path $stageUnlockReadySourcePath
$formUnlockConfirmText = Read-SourceText -Path $formUnlockConfirmSourcePath
$allOutgameHookText = $metaProgressionText + $formFlowText + $stageFlowText + $dpEconomyHookText + $progressionVisualsHookText + $formEquipConfirmText + $spendChangeReadyText + $stageSelectConfirmText + $uiFlowText + $metaUpgradeConfirmText + $dpGainResidualText + $nextActionReadyText + $stageUnlockReadyText + $formUnlockConfirmText

$missingRuntimeMarkers = [System.Collections.Generic.List[string]]::new()
foreach ($needle in @(
    "UpdateMetaProgressUi",
    "UpdateLobbyRecommendationUi",
    "UpdateLobbyMetaDetailUi",
    "UpdateMetaShopButtons",
    "TryPurchaseMetaUpgrade",
    "SizeCore",
    "ImpactCore",
    "DpAmplifier",
    "lastLobbyActionStatus",
    "GetRecommendedMetaContextTag",
    "ArmSpendChangeReadyFromMeta",
    "PlayMetaUpgradeConfirmPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $metaProgressionText -Needle $needle
}

foreach ($needle in @(
    "UpdateFormButtons",
    "UpdateFormButton",
    "TryUnlockAndSelectWithCost",
    "EQUIPPED",
    "RECOMMENDED",
    "GetEarlyLobbyFormUnlockHint",
    "PlayFormEquipConfirmPulse",
    "PlayFormUnlockConfirmPulse",
    "ArmSpendChangeReadyFromForm"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $formFlowText -Needle $needle
}

foreach ($needle in @(
    "VFX_FormEquip_Confirm",
    "PlayFormEquipConfirmPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $formEquipConfirmText -Needle $needle
}

foreach ($needle in @(
    "VFX_SpendChange_Ready",
    "PlaySpendChangeReadyPulse",
    "ArmSpendChangeReadyFromForm",
    "ArmSpendChangeReadyFromMeta"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $spendChangeReadyText -Needle $needle
}

foreach ($needle in @(
    "VFX_StageSelect_Confirm",
    "PlayStageSelectConfirmPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $stageSelectConfirmText -Needle $needle
}

foreach ($needle in @(
    "HandlePreviousLobbyStage",
    "HandleNextLobbyStage",
    "PlayStageSelectConfirmPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $uiFlowText -Needle $needle
}

foreach ($needle in @(
    "VFX_MetaUpgrade_Confirm",
    "PlayMetaUpgradeConfirmPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $metaUpgradeConfirmText -Needle $needle
}

foreach ($needle in @(
    "VFX_DP_Gain_Residual",
    "PlaceDpGainResidual"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $dpGainResidualText -Needle $needle
}

foreach ($needle in @(
    "VFX_NextAction_Ready",
    "PlaceNextActionReadyPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $nextActionReadyText -Needle $needle
}

foreach ($needle in @(
    "VFX_StageUnlock_Ready",
    "PlaceStageUnlockReadyPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $stageUnlockReadyText -Needle $needle
}

foreach ($needle in @(
    "VFX_FormUnlock_Confirm",
    "PlayFormUnlockConfirmPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $formUnlockConfirmText -Needle $needle
}

foreach ($needle in @(
    "UpdateResultStatusBadges",
    "UpdateResultMetaProgress",
    "GetResultNextFormHint",
    "GetResultNextMetaHint",
    "RETURN TO LOBBY / SPEND DP",
    "DP +",
    "BEST STAGE",
    "PlaySpendChangeReadyPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $stageFlowText -Needle $needle
}

foreach ($needle in @(
    "DpBalance",
    "HighestUnlockedStage",
    "CurrentLobbyStage",
    "GetUnlockCost",
    "TrySpendDp",
    "RegisterClearedStage",
    "GetMetaDpRewardMultiplier"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $formUnlockText -Needle $needle
}

foreach ($needle in @(
    "UI_DP_GainBurst",
    "SFX_Progression_Locked",
    "SFX_Progression_Confirm",
    "EnsureOutgameDpEconomy",
    "SignalOutgameDpInsufficient",
    "PlayProgressionConfirmCue",
    "PlaceDpGainResidual"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $dpEconomyHookText -Needle $needle
}

foreach ($needle in @(
    "UI_FormCard_StateSet",
    "UI_MetaNode_SizeCore",
    "UI_MetaNode_ImpactCore",
    "UI_MetaNode_DpAmplifier",
    "Badge_FormReady",
    "Badge_MetaReady",
    "Banner_StageUnlocked",
    "Toast_ProgressionSaved",
    "EnsureOutgameProgressionVisuals",
    "SignalOutgameStageUnlocked",
    "SignalOutgameProgressionSaved",
    "PlaceNextActionReadyPulse",
    "PlaceStageUnlockReadyPulse",
    "ResetStageUnlockReadyPulse"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $progressionVisualsHookText -Needle $needle
}

foreach ($needle in @(
    "aliencrusher_progression.json",
    "aliencrusher_progression.bak.json",
    "LoadOrCreate",
    "MarkDirtyAndSave"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $progressionSaveText -Needle $needle
}

$assetCatalog = @(
    [pscustomobject]@{ Priority = "P0"; Category = "Lobby Card"; Asset = "UI_FormCard_StateSet"; RuntimeUse = "form selection and unlock state"; State = "default, locked, ready, equipped, recommended"; Folder = "Assets/Resources/UI/Lobby/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Meta Node"; Asset = "UI_MetaNode_SizeCore"; RuntimeUse = "opening failure fix recommendation"; State = "locked, affordable, purchased, maxed, recommended"; Folder = "Assets/Resources/UI/Meta/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Meta Node"; Asset = "UI_MetaNode_ImpactCore"; RuntimeUse = "elite/boss damage fix recommendation"; State = "locked, affordable, purchased, maxed, recommended"; Folder = "Assets/Resources/UI/Meta/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Meta Node"; Asset = "UI_MetaNode_DpAmplifier"; RuntimeUse = "economy pacing recommendation"; State = "locked, affordable, purchased, maxed, recommended"; Folder = "Assets/Resources/UI/Meta/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Currency"; Asset = "UI_DP_GainBurst"; RuntimeUse = "result DP reward and lobby total DP update"; State = "small gain, big gain, spend, insufficient"; Folder = "Assets/Resources/UI/Rewards/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Badge"; Asset = "Badge_FormReady"; RuntimeUse = "result/lobby form unlock readiness"; State = "ready, newly unlocked, equipped"; Folder = "Assets/Resources/UI/Badges/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Badge"; Asset = "Badge_MetaReady"; RuntimeUse = "result/lobby meta purchase readiness"; State = "ready, purchased, maxed"; Folder = "Assets/Resources/UI/Badges/" },
    [pscustomobject]@{ Priority = "P0"; Category = "Banner"; Asset = "Banner_StageUnlocked"; RuntimeUse = "stage clear and highest-stage advance"; State = "new best, next stage, boss-tier approaching"; Folder = "Assets/Resources/UI/Rewards/" },
    [pscustomobject]@{ Priority = "P1"; Category = "Toast"; Asset = "Toast_ProgressionSaved"; RuntimeUse = "save confirmation after DP spend/unlock/stage clear"; State = "saved, backup restored, migration complete"; Folder = "Assets/Resources/UI/Rewards/" },
    [pscustomobject]@{ Priority = "P1"; Category = "Audio"; Asset = "SFX_Progression_Confirm"; RuntimeUse = "purchase/unlock/selected positive confirmation"; State = "purchase, unlock, equip"; Folder = "Assets/Audio/SFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "Audio"; Asset = "SFX_Progression_Locked"; RuntimeUse = "not enough DP or stage locked"; State = "locked, insufficient DP"; Folder = "Assets/Audio/SFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_FormEquip_Confirm"; RuntimeUse = "lobby form equip confirmation pulse"; State = "equipped lock-in flash"; Folder = "Assets/Art/VFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_SpendChange_Ready"; RuntimeUse = "next-run spend-change readout on StartStage"; State = "form ready, meta ready"; Folder = "Assets/Art/VFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_StageSelect_Confirm"; RuntimeUse = "lobby stage select confirmation pulse"; State = "stage locked-in flash"; Folder = "Assets/Art/VFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_MetaUpgrade_Confirm"; RuntimeUse = "lobby meta purchase confirmation pulse"; State = "purchased lock-in flash"; Folder = "Assets/Art/VFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_DP_Gain_Residual"; RuntimeUse = "result/lobby DP gain residual afterglow"; State = "small gain afterglow, big gain afterglow"; Folder = "Assets/Art/VFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_NextAction_Ready"; RuntimeUse = "result next-action ready pulse"; State = "form ready, meta ready, recommended"; Folder = "Assets/Art/VFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_StageUnlock_Ready"; RuntimeUse = "result/lobby stage-unlock banner residual afterglow"; State = "new best, next stage"; Folder = "Assets/Art/VFX/UI/" },
    [pscustomobject]@{ Priority = "P1"; Category = "VFX"; Asset = "VFX_FormUnlock_Confirm"; RuntimeUse = "lobby form unlock confirmation pulse"; State = "newly unlocked lock-in flash"; Folder = "Assets/Art/VFX/UI/" }
)

$productionBatches = @(
    [pscustomobject]@{
        Batch = "A. DP economy signal"
        Goal = "make earn, spend, total, and insufficient DP states obvious"
        Targets = @("UI_DP_GainBurst", "Icon_DP", "SFX_Progression_Locked")
        Acceptance = "result and lobby clearly show what was earned, what was spent, and why a purchase failed"
    },
    [pscustomobject]@{
        Batch = "B. Form and meta choice states"
        Goal = "make the recommended next spend target louder than secondary options"
        Targets = @("UI_FormCard_StateSet", "UI_MetaNode_SizeCore", "UI_MetaNode_ImpactCore", "UI_MetaNode_DpAmplifier")
        Acceptance = "locked, affordable, equipped/purchased, maxed, and recommended states read without long copy"
    },
    [pscustomobject]@{
        Batch = "C. Result-to-lobby payoff"
        Goal = "connect run outcome to the next run's concrete change"
        Targets = @("Badge_FormReady", "Badge_MetaReady", "Banner_StageUnlocked")
        Acceptance = "result screen points to one next action and lobby confirms the new option/state"
    },
    [pscustomobject]@{
        Batch = "D. Persistence and confirmation feedback"
        Goal = "confirm unlocks, purchases, equips, and save safety without slowing the run-return rhythm"
        Targets = @("Toast_ProgressionSaved", "SFX_Progression_Confirm", "SFX_Progression_Locked")
        Acceptance = "positive/locked/save feedback is noticeable but does not require modal acknowledgement"
    },
    [pscustomobject]@{
        Batch = "E. Form equip confirm pulse"
        Goal = "make lobby form equip feel locked-in before stage start"
        Targets = @("VFX_FormEquip_Confirm")
        Acceptance = "equipping a form shows a short champagne ring pulse on that card that is not the cyan equipped frame, result badges, or in-run smash/route VFX"
    },
    [pscustomobject]@{
        Batch = "F. Next-run spend change pulse"
        Goal = "make the next run name what the last lobby spend changed"
        Targets = @("VFX_SpendChange_Ready")
        Acceptance = "starting a run after a lobby form or meta spend shows a short jade ready plate naming that change, not the champagne form-equip ring, result badges, or in-run smash/route VFX"
    },
    [pscustomobject]@{
        Batch = "G. Stage select confirm pulse"
        Goal = "make lobby stage select feel locked-in before stage start"
        Targets = @("VFX_StageSelect_Confirm")
        Acceptance = "selecting a lobby stage shows a short ice-slate bracket pulse on the stage readout that is not the champagne form-equip ring, jade spend-change plate, result badges, unlock banners, or in-run smash/route VFX"
    },
    [pscustomobject]@{
        Batch = "H. Meta purchase confirm pulse"
        Goal = "make a lobby meta spend feel locked-in before the next run"
        Targets = @("VFX_MetaUpgrade_Confirm")
        Acceptance = "buying a meta upgrade shows a short copper diamond pulse on that node that is not the champagne form-equip ring, ice-slate stage-select brackets, jade spend-change plate, result badges, or in-run smash/route VFX"
    },
    [pscustomobject]@{
        Batch = "I. DP gain residual afterglow"
        Goal = "keep earned DP readable after the existing gain burst fades"
        Targets = @("VFX_DP_Gain_Residual")
        Acceptance = "a successful DP earn shows a short aqua afterglow beside UI_DP_GainBurst that is not the burst itself, copper meta diamond, champagne form-equip ring, ice-slate stage-select brackets, or in-run smash/route VFX"
    },
    [pscustomobject]@{
        Batch = "J. Result next-action ready pulse"
        Goal = "make the result screen point at one next spend or unlock before retry"
        Targets = @("VFX_NextAction_Ready")
        Acceptance = "result opening with a form-ready, meta-ready, or recommended next action shows a short lilac caret pulse on that badge that is not the aqua DP residual, copper meta diamond, champagne form-equip ring, ice-slate stage-select brackets, jade spend-change plate, or in-run smash/route VFX"
    },
    [pscustomobject]@{
        Batch = "K. Stage unlock banner pulse"
        Goal = "make a newly unlocked stage readable before the next run"
        Targets = @("VFX_StageUnlock_Ready")
        Acceptance = "a first-time stage clear shows a short honey-ivory residual afterglow beside Banner_StageUnlocked that is not the banner itself, ice-slate stage-select brackets, lilac next-action caret, aqua DP residual, copper meta diamond, champagne form-equip ring, jade spend-change plate, or in-run smash/route VFX"
    },
    [pscustomobject]@{
        Batch = "L. Form unlock confirm pulse"
        Goal = "make a lobby form unlock feel newly opened before the next run"
        Targets = @("VFX_FormUnlock_Confirm")
        Acceptance = "unlocking a form shows a short periwinkle petal burst on that card that is not the champagne form-equip ring, cyan equipped frame, honey-ivory stage-unlock residual, lilac next-action caret, aqua DP residual, copper meta diamond, ice-slate stage-select brackets, jade spend-change plate, or in-run smash/route VFX"
    }
)

$loopRows = @(
    [pscustomobject]@{ Beat = "Earn"; RuntimeSignal = "DP +, TOTAL DP, BIG DP"; ProductionRule = "DP gain should visibly travel from result to next lobby decision" },
    [pscustomobject]@{ Beat = "Read"; RuntimeSignal = "NEXT FORM, NEXT META, recommendation text"; ProductionRule = "one primary spend target must be visually louder than secondary options" },
    [pscustomobject]@{ Beat = "Spend"; RuntimeSignal = "TryPurchaseMetaUpgrade, TryUnlockAndSelectWithCost"; ProductionRule = "affordable and insufficient states must be unmistakable" },
    [pscustomobject]@{ Beat = "Equip"; RuntimeSignal = "EQUIPPED, selected form, ApplySelectedFormToPlayer"; ProductionRule = "selected form must feel locked-in before stage start" },
    [pscustomobject]@{ Beat = "Return"; RuntimeSignal = "StartStage, StartNextStage, CurrentLobbyStage"; ProductionRule = "next run should clearly explain what changed because of the spend" }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Outgame Progression Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`, `{3}`, `{4}`, `{5}`, `{6}`, `{7}`' -f $metaProgressionSourcePath, $formFlowSourcePath, $stageFlowSourcePath, $formUnlockSourcePath, $progressionSaveSourcePath, $nextActionReadySourcePath, $stageUnlockReadySourcePath, $formUnlockConfirmSourcePath))
$lines.Add("")
$lines.Add("Purpose: turn the current DP, form unlock, meta upgrade, result reward, lobby recommendation, and save systems into a concrete outgame production checklist.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime markers missing: $(if ($missingRuntimeMarkers.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingRuntimeMarkers) })")
$lines.Add("- Checklist rows: $($assetCatalog.Count)")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Make DP gain, total DP, and insufficient DP states readable before adding decoration.")
$lines.Add("2. Build form card states for locked, affordable, equipped, and recommended forms.")
$lines.Add("3. Build meta node states for Size Core, Impact Core, and DP Amplifier with a strong recommended focus state.")
$lines.Add("4. Add result badges and stage-unlocked banners so a run clearly creates an outgame consequence.")
$lines.Add("5. Add confirmation/locked audio and save toasts after visual states are readable.")
$lines.Add("")
$lines.Add("## Production Batches")
$lines.Add("| Batch | Goal | Targets | Acceptance check |")
$lines.Add("|---|---|---|---|")
foreach ($batch in $productionBatches) {
    $targetNames = @($batch.Targets | ForEach-Object { "``$_``" })
    $lines.Add(('| {0} | {1} | {2} | {3} |' -f $batch.Batch, $batch.Goal, ([string]::Join(", ", $targetNames)), $batch.Acceptance))
}

$lines.Add("")
$lines.Add("## Next Outgame Progression Batch Task Card")
if ($productionBatches.Count -eq 0) {
    $lines.Add("- none")
}
else {
    $nextBatch = $productionBatches[0]
    $targetNames = @($nextBatch.Targets | ForEach-Object { "``$_``" })
    $runtimeUses = @()
    foreach ($targetName in $nextBatch.Targets) {
        $asset = $assetCatalog | Where-Object { $_.Asset -eq $targetName } | Select-Object -First 1
        if ($null -ne $asset) {
            $runtimeUses += "$($asset.Asset): $($asset.RuntimeUse)"
        }
    }

    $lines.Add("- Batch: $($nextBatch.Batch)")
    $lines.Add("- Goal: $($nextBatch.Goal)")
    $lines.Add("- Targets: $([string]::Join(', ', $targetNames))")
    $lines.Add("- Runtime uses: $([string]::Join('; ', $runtimeUses))")
    $lines.Add("- Acceptance: $($nextBatch.Acceptance)")
    $lines.Add("- Done means: DP gain, DP total, spend, and insufficient states have draft visuals, audio note, or explicit placeholder rules before adding broader lobby decoration.")
}

$lines.Add("")
$lines.Add("## Outgame Loop Contract")
$lines.Add("| Beat | Runtime signal | Production rule | Done? |")
$lines.Add("|---|---|---|---|")
foreach ($row in $loopRows) {
    $lines.Add(("| {0} | {1} | {2} | [ ] |" -f $row.Beat, $row.RuntimeSignal, $row.ProductionRule))
}

$lines.Add("")
$lines.Add("## Current Outgame Progression Targets")
$lines.Add("| Priority | Category | Asset | Runtime use | Required states | Folder | Done? |")
$lines.Add("|---|---|---|---|---|---|---|")
foreach ($asset in $assetCatalog) {
    $doneMark = if ($allOutgameHookText.Contains($asset.Asset)) { "[x]" } else { "[ ]" }
    $lines.Add(("| {0} | {1} | `{2}` | {3} | {4} | `{5}` | {6} |" -f $asset.Priority, $asset.Category, $asset.Asset, $asset.RuntimeUse, $asset.State, $asset.Folder, $doneMark))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Outgame UX should answer: what did I earn, what can I buy, what should I equip, and why is the next run different?")
$lines.Add("- Do not add more lobby text until the card/node states carry the meaning visually.")
$lines.Add("- Result badges should point to one next action, not celebrate everything equally.")
$lines.Add("- Leftover `VFX_NextAction_Ready` is the ready pulse on that one result next-action badge, not a second advice system.")
$lines.Add("- Leftover `VFX_StageUnlock_Ready` is the residual afterglow beside the existing stage-unlock banner, not a second banner or stage-select confirm.")
$lines.Add("- Leftover `VFX_FormUnlock_Confirm` is the unlock petal burst on a newly unlocked form card, not the champagne equip ring.")
$lines.Add("- Save confirmation should stay lightweight; interruption-heavy modals would slow the run-return rhythm.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Outgame progression checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
