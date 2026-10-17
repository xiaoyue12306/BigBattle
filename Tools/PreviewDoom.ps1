$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskArgs=@('--doom-preview','-screen-fullscreen','0','-screen-width','1600','-screen-height','900','-logFile',('"'+(Join-Path $taskRoot 'Artifacts/player-doom-preview.log')+'"'))
# Visible game window is required for real Windows rendering capture.
$taskProcess=Start-Process -FilePath (Join-Path $taskRoot 'Builds/Windows/BigBattle.exe') -ArgumentList $taskArgs -WindowStyle Normal -PassThru
if(-not $taskProcess.WaitForExit(60000)){Stop-Process -Id $taskProcess.Id;throw 'Doom preview exceeded 60 seconds'}
if($taskProcess.ExitCode -ne 0){throw 'Doom preview failed'}
Select-String -LiteralPath (Join-Path $taskRoot 'Artifacts/player-doom-preview.log') -Pattern 'DOOM_PREVIEW_OK'
