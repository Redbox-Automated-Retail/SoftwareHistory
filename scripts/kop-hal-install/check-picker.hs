        #
        # CHECK PICKER Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.10.0.32
        #

	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"PICKEREMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
