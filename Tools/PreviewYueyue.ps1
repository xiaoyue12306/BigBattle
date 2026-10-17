$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskPlayer=Join-Path $taskRoot 'Builds/Windows/BigBattle.exe'
$taskArgs=@('--yueyue-preview','-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $taskRoot 'Artifacts/player-yueyue-preview.log')+'"'))
# This visible game window is required for real rendered capture on Windows.
$taskProcess=Start-Process -FilePath $taskPlayer -ArgumentList $taskArgs -WindowStyle Normal -PassThru
if(-not $taskProcess.WaitForExit(60000)){Stop-Process -Id $taskProcess.Id;throw 'Preview exceeded 60 seconds'}
if($taskProcess.ExitCode -ne 0){throw 'Yueyue preview failed'}
Select-String -LiteralPath (Join-Path $taskRoot 'Artifacts/player-yueyue-preview.log') -Pattern 'YUEYUE_PREVIEW_OK'
