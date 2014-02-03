--
-- obj-unplayable-expire-timer.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


function newunplayableexpiretimer()
	local t = {}
	t.start = function( self, start, frequency )
		kernel.timercreate(
			"unplayable-expire",
			start or kernel.random(60000,600000),
			frequency or 1800000,
			self.on_timer )
		kernel.timerstart( "unplayable-expire" )
	end
	
	t.stop = function( self )
        kernel.log("unplayable-expire-timer:stop()")
        kernel.timerstop( "unplayable-expire" )
	end

	t.on_timer = function()
        kernel.timerstop( "unplayable-expire" )
        kernel.log("Unplayable-Expire.on_timer: Executing")
		local discs_to_expire = {}
		local current_date = get_current_date_string( "%4d%02d%02d%02d%02d" )
		for k, v in UnplayableProduct:pairs() do
			kernel.log( "> candidate barcode " .. tostring( k ) .."  returned: " .. tostring( v.returned_time ))
			if VerboseLogging == true then kernel.log( "> current Date: " .. tostring( current_date )) end
			local hours_elapsed = kernel.getelapsedhours( v.returned_time, current_date )
			if VerboseLogging == true then kernel.log( "> hours elapsed = " .. tostring( hours_elapsed )) end
			if hours_elapsed > 24 then
				discs_to_expire[ k ] = true
			end
		end
		for k, v in pairs( discs_to_expire ) do
			UnplayableProduct:remove( k )
			kernel.log( "> expired unplayable record for barcode: " .. tostring( k ) )
		end
		kernel.log("Unplayable-Expire.on_timer:  Complete" )
		kernel.timerstart( "unplayable-expire" )
	end

	return t
end
