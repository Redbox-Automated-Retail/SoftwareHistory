--
-- show-touchscreen-calibration.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

local msgbox = newmessageboxmanager()
local win_7_mt_touchscreen_calibration_path = "C:\\Program Files\\MicroTouch\\MT 7"
local win_10_mt_touchscreen_calibration_path = "C:\\Program Files (x86)\\MicroTouch\\MT 7"
local win_7_directory_exists = kernel.doesdirectoryexist(win_7_mt_touchscreen_calibration_path)
local win_10_directory_exists = kernel.doesdirectoryexist(win_10_mt_touchscreen_calibration_path)

local log_value = kernel.getlasthitactortag() or kernel.getlasthitactorname()
kernel.logto("FieldMaintenanceActionLog", log_value)

if win_7_directory_exists == false and win_10_directory_exists == false then
	msgbox:show("Could not find MicroTouch Touchscreen Calibration directory.", msgbox.ButtonTypeOK, msgbox.IconTypeError)
	return
end

local mt_touchscreen_calibration_path = win_7_mt_touchscreen_calibration_path
if win_7_directory_exists == false then
	mt_touchscreen_calibration_path = win_10_mt_touchscreen_calibration_path
end

local pid = kernel.startprocess(mt_touchscreen_calibration_path .. "\\TwUI.exe", "TwUICP.dll CPMain", false)
if pid < 0 then
	msgbox:show("Unable to start TwUI.exe.", msgbox.ButtonTypeOK, msgbox.IconTypeError)
end