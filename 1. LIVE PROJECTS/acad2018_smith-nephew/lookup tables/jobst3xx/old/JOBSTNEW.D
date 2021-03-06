// File Name:	JOBSTNEW.D
// Purpose:	New drawing set up for JOBST system.
//		See also DRAFIX.INI
//		
// Version:	2.03
// Date:		15.July.93
// Author:		Gary George
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date	By	Action
//---------------------------------------------------------------------------------------------
// 18.Jan.94	gg	Removed Vest fields to seperate
//			file VFIELDS.D
// 20.Feb.95	gg	Modifications wrt DRAFIX 3.01
//
// 17.May.95	gg	Rationalised the DB Fields
//
// 17.Feb.98	gg	No longer called as an Automacro

// Setup DataBase fields
// TitleBox fields
   Table("add", "field", "patient", "string");
   Table("add", "field", "date", "string");
   Table("add", "field", "orderdate", "string");
   Table("add", "field", "TemplateEngineer", "string");
   Table("add", "field", "WorkOrder", "string");
   Table("add", "field", "Qty", "string");
   Table("add", "field", "fileno", "string");
   Table("add", "field", "units", "string");
   Table("add", "field", "DOBmth", "string");
   Table("add", "field", "DOByr", "string");
   Table("add", "field", "sex", "string");
   Table("add", "field", "diagnosis", "string");
   Table("add", "field", "age", "string");

// General 
   Table("add", "field", "Fabric", "string"); 
   Table("add", "field", "Modulus","string");
   Table("add", "field", "ID", "string");
   Table("add", "field", "Data","string") ;   
   Table("add", "field", "ZipperLength","length");
   Table("add", "field", "Zipper","length");
   
// Vest Sleeve and Arm fields
// Added here as they are common to both ARM and Vest modules
   Table("add", "field", "TapeLengths","string");
   Table("add", "field", "TapeMMs","string");
   Table("add", "field", "Grams","string");
   Table("add", "field", "Reduction","string");
   Table("add", "field", "MM","string");
   Table("add", "field", "Sleeve","string");
   Table("add", "field", "Status","string");
   Table("add", "field", "WristPleat","string");
   Table("add", "field", "ShoulderPleat","string");
   Table("add", "field", "Lining","string");
   Table("add", "field", "Contracture","string");
   Table("add", "field", "curvetype","string");
   Table("add", "field", "Flap","string");
   Table("add", "field", "Gauntlet","string");
   Table("add", "field", "Stump","string");
   Table("add", "field", "TapeLengths2","string");
   Table("add", "field", "TapeMMs2","string");
   Table("add", "field", "Reduction2","string");
   Table("add", "field", "Grams2","string");  
   
// Leg field added here because of its use in
// Patient Details   
   Table("add", "field", "Leg","string");

   
// NOTE
// Additional fields are added according to the application
// Waist Height	- C:\JOBST\WAIST\WHFIELDS.D
// Vest		- C:\JOBST\VEST\VFIELDS.D  
//

// Setup Layers  
//
   Table("add", "layer", "titlebox", "Patient details");  
   Table("add", "layer", "Construct", "Construction layer");
   Table("add", "layer", "Data", "Data layer");
   Table("add", "layer", "Notes", "Notes in Green", "editable", Table("find", "color","green"));
   Table("add", "layer", "TemplateLeft", "Paper Template Left Hand side","editable",Table("find","color","blue"));
   Table("add", "layer", "TemplateRight", "Paper Template Right Hand side","editable",Table("find","color","red"));

// Set units and precision
//
   SetData("UnitLinearType", 6);	// "Inches/Fraction"
   SetData("UnitLinearFracPrec", 3);	// "1/8 ths"
   SetData("DrawingScale", 1.0);	// "1:1"

// Set sheet size
//
   SetData("SheetSizingMethod", 3);	// "Use Printer Page"
   
// Ensure that bylayer is used for style and color
   Execute ("menu", "SetStyle", Table("find", "style", "bylayer")) ;
   Execute ("menu", "SetColor", Table("find", "color", "bylayer")) ; 