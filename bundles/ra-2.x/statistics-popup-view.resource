<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="statistics_popup_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="StatisticsPopup"
			cached="false"
			clear="false"
			stylesheet="style_sheet_common"
			onEnter="statistics_popup_enter"
			onLeave="statistics_popup_leave"
			viewWindow="75,42,874,682" >

			<actor
				name="background"
				x="0"
				y="0"
				width="874"
				height="682"
				image="product_detail_background"
				strokeWeight="3.0"
				borderColor="92, 92, 92"
				optionFlags="DrawBorder, Image"
				hitFlags="Enabled, Obstruction" />

			<actor
				name="title_area"
				x="0"
				y="0"
				width="874"
				height="50"
				optionFlags="Background, Text, TextShadow"
				backgroundColor="92, 92, 92"
				foregroundColor="White"
				verticalAlignment="center"
				text="${StatisticsPopupView:TitleArea}"
				font="font_target_medium" />

			<actor
				name="product_image"
				x="738"
				y="60"
				width="120"
				height="165"
				hitFlags="None"
				strokeWeight="2.0"
				backgroundColor="0,0,0"
				optionFlags="Image, Background" />

			<actor
				name="title_rollup_button"
				x="100"
				y="610"
				width="190"
				height="46"
				visisble="false"
				font="font_target_small_14"
				cornerSize="23, 23"
				text="VIEW TITLE ROLLUP"
				style="red_button_style" />

			<actor
				name="dvd_button"
				x="300"
				y="610"
				width="190"
				height="46"
				visisble="false"
				font="font_target_small_14"
				cornerSize="23, 23"
				text="VIEW DVD TITLE"
				style="red_button_style" />

			<actor
				name="bluray_button"
				x="500"
				y="610"
				width="190"
				height="46"
				visisble="false"
				font="font_target_small_14"
				cornerSize="23, 23"
				text="VIEW BLU-RAY TITLE"
				style="red_button_style" />

			<actor
				name="close_button"
				x="708"
				y="610"
				width="140"
				height="46"
				font="font_target_small_button_overlays"
				cornerSize="23, 23"
				text="CLOSE"
				onHit="go_back"
				style="red_button_style" />
		</scene>
	</aspect>
</resource>
