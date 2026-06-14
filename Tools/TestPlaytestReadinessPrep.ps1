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

$projectRoot = Resolve-ProjectRoot
$prepScriptPath = Join-Path $PSScriptRoot "RunPlaytestReadinessPrep.ps1"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestReadinessPrepRegression.log"
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

if (-not (Test-Path -Path $prepScriptPath -PathType Leaf)) {
    $errors.Add("Playtest readiness prep runner not found: $prepScriptPath")
}

$tempId = [guid]::NewGuid().ToString("N")
$tempPrepReportPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestReadinessPrepRegression-{0}.log" -f $tempId)
$tempProductionPrepReportPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestReadinessPrepProductionRegression-{0}.log" -f $tempId)
$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

if ($errors.Count -eq 0) {
    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $prepScriptPath `
        -MaxStage $MaxStage `
        -MaxGrowthStage $MaxGrowthStage `
        -ReportPath $tempPrepReportPath `
        -SkipStaticAudits |
        Out-Null

    $prepExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($prepExitCode -ne 0) {
        $errors.Add("Playtest readiness prep runner exited with code $prepExitCode")
    }
    elseif (-not (Test-Path -Path $tempPrepReportPath -PathType Leaf)) {
        $errors.Add("Playtest readiness prep runner did not create expected report: $tempPrepReportPath")
    }
    else {
        $prepReportText = Get-Content -Path $tempPrepReportPath -Raw
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "[AlienCrusher][PlaytestReadinessPrep] Playtest readiness prep" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "SkipStaticAudits: True" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "IncludeProductionChecklists: False" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "PASS: Stage playtest checklist" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "PASS: Autonomous work backlog" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "PASS: Resource production backlog" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "PASS: Architecture extraction plan" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "PASS: Playtest telemetry summary" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "PASS: Evidence gate readiness report" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "PASS: Automation status summary" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Evidence gate readiness: Result:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "more issue(s) omitted from prep console summary" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "## Next Required Human Evidence" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Evidence gate snapshot: Result:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Missing stage note fields:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Missing save smoke result:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle 'Run one real editor/development `F10` Stage 1-7 sweep.' -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "## Next Autonomous Work While Waiting" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Improve checklist/report readability, evidence-gate diagnostics, and handoff docs without changing rhythm tuning values." -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Autonomous work backlog:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Resource production backlog:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Architecture extraction plan:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Automation status summary:" -Label "Prep report"
        Add-Check -Errors $errors -ReportText $prepReportText -Needle "Result: playtest readiness prep completed" -Label "Prep report"
    }
}

if ($errors.Count -eq 0) {
    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $prepScriptPath `
        -MaxStage $MaxStage `
        -MaxGrowthStage $MaxGrowthStage `
        -ReportPath $tempProductionPrepReportPath `
        -SkipStaticAudits `
        -IncludeProductionChecklists |
        Out-Null

    $productionPrepExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($productionPrepExitCode -ne 0) {
        $errors.Add("Playtest readiness prep production checklist run exited with code $productionPrepExitCode")
    }
    elseif (-not (Test-Path -Path $tempProductionPrepReportPath -PathType Leaf)) {
        $errors.Add("Playtest readiness prep production checklist run did not create expected report: $tempProductionPrepReportPath")
    }
    else {
        $productionPrepReportText = Get-Content -Path $tempProductionPrepReportPath -Raw
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "IncludeProductionChecklists: True" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "PASS: Audio resource assignment checklist" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "PASS: Route payoff layout checklist" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "PASS: Autonomous work backlog" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "PASS: Resource production backlog" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "PASS: Architecture extraction plan" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "PASS: Automation status summary" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "Audio resource assignment checklist:" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "Route payoff layout checklist:" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "more issue(s) omitted from prep console summary" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "## Next Required Human Evidence" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "Evidence gate snapshot: Result:" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "Missing screenshot/video references:" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "## Next Autonomous Work While Waiting" -Label "Production prep report"
        Add-Check -Errors $errors -ReportText $productionPrepReportText -Needle "Result: playtest readiness prep completed" -Label "Production prep report"
    }
}

foreach ($tempFilePath in @($tempPrepReportPath, $tempProductionPrepReportPath)) {
    if (Test-Path -Path $tempFilePath -PathType Leaf) {
        Remove-Item -Path $tempFilePath -Force
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][PlaytestReadinessPrepRegression] Playtest readiness prep regression")
$lines.Add("Prep script: $prepScriptPath")
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
