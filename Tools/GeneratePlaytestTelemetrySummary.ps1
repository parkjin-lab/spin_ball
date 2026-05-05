[CmdletBinding()]
param(
    [string]$TelemetryLogPath = "",
    [string]$ReportPath = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
}

function Resolve-ProjectPath {
    param(
        [string]$ProjectRoot,
        [string]$OverridePath,
        [string]$RelativePath
    )

    if (-not [string]::IsNullOrWhiteSpace($OverridePath)) {
        if ([System.IO.Path]::IsPathRooted($OverridePath)) {
            return $OverridePath
        }

        return Join-Path $ProjectRoot $OverridePath
    }

    return Join-Path $ProjectRoot $RelativePath
}

function Parse-TelemetryLine {
    param([string]$Line)

    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $null
    }

    $pattern = '^\[AlienCrusher\]\[Playtest\] time=(?<time>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) stage=(?<stage>\d+) event=(?<event>[A-Z_]+)(?<detail>.*)$'
    $match = [regex]::Match($Line, $pattern)
    if (-not $match.Success) {
        return $null
    }

    return [pscustomobject]@{
        Time = [datetime]::ParseExact($match.Groups['time'].Value, 'yyyy-MM-dd HH:mm:ss', [System.Globalization.CultureInfo]::InvariantCulture)
        Stage = [int]$match.Groups['stage'].Value
        Event = $match.Groups['event'].Value
        Detail = $match.Groups['detail'].Value.Trim()
        RawLine = $Line
    }
}

function New-RunRecord {
    param(
        [int]$Stage,
        [datetime]$Time,
        [string]$SweepLabel
    )

    return [pscustomobject]@{
        Stage = $Stage
        StartTime = $Time
        EndTime = $null
        SweepLabel = $SweepLabel
        Entries = [System.Collections.Generic.List[object]]::new()
    }
}

function New-SweepRecord {
    param(
        [string]$Label,
        $StartEntry
    )

    return [pscustomobject]@{
        Label = $Label
        StartEntry = $StartEntry
        EndEntry = $null
        Entries = [System.Collections.Generic.List[object]]::new()
        Stages = [System.Collections.Generic.List[int]]::new()
    }
}

function Get-FirstEntryByEvent {
    param(
        [System.Collections.Generic.List[object]]$Entries,
        [string]$EventName
    )

    foreach ($entry in $Entries) {
        if ($entry.Event -eq $EventName) {
            return $entry
        }
    }

    return $null
}

function Format-EntryLine {
    param($Entry)

    if ($null -eq $Entry) {
        return "not observed"
    }

    if ([string]::IsNullOrWhiteSpace($Entry.Detail)) {
        return $Entry.Time.ToString("HH:mm:ss")
    }

    return "{0}  /  {1}" -f $Entry.Time.ToString("HH:mm:ss"), $Entry.Detail
}

function Get-StageEndResult {
    param([string]$Detail)

    if ([string]::IsNullOrWhiteSpace($Detail)) {
        return "UNKNOWN"
    }

    $match = [regex]::Match($Detail, 'result=(?<value>\S+)')
    if ($match.Success) {
        return $match.Groups['value'].Value
    }

    return "UNKNOWN"
}

function Get-StageEndBucket {
    param([string]$Detail)

    if ([string]::IsNullOrWhiteSpace($Detail)) {
        return "UNKNOWN"
    }

    $match = [regex]::Match($Detail, 'bucket=(?<value>.+?) destroyed=')
    if ($match.Success) {
        return $match.Groups['value'].Value.Trim()
    }

    return "UNKNOWN"
}

function Get-DetailField {
    param(
        [string]$Detail,
        [string]$FieldName
    )

    if ([string]::IsNullOrWhiteSpace($Detail) -or [string]::IsNullOrWhiteSpace($FieldName)) {
        return ""
    }

    $pattern = "(?:^|\s)$([regex]::Escape($FieldName))=(?<value>\S+)"
    $match = [regex]::Match($Detail, $pattern)
    if ($match.Success) {
        return $match.Groups['value'].Value.Trim()
    }

    return ""
}

