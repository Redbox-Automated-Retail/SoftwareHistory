	#
	# Quick Return Setup
	# Copyright (C) 2011 Redbox Automated Retail, LLC
	# All Rights Reserved
	# VERSION 1.3.0.119
	#

	QUICKRETURN		POLLSTATUS
	POP		QR-STATUS
	IF	"ACTIVE" != QR-STATUS
		JUMP		_NOT_CONFIGURED
	ENDIF
	
	QUICKRETURN RESET
	POP		RESET-STATUS
	IF	FALSE == RESET-STATUS
		LOG	"Unable to reset quick return device." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	
	QUICKRETURN DEFSTATE NAME="Start" FILE="start.grf" LEDSTATES="led=Background,mode=On;led=GreenButton,mode=Blink;led=GreenArrow,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off" 
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	QUICKRETURN DEFSTATE NAME="Busy" FILE="busy.grf" LEDSTATES="led=Background,mode=On;led=GreenButton,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=On;led=RedArrow,mode=Off" 
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	QUICKRETURN DEFSTATE NAME="Success" FILE="success.grf" LEDSTATES="led=Background,mode=On;led=GreenButton,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off"
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	QUICKRETURN DEFSTATE NAME="Insert-Error" FILE="insert-error.grf" LEDSTATES="led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=Blink;led=RedArrow,mode=Off" 
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	QUICKRETURN DEFSTATE NAME="Return-Error" FILE="return-error.grf" LEDSTATES="led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off" 
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	QUICKRETURN DEFSTATE NAME="Return" FILE="return.grf" LEDSTATES="led=Background,mode=On;led=GreenButton,mode=Off;led=GreenArrow,mode=Blink;led=RedButton,mode=Off;led=RedArrow,mode=Off"
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	QUICKRETURN DEFSTATE NAME="Error" FILE="Error.grf" LEDSTATES="led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenButton,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off"
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	
	QUICKRETURN DEFSTATE NAME="Wait" FILE="Wait.grf" LEDSTATES="led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenButton,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off"
	POP		CREATE-STATUS
	IF		CREATE-STATUS == FALSE
		LOG	"Unable to create state." ENTRYTYPE=ERROR
		JUMP	_EXIT_ON_ERROR
	ENDIF
	
	JUMP		_NOT_CONFIGURED
		
_EXIT_ON_ERROR:
	EXIT "Unable to setup the quick return device."
	
_NOT_CONFIGURED:
	NOP
	
