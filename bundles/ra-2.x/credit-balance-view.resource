<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="credit_balance_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
			onEnter="credit_balance_view_enter"
			onLeave="credit_balance_view_leave">
			
			<actor
				name="scene_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				backgroundColor="black"
				opacity="0.6"
				optionFlags="Background" />
				
			<actor
				name="background"
				x="50"
				y="50"
				width="924"
				height="668"
				backgroundColor="white"
				cornerSize="28,28"
				borderColor="228,32,69"
				strokeWeight="5"
				optionFlags="Background, DrawBorder" />
      
			<actor
				name="ad"
				x="178"
				y="65"
				width="668"
				height="83"
				visible="false"
				optionFlags="Image" />
      
			<actor
				name="welcome_message_name"
				x="100"
				y="100"
				font="font_target_medium_large"
				foregroundColor="228,32,69"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_using"
				x="100"
				y="145"
				text="You're using"
				font="font_target_medium_large"
				foregroundColor="102,102,102"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_credits_used"
				x="335"
				y="145"
				width="50"
				font="font_target_medium_large"
				foregroundColor="228,32,69"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_of"
				x="393"
				y="145"
				text="of"
				font="font_target_medium_large"
				foregroundColor="102,102,102"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_credits_available"
				x="441"
				y="145"
				width="50"
				font="font_target_medium_large"
				foregroundColor="228,32,69"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_credits"
				x="499"
				y="145"
				text="credits."
				font="font_target_medium_large"
				foregroundColor="102,102,102"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_no_credits"
				x="100"
				y="145"
				text="You have no credits in your account. To pay with your card, click Confirm."
				font="font_target_small"
				foregroundColor="102,102,102"
				visible="false"
				optionFlags="Text" />
				
			<actor
				name="red_coin_credits"
				x="214"
				y="247"
				width="200"
				height="200"
				image="image_red_coin_credits"
				optionFlags="Image" />
				
			<actor
				name="red_coin_credits_used"
				x="238"
				y="264"
				width="150"
				font="font_target_regular_largest"
				foregroundColor="255,221,153"
				horizontalAlignment="center"
				optionFlags="Text" />
			
			<actor
				name="credits_disclaimer"
				x="115"
				y="450"
				width="440"
				text="If you're using your Redbox Perks points or remaining Play Pass credits for the first time, your rental confirms that you accept the"
				font="font_akzidenz_regular_tiny"
				foregroundColor="80,80,80"
				optionFlags="Text" />
			
			<actor
				name="disclaimer_button"
				x="420"
				y="468"
				width="130"
				height="17"
				text="Redbox Perks Terms"
				font="font_akzidenz_regular_tiny"
				cornerSize="30,30"
				style="warm_purple_button_style" />
			
			<actor
				name="credits_disclaimer_2"
				x="115"
				y="480"
				width="440"
				text="or"
				font="font_akzidenz_regular_tiny"
				foregroundColor="80,80,80"
				optionFlags="Text" />
			
			<actor
				name="disclaimer_button_2"
				x="132"
				y="483"
				width="190"
				height="17"
				text="Play Pass Terms &amp; Conditions"
				font="font_akzidenz_regular_tiny"
				cornerSize="30,30"
				style="warm_purple_button_style" />
			
			<actor
				name="credits_do_not_apply_message"
				x="590"
				y="249"
				width="240"
				font="font_akzidenz_regular_tiny"
				foregroundColor="black"
				optionFlags="Text" />
				
			<actor
				name="subtotal_heading"
				x="597"
				y="332"
				text="Subtotal"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				optionFlags="Text" />
				
			<actor
				name="subtotal_amount"
				x="723"
				y="332"
				width="100"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				horizontalAlignment="far"
				optionFlags="Text" />
				
			<actor
				name="credits_background"
				x="590"
				y="362"
				width="240"
				height="40"
				backgroundColor="228,32,69"
				optionFlags="Background" />
				
			<actor
				name="credits_heading"
				x="597"
				y="369"
				text="Credits"
				font="font_arial_regular_medium"
				foregroundColor="255,255,255"
				optionFlags="Text" />
				
			<actor
				name="credits_amount"
				x="723"
				y="369"
				width="100"
				font="font_arial_regular_medium"
				foregroundColor="255,255,255"
				horizontalAlignment="far"
				optionFlags="Text" />
				
			<actor
				name="tax_heading"
				x="597"
				y="407"
				text="Tax"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				optionFlags="Text" />
				
			<actor
				name="tax_amount"
				x="723"
				y="407"
				width="100"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				horizontalAlignment="far"
				optionFlags="Text" />
				
			<actor
				name="grey_line"
				x="590"
				y="437"
				width="240"
				height="4"
				backgroundColor="204,204,204"
				optionFlags="Background" />
				
			<actor
				name="total_heading"
				x="597"
				y="447"
				text="New total"
				font="font_medium"
				foregroundColor="black"
				optionFlags="Text" />
				
			<actor
				name="total_amount"
				x="723"
				y="447"
				width="100"
				font="font_medium"
				foregroundColor="black"
				horizontalAlignment="far"
				optionFlags="Text" />

			<actor
				name="credits_left"
				x="590"
				y="499"
				width="260"
				font="font_akzidenz_regular_small"
				foregroundColor="black"
				optionFlags="Text" />
				
			<actor
				name="back_button"
				x="100"
				y="591"
				width="170"
				height="65"
				text="    BACK"
				font="font_target_medium"
				cornerSize="30,30"
				style="warm_purple_button_style" />
				
			<actor
				name="back_button_arrow"
				x="130"
				y="612"
				image="image_back_white_arrow"
				optionFlags="image" />
				
			<actor
				name="confirm_button"
				x="729"
				y="590"
				width="195"
				height="65"
				text="CONFIRM"
				font="font_target_medium"
				cornerSize="30,30"
				style="rubine_button_style" />
		</scene>
	</aspect>
</resource>