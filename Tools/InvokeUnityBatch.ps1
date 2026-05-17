[CmdletBinding()]
param(
    [string]$UnityPath = "",
    [string]$ProjectPath = "",
    [Parameter(Mandatory = $true)]
    [string]$ExecuteMethod,
    [string]$EditorLogPath = "",
    [string]$ExpectedReportPath = "",
    [int]$TimeoutSeconds = 900,
    [switch]$UseGraphics,
    [switch]$ClearStaleUnityLock
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-ProjectRoot {
    param([string]$OverridePath)

    if (-not [string]::IsNullOrWhiteSpace($OverridePath)) {
        return (Resolve-Path -Path $OverridePath).Path
    }

    return Split-Path -Parent $PSScriptRoot
}

function Resolve-ProjectFilePath {
    param([string]$ProjectRoot, [string]$Path, [string]$FallbackRelativePath)

    $candidate = $Path
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = Join-Path $ProjectRoot $FallbackRelativePath
    }
    elseif (-not [System.IO.Path]::IsPathRooted($candidate)) {
        $candidate = Join-Path $ProjectRoot $candidate
    }

    return $candidate
}

function Resolve-UnityEditorPath {
    param(
        [string]$ProjectRoot,
        [string]$OverridePath
    )

    if (-not [string]::IsNullOrWhiteSpace($OverridePath)) {
        return $OverridePath
    }

    $projectVersionPath = Join-Path $ProjectRoot "ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path -Path $projectVersionPath -PathType Leaf)) {
        return "D:\Unity\Editor\Unity.exe"
    }

    $versionLine = Get-Content -Path $projectVersionPath | Where-Object { $_ -match '^m_EditorVersion:\s*(?<version>\S+)' } | Select-Object -First 1
    if ($null -eq $versionLine) {
        return "D:\Unity\Editor\Unity.exe"
    }

    $version = ([regex]::Match($versionLine, '^m_EditorVersion:\s*(?<version>\S+)')).Groups['version'].Value
    return Join-Path (Join-Path (Join-Path "D:\Unity" $version) "Editor") "Unity.exe"
}

function Resolve-DefaultReportPath {
    param([string]$ProjectRoot, [string]$MethodName)

    if ($MethodName -like "*AlienCrusherSceneValidator*") {
        return Join-Path $ProjectRoot "Logs\AlienCrusherSceneValidation.log"
    }

    if ($MethodName -like "*AlienCrusherMapLayoutAuditor*") {
        return Join-Path $ProjectRoot "Logs\AlienCrusherMapLayoutAudit.log"
    }

    return ""
}

function Get-UnityEditorProcesses {
    return @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
        $_.ProcessName -eq "Unity" -or $_.ProcessName -eq "Unity Editor"
    })
}

function Format-ProcessList {
    param($Processes)

    if ($Processes.Count -eq 0) {
        return "none"
    }

    $parts = [System.Collections.Generic.List[string]]::new()
    foreach ($process in $Processes) {
        $parts.Add(("{0}:{1}" -f $process.ProcessName, $process.Id))
    }

    return [string]::Join(", ", $parts)
}

function Get-FileTimestamp {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -Path $Path -PathType Leaf)) {
        return $null
    }

    return (Get-Item -Path $Path).LastWriteTimeUtc
}

function Write-CapturedOutput {
    param([string]$Path)

    if (-not (Test-Path -Path $Path -PathType Leaf)) {
        return
    }

    $content = Get-Content -Path $Path -Raw
    if (-not [string]::IsNullOrWhiteSpace($content)) {
        Write-Output $content.TrimEnd()
    }
}

function ConvertTo-ProcessArgument {
    param([string]$Value)

    if ($Value -notmatch '[\s"]') {
        return $Value
    }

    return '"' + ($Value -replace '"', '\"') + '"'
}

$projectRoot = Resolve-ProjectRoot -OverridePath $ProjectPath
$UnityPath = Resolve-UnityEditorPath -ProjectRoot $projectRoot -OverridePath $UnityPath
$logsPath = Join-Path $projectRoot "Logs"
New-Item -ItemType Directory -Path $logsPath -Force | Out-Null

if (-not (Test-Path -Path $UnityPath -PathType Leaf)) {
    Write-Output "ERROR: Unity executable not found: $UnityPath"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($EditorLogPath)) {
    $safeMethodName = ($ExecuteMethod -replace "[^A-Za-z0-9_.-]", "_")
    $EditorLogPath = Join-Path $logsPath ("UnityBatch_{0}_{1}.log" -f $safeMethodName, (Get-Date -Format "yyyyMMdd_HHmmss"))
}
elseif (-not [System.IO.Path]::IsPathRooted($EditorLogPath)) {
    $EditorLogPath = Join-Path $projectRoot $EditorLogPath
}

if ([string]::IsNullOrWhiteSpace($ExpectedReportPath)) {
    $ExpectedReportPath = Resolve-DefaultReportPath -ProjectRoot $projectRoot -MethodName $ExecuteMethod
}
elseif (-not [System.IO.Path]::IsPathRooted($ExpectedReportPath)) {
    $ExpectedReportPath = Join-Path $projectRoot $ExpectedReportPath
}

