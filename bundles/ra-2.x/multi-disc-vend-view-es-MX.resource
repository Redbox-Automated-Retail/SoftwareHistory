<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="multi_disc_vend_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="228,32,69"
			stylesheet="style_sheet_common"
			onEnter="multi_disc_vend_view_enter"
			onLeave="multi_disc_vend_view_leave">
			
			<actor
				name="header"
				x="103"
				y="86"
				font="font_target_large_52"
				foregroundColor="white"
				text="No se olvide"
				optionFlags="Text" />
				
			<actor
				name="body"
				x="103"
				y="220"
				width="835"
				height="120"
				font="font_target_large_38"
				foregroundColor="white"
				text="Usted alquil&#243; m&#250;ltiples discos, as&#237; que por favor espere para todos ellos."
				optionFlags="Text" />
				
			<actor
				name="discs_image"
				x="286"
				y="449"
				image="image_number_of_discs"
				optionFlags="Image" />
				
			<actor
				name="discs_image_number_text"
				x="434"
				y="434"
				width="300"
				height="110"
				font="font_target_regular_largest"
				foregroundColor="black"
				optionFlags="Text" />
				
			<actor
				name="discs_image_discs_text"
				x="511"
				y="489"
				width="300"
				height="110"
				font="font_target_large_52"
				foregroundColor="black"
				text="DISCOS"
				optionFlags="Text" />
				
		</scene>
	</aspect>
</resource>