@echo off
echo ========================================
echo KGWin Protocol Registry Fix
echo ========================================
echo.
echo This script will fix the registry entries to point to the correct path.
echo You must run this as Administrator!
echo.

REM Check if running as Administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running as Administrator - proceeding...
) else (
    echo ERROR: This script must be run as Administrator!
    echo Right-click on this file and select "Run as administrator"
    pause
    exit /b 1
)

echo.
echo Removing old registry entries...
reg delete "HKEY_CLASSES_ROOT\kgwin" /f >nul 2>&1

echo Adding new registry entries with correct path...
reg add "HKEY_CLASSES_ROOT\kgwin" /ve /d "KGWin Application Protocol" /f
reg add "HKEY_CLASSES_ROOT\kgwin" /v "URL Protocol" /d "" /f
reg add "HKEY_CLASSES_ROOT\kgwin\DefaultIcon" /ve /d "\"D:\Work\Office\Damco\KloudGin\Deployed\KGWin\KGWin.exe\",0" /f
reg add "HKEY_CLASSES_ROOT\kgwin\shell\open\command" /ve /d "\"D:\Work\Office\Damco\KloudGin\Deployed\KGWin\KGWin.exe\" \"%%1\"" /f

echo.
echo Registry entries updated successfully!
echo.
echo Verifying registry entries:
reg query "HKEY_CLASSES_ROOT\kgwin" /s

echo.
echo ========================================
echo Protocol handler registration complete!
echo ========================================
echo.
echo You can now test kgwin:// URLs in your browser.
echo Test URL: kgwin://launch?assetId=ASSET_001&layerId=LAYER_001
echo.
pause
