<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_choose_quantity_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_choose_quantity_view_enter">

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_background"
				optionFlags="Image" />

			<actor
				name="event_label"
				x="75"
				y="64"
				width="874"
				height="50"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_23_bold"
				horizontalAlignment="near"
				optionFlags="Text, ElideText"
				text="${ChooseQuantity:EventName}"
				visible="true" />

			<actor
				name="event_art"
				x="75"
				y="108"
				width="407"
				height="300"
				optionFlags="Image, DrawBorder"
				borderColor="60, 56, 57"
				visible="true" />
				
			<actor
				name="event_short_note"
				x="525"
				y="108"
				width="435"
				height="180"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				visible="false"
				optionFlags="Text" />
				
			<actor
				name="production_short_note"
				x="525"
				y="102"
				width="410"
				height="20"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				visible="false"
				optionFlags="Text" />
				
			<actor
				name="with_label"
				x="525"
				y="122"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="${ChooseQuantity:WithLabel}"
				optionFlags="Text"
				visible="false" />

			<actor
				name="with_text"
				x="608"
				y="122"
				width="400"
				height="30"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:WithText}"
				optionFlags="Text, ElideText"
				visible="false" />

			<actor
				name="date_label"
				x="525"
				y="147"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Date:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="date_text"
				x="608"
				y="147"
				width="345"
				height="40"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Date}"
				optionFlags="Text, ElideText"
				visible="true" />

			<actor
				name="time_label"
				x="525"
				y="172"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Time:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="time_text"
				x="608"
				y="172"
				width="345"
				height="40"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Time}"
				optionFlags="Text, ElideText"
				visible="true" />

			<actor
				name="price_label"
				x="525"
				y="197"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Price:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="price_text"
				x="608"
				y="197"
				width="345"
				height="40"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Price}"
				optionFlags="Text, ElideText"
				visible="true" />

			<actor
				name="venue_label"
				x="525"
				y="222"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Venue:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="venue_text"
				x="608"
				y="222"
				width="345"
				height="55"
				horizontalAlignment="near"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Venue}"
				optionFlags="Text, ElideText"
				visible="true" />
				
			<actor
				name="how_many_tickets_label"
				x="525"
				y="287"
				foregroundColor="0, 0, 0"
				horizontalAlignment="near"
				optionFlags="Text"
				font="font_akzidenz_23_bold"
				text="How many tickets?"
				visible="true" />

			<actor
				name="ticket_1"
				x="525"
				y="332"
				width="175"
				height="277"
				optionFlags="image"
				image="image_price_code_selector_background" />
				
			<actor
				name="content_rating_short_note_label_1"
				x="525"
				y="361"
				width="175"
				horizontalAlignment="center"
				foregroundColor="60, 57, 57"
				optionFlags="Text"
				font="font_target_22_regular"
				text="${ChooseQuantity:ContentRatingShortNote1}" />
				
			<actor
				name="supplier_price_label_1"
				x="525"
				y="392"
				width="175"
				height="35"
				horizontalAlignment="center"
				foregroundColor="60, 57, 57"
				optionFlags="Text, ElideText"
				font="font_target_18_regular"
				text="${ChooseQuantity:SupplierPrice1}" />
				
			<actor
				name="content_rating_type_name_label_1"
				x="530"
				y="420"
				width="165"
				height="35"
				horizontalAlignment="center"
				foregroundColor="60, 57, 57"
				optionFlags="Text, ElideText"
				font="font_akzidenz_11_regular"
				text="${ChooseQuantity:ContentRatingTypeName1}" />
				
			<actor
				name="ticket_quantity_field_1"
				x="608"
				y="465"
				optionFlags="Text"
				foregroundColor="0, 0, 0"
				font="font_target_21_regular"
				text="0" />

			<actor
				name="plus_button_1"
				x="540"
				y="524"
				width="53"
				height="53"
				hitFlags="Enabled"
				optionFlags="Text, DrawBorder, BackgroundGradient"
				foregroundColor="255, 255, 255"
				backgroundColor="0, 154, 51"
				font="font_target_35_regular"
				textRegion="12, 8, 30, 30"
				horizontalAlignment="center"
				verticalAlignment="center"
				cornerSize="26, 26"
				strokeWeight="2.0"
				borderColor="255, 255, 255"
				gradientAngle="90.0"
				gradientTargetColor="0, 105, 35"
				text="+" />

			<actor
				name="minus_button_1"
				x="630"
				y="524"
				width="53"
				height="53"
				hitFlags="Enabled"
				optionFlags="Text, DrawBorder, BackgroundGradient"
				foregroundColor="255, 255, 255"
				backgroundColor="226, 120, 18"
				font="font_target_35_regular"
				textRegion="12, 8, 30, 30"
				horizontalAlignment="center"
				verticalAlignment="center"
				cornerSize="26, 26"
				strokeWeight="2.0"
				borderColor="255, 255, 255"
				gradientAngle="90.0"
				gradientTargetColor="177, 77, 1"
				text="-" />

			<actor
				name="ticket_2"
				x="776"
				y="332"
				width="175"
				height="277"
				optionFlags="image"
				image="image_price_code_selector_background"
				visible="false" />
				
			<actor
				name="content_rating_short_note_label_2"
				x="776"
				y="361"
				width="175"
				horizontalAlignment="center"
				foregroundColor="60, 57, 57"
				optionFlags="Text"
				font="font_target_22_regular"
				text="${ChooseQuantity:ContentRatingShortNote2}"
				visible="false" />
				
			<actor
				name="supplier_price_label_2"
				x="776"
				y="392"
				width="175"
				height="35"
				horizontalAlignment="center"
				foregroundColor="60, 57, 57"
				optionFlags="Text, ElideText"
				font="font_target_18_regular"
				text="${ChooseQuantity:SupplierPrice2}"
				visible="false" />
				
			<actor
				name="content_rating_type_name_label_2"
				x="781"
				y="420"
				width="165"
				height="35"
				horizontalAlignment="center"
				foregroundColor="60, 57, 57"
				optionFlags="Text, ElideText"
				font="font_akzidenz_11_regular"
				text="${ChooseQuantity:ContentRatingTypeName2}"
				visible="false" />
				
			<actor
				name="ticket_quantity_field_2"
				x="859"
				y="465"
				optionFlags="Text"
				foregroundColor="0, 0, 0"
				font="font_target_21_regular"
				text="0"
				visible="false" />

			<actor
				name="plus_button_2"
				x="791"
				y="524"
				width="53"
				height="53"
				hitFlags="Enabled"
				optionFlags="Text, DrawBorder, BackgroundGradient"
				foregroundColor="255, 255, 255"
				backgroundColor="0, 154, 51"
				font="font_target_35_regular"
				textRegion="12, 8, 30, 30"
				horizontalAlignment="center"
				verticalAlignment="center"
				cornerSize="26, 26"
				strokeWeight="2.0"
				borderColor="255, 255, 255"
				gradientAngle="90.0"
				gradientTargetColor="0, 105, 35"
				text="+"
				visible="false" />

			<actor
				name="minus_button_2"
				x="881"
				y="524"
				width="53"
				height="53"
				hitFlags="Enabled"
				optionFlags="Text, DrawBorder, BackgroundGradient"
				foregroundColor="255, 255, 255"
				backgroundColor="226, 120, 18"
				font="font_target_35_regular"
				textRegion="12, 8, 30, 30"
				horizontalAlignment="center"
				verticalAlignment="center"
				cornerSize="26, 26"
				strokeWeight="2.0"
				borderColor="255, 255, 255"
				gradientAngle="90.0"
				gradientTargetColor="177, 77, 1"
				text="-"
				visible="false" />

			<actor
				name="more_options_label"
				x="75"
				y="580"
				width="874"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_12_regular"
				horizontalAlignment="near"
				optionFlags="Text"
				text="More Ticket options may be available on redbox.com"
				visible="true" />

			<actor
				name="separator_1_top"
				x="75"
				y="613"
				width="874"
				height="1"
				backgroundColor="150, 150, 150"
				optionFlags="Background"
				visible="true" />

			<actor
				name="separator_1_bottom"
				x="75"
				y="614"
				width="874"
				height="1"
				backgroundColor="255, 255, 255"
				optionFlags="Background"
				visible="true" />

			<actor
				name="browse_button"
				x="75"
				y="645"
				width="172"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				style="light_gray_button_style"
				text="    BROWSE"
				visible="true" />

			<actor
				name="browse_button_back_arrow"
				x="90"
				y="669"
				optionFlags="Image"
				hitFlags="Enabled"
				image="image_back_arrow"
				visible="true" />

			<actor
				name="change_date_button"
				x="518"
				y="645"
				width="212"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				style="light_gray_button_style"
				text="CHANGE DATE"
				visible="true" />

			<actor
				name="choose_seats_button"
				x="741"
				y="645"
				width="212"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				style="red_button_white_border_style"
				borderColor="255, 255, 255"
				horizontalAlignment="near"
				text="NEXT"
				textRegion="60, 16, 150, 40"
				visible="true" />

			<actor
				name="choose_seats_button_next_arrow"
				x="875"
				y="670"
				optionFlags="Image"
				hitFlags="Enabled"
				image="image_next_right_arrow"
				visible="true" />

		</scene>
	</aspect>
</resource>
