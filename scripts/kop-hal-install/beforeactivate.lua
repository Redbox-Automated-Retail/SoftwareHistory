--
-- kop-hal-install beforeactivate.lua 8/27/2014

kernel.markscriptcomplete()
kernel.log("running beforeactivate.lua in Halservice")

local file1 = "C:\\Program Files\\Redbox\\halservice\\bin\\CiBcfDm80000002.dll" if kernel.fileexists(  file1  ) then kernel.filedelete(  file1  ) end
local file2 = "C:\\Program Files\\Redbox\\halservice\\bin\\CiBcfDm80000002.dll.hash" if kernel.fileexists(  file2  ) then kernel.filedelete(  file2  ) end
local file3 = "C:\\Program Files\\Redbox\\halservice\\bin\\CiBcfKo86572612.dll" if kernel.fileexists(  file3  ) then kernel.filedelete(  file3  ) end
local file4 = "C:\\Program Files\\Redbox\\halservice\\bin\\CiBcfKo86572612.dll.hash" if kernel.fileexists(  file4  ) then kernel.filedelete(  file4  ) end
local file5 = "C:\\Program Files\\Redbox\\halservice\\bin\\Redbox.HAL.Camera.Framework.dll" if kernel.fileexists(  file5  ) then kernel.filedelete(  file5  ) end
local file6 = "C:\\Program Files\\Redbox\\halservice\\bin\\Redbox.HAL.ClearimageV1.dll" if kernel.fileexists(  file6  ) then kernel.filedelete(  file6  ) end
local file7 = "C:\\Program Files\\Redbox\\halservice\\bin\\camera.plugins\\DirectShow.Descriptor.xml" if kernel.fileexists(  file7  ) then kernel.filedelete(  file7  ) end
local file8 = "C:\\Program Files\\Redbox\\halservice\\bin\\camera.plugins\\Redbox.HAL.DirectShow.Plugin.dll" if kernel.fileexists(  file8  ) then kernel.filedelete(  file8  ) end
local file9 = "C:\\Program Files\\Redbox\\halservice\\bin\\camera.plugins\\Redbox.HAL.ICameraPlugin.Vidcap.dll" if kernel.fileexists(  file9  ) then kernel.filedelete(  file9  ) end
local file10 = "C:\\Program Files\\Redbox\\halservice\\bin\\camera.plugins\\Vidcap.Descriptor.xml" if kernel.fileexists(  file10  ) then kernel.filedelete(  file10  ) end
local file11 = "C:\\Program Files\\Redbox\\halservice\\Scripts\\clear-picker.hs" if kernel.fileexists(  file11  ) then kernel.filedelete(  file11  ) end
local file12 = "C:\\Program Files\\Redbox\\halservice\\Scripts\\quick-return-restart.hs" if kernel.fileexists(  file12  ) then kernel.filedelete(  file12  ) end
local file13 = "C:\\Program Files\\Redbox\\halservice\\Scripts\\quick-return-setup.hs" if kernel.fileexists(  file13  ) then kernel.filedelete(  file13  ) end
local file14 = "C:\\Program Files\\Redbox\\halservice\\Scripts\\return-unknown.hs" if kernel.fileexists(  file14  ) then kernel.filedelete(  file14  ) end
local file15 = "C:\\Program Files\\Redbox\\halservice\\Scripts\\vend-unknown.hs" if kernel.fileexists(  file15  ) then kernel.filedelete(  file15  ) end