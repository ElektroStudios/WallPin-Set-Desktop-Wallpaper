@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

:: --- LANGUAGE CONFIGURATION ---
SET "MenuName=Set as desktop &background"

:: Define sub-menu items using format "RegistryKeyName|MenuDisplayText|IconFile|Arguments"
SET "Item0=0. Default|&Default|default.ico|default"
SET "Item1=1. Center|&Center|center.ico|center"
SET "Item2=2. Tile|&&Tile|tile.ico|tile"
SET "Item3=3. Stretch|&Stretch|stretch.ico|stretch"
SET "Item4=4. Fit|&Fit|fit.ico|fit"
SET "Item5=5. Fill|F&ill|fill.ico|fill"
SET "Item6=6. Span|S&pan|span.ico|span"

:: Call the core installation script
CALL "%~dp0Add registry keys [core].bat"
EXIT /B %ERRORLEVEL%
