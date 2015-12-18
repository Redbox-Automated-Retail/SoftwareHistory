<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="product_detail_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="229, 229, 229"
			stylesheet="style_sheet_common"
			onEnter="product_detail_view_enter"
			onLeave="product_detail_view_leave">

			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="product_detail_starburst_background"
				optionFlags="Image"
				stylesheet="style_sheet_common" />

			<actor
				name="left_arrow"
				x="0"
				y="150"
				opacity="0.9"
				hitFlags="Enabled"
				image="image_left_navigation_arrow"
				optionFlags="Image" />

			<actor
				name="selected_product_background1"
				x="71"
				y="57"
				width="414"
				height="574"
				hitFlags="None"
				cornerSize="7,7"
				strokeWeight="1.0"
				backgroundColor="187,187,187"
				optionFlags="Background, BackgroundGradient" />

			<actor
				name="selected_product_background2"
				x="72"
				y="58"
				width="412"
				height="572"
				hitFlags="None"
				cornerSize="7,7"
				strokeWeight="1.0"
				backgroundColor="176,176,176"
				optionFlags="Background, BackgroundGradient" /> 

			<actor
				name="selected_product_background3"
				x="73"
				y="59"
				width="410"
				height="570"
				hitFlags="None"
				cornerSize="6,6"
				strokeWeight="1.0"
				backgroundColor="99,99,99"
				optionFlags="Background, BackgroundGradient" />

			<actor
				name="selected_product_image"
				x="74"
				y="60"
				width="408"
				height="568"
				hitFlags="None"
				strokeWeight="2.0"
				backgroundColor="0,0,0"
				optionFlags="Image, Background" />

			<actor
				name="flag"
				x="53"
				y="17"
				width="170"
				height="65"
				font="font_akzidenz_bold_smaller"
				foregroundColor="white"
				textRegion="-3,10,160,40"
				horizontalAlignment="center"
				visible="false"
				optionFlags="Image, Text, TextShadow" />

			<!--actor
				name="banner"
				x="74"
				y="580"
				width="410"
				height="50"
				font="font_akzidenz_bold_smaller"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="center"
				visible="false"
				text="${ProductDetail:ProductType}"
				optionFlags="Image, Text" /-->

			<actor
				name="coming_soon_violator"
				x="74"
				y="550"
				width="410"
				height="80"
				font="font_large"
				foregroundColor="white"
				verticalAlignment="center"
				horizontalAlignment="center"
				visible="false"
				optionFlags="Image, Text" />				

			<actor
				name="selected_product_title"
				x="546"
				y="60"
				width="350"
				height="100"
				optionFlags="Text, ElideText"
				text="${ProductDetail:Name}"
				foregroundColor="196, 38, 46"
				font="font_cooper_23" />

			<actor
				name="selected_product_Rating"
				x="546"
				y="136"
				width="48"
				height="22"
				borderColor="138, 118, 102"
				textRegion="0,0,48,22"
				optionFlags="Text,DrawBorder"
				foregroundColor="138, 118, 102"
				text="${ProductDetail:Rating}"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_vag_black_10" />

			<actor
				name="selected_product_Rating_big"
				x="546"
				y="136"
				width="65"
				height="22"
				borderColor="138, 118, 102"
				textRegion="0,0,63,22"
				optionFlags="Text,DrawBorder"
				foregroundColor="138, 118, 102"
				text="${ProductDetail:Rating}"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_vag_black_10" />

			<actor
				name="selected_product_Media"
				x="602"
				y="136"
				width="40"
				height="22"
				borderColor="138, 118, 102"
				textRegion="0,0,40,22"
				optionFlags="Text,DrawBorder"
				foregroundColor="138, 118, 102"
				text="${ProductDetail:Product_type}"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_vag_black_10" />

			<actor
				name="selected_product_Media_big"
				x="602"
				y="136"
				width="72"
				height="22"
				borderColor="138, 118, 102"
				textRegion="0,0,72,22"
				optionFlags="Text,DrawBorder"
				foregroundColor="138, 118, 102"
				text="${ProductDetail:Product_type}"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_vag_black_10" />

			<actor
				name="selected_product_running_time"
				x="602"
				y="136"
				width="40"
				height="22"
				borderColor="138, 118, 102"
				textRegion="0,0,40,22"
				optionFlags="Text,DrawBorder"
				foregroundColor="138, 118, 102"
				text="${ProductDetail:RunningTime}"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_vag_black_10" />

			<actor
				name="selected_product_national_street_date"
				x="650"
				y="136"
				width="42"
				height="22"
				borderColor="138, 118, 102"
				textRegion="0,0,42,22"
				optionFlags="Text,DrawBorder"
				foregroundColor="138, 118, 102"
				text="${ProductDetail:NationalStreetDate}"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_vag_black_10" />

			<actor
				name="selected_product_CC"
				x="700"
				y="136"
				width="48"
				height="22"
				borderColor="138, 118, 102"
				textRegion="0,0,48,22"
				optionFlags="Text,DrawBorder"
				foregroundColor="138, 118, 102"
				text="CC"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_vag_black_10" />

			<actor
				name="selected_product_description"
				x="546"
				y="180"
				width="380"
				height="300"
				font="font_vag_light_10"
				optionFlags="Text"
				text="${ProductDetail:Description}" />

			<actor
				name="separator_1"
				x="528"
				y="483"
				width="404"
				height="2"
				optionFlags="Image"
				image="line" />

			<actor
				name="starring_label"
				x="546"
				y="492"
				optionFlags="Text"
				font="font_vag_black_11"
				text="Starring: " />

			<actor
				name="starring"
				x="606"
				y="492"
				width="320"
				height="30"
				optionFlags="Text"
				text="${ProductDetail:Starring}"
				font="font_vag_light_11"/>

			<actor
				name="category_label"
				x="546"
				y="510"
				optionFlags="Text"
				font="font_vag_black_11"
				text="Category: " />

			<actor
				name="category"
				x="612"
				y="510"
				width="300"
				height="20"
				optionFlags="Text"
				font="font_vag_light_11"
				text="${ProductDetail:Category}" />

			<actor
				name="rating_label"
				x="546"
				y="531"
				optionFlags="Text"
				visible="false"
				font="font_vag_black_11" />

			<actor
				name="rating"
				x="635"
				y="531"
				optionFlags="Text"
				text="${ProductDetail:Rating}"
				visible="false"
				font="font_vag_light_11" />
				
			<actor
				name="rating_description"
				x="546"
				y="531"
				height="70"
				optionFlags="Text"
				visible="false"
				font="font_vag_light_11" />

			<actor
				name="first_night_price_label"
				x="546"
				y="531"
				optionFlags="Text"
				font="font_vag_black_11"
				text="" />

			<actor
				name="first_night_price"
				x="648"
				y="531"
				optionFlags="Text"
				font="font_vag_light_11"
				text="${ProductDetail:FirstNightPrice}" />

			<actor
				name="extra_night_price_label"
				x="546"
				y="551"
				optionFlags="Text"
				font="font_vag_black_11"
				text="" />

			<actor
				name="extra_night_price"
				x="653"
				y="551"
				optionFlags="Text"
				font="font_vag_light_11"
				text="${ProductDetail:ExtraNightPrice}" />				

			<actor
				name="return_time_label"
				x="546"
				y="571"
				optionFlags="Text"
				font="font_vag_black_11"
				text="Return: " />

			<actor
				name="return_time"
				x="597"
				y="571"
				height="70"
				optionFlags="Text"
				font="font_vag_light_11"
				text="${ProductDetail:ReturnTime}" />

			<actor
				name="separator_2"
				x="528"
				y="607"
				width="404"
				height="2"
				optionFlags="Image"
				image="line" />

			<actor
				name="separator_3"
				x="764"
				y="483"
				height="124"
				style="browse_alpha_separator_vertical_grey_style"
				visible="false" />

			<actor
				name="sub_platform_indicator"
				x="766"
				y="485"
				image=""
				optionFlags="Image"
				visible="false" />

			<actor
				name="out_of_stock_line1"
				x="694"
				y="634"
				height="30"
				width="300"
				strokeWeight="4.0"
				borderColor="white"
				font="font_akzidenz_bold_medium"
				foregroundColor="162,8,16"
				optionFlags="Text"
				verticalAlignment="near"
				horizontalAlignment="near"
				visible="false"
				text="Text LOCATE to 727272" />

			<actor
				name="out_of_stock_line2"
				x="694"
				y="660"
				height="40"
				width="300"
				strokeWeight="4.0"
				borderColor="white"
				font="font_akzidenz_regular_smaller"
				foregroundColor="162,8,16"
				optionFlags="Text"
				verticalAlignment="near"
				horizontalAlignment="near"
				visible="false"
				text="to find other Redbox kiosks nearby that might have this {0}.*" />

			<actor
				name="out_of_stock_line3"
				x="694"
				y="704"
				height="55"
				width="300"
				strokeWeight="4.0"
				borderColor="white"
				font="font_akzidenz_regular_8"
				foregroundColor="black"
				optionFlags="Text"
				verticalAlignment="near"
				horizontalAlignment="near"
				visible="false"
				text="*WHEN YOU TEXT LOCATE TO 727272, YOU WILL GET UP TO 2 MESSAGES. Up to 2 msgs&#47;week. Msg &#38; data rates may apply. Text STOP to quit, HELP for help." />		

			<actor
				name="collection_button"
				x="240"
				y="645"
				width="276"
				height="70"
				cornerSize="33,33"
				font="font_bryant_bold_18"
				text="SEE RELATED&#10;COLLECTION"
				visible="true"
				style="gray_button_white_border_style" />

			<actor
				name="buy_used_product_button"
				x="536"
				y="635"
				width="88"
				height="88"
				cornerSize="47,47"
				text="BUY"
				font="font_target_medium_small"
				style="kiosk_2_blue_button_style" />

			<actor
				name="buy_other_product_button"
				x="690"
				y="630"
				width="94"
				height="94"
				cornerSize="47,47"
				text="BUY"
				textRegion="0,15,94,45"
				font="font_target_small_medium"
				style="kiosk_2_blue_button_style" />

			<actor
				name="buy_other_product_amount_label"
				x="690"
				y="670"
				width="94"
				height="45"
				foregroundColor="White"
				text="${ProductDetail:PurchasePrice}"
				font="font_target_small_medium"
				visible="true"
				verticalAlignment="center"
				horizontalAlignment="center"
				optionFlags="Text"	/>
				
			<actor
				name="back_button"
				x="74"
				y="644"
				width="146"
				height="70"
				text="    BACK"
				cornerSize="35,35"
				font="font_bryant_bold_24"
				onHit="go_back"
				style="yellow_button_style" />

			<actor
				name="back_button_arrow"
				x="90"
				y="668"
				hitFlags="Enabled"
				tag="back"
				image="image_back-black-arrow-gray-background"
				onHit="go_back"
				optionFlags="image" />

			<actor
				name="add_button_other_product"
				x="792"
				y="644"
				width="146"
				height="70"
				cornerSize="35,35"
				font="font_bryant_bold_24"
				text="   ADD"
				style="red_button_style" />

			<actor
				name="plus_button_other_product"
				x="820"
				y="668"
				width="22"
				height="22"
				hitFlags="Enabled"
				image="image_plus"
				optionFlags="Image" />

			<actor
				name="add_button_dvd"
				x="642"
				y="644"
				width="146"
				height="70"
				cornerSize="35,35"
				font="font_bryant_bold_24"
				text="   DVD"
				style="red_button_style" />

			<actor
				name="dvd_in_cart"
				x="669"
				y="644"
				height="70"
				width="90"
				optionFlags="Text"
				enabled="false"
				visible="false"
				font="font_bryant_bold_18"
				text="DVD       IN CART"
				foregroundColor="white"
				style="red_button_style" />

			<actor
				name="dvd_out_of_stock"
				x="660"
				y="631"
				height="70"
				width="110"
				optionFlags="Text"
				enabled="false"
				visible="false"
				font="font_bryant_bold_18"
				text="DVD"
				foregroundColor="white"
				style="red_button_style" />

			<actor
				name="dvd_out_of_stock2"
				x="640"
				y="656"
				height="70"
				width="150"
				optionFlags="Text"
				enabled="false"
				visible="false"
				font="font_target_small_14"
				text="OUT OF STOCK"
				foregroundColor="white"
				style="red_button_style" />

			<actor
				name="plus_button_dvd"
				x="670"
				y="668"
				width="22"
				height="22"
				hitFlags="Enabled"
				image="image_plus"
				optionFlags="Image" />

			<actor
				name="add_button_bluray"
				x="806"
				y="644"
				width="194"
				height="70"
				cornerSize="35,35"
				font="font_bryant_bold_24"
				text="   BLU-RAY"
				style="blue_button_style" />

			<actor
				name="bluray_in_cart"
				x="843"
				y="644"
				height="70"
				width="120"
				optionFlags="Text"
				enabled="false"
				visible="false"
				font="font_bryant_bold_18"
				text="BLU-RAY  IN CART"
				foregroundColor="white"
				style="red_button_style" />

			<actor
				name="bluray_out_of_stock"
				x="828"
				y="631"
				height="70"
				width="150"
				optionFlags="Text"
				enabled="false"
				visible="false"
				font="font_bryant_bold_18"
				text="BLU-RAY"
				foregroundColor="white"
				style="red_button_style" />

			<actor
				name="bluray_out_of_stock2"
				x="828"
				y="656"
				height="70"
				width="150"
				optionFlags="Text"
				enabled="false"
				visible="false"
				font="font_target_small_14"
				text="OUT OF STOCK"
				foregroundColor="white"
				style="red_button_style" />

			<actor
				name="plus_button_bluray"
				x="824"
				y="668"
				width="22"
				height="22"
				hitFlags="Enabled"
				image="image_plus"
				optionFlags="Image" />

			<actor
				name="coming_soon_date"
				x="810"
				y="615"
				optionFlags="Text"
				font="font_target_smaller_overlays"
				foregroundColor="172,44,50"
				visible="false" />

			<actor
				name="email_me_button"
				x="780"
				y="645"
				width="170"
				height="70"
				cornerSize="33,33"
				font="font_target_small_button_overlays"
				text="EMAIL ME"
				visible="false"
				style="red_button_style" />

			<actor
				name="right_arrow"
				x="958"
				y="150"
				opacity="0.9"
				hitFlags="Enabled"
				image="image_right_navigation_arrow"
				optionFlags="Image" />

		</scene>
	</aspect>
</resource>
