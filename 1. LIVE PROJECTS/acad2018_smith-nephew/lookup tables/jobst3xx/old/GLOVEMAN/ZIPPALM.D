// File Name:	ZIPPALM.D
// Purpose:	Draws a Palmer Zipper for
//		CAD Glove
// Version:	1.01
// Date:		19.Sep.95
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
	hCurve,
	hLayer;

XY
	xyZipStart,
	xyZipEnd,
	xyPALMER,
	xyPALMERWEB,
	xyStartEOS, xyEndEOS,
	xyGivenPoint,
	xyPt1,
	xyPt2,
	xyTmp;

STRING
	sClass,
	sData,
	sText,
	sAge,
	sUnits,
	sElasticProximal,
	sUnits,
	sZipLength,
	sZipOffset,
	sProfileID,
	sTmp, sTmp1,
	sSide,
	sDlgElasticList ,
	sDlgLengthList,
	sDlgWebOffSetList ,
	sWebOffSet,
	sZipperID; // Unique ID for each Zipper, allows easy selection and deletion

NUMBER
	Ok,
	nFound,
	nDirection,
	nReqired,
	nType,
	nElasticFactor,
	nAge,
	nElasticProximal,
	nElastic,	//sum of Proximal and Distal
	ii, nn,
	nWebOffSet,
	nZipLength ;
BOOL
	EOStoSelectedPoint,
	EOStoCalculatedPoint,
	Loop ;

ANGLE
	aPALMER,
	aPALMERWEB,
	aEOS,
	aZipper,
	aAngle;

// Clear user selection
//
   UserSelection ("clear") ;
   UserSelection ("update") ;
   
   STRING		sPathJOBST;
   sPathJOBST = GetProfileString("JOBST", "PathJOBST", "\\JOBST", "DRAFIX.INI") ;
   SetData("PathMacro",  sPathJOBST + "\\GLOVES" );
   
// Get profile, identify side 
//
   if ( !hCurve=GetEntity ("Select Glove Profile", "all") ) Exit (%ok, "User Cancelled") ;

Display ("message", "error", "Profile selected OK") ;

   Display ("cursor", "wait", "Searching for more data");
   GetDBData (hCurve, "ID", &sProfileID);
   
Display ("message", "error", "sProfileID=" + sProfileID) ;

   nStringLength = StringLength(sProfileID);
   if (StringCompare( "Left", StringMiddle(sProfileID, nStringLength - 3, 4))){
	sSide = "Left";
	nDirection = 1;
	}
    if (StringCompare( "Right", StringMiddle(sProfileID, nStringLength - 4, 5))) {
	sSide = "Right";
	nDirection = -1;
	}
	
Display ("message", "error", "sSide =" + sSide ) ;

// Check if this is a valid Glove exit if not
// This is crude check only
    if (StringLength (sSide) == 0) Exit (%cancel, "A Glove Profile was not selected\n") ;

// Get data for a PALMER Zipper 
   sTmp = "DB ID = '" + sProfileID + "' AND  (DB Zipper ='PALMER' OR DB Zipper ='PALMER-WEB' OR DB Zipper = 'EOS')";

Display ("message", "error", "sTmp=" + sTmp ) ;

   nFound = 0 ; nReqired = 3;
   hChan = Open ("selection", sTmp );
   
   if (hChan) {
Display ("message", "error", "sChan Open" ) ;
	ResetSelection(hChan);
	while (hEnt = GetNextSelection(hChan)) {
		GetDBData(hEnt,"Zipper", &sTmp) ;
		if (StringCompare(sTmp, "PALMER")) {
			nFound = nFound + 1 ;
			GetGeometry(hEnt, &sTmp, &xyPALMER , &xyTmp, &aAngle) ;
			GetDBData(hEnt, "Age", &sAge);
			GetDBData(hEnt, "Data", &sData);
			nType = Value( "scalar", StringMiddle(sData, 10, 2));
			nAge = Value("scalar", sAge) ;
			}
		if (StringCompare(sTmp, "PALMER-WEB" )){
			nFound = nFound + 1 ;
			GetGeometry(hEnt, &sTmp, &xyPALMERWEB, &xyTmp, &aAngle) ;
			// Ignore insert size for Palmer Zippers
			nInsertSize = 0;
			}							
		if (StringCompare(sTmp, "EOS" )){
			nFound = nFound + 1 ;
			hEOS = hEnt ;
			GetGeometry(hEnt, &xyStartEOS, &xyEndEOS) ;
			}
		}
   	}
   Close("selection", hChan); 

