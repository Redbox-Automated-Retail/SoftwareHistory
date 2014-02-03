<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="filter_alpha_popup_view" />
  <aspect name="content">
    <scene name="filteralphaPopup"
      clear="false"
      onEnter="filter_alpha_popup_enter"
	  onLeave="filter_alpha_popup_leave"
      viewWindow="65,63,900,650" >

 		<actor
		name="background"
		x="0"
		y="0"
		width="900"
		height="650"
		backgroundColor="Gray"
		strokeWeight="3.0"
		borderColor="192,192,192"
		cornerSize="10,10"
		optionFlags="Background, DrawBorder"
        hitFlags="Enabled, Obstruction" />

		<actor
		name="title_area"
		x="2"
		y="2"
		optionFlags="Background, Text, TextShadow, DrawRoundedCorners"
		backgroundColor="DarkRed"
		text=" Filter Titles "
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font"
		cornerSize="10,10"
		width="895"
		height="50" />

		<actor
		name="close_area"
		x="823"
		y="0"
		width="77"
		height="52"
		verticalAlignment="center"
		horizontalAlignment="center"
		foregroundColor="White"
		backgroundColor="192,192,192"
		cornerSize="10,10"
		font="medium_larger_font"
		text= "X"
		onHit="go_back"
		hitFlags="Enabled"
		optionFlags="Background, Text, DrawRoundedCorners" />
	
<!-- Media Type -->
	
	<actor
		name="media_type_background"
		x="34"
		y="63"
		width="290"
		height="187"
		backgroundColor="192,192,192"
		borderColor="192,192,192"
		strokeWeight="1.0"
		cornerSize="7,7"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        visible="true"
        enabled="true" />
		
	<actor
        name="media_type_label"
        x="47"
        y="72"
        width="100"
        height="20"
        text="Media Type"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true"/>
		
	<actor
        name="DVD_check_box_image"
        x="47"
        y="94"		
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="DVD_label"
        x="104"
        y="107"
        width="70"
        height="20"
        text="DVD"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="blu_ray_check_box_image"
        x="47"
        y="144"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="blu_ray_label"
        x="104"
        y="155"
        width="70"
        height="20"
        text="Blu-ray"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="games_check_box_image"
        x="197"
        y="94"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="games_label"
        x="254"
        y="107"
        width="70"
        height="20"
        text="Games"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="MT_all_check_box_image"
        x="197"
        y="144"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box_checked"
		tag="checked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="MT_all_label"
        x="254"
        y="155"
        width="70"
        height="20"
        text="All"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
<!-- Movie Ratings -->		

	<actor
		name="movie_ratings_background"
		x="329"
		y="63"
		width="284"
		height="187"
		backgroundColor="192,192,192"
		borderColor="192,192,192"
		strokeWeight="1.0"
		cornerSize="7,7"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        visible="true"
        enabled="true" />
		
	<actor
        name="movie_ratings_label"
        x="341"
        y="72"
        width="150"
        height="20"
        text="Movie Ratings"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="G_check_box_image"
        x="341"
        y="94"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="G_label"
        x="398"
        y="109"
        width="70"
        height="20"
        text="G"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="PG_check_box_image"
        x="341"
        y="144"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="PG_label"
        x="398"
        y="156"
        width="70"
        height="20"
        text="PG"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true"/>
		
	<actor
        name="PG13_check_box_image"
        x="341"
        y="194"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="PG13_label"
        x="398"
        y="207"
        width="70"
        height="20"
        text="PG-13"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="R_check_box_image"
        x="484"
        y="94"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="R_label"
        x="543"
        y="109"
        width="70"
        height="20"
        text="R"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true"/>
		
	<actor
        name="NR_check_box_image"
        x="484"
        y="144"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="NR_label"
        x="543"
        y="156"
        width="70"
        height="20"
        text="NR"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="MR_all_check_box_image"
        x="484"
        y="194"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box_checked"
		tag="checked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="MR_all_label"
        x="543"
        y="207"
        width="70"
        height="20"
        text="All"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />


<!-- Game Ratings -->		

	<actor
		name="game_ratings_background"
		x="618"
		y="63"
		width="247"
		height="187"
		backgroundColor="192,192,192"
		borderColor="192,192,192"
		strokeWeight="1.0"
		cornerSize="7,7"
        optionFlags="Background, Text, TextShadow, DrawBorder"
        visible="true"
        enabled="true" />
		
	<actor
        name="game_ratings_label"
        x="630"
        y="72"
        width="150"
        height="20"
        text="Game Ratings"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="E_check_box_image"
        x="630"
        y="94"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="E_label"
        x="688"
        y="109"
        width="70"
        height="20"
        text="E"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="E10_check_box_image"
        x="630"
        y="144"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="E10_label"
        x="688"
        y="156"
        width="70"
        height="20"
        text="E (10+)"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="T_check_box_image"
        x="630"
        y="194"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="T_label"
        x="688"
        y="207"
        width="70"
        height="20"
        text="T"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="M_check_box_image"
        x="760"
        y="94"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box"
		tag="unchecked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="M_label"
        x="818"
        y="109"
        width="70"
        height="20"
        text="M"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
	<actor
        name="GR_all_check_box_image"
        x="760"
        y="144"
		optionFlags="Background, Image, Text, TextShadow"
		image="check_box_checked"
		tag="checked"
		hitFlags="Enabled"
		onHit="filter_titles"
		backgroundColor="192,192,192"
		width="50"
        height="46"
        visible="true"
		enabled="true" />	
		
	<actor
        name="GR_all_label"
        x="818"
        y="156"
        width="70"
        height="20"
        text="All"
        font="medium_font"
        optionFlags="Text"
        foregroundColor="black"
        visible="true"
        enabled="true" />
		
<!-- Buttons Used -->

     <actor
		name="clear_button"
		x="280"
		y="579"
		width="165"
		height="59"
		strokeWeight="3.0"
		borderColor="64,64,64"
		backgroundColor="192,192,192"
		foregroundColor="black"
		optionFlags="Background, Text, DrawBorder"
		hitFlags="Enabled"
		font="medium_large_font"
		verticalAlignment="center"
		horizontalAlignment="center"
		onHit="clear_selection"
		text="Clear"
		visible="true"
		enabled="true" />

      <actor
		name="enter_button"
		x="470"
		y="579"
		width="165"
		height="59"
		strokeWeight="3.0"
		borderColor="64,64,64"
		backgroundColor="192,192,192"
		foregroundColor="black"
		optionFlags="Background, Text, DrawBorder"
		hitFlags="Enabled"
		font="medium_large_font"
		verticalAlignment="center"
		horizontalAlignment="center"
		onHit="set_alphabets"
		text="Enter"
		visible="true"
		enabled="true" />

    </scene>
  </aspect>
</resource>
