<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_seating_chart_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_seating_chart_view_enter">

			<actor
				name="seating_view"
				x="0"
				y="0"
				width="1024"
				height="612"
				optionFlags="Background, Image" />

			<actor
				name="ga_overlay_text_large"
				x="162"
				y="273"
				hitFlags="Enabled"
				optionFlags="Image" />

			<actor
				name="footer_area"
				x="0"
				y="600"
				width="1024"
				height="200"
				backgroundColor="216, 216, 216"
				optionFlags="Background, TextShadow" />

			<actor
				name="option_1_button"
				x="22"
				y="624"
				width="402"
				height="128"
				image="image_seating_chart_button"
				font="font_akzidenz_15_bold"
				text="${SeatSelection:Option1SuggestionType}: ${SeatSelection:Option1SubTotal}"
				textRegion="79, 28, 310, 30"
				foregroundColor="0, 0, 0"
				hitFlags="Enabled"
				optionFlags="Image, Text, ElideText" />

			<actor
				name="option_1_zone_level"
				x="101"
				y="675"
				width="325"
				height="30"
				font="font_akzidenz_16_regular"
				text="${SeatSelection:Option1ZoneLevel}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_1_section_row"
				x="101"
				y="700"
				width="310"
				height="30"
				font="font_akzidenz_15_regular"
				text="Section ${SeatSelection:Option1Section}, Row ${SeatSelection:Option1Row}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_1_prices"
				x="101"
				y="725"
				width="320"
				height="60"
				font="font_akzidenz_11_regular"
				text="${SeatSelection:Option1PricePerTicket} + ${SeatSelection:Option1FeePerTicket} fee per ticket x ${SeatSelection:Option1Quantity} tickets"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_1_checkmark"
				x="37"
				y="648"
				width="56"
				height="57"
				image="image_gray_checkmark"
				optionFlags="Image" />

			<actor
				name="option_2_button"
				x="442"
				y="624"
				width="402"
				height="128"
				image="image_seating_chart_button"
				font="font_akzidenz_15_bold"
				text="${SeatSelection:Option2SuggestionType}: ${SeatSelection:Option2SubTotal}"
				textRegion="79, 28, 310, 30"
				foregroundColor="0, 0, 0"
				hitFlags="Enabled"
				optionFlags="Image, Text, ElideText" />

			<actor
				name="option_2_zone_level"
				x="521"
				y="675"
				width="325"
				height="30"
				font="font_akzidenz_16_regular"
				text="${SeatSelection:Option2ZoneLevel}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_2_section_row"
				x="521"
				y="700"
				width="310"
				height="30"
				font="font_akzidenz_15_regular"
				text="Section ${SeatSelection:Option2Section}, Row ${SeatSelection:Option2Row}"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_2_prices"
				x="521"
				y="725"
				width="320"
				height="60"
				font="font_akzidenz_11_regular"
				text="${SeatSelection:Option2PricePerTicket} + ${SeatSelection:Option2FeePerTicket} fee per ticket x ${SeatSelection:Option2Quantity} tickets"
				foregroundColor="0, 0, 0"
				optionFlags="Text, ElideText" />

			<actor
				name="option_2_checkmark"
				x="457"
				y="648"
				width="56"
				height="57"
				image="image_gray_checkmark"
				optionFlags="Image" />

			<actor
				name="unzoom_button"
				x="935"
				y="545"
				width="56"
				height="50"
				optionFlags="Image"
				hitFlags="Enabled"
				image="image_unzoom_button" />

			<actor
				name="buy_button"
				x="850"
				y="645"
				width="139"
				height="69"
				cornerSize="36, 36"
				foregroundColor="white"
				font="font_target_21_regular"
				text="BUY"
				strokeWeight="3"
				style="light_gray_button_style" />

		</scene>
	</aspect>
</resource>