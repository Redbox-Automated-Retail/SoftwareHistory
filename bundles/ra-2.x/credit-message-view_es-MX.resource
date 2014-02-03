<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="credit_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common">
			
			<actor name="scene_background"
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
				backgroundColor="white"
				hitFlags="None"
				optionFlags="Background, DrawBorder" /> 
				
			<actor
				name="credit_message_label"
				x="190"
				y="229"
				width="644"
				height="349"
				text="${credit_MessageText}"
				font="font_target_small_overlays"
				foregroundColor="103,101,101"
				horizontalAlignment="near"
				optionFlags="Text" />

			<actor
				name="credit_first_button"
				x="190"
				y="578"
				width="229"
				height="69"
				text=""
				cornerSize="32,15"
				font="font_target_small_medium"
				visible="false"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
                name="credit_second_button"
				x="643"
				y="578"
                width="191"
                height="69"
                cornerSize="32,19" 
				font="font_target_small_button_overlays"
                text=""
                visible="false"	
				style="red_button_style" />

		</scene>
	</aspect>
</resource>
