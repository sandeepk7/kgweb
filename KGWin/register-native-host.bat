@echo off
echo Registering KGConnect Native Messaging Host...
echo This requires administrator privileges.

powershell -ExecutionPolicy Bypass -File "%~dp0register-native-host.ps1"

echo.
echo Registration complete! Please restart your browser.
pause
