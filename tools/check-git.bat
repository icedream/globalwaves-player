@echo off

:: Check git ::
echo Checking if git is accessible... | xecho /a:f
git --version >NUL 2>NUL
if %errorlevel% neq 0 goto E
exit /B 0

:E
echo [!] Can't access git. Make sure you have it installed and integrated into the PATH variable.>&2
echo [!] For more infos, google "Expanding PATH on Windows.">&2
exit /B -1
