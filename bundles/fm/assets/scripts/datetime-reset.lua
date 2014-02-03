--
-- datetime-reset.lua
-- Copyright (C) 2009 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

working_date = newdatetime()
working_date = working_date - "00:05:00"
refresh_actors(working_date)
