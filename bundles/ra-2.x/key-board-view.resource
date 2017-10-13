<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="key_board_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">		
		<scene
			width="1024"
			height="768"
			stylesheet="style_sheet_common"
			onEnter="key_board_view_enter">

			<!--These are dummy actor font items to force font load-->
			<actor
							name="akzidenz_regular_small"
							text="" 
							x="0"
							y="0"
							width="0"
							height="0"
							optionFlags="Text"
							foregroundColor="black"
							font="font_akzidenz_regular_small"
							visible="false" />

			<actor
							name="akzidenz_bold_small"
							text="" 
							x="0"
							y="0"
							width="0"
							height="0"
							optionFlags="Text"
							foregroundColor="black"
							font="font_akzidenz_bold_small"
							visible="false" />

			<actor
							name="akzidenz_bold_tiny"
							text="" 
							x="0"
							y="0"
							width="0"
							height="0"
							optionFlags="Text"
							foregroundColor="black"
							font="font_akzidenz_bold_tiny"
							visible="false" />

			<actor
							name="target_small"
							text="" 
							x="0"
							y="0"
							width="0"
							height="0"
							optionFlags="Text"
							foregroundColor="black"
							font="font_target_small"
							visible="false" />

			<actor
							name="akzidenz_bold_smaller"
							text=""
							x="0"
							y="0"
							width="0"
							height="0"
							optionFlags="Text"
							foregroundColor="black"
							font="font_akzidenz_bold_smaller"
							visible="false" />

		</scene>
	</aspect>
</resource>
