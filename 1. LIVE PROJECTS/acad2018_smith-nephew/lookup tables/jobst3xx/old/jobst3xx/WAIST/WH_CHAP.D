// File Name:	WH_CHAP.D 
// Purpose:	Draw body portion of a Chap
//			Pokes all Data to a VB program
//			VB program then starts the drawing
//
// Version:	1.01
// Date:		14.Nov.96
// Author:		Gary George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date		By	Action
   Display ("cursor", "wait", "Loading");

HANDLE
	hDDE,
	hChan,
	hEnt,
	hMarker,
	hLeg;			// LEGBOX 

STRING
	sPathJOBST,
	sFileNo,
	sLeg,
	sClass,
	sTmp,
	sSymSelection,
	sID,
	sProfileID,
	sTmp1;
	
NUMBER
	nLegVertexCount ;
XY
	xyO,
	xyPt1,
	xyPt2,
	xyScale;
	
ANGLE
	aAngle;

// Find JOBST installed directory
//
   sPathJOBST =  GetProfileString("JOBST", "PathJOBST", "\\JOBST", "DRAFIX.INI") ;

// Clear User any user selections
//
   UserSelection("clear") ;
   UserSelection("update") ;
   Execute ( "menu", "SetStyle", Table( "find",  "style", "bylayer"));
   Execute ( "menu", "SetColor", Table( "find",  "color", "bylayer"));
 
// Get profile, identify Leg and FileNo

   if(!hCurv = GetEntity ("Select a Leg Profile", "curve line")) Exit (%ok, "User cancelled") ;

   Display ("cursor", "wait", "Searching for more data");
   GetDBData (hCurv, "ID", &sProfileID);
   nStringLength = StringLength(sProfileID);
   if (StringCompare( "LeftLegCurve", StringMiddle(sProfileID, nStringLength - 11, 12))){
	sID = StringMiddle(sProfileID, 1, nStringLength - 8);
	sLeg = "Left";
	}
    if (StringCompare( "RightLegCurve", StringMiddle(sProfileID, nStringLength - 12, 13))) {
	sID = StringMiddle(sProfileID, 1, nStringLength - 8);
	sLeg = "Right";
	}

// Check if this is a valid Leg Curve exit if not
    if (StringLength (sLeg) == 0) Exit (%cancel, "A Leg Profile was not selected\n") ;

   sSymSelection = "type = 'marker' AND DB ID = '" + sID + "Origin' AND DB Data = 'CHAP'" ;

// Get CHAP origin marker
//
   nMarkersRequired = 1 ;
   nMarkersFound = 0 ;
   
   hChan = Open ("selection",sSymSelection );
   if ( hChan) {
          ResetSelection(hChan);
          while (hEnt = GetNextSelection(hChan)) {
          	hMarker = hEnt;
             	GetGeometry(hMarker, &sTmp, &xyO, &xyScale, &aAngle);
             	nMarkersFound = nMarkersFound + 1 ;
             	}
          }
   Close("selection", hChan);
             	
// Check if that the markers have been found, otherwise exit
//
    if (nMarkersFound < nMarkersRequired) Exit (%cancel, "This is not a CHAP leg or there are missing markers!\n") ;
    if (nMarkersFound > nMarkersRequired) Exit (%cancel, "Two or more drawings of the same style exist!\nDelete the extra drawing/s and try again.\n") ;


// Start the WHDRAW application and get DDE link
//
   Execute ("application",sPathJOBST + "\\waist\\whdraw", "normal");
   hDDE = Open ("dde", "whdraw", "whdraw");
   
   if (hDDE && hMarker){ 
      		Poke ( hDDE, "txtDrawType", "CHAPBODY" + sLeg);
      		Poke ( hDDE, "txtID", sID);

   		GetDBData( hMarker, "TOSCir", &sTmp); 
		Poke ( hDDE, "txtTOSCir", sTmp);

   		GetDBData( hMarker, "TOSGivenRed", &sTmp); 
		Poke ( hDDE, "txtTOSRed", sTmp);

   		GetDBData( hMarker, "TOSHt", &sTmp); 
		Poke ( hDDE, "txtTOSHt", sTmp);

   		GetDBData( hMarker, "WaistCir", &sTmp);
		Poke ( hDDE, "txtWaistCir", sTmp);

   		GetDBData( hMarker, "WaistGivenRed", &sTmp);
		Poke ( hDDE, "txtWaistRed", sTmp);

   		GetDBData( hMarker, "WaistHt", &sTmp);
		Poke ( hDDE, "txtWaistHt", sTmp);

   		GetDBData( hMarker, "FoldHt", &sTmp);
		Poke ( hDDE, "txtFoldHt", sTmp);
		
   		GetDBData( hMarker, "Body", &sTmp);
		Poke ( hDDE, "txtLegStyle", sTmp);

   		GetDBData( hMarker, "units", &sTmp);
		Poke ( hDDE, "txtUnits", sTmp);
    }


// Write leg curve to file
// Do this now to avoid any contention with the VB programme
//
     GetEntityClass(hCurv, &sClass) ;
     hFileCurve = Open ("file",  "C:\\JOBST\\LEGCURVE.DAT", "write") ;
      SetData("UnitLinearType", 0);	// "Inches"
      PrintFile(hFileCurve,  xyO, "\n") ;
      if (StringCompare("curve", sClass)) {
      	nLegVertexCount = GetVertexCount(hCurv) ;
      	ii = 1;
      	while ( ii <= nLegVertexCount) {
		GetVertex (hCurv, ii, &xyPt1) ;
		PrintFile(hFileCurve,  xyPt1, "\n") ;
		ii = ii + 1;
		}
	}
     else {
     	GetGeometry(hCurv, &xyPt1, xyPt2) ;
	PrintFile(hFileCurve,  xyPt1, "\n") ;
	PrintFile(hFileCurve,  xyPt2, "\n") ;
     	}
	SetData("UnitLinearType", 6);	// "Inches/Fraction"
     Close("file", hFileCurve) ;

    Close ( "dde", hDDE);
    
 
 