function Get-RunIssues {
    param($Run)

    $issues = [System.Collections.Generic.List[string]]::new()
    $entryNames = @($Run.Entries | ForEach-Object { $_.Event })
    $hasStart = $entryNames -contains 'STAGE_START'
    $hasOpen = $entryNames -contains 'ROUTE_OPEN'
    $hasHold = $entryNames -contains 'ROUTE_HOLD_CLEAR'
    $hasBonus = $entryNames -contains 'ROUTE_BONUS'
    $hasSmash = $entryNames -contains 'FORWARD_SMASH'
    $hasEnd = $entryNames -contains 'STAGE_END'

    if (-not $hasStart) {
        $issues.Add("missing STAGE_START")
    }
    if ($hasOpen -and -not $hasHold) {
        $issues.Add("route opened but hold clear was not observed")
    }
    if ($hasHold -and -not $hasBonus) {
        $issues.Add("route hold cleared but route bonus was not observed")
    }
    if ($hasBonus -and -not $hasSmash) {
        $issues.Add("route bonus triggered but forward smash was not observed")
    }
    if (-not $hasEnd) {
        $issues.Add("run has no STAGE_END entry")
    }

    return $issues
}

function Test-RunHasEvent {
    param(
        $Run,
        [string]$EventName
    )

    foreach ($entry in $Run.Entries) {
        if ($entry.Event -eq $EventName) {
            return $true
        }
    }

    return $false
}

function Get-RunEndEntry {
    param($Run)

    return Get-FirstEntryByEvent -Entries $Run.Entries -EventName 'STAGE_END'
}

function Get-RunResult {
    param($Run)

    $endEntry = Get-RunEndEntry -Run $Run
    if ($null -eq $endEntry) {
        return "OPEN"
    }

    return Get-StageEndResult -Detail $endEntry.Detail
}

function Get-RunBucket {
    param($Run)

    $endEntry = Get-RunEndEntry -Run $Run
    if ($null -eq $endEntry) {
        return "UNKNOWN"
    }

    return Get-StageEndBucket -Detail $endEntry.Detail
}

function Get-BucketFocusHint {
    param([string]$Bucket)

    switch ($Bucket) {
        'OPENING FAILED' { return "Check opening lane clarity, dense low-rise placement, and early LANE BREAK pacing." }
        'ROUTE HOLD MISSED' { return "Check route beacon/trail readability, target distance, and ROUTE HOLD window pressure." }
        'MID-RUN DRIFT' { return "Check next-cluster readability, carry speed, and whether the route payoff pulls the player forward." }
        'FINAL PUSH FAILED' { return "Check goal-lane commitment, end-of-run target spacing, and whether side props distract from the finish." }
        'BOSS PHASE' { return "Check pylon/core readability, damage windows, and phase transition pressure." }
        'NONE' { return "Stable finish; compare pacing, map identity, and payoff feel against nearby stages." }
        default { return "Check the first missing route milestone and compare the stage note against the checklist." }
    }
}

function Get-RunFocusHint {
    param($Run)

    if (-not (Test-RunHasEvent -Run $Run -EventName 'ROUTE_OPEN')) {
        return "Check opening lane clarity, dense row placement, and early LANE BREAK target count."
    }

    if (-not (Test-RunHasEvent -Run $Run -EventName 'ROUTE_HOLD_CLEAR')) {
        return "Check route beacon/trail readability, target distance, and ROUTE HOLD pressure after ROUTE OPEN."
    }

    if (-not (Test-RunHasEvent -Run $Run -EventName 'ROUTE_BONUS')) {
        return "Check ROUTE BONUS trigger timing and whether the reward cluster is spawning clearly."
    }

    if (-not (Test-RunHasEvent -Run $Run -EventName 'FORWARD_SMASH')) {
        return "Check Forward Smash target highlight, reward cluster spacing, and payoff contrast."
    }

    return Get-BucketFocusHint -Bucket (Get-RunBucket -Run $Run)
}

