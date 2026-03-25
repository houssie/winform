$content = Get-Content 'c:\Users\itiel\Documents\progS4\winform\point\GameService.cs'
$content = $content -replace 'SÃ©rialiserGrille', 'SerialiserGrille'
$content = $content -replace 'SérialiserGrille', 'SerialiserGrille'
Set-Content -Path 'c:\Users\itiel\Documents\progS4\winform\point\GameService.cs' -Value $content -Encoding UTF8
