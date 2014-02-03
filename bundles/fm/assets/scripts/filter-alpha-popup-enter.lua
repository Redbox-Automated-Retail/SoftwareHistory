--
-- filter-alpha-popup-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local x = 93
local y = 260

idle_timer = newidletimer()
idle_timer:start()

local alpha_table = { [1] = "A",
					[2] = "B",
					[3] = "C",
					[4] = "D",
					[5] = "E",
					[6] = "F",
					[7] = "G",
					[8] = "H",
					[9] = "I",
					[10] = "J",
					[11] = "K",
					[12] = "L",
					[13] = "M",
					[14] = "N",
					[15] = "O",
					[16] = "P",
					[17] = "Q",
					[18] = "R",
					[19] = "S",
					[20] = "T",
					[21] = "U",
					[22] = "V",
					[23] = "W",
					[24] = "X",
					[25] = "Y",
					[26] = "Z",
					[27] = "0-9"}

filter_titles_tables = { media = { [ "DVD" ] = false, [ "Blu-ray" ] = false, [ "Games" ] = false },
						movie_rating = { [ "G" ] = false, [ "PG" ] = false, [ "PG-13" ] = false, [ "R" ] = false, [ "NR" ] = false, [ "TVMA" ] = false, [ "TVPG" ] = false, [ "TV14" ] = false, [ "TVG" ] = false, [ "TVY" ] = false, [ "TVY7" ] = false, },
						game_rating = { [ "E" ] = false, [ "E 10+" ] = false, [ "T" ] = false, [ "M (17+)" ] = false, [ "RP" ] = false, ["AO (18+)"] = false } }
	


for k,v in ipairs(alpha_table) do
	
	local alpha = k 
	local actor_name = alpha .. "_label"
	local actor_glow = actor_name .. "_glow"
		kernel.createactor(
	actor_glow, 
		false, 
		x-6, 
		y-6, 
		100, 
		74, 
		tostring(v), 
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
		88, 
		62, 
		tostring(v), 
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
	--if table.foreach(deck_table,
	if table.foreach(alpha_table,
								function(key, value)
									if alpha == value then
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
										kernel.logto( "FieldMaintenanceActionLog","Selected :" .. tostring(v) )
									end)
	x = x + 106
	if k < 14 and alpha % 7 == 0 then
		y = y + 78
		x = 93
	elseif k >= 14 and alpha % 7 == 0 then
		y = y + 78
		x = 93
	end
end

if DeckPopupMode ~= nil and DeckPopupMode == "single" then	
	kernel.removeactor( "clear_button" )
	kernel.setactorx( "enter_button", x )
	kernel.setactory( "enter_button", y )
end