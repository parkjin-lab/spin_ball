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
$checklistScriptPath = Join-Path $PSScriptRoot "GenerateStagePlaytestChecklist.ps1"
$summaryScriptPath = Join-Path $PSScriptRoot "GeneratePlaytestTelemetrySummary.ps1"
$audioChecklistScriptPath = Join-Path $PSScriptRoot "GenerateAudioResourceAssignmentChecklist.ps1"
$formChecklistScriptPath = Join-Path $PSScriptRoot "GenerateFormIdentityProductionChecklist.ps1"
$destructionChecklistScriptPath = Join-Path $PSScriptRoot "GenerateDestructionReadabilityChecklist.ps1"
$streetPropChecklistScriptPath = Join-Path $PSScriptRoot "GenerateStreetPropVarietyChecklist.ps1"
$uiIconChecklistScriptPath = Join-Path $PSScriptRoot "GenerateUiIconStatusChecklist.ps1"
$bossIdentityChecklistScriptPath = Join-Path $PSScriptRoot "GenerateBossIdentityProductionChecklist.ps1"
$districtPaletteChecklistScriptPath = Join-Path $PSScriptRoot "GenerateDistrictPaletteProductionChecklist.ps1"
$outgameProgressionChecklistScriptPath = Join-Path $PSScriptRoot "GenerateOutgameProgressionChecklist.ps1"
$routePayoffChecklistScriptPath = Join-Path $PSScriptRoot "GenerateRoutePayoffLayoutChecklist.ps1"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherReadinessReportsRegression.log"
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

if (-not (Test-Path -Path $checklistScriptPath -PathType Leaf)) {
    $errors.Add("Checklist generator not found: $checklistScriptPath")
}

if (-not (Test-Path -Path $summaryScriptPath -PathType Leaf)) {
    $errors.Add("Telemetry summary generator not found: $summaryScriptPath")
}

