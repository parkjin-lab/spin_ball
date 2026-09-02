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
$uiFlowPath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\DummyFlowController.UIFlow.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherMobileHudReadabilityStaticAudit.log"
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

if (-not (Test-Path -Path $uiFlowPath -PathType Leaf)) {
    $errors.Add("UI flow source not found: $uiFlowPath")
}

if ($errors.Count -eq 0) {
    $uiFlowText = Get-Content -Path $uiFlowPath -Raw

    foreach ($needle in @(
        "private static string CompactDirectionLabel(string directionLabel)",
        "private static string FormatHudRouteIndicatorText(string stateLabel, string directionLabel, float distanceMeters)",
        "private static void ConfigureMobileSafeText(Text text, int maxFontSize, int minFontSize, bool wrap)",
        "private void ApplyMobileHudTextSafeguards()",
        "text.resizeTextForBestFit = true;",
        "text.resizeTextMaxSize = Mathf.Max(minFontSize, maxFontSize);",
        "text.resizeTextMinSize = Mathf.Clamp(minFontSize, 8, text.resizeTextMaxSize);",
        "text.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;",
        "text.verticalOverflow = VerticalWrapMode.Truncate;",
        "ApplyMobileHudTextSafeguards();"
    )) {
        Add-RequiredTextCheck -Errors $errors -Text $uiFlowText -Needle $needle -Label "Mobile HUD safeguard"
    }

    foreach ($needle in @(
        'hudProgressText.text = $"HOLD {Mathf.RoundToInt(routeHoldProgress * 100f):0}%  {GetRouteHoldRemainingWrecks(num):0}  {Mathf.CeilToInt(GetRouteHoldRemainingSeconds()):0}s";',
        'hudProgressText.text = $"WRECK {num:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}";',
        'text4 = $"HOLD {Mathf.RoundToInt(GetRouteHoldProgress01(num) * 100f):0}%  {GetRouteHoldRemainingWrecks(num):0}";',
        "hudRouteIndicatorText.text = FormatHudRouteIndicatorText(text, text2, num);",
        'hudStageGoalText.text = $"HOLD  {Mathf.RoundToInt(routeHoldProgress * 100f):0}%  {routeHoldRemaining:0}";',
        'hudStageGoalText.text = $"GOAL  {num:0}/{num2:0}  BOSS";',
        'hudStageGoalText.text = $"GOAL  {num:0}/{num2:0}  PUSH";',
        'hudStageGoalText.text = $"GOAL  {num:0}/{num2:0}";'
    )) {
        Add-RequiredTextCheck -Errors $errors -Text $uiFlowText -Needle $needle -Label "Mobile HUD compact copy"
    }

    foreach ($unsafeNeedle in @(
        "ROUTE HOLD {Mathf.RoundToInt(routeHoldProgress * 100f):0}%  /  {GetRouteHoldRemainingWrecks(num):0} TO CLUSTER",
        "DESTRUCTION {num:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}",
        "ROUTE HOLD {Mathf.RoundToInt(GetRouteHoldProgress01(num) * 100f):0}%, {GetRouteHoldRemainingWrecks(num):0} to cluster",
        'hudProgressText.text = $"HOLD {Mathf.RoundToInt(routeHoldProgress * 100f):0}%  /  {GetRouteHoldRemainingWrecks(num):0} LEFT  /  {Mathf.CeilToInt(GetRouteHoldRemainingSeconds()):0}s";',
        'hudProgressText.text = flag2 ? $"WRECK {num:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}  {Mathf.RoundToInt(num3 * 100f)}%  /  NEXT" : $"WRECK {num:0}/{Mathf.Max(0, stageTotalDestructibleCount):0}  {Mathf.RoundToInt(num3 * 100f)}%";',
        'text4 = $"NEXT  /  HOLD {Mathf.RoundToInt(GetRouteHoldProgress01(num) * 100f):0}%, {GetRouteHoldRemainingWrecks(num):0} left";',
        'hudStageGoalText.text = $"HOLD  {Mathf.RoundToInt(routeHoldProgress * 100f):0}%  /  {routeHoldRemaining:0} LEFT";',
        'hudStageGoalText.text = $"NEXT  {num:0}/{num2:0}  /  BOSS";',
        'hudStageGoalText.text = $"NEXT  {num:0}/{num2:0}  /  PUSH";',
        'hudStageGoalText.text = $"NEXT  {num:0}/{num2:0}";',
        'hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}  /  BOSS BLOCKING";',
        'hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}  /  FINAL PUSH";',
        'hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}  /  GOOD START";',
        'hudStageGoalText.text = $"NEXT STAGE  {num:0}/{num2:0}";'
    )) {
        if ($uiFlowText.Contains($unsafeNeedle)) {
            $warnings.Add("Mobile HUD still contains long runtime copy: $unsafeNeedle")
        }
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][MobileHudReadabilityStaticAudit] Mobile HUD readability audit")
$lines.Add("UI flow source: $uiFlowPath")

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
