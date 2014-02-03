<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="vmz_merch_rebalances_view" />
  <property name="active_flag" value="active" />
  <aspect name="content">
	<scene name="VMZMerchRebalances"
		cached="false"
		clear="false"
		width="1024"
		height="768"
		backgroundColor="transparent"		
		onEnter="vmz_merch_rebalances_enter"
		onLeave="vmz_merch_rebalances_leave"
		stylesheet="style_sheet_common_fma">

	<actor
		name="opacity_background"
		x="0"
		y="0"
		width="1024"
		height="768"
		backgroundColor="Black"
		opacity="0.5"
		optionFlags="Background"/>

	<actor
		name="popup_background"
		x="100"
		y="120"
		width="824"
		height="528"
		backgroundColor="LightGray"
		strokeWeight="3.0"
		borderColor="White"
		optionFlags="Background, Drawborder"/>

	<actor
		name="rebalance_header_area"
		x="100"
		y="120"
		textregion="40,3,300,44"
		optionFlags="Background"
		backgroundColor="DarkRed"	
		width="824"
		height="50"
		visible="true" />
		
	<actor
		name="rebalance_header_area_text"
		x="130"
		y="120"
		width="784"
		height="50"
		optionFlags="Text, TextShadow"
		text=""
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font" />
		
	<actor
		name="rebalance_title_area"
		x="130"
		y="180"
		optionFlags="Text"
		text="${VMZRebalance:Title}"
		foregroundColor="Black"
		verticalAlignment="center"
		font="medium_larger_font"
		width="784"
		height="50"
		visible="true" />
	
	<actor
		name="rebalance_background_body_area"
		x="130"
		y="240"
		height="270"
		width="764"
		optionFlags="Background, DrawBorder"
		backgroundColor="White"
		foregroundColor="Black"
		strokeWeight="3.0"
		borderColor="Gray"
		cornerSize="15,15"
		visible="true" />
		
	<actor
		name="rebalance_column_headers_area"
		x="130"
		y="240"
		height="40"
		width="764"
		optionFlags="Background"
		backgroundColor="Gray"
		cornerSize="15,15"
		visible="true" />

	<actor
		name="rebalance_column_headers_square_off_area"
		x="131"
		y="255"
		height="27"
		width="762"
		optionFlags="Background"
		backgroundColor="Gray"
		visible="true" />

	<actor
		name="scroll_bar_background"
		x="841"
		y="283"
		width="50"
		height="224"
		backgroundColor="180,180,180"
		cornerSize="10,10"
		optionFlags="Background"
		visible="true" />
		
	<actor
		name="scroll_up_background"		
		x="846"
		y="288"
		width="40"
		height="40"
		cornerSize="5,5"
		borderColor="32,32,32"
		backgroundColor="96,96,96"
		optionFlags="Background,DrawBorder"	/>
		
	<actor
		name="rebalance_scroll_up"
		x="850"
		y="292"
		width="32"
		height="32"
		image="image_scroll_up"
		optionFlags="Image"
		hitFlags="Enabled"
		priority="4"
        enabled="true"	/>
		
	<actor
		name="scroll_down_background"		
		x="846"
		y="463"
		width="40"
		height="40"
		cornerSize="5,5"
		borderColor="32,32,32"
		backgroundColor="96,96,96"
		optionFlags="Background,DrawBorder"	/>
		
	<actor
		name="rebalance_scroll_down"
		x="850"
		y="467"
		width="32"
		height="32"
		image="image_scroll_down"
		optionFlags="Image"
		hitFlags="Enabled"
		priority="6"
		enabled="true" />
		
	<actor
		name="rebalance_scroll_track"		
		x="846"
		y="333"
		width="40"
		height="40"
		cornerSize="5,5"
		borderColor="32,32,32"
		backgroundColor="White"
		optionFlags="Background,DrawBorder"	/>

	<actor
		name="rebalance_quantity_header_text"
		x="145"
		y="240"
		width="105"
		height="40"
		text="${VMZRebalance:QuantityHeader}"
		foregroundColor="White"
		optionFlags="Text, TextShadow"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_title_header_text"
		x="260"
		y="240"
		width="105"
		height="40"
		text="${VMZRebalance:TitleHeader}"
		foregroundColor="White"
		optionFlags="Text, TextShadow"
		verticalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_quantity_text_line1"
		x="145"
		y="292"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />
	
	<actor
		name="rebalance_quantity_text_line2"
		x="145"
		y="318"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />
		
	<actor
		name="rebalance_quantity_text_line3"
		x="145"
		y="344"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_quantity_text_line4"
		x="145"
		y="370"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_quantity_text_line5"
		x="145"
		y="396"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_quantity_text_line6"
		x="145"
		y="422"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_quantity_text_line7"
		x="145"
		y="448"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_quantity_text_line8"
		x="145"
		y="474"
		width="105"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_title_text_line1"
		x="260"
		y="292"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_title_text_line2"
		x="260"
		y="318"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_title_text_line3"
		x="260"
		y="344"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />
		
	<actor
		name="rebalance_title_text_line4"
		x="260"
		y="370"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />
		
	<actor
		name="rebalance_title_text_line5"
		x="260"
		y="396"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />

	<actor
		name="rebalance_title_text_line6"
		x="260"
		y="422"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />	
		
	<actor
		name="rebalance_title_text_line7"
		x="260"
		y="448"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />
		
	<actor
		name="rebalance_title_text_line8"
		x="260"
		y="474"
		width="580"
		height="26"
		foregroundColor="Black"
		optionFlags="Text, ElideText"
		verticalAlignment="center"
		horizontalAlignment="near"
		font="medium_large_font"
		visible="true" />
		
	<actor
		name="rebalance_instructions_area"
		x="140"
		y="547"
		optionFlags="Text"
		text="${VMZRebalance:Instructions}"
		foregroundColor="Black"
		verticalAlignment="center"
		font="medium_font"
		width="575"
		height="75"
		visible="true" />

	<!-- Buttons -->	
	<actor
		name="rebalance_close_button"
		x="733"
		y="540"
		width="160"
		height="80"
		text="${VMZRebalance:CloseButton}"
		font="medium_large_font"
		cornerSize="15,15"
		style="red_button_style"
		/>

	</scene>
  </aspect>
</resource>