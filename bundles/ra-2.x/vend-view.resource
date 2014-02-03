<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="vend_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene 
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			onEnter="vend_view_enter" 
			onLeave="vend_view_leave">
			
			<actor
				name="text_label"
				x="63"
				y="260"
				width="450"
				text="YOUR RENTALS ARE ON THEIR WAY!"
				foregroundColor="white"
				font="font_target_larger"
				optionFlags="Text, TextShadow"
				visible = "false" />
				
			<actor
				name="redbox-disc-background_image"
				x="700"
				y="155"
				height="465"
				width="342"
				image="image_redbox_disc_case_plan"
				horizontalAlignment="center"
				verticalAlignment="center"
				hitFlags="None"
				visible="false"
				optionFlags="Image" />
				
			<actor
				name="redbox_logo"
				x="795"
				y="350"
				width="220"
				height="70"
				image="image_redbox_logo"
				optionFlags="Image"
				visible = "false"/>
		</scene>
	</aspect>
</resource>
