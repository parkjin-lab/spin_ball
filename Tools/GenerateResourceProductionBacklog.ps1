[CmdletBinding()]
param(
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

function Convert-TableRow {
    param(
        [string]$Line
    )

    if ($Line -notmatch '^\|\s*P[0-9]\s*\|') {
        return $null
    }

    $cells = @($Line.Trim("|").Split("|") | ForEach-Object { $_.Trim().Trim('`') })
    if ($cells.Count -lt 4) {
        return $null
    }

    return $cells
}

$projectRoot = Resolve-ProjectRoot
$resolvedReportPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ReportPath -RelativePath "Logs\AlienCrusherResourceProductionBacklog.md"
$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$checklists = @(
    [pscustomobject]@{ Label = "Audio"; Path = Join-Path $projectRoot "Logs\AlienCrusherAudioResourceAssignmentChecklist.md"; Why = "sound carries route, failure, destruction, and boss rhythm"; ItemIndex = 2; ContextIndex = 3; FolderIndex = 4 },
    [pscustomobject]@{ Label = "Form Identity"; Path = Join-Path $projectRoot "Logs\AlienCrusherFormIdentityProductionChecklist.md"; Why = "forms should explain player capability and upgrade desire"; ItemIndex = 1; ContextIndex = 3; FolderIndex = -1 },
    [pscustomobject]@{ Label = "Destruction Readability"; Path = Join-Path $projectRoot "Logs\AlienCrusherDestructionReadabilityChecklist.md"; Why = "materials and break states make smash decisions readable"; ItemIndex = 2; ContextIndex = 3; FolderIndex = 5 },
    [pscustomobject]@{ Label = "Street Props"; Path = Join-Path $projectRoot "Logs\AlienCrusherStreetPropVarietyChecklist.md"; Why = "props shape route reading and chain density"; ItemIndex = 2; ContextIndex = 3; FolderIndex = 5 },
    [pscustomobject]@{ Label = "UI Icons"; Path = Join-Path $projectRoot "Logs\AlienCrusherUiIconStatusChecklist.md"; Why = "icons reduce HUD reading cost on mobile"; ItemIndex = 1; ContextIndex = 2; FolderIndex = 4 },
    [pscustomobject]@{ Label = "Boss Identity"; Path = Join-Path $projectRoot "Logs\AlienCrusherBossIdentityProductionChecklist.md"; Why = "boss assets clarify climax pressure and release"; ItemIndex = 2; ContextIndex = 3; FolderIndex = 5 },
    [pscustomobject]@{ Label = "District Palette"; Path = Join-Path $projectRoot "Logs\AlienCrusherDistrictPaletteProductionChecklist.md"; Why = "district palettes make stage rhythm changes legible"; ItemIndex = 2; ContextIndex = 3; FolderIndex = 5 },
    [pscustomobject]@{ Label = "Outgame Progression"; Path = Join-Path $projectRoot "Logs\AlienCrusherOutgameProgressionChecklist.md"; Why = "result and upgrade feedback connect runs to growth"; ItemIndex = 2; ContextIndex = 3; FolderIndex = 5 },
    [pscustomobject]@{ Label = "Route Payoff"; Path = Join-Path $projectRoot "Logs\AlienCrusherRoutePayoffLayoutChecklist.md"; Why = "payoff layouts make ROUTE BONUS feel earned"; ItemIndex = 2; ContextIndex = 3; FolderIndex = 6 }
)

$items = [System.Collections.Generic.List[object]]::new()
$missing = [System.Collections.Generic.List[string]]::new()
$markdownTick = [char]96

foreach ($checklist in $checklists) {
    if (-not (Test-Path -Path $checklist.Path -PathType Leaf)) {
        $missing.Add($checklist.Label)
        continue
    }

    $lines = Get-Content -Path $checklist.Path
    foreach ($line in $lines) {
        $cells = Convert-TableRow -Line $line
        if ($null -eq $cells) {
            continue
        }

        $asset = if ($checklist.ItemIndex -ge 0 -and $cells.Count -gt $checklist.ItemIndex) { $cells[$checklist.ItemIndex] } else { $cells[1] }
        $runtimeMoment = if ($checklist.ContextIndex -ge 0 -and $cells.Count -gt $checklist.ContextIndex) { $cells[$checklist.ContextIndex] } else { "" }
        $folder = if ($checklist.FolderIndex -ge 0 -and $cells.Count -gt $checklist.FolderIndex) { $cells[$checklist.FolderIndex] } else { "" }

        $items.Add([pscustomobject]@{
            Priority = $cells[0]
            Source = $checklist.Label
            Item = $asset
            Context = $runtimeMoment
            Folder = $folder
            Why = $checklist.Why
        })
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Resource Production Backlog")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$lines.Add("Project: $projectRoot")
$lines.Add("")
$lines.Add("## Purpose")
$lines.Add("This report merges generated production checklists into one resource backlog that agents can advance without changing gameplay tuning.")
$lines.Add("")
$lines.Add("## Production Priority Policy")
$lines.Add("- P0 route, failure, destruction, boss, district, and payoff readability assets come before broad polish.")
$lines.Add("- Temporary assets are acceptable when they help test rhythm, but tuning remains locked until Evidence Green.")
$lines.Add("- Prefer assets that clarify what to smash, what to chase, what changed, or what was earned.")
$lines.Add("")
$lines.Add("## Source Checklist Status")
foreach ($checklist in $checklists) {
    $status = if (Test-Path -Path $checklist.Path -PathType Leaf) { "present" } else { "missing" }
    $lines.Add("- $($checklist.Label): $status")
}
$lines.Add("")

foreach ($priority in @("P0", "P1", "P2")) {
    $priorityItems = @($items | Where-Object { $_.Priority -eq $priority })
    $lines.Add("## $priority Backlog")
    if ($priorityItems.Count -eq 0) {
        $lines.Add("- none")
    }
    else {
        foreach ($item in $priorityItems) {
            $context = if ([string]::IsNullOrWhiteSpace($item.Context)) { "" } else { " - $($item.Context)" }
            $folder = if ([string]::IsNullOrWhiteSpace($item.Folder) -or $item.Folder -eq "[ ]") { "" } else { " ($markdownTick$($item.Folder)$markdownTick)" }
            $lines.Add("- [$($item.Source)] $($item.Item)$context$folder")
        }
    }
    $lines.Add("")
}

$lines.Add("## Recommended Autonomous Order")
$lines.Add("1. Audio route/failure/boss slots, because silence hides rhythm.")
$lines.Add("2. Route payoff layout markers, because ROUTE BONUS must read as earned city opening.")
$lines.Add("3. Boss identity materials/VFX, because Stage 4+ needs a distinct climax language.")
$lines.Add("4. District palette and prop silhouettes, because later stages must differ by rhythm problem.")
$lines.Add("5. UI/status icons, because mobile HUD text should shrink toward fast recognition.")
$lines.Add("")
$lines.Add("## Missing Inputs")
if ($missing.Count -eq 0) {
    $lines.Add("- none")
}
else {
    foreach ($label in $missing) {
        $lines.Add("- $label checklist")
    }
}
$lines.Add("")
$lines.Add("## Result")
$lines.Add("Result: resource production backlog generated with $($items.Count) item(s)")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
Write-Output $report

exit 0
