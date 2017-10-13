<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="reservation_not_found_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="228,32,69"
			stylesheet="style_sheet_common"
			onEnter="reservation_not_found_view_enter"
			onLeave="reservation_not_found_view_leave">
			
			<actor
				name="phone_image"
				width="279"
				height="304"
				x="670"
				y="298"
				image="image_reservation_not_found"
				optionFlags="Image"	/>

			<actor
				name="title_label"
				x="60"
				y="49"
				width="904"
				height="180"
				text="Your reservation is waiting.&#010;Let us find it for you!"
				font="font_target_large_52"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />

			<actor
				name="message_label"
				x="60"
				y="246"
				width="904"
				height="300"
				text="This box doesn&#39;t have a reservation for you. That might be because:&#010;&#010;-    You may have reserved at a different&#010;      location nearby.&#010;&#010;-    You may have used a different card&#010;      to reserve than you&#39;re using now."
				font="font_target_medium_20"
				foregroundColor="white"
				horizontalAlignment="near"
				optionFlags="Text"
				visible="true" />

			<actor
				name="footer_area"
				x="0"
				y="628"
				optionFlags="Background, TextShadow"
				backgroundColor="56,11,69"
				width="1024"
				height="142"
				visible="true" />
			
			<actor
				name="footer_highlight"
				x="0"
				y="626"
				optionFlags="Background, TextShadow"
				backgroundColor="56,11,69"
				width="1024"
				height="2"
				visible="true" />
				
			<actor
				name="reservation_not_found_ok_button"
				x="794"
				y="660"
				width="191"
				height="66"
				cornerSize="32,19" 
				font="font_target_small_button_overlays"
				text="OK"
				visible="true"	
				style="white_rubine_next_button_style" />

		</scene>
	</aspect>
</resource>
