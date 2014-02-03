<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="platform_selection_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="platform_selection_view_enter">
			
			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="background_sunburst"
				optionFlags="Image"	/>
		
			<actor
				name="title_label"
				x="512"
				y="79"
				text="${Strings:select_a_platform}"
				font="font_target_large_46"
				foregroundColor="black"
				horizontalAlignment="Center"
				optionFlags="Text"
				visible="true" />

			<actor
				name="xbox_360_button"
				x="40"
				y="193"
				optionFlags="Image"
				hitFlags="Enabled"
				image="image_xbox_360_button"
				visible="true"  />
			
			<actor
				name="xbox_one_button"
				x="40"
				y="395"
				image="image_xbox_one_button"
				optionFlags="Image"
				hitFlags="Enabled"
				visible="true" />
				
			<actor
				name="ps3_button"
				x="367"
				y="193"
				image="image_ps3_button"
				optionFlags="Image"
				hitFlags="Enabled"
				visible="true" />
			
			<actor
				name="ps4_button"
				x="367"
				y="395"
				image="image_ps4_button"
				optionFlags="Image"
				hitFlags="Enabled"
				visible="true" />
				
			<actor
				name="wii_button"
				x="686"
				y="193"
				image="image_wii_button"
				optionFlags="Image"
				hitFlags="Enabled"
				visible="true" />
				
			<actor
				name="wii_u_button"
				x="686"
				y="395"
				image="image_wii_u_button"
				optionFlags="Image"
				hitFlags="Enabled"
				visible="true" />

			<actor
				name="back_button"
				x="40"
				y="673"
				width="175"
				height="73"
				text="    ${Strings:back_button}"
				cornerSize="32,15"
				font="font_target_small_button_overlays"
				visible="true"
				style="light_gray_with_gray_border_button_style"
				onHit="start_over" />

			<actor
				name="back_button_arrow"
				x="70"
				y="698"
				image="image_back_white_arrow"
				optionFlags="image"
                visible="true" />

			<actor
				name="view_all_button"
				x="687"
				y="673"
				width="294"
				height="73"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text="${Strings:view_all_games}"
				visible="true"	
				style="red_button_style" />
		</scene>
	</aspect>
</resource>
