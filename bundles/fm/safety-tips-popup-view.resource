<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="safety_tips_popup_view" />
  <property name="active_flag" value="active" />
  <aspect name="content">
    <scene name="SafetyTipsPopup"
      cached="false"
      clear="false"
      onEnter="safety_tips_popup_view_enter"
      viewWindow="162,28,1024,768">
        
      <actor
        name="opacity_background"
        x="-162"
        y="-28"
        width="1024"
        height="768"
        backgroundColor="Black"
        opacity="0.5"
        optionFlags="Background" />
      
      <actor
        name="background"
        x="0"
        y="0"
        width="726"
        height="507"
        backgroundColor="Gray"
        strokeWeight="3.0"
        borderColor="192,192,192"
        cornerSize="10,10"
        optionFlags="Background, DrawBorder"
        hitFlags="Enabled, Obstruction" />
      
      <actor
        name="title_area"
        x="2"
        y="2"
        width="721"
        height="50" 
        optionFlags="Background, Text, TextShadow, DrawRoundedCorners"
        backgroundColor="DarkRed"
        text="  Safety Tips"
        foregroundColor="White"
        verticalAlignment="center"
        font="medium_larger_font"
        cornerSize="10,10" />
      
      <actor
        name="close_button"
        x="649"
        y="0"
        width="77"
        height="52"
        font="large_font"
        foregroundColor="White"
        backgroundColor="192,192,192"
        verticalAlignment="center"
        horizontalAlignment="center"
        cornerSize="10,10"
        text="X"
        optionFlags="Background, Text, DrawRoundedCorners"
        hitFlags="Enabled"
        onHit="go_back" />

      <actor
        name="safety_tip"
        x="20"
        y="125"
        width="690"
        height="200"
        font="large_regular_font"
        foregroundColor="Black"
        optionFlags="Text" />
      
      <actor
				name="ok_button"
				x="263"
				y="380"
				width="200"
				height="78"
				text="OK"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="White"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				hitFlags="Enabled"
        onHit="go_back" />
      
    </scene>
  </aspect>
</resource>