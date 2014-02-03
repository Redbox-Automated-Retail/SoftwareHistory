<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_email_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			backgroundImage="zip_code_background"
			onEnter="tickets_email_view_enter"
			onLeave="tickets_email_view_leave">

			<actor
				name="email_address_label"
				x="20"
				y="20"
				font="font_target_medium"
				optionFlags="Text"
				foregroundColor="black"
				text="${Email:PanelTag}" />

			<actor
				name="background_line"
				x="20"
				y="67"
				width="984"
				height="3"
				backgroundColor="black"
				optionFlags="Background" />

			<actor
				name="email_top_text_label"
				x="20"
				y="80"
				font="font_akzidenz_16_regular"
				optionFlags="Text"
				foregroundColor="black"
				text="${Email:MessageLabel}" />

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
				text="${Email:ErrorLabel}" />

			<actor
				name="image_text_box"
				x="65"
				y="147"
				width="620"
				image="image_email_entry_field"
				hitFlags="enabled"
				optionFlags="Image" />

			<actor
				name="text_label"
				x="67"
				y="147"
				width="615"
				height="67"
				foregroundColor="black"
				optionFlags="Text"
				verticalAlignment="center"
				horizontalAlignment="center"
				font="font_target_21_bold"
				text="${Email:Text}" />

			<actor
				name="back_button"
				x="695"
				y="147"
				width="136"
				height="68"
				cornerSize="31,31"
				font="font_akzidenz_bold_smaller"
				text="    BACK"
				tag="back"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="back_button_arrow"
				x="715"
				y="171"
				hitFlags="Enabled"
				tag="back"
				onHit="tickets_email_view_update"
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
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="clear_button_icon"
				x="868"
				y="169"
				hitFlags="Enabled"
				tag="clear"
				onHit="tickets_email_view_update"
				image="image_clear_icon"
				optionFlags="image" />

			<actor
				name="one_button"
				x="67"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="1"
				text="1"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="two_button"
				x="137"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="2"
				text="2"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="three_button"
				x="207"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="3"
				text="3"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="four_button"
				x="277"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="4"
				text="4"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="five_button"
				x="347"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="5"
				text="5"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="six_button"
				x="417"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="6"
				text="6"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="seven_button"
				x="487"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="7"
				text="7"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="eight_button"
				x="557"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="8"
				text="8"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="nine_button"
				x="627"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="9"
				text="9"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="zero_button"
				x="697"
				y="227"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="0"
				text="0"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="yahoo_button"
				x="767"
				y="227"
				width="187"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_smaller"
				tag="@YAHOO.COM"
				text="@YAHOO.COM"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="q_button"
				x="97"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="Q"
				text="Q"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="w_button"
				x="167"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="W"
				text="W"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="e_button"
				x="237"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="E"
				text="E"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="r_button"
				x="307"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="R"
				text="R"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="t_button"
				x="377"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="T"
				text="T"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="y_button"
				x="447"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="Y"
				text="Y"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="u_button"
				x="517"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="U"
				text="U"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="i_button"
				x="587"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="I"
				text="I"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="o_button"
				x="657"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="O"
				text="O"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="p_button"
				x="727"
				y="297"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="P"
				text="P"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="gmail_button"
				x="797"
				y="297"
				width="134"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_smaller"
				tag="@GMAIL.COM"
				text="@GMAIL.COM"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="a_button"
				x="127"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="A"
				text="A"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="s_button"
				x="197"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="S"
				text="S"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="d_button"
				x="267"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="D"
				text="D"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="f_button"
				x="337"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="F"
				text="F"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="g_button"
				x="407"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="G"
				text="G"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="h_button"
				x="477"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="H"
				text="H"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="j_button"
				x="547"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="J"
				text="J"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="k_button"
				x="617"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="K"
				text="K"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="l_button"
				x="687"
				y="367"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="L"
				text="L"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="hotmail_button"
				x="757"
				y="367"
				width="150"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_smaller"
				tag="@HOTMAIL.COM"
				text="@HOTMAIL.COM"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="z_button"
				x="157"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="Z"
				text="Z"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="x_button"
				x="227"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="X"
				text="X"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="c_button"
				x="297"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="C"
				text="C"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="v_button"
				x="367"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="V"
				text="V"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="b_button"
				x="437"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="B"
				text="B"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="n_button"
				x="507"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="N"
				text="N"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="m_button"
				x="577"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="M"
				text="M"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="hypen_button"
				x="647"
				y="437"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="-"
				text="-"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="aol_button"
				x="717"
				y="437"
				width="156"
				height="56"
				cornerSize="27,27"
				font="font_akzidenz_bold_smaller"
				tag="@AOL.COM"
				text="@AOL.COM"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="space_bar_button"
				x="187"
				y="507"
				width="315"
				height="56"
				cornerSize="28,28"
				font="font_akzidenz_bold_smaller"
				tag=" "
				text="SPACE"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="under_score_button"
				x="518"
				y="507"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="_"
				text="_"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="at_button"
				x="587"
				y="507"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="@"
				text="@"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="dot_button"
				x="655"
				y="507"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="."
				text="."
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="dotcom_button"
				x="722"
				y="507"
				width="123"
				height="56"
				cornerSize="28,28"
				font="font_akzidenz_bold_smaller"
				tag=".COM"
				text=".COM"
				onHit="tickets_email_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="cancel_button"
				x="583"
				y="662"
				width="196"
				height="66"
				cornerSize="33,25"
				font="font_target_small"
				text="NO THANKS"
				tag="cancel"
				visible="false"
				onHit="tickets_email_view_update"
				style="light_gray_button_style" />

			<actor
				name="ok_button"
				x="798"
				y="662"
				width="196"
				height="66"
				cornerSize="33,25"
				font="font_target_21_regular"
				textRegion="40, 0, 196, 66"
				text="CONTINUE"
				tag="enter"
				onHit="tickets_email_view_update"
				enabled="True"
				style="red_button_3pt_red_stroke_disableable" />

			<actor
				name="ok_button_arrow"
				x="920"
				y="683"
				image="image_next_right_arrow_gray"
				visible="false"
				optionFlags="Image" />

			<actor
				name="image_request_sale_updates"
				x="48"
				y="670"
				image="image_gray_unselected_checkbox"
				hitFlags="enabled"
				optionFlags="Image, Text"
				tag="request_sale"
				onHit="tickets_email_view_update" />

			<actor
				name="request_sale_updates_label"
				x="118"
				y="690"
				width="808"
				height="24"
				optionFlags="Text"
				font="font_akzidenz_15_regular"
				foregroundColor="white"
				verticalAlignment="near"
				horizontalAlignment="near"
				text="Please send me updates about Redbox offers and promotions." />

		</scene>
	</aspect>
</resource>