// Check that sufficent data have been found, otherwise exit
//
    if (nFound < nReqired) Exit (%cancel, "Missing data for selected Glove!\n") ;
    if (nFound > nReqired) Exit (%cancel, "Two or more drawings of the same style exist!\nDelete the extra drawing/s and try again.\n") ;

// Create Dialog
// Get Zipper style
//
   sUnits = "Inches";

   EOStoSelectedPoint = %false ;
   EOStoCalculatedPoint = %false ;
   
// Proximal elastic
   sDlgElasticList =  "3/8\" Elastic\n3/4\" Elastic\n1�\" Elastic\nNo Elastic";
   if (nAge < 10)
   	sDlgElasticList =  "3/8\" Elastic\n" + sDlgElasticList;  // 1/2" for children under 10
   else
      	sDlgElasticList =  "3/4\" Elastic\n" + sDlgElasticList;  // Inch for adults 

// Length specification      	
   sDlgLengthList =  "Standard\nGive a length\nSelected Point";
   
// Offset from web
   sDlgWebOffSetList =  "1-1/8\"\n3/4\"";
   sDlgWebOffSetList =  "1-1/8\"\n"  + sDlgWebOffSetList ;	// Set Default


   Loop = %true ;
   while (Loop) {		
            nButX = 65; nButY = 55;
            hDlg = Open ("dialog", sSide + " Glove Zipper (PALMER)", "font Helv 8", 20,20, 210, 75);

	AddControl ( hDlg, "pushbutton",  nButX, nButY, 35, 14, "Cancel", "%cancel", "");
	AddControl ( hDlg, "pushbutton",  nButX + 48 , nButY, 35,14, "OK", "%ok", "");

	AddControl ( hDlg, "ltext", 5, 12, 28, 14 , "EOS to", "string", "");
	AddControl ( hDlg, "combobox", 35, 10, 70, 40 , sDlgLengthList , "string", "sZipLength");

	AddControl ( hDlg, "ltext", 110, 12, 30, 14 , "Proximal:", "string", "");
 	AddControl ( hDlg, "dropdownlist", 145, 10, 60, 70 , sDlgElasticList, "string", "sElasticProximal");

	AddControl ( hDlg, "ltext", 100, 32, 50, 14 , "Below Web:", "string", "");
 	AddControl ( hDlg, "dropdownlist", 145, 30, 60, 40 , sDlgWebOffSetList, "string", "sWebOffSet");

      	Ok = Display("dialog", hDlg, "%center");
  	Close("dialog", hDlg);

	if (Ok == %cancel ) Exit (%ok, "User Cancel!") ;	
	
	if (StringCompare("Selected Point", sZipLength)) EOStoSelectedPoint = %true;
	
	if (StringCompare("Standard", sZipLength)) EOStoCalculatedPoint = %true;
	    
	if (EOStoSelectedPoint || EOStoCalculatedPoint   )
		Loop = %false ;
	else {
		nZipLength = Value("scalar", sZipLength);
		if ( nZipLength == 0  && StringLength (sZipLength) > 0) {
			Display ("message", "error", "Invalid given length!\n\nTo use this option, type over the text in the \"EOS to:\" box with the required length in Inches (Decimal Inches).") ;
			Loop = %true ;
			}
   		else
			Loop = %false ;
		}
     }   
    
    Display ("cursor", "wait", "Drawing Zipper");
	
    	
 // Establish allowance for zippers
 //
    nElasticProximal = 0.75 ;
    if (StringCompare(StringMiddle(sElasticProximal,1,1), "N")) nElasticProximal = 0 ;
    if (StringCompare(StringMiddle(sElasticProximal,1,3), "3/8")) nElasticProximal = 0.375 ;
    if (StringCompare(StringMiddle(sElasticProximal,1,1), "1")) nElasticProximal = 1.5 ;
    nElastic = nElasticProximal ;

