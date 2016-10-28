--
-- email-optin-view-enter.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local email_optin_campaign = EmailOptinCampaign

kernel.registeractorhitnotify(
	"accept_optin_button",
	function()
		view_analytics_event( "buttonPress", "accept-optin-button" )
		view_analytics_event( "context", "campaign:".. email_optin_campaign.campaign_id )
		kernel.timerremove( "optin_view_timer" )
		local email_optin = {}
		table.insert( email_optin,
			{
				item_id = tostring( 0 ),
				item_type = "Non_Item_Specific",
				optin_method_type = "EmailAddress",
				optin_method_data = ShoppingCart.email,
				template_name = email_optin_campaign.email_template_name,
				campaign_name = email_optin_campaign.campaign_id
			}
		)
		kernel.promosvcs_kioskoptin(
			StoreManager:get_number(),
			email_optin,
			nil
		)
		continue_vend()
	end
)

kernel.registeractorhitnotify(
	"reject_optin_button",
	function()
		view_analytics_event( "buttonPress", "reject-optin-button" )
		view_analytics_event( "context", "campaign:".. email_optin_campaign.campaign_id )
		kernel.timerremove( "optin_view_timer" )
		continue_vend()
	end
)

local timeout = kernel.ttsgettimeout( "short", 10000 )
kernel.timercreate(
	"optin_view_timer",
	timeout,
	timeout,
	function()
		view_analytics_event( "context", "campaign:".. email_optin_campaign.campaign_id )
		kernel.timerremove( "optin_view_timer" )
		continue_vend()
	end
)

kernel.setactortext( "email_optin_offer_text", strings.email_optin_offer_message )
kernel.setactortext( "accept_optin_button", strings.yes_button )
kernel.setactortext( "reject_optin_button", strings.no_thanks_button)

kernel.setactorimage("campaign_image_placeholder", email_optin_campaign.campaign_image)

kernel.timerstart( "optin_view_timer" )