--
-- set-decks.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local actors = kernel.getactorsbytag("selected")

if actors == nil then
	log("Error getting actors by tag")
	return
end

deck_table = {}
if #actors ~= max_decks then
	for k, v in ipairs( actors ) do
		local deck = tonumber(kernel.getactortext(v))
		table.insert(deck_table, deck)
	end
end

view_goback()