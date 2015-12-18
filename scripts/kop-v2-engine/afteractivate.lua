--06/4/2014 
kernel.markscriptcomplete()
kernel.log("running afteractivate.lua from engine repo")

	kernel.log("engine-afteractive.lua: Removing  files ")
	kernel.filedelete( "${REDS}\\Kiosk Engine\\bin\\Redbox.HAL.Client.dll" )
	kernel.filedelete( "${REDS}\\Kiosk Engine\\bin\\Redbox.KioskServices.ComponentModel.dll" )
	kernel.filedelete( "${REDS}\\Kiosk Engine\\bin\\Redbox.UpdateManager.Client.dll" )
kernel.filedelete( "${REDS}\\Kiosk Engine\\kext\\Redbox.DVD.TicketServices.dll" )
kernel.filedelete( "${REDS}\\Kiosk Engine\\kext\\ticket-services.kext" )
kernel.filedelete( "${REDS}\\Kiosk Engine\\bundles\\library-tickets.bundle" )
		