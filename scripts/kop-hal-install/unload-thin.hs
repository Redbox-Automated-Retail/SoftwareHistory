        #
        # QLM-UNLOAD Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.12.1.64
        #

        LOG                 "Begin QLM unload request."
	
	APPLOG		CONFIGURE	SUBDIRECTORY="QLM"
	
	APPLOG			"Start QLM unload." 
	
	VMZ		IS-CONFIGURED
	POP		VMZ-STATUS
	IF	TRUE == VMZ-STATUS
		APPLOG	"The unload-thin job is unsupported for the VMZ."
		EXIT		"Unsupported job."
	ENDIF
		
	SET		UNLOADED-DISCS	0
		
	QLM			CLEAR-PICKER
		
	QLM		SETUP
	POP		SETUP-RESULT
	IF	FALSE == SETUP-RESULT
		JUMP	_JOB_DONE
	ENDIF
	
	POP		MAX-QLM-SLOTS
		 
	SET					QLM-SLOT 1
	        
	RESULT				CODE="QlmUnloadStart" MESSAGE="Starting Qlm Unload run." END=BOTTOM
        DO					MAX-QLM-SLOTS
		QLM	UNLOAD-DISK	CURRENT-SLOT=QLM-SLOT
		POP	UNLOAD-RESULT
			
		IF	"MACHINEFULL" == UNLOAD-RESULT
			JUMP		_DISENGAGE
		ENDIF
		
		IF	"HARDWAREERROR" == UNLOAD-RESULT
			JUMP 	_END
		ENDIF
		
		IF	"SUCCESS" == UNLOAD-RESULT
			INC	UNLOADED-DISCS
		ENDIF
		
		INC				QLM-SLOT
		YIELD
        LOOP
	
	APPLOG	"Successfully unloaded {0} discs to the QLM." UNLOADED-DISCS
        
        RESULT				CODE="QlmUnloadComplete" MESSAGE="The unload request completed." END=BOTTOM

        LOG                 "Begin thin request."

	APPLOG	"Start thin."  

	GRIPPER	CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
	
			        
        ; The top of the stack should have the number of items to thin.
        POP					ITEMS-TO-THIN
        LOG                 "Thinning {0} items" ITEMS-TO-THIN
		
	SET		THINNED-DISCS	0		        
	
        ; Loop the number of items to thin, popping the next item to thin off the top of the stack.
	RESULT				CODE="ThinStart" MESSAGE="Starting Thin run." END=BOTTOM
        DO					ITEMS-TO-THIN
		POP	ITEM-TO-THIN
		
		QLM		THIN-DISK	ID=ITEM-TO-THIN
		POP		THIN-RESULT
		
		IF	"QLMFULL" == THIN-RESULT
			JUMP		_JOB_DONE
		ENDIF
		
		IF	"HARDWAREERROR" == THIN-RESULT
			JUMP		_END
		ENDIF
		
		IF	"SUCCESS" == THIN-RESULT
			INC	THINNED-DISCS
		ENDIF
		
		YIELD
	LOOP
  
_JOB_DONE:
	APPLOG		"Successfully thinned {0} discs to the QLM." THINNED-DISCS
        RESULT              CODE="ThinComplete" MESSAGE="The thin run completed." END=BOTTOM

	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
	
_DISENGAGE:
	
	QLM		ENDQLMOP
_END:		
    	NOP
	
	
