// File Name:	WHZIPANK.D
// Purpose:	Draws an Ankle Zip
//		Waist Height Leg
// Version:	1.01
// Date:		15.Dec.1994
// Author:		Gary George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date		By	Action

HANDLE
	hDlg,
	hEnt,
	hEOS,
	hChan,
	hLeg,
	hTextEnt,
	hLayer;

XY
	xyInt,		// FN_CirLinInt return value
	xySmallestCir,
	xyConstruct,
	xyStart,
	xyEnd,
	xyCurveStart,
	xyHorizStart,
	xyHorizEnd,
	xyEOSCen,
	xyMax, xyMin,
	xyAnkle,
	xyHeel,
	xyCO_WaistBott, 
	xyOtemplate,
	xyPt1,
	xyTmp;

STRING
	sClass,
	sElastic,
	sMedial,
	sZipperStyle,
	sUnits,
	sZipLength,
	sProfileID,
	sHeelID,
	sAnkleID,
	sOtemplateID,
	sCO_WaistBottID,
	sTmp, sTmp1,
	sLeg,
	sStyleID,
	sDlgElasticList ,
	sDlgLengthList,
	sFileNo,
	sTemplate,
	sZipperID,
	sType;

NUMBER
	nMedial,
	nElastic,
	nType,
	ii, nn,
	nY,nX,
	nEOSRad,
	nZipLength,
	nHeelOff,
	nHeelRad;

BOOL
	EOS,
	EOSFound,
	DrawCurve,
	Select,
	Loop,
	SmallHeel;

MSGID
	Ok;

ANGLE
	aStart,
	aDelta,
	aPrevAngle,
	aAngle;
	
// Find JOBST installed directory
// Set path to macros
//
   STRING	 sPathJOBST;
   sPathJOBST = GetProfileString("JOBST", "PathJOBST", "\\JOBST", "DRAFIX.INI") ;
   SetData("PathMacro",  sPathJOBST + "\\WAIST" );

 // Reset 
    UserSelection ("clear");
    UserSelection("update") ;
    Execute ( "menu", "SetStyle", Table( "find",  "style", "bylayer"));
    Execute ( "menu", "SetColor", Table( "find",  "color", "bylayer")); 

// Sub Routines etc
//
   @WHZIPDEF.D;

// Get profile, identify Leg and FileNo
//
   if(!hEnt = GetEntity ("Select a Leg Profile", "curve")) Exit (%ok, "User cancelled") ;

   Display ("cursor", "wait", "Searching for more data");
   GetDBData (hEnt, "ID", &sProfileID);
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


// Get Marker data
   sHeelID          = sStyleID + sLeg+ "Heel";
   sOtemplateID = sStyleID + sLeg + "Origin";
   sAnkleID        =  sStyleID + sLeg + "Ankle";
   sCO_WaistBottID = sStyleID + sLeg + "CO_WaistBott";
   sTmp = "type = 'Marker' AND (DB ID ='" + sHeelID + "' OR DB ID ='"  + sAnkleID  + "' OR DB ID ='"  + sCO_WaistBottID + "' OR DB ID ='" + sOtemplateID + "')";
   nMarkersFound = 0 ;
   hChan = Open ("selection", sTmp );
   if (hChan) {
	ResetSelection(hChan);
	while (hEnt = GetNextSelection(hChan)) {
		GetDBData(hEnt,"ID", &sTmp) ;
		GetGeometry(hEnt, &sTmp1, &xyPt1, &xyTmp, &aAngle) ;
	                if ( xyPt1.x != 0 || xyPt1.y != 0) {
			if (StringCompare(sTmp, sHeelID )){
				nMarkersFound = nMarkersFound + 1 ;
				GetGeometry(hEnt, &sTmp, &xyHeel, &xyTmp, &aAngle) ;
				GetDBData(hEnt, "Data", &sTmp);
				SmallHeel = Value("scalar", sTmp);
				}
			if (StringCompare(sTmp, sOtemplateID)) {
				nMarkersFound = nMarkersFound + 1 ;
				GetGeometry(hEnt, &sTmp, &xyOtemplate , &xyTmp, &aAngle) ;
				GetDBData(hEnt, "units", &sUnits);
   				GetDBData(hEnt, "Pressure", &sTemplate); 	
   				GetDBData(hEnt, "Data", &sType); 	
				}
			if (StringCompare(sTmp, sAnkleID)) {
				nMarkersFound = nMarkersFound + 1 ;
				GetGeometry(hEnt, &sTmp, &xyAnkle , &xyTmp, &aAngle) ;
				GetDBData(hEnt, "Data", &sTmp);
				if ( !ScanLine(sTmp, "blank", &nX, &nY)) Exit(%cancel,"Can't get data from Ankle Marker!\n");
				}
			if (StringCompare(sTmp, sCO_WaistBottID )){
				GetGeometry(hEnt, &sTmp, &xyCO_WaistBott, &xyTmp, &aAngle) ;
				}
			}
		}
   	}
   Close("selection", hChan); 

