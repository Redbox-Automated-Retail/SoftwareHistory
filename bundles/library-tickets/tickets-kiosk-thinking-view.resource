<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="tickets_kiosk_thinking_view" />
    <property name="version" value="2.0,Tickets,A" />
    <property name="active_flag" value="passive" />
    <aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundImage="image_red_diagonal_stripes"
			onEnter="tickets_kiosk_thinking_view_enter">

			<actor
				name="body_1"
				x="76"
				y="210"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_54_regular"
				optionFlags="Text"
				text="Wait!" />

			<actor
				name="body_2"
				x="76"
				y="300"
				width="918"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_54_regular"
				optionFlags="Text"
				text="You&apos;re not done yet!" />

			<actor
				name="body_3"
				x="76"
				y="390"
				width="918"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_54_regular"
				optionFlags="Text"
				text="There's just one more" />

			<actor
				name="body_4"
				x="76"
				y="480"
				width="918"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_54_regular"
				optionFlags="Text"
				text="(important) thing to do." />

		</scene>
	</aspect>
</resource>