<?xml version="1.0"?>
<resource type="view">
    <property name="name" value="maintenance_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
    <aspect name="content" >
		<scene 
			name="Default"
			width="1024"
			height="768"
			backgroundImage="${MaintenanceView:BackgroundImage}"
			onEnter="maintenance_view_enter"
			onLeave="maintenance_view_leave" >
			
			<actor
				name="title"
				x="82"
				y="150"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_larger"
				optionFlags="Text"
				text="Redbox is sorry" />
			
			<actor
				name="body_1"
				x="82"
				y="250"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_medium_large"
				optionFlags="Text"
				text="This kiosk is not in service at this time." />
				
			<actor
				name="body_2"
				x="82"
				y="350"
				width="918"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_medium_large"
				optionFlags="Text"
				text="Call 1.866.REDBOX3 or text FIND to 727272" />

			<actor
				name="body_3"
				x="82"
				y="450"
				width="918"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_medium_large"
				optionFlags="Text"
				text="for another kiosk location*." />
				
			<actor
				name="subtext"
				x="82"
				y="550"
				foregroundColor="white"
				verticalAlignment="center"
				font="font_target_smaller"
				optionFlags="Text"
				text="*Message and data rates may apply. Text STOP to quit. HELP for help." />
				
			<actor
				name="store_label"
				x="70"
				y="680"
				font="font_target_small"
				optionFlags="Text" 
				foregroundColor="white"
				text="Kiosk ID: ${StoreNumber}    |" />
				
			<actor
				name="zip_code"
				x="280"
				y="680"
				font="font_target_small"
				optionFlags="Text"
				foregroundColor="white"
				text="ZIP code: ${MaintenanceView:ZipCode}    |" />
				
			<actor
				name="error_code_label"
				x="500"
				y="680"
				width="900"
                height="64"
				font="font_target_small"
				optionFlags="Text"
				foregroundColor="white"
				text="Machine code: ${MaintenanceView:ErrorCode}" />
				
				
		</scene>
	</aspect>
</resource>