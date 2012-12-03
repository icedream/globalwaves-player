@echo off

set EnableNuGetPackageRestore=true

::if "%1"=="-self" (
::	xecho /a:%col_stat% "Updating NuGet..."
::	%nuget% update %nugetopt% -Self
::)

xecho /a:%col_stat% "Updating NuGet packages..."
%nuget% update %nugetopt% "globalwaves Player.sln" | xecho /a:%col_proc% /f:"\t{}"