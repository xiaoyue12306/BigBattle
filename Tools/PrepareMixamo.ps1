param([string]$Archive)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
if (-not $Archive) { $Archive = Join-Path $taskRoot 'Artifacts\MixamoInbox\Sword and Shield Pack.zip' }
$taskExtract = Join-Path $taskRoot 'Artifacts\MixamoSource'
$taskImport = Join-Path $taskRoot 'Assets\LocalMixamoSource'
New-Item -ItemType Directory -Path $taskExtract,$taskImport -Force | Out-Null
Expand-Archive -LiteralPath $Archive -DestinationPath $taskExtract -Force
$taskNames = @('sword and shield idle','sword and shield walk','sword and shield run','sword and shield slash','sword and shield slash (3)','sword and shield attack (2)','sword and shield attack (4)','sword and shield casting (2)','sword and shield crouching (3)','sword and shield slash (4)','sword and shield impact (2)','sword and shield death','sword and shield power up')
foreach ($taskName in $taskNames) {
    $taskSource = Join-Path $taskExtract ($taskName + '.fbx')
    if (-not (Test-Path -LiteralPath $taskSource)) { throw "Missing Mixamo animation: $taskName" }
    Copy-Item -LiteralPath $taskSource -Destination $taskImport -Force
}
Write-Output "Prepared $($taskNames.Count) local Mixamo clips. Raw FBX and baked motion assets are excluded from public Git."
