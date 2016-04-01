<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="help_document_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="0, 0, 0"
			cached="true"
			stylesheet="style_sheet_common"
			onEnter="help_document_view_enter"
      onLeave="help_document_view_leave">

      <actor
				name="HelpViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.Services.dll"
        WPFControlName="Redbox.Rental.Services.Views.HelpView.HelpViewUserControl"/>
    </scene>
	</aspect>
</resource>
