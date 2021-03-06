// File Name:	WH_LGEDT.D 
// Purpose:	Selects a leg for editing
// Version:	1.02
// Date:		11.Jan.95
// Author:		Gary George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date		By	Action
// 03.Feb		gg	Explicit poke of Stretch and Reduction
//			To allow editing with stretch

   Display ("cursor", "wait", "Loading");
  
// Set path to macros
//
   STRING	 sPathJOBST;
   sPathJOBST = GetProfileString("JOBST", "PathJOBST", "\\JOBST", "DRAFIX.INI") ;

   UserSelection("clear") ;
   UserSelection("update") ;
   Execute ( "menu", "SetStyle", Table( "find",  "style", "bylayer"));
   Execute ( "menu", "SetColor", Table( "find",  "color", "bylayer"));

HANDLE
	hCurv,
	hEnt,
	hChan,
	hLeg,
	hFileCurve,
	hBody;
XY
	xyFold,
	xyOtemplate,
	xyAnkle,
	xyPt1,
	xyTmp;
STRING
	sProfileID,
	sStyleID,
	sOtemplateID,
	sAnkleID,
	sFoldID,
	sUnits,
	sData,
	sFabric,
	sTmp,
	s1, s2, s3, s4, s5, s6, s7, s8, s9, s10,
	sLeg,
	sFileNo,
	sType;
ANGLE
	aAngle;
NUMBER
	ii, nn, n1, n2, n3, n4, n5, n6, 
	nAnkle, nFabricClass ;

// Get profile, identify Leg and FileNo

   if(!hCurv = GetEntity ("Select a Leg Profile", "curve")) Exit (%ok, "User cancelled") ;

   Display ("cursor", "wait", "Searching for more data");
   GetDBData (hCurv, "ID", &sProfileID);
   nStringLength = StringLength(sProfileID);
   if (StringCompare( "LeftLegCurve", StringMiddle(sProfileID, nStringLength - 11, 12))){
	sStyleID = StringMiddle(sProfileID, 1, nStringLength - 12);
	sLeg = "Left";
	}
    if (StringCompare( "RightLegCurve", StringMiddle(sProfileID, nStringLength - 12, 13))) {
	sStyleID = StringMiddle(sProfileID, 1, nStringLength - 13);
	sLeg = "Right";
	}

// Check if this is a valid Leg Curve exit if not
    if (StringLength (sLeg) == 0) Exit (%cancel, "A Leg Profile was not selected\n") ;

// Get leg box data
   sFileNo = StringMiddle(sStyleID, 5, StringLength(sStyleID)-4) ;
   sStyleID= sStyleID + sLeg;
   sTmp = "DB SymbolName = 'waistleg' AND DB fileno = '" + sFileNo + "' AND  DB Leg = '" + sLeg + "'" ;

   hChan = Open ("selection", sTmp);
   hLeg = %zero ;
   if (hChan) {
	ResetSelection(hChan);
   	hLeg = GetNextSelection(hChan) ;
	Close("selection", hChan); 
	}

   if (hLeg) {
   	GetDBData( hLeg, "AnkleTape", &sTmp); 
	if (!ScanLine( sTmp, "blank", &nAnkleTape, &n1, &n2, &n3, &n4, &n5, &n6, &nFabricClass))
		Exit (%cancel, "Can't extract Ankle data "+ sLeg + " Leg\n") ;
	}
   else{
	Close("selection", hChan); 
	Exit (%cancel, "No data found for " + sLeg + " Leg\n") ;
	}
   Close("selection", hChan); 

   if (nAnkleTape < 0) Exit (%cancel, "Chosen Leg is FOOTLESS!\n") ;
   if (nFabricClass != 2) Exit (%cancel, "Chosen Leg is not Editable!\n") ;
   