function Get-TuningCandidateDefinition {
    param([string]$Key)

    switch ($Key) {
        'OPENING' {
            return [pscustomobject]@{
                Label = "Opening Clarity"
                TuneFirst = '`routeOpenBeatSeconds`, starter-lane density / minimum opening in `DummyFlowController.RuntimeMapFallback.cs`, early lane-break target pacing'
                Why = 'Runs are failing before ROUTE OPEN, so the first crush lane may be too sparse or the opening beat may not read fast enough.'
            }
        }
        'ROUTE_HOLD' {
            return [pscustomobject]@{
                Label = "Route Hold Readability"
                TuneFirst = '`routeHoldWindowSeconds`, `routeHoldProgressThreshold`, `routeHoldTrailPipCount`, `routeHoldTrailMaxDistance`, `routeHoldTrailMinPipSpacing`, `routeHoldTrailCloseHideDistance`, `Target_A/B` spacing'
                Why = 'ROUTE OPEN is happening, but the player is not converting it into a readable hold path often enough.'
            }
        }
        'ROUTE_BONUS' {
            return [pscustomobject]@{
                Label = "Route Bonus Payoff"
                TuneFirst = '`routeRewardClusterRadius`, `routeRewardClusterPropCount`, reward-cluster spawn readability near `Target_A/B`'
                Why = 'ROUTE HOLD is clearing, but the bonus cluster is not landing as a clear payoff beat.'
            }
        }
        'FORWARD_SMASH' {
            return [pscustomobject]@{
                Label = "Forward Smash Visibility"
                TuneFirst = 'Forward Smash target highlight/contrast, reward-cluster spacing, end-of-route `Target_A/B` staging'
                Why = 'The reward beat is appearing, but the final smash target is not closing the loop cleanly.'
            }
        }
        'MID_RUN' {
            return [pscustomobject]@{
                Label = "Mid-Run Carry"
                TuneFirst = '`routeHoldTrail*`, next-cluster visibility, `stageDurationSeconds`, carry-speed preservation'
                Why = 'The route opens, but momentum collapses before the next cluster or finish can be committed to.'
            }
        }
        'FINAL_PUSH' {
            return [pscustomobject]@{
                Label = "Final Push Pressure"
                TuneFirst = '`stageDurationSeconds`, stage target pacing, end-lane `Target_A/B` spacing, side-prop distraction around the finish'
                Why = 'The player is reaching late-run value, then losing the close because the finish lane reads too late or asks too much.'
            }
        }
        'BOSS' {
            return [pscustomobject]@{
                Label = "Boss Window Readability"
                TuneFirst = '`bossBreakWindowDuration`, `bossPressurePulseInterval`, `bossPressurePulseRadius`, `bossShieldRegenInterval`'
                Why = 'The boss phase is being reached, but the break window or pressure cadence is likely too hard to parse in motion.'
            }
        }
        default {
            return [pscustomobject]@{
                Label = "General Review"
                TuneFirst = 'Checklist notes, route telemetry order, and stage identity comparison'
                Why = 'Use the stage notes to decide whether the issue is readability, pacing, or payoff clarity.'
            }
        }
    }
}

function Get-RunTuningCandidateKeys {
    param($Run)

    $keys = [System.Collections.Generic.List[string]]::new()

    if (-not (Test-RunHasEvent -Run $Run -EventName 'ROUTE_OPEN')) {
        $keys.Add('OPENING')
    }
    elseif (-not (Test-RunHasEvent -Run $Run -EventName 'ROUTE_HOLD_CLEAR')) {
        $keys.Add('ROUTE_HOLD')
    }
    elseif (-not (Test-RunHasEvent -Run $Run -EventName 'ROUTE_BONUS')) {
        $keys.Add('ROUTE_BONUS')
    }
    elseif (-not (Test-RunHasEvent -Run $Run -EventName 'FORWARD_SMASH')) {
        $keys.Add('FORWARD_SMASH')
    }

    switch (Get-RunBucket -Run $Run) {
        'OPENING FAILED' { if (-not $keys.Contains('OPENING')) { $keys.Add('OPENING') } }
        'ROUTE HOLD MISSED' { if (-not $keys.Contains('ROUTE_HOLD')) { $keys.Add('ROUTE_HOLD') } }
        'MID-RUN DRIFT' { if (-not $keys.Contains('MID_RUN')) { $keys.Add('MID_RUN') } }
        'FINAL PUSH FAILED' { if (-not $keys.Contains('FINAL_PUSH')) { $keys.Add('FINAL_PUSH') } }
        'BOSS PHASE' { if (-not $keys.Contains('BOSS')) { $keys.Add('BOSS') } }
    }

    return $keys
}

