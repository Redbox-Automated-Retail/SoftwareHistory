--
-- multi-disc-insert-popup-view-leave.lua
-- Copyright (C) 2018 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- send alert message that will trigger an email to the ROS' about what discs were mulit-disc inserted
if inserted_discs ~= nil and #inserted_discs > 0 then
    kernel.log("Sending alert message for multi-disc inserts")
    -- create alert text
    local alert_message_text = "<h2>Multi-disc Insert</h2>"
    alert_message_text = alert_message_text .. "<p>Kiosk Id: " .. tostring(StoreID)
    alert_message_text = alert_message_text .. "<br>Date: " .. tostring( newdatetime() ) .. "</br>"
    alert_message_text = alert_message_text .. "<br>Total Discs inserted: " .. tostring(#inserted_discs) .. "</br>"
    alert_message_text = alert_message_text .. "<br>Discs inserted:</br>"
	for _, v in ipairs( inserted_discs ) do
        alert_message_text = alert_message_text .. "<br>" .. tostring(_) .. ". Barcode: " .. tostring(v.barcode) .. ", Deck: " .. tostring(v.deck) .. ", Slot: " .. tostring(v.slot) .. ", Title: " .. tostring(v.title) .. "  </br>"
    end
    alert_message_text = alert_message_text .. "</p>"

    kernel.log(alert_message_text)

    -- send alert message
    kernel.healthsvcs_sendalert2(
	    StoreID,
	    "TitleEmpty",
	    alert_message_text
    )

end

