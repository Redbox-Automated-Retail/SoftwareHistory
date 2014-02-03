--
-- check-box-hit.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

if kernel.getlasthitactorname() == nil then
	return
end

if check_box_state[kernel.getlasthitactorname()] ~= nil then
	check_box_state[kernel.getlasthitactorname()] = nil
	kernel.setactorimage( 
		kernel.getlasthitactorname(), 
		"check_box" )
else
	check_box_state[kernel.getlasthitactorname()] = kernel.getactortag( kernel.getlasthitactorname() ) or true
	kernel.setactorimage( 
		kernel.getlasthitactorname(),
			 "check_box_checked")
end



