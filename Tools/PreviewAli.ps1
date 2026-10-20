$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskArgs=@('--ali-preview','-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $taskRoot 'Artifacts/player-ali-preview.log')+'"'))
$taskProcess=Start-Process -FilePath (Join-Path $taskRoot 'Builds/Windows/BigBattle.exe') -ArgumentList $taskArgs -WindowStyle Normal -PassThru
if(-not $taskProcess.WaitForExit(60000)){Stop-Process -Id $taskProcess.Id;throw 'Ali preview exceeded 60 seconds'}
if($taskProcess.ExitCode -ne 0){throw 'Ali preview failed'}
Select-String -LiteralPath (Join-Path $taskRoot 'Artifacts/player-ali-preview.log') -Pattern 'ALI_PREVIEW_OK'
