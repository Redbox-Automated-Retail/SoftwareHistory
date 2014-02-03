<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="remove_frauds_view" />
  <aspect name="content">
    <scene
	name="RemoveFrauds"
	width="1024"
	height="768"
	onEnter="remove_frauds_view_enter">

		<actor
			name="title_arrow"
			x="0"
			y="0"
			optionFlags="Background, Image"
			backgroundColor="DarkRed"
			verticalAlignment="center"
			width="45"
			height="50"
			image="title_arrow"
			visible="true"
			tag="export_inventory"
			onHit="export_inventory"
			hitFlags="Enabled" />

		<actor
			name="title_area"
			x="45"
			y="0"
			optionFlags="Background, Text, TextShadow"
			backgroundColor="DarkRed"
			text="  Remove Frauds"
			foregroundColor="White"
			verticalAlignment="center"
			font="medium_larger_font"
			width="895"
			height="50"
			visible="true" />

		<actor
			name="title_warning_message"
			x="0"
			y="300"
			width="1024"
			height="60"
			text="WARNING! The kiosk is pulling out Fraud discs at the moment.&#10;Please hit 'Pause Pull' to stop removing Fraud discs."
			foregroundColor="Red"
			font="callout_large_font"
			horizontalAlignment="center"
			optionFlags="Text, TextShadow"
			visible="false" />

		<actor
			name="frauds_left_label"
			x="60"
			y="115"
			width="250"
			height="20"
			text="Frauds Remaining:"
			font="medium_large_font"
			verticalAlignment="near"
			horizontalAlignment="near"
			foregroundColor="black"
			optionFlags="Text"
			visible="true"
			enabled="true" />

		<actor
			name="frauds_remaining_count"
			x="305"
			y="115"
			width="50"
			height="20"
			textRegion="2,2,66,38"
			font="medium_large_font"
			verticalAlignment="near"
			horizontalAlignment="near"
			foregroundColor="black"
			optionFlags="Text"
			hitFlags="Enabled" />

		<actor
			name="pull_button"
			x="60"
			y="180"
			width="135"
			height="90"
			text="Pull&#10;Frauds"
			font="medium_large_font"
			verticalAlignment="center"
			horizontalAlignment="center"
			foregroundColor="white"
			backgroundColor="DarkRed"
			strokeWeight="3.0"
			borderColor="White"
			cornerSize="15,15"
			optionFlags="Background, Text, TextShadow, DrawBorder"
			visible="true"
			enabled="true"
			hitFlags="Enabled" />

		<actor
			name="pause_button"
			x="215"
			y="180"
			width="135"
			height="90"
			text="Pause Pull"
			font="medium_large_font"
			verticalAlignment="center"
			horizontalAlignment="center"
			foregroundColor="white"
			backgroundColor="DarkRed"
			strokeWeight="3.0"
			borderColor="White"
			cornerSize="15,15"
			optionFlags="Background, Text, TextShadow, DrawBorder"
			visible="true"
			enabled="true"
			hitFlags="Enabled" />

		<actor
			name="output_window_label"
			x="20"
			y="375"
			width="500"
			height="20"
			text="Output Window"
			font="medium_large_font"
			verticalAlignment="near"
			horizontalAlignment="near"
			optionFlags="Text"
			foregroundColor="black"
			visible="true"
			enabled="true" />

		<actor
			name="output_window_textbox"
			x="20"
			y="408"
			width="942"
			height="253"
			textRegion="5,2,925,250"
			font="small_medium_font"
			foregroundColor="black"
			backgroundColor="white"
			cornerSize="7,7"
			optionFlags="Text, Background"
			hitFlags="None" />

		<actor
			name="up_down_arrows_background"
			x="964"
			y="408"
			width="40"
			height="253"
			backgroundColor="96,96,96"
			cornerSize="10,10"
			borderColor="black"
			strokeWeight="2.0"
			opacity="0.50"
			optionFlags="Background, DrawBorder"
			hitFlags="Enabled"
			enabled="true"
			visible="true" />

		<actor
			name="up_arrow_background"
			x="964"
			y="408"
			width="40"
			height="70"
			cornerSize="10,10"
			borderColor="black"
			backgroundColor="96,96,96"
			hitFlags="Enabled"
			enabled="true"
			optionFlags="Background, DrawBorder" />

		<actor
			name="image_up_arrow"
			x="964"
			y="418"
			width="50"
			height="50"
			hitFlags="Enabled"
			image="up_arrow"
			optionFlags="Image" />

		<actor
			name="down_arrow_background"
			x="964"
			y="591"
			width="40"
			height="70"
			cornerSize="10,10"
			borderColor="black"
			backgroundColor="96,96,96"
			hitFlags="Enabled"
			enabled="true"
			optionFlags="Background,DrawBorder" />

		<actor
			name="image_down_arrow"
			x="964"
			y="631"
			width="55"
			height="50"
			hitFlags="Enabled"
			image="down_arrow"
			optionFlags="Image" />

		<actor
			name="footer"
			x="0"
			y="676"
			width="1024"
			height="92"
			backgroundColor="64,64,64"
			visible="true"
			enabled="true"
			optionFlags="Background" />

		<actor
			name="close_button"
			x="840"
			y="691"
			width="164"
			height="62"
			text="Close"
			font="medium_font"
			verticalAlignment="center"
			horizontalAlignment="center"
			foregroundColor="white"
			backgroundColor="DarkRed"
			strokeWeight="2.0"
			borderColor="White"
			cornerSize="10,10"
			optionFlags="Background, Text, TextShadow, DrawBorder"
			visible="true"
			enabled="true"
			hitFlags="Enabled" />

		<actor
			name="close_area"
			x="940"
			y="0"
			width="84"
			height="50"
			verticalAlignment="center"
			horizontalAlignment="center"
			foregroundColor="White"
			backgroundColor="192,192,192"
			font="medium_larger_font"
			text= "X"
			tag="logout"
			hitFlags="Enabled"
			optionFlags="Background, Text" />
	</scene>
  </aspect>
</resource>