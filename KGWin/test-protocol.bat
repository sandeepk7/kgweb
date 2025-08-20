@echo off
echo Testing KGWin protocol handler...
echo.

echo Test 1: Direct protocol call
echo Testing: kgwin://launch?assetId=TEST_ASSET&layerId=TEST_LAYER
echo.

REM Test the protocol directly
start "" "kgwin://launch?assetId=TEST_ASSET&layerId=TEST_LAYER"

echo.
echo Test 2: Check if KGWin.exe exists at expected path
if exist "D:\Work\Office\Damco\KloudGin\Deployed\KGWin\KGWin.exe" (
    echo ✅ KGWin.exe found at correct path
) else (
    echo ❌ KGWin.exe NOT found at expected path
)

echo.
echo Test 3: Check registry entries
echo Current registry entries:
reg query "HKEY_CLASSES_ROOT\kgwin" /s

echo.
echo Test complete. Check if KGWin launched with popup.
pause
