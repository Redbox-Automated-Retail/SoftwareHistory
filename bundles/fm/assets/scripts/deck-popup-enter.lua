--
-- deck-popup-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local x = 157
local y = 134

if deck_table == nil then
	 deck_table = {}
end

for deck = 1, (max_decks or 0) do
	local actor_name = "deck_button_" .. tostring(deck)
	local actor_glow = actor_name .. "_glow"

	kernel.createactor(
		actor_glow, 
		false, 
		x-6, 
		y-6, 
		147, 
		121, 
		tostring(deck), 
		nil, 
		nil, 
		"black", 
		nil, 
		nil, 
		nil, 
		nil, 
		nil, 
		"Background",
		"Enabled")

	kernel.createactor(
		actor_name, 
		false, 
		x, 
		y, 
		135, 
		109, 
		tostring(deck), 
		"large_font", 
		nil, 
		"white", 
		"black", 
		nil, 
		nil, 
		nil, 
		nil, 
		"Background, Text",
		"Enabled")
	
	-- deddie!  Don't touch this!
	if table.foreach(deck_table,
								function(key, value)
									if deck == value then
										return true
									end
								end) then
		kernel.showactor(actor_glow)
		kernel.setactortag(actor_name, "selected")
	else
		kernel.hideactor(actor_glow)
	end
	
	kernel.setactortextalignx(actor_name, "center")
	kernel.setactortextaligny(actor_name, "center")
	kernel.registeractorhitnotify( actor_name, 
								   function()
										local tag = kernel.getactortag(actor_name)
										if tag == nil then
											if DeckPopupMode ~= nil
											and DeckPopupMode == "single" then
												local actors = kernel.getactorsbytag("selected")
												if actors ~= nil then
													for _, v in ipairs( actors ) do
														kernel.hideactor(v .. "_glow")
														kernel.setactortag(v, nil)	
													end
												end	
											end
											tag = "selected"
											kernel.showactor(actor_glow)
										else
											tag = nil
											kernel.hideactor(actor_glow)
										end
										kernel.setactortag(actor_name, tag)
									end)
	x = x + 147
	if deck % 3 == 0 then
		y = y + 122
		x = 157
	end
end

if DeckPopupMode ~= nil and DeckPopupMode == "single" then	
	kernel.removeactor( "all_button" )
	kernel.removeactor( "clear_button" )
	kernel.setactorx( "enter_button", x )
	kernel.setactory( "enter_button", y )
end
