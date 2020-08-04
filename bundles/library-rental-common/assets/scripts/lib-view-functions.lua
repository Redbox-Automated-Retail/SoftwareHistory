--
-- lib-view-functions.lua
-- Copyright (C) 2016 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function in_maintenance_mode()
    return kernel.is_in_maintenance_mode()
end

function safe_to_change_view()
    return kernel.safe_to_change_view()
end

function view_show( name )
	kernel.pushview( name )
	kernel.showview()
end

function view_popview( context )
	kernel.log( "> view_popview; context: " .. tostring( context ) )
	kernel.popview()
end

function view_show_safe( name )
	if safe_to_change_view() == true then
		view_show( name )
	else
		kernel.log( "view_show_safe( " .. name .. " ) denied." )
	end
end

function view_goback( context, pop_only )
    kernel.view_goback(context, pop_only)
end

function view_goback_safe( context, pop_only )
    kernel.view_goback_safe(context, pop_only)
end

function view_unwindto( name, discard, context )
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
			kernel.log( "> PopViewDiscard " .. view .. "; context: " .. tostring( context ) )
			kernel.popviewdiscard()
		else
			kernel.log( "> PopView " .. view .. "; context: " .. tostring( context ) )
			kernel.popview()
		end
	end
	kernel.showview()
end

function view_unwindto_safe( name, discard, context )
	if safe_to_change_view() == true then			
		view_unwindto( name, discard, context )
	else
		kernel.log( "view_unwindto_safe( " .. tostring( name ) .. ", " .. tostring( context ) .. " ) denied." )
	end
end

function view_unwindtobottom( context )
    -- this is obsolete.  Call kernel.close_open_session_and_start_new_rental_session(reason) instead
	view_unwindto( kernel.get_start_over_view_name(), true, context )
end

function view_unwindtobottom_safe( context )
    -- this is obsolete. Call kernel.close_open_session_and_start_new_rental_session(reason) instead
	if safe_to_change_view() == true then			
		view_unwindtobottom( context )
	else
		kernel.log( "view_unwindtobottom_safe( " .. context .. " ) denied." )
	end
end

function view_analytics_event( subItemType, subItemValue )
    kernel.analyticsaddviewevent( subItemType, subItemValue )

	--kernel.analyticsserviceaddeventviewevent( subItemType, subItemValue )
end

function view_analytics_event2( subItemType, subItemValue, session_id )
    kernel.analyticsaddviewevent2( subItemType, subItemValue, session_id )

	--kernel.analyticsserviceaddeventviewevent( session_id, subItemType, subItemValue )
end