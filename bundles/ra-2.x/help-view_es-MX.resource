<?xml version="1.0" encoding="utf-8" ?>
<resource type="view">
	<property name="name" value="help_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="0, 0, 0"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="help_view_enter">
			
			<actor
				name="upperpart_gray_background_border"
				x="0"
				y="0"
				width="1024"
				height="265"
				optionFlags="Background"
				backgroundColor="60,60,60" />
				
			<actor
				name="upperpart_gray_background"
				x="0"
				y="0"
				width="1024"
				height="568"			
				backgroundColor="234, 234, 234"
				optionFlags="Background" />
			
			 <actor
				name="center_gray_background"
				x="0"
				y="567"
				width="1024"
				height="68"
				gradientAngle="90"
				backgroundColor="159,159,159"
				gradientTargetColor="159,159,159"
				optionFlags="BackgroundGradient"/>	
		
			<actor
				name="title_label"
				x="25"
				y="14"
				font="font_target_medium"
				foregroundColor="44, 46, 50"
				textRegion="0,0,512,32"
				optionFlags="Text"
				text="${Help:Title}" />
				
			<actor
				name="contact_redbox_label"
				x="110"
				y="29"
				width="600"
				height="40"
				font="font_akzidenz_regular_small"
				foregroundColor="44, 46, 50"
				horizontalAlignment="near"
				optionFlags="Text"
				text="&#191;Necesita ayuda? 1.866.REDBOX3 (1.866.733.2693)" />
				

			<actor
				name="image_container"
				x="58"
				y="90"
				width="907"
				height="460"
				optionFlags="Image" />
			
			<actor
				name="draw_line"
				x="19"
				y="69"
				width="985"
				height="3"
				visible="true"
				optionFlags="Background"
				backgroundColor="44, 46, 50" />	

			<actor
				name="page_counter"
				x="470"
				y="72"
				width="600"
				height="40"
				font="font_akzidenz_regular_tiny"
				foregroundColor="44, 46, 50"
				horizontalAlignment="near"
				optionFlags="Text"
				text="" />

			<actor
				name="back_button"
				x="23"
				y="580"
				width="114"
				height="32"
				text="      ATR&#193;S"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,26"
				strokeWeight="2"
				borderColor="white"
				onHit="go_back"
				style="gray_button_style" />
   
			<actor
				name="back_button_arrow"
				x="40"
				y="587"
				width="21"
				height="18"
				image="image_back_white_arrow"
				optionFlags="Image" />
			 
			<actor
				name="selected_button"
				x="287"
				y="580"
				height="32"
				width="110"				
				visible="false"
				font="font_akzidenz_regular_smaller"
				hitFlags="Enabled"
				image="image_selected_button"
				text="${HelpButtonText}"
				horizontalAlignment="center"
				verticalAlignment="center"
				optionFlags="Image, Text" />

			<actor
				name="faq_button"
				x="376"
				y="580"
				width="110"
				height="32"
				onHit="show_faq"
				text="FAQ Alquiler"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,26"
				strokeWeight="2"
				borderColor="white"
				style="gray_button_style" />

			<actor
				name="faq_tickets_button"
				x="751"
				y="580"
				width="110"
				height="32"
				onHit="show_faq_tickets" 
				text="FAQs Boletos"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,24"
				strokeWeight="2"
				visible="true"
				borderColor="white"
				style="gray_button_style" />

			<actor
				name="terms_button"
				x="505"
				y="580"
				width="135"
				height="32"
				onHit="show_terms"
				text="T&#233;rminos Alquiler"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,24"
				strokeWeight="2"
				borderColor="white"
				style="gray_button_style" />

			<actor
				name="terms_tickets_button"
				x="801"
				y="580"
				width="135"
				height="32"
				onHit="show_terms_tickets" 
				text="T&#233;rminos Boletos"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,24"
				strokeWeight="2"
				visible="true"
				borderColor="white"
				style="gray_button_style" />
				
			<actor
				name="terms_redbox_card_button"
				x="659"
				y="580"
				width="135"
				height="32"
				onHit="show_terms_redbox_card"
				text="T&#233;rminos Tarjeta"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,24"
				strokeWeight="2"
				borderColor="white"
				style="gray_button_style" />
				
			<actor
				name="privacy_button"
				x="813"
				y="580"
				width="110"
				height="32"
				onHit="show_privacy"
				text="Privacidad"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,24"
				strokeWeight="2"
				borderColor="white"
				style="gray_button_style" />
				
			<actor
				name="unplayable_return_button"
				x="786"
				y="580"
				width="180"
				height="32"
				onHit="unplayable_return" 
				text="Disco defectuoso"
				font="font_akzidenz_regular_smaller"
				cornerSize="17,24"
				strokeWeight="2"
				borderColor="white"
				style="gray_button_style" />
				
			<actor
				name="action_zone_background_border"
				x="0"
				y="622"
				width="1024"
				height="69"
				optionFlags="Background"
				backgroundColor="60,60,60" /> 
			
			<actor
				name="action_zone_background"
				x="0"
				y="624"
				optionFlags="Background"
				backgroundColor="48, 48, 48"
				width="1024"
				height="146"
				visible="true" />	
				
			<actor
				name="left_arrow"
				x="0"
				y="197"
				width="53"
				height="292"
				onHit="help_page_back"
				style="left_arrow_style" />
				
			<actor
				name="right_arrow"
				x="971"
				y="197"
				width="53"
				height="292"
				onHit="help_page_next"
				style="right_arrow_style" />

			<actor
				name="redbox_logo"
				x="50"
				y="667"
				width="133"
				height="40"
				hitFlags="Enabled"
				onHit="activate_fa" 
				style="redbox_logo" />

			<actor
				name="store_number_label"
				x="864"
				y="29"
				width="90"
				font="font_akzidenz_regular_16"
				optionFlags="Text"
				foregroundColor="black"
				text="Kiosk ID:" />

			<actor
				name="store_number_text"
				x="945"
				y="29"
				width="110"
				font="font_akzidenz_regular_16"
				optionFlags="Text"
				foregroundColor="black"
				text="${StoreNumber}" />

		</scene>
	</aspect>
</resource>
