idle_timer:suspend()

local skipped = {}
skipped["rb_offsale_utc"] = true
skipped["rb_onsale_utc"] = true
skipped["production_end_date_time_local"] = true
skipped["production_start_date_time_local"] = true
skipped["event_id"] = true
skipped["has_ga_groups"] = true
skipped["seating_chart_id"] = true

local replace = {}
replace["production_type_id"] = "type"

local requires_dump = {}

local format_date = {}
format_date["production_end_date_time_utc"] = true
format_date["production_start_date_time_utc"] = true

local DisplayRows = function (data, x, y)
	local height = 20
	for k, v in pairs(data) do
		should_skip = skipped[k]
		if (should_skip == nil) then

			local labelName = k .. "_label_key"
			kernel.createactor(
				labelName,
				false,
				x,
				y,
				200,
				20,
				tostring(string.upper(k)),
				"font_akzidenz_regular_tiny",
				nil,
				"Gray",
				"Black",
				nil,
				nil,
				nil,
				nil,
				"Text",
				"None")

			x = x + 200
			local text
			if format_date[k] == true then
				_,_,yr,m,d=string.find(v, "(%d%d%d%d)(%d%d)(%d%d)")
				text = m .. "/" .. d .. "/" .. yr
			elseif requires_dump[k] ~= nil then
				text = DataDumper( v, k, true )
			else
				text = v
			end

			labelName = tostring(v) .. "_label_value"
			height = 20
			if k == "long_note" then
				height = 40
			elseif k == "short_note" then
				height = 40
			end
			kernel.createactor(
				labelName,
				false,
				x,
				y,
				500,
				height,
				tostring(text),
				"font_akzidenz_regular_tiny",
				nil,
				"Gray",
				"Black",
				nil,
				nil,
				nil,
				nil,
				"Text",
				"None")

			x = x - 200
			if k == "long_note" then
				y =	y + 40
			elseif k == "short_note" then
				y =	y + 40
			else
				y =	y + 17
			end
		end
	end
	
	-- Display the Attributes if any
	local productionAttributes = ShoppingCart.TicketsCart:get_attributes_and_identifer(ShoppingCart.TicketsCart.production.production_id )
	
	for k, v in pairs( productionAttributes ) do
		local labelName = k .. "_attribute_key"
		local attributeTypeName =	"Attribute "..tostring(v.identifier_type_id).." ".. IdentifierType[ tostring(v.identifier_type_id) ].identifier_type_name
		kernel.createactor(
			labelName,
			false,
			x,
			y,
			200,
			20,
			attributeTypeName,
			"font_akzidenz_regular_tiny",
			nil,
			"Gray",
			"Black",
			nil,
			nil,
			nil,
			nil,
			"Text",
			"None")

		labelName = tostring(k) .. "_name_label_value"
		x = x + 200
		kernel.createactor(
			labelName,
			false,
			x,
			y,
			200,
			20,
			tostring(v.name),
			"font_akzidenz_regular_tiny",
			nil,
			"Gray",
			"Black",
			nil,
			nil,
			nil,
			nil,
			"Text",
			"None")

		labelName = tostring(k) .. "_value_label_value"
		x = x + 210
		kernel.createactor(
			labelName,
			false,
			x,
			y,
			300,
			20,
			tostring(v.value),
			"font_akzidenz_regular_tiny",
			nil,
			"Gray",
			"Black",
			nil,
			nil,
			nil,
			nil,
			"Text",
			"None")

		x = x - 410
		y =	y + 20

	end

	return x, y
end

local DisplayProductionHeaders = function (data, x, y)
	for k, v in pairs(data) do
		should_skip = skipped[k]
		if (should_skip == nil) then
			local labelName = k .. "_label_key"
			if k == "production_end_date_time_utc" then
				k = "END_UTC"
			elseif k == "production_start_date_time_utc" then
				k = "START_UTC"
			elseif k == "production_id" then
				k = "PROD_ID"
			elseif k == "ticket_limit" then
				k = "LIMIT"
			elseif k == "secondary_event_id" then
				k = "SEC_EVNT_ID"
			elseif k == "content_rating_type_id" then
				k = "RATING"
			end
			
			local text_to_display = tostring(string.upper(k))
			if ( replace[k] ~= nil ) then
				text_to_display = tostring(string.upper(replace[k]))
			end

			kernel.createactor(
				labelName,
				false,
				x,
				y,
				200,
				20,
				text_to_display,
				"font_akzidenz_regular_tiny",
				nil,
				"Gray",
				"Black",
				nil,
				nil,
				nil,
				nil,
				"Text",
				"None")
			x = x + 80
		end
	end
	return x, y
end

local DisplayProductionDetails = function (data, x, y)
	for k, v in pairs(data) do
		should_skip = skipped[k]
		if (should_skip == nil) then

			local text
			if format_date[k] == true then
				_,_,yr,m,d=string.find(v, "(%d%d%d%d)(%d%d)(%d%d)")
				text = m .. "/" .. d .. "/" .. yr
			elseif requires_dump[k] ~= nil then
				text = DataDumper( v, k, true )
			else
				text = v
			end

			labelName = tostring(v) .. "_label_value"
			kernel.createactor(
				labelName,
				false,
				x,
				y,
				300,
				20,
				tostring(text),
				"font_akzidenz_regular_tiny",
				nil,
				"Gray",
				"Black",
				nil,
				nil,
				nil,
				nil,
				"Text",
				"None")
			x = x + 80
		end
	end
	return x, y
end

kernel.createactor(
	"Event_label",
	false,
	20,
	60,
	250,
	40,
	"Event Details",
	"font_target_smaller_overlays",
	nil,
	"White",
	"181,18,27",
	nil,
	nil,
	nil,
	nil,
	"Text",
	"None")

local x = 40
local y = 85

kernel.setactortext( "title_area", "  " .. tostring( ShoppingCart.TicketsCart.event.event_id ) .. " - ".. ShoppingCart.TicketsCart.event.sort_name )
x, y = DisplayRows( ShoppingCart.TicketsCart.event, x, y )

kernel.createactor(
	"Production_label",
	false,
	20,
	y,
	250,
	40,
	"Productions",
	"font_target_smaller_overlays",
	nil,
	"White",
	"181,18,27",
	nil,
	nil,
	nil,
	nil,
	"Text",
	"None")

local _tickets_production_selection_productions = tickets_production_selection_productions
local _Productions = Productions
local production = _Productions[_tickets_production_selection_productions[1]]
x, y = DisplayProductionHeaders( production, x, y + 30 )

for k, _ in ipairs( _tickets_production_selection_productions ) do
	local production = _Productions[_tickets_production_selection_productions[k]]
	kernel.log(production)
	x, y = DisplayProductionDetails( production, 40, y + 17 )
	kernel.log(production)
end

