<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="key_pad_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			backgroundImage="image_zipcode_background"
			onEnter="key_pad_view_enter" >

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
				text="ZIPCODE" />
				
			<actor
				name="text_label"
				x="372"
				y="124"
				width="275"
				height="67"
				font="font_target_small_bold"
				strokeWeight="3"				
				text="${KeyPad:Text}"
				foregroundColor="0,0,0"
				verticalAlignment="center"
				horizontalAlignment="center"
				image="image_zip_entry_field"
				optionFlags="Image, Text" />

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
				style="red_button_style_bold_border" /> 
				
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
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
			<actor
				name="clearall_button"
				x="372"
				y="494"
				width="75"
				height="75"
				text="CLEAR ALL"
				font="font_akzidenz_bold_tiny"
				onHit="key_pad_update"
				cornerSize="36,36"
				textRegion="12,22,50,35"
				style="red_button_style_bold_border" />
				
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
				style="red_button_style_bold_border" />
				
			<actor
				name="back_button"
				x="576"
				y="494"
				width="75"
				height="75"
				text="BACK"
				font="font_akzidenz_bold_tiny"
				onHit="key_pad_update"
				cornerSize="36,36"
				style="red_button_style_bold_border" />

				
			<actor
				name="note_label"
				x="30"
				y="677"
				width="450"
				foregroundColor="128,128,128"
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				text="Note: The zipcode associated with your credit card is needed for verification purposes only." />
				
			<actor
				name="next_button"
				x="794"
				y="660"
				width="196"
				height="66"
				onHit="key_pad_update"
				text="          NEXT"
				font="font_target_small"
				cornerSize="32,32"
				style="red_next_button_style" /> 
					
	       	<actor
                name="next_button_right_arrow_icon"
                x="915"
                y="682"
				image="image_email_entry_right_arrow_icon"
                hitFlags="enabled"
                optionFlags="Image" />
				
			<actor
                name="image_next_white_arrow_big"
                x="915"
                y="682"
				onHit="key_pad_update"
				image="image_next_white_arrow_big"
                hitFlags="enabled"
                optionFlags="Image" />	 
		</scene>
	</aspect>
</resource>
