<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_shopping_cart_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="159, 159, 159"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_shopping_cart_view_enter">

			<actor
				name="cart_background"
				x="0"
				y="0"
				width="1024"
				height="768"
				image="image_tickets_cart_background"
				optionFlags="Image" />

			<actor
				name="label_top"
				x="25"
				y="23"
				optionFlags="Text"
				foregroundColor="black"
				font="font_target_medium"
				text="MY CART"/>

			<actor
				name="cart_note_label"
				x="25"
				y="80"
				optionFlags="Text"
				foregroundColor="black"
				font="font_akzidenz_10_regular"
				text="*Note that we may share your information with the applicable venue and/or ticket seller to process your transaction, for recordkeeping or for other internal business purposes."/>

			<actor
				name="event_art"
				x="350"
				y="205"
				width="337"
				height="248"
				borderColor="150,150,150"
				strokeWeight="2"
				optionFlags="Image, DrawBorder" />

			<actor
				name="remove_tickets_button"
				x="662"
				y="191"
				width="50"
				height="50"
				backgroundColor="black"
				foregroundColor="white"
				text="X"
				textRegion="16,12,50,50"
				font="font_akzidenz_18_bold"
				borderColor="white"
				strokeWeight="4"
				cornerSize="25,25"
				hitFlags="Enabled"
				optionFlags="Text, Background, DrawBorder" />

			<actor
				name="event_price"
				x="0"
				y="468"
				width="1024"
				height="30"
				horizontalAlignment="center"
				font="font_akzidenz_15_bold"
				optionFlags="Text" />

			<actor
				name="event_price_breakdown"
				x="0"
				y="490"
				width="1024"
				height="60"
				horizontalAlignment="center"
				font="font_akzidenz_11_regular"
				optionFlags="Text" />
				
			<actor
				name="tax_note"
				x="400"
				y="543"
				width="254"
				height="20"
				font="font_akzidenz_11_regular"
				visible="false"
				optionFlags="Text" />
				
			<actor
				name="start_over_button"
				x="8"
				y="580"
				width="115"
				height="36"
				font="font_target_12_regular"
				text="     Start Over"
				style="gray_button_style" />

			<actor
				name="start_over_arrow"
				x="20"
				y="589"
				image="image_start_over_arrow"
				optionFlags="Image" />

			<actor
				name="redbox_logo"
				x="56"
				y="669"
				width="133"
				height="40"
				image="image_redbox_logo"
				optionFlags="Image" />

			<actor
				name="tickets_terms_conditions_button"
				x="311"
				y="661"
				width="194"
				height="66"
				cornerSize="33, 33"
				text="Terms &amp; Privacy"
				font="font_akzidenz_12_bold"
				strokeWeight="2"
				style="gray_button_style" />
				
			<actor
				name="tickets_terms_conditions_small_button"
				x="286"
				y="661"
				width="107"
				height="66"
				cornerSize="33, 33"
				text="Terms &amp;&#010;Privacy"
				font="font_akzidenz_14_bold"
				visible="false"
				style="gray_button_style" />
				
			<actor
				name="rules_button"
				x="422"
				y="661"
				width="107"
				height="66"
				cornerSize="33, 33"
				text="RULES"
				font="font_akzidenz_14_bold"
				visible="false"
				style="gray_button_style" />
				
			<actor
				name="label_sub_total"
				x="593"
				y="654"
				optionFlags="Text"
				foregroundColor="white"
				font="font_akzidenz_14_regular"
				text="Subtotal:"/>

			<actor
				name="sub_total_amount"
				x="679"
				y="654"
				width="105"
				text="${ShoppingCart:SubTotal}"
				optionFlags="Text"
				font="font_akzidenz_14_regular"
				horizontalAlignment="far"
				foregroundColor="white" />

			<actor
				name="label_tax"
				x="593"
				y="681"
				optionFlags="Text"
				foregroundColor="white"
				font="font_akzidenz_14_regular"
				text="Ticket Tax:"/>

			<actor
				name="tax_amount"
				x="679"
				y="681"
				width="105"
				text="${ShoppingCart:Tax}"
				optionFlags="Text"
				font="font_akzidenz_14_regular"
				horizontalAlignment="far"
				foregroundColor="white" />

			<actor
				name="total_border"
				x="593"
				y="707"
				width="191"
				height="2"
				optionFlags="Background"
				backgroundColor="white"  />

			<actor
				name="label_total"
				x="593"
				y="712"
				optionFlags="Text"
				foregroundColor="white"
				font="font_akzidenz_14_bold"
				text="TOTAL:"/>

			<actor
				name="total_amount"
				x="679"
				y="712"
				width="105"
				text="${ShoppingCart:Total}"
				optionFlags="Text"
				font="font_akzidenz_14_bold"
				horizontalAlignment="far"
				foregroundColor="white" />

			<actor
				name="label_bottom"
				x="593"
				y="742"
				optionFlags="Text"
				foregroundColor="white"
				font="font_akzidenz_10_regular"
				text="By pressing 'PAY' you agree to the Tickets Terms."
				textRegion="0,0,390,45" />

			<actor
				name="pay_button"
				x="803"
				y="658"
				width="196"
				height="66"
				cornerSize="33, 33"
				font="font_target_18_bold"
				text="PAY (${ShoppingCart:Total})"
				style="red_button_style_3pt_redstroke" />

		</scene>
	</aspect>
</resource>
