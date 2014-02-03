        #
        # RETURN UNKNOWNS Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.3.0.84
        #

	CONST		RETURN-UNKNOWN-LOGNAME	"ReturnUnknown"
	CONST		RETURN-UNKNOWN-SUBDIR	"Unknowns"
	
	POP			PREVIOUS-RETURN-TIME
	
	SET		UNKNOWNS-LOGNAME	RETURN-UNKNOWN-LOGNAME
	SET		UNKNOWNS-SUBDIR	RETURN-UNKNOWN-SUBDIR
	
	IF		"FMA-INSERT" == PREVIOUS-RETURN-TIME
		SET		UNKNOWNS-LOGNAME	"FieldInsert"
		SET		UNKNOWNS-SUBDIR	"Return"
	ENDIF
		
	
	LOG			"Return-unknown start."
	APPLOG		CONFIGURE	NAME=UNKNOWNS-LOGNAME SUBDIRECTORY=UNKNOWNS-SUBDIR FLUSHONWRITE=TRUE
	
	APPLOG		"Return-unknown start." 
	
	GETCFG		"ReturnSlotBuffer" TYPE=SCRIPT
	POP			SLOT-BUFFER
	
	FINDEMPTY	SLOT-BUFFER
	POP			EMPTY-SLOT-STATE
	IF			"MACHINE FULL" == EMPTY-SLOT-STATE
		LOG			"The machine is full, can't continue."
		RESULT		CODE="MachineError" MESSAGE="The machine is full." 
		MSG			"MachineFull"
		EXIT		"The machine is full."
	ENDIF

	POP
	POP
    		
	GRIPPER		STATUS
	POP			GRIPPER-STATUS
	IF		"FULL" == GRIPPER-STATUS
		LOG	"The gripper is full - please fix."
		APPLOG		"The gripper is obstructed - exiting." 
		RESULT	CODE="ItemStuckInGripper" MESSAGE="There is a disc stuck in the picker."
		EXIT	"Gripper is obstructed."
	ENDIF
		
	GETCFG		"PutAwayItemAttempts" TYPE=Controller
	POP			ITEM-AWAY-ATTEMPTS

	SET			PUT-ATTEMPTS 0
    
	SET			CLOSE-COUNT	0
	SET			VENDDOOR-CLOSE-RETRIES	1
	SET			BARCODE-READ FALSE
	RUNTIME		YIELD-ONLY
	
	TRACK		STATUS
	POP			TRK-STATUS
	IF			"OPEN" == TRK-STATUS
		TRACK		CLOSE
		POP	
	ENDIF

	MOVEVEND	MODE=GET
	POP			MOVE-RESULT
	IF			MOVE-RESULT != "SUCCESS"
		PUSHF		"There was an error while executing the MOVEVEND instruction: {0}" MOVE-RESULT
		POP			FORMATTED-MESSAGE
		RESULT		CODE="MachineError" MESSAGE=FORMATTED-MESSAGE END=BOTTOM
		MSG			"MachineError"
		EXIT		"Unable to move to the vend door."
	ENDIF

	RINGLIGHT	ON
	CAMERA		START
	POP
	
	
	VENDDOOR	RENT
	POP			VEND-DOOR-STATE
	IF			"SUCCESS" != VEND-DOOR-STATE
		LOG			"VENDDOOR RENT returned timeout." ENTRYTYPE=ERROR
		PUSHF		"VENDDOOR RENT failed with error code {0}" VEND-DOOR-STATE
		POP			VD-ERR-MSG
		RESULT		CODE="MachineError" MESSAGE=VD-ERR-MSG END=BOTTOM
		APPLOG		"VENDDOOR RENT returned an error status {0}" VEND-DOOR-STATE 
		EXIT		"Unable to open the vend door."
	ENDIF
	
_TAKE_DISC:
	; Start rollers moving in so the item is pulled into picker
       
	MSG			"BlockingOnSensor"
	
	GRIPPER		ACCEPT-DISC-AT-DOOR
	POP			ACCEPT-DISC-STATUS
	
	IF			"HARDWAREERROR" == ACCEPT-DISC-STATUS
		APPLOG  		"Accept disc into the picker returned a hardware error; check the error logs." 
		MSG			"MachineError"
		RESULT		CODE="MachineError" MESSAGE="Unable to accept disc in picker." END=BOTTOM
		EXIT 		"Machine error during accept disc."
	ENDIF
	
	IF			"EMPTY" == ACCEPT-DISC-STATUS
		LOG			"After 15 seconds, didn't get anything in the picker; exiting." ENTRYTYPE=INFO
		APPLOG		"No item was put in the picker - exiting." 
		RESULT		CODE="NoReceivedItem" MESSAGE="No disc was received by picker." END=BOTTOM
		JUMP		_END
	ENDIF
	
	IF			"DISCINPICKER" == ACCEPT-DISC-STATUS
		VENDDOOR	STATUS
		POP
		JUMP	_DISK_IN_PICKER
	ENDIF
	
	IF 	"OBSTRUCTED" == ACCEPT-DISC-STATUS
		LOG			"More than 3 sensors lit; reject disk."
		GRIPPER		PUSH-OUT
		POP
		RESULT		CODE="ItemStuckInGripper" MESSAGE="More than 3 sensors are lit." END=BOTTOM
		EXIT		"Item Stuck in gripper."
	ENDIF
	
	VENDDOOR		CLOSE
	POP			VENDDOOR-TIMEOUT
	IF 			"TIMEOUT" == VENDDOOR-TIMEOUT
		LOG			"There are no discs in the picker and the vend door won't close."
		RESULT		CODE="MachineError" MESSAGE="VENDDOOR CLOSE TIMEOUT" END=BOTTOM
		EXIT		"Unable to close the vend door."
	ENDIF
	JUMP		_END
	
	
