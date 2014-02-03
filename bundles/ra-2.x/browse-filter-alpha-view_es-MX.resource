<?xml version="1.0" encoding="utf-8"?>
<resource type="view">
	<property name="name" value="browse_filter_alpha_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="24, 24, 24"
			stylesheet="style_sheet_common"
			onEnter="browse_filter_alpha_view_enter"
			onLeave="browse_filter_alpha_view_leave">

			<!-- Page Background Graphics -->

			<actor
				name="alpha_back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="alpha_movies_background"
				optionFlags="Image"
				stylesheet="style_sheet_common"
				visible="false" />

			<actor
				name="bg_box_top"
				x="0"
				y="61"
				width="1024"
				height="517"
				gradientAngle="-90"
				backgroundColor="0,0,0"
				gradientTargetColor="26,26,26"
				optionFlags="BackgroundGradient" />

			<actor
				name="center_gray_background"
				x="0"
				y="567"
				width="1024"
				height="59"
				gradientAngle="90"
				backgroundColor="25,25,25"
				gradientTargetColor="8,8,8"
				optionFlags="BackgroundGradient"
				visible="true" />

			<actor
				name="lowerpart_gray_background"
				x="0"
				y="622"
				optionFlags="Background"
				backgroundColor="60,56,57"
				width="1024"
				height="145"
				visible="true" />

				<!-- TABS ACROSS THE TOP -->

			<actor
				name="tab_switch_product_family_border"
				x="882"
				y="0"
				width="144"
				height="76"
				style="product_family_switch_tab_border_style" />

			<actor
				name="tab_switch_product_family"
				x="884"
				y="0"
				width="142"
				height="74"
				text=""
				textRegion="10,12,130,45"
				verticalAlignment="center"
				horizontalAlignment="near"
				font="font_akzidenz_bold_smaller"
				style="product_family_switch_tab_style" />

			<actor
				name="tab_no_switch_product_family"
				x="882"
				y="0"
				width="144"
				height="69"
				style="unselected_tab_style"
				visible="false" />

			<actor
				name="image_next_white_arrow"
				x="999"
				y="27"
				image="image_next_white_arrow"
				hitFlags="enabled"
				optionFlags="Image" />

			<actor
				name="tab_1"
				x="0"
				y="0"
				text="TODAS PEL&#205;CULAS"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="selected_tab_style" />

			<actor
				name="tab_2"
				x="126"
				y="0"
				text="LOS &#201;XITOS 20"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_2_text_a"
				x="139"
				y="27"
				width="100"
				height="15"
				text="TOP"
				optionFlags="Text"
				font="font_akzidenz_bold_tiny"
				foregroundColor="white"/>

			<actor
				name="tab_2_text_b"
				x="225"
				y="25"
				width="40"
				height="15"
				text="20"
				optionFlags="Text"
				font="font_akzidenz_bold_11" 
				foregroundColor="white" />

			<actor
				name="tab_3"
				x="252"
				y="0"
				text="ACCI&#211;ON"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_4"
				x="378"
				y="0"
				text="DRAMA"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_5"
				x="504"
				y="0"
				text="TERROR"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_6"
				x="630"
				y="0"
				text="COMEDIA"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

			<actor
				name="tab_7"
				x="756"
				y="0"
				text="FAMILIA"
				width="126"
				height="69"
				font="font_akzidenz_bold_tiny"
				style="unselected_tab_style" />

				<!-- Alphabet Letters Area -->

			<actor
				name="alphabets_bg_shadow"
				x="32"
				y="88"
				width="960"
				height="37"
				cornerSize="10,10"
				visible="true"
				backgroundColor="black"
				opacity="0.25"
				optionFlags="Background" />

			<actor
				name="alphabets_bg"
				x="30"
				y="85"
				width="960"
				height="37"
				cornerSize="10,10"
				strokeWeight="2"
				visible="true"
				borderColor="white"
				backgroundColor="128,128,128"
				style="gray_button_style" />

			<actor
				name="#_label"
				x="40"
				y="90"
				text="#"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_1"
				x="70"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_1"
				x="71"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="A_label"
				x="75"
				y="90"
				text="A"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_2"
				x="105"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_2"
				x="106"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="B_label"
				x="110"
				y="90"
				text="B"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_3"
				x="140"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_3"
				x="141"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="C_label"
				x="145"
				y="90"
				text="C"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_4"
				x="175"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_4"
				x="176"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="D_label"
				x="180"
				y="90"
				text="D"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_5"
				x="210"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_5"
				x="211"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="E_label"
				x="215"
				y="90"
				text="E"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_6"
				x="245"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_6"
				x="246"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="F_label"
				x="250"
				y="90"
				text="F"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_7"
				x="280"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_7"
				x="281"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="G_label"
				x="285"
				y="90"
				text="G"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_8"
				x="315"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_8"
				x="316"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="H_label"
				x="320"
				y="90"
				text="H"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_9"
				x="350"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_9"
				x="351"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="I_label"
				x="355"
				y="90"
				text="I"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_10"
				x="385"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_10"
				x="386"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="J_label"
				x="390"
				y="90"
				text="J"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_11"
				x="420"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_11"
				x="421"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="K_label"
				x="425"
				y="90"
				text="K"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_12"
				x="455"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_12"
				x="456"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="L_label"
				x="460"
				y="90"
				text="L"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_13"
				x="490"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_13"
				x="491"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="M_label"
				x="495"
				y="90"
				text="M"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_14"
				x="525"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_14"
				x="526"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="N_label"
				x="530"
				y="90"
				text="N"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_15"
				x="560"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_15"
				x="561"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="O_label"
				x="565"
				y="90"
				text="O"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_16"
				x="595"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_16"
				x="596"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="P_label"
				x="600"
				y="90"
				text="P"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_17"
				x="630"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_17"
				x="631"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="Q_label"
				x="635"
				y="90"
				text="Q"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_18"
				x="665"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_18"
				x="666"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="R_label"
				x="670"
				y="90"
				text="R"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_19"
				x="700"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_19"
				x="701"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="S_label"
				x="705"
				y="90"
				text="S"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_20"
				x="735"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_20"
				x="736"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="T_label"
				x="740"
				y="90"
				text="T"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_21"
				x="770"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_21"
				x="771"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="U_label"
				x="775"
				y="90"
				text="U"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_22"
				x="805"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_22"
				x="806"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="V_label"
				x="810"
				y="90"
				text="V"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_23"
				x="840"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_23"
				x="841"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="W_label"
				x="845"
				y="90"
				text="W"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_24"
				x="875"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_24"
				x="876"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="X_label"
				x="880"
				y="90"
				text="X"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_25"
				x="910"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_25"
				x="911"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="Y_label"
				x="915"
				y="90"
				text="Y"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

			<actor
				name="aplha_separatorb_26"
				x="945"
				y="87"
				style="browse_alpha_separator_vertical_black_style" />

			<actor
				name="aplha_separatorg_26"
				x="946"
				y="87"
				style="browse_alpha_separator_vertical_grey_style" />

			<actor
				name="Z_label"
				x="950"
				y="90"
				text="Z"
				font="font_akzidenz_bold_smaller_14"
				style="alphabet_style" />

				<!-- Browse Page Navigation and shadow effects -->

			<actor
				name="left_arrow_button"
				x="0"
				y="190"
				width="58"
				height="306"
				style="left_arrow_style" />

			<actor
				name="right_arrow_button"
				x="966"
				y="190"
				width="58"
				height="306"
				style="right_arrow_style" />

				<!-- Filtering and Sorting Buttons - MOVIES -->

			<actor
				name="blu_ray_button"
				x="287"
				y="578"
				height="30"
				width="94"
				cornerSize="16,16"
				strokeWeight="2"
				text="Blu-ray"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="selected_button"
				x="287"
				y="578"
				height="30"
				width="94"
				visible="false"
				font="font_akzidenz_regular_smaller"
				hitFlags="Enabled"
				image="image_selected_button"
				text="${SortName}"
				horizontalAlignment="center"
				verticalAlignment="center"
				optionFlags="Image, Text" />

			<actor
				name="newest_button"
				x="397"
				y="578"
				height="30"
				width="94"
				cornerSize="16,16"
				strokeWeight="2"
				text="&#218;ltimos"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="abc_button"
				x="507"
				y="578"
				width="94"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="A-Z"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="for_sale_button"
				x="617"
				y="578"
				width="94"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="Se Vende"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="coming_soon_button"
				x="617"
				y="578"
				height="30"
				width="110"
				cornerSize="16,16"
				strokeWeight="2"
				text="Pr&#243;ximamente"
				visible="true"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

				<!-- Filtering and Sorting Buttons - GAMES -->

			<actor
				name="xbox_360_button"
				x="152"
				y="578"
				height="30"
				width="94"
				cornerSize="16,16"
				strokeWeight="2"
				text="Xbox 360"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="ps3_button"
				x="262"
				y="578"
				width="74"
				height="30"
				cornerSize="16,16"
				strokeWeight="2"
				text="PS3"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="wii_button"
				x="352"
				y="578"
				height="30"
				width="74"
				cornerSize="16,16"
				strokeWeight="2"
				text="Wii"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

			<actor
				name="wii_u_button"
				x="442"
				y="578"
				height="30"
				width="74"
				cornerSize="16,16"
				strokeWeight="2"
				text="Wii U"
				visible="false"
				font="font_akzidenz_regular_smaller"
				style="gray_button_style" />

				<!-- Mini Cart -->

			<actor
				name="selectedmovie_1_border"
				x="233"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_1"
				x="235"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_1_flag"
				x="235"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_1"
				x="290"
				y="625"
				height="43"
				width="43"
				text="X"
				font="font_akzidenz_bold_small"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_2_border"
				x="348"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_2"
				x="350"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_2_flag"
				x="350"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_2"
				x="405"
				y="625"
				height="43"
				width="43"
				text="X"
				font="font_akzidenz_bold_small"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_3_border"
				x="458"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_3"
				x="460"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_3_flag"
				x="460"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_3"
				x="515"
				y="625"
				height="43"
				width="43"
				text="X"
				font="font_akzidenz_bold_small"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_4_border"
				x="572"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_4"
				x="574"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_4_flag"
				x="574"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_4"
				x="629"
				y="625"
				height="43"
				width="43"
				text="X"
				font="font_akzidenz_bold_small"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

			<actor
				name="selectedmovie_5_border"
				x="683"
				y="636"
				visible="false"
				style="selectedmovie_border_style" />

			<actor
				name="selectedmovie_5"
				x="685"
				y="638"
				style="selectedmovie_style" />

			<actor
				name="selectedmovie_5_flag"
				x="685"
				y="742"
				width="78"
				height="12"
				visible="false"
				verticalAlignment="Center"
				horizontalAlignment="Center"
				text="COMPRA"
				backgroundColor="162,16,24"
				foregroundColor="White"
				font="font_akzidenz_bold_tiny_8"
				optionFlags = "Background, Text" />

			<actor
				name="cancel_5"
				x="740"
				y="625"
				height="43"
				width="43"
				text="X"
				font="font_akzidenz_bold_small"
				cornerSize="21,21"
				visible="false"
				style="round_gray_with_red_border_button_style" />

				<!-- Action Zone -->

			<actor
				name="redbox_logo"
				x="50"
				y="667"
				width="133"
				height="40"
				style="redbox_logo" />

			<actor
				name="startover_button"
				x="8"
				y="573"
				width="121"
				height="46"
				onHit="start_over"
				addToFront="true"
				text="P&#225;gina Principal"
				font="font_target_smaller"
				cornerSize="22,22"
				strokeWeight="2"
				textRegion="25,1,85,38"
				style="gray_button_style" />

			<actor
				name="startover_arrow"
				x="20"
				y="586"
				hitFlags="None"
				image="image_start_over_arrow"
				optionFlags="Image" />

			<actor
				name="add_to_cart_label"
				x="253"
				y="674"
				width="499"
				height="41"
				hitFlags="None"
				foregroundColor="gray"
				strokeWeight="1"
				opacity=".8"
				text="Presione     para agregarlo al carrito"
				verticalAlignment="center"
				horizontalAlignment="center"
				backgroundColor="50, 50, 50"
				borderColor="80,80,80"
				font="font_akzidenz_regular_medium"
				optionFlags="Background, DrawBorder, Image, Text, TextShadow" />

			<actor
				name="add_to_cart_label_image"
				x="385"
				y="675"
				width="41"
				height="40"
				image="image_add_to_cart"
				hitFlags="None"
				opacity=".9"
				optionFlags="Image" />

			<actor
				name="my_rentals_button"
				x="784"
				y="660"
				width="223"
				height="66"
				cornerSize="30,30"
				font="font_target_small_bold"
				text="CAJA (0)"
				onHit="view_shopping_cart"
				style="red_button_style" />

			<actor
				name="selected_label"
				width="34"
				height="39"
				text=""
				font="font_akzidenz_bold_smaller_14"
				visible="true"
				gradientAngle="90"
				gradientTargetColor = "233,35,45"
				borderColor="black"
				backgroundColor= "121,0,0"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="center"
				optionFlags="BackgroundGradient, Text, DrawBorder" />
				
<!-- Filter Results Empty -->
			<actor
				name="image_filter_empty_background"
				x="0"
				y="70"
				width="1024"
				height="494"
				image="image_filter_empty_background"
				hitFlags="None"
				visible="false"
				optionFlags="Image"/>

			<actor
				name="filter_empty_text"
				x="54"
				y="298"
				width="916"
				height="140"
				text=""
				font="font_bryant_bold_30"
				foregroundColor="50,47,47"
				horizontalAlignment="center"
				hitFlags="None"
				visible="false"
				optionFlags="Text" />
				
		</scene>
	</aspect>
</resource>
