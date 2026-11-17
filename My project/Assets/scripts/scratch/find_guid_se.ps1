$seFiles = Get-ChildItem -Path '../resources/SE' | Where-Object { $_.Extension -match 'wav|mp3' }

$guidMap = @{}
foreach ($se in $seFiles) {
    $metaPath = $se.FullName + ".meta"
    if (Test-Path $metaPath) {
        $metaContent = Get-Content $metaPath -Raw
        if ($metaContent -match 'guid:\s*([a-f0-9]{32})') {
            $guidMap[$Matches[1]] = $se.Name
        }
    }
}

# シーンとプレハブ、アセットをチェック
$unityFiles = Get-ChildItem -Path '../../' -Recurse -Include '*.unity','*.prefab','*.asset' | Where-Object { $_.FullName -notmatch 'Library|Temp' }

$assetReferences = @{}

foreach ($f in $unityFiles) {
    $content = Get-Content $f.FullName -Raw
    foreach ($guid in $guidMap.Keys) {
        if ($content.Contains($guid)) {
            $seName = $guidMap[$guid]
            if (-not $assetReferences.ContainsKey($seName)) {
                $assetReferences[$seName] = @()
            }
            $assetReferences[$seName] += $f.Name
        }
    }
}

Write-Host "=== シーン/プレハブ/ScriptableObjectで直接参照されているSE ==="
foreach ($key in $assetReferences.Keys | Sort-Object) {
    Write-Host "$key  <-  ($($assetReferences[$key] -join ', '))"
}
