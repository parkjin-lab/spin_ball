[CmdletBinding()]
param(
    [int]$MaxStage = 10,
    [int]$MaxGrowthStage = 10,
    [string]$ReportPath = "",
    [switch]$SkipStaticAudits,
    [switch]$IncludeProductionChecklists
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

function Add-ResultLine {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$Path,
        [string]$Label
    )

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        $Lines.Add("$Label`: missing")
        return
    }

    $result = Select-String -Path $Path -Pattern "Result:" | Select-Object -Last 1
    if ($null -eq $result) {
        $Lines.Add("$Label`: no Result line")
        return
    }

    $Lines.Add("$Label`: $($result.Line)")
}

function Add-NextHumanEvidenceAction {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$TelemetryLogPath,
        [string]$EvidenceGateReportPath
    )

    $needsHumanEvidence = -not (Test-Path -Path $TelemetryLogPath -PathType Leaf)
    if (-not $needsHumanEvidence -and (Test-Path -Path $EvidenceGateReportPath -PathType Leaf)) {
        $gateResult = Select-String -Path $EvidenceGateReportPath -Pattern "Result:" | Select-Object -Last 1
        $needsHumanEvidence = $null -ne $gateResult -and $gateResult.Line -notmatch "Result: 0 error\(s\)"
    }

    if (-not $needsHumanEvidence) {
        return
    }

    $Lines.Add("")
    $Lines.Add("## Next Required Human Evidence")
    if (Test-Path -Path $EvidenceGateReportPath -PathType Leaf) {
        $evidenceGateLines = @(Get-Content -Path $EvidenceGateReportPath)
        $resultLine = $evidenceGateLines | Where-Object { $_ -match '^Result:' } | Select-Object -Last 1
        $missingNoteCount = @($evidenceGateLines | Where-Object { $_ -match '^ERROR: Stage \d{2} missing meaningful note field:' }).Count
        $missingScreenshotCount = @($evidenceGateLines | Where-Object { $_ -match '^WARN: Stage \d{2} has no screenshot/video reference\.' }).Count
        $missingTelemetry = @($evidenceGateLines | Where-Object { $_ -match '^ERROR: Missing telemetry log\.' }).Count -gt 0
        $missingSaveSmoke = @($evidenceGateLines | Where-Object { $_ -match '^ERROR: Progression Save Smoke Pass missing meaningful Save/load result\.' }).Count -gt 0

        $Lines.Add("Evidence gate snapshot: $(if ($null -ne $resultLine) { $resultLine } else { 'Result line missing' })")
        $Lines.Add("- Missing telemetry sweep: $(if ($missingTelemetry) { 'yes' } else { 'no' })")
        $Lines.Add("- Missing stage note fields: $missingNoteCount")
        $Lines.Add("- Missing save smoke result: $(if ($missingSaveSmoke) { 'yes' } else { 'no' })")
        $Lines.Add("- Missing screenshot/video references: $missingScreenshotCount")
        $Lines.Add("")
    }

    $Lines.Add('- Run one real editor/development `F10` Stage 1-7 sweep.')
    $Lines.Add('- Fill `Docs/AlienCrusherStagePlaytestNotes.md` with meaningful Stage 1-7 readability, route pressure, map identity, and rhythm identity notes.')
    $Lines.Add("- Complete the Progression Save Smoke Pass with a concrete save/load result.")
    $Lines.Add("- Re-run this prep after the sweep and notes are captured.")
}

function Add-NextAutonomousWorkAction {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [bool]$IncludeProductionChecklists
    )

    $Lines.Add("")
    $Lines.Add("## Next Autonomous Work While Waiting")
    $Lines.Add("- Keep ``Tools/RunStaticAudits.ps1`` and ``Tools/TestPlaytestReadinessPrep.ps1 -FailOnWarnings`` green after every readiness/tooling change.")
    $Lines.Add("- Improve checklist/report readability, evidence-gate diagnostics, and handoff docs without changing rhythm tuning values.")
    $Lines.Add("- Start resource work from ``Logs/AlienCrusherResourceProductionBacklog.md`` ``## Recommended Production Batch Order``; use ``## Production Batch Focus`` for the full batch details.")
    $Lines.Add("- Update ``Docs/NEXT_SESSION_CONTEXT_PACKET.md`` and ``Docs/GAME_UPDATE_ROADMAP.md`` whenever automation changes the next safe task.")

    if (-not $IncludeProductionChecklists) {
        $Lines.Add("- If the next run is resource-focused, re-run this prep with ``-IncludeProductionChecklists`` before assigning asset work.")
    }
}

