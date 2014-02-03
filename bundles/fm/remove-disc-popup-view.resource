<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="remove_disc_popup_view" />
  <aspect name="content">
    <scene name="RemoveDiscPopup"
      clear="false"
      onEnter="remove_disc_popup_enter"
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
		text="  Remove Selected Discs"
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font"
		cornerSize="10,10"
		width="721"
		height="50" />

      <actor
        name="title_label"
        x="115"
        y="58"
        width="200"
        height="35"
        font="medium_font"
        optionFlags="Text, TextShadow"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Title"
        visible="true"
        enabled="true" />

      <actor
        name="barcode_label"
        x="385"
        y="58"
        width="105"
        height="35"
        font="medium_font"
        optionFlags="Text, TextShadow"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Barcode"
        visible="true"
        enabled="true" />

      <actor
        name="deck_label"
        x="485"
        y="58"
        width="60"
        height="35"
        font="medium_font"
        optionFlags="Text, TextShadow"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Deck"
        visible="true"
        enabled="true" />

      <actor
        name="slot_label"
        x="543"
        y="58"
        width="70"
        height="35"
        font="medium_font"
        optionFlags="Text, TextShadow"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Slot"
        visible="true"
        enabled="true" />

	  <actor
        name="progress_label"
        x="50"
        y="567"
        width="200"
        height="30"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        font="medium_font"
        optionFlags="Text, TextShadow"
        visible="true"
        enabled="true" />

	  <actor
		name="footer"
		x="2"
		y="530"
		optionFlags="Background, DrawRoundedCorners"
		backgroundColor="64,64,64"
		cornerSize="10,10"
		width="721"
		height="110" />

	  <actor
        name="cancel_button"
        x="500"
        y="560"
        width="198"
        height="58"
        text="Cancel"
        foregroundColor="white"
		backgroundColor="DarkRed"
		strokeWeight="3.0"
		borderColor="White"
		cornerSize="15,15"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        verticalAlignment="center"
        horizontalAlignment="center"
        font="medium_large_font"
        visible="true"
        enabled="true"
        hitFlags="Enabled" />
		
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
        hitFlags="Enabled" />

    </scene>
  </aspect>
</resource>
