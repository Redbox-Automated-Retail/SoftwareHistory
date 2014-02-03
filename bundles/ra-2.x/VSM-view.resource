<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="vsm_view" />
	<property name="version" value="2.0,Core,A" />
	<property name="active_flag" value="passive" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			onLeave="vsm_view_leave">
		
			<actor
                name="vsm_image_placeholder"
                x="0"
                y="0"
                width="1024"
                height="632"
                optionFlags="Image" />
				
			<actor
                name="vsm_image_bottom"
                x="0"
                y="632"
                width="1024"
                height="136"
				image="image_vsm_image_bottom"
                optionFlags="Image" />
		</scene>
	</aspect>
</resource>