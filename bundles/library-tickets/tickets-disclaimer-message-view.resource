<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_disclaimer_message_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_disclaimer_message_view_enter"
			onLeave="tickets_disclaimer_message_view_leave" >

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>

			<actor
				name="tickets_disclaimer_header"
				x="60"
				y="117"
				font="font_target_62_regular"
				optionFlags="Text"
				foregroundColor="white"
				text=""
				visible="true" />

			<actor
				name="tickets_disclaimer_area"
				x="60"
				y="237"
				width="900"
				height="330"
				font="font_target_23_regular"
				optionFlags="Text"
				foregroundColor="white"
				text=""
				visible="true" />

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
				name="reject_tickets_disclaimer"
				x="573"
				y="664"
				width="196"
				height="66"
				cornerSize="33, 33"
				font="font_target_21_regular"
				verticalAlignment="center"
				horizontalAlignment="center"
				style="gray_button_style"
				text="NO"
				visible="true" />

			<actor
				name="accept_tickets_disclaimer"
				x="790"
				y="664"
				width="196"
				height="66"
				cornerSize="33, 33"
				font="font_target_21_regular"
				verticalAlignment="center"
				horizontalAlignment="center"
				text="YES"
				style="red_button_style_3pt_redstroke"
				visible="true"/>

		</scene>
	</aspect>
</resource>
