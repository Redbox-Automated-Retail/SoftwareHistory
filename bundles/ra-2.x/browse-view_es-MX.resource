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
				name="bg_box_top"
				x="0"
				y="61"
				width="1024"
				height="258"
				gradientAngle="-90"
				backgroundColor="0,0,0"
				gradientTargetColor="26,26,26"
				optionFlags="BackgroundGradient" />

			<actor
				name="bg_box_bottom"
				x="0"
				y="322"
				width="1024"
				height="259"
				gradientAngle="-90"
				backgroundColor="0,0,0"
				gradientTargetColor="26,26,26"
				optionFlags="BackgroundGradient" />

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

			<actor
				name="paddle_shadow"
				x="0"
				y="4"
				width="1024"
				height="757"
				visible="false"
				hitFlags="None"
				style="paddle_shadow_style" />

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

<!-- TABS ACROSS THE TOP -->

			<actor
				name="tab_switch_product_family_border"
				x="882"
				y="0"
				width="144"
				height="76"
				style="product_family_switch_tab_border_style" />

			<actor
				name="tab_switch_product_family"
				x="884"
				y="0"
				width="142"
				height="74"
				text=""
				textRegion="10,12,130,45"
				verticalAlignment="center"
				horizontalAlignment="near"
				font="font_akzidenz_bold_smaller"
				style="product_family_switch_tab_style" />

			<actor
				name="tab_no_switch_product_family"
				x="882"
				y="0"
				width="144"
				height="69"
				style="unselected_tab_style"
				visible="false" />

			<actor
				name="image_next_white_arrow"
				x="999"
				y="27"
				image="image_next_white_arrow"
				hitFlags="enabled"
				optionFlags="Image" />

			<actor
				name="tab_1"
				x="0"
				y="0"
				text="TODAS PEL&#205;CULAS"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="selected_tab_style" />

			<actor
				name="tab_2"
				x="126"
				y="0"
				text="LOS &#201;XITOS 20"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_2_text_a"
				x="139"
				y="27"
				width="100"
				height="15"
				text="TOP"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				foregroundColor="white"/>

			<actor
				name="tab_2_text_b"
				x="225"
				y="25"
				width="40"
				height="15"
				text="20"
				optionFlags="Text"
				font="font_akzidenz_bold_11" 
				foregroundColor="white" />

			<actor
				name="tab_3"
				x="252"
				y="0"
				text="ACCI&#211;ON"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_4"
				x="378"
				y="0"
				text="DRAMA"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_5"
				x="504"
				y="0"
				text="TERROR"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_6"
				x="630"
				y="0"
				text="COMEDIA"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_7"
				x="756"
				y="0"
				text="FAMILIA"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