_EJECT_DISC:

	SET BLOCKED-COUNT  0
	VENDDOOR		RENT
	POP
	GRIPPER	VEND-DISC-AT-DOOR
	POP		VEND-DISC-STATUS

	IF		"HARDWAREERROR" == VEND-DISC-STATUS
		MSG			"MachineError"
		LOG			"There was a hardware error vending the disc." ENTRYTYPE=ERROR
		RESULT		CODE="MachineError" MESSAGE="Machine error ejecting the disc."
		APPLOG			"Vend disc returned an error code; check the logs." 
		EXIT			"Hardware error."
	ENDIF
	
	IF		"EMPTY" == VEND-DISC-STATUS
		RESULT	CODE="BackwardDiscTaken" MESSAGE="The backward disc was taken." END=BOTTOM
		LOG                 "Eject disc successful; close door and exit."
		VENDDOOR		CLOSE
		POP			CLOSE-STATUS
		IF			"TIMEOUT" == CLOSE-STATUS
			JUMP                _END
		ENDIF
		JUMP		_END	
	ENDIF
		
	LOG             "Disk not taken; accepting disk into kiosk."
	
	TRACK		CLOSE
	POP
	
	GRIPPER	ACCEPT-DISC-AT-DOOR
	POP		ACC-STATUS
	
	IF	"EMPTY" == ACC-STATUS
		VENDDOOR		CLOSE
		POP
		JUMP		_END
	ENDIF
	
	IF	"DISCINPICKER" != ACC-STATUS
		RESULT	CODE="ItemStuckInGripper" MESSAGE="There is something obstructing the picker."
		EXIT		"Item stuck in the vend door."
	ENDIF
	
	VENDDOOR 	CLOSE
	POP	CLOSE-STATUS
	SET		STATE  "EJECT-FAILED"
	JUMP		_RETRY_PUT_AWAY
	
_DISK_IN_PICKER:
	LOG			"Item received into picker."
	
	TRACK		CYCLE
	POP
	WAIT		500
	
	SET		ID-IN-PICKER	"UNKNOWN"

	GRIPPER		READ-DISC SHUFFLE="DrumAndBack"
	POP			READ-STATUS

	IF			"SUCCESS" == READ-STATUS
		POP			ID-IN-PICKER
	ENDIF	
	IF			"SUCCESS" != READ-STATUS
		APPLOG		"The READ-DISC instruction returned an error {0}" READ-STATUS
		LOG			"The CAMERA CAPTURE instruction errored." ENTRYTYPE=ERROR
		RESULT		CODE="CameraCaptureFailure" MESSAGE="The CAMERA CAPTURE instruction errored." END=BOTTOM
		SET			ID-IN-PICKER "UNKNOWN"
	ENDIF
	
	RESULT		CODE="BarcodeRead" ITEM=ID-IN-PICKER MESSAGE="The returned item's barcode."
	
	RINGLIGHT	OFF
	WAIT		200
	CAMERA		STOP
	POP

	; check venddoor here before putting away
	VENDDOOR	CLOSE
	POP			VENDDOOR-TIMEOUT
	IF			"TIMEOUT" == VENDDOOR-TIMEOUT
		LOG			"Unable to close venddoor."
		APPLOG		"Unable to close venddoor." 
		RESULT		CODE="MachineError" MESSAGE="Venddoor close timeout." END=BOTTOM
		EXIT		"Unable to close the vend door."
	ENDIF

	; Grab the barcode off the stack from the READ instruction
	IF			ID-IN-PICKER == "UNKNOWN"
		LOG			"The barcode couldn't be read."
		RESULT		CODE="BadIdRead" MESSAGE="The READ request failed." END=BOTTOM
		IF		"FMA-INSERT" == PREVIOUS-RETURN-TIME
			JUMP		_EJECT_DISC			
		ENDIF
		JUMP		_RETRY_PUT_AWAY
	ENDIF
	IF			ID-IN-PICKER == "redbox"
		LOG			"The item was placed into the picker backwards."
		RESULT		CODE="ItemReceivedBackward" ITEM=ID-IN-PICKER MESSAGE="The READ request failed." END=BOTTOM
		IF		"FMA-INSERT" == PREVIOUS-RETURN-TIME
			JUMP		_EJECT_DISC			
		ENDIF
		JUMP		_RETRY_PUT_AWAY
	ENDIF

	SET			BARCODE-READ TRUE
	LOG			"The ID in the picker: {0}" ID-IN-PICKER
	MSG			"ReturnEnd:{0}" ID-IN-PICKER