$unityProcesses = Get-UnityEditorProcesses
$lockPath = Join-Path $projectRoot "Temp\UnityLockfile"
if (Test-Path -Path $lockPath -PathType Leaf) {
    if ($unityProcesses.Count -gt 0) {
        Write-Output "ERROR: Unity lock file exists and Unity editor process is running."
        Write-Output "Lock: $lockPath"
        Write-Output "Processes: $(Format-ProcessList -Processes $unityProcesses)"
        exit 2
    }

    if (-not $ClearStaleUnityLock) {
        Write-Output "ERROR: Unity lock file exists but no Unity editor process was detected."
        Write-Output "Lock: $lockPath"
        Write-Output "Rerun with -ClearStaleUnityLock only after confirming the project is not open in Unity."
        exit 2
    }

    Remove-Item -LiteralPath $lockPath -Force
    Write-Output "Cleared stale Unity lock: $lockPath"
}

$beforeReportTimestamp = Get-FileTimestamp -Path $ExpectedReportPath
$startedAtUtc = [DateTime]::UtcNow
$stdoutPath = Join-Path $logsPath ("UnityBatch_stdout_{0}.log" -f (Get-Date -Format "yyyyMMdd_HHmmss"))
$stderrPath = Join-Path $logsPath ("UnityBatch_stderr_{0}.log" -f (Get-Date -Format "yyyyMMdd_HHmmss"))
$arguments = [System.Collections.Generic.List[string]]::new()
$arguments.Add("-batchmode")
if (-not $UseGraphics) {
    $arguments.Add("-nographics")
}
$arguments.Add("-quit")
$arguments.Add("-projectPath")
$arguments.Add($projectRoot)
$arguments.Add("-executeMethod")
$arguments.Add($ExecuteMethod)
$arguments.Add("-logFile")
$arguments.Add($EditorLogPath)

Write-Output "[AlienCrusher][UnityBatch] $ExecuteMethod"
Write-Output "Project: $projectRoot"
Write-Output "Unity: $UnityPath"
Write-Output "Editor log: $EditorLogPath"
if (-not [string]::IsNullOrWhiteSpace($ExpectedReportPath)) {
    Write-Output "Expected report: $ExpectedReportPath"
}

$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = $UnityPath
$startInfo.WorkingDirectory = $projectRoot
$startInfo.UseShellExecute = $false
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
$startInfo.Arguments = [string]::Join(" ", ($arguments | ForEach-Object { ConvertTo-ProcessArgument -Value $_ }))

$process = [System.Diagnostics.Process]::new()
$process.StartInfo = $startInfo
[void]$process.Start()
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()

if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    try {
        $process.Kill()
    }
    catch {
    Write-Output "WARN: Failed to stop timed-out Unity process: $($_.Exception.Message)"
    }

    Write-Output "ERROR: Unity batch timed out after $TimeoutSeconds second(s)."
    if (-not (Test-Path -Path $EditorLogPath -PathType Leaf)) {
        Write-Output "Editor log was not created before timeout."
    }
    Write-CapturedOutput -Path $stdoutPath
    Write-CapturedOutput -Path $stderrPath
    exit 3
}

$process.WaitForExit()
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result
Set-Content -Path $stdoutPath -Value $stdout -Encoding UTF8
Set-Content -Path $stderrPath -Value $stderr -Encoding UTF8
Write-CapturedOutput -Path $stdoutPath
Write-CapturedOutput -Path $stderrPath

$exitCode = [int]$process.ExitCode
if ($exitCode -ne 0) {
    Write-Output "ERROR: Unity exited with code $exitCode."
    exit $exitCode
}

if (-not (Test-Path -Path $EditorLogPath -PathType Leaf)) {
    Write-Output "ERROR: Unity exited successfully but did not write the editor log."
    exit 4
}

if (-not [string]::IsNullOrWhiteSpace($ExpectedReportPath)) {
    $afterReportTimestamp = Get-FileTimestamp -Path $ExpectedReportPath
    if ($null -eq $afterReportTimestamp) {
        Write-Output "ERROR: Expected report was not created: $ExpectedReportPath"
        exit 5
    }

    if ($null -ne $beforeReportTimestamp -and $afterReportTimestamp -le $beforeReportTimestamp) {
        Write-Output "ERROR: Expected report timestamp did not advance."
        Write-Output "Report: $ExpectedReportPath"
        exit 6
    }

    if ($afterReportTimestamp -lt $startedAtUtc.AddSeconds(-2)) {
        Write-Output "ERROR: Expected report is older than this Unity batch run."
        Write-Output "Report: $ExpectedReportPath"
        exit 6
    }

    $resultLine = Select-String -Path $ExpectedReportPath -Pattern "Result:" | Select-Object -Last 1
    if ($null -ne $resultLine) {
        Write-Output $resultLine.Line
    }
}

Write-Output "PASS: Unity batch completed and report/log timestamps advanced."
exit 0
