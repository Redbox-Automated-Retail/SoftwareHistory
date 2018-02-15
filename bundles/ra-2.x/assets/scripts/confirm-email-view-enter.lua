--
-- confirm-email-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

kernel.configure_confirm_email_view( do_marketing_confirm ~= nil, ShoppingCart.email, continue_vend )
