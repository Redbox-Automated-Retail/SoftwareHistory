<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_swipe_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_swipe_view_enter"
			onLeave="tickets_swipe_view_leave">

			<actor
				name="swipe_message_label"
				x="80"
				y="280"
				text="Ready. Set. Swipe!"
				font="font_target_largest"
				foregroundColor="White"
				textRegion="0,0,500,250"
				optionFlags="Text" />

			<actor
				name="back_button"
				x="35"
				y="665"
				width="194"
				height="70"
				cornerSize="35, 35"
				font="font_target_small"
				text="BACK"
				style="red_button_style" />

			<actor
				name="back_button_arrow"
				x="65"
				y="690"
				image="image_back_white_arrow"
				optionFlags="image" />

		</scene>
	</aspect>
</resource>
