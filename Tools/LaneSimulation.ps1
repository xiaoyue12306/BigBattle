$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskArgs=@('--lane-sim','-batchmode','-logFile',('"'+(Join-Path $taskRoot 'Artifacts/player-lane-sim.log')+'"'))
$taskProcess=Start-Process -FilePath (Join-Path $taskRoot 'Builds/Windows/BigBattle.exe') -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
if(-not $taskProcess.WaitForExit(180000)){Stop-Process -Id $taskProcess.Id;throw 'Lane simulation exceeded 180 seconds'}
Get-Content -LiteralPath (Join-Path $taskRoot 'Artifacts/LaneSim/report.json')
if($taskProcess.ExitCode -ne 0){throw 'Lane simulation did not reach a core victory / defeat; inspect report'}
