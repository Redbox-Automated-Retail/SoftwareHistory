<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="free_trial_cancel_popup" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			onEnter="free_trial_cancel_popup_enter"
			backgroundColor="transparent"
			stylesheet="style_sheet_common">
			<actor
				name="scene_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				backgroundColor="0,0,0"
				opacity="0.6"
				optionFlags="Background" />

			<actor
				name="center_background"
				x="128"
				y="96"
				width="768"
				height="576"
				cornerSize="28,28"
				strokeWeight="5"
				optionFlags="Background , DrawBorder"
				borderColor	="138,7,24"
				backgroundColor="255,255,255"
				visible="true"	/>

			<actor
				name="header_label"
				x="190"
				y="142"
				width="644"
				height="45"
				font="font_target_large_overlays"
				optionFlags="Text"
				horizontalAlignment="near"
				foregroundColor="181, 18, 27"
				text= "Cancel signup?"
				visible="true"	/>
			
			<actor
				name="message_label"
				x="190"
				y="229"
				width="644"
				height="349"
				font="font_target_smaller_overlays"
				foregroundColor="103,101,101"
				optionFlags="Text"
				horizontalAlignment="near"
				text="Are you sure you don't want to get a free month of&#10;Redbox Instant by Verizon?"
				visible="true"	/>

			<actor
				name="back_button"
				x="190"
				y="578"
				width="191"
				height="66"
				text="BACK"
				cornerSize="32,15"
				font="font_target_small_button_overlays"
				visible="true"	
				style="light_gray_with_gray_border_button_style" />

			<actor
				name="back_button_arrow"
				x="212"
				y="600"
				hitFlags="Enabled"
				tag="back"
				image="image_back-white-arrow-gray-background"
				optionFlags="image" 
				visible="true"	/>		
		
			<actor
				name="continue_button"
				x="663"
				y="578"
				width="171"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text="YES"
				tag="continue_button"
				hitFlags="enabled"
				visible="true"
				style="red_button_style" />				
		</scene>
	</aspect>
</resource>
