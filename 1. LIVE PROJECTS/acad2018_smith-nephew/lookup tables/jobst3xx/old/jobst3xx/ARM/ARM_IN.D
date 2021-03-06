// File Name:	ARM_IN.D
// Purpose: 	Get Patient Details From Drawing
//		And Pass Control To Visual Basic
//		For Arm Details
//		Called by the shells ARML.D and ARMR.D
// Version:	3
// Date:		19/July/94
// By:		Ciaran McKavanagh
//		G. George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date		By	Action
//---------------------------------------------------------------------------------------------
// Notes:
// 

Display ("cursor", "wait", "Loading Data");

HANDLE	
	hChan,
	hSym,
	hDDE,
	hEnt;
	
XY	
	xySymO,
	xyScale;
	
ANGLE
	aAngle;

STRING	
	sSymName,
	sSymSelection,
	sPathJOBST,
	sWorkOrder,
	// Strings are initialized to empty "", this ensures empty strings
	// are poked if there is no DB field.
	s1, s2, s3, s4, s5, s6, s10, s11, s20, s21, s22, s23, s24, s25, s26, s27,
	s28, s29, s30, s31, s32, s33, s34, s35;	

//STRING sSleeve; Set by calling shell

   
// Find JOBST installed directory
//
   sPathJOBST =  GetProfileString("JOBST", "PathJOBST", "C:\\JOBST", "DRAFIX.INI") ;

// Start the application and get DDE link
//
   Execute ("application",  sPathJOBST + "\\arm\\armdia", "normal");
   hDDE = Open ( "dde",  "armdia", "armdia");
   if ( !hDDE) Exit (%abort, "Unable to open \"armdia\" dialogue!") ;

// Get all of the symbols for uploading
//
   sSymSelection = 	"DB SymbolName = 'mainpatientdetails' OR " +
   		"(DB SymbolName = 'armarm' AND DB Sleeve ='"  + sSleeve +"') OR " +
   		"DB SymbolName = 'armcommon'"  ;

   hChan = Open ("selection", sSymSelection );  
   if (hChan && hDDE ) {
      	ResetSelection(hChan);
      	// Poke side
	Poke(hDDE, "txtSleeve",sSleeve);
	Poke(hDDE, "txtType", "ARM");
          	while (hSym = GetNextSelection(hChan)) {
   		GetGeometry(hSym, &sSymName, &xySymO, &xyScale, &aAngle);
      		if(StringCompare(sSymName, "mainpatientdetails" )){ 
			GetDBData( hSym, "fileno", &s1); Poke ( hDDE, "txtFileNo", s1);
			GetDBData( hSym, "patient", &s2); Poke ( hDDE, "txtPatientName", s2);
 			GetDBData( hSym, "age",  &s3); Poke ( hDDE, "txtAge", s3);
			GetDBData( hSym, "units", &s4); Poke ( hDDE, "txtinchflag",s4);
			GetDBData( hSym, "sex", &s5); Poke ( hDDE, "txtSEX",s5);
			GetDBData( hSym, "Diagnosis", &s6); Poke ( hDDE, "txtDiagnosis", s6);
			Poke ( hDDE, "txtUidMPD", MakeString("long",UID("get",hSym)));
			GetDBData( hSym, "WorkOrder", &s10); Poke ( hDDE, "txtWorkOrder",s10);
   			}
      		if(StringCompare(sSymName, "armcommon" )){ 
			GetDBData( hSym, "Fabric", &s11); Poke ( hDDE, "txtFabric",s11);
			Poke ( hDDE, "txtUidAC", MakeString("long",UID("get",hSym)));
   			}
      		if(StringCompare(sSymName, "armarm" )){ 
			GetDBData( hSym, "ID",&s21); Poke(hDDE, "txtID",s21);
			GetDBData( hSym, "WristPleat",&s22);	Poke(hDDE, "txtWristPleat1",s22);
			GetDBData( hSym, "ShoulderPleat",&s23); Poke(hDDE, "txtShoulderPleat1",s23);
			GetDBData( hSym, "Contracture",&s24); Poke(hDDE, "txtContractureWidth",s24);
			GetDBData( hSym, "Lining",&s25); Poke(hDDE, "txtLining",s25);
			GetDBData( hSym, "MM",&s26); Poke(hDDE, "txtMM",s26);
			GetDBData( hSym, "Stump",&s27); Poke(hDDE, "txtStump",s27);
			GetDBData( hSym, "TapeLengths",&s28); Poke(hDDE, "txtTapeLent",s28);
			GetDBData( hSym, "TapeMMs",&s29); Poke(hDDE, "txtTapeMM",s29);
			GetDBData( hSym, "Reduction",&s30); Poke(hDDE, "txtReduction",s30);
			GetDBData( hSym, "Grams",&s31); Poke(hDDE, "txtWeight",s31);
			GetDBData( hSym, "Flap",&s32); Poke(hDDE, "txtFlap",s32);
			GetDBData( hSym, "Gauntlet",&s33); Poke(hDDE, "txtGauntlet",s33);	
			Poke ( hDDE, "txtUidArm", MakeString("long",UID("get",hSym)));
   			}
   		}
	}
   
   Close ( "selection", hChan); 
   Close ( "dde", hDDE);
 
   Exit (%ok, "Patient and Arm Details transfered");

 