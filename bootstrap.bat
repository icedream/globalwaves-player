@echo off
path %path%;%cd%\tools;%cd%\.nuget;tools;.nuget
::setlocal enabledelayedexpansion
set col_stat=e
set col_err=c
set col_dbg=8
set col_proc=7
set col_ok=2

call check-nuget     
if %errorlevel% neq 0 goto E_EXIT
call check-svn
if %errorlevel% neq 0 goto E_EXIT
call check-git                   
if %errorlevel% neq 0 goto E_EXIT
call check-hg     
if %errorlevel% neq 0 goto E_EXIT 

call init-nuget
call init-git https://github.com/JamesNK/Newtonsoft.Json.git json.net
call init-hg https://hg.codeplex.com/naudio naudio
call init-svn https://wpfsvl.svn.codeplex.com/svn/WPFSoundVisualizationLib/Main/Source/WPFSoundVisualizationLibrary wpfsvl
if exist libraries\metrotoolkit rmdir /q /s libraries\metrotoolkit
if exist libraries\elysium4 rmdir /q /s libraries\elysium4
if exist libraries\nvorbis /q /s libraries\nvorbis
if exist libraries\oggsharp /q /s libraries\oggsharp

xecho /a:%col_ok% Finished.
exit /B 0

:E_EXIT
xecho /a:%col_err% "Bootstrapping failed, error code: %errorlevel%"
pause
exit /B -1