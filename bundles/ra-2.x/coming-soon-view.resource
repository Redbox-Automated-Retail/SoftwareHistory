<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="coming_soon_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
      render_type="WPF"
			name="Default"
			width="1024"
			height="768"
			onEnter="coming_soon_view_enter">
		
			<actor
        name="ComingSoonViewUserControl"
        x="0"
        y="0"
        width="1024"
        height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
				WPFControlName="Redbox.Rental.UI.Views.ComingSoonViewUserControl"/>
		</scene>
	</aspect>
</resource>