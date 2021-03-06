// File Name:	GLM_EDT.D
// Purpose:	Selects a Manual glove for editing
// Version:	1.01
// Date:		28.Mar.95
// Author:		Gary George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date		By	Action
// 30.Sep.99	GG	NT mods, Now writes vertex file before starting 
//			editor dialog.


   Display ("cursor", "wait", "Loading");
   UserSelection("clear") ;
   UserSelection("update") ;

HANDLE
	hEnt,
	hChan,
	hETS, hPALMER, hLFS, hEOS, hPALM6, 
	hFileCurve;

XY
	xyPALMER,
	xyPt1, xyEnt,
	xyPALM2, xyPALM3, xyPALM4, xyPALM5, xyPALM6, 
	xyTmp;

STRING
	sProfileID,
	sPathJOBST,
	sUnits,
	sFabric,
	sCurveType,
	sZipper,
	sType,
	sEntSelection,
	s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14,
	sSide;
ANGLE
	aAngle;
	
NUMBER
	ii, nn ;

// Get profile, identify Leg and FileNo
//
   hEnt = GetEntity ("Select a GLOVE Profile", "all") ;
   if (hEnt == %zero)Exit(%ok,"No GLOVE Profile selected");

   Display ("cursor", "wait", "Searching for more data");
   
   GetDBData (hEnt, "ID", &sProfileID);
   nStringLength = StringLength(sProfileID);
   if (StringCompare( "Left", StringMiddle(sProfileID, nStringLength - 3, 4))) sSide = "Left";
   if (StringCompare( "Right",StringMiddle(sProfileID, nStringLength - 4, 5))) sSide = "Right";

// Check if this is a valid Glove Arm Curve exit if not
    if (StringLength (sSide) == 0) Exit (%cancel, "A GLOVE Profile was not selected\n") ;
	
// Get Profile Origin Marker data
//
// Get all of the Entities for uploading
//
   sEntSelection = 	"(DB ID = '" + sProfileID + "' AND  DB Zipper = 'PALMER') OR " +
   		"(DB ID = '" + sProfileID + "' AND  DB Data = 'PALM*') OR " +
   		"(DB ID = '" + sProfileID + "' AND  DB curvetype = 'ETS') OR " +
   		"(DB ID = '" + sProfileID + "' AND  DB curvetype = 'LFS') OR " +
   		"(DB ID = '" + sProfileID + "' AND  DB Zipper = 'EOS')" ;
   hChan = Open ("selection", sEntSelection );
   nFound = 0;
   nRequired = 9;
   if (hChan) {
	ResetSelection(hChan);
	while (hEnt = GetNextSelection(hChan)) {
		GetEntityClass(hEnt, &sType) ;
		if (StringCompare(sType, "marker")) {
			GetGeometry(hEnt, &sType, &xyEnt, &xyTmp, &aAngle) ;
			GetDBData ( hEnt, "Zipper", &sCurveType) ;
			if (StringCompare(sCurveType, "PALMER")){
				xyPALMER = xyEnt;
				nFound = nFound + 1;
				hPALMER = hEnt ;
				}
			GetDBData ( hEnt, "Data", &sCurveType) ;
			if (StringCompare(sCurveType, "PALM6")){
				nFound = nFound + 1;
				xyPALM6 = xyEnt;
				hPALM6 = hEnt ;
				}
			if (StringCompare(sCurveType, "PALM5")){
				nFound = nFound + 1;
				xyPALM5 = xyEnt;
				}
			if (StringCompare(sCurveType, "PALM4")){
				nFound = nFound + 1;
				xyPALM4 = xyEnt;
				}
			if (StringCompare(sCurveType, "PALM3")){
				nFound = nFound + 1;
				xyPALM3 = xyEnt;
				}
			if (StringCompare(sCurveType, "PALM2")){
				nFound = nFound + 1;
				xyPALM2 = xyEnt;
				}
			}
		if (StringCompare(sType, "line") || StringCompare(sType, "curve")  || StringCompare(sType, "polyline")) {
			GetDBData ( hEnt, "curvetype", &sCurveType) ;
			GetDBData ( hEnt, "Zipper", &sZipper) ;
			if (StringCompare(sCurveType, "ETS")){
				nFound = nFound + 1;
				hETS = hEnt ;
				} 
			if (StringCompare(sZipper, "EOS")){
				nFound = nFound + 1;
				hEOS = hEnt ;
				} 
			if (StringCompare(sCurveType, "LFS")){
				nFound = nFound + 1;
				hLFS = hEnt ;
				} 
			}
		}
   	}
   Close("selection", hChan); 

