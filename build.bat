@echo off

path %path%;%cd%\tools

::echo Updating submodules...
::git submodule update --init

set netfx_dir_flag=0

xecho /a:f /nolf "Finding compiler..." 

:BUILD_WITH_NETFX
msbuild.exe /version >NUL 2>NUL
if %errorlevel% neq 0 if not "%netfx_dir_flag"=="0" goto FIND_NETFX
msbuild.exe /version | xecho /nolf /f:"{1:s} " | xecho /a:f /f:" .NET compiler v{4:s}"
xecho /a:f "Compiling..."
msbuild.exe "globalwaves Player.sln" /maxcpucount /toolsversion:4.0 /verbosity:m /nologo /p:Configuration=Release
msbuild.exe "globalwaves Player.sln" /maxcpucount /toolsversion:4.0 /verbosity:m /nologo /p:Configuration=Debug
if %ERRORLEVEL% NEQ 0 goto BUILD_NOT
exit /B 0

:FIND_NETFX
dir /B "%windir%\Microsoft.NET\Framework\v4.*">.tmp
if %errorlevel% neq 0 goto BUILD_NOT
set /p dotnetversion=<.tmp
del .tmp
if "%dotnetversion%"=="" goto BUILD_NOT
set dotnetpath=%windir%\Microsoft.NET\Framework\%dotnetversion%
"%dotnetpath%\msbuild.exe" /version >NUL 2>NUL
if %errorlevel% neq 0 goto BUILD_NOT
path %path%;%dotnetpath%
echo .NET framework %dotnetversion% (not in path),|xecho /a:f /nolf /f:" {}"
goto BUILD_WITH_NETFX

:: :BUILD_WITH_MONO
:: echo Trying to build with Mono... |xecho /a:f
:: :xbuild /p:TargetFrameworkProfile="" /verbosity:normal /nologo "globalwaves Player.sln"
:: if %ERRORLEVEL% NEQ 0 goto BUILD_NOT
:: exit /B

:BUILD_NOT
echo Could not find a working compiler. |xecho /a:c
echo [!] No working compiler found. Make sure a valid C# compiler and framework>&2
echo [!] are installed. If this message appears with Visual Studio 2010/2012 being>&2
echo [!] installed, try running this script from Visual Studio Command Line Tools.>&2
exit /B -1