[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [string]$RuntimeConfigPath = "",
    [string]$GameFlowConfigPath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Clamp-Double {
    param([double]$Value, [double]$Min, [double]$Max)
    return [Math]::Min([Math]::Max($Value, $Min), $Max)
}

function Clamp-Int {
    param([int]$Value, [int]$Min, [int]$Max)
    return [Math]::Min([Math]::Max($Value, $Min), $Max)
}

function Lerp {
    param([double]$A, [double]$B, [double]$T)
    return $A + (($B - $A) * $T)
}

function Resolve-ProjectPath {
    param([string]$OverridePath, [string]$RelativePath)

    $projectRoot = Split-Path -Parent $PSScriptRoot
    if (-not [string]::IsNullOrWhiteSpace($OverridePath)) {
        if ([System.IO.Path]::IsPathRooted($OverridePath)) {
            return $OverridePath
        }

        return Join-Path $projectRoot $OverridePath
    }

    return Join-Path $projectRoot $RelativePath
}

function Read-SourceText {
    param(
        [string]$Path,
        [string]$Label,
        [System.Collections.Generic.List[string]]$Warnings
    )

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        $Warnings.Add("$Label config source not found: $Path")
        return ""
    }

    return Get-Content -Path $Path -Raw
}

function Read-CSharpNumberDefault {
    param(
        [string]$SourceText,
        [string]$FieldName,
        [double]$Fallback,
        [string]$Kind,
        [string]$SourceLabel,
        [System.Collections.Generic.List[string]]$Warnings
    )

    $escapedFieldName = [regex]::Escape($FieldName)
    $pattern = "(?m)^\s*(?:\[[^\]]+\]\s*)*(?:private|protected|public)\s+(?:int|float|double)\s+$escapedFieldName\s*=\s*([-+]?(?:\d+(?:\.\d+)?|\.\d+))(?:[fFdD])?\s*;"
    $match = [regex]::Match($SourceText, $pattern)
    if (-not $match.Success) {
        $Warnings.Add("$SourceLabel default '$FieldName' not found; using fallback $Fallback")
        if ($Kind -eq "int") {
            return [int]$Fallback
        }

        return [double]$Fallback
    }

    $raw = $match.Groups[1].Value
    if ($Kind -eq "int") {
        return [int][double]$raw
    }

    return [double]$raw
}

function Resolve-MapGrowth {
    param([int]$Stage, [int]$GrowthMax)

    $stageNumber = [Math]::Max(1, $Stage)
    $growthMaxStage = [Math]::Max(1, $GrowthMax)
    $growthTier = Clamp-Int -Value ($stageNumber - 1) -Min 0 -Max ($growthMaxStage - 1)
    $growth01 = if ($growthMaxStage -le 1) { 0.0 } else { Clamp-Double -Value ($growthTier / [double]($growthMaxStage - 1)) -Min 0.0 -Max 1.0 }
    $cells = Clamp-Int -Value (13 + $growthTier) -Min 13 -Max 19

    return [pscustomobject]@{
        Stage = $stageNumber
        GrowthTier = $growthTier
        Growth01 = $growth01
        XCells = $cells
        ZCells = $cells
    }
}

function Resolve-MinimumDestructibles {
    param($Growth)

    $ratio = Lerp 0.34 0.42 $Growth.Growth01
    return [Math]::Max(52, [Math]::Round($Growth.XCells * $Growth.ZCells * $ratio))
}

