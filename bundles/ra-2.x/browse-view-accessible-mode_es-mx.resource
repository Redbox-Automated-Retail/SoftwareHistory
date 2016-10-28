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
      
      <!-- Filtering and Sorting Buttons - MOVIES -->
      <actor
				name="top_20_filter_button"
				x="445"
				y="578"
				width="105"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="Los Éxitos 20"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

      <actor
				name="action_filter_button"
				x="566"
				y="578"
				width="75"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="Acción"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

      <actor
       name="drama_filter_button"
       x="657"
       y="578"
       width="75"
       height="30"
       cornerSize="16,16"
       strokeWeight="2"
       text="Drama"
       visible="true"
       font="font_akzidenz_regular_smaller"
       style="gray_button_style" />

      <actor
				name="horror_filter_button"
				x="748"
				y="578"
				width="75"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="Terror"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

      <actor
				name="comedy_filter_button"
				x="839"
				y="578"
				width="75"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="Comedia"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

      <actor
        name="family_filter_button"
        x="930"
        y="578"
        width="75"
        height="30"
        cornerSize="16,16"
        strokeWeight="2"
        text="Familia"
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
