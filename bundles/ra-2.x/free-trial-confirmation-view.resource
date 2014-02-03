<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="free_trial_confirmation_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			stylesheet="style_sheet_common"
			onEnter="free_trial_confirmation_view_enter">			
			<actor
				name="scene_background"
				x="0"
				y="630"
				width="1024"
				height="138"
				backgroundColor="58, 54, 55"
				optionFlags="Background" />	
		
			<actor
				name="header_label"
				x="85"
				y="120"
				width="980"
				text="Thanks for signing up!"
				font="font_target_larger"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />

			<actor
				name="message_label"
				x="85"
				y="240"
				width="900"
				text="Check your email for next steps.&#10;&#10;You should be receiving it shortly."
				font="font_target_medium"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />
			
			<actor
				name="ok_button"
				x="810"
				y="660"
				width="170"
				height="66"
				text="            OK"
				font="font_target_small"
				cornerSize="30, 19"
				style="red_next_button_style"
				visible="True"/>		
		</scene>
	</aspect>
</resource>