function Resolve-StageAdvanceTarget {
    param(
        [int]$Stage,
        [int]$DestructibleCount,
        [int]$BaseTarget,
        [int]$PerStage,
        [double]$TargetRatio,
        [int]$BossStageStart
    )

    $ratio = $TargetRatio
    if ($Stage -le 2) {
        $ratio -= 0.11
    }
    elseif ($Stage -eq 3) {
        $ratio -= 0.03
    }

    $ratioTarget = [Math]::Round($DestructibleCount * (Clamp-Double -Value $ratio -Min 0.2 -Max 0.95))
    $baseGate = [Math]::Max(4, $BaseTarget + ([Math]::Max(0, $Stage - 1) * [Math]::Max(0, $PerStage)))
    if ($Stage -eq 1) {
        $baseGate = [Math]::Max(4, $baseGate - 2)
    }
    elseif ($Stage -eq 2) {
        $baseGate = [Math]::Max(4, $baseGate - 1)
    }

    $target = [Math]::Max($baseGate, $ratioTarget)
    if ($Stage -ge [Math]::Max(2, $BossStageStart)) {
        $target = [Math]::Max($target, [Math]::Round([Math]::Max(1, $DestructibleCount) * 0.44))
    }

    return Clamp-Int -Value $target -Min 4 -Max ([Math]::Max(4, $DestructibleCount))
}

function Resolve-RouteHoldTarget {
    param([int]$StageAdvanceTarget, [int]$LaneBreakTarget, [double]$ProgressThreshold)

    if ($StageAdvanceTarget -le 0) {
        return $LaneBreakTarget
    }

    $threshold = Clamp-Double -Value $ProgressThreshold -Min 0.25 -Max 0.75
    return [Math]::Max($LaneBreakTarget, [Math]::Ceiling($StageAdvanceTarget * $threshold))
}

function Resolve-ActivePipCount {
    param(
        [double]$Distance,
        [int]$AllocatedCount,
        [int]$ConfiguredCount,
        [double]$MaxDistance,
        [double]$MinPipSpacing,
        [double]$CloseHideDistance
    )

    if ($Distance -le [Math]::Max(0.75, $CloseHideDistance)) {
        return [pscustomobject]@{ Active = 0; VisibleDistance = 0.0; SegmentSpacing = 0.0 }
    }

    $visibleDistance = [Math]::Min($Distance, [Math]::Max(4.0, $MaxDistance))
    $maxCount = Clamp-Int -Value $AllocatedCount -Min 0 -Max (Clamp-Int -Value $ConfiguredCount -Min 3 -Max 8)
    if ($maxCount -le 0) {
        return [pscustomobject]@{ Active = 0; VisibleDistance = $visibleDistance; SegmentSpacing = 0.0 }
    }

    $spacing = [Math]::Max(0.75, $MinPipSpacing)
    $distanceCount = [Math]::Floor($visibleDistance / $spacing)
    $active = Clamp-Int -Value $distanceCount -Min ([Math]::Min(2, $maxCount)) -Max $maxCount
    $segmentSpacing = $visibleDistance / ($active + 1)
    return [pscustomobject]@{ Active = $active; VisibleDistance = $visibleDistance; SegmentSpacing = $segmentSpacing }
}

function Format-RoutePips {
    param($Samples)

    $parts = [System.Collections.Generic.List[string]]::new()
    foreach ($sample in $Samples) {
        $parts.Add(("{0:0.#}m:{1}p/{2:0.00}m" -f $sample.Distance, $sample.Active, $sample.SegmentSpacing))
    }
    return [string]::Join(", ", $parts)
}

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $projectRoot = Split-Path -Parent $PSScriptRoot
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherRouteHoldStaticAudit.log"
}

$configWarnings = [System.Collections.Generic.List[string]]::new()
$resolvedRuntimeConfigPath = Resolve-ProjectPath -OverridePath $RuntimeConfigPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.cs"
$resolvedGameFlowConfigPath = Resolve-ProjectPath -OverridePath $GameFlowConfigPath -RelativePath "Assets\Scripts\Runtime\Systems\GameFlowSystem.cs"
$runtimeConfigText = Read-SourceText -Path $resolvedRuntimeConfigPath -Label "Runtime" -Warnings $configWarnings
$gameFlowConfigText = Read-SourceText -Path $resolvedGameFlowConfigPath -Label "Game flow" -Warnings $configWarnings

