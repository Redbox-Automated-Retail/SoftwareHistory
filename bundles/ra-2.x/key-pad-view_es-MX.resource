<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="key_pad_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
      name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			backgroundImage="image_zipcode_background"
			onEnter="key_pad_view_enter" >

      <actor
				name="ad"
				x="178"
				y="25"
				width="668"
				height="83"
        visible="false"
				optionFlags="Image" />

      <actor
				name="bg_top_border"
				x="20"
				y="72"
				width="984"
				height="2"
				optionFlags="Background"
				backgroundColor="44,46,50" />

      <actor
				name="zipcode_label"
				x="20"
				y="20"
				font="font_target_medium"
				optionFlags="Text"
				foregroundColor="44,46,50"
				text="C&#211;DIGO POSTAL" />

      <actor
				name="zipcode_label_small"
				x="20"
				y="88"
				font="font_target_small_medium"
				optionFlags="Text"
				foregroundColor="44,46,50"
				text="C&#211;DIGO POSTAL"
        visible="false" />

      <actor
				name="text_label"
				x="372"
				y="124"
				width="275"
				height="67"
				font="font_target_small_bold"
				text="${KeyPad:Text}"
				verticalAlignment="center"
				horizontalAlignment="center"
				optionFlags="Text" />

      <actor
				name="one_button_shadow"
				x="370"
				y="220"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="one_button"
				x="372"
				y="218"
				width="75"
				height="75"
				tag="1"
				text="1"
				font="font_target_small_bold"
				onHit="key_pad_update" 
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="two_button_shadow"
				x="472"
				y="220"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="two_button"
				x="474"
				y="218"
				width="75"
				height="75"
				tag="2"
				text="2"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="three_button_shadow"
				x="574"
				y="220"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="three_button"
				x="576"
				y="218"
				width="75"
				height="75"
				tag="3"
				text="3"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="four_button_shadow"
				x="370"
				y="312"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="four_button"
				x="372"
				y="310"
				width="75"
				height="75"
				tag="4"
				text="4"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="five_button_shadow"
				x="472"
				y="312"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="five_button"
				x="474"
				y="310"
				width="75"
				height="75"
				tag="5"
				text="5"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="six_button_shadow"
				x="574"
				y="312"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="six_button"
				x="576"
				y="310"
				width="75"
				height="75"
				tag="6"
				text="6"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="seven_button_shadow"
				x="370"
				y="404"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="seven_button"
				x="372"
				y="402"
				width="75"
				height="75"
				tag="7"
				text="7"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="eight_button_shadow"
				x="472"
				y="404"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="eight_button"
				x="474"
				y="402"
				width="75"
				height="75"
				tag="8"
				text="8"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="nine_button_shadow"
				x="574"
				y="404"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="nine_button"
				x="576"
				y="402"
				width="75"
				height="75"
				tag="9"
				text="9"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="clearall_button_shadow"
				x="370"
				y="496"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
        name="clearall_button"
        x="372"
        y="494"
        width="75"
        height="75"
        text="CANCELA"
        font="font_akzidenz_bold_tiny"
        onHit="key_pad_update"
        cornerSize="36,36"
        style="rubine_button_style_white_border" />

      <actor
				name="zero_button_shadow"
				x="472"
				y="496"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
				name="zero_button"
				x="474"
				y="494"
				width="75"
				height="75"
				tag="0"
				text="0"
				font="font_target_small_bold"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="rubine_button_style_white_border" />

      <actor
				name="back_button_shadow"
				x="574"
				y="496"
				width="79"
				height="77"
				cornerSize="36,36"
        backgroundColor="215,215,215"
        opacity="100"
        optionFlags="Background"/>
      <actor
        name="back_button"
        x="576"
        y="494"
        width="75"
        height="75"
        text="ATR&#193;S"
        font="font_akzidenz_bold_tiny"
        onHit="key_pad_update"
        cornerSize="36,36"
        style="rubine_button_style_white_border" />
				
			<actor
				name="note_label"
				x="20"
				y="640"
				width="700"
				foregroundColor="white"
				optionFlags="Text"
				font="font_akzidenz_regular_smaller_13"
				text="El c&#243;digo postal asociado a su tarjeta de cr&#233;dito es necesario para fines de verificaci&#243;n." />
      
      <actor
				name="note_label2"
				x="20"
				y="680"
				width="700"
				foregroundColor="white"
				optionFlags="Text"
				font="font_akzidenz_regular_smaller_13"
				text="${KeyPad:AuthText}" />
			
      <actor
        name="next_button"
        x="794"
        y="660"
        width="196"
        height="66"
        onHit="key_pad_update"
        text="CONTINUAR"
        font="font_target_small"
        cornerSize="32,32"
        style="white_rubine_next_button_style" />

      
      <actor
        name="next_button_right_arrow_icon"
        x="953"
        y="689"
        image="image_next_red_arrow"
        hitFlags="enabled"
        optionFlags="Image" />
		</scene>	
	</aspect>
</resource>