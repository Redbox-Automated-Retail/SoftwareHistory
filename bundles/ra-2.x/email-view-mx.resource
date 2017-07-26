<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="email_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="email_view_next" >
			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>

      <actor
				name="ad"
				x="148"
				y="25"
				width="728"
				height="90"
        visible="false"
				optionFlags="Image" />

      <actor
				name="email_message_header"
				x="68"
				y="160"
				font="font_target_medium_overlays"
				optionFlags="Text"
				foregroundColor="white"
				text="Tu recibo se enviar&#225; a tu direcci&#243;n de email:"
				visible="true" />

			<actor
				name="email_message"
				x="68"
				y="286"
				font="font_target_large_46"
				optionFlags="Text"
				foregroundColor="white"
				text="${EmailText}"
				visible="true" />

			<actor
				name="change_info_message"
				x="68"
				y="378"
				font="font_target_small"
				optionFlags="Text"
				foregroundColor="white"
				text="Puedes cambiarla cuando gustes en redbox.com/account."
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
				name="email_view_next_button"
				x="803"
				y="657"
				width="196"
				height="66"
				cornerSize="33,25"
				font="font_target_small"
				text="    CONTINUAR"
				style="red_next_button_style" />
				
			<actor
                name="image_next_right_arrow"
                x="955"
                y="678"
				image="image_next_white_arrow_big"
                hitFlags="enabled"
                optionFlags="Image" />
		</scene>
	</aspect>
</resource>