function Invoke-PrepStep {
    param(
        [string]$Label,
        [string]$ScriptPath,
        [string[]]$Arguments,
        [string]$PowerShellExecutable,
        [System.Collections.Generic.List[string]]$Lines
    )

    $Lines.Add("")
    $Lines.Add("== $Label ==")

    if (-not (Test-Path -Path $ScriptPath -PathType Leaf)) {
        $Lines.Add("FAIL: missing script $ScriptPath")
        return 1
    }

    $output = & $PowerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $ScriptPath @Arguments 2>&1
    $exitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }

    $outputLines = @($output | ForEach-Object { [string]$_ })
    $resultLines = @($outputLines | Where-Object { $_ -match '^Result:' })
    $warningLines = @($outputLines | Where-Object { $_ -match '^(No playtest telemetry log exists yet\.|ReportOnly:|ERROR:|WARN:)' })

    if ($resultLines.Count -gt 0) {
        $Lines.Add(($resultLines | Select-Object -Last 1))
    }

    $maxIssueLines = 12
    $issueLineCount = 0
    foreach ($line in $warningLines) {
        if ($line -match '^(ERROR:|WARN:)' -and $issueLineCount -ge $maxIssueLines) {
            continue
        }

        $Lines.Add($line)
        if ($line -match '^(ERROR:|WARN:)') {
            $issueLineCount++
        }
    }

    $omittedIssueCount = @($warningLines | Where-Object { $_ -match '^(ERROR:|WARN:)' }).Count - $issueLineCount
    if ($omittedIssueCount -gt 0) {
        $Lines.Add("... $omittedIssueCount more issue(s) omitted from prep console summary; see the generated report for full details.")
    }

    if ($exitCode -eq 0) {
        $Lines.Add("PASS: $Label")
    }
    else {
        $Lines.Add("FAIL: $Label exited with code $exitCode")
    }

    return $exitCode
}

$projectRoot = Resolve-ProjectRoot
$resolvedReportPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ReportPath -RelativePath "Logs\AlienCrusherPlaytestReadinessPrep.log"
$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

$stageChecklistPath = Join-Path $projectRoot "Logs\AlienCrusherStagePlaytestChecklist.md"
$autonomousBacklogPath = Join-Path $projectRoot "Logs\AlienCrusherAutonomousWorkBacklog.md"
$resourceProductionBacklogPath = Join-Path $projectRoot "Logs\AlienCrusherResourceProductionBacklog.md"
$architectureExtractionPlanPath = Join-Path $projectRoot "Logs\AlienCrusherArchitectureExtractionPlan.md"
$automationStatusSummaryPath = Join-Path $projectRoot "Logs\AlienCrusherAutomationStatusSummary.md"
$telemetrySummaryPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetrySummary.md"
$evidenceGateReportPath = Join-Path $projectRoot "Logs\AlienCrusherPlaytestEvidenceGate.log"
$productionChecklistSpecs = @(
    [pscustomobject]@{ Label = "Audio resource assignment checklist"; Script = "GenerateAudioResourceAssignmentChecklist.ps1"; Report = "AlienCrusherAudioResourceAssignmentChecklist.md" },
    [pscustomobject]@{ Label = "Form identity production checklist"; Script = "GenerateFormIdentityProductionChecklist.ps1"; Report = "AlienCrusherFormIdentityProductionChecklist.md" },
    [pscustomobject]@{ Label = "Destruction readability checklist"; Script = "GenerateDestructionReadabilityChecklist.ps1"; Report = "AlienCrusherDestructionReadabilityChecklist.md" },
    [pscustomobject]@{ Label = "Street prop variety checklist"; Script = "GenerateStreetPropVarietyChecklist.ps1"; Report = "AlienCrusherStreetPropVarietyChecklist.md" },
    [pscustomobject]@{ Label = "UI icon status checklist"; Script = "GenerateUiIconStatusChecklist.ps1"; Report = "AlienCrusherUiIconStatusChecklist.md" },
    [pscustomobject]@{ Label = "Boss identity production checklist"; Script = "GenerateBossIdentityProductionChecklist.ps1"; Report = "AlienCrusherBossIdentityProductionChecklist.md" },
    [pscustomobject]@{ Label = "District palette production checklist"; Script = "GenerateDistrictPaletteProductionChecklist.ps1"; Report = "AlienCrusherDistrictPaletteProductionChecklist.md" },
    [pscustomobject]@{ Label = "Outgame progression checklist"; Script = "GenerateOutgameProgressionChecklist.ps1"; Report = "AlienCrusherOutgameProgressionChecklist.md" },
    [pscustomobject]@{ Label = "Route payoff layout checklist"; Script = "GenerateRoutePayoffLayoutChecklist.ps1"; Report = "AlienCrusherRoutePayoffLayoutChecklist.md" }
)

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][PlaytestReadinessPrep] Playtest readiness prep")
$lines.Add("Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm K")")
$lines.Add("Project: $projectRoot")
$lines.Add("PowerShell: $powerShellExecutable")
$lines.Add("MaxStage: $MaxStage")
$lines.Add("MaxGrowthStage: $MaxGrowthStage")
$lines.Add("SkipStaticAudits: $SkipStaticAudits")
$lines.Add("IncludeProductionChecklists: $IncludeProductionChecklists")

