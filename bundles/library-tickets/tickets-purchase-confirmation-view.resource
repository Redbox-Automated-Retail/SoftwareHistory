<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="tickets_purchase_confirmation_view" />
    <property name="version" value="2.0,Tickets,A" />
    <property name="active_flag" value="passive" />
    <aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundImage="image_red_diagonal_stripes"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_purchase_confirmation_view_enter">

			<actor
				name="header"
				x="76"
				y="192"
				width="635"
				font="font_target_54_regular"
				foregroundColor="255,255,255"
				optionFlags="Text" />

			<actor
				name="body_1"
				x="76"
				y="371"
				width="635"
				font="font_target_23_regular"
				foregroundColor="255,255,255"
				optionFlags="Text" />

			<actor
				name="body_2"
				x="76"
				y="444"
				width="635"
				font="font_target_23_regular"
				foregroundColor="255,255,255"
				optionFlags="Text" />

			<actor
				name="done_button"
				x="790"
				y="664"
				width="196"
				height="66"
				cornerSize="33,25"
				font="font_target_21_regular"
				verticalAlignment="center"
				horizontalAlignment="center"
				text="I'M DONE"
				enabled="True"
				style="red_button_style_3pt_redstroke" />

		</scene>
	</aspect>
</resource>