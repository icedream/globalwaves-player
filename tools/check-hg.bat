@echo off

:: Check Mercurial ::
echo Checking if Mercurial is accessible... | xecho /a:f
hg --version >NUL 2>NUL
if %errorlevel% neq 0 goto E
exit /B 0

:E
echo [!] Can't access Mercurial (hg). Make sure you have it installed and integrated into the PATH variable. >&2
echo [!] For more infos, google "Expanding PATH on Windows." >&2
exit /B -1
