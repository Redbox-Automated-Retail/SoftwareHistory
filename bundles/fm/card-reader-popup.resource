<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="card_reader_popup_view" />
  <property name="active_flag" value="active" />
  <aspect name="content">
    <scene name="CardReaderPopup"
      cached="false"
	  clear="false"
      onEnter="card_reader_popup_enter"
      viewWindow="150,222,704,768" 
	  stylesheet="style_sheet_common_fma">

	<actor
		name="lightbox"
		x="-150"
		y="-222"
		height="768"
		width="1024"
		backgroundColor="black"
		opacity="0.5"
		optionFlags="Background"
		/>
		
 	<actor
		name="background"
		x="0"
		y="0"
		width="724"
		height="304"
		backgroundColor="Gray"
		strokeWeight="3.0"
		borderColor="192,192,192"
		cornerSize="10,10"
		optionFlags="Background, DrawBorder"
        hitFlags="Enabled, Obstruction"
		/>

	<actor
		name="title_area"
		x="2"
		y="2"
		optionFlags="Background, Text, TextShadow, DrawRoundedCorners"
		backgroundColor="DarkRed"
		text="  Examine Kiosk"
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font"
		cornerSize="10,10"
		width="721"
		height="50"
		/>

	<actor
		name="close_button"
		x="647"
		y="0"
		width="77"
		height="52"
		font="large_font"
		foregroundColor="white"
		backgroundColor="192,192,192"
		verticalAlignment="center"
		horizontalAlignment="center"
		cornerSize="10,10"
		text="X"
		optionFlags= "Background, Text, DrawRoundedCorners"
		hitFlags="Enabled"
		onHit="go_back"/>

	<actor
		name="card_reader_body"
		x="40"
		y="80"
		width="644"
		foregroundColor="white"
		horizontalAlignment="near"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="${CardReaderPopup:Body}" />

	<actor
		name="card_reader_yes_button"
		x="140"
		y="200"
		width="180"
		height="80"
		text="YES"
		font="medium_large_font"
		cornerSize="15,15"
		style="green_button_style"
		visible="true" />
		
	<actor
		name="card_reader_no_button"
		x="404"
		y="200"
		width="180"
		height="80"
		text="NO"
		font="medium_large_font"
		cornerSize="15,15"
		style="red_button_style"
		visible="true" />
		
    </scene>
  </aspect>
</resource>