<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="vmz_merch_popup_view" />
  <aspect name="content">
    <scene name="VMZMerchViewPopup"
	clear="false"
	onEnter="vmz_merch_view_popup_enter"
	viewWindow="149,62,726,642" >

 	  <actor
		name="background"
		x="0"
		y="0"
		width="726"
		height="642"
		backgroundColor="165,165,165"
		strokeWeight="3.0"
		borderColor="192,192,192"
		cornerSize="10,10"
		optionFlags="Background, DrawBorder"
		hitFlags="Enabled, Obstruction" />

	  <actor
		name="title_area"
		x="2"
		y="2"
		optionFlags="Background, Text, TextShadow, DrawRoundedCorners"
		backgroundColor="DarkRed"
		text="   Merchandizing View Popup"
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font"
		cornerSize="10,10"
		width="721"
		height="50" />

	  <actor
		name="close_area_button"
		x="649"
		y="0"
		width="77"
		height="52"
		text="X"
		foregroundColor="white"
		backgroundColor="192,192,192"
		cornerSize="10,10"
		optionFlags="Background, Text"
		verticalAlignment="center"
		horizontalAlignment="center"
		font="medium_larger_font"
		visible="true"
		enabled="true"
		onHit="go_back"
		hitFlags="Enabled" />
	
	<actor
		name="merch_image_box"
		x="10"
		y="60"		
		optionFlags="Background, Image, Text, TextShadow"
		hitFlags="Enabled"
		backgroundColor="192,192,192"
		width="550"
		height="550"
		visible="true"
		enabled="true" />	

    </scene>
  </aspect>
</resource>
