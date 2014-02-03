<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="free_trial_gift_card_swiped_popup" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="free_trial_gift_card_swiped_popup_enter" >
			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>
				
			<actor
				name="free_trial_image"
				x="0"
				y="0"
				image="image_free_trial_banner"
				optionFlags="Image" />

			<actor
				name="free_trial_gift_card_swiped_header"
				x="68"
				y="160"
				font="font_target_medium_overlays"
				optionFlags="Text"
				foregroundColor="white"
				text="Sorry, we're having some trouble."
				visible="true" />

			<actor
				name="free_trial_gift_card_swiped_message_line1"
				x="68"
				y="286"
				font="font_target_medium"
				optionFlags="Text"
				foregroundColor="white"
				text="We can't process a Redbox gift card for the &#13;&#10;Redbox Instant by Verizon subscription signup."
				visible="true" />

			<actor
				name="free_trial_gift_card_swiped_message_line2"
				x="68"
				y="409"
				font="font_target_medium"
				optionFlags="Text"
				foregroundColor="white"
				text="Please swipe another card."
				visible="true" />

			<actor
				name="footer_area"
				x="0"
				y="626"
				optionFlags="Background, TextShadow"
				backgroundColor="60,57,57"
				width="1024"
				height="142"
				visible="true" />

			<actor
				name="free_trial_gift_card_swiped_view_ok_button"
				x="794"
				y="660"
				width="196"
				height="66"
				cornerSize="32,32"
				font="font_target_small"
				text="OK"
				style="red_button_style" />

		</scene>
	</aspect>
</resource>
