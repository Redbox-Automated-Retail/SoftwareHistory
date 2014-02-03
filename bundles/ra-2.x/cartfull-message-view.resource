<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="cartfull_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
			onEnter="cartfull_message_view_enter">
			<actor
				name="cart_full_scene_background"
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
				<!-- except the border color difference the above actor can be replaced with cart_full_label_border-->

			<actor
				name="warning_header_label"
				x="190"
				y="142"
				width="644"
				height="45"
				font="font_target_large_overlays"
				optionFlags="Text"
				horizontalAlignment="near"
				foregroundColor="181, 18, 27"
				text= "${CartfullWarning:Header}"
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
				text="${CartfullWarning:Message}"
				visible="true"	/>
		
			<actor
				name="cart_full_continue_button"
				x="663"
				y="578"
				width="171"
				height="66"
				cornerSize="32,19"
				font="font_target_small_button_overlays"
				text="CONTINUE"
				onHit="hide_cart_full"
				visible="true"
				style="red_button_style" />
				
		</scene>
	</aspect>
</resource>
