// File Name:	ARM_EDT.D
// Purpose:	Selects an arm for editing
// Version:	1.01
// Date:		28.Mar.95
// Author:		Gary George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date		By	Action
// 30.Sep.99	GG	NT mods. Writes file before starting
//			editor dialog 

   Display ("cursor", "wait", "Loading");
   UserSelection("clear") ;
   UserSelection("update") ;

HANDLE
	hCurv,
	hEnt,
	hChan,
	hArm,
	hFileCurve;

XY
	xyOtemplate,
	xyPt1,
	xyTmp;

STRING
	sProfileID,
	sOtemplateID,
	sPathJOBST,
	sUnits,
	sFabric,
	sTmp,
	sType,
	sTmp1,
	sArm,
	sStyle;
ANGLE
	aAngle;
NUMBER
	ii, nn ;

// Get profile, identify Leg and FileNo

   hCurv = GetEntity ("Select a Arm Profile", "curve") ;
   if (hCurv == %zero)Exit(%ok,"No Arm Profile selected");

   Display ("cursor", "wait", "Searching for more data");
   GetDBData (hCurv, "ID", &sProfileID);
   nStringLength = StringLength(sProfileID);
   if (StringCompare( "LeftProfile", StringMiddle(sProfileID, nStringLength - 10, 11))){
	sStyle = StringMiddle(sProfileID, 1, nStringLength - 11);
	sArm = "Left";
	}
    if (StringCompare( "RightProfile",StringMiddle(sProfileID, nStringLength - 11, 12))) {
	sStyle = StringMiddle(sProfileID, 1, nStringLength - 12);
	sArm = "Right";
	}

// Check if this is a valid Arm Curve exit if not
    if (StringLength (sArm) == 0) Exit (%cancel, "An ARM Profile was not selected\n") ;


// Get Profile Origin Marker data
//
   sOtemplateID = sStyle + sArm + "originmark";
   sTmp = "type = 'Marker' AND DB ID = '" + sOtemplateID + "'";
   hChan = Open ("selection", sTmp );
   nMarkersFound = 0;
   if (hChan) {
	ResetSelection(hChan);
	while (hEnt = GetNextSelection(hChan)) {
		GetDBData(hEnt,"ID", &sTmp) ;
		GetGeometry(hEnt, &sType, &xyPt1, &xyTmp, &aAngle) ;
	                if ( xyPt1.x != 0 || xyPt1.y != 0) {
			if (StringCompare(sTmp, sOtemplateID)) {
				GetGeometry(hEnt, &sType, &xyOtemplate, &xyTmp, &aAngle) ;
				nMarkersFound = nMarkersFound + 1;
				hArm = hEnt ;
				}
			}
		}
   	}
   Close("selection", hChan); 

// Check if that the markers have been found, otherwise exit
//
    if (nMarkersFound < 1) Exit (%cancel, "Missing origin marker for selected ARM, data not found!\n") ;
    if (nMarkersFound > 1) Exit (%cancel, "Two or more drawings of the same style exist!\nDelete the extra drawing/s and try again.\n") ;

// Find JOBST installed directory
//
   sPathJOBST =  GetProfileString("JOBST", "PathJOBST", "C:\\JOBST", "DRAFIX.INI") ;
   
// Write data to file for subsequent use in editor
//
   hFileCurve = Open ("file", "C:\\JOBST\\ARMCURVE.DAT", "write") ;
     
     SetData("UnitLinearType", 0);	// "Inches"
     PrintFile(hFileCurve,  xyOtemplate, "\n") ;

     nVertexCount = GetVertexCount(hCurv) ;
     ii = 1;
     while ( ii <= nVertexCount) {
	GetVertex (hCurv, ii, &xyPt1) ;
	PrintFile(hFileCurve,  xyPt1, "\n") ;
	ii = ii + 1;
	}
      SetData("UnitLinearType", 6);	// "Inches/Fraction"

   Close("file", hFileCurve) ;


// Start the application and get DDE link
//
    Execute ("application", sPathJOBST  + "\\arm\\armeddia", "normal");
    hDDE = Open ("dde", "armeddia", "armeddia");

    if (hArm && hDDE) {
  	GetDBData( hArm, "TapeLengths", &sTmp); 
	Poke ( hDDE, "txtTapeLengths", sTmp );

  	GetDBData( hArm, "TapeMMs", &sTmp); 
	Poke ( hDDE, "txtTapeMMs", sTmp );

  	GetDBData( hArm, "Grams", &sTmp); 
	Poke ( hDDE, "txtGrams", sTmp );

  	GetDBData( hArm, "Reduction", &sTmp); 
	Poke ( hDDE, "txtReduction", sTmp );
	
  	GetDBData( hArm, "Contracture", &sTmp); 
	Poke ( hDDE, "txtContracture", sTmp );
	
  	GetDBData( hArm, "Lining", &sTmp); 
	Poke ( hDDE, "txtLining", sTmp );
	
  	GetDBData( hArm, "Stump", &sTmp); 
	Poke ( hDDE, "txtStump", sTmp );

 	GetDBData( hArm, "Fabric",  &sTmp);
	Poke ( hDDE, "txtFabric",sTmp);
	
 	GetDBData( hArm, "Gauntlet",  &sTmp);
	Poke ( hDDE, "txtGauntlet",sTmp);

 	GetDBData( hArm, "units",  &sTmp);
	Poke ( hDDE, "txtUnits",sTmp);
	
 	GetDBData( hArm, "MM",  &sTmp);
	Poke ( hDDE, "txtMM",sTmp);

	Poke ( hDDE, "txtArm", sArm);	
	Poke ( hDDE, "txtID", sStyle);
	Poke ( hDDE, "txtUIDArm", MakeString("long",UID("get",hArm)));
	Poke ( hDDE, "txtUIDCurve", MakeString("long",UID("get",hCurv)));
 	}

    Close ( "dde", hDDE);
   
   Exit (%ok, "Arm data EDIT transfer complete");
 