$projectRoot = Resolve-ProjectRoot
$resolvedTelemetryLogPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $TelemetryLogPath -RelativePath "Logs\AlienCrusherPlaytestTelemetry.log"
$resolvedReportPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ReportPath -RelativePath "Logs\AlienCrusherPlaytestTelemetrySummary.md"

$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Playtest Telemetry Summary")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm K")")
$lines.Add("Source log: $resolvedTelemetryLogPath")
$lines.Add("")

if (-not (Test-Path -Path $resolvedTelemetryLogPath -PathType Leaf)) {
    $lines.Add("No playtest telemetry log exists yet.")
    $lines.Add("")
    $lines.Add("Next step:")
    $lines.Add("- Run an editor/development playtest or `F10` sweep.")
    $lines.Add("- Re-run `Tools/GeneratePlaytestTelemetrySummary.ps1` after the run.")

    $report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
    Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
    Write-Output $report
    exit 0
}

$entries = [System.Collections.Generic.List[object]]::new()
Get-Content -Path $resolvedTelemetryLogPath | ForEach-Object {
    $entry = Parse-TelemetryLine -Line $_
    if ($null -ne $entry) {
        $entries.Add($entry)
    }
}

if ($entries.Count -eq 0) {
    $lines.Add("The playtest telemetry log exists, but no parseable `[AlienCrusher][Playtest]` lines were found.")
    $report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
    Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
    Write-Output $report
    exit 0
}

$runs = [System.Collections.Generic.List[object]]::new()
$sweeps = [System.Collections.Generic.List[object]]::new()
$activeRuns = @{}
$currentSweep = $null
$sweepIndex = 0
foreach ($entry in $entries) {
    if ($entry.Event -eq 'SWEEP_START') {
        $sweepIndex++
        $currentSweep = New-SweepRecord -Label ("Sweep {0:00}" -f $sweepIndex) -StartEntry $entry
        $sweeps.Add($currentSweep)
        continue
    }

    if ($entry.Event -eq 'SWEEP_END') {
        if ($null -ne $currentSweep) {
            $currentSweep.EndEntry = $entry
            $currentSweep.Entries.Add($entry)
            $currentSweep = $null
        }
        continue
    }

    if ($null -ne $currentSweep) {
        $currentSweep.Entries.Add($entry)
        if ($entry.Event -eq 'STAGE_START' -and -not $currentSweep.Stages.Contains($entry.Stage)) {
            $currentSweep.Stages.Add($entry.Stage)
        }
    }

    $sweepLabel = if ($null -ne $currentSweep) { $currentSweep.Label } else { "Ad hoc" }

    $stageKey = "{0:00}" -f $entry.Stage
    if ($entry.Event -eq 'STAGE_START') {
        $run = New-RunRecord -Stage $entry.Stage -Time $entry.Time -SweepLabel $sweepLabel
        $runs.Add($run)
        $activeRuns[$stageKey] = $run
    }
    elseif (-not $activeRuns.ContainsKey($stageKey)) {
        $run = New-RunRecord -Stage $entry.Stage -Time $entry.Time -SweepLabel $sweepLabel
        $runs.Add($run)
        $activeRuns[$stageKey] = $run
    }

    $currentRun = $activeRuns[$stageKey]
    $currentRun.Entries.Add($entry)
    if ($entry.Event -eq 'STAGE_END') {
        $currentRun.EndTime = $entry.Time
        $activeRuns.Remove($stageKey)
    }
}

