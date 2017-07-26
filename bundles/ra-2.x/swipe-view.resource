<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="swipe_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
      render_type="WPF"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="swipe_view_enter"
			onLeave="swipe_view_leave">

      <actor
				name="SwipeViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="Redbox.Rental.UI.Views.SwipeViewUserControl"/>

		</scene>
	</aspect>
</resource>