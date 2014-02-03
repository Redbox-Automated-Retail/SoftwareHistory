<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="browse_view_ada_accessible" />
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
				height="59"
				gradientAngle="90"
				backgroundColor="25,25,25"
				gradientTargetColor="8,8,8"
				optionFlags="BackgroundGradient"/>

			<actor
				name="lowerpart_gray_background"
				x="0"
				y="622"
				optionFlags="Background"
				backgroundColor="60,56,57"
				width="1024"
				height="145"
				visible="true"/>
				
<!-- All Product Box Art Browse Buttons -->

			<actor
				name="movie_1"
				x="64"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_1"
				x="50"
				y="92"
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
				name="add_movie_1"
				x="153"
				y="264"
				text="1"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_2"
				x="247"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_2"
				x="233"
				y="92"
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
				name="add_movie_2"
				x="338"
				y="264"
				text="2"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_3"
				x="434"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_3"
				x="420"
				y="92"
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
				name="add_movie_3"
				x="523"
				y="264"
				text="3"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_4"
				x="618"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_4"
				x="604"
				y="92"
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
				name="add_movie_4"
				x="708"
				y="264"
				text="4"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_5"
				x="803"
				y="114"
				style="browse_movie_style" />

			<actor
				name="flag_5"
				x="789"
				y="92"
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
				name="add_movie_5"
				x="894"
				y="264"
				text="5"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_6"
				x="64"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_6"
				x="50"
				y="335"
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
				name="add_movie_6"
				x="153"
				y="507"
				text="6"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_7"
				x="247"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_7"
				x="233"
				y="335"
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
				name="add_movie_7"
				x="338"
				y="507"
				text="7"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_8"
				x="434"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_8"
				x="420"
				y="335"
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
				name="add_movie_8"
				x="523"
				y="507"
				text="8"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_9"
				x="618"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_9"
				x="604"
				y="335"
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
				name="add_movie_9"
				x="708"
				y="507"
				text="9"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

			<actor
				name="movie_10"
				x="803"
				y="358"
				style="browse_movie_style" />

			<actor
				name="flag_10"
				x="789"
				y="335"
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
				name="add_movie_10"
				x="894"
				y="507"
				text="10"
                font="font_akzidenz_bold"
				style="browse_add_movie_accessible_style" />

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
				x="342"
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
				x="342"
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
				x="452"
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
				name="for_sale_button"
				x="562"
				y="578"
				width="94"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="Se Vende"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

<!-- Filtering and Sorting Buttons - GAMES -->

			<actor
				name="xbox_360_button"
				x="202"
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
				x="312"
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
				x="402"
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
				x="492"
				y="578"
				height="30"
				width="74"
				cornerSize="16,16"
				strokeWeight="2"
				text="Wii U"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />


<!-- ADA Accessible Add Buttons at the bottom -->

			<actor
                name="add_movie_lower_1"
                x="240"
                y="636"
				text="1"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_2"
                x="315"
                y="636"
				text="2"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_3"
                x="390"
                y="636"
				text="3"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_4"
                x="465"
                y="636"
				text="4"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_5"
                x="540"
                y="636"
				text="5"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_6"
                x="240"
                y="695"
				text="6"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_7"
                x="315"
                y="695"
				text="7"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_8"
                x="390"
                y="695"
				text="8"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_9"
                x="465"
                y="695"
				text="9"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

			<actor
                name="add_movie_lower_10"
                x="540"
                y="695"
				text="10"
                font="font_akzidenz_bold_small" 
                style="browse_add_movie_accessible_style_small" />

<!-- Action Zone -->

			<actor
				name="bottom_section_border"
				x="0"
				y="621"
				width="1024"
				height="1"
				backgroundColor="78,78,78"
				optionFlags="Background" />

			<actor 
				name="redbox_logo_border"
				x="0"
				y="622"
				width="230"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"
				visible="true" />

			<actor
				name="redbox_logo"
				x="50"
				y="667"
				width="133"
				height="40"
				hitFlags="Enabled"
				style="redbox_logo" />
				

			<actor
				name="startover_button"
				x="8"
				y="570"
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
                y="583"
                hitFlags="None"
                image="image_start_over_arrow"
                optionFlags="Image" />

			<actor
				name="add_to_cart_ada_label"
				x="230"
				y="740"
				width="375"
				height="28"
				hitFlags="None"
				foregroundColor="234,234,234"
				text="Seleccione la pintura o el n&#250;mero y alquilelo."
				verticalAlignment="center"
				horizontalAlignment="center"
				font="font_akzidenz_regular_smaller"
				optionFlags="Text"
				/>

			<actor
				name="tab_switch_product_family_border"
				x="605"
				y="622"
				width="151"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"
				visible = "true" />	
				

			<actor
				name="tab_switch_product_family"
				x="627"
				y="638"
				width="106"
				height="106"
				cornerSize="52,52"		
				style="red_button_style" />

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

