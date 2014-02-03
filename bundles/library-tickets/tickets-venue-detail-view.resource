<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_venue_detail_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_venue_detail_view_enter">

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_background"
				optionFlags="Image" />

			<actor
				name="venue_label"
				x="75"
				y="70"
				width="874"
				height="50"
				foregroundColor="0, 0, 0"
				font="font_akzidenz_23_bold"
				horizontalAlignment="near"
				optionFlags="Text, ElideText"
				text="${VenueDetail:VenueName}"
				visible="true" />

			<actor
				name="venue_map_art"
				x="75"
				y="122"
				width="407"
				height="300"
				optionFlags="Image, DrawBorder"
				borderColor="60, 56, 57"
				visible="true" />

			<actor
				name="venue_description"
				x="520"
				y="122"
				width="404"
				height="60"
				optionFlags="Text, ElideText"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				horizontalAlignment="near"
				text="${VenueDetail:VenueDescription}"
				visible="true" />

			<actor
				name="venue_address_label"
				x="520"
				y="180"
				width="345"
				optionFlags="Text"
				font="font_akzidenz_12_bold"
				foregroundColor="black"
				horizontalAlignment="near"
				text="Address"
				visible="true" />

			<actor
				name="venue_address_line_1"
				x="520"
				y="198"
				width="450"
				height="20"
				optionFlags="Text, ElideText"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				horizontalAlignment="near"
				text="${VenueDetail:VenueAddressLine1}"
				visible="true" />

			<actor
				name="venue_address_line_2"
				x="520"
				y="216"
				width="450"
				height="20"
				optionFlags="Text, ElideText"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				horizontalAlignment="near"
				text="${VenueDetail:VenueAddressLine2}"
				visible="true" />

			<actor
				name="venue_address_citystatezip"
				x="520"
				y="234"
				width="345"
				optionFlags="Text"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				horizontalAlignment="near"
				text="${VenueDetail:VenueCityStateZip}"
				visible="true" />

			<actor
				name="venue_address_phone"
				x="520"
				y="252"
				width="345"
				optionFlags="Text"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				horizontalAlignment="near"
				text="${VenueDetail:VenuePhone}"
				visible="true" />

			<actor
				name="accessibility_icon"
				x="520"
				y="280"
				width="45"
				height="50"
				optionFlags="Image"
				font="font_akzidenz_24_bold"
				foregroundColor="255,255,255"
				image="image_handicap_icon"
				visible="true" />

			<actor
				name="venue_accessibility_label"
				x="520"
				y="360"
				width="345"
				optionFlags="Text"
				font="font_akzidenz_12_bold"
				foregroundColor="black"
				horizontalAlignment="near"
				text="Accessibility"
				visible="true" />

			<actor
				name="venue_accessibility_info"
				x="520"
				y="380"
				width="400"
				height="40"
				optionFlags="Text, ElideText"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				horizontalAlignment="near"
				text="${VenueDetail:AccessibilityInfo}"
				visible="true" />

			<actor
				name="parking_icon"
				x="520"
				y="440"
				width="50"
				height="50"
				optionFlags="Image"
				font="font_akzidenz_24_bold"
				image="image_parking_icon"
				foregroundColor="255,255,255"
				visible="true" />

			<actor
				name="venue_parking_label"
				x="520"
				y="520"
				width="345"
				optionFlags="Text"
				font="font_akzidenz_12_bold"
				foregroundColor="black"
				horizontalAlignment="near"
				text="Parking"
				visible="true" />

			<actor
				name="venue_parking_info"
				x="520"
				y="538"
				width="400"
				height="60"
				optionFlags="Text, ElideText"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				horizontalAlignment="near"
				text="${VenueDetail:ParkingInfo}"
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
				name="back_button"
				x="730"
				y="645"
				width="225"
				height="74"
				cornerSize="37, 37"
				font="font_target_21_regular"
				style="red_button_white_border_style"
				horizontalAlignment="center"
				text="BACK"
				visible="true" />

		</scene>
	</aspect>
</resource>
