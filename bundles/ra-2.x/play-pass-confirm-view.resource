<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="play_pass_confirm_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundImage="image_play_pass_confirm"
			stylesheet="style_sheet_common"
			onEnter="play_pass_confirm_view_enter"
			onLeave="play_pass_confirm_view_leave">
      
      <actor
				name="message"
				x="163"
				y="190"
        width="700"
        height="100"
				font="font_akzidenz_bold_large"
        horizontalAlignment="center"
				foregroundColor="177,35,39"
        optionFlags="Text" />

      <actor
				name="email_message"
				x="123"
				y="330"
        width="780"
        height="100"
				font="font_akzidenz_18_bold"
        horizontalAlignment="center"
				foregroundColor="95,95,95"
				optionFlags="Text" />
      
      <actor
				name="disclaimer_message"
				x="100"
				y="540"
        width="824"
        height="80"
				font="font_akzidenz_regular_smaller_13"
        horizontalAlignment="center"
				foregroundColor="black"
				optionFlags="Text" />
      
      <actor
				name="disclaimer_button"
				x="641"
				y="585"
				width="90"
				height="30"
				font="font_akzidenz_bold_smaller"
				cornerSize="30,30"
				style="gray_button_style" />
      
      <actor
				name="bottom"
				x="0"
				y="632"
				width="1024"
				height="136"
				backgroundColor="58,55,55"
				optionFlags="Background" />
      
			<actor
				name="yes_button"
				x="775"
				y="667"
				width="200"
				height="65"
				font="font_akzidenz_18_bold"
				cornerSize="30,30"
				style="red_button_style" />
		</scene>
	</aspect>
</resource>