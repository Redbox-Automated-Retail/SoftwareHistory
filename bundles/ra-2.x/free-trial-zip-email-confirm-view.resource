<?xml version="1.0" ?>
<resource type="view">
	<property name="name" value="free_trial_zip_email_confirmation_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"			
			stylesheet="style_sheet_common"
			backgroundImage="image_zipcode_background"
			onEnter="free_trial_zip_email_confirm_view_enter" >

			<actor
				name="freetrial_offer_banner"
				x="0"
				y="0"
				image="image_free_trial_banner"
				optionFlags="Image"/>

			<actor
				name="free_trial_email_presentation_text"
				x="25"
				y="100"
				width="590"
				height="110"
				font="font_akzidenz_regular_16" 
				optionFlags="Text"/>
			
			<actor
				name="free_trial_email_presentation_entry"
				x="74"
				y="192"
				optionFlags="Image"
				image="image_free_trial_email_presentation_entry" />
			
			<actor
				name="free_trial_current_zip"
				x="110"
				y="285"
				optionFlags="Text"
				font="font_target_medium_small" />
			
			<actor
				name="free_trial_current_email"
				x="110"
				y="420"
				optionFlags="Text"
				font="font_target_medium_small" />

			<actor
				name="free_trial_edit_zip"
				x="770"
				y="220"
				width="170"
				height="66"
				font="font_target_small_button_overlays"
				cornerSize="32, 19"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="free_trial_edit_email"
				x="770"
				y="360"
				width="170"
				height="66"
				font="font_target_small_button_overlays"
				cornerSize="32, 19"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="reject_optin_button"
				x="576"
				y="660"
				width="196"
				height="66"
				font="font_target_small"
				cornerSize="32,32"
				style="gray_button_style" /> 
				
			<actor
				name="accept_optin_button"
				x="794"
				y="660"
				width="196"
				height="66"
				font="font_target_small"
				cornerSize="30, 19"
				style="red_button_style" />
				
		</scene>
	</aspect>
</resource>