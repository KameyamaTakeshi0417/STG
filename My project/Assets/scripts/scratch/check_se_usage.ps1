$seFiles = Get-ChildItem -Path '../resources/SE' | Where-Object { $_.Extension -match 'wav|mp3|ogg' }

# 全Assetを検索対象にする
$searchFiles = Get-ChildItem -Path '../../' -Recurse -Include '*.cs','*.unity','*.prefab','*.asset'

$results = @()

foreach ($se in $seFiles) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($se.Name)
    $pattern = [regex]::Escape($baseName)
    
    # GUIDも取得
    $metaFile = $se.FullName + ".meta"
    $guid = ""
    if (Test-Path $metaFile) {
        $metaContent = Get-Content $metaFile -Raw
        if ($metaContent -match 'guid:\s*([a-f0-9]{32})') {
            $guid = $Matches[1]
        }
    }

    $hitLocations = @()
    foreach ($file in $searchFiles) {
        # generatorやscratchスクリプトは除外
        if ($file.FullName -match 'scratch|TouhouShotAudioGenerator') { continue }

        $content = Get-Content $file.FullName -Raw
        if ($content -match $pattern -or ($guid -ne "" -and $content -match $guid)) {
            $hitLocations += $file.Name
        }
    }

    $results += [PSCustomObject]@{
        Name = $se.Name
        IsUsed = ($hitLocations.Count -gt 0)
        Locations = ($hitLocations -join ", ")
    }
}

$results | Format-Table -AutoSize
