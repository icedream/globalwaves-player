@echo off

::if "%1"=="-self" (
::	echo * Updating NuGet...
::	%nuget% update %nugetopt% -Self
::)

echo * Updating NuGet packages...
%nuget% update %nugetopt% "globalwaves Player.sln"