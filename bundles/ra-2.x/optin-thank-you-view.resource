<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="optin_thank_you_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="optin_thank_you_view_enter"
			onLeave="optin_thank_you_view_leave">
			
			<actor
				name="back_ground_image"
				width="1024"
				height="768"
				x="0"
				y="0"
				image="Email_Opt_In_KioskScreen_R4_Full"
				optionFlags="Image"	/>
		</scene>
	</aspect>
</resource>