<!-- All Product Box Art Browse Buttons -->

			<actor
				name="movie_1"
				x="64"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_1"
				x="48"
				y="94"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_1"
				x="65"
				y="278"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_1"
				x="65"
				y="258"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_1"
				x="68"
				y="278"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_1"
				x="68"
				y="270"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_1"
				x="133"
				y="234"
				style="browse_add_movie_style" />

			<actor
				name="movie_2"
				x="247"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_2"
				x="231"
				y="94"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_2"
				x="248"
				y="278"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_2"
				x="248"
				y="258"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_2"
				x="251"
				y="278"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_2"
				x="251"
				y="270"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_2"
				x="318"
				y="234"
				style="browse_add_movie_style" />

			<actor
				name="movie_3"
				x="434"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_3"
				x="418"
				y="94"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_3"
				x="435"
				y="278"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_3"
				x="435"
				y="258"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_3"
				x="438"
				y="278"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_3"
				x="438"
				y="270"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_3"
				x="503"
				y="234"
				style="browse_add_movie_style" />

			<actor
				name="movie_4"
				x="618"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_4"
				x="602"
				y="94"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_4"
				x="619"
				y="278"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_4"
				x="619"
				y="258"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_4"
				x="622"
				y="278"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_4"
				x="622"
				y="270"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_4"
				x="688"
				y="234"
				style="browse_add_movie_style" />

			<actor
				name="movie_5"
				x="803"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_5"
				x="787"
				y="94"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_5"
				x="804"
				y="278"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_5"
				x="804"
				y="258"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_5"
				x="807"
				y="278"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_5"
				x="807"
				y="270"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_5"
				x="874"
				y="234"
				style="browse_add_movie_style" />

			<actor
				name="movie_6"
				x="64"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_6"
				x="48"
				y="339"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_6"
				x="65"
				y="522"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_6"
				x="65"
				y="502"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_6"
				x="68"
				y="522"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_6"
				x="68"
				y="514"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_6"
				x="133"
				y="477"
				style="browse_add_movie_style" />

			<actor
				name="movie_7"
				x="247"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_7"
				x="231"
				y="339"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_7"
				x="248"
				y="522"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_7"
				x="248"
				y="502"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_7"
				x="251"
				y="522"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_7"
				x="251"
				y="514"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_7"
				x="318"
				y="477"
				style="browse_add_movie_style" />

			<actor
				name="movie_8"
				x="434"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_8"
				x="418"
				y="339"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_8"
				x="435"
				y="522"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_8"
				x="435"
				y="502"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_8"
				x="438"
				y="522"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_8"
				x="438"
				y="514"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_8"
				x="503"
				y="477"
				style="browse_add_movie_style" />

			<actor
				name="movie_9"
				x="618"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_9"
				x="602"
				y="339"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_9"
				x="619"
				y="522"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_9"
				x="619"
				y="502"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_9"
				x="622"
				y="522"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_9"
				x="622"
				y="514"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_9"
				x="688"
				y="477"
				style="browse_add_movie_style" />

			<actor
				name="movie_10"
				x="803"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_10"
				x="787"
				y="339"
				visible="false"
				font="font_akzidenz_bold_tiny"
				style="browse_flag_spanish_style" />

			<actor
				name="banner_10"
				x="804"
				y="522"
				visible="false"
				style="browse_banner_style" />

			<actor
				name="banner_coming_soon_10"
				x="804"
				y="502"
				visible="false"
				style="browse_banner_coming_soon_style" />

			<actor
				name="button_email_reminder_10"
				x="807"
				y="522"
				visible="false"
				style="browse_button_email_reminder_spanish_style" />

			<actor
				name="button_email_reminder_set_10"
				x="807"
				y="514"
				visible="false"
				style="browse_button_email_reminder_set_spanish_style" />

			<actor
				name="add_movie_10"
				x="874"
				y="477"
				style="browse_add_movie_style" />

			<actor
				name="movie_11"
				x="-120"
				y="114"
				width="148"
				height="205"
				borderColor="60,56,57"
				opacity="0.2"
				optionFlags="Image" />

			<actor
				name="movie_13"
				x="986"
				y="114"
				width="148"
				height="205"
				borderColor="60,56,57"
				opacity="0.2"
				optionFlags="Image" />

			<actor
				name="movie_12"
				x="-120"
				y="358"
				width="148"
				height="205"
				borderColor="60,56,57"
				opacity="0.2"
				optionFlags="Image" />

			<actor
				name="movie_14"
				x="986"
				y="358"
				width="148"
				height="205"
				borderColor="60,56,57"
				opacity="0.2"
				optionFlags="Image" />



<!-- Browse Page Navigation and shadow effects -->

			<actor
				name="left_page_shadow"
				x="0"
				y="105"
				width="40"
				height="465"
				hitFlags="None"
				style="left_arrow_shadow_style" />

			<actor
				name="left_arrow_button"
				x="0"
				y="190"
				width="58"
				height="306"
				style="left_arrow_style" />

			<actor
				name="right_page_shadow"
				x="984"
				y="105"
				width="40"
				height="465"
				hitFlags="None"
				style="right_arrow_shadow_style" />

			<actor
				name="right_arrow_button"
				x="966"
				y="190"
				width="58"
				height="306"
				style="right_arrow_style" />


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


