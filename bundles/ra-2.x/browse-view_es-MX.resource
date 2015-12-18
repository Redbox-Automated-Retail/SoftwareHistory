<?xml version="1.0" encoding="utf-8"?>
<resource type="view">
	<property name="name" value="browse_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="24, 24, 24"
			stylesheet="style_sheet_common"
			onEnter="browse_view_enter"
			onLeave="browse_view_leave">

<!-- Page Background Graphics -->

			<actor
				name="center_gray_background"
				x="0"
				y="567"
				width="1024"
				height="68"
				gradientAngle="90"
				backgroundColor="25,25,25"
				gradientTargetColor="8,8,8"
				optionFlags="BackgroundGradient" />

			<actor
				name="lowerpart_gray_background"
				x="0"
				y="622"
				optionFlags="Background"
				backgroundColor="60,56,57"
				width="1024"
				height="145"
				visible="true" />

<!-- SPECIAL HEADER ONLY USED FOR TITLE MARKETING VERSION -->

			<actor
				name="title_marketing_h1"
				x="40"
				y="25"
				height="40"
				width="944"
				optionFlags="Text"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="center"
				font="font_target_medium_large"
				text="" />

<!-- Filtering and Sorting Buttons - MOVIES -->


			<actor
				name="blu_ray_button"
				x="287"
				y="578"
				height="30"
				width="94"
				cornerSize="16,16"
				strokeWeight="2"
				text="Blu-ray"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="selected_button"
				x="287"
				y="578"
				height="30"
				width="94"
				visible="false"
				font="font_akzidenz_regular_smaller"
				hitFlags="Enabled"
				image="image_selected_button"
				text="${SortName}"
				horizontalAlignment="center"
				verticalAlignment="center"
				optionFlags="Image, Text" />

			<actor
				name="newest_button"
				x="397"
				y="578"
				height="30"
				width="94"
				cornerSize="16,16"
				strokeWeight="2"
				text="&#218;ltimos"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="abc_button"
				x="507"
				y="578"
				width="94"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="A-Z"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="for_sale_button"
				x="617"
				y="578"
				width="94"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="Se Vende"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="coming_soon_button"
				x="727"
				y="578"
				height="30"
				width="110"
				cornerSize="16,16"
				strokeWeight="2"
				text="Pr&#243;ximamente"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

<!-- Filtering and Sorting Buttons - GAMES -->

			<actor
				name="xbox_360_button"
				x="152"
				y="578"
				height="30"
				width="94"
				cornerSize="16,16"
				strokeWeight="2"
				text="Xbox 360"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="xbox_one_button"
				x="152"
				y="578"
				height="30"
				width="94"
				cornerSize="16,16"
				strokeWeight="2"
				text="Xbox One"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="ps3_button"
				x="262"
				y="578"
				width="74"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="PS3"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />
				
			<actor
				name="ps4_button"
				x="262"
				y="578"
				width="74"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="PS4"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="wii_button"
				x="352"
				y="578"
				height="30"
				width="74"
				cornerSize="16,16"
				strokeWeight="2"
				text="Wii"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="wii_u_button"
				x="442"
				y="578"
				height="30"
				width="74"
				cornerSize="16,16"
				strokeWeight="2"
				text="Wii U"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

<!-- Start Over Button -->

			<actor
				name="startover_button"
				x="8"
				y="573"
				width="121"
				height="46"
				onHit="start_over"
				addToFront="true"
				text="P&#225;gina Principal"
				font="font_target_smaller"
				cornerSize="22,22"
				strokeWeight="2"
				textRegion="25,1,85,38"
				style="gray_button_style" />

			<actor
				name="startover_arrow"
				x="20"
				y="586"
				hitFlags="None"
				image="image_start_over_arrow"
				optionFlags="Image" />

		</scene>
	</aspect>
</resource>