// Elastic factor
// Set by age
   if (nAge <= 6 )
   	nElasticFactor = 1;
   else	{
   	nElasticFactor = 0.95; 		// Glove to elbow & Normal Glove
   	if (nType == 2) nElasticFactor = 0.92; 	// Glove to axilla
   	}

// Establish Minimum offset from web    
   nWebOffSet = 1.125 ;
   if (StringCompare(StringMiddle(sWebOffSet,1,1), "3")) nWebOffSet = 0.75 ;

// Draw on layer Notes
//
   Execute ("menu", "SetLayer",  Table("find", "layer", "Notes"));
 
// 
// Draw Zipper
// 
// Establish EOS start and end
//
   nLengthToStart = Calc ("length", xyStartEOS, xyPALMER);	
   nLengthToEnd = Calc ("length", xyEndEOS, xyPALMER);
   if (nLengthToStart  > nLengthToEnd){
   	// Swap start and end
   	xyTmp = xyStartEOS ;
   	xyStartEOS =  xyEndEOS ;
   	xyEndEOS = xyTmp ; 
   	}
   
// Angles
//
   aEOS = Calc ("angle", xyStartEOS, xyEndEOS );
   aPALMER = Calc ("angle", xyStartEOS, xyPALMER );
   
// Zipper start point on EOS
// 
   aAngle = abs(aEOS - aPALMER);
   nLength = Calc ("length", xyStartEOS, xyPALMER );
   nB = cos(aAngle) * nLength;
   xyZipStart = CalcXY ("relpolar", xyStartEOS, (nB + 0.625) - nInsertSize, aEOS) ;
   
   
   aPALMERWEB = Calc ("angle", xyStartEOS, xyPALMERWEB );
   aAngle = abs( aEOS - aPALMERWEB) ;
   nLength = Calc ("length", xyStartEOS, xyPALMERWEB );
   nZipConstructLen = abs(sin(aAngle) * nLength);
   
   aZipper = aEOS + (90 * nDirection);
       	
   if (EOStoCalculatedPoint) {
   	xyZipEnd = CalcXY ("relpolar", xyZipStart, nZipConstructLen  - nWebOffSet, aZipper) ;  
   	}
   	 	
   if (EOStoSelectedPoint){
   	// Only allow the point if it lies within the rectangle formed by the
   	// EOS and the xyPALMERWEB plus a .0625" tolerance
   	// EOS = length and angle, xyPALMERWEB = width
   	xyPt1 = CalcXY ("relpolar", xyStartEOS, nZipConstructLen + .0625, aZipper) ; 
   	xyPt1 = CalcXY ("relpolar", xyPt1,  2, (aEOS + (nDirection*180))) ;   	
   	xyPt2 = CalcXY ("relpolar", xyEndEOS, nZipConstructLen + .0625, aZipper) ; 
   	xyPt2 = CalcXY ("relpolar", xyPt2,  2, aEOS ) ;   	

      	Loop = %true;
   	while(Loop ){
   		if (!GetUser("xy", "Select End of Zipper", &xyGivenPoint)) Exit(%ok, "Start not selected") ;
		if ( !Calc ("inpoly", xyGivenPoint, xyStartEOS, xyPt1, xyPt2, xyEndEOS) ) {
			Ok = Display("message", "OKquestion", "Given point can't be used! Try again.\nOr use cancel to Exit");
			if ( Ok == %cancel) Exit (%ok, "User Cancelled");
			}
		else
			Loop = %false;
		}	
		
   	aPALMERWEB = Calc ("angle", xyStartEOS, xyGivenPoint);
   	aAngle = abs( aEOS - aPALMERWEB) ;
   	nLength = Calc ("length", xyStartEOS, xyGivenPoint );
   	nA = abs(sin(aAngle) * nLength);
   	xyZipEnd = CalcXY ("relpolar", xyZipStart, nA, aZipper) ;   	
   	
   	// Ensure that selected point is no closer than nWebOffSet to the web
   	// within 1/8th of an inch
   	nLength = Calc("length",xyZipStart, xyZipEnd) - nZipConstructLen   ;
	if ( (abs(nLength) < (nWebOffSet - 0.125)) || (nLength > 0)) {
		if (nLength > 0 ) 
			sText =  "The end of the zipper will above the nearest web"  ;
		else
			sText = "The end of the zipper will be closer to the nearest web than " 
				+ Format ("length",nWebOffSet) + ".  Actual distance is " + Format ("length",abs(nLength)) ;
		Ok = Display ("message", "YNCquestion", sText  + "\nUse YES to use this point or\nUse NO to default to Standard." ) ;
		if (Ok == %cancel) Exit (%ok, "User Cancelled");
		if (Ok == %no){ 
   			xyZipEnd = CalcXY ("relpolar", xyZipStart, nZipConstructLen  - nWebOffSet, aZipper) ;  
			}	
		}
	}
	
