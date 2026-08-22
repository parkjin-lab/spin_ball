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
        "BOSS_Sentinel_Body_Kit",
        "BOSS_Shield_Pylon_Kit",
        "BOSS_Phase2_Drone_Kit",
        "PAL_RouteMarker_Tints",
        "Icon_DP",
        "Icon_Stage",
        "Icon_NextStep",
        "Icon_Route"
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
        "BOSS_Sentinel_Body_Kit",
        "BOSS_Shield_Pylon_Kit",
        "BOSS_Phase2_Drone_Kit",
        "PAL_RouteMarker_Tints",
        "Icon_DP",
        "Icon_Stage",
        "Icon_NextStep",
        "Icon_Route",
        "Assets/Resources/UI/Icons/"
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
