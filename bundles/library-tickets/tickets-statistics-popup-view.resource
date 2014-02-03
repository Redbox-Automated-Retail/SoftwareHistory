<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_statistics_popup_view" />
	<property name="version" value="2.0" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="TicketsStatisticsPopup"
			cached="false"
			clear="false"
			stylesheet="style_sheet_common"
			onEnter="tickets_statistics_popup_enter"
			onLeave="tickets_statistics_popup_leave"
			viewWindow="139,42,746,682" >

			<actor
				name="background"
				x="0"
				y="0"
				width="746"
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
				width="746"
				height="50"
				optionFlags="Background, Text, TextShadow"
				backgroundColor="92, 92, 92"
				text="  Product Information"
				foregroundColor="White"
				verticalAlignment="center"
				font="font_target_medium" />

			<actor
				name="close_button"
				x="580"
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
