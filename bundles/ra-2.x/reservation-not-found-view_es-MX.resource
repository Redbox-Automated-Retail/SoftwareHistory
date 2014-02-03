<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="reservation_not_found_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="reservation_not_found_view_enter"
			onLeave="reservation_not_found_view_leave">
			
			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>
		
			<actor
				name="phone_image"
				width="279"
				height="304"
				x="670"
				y="298"
				image="image_reservation_not_found_es_MX"
				optionFlags="Image"	/>

			<actor
				name="title_label"
				x="60"
				y="49"
				width="904"
				height="180"
				text="Tu reservaci&#243;n te aguarda.&#010;&#161;Encontr&#233;mosla!"
				font="font_target_large_52"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />

			<actor
				name="message_label"
				x="60"
				y="246"
				width="904"
				height="300"
				text="No hay reservaci&#243;n para ti en este kiosco. Tal vez sea porque:&#010;&#010;-    Hiciste una reservaci&#243;n en otra&#010;      localidad vecina.&#010;&#010;-    Al reservar usaste una tarjeta&#010;     diferente a la que usas ahora."
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
				name="reservation_not_found_ok_button"
				x="794"
				y="660"
				width="191"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text="OK"
				visible="true"	
				style="red_button_style" />

		</scene>
	</aspect>
</resource>
