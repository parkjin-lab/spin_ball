[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [string]$FixturePath = "",
    [switch]$FailOnWarnings
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

function Add-Check {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$ReportText,
        [string]$Needle,
        [string]$Label
    )

    if ($ReportText.Contains($Needle)) {
        return
    }

    $Errors.Add("$Label missing expected marker: $Needle")
}

$projectRoot = Resolve-ProjectRoot
$resolvedFixturePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FixturePath -RelativePath "Tools\TestData\PlaytestTelemetrySummary\basic-sweep.txt"
$summaryScriptPath = Join-Path $PSScriptRoot "GeneratePlaytestTelemetrySummary.ps1"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetrySummaryRegression.log"
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

if (-not (Test-Path -Path $resolvedFixturePath -PathType Leaf)) {
    $errors.Add("Fixture log not found: $resolvedFixturePath")
}

if (-not (Test-Path -Path $summaryScriptPath -PathType Leaf)) {
    $errors.Add("Summary script not found: $summaryScriptPath")
}

$tempSummaryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestTelemetrySummaryRegression-{0}.md" -f ([guid]::NewGuid().ToString("N")))
$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

if ($errors.Count -eq 0) {
    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $summaryScriptPath `
        -TelemetryLogPath $resolvedFixturePath `
        -ReportPath $tempSummaryPath |
        Out-Null

    $exitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($exitCode -ne 0) {
        $errors.Add("Summary generator exited with code $exitCode")
    }
    elseif (-not (Test-Path -Path $tempSummaryPath -PathType Leaf)) {
        $errors.Add("Summary generator did not create expected report: $tempSummaryPath")
    }
    else {
        $summaryText = Get-Content -Path $tempSummaryPath -Raw
        Add-Check -Errors $errors -ReportText $summaryText -Needle "- Entries parsed: 10" -Label "Totals"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "- Runs parsed: 2" -Label "Totals"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "## Rhythm Snapshot" -Label "Rhythm"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "## Tune Next" -Label "Tune Next"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "- Primary bottleneck: Opening Clarity" -Label "Tune Next"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "### Sweep 01" -Label "Sweep"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "- Completed: yes" -Label "Sweep"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "### Stage 02" -Label "Stage Trends"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "- OPENING FAILED: 1 run(s), stages 02" -Label "Failure Buckets"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "### Run 02 - Stage 02" -Label "Run Summary"
    }
}

if (Test-Path -Path $tempSummaryPath -PathType Leaf) {
    Remove-Item -Path $tempSummaryPath -Force
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][PlaytestTelemetrySummaryRegression] Playtest telemetry summary regression")
$lines.Add("Fixture: $resolvedFixturePath")
$lines.Add("Summary script: $summaryScriptPath")
$lines.Add("PowerShell: $powerShellExecutable")

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
