[CmdletBinding()]
param(
    [int]$MaxStage = 10,
    [int]$MaxGrowthStage = 10,
    [string]$ReportPath = "",
    [switch]$FailOnWarnings
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot "UnityBatchProcessHelpers.ps1")

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $projectRoot "Logs\AlienCrusherUnityBatchProjectLockRegression.log"
}
elseif (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path $projectRoot $ReportPath
}

$errors = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

function Assert-Equal {
    param(
        [object]$Actual,
        [object]$Expected,
        [string]$Label
    )

    if ($Actual -ne $Expected) {
        $errors.Add("$Label expected '$Expected' but got '$Actual'")
    }
}

Assert-Equal -Actual (Get-UnityProjectPathFromCommandLine 'Unity.exe -batchmode -projectPath D:\uni\spinball -quit') -Expected 'D:\uni\spinball' -Label "Unquoted project path"
Assert-Equal -Actual (Get-UnityProjectPathFromCommandLine 'Unity.exe -batchMode -projectPath "D:/uni/spinball" -name AssetImportWorker0') -Expected 'D:/uni/spinball' -Label "Quoted worker project path"
Assert-Equal -Actual (Get-UnityProjectPathFromCommandLine 'Unity.exe -PROJECTPATH "D:\UNI\SPINBALL"') -Expected 'D:\UNI\SPINBALL' -Label "Case-insensitive option"
Assert-Equal -Actual (Get-UnityProjectPathFromCommandLine 'Unity.exe -batchmode -quit') -Expected '' -Label "Missing project path"
Assert-Equal -Actual (ConvertTo-NormalizedProjectPath 'D:\uni\spinball\') -Expected 'd:/uni/spinball' -Label "Windows path normalization"
Assert-Equal -Actual (ConvertTo-NormalizedProjectPath 'D:/UNI/spinball/') -Expected 'd:/uni/spinball' -Label "Slash and case normalization"

$records = @(
    [pscustomobject]@{
        ProcessId = 101
        CommandLine = 'Unity.exe -projectPath D:\uni\spinball -useHub'
    },
    [pscustomobject]@{
        ProcessId = 102
        CommandLine = 'Unity.exe -batchMode -projectPath "D:/uni/spinball" -name AssetImportWorker0'
    },
    [pscustomobject]@{
        ProcessId = 201
        CommandLine = 'Unity.exe -projectPath D:\uni\hypercasual\other-game -useHub'
    }
)
$matching = @(Get-UnityEditorProcessesForProject -ProjectRoot 'D:\uni\spinball' -ProcessRecords $records)
Assert-Equal -Actual $matching.Count -Expected 2 -Label "Project-specific process count"
Assert-Equal -Actual $matching[0].Id -Expected 101 -Label "Main editor retained"
Assert-Equal -Actual $matching[1].Id -Expected 102 -Label "Matching worker retained"

$reportDirectory = Split-Path -Parent $ReportPath
if (-not [string]::IsNullOrWhiteSpace($reportDirectory)) {
    New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("[AlienCrusher][UnityBatchProjectLockRegression] Project-specific Unity lock detection")
$lines.Add("Helper: $(Join-Path $PSScriptRoot 'UnityBatchProcessHelpers.ps1')")
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