if (-not (Test-Path -Path $audioChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Audio resource checklist generator not found: $audioChecklistScriptPath")
}

if (-not (Test-Path -Path $formChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Form identity checklist generator not found: $formChecklistScriptPath")
}

if (-not (Test-Path -Path $destructionChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Destruction readability checklist generator not found: $destructionChecklistScriptPath")
}

if (-not (Test-Path -Path $streetPropChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Street prop variety checklist generator not found: $streetPropChecklistScriptPath")
}

if (-not (Test-Path -Path $uiIconChecklistScriptPath -PathType Leaf)) {
    $errors.Add("UI icon/status checklist generator not found: $uiIconChecklistScriptPath")
}

if (-not (Test-Path -Path $bossIdentityChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Boss identity checklist generator not found: $bossIdentityChecklistScriptPath")
}

if (-not (Test-Path -Path $districtPaletteChecklistScriptPath -PathType Leaf)) {
    $errors.Add("District palette checklist generator not found: $districtPaletteChecklistScriptPath")
}

if (-not (Test-Path -Path $outgameProgressionChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Outgame progression checklist generator not found: $outgameProgressionChecklistScriptPath")
}

if (-not (Test-Path -Path $routePayoffChecklistScriptPath -PathType Leaf)) {
    $errors.Add("Route payoff layout checklist generator not found: $routePayoffChecklistScriptPath")
}

$tempId = [guid]::NewGuid().ToString("N")
$tempChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherStagePlaytestChecklistReadiness-{0}.md" -f $tempId)
$tempSummaryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherPlaytestTelemetrySummaryReadiness-{0}.md" -f $tempId)
$tempAudioChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherAudioResourceAssignmentChecklistReadiness-{0}.md" -f $tempId)
$tempFormChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherFormIdentityProductionChecklistReadiness-{0}.md" -f $tempId)
$tempDestructionChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherDestructionReadabilityChecklistReadiness-{0}.md" -f $tempId)
$tempStreetPropChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherStreetPropVarietyChecklistReadiness-{0}.md" -f $tempId)
$tempUiIconChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherUiIconStatusChecklistReadiness-{0}.md" -f $tempId)
$tempBossIdentityChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherBossIdentityProductionChecklistReadiness-{0}.md" -f $tempId)
$tempDistrictPaletteChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherDistrictPaletteProductionChecklistReadiness-{0}.md" -f $tempId)
$tempOutgameProgressionChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherOutgameProgressionChecklistReadiness-{0}.md" -f $tempId)
$tempRoutePayoffChecklistPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherRoutePayoffLayoutChecklistReadiness-{0}.md" -f $tempId)
$tempMissingTelemetryPath = Join-Path ([System.IO.Path]::GetTempPath()) ("AlienCrusherMissingTelemetry-{0}.log" -f $tempId)
$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "powershell"
}

if ($errors.Count -eq 0) {
    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $checklistScriptPath `
        -MaxStage $MaxStage `
        -MaxGrowthStage $MaxGrowthStage `
        -ReportPath $tempChecklistPath |
        Out-Null

    $checklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($checklistExitCode -ne 0) {
        $errors.Add("Checklist generator exited with code $checklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempChecklistPath -PathType Leaf)) {
        $errors.Add("Checklist generator did not create expected report: $tempChecklistPath")
    }
    else {
        $checklistText = Get-Content -Path $tempChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## Validation Gate" -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle 'Manual notes file: `Docs/AlienCrusherStagePlaytestNotes.md`' -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## First F10 Sweep Run Sheet" -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## Rhythm Pass" -Label "Checklist"
        Add-Check -Errors $errors -ReportText $checklistText -Needle "## Post-Sweep Decision" -Label "Checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $summaryScriptPath `
        -TelemetryLogPath $tempMissingTelemetryPath `
        -ReportPath $tempSummaryPath |
        Out-Null

    $summaryExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($summaryExitCode -ne 0) {
        $errors.Add("Telemetry summary generator exited with code $summaryExitCode")
    }
    elseif (-not (Test-Path -Path $tempSummaryPath -PathType Leaf)) {
        $errors.Add("Telemetry summary generator did not create expected report: $tempSummaryPath")
    }
    else {
        $summaryText = Get-Content -Path $tempSummaryPath -Raw
        Add-Check -Errors $errors -ReportText $summaryText -Needle "No playtest telemetry log exists yet." -Label "No-log summary"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "Manual gate required:" -Label "No-log summary"
        Add-Check -Errors $errors -ReportText $summaryText -Needle 'Expected `Tune Next` fields after real telemetry:' -Label "No-log summary"
        Add-Check -Errors $errors -ReportText $summaryText -Needle "Do not tune stage rhythm presets, payoff layouts, boss windows, or route timing until this log exists." -Label "No-log summary"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $audioChecklistScriptPath `
        -ReportPath $tempAudioChecklistPath |
        Out-Null

    $audioChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($audioChecklistExitCode -ne 0) {
        $errors.Add("Audio resource checklist generator exited with code $audioChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempAudioChecklistPath -PathType Leaf)) {
        $errors.Add("Audio resource checklist generator did not create expected report: $tempAudioChecklistPath")
    }
    else {
        $audioChecklistText = Get-Content -Path $tempAudioChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle "## Assignment Pass Order" -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle "## Current FeedbackSystem Audio Slots" -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle 'failureWarningClip' -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle 'failureBossClip' -Label "Audio checklist"
        Add-Check -Errors $errors -ReportText $audioChecklistText -Needle 'SFX_Route_Open' -Label "Audio checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $formChecklistScriptPath `
        -ReportPath $tempFormChecklistPath |
        Out-Null

    $formChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($formChecklistExitCode -ne 0) {
        $errors.Add("Form identity checklist generator exited with code $formChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempFormChecklistPath -PathType Leaf)) {
        $errors.Add("Form identity checklist generator did not create expected report: $tempFormChecklistPath")
    }
    else {
        $formChecklistText = Get-Content -Path $tempFormChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle "## Production Pass Order" -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle "## Current Runtime Form Identity Targets" -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Sphere' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Spike' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Ram' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Saucer' -Label "Form checklist"
        Add-Check -Errors $errors -ReportText $formChecklistText -Needle 'Crusher' -Label "Form checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $destructionChecklistScriptPath `
        -ReportPath $tempDestructionChecklistPath |
        Out-Null

    $destructionChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($destructionChecklistExitCode -ne 0) {
        $errors.Add("Destruction readability checklist generator exited with code $destructionChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempDestructionChecklistPath -PathType Leaf)) {
        $errors.Add("Destruction readability checklist generator did not create expected report: $tempDestructionChecklistPath")
    }
    else {
        $destructionChecklistText = Get-Content -Path $tempDestructionChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle "## Production Pass Order" -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle "## Current Destruction Readability Targets" -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'MAT_WeakPoint_Glow' -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'MAT_Shielded_Pylon' -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'MAT_Exposed_Core' -Label "Destruction checklist"
        Add-Check -Errors $errors -ReportText $destructionChecklistText -Needle 'SFX_Break_LargeCollapse' -Label "Destruction checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $streetPropChecklistScriptPath `
        -ReportPath $tempStreetPropChecklistPath |
        Out-Null

    $streetPropChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($streetPropChecklistExitCode -ne 0) {
        $errors.Add("Street prop variety checklist generator exited with code $streetPropChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempStreetPropChecklistPath -PathType Leaf)) {
        $errors.Add("Street prop variety checklist generator did not create expected report: $tempStreetPropChecklistPath")
    }
    else {
        $streetPropChecklistText = Get-Content -Path $tempStreetPropChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $streetPropChecklistText -Needle "## Production Pass Order" -Label "Street prop checklist"
        Add-Check -Errors $errors -ReportText $streetPropChecklistText -Needle "## Current Street Prop Variety Targets" -Label "Street prop checklist"
        Add-Check -Errors $errors -ReportText $streetPropChecklistText -Needle 'PROP_Car_Compact_A' -Label "Street prop checklist"
        Add-Check -Errors $errors -ReportText $streetPropChecklistText -Needle 'PROP_Kiosk' -Label "Street prop checklist"
        Add-Check -Errors $errors -ReportText $streetPropChecklistText -Needle 'PROP_Transformer' -Label "Street prop checklist"
        Add-Check -Errors $errors -ReportText $streetPropChecklistText -Needle 'PROP_ExplosiveBarrel' -Label "Street prop checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $uiIconChecklistScriptPath `
        -ReportPath $tempUiIconChecklistPath |
        Out-Null

    $uiIconChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($uiIconChecklistExitCode -ne 0) {
        $errors.Add("UI icon/status checklist generator exited with code $uiIconChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempUiIconChecklistPath -PathType Leaf)) {
        $errors.Add("UI icon/status checklist generator did not create expected report: $tempUiIconChecklistPath")
    }
    else {
        $uiIconChecklistText = Get-Content -Path $tempUiIconChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $uiIconChecklistText -Needle "## Production Pass Order" -Label "UI icon checklist"
        Add-Check -Errors $errors -ReportText $uiIconChecklistText -Needle "## Current UI Icon And Status Targets" -Label "UI icon checklist"
        Add-Check -Errors $errors -ReportText $uiIconChecklistText -Needle 'Icon_DP' -Label "UI icon checklist"
        Add-Check -Errors $errors -ReportText $uiIconChecklistText -Needle 'Icon_Route' -Label "UI icon checklist"
        Add-Check -Errors $errors -ReportText $uiIconChecklistText -Needle 'Icon_Shield' -Label "UI icon checklist"
        Add-Check -Errors $errors -ReportText $uiIconChecklistText -Needle 'Icon_Boss' -Label "UI icon checklist"
        Add-Check -Errors $errors -ReportText $uiIconChecklistText -Needle 'Badge_Recommended' -Label "UI icon checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $bossIdentityChecklistScriptPath `
        -ReportPath $tempBossIdentityChecklistPath |
        Out-Null

    $bossIdentityChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($bossIdentityChecklistExitCode -ne 0) {
        $errors.Add("Boss identity checklist generator exited with code $bossIdentityChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempBossIdentityChecklistPath -PathType Leaf)) {
        $errors.Add("Boss identity checklist generator did not create expected report: $tempBossIdentityChecklistPath")
    }
    else {
        $bossIdentityChecklistText = Get-Content -Path $tempBossIdentityChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $bossIdentityChecklistText -Needle "## Production Pass Order" -Label "Boss identity checklist"
        Add-Check -Errors $errors -ReportText $bossIdentityChecklistText -Needle "## Boss Rhythm Contract" -Label "Boss identity checklist"
        Add-Check -Errors $errors -ReportText $bossIdentityChecklistText -Needle "## Current Boss Identity Targets" -Label "Boss identity checklist"
        Add-Check -Errors $errors -ReportText $bossIdentityChecklistText -Needle 'BOSS_Sentinel_Body_Kit' -Label "Boss identity checklist"
        Add-Check -Errors $errors -ReportText $bossIdentityChecklistText -Needle 'BOSS_Shield_Pylon_Kit' -Label "Boss identity checklist"
        Add-Check -Errors $errors -ReportText $bossIdentityChecklistText -Needle 'BOSS_Phase2_Drone_Kit' -Label "Boss identity checklist"
        Add-Check -Errors $errors -ReportText $bossIdentityChecklistText -Needle 'SFX_Boss_Down' -Label "Boss identity checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $districtPaletteChecklistScriptPath `
        -ReportPath $tempDistrictPaletteChecklistPath |
        Out-Null

    $districtPaletteChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($districtPaletteChecklistExitCode -ne 0) {
        $errors.Add("District palette checklist generator exited with code $districtPaletteChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempDistrictPaletteChecklistPath -PathType Leaf)) {
        $errors.Add("District palette checklist generator did not create expected report: $tempDistrictPaletteChecklistPath")
    }
    else {
        $districtPaletteChecklistText = Get-Content -Path $tempDistrictPaletteChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $districtPaletteChecklistText -Needle "## Production Pass Order" -Label "District palette checklist"
        Add-Check -Errors $errors -ReportText $districtPaletteChecklistText -Needle "## District Rhythm Palette Contract" -Label "District palette checklist"
        Add-Check -Errors $errors -ReportText $districtPaletteChecklistText -Needle "## Current District Palette Targets" -Label "District palette checklist"
        Add-Check -Errors $errors -ReportText $districtPaletteChecklistText -Needle 'PAL_District_StarterResidential' -Label "District palette checklist"
        Add-Check -Errors $errors -ReportText $districtPaletteChecklistText -Needle 'PAL_District_SentinelCheckpoint' -Label "District palette checklist"
        Add-Check -Errors $errors -ReportText $districtPaletteChecklistText -Needle 'PAL_District_SkylineBlock' -Label "District palette checklist"
        Add-Check -Errors $errors -ReportText $districtPaletteChecklistText -Needle 'PAL_RouteMarker_Tints' -Label "District palette checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $outgameProgressionChecklistScriptPath `
        -ReportPath $tempOutgameProgressionChecklistPath |
        Out-Null

    $outgameProgressionChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($outgameProgressionChecklistExitCode -ne 0) {
        $errors.Add("Outgame progression checklist generator exited with code $outgameProgressionChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempOutgameProgressionChecklistPath -PathType Leaf)) {
        $errors.Add("Outgame progression checklist generator did not create expected report: $tempOutgameProgressionChecklistPath")
    }
    else {
        $outgameProgressionChecklistText = Get-Content -Path $tempOutgameProgressionChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $outgameProgressionChecklistText -Needle "## Production Pass Order" -Label "Outgame progression checklist"
        Add-Check -Errors $errors -ReportText $outgameProgressionChecklistText -Needle "## Outgame Loop Contract" -Label "Outgame progression checklist"
        Add-Check -Errors $errors -ReportText $outgameProgressionChecklistText -Needle "## Current Outgame Progression Targets" -Label "Outgame progression checklist"
        Add-Check -Errors $errors -ReportText $outgameProgressionChecklistText -Needle 'UI_FormCard_StateSet' -Label "Outgame progression checklist"
        Add-Check -Errors $errors -ReportText $outgameProgressionChecklistText -Needle 'UI_MetaNode_SizeCore' -Label "Outgame progression checklist"
        Add-Check -Errors $errors -ReportText $outgameProgressionChecklistText -Needle 'UI_DP_GainBurst' -Label "Outgame progression checklist"
        Add-Check -Errors $errors -ReportText $outgameProgressionChecklistText -Needle 'Banner_StageUnlocked' -Label "Outgame progression checklist"
    }

    & $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $routePayoffChecklistScriptPath `
        -ReportPath $tempRoutePayoffChecklistPath |
        Out-Null

    $routePayoffChecklistExitCode = if ($null -eq $global:LASTEXITCODE) { 0 } else { [int]$global:LASTEXITCODE }
    if ($routePayoffChecklistExitCode -ne 0) {
        $errors.Add("Route payoff layout checklist generator exited with code $routePayoffChecklistExitCode")
    }
    elseif (-not (Test-Path -Path $tempRoutePayoffChecklistPath -PathType Leaf)) {
        $errors.Add("Route payoff layout checklist generator did not create expected report: $tempRoutePayoffChecklistPath")
    }
    else {
        $routePayoffChecklistText = Get-Content -Path $tempRoutePayoffChecklistPath -Raw
        Add-Check -Errors $errors -ReportText $routePayoffChecklistText -Needle "## Production Pass Order" -Label "Route payoff checklist"
        Add-Check -Errors $errors -ReportText $routePayoffChecklistText -Needle "## Route Payoff Rhythm Contract" -Label "Route payoff checklist"
        Add-Check -Errors $errors -ReportText $routePayoffChecklistText -Needle "## Current Route Payoff Layout Targets" -Label "Route payoff checklist"
        Add-Check -Errors $errors -ReportText $routePayoffChecklistText -Needle 'PAYOFF_ParkCut_Layout' -Label "Route payoff checklist"
        Add-Check -Errors $errors -ReportText $routePayoffChecklistText -Needle 'PAYOFF_MarketChain_Layout' -Label "Route payoff checklist"
        Add-Check -Errors $errors -ReportText $routePayoffChecklistText -Needle 'PAYOFF_SkylineBreach_Layout' -Label "Route payoff checklist"
        Add-Check -Errors $errors -ReportText $routePayoffChecklistText -Needle 'VFX_ForwardSmash_Confirm' -Label "Route payoff checklist"
    }
}

foreach ($tempFilePath in @($tempChecklistPath, $tempSummaryPath, $tempAudioChecklistPath, $tempFormChecklistPath, $tempDestructionChecklistPath, $tempStreetPropChecklistPath, $tempUiIconChecklistPath, $tempBossIdentityChecklistPath, $tempDistrictPaletteChecklistPath, $tempOutgameProgressionChecklistPath, $tempRoutePayoffChecklistPath, $tempMissingTelemetryPath)) {
    if (Test-Path -Path $tempFilePath -PathType Leaf) {
        Remove-Item -Path $tempFilePath -Force
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][ReadinessReportsRegression] Readiness report generator regression")
$lines.Add("Checklist script: $checklistScriptPath")
$lines.Add("Summary script: $summaryScriptPath")
$lines.Add("Audio checklist script: $audioChecklistScriptPath")
$lines.Add("Form checklist script: $formChecklistScriptPath")
$lines.Add("Destruction checklist script: $destructionChecklistScriptPath")
$lines.Add("Street prop checklist script: $streetPropChecklistScriptPath")
$lines.Add("UI icon checklist script: $uiIconChecklistScriptPath")
$lines.Add("Boss identity checklist script: $bossIdentityChecklistScriptPath")
$lines.Add("District palette checklist script: $districtPaletteChecklistScriptPath")
$lines.Add("Outgame progression checklist script: $outgameProgressionChecklistScriptPath")
$lines.Add("Route payoff checklist script: $routePayoffChecklistScriptPath")
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
