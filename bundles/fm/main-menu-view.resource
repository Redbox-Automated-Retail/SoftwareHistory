<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="main_menu_view" />
	<aspect name="content">
		<scene
			name="Menu"
			width="1024"
			height="768"
			onEnter="main_menu_view_enter"
			onLeave="main_menu_view_leave">

			<actor
				name="title_arrow_label"
				x="0"
				y="0"
				optionFlags="Background, Image, Text, TextShadow"
				backgroundColor="DarkRed"
				foregroundColor="White"
				verticalAlignment="center"
				font="medium_larger_font"
				width="45"
				height="50"
				image="title_arrow"
				visible="true" />

      <actor
				name="title_area"
				x="45"
				y="0"
				optionFlags="Background, Text, TextShadow"
				backgroundColor="DarkRed"
				text="  Field Maintenance Application"
				foregroundColor="White"
				verticalAlignment="center"
				font="medium_larger_font"
				width="895"
				height="50"
				visible="true" />

      <actor
        name="title_area_kiosk_id"
        x="635"
        y="0"
        optionFlags="Background, Text, TextShadow"
        backgroundColor="DarkRed"
        text="Kiosk ID: ${KioskID}"
        foregroundColor="White"
        horizontalAlignment="far"
        verticalAlignment="center"
        font="medium_larger_font"
        width="285"
        height="50" />

      <actor
				name="close_area"
				x="940"
				y="0"
				width="84"
				height="50"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="White"
				backgroundColor="192,192,192"
				font="medium_larger_font"
				text= "X"
				tag="logout"
				onHit="activate_rental"
				hitFlags="Enabled"
				optionFlags="Background, Text" />

			<actor
				name="kiosk_stats_label"
				x="50"
				y="80"
				width="267"
				height="20"
				text="Kiosk Statistics"
				font="medium_large_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />

			<actor
				name="kiosk_stats_group"
				x="50"
				y="110"
				width="267"
				height="295"
				backgroundColor="Gray"
				strokeWeight="1.0"
				borderColor="Gray"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true" />

			<actor
				name="rented_discs_label"
				x="56"
				y="120"
				width="200"
				height="20"
				text="Rented Discs:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="false"
				enabled="true" />

			<actor
				name="rented_discs_value"
				x="266"
				y="120"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="false"
				enabled="true" />

			<actor
				name="empty_slots_label"
				x="56"
				y="142"
				width="200"
				height="20"
				text="Empty Slots:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="empty_slots_value"
				x="266"
				y="142"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="empty_case_count_label"
				x="56"
				y="164"
				width="200"
				height="20"
				text="Empty Cases:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="empty_case_count_value"
				x="266"
				y="164"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="unknown_count_label"
				x="56"
				y="186"
				width="200"
				height="20"
				text="Unknown Count:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="unknown_count_value"
				x="266"
				y="186"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="thins_outside_vmz_label"
				x="56"
				y="208"
				width="200"
				height="20"
				text="Thins Outside VMZ:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="thins_outside_vmz_value"
				x="266"
				y="208"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="thin_redeploys_outside_vmz_label"
				x="56"
				y="230"
				width="200"
				height="20"
				text="Redeploys Outside VMZ:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="thin_redeploys_outside_vmz_value"
				x="266"
				y="230"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="fraud_discs_count_label"
				x="56"
				y="252"
				width="200"
				height="20"
				text="Fraud Discs:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="fraud_discs_count_value"
				x="266"
				y="252"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="DNR_disc_count_label"
				x="56"
				y="274"
				width="200"
				height="20"
				text="Do Not Rent Discs:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="DNR_disc_count_value"
				x="266"
				y="274"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="damaged_discs_count_label"
				x="56"
				y="296"
				width="200"
				height="20"
				text="Damaged Discs:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="damaged_discs_count_value"
				x="266"
				y="296"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="distinct_title_count_label"
				x="56"
				y="318"
				width="200"
				height="20"
				text="Distinct Titles:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="distinct_title_count_value"
				x="266"
				y="318"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="reserved_disc_count_label"
				x="56"
				y="340"
				width="200"
				height="20"
				text="Reserved Discs:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="reserved_disc_count_value"
				x="266"
				y="340"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="rebalance_disc_count_label"
				x="56"
				y="362"
				width="200"
				height="20"
				text="Rebalance Discs:"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="far"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="rebalance_disc_count_value"
				x="266"
				y="362"
				width="125"
				height="20"
				text=" "
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="scheduled_jobs_label"
				x="50"
				y="420"
				width="267"
				height="20"
				text="Scheduled Jobs"
				font="medium_large_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />

			<actor
				name="scheduled_jobs_group"
				x="50"
				y="450"
				width="267"
				height="90"
				backgroundColor="Gray"
				strokeWeight="1.0"
				borderColor="Gray"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true" />

			<actor
				name="qlm_unload_time_label"
				x="60"
				y="460"
				width="200"
				height="20"
				text="Unload:"
				font="small_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="qlm_unload_time_value"
				x="150"
				y="460"
				width="200"
				height="20"
				text="n/a"
				font="small_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="qlm_thin_time_label"
				x="60"
				y="485"
				width="200"
				height="20"
				text="Thin:"
				font="small_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="qlm_thin_time_value"
				x="150"
				y="485"
				width="200"
				height="20"
				text="n/a"
				font="small_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="inv_sync_time_label"
				x="60"
				y="510"
				width="200"
				height="20"
				text="Inv Sync:"
				font="small_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="inv_sync_time_value"
				x="150"
				y="510"
				width="200"
				height="20"
				text="n/a"
				font="small_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<!-- Maintenance Mode Button -->
			<actor
				name="maintenance_mode_button"
				x="50"
				y="580"
				width="267"
				height="70"
				text="Turn On &#10;Maintenance Mode"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="toggle_maintenance_mode" />

			<!-- Control Panel Log In Button -->
			<actor
				name="control_panel_login_button"
				x="50"
				y="675"
				width="130"
				height="70"
				text="Control Panel Log In"
				font="medium_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="activate_cp" />

			<!-- Software Version Info Button -->
			<actor
				name="software_version_info_button"
				x="188"
				y="675"
				width="130"
				height="70"
				text="Software&#10;Version Info"
				font="medium_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="show_software_version_info" />

			<actor
				name="send_inventory_alert_message"
				x="640"
				y="744"
				width="400"
				height="24"
				text="* Use this to report insufficient empty slots to load NR's."
				font="font_tahoma_bold_10"
				verticalAlignment="center"
				horizontalAlignment="near"
				foregroundColor="black"
				optionFlags="Text"
				visible="true"
				enabled="true" />

