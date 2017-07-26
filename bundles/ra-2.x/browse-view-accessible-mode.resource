<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="browse_view_ada_accessible" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="24, 24, 24"
			stylesheet="style_sheet_common"
			onEnter="browse_view_enter"
			onLeave="browse_view_leave">

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
				

<!-- SPECIAL HEADER ONLY USED FOR TITLE MARKETING VERSION -->

			<actor
				name="title_marketing_h1"
				x="40"
				y="25"
				height="40"
				width="944"
				optionFlags="Text"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="center"
				font="font_target_large"
				text="" />

      <!-- all other actors are created by the controls in .Net -->

    </scene>
	</aspect>
</resource>
