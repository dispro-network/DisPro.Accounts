@echo off
setlocal enabledelayedexpansion
TITLE Modifying your HOSTS file
ECHO Running AddHosts

REM Check if hostnames were provided
IF "%~1" == "" (
    ECHO No host names provided as the 2nd arg. Ex. 'AddHost.cmd "website0.com website1.com"'
    GOTO END
)

:: Get list of domains from first argument
set LIST=%1
set quote="

:: BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    ECHO Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params=%*
    REM Need to execute differently if params are quoted
    if "!LIST:~0,1!" EQU "!quote!" (
        echo UAC.ShellExecute "cmd.exe", "/c %~s0 "%params%"", "", "runas", 1 >> "%temp%\getadmin.vbs"
    ) else (
        echo UAC.ShellExecute "cmd.exe", "/c %~s0 %params%", "", "runas", 1 >> "%temp%\getadmin.vbs"
    )    

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------

:: If LIST begins with a quote, remove surrounding quotes
if "!LIST:~0,1!" EQU "!quote!" (
    set _list=%LIST:~1,-1%
    ) else ( set _list=%LIST% )

ECHO Running wdt addhosts
dotnet wdt addhosts %_list%
PAUSE