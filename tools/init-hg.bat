@echo off
set url=%1
set name=%2
set target=libraries\%name%

if not exist "%target%\.hg\hgrc" (
	echo Cloning library "%name%"...|xecho /a:f
	hg clone %url% "%target%"|xecho /a:7 /f:"\t{}"
) else (
	echo Fetching updates for library "%name%"...|xecho /a:f
	pushd "%target%"
  hg pull|xecho /a:7 /f:"\t{}"
  hg update|xecho /a:7 /f:"\t{}"
  popd
)