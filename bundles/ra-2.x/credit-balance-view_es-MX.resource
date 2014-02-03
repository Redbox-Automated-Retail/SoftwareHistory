<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="credit_balance_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<property name="culture" value="es-MX" />
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
				borderColor="162,16,24"
				strokeWeight="5"
				optionFlags="Background, DrawBorder" />
				
			<actor
				name="welcome_message_name"
				x="100"
				y="100"
				font="font_target_medium_small"
				foregroundColor="180,18,13"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_using"
				x="100"
				y="135"
				text="Est&#225; utilizando"
				font="font_target_medium_small"
				foregroundColor="102,102,102"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_credits_used"
				x="295"
				y="135"
				width="50"
				font="font_target_medium_small"
				foregroundColor="180,18,13"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_of"
				x="343"
				y="135"
				text="cr&#233;ditos del total de"
				font="font_target_medium_small"
				foregroundColor="102,102,102"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_credits_available"
				x="610"
				y="135"
				width="50"
				font="font_target_medium_small"
				foregroundColor="180,18,13"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_credits"
				x="657"
				y="135"
				text="en este transacci&#243;n."
				font="font_target_medium_small"
				foregroundColor="102,102,102"
				optionFlags="Text" />
				
			<actor
				name="welcome_message_no_credits"
				x="100"
				y="135"
				text="En este momento no tiene cr&#233;dito. Para pagar con su tarjeta y seguir, marca Confirmo."
				font="font_target_small_medium"
				foregroundColor="102,102,102"
				visible="false"
				optionFlags="Text" />
				
			<actor
				name="red_coin_credits"
				x="220"
				y="220"
				width="200"
				height="245"
				image="image_red_coin_credits"
				optionFlags="Image" />
				
			<actor
				name="red_coin_credits_used"
				x="244"
				y="237"
				width="150"
				font="font_target_regular_largest"
				foregroundColor="255,221,153"
				horizontalAlignment="center"
				optionFlags="Text" />
				
			<actor
				name="credits_disclaimer"
				x="150"
				y="500"
				width="322"
				text="Se utiliza y se aplica cr&#233;ditos de su cuenta cada noche para est&#225; transacci&#243;n. En el momento que se acaban se cargar&#237;a el balance a su tarjeta de cr&#233;dito."
				font="font_akzidenz_regular_tiny"
				foregroundColor="80,80,80"
				optionFlags="Text" />
				
			<actor
				name="credits_do_not_apply_message"
				x="590"
				y="215"
				width="240"
				font="font_akzidenz_regular_tiny"
				foregroundColor="black"
				optionFlags="Text" />
				
			<actor
				name="subtotal_heading"
				x="597"
				y="305"
				text="Subtotal"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				optionFlags="Text" />
				
			<actor
				name="subtotal_amount"
				x="723"
				y="305"
				width="100"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				horizontalAlignment="far"
				optionFlags="Text" />
				
			<actor
				name="credits_background"
				x="590"
				y="335"
				width="240"
				height="40"
				backgroundColor="174,4,13"
				optionFlags="Background" />
				
			<actor
				name="credits_heading"
				x="597"
				y="342"
				text="Cr&#233;ditos"
				font="font_arial_regular_medium"
				foregroundColor="255,255,255"
				optionFlags="Text" />
				
			<actor
				name="credits_amount"
				x="723"
				y="342"
				width="100"
				font="font_arial_regular_medium"
				foregroundColor="255,255,255"
				horizontalAlignment="far"
				optionFlags="Text" />
				
			<actor
				name="tax_heading"
				x="597"
				y="380"
				text="Impuesto"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				optionFlags="Text" />
				
			<actor
				name="tax_amount"
				x="723"
				y="380"
				width="100"
				font="font_arial_regular_medium"
				foregroundColor="110,110,110"
				horizontalAlignment="far"
				optionFlags="Text" />
				
			<actor
				name="grey_line"
				x="590"
				y="410"
				width="240"
				height="4"
				backgroundColor="204,204,204"
				optionFlags="Background" />
				
			<actor
				name="total_heading"
				x="597"
				y="420"
				text="Nuevo total"
				font="font_medium"
				foregroundColor="black"
				optionFlags="Text" />
				
			<actor
				name="total_amount"
				x="723"
				y="420"
				width="100"
				font="font_medium"
				foregroundColor="black"
				horizontalAlignment="far"
				optionFlags="Text" />
				
			<actor
				name="credits_left"
				x="592"
				y="470"
				width="260"
				font="font_akzidenz_regular_small"
				foregroundColor="black"
				optionFlags="Text" />
				
			<actor
				name="back_button"
				x="100"
				y="591"
				width="235"
				height="65"
				text="    CANCELA"
				font="font_target_medium"
				cornerSize="30,30"
				style="gray_button_style" />
				
			<actor
				name="back_button_arrow"
				x="130"
				y="612"
				image="image_back_white_arrow"
				optionFlags="image" />
				
			<actor
				name="confirm_button"
				x="689"
				y="590"
				width="215"
				height="65"
				text="CONFIRMO"
				font="font_target_medium"
				cornerSize="30,30"
				style="red_button_style" />
		</scene>
	</aspect>
</resource>