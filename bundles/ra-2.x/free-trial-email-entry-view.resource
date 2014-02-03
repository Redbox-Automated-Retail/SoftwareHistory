<?xml version="1.0"?>
<resource type="view">  
    <property name="name" value="free_trial_email_entry_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />	
	<aspect name="content">		
		<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			backgroundImage="image_keyboard_background"
			onEnter="free_trial_email_entry_view_enter">

			<actor
				name="top_banner"
				x="0"
				y="0"
				optionFlags="Image"
				image="image_free_trial_banner" />

			<actor
				name="header_label"
				x="20"
				y="95"
				font="font_akzidenz_regular_16"
				optionFlags="Text"
				text="Enter your email address:" />			

			<actor
				name="error_label"
				x="80"
				y="120"
				width="808"
				height="24"
				optionFlags="Text"
				font="font_akzidenz_bold_small"
				foregroundColor="red"
				verticalAlignment="center"
				horizontalAlignment="center"
				text="" />		
				
			<actor
				name="text_label"
				x="67"
				y="147"
				width="706"
				height="67"
				font="font_target_small_bold"
				strokeWeight="3"				
				text="${KeyBoard:Text}"
				foregroundColor="0,0,0"
				verticalAlignment="center"
				horizontalAlignment="center"
				image="image_email_entry_field"
				optionFlags="Image, Text" />

			<actor
				name="back_button"
				x="787"
				y="147"
				width="136"
				height="68"
				cornerSize="31,31"
				font="font_akzidenz_bold_smaller"
				text="    BACK" 
				tag="back" 
				onHit="free_trial_email_entry_update"
				style="red_button_style" /> 
			
			<actor
				name="back_button_arrow"
				x="808"
				y="171"
				hitFlags="Enabled"
				tag="back" 
				onHit="free_trial_email_entry_update"
				image="image_back_icon"
				optionFlags="image" />
				
			<actor
				name="clear_button"
				x="845"
				y="147"
				width="136"
				height="68"
				cornerSize="31,31"
				font="font_akzidenz_bold_smaller"
				text="    CLEAR" 
				tag="clear"
				visible="false"
				onHit="free_trial_email_entry_update"
				style="red_button_style" />  
				
			<actor
				name="clear_button_icon"
				x="868"
				y="169"
				hitFlags="Enabled"
				tag="clear" 
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
				style="red_button_style"			     
				/>
				
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
				style="red_button_style" />
				
			<actor
				name="gmail_button"
				x="797"
				y="297"
				width="134"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_tiny"
				tag="@GMAIL.COM"
				text="@GMAIL.COM"
				visible="false" 
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
				style="red_button_style" />			
				
			<actor
				name="space_bar_button"
				x="187"
				y="507"
				width="446"
				height="56"
				cornerSize="28,28"
				font="font_akzidenz_bold_smaller"
				tag=" "
				text="SPACE" 
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				onHit="free_trial_email_entry_update"
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
				name="cancel_button"
				x="576"
				y="660"
				width="196"
				height="66"
				cornerSize="32,32"
				font="font_target_small"
				tag="cancel"
				onHit="free_trial_email_entry_update"
				text="CANCEL SIGNUP" 
				style="gray_button_style" />
			
			<actor
				name="ok_button"
				x="794"
				y="660"
				width="196"
				height="66"
				cornerSize="32,32"
				font="font_target_small"
				text="          NEXT"
				tag="enter"
				onHit="free_trial_email_entry_update"
				style="red_next_button_style" />
				
			<actor
                name="image_next_right_arrow"
                x="918"
                y="681"
				image="image_next_white_arrow_big"
                hitFlags="enabled"
                optionFlags="Image"
				tag="enter"
				onHit="free_trial_email_entry_update"/>
				
			<actor
                name="image_next_right_arrow_gray"
                x="918"
                y="681"
				image="image_next_right_arrow_gray"
                optionFlags="Image"
				tag="enter"/>
		</scene>
	</aspect>
</resource>
