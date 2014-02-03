<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="free_trial_terms_and_privacy_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
			<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			backgroundImage="image_zipcode_background"
			onEnter="free_trial_terms_and_privacy_agree_view_enter" >

			<actor
				name="top_banner"
				x="0"
				y="0"
				optionFlags="Image"
				image="image_free_trial_banner" />
				
			<actor
				name="header_label"
				x="65"
				y="100"
				width="980"
				text="Before signing up, please confirm:"
				font="font_target_large"
				foregroundColor="black"
				horizontalAlignment="near"
				optionFlags="Text" />

			<actor
				name="message_label"
				x="65"
				y="190"
				width="900"
				textregion="65,190,900,300"
				text="By checking the boxes below and touching NEXT, I agree to the terms of service, and I understand that the card I
				swipe will be stored and will become effective immediately at the end of my one-month trial. All future Redbox
				Instant by Verizon subscription charges will be billed automatically to this card.&#10;&#10;
				For my convenience, my Redbox Instant by Verizon service will be linked to my Redbox account (existing or new)
				and both will be accesible using the same login email and password.<redacted>;&#10;
				Full terms and conditions, and privacy policies for subscriptions signup, can be found by clicking the &#34;Terms &#38; Privacy&#34;
				button at the bottom of the screen.
				"
				font="font_target_smaller"
				foregroundColor="black"
				horizontalAlignment="near"
				optionFlags="Text" />
				
			<actor
				name="agree_checkbox"
				x="65"
				y="435"
				image="image_gray_unselected_checkbox"
				hitFlags="enabled"
				optionFlags="Image, Text"
				tag="unchecked"
				onHit="" />
			
			<actor
				name="agree_label"
				x="140"
				y="435"
				width="900"
				text="I have read and I accept the Redbox Instant by Verizon and Redbox terms.&#10;I agree to the automatic payment for these charges and the Subscriber Billing Agreement."
				font="font_target_smaller"
				foregroundColor="black"
				horizontalAlignment="near"
				optionFlags="Text" />
				
			<actor
				name="consent_checkbox"
				x="65"
				y="510"
				image="image_gray_unselected_checkbox"
				hitFlags="enabled"
				optionFlags="Image, Text"
				tag="unchecked"
				onHit="" />

			<actor
				name="consent_label"
				x="140"
				y="510"
				width="900"
				text="I consent to both Redbox and Redbox Instant by Verizon having access to my account&#10;information, including streaming and video-rental history."
				font="font_target_smaller"
				foregroundColor="black"
				horizontalAlignment="near"
				optionFlags="Text" />
			
			<actor
				name="terms_button"
				x="35"
				y="660"
				width="225"
				height="66"
				text="Terms &#38; Conditions"
				font="font_target_small"
				cornerSize="32,32"
				style="gray_button_style" /> 
			
			<actor
				name="cancel_button"
				x="576"
				y="660"
				width="196"
				height="66"
				text="CANCEL SIGNUP"
				font="font_target_small"
				cornerSize="32,32"
				style="gray_button_style" /> 
							
			<actor
				name="next_button"
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
