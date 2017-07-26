	#
        # THIN Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.10.0.32
        #
        
        LOG                 "Begin thin request."
	SET		THINNED-DISCS	0
	
	VMZ		IS-CONFIGURED
	POP		VMZ-CONFIGURED
	IF	TRUE == VMZ-CONFIGURED
		APPLOG		CONFIGURE	SUBDIRECTORY="VMZ"
		APPLOG	"Thin: this job is not supported on VMZ kiosks. Try thin-vmz instead."
		EXIT	"Thin is not supported on VMZ; try thin-vmz."
	ENDIF
	
	APPLOG		CONFIGURE	SUBDIRECTORY="QLM"

	APPLOG	"Start thin."  

	GRIPPER	CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
	
	QLM		SETUP	CHECK-SLOTS=FALSE
	POP		SETUP-RESULT
	IF	FALSE == SETUP-RESULT
		JUMP	_DISENGAGE
	ENDIF
	
	POP
	
	MOTIONCONTROL		COMSTATUS
	POP		MC-STATUS
	IF	"OK" != MC-STATUS
		APPLOG	"Motion controller not available; attempt reboot."
		LOG		"Motion controller not available; attempt reboot." ENTRYTYPE=ERROR
		SYSCALL	REBOOT
		APPLOG	"Thin job restarted."
		LOG		"Restarting thin job."
		
		MOTIONCONTROL		COMSTATUS
		POP		MC-STATUS
		IF	"OK" != MC-STATUS
			APPLOG	"Communication with the motion controller isn't working."
			LOG		"Communication with the motion controller isn't working." ENTRYTYPE=ERROR
			EXIT		"The motion controller isn't working."
		ENDIF
	ENDIF
	
        ; The top of the stack should have the number of items to thin.
        POP					ITEMS-TO-THIN
        LOG                 "Thinning {0} items" ITEMS-TO-THIN
	APPLOG                 "Thinning {0} items" ITEMS-TO-THIN
	
        ; Loop the number of items to thin, popping the next item to thin off the top of the stack.
	RESULT				CODE="ThinStart" MESSAGE="Starting Thin run." END=BOTTOM
        DO					ITEMS-TO-THIN
		POP	ITEM-TO-THIN
		
		QLM		THIN-DISK	ID=ITEM-TO-THIN
		POP		THIN-RESULT
		
		IF	"MACHINEFULL" == THIN-RESULT
			JUMP		_THIN_DONE
		ENDIF
		
		IF	"QLMFULL" == THIN-RESULT
			JUMP		_THIN_DONE
		ENDIF
		
		IF	"HARDWAREERROR" == THIN-RESULT
			JUMP		_REAL_THIN_END
		ENDIF
		
		IF	"SUCCESS" == THIN-RESULT
			INC	THINNED-DISCS
		ENDIF
		
		YIELD
	LOOP
   
_THIN_DONE:   
        RESULT              CODE="ThinComplete" MESSAGE="The thin run completed." END=BOTTOM
	APPLOG		"Successfully thinned {0} discs to the QLM." THINNED-DISCS

	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF

_DISENGAGE:
	QLM		ENDQLMOP
		
	MOTIONCONTROL		COMSTATUS
	POP		MC-STATUS
	IF	"OK" != MC-STATUS
		APPLOG	"Motion controller not available; attempt reboot."
		LOG		"Motion controller not available; attempt reboot." ENTRYTYPE=ERROR
		SYSCALL	REBOOT
		APPLOG	"Thin job restarted."
		LOG		"Restarting thin job."
		
		MOTIONCONTROL		COMSTATUS
		POP		MC-STATUS
		IF	"OK" != MC-STATUS
			APPLOG	"The job completed; however, communication with the motion controller isn't working."
			LOG		"The job completed; however, communication with the motion controller isn't working."	ENTRYTYPE=ERROR
			EXIT		"The motion controller isn't working."
		ENDIF
	ENDIF
	
_REAL_THIN_END:
	NOP

	