<!-- Cart full Message -->

			<actor
				name="cart_full_background"
				x="0"
				y="0"
				width="1024"
				height="622"
				backgroundColor="0,0,0"
				opacity="0.6"
				hitFlags="None"
				optionFlags="Background"
				visible="false" />

			<actor
				name="cart_full_label_border"
				x="203"
				y="150"
				width="657"
				height="429"
				cornerSize="28,28"
				strokeWeight="5"
				borderColor="162,16,24"
				backgroundColor="white"
				hitFlags="None"
				optionFlags="Background, DrawBorder"
				visible="false" />

            <actor
				name="cart_full_label"
				x="265"
				y="196"
				width="500"
				height="200"
				foregroundColor="181,18,27"
				hitFlags="None"
				text="Su Caja Esta Llena"
				font="font_target_large_overlays"
				optionFlags="Text"
				visible="false" />

			<actor
				name="cart_full_label_text"
				x="265"
				y="283"
				width="500"
				height="200"
				foregroundColor="103,101,101"
				hitFlags="None"
				text="Finaliza ahora o hacer cambios."
				font="font_target_small_overlays"
				optionFlags="Text"
				visible="false" />
				
			<actor
                name="cart_full_cancel_button"
				x="629"
				y="485"
                width="171"
                height="66"
                cornerSize="32,19" 
				font="font_target_small_button_overlays"
                text="CONTINUAR"
				hitFlags="enabled"
				onHit="hide_cart_full"
				visible="false"
                style="red_button_style" />
				
<!-- MRated Warning Message-->
			<actor
				name="scene_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				backgroundColor="0,0,0"
				opacity="0.8"
				optionFlags="Background"
				visible="false"	/>
				<!--No other actor in this resource can be used to replace the above actor-->
			
			<actor
				name="center_background"
				x="203"
				y="150"
				width="657"
				height="429"
				cornerSize="28,28"
				strokeWeight="5"
				optionFlags="Background , DrawBorder"
				borderColor	="138,7,24"
				backgroundColor="255,255,255"
				visible="false"	/>
				<!-- except the border color difference the above actor can be replaced with cart_full_label_border-->
				
			<actor
				name="rating_label"
				x="265"
				y="196"
				width="575"
				height="60"
				font="font_target_medium_overlays"
				optionFlags="Text"
				horizontalAlignment="near"
				foregroundColor="181, 18, 27"
				text= "${MatureWarning:Header}"
				visible="false"	/>

			<actor
				name="message_label"
				x="265"
				y="283"
				width="537"
				height="230"
				font="font_target_smaller_overlays"
				foregroundColor="103,101,101"
				optionFlags="Text"
				horizontalAlignment="near"
				text="${MatureWarning:Message}"
				visible="false"	/>	
				
			 <actor
				name="bluray_message_label"
				x="265"
				y="283"
				width="542"
				height="230"
				font="font_target_small_overlays"
				foregroundColor="103,101,101"
				optionFlags="Text"
				horizontalAlignment="near"
				text="${Bluray:Message}"
				visible="false"	/>
			
			<actor
				name="mature_rating_back_button"
				x="265"
				y="485"
				width="191"
				height="66"
				text="ATR&#193;S"
				cornerSize="32,15"
				font="font_target_small_button_overlays"
				visible="false"	
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="mature_rating_back_button_arrow"
				x="287"
				y="507"
				hitFlags="Enabled"
				tag="back" 
				image="image_back-white-arrow-gray-background"
				optionFlags="image" 
				visible="false"	/>		
				
			<actor
                name="continue_button"
                x="629"
                y="485"
                width="171"
                height="66"
                cornerSize="32,19" 
				font="font_target_small_button_overlays"
                text="CONTINUAR"
				tag="continue_button"
				hitFlags="enabled"
                visible="false"	
				style="red_button_style" />
				
			<actor
                name="ok_button"
                x="629"
                y="485"
                width="171"
                height="66"
                cornerSize="32,19" 
				font="font_target_small_button_overlays"
                text="CONTINUAR"
				tag="ok_button"
				hitFlags="enabled"
                visible="false"
                style="red_button_style" />
				
			<actor
                name="subplatform_button"
                x="615"
                y="452"
                width="171"
                height="66"
                cornerSize="32,19" 
				font="font_target_small"
                text="CONTINUAR"
				hitFlags="enabled"
				tag="subplatform_button"
                visible="false"
                style="red_button_style" />
				
			<actor
                name="purchase_continue_button"
                x="629"
                y="485"
                width="171"
                height="66"
                cornerSize="32,19" 
				font="font_target_small_button_overlays"
                text="CONTINUAR"
				hitFlags="enabled"
				tag="purchase_continue_button"
                visible="false"
                style="red_button_style" />
				
			<actor
				name="subplatform_message_label"
				x="265"
				y="283"
				width="544"
				height="230"
				text="${Subplatform:Message}"
				font="font_target_small_overlays"
				foregroundColor="103,101,101"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="false" />
				
			<actor
				name="paddle_shadow"
				x="0"
				y="-3"
				width="1024"
				height="766"
				visible="false"
				hitFlags="None"
				style="paddle_shadow_style" /> 

<!-- Filter Results Empty -->
			<actor
				name="image_filter_empty_background"
				x="0"
				y="63"
				width="1024"
				height="500"
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

<!-- Header Background and Title Text -->

			<actor
				name="toppart_gray_background"
				x="0"
				y="0"
				optionFlags="Background"
				backgroundColor="60,56,57"
				width="1024"
				height="70"
				visible="true"/>

			<actor
				name="title_browsing_h1"
				x="40"
				y="0"
				height="70"
				width="944"
				hitFlags="None"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="center"
				visible="true"
				font="font_target_medium_large"
				text="${Browse:ProductFamily}" 
				optionFlags="Text" />
				

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
				font="font_target_large"
				text="" />

		</scene>
	</aspect>
</resource>
