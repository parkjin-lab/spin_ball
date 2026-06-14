[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$MetaProgressionPath = "",
    [string]$FormFlowPath = "",
    [string]$StageFlowPath = "",
    [string]$FormUnlockSystemPath = "",
    [string]$ProgressionSaveSystemPath = ""
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
    "GetRecommendedMetaContextTag"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $metaProgressionText -Needle $needle
}

foreach ($needle in @(
    "UpdateFormButtons",
    "UpdateFormButton",
    "TryUnlockAndSelectWithCost",
    "EQUIPPED",
    "RECOMMENDED",
    "GetEarlyLobbyFormUnlockHint"
)) {
    Add-MissingMarker -Missing $missingRuntimeMarkers -Source $formFlowText -Needle $needle
}

foreach ($needle in @(
    "UpdateResultStatusBadges",
    "UpdateResultMetaProgress",
    "GetResultNextFormHint",
    "GetResultNextMetaHint",
    "RETURN TO LOBBY / SPEND DP",
    "DP +",
    "BEST STAGE"
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
    [pscustomobject]@{ Priority = "P1"; Category = "Audio"; Asset = "SFX_Progression_Locked"; RuntimeUse = "not enough DP or stage locked"; State = "locked, insufficient DP"; Folder = "Assets/Audio/SFX/UI/" }
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
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`, `{3}`, `{4}`' -f $metaProgressionSourcePath, $formFlowSourcePath, $stageFlowSourcePath, $formUnlockSourcePath, $progressionSaveSourcePath))
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
    $lines.Add(("| {0} | {1} | `{2}` | {3} | {4} | `{5}` | [ ] |" -f $asset.Priority, $asset.Category, $asset.Asset, $asset.RuntimeUse, $asset.State, $asset.Folder))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Outgame UX should answer: what did I earn, what can I buy, what should I equip, and why is the next run different?")
$lines.Add("- Do not add more lobby text until the card/node states carry the meaning visually.")
$lines.Add("- Result badges should point to one next action, not celebrate everything equally.")
$lines.Add("- Save confirmation should stay lightweight; interruption-heavy modals would slow the run-return rhythm.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Outgame progression checklist written: $ReportPath"

if ($missingRuntimeMarkers.Count -gt 0) {
    exit 1
}

exit 0
