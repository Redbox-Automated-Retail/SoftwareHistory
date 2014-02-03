<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="two_line_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundImage="image_red_diagonal_stripes"
			onEnter="two_line_message_view_enter"
			onLeave="two_line_message_view_leave" >

		<actor
			name="message_label_header"
			x="85"
			y="135"
			width="800"
			height="100"
			text="${HeaderMessageText}"
			font="font_target_larger"
			foregroundColor="white"
			horizontalAlignment="near"
			optionFlags="Text" />
		
		<actor
			name="message_label_one"
			x="85"
			y="264"
			width="850"
			height="400"
			text="${LineOneMessageText}"
			font="font_target_medium_30"
			foregroundColor="white"
			horizontalAlignment="near"
			optionFlags="Text" />
		
		<actor
			name="message_label_two"
			x="85"
			y="415"
			width="900"
			height="400"
			text="${LineTwoMessageText}"
			font="font_target_medium_30"
			foregroundColor="white"
			horizontalAlignment="near"
			optionFlags="Text" />
		
		</scene>
	</aspect>
</resource>
