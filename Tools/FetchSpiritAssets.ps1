$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskCache=Join-Path $taskRoot 'Artifacts/SpiritAssets'
$taskManifest=Get-Content -LiteralPath (Join-Path $taskRoot 'AssetLibrary/VFX/SpiritAnimations/download-report.json') -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Path $taskCache -Force | Out-Null
foreach($taskSource in $taskManifest.archives){
    $taskName=[System.IO.Path]::GetFileName(([uri]$taskSource.download).AbsolutePath)
    $taskFile=Join-Path $taskCache $taskName
    if(-not(Test-Path -LiteralPath $taskFile)){Invoke-WebRequest -Uri $taskSource.download -OutFile $taskFile}
    if((Get-FileHash -LiteralPath $taskFile -Algorithm SHA256).Hash.ToLowerInvariant() -ne $taskSource.sha256){throw "Publisher archive changed: inspect source and license before updating $taskName"}
}
Expand-Archive -LiteralPath (Join-Path $taskCache 'Arcane_Effect.zip') -DestinationPath (Join-Path $taskCache 'Arcane') -Force
Expand-Archive -LiteralPath (Join-Path $taskCache 'para_CC0_particlefx-2.zip') -DestinationPath (Join-Path $taskCache 'Para') -Force
$taskSevenZip=Join-Path ${env:ProgramFiles} '7-Zip/7z.exe'
if(-not(Test-Path -LiteralPath $taskSevenZip)){throw '7-Zip is required only to unpack the author archive; checked-in game atlases already work without it.'}
& $taskSevenZip x (Join-Path $taskCache 'auras-001-002.7z') ('-o'+(Join-Path $taskCache 'Auras')) -y -bso0
if($LASTEXITCODE -ne 0){throw 'Aura archive extraction failed'}
Write-Output 'SPIRIT_ARCHIVES_READY; run Tools/PrepareSpiritSheets.py with Python/Pillow'
