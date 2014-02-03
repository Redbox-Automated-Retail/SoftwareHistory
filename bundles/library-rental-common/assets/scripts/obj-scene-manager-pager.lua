--
-- obj-scene-manager-pager.lua
-- Copyright (C) 2010 Redbox Automated Retail, LLC.
-- All Rights Reserved
--

function newscenemanager()
	local t = {
		add_actor = function(
				self,
				name,
				x,
				y,
				width,
				height,
				parms,
				onhitfunction)
			if parms == nil then
				parms = {}
			end
			kernel.createactor(
				name,
				parms.addToFront or false,
				x,
				y,
				width,
				height,
				parms.text,
				parms.font,
				parms.image,
				parms.backgroundColor,
				parms.foregroundColor,
				parms.textRegion,
				parms.textTranslationPoint,
				parms.textRotationAngle,
				parms.hotSpot,
				parms.renderOptions,
				parms.hitOptions )
			if parms.verticalAlignment ~= nil then
				kernel.setactortextaligny( name, parms.verticalAlignment )
			end
			if parms.horizontalAlignment ~= nil then
				kernel.setactortextalignx( name, parms.horizontalAlignment )
			end
			if parms.opacity ~= nil then
				kernel.setactoropacity( name, parms.opacity )
			end
			if onhitfunction ~= nil then
				kernel.registeractorhitnotify( name, onhitfunction )
			end
			if parms.tabs ~= nil then
				kernel.setactortabstops( name, parms.tabs )
			end
			if parms.tag ~= nil then
				kernel.setactortag( name, parms.tag )
			end
			if parms.cornerSize ~= nil and parms.cornerSize.width ~= nil and parms.cornerSize.height ~= nil then
				kernel.setcornersize( name, parms.cornerSize.width, parms.cornerSize.height )
			end
			if parms.borderColor ~= nil then
				kernel.setbordercolor( name, parms.borderColor )
			end
			if parms.strokeWeight ~= nil then
				kernel.setborderstrokeweight( name, parms.strokeWeight )
			end
			if parms.gradientTargetColor ~= nil then
				kernel.setactorgradienttargetcolor( name, parms.gradientTargetColor )
			end
			if parms.gradientAngle ~= nil then
				kernel.setactorgradientangle( name, parms.gradientAngle )
			end
			
			table.insert( self.actors, name )
		end,

		remove_actor = function( self, name )
			kernel.removeactor( name )
			table.remove( self.actors, name )
		end,

		clear = function( self )
			kernel.removeactors( self.actors )
			self.actors = {}
		end,

		actors = {}
	}
	return t
end

function newpager()
	local t = {
		next_changed = nil,

		prev_changed = nil,

		page_number = function( self )
			return #self.previous_pages.values + 1
		end,

		mark_current_page = function( self )
			self.last_start_index = self.current_index
			self.in_page_index = 1
		end,

		reset = function( self )
			if self.next_changed ~= nil then
				self.next_changed( false )
			end
			if self.prev_changed ~= nil then
				self.prev_changed( false )
			end
			self.previous_pages:clear()
			self.in_page_index = 0
			self.current_index = 0
			self.last_start_index = 0
			self.is_valid = true
		end,

		update_paging = function( self, size )
			if self.next_changed ~= nil then
				self.next_changed( self.current_index + self.in_page_index < size )
			end
			if self.prev_changed ~= nil then
				self.prev_changed( self:has_previous_pages() )
			end
		end,

		invalidate = function( self )
			self.is_valid = false
		end,

		get_index = function( self )
			return self.current_index + self.in_page_index
		end,

		increment_index = function( self )
			self.in_page_index = self.in_page_index + 1
		end,

		page_next = function( self )
			self.current_index = self.current_index + self.in_page_index
			self.previous_pages:push( self.last_start_index )
			self.in_page_index = 1
		end,

		page_back = function( self )
			self.current_index = self.previous_pages:pop()
			self.in_page_index = 1
		end,

		has_previous_pages = function( self )
			return #self.previous_pages.values > 0
		end,

		previous_pages = {
			clear = function( self )
				self.values = {}
			end,
			push = function( self, value )
				table.insert( self.values, value )
			end,
			pop = function( self )
				local size = #self.values
				local value = self.values[size]
				table.remove( self.values, size )
				return value
			end,
			values = {}
		},

		in_page_index = 0,

		current_index = 0,

		last_start_index = 0,

		is_valid = false
	}
	return t
end

