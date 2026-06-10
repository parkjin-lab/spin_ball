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

function Get-OwnershipLane {
    param(
        [string]$FileName
    )

    switch -Regex ($FileName) {
        "PlaytestTelemetry" { return "Telemetry and evidence" }
        "RuntimeMapFallback|Traffic|AlleyLines" { return "Map, traffic, and city props" }
        "ProgressionCore|StageFlow|Lifecycle" { return "Stage route and run loop" }
        "UIFlow|UpgradeUI|MetaProgression|FormFlow" { return "HUD, lobby, and outgame UI" }
        "StageEncounter" { return "Boss and stage encounter" }
        "ActiveSkills|ComboOverdrive" { return "Player ability and destruction rhythm" }
        default { return "Core shared controller state" }
    }
}

function Get-ExtractionRisk {
    param(
        [int]$LineCount,
        [int]$MethodCount,
        [int]$SerializedFieldCount
    )

    if ($LineCount -ge 900 -or $MethodCount -ge 35) {
        return "High"
    }

    if ($LineCount -ge 450 -or $MethodCount -ge 18 -or $SerializedFieldCount -ge 10) {
        return "Medium"
    }

    return "Low"
}

function Get-RiskRank {
    param(
        [string]$Risk
    )

    switch ($Risk) {
        "High" { return 0 }
        "Medium" { return 1 }
        default { return 2 }
    }
}

$projectRoot = Resolve-ProjectRoot
$resolvedReportPath = Resolve-ProjectPath -ProjectRoot $projectRoot -OverridePath $ReportPath -RelativePath "Logs\AlienCrusherArchitectureExtractionPlan.md"
$reportDirectory = Split-Path -Parent $resolvedReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$systemsRoot = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems"
$files = @(Get-ChildItem -Path $systemsRoot -Filter "DummyFlowController*.cs" -File | Sort-Object Name)
$records = [System.Collections.Generic.List[object]]::new()
$markdownTick = [char]96

foreach ($file in $files) {
    $text = Get-Content -Path $file.FullName -Raw
    $lineCount = @($text -split "`r?`n").Count
    $methodMatches = [regex]::Matches($text, "(?m)^\s*(private|public|protected|internal)\s+(static\s+)?[\w<>\[\],]+\s+\w+\s*\(")
    $serializedFieldMatches = [regex]::Matches($text, "\[SerializeField\]")
    $lane = Get-OwnershipLane -FileName $file.Name
    $risk = Get-ExtractionRisk -LineCount $lineCount -MethodCount $methodMatches.Count -SerializedFieldCount $serializedFieldMatches.Count
    $riskRank = Get-RiskRank -Risk $risk

    $records.Add([pscustomobject]@{
        FileName = $file.Name
        RelativePath = $file.FullName.Substring($projectRoot.Length + 1)
        Lines = $lineCount
        Methods = $methodMatches.Count
        SerializedFields = $serializedFieldMatches.Count
        Lane = $lane
        Risk = $risk
        RiskRank = $riskRank
    })
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Alien Crusher Architecture Extraction Plan")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$lines.Add("Project: $projectRoot")
$lines.Add("")
$lines.Add("## Purpose")
$lines.Add("This report maps `DummyFlowController` partial ownership before any behavior refactor. It is safe to generate before Evidence Green because it does not change gameplay.")
$lines.Add("")
$lines.Add("## Guardrail")
$lines.Add("- Do not extract or retune route, payoff, boss, or stage rhythm behavior before Evidence Green.")
$lines.Add("- Use this report to plan seams, tests, and future pull-apart order only.")
$lines.Add("- Prefer extracting pure reporting, telemetry, and UI formatting before gameplay timing.")
$lines.Add("")
$lines.Add("## Partial Surface Summary")
$lines.Add("| File | Lane | Lines | Methods | Serialized fields | Risk |")
$lines.Add("|---|---|---:|---:|---:|---|")
foreach ($record in ($records | Sort-Object @{ Expression = "RiskRank"; Descending = $false }, @{ Expression = "Lines"; Descending = $true })) {
    $lines.Add("| $markdownTick$($record.FileName)$markdownTick | $($record.Lane) | $($record.Lines) | $($record.Methods) | $($record.SerializedFields) | $($record.Risk) |")
}
$lines.Add("")

$highRisk = @($records | Where-Object { $_.Risk -eq "High" } | Sort-Object Lines -Descending)
$mediumRisk = @($records | Where-Object { $_.Risk -eq "Medium" } | Sort-Object Lines -Descending)

$lines.Add("## High-Risk Surfaces")
if ($highRisk.Count -eq 0) {
    $lines.Add("- none")
}
else {
    foreach ($record in $highRisk) {
        $lines.Add("- $markdownTick$($record.RelativePath)$($markdownTick): $($record.Lane), $($record.Lines) lines, $($record.Methods) methods.")
    }
}
$lines.Add("")

$lines.Add("## Recommended Extraction Order")
$lines.Add("1. Telemetry/reporting helpers: lowest gameplay risk and easiest regression coverage.")
$lines.Add("2. HUD route indicator and formatting helpers: improves mobile readability ownership without changing numbers.")
$lines.Add("3. Runtime map audit/debug helpers: separates generation diagnostics from live stage flow.")
$lines.Add("4. Stage route state machine: only after Evidence Green identifies which route behavior must remain stable.")
$lines.Add("5. Boss encounter rhythm: only after Stage 4/7 evidence confirms the climax problem.")
$lines.Add("6. Active skills and combo/overdrive: last, because they touch core destruction feel.")
$lines.Add("")

$lines.Add("## Medium-Risk Watch List")
if ($mediumRisk.Count -eq 0) {
    $lines.Add("- none")
}
else {
    foreach ($record in $mediumRisk) {
        $lines.Add("- $markdownTick$($record.RelativePath)$($markdownTick): consider adding focused tests or report coverage before modifying.")
    }
}
$lines.Add("")

$lines.Add("## Result")
$lines.Add("Result: architecture extraction plan generated for $($records.Count) DummyFlowController partial file(s)")

$report = [string]::Join([Environment]::NewLine, $lines) + [Environment]::NewLine
Set-Content -Path $resolvedReportPath -Value $report -Encoding UTF8
Write-Output $report

exit 0
