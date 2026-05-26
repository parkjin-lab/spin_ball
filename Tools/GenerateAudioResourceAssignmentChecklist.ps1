[CmdletBinding()]
param(
    [string]$ReportPath = "",
    [string]$FeedbackSystemPath = ""
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

$projectRoot = Resolve-ProjectRoot
$feedbackPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $FeedbackSystemPath -RelativePath "Assets\Scripts\Runtime\Systems\FeedbackSystem.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherAudioResourceAssignmentChecklist.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

if (-not (Test-Path -Path $feedbackPath -PathType Leaf)) {
    throw "FeedbackSystem not found: $feedbackPath"
}

$feedbackText = Get-Content -Path $feedbackPath -Raw
$slotMatches = [regex]::Matches($feedbackText, "\[SerializeField\][^\r\n]*private\s+AudioClip\s+([A-Za-z0-9_]+)")
$runtimeSlots = @()
foreach ($match in $slotMatches) {
    $runtimeSlots += $match.Groups[1].Value
}
$runtimeSlots = @($runtimeSlots | Sort-Object -Unique)

$slotCatalog = @(
    [pscustomobject]@{ Field = "hitLightClip"; AssetName = "SFX_Hit_Light"; Category = "Impact"; Folder = "Assets/Audio/SFX/Impact/"; RuntimeMoment = "small or glancing hit"; Priority = "P0" },
    [pscustomobject]@{ Field = "hitMediumClip"; AssetName = "SFX_Hit_Medium"; Category = "Impact"; Folder = "Assets/Audio/SFX/Impact/"; RuntimeMoment = "strong normal hit"; Priority = "P0" },
    [pscustomobject]@{ Field = "hitHeavyClip"; AssetName = "SFX_Hit_Heavy"; Category = "Impact"; Folder = "Assets/Audio/SFX/Impact/"; RuntimeMoment = "weak point or heavy body hit"; Priority = "P0" },
    [pscustomobject]@{ Field = "breakSmallClip"; AssetName = "SFX_Break_Small"; Category = "Destruction"; Folder = "Assets/Audio/SFX/Impact/"; RuntimeMoment = "small prop or light break"; Priority = "P0" },
    [pscustomobject]@{ Field = "breakLargeClip"; AssetName = "SFX_Break_LargeCollapse"; Category = "Destruction"; Folder = "Assets/Audio/SFX/Impact/"; RuntimeMoment = "large object collapse"; Priority = "P0" },
    [pscustomobject]@{ Field = "comboRiseClip"; AssetName = "SFX_Combo_Rise"; Category = "Rhythm"; Folder = "Assets/Audio/SFX/Skills/"; RuntimeMoment = "combo rush or frenzy rise"; Priority = "P0" },
    [pscustomobject]@{ Field = "routeOpenClip"; AssetName = "SFX_Route_Open"; Category = "Route"; Folder = "Assets/Audio/SFX/Skills/"; RuntimeMoment = "LANE BREAK -> ROUTE OPEN"; Priority = "P0" },
    [pscustomobject]@{ Field = "routeHoldWarningClip"; AssetName = "SFX_Route_HoldWarning"; Category = "Route"; Folder = "Assets/Audio/SFX/Skills/"; RuntimeMoment = "route hold urgency warning"; Priority = "P0" },
    [pscustomobject]@{ Field = "routeBonusClip"; AssetName = "SFX_Route_Bonus"; Category = "Route"; Folder = "Assets/Audio/SFX/Skills/"; RuntimeMoment = "ROUTE BONUS / Forward Smash payoff"; Priority = "P0" },
    [pscustomobject]@{ Field = "bossWarningClip"; AssetName = "SFX_Boss_Warning"; Category = "Boss"; Folder = "Assets/Audio/SFX/Boss/"; RuntimeMoment = "boss warning, drone, or shield pressure"; Priority = "P0" },
    [pscustomobject]@{ Field = "bossBreakClip"; AssetName = "SFX_Boss_Break"; Category = "Boss"; Folder = "Assets/Audio/SFX/Boss/"; RuntimeMoment = "boss shield, pylon, or core break"; Priority = "P0" },
    [pscustomobject]@{ Field = "bossDownClip"; AssetName = "SFX_Boss_Down"; Category = "Boss"; Folder = "Assets/Audio/SFX/Boss/"; RuntimeMoment = "Justice Sentinel downbeat"; Priority = "P0" },
    [pscustomobject]@{ Field = "levelUpClip"; AssetName = "SFX_LevelUp_Open"; Category = "Progression"; Folder = "Assets/Audio/SFX/UI/"; RuntimeMoment = "level-up choice opens"; Priority = "P0" },
    [pscustomobject]@{ Field = "failureWarningClip"; AssetName = "SFX_Failure_Warning"; Category = "Failure"; Folder = "Assets/Audio/SFX/Failure/"; RuntimeMoment = "ordinary route, timer, or score failure"; Priority = "P0" },
    [pscustomobject]@{ Field = "failureBossClip"; AssetName = "SFX_Failure_Boss"; Category = "Failure"; Folder = "Assets/Audio/SFX/Failure/"; RuntimeMoment = "boss-phase failure"; Priority = "P0" }
)

$catalogFields = @($slotCatalog | ForEach-Object { $_.Field } | Sort-Object -Unique)
$missingFromCatalog = @($runtimeSlots | Where-Object { $catalogFields -notcontains $_ })
$missingFromRuntime = @($catalogFields | Where-Object { $runtimeSlots -notcontains $_ })

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Audio Resource Assignment Checklist")
$lines.Add("")
$lines.Add(('Generated from: `{0}`' -f $feedbackPath))
$lines.Add("")
$lines.Add('Purpose: assign temporary or final clips to the current `FeedbackSystem` slots before judging rhythm feel.')
$lines.Add("")
$lines.Add("## Validation")
$lines.Add("- Runtime audio slots found: $($runtimeSlots.Count)")
$lines.Add("- Catalog slots tracked: $($catalogFields.Count)")
$lines.Add("- Missing from catalog: $(if ($missingFromCatalog.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingFromCatalog) })")
$lines.Add("- Missing from runtime: $(if ($missingFromRuntime.Count -eq 0) { 'none' } else { [string]::Join(', ', $missingFromRuntime) })")
$lines.Add("")
$lines.Add("## Assignment Pass Order")
$lines.Add("1. Assign route and failure beats first: they carry the current rhythm problem.")
$lines.Add("2. Assign hit and break weight next: they carry destruction pleasure.")
$lines.Add("3. Assign boss and level-up beats after that: they carry climax and progression payoff.")
$lines.Add("4. Use short temporary clips if final assets do not exist yet; do not leave critical beats silent.")
$lines.Add("")
$lines.Add("## Current FeedbackSystem Audio Slots")
$lines.Add("| Priority | Category | Field | Suggested asset name | Folder | Runtime moment | Assigned? | Notes |")
$lines.Add("|---|---|---|---|---|---|---|---|")
foreach ($slot in $slotCatalog) {
    $present = if ($runtimeSlots -contains $slot.Field) { "[ ]" } else { "MISSING" }
    $lines.Add(('| {0} | {1} | `{2}` | `{3}` | `{4}` | {5} | {6} |  |' -f $slot.Priority, $slot.Category, $slot.Field, $slot.AssetName, $slot.Folder, $slot.RuntimeMoment, $present))
}

$lines.Add("")
$lines.Add("## Review Notes")
$lines.Add("- Ordinary failure and boss-phase failure should not use the same pitch, tail, or emotional color.")
$lines.Add("- Route open should feel like a turn signal; route bonus should feel like permission to smash.")
$lines.Add("- Big collapse and boss down should occupy different weight classes.")
$lines.Add("- After assigning clips, run a Stage 1 / 4 / 7 sweep and record whether the audio clarifies opener -> pivot -> sustain -> payoff -> climax.")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $ReportPath -Value $report -Encoding UTF8
Write-Output "Audio resource assignment checklist written: $ReportPath"

if ($missingFromCatalog.Count -gt 0 -or $missingFromRuntime.Count -gt 0) {
    exit 1
}

exit 0
