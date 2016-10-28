<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="idle_timeout_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
			onEnter="idle_timeout_message_view_enter"
			onLeave="idle_timeout_message_view_leave"
      clear="false"
      render_type="WPF">

      <actor
				name="IdleTimeoutMessageViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="Redbox.Rental.UI.Views.IdleTimeoutMessageViewUserControl"/>

    </scene>
	</aspect>
</resource>