// Specified ZIP Length
//	
   if (nZipLength > 0 ) {
   	xyZipEnd = CalcXY ("relpolar", xyZipStart, (nZipLength * nElasticFactor) - nElastic, aZipper) ;   	
   	nDrawnLength = Calc("length",xyZipStart, xyZipEnd) ;
   	nLength = nDrawnLength - nZipConstructLen   ;
	if ( (abs(nLength) < (nWebOffSet - 0.125)) || (nLength > 0)) {
		Ok = Display ("message", "OKquestion", 
			 "The end of the zipper will be above or closer to the nearest web than " 
			+ Format ("length",nWebOffSet) + " with the given length."
			+  "\nUse OK to default to Standard or\nUse CANCEL to exit." ) ;
		if (Ok == %cancel) Exit (%ok, "User Cancelled");
		xyZipEnd = CalcXY ("relpolar", xyZipStart, nZipConstructLen  - nWebOffSet, aZipper) ;  
   		nDrawnLength = Calc("length",xyZipStart, xyZipEnd) ;
		nZipLength = (nDrawnLength + nElastic ) / nElasticFactor ;
		}
	}  
   else {
   	nDrawnLength = Calc("length",xyZipStart, xyZipEnd) ;
	nZipLength = (nDrawnLength + nElastic ) / nElasticFactor ;
	}

   sZipLength = Format("length", nZipLength);

// Draw markers and Text label
// Each entity is given an ID Data Base value that links them together
// This is based on the UID of the symbol "ZipperText" and the Glove Profile ID.
// A Symbol is used as DB values can't be attached to text entities 

// Add text symbol
   xyTmp = CalcXY("relpolar", xyZipStart, nDrawnLength / 2, aZipper) ;   	     
   
   SetSymbolLibrary( sPathJOBST + "\\JOBST.SLB") ;   	     
   Symbol( "find", "TextAsSymbol") ;
   hEnt = AddEntity ("symbol", "TextAsSymbol", xyTmp, 1, 1, aZipper - 180) ;  
   SetDBData(hEnt, "Data", sZipLength + " Palmer" );

// Create ID string
   sZipperID =  sProfileID + MakeString("scalar", UID("get", hEnt)) ;
   
// Label entity with ID string and make Zipper %true
   SetDBData(hEnt, "ID", sZipperID );
   SetDBData(hEnt, "Zipper", "1" );
    	
// Add label and arrows
   hEnt = AddEntity("marker","closed arrow", xyZipStart , 0.5 ,0.125, aEOS + (90 * nDirection )) ;	
   SetDBData(hEnt, "ID", sZipperID );
   SetDBData(hEnt, "Zipper", "1" );
   
   hEnt = AddEntity("marker","closed arrow", xyZipEnd , 0.5 ,0.125, aEOS - (90 * nDirection)) ;		
   SetDBData(hEnt, "ID", sZipperID );
   SetDBData(hEnt, "Zipper", "1" );
   
   hEnt = AddEntity ("line",  xyZipStart, xyZipEnd );
   SetDBData(hEnt, "ID", sZipperID );
   SetDBData(hEnt, "Zipper", "1" );

// Reset and exit
   Execute ("menu", "SetLayer",  Table("find", "layer", "1")) ;
   Exit (%ok, "Zipper drawing Complete");
