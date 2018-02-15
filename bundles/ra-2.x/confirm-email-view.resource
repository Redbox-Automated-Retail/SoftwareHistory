<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="confirm_email_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
    <scene
      render_type="WPF"
			name="Default"
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
      onEnter="confirm_email_view_enter">
      
      <actor
        name="ConfirmEmailViewUserControl"
        x="0"
        y="0"
        width="1024"
        height="768"
        WPFControlAssemblyName="Redbox.Rental.UI.exe"
        WPFControlName="ConfirmEmailView"/>
    </scene>
	</aspect>
</resource>
