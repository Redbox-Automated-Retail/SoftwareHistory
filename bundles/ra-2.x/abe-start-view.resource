<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="abe_start_view" />
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
			onEnter="abe_start_view_enter"
			onLeave="abe_start_view_leave" >

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="start_view_background_lighter"
				optionFlags="Image" />

			<actor
				name="ada_icon"
				x="54"
				y="150"
				image="image_ada_image"
				optionFlags="Image"
				visible="true" />

			<actor
				name="redbox_logo"
				x="339"
				y="167"
				optionFlags="Image"
				image="image_redbox_logo_ada_page" />
				
			<actor
				name="abe_mode_text"
				x="548"
				y="167"
				width="450"
				height="100"
				optionFlags="Text"
				font="font_target_large_46"
				foregroundColor="153,14,22"
				horizontalAlignment="near"
				visible="true" />
			
			<actor
				name="abe_mode_description"
				x="346"
				y="267"
				width="580"
				height="300"
				optionFlags="Text"
				foregroundColor="0, 0, 0"
				font="font_target_small_medium"
				visible="true" />

			<actor
				name="abe_espanol_button"
				x="17"
				y="525"
				width="131"
				height="41"
				text="ESPA&#209;OL"
				font="font_akzidenz_bold_smaller"
				cornerSize="20,26"
				style="gray_button_white_border_style" />

			<actor
				name="abe_handicap_button"
				x="166"
				y="526"
				width="134"
				height="42"
				optionFlags="Image, Text"
				font="font_akzidenz_bold_smaller"
				foregroundColor="0, 0, 0"
				textRegion="48, 13, 100, 35"
				image="image_ada_on_button"
				text="ADA ON"
				hitFlags="Enabled" />

			<actor
				name="abe_help_button"
				x="876"
				y="526"
				width="131"
				height="41"
				onHit="help"
				text="HELP"
				font="font_akzidenz_bold_smaller"
				cornerSize="20,26"
				style="gray_button_white_border_style" />

			<actor
				name="abe_tickets_button"
				x="22"
				y="625"
				width="313"
				height="106"
				cornerSize="57,56"
				font="font_target_medium"
				text="TICKETS"
				style="red_button_style"
				strokeWeight="4" />

			<actor
				name="abe_games_button"
				x="356"
				y="625"
				width="313"
				height="106"
				cornerSize="57,56"
				font="font_target_medium"
				text="GAMES"
				onHit="browse_products"
				style="red_button_style"
				strokeWeight="4" />

			<actor
				name="abe_all_movies_button"
				x="690"
				y="625"
				width="313"
				height="106"
				cornerSize="57,56"
				font="font_target_medium"
				text="MOVIES"
				onHit="browse_products"
				style="red_button_style"
				strokeWeight="4" />
		</scene>
	</aspect>
</resource>
