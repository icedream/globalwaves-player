@echo off

::if "%1"=="-self" (
::	echo * Updating NuGet...
::	%nuget% update %nugetopt% -Self
::)

xecho /a:%col_stat% Updating NuGet packages...
%nuget% update %nugetopt% "globalwaves Player.sln" | xecho /a:%col_proc% /f:"\t{}"