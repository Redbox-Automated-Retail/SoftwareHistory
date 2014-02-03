<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="in_cart_offer_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			onEnter="in_cart_offer_message_view_enter"
			onLeave="in_cart_offer_message_view_leave"
			backgroundColor="transparent"
			stylesheet="style_sheet_common">
			
			<actor
				name="in_cart_offer_scene_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				backgroundColor="0,0,0"
				opacity="0.6"
				optionFlags="Background" />
		
			<actor
				name="in_cart_offer_center_background"
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
				name="header_message_label"
				x="190"
				y="142"
				width="644"
				height="45"
				text="${InCartPromoHeaderMessage}"
				font="font_target_medium"
				foregroundColor="181,18,27"
				horizontalAlignment="near"
				optionFlags="Text" />

			<actor
				name="body_message_label"
				x="190"
				y="229"
				width="644"
				height="349"
				text="${InCartPromoBodyMessage}"
				font="font_target_small_15"
				foregroundColor="103,101,101"
				horizontalAlignment="near"
				optionFlags="Text" />

			<actor
				name="no_button"
				x="190"
				y="578"
				width="191"
				height="66"
				text="${InCartPromoNoButton}"
				cornerSize="32,15"
				font="font_target_small_button_overlays"
				visible="true"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="yes_button"
				x="643"
				y="578"
				width="191"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text="${InCartPromoYesButton}"
				visible="true"	
				style="red_button_style" />

		</scene>
	</aspect>
</resource>
