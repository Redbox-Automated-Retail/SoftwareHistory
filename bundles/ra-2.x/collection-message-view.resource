<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="collection_message_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
      render_type="WPF"
			width="1024"
			height="768"
			backgroundColor="transparent"
			stylesheet="style_sheet_common"
			onEnter="collection_message_view_enter"
			onLeave="collection_message_view_leave"
      clear="false">

      <actor
				name="CollectionMessageViewUserControl"
				x="0"
				y="0"
				width="1024"
				height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="Redbox.Rental.UI.Views.CollectionMessageViewUserControl"/>

    </scene>
	</aspect>
</resource>
