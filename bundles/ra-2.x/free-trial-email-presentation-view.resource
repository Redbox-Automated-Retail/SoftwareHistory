<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="free_trial_email_presentation_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			backgroundColor="227,227,227"
			stylesheet="style_sheet_common"
			onEnter="free_trial_email_presentation_view_enter" >

			<actor
				name="freetrial_offer_banner"
				x="0"
				y="0"
				image="image_free_trial_banner"
				optionFlags="Image"/>

			<actor
				name="free_trial_email_presentation_message1"
				x="20"
				y="95"
				width="590"
				height="100"
				font="font_akzidenz_regular_16" 
				text="A link to your free trial will be sent to this address:"
				optionFlags="Text"/>
			
			<actor
				name="free_trial_email_presentation_current_email"
				x="65"
				y="130"
				width="890"
				height="170"
				optionFlags="Text"
				verticalAlignment="center"
				text=""
				font="font_target_medium_large" />

			<actor
				name="free_trial_email_presentation_message2"
				x="65"
				y="305"
				optionFlags="Text"
				text="You can change it at any time at redbox.com/account"
				font="font_akzidenz_regular_16" />
				
			<actor
				name="button_background"
				x="0"
				y="633"
				width="1024"
				height="135"
				backgroundColor="58, 54, 55"
				foregroundColor="58, 54, 55"
				optionFlags="Background" />

			<actor
				name="reject_optin_button"
				x="576"
				y="660"
				width="196"
				height="66"
				font="font_target_small"
				cornerSize="32,32"
				style="light_gray_with_gray_border_button_style" />
				
			<actor
				name="accept_optin_button"
				x="794"
				y="660"
				width="196"
				height="66"
				text="          NEXT"
				font="font_target_small"
				cornerSize="32,32"
				style="red_next_button_style" /> 
					
	       	<actor
                name="next_button_right_arrow_icon"
                x="915"
                y="682"
				image="image_email_entry_right_arrow_icon"
                optionFlags="Image" />
				
			<actor
                name="image_next_white_arrow_big"
                x="915"
                y="682"
				image="image_next_white_arrow_big"
                optionFlags="Image" />
		</scene>
	</aspect>
</resource>