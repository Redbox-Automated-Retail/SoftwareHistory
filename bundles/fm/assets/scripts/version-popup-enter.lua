--
-- version-popup-enter.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
local skipped = {}
skipped["Hal Client"] = true
skipped["Hal Service"] = true
skipped["Kiosk Engine"] = true
idle_timer:start()
local reboottime = kernel.getmachinevalueordefault(
	"Store",
	"RebootTime",
	nil )
local store_id_text = StoreID
local reboottime_text = " (No reboot)"
if reboottime ~= nil
and #string.gsub(reboottime, " ", "") > 0 then
	reboottime_text = " (Reboot Time: ".. reboottime.. ")"
end
store_id_text = store_id_text.. reboottime_text
kernel.setactortext( "store_id", store_id_text )
local timezone = kernel.gettimezone()

--inventory and profile dates
if ProfileVersion ~= nil
and ProfileVersion.origin_machine ~= nil
and ProfileVersion.generated_on ~= nil then
    kernel.setmacro(
        "ProfileVersion",
        string.format( "%s %s %s", ProfileVersion.origin_machine.name, kernel.tolocaltime( ProfileVersion.generated_on.date, "MMMM d h:mm tt" ), timezone ) )
else
    kernel.setmacro( "ProfileVersion", "N/A" )
end

if InventoryVersion ~= nil
and InventoryVersion.origin_machine ~= nil
and InventoryVersion.generated_on ~= nil then
    kernel.setmacro(
        "InventoryVersion",
        string.format( "%s %s %s", InventoryVersion.origin_machine.name, kernel.tolocaltime( InventoryVersion.generated_on.date, "MMMM d h:mm tt" ), timezone ) )
else
    kernel.setmacro( "InventoryVersion", "N/A" )
end

local path = kernel.getmacro( "RunningPath" ) .. "..\\..\\Data\\"
local rebalancesversion = path .. "RebalanceVersion.json"
local versionDate = kernel.tablefromfileserializeddictionary( rebalancesversion )
if versionDate ~= nil and versionDate["VersionTimeStampUTC"] ~= nil then
    kernel.setmacro(
        "RebalancesVersion",
        string.format( "%s %s", kernel.tolocaltime( versionDate["VersionTimeStampUTC"], "MMMM d h:mm tt" ), timezone ) )
else
    kernel.setmacro( "RebalancesVersion", "none" )
end


if use_rapid_thin_file() == true then
	kernel.setmacro(
		"ThinVersion",
		string.format( "%s %s %s", RapidThinVersion.origin_machine.name, kernel.tolocaltime( RapidThinVersion.generated_on.date, "MMMM d h:mm tt" ), timezone ) )
else
	kernel.setmacro( "ThinVersion", kernel.getmacro( "InventoryVersion") )
end


local DisplayData = function (rows, x, y)
	for k, v in pairs(rows) do
		local labelName = k .. "_label_key"
		kernel.createactor(
			labelName, 
			false, 
			x, 
			y, 
			450, 
			25, 
			tostring( v["key"] ), 
			"medium_font", 
			nil, 
			"Gray", 
			"White", 
			nil, 
			nil, 
			nil, 
			nil, 
			"Text, TextShadow",
			"None")
		x = x + 450
		labelName = k .. "_label_value"
		kernel.createactor(
			labelName, 
			false, 
			x, 
			y, 
			200, 
			25, 
			tostring( v["val"] ), 
			"medium_font", 
			nil, 
			"Gray", 
			"White", 
			nil, 
			nil, 
			nil, 
			nil, 
			"Text, TextShadow",
			"None")
		x = x - 450
		y =	y + 25
	end
	return x, y
end


-- Set starting location of first actor
local x = 40
local y = 255

local versions = kernel.getsoftwareversions()
local rows = {}
local index = 0
local should_skip
for k, v in pairs( versions ) do
	should_skip = skipped[k]
	if (should_skip == nil) then
		local newrow = {}
		newrow["key"] = k
		newrow["val"] = v
		rows[index] = newrow
		index = index + 1
	end
end
table.sort( rows, function( a, b ) return a["key"] < b["key"] end )
x, y = DisplayData( rows, x, y )



