@echo off

::if "%1"=="-self" (
::	echo * Updating NuGet...
::	%nuget% update %nugetopt% -Self
::)

echo Updating NuGet packages... | xecho /a:F
%nuget% update %nugetopt% "globalwaves Player.sln" | xecho /a:7 /f:"\t{}"