<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="free_trial_please_wait_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common">
			
			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="image_tickets_red_background"
				optionFlags="Image"	/>

			<actor
				name="please_wait_message_header"
				x="60"
				y="133"
				font="font_target_large_62"
				optionFlags="Text"
				foregroundColor="white"
				text="Please Wait..."
				visible="true" />
				
		</scene>
	</aspect>
</resource>
