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

function Add-MissingTextCheck {
    param(
        [System.Collections.Generic.List[string]]$Errors,
        [string]$Text,
        [string]$Needle,
        [string]$Label
    )

    if ($Text.Contains($Needle)) {
        return
    }

    $Errors.Add("$Label missing required text: $Needle")
}

$projectRoot = Resolve-ProjectRoot
$saveSystemPath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\ProgressionSaveSystem.cs"
$formUnlockPath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\FormUnlockSystem.cs"
$dataPath = Join-Path $projectRoot "Assets\Scripts\Runtime\Systems\PlayerProgressionData.cs"

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherProgressionSaveSafetyStaticAudit.log"
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

foreach ($path in @($saveSystemPath, $formUnlockPath, $dataPath)) {
    if (-not (Test-Path -Path $path -PathType Leaf)) {
        $errors.Add("Required source not found: $path")
    }
}

if ($errors.Count -eq 0) {
    $saveSystemText = Get-Content -Path $saveSystemPath -Raw
    $formUnlockText = Get-Content -Path $formUnlockPath -Raw
    $dataText = Get-Content -Path $dataPath -Raw

    foreach ($needle in @(
        'aliencrusher_progression.json',
        'aliencrusher_progression.bak.json',
        'File.WriteAllText(tempPath, json)',
        'File.Copy(SavePath, BackupPath, true)',
        'File.Move(tempPath, SavePath)',
        'TryLoadProgressionFile(SavePath)',
        'TryLoadProgressionFile(BackupPath)',
        'catch (IOException)',
        'catch (System.Exception exception) when',
        'Sanitize(Current)',
        'data.stage.highestStageReached = Mathf.Max(1, data.stage.highestStageReached)',
        'data.stage.highestStageCleared = Mathf.Clamp'
    )) {
        Add-MissingTextCheck -Errors $errors -Text $saveSystemText -Needle $needle -Label "ProgressionSaveSystem"
    }

    foreach ($needle in @(
        'ResolveSaveSystem()',
        'progressionSaveSystem.LoadOrCreate()',
        'MigrateFromLegacyPlayerPrefs(progressionData)',
        'SaveProgression()',
        'RegisterClearedStage',
        'SetCurrentLobbyStage'
    )) {
        Add-MissingTextCheck -Errors $errors -Text $formUnlockText -Needle $needle -Label "FormUnlockSystem save bridge"
    }

    foreach ($needle in @(
        'public int schemaVersion = 1',
        'public MetaProgressionData meta = new MetaProgressionData()',
        'public StageProgressionData stage = new StageProgressionData()',
        'public int highestStageReached = 1',
        'public int currentLobbyStage = 1'
    )) {
        Add-MissingTextCheck -Errors $errors -Text $dataText -Needle $needle -Label "PlayerProgressionData defaults"
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][ProgressionSaveSafetyStaticAudit] Progression save safety audit")
$lines.Add("Save system: $saveSystemPath")
$lines.Add("Form unlock bridge: $formUnlockPath")
$lines.Add("Progression data: $dataPath")
$lines.Add("Contract: primary JSON can fail, backup JSON is still attempted, defaults are sanitized, and legacy PlayerPrefs can migrate into the JSON save.")

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
