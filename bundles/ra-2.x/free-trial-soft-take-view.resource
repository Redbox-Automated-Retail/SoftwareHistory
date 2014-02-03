<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="free_trial_soft_take_popup" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			stylesheet="style_sheet_common"
			onEnter="free_trial_soft_take_popup_enter">			
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
				text="Sorry, we're having some trouble."
				font="font_target_large"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />

			<actor
				name="message_label_line1"
				x="85"
				y="240"
				width="900"
				text="We can't process your signup at the kiosk."
				font="font_target_medium"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />
			
			<actor
				name="message_label_line2"
				x="85"
				y="320"
				width="900"
				text="We are sending you an email with more information on how to get your free trial."
				font="font_target_medium"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />
			
			<actor
				name="message_label_line3"
				x="85"
				y="440"
				width="900"
				text="For more information on the free trial, please go to redbox.com/instant."
				font="font_target_medium"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />
			
			<actor
				name="freetrial_soft_take_done_button"
				x="794"
				y="660"
				width="196"
				height="66"
				cornerSize="32,32"
				text="DONE"
				font="font_target_small"
				style="red_button_style"
				visible="True"/>		
		</scene>
	</aspect>
</resource>
