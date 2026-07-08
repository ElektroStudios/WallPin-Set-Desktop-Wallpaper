The "Add new registry keys*.bat" script files rely on relative paths 
and expect WallPin.exe to be in the parent folder (one level above).

If you change the directory structure, you must manually update the 
WallPin.exe location variable inside the .bat file.