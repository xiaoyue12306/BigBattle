param([string]$EditorData = 'E:\Unity\2022.3.62t11\Editor\Data')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskOutput = Join-Path $taskRoot 'Artifacts\SourceCheck'
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskMono = Join-Path $EditorData 'MonoBleedingEdge\bin\mono.exe'
$taskCompiler = Join-Path $EditorData 'MonoBleedingEdge\lib\mono\4.5\csc.exe'
$taskReferences = @(Get-ChildItem -LiteralPath (Join-Path $EditorData 'Managed\UnityEngine') -Filter '*.dll')
$taskReferences += Get-Item -LiteralPath (Join-Path $EditorData 'MonoBleedingEdge\lib\mono\4.8-api\Facades\netstandard.dll')
$taskReferences += @(Get-ChildItem -LiteralPath (Join-Path $EditorData 'Managed') -Filter 'UnityEditor*.dll')
$taskReferences = $taskReferences | Group-Object Name | ForEach-Object { $_.Group[0] }
$taskRuntime = @(Get-ChildItem -LiteralPath (Join-Path $taskRoot 'Assets\BigBattle\Runtime') -Filter '*.cs' -Recurse)
$taskArgs = @('/nologo','/target:library','/langversion:9.0',('/out:' + (Join-Path $taskOutput 'BigBattle.Runtime.dll')))
$taskArgs += $taskReferences | ForEach-Object { '/reference:' + $_.FullName }
$taskArgs += $taskRuntime | ForEach-Object { $_.FullName }
& $taskMono $taskCompiler @taskArgs
if ($LASTEXITCODE -ne 0) { throw 'Runtime C# source check failed.' }
$taskEditorRuntimeArgs = $taskArgs | Where-Object { -not $_.StartsWith('/out:') }
$taskEditorRuntimeArgs += @('/define:UNITY_EDITOR',('/out:' + (Join-Path $taskOutput 'BigBattle.EditorRuntime.dll')))
& $taskMono $taskCompiler @taskEditorRuntimeArgs
if ($LASTEXITCODE -ne 0) { throw 'Editor-conditional runtime C# source check failed.' }
$taskEditorSources = @(Get-ChildItem -LiteralPath (Join-Path $taskRoot 'Assets\BigBattle\Editor') -Filter '*.cs' -Recurse)
$taskEditorArgs = @('/nologo','/target:library','/langversion:9.0','/define:UNITY_EDITOR',('/out:' + (Join-Path $taskOutput 'BigBattle.Editor.dll')),('/reference:' + (Join-Path $taskOutput 'BigBattle.Runtime.dll')))
$taskEditorArgs += $taskReferences | ForEach-Object { '/reference:' + $_.FullName }
$taskEditorArgs += $taskEditorSources | ForEach-Object { $_.FullName }
& $taskMono $taskCompiler @taskEditorArgs
if ($LASTEXITCODE -ne 0) { throw 'Editor C# source check failed.' }
$taskCheckArgs = @('/nologo','/target:exe','/langversion:9.0',('/out:' + (Join-Path $taskOutput 'SourceChecks.exe')),('/reference:' + (Join-Path $taskOutput 'BigBattle.Runtime.dll')))
$taskCheckArgs += $taskReferences | ForEach-Object { '/reference:' + $_.FullName }
$taskCheckArgs += Join-Path $taskRoot 'Tools\Checks\SourceChecks.cs'
& $taskMono $taskCompiler @taskCheckArgs
if ($LASTEXITCODE -ne 0) { throw 'Source logic checks failed to compile.' }
$taskPreviousMonoPath = $env:MONO_PATH
try {
    $env:MONO_PATH = (Join-Path $EditorData 'Managed\UnityEngine') + ';' + $taskOutput
    & $taskMono (Join-Path $taskOutput 'SourceChecks.exe')
    if ($LASTEXITCODE -ne 0) { throw 'Source logic checks failed.' }
} finally { $env:MONO_PATH = $taskPreviousMonoPath }
Write-Output 'SOURCE_CHECK_PASS (C# compile only; not an editor build or runtime verification)'
