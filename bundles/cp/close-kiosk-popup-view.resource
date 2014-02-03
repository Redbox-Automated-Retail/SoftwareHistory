<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="close_kiosk_popup_view" />
	<aspect name="content">
		<scene name="CloseKiosk" clear="false">
			
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
				x="200"
				y="100"
				width="624"
				height="468"
				backgroundColor="White"
				borderColor="147, 3, 26"
				strokeWeight="5.0"
				cornerSize="28, 28"
				optionFlags="Background, DrawBorder" />
				
			<actor
				name="header_text"
				x="250"
				y="150"
				width="524"
				foregroundColor="147, 3, 26"
				font="medium_larger_font"
				optionFlags="Text" />
				
			<actor
				name="message_text"
				x="250"
				y="200"
				width="524"
				foregroundColor="92, 92, 92"
				font="medium_large_font"
				optionFlags="Text" />

			<actor
				name="confirm_textbox"
				x="343"
				y="372"
				width="337"
				height="64"
				textRegion="2,2,333,60"
				font="medium_large_font"
				cornerSize="10, 10"
				foregroundColor="black"
				backgroundColor="white"
				borderColor="black"
				text="${CloseKiosk:ConfirmText}"
				verticalAlignment="center"
				horizontalAlignment="center"
				optionFlags="Text, Background, DrawBorder"
				hitFlags="Enabled"
				visible="false"/>

			<actor
				name="button_1"
				x="250"
				y="468"
				width="150"
				height="75"
				backgroundColor="147, 3, 26"
				foregroundColor="White"
				font="medium_large_font"
				cornerSize="10, 10"
				horizontalAlignment="Center"
				verticalAlignment="Center"
				optionFlags="Background, Text, TextShadow"
				hitFlags="Enabled" />
				
			<actor
				name="button_2"
				x="624"
				y="468"
				width="150"
				height="75"
				backgroundColor="147, 3, 26"
				foregroundColor="White"
				font="medium_large_font"
				cornerSize="10, 10"
				horizontalAlignment="Center"
				verticalAlignment="Center"
				optionFlags="Background, Text, TextShadow"
				hitFlags="Enabled" />
				
			<actor
				name="spinner"
				x="496"
				y="318"
				image="spinner"
				visible="False"
				optionFlags="Image" />
				
		</scene>
	</aspect>
</resource>