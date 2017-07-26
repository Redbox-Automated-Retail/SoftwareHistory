        #
        # QLM-UNLOAD Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.10.0.32
        #
	
	VMZ	IS-CONFIGURED
	POP	VMZ-CONFIGURED
	
	SET		UNLOADED-DISCS	0
	IF	TRUE == VMZ-CONFIGURED
		APPLOG		CONFIGURE		SUBDIRECTORY="VMZ"
		
		MOTIONCONTROL	HOMEALL CHECKCOM="TRUE"
		POP                 HOMEALL-RESULT
		IF                  TRUE != HOMEALL-RESULT 
			RESULT			CODE="HOMEAXESFAILURE" MESSAGE="Homing the axes failed." 
			EXIT            "The homing operation failed."
		ENDIF	
	
		QLM	CLEAR-PICKER
	
_CONTINUE_VMZ_UNLOAD:
		
		VMZ	UNLOAD-DISC	
		POP	UNLOAD-RESULT
		
		IF	"UNLOADSCLEARED" == UNLOAD-RESULT
			JUMP	_VMZ_UNLOAD_DONE
		ENDIF
		
		IF	"MACHINEFULL" == UNLOAD-RESULT
			JUMP		_VMZ_UNLOAD_DONE
		ENDIF
		
		IF	"SUCCESS" == UNLOAD-RESULT
			INC	UNLOADED-DISCS
		ENDIF
		
		IF	"HARDWAREERROR" == UNLOAD-RESULT
			JUMP		_END
		ENDIF
			
		YIELD
		JUMP  _CONTINUE_VMZ_UNLOAD
		
_VMZ_UNLOAD_DONE:
		MOVEVEND		MODE=GET
		POP
		APPLOG	"Successfully unloaded {0} discs to the VMZ." UNLOADED-DISCS
		JUMP	_END
	ENDIF
	
        LOG                 "Begin QLM unload request."
	
	APPLOG		CONFIGURE		SUBDIRECTORY="QLM"
	
	APPLOG		"Start QLM unload." 
		
	QLM		CLEAR-PICKER
	
	QLM		SETUP
	POP		SETUP-RESULT
	IF		FALSE == SETUP-RESULT 
		JUMP		_UNLOAD_DONE
	ENDIF
	
	POP		MAX-QLM-SLOTS
		
	MOTIONCONTROL		COMSTATUS
	POP		MC-STATUS
	IF	"OK" != MC-STATUS
		APPLOG	"Motion controller not available; attempt reboot."
		LOG		"Motion controller not available; attempt reboot." ENTRYTYPE=ERROR
		SYSCALL	REBOOT
		APPLOG	"Unload job restarted."
		LOG		"Restarting unload job."
		
		MOTIONCONTROL		COMSTATUS
		POP		MC-STATUS
		IF	"OK" != MC-STATUS
			APPLOG	"Communication with the motion controller isn't working."
			LOG		"Communication with the motion controller isn't working." ENTRYTYPE=ERROR
			EXIT		"The motion controller isn't working."
		ENDIF
	ENDIF

	SET		QLM-SLOT		1
	        
	RESULT				CODE="QlmUnloadStart" MESSAGE="Starting Qlm Unload run." END=BOTTOM
        DO					MAX-QLM-SLOTS
		QLM	UNLOAD-DISK	CURRENT-SLOT=QLM-SLOT
		POP	UNLOAD-RESULT
			
		IF	"MACHINEFULL" == UNLOAD-RESULT
			JUMP		_UNLOAD_DONE
		ENDIF
		
		IF	"HARDWAREERROR" == UNLOAD-RESULT
			JUMP 	_END
		ENDIF
		
		IF	"SUCCESS" == UNLOAD-RESULT
			INC	UNLOADED-DISCS
		ENDIF
		
		INC	QLM-SLOT
		YIELD
        LOOP
	
	APPLOG	"Successfully unloaded {0} discs to the QLM." UNLOADED-DISCS
        
        RESULT				CODE="QlmUnloadComplete" MESSAGE="The unload request completed." END=BOTTOM
_UNLOAD_DONE:
	
	QLM		ENDQLMOP
	
	MOTIONCONTROL		COMSTATUS
	POP		MC-STATUS
	IF	"OK" != MC-STATUS
		APPLOG	"Motion controller not available; attempt reboot."
		LOG		"Motion controller not available; attempt reboot." ENTRYTYPE=ERROR
		SYSCALL	REBOOT
		APPLOG	"Job restarted."
		LOG		"Restarting job."
		
		MOTIONCONTROL		COMSTATUS
		POP		MC-STATUS
		IF	"OK" != MC-STATUS
			APPLOG	"The job completed; however, communication with the motion controller isn't working."
			LOG		"The job completed; however, communication with the motion controller isn't working."	ENTRYTYPE=ERROR
			EXIT		"The motion controller isn't working."
		ENDIF
	ENDIF
	
_END:		
    	NOP
	
	
