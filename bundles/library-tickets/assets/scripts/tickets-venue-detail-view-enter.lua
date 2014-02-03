--
-- tickets-venue-detail-view-enter.lua
-- Copyright (C) 2012 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

-- { venue_id = 4064 , venue_name = "Noble Horse Theatre" , address1 = "1410 North Orleans Street" , address2 = nil , city = "Chicago" ,
--   state = "IL" , zip = "60610" , country = "US" , short_note = nil , long_note = "Noble Horse Theatre holds year round performances in an indoor,
--   climate-controlled environment." , timezone_name = "Central Time (US & Canada), Mexico City, Monterrey" , phone_number = "8472222222" ,
--   accessible = true , accessibility_information = nil , parking_information = nil
-- }

format_phone_number = function(Phone_Number)
	if Phone_Number == nil then
		return ""
	end

	if string.len(Phone_Number) == 0 then
		return ""
	end
	
	if string.len(Phone_Number) == 7 then
		return string.sub(Phone_Number, 1, 3) .. "-" .. string.sub(Phone_Number, 4, 7)
	end

	if string.len(Phone_Number) == 10 then
		return string.sub(Phone_Number, 1, 3) .. "-" .. string.sub(Phone_Number, 4, 6) .. "-" .. string.sub(Phone_Number, 7, 10)
	end
	
	--
	-- Don't know what to do...assume invalid phone number...
	--
	return ""
end

format_description = function(Description, APName)
	if Description == nil then
		return "No " .. APName .. " information is available. Please call the venue for more information."
	else
		return Description
	end
end

local venue = Venues[ShoppingCart.TicketsCart.production.venue_id]
local venue_image_to_draw = "image_tickets_no_image_available"
local venue_map = kernel.ticketsvcs_registerimagefromcache( 3, 3, ShoppingCart.TicketsCart.production.venue_id )
if venue_map ~= nil then
	venue_image_to_draw = string.format( "cache:%s", venue_map )
end
kernel.setactorimage( "venue_map_art", venue_image_to_draw )

function fif(condition, if_true, if_false)
  if condition then return if_true else return if_false end
end

if venue ~= nil then
	kernel.setmacro( "VenueDetail:VenueName", venue.venue_name )
	kernel.log( "long_note=" .. fif(venue.long_note == nil, "",  venue.long_note) )
	kernel.setmacro( "VenueDetail:VenueDescription", fif( venue.long_note == nil, "", venue.long_note ) )
	kernel.log( "venue.address1=" .. fif( venue.address1 == nil, "nil", venue.address1 ) )
	kernel.setmacro( "VenueDetail:VenueAddressLine1", fif( venue.address1 == nil, "", venue.address1 ) )
	kernel.log( "venue.address2=" .. fif( venue.address2 == nil, "nil", venue.address2 ) )
	kernel.setmacro( "VenueDetail:VenueAddressLine2", fif( venue.address2 == nil, "", venue.address2 ) )
	kernel.log( "venue.city=" .. fif( venue.city == nil, "nil", venue.city ) )
	kernel.log( "venue.state=" .. fif( venue.state == nil, "nil", venue.state ) )
	kernel.log( "venue.zip=" .. fif( venue.zip == nil, "nil", venue.zip ) )
	kernel.setmacro( "VenueDetail:VenueCityStateZip", fif( venue.city == nil, "", venue.city) .. " " .. fif( venue.state == nil, "", venue.state ) .. ", " .. fif( venue.zip == nil, "", venue.zip ) )
	kernel.log( "venue.phone_number=" .. fif( venue.phone_number == nil, "nil", venue.phone_number ) )
	kernel.setmacro( "VenueDetail:VenuePhone", fif( venue.phone_number == nil, "", format_phone_number(venue.phone_number) ) )
	kernel.log( "venue.accessibility_information=" .. format_description( venue.accessibility_information, "accessibility" ) )
	kernel.setmacro( "VenueDetail:AccessibilityInfo", format_description( venue.accessibility_information, "accessibility" ) )
	kernel.log( "venue.parking_information=" .. format_description( venue.parking_information, "parking" ) )
	kernel.setmacro( "VenueDetail:ParkingInfo", format_description( venue.parking_information, "parking" ) )

	--
	-- If no address line 2, move the rest of the address up...
	--
	if venue.address2 == nil then
		kernel.moveactory( "venue_address_line_2", -18)
		kernel.moveactory( "venue_address_citystatezip", -18)
		kernel.moveactory( "venue_address_phone", -18)
	end

	kernel.log("All data retrieved!")
else
	kernel.log("No data!")
end

kernel.registeractorhitnotify(
	"back_button",
	function()
		view_analytics_event( "buttonPress", "back_button" )
		view_goback()
	end
)