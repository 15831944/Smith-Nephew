// File Name:	JOBST.D
// Purpose:	Create User-Defined Button Box for
//		use with Jobst System 
//
// Version:	1.01
// Date:		15.July.93
// Author:		Gary George
//---------------------------------------------------------------------------------------------
// REVISIONS:	
// Date	By	Action
//---------------------------------------------------------------------------------------------
//
// 
// initialize
STRING sPathJOBST;
sPathJOBST = "@" + GetProfileString("JOBST", "PathJOBST", "\\JOBST", "DRAFIX.INI") ;

UserButtons("init", "all","JOBST");

// run macros
UserButtons("add", "macro", " -- JOBST -- ", "");
UserButtons("add", "macro", "File", sPathJOBST+ "\\BUTTONS\\FILBUTTN;");
UserButtons("add", "macro", "Vest", sPathJOBST+ "\\BUTTONS\\VSTBUTTN;");
UserButtons("add", "macro", "Waist Ht",  sPathJOBST+ "\\BUTTONS\\WHBUTTON;");
UserButtons("add", "macro", "Arm", sPathJOBST+ "\\BUTTONS\\ARMBUTTN.D;");
UserButtons("add", "macro", "Leg",  sPathJOBST+ "\\BUTTONS\\LGBUTTON; ");
UserButtons("add", "macro", "Head", sPathJOBST+ "\\BUTTONS\\HNBUTTON; ");
UserButtons("add", "macro", "CAD Glv.", sPathJOBST+ "\\BUTTONS\\GLBUTCAD; ");
UserButtons("add", "macro", "Man Glv.", sPathJOBST+ "\\BUTTONS\\GLBUTMAN; ");
UserButtons("add", "macro", "Bodysuit", sPathJOBST+ "\\BUTTONS\\BDYBUTTN; ");
UserButtons("add", "macro", "Plot", sPathJOBST+ "\\BUTTONS\\PLTBUTTN; ");
UserButtons("add", "macro", "Tools", sPathJOBST+ "\\BUTTONS\\TLSBUTTN; ");

// [EndOfMacro]
 