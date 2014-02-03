<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="vmz_merch_flow_view" />
  <property name="active_flag" value="active" />
  <aspect name="content">
	<scene name="VMZMerchFlow"
		width="1024"
		height="768"
		onEnter="vmz_merch_flow_enter"
		onLeave="vmz_merch_flow_leave"
		stylesheet="style_sheet_common_fma">

	<actor
		name="header_area"
		x="0"
		y="0"
		textregion="40,3,300,44"
		optionFlags="Background"
		backgroundColor="DarkRed"	
		width="1024"
		height="50"
		visible="true" />
		
	<actor
		name="header_area_text"
		x="40"
		y="0"
		width="1024"
		height="50"
		optionFlags="Text, TextShadow"
		text="Merchandize VMZ"
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font"
		/>
		
	  <actor
		name="close_area"
		x="940"
		y="0"
		width="84"
		height="51"
		verticalAlignment="center"
		horizontalAlignment="center"
		foregroundColor="White"
		backgroundColor="192,192,192"
		font="medium_larger_font"
		text= "X"
		hitFlags="Enabled"
		optionFlags="Background, Text"
		visible="false"
		enabled="false" 
		/>

	<actor
		name="title_area"
		x="40"
		y="60"
		optionFlags="Text"
		text="${VMZMerchFlow:Title}"
		foregroundColor="Black"
		verticalAlignment="center"
		font="medium_larger_font"
		width="1024"
		height="50"
		visible="true" />
	
	<actor
		name="background_body_area"
		x="30"
		y="120"
		height="618"
		width="964"
		optionFlags="Background, DrawBorder"
		backgroundColor="gray"
		foregroundColor="White"
		strokeWeight="3.0"
		borderColor="White"
		cornerSize="15,15"
		visible="false" />
		
	<actor
		name="body_text"
		x="50"
		y="160"
		width="964"
		text="${VMZMerchFlow:Body}"
		optionFlags="Text, TextShadow"
		foregroundColor="White"
		font="font_target_small"
		visible="true" />
	
	<!-- Buttons -->	
	<actor
		name="vend_button"
		x="564"
		y="638"
		width="180"
		height="80"
		text="${VMZMerchFlow:VendButton}"
		font="medium_large_font"
		cornerSize="15,15"
		style="red_button_style"
		visible="false" />

	<actor
		name="continue_button"
		x="784"
		y="638"
		width="180"
		height="80"
		text="${VMZMerchFlow:ContinueButton}"
		font="medium_large_font"
		cornerSize="15,15"
		style="red_button_style"
		visible="false"/>		
	  
	</scene>
  </aspect>
</resource>