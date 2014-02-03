<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="free_trial_swipe_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="free_trial_swipe_view_enter"
			onLeave="free_trial_swipe_view_leave">

			<actor
				name="free_trial_swipe_background_image"
				width="1024"
				height="650"
				optionFlags="Image"
				visible="false" />

			<actor
				name="free_trial_swipe_title_label"
				x="20"
				y="10"
				optionFlags="Text"
				text="REDBOX.COM PICKUP"
				font="font_akzidenz_bold_large"
				foregroundColor="White"
				verticalAlignment="near"
				horizontalAlignment="near"
				visible="false" />

			<actor
				name="free_trial_swipe_message_label"
				x="80"
				y="280"
				optionFlags="Text"
				text="Ready. Set. Swipe!"
				font="font_target_largest"
				foregroundColor="White"
				textRegion="0,0,500,250"
				visible="false" />

			<actor
				name="free_trial_swipe_message_label"
				x="66"
				y="164"
				optionFlags="Text"
				text="Swipe your card to pickup your reserved items."
				font="font_target_larger"
				foregroundColor="White"
				verticalAlignment="near"
				horizontalAlignment="near"
				textRegion="0,0,600,270"
				visible="false" />

			<actor
				name="free_trial_swipe_message_label1"
				x="66"
				y="422"
				optionFlags="Text"
				text="(Reserve a title on Redbox.com and guarantee it's here!)"
				font="font_target_medium"
				foregroundColor="White"
				verticalAlignment="near"
				horizontalAlignment="near"
				textRegion="0,0,430,180"
				visible="false" />

			<actor
				name="free_trial_swipe_back_button"
				x="35"
				y="665"
				width="194"
				height="70"
				cornerSize="34,34"
				font="font_target_small"
				text="BACK"
				tag="back_button"
				hitFlags="enabled"
				onHit="free_trial_swipe_view_alert"
				visible="true"
				style="red_button_style" />

			<actor
				name="free_trial_swipe_back_button_arrow"
				x="65"
				y="690"
				image="image_back_white_arrow"
				tag="back_button_arrow"
				hitFlags="enabled"
				onHit="free_trial_swipe_view_alert"
				optionFlags="image" />

		</scene>
	</aspect>
</resource>