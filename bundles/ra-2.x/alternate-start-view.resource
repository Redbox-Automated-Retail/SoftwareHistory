<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="alternate_start_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
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
				text="HELP"
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
				text="ESPA&#209;OL"
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
                style="red_button_style" />
				
			<actor
				name="movies_label_1"
				x="180"
				y="260"
				width="200"
				height="32"
				foregroundColor="white"
				font="font_target_larger"
				optionFlags="Text, TextShadow"
				text="FIND" 
				hitFlags="Enabled"
				onHit="browse_products"/>
				
			<actor
				name="movies_label_2"
				x="110"
				y="320"
				width="400"
				height="32"
				foregroundColor="white"
				font="font_target_largest"
				optionFlags="Text, TextShadow"
				text="MOVIES" 
				hitFlags="Enabled"
				onHit="browse_products"/>	
			
			<!-- Games -->	
			<actor
                name="find_games_button"
                x="535"
                y="180"
                width="475"
                height="346"
                cornerSize="40,20"
                onHit="browse_products"
                style="red_button_style" />
				
			<actor
				name="games_label_1"
				x="700"
				y="260"
				width="200"
				height="32"
				foregroundColor="white"
				font="font_target_larger"
				optionFlags="Text, TextShadow"
				text="FIND" 
				hitFlags="Enabled"
				onHit="browse_products"/>
				
			<actor
				name="games_label_2"
				x="630"
				y="320"
				width="400"
				height="32"
				foregroundColor="white"
				font="font_target_largest"
				optionFlags="Text, TextShadow"
				text="GAMES" 
				hitFlags="Enabled"
				onHit="browse_products"/>
				
			<actor
                name="online_pickup_button"
                x="20"
                y="570"
                width="475"
                height="95"
                cornerSize="44,34"
				font="font_target_medium"
                text="RESERVATION PICKUP"
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
                text="RETURN"
                onHit="return_product"
                style="red_button_style" />
						
		</scene>
	</aspect>
</resource>
