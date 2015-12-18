--02/28/2014
kernel.markscriptcomplete()
kernel.log("running afteractivate.lua from rental-bundles repo")

kernel.log("rental-bundles afteractivate.lua: Removing  file ")
kernel.filedelete( "${REDS}\\Kiosk Engine\\bundles\\library-tickets.bundle" )