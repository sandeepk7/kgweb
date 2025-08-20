@echo off
echo Registering Native Messaging Host (User Level)...
echo This script will register the native messaging host for both Chrome and Edge
echo No administrator privileges required.
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0register-native-host-user.ps1"
echo.
echo Registration complete! Press any key to exit.
pause >nul
