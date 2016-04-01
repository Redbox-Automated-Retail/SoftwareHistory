<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="shopping_cart_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
		    name="Default"
			width="1024"
			height="768"
			backgroundColor="159, 159, 159" 
			stylesheet="style_sheet_common"
			onEnter="shopping_cart_view_enter"
			onLeave="shopping_cart_view_leave">
				
			<actor 
				name="cart_background"
				x="0"
				y="0"
				width="1024"
				height="568"
				image="image_cart_background"
				optionFlags="Image" />

			<actor
				name="upperpart_gray_background_border"
				x="24"
				y="74"
				width="976"
				height="2"
				optionFlags="Background"
				backgroundColor="black" />
				
			<actor
                name="lowerpart_gray_background"
                x="0"
                y="622"
				width="1024"
				height="150"
                backgroundColor="58,58,58"
                optionFlags="Background" />
				
			<actor
				name="label_top"
				x="24"
				y="20"
				optionFlags="Text"
				foregroundColor="black"
				font="font_target_medium"
				text="MY CART"/>
				
			<actor
				name="add_movie_add_game"
				x="720"
				y="170"
				width="151"
				height="216"
				visible="true"
				image="image_add_movie_add_game"
				optionFlags="Image"/>
				
			<actor
				name="add_movie_add_game_reflection"
				x="720"
				y="526"
				width="151"
				height="-140"
				visible="true"
				opacity="0.32"
				image="image_add_movie_add_game"
				optionFlags="Image"/>
				
			<actor
				name="add_movie_add_game_reflection_opacity_hiding"
				x="720"
				y="386"
				width="151"
				height="140"
				visible="true"
				image="image_transparent"
				optionFlags="Image"/>
				
			<actor
				name="games_button"
				x="720"
				y="316"
				width="151"
				height="70"
				visible="true"
				hitFlags="Enabled" />
			
			<actor
				name="all_movies_button"
				x="720"
				y="170"
				width="151"
				height="146"
				visible="true"
				hitFlags="Enabled" />
			
			<actor
				name="passive_sale_note"
				x="12"
				y="530"
				width="990"
				optionFlags="Text"
				foregroundColor="black"
				font="font_akzidenz_regular_tiny_9"
				visible="false" />

			<actor
				name="bottom_section_border"
				x="0"
				y="568"
				width="1024"
				height="2"
				backgroundColor="165,165,165"
				optionFlags="Background" />

			<actor
				name="back_button"
				x="8"
				y="576"
				width="115"
				height="36"
				text="       BACK"
				font="font_target_smaller"
				cornerSize="18,18"
				strokeWeight="2"
				borderColor="white"
				style="gray_button_style" />
   
			<actor
                name="back_button_arrow"
                x="29"
                y="585"
				width="21"
				height="18"
                image="image_back_white_arrow"
                optionFlags="Image" />
			
			<actor
				name="credits_button"
				x="877"
				y="576"
				width="121"
				height="36"
				addToFront="true"
				text="USE CREDITS"
				font="font_target_smaller"
				cornerSize="18,18"
				strokeWeight="2"
				style="red_button_style" />

			<actor 
				name="redbox_logo_border"
				x="0"
				y="622"
				width="180"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"
				visible="true" />

			<actor
				name="redbox_logo"
				x="25"
				y="667"
				width="133"
				height="40"
				hitFlags="Enabled"
				style="redbox_logo" />
				
			<actor 
				name="terms_conditions_border"
				x="180"
				y="622"
				width="225"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"
				visible = "true" />
				
			<actor
				name="Terms_conditions_button"
				x="196"
				y="661"
				width="194"
				height="66"
				onHit="help"
				text="Terms &amp; Privacy"
				font="font_akzidenz_bold_smaller"
				cornerSize="34,49"
				strokeWeight="2"
				style="gray_button_style" />
				
			<actor
				name="promo_code_border"
				x="405"
				y="622"
				width="151"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"
				visible = "true" />	
				
			<actor
				name="promo_code_button"
				x="427"
				y="638"
				width="106"
				height="106"
				onHit="add_promo_code"
				cornerSize="52,52"		
				strokeWeight="2"
				style="gray_button_style" />
				
			<actor
				name="promo_label"
				x="454"
				y="673"
				width="60"
				height="30"
				onHit="add_promo_code"
				text="Promo"
                foregroundColor="white"	
				font="font_akzidenz_bold_smaller"
				optionFlags="Text"/>
				
			<actor
				name="promocode_label"
				x="460"
				y="696"
				width="60"
				height="40"
				onHit="add_promo_code"
				text="Code"	
				font="font_akzidenz_bold_smaller"
                foregroundColor="white"			
				optionFlags="Text"/>
			
			<actor 
				name="label_total_border"
				x="556"
				y="622"
				width="468"
				height="146"
				optionFlags="DrawBorder"
				borderColor="78,78,78"
				visible = "true" />
				
            <actor
				name="promo_code_label"
				x="580"
				y="627"
				optionFlags="Text"				
				foregroundColor="lightgray"
				font="font_akzidenz_regular_small"		
				text="Discount:"/>	
			
			<actor
				name="promo_code_amount"
				x="679"
				y="627"
				width="105"
				text="${Discount}"
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				foregroundColor="lightgray" />		
			
			<actor
				name="label_sub_total"
				x="580"
				y="654"
				optionFlags="Text"				
				foregroundColor="white"
				font="font_akzidenz_regular_small"		
				text="Subtotal:"/>	
					
			<actor
				name="sub_total_amount"
				x="679"
				y="654"
				width="105"
				text="${Subtotal}"
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				foregroundColor="white" />		

			<actor
				name="label_tax"
				x="580"
				y="681"
				optionFlags="Text"				
				foregroundColor="white"
				font="font_akzidenz_regular_small"				
				text="Tax:"/>	
			
			<actor
				name="tax_amount"
				x="679"
				y="681"
				width="105"
				text="${Tax}"
				optionFlags="Text"
				font="font_akzidenz_regular_small"
				foregroundColor="white" />
				
			<actor
				name="label_total"
				x="580"
				y="708"
				optionFlags="Text"				
				foregroundColor="white"
				font="font_akzidenz_bold_small"	
				text="TOTAL:"/>	
			
			<actor
				name="total_amount"
				x="679"
				y="708"
				width="105"
				text="${Total}"
				optionFlags="Text"
				font="font_akzidenz_bold_small"
				foregroundColor="white" />

      <actor
				name="label_bottom"
				x="580"
				y="740"
				optionFlags="Text"				
				foregroundColor="white"
				font="font_akzidenz_regular_tiny"			
				text="By pressing 'PAY' you agree to the Terms."
				textRegion="0,0,390,45" />
				
			<actor
				name="pay_button"
				x="803"
				y="658"
				width="196"
				height="66"
				cornerSize="34,26"
				font="font_target_small_bold"
				text="PAY (${Total})"
				style="red_button_style" />

		</scene>
	</aspect>
</resource>
