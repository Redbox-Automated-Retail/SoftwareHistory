--
-- lib-browse-functions.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

ProductCatalog.get_default_family = function( self )
	local many_families, default_family = self:has_many_families()
	if many_families == false then
        kernel.set_session_value("BrowseState.BrowseTitleFamily", default_family or kernel.get_session_value("BrowseState.BrowseTitleFamily"))
	end
end



