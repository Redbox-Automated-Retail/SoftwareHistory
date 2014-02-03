--
-- vmz-merch-rebalances-enter.lua
-- Copyright (C) 2013 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:stop()
if kernel.timerexists("vmz_merch_rebalances_timer") then
	kernel.timerremove("vmz_merch_rebalances_timer")
end
vmz_merch_rebalances_timer = newidletimer("vmz_merch_rebalances_timer")
vmz_merch_rebalances_timer:initialize(
	VMZMerchFlowTimeOut,
	function()
		-- TODO: handle any unsent analytics regarding current state
		vmz_merch_rebalances_timer:stop()
		kernel.timerremove("vmz_merch_rebalances_timer")
		kernel.healthsvcs_sendauditevent(
			StoreID,
			"VMZ_Merch_Rebalances_Timeout",
			username,
			"FMA Bundle",
			"VMZ Merch Rebalances Timeout" )
		vmz_merch_flow_functions:close( function() kernel.executescript("activate_rental") end )
	end
)
vmz_merch_rebalances_timer:start()

local titles_per_page = 8
local current_page = 0
local scroll_top = 333
local scroll_bottom = 419

-- Get the Rebalance Titles
local titles = ( vmz_merch_flow_functions or {} ).rebalance_in_data or {}

local function display_rebalances()
	local _master_product_catalaog = MasterProductCatalog
	local quantity_text = ""
	local title_text = ""
	
	-- Determine wich Quantities and Titles to show for this page
	local start_index = current_page * titles_per_page + 1
	for i = start_index, start_index + titles_per_page do
		local current_index = i - start_index + 1
		if i <= #titles then
			local product = _master_product_catalaog[titles[i].TitleId]
				if titles[i].Quantity ~= nil and titles[i].Quantity > 0 then
					kernel.setactortext( "rebalance_quantity_text_line" .. tostring( current_index ), 
										tostring( titles[i].Quantity ) )
					kernel.setactortext( "rebalance_title_text_line" .. tostring( current_index ), 
										tostring( product ~= nil and product.long_name 
											or "**Title Information Not Found**"  ) )
				end
		else 
			--clear text
			kernel.setactortext( "rebalance_quantity_text_line" .. tostring( current_index ), "" )
			kernel.setactortext( "rebalance_title_text_line" .. tostring( current_index ), "" )
		end
	end

	-- Position the scroll_track accordingly
	if titles_per_page >= #titles then
		-- all titles fit on first page, hide scroll bar assets
		kernel.disableactor( "rebalance_scroll_up" )
		kernel.hideactor( "rebalance_scroll_track" )
		kernel.disableactor( "rebalance_scroll_down" )
		kernel.disableactor( "scroll_up_background" )
		kernel.disableactor( "scroll_down_background" )
		kernel.disableactor( "scroll_bar_background" )
	else
		local scroll_pos = 0
		if current_page == 0 then
			scroll_pos = scroll_top
		elseif ( (current_page + 1) * titles_per_page) >= #titles then
			scroll_pos = scroll_bottom
		else
			-- Based on the number of pages in additional to the 1st page
			-- Move scroll tracker based on pages left to show
			local scroll_range = scroll_bottom - scroll_top
			local additional_pages_to_display = ( math.ceil( #titles / titles_per_page ) - 1)
			scroll_pos = scroll_top + ( scroll_range * ( current_page / additional_pages_to_display ) )
		end
		kernel.setactory( "rebalance_scroll_track", scroll_pos )
	end
end

kernel.registeractorhitnotify( 
	"rebalance_scroll_up",
	function()
		if current_page > 0 then
			kernel.logto( "FieldMaintenanceActionLog",  "VMZ Flow - Button 'Scroll Up' pressed")
			current_page = current_page - 1
			display_rebalances()
		end
	end
)

kernel.registeractorhitnotify(
	"rebalance_scroll_down",
	function()
		if ( ( current_page + 1 ) * titles_per_page ) < #titles then
			kernel.logto( "FieldMaintenanceActionLog",  "VMZ Flow - Button 'Scroll Down' pressed")
			current_page = current_page + 1
			display_rebalances()
		end
	end
)

kernel.registeractorhitnotify( 
	"rebalance_close_button",
	function()
		kernel.logto( "FieldMaintenanceActionLog",  "VMZ Flow - Button 'Close' pressed on page 'Load Rebalances'")
		view_goback( "vmz_merch_flow_view" )
	end
)

kernel.setmacro( "VMZRebalance:Title", tostring(strings["rebalance_header"]) )
kernel.setmacro( "VMZRebalance:Instructions", tostring(strings["rebalance_instructions"]) )
kernel.setmacro( "VMZRebalance:CloseButton", tostring(strings["rebalance_close_button"]) )
kernel.setmacro( "VMZRebalance:QuantityHeader", tostring(strings["rebalance_column_quantity"]) )
kernel.setmacro( "VMZRebalance:TitleHeader", tostring(strings["rebalance_column_title"]) )

display_rebalances()
