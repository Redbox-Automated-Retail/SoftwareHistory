	# sync-locations.hs
	# Copyright (C) 2011 Redbox Automated Retail, LLC
	# All Rights Reserved
	# VERSION 1.13.0.27
	#
		
	LOG			"Begin sync-locations request."
	
	APPLOG		CONFIGURE	SUBDIRECTORY="Sync" FLUSHONWRITE=FALSE
	
	GETCFG		"ErrorSyncOnCameraFailure" TYPE=Controller
	POP			ERROR-SYNC-ON-CAMERA-FAILURE
	
	GETCFG		"MarkDuplicatesUnknown" TYPE=Controller
	POP			MARK-DUP-UNK
        
	DECK		MAXSLOTS NUMBER=1
	POP         MAX-SLOT-PER-DECK
    
	POP			LOCATION-COUNT
	IF			LOCATION-COUNT == 0
		LOG			"No locations were specified"
		JUMP		_SYNC_DONE
	ENDIF
    
	POP         CUR-DECK
	POP         CUR-SLOT
	
	GRIPPER	CLEAR
	POP 		CLEAR-STATUS
	IF	FALSE == CLEAR-STATUS
		SET	MSTESTER-SYMBOL-GRIPPER-OBS	"The Gripper is obstructed."
		EXIT		"The gripper is obstructed."
	ENDIF
	
	GRIPPER	CHECK-PICKER
	POP		CP-STATUS
	IF	"PICKEREMPTY" != CP-STATUS
		PUSHF		"Check picker returned an error status {0}" CP-STATUS
		POP		MSTESTER-SYMBOL-CHECK-PICKER-FAILURE
		EXIT		"check-picker was unable to clear the picker."
	ENDIF
	
	IF          CUR-SLOT > MAX-SLOT-PER-DECK
		LOG			"Slot outside machine range, skipping..."
		JUMP		_GOTO_NEXT_ITEM
	ENDIF
        
_NEXT_ITEM:
	
	SYNC		TYPE="LOCATION"	DECK=CUR-DECK SLOT=CUR-SLOT
	POP	SYNC-RESULT
		        
_GOTO_NEXT_ITEM:               
	DEC         LOCATION-COUNT
	IF          LOCATION-COUNT == 0
		JUMP        _SYNC_DONE
	ENDIF
		
	POP			CUR-DECK
	POP			CUR-SLOT

	YIELD
	JUMP        _NEXT_ITEM

        
_SYNC_DONE:
	MOVEVEND		MODE=GET
	POP
	NOP
