<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="browse_view_collection" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="24, 24, 24"
			stylesheet="style_sheet_common"
			onEnter="browse_view_collection_enter">

<!-- Page Background Graphics -->

			<actor
				name="top_part_gray_background"
				x="0"
				y="0"
				optionFlags="Text, Background"
				backgroundColor="167,167,167"
				foregroundColor="white"
				width="1024"
				height="70"
				text="Place holder for top menu"
				verticalAlignment="center"
				horizontalAlignment="center"
				visible="true"/>

			<actor
				name="lowerpart_gray_background"
				x="0"
				y="564"
				optionFlags="Background"
				backgroundColor="167,167,167"
				width="1024"
				height="60"
				visible="true" />

<!-- Collection stuff-->
			<actor
				name="collection_title"
				x="0"
				y="0"
				optionFlags="Text, Background"
				backgroundColor="167,167,167"
				foregroundColor="24,24,24"
				font="font_akzidenz_bold_large"
				width="1024"
				height="60"
				verticalAlignment="center"
				horizontalAlignment="center"
				visible="true"/>

			<actor
				name="collection_legal_text"
				x="138"
				y="569"
				optionFlags="Text"
				foregroundColor="24,24,24"
				font="font_akzidenz_regular_smaller"
				width="663"
				height="50"
				verticalAlignment="center"
				horizontalAlignment="near"
				visible="true"/>

<!-- back button and more collections button stuff -->

			<actor
				name="back_button"
				x="8"
				y="576"
				width="115"
				height="36"
				addToFront="true"
				text="       ATR&#193;S"
				font="font_target_smaller"
				cornerSize="18,18"
				strokeWeight="2"
				enabled="true"
				borderColor="white"
				style="gray_button_style" />

			<actor
				name="back_arrow"
				x="28"
				y="585"
				width="21"
				height="18"
				hitFlags="None"
				image="image_back_white_arrow"
				optionFlags="Image" />

			<actor
				name="more_collections_button"
				x="816"
				y="576"
				width="200"
				height="36"
				addToFront="true"
				text="MAS COLECCIÓNES"
				font="font_target_smaller"
				cornerSize="18,18"
				strokeWeight="2"
				enabled="true"
				style="yellow_button_style" />

		</scene>
	</aspect>
</resource>
