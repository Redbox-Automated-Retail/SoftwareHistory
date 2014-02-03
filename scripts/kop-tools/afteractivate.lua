kernel.markscriptcomplete()
kernel.log("running afteractivate.lua in tools")

--retcode = kernel.executestring("c:\\Program files\\redbox\\reds\\bin\\kioskutil.exe","-trim","c:\\Program files\\redbox\\reds\\bin",0)

retcode = kernel.executestring("c:\\Program files\\redbox\\reds\\bin\\kioskutil.exe","-shell -setbackground:\"c:\\redbox\\background.bmp\"","c:\\Program files\\redbox\\reds\\bin",0)

kernel.log("Changing windows background")

