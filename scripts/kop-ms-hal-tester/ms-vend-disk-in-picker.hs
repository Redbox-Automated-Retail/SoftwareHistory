        #
        # VEND-DISK-IN-PICKER Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION @BUILD_VERSION@
        #
                
        MOVEVEND	
        POP		MOVE-RESULT
	IF	MOVE-RESULT != "SUCCESS"
		PUSHF	"There was an error while executing the MOVEVEND instruction: {0}" MOVE-RESULT 
		POP	MOVE-FAILURE-MSG
		RESULT      CODE="HardwareError" MESSAGE=MOVE-FAILURE-MSG END=BOTTOM
		JUMP		_END
	ENDIF
	
	GRIPPER		VEND-DISC-AT-DOOR
	POP			VEND-DISC-STATUS
	IF		"HARDWAREERROR" == VEND-DISC-STATUS
		RESULT      CODE="HardwareError" MESSAGE="There was a HW error during the vend disk routine." END=BOTTOM
		JUMP			_END
	ENDIF
		
	IF		"EMPTY" == VEND-DISC-STATUS
		RESULT      CODE="ItemVended" MESSAGE="The item was successfully vended." END=BOTTOM
		JUMP		_END
	ENDIF
	
	GRIPPER	ACCEPT-DISC-AT-DOOR
	POP		ACC-STATUS
	
	IF	"DISCINPICKER" != ACC-STATUS
		RESULT      CODE="TakeDiskError" MESSAGE="Unable to get the disk." END=BOTTOM
	ENDIF
	
_END:
	VENDDOOR			STATUS
        POP					VEND-DOOR-STATUS
        IF					VEND-DOOR-STATUS == "OPEN"
        	WAIT			1000
 	  	VENDDOOR 	    CLOSE
 	   	POP				VEND-DOOR-TIMEOUT
		IF              VEND-DOOR-TIMEOUT != "SUCCESS"
			RESULT      CODE="HardwareError" MESSAGE="Unable to close the vend door." END=BOTTOM
		ENDIF
	ENDIF