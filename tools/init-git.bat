@echo off
set url=%1
set name=%2
set target=libraries\%name%

if not exist "%target%\.git\index" (
	echo * Cloning library "%name%"...
	git clone --quiet %url% "%target%"
) else (
	echo * Fetching updates for library "%name%"...
	pushd "%target%" && git pull --quiet && popd
)
