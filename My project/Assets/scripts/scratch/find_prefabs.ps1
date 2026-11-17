Get-ChildItem -Path '../../' -Recurse -Filter '*.prefab' | Where-Object { $_.Name -match 'Enemy|Mob|Boss' } | Select-Object -First 20 -ExpandProperty FullName
