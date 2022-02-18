<?xml version="1.0"?>
<resource type="script" 
	depends="
	lib_constants,
	data_error_handler,
	localization_functions, 
	vend_functions, 
	shopping_cart_functions, 
	lib_product_functions, 
	lib_browse_functions, 
	lib_sync_functions, 
	lib_statistics_functions, 
	lib_view_functions, 
	lib_scheduler_functions,
	obj_hard_sync_monitor, 
	obj_qlm_monitor, 
	obj_coming_soon_generator, 
	obj_alert_manager,
	obj_idle_timer, 
	obj_hardware_manager, 
	obj_store_manager, 
	obj_maintenance_mode_timer,
	obj_reservation_manager, 
	obj_scene_manager_pager, 
	quickreturn_functions, 
	lib_datetime,
	obj_progress_manager,
	swipe_functions,
	lib_inventory_functions,
  pickup_next">
    <property name="name" value="application_startup" />
    <aspect name="content" file="assets/scripts/application-startup.lua" />
</resource>
