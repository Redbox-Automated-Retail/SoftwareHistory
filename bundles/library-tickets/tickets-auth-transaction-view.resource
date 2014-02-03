<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_auth_transaction_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundImage="image_red_diagonal_stripes"
			onEnter="tickets_auth_transaction_view_enter"
			onLeave="tickets_auth_transaction_view_leave">

			<actor
				name="body_1"
				x="76"
				y="239"
				width="515"
				foregroundColor="white"
				font="font_target_50_regular"
				text="Please wait while we authorize your transaction."
				optionFlags="Text" />

		</scene>
	</aspect>
</resource>