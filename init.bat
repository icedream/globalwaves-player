@echo off
path %path%;%cd%\tools
::SETLOCAL ENABLEDELAYEDEXPANSION

::call check-nuget --set-quiet     
::if %errorlevel% neq 0 goto E_EXIT

call check-svn
if %errorlevel% neq 0 goto E_EXIT

::call check-git                   
::if %errorlevel% neq 0 goto E_EXIT

call check-hg     
if %errorlevel% neq 0 goto E_EXIT 

::call init-nuget
::call init-svn https://bling.svn.codeplex.com/svn/bling4/Bling.Core/ bling4_core
call init-hg https://hg.codeplex.com/naudio naudio
::call init-hg https://hg.codeplex.com/oggsharp oggsharp
::call init-git https://git01.codeplex.com/nvorbis nvorbis

echo Finished.
exit /B 0

:E_EXIT
pause
exit /B -1