<!-- Mini Cart -->

			<actor
				name="selectedmovie_1_border"
				x="233"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_1"
				x="235"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_1_flag"
				x="235"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_1"
				x="290"
				y="625"
				height="43"
				width="43"
				text="X"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_2_border"
				x="348"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_2"
				x="350"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_2_flag"
				x="350"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_2"
				x="405"
				y="625"
				height="43"
				width="43"
				text="X"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_3_border"
				x="458"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_3"
				x="460"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_3_flag"
				x="460"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_3"
				x="515"
				y="625"
				height="43"
				width="43"
				text="X"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_4_border"
				x="572"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_4"
				x="574"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_4_flag"
				x="574"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_4"
				x="629"
				y="625"
				height="43"
				width="43"
				text="X"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_5_border"
				x="683"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_5"
				x="685"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_5_flag"
				x="685"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_5"
				x="740"
				y="625"
				height="43"
				width="43"
				text="X"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />


<!-- Action Zone -->

			<actor
				name="redbox_logo"
				x="50"
				y="667"
				width="133"
				height="40"
				style="redbox_logo" />

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

			<actor
				name="add_to_cart_label"
				x="253"
				y="674"
				width="499"
				height="41"
				hitFlags="None"
				foregroundColor="gray"
				text="Presione     para agregarlo al carrito"
				verticalAlignment="center"
				horizontalAlignment="center"
				strokeWeight="1"
				opacity=".8"
				backgroundColor="50, 50, 50"
				borderColor="80,80,80"
				font="font_akzidenz_regular_medium"
				optionFlags="Background, DrawBorder, Image, Text, TextShadow" />

			<actor
				name="add_to_cart_label_image"
				x="385"
				y="675"
				width="41"
				height="40"
				image="image_add_to_cart"
				hitFlags="None"
				opacity=".9"
				optionFlags="Image"
				/>

			<actor
				name="my_rentals_button"
				x="784"
				y="660"
				width="223"
				height="66"
				cornerSize="30,30"
				font="font_target_small_bold"
				text="CAJA (0)"
				onHit="view_shopping_cart"
				style="red_button_style" />

			<actor
				name="title_marketing_no_button"
				x="784"
				y="660"
				width="223"
				height="66"
				cornerSize="30,30"
				font="font_target_small_bold"
				text="NO GRACIAS"
				onHit="view_shopping_cart"
				style="gray_button_style"
				visible="false"
				/>

<!-- Pagination -->
			<actor
				name="browse_pagination_text"
				x="408"
				y="75"
				width="60"
				height="19"
				text=""
				foregroundColor="153,156,165"
				verticalAlignment="center"
				horizontalAlignment="center"
				hitFlags="None"
				visible="false"
				optionFlags="Text" />

			<actor
				name="browse_pagination_L_image"
				x="465"
				y="74"
				visible="false"
				style="pagination_left_style" />

			<actor
				name="browse_pagination_of_text"
				x="502"
				y="74"
				width="20"
				height="19"
				text="de"
				foregroundColor="153,156,165"
				verticalAlignment="center"
				horizontalAlignment="center"
				hitFlags="None"
				visible="false"
				optionFlags="Text" />

			<actor
				name="browse_pagination_R_image"
				x="525"
				y="74"
				visible="false"
				style="pagination_right_style" />

<!-- Filter Results Empty -->
			<actor
				name="image_filter_empty_background"
				x="0"
				y="70"
				width="1024"
				height="494"
				image="image_filter_empty_background"
				hitFlags="None"
				visible="false"
				optionFlags="Image"/>

			<actor
				name="filter_empty_text"
				x="54"
				y="298"
				width="916"
				height="140"
				text=""
				font="font_bryant_bold_30"
				foregroundColor="50,47,47"
				horizontalAlignment="center"
				hitFlags="None"
				visible="false"
				optionFlags="Text" />
				
		</scene>
	</aspect>
</resource>
