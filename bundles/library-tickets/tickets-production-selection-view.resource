<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_production_selection_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_production_selection_view_enter"
			onLeave="tickets_production_selection_view_leave">

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
				y="64"
				width="945"
				height="50"
				font="font_akzidenz_23_bold"
				foregroundColor="black"
				optionFlags="Text, ElideText" />

			<actor
				name="event_image"
				x="75"
				y="108"
				width="407"
				height="300"
				borderColor="60,56,57"
				optionFlags="Image, DrawBorder" />

			<actor
				name="event_short_note"
				x="75"
				y="425"
				width="410"
				height="180"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				optionFlags="Text, ElideText" />

			<actor
				name="blackout_dates_text"
				x="75"
				y="582"
				width="410"
				height="20"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				optionFlags="Text" />

			<actor
				name="production_short_note"
				x="75"
				y="425"
				width="410"
				height="20"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				visible="false"
				optionFlags="Text" />
				
			<actor
				name="available_dates"
				x="517"
				y="142"
				font="font_akzidenz_23_bold"
				foregroundColor="black"
				text="Redbox Available Dates:"
				optionFlags="Text" />

			<actor
				name="top_panel"
				x="517"
				y="196"
				width="371"
				height="126"
				image="image_top_panel"
				optionFlags="Image" />

			<actor
				name="middle_panel"
				x="517"
				y="320"
				width="371"
				height="126"
				image="image_middle_panel"
				optionFlags="Image" />

			<actor
				name="bottom_panel"
				x="517"
				y="444"
				width="371"
				height="126"
				image="image_bottom_panel"
				optionFlags="Image" />

			<actor
				name="event_future_dates"
				x="517"
				y="582"
				width="460"
				height="25"
				font="font_akzidenz_11_regular"
				foregroundColor="black"
				optionFlags="Text" />

			<actor
				name="up_arrow"
				x="910"
				y="310"
				hitFlags="Enabled"
				onHit="tickets_production_selection_page_up"
				image="image_up_arrow"
				optionFlags="Image" />

			<actor
				name="down_arrow"
				x="910"
				y="396"
				hitFlags="Enabled"
				onHit="tickets_production_selection_page_down"
				image="image_down_arrow"
				optionFlags="Image" />

			<actor
				name="page_label"
				x="920"
				y="482"
				font="font_akzidenz_14_bold"
				foregroundColor="51,51,51"
				text="${Page} of ${TotalPages}"
				optionFlags="Text" />

			<actor
				name="dividing_line_1"
				x="75"
				y="613"
				width="874"
				height="1"
				backgroundColor="150,150,150"
				optionFlags="Background" />

			<actor
				name="dividing_line_2"
				x="75"
				y="614"
				width="874"
				height="1"
				backgroundColor="255,255,255"
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
				name="email_info_button"
				x="260"
				y="645"
				width="172"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				onHit="tickets_production_selection_email"
				text="EMAIL INFO"
				style="green_button_style_bold_border" />
				
			<actor
				name="email_info_small_button"
				x="253"
				y="645"
				width="129"
				height="71"
				cornerSize="35, 35"
				font="font_target_17_regular"
				onHit="tickets_production_selection_email"
				text="EMAIL&#010;INFO"
				visible="false"
				style="green_button_style_bold_border" />
				
			<actor
				name="email_sent_info_button"
				x="260"
				y="645"
				width="172"
				height="71"
				cornerSize="35, 35"
				font="font_target_17_regular"
				visible="false"
				text="   EMAIL SENT"
				style="green_button_style_bold_border" />
				
			<actor
				name="email_sent_info_small_button"
				x="256"
				y="645"
				width="134"
				height="71"
				image="image_email_sent_small_button"
				optionFlags="Image"
				visible="false" />
				
			<actor
				name="rules_button"
				x="388"
				y="645"
				width="107"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				text="RULES"
				visible="false"
				style="light_gray_button_style" />
				
			<actor
				name="current_selection_heading"
				x="517"
				y="647"
				font="font_akzidenz_14_bold"
				foregroundColor="171,23,23"
				text="You've chosen:"
				optionFlags="Text" />

			<actor
				name="current_selection_line_1"
				x="517"
				y="667"
				height="20"
				width="225"
				font="font_akzidenz_12_bold"
				foregroundColor="black"
				optionFlags="Text, ElideText" />

			<actor
				name="current_selection_line_2"
				x="517"
				y="687"
				height="20"
				width="225"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				optionFlags="Text, ElideText" />

			<actor
				name="current_selection_line_3"
				x="517"
				y="707"
				height="20"
				width="225"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				optionFlags="Text, ElideText" />

			<actor
				name="current_selection_line_4"
				x="517"
				y="727"
				height="20"
				width="225"
				font="font_akzidenz_12_regular"
				foregroundColor="black"
				optionFlags="Text, ElideText" />

			<actor
				name="next_button"
				x="741"
				y="645"
				width="212"
				height="71"
				cornerSize="35, 35"
				font="font_target_21_regular"
				text="NEXT"
				horizontalAlignment="near"
				textRegion="60,16,150,40"
				style="red_button_white_border_style"
				borderColor="255, 255, 255" />

			<actor
				name="next_button_next_arrow"
				x="875"
				y="670"
				image="image_next_right_arrow"
				optionFlags="Image" />

		</scene>
	</aspect>
</resource>