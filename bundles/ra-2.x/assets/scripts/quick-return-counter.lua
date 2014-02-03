--
-- quick-return-counter.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local _marker = "current"
local _table = getworktable( "cache", "QuickReturnCounter", false )

if _table[_marker] == nil then _table[_marker] = 0 end

QuickReturnCounter = {}

function QuickReturnCounter:increment()
	_table[_marker] = _table[_marker] + 1
end

function QuickReturnCounter:reset()
	_table[_marker] = 0
end

function QuickReturnCounter:value()
	return _table[_marker]
end

function QuickReturnCounter:submit()
	kernel.healthsvcs_enqueuestatistic( "QuickReturns", self:value() )
	self:reset()
end
