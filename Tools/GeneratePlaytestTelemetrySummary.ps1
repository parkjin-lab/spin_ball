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
        [datetime]$Time
    )

    return [pscustomobject]@{
        Stage = $Stage
        StartTime = $Time
        EndTime = $null
        Entries = [System.Collections.Generic.List[object]]::new()
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
$activeRuns = @{}
foreach ($entry in $entries) {
    $stageKey = "{0:00}" -f $entry.Stage
    if ($entry.Event -eq 'STAGE_START') {
        $run = New-RunRecord -Stage $entry.Stage -Time $entry.Time
        $runs.Add($run)
        $activeRuns[$stageKey] = $run
    }
    elseif (-not $activeRuns.ContainsKey($stageKey)) {
        $run = New-RunRecord -Stage $entry.Stage -Time $entry.Time
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
    $issues = Get-RunIssues -Run $run
    $result = if ($null -ne $endEntry) { Get-StageEndResult -Detail $endEntry.Detail } else { "OPEN" }
    $bucket = if ($null -ne $endEntry) { Get-StageEndBucket -Detail $endEntry.Detail } else { "UNKNOWN" }

    $lines.Add("### Run $("{0:00}" -f $runIndex) - Stage $("{0:00}" -f $run.Stage)")
    $lines.Add("")
    $lines.Add("- Start: $(Format-EntryLine -Entry $startEntry)")
    $lines.Add("- Sequence: $sequence")
    $lines.Add("- Route open: $(Format-EntryLine -Entry $openEntry)")
    $lines.Add("- Route hold clear: $(Format-EntryLine -Entry $holdEntry)")
    $lines.Add("- Route bonus: $(Format-EntryLine -Entry $bonusEntry)")
    $lines.Add("- Forward smash: $(Format-EntryLine -Entry $smashEntry)")
    $lines.Add("- End: $(Format-EntryLine -Entry $endEntry)")
    $lines.Add("- Verdict: result=$result / bucket=$bucket")
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
