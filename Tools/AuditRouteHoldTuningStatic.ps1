[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
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

$stageAdvanceBaseTarget = 16
$stageAdvanceTargetPerStage = 3
$stageAdvanceTargetRatio = 0.48
$bossStageStart = 4
$earlyCrushFlowWindowSeconds = 18.0
$earlyCrushLaneBreakTarget = 9
$routeHoldWindowSeconds = 38.0
$routeHoldProgressThreshold = 0.45
$routeHoldTrailPipCount = 5
$routeHoldTrailMaxDistance = 18.0
$routeHoldTrailMinPipSpacing = 1.65
$routeHoldTrailCloseHideDistance = 2.4
$stageDurationSeconds = 90.0
$distanceSamples = @(2.0, 3.0, 6.0, 12.0, 18.0, 24.0)

$lines = [System.Collections.Generic.List[string]]::new()
$warningCount = 0

$lines.Add("[AlienCrusher][RouteHoldStaticAudit] ROUTE HOLD tuning audit")
$lines.Add("Range: Stage 01-$("{0:00}" -f $MaxStage)")

$deadlineSeconds = [Math]::Max($earlyCrushFlowWindowSeconds, $routeHoldWindowSeconds)
if ($deadlineSeconds -ge $stageDurationSeconds) {
    $warningCount++
    $lines.Add("WARN: route hold deadline is not inside the stage timer")
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
