@echo off
setlocal
cd /d "%~dp0"

echo Starting Tripo + BattleTalent workflow setup...
echo.

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Setup-TripoWorkflow.ps1" %*
set "SETUP_EXIT=%ERRORLEVEL%"

echo.
if "%SETUP_EXIT%"=="0" (
    echo Setup completed successfully.
) else (
    echo Setup failed with exit code %SETUP_EXIT%.
)

pause
exit /b %SETUP_EXIT%
