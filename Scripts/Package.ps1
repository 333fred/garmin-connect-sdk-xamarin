
$rootDir = join-path $PSScriptRoot "..\"
$nugetPath = join-path $rootDir "NuGet.exe"

if (-not (Test-Path $nugetPath)) {
    mkdir $rootDir -ErrorAction SilentlyContinue | out-null
    invoke-webrequest -uri https://dist.nuget.org/win-x86-commandline/v4.6.2/nuget.exe -outfile $nugetPath
}

function Exec-CommandCore([string]$command, [string]$commandArgs, [switch]$useConsole = $true) {
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $command
    $startInfo.Arguments = $commandArgs

    $startInfo.UseShellExecute = $false
    $startInfo.WorkingDirectory = Get-Location

    if (-not $useConsole) {
       $startInfo.RedirectStandardOutput = $true
       $startInfo.CreateNoWindow = $true
    }

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    $process.Start() | Out-Null

    $finished = $false
    try {
        if (-not $useConsole) {
            # The OutputDataReceived event doesn't fire as events are sent by the
            # process in powershell.  Possibly due to subtlties of how Powershell
            # manages the thread pool that I'm not aware of.  Using blocking
            # reading here as an alternative which is fine since this blocks
            # on completion already.
            $out = $process.StandardOutput
            while (-not $out.EndOfStream) {
                $line = $out.ReadLine()
                Write-Output $line
            }
        }

        while (-not $process.WaitForExit(100)) {
            # Non-blocking loop done to allow ctr-c interrupts
        }

        $finished = $true
        if ($process.ExitCode -ne 0) {
            throw "Command failed to execute: $command $commandArgs"
        }
    }
    finally {
        # If we didn't finish then an error occured or the user hit ctrl-c.  Either
        # way kill the process
        if (-not $finished) {
            $process.Kill()
        }
    }
}

# Functions exactly like Exec-Command but lets the process re-use the current
# console. This means items like colored output will function correctly.
#
# In general this command should be used in place of
#   Exec-Command $msbuild $args | Out-Host
#
function Exec-Console([string]$command, [string]$commandArgs) {
    Exec-CommandCore -command $command -commandArgs $commandargs -useConsole:$true
}

$outDir = Join-Path $rootDir "bin\NuGet"
$csproj = Join-Path $rootDir ".\GarminConnectSDK\GarminConnectSDK.csproj"

Exec-Console $nugetPath "pack -OutputDirectory $outDir -Symbols -Prop Configuration=Release $csproj"