$lines.Add("## Totals")
$lines.Add("")
$lines.Add("- Entries parsed: $($entries.Count)")
$lines.Add("- Runs parsed: $($runs.Count)")
$lines.Add("- Stage starts: $((@($entries | Where-Object { $_.Event -eq 'STAGE_START' })).Count)")
$lines.Add("- Route opens: $((@($entries | Where-Object { $_.Event -eq 'ROUTE_OPEN' })).Count)")
$lines.Add("- Route hold clears: $((@($entries | Where-Object { $_.Event -eq 'ROUTE_HOLD_CLEAR' })).Count)")
$lines.Add("- Route bonuses: $((@($entries | Where-Object { $_.Event -eq 'ROUTE_BONUS' })).Count)")
$lines.Add("- Forward smashes: $((@($entries | Where-Object { $_.Event -eq 'FORWARD_SMASH' })).Count)")
$lines.Add("- Stage ends: $((@($entries | Where-Object { $_.Event -eq 'STAGE_END' })).Count)")
$lines.Add("")

$lines.Add("## Sweep Summary")
$lines.Add("")
if ($sweeps.Count -eq 0) {
    $lines.Add("No `SWEEP_START` / `SWEEP_END` telemetry was found yet.")
    $lines.Add("")
}
else {
    foreach ($sweep in $sweeps) {
        $completed = if ($null -ne $sweep.EndEntry) { Get-DetailField -Detail $sweep.EndEntry.Detail -FieldName 'completed' } else { "no" }
        $stagesObserved = if ($sweep.Stages.Count -eq 0) { "none" } else { [string]::Join(", ", @($sweep.Stages | Sort-Object | ForEach-Object { "{0:00}" -f $_ })) }
        $stageStartCount = (@($sweep.Entries | Where-Object { $_.Event -eq 'STAGE_START' })).Count
        $stageEndCount = (@($sweep.Entries | Where-Object { $_.Event -eq 'STAGE_END' })).Count
        $routeHoldCount = (@($sweep.Entries | Where-Object { $_.Event -eq 'ROUTE_HOLD_CLEAR' })).Count
        $smashCount = (@($sweep.Entries | Where-Object { $_.Event -eq 'FORWARD_SMASH' })).Count

        $lines.Add("### $($sweep.Label)")
        $lines.Add("")
        $lines.Add("- Start: $(Format-EntryLine -Entry $sweep.StartEntry)")
        $lines.Add("- End: $(Format-EntryLine -Entry $sweep.EndEntry)")
        $lines.Add("- Completed: $completed")
        $lines.Add("- Stages observed: $stagesObserved")
        $lines.Add("- Stage starts / ends: $stageStartCount / $stageEndCount")
        $lines.Add("- Route hold clears / forward smashes: $routeHoldCount / $smashCount")
        $lines.Add("")
    }
}

$lines.Add("## Stage Trends")
$lines.Add("")

