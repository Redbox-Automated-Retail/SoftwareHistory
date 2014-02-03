<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="vmz_view" />
  <aspect name="content">
    <scene name="VMZ"
      width="1024"
      height="768"
      onEnter="qlm_enter"
	  onLeave = "qlm_view_leave" >


      <actor
        name="title_arrow"
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
        visible="true"
		/>

      <actor
        name="title_area"
        x="45"
        y="0"
		optionFlags="Background, Text, TextShadow"
		backgroundColor="DarkRed"
		text="  VMZ Management"
		foregroundColor="White"
        verticalAlignment="center"
		font="medium_larger_font"
        width="895"
        height="50"
        visible="true"
		/>

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
		onHit="qlm_go_back"
		hitFlags="Enabled"
		optionFlags="Background, Text"
		/>

      <actor
		name="grid_background"
		x="17"
		y="60"
		width="990"
		height="343"
		optionFlags="Image"
		image="job_grid_background"
		visible="true"
		enabled="true" />

      <actor
		name="job_id_header"
		x="90"
		y="80"
		width="65"
		height="32"
		verticalAlignment="center"
		horizontalAlignment="near"
		foregroundColor="white"
		text="Job Id"
		optionFlags="Text, TextShadow"
		visible="true"
		enabled="true" />

      <actor
		name="label_header"
		x="180"
		y="80"
		width="65"
		height="32"
		verticalAlignment="center"
		horizontalAlignment="near"
		foregroundColor="white"
		text="Label"
		optionFlags="Text, TextShadow"
		visible="true"
		enabled="true" />

      <actor
		name="status_header"
		x="500"
		y="80"
		width="65"
		height="32"
		verticalAlignment="center"
		horizontalAlignment="near"
		foregroundColor="white"
		text="Status"
		optionFlags="Text, TextShadow"
		visible="true"
		enabled="true" />

      <actor
		name="start_time_header"
		x="615"
		y="80"
		width="100"
		height="32"
		verticalAlignment="center"
		horizontalAlignment="near"
		foregroundColor="white"
		text="Start Time"
		optionFlags="Text, TextShadow"
		visible="true"
		enabled="true" />

      <actor
		name="execution_time_header"
		x="836"
		y="80"
		width="125"
		height="32"
		verticalAlignment="center"
		horizontalAlignment="near"
		foregroundColor="white"
		text="Execution Time"
		optionFlags="Text, TextShadow"
		visible="true"
		enabled="true" />

      <actor
        name="refresh_button"
        x="40"
        y="412"
        width="118"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Refresh"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        onHit="grid_refresh"
        hitFlags="Enabled"
        enabled="True" />

      <actor
        name="resume_button"
        x="178"
        y="412"
        width="118"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Resume"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        onHit="resume_job"
        hitFlags="Enabled"
        enabled="True"
        tag="QLM"/>

      <actor
		name="suspend_button"
		x="316"
		y="412"
		width="118"
		height="58"
		font="medium_font"
		verticalAlignment="center"
		horizontalAlignment="center"
		foregroundColor="white"
		text="Suspend"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
		onHit="suspend_job"
		hitFlags="Enabled"
		enabled="True"
		tag="QLM"/>

      <actor
        name="delete_button"
        x="454"
        y="412"
        width="118"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Trash"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        onHit="trash_job"
        hitFlags="Enabled"
        enabled="True" 
        tag="QLM"/>

      <actor
        name="left_arrow_button"
        x="760"
        y="410"
        width="99"
        height="75"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="gray"
        optionFlags="Image, Text, TextShadow"
        textRegion="36,15,56,40"
        text="Back"
        image="left_arrow_disabled"
        hitFlags="Enabled"
        onHit="grid_back"
        enabled="True" />

      <actor
        name="right_arrow_button"
        x="884"
        y="410"
        width="99"
        height="75"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="gray"
        optionFlags="Image, Text, TextShadow"
        textRegion="6,15,56,40"
        text="Next"
        image="right_arrow_disabled"
        hitFlags="Enabled"
        onHit="grid_next"
        enabled="True" />

      <actor
		name="schedule_options_background"
		x="17"
		y="480"
		width="990"
		height="190"
		optionFlags="Image"
		image="schedule_options_background"
		visible="true"
		enabled="true" />

      <actor
        name="info_label"
        x="93"
        y="505"
        width="475"
        height="50"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        optionFlags="Text" />

      <actor
        name="show_schedule"
        x="85"
        y="572"
        width="118"
        height="63"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Start Time"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        hitFlags="Enabled"
        onHit="show_datetime_popup"
        enabled="True" />

      <actor
        name="schedule_value"
        x="240"
        y="591"
        width="300"
        height="25"
        font="medium_large_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        text="Immediate"
        optionFlags="Text" />

      <actor
        name="dump_bin_label"
        x="85"
        y="510"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        text=""
        optionFlags="Text" />

      <actor
        name="last_thin_label"
        x="85"
        y="530"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        text="Loading VMZ Data..."
        optionFlags="Text" />

      <actor
        name="thins_outside_vmz_label"
        x="85"
        y="550"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        text=""
        optionFlags="Text" />

      <actor
        name="unload_checkbox"
        x="638"
        y="516"
        width="51"
        height="47"
        optionFlags="Image"
        image="check_box"
        hitFlags="Enabled"
        onHit="num_newrelease_decks"
        enabled="True" />

      <actor
        name="unload_label"
        x="700"
        y="527"
        width="210"
        height="25"
        font="medium_large_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        text="Unload VMZ decks"
        optionFlags="Text" />

      <actor
        name="num_newrelease_decks_textbox"
        x="920"
        y="527"
        width="60"
        height="25"
        font="medium_large_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        text="${keyboard:newrelease_decks}"
        optionFlags="Text" />

      <actor
        name="thin_checkbox"
        x="638"
        y="581"
        width="51"
        height="47"
        optionFlags="Image"
        image="check_box"
        hitFlags="Enabled"
        onHit="select_vmz_thin_checkbox"
        enabled="True" />

      <actor
        name="thin_label"
        x="700"
        y="592"
        width="300"
        height="25"
        font="medium_large_font"
        verticalAlignment="center"
        horizontalAlignment="near"
        foregroundColor="black"
        text="Thin Kiosk"
        optionFlags="Text" />

      <actor
		name="footer"
		x="0"
		y="676"
		width="1024"
		height="92"
		backgroundColor="64,64,64"
		visible="true"
		enabled="true" 
		optionFlags="Background"/>

      <actor
        name="launch_button"
        x="20"
        y="691"
        width="150"
        height="60"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Merchandize VMZ"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        onHit="launch_vmz_screen"
        hitFlags="Enabled"
        enabled="True" />

      <actor
        name="schedule_button"
        x="855"
        y="691"
        width="144"
        height="60"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Schedule"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        onHit="schedule_qlm"
        hitFlags="Enabled"
        enabled="True" />

    </scene>
  </aspect>
</resource>
