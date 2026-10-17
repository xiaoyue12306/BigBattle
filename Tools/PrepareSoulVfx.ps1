$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskCache=Join-Path $taskRoot 'Artifacts/Kenney'
New-Item -ItemType Directory -Path $taskCache -Force | Out-Null
$taskZip=Join-Path $taskCache 'kenney_particle-pack.zip'
$taskUrl='https://kenney.nl/media/pages/assets/particle-pack/f8fe0f8cb8-1677578741/kenney_particle-pack.zip'
$taskExpected='B631D4B07F7002549FDCF155F01141AD482F79F3440E4E301EED49CE5F1D8958'
if(-not(Test-Path -LiteralPath $taskZip)){Invoke-WebRequest -Uri $taskUrl -OutFile $taskZip}
if((Get-FileHash -LiteralPath $taskZip -Algorithm SHA256).Hash -ne $taskExpected){throw 'Official archive changed: inspect its contents and license before updating the pinned hash.'}
$taskUnpack=Join-Path $taskCache 'ParticlePack'
Expand-Archive -LiteralPath $taskZip -DestinationPath $taskUnpack -Force
$taskTarget=Join-Path $taskRoot 'Assets/BigBattle/Resources/SoulVfx'
New-Item -ItemType Directory -Path $taskTarget -Force | Out-Null
$taskNames=@('circle_02','fire_01','flame_01','light_02','magic_01','magic_03','smoke_04','spark_01','spark_06','star_04','twirl_02','slash_02','trace_05')
foreach($taskName in $taskNames){Copy-Item -LiteralPath (Join-Path $taskUnpack ('PNG (Transparent)/'+$taskName+'.png')) -Destination (Join-Path $taskTarget ($taskName+'.png'))}
Copy-Item -LiteralPath (Join-Path $taskUnpack 'License.txt') -Destination (Join-Path $taskTarget 'License.txt')
Write-Output ('SOUL_ASSETS_READY '+$taskNames.Count+' CC0 textures; existing .meta files preserved')
