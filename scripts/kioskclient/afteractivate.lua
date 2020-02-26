-- 2019.10.09
kernel.markscriptcomplete()
kernel.log("running afteractivate.lua for kioskclient")

local dllPath = "${REDS}\\Kiosk Client\\bin\\"
local dllSource = dllPath .. "m.VisualStudio.Web.CodeGeneration.EntityFrameworkCore.dll";
local dllDest = dllPath .. "Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore.dll";

if (kernel.fileexists(dllSource)) then
    kernel.filecopy( dllSource, dllDest )
    kernel.log("kiosk client - afteractive.lua: finished copying file that was too long for update service 64 char limit ")
end



	