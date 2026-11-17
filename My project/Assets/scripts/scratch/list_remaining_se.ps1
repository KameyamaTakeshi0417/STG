Get-ChildItem -Path '../resources/SE' | Where-Object { $_.Name -notmatch '\.meta$' } | Select-Object -ExpandProperty Name
