<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="start_view" />
	<property name="version" value="2.0,Core,B" />
	<property name="active_flag" value="passive" />
	<property name="program_name" value="AB-Testing" />
	<property name="alternate_start_view" value="VER_NEW_LAYOUT" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="0, 0, 0"
			stylesheet="style_sheet_common"
			onEnter="start_view_enter"
			onLeave="start_view_leave">

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_kiosk_2_carousel_background"
				optionFlags="Image"
				stylesheet="style_sheet_common" />

			<actor
				name="stvw_movie_1"
				x="-53"
				y="269"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_5"
				x="973"
				y="269"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_2"
				x="186"
				y="269"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_4"
				x="734"
				y="269"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_3"
				x="425"
				y="200"
				opacity = "1.00"
				style="topten_movie_style_big" />

			<actor
				name="stvw_movie_6"
				x="1101"
				y="269"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_7"
				x="1101"
				y="269"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_8"
				x="1101"
				y="269"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_9"
				x="-300"
				y="269"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_10"
				x="-300"
				y="269"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_11"
				x="-300"
				y="269"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="movie_0_position"
				x="-300"
				y="269"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_1_position"
				x="-300"
				y="269"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_2_position"
				x="-300"
				y="269"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_3_position"
				x="-53"
				y="269"
				opacity="0.20"
				backgroundColor="black"
				optionFlags="Background"
				style = "topten_movie_position_style" />

			<actor
				name="movie_4_position"
				x="186"
				y="269"
				opacity="0.20"
				backgroundColor="black"
				optionFlags="Background"
				style = "topten_movie_position_style" />

			<actor
				name="movie_6_position"
				x="734"
				y="269"
				opacity="0.20"
				backgroundColor="black"
				optionFlags="Background"
				style="topten_movie_position_style" />

			<actor
				name="movie_7_position"
				x="973"
				y="269"
				opacity="0.20"
				backgroundColor="black"
				optionFlags="Background"
				style = "topten_movie_position_style" />

			<actor
				name="movie_8_position"
				x="1101"
				y="269"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_9_position"
				x="1101"
				y="269"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_10_position"
				x="1101"
				y="269"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_11_position"
				x="1101"
				y="269"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_5_position"
				x="425"
				y="200"
				foregroundColor="white"
				font="font_large"
				visible="false"
				textRegion="30,93,215,100"
				optionFlags="Image, DrawBorder, Text"
				style = "topten_center_movie_style" />

			<actor
				name="ticket_5_position"
				x="425"
				y="200"
				foregroundColor="white"
				font="font_large"
				visible="false"
				textRegion="30,93,215,100"
				optionFlags="Image, DrawBorder, Text"
				style = "topten_center_movie_style" />

			<actor
				name="movie_flip"
				x="425"
				y="200"
				backgroundColor="white"
				visible="false"
				optionFlags="Background, DrawBorder"
				style="topten_center_movie_style" />

			<actor
				name="movie_flip_cancel"
				x="663"
				y="173"
				height="56"
				width="56"
				text="X"
				font="font_akzidenz_bold_small"
				cornerSize="27,27"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="loading_titles_label"
				x="325"
				y="320"
				foregroundColor="153,14,22"
				text="Kiosk is Loading Titles. Please wait..."
				font="font_akzidenz_bold_small"
				visible="false"
				optionFlags="Text" />

			<actor
				name="add_to_cart"
				x="530"
				y="566"
				width="211"
				height="68"
				cornerSize="60,60"
				font="font_bryant_bold_18"
				text="    RENT IT NOW!"
				style="red_button_style"/>

			<actor
				name="plus_button"
				x="543"
				y="588"
				width="24"
				height="24"
				hitFlags="Enabled"
				image="image_plus"
				optionFlags="Image" />

			<actor
				name="selected_product_title"
				x="443"
				y="210"
				width="220"
				height="125"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_medium" />

			<actor
				name="national_street_date"
				x="443"
				y="235"
				width="238"
				height="125"
				visible="false"
				text="(${NationalStreetDate})"
				optionFlags="Text"
				font="font_akzidenz_bold_smaller" />

			<actor
				name="selected_product_description"
				x="443"
				y="270"
				width="223"
				height="160"
				visible="false"
				font="font_akzidenz_regular_tiny"
				optionFlags="Text" />

			<actor
				name="separator_1"
				x="435"
				y="378"
				width="238"
				height="2"
				visible="false"
				optionFlags="Background"
				backgroundColor="240,240,240" />

			<actor
				name="genre_label"
				x="445"
				y="385"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Genre:" />

			<actor
				name="genre"
				x="505"
				y="385"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="actors_label"
				x="445"
				y="430"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Actors: " />

			<actor
				name="actors"
				x="505"
				y="430"
				width="178"
				height="100"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="dates_label"
				x="445"
				y="410"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Dates: " />

			<actor
				name="dates"
				x="505"
				y="410"
				width="178"
				height="100"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="director_label"
				x="445"
				y="460"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Director: " />

			<actor
				name="director"
				x="505"
				y="460"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="running_time_label"
				x="445"
				y="480"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Time: " />

			<actor
				name="running_time"
				x="505"
				y="480"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="price_label"
				x="445"
				y="500"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Price: " />

			<actor
				name="price"
				x="505"
				y="500"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="price_extra_label"
				x="445"
				y="510"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Additional Days: " />

			<actor
				name="price_extra"
				x="505"
				y="510"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="return_label"
				x="445"
				y="520"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Return: " />

			<actor
				name="return"
				x="505"
				y="520"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="rating_label"
				x="445"
				y="540"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="MPAA Rating: " />

			<actor
				name="rating_description"
				x="545"
				y="540"
				width="140"
				height="100"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny" />

			<actor
				name="rating"
				x="545"
				y="540"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="separator_2"
				x="435"
				y="548"
				width="238"
				height="2"
				visible="false"
				optionFlags="Background"
				backgroundColor="240,240,240" />

			<actor
				name="offer_banner"
				hitFlags="Enabled"
				x="328"
				y="0"
				width="696"
				height="120"
				image="image_free_trial_home_banner"
				optionFlags="Image" />

			<actor
				name="navigation_side_bar"
				x="8"
				y="0"
				image="image_kiosk_2_side_bar"
				optionFlags="Image"
				hitFlags="Enabled" />

			<actor
				name="redbox_logo"
				x="0"
				y="0"
				image="image_kiosk_2_redbox_logo_v2"
				optionFlags="Image" />

			<actor
				name="offer_horz_line_top"
				x="328"
				y="0"
				width="696"
				height="3"
				optionFlags="Image"
				image="image_kiosk_2_offer_horz_line" />

			<actor
				name="offer_horz_line_bottom"
				x="328"
				y="117"
				width="696"
				height="3"
				optionFlags="Image"
				image="image_kiosk_2_offer_horz_line" />

			<actor
				name="all_movies_button"
				x="25"
				y="141"
				width="290"
				height="170"
				cornerSize="30, 30"
				onHit="browse_products"
				style="red_button_style" />

			<actor
				name="movie_button_icon"
				x="26"
				y="150"
				image="image_kiosk_2_movie_icon"
				optionFlags="Image" />

			<actor
				name="movie_button_rent_label"
				x="135"
				y="195"
				font="font_bryant_bold_20"
				foregroundColor="white"
				text="RENT"
				optionFlags="Text"  />
				
			<actor
				name="movie_button_movies_label"
				x="135"
				y="224"
				font="font_bryant_bold_34"
				foregroundColor="white"
				text="${StartView:AllMovies}"
				optionFlags="Text"  />

			<actor
				name="games_button"
				x="25"
				y="327"
				width="290"
				height="170"
				cornerSize="30,30"
				onHit="select_platform"
				style="red_button_style" />

			<actor
				name="game_button_icon"
				x="26"
				y="346"
				image="image_kiosk_2_game_icon"
				optionFlags="Image" />

			<actor
				name="games_button_rent_label"
				x="135"
				y="383"
				font="font_bryant_bold_20"
				foregroundColor="white"
				text="RENT"
				optionFlags="Text"  />
				
			<actor
				name="games_button_games_label"
				x="135"
				y="412"
				font="font_bryant_bold_34"
				foregroundColor="white"
				text="GAMES"
				optionFlags="Text"  />

			<actor
				name="return_button"
				x="25"
				y="620"
				width="290"
				height="77"
				cornerSize="60,60"
				font="font_bryant_bold_28"
				text="RETURN"
				textRegion="40, 0, 270, 77"
				onHit="return_product"
				style="kiosk_2_yellow_button_style" />

			<actor
				name="return_button_icon"
				x="54"
				y="634"
				image="image_kiosk_2_return_icon"
				optionFlags="Image" />

			<actor
				name="online_pickup_button"
				x="25"
				y="532"
				width="290"
				height="77"
				cornerSize="60,60"
				font="font_bryant_bold_28"
				text="PICK UP"
				textRegion="40, 0, 270, 77"
				onHit="pickup_reserved_product"
				style="kiosk_2_yellow_button_style" />

			<actor
				name="pickup_button_icon"
				x="54"
				y="548"
				image="image_kiosk_2_pickup_icon"
				optionFlags="Image" />

			<actor
				name="tickets_button"
				x="418"
				y="610"
				width="187"
				height="125"
				cornerSize="46,36"
				font="font_target_medium"
				text="TICKETS"
				image="image_tickets_button"
				hitFlags="Enabled"
				optionFlags="Image" />

			<actor
				name="espanol_button"
				x="770"
				y="678"
				width="148"
				height="62"
				text="ESPA&#209;OL"
				font="font_bryant_bold_18"
				onHit="switch_language"
				cornerSize="30,30"
				style="kiosk_2_blue_button_style" />
				
			<actor
				name="abe_turned_on_label"
				x="435"
				y="690"
				width="285"
				text="Screens will now be ADA-compatible.&#10;Use the buttons on the left to continue."
				foregroundColor="255, 255, 255"
				font="font_akzidenz_regular_tiny"
				optionFlags="Text"
				visible="false" />

			<actor
				name="handicap_button"
				x="695"
				y="678"
				width="62"
				height="62"
				cornerSize="31,31"
				image="image_kiosk_2_ada_icon"
				optionFlags="BackgroundGradient, DrawBorder"
				style="kiosk_2_blue_button_style"
				visible="false" />

			<actor
				name="handicap_button_icon"
				x="713"
				y="693"
				image="image_kiosk_2_ada_icon"
				optionFlags="Image"
				hitFlags="Enabled"
				visible="false" />

			<actor
				name="handicap_on_button"
				x="695"
				y="678"
				image="image_kiosk_2_ada_on_icon"
				hitFlags="Enabled"
				optionFlags="Image"
				visible="false" />

			<actor
				name="help_button"
				x="932"
				y="678"
				width="62"
				height="62"
				onHit="help"
				text="?"
				font="font_bryant_bold_28"
				cornerSize="31,31"
				style="kiosk_2_blue_button_style" />

			<actor
				name="burn_in_view_label"
				x="0"
				y="668"
				width="1024"
				height="60"
				font="font_target_large"
				text="touch here to rent or return"
				horizontalAlignment="center"
				foregroundColor="white"
				hitFlags="Enabled"
				optionFlags="Text"
				visible="false" />

		</scene>
	</aspect>
</resource>
