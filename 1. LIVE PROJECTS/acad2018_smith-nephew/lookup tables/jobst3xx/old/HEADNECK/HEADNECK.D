// File Name:	FaceMask.D
// Purpose: 	Get Patient Details From Drawing
//		And Pass Control To Visual Basic
//		For FaceMask Details
// Version:	1
// Date:		03/April/95
// By:		Ciaran McKavanagh

HANDLE	
	hSel,		 hChan, 		hDDE,
	hHeadNeck;
	
XY	
	xyO,		xyPt;

STRING
	sFileNo,		sPath,		sPatientName,
	sUnits,		sSex,		sAge,
	sDiagnosis,	sWorkOrder,	sFabric,
	sData,		sHeadNeck,	sUID,	sPathJOBST ,
	sSymbolName;	
	
NUMBER	
	nAngle;


// Find JOBST installed directory

   sPathJOBST =  GetProfileString("JOBST", "PathJOBST", "\\JOBST", "DRAFIX.INI");


   Execute ("application", sPathJOBST +"\\HEADNECK\\HeadNeck", "normal");
	hDDE = Open ( "dde","HeadNeck","headneck");
	
   GetData("DrawingFile", &sPath);
   Poke(hDDE, "txtDraw", sPath);	

//get data from drawing and poke to vb
// If "titlebox" already exists use data from it

   hChan = Open ("selection", "DB SymbolName = 'mainpatientdetails'");
   ResetSelection(hChan);
   hSel = GetNextSelection(hChan);
  
  if (hSel) {
	GetGeometry(hSel, &sSymbolName, &xyO, &xyPt, &nAngle);
	GetDBData( hSel, "patient", &sPatientName);
	Poke(hDDE, "txtPatientName", sPatientName);
   	GetDBData( hSel, "fileno", &sFileNo);
	Poke(hDDE, "txtFileNo", sFileNo);
	GetDBData( hSel, "units", &sUnits);
	Poke(hDDE, "txtUnits", sUnits);
	GetDBData( hSel, "sex", &sSex);
	Poke(hDDE, "txtSex", sSex);
	GetDBData( hSel, "diagnosis", &sDiagnosis);
	Poke(hDDE, "txtDiagnosis", sDiagnosis);
	GetDBData( hSel, "age", &sAge);
	Poke(hDDE, "txtAge", sAge);
	GetDBData( hSel, "WorkOrder", &sWorkOrder);
	Poke(hDDE, "txtWorkOrder", sWorkOrder);
	Poke ( hDDE, "txtUidMPD", MakeString("long",UID("get",hSel)));
	}

Close("selection",hChan);

   hChan = Open ("selection", "DB SymbolName = 'headneck'");
   if (hChan) {
 	ResetSelection(hChan);
   	hHeadNeck= GetNextSelection(hChan);
  	}
  
   if (hHeadNeck) {
	GetGeometry(hHeadNeck, &sSymbolName, &xyO, &xyPt, &nAngle);
	GetDBData( hHeadNeck, "UID",&sUID);
	Poke(hDDE, "txtUID",sUID);
	GetDBData( hHeadNeck, "HeadNeck", &sHeadNeck);
	Poke(hDDE, "txtMeasurements", sHeadNeck);
   	GetDBData( hHeadNeck, "Fabric", &sFabric);
	Poke(hDDE, "txtFabric", sFabric);
	GetDBData( hHeadNeck, "Data", &sData);
	Poke(hDDE, "txtData", sData);
	Poke ( hDDE, "txtUidHN", MakeString("long",UID("get",hHeadNeck)));
	}
Close("selection",hChan);
//
//
if (!hDDE) Display ( "message", "error", "Can't start a DDE Link - Retry");
Close ( "dde", hDDE);
 