// File Name:	WHLBLCRO.D
// Purpose:		Add Crotch labling details
//
// Version:		1.02
// Date:		14.Mar.94
// Author:		Gary George
//
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date	By	Action
// 02.Jun.98	GG	Buf fix w.r.t male 

// Lable Crotch
// Quick and Dirty
   sCrotchText = "";
   sGussetSize ="";
   nCrotchSize = 0 ;
   
   PRSetLayerNotes();
   if (nLegStyle == 2) {
   	SetData("TextVertJust", 16);
  	SetData("TextHorzJust", 2);
	SetData("TextAngle", 270);	
	}
   else{	
   	SetData("TextVertJust", 16);
  	SetData("TextHorzJust", 4);
	}
   nCO_ArcDiameter = FNRound (Calc ("length", xyCO_LargestTop, xyCO_LargestBott)) ;
   if ( ClosedCrotch ) {
	if ( StringCompare (sCrotchStyle, "Horizontal Fly")) {	
		sCrotchText = "Horizontal Fly, " ;
		if ( nAge >= 3 && nAge <= 6) {
			if ( nCO_ArcDiameter <= 1.5)  {
				sCrotchText =  sCrotchText + "C-1";
				nCrotchSize = 2;
				}
			else {
				sCrotchText =  sCrotchText + "C-2";
				nCrotchSize = 2.5;
				}
			}
		if ( nAge >= 7 && nAge <= 11) {
			if ( nCO_ArcDiameter <= 2)  {
				sCrotchText =  sCrotchText + "C-2";
				nCrotchSize = 2.5;
				}
			else {
				sCrotchText =  sCrotchText + "C-3";
				nCrotchSize = 2.5;
				}
			}
		if ( nAge >= 12 && nAge <= 14) {
			if ( nCO_ArcDiameter <= 2.25)  {
				sCrotchText =  sCrotchText + "C-3";
				nCrotchSize = 2.5;
				}
			if ( nCO_ArcDiameter > 2.25 && nCO_ArcDiameter <= 2.5)  {
				sCrotchText =  sCrotchText + "C-4";
				nCrotchSize = 2.5;
				}
			if ( nCO_ArcDiameter > 2.5)  {
				sCrotchText =  sCrotchText + "C-5";
				nCrotchSize = 3.125;
				}
			}
		if ( nAge >= 15) {
			if ( nCO_ArcDiameter <= 3.5)  {
				sCrotchText =  sCrotchText + "1";
				nCrotchSize = 3.5;
				}
			if ( nCO_ArcDiameter > 3.5 && nCO_ArcDiameter <= 4.25)  {
				sCrotchText =  sCrotchText + "2";
				nCrotchSize = 3.75;
				}
			if ( nCO_ArcDiameter > 4.25 && nCO_ArcDiameter <= 5)  {
				sCrotchText =  sCrotchText + "3";
				nCrotchSize = 4;
				}
			if ( nCO_ArcDiameter > 5)  {
				sCrotchText =  sCrotchText + "Oversize";
				nCrotchSize = 4;
				}
			}
		}

	if ( StringCompare (sCrotchStyle, "Diagonal Fly")) {
		sCrotchText = "Diagonal Fly, " ;
		if ( nAge < 10) {
			sCrotchText =  sCrotchText + "Small";
			nCrotchSize = 1.75;
			}
		if ( nAge >= 10 && nAge <= 14) {
			sCrotchText =  sCrotchText + "Medium";
			nCrotchSize = 2.75;
			}
		if ( nAge >= 15) {
			sCrotchText =  sCrotchText + "Large";
			nCrotchSize = 3.75;
			}
		}

	if ( StringCompare (sCrotchStyle, "Male Mesh Gusset")
	     ||
	     StringCompare (sCrotchStyle, "Mesh Gusset")
	     ||
	     StringCompare (sCrotchStyle, "Female Mesh Gusset")) {
		sCrotchText = "Mesh Gusset, ";
		if (StringCompare (sCrotchStyle, "Female Mesh Gusset")) {Female = %true; Male = %false;}
		if (StringCompare (sCrotchStyle, "Male Mesh Gusset")) {Female = %false; Male = %true;}	
		if (Male) {
			if ( nAge <= 3 ) {
				sCrotchText =  sCrotchText + Format ("length", 1.75);
				nCrotchSize = 2.125;
				}
			if ( nAge > 3 && nAge <= 14) {
				sCrotchText =  sCrotchText + "Boy's";
				nCrotchSize = 3.875;
				}
			if ( nAge >= 15) {
				sCrotchText =  sCrotchText + "Male";
				}
			}
		else {
			if ( nAge <= 3 ) 
				sCrotchText =  sCrotchText + Format ("length", 1.75);
			if ( nAge > 3 && nAge <= 14) {
				if ( nCO_ArcDiameter <= 1.75)  
					sCrotchText =  sCrotchText + Format ("length", 1);
				if ( nCO_ArcDiameter > 1.75 && nCO_ArcDiameter <= 3.125)  
					sCrotchText =  sCrotchText +  Format ("length", 1.25);
				if ( nCO_ArcDiameter > 3.125)  
					sCrotchText =  sCrotchText +  "Regular";
				}
			if ( nAge >= 15) {
				if ( nMaxCir < 45)
					sCrotchText =  sCrotchText + "Regular" ;
				else
					sCrotchText =  sCrotchText + "Oversize";
				}
			}
		}

	if ( StringCompare (sCrotchStyle, "Gusset")) {
		sCrotchText = sCrotchStyle + ", ";
		if (Male) {
			if ( nAge <= 3 ) {
				sCrotchText =  sCrotchText + Format ("length", 1.75);
				nCrotchSize = 2.125;
				}
			if ( nAge > 3 && nAge <= 14) {
				sCrotchText =  sCrotchText + "Boy's";
				nCrotchSize = 3.875;
				}
			if ( nAge >= 15) {
				sCrotchText =  sCrotchText + "Male";
				}
			}
		else {
			if ( nAge <= 3 ) 
				sCrotchText =  sCrotchText +  Format ("length", 1.75);
			if ( nAge > 3 && nAge <= 14) {
				if ( nCO_ArcDiameter <= 1.75)  
					sCrotchText =  sCrotchText +  Format ("length", 1);
				if ( nCO_ArcDiameter > 1.75 && nCO_ArcDiameter <= 3.125)  
					sCrotchText =  sCrotchText +  Format ("length", 1.25);
				if ( nCO_ArcDiameter > 3.125)  
					sCrotchText =  sCrotchText +  "Regular";
				}
			if ( nAge >= 15) {
				if ( nMaxCir < 45)
					sCrotchText =  sCrotchText + "Regular" ;
				else
					sCrotchText =  sCrotchText + "Oversize";
				}
			}		
		}


   	hEnt = AddEntity("marker","closed arrow", xyCO_CenterArrow, 0.5 ,0.125, 180 ) ;
	AddEntity( "text", 	sCrotchText, 
			xyCO_CenterArrow.x - 0.75,
			xyCO_CenterArrow.y);
   	}
   else {
	// Open Crotch
	if (nOpenOff == 0.375  ||  (nAge <= 10 && PantyLeg)) {
		AddEntity( "text", 	"1/2\" Elastic",
				xyCO_CenterArrow.x - 0.75,
				xyCO_CenterArrow.y);
   		}
	}

// Reset
   SetData("TextAngle", 0);	
   Execute("menu", "SetLayer", Table("find", "layer", "1"));

 