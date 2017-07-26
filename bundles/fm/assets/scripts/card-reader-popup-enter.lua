--
-- card-reader-popup-enter.lua
-- Copyright (C) 2015 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if VMZEnabledMachine == true then
	kernel.hideactor( "close_button" )
end

kernel.setmacro("CardReaderPopup:Body", tostring(strings["card_reader_popup_body"]))

local send_card_reader_audit_event = function( yesno )
	kernel.healthsvcs_sendauditevent(
                    StoreID,
                    "FMA_CardReaderExamine",
                    username,
                    "FMA Bundle",
                    "FMA: CardReaderExamine> " .. yesno
                )
				
	view_popview( "closing popup so qlm/vmz view can go back correctly." )

	if VMZEnabledMachine == true then
		kernel.showview()
	else
		view_show( "qlm_view" )
	end
end


kernel.registeractorhitnotify( "card_reader_yes_button",
	function()
		send_card_reader_audit_event( "YES" )
	end
)

kernel.registeractorhitnotify( "card_reader_no_button",
	function()
		send_card_reader_audit_event( "NO" )
	end
)