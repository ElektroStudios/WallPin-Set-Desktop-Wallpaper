@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

REM Script Version: 1.1

SET "FileExtensions=.bmp, .gif, .heic, .heif, .jfif, .jpg, .jpeg, .png, .tif, .tiff"

FOR %%# IN (%FileExtensions%) DO (
	ECHO Restoring default registry keys for file extension %%# ...

    SET "WallPinKey=HKCR\SystemFileAssociations\%%#\Shell\WallPin"
	SET "NativeKey=HKCR\SystemFileAssociations\%%#\Shell\setdesktopwallpaper"

	(
	    REG DELETE "!WallPinKey!" /F
	    REG DELETE "!NativeKey!" /V "LegacyDisable" /F
	)1>NUL
)

ECHO+
ECHO DONE!
ECHO+
TIMEOUT /T 5
EXIT