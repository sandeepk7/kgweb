# Fix KGWin Protocol Registry Entries
# Run this script as Administrator

Write-Host "Fixing KGWin protocol registry entries..." -ForegroundColor Yellow

# Remove old registry entries
Write-Host "Removing old registry entries..." -ForegroundColor Cyan
reg delete "HKEY_CLASSES_ROOT\kgwin" /f 2>$null

# Add new registry entries with correct path
Write-Host "Adding new registry entries..." -ForegroundColor Cyan
reg add "HKEY_CLASSES_ROOT\kgwin" /ve /d "KGWin Application Protocol" /f
reg add "HKEY_CLASSES_ROOT\kgwin" /v "URL Protocol" /d "" /f
reg add "HKEY_CLASSES_ROOT\kgwin\DefaultIcon" /ve /d "\"D:\Work\Office\Damco\KloudGin\Deployed\KGWin\KGWin.exe\",0" /f
reg add "HKEY_CLASSES_ROOT\kgwin\shell\open\command" /ve /d "\"D:\Work\Office\Damco\KloudGin\Deployed\KGWin\KGWin.exe\" \"%%1\"" /f

Write-Host "Registry entries updated successfully!" -ForegroundColor Green
Write-Host "New path: D:\Work\Office\Damco\KloudGin\Deployed\KGWin\KGWin.exe" -ForegroundColor Green

# Verify the entries
Write-Host "`nVerifying registry entries..." -ForegroundColor Yellow
reg query "HKEY_CLASSES_ROOT\kgwin" /s

Write-Host "`nProtocol handler registration complete!" -ForegroundColor Green
Write-Host "You can now test kgwin:// URLs in your browser." -ForegroundColor Green