$stageAdvanceBaseTarget = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "stageAdvanceBaseTarget" -Fallback 16 -Kind "int" -SourceLabel "Runtime" -Warnings $configWarnings
$stageAdvanceTargetPerStage = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "stageAdvanceTargetPerStage" -Fallback 3 -Kind "int" -SourceLabel "Runtime" -Warnings $configWarnings
$stageAdvanceTargetRatio = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "stageAdvanceTargetRatio" -Fallback 0.48 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$bossStageStart = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "bossStageStart" -Fallback 4 -Kind "int" -SourceLabel "Runtime" -Warnings $configWarnings
$earlyCrushFlowWindowSeconds = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "earlyCrushFlowWindowSeconds" -Fallback 18.0 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$earlyCrushLaneBreakTarget = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "earlyCrushLaneBreakTarget" -Fallback 9 -Kind "int" -SourceLabel "Runtime" -Warnings $configWarnings
$routeHoldWindowSeconds = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldWindowSeconds" -Fallback 38.0 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$routeHoldProgressThreshold = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldProgressThreshold" -Fallback 0.45 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$routeOpenBeatSeconds = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeOpenBeatSeconds" -Fallback 2.0 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$routeHoldTrailPipCount = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldTrailPipCount" -Fallback 5 -Kind "int" -SourceLabel "Runtime" -Warnings $configWarnings
$routeHoldTrailMaxDistance = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldTrailMaxDistance" -Fallback 18.0 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$routeHoldTrailMinPipSpacing = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldTrailMinPipSpacing" -Fallback 1.65 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$routeHoldTrailCloseHideDistance = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldTrailCloseHideDistance" -Fallback 2.4 -Kind "double" -SourceLabel "Runtime" -Warnings $configWarnings
$stageDurationSeconds = Read-CSharpNumberDefault -SourceText $gameFlowConfigText -FieldName "stageDurationSeconds" -Fallback 90.0 -Kind "double" -SourceLabel "Game flow" -Warnings $configWarnings
$distanceSamples = @(2.0, 3.0, 6.0, 12.0, 18.0, 24.0)

$lines = [System.Collections.Generic.List[string]]::new()
$warningCount = 0

