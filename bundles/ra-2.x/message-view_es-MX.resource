<?xml version="1.0" encoding="utf-8"?>
<resource type="view">
	<property name="name" value="message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
			onEnter="message_view_enter"
			onLeave="message_view_leave">
			
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
				borderColor="162,16,24"
				backgroundColor="255,255,255"
				hitFlags="None"
				optionFlags="Background, DrawBorder" />
				
			<actor
				name="title_timeout_label"
				x="190"
				y="142"
				width="644"
				height="45"
				text="${MessageTimeoutTitle}"
				font="font_target_large"
				foregroundColor="181, 18, 27"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />
				
			<actor
				name="title_label"
				x="190"
				y="142"
				width="644"
				height="45"
				text="${MessageTitle}"
				font="font_target_large_overlays"
				foregroundColor="181, 18, 27"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />

			<actor
				name="message_header_label"
				x="190"
				y="142"
				width="644"
				height="45"
				text="${MessageHeaderText}"
				font="font_target_medium_large"
				foregroundColor="181,18,27"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />

			<actor
				name="blacklist_message_header"
				x="190"
				y="142"
				width="644"
				height="45"
				text="${BlacklistMessageLabel}"
				font="font_target_medium"
				foregroundColor="181, 18, 27"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />

			<actor
				name="unplayable_message_header"
				x="190"
				y="142"
				width="644"
				height="45"
				text="${UnplayableMessageLabel}"
				font="font_target_medium_large"
				foregroundColor="181, 18, 27"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />

			<actor
				name="message_label"
				x="190"
				y="229"
				width="644"
				height="349"
				text="${MessageText}"
				font="font_target_small_overlays"
				foregroundColor="103,101,101"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />
			
			<actor
				name="message_label_text"
				x="190"
				y="229"
				width="644"
				height="349"
				text="${MessageLabelText}"
				font="font_target_smaller_overlays"
				foregroundColor="103,101,101"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />
				
			<actor
				name="first_button"
				x="190"
				y="578"
				width="191"
				height="66"
				text=""
				cornerSize="32,15"
				font="font_target_small_button_overlays"
				visible="false"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="second_button"
				x="663"
				y="578"
				width="171"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text=""
				visible="false"	
				style="red_button_style" />
				
			<actor
                name="third_button"
				x="663"
				y="578"
				width="171"
				height="66"
                cornerSize="32,19" 
				font="font_target_small_medium"
                text=""
                visible="false"	
				style="red_button_style" />
				
			<actor
				name="fourth_button"
				x="190"
				y="578"
				width="171"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_medium"
				text=""
				visible="false"	
				style="red_button_style" />
				
			<actor
				name="back_button_arrow"
				x="212"
				y="600"
				tag="back"
				image="image_back-white-arrow-gray-background"
				visible="false"	
				optionFlags="image" />
		</scene>
	</aspect>
</resource>
