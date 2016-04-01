--
-- credit-balance-view-enter.lua
-- Copyright (C) 2011 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()

local timeout = kernel.ttsgettimeout( "long", 40000 )
kernel.timercreate(
	"credit_balance_view_timer",
	timeout,
	timeout,
	function()
		ShoppingCart:prepos_vend_abort()
		view_unwindto_safe( "shopping_cart_view", false, "credit_balance_view_timer:timeout" )
	end
)
kernel.timerstart( "credit_balance_view_timer" )

kernel.registeractorhitnotify(
	"back_button",
	function()
		ShoppingCart:prepos_vend_abort()
		view_unwindto_safe( "shopping_cart_view", false, "credit_balance_view:back_button" )
	end
)

kernel.registeractorhitnotify(
	"confirm_button",
	function()
		--Remove credit_balance_view from the stack 
		view_goback_safe( "remove credit balance view", true )
		if GiftCardsEnabled == true and ShoppingCart:is_gift_card() == true then
			gc_authorize_func()			
		elseif key_pad_next_authorize_func ~= nil then
			key_pad_next_authorize_func()
		else
			ShoppingCart:prepos_vend_abort()
			view_unwindto_safe( "shopping_cart_view", false, "credit_balance_view:confirm_button(key_pad_next_authorize_func = nil)" )
		end
	end
)