# C# スクリプト内で参照されている "SE/..." をすべて抽出
$csFiles = Get-ChildItem -Path '.' -Recurse -Filter '*.cs' | Where-Object { $_.FullName -notmatch 'scratch|TouhouShotAudioGenerator' }

$referencedSEs = @{}

foreach ($f in $csFiles) {
    $content = Get-Content $f.FullName -Raw
    $matches = [regex]::Matches($content, 'Resources\.Load<AudioClip>\s*\(\s*["'']SE/([^"'']+)["'']\s*\)')
    foreach ($m in $matches) {
        $seName = $m.Groups[1].Value
        if (-not $referencedSEs.ContainsKey($seName)) {
            $referencedSEs[$seName] = @()
        }
        $referencedSEs[$seName] += $f.Name
    }
}

Write-Host "=== C#コード内で Resources.Load されているSE ==="
foreach ($key in $referencedSEs.Keys | Sort-Object) {
    Write-Host "$key.wav  <-  ($($referencedSEs[$key] -join ', '))"
}
