--06/29/2012 
kernel.markscriptcomplete()
kernel.log("running afteractivate.lua from engine repo")

	kernel.log("engine-afteractive.lua: Removing  files ")
	kernel.filedelete( "${REDS}\\Kiosk Engine\\bin\\Redbox.HAL.Client.dll" )
	kernel.filedelete( "${REDS}\\Kiosk Engine\\bin\\Redbox.KioskServices.ComponentModel.dll" )
	kernel.filedelete( "${REDS}\\Kiosk Engine\\bin\\Redbox.UpdateManager.Client.dll" )
		