<!-- MERCHANDISE BUTTONS -->

			<actor
				name="merchandise_label"
				x="360"
				y="80"
				width="170"
				height="20"
				text="Merchandise"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />

			<!-- Remove Unknowns Button -->
			<actor
				name="remove_unknowns_button"
				x="360"
				y="110"
				width="170"
				height="90"
				text="Remove&#10;Unknowns"
				tag="remove_unknowns"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="show_unknown_discs" />

			<!-- Remove Frauds Button -->
			<actor
				name="remove_frauds_button"
				x="360"
				y="220"
				width="170"
				height="90"
				text="Remove&#10;Frauds"
				tag="remove_frauds"
				font="medium_large_font"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				horizontalAlignment="center"
				verticalAlignment="center"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				enabled="true"
				visible="true"
				onHit="remove_frauds"
				hitFlags="Enabled" />

			<!-- Read Fraud Disc Button -->
			<actor
				name="read_fraud_disc_button"
				x="360"
				y="330"
				width="170"
				height="90"
				text="Mark As&#10;Fraud Disc"
				tag="read_fraud_disc"
				font="medium_large_font"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				horizontalAlignment="center"
				verticalAlignment="center"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				enabled="true"
				visible="true"
				onHit="read_fraud_disc"
				hitFlags="Enabled" />
			
			<!-- Inventory Management Button -->
			<actor
				name="inv_mgmt_button"
				x="360"
				y="440"
				width="170"
				height="90"
				text="Inventory&#10;Management"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="show_inventory_mgt"
				/>

<!-- JOB SCHEDULING BUTTONS -->

			<actor
				name="job_scheduling_label"
				x="570"
				y="80"
				width="170"
				height="20"
				text="Job Scheduling"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />

			<!-- QLM/VMZ Button -->
			<actor
				name="inv_qlm_button"
				x="570"
				y="110"
				width="170"
				height="90"
				text="QLM/VMZ"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				onHit="show_qlm"
				visible="true"
				enabled="true"
				hitFlags="Enabled" />

			<!-- Inventory Sync Button -->
			<actor
				name="inv_sync_button"
				x="570"
				y="220"
				width="170"
				height="90"
				text="Inventory&#10;Sync"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="show_inventory_sync" />

<!-- MAINTENANCE BUTTONS -->

			<actor
				name="maintenance_label"
				x="780"
				y="80"
				width="170"
				height="20"
				text="Maintenance"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />

			<!-- HAL Tester Button -->
			<actor
				name="hal_tester_button"
				x="780"
				y="110"
				width="170"
				height="90"
				text="HAL Tester"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="show_hal_tester" />

			<!-- Secure Browser Button-->
			<actor
				name="secure_browser_button"
				x="780"
				y="220"
				width="170"
				height="90"
				text="Secure&#10;Browser"
				tag="secure_browser"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				onHit="perform_activity"
				visible="true"
				enabled="true"
				hitFlags="Enabled" />

			<!-- Send Inventory Alert Button-->
			<actor
				name="send_inventory_alert_button"
				x="780"
				y="330"
				width="170"
				height="90"
				text="Send Inventory&#10;Alert*"
				font="font_tahoma_bold_14"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true"
				hitFlags="Enabled"
				onHit="activate_ia" />

			<!-- Door Status button-->
			<actor
				name="door_status_button"
				x="780"
				y="440"
				width="170"
				height="90"
				text="Door Status:&#10;Checking..."
				tag="door_status"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				onHit="check_door_status"
				visible="false"
				enabled="true"
				hitFlags="Enabled" />

			<!-- Air Exchanger Fan button-->
			<actor
				name="air_exchanger_button"
				x="780"
				y="550"
				width="170"
				height="90"
				text="Turn off&#10;Air Exchanger Fan"
				tag="air_exchanger"
				font="medium_font"
				verticalAlignment="center"
				horizontalAlignment="center"
				foregroundColor="white"
				backgroundColor="DarkRed"
				strokeWeight="3.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="false"
				enabled="true"
				hitFlags="Enabled" />
		</scene>
	</aspect>
</resource>
