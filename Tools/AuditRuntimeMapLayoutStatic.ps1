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
    $spawnLaneEndZ = $gridStartZ + ($cellSize * 2.45)

    return [pscustomobject]@{
        Stage = $stageNumber
        GrowthTier = $growthTier
        Growth01 = $growth01
        MapSize = $mapSize
        HalfExtent = $mapSize * 0.5
        CellSize = $cellSize
        XCells = $cells
        ZCells = $cells
        GridStartX = $gridWidth * -0.5
        GridStartZ = $gridStartZ
        OpeningRows = Clamp-Int -Value (4 + [Math]::Floor($growthTier / 2.0)) -Min 4 -Max 7
        RoadRowStride = if ($stageNumber -ge 5) { 4 } else { 5 }
        RoadColumnStride = if ($stageNumber -ge 4) { 5 } else { 6 }
        SpawnLaneEndZ = $spawnLaneEndZ
        SpawnLaneHalfWidth = Lerp 4.8 6.4 $growth01
        LowDistrictEndZ = Lerp 1.5 4.2 $growth01
        MidDistrictEndZ = Lerp 9.5 16.5 $growth01
        TargetX = Lerp 13.5 22.0 $growth01
        TargetForwardZ = Lerp 11.5 21.0 $growth01
        TargetReturnZ = Lerp -11.5 -20.0 $growth01
    }
}

function Resolve-LandmarkCenter {
    param($Layout, [int]$Index)

    switch ($Index) {
        0 { return [pscustomobject]@{ X = -(Lerp 7.2 10.2 $Layout.Growth01); Z = Lerp -1.8 -5.8 $Layout.Growth01; Name = "PocketPark" } }
        1 { return [pscustomobject]@{ X = Lerp 5.8 9.8 $Layout.Growth01; Z = Lerp 2.5 7.8 $Layout.Growth01; Name = "MarketPlaza" } }
        2 { return [pscustomobject]@{ X = -(Lerp 8.8 13.4 $Layout.Growth01); Z = Lerp 9.5 15.6 $Layout.Growth01; Name = "ConstructionYard" } }
        3 { return [pscustomobject]@{ X = Lerp 8.4 14.2 $Layout.Growth01; Z = Lerp -8.4 -14.4 $Layout.Growth01; Name = "PowerBlock" } }
        default { return [pscustomobject]@{ X = Lerp 1.8 4.2 $Layout.Growth01; Z = Lerp 13.6 18.8 $Layout.Growth01; Name = "SkylineBlock" } }
    }
}

function Resolve-LandmarkHalfExtents {
    param([int]$Index)

    switch ($Index) {
        0 { return [pscustomobject]@{ X = 3.6; Z = 2.8 } }
        1 { return [pscustomobject]@{ X = 3.8; Z = 3.1 } }
        2 { return [pscustomobject]@{ X = 4.2; Z = 3.4 } }
        3 { return [pscustomobject]@{ X = 3.8; Z = 3.4 } }
        default { return [pscustomobject]@{ X = 4.8; Z = 3.8 } }
    }
}

function Resolve-MinimumLandmarkObjects {
    param($Layout)

    if ($Layout.Stage -ge 7) { return 70 }
    if ($Layout.Stage -ge 6) { return 50 }
    if ($Layout.Stage -ge 5) { return 36 }
    if ($Layout.Stage -ge 3) { return 22 }
    if ($Layout.Stage -ge 2) { return 10 }
    return 0
}

function Test-InsideMap {
    param($Layout, [double]$X, [double]$Z, [double]$Padding)

    $limit = [Math]::Max(1.0, $Layout.HalfExtent - [Math]::Max(0.0, $Padding))
    return ([Math]::Abs($X) -le $limit) -and ([Math]::Abs($Z) -le $limit)
}

function Test-InsideLandmark {
    param([double]$X, [double]$Z, $Center, $HalfExtents)

    return ([Math]::Abs($X - $Center.X) -le $HalfExtents.X) -and ([Math]::Abs($Z - $Center.Z) -le $HalfExtents.Z)
}

function Format-Point {
    param([double]$X, [double]$Z)
    return "({0:0.0},{1:0.0})" -f $X, $Z
}

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $projectRoot = Split-Path -Parent $PSScriptRoot
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherMapLayoutStaticAudit.log"
}

$lines = [System.Collections.Generic.List[string]]::new()
$warningCount = 0
$previousSize = 0.0
$previousCells = 0

$lines.Add("[AlienCrusher][MapLayoutStaticAudit] Runtime map formula audit")
$lines.Add("Range: Stage 01-$("{0:00}" -f $MaxStage)")

