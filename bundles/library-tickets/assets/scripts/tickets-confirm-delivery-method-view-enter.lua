--
-- tickets-confirm-delivery-method-view_enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved

if ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"] ~= nil then
	kernel.registeractorhitnotify(
		"option_1_button",
		function()
			kernel.setactorimage( "option_1_checkmark", "image_red_checkmark" )
			if ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"] ~= nil then
				kernel.setactorimage( "option_2_checkmark", "image_gray_checkmark" )
			end
			ShoppingCart.TicketsCart.selected_delivery_method_id = ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"].DeliveryMethodId
			view_analytics_event( "buttonPress", "delivery-method-" .. ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"].DeliveryMethodDisplayName )
		end
	)
	kernel.setactortext( "option_1_title_label", ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"].DeliveryMethodDisplayName )
	kernel.setactortext( "option_1_body_label", ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"].DeliveryMethodDescription )
	if ShoppingCart.TicketsCart.selected_delivery_method_id == "1" and ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"] == nil then
		ShoppingCart.TicketsCart.selected_delivery_method_id = nil
	end
	if ShoppingCart.TicketsCart.selected_delivery_method_id == nil then
		ShoppingCart.TicketsCart.selected_delivery_method_id = ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"].DeliveryMethodId
	end
	view_analytics_event( "context", "delivery-method-" .. ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"].DeliveryMethodDisplayName .. "-available" )
else
	ShoppingCart.TicketsCart.selected_delivery_method_id = nil
	kernel.setactorimage( "option_1_checkmark", "image_disabled_checkmark" )
	kernel.setactorforegroundcolor( "option_1_title_label", { ["r"] = 168, ["g"] = 167, ["b"] = 167 } )
	kernel.setactorforegroundcolor( "option_1_body_label", { ["r"] = 168, ["g"] = 167, ["b"] = 167 } )
end

if ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"] ~= nil then
	kernel.registeractorhitnotify(
		"option_2_button",
		function()
			kernel.setactorimage( "option_2_checkmark", "image_red_checkmark" )
			if ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"] ~= nil then
				kernel.setactorimage( "option_1_checkmark", "image_gray_checkmark" )
			end
			ShoppingCart.TicketsCart.selected_delivery_method_id = ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"].DeliveryMethodId
			view_analytics_event( "buttonPress", "delivery-method-" .. ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"].DeliveryMethodDisplayName )
		end
	)
	kernel.setactortext( "option_2_title_label", ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"].DeliveryMethodDisplayName )
	kernel.setactortext( "option_2_body_label", ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"].DeliveryMethodDescription )
	if ShoppingCart.TicketsCart.selected_delivery_method_id == nil and ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"] == nil then
		ShoppingCart.TicketsCart.selected_delivery_method_id = ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"].DeliveryMethodId
	end
	view_analytics_event( "context", "delivery-method-" .. ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"].DeliveryMethodDisplayName .. "-available" )
else
	kernel.setactorimage( "option_2_checkmark", "image_disabled_checkmark" )
	kernel.setactorforegroundcolor( "option_2_title_label", { ["r"] = 168, ["g"] = 167, ["b"] = 167 } )
	kernel.setactorforegroundcolor( "option_2_body_label", { ["r"] = 168, ["g"] = 167, ["b"] = 167 } )
end

if ShoppingCart.TicketsCart.selected_delivery_method_id == "1" and ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["1"] ~= nil then
	kernel.setactorimage( "option_2_checkmark", "image_red_checkmark" )
elseif ShoppingCart.TicketsCart.selected_delivery_method_id == "2" and ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods["2"] ~= nil then
	kernel.setactorimage( "option_1_checkmark", "image_red_checkmark" )
end

kernel.registeractorhitnotify(
	"back_button",
	function()
		view_analytics_event( "buttonPress", "delivery-method-back-button" )
		view_unwindto( "tickets_seat_selection_view", true, "DeliveryMethod: User hit back button." )
	end
)

kernel.registeractorhitnotify(
	"back_button_arrow",
	function()
		view_analytics_event( "buttonPress", "delivery-method-back-button" )
		view_unwindto( "tickets_seat_selection_view", true, "DeliveryMethod: User hit back button." )
	end
)


kernel.registeractorhitnotify(
	"ok_button",
	function()
		view_analytics_event( "context", "delivery-method-" .. ShoppingCart.TicketsCart.selected_ticket_group.DeliveryMethods[ShoppingCart.TicketsCart.selected_delivery_method_id].DeliveryMethodDisplayName .. "-selected" )
		view_analytics_event( "buttonPress", "delivery-method-ok-button" )
	
		if ShoppingCart.TicketsCart:should_display_disclaimer() then
			view_show_safe( "tickets_disclaimer_message_view" )
		else
			view_show( "tickets_shopping_cart_view" )
		end
	end
)