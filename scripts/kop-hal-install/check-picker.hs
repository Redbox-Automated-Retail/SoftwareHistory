        #
        # CHECK PICKER Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.3.0.84
        #

	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"EMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF