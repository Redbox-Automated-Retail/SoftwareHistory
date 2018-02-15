<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="keyboard_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">

		<scene	render_type="WPF"
				width="1024"
				height="768"
				stylesheet="style_sheet_common">

				<actor	name="KeyboardViewUserControl"
						x="0"
						y="0"
						width="1024"
						height="768"
						WPFControlAssemblyName="Redbox.Rental.UI.exe"
						WPFControlName="KeyboardView">
				</actor>
			
		</scene>
	</aspect>
</resource>
