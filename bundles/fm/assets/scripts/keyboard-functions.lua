--
-- keyboard-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
function change_case()
	local actors = kernel.getactorsbytag("case")

	if actors == nil then
		log("Error getting actors by tag")
		return
	end

	upper_case_flag = not upper_case_flag

	for k, v in ipairs( actors ) do
		local actortext = kernel.getactortext( v )

		if upper_case_flag == true then
			kernel.setactortext( v, string.upper( actortext ) )
		else
			kernel.setactortext( v, string.lower( actortext ) )
		end

	end
end
