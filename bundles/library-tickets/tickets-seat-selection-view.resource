<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_seat_selection_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_seat_selection_view_enter">

			<actor
				name="background"
				x="0"
				y="0"
				width="1024"
				height="768"
				image="image_tickets_background"
				optionFlags="Image" />

			<actor
				name="event_name"
				x="75"
				y="70"
				width="945"
				height="50"
				font="font_akzidenz_23_bold"
				foregroundColor="black"
				text="${SeatSelection:EventName}"
				optionFlags="Text, ElideText" />

			<actor
				name="seating_chart"
				x="75"
				y="122"
				width="407"
				height="300"
				borderColor="60, 56, 57"
				hitFlags="Enabled"
				optionFlags="Image, DrawBorder" />

			<actor
				name="ga_overlay_text"
				x="90"
				y="254"
				hitFlags="Enabled"
				optionFlags="Image" />

			<actor
				name="venue_info_button"
				x="75"
				y="430"
				width="404"
				height="60"
				cornerSize="35, 35"
				style="light_gray_button_style"
				hitFlags="Enabled"
				font="font_akzidenz_14_bold"
				text=""
				visible="true" />

			<actor
				name="venue_info_button_label"
				x="75"
				y="438"
				width="404"
				foregroundColor="255, 255, 255"
				font="font_akzidenz_14_bold"
				horizontalAlignment="center"
				optionFlags="Text"
				text="VENUE INFO"
				visible="true" />

			<actor
				name="venue_info_button_address_label"
				x="75"
				y="460"
				width="404"
				height="20"
				foregroundColor="255, 255, 255"
				font="font_akzidenz_12_regular"
				horizontalAlignment="center"
				optionFlags="Text, ElideText"
				text="${SeatSelection:VenueAddress}"
				visible="true" />

			<actor
				name="zoom_button"
				x="428"
				y="374"
				width="36"
				height="36"
				optionFlags="Image"
				image="image_zoom_button" />

			<actor
				name="alternate_seats_label"
				x="525"
				y="110"
				width="400"
				optionFlags="Text"
				font="font_akzidenz_11_regular"
				foregroundColor="0, 0, 0"
				text="We're sorry but we could not find the discounts you were looking for. Below are other seating options for you:"
				visible="false" />

			<actor
				name="choose_a_type_label"
				x="525"
				y="150"
				optionFlags="Text"
				font="font_akzidenz_23_bold"
				foregroundColor="0, 0, 0"
				text="Redbox Seat Options:" />

			<actor
				name="option_1_button"
				x="518"
				y="202"
				width="432"
				height="149"
				image="image_top_button"
				font="font_akzidenz_16_bold"
				text="${SeatSelection:Option1SuggestionType}: ${SeatSelection:Option1SubTotal}"
				textRegion="87, 26, 325, 30"
				foregroundColor="0, 0, 0"
				hitFlags="Enabled"
				optionFlags="Image, Text, ElideText" />

			<actor
				name="option_1_zone_level"
				x="605"
				y="253"
				width="325"
				height="30"
				font="font_akzidenz_16_regular"
				text="${SeatSelection:Option1ZoneLevel}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_1_section_row"
				x="605"
				y="280"
				width="325"
				height="30"
				font="font_akzidenz_16_regular"
				text="Section ${SeatSelection:Option1Section}, Row ${SeatSelection:Option1Row}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_1_prices"
				x="605"
				y="305"
				width="340"
				height="30"
				font="font_akzidenz_12_regular"
				text="${SeatSelection:Option1PricePerTicket} + ${SeatSelection:Option1FeePerTicket} fee per ticket x ${SeatSelection:Option1Quantity} tickets"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_1_checkmark"
				x="533"
				y="226"
				width="56"
				height="57"
				image="image_gray_checkmark"
				optionFlags="Image" />

			<actor
				name="option_2_button"
				x="518"
				y="329"
				width="432"
				height="128"
				image="image_bottom_button"
				font="font_akzidenz_16_bold"
				text="${SeatSelection:Option2SuggestionType}: ${SeatSelection:Option2SubTotal}"
				textRegion="87, 26, 325, 30"
				foregroundColor="0, 0, 0"
				hitFlags="Enabled"
				optionFlags="Image, Text, ElideText" />

			<actor
				name="option_2_zone_level"
				x="605"
				y="380"
				width="325"
				height="30"
				font="font_akzidenz_16_regular"
				text="${SeatSelection:Option2ZoneLevel}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_2_section_row"
				x="605"
				y="407"
				width="325"
				height="30"
				font="font_akzidenz_16_regular"
				text="Section ${SeatSelection:Option2Section}, Row ${SeatSelection:Option2Row}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_2_prices"
				x="605"
				y="432"
				width="340"
				height="30"
				font="font_akzidenz_12_regular"
				text="${SeatSelection:Option2PricePerTicket} + ${SeatSelection:Option2FeePerTicket} fee per ticket x ${SeatSelection:Option2Quantity} tickets"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_2_checkmark"
				x="533"
				y="353"
				width="56"
				height="57"
				image="image_gray_checkmark"
				optionFlags="Image" />

			<actor
				name="email_confirmation_text"
				x="519"
				y="472"
				optionFlags="Text"
				font="font_akzidenz_11_regular"
				foregroundColor="0, 0, 0"
				text="${SeatSelection:EmailConfirmationText}" />

			<actor
				name="more_options_label_bold"
				x="500"
				y="580"
				width="874"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_11_bold"
				horizontalAlignment="near"
				optionFlags="Text"
				text="All seats together."
				visible="true" />

			<actor
				name="more_options_label"
				x="635"
				y="580"
				width="874"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_11_regular"
				horizontalAlignment="near"
				optionFlags="Text"
				text="More seat options may be available at redbox.com"
				visible="true" />

			<actor
				name="separator_1_top"
				x="75"
				y="613"
				width="874"
				height="1"
				backgroundColor="150, 150, 150"
				optionFlags="Background" />

			<actor
				name="separator_1_bottom"
				x="75"
				y="614"
				width="874"
				height="1"
				backgroundColor="255, 255, 255"
				optionFlags="Background" />

			<actor
				name="browse_button"
				x="75"
				y="645"
				width="172"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				text="    BROWSE"
				style="light_gray_button_style" />

			<actor
				name="browse_button_back_arrow"
				x="90"
				y="669"
				image="image_back_arrow"
				optionFlags="Image" />

			<actor
				name="change_quantity_button"
				x="475"
				y="645"
				width="270"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				text="CHANGE QUANTITY"
				style="light_gray_button_style" />

			<actor
				name="buy_button"
				x="755"
				y="645"
				width="196"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				style="red_button_white_border_style"
				textRegion="50, 18, 70, 30"
				text="NEXT" />

			<actor
				name="image_next_white_arrow_big"
				x="880"
				y="668"
				image="image_next_right_arrow"
				optionFlags="Image" />

		</scene>
	</aspect>
</resource>
