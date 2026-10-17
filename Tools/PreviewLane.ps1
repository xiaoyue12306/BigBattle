$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskArgs=@('--lane-preview','-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $taskRoot 'Artifacts/player-lane-preview.log')+'"'))
$taskProcess=Start-Process -FilePath (Join-Path $taskRoot 'Builds/Windows/BigBattle.exe') -ArgumentList $taskArgs -WindowStyle Normal -PassThru
if(-not $taskProcess.WaitForExit(90000)){Stop-Process -Id $taskProcess.Id;throw 'Lane preview exceeded 90 seconds'}
if($taskProcess.ExitCode -ne 0){throw 'Lane preview failed'}
Select-String -LiteralPath (Join-Path $taskRoot 'Artifacts/player-lane-preview.log') -Pattern 'LANE_PREVIEW_OK'
