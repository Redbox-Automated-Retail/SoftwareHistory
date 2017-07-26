<?xml version="1.0" encoding="utf-8"?>
<resource type="view">
	<property name="name" value="browse_filter_alpha_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="229, 229, 229"
			stylesheet="style_sheet_common"
			onEnter="browse_filter_alpha_view_enter"
			onLeave="browse_filter_alpha_view_leave">

      <!-- Header Background and Title Text -->

      <actor
				name="toppart_gray_background"
				x="0"
				y="0"
				optionFlags="Background"
				backgroundColor="60,56,57"
				width="1024"
				height="70"
				visible="true"/>
      <actor
				name="title_browsing_h1"
				x="40"
				y="0"
				height="70"
				width="944"
				hitFlags="None"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="center"
				visible="true"
				font="font_target_medium_large"
				text="${Browse:ProductFamily}"
				optionFlags="Text" />

      <!-- all other actors are created by the controls in .Net -->
		</scene>
	</aspect>
</resource>
