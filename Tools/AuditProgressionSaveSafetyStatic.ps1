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
$validatorPath = Join-Path $projectRoot "Assets\Scripts\Editor\ProgressionSaveTransactionValidator.cs"
$batchChecksPath = Join-Path $projectRoot "Tools\RunUnityBatchChecks.ps1"

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

foreach ($path in @($saveSystemPath, $formUnlockPath, $dataPath, $validatorPath, $batchChecksPath)) {
    if (-not (Test-Path -Path $path -PathType Leaf)) {
        $errors.Add("Required source not found: $path")
    }
}

if ($errors.Count -eq 0) {
    $saveSystemText = Get-Content -Path $saveSystemPath -Raw
    $formUnlockText = Get-Content -Path $formUnlockPath -Raw
    $dataText = Get-Content -Path $dataPath -Raw
    $validatorText = Get-Content -Path $validatorPath -Raw
    $batchChecksText = Get-Content -Path $batchChecksPath -Raw

    foreach ($needle in @(
        'aliencrusher_progression.json',
        'aliencrusher_progression.bak.json',
        'aliencrusher_progression.corrupt.json',
        'SetValidationStorageDirectory(string directory)',
        'return validationStorageDirectory',
        'TryLoadFromDisk(out var loadedFromBackup)',
        'TrySave(preserveExistingBackup: true)',
        'public bool TryCommit(Action<PlayerProgressionData> mutation)',
        'var snapshotJson = JsonUtility.ToJson(Current)',
        'Current = JsonUtility.FromJson<PlayerProgressionData>(snapshotJson) ?? CreateDefault()',
        'WriteAndFlushTempFile(tempPath, json)',
        'stream.Flush(flushToDisk: true)',
        'TryLoadProgressionFile(tempPath)',
        'File.Replace(tempPath, SavePath, replacementBackupPath, ignoreMetadataErrors: true)',
        'File.Move(tempPath, SavePath, overwrite: true)',
        'var replacementBackupPath = preserveExistingBackup ? CorruptPath : BackupPath',
        'TryLoadProgressionFile(SavePath)',
        'TryLoadProgressionFile(BackupPath)',
        'catch (IOException)',
        'catch (System.Exception exception) when',
        'private static bool Sanitize',
        'data.meta.dpBalance = Mathf.Max(0, data.meta.dpBalance)',
        'data.meta.selectedForm = Mathf.Clamp(data.meta.selectedForm, DefaultFormIndex, MaxKnownFormIndex)',
        'private const int DefaultFormIndex = (int)FormType.Sphere',
        'private const int MaxKnownFormIndex = (int)FormType.Crusher',
        'SanitizeUnlockedForms(data.meta)',
        'SanitizeMetaUpgradeLevels(data.meta)',
        'meta.unlockedForms.Add(DefaultFormIndex)',
        'meta.selectedForm = DefaultFormIndex',
        'var seenUpgradeIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal)',
        'entry.upgradeId = entry.upgradeId.Trim()',
        'if (!seenUpgradeIds.Add(entry.upgradeId))',
        'entry.level = Mathf.Max(0, entry.level)',
        'data.stage.highestStageReached = Mathf.Max(1, data.stage.highestStageReached)',
        'data.stage.highestStageCleared = Mathf.Clamp(data.stage.highestStageCleared, 0, Mathf.Max(0, data.stage.highestStageReached - 1))',
        'data.stage.currentLobbyStage = Mathf.Clamp(data.stage.currentLobbyStage, 1, data.stage.highestStageReached)'
    )) {
        Add-MissingTextCheck -Errors $errors -Text $saveSystemText -Needle $needle -Label "ProgressionSaveSystem"
    }

    foreach ($needle in @(
        'ResolveSaveSystem()',
        '[SerializeField] private ProgressionSaveSystem progressionSaveSystem',
        'ResolveSystemsRoot()',
        'transform.parent',
        'gameObject.scene.GetRootGameObjects()',
        'systemsRoot.GetComponentInChildren<ProgressionSaveSystem>(includeInactive: true)',
        'progressionSaveSystem.LoadOrCreate()',
        'TryCommitProgression(MigrateFromLegacyPlayerPrefs)',
        'progressionSaveSystem.TryCommit(mutation)',
        'data.meta.dpBalance = balance - requiredCost',
        'SetMetaUpgradeLevel(data.meta, upgradeType, next)',
        'data.meta.unlockedForms.Add((int)form)',
        'data.meta.selectedForm = (int)form',
        'RegisterClearedStage',
        'SetCurrentLobbyStage'
    )) {
        Add-MissingTextCheck -Errors $errors -Text $formUnlockText -Needle $needle -Label "FormUnlockSystem save bridge"
    }

    if ($formUnlockText.Contains('TrySpendDp(requiredCost)')) {
        $errors.Add("Purchase paths must not save DP separately before granting the purchased item")
    }
    if ($formUnlockText.Contains('GameObject.Find("_Systems")') -or
        $formUnlockText.Contains('FindFirstObjectByType<ProgressionSaveSystem>()')) {
        $errors.Add("FormUnlockSystem must resolve progression storage from an explicit reference or its canonical _Systems root")
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

    foreach ($needle in @(
        'public static void ValidateBatch()',
        'Atomic commit persists DP and unlock together',
        'Failed save rolls back the complete in-memory mutation',
        'Corrupt primary recovers from backup without consuming it',
        'system.TryCommit(data =>',
        'ReadProgression(backupPath)',
        'DeleteValidationRunDirectory(runRoot, validationParent)',
        'AlienCrusherProgressionSaveTransactionValidation.log'
    )) {
        Add-MissingTextCheck -Errors $errors -Text $validatorText -Needle $needle -Label "Progression save transaction validator"
    }

    foreach ($needle in @(
        'AlienCrusher.EditorTools.ProgressionSaveTransactionValidator.ValidateBatch',
        'AlienCrusherProgressionSaveTransactionValidationEditor.log',
        'AlienCrusherProgressionSaveTransactionValidation.log'
    )) {
        Add-MissingTextCheck -Errors $errors -Text $batchChecksText -Needle $needle -Label "Unity batch check wiring"
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][ProgressionSaveSafetyStaticAudit] Progression save safety audit")
$lines.Add("Save system: $saveSystemPath")
$lines.Add("Form unlock bridge: $formUnlockPath")
$lines.Add("Progression data: $dataPath")
$lines.Add("Transaction validator: $validatorPath")
$lines.Add("Contract: progression mutations roll back on commit failure; temp JSON is flushed and validated before atomic replacement; backup recovery preserves the known-good backup while replacing a corrupt primary.")

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
