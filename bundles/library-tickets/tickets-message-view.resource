<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_message_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets" >

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>

			<actor
				name="tickets_message_header"
				x="60"
				y="134"
				font="font_target_62_regular"
				optionFlags="Text"
				foregroundColor="white"
				text="${TicketsMessageView:MessageHeaderText}"
				visible="false" />

			<actor
				name="tickets_message"
				x="60"
				y="252"
				width="897"
				font="font_target_23_regular"
				optionFlags="Text"
				foregroundColor="white"
				text="${TicketsMessageView:MessageText}"
				visible="false" />

			<actor
				name="tickets_disclaimer_area"
				x="61"
				y="262"
				width="897"
				height="150"
				font="font_target_23_regular"
				optionFlags="Text, DrawBorder"
				foregroundColor="yellow"
				text=""
				visible="false" />

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
				name="first_button"
				x="573"
				y="664"
				width="196"
				height="66"
				cornerSize="33, 33"
				font="font_target_21_regular"
				verticalAlignment="center"
				horizontalAlignment="center"
				style="gray_button_style"
				text="${TicketsMessageView:FirstButtonText}"
				visible="false" />

			<actor
				name="message_view_back_arrow"
				x="593"
				y="685"
				optionFlags="Image"
				hitFlags="Enabled"
				image="image_back_arrow"
				visible="false" />

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
				text="${TicketsMessageView:SecondButtonText}"
				style="red_button_style_3pt_redstroke"
				visible="false"/>

		</scene>
	</aspect>
</resource>
