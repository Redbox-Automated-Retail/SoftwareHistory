<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="play_pass_opt_in_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
			onEnter="play_pass_opt_in_view_enter"
			onLeave="play_pass_opt_in_view_leave">
      
			<actor
				name="background"
				x="0"
				y="0"
				width="1024"
				height="768"
        image="image_play_pass_prompt"
				optionFlags="Image" />
      
			<actor
				name="prompt_message"
				x="509"
				y="182"
				font="font_akzidenz_bold_30"
				foregroundColor="217,34,49"
				optionFlags="Text" />

      <actor
				name="confirm_message"
				x="173"
				y="190"
        width="680"
        height="100"
				font="font_akzidenz_bold_large"
        horizontalAlignment="center"
				foregroundColor="177,35,39"
        optionFlags="Text" />

      <actor
				name="email_message"
				x="173"
				y="330"
        width="680"
        height="100"
				font="font_akzidenz_18_bold"
        horizontalAlignment="center"
				foregroundColor="95,95,95"
				optionFlags="Text" />

      <actor
				name="bottom"
				x="0"
				y="632"
				width="1024"
				height="136"
				backgroundColor="58,55,55"
				optionFlags="Background" />
      
			<actor
				name="no_button"
				x="49"
				y="667"
				width="200"
				height="65"
				font="font_akzidenz_18_bold"
				cornerSize="30,30"
				style="gray_button_style" />
				
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