--
-- localization-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function localization_reload_resources()
    kernel.log("localization reload resources")
	strings = kernel.getdictionary( "strings" )
	LoadStringsMacros()
end


function LoadStringsMacros()
	-- Loads items from strings.resource into macros based on current culture.
	for k, v in pairs(strings) do
		kernel.setmacro("Strings:"..k, v)
	end
end