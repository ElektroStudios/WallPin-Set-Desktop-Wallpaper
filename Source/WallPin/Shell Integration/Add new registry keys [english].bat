@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

REM Script Version: 1.1.0

:: --- CONFIGURATION ---
SET "FileExtensions=.bmp, .gif, .heic, .heif, .jfif, .jpg, .jpeg, .png, .tif, .tiff"
SET "MenuName=Set as desktop &background"
SET "Position=Top"
SET "Exe=%~dp0..\WallPin.exe"
SET "Icon=%Exe%,0"
SET "IconsDir=%~dp0Icons"

:: Define sub-menu items using format "RegistryKeyName|MenuDisplayText|IconFile|Arguments"
SET "Item0=0. Default|&Default|default.ico|default"
SET "Item1=1. Center|&Center|center.ico|center"
SET "Item2=2. Tile|&&Tile|tile.ico|tile"
SET "Item3=3. Stretch|&Stretch|stretch.ico|stretch"
SET "Item4=4. Fit|&Fit|fit.ico|fit"
SET "Item5=5. Fill|F&ill|fill.ico|fill"
SET "Item6=6. Span|S&pan|span.ico|span"

:: ---------------------

FOR %%# IN (%FileExtensions%) DO (
    ECHO Writing registry sub-menu for file extension %%# ...
    
    SET "WallPinKey=HKCR\SystemFileAssociations\%%#\Shell\WallPin"
    SET "NativeKey=HKCR\SystemFileAssociations\%%#\Shell\setdesktopwallpaper"

    REM Create parent menu item.
    (
        :: Display text of the menu item (supports "&" accelerator keys).
        REG ADD "!WallPinKey!" /V "MUIVerb" /T "REG_SZ" /D "%MenuName%" /F

        :: Icon shown next to the menu item (icon path and icon index).
        REG ADD "!WallPinKey!" /V "Icon" /T "REG_SZ" /D "%Icon%" /F

        :: Prevents this verb from ever becoming the default action (double-click) for the file type.
        REG ADD "!WallPinKey!" /V "NeverDefault" /T "REG_SZ" /D "" /F

        :: Position of the menu item within the context menu (e.g. Top or Bottom).
        REG ADD "!WallPinKey!" /V "Position" /T "REG_SZ" /D "%Position%" /F

        :: Asks Explorer to resolve the item state (enabled/disabled) synchronously instead of in a background thread.
        REG ADD "!WallPinKey!" /V "CommandStateSync" /T "REG_SZ" /D "" /F

        :: Windows licensing policy that controls whether the item is shown (same one used by the native "setdesktopwallpaper" verb).
        REG ADD "!WallPinKey!" /V "SuppressionSlapiPolicy" /T "REG_SZ" /D "ChangeDesktopBackground-Enabled" /F

        :: Selection model: how the item behaves when multiple files are selected.
        REG ADD "!WallPinKey!" /V "MultiSelectModel" /T "REG_SZ" /D "Single" /F
        
        :: Marks the item as a cascading submenu, built from the nested "Shell" subkey.
        REG ADD "!WallPinKey!" /V "SubCommands" /T "REG_SZ" /D "" /F

        REM Loop through the sub-menu items defined in the configuration section (the range must match the number of 'ItemN' entries).
        FOR /L %%I IN (0,1,6) do (
            FOR /F "tokens=1,2,3,4 delims=|" %%A IN ("!Item%%I!") DO (
                SET "SubKey=!WallPinKey!\Shell\%%A"

                :: Sub-item display text (supports "&" accelerator keys)
                CALL REG ADD "!SubKey!" /V "" /T "REG_SZ" /D "%%B" /F

                :: Icon shown next to the sub-item (icon path and icon index).
                CALL REG ADD "!SubKey!" /V "Icon" /T "REG_SZ" /D "%IconsDir%\%%C" /F

                :: Asks Explorer to resolve the item state synchronously.
                CALL REG ADD "!SubKey!" /V "CommandStateSync" /T "REG_SZ" /D "" /F

                :: Command line executed on click.
                CALL REG ADD "!SubKey!\Command" /V "" /T "REG_SZ" /D "\"%Exe%\" \"%%%%1\" \"%%D\"" /F
            )
        )

        REM Disable the native "setdesktopwallpaper" verb.
        REG ADD "!NativeKey!" /V "LegacyDisable" /T "REG_SZ" /D "" /F
    ) 1>NUL
)

ECHO+
ECHO DONE!
ECHO+
TIMEOUT /T 5
EXIT