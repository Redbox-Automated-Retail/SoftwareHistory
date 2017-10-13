<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="blu_ray_upsell_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="48, 4, 5"
			stylesheet="style_sheet_common"
			onEnter="blu_ray_upsell_view_enter"
			onLeave="blu_ray_upsell_view_leave">
			
			<actor
				name="background"
				x="50"
				y="50"
				width="924"
				height="668"
				backgroundColor="white"
				borderColor="228,32,69"
				strokeWeight="5"
				cornerSize="28,28"
				optionFlags="Background, DrawBorder" />
				
			<actor
				name="header_large"
				x="0"
				y="79"
				width="1024"
				text="Upgrade to Blu-ray&#8482;"
				font="font_target_large_52"
				foregroundColor="228,32,69"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="header_small"
				x="0"
				y="175"
				width="1024"
				text="Upgrade to Blu-ray&#8482; for just a few cents more."
				font="font_target_medium_20"
				foregroundColor="black"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="footer"
				x="100"
				y="524"
				width="824"
				text="To play this Disc, you’ll need a Blu-ray™ player. The audio on Blu-ray Discs can range from Dolby Digital and DTS Digital to Dolby TrueHD and DTS-HD."
				font="font_target_smaller_overlays"
				foregroundColor="black"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="back_button"
				x="111"
				y="588"
				width="177"
				height="71"
				cornerSize="35,35"
				text="BACK"
				font="font_target_small_button_overlays"
				textRegion="71, 22, 66, 23"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="back_button_arrow"
				x="143"
				y="610"
				image="image_back_white_arrow"
				optionFlags="Image" />
				
			<actor
				name="no_thanks_button"
				x="498"
				y="588"
				width="177"
				height="71"
				cornerSize="35,35"
				text="NO THANKS"
				font="font_target_small_button_overlays"
				style="purple_button_style_white_border" />
				
			<actor
				name="upgrade_button"
				x="700"
				y="588"
				width="215"
				height="71"
				cornerSize="35,35"
				text="UPGRADE"
				font="font_target_small_button_overlays"
				style="rubine_button_style_white_border" />
		</scene>
	</aspect>
</resource>