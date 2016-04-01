	#
        # Clean VMZ Aggregate Instruction
        # Copyright (C) 2012 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.8.0.22
        #
	
	APPLOG		CONFIGURE	SUBDIRECTORY="VMZ"
	
	VMZ IS-CONFIGURED
	POP	VMZ-CONFIGURED
	IF FALSE == VMZ-CONFIGURED
		APPLOG  "The clean-vmz job is not supported on non-VMZ machines."
		EXIT		"The clean-vmz job is not supported on non-VMZ machines."
	ENDIF
	
	HOMEY
	POP                 HOMEY-RESULT
	IF                  HOMEY-RESULT != "SUCCESS"
		RESULT			CODE=HOMEY-RESULT MESSAGE="The HOMEY instruction failed." 
		VMZ		AGGREGATE-CLEAN
		EXIT            "The homing operation on the Y axis failed."
	ENDIF	
	
	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		VMZ		AGGREGATE-CLEAN
		EXIT	"Unable to clear the picker."
	ENDIF
	
	SET	TRANSFERRED-DISKS	0

	VMZ REGISTER-CLEAN
	
_MAIN_LOOP:
		VMZ		CLEAN-SLOT
		POP		CLEAN-RESULT
		
		IF	"CLEAN" == CLEAN-RESULT
			JUMP		_FINISH
		ENDIF
		
		IF	"EMPTYSTUCK" == CLEAN-RESULT
			JUMP DO_YIELD
		ENDIF
		
		IF	"HARDWAREERROR" == CLEAN-RESULT
			VMZ		AGGREGATE-CLEAN
			EXIT	"There was a hardware error during execution."
		ENDIF
		
		IF	"MACHINEFULL" == CLEAN-RESULT
			JUMP		_FINISH
		ENDIF
		
		IF	"TRANSFERFAILURE" == CLEAN-RESULT
			VMZ		AGGREGATE-CLEAN
			EXIT	"There was a disk transfer failure."
		ENDIF
		
		INC	TRANSFERRED-DISKS
		
DO_YIELD:
		YIELD
		
		JUMP		_MAIN_LOOP
	
_FINISH:
	APPLOG	"Clean transferred {0} disks." TRANSFERRED-DISKS
	
	VMZ		AGGREGATE-CLEAN
	MOVEVEND	MODE=GET
	POP
	
	VMZ		DUMP-ZONE-TO-APPLOG
	DUMPBIN	DUMPINVENTORY

	NOP
