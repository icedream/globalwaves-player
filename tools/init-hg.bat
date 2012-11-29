@echo off
set url=%1
set name=%2
set target=libraries\%name%

if not exist "%target%\.hg\hgrc" (
	echo * Cloning library "%name%"...
	hg clone --quiet %url% "%target%"
) else (
	echo * Fetching updates for library "%name%"...
	pushd "%target%" && hg pull --quiet && popd
)