@echo off
set url=%1
set name=%2
set target=libraries\%name%

if not exist "%target%\.git\index" (
	xecho /a:%col_stat% Downloading library "%name%"...
	git clone -v %url% "%target%" |xecho /f:"\t{}" /a:%col_proc%
) else (
	xecho /a:%col_stat% Updating library "%name%"...
	pushd "%target%"
  git pull -v |xecho /f:"\t{}" /a:%col_proc%
  popd
)
