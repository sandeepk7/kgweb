@echo off
echo Registering KGWin protocol handler...
echo.

REM Register kgwin:// protocol to launch KGWin application
reg add "HKEY_CLASSES_ROOT\kgwin" /ve /d "KGWin Application Protocol" /f
reg add "HKEY_CLASSES_ROOT\kgwin" /v "URL Protocol" /d "" /f
reg add "HKEY_CLASSES_ROOT\kgwin\DefaultIcon" /ve /d "\"%~dp0KGWin.exe\",0" /f
reg add "HKEY_CLASSES_ROOT\kgwin\shell\open\command" /ve /d "\"%~dp0KGWin.exe\" \"%%1\"" /f

echo.
echo Protocol handler registered successfully!
echo You can now use kgwin:// URLs to launch the application.
echo.
pause
