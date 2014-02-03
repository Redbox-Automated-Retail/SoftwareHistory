<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="vmz-merch-ui" />
	<aspect name="content">
		<scene
			name="VMZMerchStart"
			width="1024"
			height="768"
			onEnter="vmz_merch_ui_enter"
			onLeave="vmz_merch_ui_leave">

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
				text="  Merchandize VMZ"
				foregroundColor="White"
				verticalAlignment="center"
				font="medium_larger_font"
				width="1024"
				height="50"
				visible="true" />

			<!-- group 1 -->
			<actor
				name="group1_label"
				x="50"
				y="60"
				width="267"
				height="20"
				text="Step 1: Empty the bin"
				font="medium_large_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />
			
			<actor
				name="step1_group"
				x="20"
				y="90"
				width="984"
				height="190"
				backgroundColor="Gray"
				strokeWeight="1.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true" />

			<actor
				name="step1_label1"
				x="50"
				y="100"
				width="400"
				height="20"
				text="* Remove bin and empty disks"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="step1_label2"
				x="50"
				y="130"
				width="400"
				height="20"
				text="* Re-insert bin into kiosk"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="step1_label3"
				x="50"
				y="160"
				width="800"
				height="20"
				text="* Press 'Continue' to reset counter and center drum to zone"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="step1_label4"
				x="50"
				y="190"
				width="400"
				height="20"
				text="** WARNING: DRUM WILL ROTATE **"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />

			<actor
				name="reset_bin_counter_button"
				x="714"
				y="180"
				width="280"
				height="90"
				text="Continue"
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
				/>

			
			<!-- group 2 -->
			<actor
				name="group2_label"
				x="50"
				y="300"
				width="800"
				height="20"
				text="Step 2: Clear Thins From Drum"
				font="medium_large_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />
			
			<actor
				name="step2_group"
				x="20"
				y="330"
				width="984"
				height="190"
				backgroundColor="Gray"
				strokeWeight="1.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true" />

			<actor
				name="step2_label1"
				x="50"
				y="350"
				width="800"
				height="20"
				text="* From the last thin disk, remove thins to the right and down"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="step2_label2"
				x="50"
				y="380"
				width="800"
				height="20"
				text="* To see the last thin, click 'Pull Last Thin'"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="step2_label3"
				x="50"
				y="410"
				width="400"
				height="20"
				text="** WARNING: DRUM WILL ROTATE **"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="pull_last_thin_label"
				x="384"
				y="420"
				width="610"
				height="90"
				font="medium_large_font"
				verticalAlignment="center"
				horizontalAlignment="far"
				foregroundColor="white"
				optionFlags="Text, TextShadow"
				visible="true"
				enabled="true" />
			
			<!-- group 3 -->
			<actor
				name="group3_label"
				x="50"
				y="540"
				width="800"
				height="20"
				text="Step 3: Load New Releases"
				font="medium_large_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text"
				foregroundColor="black"
				visible="true"
				enabled="true" />
			
			<actor
				name="step3_group"
				x="20"
				y="570"
				width="984"
				height="178"
				backgroundColor="Gray"
				strokeWeight="1.0"
				borderColor="White"
				cornerSize="15,15"
				optionFlags="Background, Text, TextShadow, DrawBorder"
				visible="true"
				enabled="true" />

			<actor
				name="step3_label1"
				x="50"
				y="580"
				width="800"
				height="20"
				text="* Insert disks into the zone, starting from the top, working right and down"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="step3_label2"
				x="50"
				y="610"
				width="800"
				height="20"
				text="When finished, click 'Close' to return to the previous screen"
				font="medium_font"
				verticalAlignment="near"
				horizontalAlignment="near"
				optionFlags="Text, TextShadow"
				foregroundColor="white"
				visible="true"
				enabled="true" />
				
			<actor
				name="view_unload_diagram_button"
				x="384"
				y="648"
				width="280"
				height="90"
				text="View&#10;Unload&#10;Diagram"
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
				/>
				
			<actor
				name="close_screen_button"
				x="714"
				y="648"
				width="280"
				height="90"
				text="Close"
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
				onHit="go_back"
				/>
		</scene>
	</aspect>
</resource>
