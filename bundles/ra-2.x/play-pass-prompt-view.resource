<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="play_pass_prompt_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundImage="image_play_pass_prompt"
			stylesheet="style_sheet_common"
			onEnter="play_pass_prompt_view_enter"
			onLeave="play_pass_prompt_view_leave">
      
			<actor
				name="message"
				x="509"
				y="182"
				font="font_akzidenz_bold_28"
				foregroundColor="217,34,49"
				optionFlags="Text" />
      
      <actor
				name="disclaimer_message"
				x="509"
				y="425"
        width="460"
        height="180"
				font="font_akzidenz_regular_smaller_13"
				foregroundColor="black"
				optionFlags="Text" />
      
      <actor
				name="disclaimer_button"
				x="766"
				y="529"
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