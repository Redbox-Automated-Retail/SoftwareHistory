<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="vmz_merch_warning_view" />
  <aspect name="content">
    <scene name="VMZ_Warning"
      clear = "false"
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
        text="   Please Remove Unknowns"
        foregroundColor="White"
        verticalAlignment="center"
        font="medium_larger_font"
        cornerSize="10,10"
        width="721"
        height="50" />
      
      <actor
        name="body_area_background"
        x="10"
        y="70"
        optionFlags="Background, DrawRoundedCorners"
        backgroundColor="Gray"
        cornerSize="10,10"
        width="706"
        height="560" />
      
      <actor
        name="body_area"
        x="33"
        y="100"
        optionFlags="Text, TextShadow, DrawRoundedCorners"
        text="   In order to continue merchandizing please remove the unknowns in this machine."
        foregroundColor="White"
        verticalAlignment="Top"
        font="medium_larger_font"
        cornerSize="10,10"
        width="660"
        height="530" />

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
        onHit="return_to_main"
        hitFlags="Enabled" />

      <actor
		    name="ok_button"
		    x="213"
		    y="550"
		    width="300"
		    height="65"
		    backgroundColor="Green"
		    strokeWeight="2.0"
		    borderColor="White"
		    cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
		    font="medium_larger_font"
		    hitFlags="Enabled"
		    foregroundColor="white"
		    verticalAlignment="center"
		    horizontalAlignment="center"
		    onHit="return_to_main"
		    text="OK"
		    visible="true"
		    enabled="true" />
	  </scene>
  </aspect>
</resource>