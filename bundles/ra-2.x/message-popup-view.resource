<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="message_popup_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
			onEnter="message_popup_view_enter"
			onLeave="message_popup_view_leave"
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
