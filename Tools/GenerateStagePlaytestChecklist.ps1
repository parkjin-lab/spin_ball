[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [string]$RuntimeConfigPath = "",
    [string]$GameFlowConfigPath = ""
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
        return ""
    }

    return Get-Content -Path $Path -Raw
}

function Read-CSharpNumberDefault {
    param(
        [string]$SourceText,
        [string]$FieldName,
        [double]$Fallback,
        [string]$Kind
    )

    $escapedFieldName = [regex]::Escape($FieldName)
    $pattern = "(?m)^\s*(?:\[[^\]]+\]\s*)*(?:private|protected|public)\s+(?:int|float|double)\s+$escapedFieldName\s*=\s*([-+]?(?:\d+(?:\.\d+)?|\.\d+))(?:[fFdD])?\s*;"
    $match = [regex]::Match($SourceText, $pattern)
    if (-not $match.Success) {
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

function Resolve-Layout {
    param([int]$Stage, [int]$GrowthMax)

    $stageNumber = [Math]::Max(1, $Stage)
    $growthMaxStage = [Math]::Max(1, $GrowthMax)
    $growthTier = Clamp-Int -Value ($stageNumber - 1) -Min 0 -Max ($growthMaxStage - 1)
    $growth01 = if ($growthMaxStage -le 1) { 0.0 } else { Clamp-Double -Value ($growthTier / [double]($growthMaxStage - 1)) -Min 0.0 -Max 1.0 }
    $mapSize = Lerp 44.0 62.0 $growth01
    $cellSize = Lerp 2.8 3.05 $growth01
    $cells = Clamp-Int -Value (13 + $growthTier) -Min 13 -Max 19
    $gridWidth = ($cells - 1) * $cellSize
    $gridStartZ = ($gridWidth * -0.5) - (Lerp 1.2 0.25 $growth01)

    return [pscustomobject]@{
        Stage = $stageNumber
        GrowthTier = $growthTier
        Growth01 = $growth01
        MapSize = $mapSize
        CellSize = $cellSize
        XCells = $cells
        ZCells = $cells
        GridStartZ = $gridStartZ
        TargetX = Lerp 13.5 22.0 $growth01
        TargetForwardZ = Lerp 11.5 21.0 $growth01
        TargetReturnZ = Lerp -11.5 -20.0 $growth01
    }
}

function Resolve-Landmarks {
    param($Layout)

    $landmarks = [System.Collections.Generic.List[string]]::new()
    if ($Layout.Stage -ge 2) { $landmarks.Add("Pocket park") }
    if ($Layout.Stage -ge 3) { $landmarks.Add("Market plaza") }
    if ($Layout.Stage -ge 5) { $landmarks.Add("Construction yard") }
    if ($Layout.Stage -ge 6) { $landmarks.Add("Power block") }
    if ($Layout.Stage -ge 7) { $landmarks.Add("Skyline block") }

    if ($landmarks.Count -eq 0) {
        return "none"
    }

    return [string]::Join(", ", $landmarks)
}

function Resolve-StageFocus {
    param([int]$Stage)

    switch ($Stage) {
        1 { return "Starter lane clarity and first LANE BREAK timing" }
        2 { return "Pocket park route openness and target readability" }
        3 { return "Market density, chain continuity, and route pressure" }
        4 { return "Boss approach readability and mid-map target spacing" }
        5 { return "Construction payoff objects and wider route commitment" }
        6 { return "Power block transformer payoff and long route guidance" }
        default { return "Skyline long route, high-value cluster payoff, and late pressure" }
    }
}

function Resolve-RoutePayoff {
    param([int]$Stage)

    switch ($Stage) {
        1 { return "Starter cluster" }
        2 { return "Park cut: bench/tree/barrel" }
        { $_ -ge 3 -and $_ -le 4 } { return "Market chain: kiosk/vending/barrel" }
        5 { return "Yard blast: barrel-heavy setup" }
        6 { return "Power surge: transformer-heavy setup" }
        default { return "Skyline breach: high-value anchor" }
    }
}

function Resolve-MinimumDestructibles {
    param($Layout)

    $ratio = Lerp 0.34 0.42 $Layout.Growth01
    return [Math]::Max(52, [Math]::Round($Layout.XCells * $Layout.ZCells * $ratio))
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

function Format-Point {
    param([double]$X, [double]$Z)
    return "({0:0.0}, {1:0.0})" -f $X, $Z
}

function Format-ResultLine {
    param([string]$Path)

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        return "missing"
    }

    $result = Select-String -Path $Path -Pattern "Result:" | Select-Object -Last 1
    if ($null -eq $result) {
        return "no Result line"
    }

    return $result.Line
}

$projectRoot = Split-Path -Parent $PSScriptRoot
$runtimeConfigPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $RuntimeConfigPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.cs"
$gameFlowConfigPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $GameFlowConfigPath -RelativePath "Assets\Scripts\Runtime\Systems\GameFlowSystem.cs"
$runtimeConfigText = Read-SourceText -Path $runtimeConfigPath
$gameFlowConfigText = Read-SourceText -Path $gameFlowConfigPath

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherStagePlaytestChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$stageAdvanceBaseTarget = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "stageAdvanceBaseTarget" -Fallback 16 -Kind "int"
$stageAdvanceTargetPerStage = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "stageAdvanceTargetPerStage" -Fallback 3 -Kind "int"
$stageAdvanceTargetRatio = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "stageAdvanceTargetRatio" -Fallback 0.48 -Kind "double"
$bossStageStart = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "bossStageStart" -Fallback 4 -Kind "int"
$earlyCrushFlowWindowSeconds = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "earlyCrushFlowWindowSeconds" -Fallback 18.0 -Kind "double"
$earlyCrushLaneBreakTarget = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "earlyCrushLaneBreakTarget" -Fallback 9 -Kind "int"
$routeHoldWindowSeconds = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldWindowSeconds" -Fallback 38.0 -Kind "double"
$routeHoldProgressThreshold = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeHoldProgressThreshold" -Fallback 0.45 -Kind "double"
$routeOpenBeatSeconds = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeOpenBeatSeconds" -Fallback 2.0 -Kind "double"
$routeRewardClusterRadius = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeRewardClusterRadius" -Fallback 3.4 -Kind "double"
$routeRewardClusterPropCount = Read-CSharpNumberDefault -SourceText $runtimeConfigText -FieldName "routeRewardClusterPropCount" -Fallback 4 -Kind "int"
$stageDurationSeconds = Read-CSharpNumberDefault -SourceText $gameFlowConfigText -FieldName "stageDurationSeconds" -Fallback 90.0 -Kind "double"
$deadlineSeconds = [Math]::Max($earlyCrushFlowWindowSeconds, $routeHoldWindowSeconds)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Stage 1-7 Playtest Checklist")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm K")")
$lines.Add("")
$lines.Add("Use this during an in-editor or mobile-style viewport pass. The goal is to capture feel issues that static audits cannot see.")
$lines.Add("")
$lines.Add("## Validation Gate")
$lines.Add("")
$lines.Add("- Scene validation: $(Format-ResultLine -Path (Join-Path $projectRoot "Logs\AlienCrusherSceneValidation.log"))")
$lines.Add("- Runtime map layout audit: $(Format-ResultLine -Path (Join-Path $projectRoot "Logs\AlienCrusherMapLayoutAudit.log"))")
$lines.Add("- Scene essentials static audit: $(Format-ResultLine -Path (Join-Path $projectRoot "Logs\AlienCrusherSceneEssentialsStaticAudit.log"))")
$lines.Add("- Map static audit: $(Format-ResultLine -Path (Join-Path $projectRoot "Logs\AlienCrusherMapLayoutStaticAudit.log"))")
$lines.Add("- ROUTE HOLD static audit: $(Format-ResultLine -Path (Join-Path $projectRoot "Logs\AlienCrusherRouteHoldStaticAudit.log"))")
$lines.Add("")
$lines.Add("## Controls")
$lines.Add("")
$lines.Add('- `F6`: previous debug stage')
$lines.Add('- `F7`: next debug stage')
$lines.Add('- `F8`: reset to Stage 1')
$lines.Add('- `F9`: toggle map layout overlay')
$lines.Add('- `F10`: sweep Stage 1 through the debug max stage and emit `SWEEP_START` / `SWEEP_END` markers')
$lines.Add('- Console filter: `[AlienCrusher][Playtest]` for route milestone ordering and end-state summaries')
$lines.Add('- Playtest log file: `Logs/AlienCrusherPlaytestTelemetry.log`')
$lines.Add('- Playtest summary command: `powershell -ExecutionPolicy Bypass -File Tools/GeneratePlaytestTelemetrySummary.ps1` (includes current tuning snapshot, sweep, stage trend, tuning candidate, first-pass experiment, and failure bucket summaries when `F10` is used)')
$lines.Add("")
$lines.Add("## Failure Advice Checks")
$lines.Add("")
$lines.Add("- [ ] `OPENING FAILED` result/lobby advice starts with hitting dense low-rise rows.")
$lines.Add("- [ ] `ROUTE HOLD MISSED` result/lobby advice starts with staying on the beacon after LANE BREAK.")
$lines.Add("- [ ] `MID-RUN DRIFT` result/lobby advice starts with choosing the next cluster before speed drops.")
$lines.Add("- [ ] `FINAL PUSH FAILED` result/lobby advice starts with forcing the goal lane instead of side props.")
$lines.Add("- [ ] `BOSS PHASE` result/lobby advice starts with breaking pylons, then bursting the exposed core.")
$lines.Add("")
$lines.Add("## Stage Summary")
$lines.Add("")
$lines.Add("| Stage | Focus | Map/Grid | Landmarks | Route Target | Route Pressure | Target A/B |")
$lines.Add("| --- | --- | --- | --- | --- | --- | --- |")

$stageRows = [System.Collections.Generic.List[object]]::new()
for ($stage = 1; $stage -le [Math]::Max(1, $MaxStage); $stage++) {
    $layout = Resolve-Layout -Stage $stage -GrowthMax $MaxGrowthStage
    $minimumDestructibles = Resolve-MinimumDestructibles -Layout $layout
    $stageTarget = Resolve-StageAdvanceTarget `
        -Stage $stage `
        -DestructibleCount $minimumDestructibles `
        -BaseTarget $stageAdvanceBaseTarget `
        -PerStage $stageAdvanceTargetPerStage `
        -TargetRatio $stageAdvanceTargetRatio `
        -BossStageStart $bossStageStart
    $routeTarget = Resolve-RouteHoldTarget -StageAdvanceTarget $stageTarget -LaneBreakTarget $earlyCrushLaneBreakTarget -ProgressThreshold $routeHoldProgressThreshold
    $extraWrecks = [Math]::Max(0, $routeTarget - $earlyCrushLaneBreakTarget)
    $remainingAfterLaneBreak = [Math]::Max(0.1, $deadlineSeconds - $earlyCrushFlowWindowSeconds)
    $pressure = $extraWrecks / $remainingAfterLaneBreak
    $spawnX = 0.0
    $spawnZ = [Math]::Max(-($layout.MapSize * 0.5) + 4.0, $layout.GridStartZ - 0.8)
    $targetAX = -$layout.TargetX
    $targetAZ = $layout.TargetForwardZ
    $targetBX = $layout.TargetX
    $targetBZ = $layout.TargetReturnZ
    $distA = [Math]::Sqrt([Math]::Pow($targetAX - $spawnX, 2.0) + [Math]::Pow($targetAZ - $spawnZ, 2.0))
    $distB = [Math]::Sqrt([Math]::Pow($targetBX - $spawnX, 2.0) + [Math]::Pow($targetBZ - $spawnZ, 2.0))
    $row = [pscustomobject]@{
        Stage = $stage
        Focus = Resolve-StageFocus -Stage $stage
        Layout = $layout
        MinimumDestructibles = $minimumDestructibles
        StageTarget = $stageTarget
        RouteTarget = $routeTarget
        Pressure = $pressure
        TargetA = Format-Point -X $targetAX -Z $targetAZ
        TargetB = Format-Point -X $targetBX -Z $targetBZ
        DistA = $distA
        DistB = $distB
        Landmarks = Resolve-Landmarks -Layout $layout
        RoutePayoff = Resolve-RoutePayoff -Stage $stage
    }
    $stageRows.Add($row)

    $lines.Add(("| {0:00} | {1} | {2:0.0}m / {3}x{4} | {5} | {6}/{7} by {8:0.#}s | {9:0.00}/s after lane break | {10} {11:0.#}m / {12} {13:0.#}m |" -f `
        $stage, `
        $row.Focus, `
        $layout.MapSize, `
        $layout.XCells, `
        $layout.ZCells, `
        $row.Landmarks, `
        $routeTarget, `
        $stageTarget, `
        $deadlineSeconds, `
        $pressure, `
        $row.TargetA, `
        $distA, `
        $row.TargetB, `
        $distB))
}

$lines.Add("")
$lines.Add("## Observation Checklist")
$lines.Add("")
foreach ($row in $stageRows) {
    $lines.Add("### Stage $("{0:00}" -f $row.Stage) - $($row.Focus)")
    $lines.Add("")
    $lines.Add("Expected setup:")
    $lines.Add("- Map/grid: $("{0:0.0}" -f $row.Layout.MapSize)m, $($row.Layout.XCells)x$($row.Layout.ZCells)")
    $lines.Add("- Landmarks: $($row.Landmarks)")
    $lines.Add("- Route payoff: $($row.RoutePayoff)")
    $lines.Add("- Route target: $($row.RouteTarget)/$($row.StageTarget) by $("{0:0.#}" -f $deadlineSeconds)s")
    $lines.Add("- Target A/B: $($row.TargetA) / $($row.TargetB)")
    $lines.Add("")
    $lines.Add("Check:")
    $lines.Add("- [ ] Starter lane is readable before the first crush.")
    $lines.Add("- [ ] LANE BREAK -> ROUTE OPEN beat is visible for about $("{0:0.#}" -f $routeOpenBeatSeconds)s.")
    $lines.Add("- [ ] Route beacon and trail pips point toward the next target without clutter.")
    $lines.Add("- [ ] ROUTE HOLD meter moves from low progress to near-complete without reading the objective paragraph.")
    $lines.Add("- [ ] ROUTE HOLD pressure feels fair for the target distance.")
    $lines.Add("- [ ] Route reward opens about $routeRewardClusterPropCount payoff props around the next smash cluster.")
    $lines.Add("- [ ] Route payoff identity matches the expected district.")
    $lines.Add("- [ ] Camera clamp and map bounds feel natural.")
    $lines.Add("- [ ] Notes:")
    $lines.Add("- [ ] Screenshot/video reference:")
    $lines.Add("")
}

$lines.Add("## Tuning Notes")
$lines.Add("")
$lines.Add('- If close-range pips feel noisy, raise `routeHoldTrailCloseHideDistance` or `routeHoldTrailMinPipSpacing`.')
$lines.Add("- If far targets are hard to read, lower route distance growth or increase route beacon/trail contrast.")
$lines.Add('- If LANE BREAK still feels ambiguous, tune `routeOpenBeatSeconds` before adding more HUD text.')
$lines.Add(('- If the payoff cluster feels cramped or invisible, tune `routeRewardClusterRadius` around the current {0:0.#}m default.' -f $routeRewardClusterRadius))
$lines.Add("- If ROUTE HOLD feels like distance tax, increase reward cluster density near targets before extending the timer.")
$lines.Add("- If Stage 5-7 feel samey, give construction, power, and skyline landmarks different route payoffs.")
$lines.Add("- Stage timer default: $("{0:0.#}" -f $stageDurationSeconds)s")

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output $report

exit 0
