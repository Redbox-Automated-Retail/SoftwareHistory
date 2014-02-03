<?xml version="1.0"?>
<resource type="script"
	depends=
	"lib_constants,
	data_error_handler,
	load_tables,
	lib_attract_functions,
	lib_data_store_functions,
	lib_datetime,
	lib_extension_functions,
	lib_view_functions,
	obj_alert_manager,
	obj_hardware_manager,
	obj_hard_sync_monitor,
	obj_idle_timer,
	obj_message_box_manager,
	obj_progress_manager,
	obj_qlm_monitor,
	obj_scene_manager_pager,
	keypad_controller">
    <property name="name" value="application_startup" />
    <aspect name="content" file="assets/scripts/application-startup.lua" />
</resource>