// Check if that the markers have been found, otherwise exit
//
    if (nFound < nRequired ) Exit (%cancel, "Data Not found, Missing Marker or Curves!\nor\nThis is not an editable glove.") ;
    if (nFound > nRequired ) Exit (%cancel, "Two or more drawings of the same style exist!\nDelete the extra drawing/s and try again.\n") ;

// Find JOBST installed directory
//
   sPathJOBST =  GetProfileString("JOBST", "PathJOBST", "C:\\JOBST", "DRAFIX.INI") ;

// Write data to file for subsequent use in editor
//
   hFileCurve = Open ("file", "C:\\JOBST\\ARMCURVE.DAT", "write") ;
     SetData("UnitLinearType", 0);	// "Inches"

     // Wrist and Other marker positions 
     PrintFile(hFileCurve,  xyPALMER, "\n") ;
     PrintFile(hFileCurve,  xyPALM2, "\n") ;
     PrintFile(hFileCurve,  xyPALM3, "\n") ;
     PrintFile(hFileCurve,  xyPALM4, "\n") ;
     PrintFile(hFileCurve,  xyPALM5, "\n") ;
     PrintFile(hFileCurve,  xyPALM6, "\n") ;
     // Little finger side		
     nVertexCount = GetVertexCount(hLFS) ;
     PrintFile(hFileCurve,  nVertexCount , "\n") ;
     ii = 1;
     while ( ii <= nVertexCount) {
	GetVertex (hLFS, ii, &xyPt1) ;
	PrintFile(hFileCurve,  xyPt1, "\n") ;
	ii = ii + 1;
	}
	
     // Thumb side
     nVertexCount = GetVertexCount(hETS) ;
     PrintFile(hFileCurve,  nVertexCount , "\n") ;
     ii = 1;
     while ( ii <= nVertexCount) {
	GetVertex (hETS, ii, &xyPt1) ;
	PrintFile(hFileCurve,  xyPt1, "\n") ;
	ii = ii + 1;
	}
      SetData("UnitLinearType", 6);	// "Inches/Fraction"

   Close("file", hFileCurve) ;
   

// Start the application and get DDE link
//
    Execute ("application", sPathJOBST  + "\\gloveman\\manglved", "normal");
    hDDE = Open ("dde", "manglved", "manglved");

    if ( hPALMER && hDDE) {
  	GetDBData( hPALMER , "TapeLengthPt1", &s1); 
	Poke ( hDDE, "txtTapeLengthsPt1", s1 );

  	GetDBData( hPALMER , "TapeMMs", &s2); 
	Poke ( hDDE, "txtTapeMMs", s2 );

  	GetDBData( hPALMER , "Grams", &s3); 
	Poke ( hDDE, "txtGrams", s3 );

  	GetDBData( hPALMER , "Reduction", &s4); 
	Poke ( hDDE, "txtReduction", s4);
	
  	GetDBData( hPALMER , "Contracture", &s5); 
	Poke ( hDDE, "txtContracture", s5 );
	
  	GetDBData( hPALMER , "Lining", &s6); 
	Poke ( hDDE, "txtLining", s6 );
	
  	GetDBData( hPALMER , "age", &s7); 
	Poke ( hDDE, "txtAge", s7 );
	
 	GetDBData( hPALMER , "Fabric",  &s9);
	Poke ( hDDE, "txtFabric",s9);
	
 	GetDBData( hPALMER , "units",  &s10);
	Poke ( hDDE, "txtUnits",s10);
	
 	GetDBData( hPALMER , "Data",  &s11);
	Poke ( hDDE, "txtData",s11);
	
 	GetDBData( hPALMER , "MM",  &s12);
	Poke ( hDDE, "txtMM",s12);
	
  	GetDBData( hPALMER , "TapeLengths2", &s13); 
	Poke ( hDDE, "txtTapeLengths2", s13 );

	Poke ( hDDE, "txtSide", sSide);	
	
	Poke ( hDDE, "txtID", sProfileID);	
	Poke ( hDDE, "txtUIDPALMER", MakeString("long",UID("get",hPALMER)));
	Poke ( hDDE, "txtUIDPALM6", MakeString("long",UID("get",hPALM6)));
	Poke ( hDDE, "txtUIDLFS", MakeString("long",UID("get",hLFS)));
	Poke ( hDDE, "txtUIDETS", MakeString("long",UID("get",hETS)));
	Poke ( hDDE, "txtUIDEOS", MakeString("long",UID("get",hEOS)));
 	}

    Close ( "dde", hDDE);
   
   Exit (%ok, "Glove ARM data EDIT transfer complete");
 