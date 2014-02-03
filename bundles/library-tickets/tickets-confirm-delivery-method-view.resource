<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_confirm_delivery_method_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_confirm_delivery_method_view_enter">

			<actor
				name="background_image"
				x="0"
				y="0"
				width="1024"
				height="768"
				image="image_tickets_background"
				optionFlags="Image" />

			<actor
				name="title_label"
				x="76"
				y="114"
				font="font_akzidenz_29_bold"
				optionFlags="Text"
				foregroundColor="black"
				text="How would you like to get your tickets?" />

			<actor
				name="delivery_panel"
				x="76"
				y="208"
				width="772"
				height="252"
				image="image_delivery_method"
				optionFlags="Image" />

			<actor
				name="option_1_checkmark"
				x="112"
				y="238"
				image="image_gray_checkmark"
				optionFlags="Image" />

			<actor
				name="option_2_checkmark"
				x="112"
				y="359"
				image="image_gray_checkmark"
				optionFlags="Image" />

			<actor
				name="option_1_title_label"
				x="204"
				y="235"
				font="font_akzidenz_24_bold"
				optionFlags="Text"
				foregroundColor="black"
				text="PRINT AT HOME" />

			<actor
				name="option_1_body_label"
				x="204"
				y="274"
				font="font_akzidenz_16_regular"
				optionFlags="Text"
				foregroundColor="black"
				text="I want you to email me a link to my tickets." />

			<actor
				name="option_2_title_label"
				x="204"
				y="357"
				font="font_akzidenz_24_bold"
				optionFlags="Text"
				foregroundColor="black"
				text="WILL CALL" />

			<actor
				name="option_2_body_label"
				x="204"
				y="396"
				font="font_akzidenz_16_regular"
				optionFlags="Text"
				foregroundColor="black"
				text="I want to pick them up at the event." />

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
				name="back_button"
				x="622"
				y="645"
				width="156"
				height="66"
				font="font_target_21_regular"
				style="light_gray_button_style"
				cornerSize="33, 33"
				textRegion="45, 15, 100, 35"
				text="BACK"
				hitFlags="Enabled" />

			<actor
				name="back_button_arrow"
				x="654"
				y="667"
				hitFlags="Enabled"
				tag="back"
				image="image_back-white-arrow-gray-background"
				optionFlags="image" />

			<actor
				name="ok_button"
				x="793"
				y="645"
				width="156"
				height="66"
				font="font_target_21_regular"
				style="red_button_white_border_style"
				cornerSize="33, 33"
				textRegion="30, 15, 100, 35"
				text="NEXT"
				hitFlags="Enabled" />
				
			<actor
				name="ok_button_arrow"
				x="890"
				y="667"
				image="image_next_right_arrow"
				optionFlags="Image" />
				
			<actor
				name="option_1_button"
				x="77"
				y="208"
				width="772"
				height="125"
				hitFlags="Enabled"
				optionFlags="Text" />

			<actor
				name="option_2_button"
				x="77"
				y="335"
				width="772"
				height="125"
				hitFlags="Enabled"
				optionFlags="Text" />

		</scene>
	</aspect>
</resource>
