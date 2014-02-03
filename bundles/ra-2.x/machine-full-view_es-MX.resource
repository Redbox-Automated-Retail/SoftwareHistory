<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="machine_full_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="machine_full_view_enter"
			onLeave="machine_full_view_leave">
			
			<actor
				name="scene_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				image="image_kiosk_full_grey_background"
				optionFlags="Image" />

			<actor
				name="title_text"
				x="80"
				y="136"
				width="864"
				height="100"
				text="Lamentablemente estoy lleno."
				font="font_target_large_overlays"
				foregroundColor="57,64,69"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />

			<actor
				name="body_text"
				x="80"
				y="260"
				width="864"
				height="340"
				text="Puedes esperar a que alguien alquile un disco aqu&#237; o puedes devolverlo en otro kiosco. Algunas maneras de encontrar un kiosco:&#010;&#010;-    Obt&#233;n la aplicaci&#243;n m&#243;vil Redbox y busca localidades.&#010;&#010;-    Env&#237;a MYRETURN por SMS (texto) a 727272.*"
				font="font_target_medium"
				foregroundColor="57,64,69"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />

			<actor
				name="fine_print_text"
				x="80"
				y="604"
				width="864"
				height="30"
				text="*Se pueden aplicar cargos por mensaje y datos. Env&#237;a HELP por SMS (texto) para obtener ayuda."
				font="font_target_smaller_overlays"
				foregroundColor="57,64,69"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />
				
			<actor
				name="machine_full_back_button"
				x="600"
				y="660"
				width="171"
				height="66"
				text="    VOLVER"
				cornerSize="32,15"
				font="font_target_small_button_overlays"
				visible="true"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="back_button_arrow"
				x="618"
				y="682"
				tag="back"
				image="image_back-white-arrow-gray-background"
				visible="true"	
				optionFlags="image" />

			<actor
				name="machine_full_rent_button"
				x="794"
				y="660"
				width="191"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text="ALQUILAR"
				visible="true"	
				style="red_button_style" />
				
		</scene>
	</aspect>
</resource>