$stageGroups = @($runs | Group-Object Stage | Sort-Object { [int]$_.Name })
if ($stageGroups.Count -eq 0) {
    $lines.Add("No stage runs were parsed yet.")
    $lines.Add("")
}
else {
    foreach ($stageGroup in $stageGroups) {
        $stageRuns = @($stageGroup.Group)
        $runCount = $stageRuns.Count
        $victoryCount = 0
        $defeatCount = 0
        $routeOpenCount = 0
        $routeHoldCount = 0
        $routeBonusCount = 0
        $forwardSmashCount = 0
        $bucketCounts = @{}

        foreach ($stageRun in $stageRuns) {
            $result = Get-RunResult -Run $stageRun
            $bucket = Get-RunBucket -Run $stageRun

            if ($result -eq 'VICTORY') {
                $victoryCount++
            }
            else {
                $defeatCount++
            }

            if (Test-RunHasEvent -Run $stageRun -EventName 'ROUTE_OPEN') {
                $routeOpenCount++
            }
            if (Test-RunHasEvent -Run $stageRun -EventName 'ROUTE_HOLD_CLEAR') {
                $routeHoldCount++
            }
            if (Test-RunHasEvent -Run $stageRun -EventName 'ROUTE_BONUS') {
                $routeBonusCount++
            }
            if (Test-RunHasEvent -Run $stageRun -EventName 'FORWARD_SMASH') {
                $forwardSmashCount++
            }

            if (-not $bucketCounts.ContainsKey($bucket)) {
                $bucketCounts[$bucket] = 0
            }
            $bucketCounts[$bucket]++
        }

        $bucketSummary = if ($bucketCounts.Count -eq 0) {
            "none"
        }
        else {
            [string]::Join(", ", @($bucketCounts.GetEnumerator() | Sort-Object -Property @{ Expression = 'Value'; Descending = $true }, @{ Expression = 'Key'; Descending = $false } | ForEach-Object { "{0} x{1}" -f $_.Key, $_.Value }))
        }

        $dominantDefeatBucket = "NONE"
        $defeatBucketEntries = @($bucketCounts.GetEnumerator() | Where-Object { $_.Key -ne 'NONE' -and $_.Key -ne 'UNKNOWN' } | Sort-Object -Property @{ Expression = 'Value'; Descending = $true }, @{ Expression = 'Key'; Descending = $false })
        if ($defeatBucketEntries.Count -gt 0) {
            $dominantDefeatBucket = $defeatBucketEntries[0].Key
        }

        $stageFocus = if ($routeOpenCount -eq 0) {
            "Check opening lane clarity, dense low-rise rows, and early route target count."
        }
        elseif ($routeHoldCount -lt $routeOpenCount) {
            "Check route beacon/trail readability, target distance, and ROUTE HOLD window pressure."
        }
        elseif ($routeBonusCount -lt $routeHoldCount) {
            "Check ROUTE BONUS trigger timing and whether the reward cluster spawns cleanly."
        }
        elseif ($forwardSmashCount -lt $routeBonusCount) {
            "Check Forward Smash target highlight and payoff cluster visibility."
        }
        elseif ($defeatCount -gt 0 -and $dominantDefeatBucket -ne 'NONE') {
            Get-BucketFocusHint -Bucket $dominantDefeatBucket
        }
        else {
            "Stable route loop; compare pacing, map identity, and payoff feel against nearby stages."
        }

        $lines.Add("### Stage $("{0:00}" -f [int]$stageGroup.Name)")
        $lines.Add("")
        $lines.Add("- Runs: $runCount (victory $victoryCount / defeat $defeatCount)")
        $lines.Add("- Route loop coverage: open $routeOpenCount/$runCount, hold $routeHoldCount/$runCount, bonus $routeBonusCount/$runCount, smash $forwardSmashCount/$runCount")
        $lines.Add("- Buckets: $bucketSummary")
        $lines.Add("- Suggested focus: $stageFocus")
        $lines.Add("")
    }
}

$lines.Add("## Tuning Candidates")
$lines.Add("")

$tuningCandidates = @{}
foreach ($run in $runs) {
    foreach ($candidateKey in @(Get-RunTuningCandidateKeys -Run $run)) {
        if (-not $tuningCandidates.ContainsKey($candidateKey)) {
            $tuningCandidates[$candidateKey] = [System.Collections.Generic.List[int]]::new()
        }
        $tuningCandidates[$candidateKey].Add([int]$run.Stage)
    }
}

if ($tuningCandidates.Count -eq 0) {
    $lines.Add("No tuning candidates were inferred yet.")
    $lines.Add("")
}
else {
    foreach ($candidateEntry in @($tuningCandidates.GetEnumerator() | Sort-Object -Property @{ Expression = { $_.Value.Count }; Descending = $true }, @{ Expression = 'Key'; Descending = $false })) {
        $definition = Get-TuningCandidateDefinition -Key $candidateEntry.Key
        $uniqueStages = @($candidateEntry.Value | Sort-Object -Unique)
        $stageList = [string]::Join(", ", @($uniqueStages | ForEach-Object { "{0:00}" -f $_ }))

        $lines.Add("### $($definition.Label)")
        $lines.Add("")
        $lines.Add("- Observed in: $($candidateEntry.Value.Count) run(s), stages $stageList")
        $lines.Add("- Tune first: $($definition.TuneFirst)")
        $lines.Add("- Why: $($definition.Why)")
        $lines.Add("")
    }
}

