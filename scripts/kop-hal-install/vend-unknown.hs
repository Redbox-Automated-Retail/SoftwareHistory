        #
        # VEND-UNKNOWN Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.3.0.84
        #

	CONST	UNKNOWNS-LOGNAME	"VendUnknown"
	CONST	UNKNOWNS-SUBDIR		"Unknowns"
	
	CONST	HW-ERR-MSG	"Unable to remove unknown due to hardware errors. Inspect the slot after completing the process."
	
	APPLOG	CONFIGURE	NAME=UNKNOWNS-LOGNAME SUBDIRECTORY=UNKNOWNS-SUBDIR FLUSHONWRITE=FALSE

	APPLOG		"Vend unknown start." 
	MSG			"VendStart"
    
	GETCFG		"RotateDrumDuringUnknownRemoval2" TYPE=Controller
	POP			ROT-DRUM-ON-GET-FAIL
		
	SET		VEND-DISK-POLL-COUNT		30
	
	SET		ID-TO-FIND	"UNKNOWN"
	
	MSG		"VendGraphicDisplay"
	
	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"EMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
	
	; Move to get the item requested
	POP                 DECK-LOC
	POP                 SLOT-LOC
	
	SET	ID-IN-PICKER  "UNKNOWN"

	LOG                 "Move to {0}, {1} to pick requested item." DECK-LOC SLOT-LOC
	MOVE                DECK=DECK-LOC SLOT=SLOT-LOC MODE=GET
	POP			MOVE-RESULT	
	IF					MOVE-RESULT != "SUCCESS"
		MSG				"MachineError"
		RESULT		CODE="MoveVendError" DECK=DECK-LOC  SLOT=SLOT-LOC  MESSAGE=HW-ERR-MSG
		JUMP			_MACHINE_ERROR
	ENDIF

	GET			GET-RT=TRUE
	POP                 GET-STATE
	POP                 PREVIOUS-SLOT-ID
	POP			LOC-RETURN-TIME
	
	IF		"SLOTEMPTY" == GET-STATE
		IF	ROT-DRUM-ON-GET-FAIL == FALSE
			RESULT	CODE="SlotEmpty" DECK=DECK-LOC	SLOT=SLOT-LOC	MESSAGE="The slot is empty."
			LOC			CLEARRETURNTIME 	DECK=DECK-LOC	SLOT=SLOT-LOC
			JUMP		_VEND_END
		ENDIF
		
		GRIPPER		PEEK
		POP			PEEK-RESULT
		IF			PEEK-RESULT == "FULL"
			LOG			"Disc stuck in slot, moving to next."
			RESULT		CODE="GetAfterPeekShowsFull" DECK=DECK-LOC SLOT=SLOT-LOC ITEM=PREVIOUS-SLOT-ID RETURNTIME=LOC-RETURN-TIME MESSAGE="This Location was NOT updated in inventory." END=BOTTOM
			LOC			SETINVENTORY DECK=DECK-LOC	SLOT=SLOT-LOC ID=PREVIOUS-SLOT-ID
			POP
			SET			ID-IN-PICKER "UNKNOWN"
			MOVE	SLOT-OFFSET=5
			POP		MOVE-OFFSET-STATUS
			IF		"SUCCESS" == MOVE-OFFSET-STATUS
				POP		OFFSET-COUNT
				PUSHF	"The damaged disk is located {0} slots left, starting at the disk next to the extended gripper fingers." OFFSET-COUNT 
				POP		OFFSET-MSG
				GRIPPER	RENT
				POP
				GRIPPER	EXTEND-FOR-TIME
				SENSOR	PICKER-ON
				RESULT	CODE="MoveOffsetOk" MESSAGE=OFFSET-MSG
			ENDIF
			IF		"SUCCESS" != MOVE-OFFSET-STATUS
				RESULT	CODE="MoveOffsetError" MESSAGE="There was an error moving the offset."
				EXIT		"There was an error moving to the offset position."
			ENDIF
			JUMP		_SKIP_VEND_MOVE
		ENDIF
		
		IF			PEEK-RESULT == "EMPTY"
			LOG         "Slot is empty, moving to next."
			SET			ID-IN-PICKER "EMPTY"
			RESULT		CODE="SlotEmpty" DECK=DECK-LOC	SLOT=SLOT-LOC	MESSAGE="The slot is empty."
			LOC			CLEARRETURNTIME DECK=DECK-LOC	SLOT=SLOT-LOC
			POP
			JUMP		_VEND_END
		ENDIF
		
		PUSHF		"Peek Deck={0} Slot={1} returned status {2} after GET returned SLOTEMPTY" DECK-LOC SLOT-LOC PEEK-RESULT
		POP			PEEK-ERROR
		APPLOG		PEEK-ERROR
		LOG			PEEK-ERROR ENTRYTYPE=ERROR
		RESULT		CODE="MachineError" DECK=DECK-LOC SLOT=SLOT-LOC MESSAGE="An error occurred when attempting to peek the slot." END=BOTTOM
		EXIT		"There was an error performing a PEEK."
	ENDIF
	
	IF                  GET-STATE != "SUCCESS"
		RESULT		CODE="GetError" DECK=DECK-LOC SLOT=SLOT-LOC MESSAGE=HW-ERR-MSG END=BOTTOM
		JUMP			_MACHINE_ERROR
	ENDIF
        
	LOG                 "Move to vend location."
	MSG                 "MoveToVend"
	MOVEVEND
	POP					MOVE-RESULT
	IF					MOVE-RESULT != "SUCCESS"
		MSG				"MachineError"
		RESULT		CODE="MoveError" DECK=DECK-LOC SLOT=SLOT-LOC MESSAGE=HW-ERR-MSG END=BOTTOM
		JUMP			_MACHINE_ERROR
	ENDIF

	VENDDOOR            RENT
	POP                 VEND-DOOR-STATE
	IF		"SUCCESS" != VEND-DOOR-STATE
		LOG				"VENDDOOR RENT returned timeout." ENTRYTYPE=ERROR
		RESULT		CODE="VendDoorError" DECK=DECK-LOC SLOT=SLOT-LOC MESSAGE=HW-ERR-MSG END=BOTTOM
		APPLOG			"VENDDOOR RENT returned an error status {0}" VEND-DOOR-STATE 
		JUMP			_MACHINE_ERROR
	ENDIF
	
	TRACK	CYCLE
	POP
	
	LOG                 "Start pushing item out of picker."
	MSG					"PickupGraphicDisplay"
   
	GRIPPER		VEND-DISC-AT-DOOR
	POP			VEND-DISC-STATUS
	IF		"HARDWAREERROR" == VEND-DISC-STATUS
		MSG			"MachineError"
		RESULT		CODE="MachineError" MESSAGE=HW-ERR-MSG
		LOG			"There was a hardware error vending the disc." ENTRYTYPE=ERROR
		APPLOG			"Vend disc returned a hardware error; check the logs." 
		JUMP			_MACHINE_ERROR
	ENDIF
		
	IF		"EMPTY" == VEND-DISC-STATUS
		VENDDOOR		CLOSE
		POP			CLOSE-STATUS
		IF		"TIMEOUT" == CLOSE-STATUS
			LOG	"The item was taken, however the vend door could not be closed."
		ENDIF
		MSG				"ItemVended"
		APPLOG	"The item was taken." 
		RESULT          CODE="ItemVended" DECK=DECK-LOC SLOT=SLOT-LOC RETURNTIME=LOC-RETURN-TIME MESSAGE="The item was successfully vended." END=BOTTOM
		LOC			CLEARRETURNTIME 	DECK=DECK-LOC	SLOT=SLOT-LOC
		JUMP			_VEND_END
	ENDIF

	CAMERA		START
	POP	
	RINGLIGHT		ON
	
    LOG                 "The item was not taken - pull it in and file away."
    
    GRIPPER		ACCEPT-DISC-AT-DOOR
    POP		ACCEPT-STATUS
    
    IF                  ACCEPT-STATUS != "DISCINPICKER"
		LOG				"Roll in to sensor 3 timed out." ENTRYTYPE=ERROR
		PUSHF               "ACCEPT-DISK-AT-DOOR failed with error code {0}" ACCEPT-STATUS
		POP                 	S3-ERR-MSG
		RESULT		CODE="RollInToSensor3TimedOut" DECK=DECK-LOC SLOT=SLOT-LOC MESSAGE=HW-ERR-MSG END=BOTTOM
		APPLOG			"ROLL IN TO SENSOR 3 timed out." 
		JUMP			_MACHINE_ERROR
    ENDIF
	
		
	TRACK		CYCLE
	POP
	
	MOVE		DECK=DECK-LOC	SLOT=SLOT-LOC	MODE=PUT
    POP			MOVE-RESULT	
    IF					MOVE-RESULT != "SUCCESS"
		MSG				"MachineError"
		RESULT		CODE="MoveError" DECK=DECK-LOC  SLOT=SLOT-LOC  MESSAGE=HW-ERR-MSG
		JUMP			_MACHINE_ERROR
    ENDIF
	
	PUSH                "UNKNOWN"
    PUT                 
    POP                 NO-PUT-RESULT
    POP                 PUT-RESULT
	IF                  NO-PUT-RESULT == 2
		POP             ADDITIONAL-PUT-RESULT
    ENDIF
	
	IF	"SUCCESS" != PUT-RESULT
		PUSHF               "The PUT failed with error code {0}" PUT-RESULT
		POP                 	PUT-ERR-MSG
		RESULT		CODE="PutError" DECK=DECK-LOC  SLOT=SLOT-LOC  MESSAGE=HW-ERR-MSG
		JUMP			_MACHINE_ERROR
	ENDIF
	
	LOC		SETRETURNTIME	LOC-RETURN-TIME	DECK=DECK-LOC	SLOT=SLOT-LOC
	RESULT	CODE="UnclaimedVend" DECK=DECK-LOC SLOT=SLOT-LOC ITEM=ID-IN-PICKER MESSAGE="The vend request failed because the item was not taken.  It has been returned." END=BOTTOM
    JUMP	_VEND_END

_MACHINE_ERROR:
	LOG				"An unrecoverable error occurred in vend-unknown." ENTRYTYPE=ERROR
	EXIT				"MACHINE ERROR"
	
_VEND_END:
    VENDDOOR			STATUS
    POP					VEND-DOOR-STATUS
    IF					VEND-DOOR-STATUS == "OPEN"
		WAIT			1000
 	   	VENDDOOR 	    CLOSE
 	   	POP				VEND-DOOR-TIMEOUT
	ENDIF
	
	TRACK				STATUS
	POP					TRACK-STATUS
	IF					"OPEN"	== TRACK-STATUS
		TRACK           CLOSE
		POP             TRACK-CLOSE-STATUS
	ENDIF

	MOVEVEND		MODE=GET
	POP
	
_SKIP_VEND_MOVE:
	NOP
	