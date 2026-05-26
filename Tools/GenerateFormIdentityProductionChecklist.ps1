[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$FormTypePath = "",
    [string]$FormFlowPath = "",
    [string]$FormUnlockPath = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    return Split-Path -Parent $PSScriptRoot
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
        throw "Required source not found: $Path"
    }

    return Get-Content -Path $Path -Raw
}

function Read-IntDefault {
    param([string]$SourceText, [string]$FieldName, [int]$Fallback)

    $pattern = "(?m)^\s*(?:\[[^\]]+\]\s*)*private\s+int\s+$([regex]::Escape($FieldName))\s*=\s*(\d+)\s*;"
    $match = [regex]::Match($SourceText, $pattern)
    if (-not $match.Success) {
        return $Fallback
    }

    return [int]$match.Groups[1].Value
}

$projectRoot = Resolve-ProjectRoot
$formTypeSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FormTypePath -RelativePath "Assets\Scripts\Runtime\Gameplay\FormType.cs"
$formFlowSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FormFlowPath -RelativePath "Assets\Scripts\Runtime\Systems\DummyFlowController.FormFlow.cs"
$formUnlockSourcePath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FormUnlockPath -RelativePath "Assets\Scripts\Runtime\Systems\FormUnlockSystem.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherFormIdentityProductionChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$formTypeText = Read-SourceText -Path $formTypeSourcePath
$formFlowText = Read-SourceText -Path $formFlowSourcePath
$formUnlockText = Read-SourceText -Path $formUnlockSourcePath

$formMatches = [regex]::Matches($formTypeText, "(?m)^\s*(Sphere|Spike|Ram|Saucer|Crusher)\s*,?\s*$")
$runtimeForms = @()
foreach ($match in $formMatches) {
    $runtimeForms += $match.Groups[1].Value
}
$runtimeForms = @($runtimeForms | Sort-Object -Unique)

$unlockCosts = @{
    Sphere = 0
    Spike = Read-IntDefault -SourceText $formUnlockText -FieldName "spikeUnlockCost" -Fallback 450
    Ram = Read-IntDefault -SourceText $formUnlockText -FieldName "ramUnlockCost" -Fallback 900
    Saucer = Read-IntDefault -SourceText $formUnlockText -FieldName "saucerUnlockCost" -Fallback 1350
    Crusher = Read-IntDefault -SourceText $formUnlockText -FieldName "crusherUnlockCost" -Fallback 2100
}

$formCatalog = @(
    [pscustomobject]@{ Form = "Sphere"; Skill = "SPHERE PULSE"; Role = "stable default starter-lane control"; FailureProblem = "opening failed"; Silhouette = "smooth body plus subtle emissive belt"; Icon = "solid circle with orbit band"; Material = "cool green core with soft alien glow"; Priority = "P0" },
    [pscustomobject]@{ Form = "Spike"; Skill = "SPIKE BURST"; Role = "puncture and weak-point aggression"; FailureProblem = "weak point or dense-object stall"; Silhouette = "radial cone crown and sharper forward read"; Icon = "circle with four to six spikes"; Material = "hot magenta or acid accent over dark body"; Priority = "P0" },
    [pscustomobject]@{ Form = "Ram"; Skill = "RAM BREACH"; Role = "route recovery and mid-run drift correction"; FailureProblem = "route hold missed or final push failed"; Silhouette = "forward wedge nose with side horns"; Icon = "wedge arrow inside circle"; Material = "amber impact plate with darker shell"; Priority = "P0" },
    [pscustomobject]@{ Form = "Saucer"; Skill = "SAUCER DASH"; Role = "navigation, reach, and target access"; FailureProblem = "target reach or long-route commitment"; Silhouette = "flattened disk with wide rim"; Icon = "flat disk with motion streak"; Material = "cyan rim with pale underside"; Priority = "P0" },
    [pscustomobject]@{ Form = "Crusher"; Skill = "CRUSHER SLAM"; Role = "heavy finish, boss pressure, and collapse payoff"; FailureProblem = "boss phase or late-stage pressure"; Silhouette = "layered heavy shell and frontal plate"; Icon = "blocky mass with impact crack"; Material = "deep steel body with bright blue pressure seams"; Priority = "P0" }
)

$catalogForms = @($formCatalog | ForEach-Object { $_.Form } | Sort-Object -Unique)
$missingFromCatalog = @($runtimeForms | Where-Object { $catalogForms -notcontains $_ })
$missingFromRuntime = @($catalogForms | Where-Object { $runtimeForms -notcontains $_ })
$missingSkillMappings = @()
foreach ($form in $catalogForms) {
    if (-not $formFlowText.Contains("FormType.$form")) {
        $missingSkillMappings += $form
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Form Identity Production Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`, `{1}`, `{2}`' -f $formTypeSourcePath, $formFlowSourcePath, $formUnlockSourcePath))
$lines.Add("")
$lines.Add("Purpose: make every runtime form readable by silhouette, icon, color/material, skill fantasy, and unlock motivation.")
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime forms found: $([string]::Join(', ', $runtimeForms))")
$lines.Add("- Catalog forms tracked: $([string]::Join(', ', $catalogForms))")
$lines.Add("- Missing from catalog: $(if ($missingFromCatalog.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingFromCatalog) })")
$lines.Add("- Missing from runtime: $(if ($missingFromRuntime.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingFromRuntime) })")
$lines.Add("- Missing FormFlow references: $(if ($missingSkillMappings.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingSkillMappings) })")
$lines.Add("")
$lines.Add("## Production Pass Order")
$lines.Add("1. Build readable primitive silhouette addons for all five forms.")
$lines.Add("2. Capture temporary icon placeholders from the same silhouettes.")
$lines.Add("3. Assign one material/color family per form.")
$lines.Add("4. Verify the lobby buttons and in-run camera distance still distinguish each form.")
$lines.Add("")
$lines.Add("## Current Runtime Form Identity Targets")
$lines.Add("| Priority | Form | Unlock DP | Runtime skill | Gameplay role | Failure problem it should answer | Primitive silhouette target | Icon target | Material target | Done? |")
$lines.Add("|---|---|---:|---|---|---|---|---|---|---|")
foreach ($form in $formCatalog) {
    $lines.Add(("| {0} | `{1}` | {2} | {3} | {4} | {5} | {6} | {7} | {8} | [ ] |" -f $form.Priority, $form.Form, $unlockCosts[$form.Form], $form.Skill, $form.Role, $form.FailureProblem, $form.Silhouette, $form.Icon, $form.Material))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Sphere is allowed to be simple, but it must still look intentional beside the unlocked forms.")
$lines.Add("- Spike and Crusher can both be aggressive, but Spike should read sharp/fast while Crusher reads heavy/slow.")
$lines.Add("- Ram and Saucer are both route helpers, but Ram should read forward force while Saucer reads lateral reach.")
$lines.Add("- Do not judge final form balance from this checklist; it is only for identity/readability production.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Form identity production checklist written: $ReportPath"

if ($missingFromCatalog.Count -gt 0 -or $missingFromRuntime.Count -gt 0 -or $missingSkillMappings.Count -gt 0) {
    exit 1
}

exit 0