$lines.Add("## Failure Buckets")
$lines.Add("")

$defeatRuns = @($runs | Where-Object { (Get-RunResult -Run $_) -ne 'VICTORY' })
if ($defeatRuns.Count -eq 0) {
    $lines.Add("No failing stage-end buckets were observed yet.")
    $lines.Add("")
}
else {
    $bucketStages = @{}
    foreach ($defeatRun in $defeatRuns) {
        $bucket = Get-RunBucket -Run $defeatRun
        if (-not $bucketStages.ContainsKey($bucket)) {
            $bucketStages[$bucket] = [System.Collections.Generic.List[int]]::new()
        }
        $bucketStages[$bucket].Add([int]$defeatRun.Stage)
    }

    foreach ($bucketEntry in @($bucketStages.GetEnumerator() | Sort-Object -Property @{ Expression = { $_.Value.Count }; Descending = $true }, @{ Expression = 'Key'; Descending = $false })) {
        $stageList = [string]::Join(", ", @($bucketEntry.Value | Sort-Object | ForEach-Object { "{0:00}" -f $_ }))
        $lines.Add("- $($bucketEntry.Key): $($bucketEntry.Value.Count) run(s), stages $stageList. Action: $(Get-BucketFocusHint -Bucket $bucketEntry.Key)")
    }
    $lines.Add("")
}

$lines.Add("## Run Summary")
$lines.Add("")

$runIndex = 0
foreach ($run in $runs) {
    $runIndex++
    $sequence = [string]::Join(" -> ", @($run.Entries | ForEach-Object { $_.Event }))
    $startEntry = Get-FirstEntryByEvent -Entries $run.Entries -EventName 'STAGE_START'
    $openEntry = Get-FirstEntryByEvent -Entries $run.Entries -EventName 'ROUTE_OPEN'
    $holdEntry = Get-FirstEntryByEvent -Entries $run.Entries -EventName 'ROUTE_HOLD_CLEAR'
    $bonusEntry = Get-FirstEntryByEvent -Entries $run.Entries -EventName 'ROUTE_BONUS'
    $smashEntry = Get-FirstEntryByEvent -Entries $run.Entries -EventName 'FORWARD_SMASH'
    $endEntry = Get-FirstEntryByEvent -Entries $run.Entries -EventName 'STAGE_END'
    $issues = @(Get-RunIssues -Run $run)
    $result = if ($null -ne $endEntry) { Get-StageEndResult -Detail $endEntry.Detail } else { "OPEN" }
    $bucket = if ($null -ne $endEntry) { Get-StageEndBucket -Detail $endEntry.Detail } else { "UNKNOWN" }

    $lines.Add("### Run $("{0:00}" -f $runIndex) - Stage $("{0:00}" -f $run.Stage)")
    $lines.Add("")
    $lines.Add("- Sweep: $($run.SweepLabel)")
    $lines.Add("- Start: $(Format-EntryLine -Entry $startEntry)")
    $lines.Add("- Sequence: $sequence")
    $lines.Add("- Route open: $(Format-EntryLine -Entry $openEntry)")
    $lines.Add("- Route hold clear: $(Format-EntryLine -Entry $holdEntry)")
    $lines.Add("- Route bonus: $(Format-EntryLine -Entry $bonusEntry)")
    $lines.Add("- Forward smash: $(Format-EntryLine -Entry $smashEntry)")
    $lines.Add("- End: $(Format-EntryLine -Entry $endEntry)")
    $lines.Add("- Verdict: result=$result / bucket=$bucket")
    $lines.Add("- Suggested focus: $(Get-RunFocusHint -Run $run)")
    if ($issues.Count -eq 0) {
        $lines.Add("- Issues: none")
    }
    else {
        $lines.Add("- Issues: $([string]::Join('; ', $issues))")
    }
    $lines.Add("")
}

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
Write-Output $report
exit 0
