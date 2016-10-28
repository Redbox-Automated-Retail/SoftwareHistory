<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="return_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="return_view_enter"
			onLeave="return_view_leave">

      <actor
				name="ReturnViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="Redbox.Rental.UI.Views.ReturnViewUserControl"/>
		</scene>
	</aspect>
</resource>
