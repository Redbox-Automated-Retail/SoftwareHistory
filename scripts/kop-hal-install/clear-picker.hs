        #
        # CLEAR PICKER Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.3.0.119
        #

        
	LOG                 "Clear picker to see if we need to put something away."
	GRIPPER		STATUS
	POP			PICKER-STATUS
	IF			"FULL" == PICKER-STATUS
		GRIPPER            CLEAR
		POP                 GRIPPER-CLEAR-STATUS
	
		IF	"FALSE" == GRIPPER-CLEAR-STATUS
			LOG		"There is an item partially stuck in the gripper track." ENTRYTYPE=ERROR
			EXIT	"There is an item partially stuck in the gripper track."
		ENDIF
		GRIPPER		STATUS
		POP			PICKER-STATUS
		IF			"EMPTY" == PICKER-STATUS
			LOG		"Item was cleared from picker." ENTRYTYPE=INFO
			JUMP	_DONE
		ENDIF
		LOG             "Picker is full, trying to find a slot for product."
		
		; Grab the ID for the item in the picker
		RINGLIGHT			ON
		CAMERA			START	CENTER=TRUE
		POP
		
		TRACK	OPEN
		POP
		TRACK	CLOSE
		POP
		
		WAIT		300
		
		CAMERA          SNAP
		RINGLIGHT       OFF
		CAMERA			STOP
		POP
		POP				CAMERA-CAPTURE-RESULT
		IF					"CAPTURE ERROR" == CAMERA-CAPTURE-RESULT
			LOG		"The CAMERA CAPTURE instruction errored." ENTRYTYPE=ERROR
			SET ID-IN-PICKER	"UNKNOWN"
			JUMP	_CAMERA_CAPTURE_READ_FAILURE
		ENDIF
		IF					"CAPTURE TIMED OUT" == CAMERA-CAPTURE-RESULT
			LOG		"The CAMERA CAPTURE instruction timed out." ENTRYTYPE=ERROR
			SET ID-IN-PICKER	"UNKNOWN"
			JUMP	_CAMERA_CAPTURE_READ_FAILURE
		ENDIF
		READ
		POP				NUM-BARCODES
		IF				NUM-BARCODES > 0
			POP			ID-IN-PICKER DEPTH=NUM-BARCODES
		ENDIF
			
		IF				NUM-BARCODES == 0
			POP			ID-IN-PICKER
		ENDIF
			
		; Grab the barcode off the stack from the READ instruction
		IF              ID-IN-PICKER == "UNKNOWN"
			LOG         "The barcode couldn't be read."  ENTRYTYPE=ERROR
		ENDIF 
		IF              ID-IN-PICKER == "redbox"
			LOG         "The item was placed into the picker backwards."  ENTRYTYPE=ERROR
		ENDIF
            
_CAMERA_CAPTURE_READ_FAILURE:
		LOG             "The ID in the picker: {0}" ID-IN-PICKER
		GETCFG			"PutAwayItemAttempts" TYPE=Controller
		POP				ITEM-AWAY-ATTEMPTS
		SET				PUT-ATTEMPTS 0
            
_RETRY_PUT_AWAY:
		; Is there an empty slot we can use?
		IF	PUT-ATTEMPTS == ITEM-AWAY-ATTEMPTS
			LOG		"Attempted to put away item in picker {0} times; bailing." PUT-ATTEMPTS ENTRYTYPE=ERROR
			EXIT	"Unable to put away disk."
		ENDIF
	    
		LOG             "Find an empty slot for item in picker."
		FINDEMPTY
		POP             EMPTY-SLOT-STATE
		IF              "MACHINE FULL" == EMPTY-SLOT-STATE
			LOG         "The machine is full, can't continue."
			EXIT        "The picker is full and there are no empty slots available." 
		ENDIF
		
		IF              "NO EMPTY SLOT" == EMPTY-SLOT-STATE
			LOG         "There are no empty slots, and the machine hasn't seen a full sync; can't continue."
			EXIT        "The picker is full and there are no empty slots available." 
		ENDIF
            
		POP             DECK-LOC
		POP             SLOT-LOC
		LOG             "Moving to {0}, {1} to put item in picker away." DECK-LOC SLOT-LOC
		MOVE            DECK=DECK-LOC SLOT=SLOT-LOC MODE=PUT
		POP             MOVE-RESULT
		IF				MOVE-RESULT != "SUCCESS"
			LOG			"There was an error while executing the MOVE instruction: {0}" MOVE-RESULT  ENTRYTYPE=ERROR
			EXIT		"The MOVE instruction failed."
	        ENDIF
            
		; Put the item in picker away
		LOG             "Putting item in picker into slot."
		PUSH            ID-IN-PICKER
		PUT
		POP             PUT-RESULT-COUNT
		POP             PUT-RESULT
		IF              PUT-RESULT == "SUCCESS"
			JUMP		_COMPLETE
		ENDIF
		IF				PUT-RESULT == "PICKEREMPTY"
			JUMP		_COMPLETE
		ENDIF
		IF				PUT-RESULT == "LOCATIONOUTOFRANGE"
			JUMP		_COMPLETE
		ENDIF
		INC	PUT-ATTEMPTS
		IF	"SLOTINUSE" == PUT-RESULT
			RESULT			CODE=PUT-RESULT	DECK=DECK-LOC	SLOT=SLOT-LOC	ITEM=ID-IN-PICKER	MESSAGE="The slot is in use."	END=BOTTOM
    		ENDIF
		JUMP            _RETRY_PUT_AWAY
_COMPLETE:
		IF				PUT-RESULT-COUNT == 2
			POP			PUT-RESULT2
		ENDIF 
		
	ENDIF
_DONE:
	LOG                 "Clear-picker complete."