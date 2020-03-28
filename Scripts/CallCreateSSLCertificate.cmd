@echo off
setlocal enabledelayedexpansion
TITLE Creating and importing SSL Certificate
ECHO Running CallCreateSSLCertificate

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
    echo UAC.ShellExecute "cmd.exe", "/c %~s0", "", "runas", 1 >> "%temp%\getadmin.vbs" 

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------

ECHO Calling CreateSSLCertificate
powershell -File CreateSSLCertificate.ps1

REM Copy pfx files into projects
ECHO Copying pfx files into the DisPro.Accounts and Sample projects

REM DisPro.Accounts
xcopy /vy ..\certificates\accounts.dispro.network.local.pfx ..\DisPro.Accounts\
REM dotnet
xcopy /vy ..\certificates\accounts.dispro.network.local.pfx ..\Samples\dotnet\Api\
xcopy /vy ..\certificates\accounts.dispro.network.local.pfx ..\Samples\dotnet\MvcClientImplicit\
xcopy /vy ..\certificates\accounts.dispro.network.local.pfx ..\Samples\dotnet\MvcClientHybrid\
xcopy /vy ..\certificates\accounts.dispro.network.local.pfx ..\Samples\dotnet\JavaScriptClient\
REM node
echo f | xcopy /vy ..\certificates\accounts.dispro.network.local.pem ..\Samples\node\react-client\server.pem

PAUSE