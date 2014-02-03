--
-- view-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function view_show( name )
	kernel.pushview( name )
	kernel.showview()
end

function view_goback()
	kernel.popview()
	kernel.showview()
end

function view_unwindto( name, discard )
	if kernel.peekview() == name
	or kernel.peekview() == nil then
		return
	end
	while true do
		local view = kernel.peekview()
		if view == nil
		or view == name then
			break
		end
		if discard == true then
			kernel.popviewdiscard()
		else
			kernel.popview()
		end
	end
	kernel.showview()
end

function view_unwindtobottom()
	view_unwindto( "control_panel_view", true )
end
