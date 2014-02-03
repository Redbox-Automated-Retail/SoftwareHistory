<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="alternate_start_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="0, 0, 0"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="alternate_start_view_enter"
			onLeave="alternate_start_view_leave">
				
			<actor
				name="scene_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				optionFlags="BackgroundGradient"
				backgroundColor="0, 0, 0"
				gradientTargetColor="64, 64, 64"
				gradientAngle="90"
				visible="true" />

			<actor
				name="help_button"
				x="20"
				y="55"
				width="120"
				height="45"
				onHit="help"
				text="AYUDA"
				font="font_akzidenz_bold_smaller"
				cornerSize="23,27"
				style="gray_button_style" />
				
			<actor
				name="redbox_logo"
				x="405"
				y="28"
				width="227"
				height="75"
				style="redbox_logo" />

			<actor
				name="espanol_button"
				x="882"
				y="55"
				width="120"
				height="45"
				text="ENGLISH"
				font="font_akzidenz_bold_smaller"
				onHit="switch_language"
				cornerSize="23,27"
				style="gray_button_style" />
				
			<!-- Movies -->		
			<actor
                name="find_movies_button"
                x="20"
                y="180"
                width="475"
                height="346"
                cornerSize="40,20"
				onHit="browse_products"
				font="font_target_largest"
				text="PEL&#205;CULAS" 
                style="red_button_style" />
			
			<!-- Games -->
			<actor
                name="find_games_button"
                x="535"
                y="180"
                width="475"
                height="346"
                cornerSize="40,20"
                font="font_target_largest"
				text="JUEGOS" 
				onHit="browse_products"
                style="red_button_style" />
				
			<actor
                name="online_pickup_button"
                x="20"
                y="570"
                width="475"
                height="95"
                cornerSize="44,34"
                font="font_target_medium"
                text="RECOGER RESERVACI&#211;N"
                onHit="pickup_reserved_product"
                style="red_button_style" />	
				
			<actor
                name="return_button"
                x="535"
                y="570"
                width="475"
                height="95"
                cornerSize="44,34"
				font="font_target_medium"
                text="DEVOLVER"
                onHit="return_product"
                style="red_button_style" />		
			
		</scene>
	</aspect>
</resource>
