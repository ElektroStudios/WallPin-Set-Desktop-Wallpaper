@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

:: --- CONFIGURATION ---
SET "Extensions=.bmp, .gif, .jpg, .jpeg, .png, .tif, .tiff"
SET "MenuName=Establecer como &fondo de pantalla"
SET "Position=Top"
SET "Exe=%~dp0..\WallPin.exe"
SET "Icon=%Exe%,0"

:: Define sub-menu items using format "RegistryKeyName|MenuDisplayText|Arguments"
SET "Item0=0. Default|Por defecto|"
SET "Item1=1. Center|Centrado|center"
SET "Item2=2. Tile|Mosaico|tile"
SET "Item3=3. Stretch|Expandido|stretch"
SET "Item4=4. Fit|Ajustar|fit"
SET "Item5=5. Fill|Rellenar|fill"
SET "Item6=6. Span|Extender|span"
:: ---------------------

FOR %%# IN (%Extensions%) DO (
    ECHO Writing registry sub-menu for file extension %%# ...
    
    SET "WallPinKey=HKCR\SystemFileAssociations\%%#\Shell\WallPin"
    SET "WindowsKey=HKCR\SystemFileAssociations\%%#\Shell\setdesktopwallpaper"
    (
        :: Create parent menu item
        REG ADD "!WallPinKey!" /V "MUIVerb"                /T "REG_SZ" /D "%MenuName%"                      /F
        REG ADD "!WallPinKey!" /V "Icon"                   /T "REG_SZ" /D "%Icon%"                          /F
		REG ADD "!WallPinKey!" /V "NeverDefault"           /T "REG_SZ" /D ""                                /F
        REG ADD "!WallPinKey!" /V "Position"               /T "REG_SZ" /D "%Position%"                      /F
		REG ADD "!WallPinKey!" /V "SuppressionSlapiPolicy" /T "REG_SZ" /D "ChangeDesktopBackground-Enabled" /F
        REG ADD "!WallPinKey!" /V "MultiSelectModel"       /T "REG_SZ" /D "Single"                          /F
        REG ADD "!WallPinKey!" /V "SubCommands"            /T "REG_SZ" /D ""                                /F
        
        :: Loop through defined sub-menu items (0 to 6)
        FOR /L %%I IN (0,1,6) do (
            FOR /F "tokens=1,2,3 delims=|" %%A IN ("!Item%%I!") DO (
                SET "SubKey=!WallPinKey!\Shell\%%A"
                
                :: Write sub-item layout and target execution command
                CALL REG ADD "!SubKey!" /V "" /T "REG_SZ" /D "%%B" /F
                CALL REG ADD "!SubKey!\Command" /V "" /T "REG_SZ" /D "\"%Exe%\" \"%%%%1\" \"%%C\""  /F
            )
        )
        
        :: Disable original key
        REG ADD "!WindowsKey!" /V "LegacyDisable" /T "REG_SZ" /D "" /F
    ) 1>NUL
)

ECHO+
ECHO DONE!
EXIT