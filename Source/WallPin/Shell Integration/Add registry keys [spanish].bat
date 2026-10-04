@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

:: --- LANGUAGE CONFIGURATION ---
SET "MenuName=Establecer como &fondo de pantalla"

:: Define sub-menu items using format "RegistryKeyName|MenuDisplayText|IconFile|Arguments"
SET "Item0=0. Default|&Por defecto|default.ico|default"
SET "Item1=1. Center|&Centrado|center.ico|center"
SET "Item2=2. Tile|&Mosaico|tile.ico|tile"
SET "Item3=3. Stretch|&Expandido|stretch.ico|stretch"
SET "Item4=4. Fit|&Ajustar|fit.ico|fit"
SET "Item5=5. Fill|&Rellenar|fill.ico|fill"
SET "Item6=6. Span|E&xtender|span.ico|span"

:: Call the core installation script
CALL "%~dp0Add registry keys [core].bat"
EXIT /B %ERRORLEVEL%