// Check if that the markers have been found, otherwise exit
//
    if (StringCompare("CHAP", sType))Exit (%cancel, "Can't use this zipper for a CHAP style!\n") ;
    if (nMarkersFound < 3) Exit (%cancel, "Missing markers for selected foot, data not found!\n") ;
    if (nMarkersFound > 3) Exit (%cancel, "Two or more drawings of the same style exist!\nDelete the extra drawing/s and try again.\n") ;

// Get Smallest circumference (by requesting) a point
// Do some checking that the given point is sensible
   Loop = %true ;	
   while (Loop) {
   	GetUser("xy", "Select smallest leg circumference", &xySmallestCir);
	if ( xySmallestCir.y > xyAnkle.y || xySmallestCir.y < xyOtemplate.y){
		Display ("beep", "Selected point above Ankle or below edge of template. Try again");
		Loop = %true;
		 }
	else 
		Loop = %false;
	}

// Create Dialog
// Get Zipper style
//
   sUnits = "Inches";
   EOS = %false;
   Loop = %true ;
   Select = %false ;
   sDlgElasticList =  "3/8\" Elastic\n3/4\" Elastic\n1�\" Elastic\nNo Elastic";
   sDlgElasticList =  "3/4\" Elastic\n" + sDlgElasticList;
   sDlgLengthList =  "EOS\nGive a Length\nSelected Point\n";
   nMedial = 0 ;
   while (Loop) {		
        nButX = 60; nButY = 45;
        hDlg = Open ("dialog", sLeg + " Ankle Zipper (Waist Height)", "font Helv 8", 20,20, 215, 70);

	AddControl ( hDlg, "pushbutton",  nButX, nButY, 35, 14, "Cancel", "%cancel", "");
	AddControl ( hDlg, "pushbutton",  nButX + 48 , nButY, 35,14, "OK", "%ok", "");

	AddControl ( hDlg, "ltext", 5, 12, 30, 14 , "Ankle to", "string", "");
	AddControl ( hDlg, "combobox", 40, 10, 65, 45 , sDlgLengthList , "string", "sZipLength");
	AddControl ( hDlg, "ltext", 113, 12, 33, 14 , "Proximal:", "string", "");
 	AddControl ( hDlg, "dropdownlist", 145, 10, 60, 60 , sDlgElasticList, "string", "sElastic");

  	AddControl ( hDlg, "ltext", 5 , nButY - 15, 60, 15 ,"Template: " + sTemplate, "String", "nMedial");
  	AddControl ( hDlg, "checkbox", 120, nButY - 18, 60, 15 , "Medial Zipper", "number", "nMedial");

   	Ok = Display("dialog", hDlg, "%center");
  	Close("dialog", hDlg);

	if (Ok == %cancel ) Exit (%ok, "User Cancel!") ;	
	
	if (StringCompare("EOS", sZipLength) || StringCompare("Selected Point", sZipLength)){ 
		Loop = %false ;
		if (StringCompare("EOS", sZipLength)) EOS = %true ;
			else Select = %true ;
		}
	else {
		nZipLength = Value("scalar", sZipLength);
		if ( nZipLength == 0 && StringLength (sZipLength) > 0) 
			Loop = %true ;
   		else {
			Loop = %false ;
			EOS = %false;
			Select = %false;
		        }
		}
    }   

   Display ("cursor", "wait", "Drawing Zipper");

