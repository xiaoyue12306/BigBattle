param([string]$Editor = 'E:\Unity\2022.3.62t11\Editor\Tuanjie.exe')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskArtifact = Join-Path $taskRoot 'Artifacts'
New-Item -ItemType Directory -Path $taskArtifact -Force | Out-Null
if (-not (Test-Path -LiteralPath $Editor)) { throw "Editor not found: $Editor" }
$taskArguments = @('-batchmode','-quit','-projectPath',('"' + $taskRoot + '"'),'-executeMethod','BigBattle.Editor.ProjectBuilder.Build','-logFile',('"' + (Join-Path $taskArtifact 'editor-build.log') + '"'))
$taskProcess = Start-Process -FilePath $Editor -ArgumentList $taskArguments -WindowStyle Hidden -PassThru -Wait
if ($taskProcess.ExitCode -ne 0) { throw "Build failed ($($taskProcess.ExitCode)); see Artifacts/editor-build.log" }
Write-Output (Join-Path $taskRoot 'Builds\Windows\BigBattle.exe')
