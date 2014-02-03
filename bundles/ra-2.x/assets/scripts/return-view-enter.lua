--
-- return-view-enter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

idle_timer:suspend()
culture_idle_timer:suspend()

kernel.pushmovie("return_movie", { x = 0, y = 112, width = 1024, height = 543 } )

local top_movie = tostring( kernel.peekmovie() )

kernel.log( "peekmovie() = " .. top_movie )
