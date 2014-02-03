--
-- obj-progress-bar-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
function newprogressmanager()
	local t = {}

	t.start = function( self )
		self:add_background()
		kernel.createactor(
			"progress_bar",
			false,
			(1024-32)/2,
			(768-32)/2,
			32,
			32,
			nil,
			nil,
			"spinner",
			nil,
			nil,
			nil,
			nil,
			nil,
			nil,
			"Image",
			"Obstruction")
		kernel.timercreate(
			"progress_timer",
			0,
			75,
			function()
				kernel.setactorframe("progress_bar")
			end )
		kernel.timerstart( "progress_timer" )
	end

	t.stop = function( self )
		self:remove_background()
		kernel.removeactor("progress_bar")
		if kernel.timerexists( "progress_timer" ) then
			kernel.timerstop( "progress_timer" )
			kernel.timerremove( "progress_timer" )
		end
	end

	t.add_background = function( self )
		kernel.createactor(
			"progress_bar_bg",
			false,
			0,
			0,
			1024,
			768,
			nil,
			nil,
			nil,
			"black",
			nil,
			nil,
			nil,
			nil,
			nil,
			"Background",
			nil)
		kernel.setactoropacity( "progress_bar_bg", .50 )
	end

	t.remove_background = function( self )
		kernel.removeactor( "progress_bar_bg" )
	end

	return t
end
