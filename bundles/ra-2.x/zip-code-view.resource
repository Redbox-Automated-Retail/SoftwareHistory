<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="zip_code_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene 
			render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="255,255,255"
			onEnter="zip_code_view_enter">
			
			<actor
				name="ZipCodeViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
				WPFControlAssemblyName="Redbox.Rental.UI.exe"
				WPFControlName="Redbox.Rental.UI.Views.ZipCodeViewUserControl"/>
		</scene>
	</aspect>
</resource>