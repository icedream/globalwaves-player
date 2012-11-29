@echo off
if not exist libraries\NAudio (
	echo * Cloning library "NAudio"...
	hg --quiet clone https://hg.codeplex.com/naudio libraries\NAudio
) else (
	echo * Fetching updates for library "NAudio"...
	pushd "%cd%\libraries\NAudio" && hg --quiet pull && popd )
)