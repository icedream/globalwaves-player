@echo off

:: Check git ::
echo * Checking if git is accessible...
git --version >NUL 2>NUL
if %errorlevel% neq 0 goto E
exit /B 0

:E
echo [!] Can't access git. Make sure you have it installed and integrated into the PATH variable.
echo [!] For more infos, google "Expanding PATH on Windows."
exit /B -1
