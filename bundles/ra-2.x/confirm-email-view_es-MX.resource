<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="confirm_email_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="confirm_email_view_enter"
			onLeave="confirm_email_view_leave">
			
			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>
		
			<actor
				name="title_label"
				x="60"
				y="80"
				width="924"
				height="200"
				text="¿Quieres mas cosas gratis de parte nuestra?"
				font="font_target_large_62"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />

			<actor
				name="message_label"
				x="60"
				y="318"
				width="924"
				height="200"
				text="Recibe una noche de película gratis (más información sobre nuevos estrenos y ofertas) cuando te suscribes a nuestros correos electrónicos.&#10;&#10;Toque SÍ para empezar."
				font="font_target_medium_20"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />

			<actor
				name="footer_area"
				x="0"
				y="628"
				optionFlags="Background, TextShadow"
				backgroundColor="60,57,57"
				width="1024"
				height="142"
				visible="true" />
			
			<actor
				name="footer_highlight"
				x="0"
				y="626"
				optionFlags="Background, TextShadow"
				backgroundColor="80,72,72"
				width="1024"
				height="2"
				visible="true" />

			<actor
				name="confirm_email_no_thanks_button"
				x="580"
				y="660"
				width="191"
				height="66"
				text="DESPU&#201;S"
				cornerSize="32,15"
				font="font_target_small_button_overlays"
				visible="true"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="confirm_email_yes_button"
				x="794"
				y="660"
				width="191"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text="S&#205;"
				visible="true"	
				style="red_button_style" />
		</scene>
	</aspect>
</resource>
