Set-StrictMode -Version Latest

function ConvertTo-NormalizedProjectPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    try {
        $fullPath = [System.IO.Path]::GetFullPath($Path)
    }
    catch {
        return ""
    }

    $normalized = $fullPath -replace '[\\/]+$', ''
    return $normalized.Replace([char]92, [char]47).ToLowerInvariant()
}

function Get-UnityProjectPathFromCommandLine {
    param([string]$CommandLine)

    if ([string]::IsNullOrWhiteSpace($CommandLine)) {
        return ""
    }

    $projectMatch = [regex]::Match(
        $CommandLine,
        '(?i)(?:-projectpath)\s+(?:"(?<quoted>[^"]+)"|(?<plain>\S+))')
    if (-not $projectMatch.Success) {
        return ""
    }

    if ($projectMatch.Groups['quoted'].Success) {
        return $projectMatch.Groups['quoted'].Value
    }

    return $projectMatch.Groups['plain'].Value
}

function Get-UnityEditorProcessesForProject {
    param(
        [string]$ProjectRoot,
        [AllowNull()]
        [object[]]$ProcessRecords = $null
    )

    $normalizedProjectRoot = ConvertTo-NormalizedProjectPath -Path $ProjectRoot
    $matchingProcesses = [System.Collections.Generic.List[object]]::new()
    if ($null -eq $ProcessRecords) {
        try {
            $ProcessRecords = @(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" -ErrorAction Stop)
        }
        catch {
            return @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
                $_.ProcessName -eq "Unity" -or $_.ProcessName -eq "Unity Editor"
            })
        }
    }

    foreach ($record in @($ProcessRecords)) {
        $candidatePath = Get-UnityProjectPathFromCommandLine -CommandLine $record.CommandLine
        if ((ConvertTo-NormalizedProjectPath -Path $candidatePath) -ne $normalizedProjectRoot) {
            continue
        }

        $matchingProcesses.Add([pscustomobject]@{
            ProcessName = "Unity"
            Id = [int]$record.ProcessId
            CommandLine = $record.CommandLine
        })
    }

    return @($matchingProcesses)
}
