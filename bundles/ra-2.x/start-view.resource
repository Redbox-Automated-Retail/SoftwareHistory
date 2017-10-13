<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="start_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="0, 0, 0"
			stylesheet="style_sheet_common"
			onEnter="start_view_enter"
			onLeave="start_view_leave">

      <actor
				name="StartViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="StartView"/>

		</scene>
	</aspect>
</resource>
