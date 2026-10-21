$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskPlayer = Join-Path $taskRoot 'Builds\Windows\BigBattle.exe'
if (-not (Test-Path -LiteralPath $taskPlayer)) { throw 'Build the Windows player first using Tools/Build.ps1' }
$taskArguments = @('--smoke-test','-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"' + (Join-Path $taskRoot 'Artifacts\player-smoke.log') + '"'))
# This is the interactive game under visual test, not a background helper.
# Windows suspends presentation for hidden game windows, producing black PNGs.
$taskProcess = Start-Process -FilePath $taskPlayer -ArgumentList $taskArguments -WindowStyle Normal -PassThru
if (-not $taskProcess.WaitForExit(300000)) { Stop-Process -Id $taskProcess.Id; throw 'Smoke test exceeded 300 seconds; see Artifacts/player-smoke.log' }
Get-Content -LiteralPath (Join-Path $taskRoot 'Artifacts\Smoke\report.json')
if ($taskProcess.ExitCode -ne 0) { throw "Smoke test failed ($($taskProcess.ExitCode))" }
