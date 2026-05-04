[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ScenePath = "",
    [string]$ReportPath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Resolve-ProjectPath {
    param([string]$ProjectRoot, [string]$Path, [string]$FallbackRelativePath)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return Join-Path $ProjectRoot $FallbackRelativePath
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $ProjectRoot $Path
}

function Get-UnityObjectRecord {
    param([string]$SceneText, [string]$ObjectName)

    $pattern = "(?ms)^--- !u!1 &(?<id>\d+)\r?\nGameObject:\r?\n(?<body>.*?)(?=^--- !u!|\z)"
    foreach ($match in [regex]::Matches($SceneText, $pattern)) {
        if ($match.Groups["body"].Value -match "(?m)^\s*m_Name:\s+$([regex]::Escape($ObjectName))\s*$") {
            return [pscustomobject]@{
                Name = $ObjectName
                Id = [int64]$match.Groups["id"].Value
                Body = $match.Groups["body"].Value
            }
        }
    }

    return $null
}

function Get-ComponentIds {
    param($ObjectRecord)

    $ids = [System.Collections.Generic.List[int64]]::new()
    if ($null -eq $ObjectRecord) {
        return $ids
    }

    foreach ($match in [regex]::Matches($ObjectRecord.Body, "fileID:\s*(?<id>\d+)")) {
        $ids.Add([int64]$match.Groups["id"].Value)
    }

    return $ids
}

function Get-UnityComponentBlock {
    param([string]$SceneText, [int64]$ComponentId, [string]$UnityType)

    $pattern = "(?ms)^--- !u!$UnityType &$ComponentId\r?\n.*?(?=^--- !u!|\z)"
    $match = [regex]::Match($SceneText, $pattern)
    if ($match.Success) {
        return $match.Value
    }

    return ""
}

function Get-RectTransformId {
    param([string]$SceneText, $ObjectRecord)

    foreach ($componentId in (Get-ComponentIds -ObjectRecord $ObjectRecord)) {
        $block = Get-UnityComponentBlock -SceneText $SceneText -ComponentId $componentId -UnityType "224"
        if (-not [string]::IsNullOrWhiteSpace($block)) {
            return $componentId
        }
    }

    return 0
}

function Test-TextComponent {
    param([string]$SceneText, $ObjectRecord)

    foreach ($componentId in (Get-ComponentIds -ObjectRecord $ObjectRecord)) {
        $block = Get-UnityComponentBlock -SceneText $SceneText -ComponentId $componentId -UnityType "114"
        if ($block -match "m_EditorClassIdentifier:\s*UnityEngine\.UI::UnityEngine\.UI\.Text") {
            return $true
        }
    }

    return $false
}

function Add-CheckResult {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [ref]$WarningCount,
        [string]$Label,
        [bool]$Passed,
        [string]$WarningText
    )

    if ($Passed) {
        $Lines.Add("OK: $Label")
        return
    }

    $WarningCount.Value++
    $Lines.Add("WARN: $WarningText")
}

$projectRoot = Resolve-ProjectRoot
$resolvedScenePath = Resolve-ProjectPath -ProjectRoot $projectRoot -Path $ScenePath -FallbackRelativePath "Assets\Scenes\SampleScene.unity"
if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherSceneEssentialsStaticAudit.log"
}

$lines = [System.Collections.Generic.List[string]]::new()
$warningCount = 0

$lines.Add("[AlienCrusher][SceneEssentialsStaticAudit] Scene essentials audit")
$lines.Add("Scene: $resolvedScenePath")

if (-not (Test-Path -Path $resolvedScenePath -PathType Leaf)) {
    $warningCount++
    $lines.Add("WARN: scene file not found")
}
else {
    $sceneText = Get-Content -Path $resolvedScenePath -Raw

    $requiredObjects = @(
        "MapRoot",
        "TargetMarkers",
        "Target_A",
        "Target_B",
        "Canvas_Dummy",
        "HUD_Dummy",
        "ObjectiveText",
        "HudRouteIndicatorText",
        "HudRouteArrow",
        "ArrowText"
    )

    $records = @{}
    foreach ($objectName in $requiredObjects) {
        $record = Get-UnityObjectRecord -SceneText $sceneText -ObjectName $objectName
        $records[$objectName] = $record
        Add-CheckResult -Lines $lines -WarningCount ([ref]$warningCount) -Label $objectName -Passed ($null -ne $record) -WarningText "Missing $objectName"
    }

    foreach ($textObjectName in @("ObjectiveText", "HudRouteIndicatorText", "ArrowText")) {
        $record = $records[$textObjectName]
        $hasText = $null -ne $record -and (Test-TextComponent -SceneText $sceneText -ObjectRecord $record)
        Add-CheckResult -Lines $lines -WarningCount ([ref]$warningCount) -Label "$textObjectName Text" -Passed $hasText -WarningText "Missing Text binding for $textObjectName"
    }

    $hudRectId = Get-RectTransformId -SceneText $sceneText -ObjectRecord $records["HUD_Dummy"]
    $routeArrowRectId = Get-RectTransformId -SceneText $sceneText -ObjectRecord $records["HudRouteArrow"]
    $arrowTextRectId = Get-RectTransformId -SceneText $sceneText -ObjectRecord $records["ArrowText"]

    $routeArrowRectBlock = if ($routeArrowRectId -gt 0) { Get-UnityComponentBlock -SceneText $sceneText -ComponentId $routeArrowRectId -UnityType "224" } else { "" }
    $arrowTextRectBlock = if ($arrowTextRectId -gt 0) { Get-UnityComponentBlock -SceneText $sceneText -ComponentId $arrowTextRectId -UnityType "224" } else { "" }

    $routeArrowUnderHud = $hudRectId -gt 0 -and $routeArrowRectId -gt 0 -and $routeArrowRectBlock -match "m_Father:\s*\{fileID:\s*$hudRectId\}"
    Add-CheckResult -Lines $lines -WarningCount ([ref]$warningCount) -Label "HudRouteArrow parented to HUD_Dummy" -Passed $routeArrowUnderHud -WarningText "HudRouteArrow is not parented to HUD_Dummy"

    $arrowTextUnderRouteArrow = $routeArrowRectId -gt 0 -and $arrowTextRectId -gt 0 -and $arrowTextRectBlock -match "m_Father:\s*\{fileID:\s*$routeArrowRectId\}"
    Add-CheckResult -Lines $lines -WarningCount ([ref]$warningCount) -Label "ArrowText parented to HudRouteArrow" -Passed $arrowTextUnderRouteArrow -WarningText "ArrowText is not parented to HudRouteArrow"
}

$lines.Add("Result: 0 error(s), $warningCount warning(s)")

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output $report

if ($FailOnWarnings -and $warningCount -gt 0) {
    exit 1
}

exit 0
