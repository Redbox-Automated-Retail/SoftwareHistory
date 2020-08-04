	#
        # THIN VMZ Aggregate Instruction
        # Copyright (C) 2012 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.12.1.71
        #
	
	APPLOG		CONFIGURE	SUBDIRECTORY="VMZ"
	
	SET		THIN-JOB-STATUS	"SUCCESS"
	
	MOTIONCONTROL	HOMEALL	CHECKCOM="TRUE"
	POP                 HOMEALL-RESULT
	IF                  TRUE != HOMEALL-RESULT 
		RESULT			CODE="HOMEAXESFAILURE" MESSAGE="Homing the axes failed." 
		EXIT            "The homing operation failed."
	ENDIF	
	
	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
	
	
	 ; The top of the stack should have the number of items to thin.
        POP					ITEMS-TO-BIN
        LOG                 "Try to PUT {0} items to bin." ITEMS-TO-BIN
	APPLOG            "Try to PUT {0} items to bin." ITEMS-TO-BIN
	
	SET		LOADED-DISKS	0
        ; Loop the number of items to thin, popping the next item to thin off the top of the stack.
	RESULT				CODE="ThinStart" MESSAGE="Starting Thin run." END=BOTTOM
        DO					ITEMS-TO-BIN
		POP	ID-TO-THIN
		
		DUMPBIN	FILEDISK ID=ID-TO-THIN
		POP		FILE-RESULT
		
		IF	"SUCCESS" == FILE-RESULT
			INC	LOADED-DISKS
		ENDIF
		
		IF	"HARDWAREERROR" == FILE-RESULT
			JUMP		_FINISH
		ENDIF
		
		IF	"DUMPBINFULL" == FILE-RESULT
			SET		TEMP-LOADED	LOADED-DISKS
			INC		TEMP-LOADED
			SUB		ITEMS-TO-BIN	TEMP-LOADED
			POP		DEPTH=ITEMS-TO-BIN
			JUMP		_START_REBALANCE
		ENDIF
		YIELD
	LOOP
	
_START_REBALANCE:
	APPLOG	"Successfully placed {0} disks to the bin."   LOADED-DISKS
	
	SET		REBALANCED-DISKS 0
	
	SET		THIN-JOB-STATUS	"SUCCESS"
	
	APPLOG	"Start thin of rebalance disks."  
	
	POP					REBALANCE-ITEMS
	LOG                 "Thinning {0} rebalance items to the VMZ." REBALANCE-ITEMS
	APPLOG                 "Thinning {0} rebalance items to the VMZ." REBALANCE-ITEMS
	DO					REBALANCE-ITEMS
		POP				ID-TO-THIN
		
		VMZ		THIN-REBALANCE	ID=ID-TO-THIN
		POP				THIN-RESULT
		
		IF	"MACHINEFULL" == THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
		
		IF	"VMZFULL" == THIN-RESULT
			SET		TEMP-REBALANCE	REBALANCED-DISKS
			INC		TEMP-REBALANCE
			SUB		REBALANCE-ITEMS	TEMP-REBALANCE
			POP		DEPTH=REBALANCE-ITEMS
			JUMP		_THIN_DISKS
		ENDIF
		
		IF	"TRANSFERFAILURE" == THIN-RESULT
			SET		THIN-JOB-STATUS	THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
	
		IF	"HARDWAREERROR" == THIN-RESULT
			RESULT			CODE="MachineError" MESSAGE="There was a hardware error thinning the disk." END=BOTTOM
			EXIT		"There was a hardware error during the thin."
		ENDIF
		
		IF	"SUCCESS" == THIN-RESULT
			INC		REBALANCED-DISKS
		ENDIF
			
		YIELD
	LOOP
	
	APPLOG	"Successfully placed {0} disks to the VMZ."   REBALANCED-DISKS
	
	SET		REDEPLOYED-DISKS 0
	
	SET		THIN-JOB-STATUS	"SUCCESS"
	
	APPLOG	"Start thin of redeploy disks."  
	
	POP					REDEPLOY-ITEMS
	LOG                 "Thinning {0} redeploy items to the VMZ." REDEPLOY-ITEMS
	APPLOG                 "Thinning {0} redeploy items to the VMZ." REDEPLOY-ITEMS
	DO					REDEPLOY-ITEMS
		POP				ID-TO-THIN
		
		VMZ		THIN-REDEPLOY	ID=ID-TO-THIN
		POP				THIN-RESULT
		
		IF	"MACHINEFULL" == THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
		
		IF	"VMZFULL" == THIN-RESULT
			SET		TEMP-REDEP	REDEPLOYED-DISKS
			INC		TEMP-REDEP
			SUB		REDEPLOYED-DISKS	TEMP-REDEP
			POP		DEPTH=REBALANCE-ITEMS
			JUMP		_THIN_DISKS
		ENDIF
		
		IF	"TRANSFERFAILURE" == THIN-RESULT
			SET		THIN-JOB-STATUS	THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
	
		IF	"HARDWAREERROR" == THIN-RESULT
			RESULT			CODE="MachineError" MESSAGE="There was a hardware error thinning the disk." END=BOTTOM
			EXIT		"There was a hardware error during the thin."
		ENDIF
		
		IF	"SUCCESS" == THIN-RESULT
			INC		REDEPLOYED-DISKS
		ENDIF
			
		YIELD
	LOOP
	
	APPLOG	"Successfully placed {0} redeployed disks to the VMZ."   REDEPLOYED-DISKS
	
_THIN_DISKS:
	
	SET		THINNED-DISCS	0
	POP					ITEMS-TO-THIN
	LOG                 "Thinning {0} items to the VMZ." ITEMS-TO-THIN
	APPLOG                 "Thinning {0} items to the VMZ." ITEMS-TO-THIN
	DO					ITEMS-TO-THIN
		POP				ID-TO-THIN
		
		VMZ		THIN-DISC	ID=ID-TO-THIN
		POP				THIN-RESULT
		
		IF	"MACHINEFULL" == THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
		
		IF	"VMZFULL" == THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
		
		IF	"DUMPBINFULL" == THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
		
		IF	"TRANSFERFAILURE" == THIN-RESULT
			SET		THIN-JOB-STATUS	THIN-RESULT
			JUMP		_VMZ_THIN_DONE
		ENDIF
	
		IF	"HARDWAREERROR" == THIN-RESULT
			RESULT			CODE="MachineError" MESSAGE="There was a hardware error thinning the disk." END=BOTTOM
			EXIT		"There was a hardware error during the thin."
		ENDIF
		
		IF	"SUCCESS" == THIN-RESULT
			INC		THINNED-DISCS 
		ENDIF
			
		YIELD
	LOOP
 
_VMZ_THIN_DONE: 
	APPLOG	"Successfully thinned {0} disks."   THINNED-DISCS
	RESULT              CODE="ThinComplete" MESSAGE="The thin run completed." END=BOTTOM


	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
	VMZ		DUMP-ZONE-TO-APPLOG
	DUMPBIN	DUMPINVENTORY
	
	MOVEVEND
	POP					MOVE-RESULT
	IF					MOVE-RESULT != "SUCCESS"
		EXIT			"The MOVEVEND instruction failed."
	ENDIF
	
_FINISH:
	IF	"SUCCESS" != THIN-JOB-STATUS
		EXIT		"The job stopped with an error status."
	ENDIF
	NOP
