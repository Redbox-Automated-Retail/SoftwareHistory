<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="vend_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene 
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="181,18,27"
			onEnter="vend_view_enter" 
			onLeave="vend_view_leave">

      <actor
				name="VendViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="Redbox.Rental.UI.Views.VendViewUserControl"/>

    </scene>
	</aspect>
</resource>
