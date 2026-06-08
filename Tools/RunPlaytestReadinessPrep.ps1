[CmdletBinding()]
param(
    [int]$MaxStage = 7,
    [int]$MaxGrowthStage = 7,
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
    $Lines.Add('- Run one real editor/development `F10` Stage 1-7 sweep.')
    $Lines.Add('- Fill `Docs/AlienCrusherStagePlaytestNotes.md` with meaningful Stage 1-7 readability, route pressure, map identity, and rhythm identity notes.')
    $Lines.Add("- Complete the Progression Save Smoke Pass with a concrete save/load result.")
    $Lines.Add("- Re-run this prep after the sweep and notes are captured.")
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

$lines.Add("")
$lines.Add("## Output Summary")
Add-ResultLine -Lines $lines -Path (Join-Path $projectRoot "Logs\AlienCrusherReadinessReportsRegression.log") -Label "Readiness report regression"
Add-ResultLine -Lines $lines -Path $evidenceGateReportPath -Label "Evidence gate readiness"
$lines.Add("Stage checklist: $stageChecklistPath")
if ($IncludeProductionChecklists) {
    foreach ($spec in $productionChecklistSpecs) {
        $lines.Add("$($spec.Label): $(Join-Path $projectRoot "Logs\$($spec.Report)")")
    }
}
$lines.Add("Telemetry summary: $telemetrySummaryPath")
$lines.Add("Evidence gate report: $evidenceGateReportPath")

Add-NextHumanEvidenceAction -Lines $lines -TelemetryLogPath (Join-Path $projectRoot "Logs\AlienCrusherPlaytestTelemetry.log") -EvidenceGateReportPath $evidenceGateReportPath

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
