$seFiles = Get-ChildItem -Path '../resources/SE' | Where-Object { $_.Extension -match 'wav|mp3' }

# 全スクリプト、シーン、プレハブのテキストを一度に読み込んで結合して検索
$allTextFiles = Get-ChildItem -Path '../../' -Recurse -Include '*.cs','*.unity','*.prefab' | Where-Object { $_.FullName -notmatch 'scratch' }

Write-Host "Found $($allTextFiles.Count) files to search."

$used = @()
$unused = @()

foreach ($se in $seFiles) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($se.Name)
    $metaFile = $se.FullName + ".meta"
    $guid = ""
    if (Test-Path $metaFile) {
        $metaContent = Get-Content $metaFile -Raw
        if ($metaContent -match 'guid:\s*([a-f0-9]{32})') {
            $guid = $Matches[1]
        }
    }

    $matchedFiles = @()
    foreach ($f in $allTextFiles) {
        # 同名metaやGeneratorスクリプトは除外
        if ($f.Name -eq "$($se.Name).meta" -or $f.Name -match 'TouhouShotAudioGenerator') { continue }
        
        $c = Get-Content $f.FullName -Raw
        if ($c -match [regex]::Escape($baseName) -or ($guid -ne "" -and $c -match $guid)) {
            $matchedFiles += $f.Name
        }
    }

    if ($matchedFiles.Count -gt 0) {
        $used += [PSCustomObject]@{
            FileName = $se.Name
            UsedIn = ($matchedFiles | Select-Object -Unique) -join ", "
        }
    } else {
        $unused += [PSCustomObject]@{
            FileName = $se.Name
            SizeKB = [math]::Round($se.Length / 1KB, 1)
            Date = $se.LastWriteTime.ToString("yyyy/MM/dd")
        }
    }
}

Write-Host "`n=== [USED] 採用・使用中のSE一覧 ==="
$used | Format-Table -AutoSize

Write-Host "`n=== [UNUSED] 未使用・未採用のSE一覧（削除候補） ==="
$unused | Format-Table -AutoSize
