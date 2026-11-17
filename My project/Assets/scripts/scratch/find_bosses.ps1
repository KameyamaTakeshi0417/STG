Get-ChildItem -Path 'Alpha/Enemy/enemyPrefs' -Recurse -Filter '*.prefab' | Select-Object -ExpandProperty FullName
