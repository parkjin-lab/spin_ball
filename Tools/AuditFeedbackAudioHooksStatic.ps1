[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
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

$feedbackText = ""
$bossEncounterText = ""
$stageFlowText = ""
if ($errors.Count -eq 0) {
    $feedbackText = Get-Content -Path $feedbackPath -Raw
    $bossEncounterText = Get-Content -Path $bossEncounterPath -Raw
    $stageFlowText = Get-Content -Path $stageFlowPath -Raw

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
        "private void PlayAudio(AudioClip clip",
        "audioSource.PlayOneShot"
    )) {
        Add-RequiredTextCheck -Errors $errors -Text $feedbackText -Needle $needle -Label "Feedback audio hook"
    }

    foreach ($needle in @(
        "PlayAudio(normalizedImpact > 0.62f ? hitMediumClip : hitLightClip",
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
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][FeedbackAudioHooksStaticAudit] Feedback audio hooks audit")
$lines.Add("Feedback source: $feedbackPath")
$lines.Add("Boss encounter source: $bossEncounterPath")
$lines.Add("Stage flow source: $stageFlowPath")

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
