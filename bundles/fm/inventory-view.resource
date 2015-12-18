<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="inventory_view" />
  <aspect name="content">
    <scene
      name="Inventory"
      width="1024"
      height="768"
      onEnter="inventory_view_enter"
	  onLeave="inventory_view_leave">

      <actor
        name="title_arrow"
        x="0"
        y="0"
		optionFlags="Background, Image"
		backgroundColor="DarkRed"
        verticalAlignment="center"
        width="45"
        height="50"
		image="title_arrow"
        visible="true"
		tag="export_inventory"
		onHit="export_inventory"
		hitFlags="Enabled"
		/>

      <actor
        name="title_area"
        x="45"
        y="0"
		optionFlags="Background, Text, TextShadow"
		backgroundColor="DarkRed"
		text=" Inventory Management -"
		foregroundColor="White"
        verticalAlignment="center"
		font="medium_larger_font"
        width="895"
        height="50"
        visible="true"
		/>

	  <actor
	    name="title_kiosk_id"
	    x="460"
	    y="0"
	    optionFlags="Background, Text, TextShadow"
		backgroundColor="DarkRed"
		text="${StoreNumber}"
		foregroundColor="White"
        verticalAlignment="center"
		font="medium_larger_font"
        width="490"
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
		onHit="go_back"
		hitFlags="Enabled"
		optionFlags="Background, Text" />
		
      <actor
        name="inventory_filter"
        x="915"
        y="49"
        width="38"
        height="27"
        optionFlags="Image"
        image="inventory_filter_indicator"
        visible="true"
        enabled="true" />
		
      <actor
		name="tabs_background"
		x="17"
		y="75"
		width="990"
		height="580"
		optionFlags="Image"
		image="inventory_tabs"
		visible="true"
		enabled="true" />
		
      <actor
        name="unknown_filter"
        x="20"
        y="76"
        height="32"
        width="136"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Unknown"
        tag="Unknown"
        hitFlags="Enabled"
        onHit="inventory_filter_onhit"
        optionFlags= "Text, TextShadow" />
		
      <actor
        name="damaged_filter"
        x="157"
        y="76"
        height="32"
        width="140"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Damaged"
        tag="Damaged"
        hitFlags="Enabled"
        onHit="inventory_filter_onhit"
        optionFlags=" Text, TextShadow" />
		
      <actor
        name="unmatched_filter"
        x="298"
        y="76"
        height="32"
        width="140"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Unmatched"
        tag="Unmatched"
        hitFlags="Enabled"
        onHit="inventory_filter_onhit"
        optionFlags=" Text, TextShadow" />
		
      <actor
        name="redeploy_filter"
        x="440"
        y="76"
        height="32"
        width="138"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Redeploy"
        tag="Redeployment"
        hitFlags="Enabled"
        onHit="inventory_filter_onhit"
        optionFlags=" Text, TextShadow" />
		
      <actor
        name="thinned_filter"
        x="581"
        y="76"
        height="32"
        width="140"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Thinned"
        tag="Thinned"
        hitFlags="Enabled"
        onHit="inventory_filter_onhit"
        optionFlags=" Text, TextShadow" />
		
      <actor
        name="title_unknown_filter"
        x="721"
        y="76"
        height="32"
        width="140"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Unknown Title"
        tag="Unknown Title"
        hitFlags="Enabled"
        onHit="inventory_filter_onhit"
        optionFlags=" Text, TextShadow" />
		
      <actor
        name="all_filter"
        x="864"
        y="76"
        height="32"
        width="140"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="ALL"
        tag="ALL"
        hitFlags="Enabled"
        onHit="inventory_filter_onhit"
        optionFlags="Text, TextShadow" />
		
      <actor
		name="select_button"
		x="20"
		y="113"
		width="130"
		height="35"
		font="medium_large_font"
		foregroundColor="white"
		verticalAlignment="center"
		horizontalAlignment="center"
		text="Select All"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="5,5"
        optionFlags="Background, Text, TextShadow, DrawBorder"
		hitFlags="Enabled"
		onHit="select_discs"
		enabled="true"
		tag="all"/>
      
      <actor
		name="title_label"
		x="197"
		y="116"
		width="100"
		height="31"
		font="medium_font"
		foregroundColor="white"
		verticalAlignment="center"
		horizontalAlignment="near"
		text="Title"
		optionFlags=" Text, TextShadow"
		hitFlags="Enabled"
		onHit="sort_by_title"
		enabled="true"
		tag="sort_by_title"/>
		
	  <actor
		name="title_sort_arrow"
		x="235"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
       <actor
        name="rating_label"
        x="362"
        y="116"
        width="100"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Rating"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_rating"
        enabled="true"
        tag="sort_by_rating"/>
		
		<actor
		name="rating_sort_arrow"
		x="417"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
      <actor
        name="media_label"
        x="434"
        y="116"
        width="86"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Media"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_media"
        tag="sort_by_media"/>
		
		<actor
		name="media_sort_arrow"
		x="486"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
      <actor
        name="deck_label"
        x="500"
        y="116"
        width="50"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Deck"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_deck_slot"
        tag="sort_by_deck_slot"/>
      
	  <actor
        name="slot_label"
        x="548"
        y="116"
        width="40"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Slot"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_deck_slot"
        tag="sort_by_deck_slot" />
		
	  <actor
		name="deck_slot_sort_arrow"
		x="580"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
      <actor
        name="barcode_label"
        x="589"
        y="116"
        width="75"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Barcode"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_barcode"
        tag="sort_by_barcode"/>
		
	  <actor
		name="barcode_sort_arrow"
		x="662"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
      <actor
        name="releasedate_label"
        x="680"
        y="116"
        width="110"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Released"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_release_date"
        tag="sort_by_release_date"/>
		
	  <actor
		name="releasedate_sort_arrow"
		x="775"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
      <actor
        name="rented_label"
        x="810"
        y="116"
        width="61"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="center"
        text="Rented"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_rented"
        tag="sort_by_rented"/>
		
	  <actor
		name="rented_sort_arrow"
		x="871"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
      <actor
        name="status_label"
        x="910"
        y="116"
        width="70"
        height="31"
        font="medium_font"
        foregroundColor="white"
        verticalAlignment="center"
        horizontalAlignment="near"
        text="Status"
        optionFlags=" Text, TextShadow"
        hitFlags="Enabled"
        onHit="sort_by_status"
        tag="sort_by_status"/>
		
	  <actor
		name="status_sort_arrow"
		x="966"
		y="127"
		width="11"
		height="11"
		verticalAlignment="center"
		optionFlags=" Image"
		hitFlags="None"
		visible="false"
		image="sort_asc"
		enabled="true" />
		
      <actor
        name="sort_scheme"
        x="50"
        y="630"
        width="800"
        height="35"
        font="small_font"
        foregroundColor="red"
        verticalAlignment="near"
        horizontalAlignment="near"
        optionFlags="Text"/>
		
      <actor
		name="selected_status"
		x="495"
		y="630"
		width="200"
		height="35"
		font="medium_font"
		foregroundColor="red"
		verticalAlignment="near"
		horizontalAlignment="near"
		optionFlags="Text"/>
      
      <actor
        name="count_label"
        x="658"
        y="630"
        width="320"
        height="35"
        font="medium_font"
        foregroundColor="red"
        verticalAlignment="near"
        horizontalAlignment="far"
        optionFlags="Text"/>
		
      <actor
		name="footer"
		x="0"
		y="674"
		width="1024"
		height="94"
		backgroundColor="64,64,64"
		visible="true"
		enabled="true" 
		optionFlags="Background"/>
		
      <actor
        name="left_arrow"
        x="800"
        y="690"
        foregroundColor="gray"
        image="left_arrow_disabled"
        font="medium_font"
        text="Back"
        textRegion="35,20,120,16"
        optionFlags="Image, Text, TextShadow"
        hitFlags="Enabled"
        priority="4"
        enabled="False"
        onHit="inventory_page_back" />
		
      <actor
        name="right_arrow"
        x="900"
        y="690"
        foregroundColor="gray"
        image="right_arrow_disabled"
        font="medium_font"
        text="Next"
        textRegion="15,20,120,16"
        optionFlags="Image, Text, TextShadow"
        hitFlags="Enabled"
        priority="6"
        enabled="False"
        onHit="inventory_page_next" />
		
      <actor
        name="remove_disc_button"
        x="20"
        y="695"
        width="145"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="gray"
        text="Remove Discs"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="8,8"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        hitFlags="Enabled"
        enabled="False" />
		
      <actor
        name="insert_disc_button"
        x="175"
        y="695"
        width="145"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Insert Disc"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="8,8"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        hitFlags="Enabled"
        enabled="True"
        onHit="inventory_insert_disc"/>
		
      <actor
        name="multi_disc_insert_button"
        x="330"
        y="695"
        width="145"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Multi-Disc&#10;Insert"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="8,8"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        hitFlags="Enabled"
        enabled="True"
        onHit="inventory_multi_disc_insert"/>
		
      <actor
        name="refresh_inventory_button"
        x="485"
        y="695"
        width="145"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Refresh"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="8,8"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        hitFlags="Enabled"
        enabled="True"
        onHit="refresh_inventory"/>
		
	<actor
        name="letter_navigation_button"
        x="640"
        y="695"
        width="145"
        height="58"
        font="medium_font"
        verticalAlignment="center"
        horizontalAlignment="center"
        foregroundColor="white"
        text="Filter Titles"
		backgroundColor="DarkRed"
		strokeWeight="2.0"
		borderColor="White"
		cornerSize="8,8"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        hitFlags="Enabled"
        enabled="True"
        onHit="show_filter_alpha_popup"/>	

     <actor
        name="no_inventory"
        x="0"
        y="0"
        width="600"
        height="200"
        font="larger_font"
        optionFlags="text"
        foregroundColor="red"
        verticalAlignment="center"
        horizontalAlignment="center"
        visible="false"
        text="Machine Empty" />

     </scene>
  </aspect>
</resource>
