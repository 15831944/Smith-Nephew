// File:	BODMESH.D
//	MESH AXILLA
//	~~~~~~~~~~~~~~~~~
// From the Axilla point traverse the VESTCURV picking up the following intersections
// in the order given (ie Increasing X)
//
//	1. Start of ragalan curve (1/8th) back from next point
//	2. Intersection of end of Mesh symbol and raglan curve, defines symbol angle

if (StringCompare("Mesh", sAxillaType)){ 
   if (nAge <= 14) nMeshAxillaFac = nMeshAxillaGussetFac ;
	else nMeshAxillaFac = nMeshAxillaGussetBoysFac ;
	
   if (BRAGiven && FN_CirLinInt(xyBreast, xyAxilla, xyAxilla, nMeshAxillaFac) )
	xyAxillaLow = xyInt;
   else if (FN_CirLinInt(xyWaistOFF, xyAxilla, xyAxilla, nMeshAxillaFac))
	xyAxillaLow = xyInt;
	         else  Display ("message", "error", "Can't form Axilla with this data\n");

   if (nAge <= 14) {
//	sMeshSymbol = "mesh0to14";
	nMeshSymbolLength = nMSLengthGusset;
	nMeshLength = 	2.58 ;
	}
   else {
//	sMeshSymbol = "mesh15andUP";
	nMeshSymbolLength = nMSLengthGussetBoys;	
	nMeshLength = 	2.8 ;
	}


// Open Curve for Reading
// Assume that if the tests for Existance/Corruption are passed. Then no need to repeat them

   hCurve = Open ("file", sPathJOBST + "\\TEMPLTS\\VESTCURV.DAT", "readonly");
   GetLine(hCurve, &sLine);
   ScanLine( sLine, "blank", &nLength, &aAngle);

   aPrevAngle = (aVestCurve - aCurve);      // Rotate curve to correct start angle
   xyCen = xyAxillaLow;		
   nRadius = nMeshSymbolLength  - nSeamAllowance - .25 ;
   nDistanceAlongRaglan = 0;
   nCount = 1;
   xyPt1 = xyAxilla;
   DrawSegment = %false ;		      // Set draw segments flag off
   StartPoly("polyline");

   while (GetLine(hCurve, &sLine)){
	aAngle = aAngle + aPrevAngle;
	xyPt2 = CalcXY("relpolar", xyPt1,nLength, aAngle);
//	if (!DrawSegment) nDistanceAlongRaglan = nDistanceAlongRaglan + nLength ;
	
	if ( FN_CirLinInt( xyPt1, xyPt2, xyCen, nRadius)) {
		if (nCount == 1) {
			DrawSegment = %true ;			// Draw from seam allowance
			xyRaglanAxilla = xyInt;		
			nRadius = nMeshSymbolLength;	
			// Check if seam allowance on same segment
			if ( FN_CirLinInt( xyPt1, xyPt2, xyCen, nRadius)) nCount = 2 ;
//			nDistanceAlongRaglan = nDistanceAlongRaglan + Calc ("length", xyPt1, xyPt2);
			xyPt1 = xyRaglanAxilla ;
			}
		if (nCount == 2) {
			xyMeshAtSeamAllowance = xyInt;	// Mesh symbol end for angle
			xyCen = xyBackNeckCen;		
			nRadius = nBackNeckRadius;
			}
		if (nCount == 3) {
			xyRaglanNeck = xyInt;			// Found Raglan end
//			AddEntity("line", xyPt1, xyRaglanNeck);
			AddVertex( xyPt1);
			AddVertex( xyRaglanNeck);
			EndPoly();
          		hEnt = UID ("find", UID ("getmax")) ;
    			SetDBData(hEnt, "Data",sID);
    			SetDBData(hEnt, "curvetype","vest" + sSide + "raglan");
			break;
			}
		nCount = nCount +1 ;
		}

//                if ( DrawSegment ) AddEntity("line",xyPt1,xyPt2);  // Draw raglan
                if ( DrawSegment ) AddVertex(xyPt1);  // Draw raglan
	xyPt1 = xyPt2;
	aPrevAngle = aAngle;
	ScanLine( sLine, "blank", &nLength, &aAngle);
   } //End GetLine while

   Close ("file", hCurve);

// Insert Mesh axilla symbol at correct angle 

   aAngle = Calc("angle", xyAxillaLow, xyMeshAtSeamAllowance);

//   SetSymbolLibrary( sPathJOBST + "\\JOBST.SLB"); 
//   if ( !Symbol("find", sMeshSymbol+"seam")) Exit(%cancel, "Can't find MESH Axilla symbol to insert\nCheck your installation, that JOBST.SLB exists");
//   if ( !Symbol("find", sMeshSymbol+"pro")) Exit(%cancel, "Can't find MESH Axilla symbol to insert\nCheck your installation, that JOBST.SLB exists");
// Insert  Seam
   PRSetLayerNotes() ;
//   hMesh = AddEntity("symbol", sMeshSymbol + "seam", xyAxillaLow, 1, 1, aAngle);
   if (nAge <= 14) 
   	AddEntity("text", 
		"1-3/4\" GUSSET",
	 	xyAxillaLow.x - 0.25,
	 	xyAxillaLow.y - 0.5);
   else 
   	AddEntity("text", 
		"BOYS GUSSET",
	 	xyAxillaLow.x - 0.25,
	 	xyAxillaLow.y - 0.5);

// Insert profile
   if (StringCompare (sSide, "Right")){
  	AddEntity("text", 
		"RIGHT",
	 	xyAxillaLow.x - 0.25,
	 	xyAxillaLow.y - 0.75);
	PRSetLayerTemplateRight () ;
	}
       else
	PRSetLayerTemplateLeft() ;
	
//   hMesh = AddEntity("symbol", sMeshSymbol + "pro", xyAxillaLow, 1, 1, aAngle);

// Revised code to write a datafile MESHDRAW is started at the end of the macro BODY.D 
  if (bPrevAxillaWasMesh) {
	SetData("UnitLinearType", 0);	// "Inches"
   	hCurve = Open ("file", "C:\\JOBST\\MESHDRAW.DAT", "append") ;
   	}
  else {
   	hCurve = Open ("file", "C:\\JOBST\\MESHDRAW.DAT", "write") ;
	SetData("UnitLinearType", 0);	// "Inches"
	PrintFile(hCurve, "vestmesh", "\n") ;
	PrintFile(hCurve, UID("get",hBody), "\n") ;
	}
   
	PrintFile(hCurve, xyAxillaLow, "\n") ;
	PrintFile(hCurve, xyAxilla, "\n") ;
	PrintFile(hCurve, xyAxilla, "\n") ;
	PrintFile(hCurve, xyRaglanNeck, "\n") ;
	PrintFile(hCurve, nMeshLength, "\n") ;
	PrintFile(hCurve, nDistanceAlongRaglan, "\n") ;
	PrintFile(hCurve, sSide, "\n") ;
	PrintFile(hCurve, "vest", "\n") ;
	SetData("UnitLinearType", 6);	// "Inches/Fraction"

   Close ("file", hCurve) ;
   bPrevAxillaWasMesh = %true;
   
   } // End if for MESH axilla
 