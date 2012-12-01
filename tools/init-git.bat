@echo off
set url=%1
set name=%2
set target=libraries\%name%

if not exist "%target%\.git\index" (
	echo Cloning library "%name%"... |xecho /a:f
	git clone %url% "%target%" |xecho /f:"\t{}" /a:8
) else (
	echo Fetching updates for library "%name%"... |xecho /a:f
	pushd "%target%"
  git pull |xecho /f:"\t{}" /a:8
  popd
)
