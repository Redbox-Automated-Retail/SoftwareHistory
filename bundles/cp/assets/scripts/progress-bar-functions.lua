--
-- progress-bar-functions.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--
function newprogressmanager()
	local t = {}

	t.start = function(
		self,
		width,
		height )

		if width == nil then
			width = 1024
		end
		if height == nil then
			height = 768
		end

		self:add_background( width, height )
		kernel.createactor(
			"progress_bar",
			false,
			(width-32)/ 2,
			(height-32)/2,
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

	t.add_background = function(
		self,
		width,
		height )
		if width == nil then
			width = 1024
		end

		if height == nil then
			height = 768
		end

		kernel.createactor(
			"progress_bar_bg",
			false,
			0,
			0,
			width,
			height,
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
