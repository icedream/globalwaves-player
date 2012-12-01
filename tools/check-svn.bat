@echo off

:: Check Subversion ::
echo Checking if Subversion is accessible... | xecho /a:f
svn --version >NUL 2>NUL
if %errorlevel% neq 0 goto E
exit /B 0

:E
echo [!] Can't access Subversion (svn). Make sure you have it installed and integrated into the PATH variable. >&2
echo [!] For more infos, google "Expanding PATH on Windows." >&2
exit /B -1