// Draw Chosen Zipper style
// 
   if (SmallHeel) {
	nHeelRad = 1.1760709 ; 
	nHeelOff = 0.25 ;
	}
   else {
	nHeelRad = 1.5053069 ;
	nHeelOff = 0.5;
	}
   nZipOff = 1.125 ;
	
   xyConstruct.x =  xyAnkle.x + nX ; // nX and nY stored relative position of the 
   xyConstruct.y =  xyAnkle.y + nY;  // heel construction circle center point 

   Ok = FN_CirLinInt ( xyHeel.x + nHeelOff , xyConstruct.y, xyHeel.x + nHeelOff, 0, xyConstruct, nHeelRad + nZipOff);
   if (Ok) {
   	// Draw polyline curve from intersection
      	xyCurveStart = xyInt;
   	xyHorizStart.x =  xySmallestCir.x ; 
  	xyHorizStart.y = xySmallestCir.y - nZipOff  ;	
  	DrawCurve = %true;
  	}
   else	{
   	// Otherwise degenerate to a straight line
   	xyHorizStart.x = xyHeel.x + nHeelOff;
   	xyHorizStart.y = xyHeel.y - nZipOff  ;	
   	DrawCurve = %false;
   	}
	
   // Check that horizontal zipper line can intersect with the  EOS
    if (EOS) {
          EOSFound = %false ;
          sFileNo = StringMiddle(sStyleID, 5, StringLength(sStyleID)-4) ;
          if (nMedial == 1 )
          	sTmp = "type = 'Line' AND layer = 'Construct' AND DB ID = '" + sFileNo + sLeg + "FoldLine'" ;
          else
          	sTmp = "type = 'Line' AND layer = 'Construct' AND DB ID = '" + sFileNo + sLeg + "EOSLine'" ;
          hChan = Open ("selection", sTmp );
          if (hChan) {
	ResetSelection(hChan);
	while (hEOS = GetNextSelection(hChan)) {
	      GetEntityClass(hEOS, &sClass);
	      GetExtent(hEOS, &xyMin , &xyMax) ;
	      if (xyHorizStart.y >= xyMin.y && xyHorizStart.y <= xyMax.y) {
		if (xyMin.x == xyMax.x)  
			xyHorizEnd.x = xyMin.x ;  
		else { // non vertical EOS
			GetGeometry(hEOS, &xyStart, &xyEnd);
			nTanTheta = (xyEnd.y - xyStart.y) / ( xyEnd.x - xyStart.x);
			xyHorizEnd.x = xyMin.x  +  (xyHorizStart.y - xyMin.y) / nTanTheta ;
			}

		EOSFound = %true ;
		xyHorizEnd.y = xyHorizStart.y ;
		}
	     }	
	}
	Close("selection", hChan); 
   	if (!EOSFound) {
   		Display("message","error", "Can't find an EOS for zipper line to intersect!\nSelect a point manually!");	
   		Select = %true ;
   		}
   	if (nMedial == 1) // closed zipper
   		nElastic = 0 ;
   	else {
   		// Elastic allowance at EOS
		nElastic = 0.75 ;
		if (StringCompare(StringMiddle(sElastic,1,1), "N")) nElastic = 0 ;
		if (StringCompare(StringMiddle(sElastic,1,3), "3/8")) nElastic = 0.375 ;
		if (StringCompare(StringMiddle(sElastic,1,1), "1")) nElastic = 1.5 ;
		}
          } 
   else	{
   	// No elastic as this implies a closed zipper
   	nElastic = 0 ;
   	xyHorizEnd.x = xyHorizStart.x + nZipLength  ; 	// Do this to give an approximate position
   						// As we need xyHorizEnd to insert the text symbol 
   	}
   	
// For Medial Zipper step back 3 tape
// Get template from leg box
   if (nMedial == 1) {
   	nTemplate = Value ("scalar", StringMiddle(sTemplate,1,2));
   	if (nTemplate >= 30) nFoldOffset = 1.25 * 3 ;
   	if (nTemplate == 13) nFoldOffset = 1.31 * 3 ;  
   	if (nTemplate == 9) nFoldOffset = 1.37 * 3 ;  
   	xyHorizEnd.x = xyHorizEnd.x - nFoldOffset ;
   	}
                       
// If select given then prompt user for end of zip
//
   if (Select) {
   	if (!GetUser("xy", "Select End of Zipper", &xyHorizEnd)) Exit(%ok, "End not selected") ;
   	xyHorizEnd.y = xyHorizStart.y ;
         	}
     
// Revise radius to include Zip offset
//
   nHeelRad = nHeelRad + nZipOff ;

   hLayer = Table("find", "layer", "Notes") ;
   if ( hLayer > %zero )
 	Execute ("menu", "SetLayer",  hLayer) ;
   else
   	Table ("add", "layer", sLayer, "Notes in Green", "current", Table("find", "color", "Green"));
  
   SetSymbolLibrary( sPathJOBST + "\\JOBST.SLB") ;   	     
   Symbol( "find", "TextAsSymbol") ;
   hTextEnt = AddEntity ("symbol", "TextAsSymbol", xyHorizStart.x + (xyHorizEnd.x- xyHorizStart.x)/2, xyHorizStart.y + .125, 1, 1, 0) ;  

