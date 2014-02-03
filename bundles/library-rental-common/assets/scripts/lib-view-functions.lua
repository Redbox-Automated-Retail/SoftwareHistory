--
-- lib-view-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function in_maintenance_mode()
	current_view = kernel.peekview()
	if current_view == nil then current_view = "" end
	if( string.find( current_view, "maintenance_view" ) ~= nil ) then
		return true
	else
		return false
	end
end

function safe_to_change_view()
	return ( not in_maintenance_mode() )
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
	kernel.log( "> view_goback; context: " .. tostring( context ) .. ", " .. tostring( pop_only ) )
    if kernel.peekview() == Start_Over_View_Name then
        kernel.showview()
        return
    end
	kernel.popview()
	if pop_only ~= true then
		-- This causes the view's on_enter script to run which is not always desirable
		kernel.showview()
	end
end

function view_goback_safe( context, pop_only )
	if safe_to_change_view() == true then
		view_goback( context, pop_only )
	else
		kernel.log( "view_goback_safe( " .. context .. " ) denied." )
	end
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
		kernel.log( "view_unwindto_safe( " .. name .. ", " .. context .. " ) denied." )
	end
end

function view_unwindtobottom( context )
	view_unwindto( Start_Over_View_Name, true, context )
end

function view_unwindtobottom_safe( context )
	if safe_to_change_view() == true then			
		view_unwindtobottom( context )
	else
		kernel.log( "view_unwindtobottom_safe( " .. context .. " ) denied." )
	end
end

function view_analytics_event( subItemType, subItemValue )
	if ShoppingCart ~= nil and ShoppingCart.session_id ~= nil then
		view_analytics_event2( subItemType, subItemValue, ShoppingCart.session_id )
	end
end

function view_analytics_event2( subItemType, subItemValue, session_id )
	kernel.analyticsaddnode( "viewEvents", "viewEvent", session_id )
	kernel.analyticsaddattribute( "viewEvent", "subItemType", subItemType, session_id )
	kernel.analyticsaddattribute( "viewEvent", "subItemValue", subItemValue, session_id )
end

function view_analytics_start( startViewName, sessionType )
	if ShoppingCart ~= nil 
	and ShoppingCart.session_id ~= nil 
	and kernel.analyticssessionexists( ShoppingCart.session_id ) == false 
	then
		kernel.analyticsstart( ShoppingCart.session_id, sessionType )
		kernel.analyticsaddview( ShoppingCart.session_id, startViewName )
	end
end

