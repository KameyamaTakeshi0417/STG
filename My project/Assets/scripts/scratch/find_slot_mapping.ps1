Get-ChildItem -Recurse -Filter "*.cs" | Select-String -Pattern "Primer|Casing|Bullet" | Select-String -Pattern "slot|Slot|index|Index|x ==" | Select-Object -First 40 | ForEach-Object {
    Write-Output ($_.Path + ":" + $_.LineNumber + ": " + $_.Line.Trim())
}
