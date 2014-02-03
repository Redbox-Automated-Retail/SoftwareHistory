<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="deck_popup_view" />
  <aspect name="content">
    <scene name="DeckPopup"
      clear="false"
      onEnter="deck_popup_enter"
      viewWindow="149,62,726,642" >

 		<actor
		name="background"
		x="0"
		y="0"
		width="726"
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
		optionFlags="Background, Text, TextShadow, DrawRoundedCorners"
		backgroundColor="DarkRed"
		text="  Select Decks"
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font"
		cornerSize="10,10"
		width="721"
		height="50"
		/>

		<actor
		name="close_area"
		x="649"
		y="0"
		width="77"
		height="52"
		verticalAlignment="center"
		horizontalAlignment="center"
		foregroundColor="White"
		backgroundColor="192,192,192"
		cornerSize="10,10"
		font="medium_larger_font"
		text= "X"
		onHit="go_back"
		hitFlags="Enabled"
		optionFlags="Background, Text, DrawRoundedCorners"
		/>

      <actor
		name="all_button"
		x="157"
		y="500"
		width="135"
		height="109"
		strokeWeight="3.0"
		borderColor="64,64,64"
		backgroundColor="192,192,192"
		foregroundColor="black"
		optionFlags="Background, Text, DrawBorder"
		hitFlags="Enabled"
		font="medium_large_font"
		verticalAlignment="center"
		horizontalAlignment="center"
		onHit="select_all"
		text="All"
		visible="true"
		enabled="true" />

      <actor
		name="clear_button"
		x="304"
		y="500"
		width="135"
		height="109"
		strokeWeight="3.0"
		borderColor="64,64,64"
		backgroundColor="192,192,192"
		foregroundColor="black"
		optionFlags="Background, Text, DrawBorder"
		hitFlags="Enabled"
		font="medium_large_font"
		verticalAlignment="center"
		horizontalAlignment="center"
		onHit="clear_selection"
		text="Clear"
		visible="true"
		enabled="true" />

      <actor
		name="enter_button"
		x="451"
		y="500"
		width="135"
		height="109"
		strokeWeight="3.0"
		borderColor="64,64,64"
		backgroundColor="192,192,192"
		foregroundColor="black"
		optionFlags="Background, Text, DrawBorder"
		hitFlags="Enabled"
		font="medium_large_font"
		verticalAlignment="center"
		horizontalAlignment="center"
		onHit="set_decks"
		text="Enter"
		visible="true"
		enabled="true" />

    </scene>
  </aspect>
</resource>
