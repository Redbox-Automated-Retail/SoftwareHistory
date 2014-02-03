<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_choose_quantity_view_2" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundImage="image_tickets_background"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_choose_quantity_view_2_enter">

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
				x="74"
				y="108"
				width="252"
				height="186"
				optionFlags="Image, DrawBorder"
				borderColor="60, 56, 57"
				visible="true" />
				
			<actor
				name="event_short_note"
				x="365"
				y="108"
				width="590"
				height="186"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				visible="false"
				optionFlags="Text" />
				
			<actor
				name="with_label"
				x="365"
				y="120"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="${ChooseQuantity:WithLabel}"
				optionFlags="Text"
				visible="false" />

			<actor
				name="with_text"
				x="448"
				y="120"
				width="400"
				height="30"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:WithText}"
				optionFlags="Text, ElideText"
				visible="false" />

			<actor
				name="date_label"
				x="365"
				y="145"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Date:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="date_text"
				x="448"
				y="145"
				width="345"
				height="40"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Date}"
				optionFlags="Text, ElideText"
				visible="true" />

			<actor
				name="time_label"
				x="365"
				y="170"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Time:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="time_text"
				x="448"
				y="170"
				width="500"
				height="40"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Time}"
				optionFlags="Text, ElideText"
				visible="true" />

			<actor
				name="price_label"
				x="365"
				y="195"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Price:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="price_text"
				x="448"
				y="195"
				width="345"
				height="40"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Price}"
				optionFlags="Text, ElideText"
				visible="true" />

			<actor
				name="venue_label"
				x="365"
				y="220"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_bold"
				text="Venue:"
				optionFlags="Text"
				visible="true" />

			<actor
				name="venue_text"
				x="448"
				y="220"
				width="500"
				height="55"
				horizontalAlignment="near"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_16_regular"
				text="${ChooseQuantity:Venue}"
				optionFlags="Text, ElideText"
				visible="true" />

			<actor
				name="how_many_tickets_label"
				x="83"
				y="310"
				foregroundColor="0, 0, 0"
				horizontalAlignment="near"
				optionFlags="Text"
				font="font_akzidenz_18_bold"
				text="How many tickets?"
				visible="true" />

			<actor
				name="price_code_pager_background"
				x="840"
				y="365"
				width="105"
				height="235"
				optionFlags="Background"
				backgroundColor="233, 233, 233"
				cornerSize="10, 10" />

			<actor
				name="price_code_pager_up"
				x="857"
				y="390"
				optionFlags="Image"
				image="image_up_arrow"
				hitFlags="Enabled" />

			<actor
				name="price_code_pager_down"
				x="857"
				y="490"
				optionFlags="Image"
				image="image_down_arrow"
				hitFlags="Enabled" />

			<actor
				name="price_code_page_label"
				x="840"
				y="565"
				width="105"
				font="font_akzidenz_14_bold"
				foregroundColor="51,51,51"
				text="${PriceCode:Page} of ${PriceCode:TotalPages}"
				horizontalAlignment="center"
				optionFlags="Text" />

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
				style="red_button_style"
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