for ($stage = 1; $stage -le [Math]::Max(1, $MaxStage); $stage++) {
    $layout = Resolve-Layout -Stage $stage -GrowthMax $MaxGrowthStage
    $warnings = [System.Collections.Generic.List[string]]::new()
    $spawnX = 0.0
    $spawnZ = [Math]::Max(-$layout.HalfExtent + 4.0, $layout.GridStartZ - 0.8)
    $targetAX = -$layout.TargetX
    $targetAZ = $layout.TargetForwardZ
    $targetBX = $layout.TargetX
    $targetBZ = $layout.TargetReturnZ

    if ($layout.MapSize -lt $previousSize) {
        $warnings.Add("map size regressed")
    }
    if ($layout.XCells -lt $previousCells) {
        $warnings.Add("grid cells regressed")
    }
    if (-not (Test-InsideMap -Layout $layout -X $spawnX -Z $spawnZ -Padding 1.5)) {
        $warnings.Add("spawn outside map")
    }
    if ([Math]::Abs($spawnX) -gt ($layout.SpawnLaneHalfWidth + 1.8) -or $spawnZ -gt ($layout.SpawnLaneEndZ + 2.0)) {
        $warnings.Add("spawn off starter lane")
    }
    if (-not (Test-InsideMap -Layout $layout -X $targetAX -Z $targetAZ -Padding 2.2)) {
        $warnings.Add("Target_A outside map")
    }
    if (-not (Test-InsideMap -Layout $layout -X $targetBX -Z $targetBZ -Padding 2.2)) {
        $warnings.Add("Target_B outside map")
    }

    $distA = [Math]::Sqrt([Math]::Pow($targetAX - $spawnX, 2.0) + [Math]::Pow($targetAZ - $spawnZ, 2.0))
    $distB = [Math]::Sqrt([Math]::Pow($targetBX - $spawnX, 2.0) + [Math]::Pow($targetBZ - $spawnZ, 2.0))
    if ($distA -lt 9.0) { $warnings.Add("Target_A too close to spawn") }
    if ($distB -lt 9.0) { $warnings.Add("Target_B too close to spawn") }

    $activeLandmarks = [System.Collections.Generic.List[string]]::new()
    $landmarkIndexes = @()
    if ($stage -ge 2) { $landmarkIndexes += 0 }
    if ($stage -ge 3) { $landmarkIndexes += 1 }
    if ($stage -ge 5) { $landmarkIndexes += 2 }
    if ($stage -ge 6) { $landmarkIndexes += 3 }
    if ($stage -ge 7) { $landmarkIndexes += 4 }

    foreach ($index in $landmarkIndexes) {
        $center = Resolve-LandmarkCenter -Layout $layout -Index $index
        $half = Resolve-LandmarkHalfExtents -Index $index
        $activeLandmarks.Add(("{0}{1}" -f $center.Name, (Format-Point -X $center.X -Z $center.Z)))

        $limitX = $layout.HalfExtent - 1.15 - $half.X
        $limitZ = $layout.HalfExtent - 1.15 - $half.Z
        if ([Math]::Abs($center.X) -gt $limitX -or [Math]::Abs($center.Z) -gt $limitZ) {
            $warnings.Add("$($center.Name) outside map")
        }
        if (Test-InsideLandmark -X $spawnX -Z $spawnZ -Center $center -HalfExtents $half) {
            $warnings.Add("$($center.Name) overlaps spawn lane")
        }
        if ((Test-InsideLandmark -X $targetAX -Z $targetAZ -Center $center -HalfExtents $half) -or (Test-InsideLandmark -X $targetBX -Z $targetBZ -Center $center -HalfExtents $half)) {
            $warnings.Add("$($center.Name) overlaps route target")
        }
    }

    $minimumDestructibles = [Math]::Max(52, [Math]::Round($layout.XCells * $layout.ZCells * (Lerp 0.34 0.42 $layout.Growth01)))
    $minimumOpening = Clamp-Int -Value (14 + ($layout.GrowthTier * 2)) -Min 14 -Max 24
    $minimumLandmarks = Resolve-MinimumLandmarkObjects -Layout $layout
    $landmarkText = if ($activeLandmarks.Count -gt 0) { [string]::Join(", ", $activeLandmarks) } else { "none" }
    $warningText = if ($warnings.Count -gt 0) { [string]::Join("; ", $warnings) } else { "OK" }

    if ($warnings.Count -gt 0) {
        $warningCount += $warnings.Count
    }

    $lines.Add(("{0}: stage={1:00} size={2:0.0}m grid={3}x{4} minDestructibles={5} minOpening={6} minLandmarks={7} spawn={8} targetA={9} targetB={10} landmarks={11} warnings={12}" -f `
        ($(if ($warnings.Count -gt 0) { "WARN" } else { "OK" })), `
        $stage, `
        $layout.MapSize, `
        $layout.XCells, `
        $layout.ZCells, `
        $minimumDestructibles, `
        $minimumOpening, `
        $minimumLandmarks, `
        (Format-Point -X $spawnX -Z $spawnZ), `
        (Format-Point -X $targetAX -Z $targetAZ), `
        (Format-Point -X $targetBX -Z $targetBZ), `
        $landmarkText, `
        $warningText))

    $previousSize = $layout.MapSize
    $previousCells = $layout.XCells
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
