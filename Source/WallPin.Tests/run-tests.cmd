@echo off
setlocal

dotnet test "%~dp0WallPin.Tests.vbproj" --configuration Release %*
exit /b %ERRORLEVEL%
