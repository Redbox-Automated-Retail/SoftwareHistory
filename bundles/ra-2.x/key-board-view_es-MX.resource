<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="key_board_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">		
		<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="key_board_view_enter">

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_keyboard_background"
				optionFlags="Image" />
				
			<actor
				name="message_label"
				x="28"
				y="78"
				width="808"
				height="48"
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				foregroundColor="44, 46, 50"
				text="${KeyBoard:MessageLabel}" />

			<actor
				name="error_label"
				x="32"
				y="120"
				width="808"
				height="24"
				optionFlags="Text"
				font="font_akzidenz_bold_small"
				foregroundColor="red"
				verticalAlignment="center"
				horizontalAlignment="center"
				text="${KeyBoard:ErrorLabel}" />

			<actor
				name="separator_1_top"
				x="10"
				y="75"
				width="1004"
				height="4"
				backgroundColor="44, 46, 50"
				optionFlags="Background"
				visible="true" />
			
			<actor
				name="panel_tag"
				x="30"
				y="17"
				foregroundColor="44, 46, 50"
				optionFlags="Text"
				font="font_target_medium"			
				text="${KeyBoard:PanelTag}" />
				
			<actor
				name="text_label"
				x="67"
				y="147"
				width="706"
				height="67"
				font="font_target_small_bold"
				strokeWeight="3"
				foregroundColor="0,0,0"
				optionFlags="Image, Text"
				verticalAlignment="center"
				horizontalAlignment="center"
				image="image_email_entry_field"
				text="${KeyBoard:Text}" />

			<actor
				name="back_button"
				x="787"
				y="147"
				width="136"
				height="66"
				cornerSize="31,31"
				font="font_akzidenz_bold_smaller"
				text="      ATRÁS" 
				tag="back" 
				onHit="key_board_update"
				style="red_button_style" /> 
			
			<actor
				name="back_button_arrow"
				x="814"
				y="171"
				hitFlags="Enabled"
				tag="back" 
				onHit="key_board_update"
				image="image_back_icon"
				optionFlags="image" />
			
			<actor
				name="clear_button"
				x="845"
				y="147"
				width="136"
				height="66"
				cornerSize="31,31"
				font="font_akzidenz_bold_smaller"
				text="     BORRAR" 
				tag="clear"
				visible="false"
				onHit="key_board_update"
				style="red_button_style" />  
				
			<actor
				name="clear_button_icon"
				x="863"
				y="169"
				hitFlags="Enabled"
				tag="clear" 
				onHit="key_board_update"
				image="image_clear_icon"
				visible="false"
				optionFlags="image" />
			
			<actor
				name="one_button"
				x="67"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="1"
				text="1" 
				onHit="key_board_update"
				style="red_button_style" /> 
			
			<actor
				name="two_button"
				x="137"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="2"
				text="2" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="three_button"
				x="207"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="3"
				text="3" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="four_button"
				x="277"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="4"
				text="4" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="five_button"
				x="347"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="5"
				text="5" 
				onHit="key_board_update"
				style="red_button_style" />
			
			<actor
				name="six_button"
				x="417"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="6"
				text="6" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="seven_button"
				x="487"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="7"
				text="7" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="eight_button"
				x="557"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="8"
				text="8" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="nine_button"
				x="627"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="9"
				text="9" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="zero_button"
				x="697"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="0"
				text="0" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="yahoo_button"
				x="767"
				y="227"
				width="187"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_tiny"
				tag="@YAHOO.COM"
				text="@YAHOO.COM"
                visible="false" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="q_button"
				x="97"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="Q"
				text="Q" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="w_button"
				x="167"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="W"
				text="W" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="e_button"
				x="237"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="E"
				text="E" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="r_button"
				x="307"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="R"
				text="R" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="t_button"
				x="377"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="T"
				text="T" 
				onHit="key_board_update"
				style="red_button_style" />
			
			<actor
				name="y_button"
				x="447"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="Y"
				text="Y" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="u_button"
				x="517"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="U"
				text="U" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="i_button"
				x="587"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="I"
				text="I" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="o_button"
				x="657"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="O"
				text="O" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="p_button"
				x="727"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="P"
				text="P" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="gmail_button"
				x="797"
				y="297"
				width="137"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_tiny"
				tag="@GMAIL.COM"
				text="@GMAIL.COM"
				visible="false" 
				onHit="key_board_update"
				style="red_button_style" />
			
			<actor
				name="a_button"
				x="127"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="A"
				text="A" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="s_button"
				x="197"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="S"
				text="S" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="d_button"
				x="267"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="D"
				text="D" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="f_button"
				x="337"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="F"
				text="F" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="g_button"
				x="407"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="G"
				text="G" 
				onHit="key_board_update"
				style="red_button_style" />
			
			<actor
				name="h_button"
				x="477"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="H"
				text="H" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="j_button"
				x="547"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="J"
				text="J" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="k_button"
				x="617"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="K"
				text="K" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="l_button"
				x="687"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="L"
				text="L" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="hotmail_button"
				x="757"
				y="367"
				width="150"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_tiny"
				tag="@HOTMAIL.COM"
				text="@HOTMAIL.COM" 
				visible="false" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="z_button"
				x="157"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="Z"
				text="Z" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="x_button"
				x="227"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="X"
				text="X" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="c_button"
				x="297"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="C"
				text="C" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="v_button"
				x="367"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="V"
				text="V" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="b_button"
				x="437"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="B"
				text="B" 
				onHit="key_board_update"
				style="red_button_style" />
			
			<actor
				name="n_button"
				x="507"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="N"
				text="N" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="m_button"
				x="577"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="M"
				text="M" 
				onHit="key_board_update"
				style="red_button_style" />				
				
			<actor
				name="hypen_button"
				x="647"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="-"
				text="-" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="aol_button"
				x="717"
				y="437"
				width="156"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_tiny"
				tag="@AOL.COM"
				text="@AOL.COM"
                visible="false"  
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="space_bar_button"
				x="187"
				y="507"
				width="446"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag=" "
				text="ESPACIO" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="under_score_button"
				x="518"
				y="507"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="_"
				text="_" 
				visible="false" 
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="at_button"
				x="587"
				y="507"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="@"
				text="@"
                visible="false"  
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="dot_button"
				x="655"
				y="507"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_small_bold"
				tag="."
				text="."
                visible="false"  
				onHit="key_board_update"
				style="red_button_style" />

			<actor
				name="dotcom_button"
				x="722"
				y="507"
				width="123"
				height="56"
				cornerSize="28,28"
				font="font_akzidenz_bold_tiny"
				tag=".COM"
				text=".COM"
                visible="false"  
				onHit="key_board_update"
				style="red_button_style" />
				
			<actor
				name="lowerpart_gray_ground_border"
				x="0"
				y="621"
				width="1024"
				height="148"
				backgroundColor="60,60,60"
				optionFlags="Background" />
				
			<actor
				name="lowerpart_gray_background"
				x="0"
				y="624"
				width="1024"
				height="148"
				backgroundColor="41,41,41"
				optionFlags="Background" />
				
			<actor
				name="marketing_label"
				x="30"
				y="660"
				width="460"
				height="100"
				foregroundColor="128,128,128"
				font="font_akzidenz_regular_small"
				optionFlags="Text"				
				text="${KeyBoard:MarketingLabel}" />
			
			<actor
				name="cancel_button"
				x="583"
				y="657"
				width="196"
				height="66"
				cornerSize="33,25"
				strokeWeight="3"
				font="font_target_small"	
				tag="cancel"			
				text="NO GRACIAS" 
				onHit="key_board_update"
				style="gray_button_style" />	
			
			<actor
				name="ok_button"
				x="803"
				y="657"
				width="196"
				height="66"
				cornerSize="33,25"
				font="font_target_small"
				text="   CONTINUAR"
				tag="enter"
				onHit="key_board_update"
				style="red_next_button_style" />
				
			<actor
                name="image_next_right_arrow"
                x="947"
                y="678"
				image="image_next_white_arrow_big"
                hitFlags="enabled"
                optionFlags="Image"
				tag="enter"
				onHit="key_board_update"/>
				
			<actor
                name="image_next_right_arrow_gray"
                x="947"
                y="678"
				image="image_next_right_arrow_gray"
                hitFlags="enabled"
                optionFlags="Image"
				tag="enter"
				onHit="key_board_update"/>
				
		</scene>
	</aspect>
</resource>
