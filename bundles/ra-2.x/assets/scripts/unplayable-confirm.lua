--
-- unplayable-confirm.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--


local message_view_manager = newmessageviewmanager()
message_view_manager:show()
view_analytics_event( "context", "unplayableConfirm" )
if kernel.getculture() == "es-MX" then
	message_view_manager:set_unplayable_message( strings.unplayable_confirm_messageA )
else
	message_view_manager:set_title( strings.unplayable_confirm_messageA )
end
message_view_manager:set_message( strings.unplayable_confirm_messageB )
message_view_manager:show_first_button(
	strings.back_button,
	function() view_goback_safe( "unplayableConfirm:first_button", false ) end
)
message_view_manager:show_second_button(
	strings.okay_button,
	function()
		kernel.log( "unplayble_continue button pressed." )
		view_analytics_event( "buttonPress", "unplayable_continue_button" )
		message_view_manager:hide()
		kernel.executescript( "unplayable_return" )
	end  )
message_view_manager:start_timer(
	function()
		ABE_ON = false
		view_unwindtobottom_safe( "unplayableConfirm:start_timer" )
	end
)