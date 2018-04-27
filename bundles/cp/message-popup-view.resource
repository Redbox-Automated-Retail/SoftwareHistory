<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="message_popup_view" />
	<aspect name="content">
		<scene
			name="Default"
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
      clear="false">

      <actor
				name="MessagePopupViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="Redbox.Rental.UI.Views.MessagePopupViewUserControl"/>
    </scene>
	</aspect>
</resource>
