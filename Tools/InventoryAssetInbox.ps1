param([string]$Inbox)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
if (-not $Inbox) { $Inbox = Join-Path $taskRoot 'Artifacts\AssetStoreInbox' }
if (-not (Test-Path -LiteralPath $Inbox -PathType Container)) { throw "Asset inbox does not exist: $Inbox" }
$taskInboxPath = (Resolve-Path -LiteralPath $Inbox).Path
$taskExtensions = @('.unitypackage','.zip','.wav','.ogg','.mp3','.flac')
$taskFiles = @(Get-ChildItem -LiteralPath $taskInboxPath -Recurse -File |
    Where-Object { $_.Extension.ToLowerInvariant() -in $taskExtensions } |
    Sort-Object FullName |
    ForEach-Object {
        [pscustomobject]@{
            file = $_.FullName.Substring($taskInboxPath.Length).TrimStart([char]'\',[char]'/')
            bytes = $_.Length
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            extension = $_.Extension.ToLowerInvariant()
            status = 'present_unreviewed'
        }
    })
$taskReportFolder = Join-Path $taskRoot 'Artifacts\AssetLibrary'
New-Item -ItemType Directory -Path $taskReportFolder -Force | Out-Null
$taskReport = [ordered]@{
    checkedUtc = [DateTime]::UtcNow.ToString('o')
    inbox = $taskInboxPath
    fileCount = $taskFiles.Count
    status = $(if ($taskFiles.Count -eq 0) { 'empty_no_assets_downloaded' } else { 'files_present_not_yet_verified_or_imported' })
    files = $taskFiles
}
$taskReport | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskReportFolder 'inventory.json') -Encoding UTF8
Write-Output "ASSET_INBOX_INVENTORY files=$($taskFiles.Count) status=$($taskReport.status)"
Write-Output (Join-Path $taskReportFolder 'inventory.json')
# File presence never proves an account entitlement, redistribution permission,
# complete download, successful import, or suitability for the current renderer.
