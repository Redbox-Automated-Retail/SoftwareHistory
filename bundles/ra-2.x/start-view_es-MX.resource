<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="start_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<property name="culture" value="es-MX" />
    <aspect name="content">
		<scene
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
				image="start_view_background_lighter"
				optionFlags="Image"
				stylesheet="style_sheet_common" />

			<actor
				name="redbox_logo"
				x="245"
				y="36"
				width="159"
				height="47"
				style="redbox_logo_red" />
			
			<actor
				name="logo_label"
				x="420"
				y="35"
				optionFlags="Text"
				foregroundColor="153,14,22"
				font="font_target_large_38"
				text="los diez mejores" />
				
			<actor
				name="stvw_movie_1"
				x="-62"
				y="150"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_5"
				x="869"
				y="150"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_2"
				x="159"
				y="150"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_4"
				x="648"
				y="150"
				opacity="1.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_3"
				x="380"
				y="120"
				opacity = "1.00"
				style="topten_movie_style_big" />

			<actor
				name="stvw_movie_6"
				x="1071"
				y="150"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_7"
				x="1071"
				y="150"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_8"
				x="1071"
				y="150"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_9"
				x="-300"
				y="150"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_10"
				x="-300"
				y="150"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="stvw_movie_11"
				x="-300"
				y="150"
				opacity="0.00"
				style="topten_movie_style" />

			<actor
				name="shadow_movie_1"
				x="119"
				y="150"
				width="40"
				height="318"
				opacity="0.50"
				style="right_arrow_shadow_style" />

			<actor
				name="shadow_movie_2"
				x="340"
				y="150"
				width="40"
				height="318"
				style="right_arrow_shadow_style" />

			<actor
				name="shadow_movie_4"
				x="648"
				y="150"
				width="40"
				height="318"
				style="left_arrow_shadow_style" />

			<actor
				name="shadow_movie_5"
				x="869"
				y="150"
				width="40"
				height="318"
				opacity="0.50"
				style="left_arrow_shadow_style" />
				
			<actor
				name="movie_0_position"
				x="-300"
				y="150"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_1_position"
				x="-300"
				y="150"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_2_position"
				x="-300"
				y="150"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />
				
			<actor
				name="movie_3_position"
				x="-62"
				y="150"
				opacity="0.30" 
				backgroundColor="black"
				optionFlags="Background"
				style = "topten_movie_position_style" />

			<actor
				name="movie_4_position"
				x="159"
				y="150"
				opacity="0.30"
				backgroundColor="black"
				optionFlags="Background"
				style = "topten_movie_position_style" />

			<actor
				name="movie_6_position"
				x="648"
				y="150"
				opacity="0.30"
				backgroundColor="black"
				optionFlags="Background"
				style="topten_movie_position_style" />

			<actor
				name="movie_7_position"
				x="869"
				y="150"
				opacity="0.30" 
				backgroundColor="black"
				optionFlags="Background"
				style = "topten_movie_position_style" />

			<actor
				name="movie_8_position"
				x="1071"
				y="150"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_9_position"
				x="1071"
				y="150"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_10_position"
				x="1071"
				y="150"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_11_position"
				x="1071"
				y="150"
				visible="false"
				opacity="0.00"
				style = "topten_movie_position_style" />

			<actor
				name="movie_5_position"
				x="380"
				y="120"
				foregroundColor="white"
				font="font_large"
				visible="false"
				textRegion="30,93,215,100"
				optionFlags="Image, DrawBorder, Text" 
				style = "topten_center_movie_style" />

			<actor
				name="ticket_5_position"
				x="380"
				y="120"
				width="268"
				height="388"
				opacity="0.0"
				foregroundColor="black"
				visible="false"
				textRegion="30,93,215,100"
				optionFlags="Background"
				hitFlags="Enabled"
				style = "topten_movie_position_style" />

			<actor
				name="movie_flip"
				x="380"
				y="120"
				backgroundColor="white"
				visible="false"
				optionFlags="Background, DrawBorder" 
				style="topten_center_movie_style" />

			<actor
				name="movie_flip_cancel"
				x="618"
				y="93"
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
                x="486"
                y="474"
                width="208"
                height="58"
                cornerSize="30,20"
                font="font_target_small_medium"
                text="     iAlquilelo Ahora!"
                style="red_button_style" />
			
			<actor
				name="plus_button"
				x="495"
                y="492"
				width="24"
                height="24"
				hitFlags="Enabled"
				image="image_plus"
				optionFlags="Image" />
			
			<actor
				name="selected_product_title"
				x="398"
				y="130"
				width="220"
				height="125"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_small" />

			<actor
				name="national_street_date"
				x="398"
				y="155"
				width="238"
				height="125"
				visible="false"
				text="(${NationalStreetDate})"
				optionFlags="Text"
				font="font_akzidenz_bold_smaller" />
			
			<actor
				name="selected_product_description"
				x="398"
				y="190"
				width="223"
				height="160"
				visible="false"
				font="font_akzidenz_regular_tiny"
				optionFlags="Text" />
				
			<actor
				name="separator_1"
				x="390"
				y="298"
				width="238"
				height="2"
				visible="false"
				optionFlags="Background"
				backgroundColor="240,240,240" />	
			
			<actor
				name="genre_label"
				x="400"
				y="305"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="G&#233;nero: " />
				
			<actor
				name="genre"
				x="460"
				y="305"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="actors_label"
				x="400"
				y="350"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Protagonistas: " />
			
			<actor
				name="actors"
				x="500"
				y="350"
				width="125"
				height="100"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="dates_label"
				x="400"
				y="330"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Dates: " />

			<actor
				name="dates"
				x="460"
				y="330"
				width="178"
				height="100"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="director_label"
				x="400"
				y="380"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Director: " />
				
			<actor
				name="director"
				x="466"
				y="380"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="running_time_label"
				x="400"
				y="400"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Duraci&#243;n: " />
				
			<actor
				name="running_time"
				x="466"
				y="400"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="price_label"
				x="400"
				y="420"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Precio: " />
				
			<actor
				name="price"
				x="466"
				y="420"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="price_extra_label"
				x="400"
				y="430"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="D&#237;as Adicionales: " />

			<actor
				name="price_extra"
				x="466"
				y="430"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="return_label"
				x="400"
				y="440"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="Devolver: " />
				
			<actor
				name="return"
				x="466"
				y="440"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="rating_label"
				x="400"
				y="460"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				text="MPAA Rating: " />

			<actor
				name="rating_description"
				x="500"
				y="460"
				width="140"
				height="100"
				visible="false"
				optionFlags="Text"			
				font="font_akzidenz_bold_tiny" />

			<actor
				name="rating"
				x="500"
				y="460"
				visible="false"
				optionFlags="Text"
				font="font_akzidenz_regular_tiny" />

			<actor
				name="separator_2"
				x="390"
				y="468"
				width="238"
				height="2"
				visible="false"
				optionFlags="Background"
				backgroundColor="240,240,240" />	
			
			<actor
				name="online_pickup_button"
				x="24"
				y="631"
				width="187"
				height="96"
				cornerSize="46,36"
				onHit="pickup_reserved_product"
				style="red_button_style" />

      <actor
				name="online_red_box_label"
				x="24"
				y="680"
				width="187"
				height="43"
				horizontalAlignment="center"
				font="font_target_small"
				text="RESERVACI&#211;N"
				foregroundColor="white"
				hitFlags="Enabled"
				onHit="pickup_reserved_product"
				optionFlags="Text" />

			<actor
				name="online_pickup_label"
				x="24"
				y="645"
				width="187"
				height="43"
				horizontalAlignment="center"
				font="font_target_medium"
				text="RECOGER"
				foregroundColor="white"
				hitFlags="Enabled"
				onHit="pickup_reserved_product"
				optionFlags="Text" />

			<actor
				name="return_button"
				x="221"
				y="631"
				width="187"
				height="96"
				cornerSize="46,36"
				font="font_target_medium"
				text="DEVOLVER"
				onHit="return_product"
				style="red_button_style" />

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
				name="games_button"
				x="615"
				y="631"
				width="187"
				height="96"
				cornerSize="46,36"
				font="font_target_medium"
				text="JUEGOS"
				onHit="browse_products"
				style="red_button_style" />
				
			<actor
				name="all_movies_button"
				x="812"
				y="631"
				width="187"
				height="96"
				cornerSize="46,36"
				font="font_target_medium"
				text="${StartView:AllMovies}"
				onHit="browse_products"
				style="red_button_style" />
			
			<actor
				name="left_page_shadow"
				x="0"
				y="150"
				height="320"
				style="left_arrow_shadow_style" />

			<actor
				name="left_arrow_button"
				x="0"
				y="157"
				width="68"
				height="306"
				style="left_arrow_style" />

			<actor
				name="right_page_shadow"
				x="928"
				y="150"
				height="320"
				style="right_arrow_shadow_style" />

			<actor
				name="right_arrow_button"
				x="956"
				y="157"
				width="68"
				height="306"
				style="right_arrow_style" />

			<actor
				name="espanol_button"
				x="17"
				y="525"
				width="131"
				height="41"
				text="ENGLISH"
				font="font_akzidenz_bold_smaller"
				onHit="switch_language"
				cornerSize="20,26"
				style="gray_button_white_border_style" />

			<actor
				name="handicap_button"
				x="166"
				y="522"
				width="46"
				height="46"
				image="image_handicap_button"
				optionFlags="Image"
				hitFlags="Enabled"
				onHit="ada_accessible_mode" />

			<actor
				name="help_button"
				x="876"
				y="525"
				width="131"
				height="41"
				onHit="help"
				text="AYUDA"
				font="font_akzidenz_bold_smaller"
				cornerSize="20,26"
				style="gray_button_white_border_style" />

			<actor
				name="burn_in_view_label"
				x="0"
				y="630"
				width="1024"
				height="60"
				font="font_target_large"
				text="Toque aqui para alquiler o devolver"
				horizontalAlignment="center"
				foregroundColor="white"
				hitFlags="Enabled"
				optionFlags="Text"
				visible="false"/>
		</scene>
    </aspect>
</resource>
