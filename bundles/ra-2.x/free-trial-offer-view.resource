<?xml version="1.0" ?>
<resource type="view">
	<property name="name" value="free_trial_offer_screen_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundImage="image_zipcode_background"
			stylesheet="style_sheet_common"
			onEnter="free_trial_offer_screen_view_enter" >

			<actor
				name="campaign_image_placeholder"
				x="0"
				y="0"
				image="image_free_trial_offer"
				optionFlags="Image"/>

			<actor
				name="free_trial_offer_text"
				x="20"
				y="638"
				width="590"
				height="110"
				horizontalAlignment="center"
				verticalAlignment="center"
				font="font_target_medium_small" 
				foregroundColor="white"
				optionFlags="Text"/>
			
			<actor
				name="reject_optin_button"
				x="630"
				y="660"
				width="180"
				height="66"
				font="font_target_small_button_overlays"
				cornerSize="32, 19"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="accept_optin_button"
				x="830"
				y="660"
				width="170"
				height="66"
				font="font_target_small_button_overlays"
				cornerSize="30, 19"
				style="red_button_style" />
				
		</scene>
	</aspect>
</resource>