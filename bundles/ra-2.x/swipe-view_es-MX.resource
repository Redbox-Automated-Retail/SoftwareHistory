<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="swipe_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="swipe_view_enter"
			onLeave="swipe_view_leave">

			<actor
				name="swipe_background_image"
				width="1024"
				height="650"
				optionFlags="Image"
				visible="false" />

			<actor
				name="swipe_Title_label"
				x="20"
				y="10"
				optionFlags="Text"
				text="RECOGE REDBOX.COM"
				font="font_akzidenz_bold_large"
				foregroundColor="White"
				verticalAlignment="near"
				horizontalAlignment="near"
				visible="false" />

			<actor
				name="swipe_message_label"
				x="80"
				y="250"
				optionFlags="Text"
				text="&#191;Listo? Deslice su tarjeta."
				font="font_target_largest"
				foregroundColor="White"
				textRegion="0,0,650,250"
				visible="false" />

			<actor
				name="Online_Pickup_message_label"
				x="66"
				y="164"
				optionFlags="Text"
				text="Recoge sus art&#237;culos hecho por redbox.com."
				font="font_target_larger"
				foregroundColor="White"
				verticalAlignment="near"
				horizontalAlignment="near"
				textRegion="0,0,600,270"
				visible="false" />

			<actor
				name="Online_Pickup_message_label1"
				x="66"
				y="422"
				optionFlags="Text"
				text="(Reserva en redbox.com y recogelo aqu&#237;, &#237;garantizada!)."
				font="font_target_medium"
				foregroundColor="White"
				verticalAlignment="near"
				horizontalAlignment="near"
				textRegion="0,0,480,180"
				visible="false" />

			<actor
				name="back_button"
				x="35"
				y="665"
				width="194"
				height="70"
				cornerSize="34,34"
				font="font_target_small"
				text="ATR&#193;S"
				tag="back_button"
				hitFlags="enabled"
				onHit="swipe_view_alert"
				visible="true"
				style="red_button_style" />

			<actor
				name="back_button_arrow"
				x="65"
				y="690"
				image="image_back_white_arrow"
				tag="back_button_arrow"
				hitFlags="enabled"
				onHit="swipe_view_alert"
				optionFlags="image" />

		</scene>
	</aspect>
</resource>
