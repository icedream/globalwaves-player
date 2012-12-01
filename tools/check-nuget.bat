@echo off

:: NuGet detection
echo Detecting how to run NuGet...|xecho /a:F
                          
set nuget=NuGet.exe
%nuget% config 2>NUL >NUL
if %errorlevel% equ 0 goto fin

set nuget="%cd%\.nuget\NuGet.exe"
%nuget% config 2>NUL >NUL
if %errorlevel% equ 0 goto fin

set nuget=mono NuGet.exe
%nuget% config 2>NUL >NUL
if %errorlevel% equ 0 goto fin

set nuget=mono "%cd%\.nuget\NuGet.exe"
%nuget% config 2>NUL >NUL
if %errorlevel% equ 0 goto fin

:err
echo [!] Could not find a reliable way to execute NuGet!>&2
exit /B -1

:fin
:: Set options
set nugetopt=-NonInteractive
if "%1"=="--set-verbose" set nugetopt=-Verbose -Verbosity "detailed" %nugetopt%
if "%1"=="--set-quiet" set nugetopt=-Verbosity "quiet" %nugetopt%
echo %nuget% %nugetopt%|xecho /f:"\t{}" /a:8
exit /B 0
