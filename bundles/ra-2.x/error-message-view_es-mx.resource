<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="error_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="228,32,69" 
			onEnter="error_message_view_enter"
			onLeave="error_message_view_leave" >

		<actor
			name="message_label"
			x="85"
			y="220"
			width="700"
			height="400"
			text="${MessageText}"
			font="font_target_larger"
			foregroundColor="white"
			horizontalAlignment="near"
			optionFlags="Text" />
		
		</scene>
	</aspect>
</resource>