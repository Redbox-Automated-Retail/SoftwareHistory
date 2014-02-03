<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="configure_kiosk_view" />
	<aspect name="content">

	<scene name="ConfigureKiosk"
	clear="false"
	viewWindow="149,62,726,642" 
	onEnter="configure_kiosk_on_enter">

 	  <actor
		name="background"
		x="0"
		y="0"
		width="768"
		height="642"
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
		optionFlags="Background, DrawRoundedCorners"
		backgroundColor="DarkRed"
		cornerSize="10,10"
		width="764"
		height="50"
		/>

	  <actor
		name="title_label"
		x="12"
		y="2"
		optionFlags="Text, TextShadow"
		text=""
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_large_font"
		width="764"
		height="50"
		/>

		<actor
		name="close_area"
		x="688"
		y="0"
		width="78"
		height="52"
		verticalAlignment="center"
		horizontalAlignment="center"
		foregroundColor="White"
		backgroundColor="192,192,192"
		cornerSize="10,10"
		font="medium_larger_font"
		text= "X"
		onHit="configure_kiosk_close"
		hitFlags="Enabled"
		optionFlags="Background, Text, DrawRoundedCorners"
		/>

      <actor
        name="selected_row"
        x="0"
        y="0"
        width="696"
        height="60"
        optionFlags="Image"
        image="glow"
        visible="false"
        enabled="true" 
		/>

     <actor
        name="header_image"
        x="2"
        y="56"
        width="764"
        height="40"
        optionFlags="Background, Text"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="white"
		backgroundColor="48,48,48"
        font="medium_font"
        text=""
        visible="true"
        enabled="true"
		/>

	  <actor
		name="footer"
		x="2"
		y="530"
		optionFlags="Background, DrawRoundedCorners"
		backgroundColor="64,64,64"
		cornerSize="10,10"
		width="764"
		height="110"
		/>

      <actor
        name="button_1"
		x="0"
		y="0"
        width="130"
        height="65"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Image, Text, TextShadow, Background, DrawBorder"
		font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        hitFlags="Enabled"
        visible="false"
        enabled="false"
		/>

      <actor
        name="button_2"
		x="0"
		y="0"
        width="130"
        height="65"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Image, Text, TextShadow, Background, DrawBorder"
        verticalAlignment="center"
        horizontalAlignment="center"
        font="medium_font"
        hitFlags="Enabled"
        visible="false"
        enabled="false"
		/>
		
		<actor
        name="button_3"
		x="0"
		y="0"
        width="130"
        height="65"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Image, Text, TextShadow, Background, DrawBorder"
        verticalAlignment="center"
        horizontalAlignment="center"
        font="medium_font"
        hitFlags="Enabled"
        visible="false"
        enabled="false"
        />
		
	<actor
		name="scroll_bar_background"
		x="705"
		y="96"
		width="60"
		height="434"
		backgroundColor="180,180,180"
		optionFlags="Background"
		visible="true"
		/>
		
	<actor
		name="scroll_up_background"		
		x="710"
		y="101"
		width="50"
		height="70"
		cornerSize="5,5"
		borderColor="32,32,32"
		backgroundColor="96,96,96"
		optionFlags="Background,DrawBorder"
		/>
		
	   <actor
		name="scroll_up_button"
		x="717"
		y="111"
		width="35"
		height="35"
		image="scroll_up_disabled"
		optionFlags="Image"
		hitFlags="Enabled"
		priority="4"
        enabled="False"
		/>
		
	<actor
		name="scroll_down_background"		
		x="710"
		y="454"
		width="50"
		height="70"
		cornerSize="5,5"
		borderColor="32,32,32"
		backgroundColor="96,96,96"
		optionFlags="Background,DrawBorder"
		/>
		
	   <actor
		name="scroll_down_button"
		x="717"
		y="476"
		width="35"
		height="35"
		image="scroll_down_disabled"
		optionFlags="Image"
		hitFlags="Enabled"
		priority="6"
        enabled="False"
		/>
      
   
    </scene>
  </aspect>
</resource>