_RETRY_PUT_AWAY:
	; Is there an empty slot we can use?
	LOG			"Find an empty slot for item in picker."
	IF			PUT-ATTEMPTS == ITEM-AWAY-ATTEMPTS
		PUSHF		"Failed to put away item after {0} attempts." PUT-ATTEMPTS
		POP			ERR-MSG
		LOG			ERR-MSG  ENTRYTYPE=ERROR
		RESULT		CODE="PutFailureMaxAttempts" ITEM=ID-IN-PICKER MESSAGE=ERR-MSG  END=BOTTOM
		MSG			"MaxPutAwayAttemptsExceeded"
		MOVEVEND		
		POP		MOVE-VEND-STATUS
		IF	"SUCCESS" != MOVE-VEND-STATUS
			PUSHF		"There was an error while executing the MOVEVEND instruction: {0}" MOVE-VEND-STATUS
			POP			FORMATTED-MESSAGE
			MSG			"MachineError"
			RESULT		CODE="MachineError" MESSAGE=FORMATTED-MESSAGE END=BOTTOM
			EXIT			"Machine error."
		ENDIF
		VENDDOOR		RENT
		POP			VD-STATUS
		IF		"SUCCESS" != VD-STATUS
			APPLOG		"The VENDDOOR RENT instruction timed out." 
			RESULT		CODE="MachineError" MESSAGE="Unable to open the vend door." END=BOTTOM
			EXIT		"Machine error opening vend door."
		ENDIF
		GRIPPER		PUSH-OUT
		POP
		EXIT		"Unable to put away disk."
	ENDIF
	
	FINDEMPTY	SLOT-BUFFER
    
	POP			EMPTY-SLOT-STATE
	IF			"MACHINE FULL" == EMPTY-SLOT-STATE
		LOG			"The machine is full, can't continue."  ENTRYTYPE=ERROR
		MSG			"MachineFull"
		JUMP		_END
	ENDIF
    

	POP			DECK-LOC
	POP			SLOT-LOC
   
	LOG			"Moving to {0}, {1} to put item in picker away." DECK-LOC SLOT-LOC
	MOVE		DECK=DECK-LOC SLOT=SLOT-LOC MODE=PUT
	POP			MOVE-RESULT
	IF			MOVE-RESULT != "SUCCESS"
		PUSHF		"There was an error while executing the MOVE instruction: {0}" MOVE-RESULT
		POP			FORMATTED-MESSAGE
		LOG			"{0}" FORMATTED-MESSAGE  ENTRYTYPE=ERROR
		MSG			"MachineError"
		RESULT		CODE="MachineError" MESSAGE=FORMATTED-MESSAGE END=BOTTOM
		JUMP		_END
	ENDIF

	; Put the item in picker away
	LOG			"Putting item:{0} in picker into slot." ID-IN-PICKER
	PUSH		ID-IN-PICKER
	PUT
	POP			PUT-RESULT-COUNT
	POP			PUT-RESULT
	IF			"SUCCESS" != PUT-RESULT
		PUSHF		"There was an error while executing the PUT instruction: {0}" PUT-RESULT
		POP			PUT-FORMATTED-MESSAGE
		RESULT	CODE="PutFailure" DECK=DECK-LOC SLOT=SLOT-LOC MESSAGE=PUT-FORMATTED-MESSAGE
	ENDIF
	INC			PUT-ATTEMPTS
	
	IF			PUT-RESULT-COUNT == 2
		POP		    PUT-RESULT2
	ENDIF
	
	IF			PUT-RESULT == "SLOTINUSE"
		RESULT		CODE=PUT-RESULT DECK=DECK-LOC SLOT=SLOT-LOC ITEM=ID-IN-PICKER MESSAGE="The slot is in use." END=BOTTOM
		TRACK	CYCLE
		POP
		JUMP		_RETRY_PUT_AWAY
	ENDIF
		
	IF			FALSE == BARCODE-READ
		IF 	"NONE" != PREVIOUS-RETURN-TIME
			IF	"FMA-INSERT" != PREVIOUS-RETURN-TIME
				LOC			SETRETURNTIME	PREVIOUS-RETURN-TIME DECK=DECK-LOC SLOT=SLOT-LOC
			ENDIF
		ENDIF
	ENDIF

	MSG			"ItemPlacementDone:{0}" ID-IN-PICKER
	RESULT		CODE="ItemReturned" DECK=DECK-LOC SLOT=SLOT-LOC ITEM=ID-IN-PICKER MESSAGE="The item was successfully returned."
      
_END:
	CAMERA		STOP
	POP
	RINGLIGHT	OFF
	GRIPPER		RETRACT
	ROLLER		STOP
	MOVEVEND	MODE=GET
	POP			MOVE-VEND-STATUS
	