// Get Marker data
   sOtemplateID = sStyleID + "Origin";
   sAnkleID = sStyleID + "Ankle";
   sFoldID = sStyleID  + "Fold";

   sTmp = "type = 'Marker' AND (DB ID ='" + sAnkleID + "' OR DB ID ='"  + sFoldID  + "' OR DB ID ='" + sOtemplateID + "')";

   hChan = Open ("selection", sTmp );
   nMarkersFound = 0;
   nMarkersRequired = 3;
   if (hChan) {
	ResetSelection(hChan);
	while (hEnt = GetNextSelection(hChan)) {
		GetDBData(hEnt,"ID", &sTmp) ;
		GetGeometry(hEnt, &sType, &xyPt1, &xyTmp, &aAngle) ;
	                if ( xyPt1.x != 0 || xyPt1.y != 0) {
			if (StringCompare(sTmp, sOtemplateID)) {
				nMarkersFound = nMarkersFound + 1;
				GetGeometry(hEnt, &sTmp, &xyOtemplate , &xyTmp, &aAngle) ;
				GetDBData( hEnt, "units", &sUnits);
				GetDBData( hEnt, "Data", &sData);
				}
			if (StringCompare(sTmp, sAnkleID)) {
				nMarkersFound = nMarkersFound + 1;
				GetGeometry(hEnt, &sTmp, &xyAnkle , &xyTmp, &aAngle) ;
				}
			if (StringCompare(sTmp, sFoldID)) {
				nMarkersFound = nMarkersFound + 1;
				GetGeometry(hEnt, &sTmp, &xyFold , &xyTmp, &aAngle) ;
				}
			}
		}
   	}
   Close("selection", hChan); 

// Check if that the markers have been found, otherwise exit
//
// For chap style there is no xyFold
// Unless the chap has been drawn
    if (nMarkersFound < nMarkersRequired && !StringCompare (sData, "CHAP") ) Exit (%cancel, "Missing markers for selected leg, data not found!\n") ;
    if (nMarkersFound > nMarkersRequired ) Exit (%cancel, "Two or more drawings of the same style exist!\nDelete the extra drawing/s and try again.\n") ;


// Write data to file for subsequent use

   hFileCurve = Open ("file",  "C:\\JOBST\\LEGCURVE.DAT", "write") ;
     SetData("UnitLinearType", 0);	// "Inches"

     PrintFile(hFileCurve,  xyOtemplate, "\n") ;
     PrintFile(hFileCurve,  xyAnkle, "\n") ;

     nLegVertexCount = GetVertexCount(hCurv) ;
     
     // If chap sytyle then we use last point on profile as a dummy fold
     if (StringCompare (sData, "CHAP")){
     	GetVertex (hCurv, nLegVertexCount , &xyPt1) ;
	PrintFile(hFileCurve,  xyPt1, "\n") ;
	}
     else
     	PrintFile(hFileCurve,  xyFold, "\n") ;
     
     ii = 1;
     while ( ii <= nLegVertexCount) {
	GetVertex (hCurv, ii, &xyPt1) ;
	PrintFile(hFileCurve,  xyPt1, "\n") ;
	ii = ii + 1;
	}
	SetData("UnitLinearType", 6);	// "Inches/Fraction"
   Close("file", hFileCurve) ;

// Start the application and get DDE link
//
    Execute ("application",  sPathJOBST + "\\WAIST\\lgedtdia", "normal");
    hDDE = Open ("dde", "lgedtdia", "lgedtdia");

    if (hLeg && hDDE) {
  	GetDBData( hLeg, "TapeLengthsPt1", &s1); 
  	GetDBData( hLeg, "TapeLengthsPt2", &s2); 
		sTmp = s1 + s2 ;
	Poke ( hDDE, "txtLeftLengths", sTmp );

  	GetDBData( hLeg, "TapeMMs", &s3); 
  	GetDBData( hLeg, "TapeMMs2", &s4); 
		sTmp = s3 + s4 ;
	Poke ( hDDE, "txtLeftMMs", sTmp );

  	GetDBData( hLeg, "Grams", &s5); 
  	GetDBData( hLeg, "Grams2", &s6); 
		sTmp = s5 + s6 ;
	Poke ( hDDE, "txtLeftStretch", sTmp );

  	GetDBData( hLeg, "Reduction", &s7); 
  	GetDBData( hLeg, "Reduction2", &s8); 
		sTmp = s7 + s8 ;
	Poke ( hDDE, "txtLeftRed", sTmp );

  	GetDBData( hLeg, "AnkleTape", &s9);
	Poke ( hDDE, "txtLeftAnkle", s9);

	Poke ( hDDE, "txtLeg", sLeg);
	Poke ( hDDE, "txtFileNo", sFileNo);

	Poke ( hDDE, "txtUnits",sUnits);

 	GetDBData( hLeg, "Fabric",  &sFabric);
	Poke ( hDDE, "txtFabric",sFabric);

	Poke ( hDDE, "txtUidLeftLeg", MakeString("long",UID("get",hLeg)));
	Poke ( hDDE, "txtUIDCurv", MakeString("long",UID("get",hCurv)));
 	}


    Close ( "dde", hDDE);


   
   Exit (%ok, "Waist Height Leg data EDIT transfer complete");
 