// Create ID string
   sZipperID =  sStyleID + MakeString("scalar", UID("get", hTextEnt )) ;
   SetDBData(hTextEnt, "ID", sZipperID );
   SetDBData(hTextEnt, "Zipper", "1" );
   SetDBData(hTextEnt, "Data", "INVALID ZIPPER CALCULATION" ); //Do this just incase it fails

   if (DrawCurve) {
       StartPoly("fitted");
	AddVertex ( xyCurveStart);
   	aAngle = Calc("angle" ,xyConstruct, xyHorizStart ) ;
   	aPrevAngle =  Calc("angle" ,xyConstruct, xyCurveStart ) ;

	nDrawnLength = (aAngle - aPrevAngle)  * %pi * nHeelRad / 180 ;
   	if (aAngle > aPrevAngle ) aAngleInc = (aAngle - aPrevAngle) / 4 ;
		else aAngleInc = ((aAngle + 360) - aPrevAngle) / 4 ;	
   	ii = 1 ;
  	while ( ii  <= 3) {	//*
		xyTmp = CalcXY ("relpolar", xyConstruct , nHeelRad, aPrevAngle + aAngleInc * ii); //*
		if (xyTmp.x < xyAnkle.x) {
			AddVertex ( xyTmp); //*
			xyEnd = xyTmp;
			}
		ii = ii + 1 ;
		}
	AddVertex ( xyHorizStart ) ;
	AddVertex ( xyHorizStart.x + 0.25, xyHorizStart.y) ;
	AddVertex ( xyHorizStart.x + 0.5, xyHorizStart.y) ;
	
	// Requested zipper length
	if (nZipLength > 0 ) {
		xyHorizEnd.x = xyHorizStart.x + ((nZipLength / 1.2 ) - nDrawnLength) ;
		xyHorizEnd.y = xyHorizStart.y ;
		}
	else
		{
   		nDrawnLength = nDrawnLength + Calc("length",xyHorizStart, xyHorizEnd) ;
		nZipLength = (nDrawnLength + nElastic ) * 1.2 ;
		}
	
	AddVertex ( xyHorizEnd ) ;
              EndPoly();	
              
              hEnt = UID("find", UID("getmax")) ;	
              PR_SetZipperID () ;
              SetDBData (hEnt, "ZipperLength", "%Length");

              hEnt = AddEntity("marker","closed arrow", xyCurveStart , 0.5 ,0.125, aPrevAngle + 92 ) ;	
              PR_SetZipperID () ;
              }
   else	{
	if (nZipLength > 0 ) {
		xyHorizEnd.x = xyHorizStart.x + ((nZipLength / 1.2 ))  ;
		xyHorizEnd.y = xyHorizStart.y ;
		}
	else	{
   		nDrawnLength = Calc("length",xyHorizStart, xyHorizEnd) ;
		nZipLength = (nDrawnLength + nElastic ) * 1.2 ;
		}
   	hEnt = AddEntity	("line", xyHorizStart, xyHorizEnd) ;
                PR_SetZipperID () ;
  	hEnt = AddEntity("marker","closed arrow", xyHorizStart , 0.5 ,0.125, 0 ) ;	
                PR_SetZipperID () ;
   	}
   	 	
// Add label and arrows
   if (     xyHorizEnd.y > xyCO_WaistBott.y 
       && xyCO_WaistBott.y != 0 
       && xyCO_WaistBott.x != 0 
       && nMedial == 0
       && EOS)
   	hEnt = AddEntity("marker","closed arrow",xyHorizEnd.x, xyCO_WaistBott.y - 0.03125, 0.5 ,0.125, 225 ) ;	
   else 
   	hEnt = AddEntity("marker","closed arrow",xyHorizEnd , 0.5 ,0.125, 180 ) ;	
   PR_SetZipperID () ;
   	
   if (nMedial == 1 )
   	sTmp = Format("length",nZipLength) +  " MEDIAL ZIPPER" ;
   else
      	sTmp =  Format("length",nZipLength) +  " LATERAL ZIPPER" ;
   SetDBData(hTextEnt, "Data", sTmp );

// Reset and exit
   Execute ("menu", "SetLayer",  Table("find", "layer", "1")) ;
   Exit (%ok, "Zipper drawing Complete");
