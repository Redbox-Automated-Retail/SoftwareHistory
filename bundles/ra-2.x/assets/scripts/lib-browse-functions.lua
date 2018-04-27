--
-- lib-browse-functions.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


-- TODO:  It might just be easier to put this is a table resource and localize the resource
function SetESRBRatings()
	ESRBRatings = {
		[14] = { rating_id = 14, name = "EC",   code = "EC", longname = strings.esrb_earlychildhood },
		[15] = { rating_id = 15, name = "E",    code = "E",  longname = strings.esrb_everyone },
		[16] = { rating_id = 16, name = "E 10+", code = "E10+",  longname = strings.esrb_everyone_10_plus },
		[17] = { rating_id = 17, name = "T",    code = "T",  longname = strings.esrb_teen },
		[18] = { rating_id = 18, name = "M (17+)", code = "M",  longname = strings.esrb_mature },
		[19] = { rating_id = 19, name = "RP", code = "RP", longname = strings.esrb_rating_pending },
		[20] = { rating_id = 20, name = "AO (18+)", code = "AO", longname = strings.esrb_adults_only }
	}
end

ProductCatalog.get_default_family = function( self )
	local many_families, default_family = self:has_many_families()
	if many_families == false then
        kernel.set_session_value("BrowseState.BrowseTitleFamily", default_family or kernel.get_session_value("BrowseState.BrowseTitleFamily"))
	end
end