$failed = 0
if (-not $SkipStaticAudits) {
    $failed += Invoke-PrepStep `
        -Label "Static audits" `
        -ScriptPath (Join-Path $PSScriptRoot "RunStaticAudits.ps1") `
        -Arguments @("-MaxStage", "$MaxStage", "-MaxGrowthStage", "$MaxGrowthStage") `
        -PowerShellExecutable $powerShellExecutable `
        -Lines $lines
}

$failed += Invoke-PrepStep `
    -Label "Stage playtest checklist" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateStagePlaytestChecklist.ps1") `
    -Arguments @("-MaxStage", "$MaxStage", "-MaxGrowthStage", "$MaxGrowthStage", "-ReportPath", $stageChecklistPath) `
    -PowerShellExecutable $powerShellExecutable `
    -Lines $lines

if ($IncludeProductionChecklists) {
    foreach ($spec in $productionChecklistSpecs) {
        $failed += Invoke-PrepStep `
            -Label $spec.Label `
            -ScriptPath (Join-Path $PSScriptRoot $spec.Script) `
            -Arguments @("-ReportPath", (Join-Path $projectRoot "Logs\$($spec.Report)")) `
            -PowerShellExecutable $powerShellExecutable `
            -Lines $lines
    }
}

$failed += Invoke-PrepStep `
    -Label "Autonomous work backlog" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateAutonomousWorkBacklog.ps1") `
    -Arguments @("-ReportPath", $autonomousBacklogPath) `
    -PowerShellExecutable $powerShellExecutable `
    -Lines $lines

$failed += Invoke-PrepStep `
    -Label "Resource production backlog" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateResourceProductionBacklog.ps1") `
    -Arguments @("-ReportPath", $resourceProductionBacklogPath) `
    -PowerShellExecutable $powerShellExecutable `
    -Lines $lines

$failed += Invoke-PrepStep `
    -Label "Architecture extraction plan" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateArchitectureExtractionPlan.ps1") `
    -Arguments @("-ReportPath", $architectureExtractionPlanPath) `
    -PowerShellExecutable $powerShellExecutable `
    -Lines $lines

$failed += Invoke-PrepStep `
    -Label "Playtest telemetry summary" `
    -ScriptPath (Join-Path $PSScriptRoot "GeneratePlaytestTelemetrySummary.ps1") `
    -Arguments @("-ReportPath", $telemetrySummaryPath) `
    -PowerShellExecutable $powerShellExecutable `
    -Lines $lines

$evidenceExitCode = Invoke-PrepStep `
    -Label "Evidence gate readiness report" `
    -ScriptPath (Join-Path $PSScriptRoot "TestPlaytestEvidenceGate.ps1") `
    -Arguments @("-MaxStage", "$MaxStage", "-ReportPath", $evidenceGateReportPath, "-ReportOnly") `
    -PowerShellExecutable $powerShellExecutable `
    -Lines $lines

if ($evidenceExitCode -ne 0) {
    $failed += $evidenceExitCode
}

$failed += Invoke-PrepStep `
    -Label "Automation status summary" `
    -ScriptPath (Join-Path $PSScriptRoot "GenerateAutomationStatusSummary.ps1") `
    -Arguments @("-ReportPath", $automationStatusSummaryPath) `
    -PowerShellExecutable $powerShellExecutable `
    -Lines $lines

$lines.Add("")
$lines.Add("## Output Summary")
Add-ResultLine -Lines $lines -Path (Join-Path $projectRoot "Logs\AlienCrusherReadinessReportsRegression.log") -Label "Readiness report regression"
Add-ResultLine -Lines $lines -Path $evidenceGateReportPath -Label "Evidence gate readiness"
$lines.Add("Stage checklist: $stageChecklistPath")
$lines.Add("Autonomous work backlog: $autonomousBacklogPath")
$lines.Add("Resource production backlog: $resourceProductionBacklogPath")
$lines.Add("Architecture extraction plan: $architectureExtractionPlanPath")
$lines.Add("Automation status summary: $automationStatusSummaryPath")
if ($IncludeProductionChecklists) {
    foreach ($spec in $productionChecklistSpecs) {
        $lines.Add("$($spec.Label): $(Join-Path $projectRoot "Logs\$($spec.Report)")")
    }
}
$lines.Add("Telemetry summary: $telemetrySummaryPath")
$lines.Add("Evidence gate report: $evidenceGateReportPath")

Add-NextHumanEvidenceAction -Lines $lines -TelemetryLogPath (Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetry.log") -EvidenceGateReportPath $evidenceGateReportPath
Add-NextAutonomousWorkAction -Lines $lines -IncludeProductionChecklists $IncludeProductionChecklists

if ($failed -gt 0) {
    $lines.Add("")
    $lines.Add("Result: $failed prep step(s) failed")
}
else {
    $lines.Add("")
    $lines.Add("Result: playtest readiness prep completed")
}

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
Write-Output $report

if ($failed -gt 0) {
    exit 1
}

exit 0
