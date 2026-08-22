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

function Add-RequiredTextCheck {
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
$bossEncounterPath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\DummyFlowController.StageEncounter.cs"
$stageFlowPath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\DummyFlowController.StageFlow.cs"
$progressionCorePath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\DummyFlowController.ProgressionCore.cs"
$draftClipCatalog = @(
    [pscustomobject]@{ Field = "routeOpenClip"; AssetName = "SFX_Route_Open"; DocumentedPath = "Assets\Audio\SFX\Skills\SFX_Route_Open.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Skills\SFX_Route_Open.wav" },
    [pscustomobject]@{ Field = "routeHoldWarningClip"; AssetName = "SFX_Route_HoldWarning"; DocumentedPath = "Assets\Audio\SFX\Skills\SFX_Route_HoldWarning.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Skills\SFX_Route_HoldWarning.wav" },
    [pscustomobject]@{ Field = "routeBonusClip"; AssetName = "SFX_Route_Bonus"; DocumentedPath = "Assets\Audio\SFX\Skills\SFX_Route_Bonus.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Skills\SFX_Route_Bonus.wav" },
    [pscustomobject]@{ Field = "failureWarningClip"; AssetName = "SFX_Failure_Warning"; DocumentedPath = "Assets\Audio\SFX\Failure\SFX_Failure_Warning.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Failure\SFX_Failure_Warning.wav" },
    [pscustomobject]@{ Field = "failureBossClip"; AssetName = "SFX_Failure_Boss"; DocumentedPath = "Assets\Audio\SFX\Failure\SFX_Failure_Boss.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Failure\SFX_Failure_Boss.wav" },
    [pscustomobject]@{ Field = "hitLightClip"; AssetName = "SFX_Hit_Light"; DocumentedPath = "Assets\Audio\SFX\Impact\SFX_Hit_Light.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Impact\SFX_Hit_Light.wav" },
    [pscustomobject]@{ Field = "hitMediumClip"; AssetName = "SFX_Hit_Medium"; DocumentedPath = "Assets\Audio\SFX\Impact\SFX_Hit_Medium.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Impact\SFX_Hit_Medium.wav" },
    [pscustomobject]@{ Field = "hitHeavyClip"; AssetName = "SFX_Hit_Heavy"; DocumentedPath = "Assets\Audio\SFX\Impact\SFX_Hit_Heavy.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Impact\SFX_Hit_Heavy.wav" },
    [pscustomobject]@{ Field = "breakSmallClip"; AssetName = "SFX_Break_Small"; DocumentedPath = "Assets\Audio\SFX\Impact\SFX_Break_Small.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Impact\SFX_Break_Small.wav" },
    [pscustomobject]@{ Field = "breakLargeClip"; AssetName = "SFX_Break_LargeCollapse"; DocumentedPath = "Assets\Audio\SFX\Impact\SFX_Break_LargeCollapse.wav"; ResourcePath = "Assets\Resources\Audio\SFX\Impact\SFX_Break_LargeCollapse.wav" }
)

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherFeedbackAudioHooksStaticAudit.log"
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

if (-not (Test-Path -Path $feedbackPath -PathType Leaf)) {
    $errors.Add("FeedbackSystem not found: $feedbackPath")
}

if (-not (Test-Path -Path $bossEncounterPath -PathType Leaf)) {
    $errors.Add("Boss encounter partial not found: $bossEncounterPath")
}

if (-not (Test-Path -Path $stageFlowPath -PathType Leaf)) {
    $errors.Add("Stage flow partial not found: $stageFlowPath")
}

if (-not (Test-Path -Path $progressionCorePath -PathType Leaf)) {
    $errors.Add("Progression core partial not found: $progressionCorePath")
}

$feedbackText = ""
$bossEncounterText = ""
$stageFlowText = ""
$progressionCoreText = ""
if ($errors.Count -eq 0) {
    $feedbackText = Get-Content -Path $feedbackPath -Raw
    $bossEncounterText = Get-Content -Path $bossEncounterPath -Raw
    $stageFlowText = Get-Content -Path $stageFlowPath -Raw
    $progressionCoreText = Get-Content -Path $progressionCorePath -Raw

    foreach ($needle in @(
        "private bool allowAudio",
        "private AudioSource audioSource",
        "private float audioVolume",
        "private AudioClip hitLightClip",
        "private AudioClip hitMediumClip",
        "private AudioClip hitHeavyClip",
        "private AudioClip breakSmallClip",
        "private AudioClip breakLargeClip",
        "private AudioClip comboRiseClip",
        "private AudioClip routeOpenClip",
        "private AudioClip routeHoldWarningClip",
        "private AudioClip routeBonusClip",
        "private AudioClip bossWarningClip",
        "private AudioClip bossBreakClip",
        "private AudioClip bossDownClip",
        "private AudioClip levelUpClip",
        "private AudioClip failureWarningClip",
        "private AudioClip failureBossClip",
        "public bool AllowAudio",
        "private void EnsureAudioSource()",
        "private void EnsureDraftRhythmClips()",
        "private AudioClip ResolveHitWeightClip",
        "private static AudioClip LoadDraftClip",
        "public void PlayRouteOpenCue",
        "private void PlayAudio(AudioClip clip",
        "audioSource.PlayOneShot"
    )) {
        Add-RequiredTextCheck -Errors $errors -Text $feedbackText -Needle $needle -Label "Feedback audio hook"
    }

    foreach ($needle in @(
        "PlayAudio(ResolveHitWeightClip(normalizedImpact, forceHeavy)",
        "return normalizedImpact > 0.62f ? hitMediumClip : hitLightClip",
        "PlayAudio(normalizedImpact > 0.7f ? breakLargeClip : breakSmallClip",
        "PlayAudio(comboRiseClip",
        "PlayAudio(levelUpClip",
        "PlayAudio(routeBonusClip",
        "PlayAudio(routeOpenClip",
        "PlayAudio(swarmBroken ? bossBreakClip : bossWarningClip",
        "PlayAudio(bossCore ? bossBreakClip : hitHeavyClip",
        "PlayAudio(bossRelated ? bossWarningClip : routeHoldWarningClip",
        "public void PlayFailureBeatFeedback",
        "private AudioClip ResolveFailureClip",
        "PlayAudio(ResolveFailureClip(bossRelated)",
        "public void PlayBossDownFeedback",
        "PlayAudio(bossDownClip"
    )) {
        Add-RequiredTextCheck -Errors $errors -Text $feedbackText -Needle $needle -Label "Feedback audio event"
    }

    Add-RequiredTextCheck -Errors $errors -Text $bossEncounterText -Needle "feedbackSystem?.PlayBossDownFeedback" -Label "Boss down audio call"
    Add-RequiredTextCheck -Errors $errors -Text $stageFlowText -Needle "PlayStageDefeatFeedback()" -Label "Stage defeat feedback call"
    Add-RequiredTextCheck -Errors $errors -Text $stageFlowText -Needle "feedbackSystem?.PlayFailureBeatFeedback" -Label "Stage defeat feedback call"
    Add-RequiredTextCheck -Errors $errors -Text $progressionCoreText -Needle "feedbackSystem?.PlayRouteOpenCue" -Label "Route open audio call"

    foreach ($clip in $draftClipCatalog) {
        $documentedPath = Join-Path $projectRoot $clip.DocumentedPath
        $resourcePath = Join-Path $projectRoot $clip.ResourcePath
        if (-not (Test-Path -Path $documentedPath -PathType Leaf)) {
            $errors.Add("Draft clip missing for $($clip.Field): $documentedPath")
        }
        elseif ((Get-Item -Path $documentedPath).Length -lt 16) {
            $errors.Add("Draft clip is too small to be a real audio asset for $($clip.Field): $documentedPath")
        }

        if (-not (Test-Path -Path $resourcePath -PathType Leaf)) {
            $errors.Add("Runtime draft clip missing for $($clip.Field): $resourcePath")
        }
        elseif ((Get-Item -Path $resourcePath).Length -lt 16) {
            $errors.Add("Runtime draft clip is too small to be a real audio asset for $($clip.Field): $resourcePath")
        }
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][FeedbackAudioHooksStaticAudit] Feedback audio hooks audit")
$lines.Add("Feedback source: $feedbackPath")
$lines.Add("Boss encounter source: $bossEncounterPath")
$lines.Add("Stage flow source: $stageFlowPath")
$lines.Add("Progression core source: $progressionCorePath")

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
