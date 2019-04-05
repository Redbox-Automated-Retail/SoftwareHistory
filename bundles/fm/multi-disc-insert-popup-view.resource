<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="multi_disc_insert_popup_view" />
	<aspect name="content">
		<scene
			name="MultiDiscInsertPopup"
			clear="false"
			width="1024"
			height="768"
			onEnter="multi_disc_insert_popup_view_enter"
      onLeave="multi_disc_insert_popup_view_leave">
			
			<actor
				name="opacity_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				backgroundColor="Black"
				opacity="0.5"
				optionFlags="Background"
				hitFlags="Enabled" />
			
			<actor
				name="background"
				x="149"
				y="62"
				width="726"
				height="642"
				backgroundColor="165,165,165"
				strokeWeight="3.0"
				borderColor="192,192,192"
				cornerSize="10,10"
				optionFlags="Background, DrawBorder" />
			
			<actor
				name="title_area"
				x="151"
				y="64"
				width="721"
				height="50"
				optionFlags="Background, Text, TextShadow, DrawRoundedCorners"
				backgroundColor="DarkRed"
				text="  Multi-Disc Insert"
				foregroundColor="White"
				verticalAlignment="center"
				font="medium_larger_font"
				cornerSize="10,10" />
			
			<actor
				name="title_label"
				x="350"
				y="120"
				width="200"
				height="35"
				font="medium_font"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="near"
				text="Title" />
			
			<actor
				name="barcode_label"
				x="554"
				y="120"
				width="105"
				height="35"
				font="medium_font"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="near"
				text="Barcode" />
			
			<actor
				name="deck_label"
				x="669"
				y="120"
				width="60"
				height="35"
				font="medium_font"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="near"
				text="Deck" />
			
			<actor
				name="slot_label"
				x="747"
				y="120"
				width="70"
				height="35"
				font="medium_font"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="near"
				text="Slot" />
			
			<actor
				name="up_down_arrows_background"
				x="831"
				y="165"
				width="40"
				height="406"
				backgroundColor="96,96,96"
				cornerSize="10,10"
				borderColor="black"
				strokeWeight="2.0"
				opacity="0.50"
				optionFlags="Background,DrawBorder"
				hitFlags="Enabled"
				enabled="true"
				visible="true" />
			
			<actor
				name="up_arrow_background"
				x="831"
				y="165"
				width="40"
				height="70"
				cornerSize="10,10"
				borderColor="black"
				backgroundColor="96,96,96"
				hitFlags="Enabled"
				enabled="true"
				optionFlags="Background,DrawBorder" />
			
			<actor
				name="image_up_arrow"
				x="831"
				y="175"
				width="50"
				height="50"
				hitFlags="Enabled"
				image="up_arrow"
				optionFlags="Image" />
			
			<actor
				name="down_arrow_background"
				x="831"
				y="501"
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
				x="831"
				y="541"
				width="55"
				height="50"
				hitFlags="Enabled"
				image="down_arrow"
				optionFlags="Image" />
			
			<actor
				name="footer"
				x="151"
				y="592"
				width="721"
				height="110"
				optionFlags="Background, DrawRoundedCorners"
				backgroundColor="64,64,64"
				cornerSize="10,10" />
			
			<actor
				name="empty_slots_label"
				x="173"
				y="615"
				width="200"
				height="60"
				text="Empty Slots&#10;Remaining: ${EmptySlotsValue}"
				font="medium_large_font"
				foregroundColor="white"
				optionFlags="Text, TextShadow" />
			
			<actor
				name="time_out_message"
				x="550"
				y="615"
				width="275"
				height="90"
				text="Process will timeout after 10 seconds. After inserting last disc, please wait for vend door to close. After reviewing inserts, push 'X' to close."
				foregroundColor="white"
				optionFlags="Text, TextShadow"
				font="small_font" />
			
			<actor
				name="close_area_button"
				x="798"
				y="62"
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
				hitFlags="Enabled" />
			
		</scene>
	</aspect>
</resource>
