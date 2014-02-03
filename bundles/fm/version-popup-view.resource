<?xml version="1.0"?>
<resource type="view">
  <property name="name" value="version_popup_view" />
  <property name="active_flag" value="active" />
  <aspect name="content">
    <scene name="VersionPopup"
      cached="false"
	  clear="false"
      onEnter="version_popup_enter"
      viewWindow="162,68,1024,768" >

		<actor
		name="lightbox"
		x="-162"
		y="-68"
		height="768"
		width="1024"
		backgroundColor="black"
		opacity="0.5"
		optionFlags="Background"
		/>
		
 		<actor
		name="background"
		x="0"
		y="0"
		width="726"
		height="642"
		backgroundColor="Gray"
		strokeWeight="3.0"
		borderColor="192,192,192"
		cornerSize="10,10"
		optionFlags="Background, DrawBorder"
        hitFlags="Enabled, Obstruction"
		/>

		<actor
		name="title_area"
		x="2"
		y="2"
		optionFlags="Background, Text, TextShadow, DrawRoundedCorners"
		backgroundColor="DarkRed"
		text="  Software Versions - Caddyshack"
		foregroundColor="White"
		verticalAlignment="center"
		font="medium_larger_font"
		cornerSize="10,10"
		width="721"
		height="50"
		/>

		<actor
		name="close_button"
		x="649"
		y="0"
		width="77"
		height="52"
		font="large_font"
		foregroundColor="white"
		backgroundColor="192,192,192"
		verticalAlignment="center"
		horizontalAlignment="center"
		cornerSize="10,10"
		text="X"
		optionFlags= "Background, Text, DrawRoundedCorners"
		hitFlags="Enabled"
		onHit="go_back"/>

		<actor
		name="store_label"
		x="40"
		y="60"
		width="80"
		height="25"
		foregroundColor="white"
		horizontalAlignment="near"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="Kiosk ID:" />

		<actor
		name="store_id"
		x="220"
		y="60"
		width="400"
		height="16"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow" />

		<actor
		name="engine_version_label"
		x="40"
		y="85"
		width="110"
		height="25"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="Kiosk Engine:" />

		<actor
		name="engine_version"
		x="220"
		y="85"
		width="400"
		height="25"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="${EngineVersion}" />

		<actor
		name="hal_versions_label"
		x="40"
		y="110"
		width="120"
		height="25"
		foregroundColor="white"
		horizontalAlignment="near"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="HAL Versions:" />

		<actor
		name="hal_versions"
		x="220"
		y="110"
		width="400"
		height="25"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="Srvc(${HALServiceVersion}) Client DLL(${HALVersion})" />

		<actor
		name="inventory_version_label"
		x="40"
		y="135"
		width="133"
		height="25"
		foregroundColor="white"
		horizontalAlignment="near"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="Inventory Data: " />

		<actor
		name="inventory_version"
		x="220"
		y="135"
		width="400"
		height="25"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="${InventoryVersion}" />

		<actor
		name="profile_version_label"
		x="40"
		y="160"
		width="110"
		height="25"
		foregroundColor="white"
		horizontalAlignment="near"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="Profile Data: " />

		<actor
		name="profile_version"
		x="220"
		y="160"
		width="400"
		height="25"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="${ProfileVersion}" />

		<actor
		name="thin_version_label"
		x="40"
		y="185"
		width="110"
		height="25"
		foregroundColor="white"
		horizontalAlignment="near"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="Thin Data: " />

		<actor
		name="thin_version"
		x="220"
		y="185"
		width="400"
		height="25"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="${ThinVersion}" />

		<actor
		name="rebalance_version_label"
		x="40"
		y="210"
		width="400"
		height="25"
		foregroundColor="white"
		horizontalAlignment="near"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="Rebalance Data: " />

		<actor
		name="rebalance_version"
		x="220"
		y="210"
		width="400"
		height="25"
		horizontalAlignment="near"
		foregroundColor="white"
		font="medium_font"
		optionFlags="Text, TextShadow"
		text="${RebalancesVersion}" />

    </scene>
  </aspect>
</resource>
