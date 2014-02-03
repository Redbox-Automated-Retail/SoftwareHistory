<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_how_it_works_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_how_it_works_view_enter">

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image" />

			<actor
				name="how_it_works_label"
				x="50"
				y="50"
				font="font_target_50_regular"
				optionFlags="Text"
				foregroundColor="white"
				text="How It Works" />

			<actor
				name="image_1"
				x="110"
				y="190"
				height="63"
				width="64"
				text="1"
				font="font_target_21_regular"
				textRegion="29,10,63,64"
				foregroundColor="white"
				image="image_round_gray_button"
				optionFlags="Image, Text" />

			<actor
				name="point_1"
				x="190"
				y="199"
				optionFlags="Text"
				foregroundColor="white"
				text="Pick a local event"
				font="font_target_21_regular" />

			<actor
				name="image_2"
				x="110"
				y="270"
				height="63"
				width="64"
				text="2"
				font="font_target_21_regular"
				textRegion="25,10,63,64"
				foregroundColor="white"
				image="image_round_gray_button"
				optionFlags="Image, Text" />

			<actor
				name="point_2"
				x="190"
				y="279"
				optionFlags="Text"
				foregroundColor="white"
				text="Choose the date"
				font="font_target_21_regular" />

			<actor
				name="image_3"
				x="110"
				y="350"
				height="63"
				width="64"
				text="3"
				font="font_target_21_regular"
				textRegion="27,10,63,64"
				foregroundColor="white"
				image="image_round_gray_button"
				optionFlags="Image, Text" />

			<actor
				name="point_3"
				x="190"
				y="359"
				optionFlags="Text"
				foregroundColor="white"
				text="${point_3:text}"
				font="font_target_21_regular" />

			<actor
				name="image_4"
				x="110"
				y="430"
				height="63"
				width="64"
				text="4"
				font="font_target_21_regular"
				textRegion="25,10,63,64"
				foregroundColor="white"
				image="image_round_gray_button"
				optionFlags="Image, Text" />

			<actor
				name="point_4"
				x="190"
				y="439"
				optionFlags="Text"
				foregroundColor="white"
				text="Print them at home or pick them up at the venue"
				font="font_target_21_regular" />

			<actor
				name="footer_area"
				x="0"
				y="626"
				optionFlags="Background, TextShadow"
				backgroundColor="60,57,57"
				width="1024"
				height="142"
				visible="true" />

			<actor
				name="continue_button"
				x="790"
				y="664"
				width="196"
				height="66"
				cornerSize="33,25"
				font="font_target_21_regular"
				verticalAlignment="center"
				horizontalAlignment="center"
				text="GOT IT!"
				tag="continue"
				onHit="view_goback"
				enabled="True"
				style="red_button_style_3pt_redstroke" />

		</scene>
	</aspect>
</resource>
