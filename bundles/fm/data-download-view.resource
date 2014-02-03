<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="data_download_view" />
  <aspect name="content">
    <scene name="Datadownload"
      width="1024"
      height="768"
      onEnter="data_download_view_enter"
	  onLeave = "data_download_view_leave" >

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
		text="  Data Download"
		foregroundColor="White"
        verticalAlignment="center"
		font="medium_larger_font"
        width="895"
        height="50"
        visible="true"
		/>

	  <actor
		name="close_button"
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
		onHit="go_back"
		hitFlags="Enabled"
		optionFlags="Background, Text"
		/>
		
	  <actor
		name="grid_caption"
		x="45"
		y="65"
		width="200"
		height="30"		
		text="CURRENT DOWNLOADS"
		font="small_tiny_font"
		backgroundColor="152,152,152"
		foregroundColor="black"		
		optionFlags="Background,Text"
		visible="true"
		/>
		
	  <actor
		name="grid_header"
		x="22"
		y="80"
		width="975"
		height="40"
		backgroundColor="136,136,136"
		cornerSize="10,10"
		optionFlags="Background"
		/>
		
		
	  <actor
		name="grid_header_job_name"
		x="40"
		y="90"
		width="150"
		height="30"
		text="Job Name"
		font="medium_font"
		foregroundColor="white"
		backgroundColor="136,136,136"
		optionFlags="Background,Text"
		Visible="true"
		/>
		
	   <actor
		name="grid_header_status"
		x="235"
		y="90"
		width="120"
		height="30"
		text="Status"
		font="medium_font"
		foregroundColor="white"
		backgroundColor="136,136,136"
		optionFlags="Background,Text"
		Visible="true"
		/>
		
	   <actor
		name="grid_header_job_type"
		x="335"
		y="90"
		width="120"
		height="30"
		text="Job Type"
		font="medium_font"
		foregroundColor="white"
		backgroundColor="136,136,136"
		optionFlags="Background,Text"
		Visible="true"
		/>
		
	  		
	  <actor
		name="grid_header_started"
		x="430"
		y="90"
		width="120"
		height="30"
		text="Started"
		font="medium_font"
		foregroundColor="white"
		backgroundColor="136,136,136"
		optionFlags="Background,Text"
		Visible="true"
		/>
		
	  <actor
		name="grid_header_finished"
		x="570"
		y="90"
		width="120"
		height="30"
		text="Finished"
		font="medium_font"
		foregroundColor="white"
		backgroundColor="136,136,136"
		optionFlags="Background,Text"
		Visible="true"
		/>
		
	  <actor
		name="grid_header_total_bytes"
		x="690"
		y="90"
		width="120"
		height="30"
		text="Total Bytes"
		font="medium_font"
		foregroundColor="white"
		backgroundColor="136,136,136"
		optionFlags="Background,Text"
		Visible="true"
		/>
		
	  <actor
		name="grid_header_progress"
		x="820"
		y="90"
		width="150"
		height="30"
		text="Progress"
		font="medium_font"
		foregroundColor="white"
		backgroundColor="136,136,136"
		optionFlags="Background,Text"
		Visible="true"
		/>
		
	  <actor
		name="up_down_arrows_back_ground"
		x="959"
		y="120"
		width="40"
		height="420"
		backgroundColor="96,96,96"
		cornerSize="10,10"
		borderColor="black"
		strokeWeight="2.0"	
		opacity="0.50"
		optionFlags="Background,DrawBorder"
		visible="true"
		/>
		
	  <actor
		name="up_arrow_back_ground"		
		x="959"
		y="120"
		width="40"
		height="70"
		cornerSize="10,10"
		borderColor="black"
		backgroundColor="96,96,96"
		optionFlags="Background,DrawBorder"
		/>
		
	   <actor
		name="image_up_arrow"
		x="959"
		y="130"
		width="50"
		height="50"
		image="up_arrow"
		optionFlags="Image"
		hitFlags="Enabled"
		onHit="page_back"
		/>
		
	  <actor
		name="down_arrow_back_ground"		
		x="959"
		y="470"
		width="40"
		height="70"
		cornerSize="10,10"
		borderColor="black"
		backgroundColor="96,96,96"
		optionFlags="Background,DrawBorder"
		/>
		
	  <actor
		name="image_down_arrow"
		x="960"
		y="490"
		width="50"
		height="50"
		image="down_arrow"		
		optionFlags="Image"
		hitFlags="Enabled"
		onHit="page_next"
		/>
		
		
      <actor
		name="grid_back_ground"
		x="22"
		y="120"
		width="935"
		height="420"
		backgroundColor="white"	
		cornerSize="10,10"
		optionFlags="Background"
		visible="true"
		/>  
      
      <actor
        name="check_for_updates_button"
        x="20"
        y="586"
        width="165"
        height="65"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Check for Updates"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        hitFlags="Enabled"
        enabled="True" 
		/>
 
	  <actor
		name="last_server_data_update_caption"
		x="220"
		y="570"
		width="520"
		height="50"
		font="small_tiny_font"
		text="LAST SERVER DATA UPDATE FILE BUILD"
		foregroundColor="black"
		backgroundColor="152,152,152"
		optionFlags="Background,Text"
		visible="true"
		/>

	  <actor
        name="last_server_data_update_value"
        x="210"
        y="586"
        width="615"
        height="65"
        font="medium_font"
        verticalAlignment="center"
        foregroundColor="black"
		backgroundColor="152,152,152"
		strokeWeight="2.0"
		borderColor="black"
		cornerSize="10,10"
		textRegion="10,0,506,65"
        optionFlags="Background, Text, DrawBorder"
        enabled="True" 
		/>

      <actor
        name="show_errors_button"
        x="839"
        y="586"
        width="165"  
        height="65"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="gray"
        text="Show Errors"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"        
        hitFlags="Enabled"
        enabled="True" 
		
		/>

      <actor
		name="footer"
		x="0"
		y="676"
		width="1024"
		height="92"
		backgroundColor="64,64,64"
		visible="true"
		enabled="true" 
		optionFlags="Background"
		/>

      <actor
        name="force_data_updates_button"
        x="839"
        y="691"
        width="165"
        height="65"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Force Data Updates"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="10,10"
        optionFlags="Background, Text, TextShadow, DrawBorder"        
        hitFlags="Enabled"
        enabled="True" 
		/>

    </scene>
  </aspect>
</resource>
