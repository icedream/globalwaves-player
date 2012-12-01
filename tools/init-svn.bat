@echo off
set url=%1
set name=%2
set target=libraries\%name%

if not exist "%target%\.svn\wc.db" (
	echo Cloning library "%name%"... |xecho /a:f
	svn co --non-interactive --trust-server-cert %url% "%target%" | xecho /a:7 /f:"\t{}"
) else (
	echo Fetching updates for library "%name%"... |xecho /a:f
	svn cleanup --non-interactive "%target%" | xecho /a:8 /f:"\t{}"
	svn up --non-interactive "%target%"  | xecho /a:7 /f:"\t{}"
)
