Option Strict Off
Option Explicit On
Module MANUTILS
	'File:      MANUTILS.BAS
	'Purpose:   Module of utilities for use by the manual
	'           Glove
	'
	'
	'Version:   1.01
	'Date:      30.Jun.95
	'Author:    Gary George
	'
	'-------------------------------------------------------
	'REVISIONS:
	'Date       By      Action
	'-------------------------------------------------------
	'
	'Notes:-
	
	
	'Global constants for use with PR_SetTextData
	'
	Public Const HELVETICA As Short = 10
	Public Const BLOCK As Short = 0
	Public Const LEFT_ As Short = 1
	Public Const RIGHT_ As Short = 4
	Public Const HORIZ_CENTER As Short = 2
	Public Const TOP_ As Short = 8
	Public Const BOTTOM_ As Short = 32
	Public Const VERT_CENTER As Short = 16
	Public Const CURRENT As Short = -1
	
	
	Function Arccos(ByRef X As Double) As Double
		Arccos = System.Math.Atan(-X / System.Math.Sqrt(-X * X + 1)) + 1.5708
	End Function
	
	Sub DebugMarker(ByRef xyPt As XY, ByRef sName As String)
		
		PR_DrawMarker(xyPt)
		PR_DrawText(sName, xyPt, 0.1)
		
	End Sub
	
	
	Function FN_CalcAngle(ByRef xyStart As XY, ByRef xyEnd As XY) As Double
		'Function to return the angle between two points in degrees
		'in the range 0 - 360
		'Zero is always 0 and is never 360
		
		Dim X, Y As Object
		Dim rAngle As Double
		
		'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		X = xyEnd.X - xyStart.X
		'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		Y = xyEnd.Y - xyStart.Y
		
		'Horizontal
		'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If X = 0 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If Y > 0 Then
				FN_CalcAngle = 90
			Else
				FN_CalcAngle = 270
			End If
			Exit Function
		End If
		
		'Vertical (avoid divide by zero later)
		'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If Y = 0 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If X > 0 Then
				FN_CalcAngle = 0
			Else
				FN_CalcAngle = 180
			End If
			Exit Function
		End If
		
		'All other cases
		'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		rAngle = System.Math.Atan(Y / X) * (180 / PI) 'Convert to degrees
		
		If rAngle < 0 Then rAngle = rAngle + 180 'rAngle range is -PI/2 & +PI/2
		
		'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If Y > 0 Then
			FN_CalcAngle = rAngle
		Else
			FN_CalcAngle = rAngle + 180
		End If
		
	End Function
	
	Function FN_CalcLength(ByRef xyStart As XY, ByRef xyEnd As XY) As Double
		'Fuction to return the length between two points
		'Greatfull thanks to Pythagorus
		
		FN_CalcLength = System.Math.Sqrt((xyEnd.X - xyStart.X) ^ 2 + (xyEnd.Y - xyStart.Y) ^ 2)
		
	End Function
	
	Function FN_CirLinInt(ByRef xyStart As XY, ByRef xyEnd As XY, ByRef xyCen As XY, ByRef nRad As Double, ByRef xyInt As XY) As Short
		'Function to calculate the intersection between
		'a line and a circle.
		'Note:-
		'    Returns true if intersection found.
		'    The first intersection (only) is found.
		'    Ported from DRAFIX CAD DLG version.
		'
		
		Static nM, nC, nA, nSlope, nB, nK, nCalcTmp As Object
		Static nRoot As Double
		Static nSign As Short
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nSlope. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nSlope = FN_CalcAngle(xyStart, xyEnd)
		
		'Horizontal Line
		'UPGRADE_WARNING: Couldn't resolve default property of object nSlope. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nSlope = 0 Or nSlope = 180 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object nSlope. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nSlope = -1
			'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nC = nRad ^ 2 - (xyStart.Y - xyCen.Y) ^ 2
			'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If nC < 0 Then
				FN_CirLinInt = False 'no roots
				Exit Function
			End If
			nSign = 1 'test each root
			While nSign > -2
				'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				nRoot = xyCen.X + System.Math.Sqrt(nC) * nSign
				'UPGRADE_WARNING: Couldn't resolve default property of object max(xyStart.X, xyEnd.X). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object min(xyStart.X, xyEnd.X). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				If nRoot >= min(xyStart.X, xyEnd.X) And nRoot <= max(xyStart.X, xyEnd.X) Then
					xyInt.X = nRoot
					xyInt.Y = xyStart.Y
					FN_CirLinInt = True
					Exit Function
				End If
				nSign = nSign - 2
			End While
			FN_CirLinInt = False
			Exit Function
		End If
		
		'Vertical Line
		'UPGRADE_WARNING: Couldn't resolve default property of object nSlope. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nSlope = 90 Or nSlope = 270 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object nSlope. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nSlope = -1
			'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nC = nRad ^ 2 - (xyStart.X - xyCen.X) ^ 2
			'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If nC < 0 Then
				FN_CirLinInt = False 'no roots
				Exit Function
			End If
			nSign = 1 'test each root
			While nSign > -2
				'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				nRoot = xyCen.Y + System.Math.Sqrt(nC) * nSign
				'UPGRADE_WARNING: Couldn't resolve default property of object max(xyStart.Y, xyEnd.Y). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object min(xyStart.Y, xyEnd.Y). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				If nRoot >= min(xyStart.Y, xyEnd.Y) And nRoot <= max(xyStart.Y, xyEnd.Y) Then
					xyInt.Y = nRoot
					xyInt.X = xyStart.X
					FN_CirLinInt = True
					Exit Function
				End If
				nSign = nSign - 2
			End While
			FN_CirLinInt = False
			Exit Function
		End If
		
		'Non-othogonal line
		'UPGRADE_WARNING: Couldn't resolve default property of object nSlope. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nSlope > 0 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object nM. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nM = (xyEnd.Y - xyStart.Y) / (xyEnd.X - xyStart.X) 'Slope
			'UPGRADE_WARNING: Couldn't resolve default property of object nM. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nK. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nK = xyStart.Y - nM * xyStart.X 'Y-Axis intercept
			'UPGRADE_WARNING: Couldn't resolve default property of object nM. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nA. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nA = (1 + nM ^ 2)
			'UPGRADE_WARNING: Couldn't resolve default property of object nM. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nK. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nB. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nB = 2 * (-xyCen.X + (nM * nK) - (xyCen.Y * nM))
			'UPGRADE_WARNING: Couldn't resolve default property of object nK. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nC = (xyCen.X ^ 2) + (nK ^ 2) + (xyCen.Y ^ 2) - (2 * xyCen.Y * nK) - (nRad ^ 2)
			'UPGRADE_WARNING: Couldn't resolve default property of object nA. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nB. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nCalcTmp. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nCalcTmp = (nB ^ 2) - (4 * nC * nA)
			
			'UPGRADE_WARNING: Couldn't resolve default property of object nCalcTmp. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If (nCalcTmp < 0) Then
				FN_CirLinInt = False 'No Roots
				Exit Function
			End If
			nSign = 1
			While nSign > -2
				'UPGRADE_WARNING: Couldn't resolve default property of object nA. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object nCalcTmp. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object nB. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				nRoot = (-nB + (System.Math.Sqrt(nCalcTmp) / nSign)) / (2 * nA)
				'UPGRADE_WARNING: Couldn't resolve default property of object max(xyStart.X, xyEnd.X). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object min(xyStart.X, xyEnd.X). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				If nRoot >= min(xyStart.X, xyEnd.X) And nRoot <= max(xyStart.X, xyEnd.X) Then
					xyInt.X = nRoot
					'UPGRADE_WARNING: Couldn't resolve default property of object nK. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					'UPGRADE_WARNING: Couldn't resolve default property of object nM. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					xyInt.Y = nM * nRoot + nK
					FN_CirLinInt = True
					Exit Function 'Return first root found
				End If
				nSign = nSign - 2
			End While
			FN_CirLinInt = False 'Should never get to here
		End If
		FN_CirLinInt = False
		
	End Function
	
	Function FN_EscapeSlashesInString(ByRef sAssignedString As Object) As String
		'Search through the string looking for " (double quote characater)
		'If found use \ (Backslash) to escape it
		'
		Static ii As Short
		'UPGRADE_NOTE: Char was upgraded to Char_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
		Static Char_Renamed As String
		Static sEscapedString As String
		
		FN_EscapeSlashesInString = ""
		
		For ii = 1 To Len(sAssignedString)
			'UPGRADE_WARNING: Couldn't resolve default property of object sAssignedString. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			Char_Renamed = Mid(sAssignedString, ii, 1)
			If Char_Renamed = "\" Then
				sEscapedString = sEscapedString & "\" & Char_Renamed
			Else
				sEscapedString = sEscapedString & Char_Renamed
			End If
		Next ii
		
		FN_EscapeSlashesInString = sEscapedString
		sEscapedString = ""
		
	End Function
	
	Function max(ByRef nFirst As Object, ByRef nSecond As Object) As Object
		' Returns the maximum of two numbers
		'UPGRADE_WARNING: Couldn't resolve default property of object nSecond. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nFirst. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nFirst >= nSecond Then
			'UPGRADE_WARNING: Couldn't resolve default property of object nFirst. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object max. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			max = nFirst
		Else
			'UPGRADE_WARNING: Couldn't resolve default property of object nSecond. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object max. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			max = nSecond
		End If
	End Function
	
	Function min(ByRef nFirst As Object, ByRef nSecond As Object) As Object
		' Returns the minimum of two numbers
		'UPGRADE_WARNING: Couldn't resolve default property of object nSecond. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nFirst. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nFirst <= nSecond Then
			'UPGRADE_WARNING: Couldn't resolve default property of object nFirst. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object min. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			min = nFirst
		Else
			'UPGRADE_WARNING: Couldn't resolve default property of object nSecond. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object min. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			min = nSecond
		End If
	End Function
	
	Sub PR_AddDBValueToLast(ByRef sDBName As String, ByRef sDBValue As String)
		'The last entity is given by hEnt
		'
		'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "if (hEnt) SetDBData( hEnt," & QQ & sDBName & QQ & CC & QQ & sDBValue & QQ & ");")
		
	End Sub
	
	
	Sub PR_AddVertex(ByRef xyPoint As XY, ByRef nBulge As Double)
		'To the DRAFIX macro file (given by the global fNum)
		'write the syntax to add a Vertex to a polyline opened
		'by PR_StartPoly
		'Allow the use of a bulge factor, but start and end widths
		'will be 0.
		'For this to work it assumes that the following DRAFIX variables
		'are defined and initialised
		'    XY      xyStart
		'    HANDLE  hEnt
		'
		'Note:-
		'    fNum, CC, QQ, NL,  are globals initialised by FN_Open
		'
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "  AddVertex(" & "xyStart.x+" & xyPoint.X & CC & "xyStart.y+" & xyPoint.Y & CC & "0,0," & nBulge & ");")
		
	End Sub
	
	Sub PR_CalcMidPoint(ByRef xyStart As XY, ByRef xyEnd As XY, ByRef xyMid As XY)
		
		Static aAngle, nLength As Double
		
		aAngle = FN_CalcAngle(xyStart, xyEnd)
		nLength = FN_CalcLength(xyStart, xyEnd)
		
		If nLength = 0 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object xyMid. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			xyMid = xyStart 'Avoid overflow
		Else
			PR_CalcPolar(xyStart, aAngle, nLength / 2, xyMid)
		End If
		
	End Sub
	
	Sub PR_CalcPolar(ByRef xyStart As XY, ByVal nAngle As Double, ByVal nLength As Double, ByRef xyReturn As XY)
		'Procedure to return a point at a distance and an angle from a given point
		'
		'PI is a Global Constant
		
		Static A, B As Double
		
		'Ensure angles are +ve and between 0 to 360 degrees
		If nAngle > 360 Then nAngle = nAngle - 360
		If nAngle < 0 Then nAngle = nAngle + 360
		
		Select Case nAngle
			Case 0
				xyReturn.X = xyStart.X + nLength
				xyReturn.Y = xyStart.Y
			Case 180
				xyReturn.X = xyStart.X - nLength
				xyReturn.Y = xyStart.Y
			Case 90
				xyReturn.X = xyStart.X
				xyReturn.Y = xyStart.Y + nLength
			Case 270
				xyReturn.X = xyStart.X
				xyReturn.Y = xyStart.Y - nLength
			Case Else
				'Convert from degees to radians
				nAngle = nAngle * PI / 180
				B = System.Math.Sin(nAngle) * nLength
				A = System.Math.Cos(nAngle) * nLength
				xyReturn.X = xyStart.X + A
				xyReturn.Y = xyStart.Y + B
		End Select
		
	End Sub
	
	Sub PR_DrawArc(ByRef xyCen As XY, ByRef xyArcStart As XY, ByRef xyArcEnd As XY)
		'Draws an arc
		'Restrictions
		'    1. Arc must be 180 degrees or less
		
		Static nAdj, nDeltaAng, nStartAng, nRad, nEndAng, nOpp, nSign As Object
		Static xyMidPoint As XY
		
		' ORIGINAL CODE ********************
		'    nRad = FN_CalcLength(xyCen, xyArcStart)
		'
		'    nStartAng = FN_CalcAngle(xyCen, xyArcStart)
		'
		'    nEndAng = FN_CalcAngle(xyCen, xyArcEnd)
		'
		'    If Side > 0 Then nDeltaAng = Abs(nEndAng - nStartAng) Else nDeltaAng = nEndAng - nStartAng
		' ORIGINAL CODE ********************
		'
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nRad. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nRad = FN_CalcLength(xyCen, xyArcStart)
		'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nStartAng = FN_CalcAngle(xyCen, xyArcStart)
		'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nEndAng = FN_CalcAngle(xyCen, xyArcEnd)
		
		'Direction of arc
		'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nSign. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nSign = System.Math.Sign(nEndAng - nStartAng)
		'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If System.Math.Abs(nEndAng - nStartAng) > 180 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object nSign. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nSign = 0 - nSign
		End If
		
		'Included angle
		PR_CalcMidPoint(xyArcStart, xyArcEnd, xyMidPoint)
		'UPGRADE_WARNING: Couldn't resolve default property of object nAdj. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nAdj = FN_CalcLength(xyCen, xyMidPoint)
		'UPGRADE_WARNING: Couldn't resolve default property of object nOpp. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nOpp = FN_CalcLength(xyArcStart, xyArcEnd) / 2
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nAdj. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nAdj = 0 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nDeltaAng = 180
		Else
			'UPGRADE_WARNING: Couldn't resolve default property of object nAdj. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nOpp. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			nDeltaAng = (System.Math.Atan(nOpp / nAdj) * 2) * (180 / PI)
		End If
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nSign. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nDeltaAng = nDeltaAng * nSign
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nRad. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hEnt = AddEntity(" & QQ & "arc" & QC & "xyStart.x +" & Str(xyCen.X) & CC & "xyStart.y +" & Str(xyCen.Y) & CC & Str(nRad) & CC & Str(nStartAng) & CC & Str(nDeltaAng) & ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		
	End Sub
	
	Sub PR_DrawFitted(ByRef Profile As Curve)
		'To the DRAFIX macro file (given by the global fNum)
		'write the syntax to draw a FITTED curve through the points
		'given in Profile.
		'For this to work it assumes that the following DRAFIX variables
		'are defined
		'    XY      xyStart
		'    HANDLE  hEnt
		'
		'Note:-
		'    fNum, CC, QQ, NL are globals initialised by FN_Open
		'
		'
		Dim ii As Short
		
		'Draw the profile
		'    If there is no vertex or only one vertex then exit.
		'    For two vertex draw as a polyline (this degenerates to a Single line).
		'    For three vertex draw as a polyline (as no fitted curve can be drawn
		'    by a macro).
		'
		Select Case Profile.n
			Case 0 To 1
				Exit Sub
			Case 3
				PR_DrawPoly(Profile)
				'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
				'Warn the user to smooth the curve
				'Not required by Glove
				'           Print #fNum, "Display ("; QQ; "message"; QCQ; "OKquestion"; QCQ; "The Profile has been drawn as a POLYLINE\nEdit this line and make it OPEN FITTED,\n this will then be a smooth line"; QQ; ");"
			Case Else
				'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				PrintLine(fNum, "hEnt = AddEntity(" & QQ & "poly" & QCQ & "fitted" & QQ)
				For ii = 1 To Profile.n
					'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					PrintLine(fNum, CC & "xyStart.x+" & Str(Profile.X(ii)) & CC & "xyStart.y+" & Str(Profile.Y(ii)))
				Next ii
				'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				PrintLine(fNum, ");")
				'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		End Select
		
	End Sub
	
	Sub PR_DrawLine(ByRef xyStart As XY, ByRef xyFinish As XY)
		'To the DRAFIX macro file (given by the global fNum).
		'Write the syntax to draw a LINE between two points.
		'For this to work it assumes that the following DRAFIX variables
		'are defined
		'    XY      xyStart
		'    HANDLE  hEnt
		'
		'Note:-
		'    fNum, CC, QQ, NL are globals initialised by FN_Open
		'
		'Ignore 0 length lines
		If xyStart.X = xyFinish.X And xyStart.Y = xyFinish.Y Then Exit Sub
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hEnt = AddEntity(")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, QQ & "line" & QC)
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "xyStart.x+" & Str(xyStart.X) & CC & "xyStart.y+" & Str(xyStart.Y) & CC)
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "xyStart.x+" & Str(xyFinish.X) & CC & "xyStart.y+" & Str(xyFinish.Y))
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		
		
	End Sub
	
	Sub PR_DrawMarker(ByRef xyPoint As XY)
		'Draw a Marker at the given point
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hEnt = AddEntity(" & QQ & "marker" & QCQ & "xmarker" & QC & "xyStart.x+" & xyPoint.X & CC & "xyStart.y+" & xyPoint.Y & CC & "0.0625);")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		
	End Sub
	
	Sub PR_DrawMarkerNamed(ByRef sName As String, ByRef xyPoint As XY, ByRef nWidth As Object, ByRef nHeight As Object, ByRef nAngle As Object)
		'Draw a Marker at the given point
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hEnt = AddEntity(" & QQ & "marker" & QCQ & sName & QC & "xyStart.x+" & xyPoint.X & CC & "xyStart.y+" & xyPoint.Y & CC & nWidth & CC & nHeight & CC & nAngle & ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		
	End Sub
	
	Sub PR_DrawPoly(ByRef Profile As Curve)
		'To the DRAFIX macro file (given by the global fNum)
		'write the syntax to draw a POLYLINE through the points
		'given in Profile.
		'For this to work it assumes that the following DRAFIX variables
		'are defined
		'    XY      xyStart
		'    HANDLE  hEnt
		'
		'Note:-
		'    fNum, CC, QQ, NL are globals initialised by FN_Open
		'
		'
		Dim ii As Short
		
		'Exit if nothing to draw
		If Profile.n <= 1 Then Exit Sub
		
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hEnt = AddEntity(" & QQ & "poly" & QCQ & "polyline" & QQ)
		For ii = 1 To Profile.n
			'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PrintLine(fNum, CC & "xyStart.x+" & Str(Profile.X(ii)) & CC & "xyStart.y+" & Str(Profile.Y(ii)))
		Next ii
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		
	End Sub
	
	
	Sub PR_DrawText(ByRef sText As Object, ByRef xyInsert As XY, ByRef nHeight As Object)
		'To the DRAFIX macro file (given by the global fNum).
		'Write the syntax to draw TEXT at the given height.
		'
		'For this to work it assumes that the following DRAFIX variables
		'are defined
		'    XY      xyStart
		'
		'Note:-
		'    fNum, CC, QQ, NL, g_nCurrTextAspect are globals initialised by FN_Open
		'
		'
		Dim nWidth As Object
		'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nHeight. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object nWidth. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		nWidth = nHeight * g_nCurrTextAspect
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "AddEntity(" & QQ & "text" & QCQ & sText & QC & "xyStart.x+" & Str(xyInsert.X) & CC & "xyStart.y+" & Str(xyInsert.Y) & CC & nWidth & CC & nHeight & CC & g_nCurrTextAngle & ");")
		
	End Sub
	
	
	Sub PR_EndPoly()
		'To the DRAFIX macro file (given by the global fNum)
		'write the syntax to end a POLYLINE
		'For this to work it assumes that the following DRAFIX variables
		'are defined and initialised
		'    XY      xyStart
		'    HANDLE  hEnt
		'    STRING  sID
		'
		'Note:-
		'    fNum, CC, QQ, NL,  are globals initialised by FN_Open
		'
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "EndPoly();")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hEnt = UID (""find"", UID (""getmax"")) ;")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		
	End Sub
	
	Sub PR_InsertSymbol(ByRef sSymbol As String, ByRef xyInsert As XY, ByRef nScale As Single, ByRef nRotation As Single)
		'To the DRAFIX macro file (given by the global fNum).
		'Write the syntax to insert a SYMBOL.
		'Where:-
		'    sSymbol     Symbol name, must exist and be in the symbol library
		'    xyInsert    The insertion point
		'    nScale      Symbol scaling factor, 1 = No scaling
		'    nRotation   Symbol rotation about insertion point
		'For this to work it assumes that the following DRAFIX variables
		'are defined
		'    XY      xyStart
		'    HANDLE  hEnt
		'and
		'    The DRAFIX symbol library sPathJOBST + "\JOBST.SLB" exists
		'
		'Note:-
		'    fNum, CC, QQ, NL, QCQ are globals initialised by FN_Open
		'    g_sPathJOBST is path to JOBST CAD System
		'
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetSymbolLibrary(" & QQ & FN_EscapeSlashesInString(g_sPathJOBST) & "\\JOBST.SLB" & QQ & ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "Symbol(" & QQ & "find" & QCQ & sSymbol & QQ & ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hEnt = AddEntity(" & QQ & "symbol" & QCQ & sSymbol & QC)
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "xyStart.x+" & Str(xyInsert.X) & CC & "xyStart.y+" & Str(xyInsert.Y) & CC)
		'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, Str(nScale) & CC & Str(nScale) & CC & Str(nRotation) & ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")
		
	End Sub
	
	Sub PR_MakeXY(ByRef xyReturn As XY, ByRef X As Double, ByRef Y As Double)
		'Utility to return a point based on the X and Y values
		'given
		xyReturn.X = X
		xyReturn.Y = Y
		
	End Sub
	
	Sub PR_MirrorCurveInYaxis(ByRef nXValue As Double, ByRef nTranslate As Double, ByRef Profile As Curve)
		'Procedure to mirror a curve in the y axis about
		'a line given by the X value
		'
		Static ii As Short
		For ii = 1 To Profile.n
			Profile.X(ii) = (nXValue - (Profile.X(ii) - nXValue)) + nTranslate
		Next ii
		
	End Sub
	
	Sub PR_MirrorPointInYaxis(ByRef nXValue As Double, ByRef nTranslate As Double, ByRef xyPoint As XY)
		'Procedure to mirror a Point in the y axis about
		'a line given by the X value
		'
		xyPoint.X = (nXValue - (xyPoint.X - nXValue)) + nTranslate
		
	End Sub
	
	Sub PR_NamedHandle(ByRef sHandleName As String)
		'To the DRAFIX macro file (given by the global fNum).
		'Write the syntax to retain the entity handle of a previously
		'added entity.
		'
		'Assumes that hEnt is the entity handle to the just inserted entity.
		'
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "HANDLE " & sHandleName & ";")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, sHandleName & " = hEnt;")
		
	End Sub
	
	Sub PR_RotateCurve(ByRef xyRotate As XY, ByRef aRotation As Double, ByRef Profile As Curve)
		Static ii As Short
		Static xyVertex As XY
		For ii = 1 To Profile.n
			PR_MakeXY(xyVertex, Profile.X(ii), Profile.Y(ii))
			PR_RotatePoint(xyRotate, aRotation, xyVertex)
			Profile.X(ii) = xyVertex.X
			Profile.Y(ii) = xyVertex.Y
		Next ii
	End Sub
	
	Sub PR_RotatePoint(ByRef xyRotate As XY, ByRef aRotation As Double, ByRef xyPoint As XY)
		Static aAngle, nLength As Double
		
		'UPGRADE_WARNING: Mod has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
		aAngle = (FN_CalcAngle(xyRotate, xyPoint) + aRotation) Mod 360
		nLength = FN_CalcLength(xyRotate, xyPoint)
		PR_CalcPolar(xyRotate, aAngle, nLength, xyPoint)
		
	End Sub
	
	Sub PR_Setlayer(ByRef sNewLayer As String)
		'To the DRAFIX macro file (given by the global fNum).
		'Write the syntax to set the current LAYER.
		'For this to work it assumes that hLayer is defined in DRAFIX as
		'a HANDLE.
		'
		'Note:-
		'    fNum, CC, QQ, NL, g_sCurrentLayer are globals initialised by FN_Open
		'
		'To reduce unessesary writing of DRAFIX code check that the new layer
		'is different from the Current layer, change only if it is different.
		'
		
		If g_sCurrentLayer = sNewLayer Then Exit Sub
		g_sCurrentLayer = sNewLayer
		
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "hLayer = Table(" & QQ & "find" & QCQ & "layer" & QCQ & sNewLayer & QQ & ");")
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "if ( hLayer > %zero && hLayer != 32768)" & "Execute (" & QQ & "menu" & QCQ & "SetLayer" & QC & "hLayer);")
		
	End Sub
	
	Sub PR_SetLineStyle(ByRef sStyle As String)
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "Execute (" & QQ & "menu" & QCQ & "SetStyle" & QC & "Table(" & QQ & "find" & QCQ & "style" & QCQ & sStyle & QQ & "));")
	End Sub
	
	Sub PR_SetTextData(ByRef nHoriz As Object, ByRef nVert As Object, ByRef nHt As Object, ByRef nAspect As Object, ByRef nFont As Object)
		'To the DRAFIX macro file (given by the global fNum).
		'Write the syntax to set the TEXT default attributes, these are
		'based on the values in the arguments.  Where the value is -ve then this
		'attribute is not set.
		'where :-
		'    nHoriz      Horizontal justification (1=Left, 2=Cen, 4=Right)
		'    nVert       Verticalal justification (8=Top, 16=Cen, 32=Bottom)
		'    nHt         Text height
		'    nAspect     Text aspect ratio (heigth/width)
		'    nFont       Text font (0 to 18)
		'
		'N.B. No checking is done on the values given
		'
		'Note:-
		'    fNum, CC, QQ, NL, g_nCurrTextHt, g_nCurrTextAspect,
		'    g_nCurrTextHorizJust, g_nCurrTextVertJust, g_nCurrTextFont
		'    are globals initialised by FN_Open
		'
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nHoriz. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHorizJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nHoriz >= 0 And g_nCurrTextHorizJust <> nHoriz Then
			'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PrintLine(fNum, "SetData(" & QQ & "TextHorzJust" & QC & nHoriz & ");")
			'UPGRADE_WARNING: Couldn't resolve default property of object nHoriz. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHorizJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			g_nCurrTextHorizJust = nHoriz
		End If
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nVert. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextVertJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nVert >= 0 And g_nCurrTextVertJust <> nVert Then
			'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PrintLine(fNum, "SetData(" & QQ & "TextVertJust" & QC & nVert & ");")
			'UPGRADE_WARNING: Couldn't resolve default property of object nVert. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextVertJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			g_nCurrTextVertJust = nVert
		End If
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nHt >= 0 And g_nCurrTextHt <> nHt Then
			'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PrintLine(fNum, "SetData(" & QQ & "TextHeight" & QC & nHt & ");")
			'UPGRADE_WARNING: Couldn't resolve default property of object nHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			g_nCurrTextHt = nHt
		End If
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nAspect >= 0 And g_nCurrTextAspect <> nAspect Then
			'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PrintLine(fNum, "SetData(" & QQ & "TextAspect" & QC & nAspect & ");")
			'UPGRADE_WARNING: Couldn't resolve default property of object nAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			g_nCurrTextAspect = nAspect
		End If
		
		'UPGRADE_WARNING: Couldn't resolve default property of object nFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If nFont >= 0 And g_nCurrTextFont <> nFont Then
			'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PrintLine(fNum, "SetData(" & QQ & "TextFont" & QC & nFont & ");")
			'UPGRADE_WARNING: Couldn't resolve default property of object nFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			g_nCurrTextFont = nFont
		End If
		
		
	End Sub
	
	Sub PR_StartPoly()
		'To the DRAFIX macro file (given by the global fNum)
		'write the syntax to start a POLYLINE the points will
		'be given by the PR_AddVertex and finished by PR_EndPoly
		'For this to work it assumes that the following DRAFIX variables
		'are defined and initialised
		'    XY      xyStart
		'    HANDLE  hEnt
		'    STRING  sID
		'
		'
		'Note:-
		'    fNum, CC, QQ, NL are globals initialised by FN_Open
		'
		'
		
		'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		PrintLine(fNum, "StartPoly (""PolyLine"");")
		
		
	End Sub
	
	Function round(ByVal nNumber As Double) As Short
		'Fuction to return the rounded value of a decimal number
		'E.G.
		'    round(1.35) = 1
		'    round(1.55) = 2
		'    round(2.50) = 3
		'    round(-2.50) = -3
		'
		
		Static nInt, nSign As Short
		
		nSign = System.Math.Sign(nNumber)
		nNumber = System.Math.Abs(nNumber)
		nInt = Int(nNumber)
		If (nNumber - nInt) >= 0.5 Then
			round = (nInt + 1) * nSign
		Else
			round = nInt * nSign
		End If
		
	End Function
End Module