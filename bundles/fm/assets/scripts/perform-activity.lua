--
-- perform-activity.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

kernel.launchsecurebrowser(
	kernel.gettable( "approved_urls" ),
		function( browse_type, url )
			kernel.healthsvcs_sendauditevent(
				StoreID,
				browse_type,
				username,
				"FMA_SecureBrowser",
				"URL: " .. url )
		end )

