<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="application_load_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
    <scene
			name="Default"
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="153, 153, 255"
			stylesheet="style_sheet_common">

      <actor
				name="MessagePopupViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="Redbox.Rental.UI.Views.ApplicationLoadViewUserControl"/>

    </scene>
	</aspect>
</resource>
