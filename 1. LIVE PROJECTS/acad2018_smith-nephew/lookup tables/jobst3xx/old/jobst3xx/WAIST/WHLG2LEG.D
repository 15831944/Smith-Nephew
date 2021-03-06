// File Name:	WHLG2LEG.D
// Purpose:	Get 1st Leg profile data
//
// Version:	2.01
// Date:		05.Dec.94
// Author:		Gary George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date		By	Action
//
// Get the  1st Leg profile
// Using this profile the modfications to the 1st leg body (above fold) can be
// copied to the 2nd leg

   SCALAR  nPtX, nPtY ;
   hChan = Open ("selection", " type = 'Curve' AND DB ID = '????" + sFileNo + sFirstLeg + "LegCurve" + "'" );
   hFirstLegCurve = %false;
   if (hChan) {
	ResetSelection(hChan);
   	hFirstLegCurve = GetNextSelection(hChan) ;
	Close("selection", hChan); 
	// Write data to file for subsequent use
	// N.B. Write to Local JOBST Directory C:\JOBST 
	hFileCurve = Open ("file", "C:\\JOBST\\LEGCURVE.DAT", "write") ;
	SetData("UnitLinearType", 0);	// "Inches"
	ii = 1;
	nLegVertexCount = GetVertexCount(hFirstLegCurve) ;
   	while ( ii <= nLegVertexCount) {
	     	GetVertex (hFirstLegCurve, ii, &xyPt1) ;
	     	nPtX = xyPt1.x ;   nPtY = xyPt1.y ;
		PrintFile(hFileCurve,  nPtX, nPtY, "\n") ;
		ii = ii + 1;
		}
	SetData("UnitLinearType", 6);	// "Inches/Fraction"
	Close("file", hFileCurve) ;
	}
   if ( !hFirstLegCurve ) {
   	Close("selection", hChan); 
	Exit (%cancel, "Can't find First Leg Curve, to copy body curve\nDraw or Re-Draw First Leg.\n") ;
	}
   Close("selection", hChan);  