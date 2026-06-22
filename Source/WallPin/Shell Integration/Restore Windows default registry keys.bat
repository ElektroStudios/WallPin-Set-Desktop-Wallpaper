@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

SET "Extensions=.bmp, .gif, .jpg, .jpeg, .png, .tif, .tiff"

FOR %%# IN (%Extensions%) DO (
	ECHO Restoring default registry keys for file extension %%# ...

    SET "WallPinKey=HKCR\SystemFileAssociations\%%#\Shell\WallPin"
	SET "WindowsKey=HKCR\SystemFileAssociations\%%#\Shell\setdesktopwallpaper"

	(
	    REG DELETE "!WallPinKey!"                     /F
	    REG DELETE "!WindowsKey!" /V "LegacyDisable" /F
	)1>NUL
)

ECHO+
ECHO:DONE!
EXIT