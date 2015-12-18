<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="blu_ray_upsell_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="48, 4, 5"
			stylesheet="style_sheet_common"
			onEnter="blu_ray_upsell_view_enter"
			onLeave="blu_ray_upsell_view_leave">
			
			<actor
				name="background"
				x="50"
				y="50"
				width="924"
				height="668"
				backgroundColor="white"
				borderColor="138, 7, 13"
				strokeWeight="5"
				cornerSize="28,28"
				optionFlags="Background, DrawBorder" />
				
			<actor
				name="header_large"
				x="0"
				y="79"
				width="1024"
				text="Actualiza a Blu-ray&#8482;"
				font="font_target_large_52"
				foregroundColor="30, 154, 228"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="header_small"
				x="0"
				y="175"
				width="1024"
				text="Mejor imagen, mejor sonido por s&#243;lo unos centavos m&#225;s."
				font="font_target_medium_20"
				foregroundColor="black"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="footer"
				x="0"
				y="544"
				width="1024"
				text="Se require un lector Blu-ray, PS3, PS4, o un Xbox One."
				font="font_target_smaller_overlays"
				foregroundColor="black"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="back_button"
				x="111"
				y="588"
				width="177"
				height="71"
				cornerSize="35,35"
				text="ATR&#193;S"
				font="font_target_small_button_overlays"
				textRegion="71, 22, 80, 23"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="back_button_arrow"
				x="143"
				y="610"
				image="image_back_white_arrow"
				optionFlags="Image" />
				
			<actor
				name="no_thanks_button"
				x="498"
				y="588"
				width="177"
				height="71"
				cornerSize="35,35"
				text="NO GRACIAS"
				font="font_target_small_button_overlays"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="upgrade_button"
				x="700"
				y="588"
				width="215"
				height="71"
				cornerSize="35,35"
				text="ACTUALIZA"
				font="font_target_small_button_overlays"
				style="red_button_style" />
		</scene>
	</aspect>
</resource>