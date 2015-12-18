        #
        # SYNC Aggregate Instruction
        # Copyright (C) 2011 Redbox Automated Retail, LLC
        # All Rights Reserved
	# VERSION 1.4.0.246
        #

	LOG			"Begin sync request."
	
	APPLOG		CONFIGURE		SUBDIRECTORY="Sync" 
	
	SET			YIELD-TIME		0
	QUICKRETURN		POLLSTATUS
	POP		QR-STATUS
	IF	"ACTIVE" == QR-STATUS
		SET	YIELD-TIME	2000
	ENDIF
	
	
	GRIPPER		CHECK-PICKER
	POP		CP-RESULT
	IF		"EMPTY" != CP-RESULT
		EXIT	"Unable to clear the picker."
	ENDIF
        
	DECK		MAXSLOTS NUMBER=1
	POP         MAX-SLOT-PER-DECK
        
	POP			START-DECK
	POP			START-SLOT
	POP         END-DECK
	POP         END-SLOT
		
	IF          END-SLOT > MAX-SLOT-PER-DECK
		SET			END-SLOT MAX-SLOT-PER-DECK
	ENDIF
    
	SET			LAST-SLOT MAX-SLOT-PER-DECK
    
	IF			START-DECK == END-DECK
		SET			LAST-SLOT END-SLOT
	ENDIF
        
	SET			CUR-DECK START-DECK
	SET         CUR-SLOT START-SLOT
        
	APPLOG		"Sync Deck={0} Slot={1} to Deck={2} Slot={3}" START-DECK START-SLOT END-DECK END-SLOT 
                
	GETCFG		"SyncErrorThreshold" TYPE=Controller
	POP			SYNC-ERROR-THRESHOLD
        
	SET			SYNC-ERRORS 1
	
_NEXT_ITEM:
	
	SYNC		DECK=CUR-DECK SLOT=CUR-SLOT
	POP	SYNC-RESULT
	
	IF	"GETERROR" == SYNC-RESULT
		INC	SYNC-ERRORS
	ENDIF

	IF			SYNC-ERRORS > 	SYNC-ERROR-THRESHOLD
		APPLOG		"Sync exceeded the error threshold." 
		RESULT		CODE="ErrorThresholdExceeded" MESSAGE="The number of sync errors exceeded the threshold."
		EXIT		"Sync failed."
	ENDIF
	
	INC			CUR-SLOT
	IF			CUR-SLOT > LAST-SLOT
		SET			CUR-SLOT 1
		INC			CUR-DECK
		IF			CUR-DECK > END-DECK
			JUMP		_SYNC_DONE
		ENDIF
		IF			CUR-DECK == END-DECK
			SET			LAST-SLOT END-SLOT
		ENDIF
	ENDIF
	YIELD		YIELD-TIME
	JUMP        _NEXT_ITEM
        
_SYNC_DONE:
	MOVEVEND	MODE=GET
	POP
	NOP
