@echo off
set url=%1
set name=%2
set target=libraries\%name%

if not exist "%target%\.svn\wc.db" (
	echo * Cloning library "%name%"...
	svn co --quiet --non-interactive --trust-server-cert %url% "%target%"
) else (
	echo * Fetching updates for library "%name%"...
	svn up --quiet "%target%"
)
