<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="reco_on_pickup_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
		    name="Default"
			width="1024"
			height="768"
			backgroundColor="159, 159, 159" 
			stylesheet="style_sheet_common"
			onEnter="reco_on_pickup_view_enter"
			onLeave="reco_on_pickup_view_leave">
				
			<actor 
				name="cart_background"
				x="0"
				y="0"
				width="1024"
				height="414"
				image="sunburst_background"
				optionFlags="Image" />

			<actor
				name="label_cart_description"
				x="64"
				y="48"
				foregroundColor="57,65,69"
				font="font_vag_black_30"
				text=""
				optionFlags="Text"/>
				
			<actor
				name="cart_image_1"
				x="71"
				y="128"
				width="150"
				height="214"
				image="add_more_box"
				borderColor="128,128,128"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="Image, DrawBorder"  />

			<actor
				name="label_add_more_1"
				x="81"
				y="192"
				width="130"
				font="font_bryant_bold_20"
				text=""
				horizontalAlignment="center"
				foregroundColor="71,80,85"
				optionFlags="Text"/>

			<actor
				name="cart_add_more_arrow_1"
				x="127"
				y="263"
				image="image_add_more_arrow"
				optionFlags="Image"  />

			<actor
				name="cart_remove_button_1"
				x="195"
				y="113"
				hitFlags="Enabled"
				image="image_x_out_button"
				optionFlags="Image"  />

			<actor
				name="cart_bluray_banner_1"
				x="71"
				y="307"
				width="150"
				height="35"
				font="font_bryant_bold_16"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="label_reserved_title_1"
				x="71"
				y="356"
				width="150"
				foregroundColor="black"
				fontStyle="Italic"
				font="font_akzidenz_regular_smaller_italic"
				text=""
				horizontalAlignment="center"
				optionFlags="Text"/>

			<actor
				name="cart_image_2"
				x="254"
				y="128"
				width="150"
				height="214"
				image="add_more_box"
				borderColor="128,128,128"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="Image, DrawBorder"  />

			<actor
				name="label_add_more_2"
				x="264"
				y="192"
				width="130"
				font="font_bryant_bold_20"
				text=""
				horizontalAlignment="center"
				foregroundColor="71,80,85"
				optionFlags="Text"/>

			<actor
				name="cart_add_more_arrow_2"
				x="310"
				y="263"
				image="image_add_more_arrow"
				optionFlags="Image"  />

			<actor
				name="cart_remove_button_2"
				x="378"
				y="113"
				hitFlags="Enabled"
				image="image_x_out_button"
				optionFlags="Image"  />

			<actor
				name="cart_bluray_banner_2"
				x="254"
				y="307"
				width="150"
				height="35"
				font="font_bryant_bold_16"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="label_reserved_title_2"
				x="254"
				y="356"
				width="150"
				foregroundColor="black"
				font_style="Italic"
				font="font_akzidenz_regular_smaller_italic"
				text=""
				horizontalAlignment="center"
				optionFlags="Text"/>

			<actor
				name="cart_image_3"
				x="437"
				y="128"
				width="150"
				height="214"
				image="add_more_box"
				borderColor="128,128,128"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="Image, DrawBorder"  />

			<actor
				name="label_add_more_3"
				x="447"
				y="192"
				width="130"
				font="font_bryant_bold_20"
				text=""
				horizontalAlignment="center"
				foregroundColor="71,80,85"
				optionFlags="Text"/>

			<actor
				name="cart_add_more_arrow_3"
				x="493"
				y="263"
				image="image_add_more_arrow"
				optionFlags="Image"  />

			<actor
				name="cart_remove_button_3"
				x="561"
				y="113"
				hitFlags="Enabled"
				image="image_x_out_button"			
				optionFlags="Image"  />

			<actor
				name="cart_bluray_banner_3"
				x="437"
				y="307"
				width="150"
				height="35"
				font="font_bryant_bold_16"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="label_reserved_title_3"
				x="437"
				y="356"
				width="150"
				foregroundColor="black"
				font_style="Italic"
				font="font_akzidenz_regular_smaller_italic"
				text=""
				horizontalAlignment="center"
				optionFlags="Text"/>

			<actor
				name="cart_image_4"
				x="620"
				y="128"
				width="150"
				height="214"
				image="add_more_box"
				borderColor="128,128,128"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="Image, DrawBorder"  />

			<actor
				name="label_add_more_4"
				x="630"
				y="192"
				width="130"
				font="font_bryant_bold_20"
				text=""
				horizontalAlignment="center"
				foregroundColor="71,80,85"
				optionFlags="Text"/>

			<actor
				name="cart_add_more_arrow_4"
				x="676"
				y="263"
				image="image_add_more_arrow"
				optionFlags="Image"  />

			<actor
				name="cart_remove_button_4"
				x="744"
				y="113"
				hitFlags="Enabled"
				image="image_x_out_button"
				optionFlags="Image"  />

			<actor
				name="cart_bluray_banner_4"
				x="620"
				y="307"
				width="150"
				height="35"
				font="font_bryant_bold_16"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="label_reserved_title_4"
				x="620"
				y="356"
				width="150"
				foregroundColor="black"
				font_style="Italic"
				font="font_akzidenz_regular_smaller_italic"
				text=""
				horizontalAlignment="center"
				optionFlags="Text"/>

			<actor
				name="cart_image_5"
				x="803"
				y="128"
				width="150"
				height="214"
				image="add_more_box"
				borderColor="128,128,128"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="Image, DrawBorder"  />

			<actor
				name="label_add_more_5"
				x="813"
				y="192"
				width="130"
				font="font_bryant_bold_20"
				text=""
				horizontalAlignment="center"
				foregroundColor="71,80,85"
				optionFlags="Text"/>

			<actor
				name="cart_add_more_arrow_5"
				x="859"
				y="263"
				image="image_add_more_arrow"
				optionFlags="Image"  />

			<actor
				name="cart_remove_button_5"
				x="927"
				y="113"
				hitFlags="Enabled"
				image="image_x_out_button"
				optionFlags="Image"  />

			<actor
				name="cart_bluray_banner_5"
				x="803"
				y="307"
				width="150"
				height="35"
				font="font_bryant_bold_16"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="label_reserved_title_5"
				x="803"
				y="356"
				width="150"
				foregroundColor="black"
				font_style="Italic"
				font="font_akzidenz_regular_smaller_italic"
				text=""
				horizontalAlignment="center"
				optionFlags="Text"/>

			<actor
				name="recos_background"
				x="0"
				y="414"
				width="1024"
				height="209"
				image="background_grey_bar"
				optionFlags="Image"  />

			<actor
				name="label_reco_description"
				x="61"
				y="432"
				foregroundColor="71,80,85"
				font="font_bryant_bold_20"
				text=""
				optionFlags="Text"/>

			<actor
				name="reco_image_1"
				x="82"
				y="478"
				width="85"
				height="122"
				borderColor="white"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="DrawBorder, Image" />

			<actor
				name="reco_dark_overlay_1"
				x="82"
				y="478"
				width="85"
				height="122"
				backgroundColor="black"
				opacity="0.6"
				optionFlags="Background" />

			<actor
				name="reco_in_cart_1"
				x="82"
				y="520"
				width="85"
				font="font_akzidenz_bold_tiny"
				horizontalAlignment="center"
				foregroundColor="white"
				optionFlags="Text" />

			<actor
				name="reco_cart_image_1"
				x="108"
				y="538"
				width="33"
				height="30"
				image="image_cart_icon"
				optionFlags="Image" />
			
			<actor
				name="reco_bluray_banner_1"
				x="83"
				y="574"
				width="65"
				height="25"
				font="font_akzidenz_bold_tiny"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="reco_add_button_1"
				x="138"
				y="565"
				style="reco_add_item_style"/>

			<actor
				name="reco_image_2"
				x="237"
				y="478"
				width="85"
				height="122"
				borderColor="white"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="DrawBorder, Image" />

			<actor
				name="reco_dark_overlay_2"
				x="237"
				y="478"
				width="85"
				height="122"
				backgroundColor="black"
				opacity="0.6"
				optionFlags="Background" />

			<actor
				name="reco_in_cart_2"
				x="237"
				y="520"
				width="85"
				font="font_akzidenz_bold_tiny"
				horizontalAlignment="center"
				foregroundColor="white"
				optionFlags="Text" />

			<actor
				name="reco_cart_image_2"
				x="263"
				y="538"
				width="33"
				height="30"
				image="image_cart_icon"
				optionFlags="Image" />
			
			<actor
				name="reco_bluray_banner_2"
				x="238"
				y="574"
				width="65"
				height="25"
				font="font_akzidenz_bold_tiny"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="reco_add_button_2"
				x="293"
				y="565"
				style="reco_add_item_style"/>

			<actor
				name="reco_image_3"
				x="392"
				y="478"
				width="85"
				height="122"
				borderColor="white"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="DrawBorder, Image" />

			<actor
				name="reco_dark_overlay_3"
				x="392"
				y="478"
				width="85"
				height="122"
				backgroundColor="black"
				opacity="0.6"
				optionFlags="Background" />

			<actor
				name="reco_in_cart_3"
				x="392"
				y="520"
				width="85"
				font="font_akzidenz_bold_tiny"
				horizontalAlignment="center"
				foregroundColor="white"
				optionFlags="Text" />

			<actor
				name="reco_cart_image_3"
				x="418"
				y="538"
				width="33"
				height="30"
				image="image_cart_icon"
				optionFlags="Image" />
			
			<actor
				name="reco_bluray_banner_3"
				x="393"
				y="574"
				width="65"
				height="25"
				font="font_akzidenz_bold_tiny"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="reco_add_button_3"
				x="448"
				y="565"
				style="reco_add_item_style"/>

			<actor
				name="reco_image_4"
				x="547"
				y="478"
				width="85"
				height="122"
				borderColor="white"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="DrawBorder, Image" />

			<actor
				name="reco_dark_overlay_4"
				x="547"
				y="478"
				width="85"
				height="122"
				backgroundColor="black"
				opacity="0.6"
				optionFlags="Background" />

			<actor
				name="reco_in_cart_4"
				x="547"
				y="520"
				width="85"
				font="font_akzidenz_bold_tiny"
				horizontalAlignment="center"
				foregroundColor="white"
				optionFlags="Text" />

			<actor
				name="reco_cart_image_4"
				x="573"
				y="538"
				width="33"
				height="30"
				image="image_cart_icon"
				optionFlags="Image" />
			
			<actor
				name="reco_bluray_banner_4"
				x="548"
				y="574"
				width="65"
				height="25"
				font="font_akzidenz_bold_tiny"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="reco_add_button_4"
				x="603"
				y="565"
				style="reco_add_item_style"/>

			<actor
				name="reco_image_5"
				x="702"
				y="478"
				width="85"
				height="122"
				borderColor="white"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="DrawBorder, Image" />

			<actor
				name="reco_dark_overlay_5"
				x="702"
				y="478"
				width="85"
				height="122"
				backgroundColor="black"
				opacity="0.6"
				optionFlags="Background" />

			<actor
				name="reco_in_cart_5"
				x="702"
				y="520"
				width="85"
				font="font_akzidenz_bold_tiny"
				horizontalAlignment="center"
				foregroundColor="white"
				optionFlags="Text" />

			<actor
				name="reco_cart_image_5"
				x="728"
				y="538"
				width="33"
				height="30"
				image="image_cart_icon"
				optionFlags="Image" />
			
			<actor
				name="reco_bluray_banner_5"
				x="703"
				y="574"
				width="65"
				height="25"
				font="font_akzidenz_bold_tiny"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="reco_add_button_5"
				x="758"
				y="565"
				style="reco_add_item_style"/>

			<actor
				name="reco_image_6"
				x="857"
				y="478"
				width="85"
				height="122"
				borderColor="white"
				strokeWeight="2"
				hitFlags="Enabled"
				optionFlags="DrawBorder, Image" />

			<actor
				name="reco_dark_overlay_6"
				x="857"
				y="478"
				width="85"
				height="122"
				backgroundColor="black"
				opacity="0.6"
				optionFlags="Background" />

			<actor
				name="reco_in_cart_6"
				x="857"
				y="520"
				width="85"
				font="font_akzidenz_bold_tiny"
				horizontalAlignment="center"
				foregroundColor="white"
				optionFlags="Text" />

			<actor
				name="reco_cart_image_6"
				x="883"
				y="538"
				width="33"
				height="30"
				image="image_cart_icon"
				optionFlags="Image" />
			
			<actor
				name="reco_bluray_banner_6"
				x="858"
				y="574"
				width="65"
				height="25"
				font="font_akzidenz_bold_tiny"
				text="Blu-ray"
				horizontalAlignment="center"
				verticalAlignment="center"
				foregroundColor="white"
				image="image_banner_blu_ray"
				optionFlags="Image, Text"  />

			<actor
				name="reco_add_button_6"
				x="913"
				y="565"
				style="reco_add_item_style"/>

			<actor
                name="lowerpart_gray_background"
                x="0"
                y="623"
				width="1024"
				height="150"
                backgroundColor="58,58,58"
                optionFlags="Background" />
				
			<actor
				name="bottom_section_border"
				x="0"
				y="623"
				width="1024"
				height="2"
				backgroundColor="78,78,78"
				optionFlags="Background" />

			<actor 
				name="redbox_logo_border"
				x="0"
				y="625"
				width="202"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"/>

			<actor
				name="redbox_logo"
				x="43"
				y="672"
				width="133"
				height="40"
				style="redbox_logo" />

			<actor 
				name="terms_conditions_border"
				x="202"
				y="625"
				width="245"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"/>
				
			<actor
				name="terms_conditions_button"
				x="227"
				y="661"
				width="194"
				height="66"
				onHit="help"
				text=""
				font="font_akzidenz_bold_smaller"
				cornerSize="34,49"
				strokeWeight="2"
				style="gray_button_style" />
				
				
			<actor 
				name="totals_border"
				x="447"
				y="625"
				width="283"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"/>

            <actor
				name="reserved_discs_label"
				x="470"
				y="634"
				optionFlags="Text"				
				foregroundColor="lightgray"
				font="font_akzidenz_regular_small"		
				text=""/>	
			
			<actor
				name="reserved_discs_amount"
				x="635"
				y="634"
				width="75"
				text=""
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				horizontalAlignment = "far"
				foregroundColor="lightgray" />		
			
            <actor
				name="added_discs_label"
				x="470"
				y="658"
				optionFlags="Text"				
				foregroundColor="lightgray"
				font="font_akzidenz_regular_small"		
				text="Added discs:"/>	
			
			<actor
				name="added_discs_amount"
				x="635"
				y="658"
				width="75"
				text=""
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				horizontalAlignment = "far"
				foregroundColor="lightgray" />		
				
            <actor
				name="credit_label"
				x="470"
				y="682"
				optionFlags="Text"				
				foregroundColor="lightgray"
				font="font_akzidenz_regular_small"		
				text="Credit:"/>	
			
			<actor
				name="credit_amount"
				x="635"
				y="682"
				width="75"
				text=""
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				horizontalAlignment = "far"
				foregroundColor="lightgray" />		
				
            <actor
				name="tax_label"
				x="470"
				y="706"
				optionFlags="Text"				
				foregroundColor="lightgray"
				font="font_akzidenz_regular_small"		
				text="Tax:"/>	
			
			<actor
				name="tax_amount"
				x="635"
				y="706"
				width="75"
				text=""
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				horizontalAlignment = "far"
				foregroundColor="lightgray" />		

			<actor
				name="total_line_seperator"
				x="470"
				y="731"
				width="240"
				height="2"
				backgroundColor="white"
				optionFlags="Background" />

            <actor
				name="total_label"
				x="470"
				y="740"
				optionFlags="Text"				
				foregroundColor="lightgray"
				font="font_akzidenz_bold_smaller_14"		
				text="TOTAL:"/>	
			
			<actor
				name="total_amount"
				x="635"
				y="740"
				width="75"
				text=""
				optionFlags="Text"
				font="font_akzidenz_bold_smaller_14"
				horizontalAlignment = "far"
				foregroundColor="lightgray" />		

			<actor 
				name="label_total_border"
				x="730"
				y="625"
				width="294"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"/>
				
			<actor
				name="label_charged_once"
				x="730"
				y="726"
				width="294"
				foregroundColor="white"
				font="font_akzidenz_regular_11"
				text=""
				horizontalAlignment="center"
				optionFlags="Text" />

			<actor
				name="label_agree_to_terms"
				x="730"
				y="746"
				width="294"
				foregroundColor="white"
				font="font_akzidenz_regular_tiny_9"
				text=""
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="pay_button"
				x="779"
				y="648"
				width="196"
				height="66"
				cornerSize="34,26"
				font="font_target_small_bold"
				text=""
				style="red_button_style" />

		</scene>
	</aspect>
</resource>
