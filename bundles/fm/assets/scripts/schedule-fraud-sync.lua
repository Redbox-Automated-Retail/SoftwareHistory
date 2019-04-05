--
-- schedule-maintenance-sync.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto( "FieldMaintenanceActionLog", log_value )

local progress_manager = newprogressmanager()
progress_manager:start( false )
	
kernel.schedule_fraud_sync( schedule_time.backing, username )
	
job_grid:display(true)
inventory_sync_reset()
progress_manager:stop()
	