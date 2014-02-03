<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_promotional_rules_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_promotional_rules_view_enter"
			onLeave="tickets_promotional_rules_view_leave">

			<actor
				name="background_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>

			<actor
				name="tickets_message_header"
				x="62"
				y="156"
				width="900"
				height="100"
				font="font_target_62_regular"
				optionFlags="Text"
				foregroundColor="white" />

			<actor
				name="tickets_message"
				x="62"
				y="263"
				width="900"
				height="355"
				font="font_target_16_regular"
				optionFlags="Text"
				foregroundColor="white" />
				
			<actor
				name="footer_area"
				x="0"
				y="626"
				optionFlags="Background, TextShadow"
				backgroundColor="60,57,57"
				width="1024"
				height="142" />

			<actor
				name="first_button"
				x="573"
				y="664"
				width="196"
				height="66"
				cornerSize="33, 33"
				font="font_target_21_regular"
				verticalAlignment="center"
				horizontalAlignment="center"
				style="gray_button_style" />

			<actor
				name="first_button_back_arrow"
				x="593"
				y="685"
				optionFlags="Image"
				image="image_back_arrow" />

			<actor
				name="second_button"
				x="790"
				y="664"
				width="196"
				height="66"
				cornerSize="33, 33"
				font="font_target_21_regular"
				verticalAlignment="center"
				horizontalAlignment="center"
				style="red_button_style_3pt_redstroke" />
				
			<actor
				name="second_button_next_arrow"
				x="935"
				y="685"
				optionFlags="Image"
				image="image_next_right_arrow" />
				
		</scene>
	</aspect>
</resource>
