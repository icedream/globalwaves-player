@echo off
path %path%;%cd%\tools
::SETLOCAL ENABLEDELAYEDEXPANSION

call check-nuget     
if %errorlevel% neq 0 goto E_EXIT

call check-svn
if %errorlevel% neq 0 goto E_EXIT

::call check-git                   
::if %errorlevel% neq 0 goto E_EXIT

call check-hg     
if %errorlevel% neq 0 goto E_EXIT 

call init-nuget
call init-hg https://hg.codeplex.com/naudio naudio
call init-svn https://wpfsvl.svn.codeplex.com/svn/WPFSoundVisualizationLib/Main/Source/WPFSoundVisualizationLibrary wpfsvl
if exist libraries\metrotoolkit rmdir /q /s libraries\metrotoolkit
if exist libraries\elysium4 rmdir /q /s libraries\elysium4
if exist libraries\nvorbis /q /s libraries\nvorbis
if exist libraries\oggsharp /q /s libraries\oggsharp

xecho /a:F Finished.
exit /B 0

:E_EXIT
pause
exit /B -1