<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="free_trial_whoops_popup" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			backgroundImage="image_red_diagonal_stripes"
			onEnter="free_trial_whoops_popup_enter"
			onLeave="" >

			<actor
				name="message_label1"
				x="85"
				y="120"
				width="980"
				text="Whoops! Something went wrong."
				font="font_target_large"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />

			<actor
				name="message_label2"
				x="85"
				y="240"
				width="900"
				text="Sorry, we can't complete your signup at this time.&#10;&#10;To get your free trial, please go to redbox.com/instant&#10;&#10;or try at a different Redbox location."
				font="font_target_medium"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text" />
				
			<actor
				name="free_trial_error_message_ok_button"
				x="794"
				y="660"
				width="196"
				height="66"
				cornerSize="32,32"
				text="OK"
				font="font_target_small"
				style="red_button_style"
				visible="True"/>		
				
		</scene>
	</aspect>
</resource>
