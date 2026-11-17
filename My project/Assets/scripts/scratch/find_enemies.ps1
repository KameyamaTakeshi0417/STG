Get-ChildItem -Path '../../' -Recurse -Filter '*.asset' | Where-Object { $_.FullName -match 'Sequence|Wave|Enemy|Stage' } | Select-Object -First 25 -ExpandProperty FullName
