--
-- localization-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function localization_init()
	culture_idle_timer = newidletimer( "culture_idle_timer" )
	culture_idle_timer:initialize(
		50000,
		function()
			kernel.timerstop("carousel-timer")
			kernel.timerstop("carousel-user-timer")
			localization_toggle_culture()
			refresh_start_view()
			kernel.timerwait(100)
			kernel.timerstart("carousel-timer")
		end )
	kernel.setculture( DefaultCulture )
	localization_reload_resources()
end

function localization_toggle_culture()
	if kernel.getculture() == DefaultCulture then
		kernel.log( "Toggling Culture to ".. OtherCulture)
		kernel.setculture( OtherCulture )
		culture_idle_timer:start()
	elseif kernel.getculture() == OtherCulture then
		kernel.log( "Toggling Culture to ".. DefaultCulture)
		kernel.setculture( DefaultCulture )
		culture_idle_timer:stop()
	end
	localization_reload_resources()
end

function localization_reload_resources()
	faq = kernel.gettable( "faqs" )
	terms = kernel.gettable( "terms" )
	privacy = kernel.gettable( "privacy" )
	terms_redbox_card = kernel.gettable( "terms_redbox_card" )
	
	strings = kernel.getdictionary( "strings" )
	LoadStringsMacros()
	if SetESRBRatings ~= nil then
		SetESRBRatings()
	end
end


function LoadStringsMacros()
	-- Loads items from strings.resource into macros based on current culture.
	for k, v in pairs(strings) do
		kernel.setmacro("Strings:"..k, v)
	end
end