$lines.Add("[AlienCrusher][RouteHoldStaticAudit] ROUTE HOLD tuning audit")
$lines.Add("Range: Stage 01-$("{0:00}" -f $MaxStage)")
$lines.Add("Runtime config: $resolvedRuntimeConfigPath")
$lines.Add("Game flow config: $resolvedGameFlowConfigPath")
$lines.Add(("Tuning: stageBase={0} perStage={1} ratio={2:0.##} laneBreakWindow={3:0.#}s laneBreakTarget={4} routeWindow={5:0.#}s routeThreshold={6:0.##} routeOpenBeat={7:0.#}s pips={8} maxDistance={9:0.#}m minSpacing={10:0.##}m closeHide={11:0.##}m stageDuration={12:0.#}s" -f `
    $stageAdvanceBaseTarget, `
    $stageAdvanceTargetPerStage, `
    $stageAdvanceTargetRatio, `
    $earlyCrushFlowWindowSeconds, `
    $earlyCrushLaneBreakTarget, `
    $routeHoldWindowSeconds, `
    $routeHoldProgressThreshold, `
    $routeOpenBeatSeconds, `
    $routeHoldTrailPipCount, `
    $routeHoldTrailMaxDistance, `
    $routeHoldTrailMinPipSpacing, `
    $routeHoldTrailCloseHideDistance, `
    $stageDurationSeconds))
foreach ($configWarning in $configWarnings) {
    $warningCount++
    $lines.Add("WARN: $configWarning")
}

$deadlineSeconds = [Math]::Max($earlyCrushFlowWindowSeconds, $routeHoldWindowSeconds)
if ($deadlineSeconds -ge $stageDurationSeconds) {
    $warningCount++
    $lines.Add("WARN: route hold deadline is not inside the stage timer")
}
if ($routeOpenBeatSeconds -lt 0.6 -or $routeOpenBeatSeconds -gt 4.0) {
    $warningCount++
    $lines.Add("WARN: route open beat should stay between 0.6s and 4.0s for readability")
}

for ($stage = 1; $stage -le [Math]::Max(1, $MaxStage); $stage++) {
    $growth = Resolve-MapGrowth -Stage $stage -GrowthMax $MaxGrowthStage
    $minimumDestructibles = Resolve-MinimumDestructibles -Growth $growth
    $stageTarget = Resolve-StageAdvanceTarget `
        -Stage $stage `
        -DestructibleCount $minimumDestructibles `
        -BaseTarget $stageAdvanceBaseTarget `
        -PerStage $stageAdvanceTargetPerStage `
        -TargetRatio $stageAdvanceTargetRatio `
        -BossStageStart $bossStageStart
    $routeTarget = Resolve-RouteHoldTarget -StageAdvanceTarget $stageTarget -LaneBreakTarget $earlyCrushLaneBreakTarget -ProgressThreshold $routeHoldProgressThreshold
    $remainingAfterLaneBreak = [Math]::Max(0.1, $deadlineSeconds - $earlyCrushFlowWindowSeconds)
    $extraWrecks = [Math]::Max(0, $routeTarget - $earlyCrushLaneBreakTarget)
    $wrecksPerSecondAfterLaneBreak = $extraWrecks / $remainingAfterLaneBreak
    $warnings = [System.Collections.Generic.List[string]]::new()

    if ($routeTarget -le $earlyCrushLaneBreakTarget) {
        $warnings.Add("route target does not extend beyond lane break")
    }
    if ($routeTarget -ge $stageTarget) {
        $warnings.Add("route target reaches stage gate")
    }
    if ($wrecksPerSecondAfterLaneBreak -gt 1.45) {
        $warnings.Add("route hold pressure too high")
    }
    if ($routeHoldTrailCloseHideDistance -lt 1.5 -or $routeHoldTrailCloseHideDistance -gt 4.5) {
        $warnings.Add("close-hide distance outside mobile readability band")
    }
    if ($routeHoldTrailMinPipSpacing -lt 1.0 -or $routeHoldTrailMinPipSpacing -gt 2.6) {
        $warnings.Add("pip spacing outside mobile readability band")
    }

    $pipSamples = [System.Collections.Generic.List[object]]::new()
    foreach ($distance in $distanceSamples) {
        $sample = Resolve-ActivePipCount `
            -Distance $distance `
            -AllocatedCount $routeHoldTrailPipCount `
            -ConfiguredCount $routeHoldTrailPipCount `
            -MaxDistance $routeHoldTrailMaxDistance `
            -MinPipSpacing $routeHoldTrailMinPipSpacing `
            -CloseHideDistance $routeHoldTrailCloseHideDistance
        $pipSamples.Add([pscustomobject]@{
            Distance = $distance
            Active = $sample.Active
            SegmentSpacing = $sample.SegmentSpacing
        })
    }

    $nearSample = $pipSamples | Where-Object { $_.Distance -eq 2.0 } | Select-Object -First 1
    $closeSample = $pipSamples | Where-Object { $_.Distance -eq 3.0 } | Select-Object -First 1
    $farSample = $pipSamples | Where-Object { $_.Distance -eq 18.0 } | Select-Object -First 1
    if ($nearSample.Active -ne 0) {
        $warnings.Add("near target trail should hide")
    }
    if ($closeSample.Active -gt 2) {
        $warnings.Add("close target trail shows too many pips")
    }
    if ($farSample.Active -lt 4) {
        $warnings.Add("far target trail lacks readability")
    }

    if ($warnings.Count -gt 0) {
        $warningCount += $warnings.Count
    }

    $warningText = if ($warnings.Count -gt 0) { [string]::Join("; ", $warnings) } else { "OK" }
    $lines.Add(("{0}: stage={1:00} minDestructibles={2} stageTarget={3} routeHoldTarget={4} deadline={5:0.#}s afterLaneBreak={6:0.00}/s pips=[{7}] warnings={8}" -f `
        ($(if ($warnings.Count -gt 0) { "WARN" } else { "OK" })), `
        $stage, `
        $minimumDestructibles, `
        $stageTarget, `
        $routeTarget, `
        $deadlineSeconds, `
        $wrecksPerSecondAfterLaneBreak, `
        (Format-RoutePips -Samples $pipSamples), `
        $warningText))
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
