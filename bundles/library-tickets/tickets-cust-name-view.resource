<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_cust_name_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="stylesheet_tickets"
			backgroundImage="zip_code_background"
			onEnter="tickets_cust_name_view_enter"
			onLeave="tickets_cust_name_view_leave">

			<actor
				name="cust_name_address_label"
				x="20"
				y="20"
				font="font_target_medium"
				optionFlags="Text"
				foregroundColor="black"
				text="${CustName:PanelTag}" />

			<actor
				name="cust_name_top_text_label"
				x="20"
				y="80"
				font="font_akzidenz_16_regular"
				optionFlags="Text"
				foregroundColor="black"
				text="${CustName:MessageLabel}" />

			<actor
				name="bg_top_border"
				x="20"
				y="67"
				width="984"
				height="3"
				optionFlags="Background"
				backgroundColor="black" />

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
				text="${CustName:ErrorLabel}" />

			<actor
				name="image_first_name_text_box"
				x="46"
				y="142"
				width="319"
				height="81"
				image="image_will_call_entry_field_active"
				tag="first"
				onHit="tickets_cust_name_view_update"
				hitFlags="enabled"
				optionFlags="Image" />

			<actor
				name="image_last_name_text_box"
				x="368"
				y="147"
				width="309"
				height="71"
				image="image_will_call_entry_field"
				tag="last"
				onHit="tickets_cust_name_view_update"
				hitFlags="enabled"
				optionFlags="Image" />

			<actor
				name="text_first_name_label"
				x="50"
				y="147"
				width="287"
				height="67"
				foregroundColor="black"
				optionFlags="Text"
				verticalAlignment="center"
				horizontalAlignment="center"
				font="font_target_21_bold"
				text="${CustName:First}" />

			<actor
				name="text_last_name_label"
				x="368"
				y="147"
				width="287"
				height="67"
				foregroundColor="black"
				optionFlags="Text"
				verticalAlignment="center"
				horizontalAlignment="center"
				font="font_target_21_bold"
				text="${CustName:Last}" />

			<actor
				name="back_button"
				x="700"
				y="147"
				width="130"
				height="68"
				cornerSize="35,31"
				font="font_akzidenz_bold_smaller"
				text="    BACK"
				tag="back"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="back_button_arrow"
				x="720"
				y="172"
				hitFlags="Enabled"
				tag="back"
				onHit="tickets_cust_name_view_update"
				image="image_back_icon"
				optionFlags="image" />

			<actor
				name="clear_button"
				x="845"
				y="147"
				width="130"
				height="68"
				cornerSize="35,31"
				font="font_akzidenz_bold_smaller"
				text="    CLEAR"
				tag="clear"
				visible="true"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="clear_button_icon"
				x="868"
				y="169"
				hitFlags="Enabled"
				tag="clear"
				onHit="tickets_cust_name_view_update"
				image="image_clear_icon"
				visible="true"
				optionFlags="image" />

			<actor
				name="q_button"
				x="147"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="Q"
				text="Q"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="w_button"
				x="217"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="W"
				text="W"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="e_button"
				x="287"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="E"
				text="E"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="r_button"
				x="357"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="R"
				text="R"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="t_button"
				x="427"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="T"
				text="T"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="y_button"
				x="497"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="Y"
				text="Y"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="u_button"
				x="567"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="U"
				text="U"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="i_button"
				x="637"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="I"
				text="I"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="o_button"
				x="707"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="O"
				text="O"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="p_button"
				x="777"
				y="247"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="P"
				text="P"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="a_button"
				x="177"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="A"
				text="A"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="s_button"
				x="247"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="S"
				text="S"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="d_button"
				x="317"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="D"
				text="D"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="f_button"
				x="387"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="F"
				text="F"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="g_button"
				x="457"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="G"
				text="G"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="h_button"
				x="527"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="H"
				text="H"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="j_button"
				x="597"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="J"
				text="J"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="k_button"
				x="667"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="K"
				text="K"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="l_button"
				x="737"
				y="317"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="L"
				text="L"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="z_button"
				x="207"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="Z"
				text="Z"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="x_button"
				x="277"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="X"
				text="X"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="c_button"
				x="347"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="C"
				text="C"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="v_button"
				x="417"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="V"
				text="V"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="b_button"
				x="487"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="B"
				text="B"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="n_button"
				x="557"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="N"
				text="N"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="m_button"
				x="627"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="M"
				text="M"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="hypen_button"
				x="697"
				y="387"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="-"
				text="-"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="space_bar_button"
				x="237"
				y="457"
				width="408"
				height="56"
				cornerSize="28,28"
				font="font_akzidenz_bold_smaller"
				tag=" "
				text="SPACE"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="under_score_button"
				x="568"
				y="457"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="_"
				text="_"
				visible="false"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="dot_button"
				x="667"
				y="457"
				width="56"
				height="56"
				cornerSize="28,28"
				font="font_target_21_bold"
				tag="."
				text="."
				visible="true"
				onHit="tickets_cust_name_view_update"
				style="red_button_style_bold_border" />

			<actor
				name="ok_button"
				x="798"
				y="662"
				width="196"
				height="66"
				cornerSize="33,25"
				font="font_target_21_regular"
				text="NEXT"
				textRegion="55, 0, 70, 66"
				tag="enter"
				onHit="tickets_cust_name_view_update"
				style="red_button_3pt_red_stroke_disableable" />

			<actor
				name="image_next_right_arrow_btn"
				x="935"
				y="683"
				image="image_next_right_arrow"
				hitFlags="enabled"
				optionFlags="Image, Text"
				tag="enter"
				onHit="tickets_cust_name_view_update"/>

		</scene>
	</aspect>
</resource>
