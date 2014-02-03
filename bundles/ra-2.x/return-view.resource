<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="return_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="return_view_enter"
			onLeave="return_view_leave">
			
			<actor
				name="return_message_label"
				x="69"
				y="251"
				text="Please insert your disc."
				font="font_target_largest"
				foregroundColor="white"
				verticalAlignment="near"
				horizontalAlignment="near"
				textRegion="0,0,550,250"
				optionFlags="Text"
				visible = "false" />
				
			<actor
				name="redbox-disc-background_image"
				x="700"
				y="155"
				height="465"
				width="342"
				image="image_redbox_disc_case_plan"
				horizontalAlignment="center"
				verticalAlignment="center"
				hitFlags="None"
				visible="false"
				optionFlags="Image" />
			
			<actor
				name="redbox_logo"
				x="795"
				y="350"
				width="220"
				height="70"
				style="redbox_logo"
				visible = "false" />
				
			<actor
				name="return_back_button"
				x="35"
                y="665"
                width="194"
                height="66"
                cornerSize="34,34"
                font="font_target_small"
                text="BACK"
				visible="true" 
				style="red_button_style" />
			
			<actor
				name="return_back_button_arrow"
				x="65"
				y="686"
				image="image_back_white_arrow"
				optionFlags="image"
                visible="true" />

		</scene>
	</aspect>
</resource>
