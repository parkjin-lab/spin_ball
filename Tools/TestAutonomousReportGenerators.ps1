[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
    [string]$ReportPath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
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

function Invoke-GeneratorCheck {
    param(
        [string]$Label,
        [string]$ScriptPath,
        [string]$ReportPath,
        [string[]]$Needles,
        [string]$PowerShellExecutable,
        [System.Collections.Generic.List[string]]$Errors
    )

    if (-not (Test-Path -Path $ScriptPath -PathType Leaf)) {
        $Errors.Add("$Label script not found: $ScriptPath")
        return
    }

    & $PowerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $ScriptPath -ReportPath $ReportPath | Out-Null
    $exitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($exitCode -ne 0) {
        $Errors.Add("$Label exited with code $exitCode")
        return
    }

    if (-not (Test-Path -Path $ReportPath -PathType Leaf)) {
        $Errors.Add("$Label did not create expected report: $ReportPath")
        return
    }

    $reportText = Get-Content -Path $ReportPath -Raw
    foreach ($needle in $Needles) {
        Add-Check -Errors $Errors -ReportText $reportText -Needle $needle -Label $Label
    }
}

$projectRoot = Resolve-ProjectRoot
if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherAutonomousReportGeneratorsRegression.log"
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
$tempId = [guid]::NewGuid().ToString("N")
$tempDirectory = [System.IO.Path]::GetTempPath()
$tempReports = @(
    (Join-Path $tempDirectory ("AlienCrusherAutonomousWorkBacklog-{0}.md" -f $tempId)),
    (Join-Path $tempDirectory ("AlienCrusherResourceProductionBacklog-{0}.md" -f $tempId)),
    (Join-Path $tempDirectory ("AlienCrusherArchitectureExtractionPlan-{0}.md" -f $tempId)),
    (Join-Path $tempDirectory ("AlienCrusherAutomationStatusSummary-{0}.md" -f $tempId))
)

$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

Invoke-GeneratorCheck `
    -Label "Autonomous work backlog" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateAutonomousWorkBacklog.ps1") `
    -ReportPath $tempReports[0] `
    -Needles @("## Current Blocking State", "Resource production batch focus:", "Recommended production batch order:", "## Safe Autonomous Work Queue", "## Recommended Production Batch Order", "## Production Batch Focus", "Prefer complete production batches", "Result: autonomous work backlog generated") `
    -PowerShellExecutable $powerShellExecutable `
    -Errors $errors

Invoke-GeneratorCheck `
    -Label "Resource production backlog" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateResourceProductionBacklog.ps1") `
    -ReportPath $tempReports[1] `
    -Needles @("## Recommended Production Batch Order", "1. [Audio] A. Route and failure rhythm", "2. [Route Payoff] A. District payoff layouts", "## Next Recommended Batch Task Card", "Source: Audio", "Done means: every target has a draft asset", "## Production Batch Focus", "A. Traffic silhouette set", "## P0 Backlog", "## Recommended Autonomous Order", "Result: resource production backlog generated", "Result: production batch focus generated", "Result: recommended production batch order generated") `
    -PowerShellExecutable $powerShellExecutable `
    -Errors $errors

Invoke-GeneratorCheck `
    -Label "Architecture extraction plan" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateArchitectureExtractionPlan.ps1") `
    -ReportPath $tempReports[2] `
    -Needles @("## Partial Surface Summary", "DummyFlowController.UIFlow.cs", "Result: architecture extraction plan generated") `
    -PowerShellExecutable $powerShellExecutable `
    -Errors $errors

Invoke-GeneratorCheck `
    -Label "Automation status summary" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateAutomationStatusSummary.ps1") `
    -ReportPath $tempReports[3] `
    -Needles @("## Progress", "Resource planning is consolidated:", "Resource production batches are consolidated:", "Resource production batch order is consolidated:", "Production checklist task cards are consolidated:", "9 / 9 checklist generators include next-batch task cards", "## Validation", "Meaningful stage notes:", "fields)", "## Next To-Do", "## Recommended Production Batch Order", "Result: automation status summary generated") `
    -PowerShellExecutable $powerShellExecutable `
    -Errors $errors

foreach ($tempReport in $tempReports) {
    if (Test-Path -Path $tempReport -PathType Leaf) {
        Remove-Item -Path $tempReport -Force
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][AutonomousReportGeneratorsRegression] Autonomous report generators regression")
$lines.Add("Project: $projectRoot")
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
