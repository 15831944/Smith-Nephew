Option Strict Off
Option Explicit On
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports VB = Microsoft.VisualBasic
Friend Class vestdia
	Inherits System.Windows.Forms.Form
    'Project:   VESTDIA
    'Purpose:   Vest Dialogue
    '
    '
    'Version:   3.01
    'Date:      4.Oct.95
    'Author:    Gary George
    '
    '-------------------------------------------------------
    'REVISIONS:
    'Date       By      Action
    '-------------------------------------------------------
    'Dec 98     GG      Ported to VB5
    '
    'Notes:-
    '
    '
    '
    '
    '   'Windows API Functions Declarations
    '    Private Declare Function GetActiveWindow Lib "User" () As Integer
    '    Private Declare Function IsWindow Lib "User" (ByVal hwnd As Integer) As Integer
    '    Private Declare Function GetWindow Lib "User" (ByVal hwnd As Integer, ByVal wCmd As Integer) As Integer
    '    Private Declare Function GetWindowText Lib "User" (ByVal hwnd As Integer, ByVal lpString As String, ByVal aint As Integer) As Integer
    '    Private Declare Function GetWindowTextLength Lib "User" (ByVal hwnd As Integer) As Integer
    '    Private Declare Function GetNumTasks Lib "Kernel" () As Integer
    '    Private Declare Function GetWindowsDirectory% Lib "Kernel" (ByVal lpBuffer$, ByVal nSize%)
    '    Private Declare Function GetPrivateProfileString% Lib "Kernel" (ByVal lpApplicationName$, ByVal lpKeyName As Any, ByVal lpDefault$, ByVal lpReturnedString$, ByVal nSize%, ByVal lpFileName$)


    '   'Constanst used by GetWindow
    '    Const GW_CHILD = 5
    '    Const GW_HWNDFIRST = 0
    '    Const GW_HWNDLAST = 1
    '    Const GW_HWNDNEXT = 2
    '    Const GW_HWNDPREV = 3
    '    Const GW_OWNER = 4

    '   'MsgBox constant
    '    Const IDCANCEL = 2
    '    Const IDYES = 6
    '    Const IDNO = 7


    Structure xy
        Dim X As Double
        Dim Y As Double
    End Structure

    'Globals set by FN_Open
    Public CC As Object 'Comma
    Public QQ As Object 'Quote
    Public NL As Object 'Newline
    Public fNum As Object 'Macro file number
    Public QCQ As Object 'Quote Comma Quote
    Public QC As Object 'Quote Comma
    Public CQ As Object 'Comma Quote


    Public g_sFrontNeck As String
    Public g_PathJOBST As String
    ''-----------Public g_nUnitsFac As Double
    Public g_sChangeChecker As String
    Public g_sBackNeck As String
    Public g_sLeftCup As String
    Public g_sRightDisk As String
    Public g_nUnderBreast As Double
    Public g_sLeftDisk As String
    Public g_sCurrentLayer As String
    Public g_sRightCup As String
    Public g_nChest As Double
    Dim xyInsertion As xy
    Dim g_nAxillaFrontNeckRad As Double
    Dim g_nAxillaBackNeckRad As Double
    Dim g_nShoulderToBackRaglan As Double
    Dim g_sSide As String

    Private Sub Cancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Cancel.Click
        Dim Response As Short
        Dim sTask, sCurrentValues As String

        'Check if data has been modified
        sCurrentValues = FN_ValuesString()

        If sCurrentValues <> g_sChangeChecker Then
            Response = MsgBox("Changes have been made, Save changes before closing", 35, "CAD - Vest Dialogue")
            Select Case Response
                Case IDYES
                    ''PR_CreateMacro_Save("c:\jobst\draw.d")
                    Dim sDrawFile As String = fnGetSettingsPath("PathDRAW") & "\draw.d"
                    PR_CreateMacro_Save(sDrawFile)
                    '               sTask = fnGetDrafixWindowTitleText()
                    'If sTask <> "" Then
                    '	AppActivate(sTask)
                    '	System.Windows.Forms.SendKeys.SendWait("@c:\jobst\draw.d{enter}")
                    '                   Return
                    '               Else
                    '	MsgBox("Can't find a Drafix Drawing to update!", 16, "VEST Body - Dialogue")
                    'End If
                    saveInfoToDWG()
                    Me.Close()
                Case IDNO
                    Me.Close()
                Case IDCANCEL
                    Exit Sub
            End Select
        Else
            Me.Close()
        End If
        VestMain.VestMainDlg.Close()
    End Sub
    Public Function PR_CloseVestBodyDialog() As Boolean
        Dim Response As Short
        Dim sCurrentValues As String

        'Check if data has been modified
        sCurrentValues = FN_ValuesString()
        If sCurrentValues <> g_sChangeChecker Then
            Response = MsgBox("Changes have been made, Save changes before closing", 35, "Vest - Body")
            Select Case Response
                Case IDYES
                    Dim sDrawFile As String = fnGetSettingsPath("PathDRAW") & "\draw.d"
                    PR_CreateMacro_Save(sDrawFile)
                    saveInfoToDWG()
                Case IDCANCEL
                    Return False
            End Select
        End If
        Return True
    End Function


    'UPGRADE_WARNING: Event cboBackNeck.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
    Private Sub cboBackNeck_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboBackNeck.SelectedIndexChanged
		Select Case VB.Left(cboBackNeck.Text, 1)
			Case "R", "S" 'Regular or Scoop
				txtBackNeck.Text = ""
				lblBackNeck.Text = ""
				txtBackNeck.Enabled = False
				labBackNeck.Enabled = False
			Case Else 'Measured Scoop
				If g_sBackNeck <> "" Then
					txtBackNeck.Text = g_sFrontNeck
					txtBackNeck_Leave(txtBackNeck, New System.EventArgs()) 'Display inches
				End If
				txtBackNeck.Enabled = True
				labBackNeck.Enabled = True
		End Select
	End Sub
	
	'UPGRADE_WARNING: Event cboFrontNeck.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
	Private Sub cboFrontNeck_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboFrontNeck.SelectedIndexChanged
		Select Case VB.Left(cboFrontNeck.Text, 1)
			Case "R", "S" 'Regular or Scoop
				txtFrontNeck.Text = ""
				lblFrontNeck.Text = ""
				txtFrontNeck.Enabled = False
				labFrontNeck.Enabled = False
			Case Else 'Measured Scoop or Turtle neck
				If g_sFrontNeck <> "" Then
					txtFrontNeck.Text = g_sFrontNeck
					txtFrontNeck_Leave(txtFrontNeck, New System.EventArgs()) 'Display inches
				End If
				txtFrontNeck.Enabled = True
				labFrontNeck.Enabled = True
		End Select
	End Sub
	
	'UPGRADE_WARNING: Event cboLeftCup.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
	Private Sub cboLeftCup_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboLeftCup.SelectedIndexChanged
		If Me.Visible And (cboLeftCup.Text = "" Or cboLeftCup.Text = "None") Then
			txtLeftDisk.Text = ""
			g_sLeftCup = ""
			g_sLeftDisk = ""
		Else
			PR_EnableCalculateDiskButton()
		End If
	End Sub
	
	'UPGRADE_WARNING: Event cboRightCup.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
	Private Sub cboRightCup_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboRightCup.SelectedIndexChanged
		If Me.Visible And (cboRightCup.Text = "" Or cboRightCup.Text = "None") Then
			txtRightDisk.Text = ""
			g_sRightCup = ""
			g_sRightDisk = ""
		Else
			PR_EnableCalculateDiskButton()
			'PR_DoBraCupsAndDisks
		End If
	End Sub

    Private Sub cmdCalculateBraDisks_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles CalculateBraDisks.Click
        PR_DoBraCupsAndDisks()
    End Sub

    Private Sub cmdTab_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdTab.Click
        'Allows the user to use enter as a tab
        System.Windows.Forms.SendKeys.Send("{TAB}")
    End Sub

    Private Function FN_CalculateBra(ByRef nChest As Double, ByRef nUnderBreast As Double, ByRef nNipple As Double, ByRef sCup As String, ByRef iDisk As Short, ByRef sSide As String) As Short
        'Procedure to calcluate the size of a BRA
        'Returning the CUP size and the Bra disk size.
        '
        'INPUT
        '    nChest   Chest Circumference (Inches)
        '
        '    nNipple  Circumference over nipple line (Inches)
        '
        '    nUnderBreast
        '             Circumference just under the breast (Inches)
        '
        '
        '    sCup        Cup size
        '                If a cup size is given then disk size
        '                is based on the given cup.
        '
        '    sSide       Used when displaying error & warning messages
        '
        'OUTPUT
        '    sCup        If no cup is given then a cup is calculated
        '                and the disk size calculated from this cup.
        '
        '    iDisk       Size of disk to be used on template
        '
        '    FN_CalculateBra
        '                True if no errors
        '                False if errors, Message returned in sError
        '
        '    If sCup [and | or] iDisk are given then these are not changed
        '    the values are calculated and a warning given if the calculated
        '    values are different from the given
        '
        'NOTES
        '    Cup sizes   TRAINING, A, B, C, D, E, NONE
        '    Cup sizes   -1, 0 and 1 to 9
        '                Where 0 => a specified missing cup.
        '                and  -1 => failure to calculate.
        '
        'SPECIFICATIONS
        '    GOP 01-02/17    VEST WITHOUT SLEEVES
        '                    Pages 11,12 29.October.1991
        '    GOP 01-02/17    VEST WITH SLEEVES
        '                    Pages 17,18 29.October.1991
        '
        '    BODYBRA.D       DRAFIX macro version 1.04
        '
        '    BRACHART.DAT    Data used in conjunction with
        '                    the macro BODYBRA.D
        '

        'Variables
        Dim nDiff As Double
        Dim nSelctedDisk As Short
        Dim sError As String

        'Initially set to false
        FN_CalculateBra = False
        sError = ""

        'Simple case for Cup type = "None"
        If sCup = "None" Then
            FN_CalculateBra = True
            iDisk = 0
            Exit Function
        End If


        'Calulate bracup
        If sCup = "" Then
            If nNipple = 0 Then
                FN_CalculateBra = False
                sError = "Can't calculate a cup as Circumference over Nipple line is missing"
                MsgBox(sError, 0, "VEST Body - Bra Cup" & "(" & sSide & ")")
                Exit Function
            End If

            nDiff = nNipple - nChest
            If nDiff < 0 Then
                FN_CalculateBra = False
                sError = "Circumference over Nipple line is smaller than Chest circumference"
                MsgBox(sError, 0, "VEST Body - Bra Cup" & "(" & sSide & ")")
                Exit Function
            End If

            'Calculate bra cup
            If nDiff <= 1.375 Then
                sCup = "A"
            ElseIf nDiff <= 2.375 Then
                sCup = "B"
            ElseIf nDiff <= 3.375 Then
                sCup = "C"
            ElseIf nDiff <= 4.375 Then
                sCup = "D"
            Else
                sCup = "E"
            End If
        End If


        'Bra cup to disk mappings
        nSelctedDisk = -1
        Select Case nUnderBreast
            Case 26 To 28.99
                If sCup = "Training" Then
                    nSelctedDisk = 1
                ElseIf sCup = "A" Then
                    nSelctedDisk = 1
                ElseIf sCup = "B" Then
                    nSelctedDisk = 2
                ElseIf sCup = "C" Then
                    nSelctedDisk = 3
                ElseIf sCup = "D" Then
                    nSelctedDisk = 4
                ElseIf sCup = "E" Then
                    nSelctedDisk = 5
                Else : nSelctedDisk = -1
                End If
            Case 29 To 31.99
                If sCup = "Training" Then
                    nSelctedDisk = 1
                ElseIf sCup = "A" Then
                    nSelctedDisk = 2
                ElseIf sCup = "B" Then
                    nSelctedDisk = 3
                ElseIf sCup = "C" Then
                    nSelctedDisk = 4
                ElseIf sCup = "D" Then
                    nSelctedDisk = 5
                ElseIf sCup = "E" Then
                    nSelctedDisk = 6
                Else : nSelctedDisk = -1
                End If
            Case 32 To 34.99
                If sCup = "Training" Then
                    nSelctedDisk = 2
                ElseIf sCup = "A" Then
                    nSelctedDisk = 3
                ElseIf sCup = "B" Then
                    nSelctedDisk = 4
                ElseIf sCup = "C" Then
                    nSelctedDisk = 5
                ElseIf sCup = "D" Then
                    nSelctedDisk = 6
                ElseIf sCup = "E" Then
                    nSelctedDisk = 7
                Else : nSelctedDisk = -1
                End If
            Case 35 To 37.99
                If sCup = "Training" Then
                    nSelctedDisk = 3
                ElseIf sCup = "A" Then
                    nSelctedDisk = 4
                ElseIf sCup = "B" Then
                    nSelctedDisk = 5
                ElseIf sCup = "C" Then
                    nSelctedDisk = 6
                ElseIf sCup = "D" Then
                    nSelctedDisk = 7
                ElseIf sCup = "E" Then
                    nSelctedDisk = 8
                Else : nSelctedDisk = -1
                End If
            Case 38 To 40.99
                If sCup = "A" Then
                    nSelctedDisk = 5
                ElseIf sCup = "B" Then
                    nSelctedDisk = 6
                ElseIf sCup = "C" Then
                    nSelctedDisk = 7
                ElseIf sCup = "D" Then
                    nSelctedDisk = 8
                ElseIf sCup = "E" Then
                    nSelctedDisk = 9
                Else : nSelctedDisk = -1
                End If
            Case 41 To 44
                If sCup = "A" Then
                    nSelctedDisk = 6
                ElseIf sCup = "B" Then
                    nSelctedDisk = 7
                ElseIf sCup = "C" Then
                    nSelctedDisk = 8
                ElseIf sCup = "D" Then
                    nSelctedDisk = 9
                Else : nSelctedDisk = -1
                End If
        End Select

        If nSelctedDisk = -1 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = "No Bra disk is availabe for a Cup Size " & sCup & NL & "For an under breast circumference of " & ARMEDDIA1.fnInchesToText(nUnderBreast)
            MsgBox(sError, 0, "VEST Body - Bra Cup" & "(" & sSide & ")")
            FN_CalculateBra = False
            Exit Function
        End If

        'Cut back disk by 1 if it is over 5
        If nSelctedDisk > 5 Then nSelctedDisk = nSelctedDisk - 1


        'Set disk size.
        'If disk is given then only check that size is the same, warn if not!
        If iDisk > 0 Then
            If iDisk <> nSelctedDisk Then
                'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                sError = "Warning" & NL & "The Given disk is different in size to the disk that would have been calculated!"
                'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                sError = sError & NL & "Given Disk      : " & Str(iDisk)
                'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                sError = sError & NL & "Calculated Disk : " & Str(nSelctedDisk)
                MsgBox(sError, 0, "VEST Body - Bra Cup" & "(" & sSide & ")")
            End If
        Else
            iDisk = nSelctedDisk
        End If

        FN_CalculateBra = True


    End Function

    Private Function FN_EscapeQuotesInString(ByRef sAssignedString As Object) As String
        'Search through the string looking for " (double quote characater)
        'If found use \ (Backslash) to escape it
        '
        Dim ii As Short
        'UPGRADE_NOTE: Char was upgraded to Char_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
        Dim Char_Renamed As String
        Dim sEscapedString As String

        FN_EscapeQuotesInString = ""

        For ii = 1 To Len(sAssignedString)
            'UPGRADE_WARNING: Couldn't resolve default property of object sAssignedString. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Char_Renamed = Mid(sAssignedString, ii, 1)
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If Char_Renamed = QQ Then
                sEscapedString = sEscapedString & "\" & Char_Renamed
            Else
                sEscapedString = sEscapedString & Char_Renamed
            End If
        Next ii

        FN_EscapeQuotesInString = sEscapedString

    End Function

    Private Function FN_Open(ByRef sDrafixFile As String, ByRef sType As String, ByRef sName As Object, ByRef sFileNo As Object) As Short
        'Open the DRAFIX macro file
        'Return the file number

        'Open file
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FreeFile()
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileOpen(fNum, sDrafixFile, VB.OpenMode.Output)
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FN_Open = fNum

        'Write header information etc. to the DRAFIX macro file
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//DRAFIX Macro created - " & DateString & "  " & TimeString)
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//Patient - " & sName & ", " & sFileNo & "")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//by Visual Basic, VEST Body")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//type - " & sType & "")


    End Function



    Private Function FN_ValidateData() As Short
        'This function is used only to make gross checks
        'for missing data.
        'It does not perform any sensibility checks on the
        'data
        Dim sError As String
        Dim ii, nn As Short

        'Initialise
        FN_ValidateData = False
        sError = ""

        Dim sCircum(11) As Object
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(0). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(0) = "Left shoulder circ."
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(1) = "Right shoulder circ."
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(2) = "Neck circ."
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(3) = "Shoulder width"
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(4). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(4) = "Shoulder to waist"
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(5). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(5) = "Chest circ."
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(6). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(6) = "Waist circ."
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(7). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(7) = "Shoulder to EOS"
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(8). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(8) = "Circ. at EOS"
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(9). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(9) = "Shoulder to under breast"
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(10). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(10) = "Circ. under breast"
        'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(11). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sCircum(11) = "Circ. over nipple"

        'Vest measurements (all must be present)
        For ii = 0 To 6
            If Val(txtCir(ii).Text) = 0 Then
                'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                sError = sError & "Missing dimension for " & sCircum(ii) & "!" & NL
            End If
        Next ii

        'EOS Measurements (if one given both must be given)
        If Val(txtCir(7).Text) = 0 And Val(txtCir(8).Text) <> 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Missing dimension for " & sCircum(7) & "!" & NL
        End If
        If Val(txtCir(8).Text) = 0 And Val(txtCir(8).Text) <> 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Missing dimension for " & sCircum(8) & "!" & NL
        End If

        'Bra Cups
        'Note:
        '    The Circumference over nipple is optional unless a cup has been
        '    specified
        '
        If Val(txtCir(9).Text) = 0 And Val(txtCir(10).Text) <> 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Missing dimension for " & sCircum(9) & "!" & NL
        End If
        If Val(txtCir(10).Text) = 0 And Val(txtCir(9).Text) <> 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Missing dimension for " & sCircum(10) & "!" & NL
        End If

        If (Val(txtCir(9).Text) <> 0 Or Val(txtCir(10).Text) <> 0) And ((cboLeftCup.Text <> "None" And txtLeftDisk.Text = "") Or (txtRightDisk.Text = "" And cboRightCup.Text <> "None")) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Bra Measurements or Bra Cups requested but no disks calculated" & "!" & NL
        End If

        If Val(txtCir(9).Text) = 0 And (txtLeftDisk.Text <> "" Or txtRightDisk.Text <> "") Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object sCircum(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Bra disks given! But missing dimension for " & sCircum(9) & "!" & NL
        End If

        'If cups or dimensions given then a disk must be present
        'NB
        '    cboXXXXCup.ListIndex = 6 = "None"
        '    cboXXXXCup.ListIndex = 7 = ""

        If cboLeftCup.SelectedIndex < 6 And Val(txtLeftDisk.Text) = 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "No disk calculated for Left BRA Cup" & "!" & NL
        End If
        If cboRightCup.SelectedIndex < 6 And Val(txtRightDisk.Text) = 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "No disk calculated for Right BRA Cup" & "!" & NL
        End If

        'Sex error
        If txtSex.Text = "Male" And (Val(txtCir(9).Text) <> 0 Or Val(txtCir(10).Text) <> 0 Or cboLeftCup.SelectedIndex < 0 Or cboRightCup.SelectedIndex < 0) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Male patient but Bra Measurements or Bra Cups requested " & "!" & NL
        End If

        'Neck at back and front
        Dim sChar As New VB6.FixedLengthString(1)
        sChar.Value = VB.Left(cboBackNeck.Text, 1)
        If sChar.Value = "M" And txtBackNeck.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "No dimension for Back Meck Measured Scoop! " & NL
        End If

        sChar.Value = VB.Left(cboFrontNeck.Text, 1)
        If sChar.Value = "M" And txtFrontNeck.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "No dimension for Front Neck Measured Scoop! " & NL
        End If

        '
        If cboLeftAxilla.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Left Axilla not given! " & NL
        End If

        If cboRightAxilla.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Right Axilla not given! " & NL
        End If

        If cboFrontNeck.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Neck not given! " & NL
        End If

        If cboBackNeck.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Back neck not given! " & NL
        End If

        If cboClosure.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Closure not given! " & NL
        End If

        If cboFabric.Text = "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sError = sError & "Fabric not given! " & NL
        End If

        If sError <> "" Then
            MsgBox(sError, 16, "VEST Body - Dialogue")
            FN_ValidateData = False
        Else
            FN_ValidateData = True
        End If

    End Function

    Private Function FN_ValuesString() As String
        'Create a string of all the values given in the
        'Text and Combo boxes.
        '
        'Ignore patient details
        '
        Dim ii As Short
        Dim sString As String

        FN_ValuesString = ""
        sString = ""

        For ii = 0 To 11
            sString = sString & txtCir(ii).Text
        Next ii

        For ii = 0 To 2
            sString = sString & cboRed(ii).Text
        Next ii

        sString = sString & cboLeftCup.Text & txtLeftDisk.Text

        sString = sString & cboRightCup.Text & txtRightDisk.Text

        sString = sString & cboLeftAxilla.Text & cboRightAxilla.Text

        sString = sString & cboFrontNeck.Text & txtFrontNeck.Text

        sString = sString & cboBackNeck.Text & txtBackNeck.Text

        sString = sString & cboClosure.Text

        sString = sString & cboFabric.Text

        FN_ValuesString = sString



    End Function

    'UPGRADE_ISSUE: Form event Form.LinkClose was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ABD9AF39-7E24-4AFF-AD8D-3675C1AA3054"'
    Private Sub Form_LinkClose()
        Dim ii As Short

        'Stop the timer used to ensure that the Dialogue dies
        'if the DRAFIX macro fails to establish a DDE Link
        Timer1.Enabled = False

        'Check that a "MainPatientDetails" Symbol has been
        'found
        'If txtUidMPD.Text = "" Then
        '    MsgBox("No Patient Details have been found in drawing!", 16, "Error, VEST Body - Dialogue")
        '    Return
        'End If

        'Assign combo boxes (set the defaults if empty)
        'Reductions
        If txtCombo(0).Text <> "" Then cboRed(0).Text = txtCombo(0).Text Else cboRed(0).Text = ""
        If txtCombo(1).Text <> "" Then cboRed(1).Text = txtCombo(1).Text Else cboRed(1).Text = ""
        If txtCombo(2).Text <> "" Then cboRed(2).Text = txtCombo(2).Text Else cboRed(2).Text = ""

        'Bra cups and disk
        For ii = 0 To (cboLeftCup.Items.Count - 1)
            If txtCombo(3).Text = VB6.GetItemString(cboLeftCup, ii) Then
                g_sLeftCup = VB6.GetItemString(cboLeftCup, ii)
                cboLeftCup.SelectedIndex = ii
            End If
        Next ii
        For ii = 0 To (cboRightCup.Items.Count - 1)
            If txtCombo(4).Text = VB6.GetItemString(cboRightCup, ii) Then
                g_sRightCup = VB6.GetItemString(cboRightCup, ii)
                cboRightCup.SelectedIndex = ii
            End If
        Next ii

        g_sLeftDisk = txtLeftDisk.Text
        g_sRightDisk = txtRightDisk.Text

        If txtCombo(5).Text <> "" Then cboLeftAxilla.Text = txtCombo(5).Text Else cboLeftAxilla.SelectedIndex = 5 - 2
        If txtCombo(6).Text <> "" Then cboRightAxilla.Text = txtCombo(6).Text Else cboRightAxilla.SelectedIndex = 5 - 2
        If txtCombo(7).Text <> "" Then cboFrontNeck.Text = txtCombo(7).Text Else cboFrontNeck.SelectedIndex = 0
        If txtCombo(8).Text <> "" Then cboBackNeck.Text = txtCombo(8).Text Else cboBackNeck.SelectedIndex = 0
        If txtCombo(9).Text <> "" Then cboClosure.Text = txtCombo(9).Text Else cboClosure.SelectedIndex = 0
        cboFabric.Text = txtCombo(10).Text

        'Set up units
        If txtUnits.Text = "cm" Then
            VESTDIA1.g_nUnitsFac = 10 / 25.4
        Else
            VESTDIA1.g_nUnitsFac = 1
        End If

        'Display dimesions sizes in inches
        For ii = 0 To 11
            txtCir_Leave(txtCir.Item(ii), New System.EventArgs())
        Next ii
        txtFrontNeck_Leave(txtFrontNeck, New System.EventArgs())
        txtBackNeck_Leave(txtBackNeck, New System.EventArgs())

        'Store values used to check for mods to bra cups
        g_nChest = Val(txtCir(5).Text)
        g_nUnderBreast = Val(txtCir(10).Text)
        VESTDIA1.g_nNipple = Val(txtCir(11).Text)

        'Store values to use on change etc
        Dim sChar As New VB6.FixedLengthString(1)
        g_sBackNeck = txtBackNeck.Text
        sChar.Value = VB.Left(cboBackNeck.Text, 1)
        If sChar.Value = "R" Or sChar.Value = "S" Then
            txtBackNeck.Enabled = False
            labBackNeck.Enabled = False
        Else
            txtBackNeck.Enabled = True
            labBackNeck.Enabled = True
        End If

        g_sFrontNeck = txtFrontNeck.Text
        sChar.Value = VB.Left(cboFrontNeck.Text, 1)
        If sChar.Value = "R" Or sChar.Value = "S" Then
            txtFrontNeck.Enabled = False
            labFrontNeck.Enabled = False
        Else
            labFrontNeck.Enabled = True
            txtFrontNeck.Enabled = True
        End If

        'Save the values in the text to a string
        'this can then be used to check if they have changed
        'on use of the close button
        g_sChangeChecker = FN_ValuesString()
        'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default ' Change pointer to hourglass.

        'Disable bras for Males
        If txtSex.Text = "Male" Then
            frmBra.Enabled = False
            For ii = 0 To 11
                LabBra(ii).Enabled = False
            Next ii
            cboLeftCup.Enabled = False
            txtLeftDisk.Enabled = False
            cboRightCup.Enabled = False
            txtRightDisk.Enabled = False
            cboRed(2).Enabled = False
            CalculateBraDisks.Enabled = False
        Else
            PR_EnableCalculateDiskButton()
        End If
        Show()
    End Sub

    Private Sub vestdia_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        Try
            Dim ii As Short

            Hide()

            'Start a timer
            'The Timer is disabled in LinkClose
            'If after 6 seconds the timer event will "End" the programme
            'This ensures that the dialogue dies in event of a failure
            'on the drafix macro side
            Timer1.Interval = 6000 'Approx 6 Seconds
            Timer1.Enabled = True

            'Check if a previous instance is running
            'If it is warn user and exit
            'UPGRADE_ISSUE: App property App.PrevInstance was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="076C26E5-B7A9-4E77-B69C-B4448DF39E58"'
            'If App.PrevInstance Then
            '	MsgBox("VEST Body Dialogue is already running!" & Chr(13) & "Use ALT-TAB and Cancel it.", 16, "Error Starting VEST Body - Dialogue")
            '          Return
            '      End If

            'Maintain while loading DDE data
            'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor ' Change pointer to hourglass.
            'Reset in Form_LinkClose
            'MsgBox("Test", 2, "Vest")
            'Position to center of screen
            Left = VB6.TwipsToPixelsX((VB6.PixelsToTwipsX(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width) - VB6.PixelsToTwipsX(Me.Width)) / 2) ' Center form horizontally.
            Top = VB6.TwipsToPixelsY((VB6.PixelsToTwipsY(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height) - VB6.PixelsToTwipsY(Me.Height)) / 2) ' Center form vertically.

            VESTDIA1.MainForm = Me

            'Initialize globals
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            QQ = Chr(34) 'Double quotes (")
            'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            NL = Chr(13) 'New Line
            'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            CC = Chr(44) 'The comma (,)
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            QCQ = QQ & CC & QQ
            'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            QC = QQ & CC
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            CQ = CC & QQ

            VESTDIA1.g_nUnitsFac = 1
            g_PathJOBST = fnPathJOBST()

            'Clear fields
            'Circumferences and lengths
            For ii = 0 To 11
                txtCir(ii).Text = ""
            Next ii

            'The data from these DDE text boxes is copied
            'to the combo boxes on Link close
            '
            For ii = 0 To 10
                txtCombo(ii).Text = ""
            Next ii

            'Bra cups
            txtLeftDisk.Text = ""
            txtRightDisk.Text = ""

            'Patient details
            txtFileNo.Text = ""
            txtUnits.Text = ""
            txtPatientName.Text = ""
            txtDiagnosis.Text = ""
            txtAge.Text = ""
            txtSex.Text = ""
            txtWorkOrder.Text = ""

            'Design choices
            txtFrontNeck.Text = ""
            txtBackNeck.Text = ""

            'UID of symbols
            txtUidMPD.Text = ""
            txtUidVB.Text = ""

            'Setup combo box fields
            cboRed(0).Items.Add("0.95")
            cboRed(0).Items.Add("0.85")
            cboRed(0).Items.Add("")

            cboRed(1).Items.Add("0.95")
            cboRed(1).Items.Add("0.85")
            cboRed(1).Items.Add("")

            cboRed(2).Items.Add("0.95")
            cboRed(2).Items.Add("0.85")
            cboRed(2).Items.Add("")

            cboLeftCup.Items.Add("Training")
            cboLeftCup.Items.Add("A")
            cboLeftCup.Items.Add("B")
            cboLeftCup.Items.Add("C")
            cboLeftCup.Items.Add("D")
            cboLeftCup.Items.Add("E")
            cboLeftCup.Items.Add("None")
            cboLeftCup.Items.Add("")

            cboRightCup.Items.Add("Training")
            cboRightCup.Items.Add("A")
            cboRightCup.Items.Add("B")
            cboRightCup.Items.Add("C")
            cboRightCup.Items.Add("D")
            cboRightCup.Items.Add("E")
            cboRightCup.Items.Add("None")
            cboRightCup.Items.Add("")

            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cboLeftAxilla.Items.Add("Regular 2" & QQ)
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cboLeftAxilla.Items.Add("Regular 1�" & QQ)
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cboLeftAxilla.Items.Add("Regular 2�" & QQ)
            ''Commented for #211 in the issue list
            ''cboLeftAxilla.Items.Add("Open")
            ''cboLeftAxilla.Items.Add("Mesh")
            cboLeftAxilla.Items.Add("Lining")
            cboLeftAxilla.Items.Add("Sleeveless")
            'cboLeftAxilla.SelectedIndex = 5

            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cboRightAxilla.Items.Add("Regular 2" & QQ)
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cboRightAxilla.Items.Add("Regular 1�" & QQ)
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cboRightAxilla.Items.Add("Regular 2�" & QQ)
            ''Commented for #211 in the issue list
            ''cboRightAxilla.Items.Add("Open")
            ''cboRightAxilla.Items.Add("Mesh")
            cboRightAxilla.Items.Add("Lining")
            cboRightAxilla.Items.Add("Sleeveless")
            'cboRightAxilla.SelectedIndex = 5

            cboFrontNeck.Items.Add("Regular")
            cboFrontNeck.Items.Add("Scoop")
            cboFrontNeck.Items.Add("Measured Scoop")
            cboFrontNeck.Items.Add("Turtle")
            cboFrontNeck.Items.Add("Turtle - Fabric same as Vest")
            cboFrontNeck.Items.Add("Turtle Detachable")
            cboFrontNeck.Items.Add("Turtle Detach. Fabric")

            cboBackNeck.Items.Add("Regular")
            cboBackNeck.Items.Add("Scoop")
            cboBackNeck.Items.Add("Measured Scoop")

            cboClosure.Items.Add("Velcro")
            cboClosure.Items.Add("Zip")
            cboClosure.Items.Add("Front Velcro")
            cboClosure.Items.Add("Front Velcro (Reversed)")
            cboClosure.Items.Add("Back Velcro")
            cboClosure.Items.Add("Back Velcro (Reversed)")
            cboClosure.Items.Add("Front Zip")
            cboClosure.Items.Add("Back Zip")

            If PR_FindBlockExist("bradisk1") = False Then
                frmBra.Enabled = False
                lblShoulderBreast.Enabled = False
                _txtCir_9.Enabled = False
                lblCircBreast.Enabled = False
                _txtCir_10.Enabled = False
                lblCircNipple.Enabled = False
                _txtCir_11.Enabled = False
                _cboRed_2.Enabled = False
                lblLeftBraCup.Enabled = False
                lblLeftCup.Enabled = False
                cboLeftCup.Enabled = False
                lblLeftDisk.Enabled = False
                txtLeftDisk.Enabled = False
                lblRightBraCup.Enabled = False
                lblRightCup.Enabled = False
                cboRightCup.Enabled = False
                lblRightDisk.Enabled = False
                txtRightDisk.Enabled = False
                CalculateBraDisks.Enabled = False
            End If

            ''----------PR_GetComboListFromFile(cboFabric, g_PathJOBST & "\FABRIC.DAT")
            Dim sSettingsPath As String = fnGetSettingsPath("LookupTables")
            PR_GetComboListFromFile(cboFabric, sSettingsPath & "\FABRIC.DAT")

            Dim fileNo As String = "", patient As String = "", diagnosis As String = "", age As String = "", sex As String = ""
            Dim workOrder As String = "", tempDate As String = "", tempEng As String = "", units As String = ""
            Dim blkId As ObjectId = New ObjectId()
            Dim obj As New BlockCreation.BlockCreation
            blkId = obj.LoadBlockInstance()
            If (blkId.IsNull()) Then
                MsgBox("No Patient Details have been found in drawing!", 16, "Error, VEST Body - Dialogue")
                Me.Close()
                Exit Sub
            End If
            obj.BindAttributes(blkId, fileNo, patient, diagnosis, age, sex, workOrder, tempDate, tempEng, units)
            txtPatientName.Text = patient
            txtFileNo.Text = fileNo
            txtDiagnosis.Text = diagnosis
            txtSex.Text = sex
            txtAge.Text = age
            txtUnits.Text = units
            txtWorkOrder.Text = workOrder
            PR_UpdateAge()
            Form_LinkClose()
            readDWGInfo()
            g_sChangeChecker = FN_ValuesString()
            ''----------------------readDWGInfo()
        Catch ex As Exception
            Me.Close()
            VestMain.VestMainDlg.Close()
        End Try
    End Sub

    Private Sub OK_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles OK.Click
        Dim sTask As String
        'Don't allow multiple clicking
        '
        ''OK.Enabled = False
        If FN_ValidateData() Then
            'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor
            Hide()
            VestMain.VestMainDlg.Hide()

            ''PR_CreateMacro_Data("c:\jobst\draw.d")
            Dim sDrawFile As String = fnGetSettingsPath("PathDRAW")
            PR_CreateMacro_Data(sDrawFile & "\draw.d")
            ''PR_CreateMacro_Axilla("c:\jobst\draw_1.d", "c:\jobst\draw_3.d")
            PR_CreateMacro_Axilla(sDrawFile & "\draw_1.d", sDrawFile & "\draw_3.d")
            ''PR_CreateMacro_Bra("c:\jobst\draw_2.d")
            PR_CreateMacro_Bra(sDrawFile & "\draw_2.d")
            PR_GetInsertionPoint()
            PR_CreateMacro_Draw()
            saveInfoToDWG()
            PR_DrawVestDia()
            'sTask = fnGetDrafixWindowTitleText()
            'If sTask <> "" Then
            '    AppActivate(sTask)
            '    System.Windows.Forms.SendKeys.SendWait("@" & g_PathJOBST & "\VEST\BODY.D{enter}")
            '    'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
            '    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default
            '    Return
            'Else
            '    MsgBox("Can't find a Drafix Drawing to update!", 16, "CAD Glove Dialogue")
            'End If
            g_sChangeChecker = FN_ValuesString()
            OK.Enabled = True
            'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default
            VestMain.g_bIsSetArmFabric = True
            VestMain.g_bIsSetTorsoFabric = True
            VestMain.VestMainDlg.Close()
        End If

    End Sub

    Private Sub PR_CreateMacro_Axilla(ByRef sfile As String, ByRef sFile2 As String)
        'Assumes that data has been validated befor this
        'procedure is called.

        g_bMeshAxilla = False

        Dim sRightAxilla, sLeftAxilla, sAxilla As String
        Dim iLoop, ii As Short

        'fNum is a global variable use in subsequent procedures
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FN_Open(sfile, "Axilla Drawing MACRO/S", (txtPatientName.Text), (txtFileNo.Text))

        sLeftAxilla = VB.Left(cboLeftAxilla.Text, 1)
        sRightAxilla = VB.Left(cboRightAxilla.Text, 1)

        If sLeftAxilla <> sRightAxilla Then iLoop = 2 Else iLoop = 1

        For ii = 1 To iLoop
            If ii = 1 Then
                sAxilla = sLeftAxilla
            Else
                sAxilla = sRightAxilla
            End If
            Select Case sAxilla
                Case "R"
                    'Regular Axilla
                    PR_PutLine("@" & g_PathJOBST & "\VEST\BODREGUL.D;")
                Case "O", "L"
                    'Open or Lining Axilla
                    PR_PutLine("@" & g_PathJOBST & "\VEST\BODOTHRS.D;")
                Case "M"
                    'Mesh Axilla
                    g_bMeshAxilla = True
                    PR_PutLine("@" & g_PathJOBST & "\VEST\BODMESH.D;")
                Case "S"
                    'Sleeveless Axilla
                    PR_PutLine("@" & g_PathJOBST & "\VEST\BODSLESS.D;")
            End Select
        Next ii

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileClose(fNum)


        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FN_Open(sFile2, "Draw mesh", (txtPatientName.Text), (txtFileNo.Text))
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If g_bMeshAxilla Then PrintLine(fNum, "Execute (" & QQ & "application" & QC & "sPathJOBST + " & QQ & "\\raglan\\meshvest" & QCQ & "normal" & QQ & " );")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileClose(fNum)

    End Sub

    Private Sub PR_CreateMacro_Bra(ByRef sfile As String)
        'Assumes that data has been validated before this
        'procedure is called.
        Dim nDiskXoff, nBraCLOffset, nDiskYoff As Double
        Dim iLeftDisk, ii, iDisk, iRightDisk As Short

        'fNum is a global variable use in subsequent procedures
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FN_Open(sfile, "Load Bradisks", (txtPatientName.Text), (txtFileNo.Text))

        If cboLeftCup.SelectedIndex < 7 Or cboLeftCup.SelectedIndex < 7 Or txtLeftDisk.Text <> "" Or txtRightDisk.Text <> "" Then
            'A cup or a disk of some sort has been given
            If cboLeftCup.Text = "None" Then iLeftDisk = -1 Else iLeftDisk = Val(txtLeftDisk.Text)
            If cboRightCup.Text = "None" Then iRightDisk = -1 Else iRightDisk = Val(txtRightDisk.Text)

            'Loop through both cups and give positioning information
            For ii = 1 To 2
                If ii = 1 Then iDisk = iLeftDisk Else iDisk = iRightDisk
                Select Case iDisk
                    Case -1, 0
                        nBraCLOffset = 0
                        nDiskXoff = 0
                        nDiskYoff = 0
                    Case 1
                        nBraCLOffset = 1.25
                        nDiskXoff = 1.45
                        nDiskYoff = 1.797
                    Case 2
                        nBraCLOffset = 1.125
                        nDiskXoff = 1.652
                        nDiskYoff = 2.029
                    Case 3
                        nBraCLOffset = 1.0#
                        nDiskXoff = 1.893
                        nDiskYoff = 2.288
                    Case 4
                        nBraCLOffset = 0.875
                        nDiskXoff = 2.175
                        nDiskYoff = 2.572
                    Case 5
                        nBraCLOffset = 0.75
                        nDiskXoff = 2.293
                        nDiskYoff = 2.882
                    Case 6
                        nBraCLOffset = 0.625
                        nDiskXoff = 2.625
                        nDiskYoff = 3.025
                    Case 7
                        nBraCLOffset = 0.5
                        nDiskXoff = 2.94
                        nDiskYoff = 3.335
                    Case 8
                        nBraCLOffset = 0.5
                        nDiskXoff = 3.139
                        nDiskYoff = 3.518
                    Case 9
                        nBraCLOffset = 0.5
                        nDiskXoff = 3.393
                        nDiskYoff = 3.938
                End Select

                If ii = 1 Then
                    'Left Disks
                    PR_PutNumberAssign("nDiskLt", iLeftDisk)
                    PR_PutNumberAssign("nBraCLOffsetLt", nBraCLOffset)
                    PR_PutNumberAssign("nDiskXoffLt", nDiskXoff)
                    PR_PutNumberAssign("nDiskYoffLt", nDiskYoff)
                Else
                    'Right Disks
                    PR_PutNumberAssign("nDiskRt", iRightDisk)
                    PR_PutNumberAssign("nBraCLOffsetRt", nBraCLOffset)
                    PR_PutNumberAssign("nDiskXoffRt", nDiskXoff)
                    PR_PutNumberAssign("nDiskYoffRt", nDiskYoff)

                End If
            Next ii

            'Insert disks and text using the Assignments from above
            PR_PutLine("@" & g_PathJOBST & "\VEST\BODYBRA.D;")

        Else
            'No bra cups are required
            PR_PutLine("// --------------------")
            PR_PutLine("// NO BRA CUPS REQUIRED")
            PR_PutLine("// --------------------")
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileClose(fNum)

    End Sub

    Private Sub PR_CreateMacro_Data(ByRef sfile As String)

        'fNum is a global variable use in subsequent procedures
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FN_Open(sfile, "Save Data and Set Variables for BODY.D", (txtPatientName.Text), (txtFileNo.Text))
        'If this is a new drawing of a vest then Define the DATA Base
        'fields for the VEST Body and insert the BODYBOX symbol
        PR_PutLine("HANDLE hMPD, hBody;")
        PR_UpdateDB()


        'Variable data
        'If display is in inches then convert to decimal inches
        'The conversion of CMs to Inches is done in BODY.D
        'in this way we do not add any extra problems due to rounding errors
        '
        'Patient details
        PR_PutStringAssign("sFileNo", (txtFileNo.Text))
        PR_PutStringAssign("sPatient", (txtPatientName.Text))
        PR_PutStringAssign("sAge", (txtAge.Text))
        PR_PutStringAssign("sSEX", (txtSex.Text))
        PR_PutStringAssign("sDiagnosis", (txtDiagnosis.Text))
        PR_PutNumberAssign("nUnitsFac", VESTDIA1.g_nUnitsFac)
        PR_PutNumberAssign("nAge", Val(txtAge.Text))
        'PR_PutStringAssign "sUnitType", txtUnits.Text

        'VEST Body Details
        PR_PutNumberAssign("nLtSCir", FN_Decimalise(Val(txtCir(0).Text)))
        PR_PutNumberAssign("nRtSCir", FN_Decimalise(Val(txtCir(1).Text)))
        PR_PutNumberAssign("nNeckCir", FN_Decimalise(Val(txtCir(2).Text)))
        PR_PutNumberAssign("nSWidth", FN_Decimalise(Val(txtCir(3).Text)))
        PR_PutNumberAssign("nS_Waist", FN_Decimalise(Val(txtCir(4).Text)))
        PR_PutNumberAssign("nChestCir", FN_Decimalise(Val(txtCir(5).Text)))
        PR_PutNumberAssign("nChestCirActual", FN_Decimalise(Val(txtCir(5).Text)))
        PR_PutNumberAssign("nWaistCir", FN_Decimalise(Val(txtCir(6).Text)))
        PR_PutNumberAssign("nS_EOS", FN_Decimalise(Val(txtCir(7).Text)))
        PR_PutNumberAssign("nEOSCir", FN_Decimalise(Val(txtCir(8).Text)))
        PR_PutNumberAssign("nS_Breast", FN_Decimalise(Val(txtCir(9).Text)))
        PR_PutNumberAssign("nBreastCir", FN_Decimalise(Val(txtCir(10).Text)))
        PR_PutNumberAssign("nBreastCirActual", FN_Decimalise(Val(txtCir(10).Text)))
        PR_PutNumberAssign("nNippleCir", FN_Decimalise(Val(txtCir(11).Text)))

        PR_PutStringAssign("sBraLtCup", (cboLeftCup.Text))
        PR_PutStringAssign("sBraRtCup", (cboRightCup.Text))
        PR_PutStringAssign("sBraLtDisk", (txtLeftDisk.Text))
        PR_PutStringAssign("sBraRtDisk", (txtRightDisk.Text))

        PR_PutStringAssign("sLtAxillaType", FN_EscapeQuotesInString(cboLeftAxilla.Text))
        PR_PutStringAssign("sRtAxillaType", FN_EscapeQuotesInString(cboRightAxilla.Text))

        PR_PutStringAssign("sNeckType", (cboFrontNeck.Text))
        PR_PutNumberAssign("nNeckDimension", FN_Decimalise(Val(txtFrontNeck.Text)))
        PR_PutStringAssign("sBackNeckType", (cboBackNeck.Text))
        PR_PutNumberAssign("nBackNeckDim", FN_Decimalise(Val(txtBackNeck.Text)))

        PR_PutStringAssign("sClosure", (cboClosure.Text))
        PR_PutStringAssign("sFabric", (cboFabric.Text))

        PR_PutNumberAssign("nWaistCirUserFac", Val(cboRed(0).Text))
        PR_PutNumberAssign("nEOSCirUserFac", Val(cboRed(1).Text))
        PR_PutNumberAssign("nBreastCirUserFac", Val(cboRed(0).Text))

        If txtWorkOrder.Text = "" Then
            PR_PutStringAssign("sWorkOrder", "-")
        Else
            PR_PutStringAssign("sWorkOrder", (txtWorkOrder.Text))
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileClose(fNum)

    End Sub

    Private Sub PR_CreateMacro_Save(ByRef sDrafixFile As String)
        'fNum is a global variable use in subsequent procedures
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FN_Open(sDrafixFile, "Save Data ONLY", (txtPatientName.Text), (txtFileNo.Text))

        'If this is a new drawing of a vest then Define the DATA Base
        'fields for the VEST Body and insert the BODYBOX symbol
        PR_PutLine("HANDLE hMPD, hBody;")

        PR_UpdateDB()

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileClose(fNum)

    End Sub


    Private Sub PR_DoBraCupsAndDisks()
        'Procedure to either
        '    1. Check if there is enough data to enable a
        '       calculation.
        ' or
        '    2. Check if a change has been made to the data
        '       requiring a recalculation.
        '
        'Then calculate the bra cups and disks.
        '
        'NOTE
        '    txtCir(5) == Chest Circumference
        '    txtCir(10) == Under Breast Circumference
        '    txtCir(11) == Circumference over nipple line
        '
        'GLOBALS
        '    g_nUnitsFac As Double
        '    g_nUnderBreast As Double
        '    g_nChest As Double
        '    g_nNipple As Double
        '    g_sLeftCup As String
        '    g_sRightCup As String
        '    g_sLeftDisk As String
        '    g_sRightDisk As String

        Dim sLeftCup, sRightCup As String
        Dim sSide As Short
        Dim ii, iError As Short
        Dim nUnderBreast, nNipple, nChest As Double
        Dim sError As String
        Dim iLeftDisk, iRightDisk As Short

        sLeftCup = cboLeftCup.Text
        sRightCup = cboRightCup.Text

        'Check if enough data to caclulate
        'Exit sub if not
        If Val(txtCir(5).Text) = 0 Then
            'No chest cir.
            Exit Sub
        ElseIf Val(txtCir(10).Text) = 0 Then
            'No under breast cir.
            Exit Sub
        ElseIf Val(txtCir(11).Text) = 0 And ((sLeftCup = "" Or sLeftCup = "None") And (sRightCup = "" Or sRightCup = "None")) Then
            'No over nipple circumference and no bra cups given for either side
            Exit Sub
        End If

        'Check if changed
        Dim Changed As Short

        Changed = False

        If sLeftCup <> g_sLeftCup Then
            Changed = True
            g_sLeftCup = sLeftCup
        End If

        If sRightCup <> g_sRightCup Then
            Changed = True
            g_sRightCup = sRightCup
        End If

        If Val(txtCir(10).Text) <> g_nUnderBreast Then
            Changed = True
            g_nUnderBreast = Val(txtCir(10).Text)
        End If

        If Val(txtCir(11).Text) <> VESTDIA1.g_nNipple Then
            Changed = True
            VESTDIA1.g_nNipple = Val(txtCir(11).Text)
            'Force a recalculation of cup
            If sLeftCup <> "None" Then sLeftCup = ""
            If sRightCup <> "None" Then sRightCup = ""
        End If

        If Val(txtCir(5).Text) <> g_nChest Then
            g_nChest = Val(txtCir(5).Text)
            'Force a recalculation of cup if a nipple circum. is given
            'Changed chest is only significant if a nipple line measurement
            'is given from which the Cup size is calculated
            If VESTDIA1.g_nNipple <> 0 Then
                Changed = True
                If sLeftCup <> "None" Then sLeftCup = ""
                If sRightCup <> "None" Then sRightCup = ""
            End If
        End If

        'Force a calculation if either disk is empty
        If txtLeftDisk.Text = "" Or txtRightDisk.Text = "" Then Changed = True

        'Force a check if disk has been changed
        If txtLeftDisk.Text <> g_sLeftDisk Or txtRightDisk.Text <> g_sRightDisk Then
            Changed = True
            iRightDisk = Val(txtRightDisk.Text)
            iLeftDisk = Val(txtLeftDisk.Text)
        End If

        'If no modifications then exit
        If Changed <> True Then Exit Sub


        'Recalculate if Changed
        'Convert to inches
        nNipple = ARMEDDIA1.fnDisplayToInches(VESTDIA1.g_nNipple)
        nUnderBreast = ARMEDDIA1.fnDisplayToInches(g_nUnderBreast)
        nChest = ARMEDDIA1.fnDisplayToInches(g_nChest)

        'Right cup
        If sRightCup <> "" Or nNipple <> 0 Then
            iError = FN_CalculateBra(nChest, nUnderBreast, nNipple, sRightCup, iRightDisk, "Right")
            If iError Then
                For ii = 0 To (cboRightCup.Items.Count - 1)
                    If VB6.GetItemString(cboRightCup, ii) = sRightCup Then cboRightCup.SelectedIndex = ii
                Next ii
                If iRightDisk = 0 Then txtRightDisk.Text = "" Else txtRightDisk.Text = CStr(iRightDisk)
                g_sRightDisk = Trim(Str(iRightDisk))
                g_sRightCup = sRightCup
            Else
                txtRightDisk.Text = ""
                cboRightCup.SelectedIndex = -1
                g_sRightDisk = ""
                g_sRightCup = ""
            End If
        End If

        If sLeftCup <> "" Or nNipple <> 0 Then
            iError = FN_CalculateBra(nChest, nUnderBreast, nNipple, sLeftCup, iLeftDisk, "Left")
            If iError Then
                For ii = 0 To (cboLeftCup.Items.Count - 1)
                    If VB6.GetItemString(cboLeftCup, ii) = sLeftCup Then cboLeftCup.SelectedIndex = ii
                Next ii
                If iLeftDisk <= 0 Then txtLeftDisk.Text = "" Else txtLeftDisk.Text = CStr(iLeftDisk)
                g_sLeftDisk = Trim(Str(iLeftDisk))
                g_sLeftCup = sLeftCup
            Else
                txtLeftDisk.Text = ""
                cboLeftCup.SelectedIndex = -1
                g_sLeftDisk = ""
                g_sLeftCup = ""
            End If
        End If

        PR_EnableCalculateDiskButton()


    End Sub

    Private Sub PR_EnableCalculateDiskButton()
        'Procedure to check if there is enough data to
        'enable the caclculate disks command button
        '
        Dim sLeftCup, sRightCup As String

        sLeftCup = cboLeftCup.Text
        sRightCup = cboRightCup.Text

        'Check if enough data to caclulate
        'Exit sub if not
        If Val(txtCir(5).Text) = 0 Then
            'No chest cir.
            CalculateBraDisks.Enabled = False
            Exit Sub
        ElseIf Val(txtCir(10).Text) = 0 Then
            'No under breast cir.
            CalculateBraDisks.Enabled = False
            Exit Sub
        ElseIf Val(txtCir(11).Text) = 0 And (sLeftCup = "" Or sLeftCup = "None") And (sRightCup = "" Or sRightCup = "None") Then
            'No over nipple circumference and no bra cups given for either side
            CalculateBraDisks.Enabled = False
            Exit Sub
        End If

        CalculateBraDisks.Enabled = True

    End Sub

    Private Sub PR_GetComboListFromFile(ByRef Combo_Name As System.Windows.Forms.ComboBox, ByRef sFileName As String)
        'General procedure to create the list section of
        'a combo box reading the data from a file

        Dim sLine As String
        Dim fFileNum As Short

        fFileNum = FreeFile()

        If FileLen(sFileName) = 0 Then
            MsgBox(sFileName & "Not found", 48, "CAD - Vest Dialogue")
            Exit Sub
        End If

        FileOpen(fFileNum, sFileName, VB.OpenMode.Input)
        Do While Not EOF(fFileNum)
            sLine = LineInput(fFileNum)
            'UPGRADE_WARNING: Couldn't resolve default property of object Combo_Name.AddItem. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'Combo_Name.AddItem(sLine)
            Combo_Name.Items.Add(sLine)
        Loop
        FileClose(fFileNum)

    End Sub

    Private Sub PR_PutLine(ByRef sLine As String)
        'Puts the contents of sLine to the opened "Macro" file
        'Puts the line with no translation or additions
        '    fNum is global variable

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, sLine)

    End Sub

    Private Sub PR_PutNumberAssign(ByRef sVariableName As String, ByRef nAssignedNumber As Object)
        'Procedure to put a number assignment
        'Adds a semi-colon
        '    fNum is global variable

        'UPGRADE_WARNING: Couldn't resolve default property of object nAssignedNumber. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, sVariableName & "=" & Str(nAssignedNumber) & ";")

    End Sub

    Private Sub PR_PutStringAssign(ByRef sVariableName As String, ByRef sAssignedString As Object)
        'Procedure to put a string assignment
        'Encloses String in quotes and adds a semi-colon
        '    fNum is global variable

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, sVariableName & "=" & QQ & sAssignedString & QQ & ";")

    End Sub

    Private Sub PR_SetLayer(ByRef sNewLayer As String)
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

    Private Sub PR_UpdateDB()
        'Procedure called from
        '    PR_CreateMacro_Save
        'and
        '    PR_CreateMacro_Data
        '
        'Used to stop duplication on code

        Dim sSymbol As String

        sSymbol = "vestbody"

        If txtUidVB.Text = "" Then
            'Define DB Fields
            PR_PutLine("@" & g_PathJOBST & "\VEST\VFIELDS.D;")

            'Find "mainpatientdetails" and get position
            PR_PutLine("XY     xyMPD_Origin, xyMPD_Scale ;")
            PR_PutLine("STRING sMPD_Name;")
            PR_PutLine("ANGLE  aMPD_Angle;")


            'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("hMPD = UID (" & QQ & "find" & QC & Val(txtUidMPD.Text) & ");")
            PR_PutLine("if (hMPD)")
            PR_PutLine("  GetGeometry(hMPD, &sMPD_Name, &xyMPD_Origin, &xyMPD_Scale, &aMPD_Angle);")
            PR_PutLine("else")
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("  Exit(%cancel," & QQ & "Can't find > mainpatientdetails < symbol, Insert Patient Data" & QQ & ");")

            'Insert bodybox
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("if ( Symbol(" & QQ & "find" & QCQ & sSymbol & QQ & ")){")
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("  Execute (" & QQ & "menu" & QCQ & "SetLayer" & QC & "Table(" & QQ & "find" & QCQ & "layer" & QCQ & "Data" & QQ & "));")
            'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("  hBody = AddEntity(" & QQ & "symbol" & QCQ & sSymbol & QC & "xyMPD_Origin);")
            PR_PutLine("  }")
            PR_PutLine("else")
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("  Exit(%cancel, " & QQ & "Can't find >" & sSymbol & "< symbol to insert\nCheck your installation, that JOBST.SLB exists!" & QQ & ");")
        Else
            'Use existing symbol
            'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("hBody = UID (" & QQ & "find" & QC & Val(txtUidVB.Text) & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PR_PutLine("if (!hBody) Exit(%cancel," & QQ & "Can't find >" & sSymbol & "< symbol to update!" & QQ & ");")

        End If

        'Update the BODY Box symbol
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "LtSCir" & QCQ & txtCir(0).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "RtSCir" & QCQ & txtCir(1).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "NeckCir" & QCQ & txtCir(2).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "SWidth" & QCQ & txtCir(3).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "S_Waist" & QCQ & txtCir(4).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "ChestCir" & QCQ & txtCir(5).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "WaistCir" & QCQ & txtCir(6).Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "S_EOS" & QCQ & txtCir(7).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "EOSCir" & QCQ & txtCir(8).Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "S_Breast" & QCQ & txtCir(9).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BreastCir" & QCQ & txtCir(10).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "NippleCir" & QCQ & txtCir(11).Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BraLtCup" & QCQ & cboLeftCup.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BraRtCup" & QCQ & cboRightCup.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BraLtDisk" & QCQ & txtLeftDisk.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BraRtDisk" & QCQ & txtRightDisk.Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "LtAxillaType" & QCQ & FN_EscapeQuotesInString(cboLeftAxilla.Text) & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "RtAxillaType" & QCQ & FN_EscapeQuotesInString(cboRightAxilla.Text) & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "NeckType" & QCQ & cboFrontNeck.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "NeckDimension" & QCQ & txtFrontNeck.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BackNeckType" & QCQ & cboBackNeck.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BackNeckDim" & QCQ & txtBackNeck.Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "Closure" & QCQ & cboClosure.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "Fabric" & QCQ & cboFabric.Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "WaistCirUserFac" & QCQ & cboRed(0).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "EOSCirUserFac" & QCQ & cboRed(1).Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "BreastCirUserFac" & QCQ & cboRed(2).Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_PutLine("SetDBData( hBody" & CQ & "fileno" & QCQ & txtFileNo.Text & QQ & ");")

    End Sub

    Private Sub Timer1_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Timer1.Tick
        'It is assumed that the link open from Drafix has failed
        'Therefor we "End" here
        Return
    End Sub

    Private Sub txtBackNeck_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtBackNeck.Enter
        VESTDIA1.PR_Select_Text(txtBackNeck)
    End Sub

    Private Sub txtBackNeck_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtBackNeck.Leave
        Dim nLen As Double
        nLen = VESTDIA1.FN_InchesValue(txtBackNeck)
        If nLen >= 0 Then
            lblBackNeck.Text = ARMEDDIA1.fnInchesToText(nLen)
            g_sBackNeck = txtBackNeck.Text
        Else
            lblBackNeck.Text = ""
        End If
    End Sub

    Private Sub txtCir_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtCir.Enter
        Dim Index As Short = txtCir.GetIndex(eventSender)
        VESTDIA1.PR_Select_Text(txtCir(Index))
    End Sub

    Private Sub txtCir_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtCir.Leave
        Dim Index As Short = txtCir.GetIndex(eventSender)
        Dim nLen As Double
        nLen = VESTDIA1.FN_InchesValue(txtCir(Index))
        If nLen > 0 Then
            lblCir(Index).Text = ARMEDDIA1.fnInchesToText(nLen)
        Else
            lblCir(Index).Text = ""
        End If

        If Index > 10 Or Index = 5 Then PR_EnableCalculateDiskButton()

    End Sub

    Private Sub txtFrontNeck_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtFrontNeck.Enter
        VESTDIA1.PR_Select_Text(txtFrontNeck)
    End Sub

    Private Sub txtFrontNeck_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtFrontNeck.Leave
        Dim nLen As Double
        nLen = VESTDIA1.FN_InchesValue(txtFrontNeck)
        If nLen >= 0 Then
            lblFrontNeck.Text = ARMEDDIA1.fnInchesToText(nLen)
            g_sFrontNeck = txtFrontNeck.Text 'Keep this for restore
        Else
            lblFrontNeck.Text = ""
        End If
    End Sub

    Private Sub txtLeftDisk_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLeftDisk.Enter
        VESTDIA1.PR_Select_Text(txtLeftDisk)
    End Sub

    Private Sub txtLeftDisk_KeyPress(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.KeyPressEventArgs) Handles txtLeftDisk.KeyPress
        Dim KeyAscii As Short = Asc(eventArgs.KeyChar)
        'Only Disks sizes 1 to 9 allowed
        Select Case KeyAscii
            Case 32
                KeyAscii = 0
            Case 8, 9, 10, 13, 49 To 57
                'Do nothing
            Case Else
                KeyAscii = 0
                MsgBox("Bra disk sizes 1 to 9 only.", 48, "VEST Body - Dialogue")
        End Select

        eventArgs.KeyChar = Chr(KeyAscii)
        If KeyAscii = 0 Then
            eventArgs.Handled = True
        End If
    End Sub

    Private Sub txtRightDisk_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtRightDisk.Enter
        VESTDIA1.PR_Select_Text(txtRightDisk)
    End Sub

    Private Sub txtRightDisk_KeyPress(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.KeyPressEventArgs) Handles txtRightDisk.KeyPress
        Dim KeyAscii As Short = Asc(eventArgs.KeyChar)
        'Only Disks sizes 1 to 9 allowed
        Select Case KeyAscii
            Case 32
                KeyAscii = 0
            Case 8, 9, 10, 13, 49 To 57
                'Do nothing
            Case Else
                KeyAscii = 0
                MsgBox("Bra disk sizes 1 to 9 only.", 48, "VEST Body - Dialogue")
        End Select
        eventArgs.KeyChar = Chr(KeyAscii)
        If KeyAscii = 0 Then
            eventArgs.Handled = True
        End If
    End Sub
    Private Function FN_Decimalise(ByRef nDisplay As Double) As Double
        'This function takes the value given and converts it to its
        'decimal equivelent.
        '
        '
        'Input:-
        '        nDisplay is the value as input by the operator in the
        '        dialog.
        '        The convention is that, Metric dimensions use the decimal
        '        point to indicate the division between CMs and mms
        '        ie 7.6 = 7 cm and 6 mm.
        '        Whereas the decimal point for imperial measurements indicates
        '        the division between inches and eighths
        '        ie 7.6 = 7 inches and 6 eighths
        '
        'Globals:-
        '        g_nUnitsFac = 1       => nDisplay in Inches
        '        g_nUnitsFac = 10/25.5 => nDisplay in CMs
        '
        'Returns:-
        '        Single,
        '        For CMs value is returned unaltered.
        '        For Inches value is returned in decimal inches
        '        -1,     on conversion error.
        '
        'Errors:-
        '        The returned value is usually +ve. UBODYSUIT1.NLess it can't
        '        be sucessfully converted to inches.
        '        Eg 7.8 is an invalid number if g_nUnitsFac = 1
        '
        '                            WARNING
        '                            ~~~~~~~
        'In most cases the input is a +ve number.  This function will handle a
        '-ve number but in this case the error checking is invalid.  This
        'is done to provide a general conversion tool.  Where the input is
        'likley to be -ve then the calling subroutine or function should check
        'the sensibility of the returned value for that specific case.
        '

        Dim iInt, iSign As Short
        Dim nDec As Double
        'retain sign
        iSign = System.Math.Sign(nDisplay)
        nDisplay = System.Math.Abs(nDisplay)

        'Simple case where Units are CM
        If VESTDIA1.g_nUnitsFac <> 1 Then
            FN_Decimalise = nDisplay * iSign
            Exit Function
        End If

        'Imperial units
        iInt = Int(nDisplay)
        nDec = nDisplay - iInt
        'Check that conversion is possible (return -1 if not)
        If nDec > 0.8 Then
            FN_Decimalise = -1
        Else
            FN_Decimalise = (iInt + (nDec * 0.125 * 10)) * iSign
        End If

    End Function
    Private Sub PR_CreateMacro_Draw()
        Dim sSettingsPath As String = fnGetSettingsPath("LookupTables")
        Dim nEOSCir As Double
        Dim nEOSCirFac As Double = 0.9
        Dim nEOSCirUserFac As Double = Val(_cboRed_1.Text)
        If Val(txtCir(8).Text) > 0 Then
            If nEOSCirUserFac > 0 Then
                nEOSCirFac = nEOSCirUserFac
            End If
            ''---------------nEOSCir = VESTDIA1.fnDisplayToInches(Val(txtCir(8).Text)) * nEOSCirFac / 4
            nEOSCir = FN_Round(Val(txtCir(8).Text) * VESTDIA1.g_nUnitsFac * nEOSCirFac / 4)
        End If

        Dim nWaistCirFac As Double = 0.9
        Dim nWaistCir As Double
        If Val(_cboRed_0.Text) > 0 Then
            nWaistCirFac = Val(_cboRed_0.Text)
        End If
        ''---------------nWaistCir = VESTDIA1.fnDisplayToInches(Val(txtCir(6).Text)) * nWaistCirFac / 4
        nWaistCir = FN_Round(Val(txtCir(6).Text) * VESTDIA1.g_nUnitsFac * nWaistCirFac / 4)

        Dim BRAGiven As Boolean = False
        Dim nBreastCirFac, nBreastCir, nBreastCirActual, nS_Breast As Double
        If Val(txtCir(10).Text) > 0 Then
            BRAGiven = True
            If Val(_cboRed_0.Text) > 0 Then
                nBreastCirFac = Val(_cboRed_0.Text)
            End If
            ''---------------------nBreastCir = VESTDIA1.fnDisplayToInches(nBreastCir) * nBreastCirFac / 4
            '-=----------------------------nBreastCirActual = VESTDIA1.fnDisplayToInches(nBreastCirActual)
            nBreastCir = FN_Round(nBreastCir * VESTDIA1.g_nUnitsFac * nBreastCirFac / 4)
            ''---------------nBreastCirActual = FN_Round(nBreastCirActual * VESTDIA1.g_nUnitsFac)
        End If

        If Val(txtCir(9).Text) > 0 Then
            ''-----------nS_Breast = VESTDIA1.fnDisplayToInches(Val(txtCir(9).Text))
            nS_Breast = FN_Round(Val(txtCir(9).Text) * VESTDIA1.g_nUnitsFac)
        End If

        Dim nNippleCir As Double = Val(txtCir(11).Text)
        If nNippleCir > 0 Then
            ''------------nNippleCir = VESTDIA1.fnDisplayToInches(nNippleCir)
            nNippleCir = FN_Round(nNippleCir * VESTDIA1.g_nUnitsFac)
        End If

        Dim nChestCir, nChestCirActual As Double
        nChestCir = Val(txtCir(5).Text)
        Dim nChestCirFac As Double = 0.9

        ''-------------nChestCirActual = VESTDIA1.fnDisplayToInches(nChestCir)
        ''-------nChestCir = VESTDIA1.fnDisplayToInches(nChestCir) * nChestCirFac / 4
        nChestCirActual = FN_Round(nChestCir * VESTDIA1.g_nUnitsFac)
        nChestCir = FN_Round(nChestCir * VESTDIA1.g_nUnitsFac * nChestCirFac / 4)

        '' Shoulder width Is relevent for Sleeveless only
        Dim nSWidth As Double = Val(txtCir(3).Text)

        ''--------------nSWidth = VESTDIA1.fnDisplayToInches(nSWidth)
        nSWidth = FN_Round(nSWidth * VESTDIA1.g_nUnitsFac)
        Dim nNeckCir As Double = Val(txtCir(2).Text)
        Dim nNeckGiven As Double = nNeckCir * VESTDIA1.g_nUnitsFac  '' Retain for later use
        Dim nNeckFac_1 As Double = 0.9
        Dim nNeckFac_2 As Double = 6
        Dim nNeckFac_3 As Double = 0.125
        Dim nNeckFac_4 As Double = 1.3
        Dim nNeckFac_5 As Double = 1.3
        Dim nRegAxillaFac_1, nRegAxillaFac_2, aPrevAngle, nX, nY, nLength, aAngle As Double
        Dim nNeckActualCir, nLowSLine, nHighSLine As Double
        nRegAxillaFac_2 = 0.75
        Dim SleeveLess As Boolean = False
        Dim xyAxillaLow, xyPt1, xyRaglanAxilla, xyRaglanNeck, xyAxillaConstruct_2, xyInsertConstruct_3, xyInsertConstruct_4 As xy
        Dim xyBackNeckCL, xyBackNeckCen, xyAxilla, xyFrontNeckOFF, xyFrontNeckCL, xyFrontNeckCen As xy
        Dim xyWaistOFF, xyEOSOFF, xyBreast, xyHighestAxilla, xyLowestAxilla, xyO, xyTextIns, xyTempSt, xyTempEnd As xy
        Dim xySleeveLess As xy
        Dim EOSGiven As Boolean
        Dim nBraAxillaHt As Double = 0
        Dim sHighestAxilla As String = ""
        ''-----------nNeckCir = VESTDIA1.fnDisplayToInches(((nNeckCir * VESTDIA1.g_nUnitsFac * nNeckFac_1) / nNeckFac_2) - nNeckFac_3)
        nNeckCir = FN_Round(((nNeckCir * VESTDIA1.g_nUnitsFac * nNeckFac_1) / nNeckFac_2) - nNeckFac_3)
        Dim sClosure As String = cboClosure.Text
        Dim nRadius, nCount As Double
        Dim bPrevAxillaWasMesh As Boolean

        '' Regular Neck
        Dim sNeckType As String = cboFrontNeck.Text
        Dim sNeckNotes As String = ""
        If sNeckType.Equals("Regular") Then
            ''-----------------nNeckGiven = VESTDIA1.fnDisplayToInches(nNeckGiven * nNeckFac_1)
            sNeckNotes = VESTDIA1.fnInchestoText(VESTDIA1.fnRoundInches(nNeckGiven * nNeckFac_1))
            nNeckGiven = FN_Round(nNeckGiven * nNeckFac_1)
            ''-----sNeckNotes = "Regular Neck " + Format("length", nNeckGiven)
            sNeckNotes = "Regular Neck " + sNeckNotes + Chr(34)
        End If
        Dim nNeckCirRetained As Double
        '' Scoop neck & Measured Scoop neck
        If sNeckType.Equals("Scoop") Or sNeckType.Equals("Measured Scoop") Then
            nNeckCirRetained = nNeckCir     '' Keep old value of neck to get radius
            '' Recalculate to eliminate any rounding error carry through
            '' Use NeckGiven
            ''---------nNeckCir = VESTDIA1.fnDisplayToInches((((nNeckGiven * nNeckFac_1) / nNeckFac_2) - nNeckFac_3) * nNeckFac_4)
            nNeckCir = FN_Round((((nNeckGiven * nNeckFac_1) / nNeckFac_2) - nNeckFac_3) * nNeckFac_4)
            sNeckNotes = sNeckType + " Neck"
        End If

        '' Turtle Necks Both attached And detachable
        '' NB. Size extraction Is VERY format dependant,
        Dim nTurtleLength, nTurtleWidth, nXInsert, nYInsert As Double
        Dim nNeckDimension As Double = Val(txtFrontNeck.Text)
        Dim strNeckVal, strTurtleVal As String
        If sNeckType.Contains("Turtle") Then
            nTurtleLength = 0
            ''----------nNeckGiven = VESTDIA1.fnDisplayToInches(nNeckGiven * nNeckFac_1)
            ''---------nTurtleWidth = VESTDIA1.fnDisplayToInches(nNeckDimension)
            strNeckVal = VESTDIA1.fnInchestoText(VESTDIA1.fnRoundInches(nNeckGiven * nNeckFac_1))
            strTurtleVal = VESTDIA1.fnInchestoText(VESTDIA1.fnRoundInches(nNeckDimension * VESTDIA1.g_nUnitsFac))
            nNeckGiven = FN_Round(nNeckGiven * nNeckFac_1)
            nTurtleWidth = FN_Round(nNeckDimension * VESTDIA1.g_nUnitsFac)
            If nTurtleWidth = 0 Then
                If Val(txtAge.Text) < 10 Then
                    nTurtleWidth = 1    '' Children under 10 yrs old
                Else
                    nTurtleWidth = 2   '' Adults Or Children over 10 yrs old
                End If
                nTurtleWidth = nTurtleWidth * VESTDIA1.g_nUnitsFac
                strTurtleVal = VESTDIA1.fnInchestoText(VESTDIA1.fnRoundInches(nTurtleWidth))
                '----------------sNeckDimension = MakeString("scalar", nTurtleWidth)
                ''---------------SetDBData(hBody, "NeckDimension", sNeckDimension)  '' update bodybox
            End If

            If sNeckType.Contains("Turtle Detach") Then '/* ie  Detachable*/
                nTurtleLength = nNeckGiven
            End If
            sNeckNotes = sNeckType + Chr(10) + strNeckVal + Chr(34) + " x " + strTurtleVal
        End If


        '' Closures
        '' Check for bracups And that selected closure Is compatible
        '' If front Or back Not given explicity then apply as per defaults in manual
        '' NB Format of sClosure Is vital

        If BRAGiven Then
            If sClosure.Equals("Front Velcro") Or sClosure.Equals("Front Velcro (Reversed)") Then '/* Front Velcro */
                sClosure = "Front Zip"
            End If

            If sClosure.Length = 5 Or sClosure.Length = 3 Then  '/* Front Or Zip */
                sClosure = "Front Zip"
            End If
            If sClosure.Length = 6 Then '/* Velcro */
                sClosure = "Back Velcro"
            End If

        Else '/* No Bra Cups */

            If sClosure.Length = 4 Or sClosure.Length = 6 Then '/* Back Or Velcro */
                sClosure = "Back Velcro"
            End If
            If sClosure.Length = 3 Then '/* Zip */
                sClosure = "Back Zip"
            End If
        End If

        Dim nS_EOS As Double
        Dim nHighSLineFac As Double = 0.5
        If Val(txtCir(7).Text) > 0 Then
            EOSGiven = True
            ''-----------nS_EOS = VESTDIA1.fnDisplayToInches(Val(txtCir(7).Text))
            nS_EOS = FN_Round(Val(txtCir(7).Text) * VESTDIA1.g_nUnitsFac)
            nLowSLine = nS_EOS
        Else
            EOSGiven = False
            ''--------nLowSLine = VESTDIA1.fnDisplayToInches(Val(txtCir(4).Text))
            nLowSLine = FN_Round(Val(txtCir(4).Text) * VESTDIA1.g_nUnitsFac)
        End If
        nHighSLine = nLowSLine + nHighSLineFac

        '' Main Program
        '' Using the figured data a number of Keypoints are established which can then be used
        '' to create a polyline to represent the body template.
        '' If Required left And right axilla are detailed seperatly

        '' End of Support (If given) And Waist points.
        Dim xyEOSCL, xyWaistCL As xy
        Dim nSeamAllowance As Double = 0.125
        If EOSGiven Then
            xyEOSCL.X = xyO.X
            xyEOSCL.Y = xyO.Y
            xyEOSOFF.X = xyO.X
            xyEOSOFF.Y = nEOSCir + nSeamAllowance + xyO.Y
            ''	xyWaistOFF.x = nLowSLine + nSeamAllowance - FN_Round((nS_Waist  * VESTDIA1.g_nUnitsFac)) + xyO.x;
            ''------------xyWaistOFF.X = nLowSLine - VESTDIA1.fnDisplayToInches(Val(txtCir(4).Text)) + xyO.X
            xyWaistOFF.X = nLowSLine - FN_Round((Val(txtCir(4).Text) * VESTDIA1.g_nUnitsFac)) + xyO.X
            xyWaistOFF.Y = nWaistCir + nSeamAllowance + xyO.Y
            'xyEOSCL.X = xyInsertion.X
            'xyEOSCL.Y = xyInsertion.Y
            'xyEOSOFF.X = xyInsertion.X
            'xyEOSOFF.Y = nEOSCir + nSeamAllowance + xyInsertion.Y
            ''	xyWaistOFF.x = nLowSLine + nSeamAllowance - FN_Round((nS_Waist  * VESTDIA1.g_nUnitsFac)) + xyO.x;
            ''------------xyWaistOFF.X = nLowSLine - VESTDIA1.fnDisplayToInches(Val(txtCir(4).Text)) + xyInsertion.X
            'xyWaistOFF.X = nLowSLine - FN_Round((Val(txtCir(4).Text) * VESTDIA1.g_nUnitsFac)) + xyInsertion.X
            'xyWaistOFF.Y = nWaistCir + nSeamAllowance + xyInsertion.Y

        Else
            xyWaistCL.X = xyO.X
            xyWaistCL.Y = xyO.Y
            xyWaistOFF.X = xyO.X
            xyWaistOFF.Y = nWaistCir + nSeamAllowance + xyO.Y
            'xyWaistCL.X = xyInsertion.X
            '    xyWaistCL.Y = xyInsertion.Y
            '    xyWaistOFF.X = xyInsertion.X
            '    xyWaistOFF.Y = nWaistCir + nSeamAllowance + xyInsertion.Y
        End If

        '' Point at just under breast if BRA mesurements are given
        If BRAGiven Then
            xyBreast.Y = nBreastCir + nSeamAllowance + xyO.Y
            ''	xyBreast.x = nLowSLine + nSeamAllowance - nS_Breast + xyO.x; '' 16.Oct.97
            xyBreast.X = nLowSLine - nS_Breast + xyO.X
            'xyBreast.Y = nBreastCir + nSeamAllowance + xyInsertion.Y
            ''	xyBreast.x = nLowSLine + nSeamAllowance - nS_Breast + xyO.x; '' 16.Oct.97
            'xyBreast.X = nLowSLine - nS_Breast + xyInsertion.X
        End If

        ''' Set text attributes
        ' --------------------------------  SetData("TextFont", 0);
        '  ------------------------------- SetData("TextVertJust", 32);		'' Top
        '  ------------------------------- SetData("TextHorzJust", 4);		'' Right
        ' ----------------------------------  SetData("TextHeight", 0.125);
        '  ----------------------------------- SetData("TextAspect", 0.6);

        '' Axilla And related points
        '' Calculate control points (NB. special case for axillas of different hieghts)
        '' Get front neck And Raglan intersection
        '' Note - This Is point that controls the angle of the raglan curve
        '' 	The variable name "nNeckActualCir" Is missleading as it Is a calculated circumference 

        Dim nLtSCir, nRtSCir, nLtSLessCir, nRtSLessCir As Double
        nLtSCir = Val(txtCir(0).Text) * VESTDIA1.g_nUnitsFac
        nRtSCir = Val(txtCir(1).Text) * VESTDIA1.g_nUnitsFac
        Dim nAxilla As Integer
        Dim nSCirFac As Double = 2.5
        If Math.Abs(nLtSCir - nRtSCir) <= 1 Then
            nAxilla = 1
            '' By default figure for left only, this will apply to both left And right 
            nLtSCir = (nLtSCir + nRtSCir) / 2
            ''------------nLtSLessCir = VESTDIA1.fnDisplayToInches(nLtSCir * 0.9)  '' SleeveLess
            nLtSLessCir = FN_Round(nLtSCir * 0.9)    '' SleeveLess
            nRtSLessCir = nLtSLessCir           '' SleeveLess
            ''-------------nLtSCir = VESTDIA1.fnDisplayToInches(nLtSCir / nSCirFac)
            nLtSCir = FN_Round(nLtSCir / nSCirFac)
            ''		nRtSCir = nLtSCir;				
            nRtSCir = nLtSCir - 0.5            '' 16.Oct.97
            xyHighestAxilla.X = nLowSLine - nRtSCir + xyO.X
            xyHighestAxilla.Y = nChestCir + nSeamAllowance + xyO.Y
            'xyHighestAxilla.X = nLowSLine - nRtSCir + xyInsertion.X
            'xyHighestAxilla.Y = nChestCir + nSeamAllowance + xyInsertion.Y
            xyLowestAxilla = xyHighestAxilla
            sHighestAxilla = "None"
        Else
            nAxilla = 2
            ''---------nLtSLessCir = VESTDIA1.fnDisplayToInches(nLtSCir * 0.9)  '' SleeveLess
            nLtSLessCir = FN_Round(nLtSCir * 0.9)   '' SleeveLess
            ''   		nLtSCir = FN_Round(nLtSCir / nSCirFac);	
            ''------------nLtSCir = VESTDIA1.fnDisplayToInches(nLtSCir / nSCirFac) - 0.5 '' 16.Oct.97
            nLtSCir = FN_Round(nLtSCir / nSCirFac) - 0.5    '' 16.Oct.97
            ''-------------nRtSLessCir = VESTDIA1.fnDisplayToInches(nRtSCir * 0.9) '' SleeveLess
            nRtSLessCir = FN_Round(nRtSCir * 0.9) '' SleeveLess
            ''   		nRtSCir = FN_Round(nRtSCir / nSCirFac);	
            ''---------------nRtSCir = VESTDIA1.fnDisplayToInches(nRtSCir / nSCirFac) - 0.5 '' 16.Oct.97
            nRtSCir = FN_Round(nRtSCir / nSCirFac) - 0.5 '' 16.Oct.97
            xyHighestAxilla.X = nLowSLine - MANGLOVE1.min(nRtSCir, nLtSCir) + xyO.X
            xyHighestAxilla.Y = nChestCir + nSeamAllowance + xyO.Y
            'xyHighestAxilla.X = nLowSLine - min(nRtSCir, nLtSCir) + xyInsertion.X
            'xyHighestAxilla.Y = nChestCir + nSeamAllowance + xyInsertion.Y
            If BRAGiven Then
                aAngle = FN_CalcAngle(xyHighestAxilla, xyBreast) * (180 / VESTDIA1.PI)
            Else
                aAngle = FN_CalcAngle(xyHighestAxilla, xyWaistOFF) * (180 / VESTDIA1.PI)
            End If
            ''--------------------------------xyLowestAxilla = CalcXY("relpolar", xyHighestAxilla, Math.Abs(nRtSCir - nLtSCir), aAngle)
            PR_CalcPolar(xyHighestAxilla, Math.Abs(nRtSCir - nLtSCir), aAngle, xyLowestAxilla)
            If nRtSCir < nLtSCir Then
                sHighestAxilla = "Right"
            Else sHighestAxilla = "Left"
            End If
        End If

        Dim sLtAxillaType, sRtAxillaType As String
        sLtAxillaType = cboLeftAxilla.Text
        sRtAxillaType = cboRightAxilla.Text
        If sLtAxillaType.Equals(sRtAxillaType) = False Then
            nAxilla = 2
        End If

        '' Establish if either axilla Is SleeveLess
        ''

        Dim xyInt, xyBackNeckConstruct As xy
        If sRtAxillaType.Equals("Sleeveless") Or sLtAxillaType.Equals("Sleeveless") Then
            SleeveLess = True
        End If
        Dim xyLenSt, xyLenEnd As xy
        PR_MakeXY(xyLenSt, nHighSLine + xyO.X, nNeckCir + xyO.Y)
        PR_MakeXY(xyLenEnd, nLowSLine + xyO.X, xyO.Y)
        'PR_MakeXY(xyLenSt, nHighSLine + xyInsertion.X, nNeckCir + xyInsertion.Y)
        'PR_MakeXY(xyLenEnd, nLowSLine + xyInsertion.X, xyInsertion.Y)
        nNeckActualCir = FN_CalcLength(xyLenSt, xyLenEnd) '' GOP 01-02/18, 6.1

        '' Back Neck Construction point
        Dim xyStart, xyEnd, xyCen As xy
        PR_MakeXY(xyStart, nHighSLine + xyO.X, xyO.Y)
        PR_MakeXY(xyEnd, nHighSLine + xyO.X, 100.0 + xyO.Y)
        PR_MakeXY(xyCen, nLowSLine + xyO.X, xyO.Y)
        'PR_MakeXY(xyStart, nHighSLine + xyInsertion.X, xyInsertion.Y)
        'PR_MakeXY(xyEnd, nHighSLine + xyInsertion.X, 100.0 + xyInsertion.Y)
        'PR_MakeXY(xyCen, nLowSLine + xyInsertion.X, xyInsertion.Y)
        If FN_CirLinInt(xyStart, xyEnd, xyCen, nNeckActualCir, xyInt) Then
            xyBackNeckConstruct = xyInt
        Else
            MsgBox("Can't form Back neck with this data!", 48, "Vest")
        End If
        ''Get front neck intesection
        '' 
        ''N.B. If either axilla Is sleeveless then the entire back neck Is constructed 
        ''        w.r.t the sleeveless axilla
        Dim nNeckFrontFac As Double = 1
        Dim xyFrontNeckOthers As xy
        PR_MakeXY(xyStart, nHighSLine - nNeckFrontFac + xyO.X, xyO.Y)
        PR_MakeXY(xyEnd, nHighSLine - nNeckFrontFac + xyO.X, 100.0 + xyO.Y)
        PR_MakeXY(xyCen, nLowSLine + xyO.X, xyO.Y)
        'PR_MakeXY(xyStart, nHighSLine - nNeckFrontFac + xyInsertion.X, xyInsertion.Y)
        'PR_MakeXY(xyEnd, nHighSLine - nNeckFrontFac + xyInsertion.X, 100.0 + xyInsertion.Y)
        'PR_MakeXY(xyCen, nLowSLine + xyInsertion.X, xyInsertion.Y)
        If FN_CirLinInt(xyStart, xyEnd, xyCen, nNeckActualCir, xyInt) Then
            xyFrontNeckOFF = xyInt
            '' note mods w.r.t. sleveless vest
            If SleeveLess Then
                xyFrontNeckOthers = xyFrontNeckOFF
                xyFrontNeckOFF = xyBackNeckConstruct
            End If
        Else
            MsgBox("Can't form Front neck with this data" & Chr(13) & "Check JOB.LOG", 48, "Vest")
        End If

        '' Calculations for front neck  
        Dim nCLNeckDrop As Double
        If Val(txtAge.Text) <= 6 Then
            nCLNeckDrop = 0.25  '' 16.Oct.97
        Else
            nCLNeckDrop = 0.5   '' 16.Oct.97
        End If

        '' No CL Neck drop for any turtle neck, overide above	
        '--------------------------If (StringCompare("Turtle", sNeckType, 6)) Then
        If sNeckType.Contains("Turtle") Then
            nCLNeckDrop = 0 '' 17.Nov.97
        End If
        If sNeckType.Equals("Measured Scoop") Then
            xyFrontNeckCL.X = nLowSLine - (nNeckCirRetained + nCLNeckDrop) + xyO.X
            ''xyFrontNeckCL.X = nLowSLine - (nNeckCirRetained + nCLNeckDrop) + xyInsertion.X
        Else
            xyFrontNeckCL.X = nLowSLine - (nNeckActualCir + nCLNeckDrop) + xyO.X
            ''xyFrontNeckCL.X = nLowSLine - (nNeckActualCir + nCLNeckDrop) + xyInsertion.X
        End If

        xyFrontNeckCL.Y = xyO.Y
        xyFrontNeckCen.X = nLowSLine + xyO.X
        xyFrontNeckCen.Y = xyO.Y
        'xyFrontNeckCL.Y = xyInsertion.Y
        'xyFrontNeckCen.X = nLowSLine + xyInsertion.X
        'xyFrontNeckCen.Y = xyInsertion.Y

        '' Calculation for Back Neck 
        '' Note:- mods for a back scooped neck ( Same Radius Is used for both)
        'Dim nSeamAllowance As Double = 0.125

        xyBackNeckCL.X = nLowSLine + nSeamAllowance + xyO.X
        xyBackNeckCL.Y = xyO.Y
        'xyBackNeckCL.X = nLowSLine + nSeamAllowance + xyInsertion.X
        '    xyBackNeckCL.Y = xyInsertion.Y

        Dim nBackNeckRadius As Double
        nLength = FN_CalcLength(xyBackNeckCL, xyBackNeckConstruct)
        aAngle = FN_CalcAngle(xyBackNeckCL, xyBackNeckConstruct) ' * (180 / VESTDIA1.PI)
        nBackNeckRadius = (nLength / 2) / System.Math.Cos(aAngle)
        '---------------Dim nBackNeckRadius As Double = 10
        If cboBackNeck.Text.Equals("Regular") Then
            '' Regular Back Neck - Arc centre
            xyBackNeckCL.X = nLowSLine + nSeamAllowance + xyO.X
            'xyBackNeckCL.X = nLowSLine + nSeamAllowance + xyInsertion.X
            xyBackNeckCen.X = xyBackNeckCL.X + nBackNeckRadius
            xyBackNeckCen.Y = xyO.Y
            'xyBackNeckCen.Y = xyInsertion.Y
        End If
        Dim nBackNeckScoopFacChild As Double = 0.375
        Dim nBackNeckScoopFacAdult As Double = 0.75
        Dim xyBackNeckConstruct_2 As xy
        If cboBackNeck.Text.Equals("Scoop") Or cboBackNeck.Text.Equals("Measured Scoop") Then
            If Val(txtAge.Text) <= 6 Then
                nLength = nBackNeckScoopFacChild
            Else
                nLength = nBackNeckScoopFacAdult
            End If
            If cboBackNeck.Text.Equals("Measured Scoop") Then
                nLength = Val(txtBackNeck.Text) * VESTDIA1.g_nUnitsFac
            End If
            xyBackNeckCL.X = nLowSLine + nSeamAllowance + xyO.X - nLength
            ''xyBackNeckCL.X = nLowSLine + nSeamAllowance + xyInsertion.X - nLength
            nLength = FN_CalcLength(xyBackNeckCL, xyBackNeckConstruct)
            aAngle = FN_CalcAngle(xyBackNeckCL, xyBackNeckConstruct) * (180 / VESTDIA1.PI)
            xyBackNeckConstruct_2.X = xyBackNeckCL.X + System.Math.Cos(aAngle) * nLength / 2
            xyBackNeckConstruct_2.Y = xyBackNeckCL.Y + System.Math.Sin(aAngle) * nLength / 2
            nLength = System.Math.Sqrt((nBackNeckRadius * nBackNeckRadius) - ((nLength * nLength) / 4))
            ''------------------------------xyBackNeckCen = CalcXY("relpolar", xyBackNeckConstruct_2, nLength, aAngle + 270)
            PR_CalcPolar(xyBackNeckConstruct_2, nLength, aAngle + 270, xyBackNeckCen)
        End If

        '' Get the vestcurve angle
        '' For Highest axilla
        ''   nLowestAxillaFrontNeckRad = Calc ("length", xyLowestAxilla, xyFrontNeckOFF); '' Approx. only
        Dim nHighestAxillaFrontNeckRad, aHighestVestCurve, aHighestCurveRotation As Double
        nHighestAxillaFrontNeckRad = FN_CalcLength(xyHighestAxilla, xyFrontNeckOFF)  '' V.I.P.
        aHighestVestCurve = FN_CalcAngle(xyHighestAxilla, xyFrontNeckOFF) * (180 / VESTDIA1.PI)     '' V.I.P.	
        aHighestCurveRotation = FN_CurveAngle(xyHighestAxilla, nHighestAxillaFrontNeckRad)
        Dim nInitial_nAxilla As Integer = nAxilla      '' Retain this number w.r.t axilla labeling

        ''-------------------------------- Main While Loop ---------------------------------------
        ''
        Dim sAxillaType, sSide As String
        Dim aCurve, aVestCurve, nAxillaFrontNeckRad As Double
        sSide = vbNullString
        While nAxilla > 0
            If nAxilla = 2 Then
                '' Set up side
                If sHighestAxilla.Equals("Right") Or sHighestAxilla.Equals("None") Then
                    sAxillaType = sRtAxillaType
                    sSide = "Right"
                Else
                    sAxillaType = sLtAxillaType
                    sSide = "Left"
                End If
                aCurve = aHighestCurveRotation
                aVestCurve = aHighestVestCurve
                nAxillaFrontNeckRad = nHighestAxillaFrontNeckRad
                '' Recalculate rotations etc if either axilla Is sleeveless
                If SleeveLess And sAxillaType.Equals("Sleeveless") = False Then
                    nHighestAxillaFrontNeckRad = FN_CalcLength(xyHighestAxilla, xyFrontNeckOthers)  '' V.I.P.
                    aCurve = FN_CurveAngle(xyHighestAxilla, nHighestAxillaFrontNeckRad)
                    aVestCurve = FN_CalcAngle(xyHighestAxilla, xyFrontNeckOthers) * (180 / VESTDIA1.PI)
                    nAxillaFrontNeckRad = nHighestAxillaFrontNeckRad
                End If
                xyAxilla = xyHighestAxilla
            Else
                If sHighestAxilla.Equals("None") Then
                    aCurve = aHighestCurveRotation
                    aVestCurve = aHighestVestCurve
                    nAxillaFrontNeckRad = nHighestAxillaFrontNeckRad
                    xyAxilla = xyHighestAxilla
                    sSide = "Left"
                    If SleeveLess And sLtAxillaType.Equals("Sleeveless") = False Then
                        nHighestAxillaFrontNeckRad = FN_CalcLength(xyHighestAxilla, xyFrontNeckOthers)  '' V.I.P.
                        aCurve = FN_CurveAngle(xyHighestAxilla, nHighestAxillaFrontNeckRad)
                        aVestCurve = FN_CalcAngle(xyHighestAxilla, xyFrontNeckOthers) * (180 / VESTDIA1.PI)
                        nAxillaFrontNeckRad = nHighestAxillaFrontNeckRad
                    End If
                Else
                    '' xyRaglanNeck Is established from the previous axilla
                    '' it ensures that both curves go to the back neck

                    nAxillaFrontNeckRad = FN_CalcLength(xyLowestAxilla, xyRaglanNeck)  '' V.I.P.
                    aCurve = FN_CurveAngle(xyLowestAxilla, nAxillaFrontNeckRad)
                    aVestCurve = FN_CalcAngle(xyLowestAxilla, xyRaglanNeck) * (180 / VESTDIA1.PI)   '' V.I.P.	
                    xyAxilla = xyLowestAxilla

                    '' Set up for Left or right Hand for lower axilla
                    If sHighestAxilla.Equals("Right") Then
                        sSide = "Left"
                        sAxillaType = sLtAxillaType
                    Else
                        sSide = "Right"
                        sAxillaType = sRtAxillaType
                    End If
                    If SleeveLess And sAxillaType.Equals("Sleeveless") Then
                        nAxillaFrontNeckRad = FN_CalcLength(xyLowestAxilla, xyFrontNeckOthers)  '' V.I.P.
                        aCurve = FN_CurveAngle(xyLowestAxilla, nAxillaFrontNeckRad)
                        aVestCurve = FN_CalcAngle(xyLowestAxilla, xyFrontNeckOthers) * (180 / VESTDIA1.PI)
                    End If
                End If
            End If
            ''End While

            '' Set up layers and colours for Left or right Hand 
            If sSide.Equals("Left") Then
                ARMDIA1.PR_SetLayer("TemplateLeft")
                sAxillaType = sLtAxillaType
            Else
                sAxillaType = sRtAxillaType
                ARMDIA1.PR_SetLayer("TemplateRight")
            End If
            '' Find axilla closest to shoulder for bra cup placement
            nBraAxillaHt = max(nBraAxillaHt, xyAxilla.X)

            '' Do axilla.
            '' The axilla MACROS are set by the visual basic programme.
            '' I.E. DRAW_1.D points to the relevant MACRO/S

            '@C\JOBST\DRAW_1.D
            Dim nRegAxillaNormal As Double = 2
            Dim nRegAxillaChild As Double = 1.5
            Dim nRegAxillaLargeAdult As Double = 2.5
            Dim DrawSegment As Boolean = False         ''// Set draw segments flag of
            Dim DrawInsert As Boolean = False
            Dim xyInsertConstruct_1, xyInsertConstruct_2 As xy
            'If StringCompare("Regular", sAxillaType, 7) Then
            If sAxillaType.Contains("Regular") Then
                If sAxillaType.Equals("Regular 2" & Chr(34)) Then
                    nRegAxillaFac_1 = nRegAxillaNormal
                End If
                If sAxillaType.Equals("Regular 1�" & Chr(34)) Then
                    nRegAxillaFac_1 = nRegAxillaChild
                End If
                If sAxillaType.Equals("Regular 2�" & Chr(34)) Then
                    nRegAxillaFac_1 = nRegAxillaLargeAdult
                End If
                If BRAGiven And FN_CirLinInt(xyBreast, xyAxilla, xyAxilla, nRegAxillaFac_1, xyInt) Then
                    xyAxillaLow = xyInt
                ElseIf FN_CirLinInt(xyWaistOFF, xyAxilla, xyAxilla, nRegAxillaFac_1, xyInt) Then
                    xyAxillaLow = xyInt
                Else
                    MsgBox("Can't form Axilla with this data" & Chr(13), 48, "Vest Body - Dialog")
                End If
                xyCen = xyAxilla
                nRadius = nSeamAllowance
                nCount = 1
                If sSide.Equals("Right") Then
                    nXInsert = -10 ''	// Right
                    nYInsert = 0
                Else
                    nXInsert = 10 ''	// Left
                    nYInsert = -3
                End If
                ''// Open Curve for Reading
                ''// Assume that if the tests for Existance/Corruption are passed. Then no need to repeat them

                ''--------------------------hCurve = Open("file", sPathJOBST + "\\TEMPLTS\\VESTCURV.DAT", "readonly")
                Dim fileNum As Object
                fileNum = FreeFile()
                FileOpen(fileNum, sSettingsPath & "\\VESTCURV.DAT", VB.OpenMode.Input)
                ''---------------GetLine(hCurve, & sLine)
                Dim sLine As String

                ''--------------------ScanLine(sLine, "blank", & nLength, & aAngle)

                aPrevAngle = (aVestCurve - aCurve)      ''// Rotate curve To correct start angle

                xyPt1 = xyAxilla
                ''------------------StartPoly("polyline")
                Dim ptPlineColl As Point3dCollection = New Point3dCollection()
                Dim xyPt2 As xy
                While Not EOF(fileNum)
                    sLine = LineInput(fileNum)
                    FN_GetNumber(sLine, nLength, aAngle)
                    aAngle = aAngle + aPrevAngle
                    PR_CalcPolar(xyPt1, nLength, aAngle, xyPt2)

                    If FN_CirLinInt(xyPt1, xyPt2, xyCen, nRadius, xyInt) Then
                        If nCount = 1 Then
                            DrawInsert = True       ''// Draw insert from seam -
                            xyPt1 = xyInt
                            xyInsertConstruct_3 = xyInt
                            nRadius = nRegAxillaFac_1       ''//   allowance
                        End If
                        If nCount = 2 Then
                            xyCen = xyInt           ''// Found Construction point
                            nRadius = nRegAxillaFac_2
                        End If
                        If nCount = 3 Then
                            xyCen = xyBackNeckCen       ''// Found raglan Start
                            nRadius = nBackNeckRadius
                            xyRaglanAxilla = xyInt
                            ''--------------------hEnt = AddEntity("line", TransXY(xyPt1, "trs", nXInsert, nYInsert), TransXY(xyInt, "trs", nXInsert, nYInsert)) ''// Draw insert
                            ''----------------------SetDBData(hEnt, "Data", sID)
                            PR_MakeXY(xyStart, xyPt1.X + nXInsert, xyPt1.Y + nYInsert)
                            PR_MakeXY(xyEnd, xyInt.X + nXInsert, xyInt.Y + nYInsert)
                            PR_DrawLine(xyStart, xyEnd)
                            xyPt1 = xyInt
                            ''// Check if xyRaglanAxilla Is too close to frontneck
                            Dim nInsertToNeckMinDist As Double = 0.625
                            If FN_CalcLength(xyRaglanAxilla, xyFrontNeckOFF) < nInsertToNeckMinDist Then
                                ''----------------------Display("message", "error", "WARNING, Axilla Insert line is too close to Front Neck\nTry a smaller Axilla insert\nModule Cancelled")
                                MsgBox("WARNING, Axilla Insert line is too close to Front Neck" & Chr(13) & "Try a smaller Axilla insert" & Chr(13) & "Module Cancelled", 48, "Vest Body - Dialog")
                                ''--------------Close("file", hCurve)
                                FileClose()
                                ''-----------------------EndPoly()
                                '-------------Exit (cancel, "Module Aborted")
                                MsgBox("Module Aborted")
                                Return
                            End If
                            DrawSegment = True      ''// Start Drawing from here
                            DrawInsert = False      ''// Stop drawing insert
                        End If
                        If nCount = 4 Then
                            xyRaglanNeck = xyInt        ''// Found Raglan End
                            ''//			AddEntity("line", xyPt1, xyRaglanNeck)
                            ''-------------------AddVertex(xyPt1)
                            ''--------------------AddVertex(xyRaglanNeck)
                            ''---------------------EndPoly()
                            ptPlineColl.Add(New Point3d(xyPt1.X, xyPt1.Y, 0))
                            ptPlineColl.Add(New Point3d(xyRaglanNeck.X, xyRaglanNeck.Y, 0))
                            ''---------------hEnt = UID("find", UID("getmax"))
                            ''--------------------SetDBData(hEnt, "Data", sID)
                            ''--------------------- SetDBData(hEnt, "curvetype", "vest" + sSide + "raglan")
                            ''-----------break
                            Exit While
                        End If
                        nCount = nCount + 1
                    End If

                    ''//         if ( DrawSegment ) AddEntity("line",xyPt1,xyPt2);  // Draw raglan
                    If DrawSegment Then
                        ''-----------------AddVertex(xyPt1)   ''// Draw raglan
                        ptPlineColl.Add(New Point3d(xyPt1.X, xyPt1.Y, 0))
                    End If
                    If DrawInsert Then
                        ''--------------------AddEntity("line", TransXY(xyPt1, "trs", nXInsert, nYInsert),TransXY(xyPt2, "trs", nXInsert, nYInsert))  ''// Draw insert
                        PR_MakeXY(xyStart, xyPt1.X + nXInsert, xyPt1.Y + nYInsert)
                        PR_MakeXY(xyEnd, xyPt2.X + nXInsert, xyPt2.Y + nYInsert)
                        PR_DrawLine(xyStart, xyEnd)
                    End If
                    xyPt1 = xyPt2
                    aPrevAngle = aAngle
                    ''------------------------ScanLine(sLine, "blank", & nLength, & aAngle)
                End While '' //End GetLine while
                If ptPlineColl.Count > 0 Then
                    Dim ii As Double
                    Dim PlineStart(ptPlineColl.Count), PlineEnd(ptPlineColl.Count) As Double
                    For ii = 0 To ptPlineColl.Count - 1
                        Dim ptPline As Point3d = ptPlineColl(ii)
                        PlineStart(ii + 1) = ptPline.X
                        PlineEnd(ii + 1) = ptPline.Y
                    Next
                    PR_DrawPoly(PlineStart, PlineEnd, ptPlineColl.Count)
                End If

                ''---------------------Close("file", hCurve)
                FileClose()
                ''// Regular Axilla Construction point
                ''//
                aAngle = System.Math.Acos(nRegAxillaFac_1 / FN_CalcLength(xyAxilla, xyRaglanAxilla))
                aAngle = (FN_CalcAngle(xyAxilla, xyRaglanAxilla) - aAngle) * (180 / VESTDIA1.PI)
                ''-----------------xyAxillaConstruct_2 = CalcXY("relpolar", xyAxilla, nRegAxillaFac_1, aAngle)
                PR_CalcPolar(xyAxilla, nRegAxillaFac_1, aAngle, xyAxillaConstruct_2)
                ''------------------xyInsertConstruct_1 = CalcXY("relpolar", xyAxilla, nRegAxillaFac_1 + nSeamAllowance, aAngle)
                PR_CalcPolar(xyAxilla, nRegAxillaFac_1 + nSeamAllowance, aAngle, xyInsertConstruct_1)
                ''------------------xyInsertConstruct_2 = CalcXY("relpolar", xyRaglanAxilla, nSeamAllowance, aAngle)
                PR_CalcPolar(xyRaglanAxilla, nSeamAllowance, aAngle, xyInsertConstruct_2)
            End If ''// End if for Regular axilla

            ''--------------Open and Lining Axilla ----------
            Dim nLengthToOpenAxillaPt, nTraversedLength As Double
            ''Dim xyText As xy
            If (sAxillaType.Equals("Open") Or sAxillaType.Equals("Lining")) Then
                xyAxillaLow = xyAxilla
                nLengthToOpenAxillaPt = 0.0
                Dim fileNum As Object
                fileNum = FreeFile()
                FileOpen(fileNum, sSettingsPath & "\\VESTCURV.DAT", VB.OpenMode.Input)
                ''-----------hCurve = Open("file", sPathJOBST + "\\TEMPLTS\\VESTCURV.DAT", "readonly")
                ''-----------------GetLine(hCurve, & sLine
                ''-----------ScanLine(sLine, "blank", & nLength, & aAngle)
                aPrevAngle = (aVestCurve - aCurve)
                xyPt1 = xyAxilla
                DrawSegment = True                  '// Set draw segments flag on
                Dim sLine As String
                Dim xyPt2 As xy
                ''------------StartPoly("polyline")
                Dim ptPlineColl As Point3dCollection = New Point3dCollection
                While Not EOF(fileNum)
                    sLine = LineInput(fileNum)
                    FN_GetNumber(sLine, nLength, aAngle)
                    aAngle = aAngle + aPrevAngle
                    ''-----xyPt2 = CalcXY("relpolar", xyPt1, nLength, aAngle)
                    PR_CalcPolar(xyPt1, nLength, aAngle, xyPt2)
                    If (FN_CirLinInt(xyPt1, xyPt2, xyBackNeckCen, nBackNeckRadius, xyInt)) Then
                        ''//			AddEntity("line", xyPt1, xyInt)
                        ''-----------AddVertex(xyPt1)
                        ptPlineColl.Add(New Point3d(xyPt1.X, xyPt1.Y, 0))
                        nLengthToOpenAxillaPt = nLengthToOpenAxillaPt + FN_CalcLength(xyPt1, xyInt)
                        xyRaglanNeck = xyInt            ''// Found Raglan End
                        ''---------AddVertex(xyRaglanNeck)
                        ptPlineColl.Add(New Point3d(xyRaglanNeck.X, xyRaglanNeck.Y, 0))
                        ''--------EndPoly()
                        ''-----------hEnt = UID("find", UID("getmax"))
                        ''------------SetDBData(hEnt, "Data", sID)
                        ''--------------SetDBData(hEnt, "curvetype", "vest" + sSide + "raglan")
                        Exit While
                    End If
                    If (DrawSegment) Then
                        ''--------AddVertex(xyPt1) ''  // Draw raglan
                        ptPlineColl.Add(New Point3d(xyPt1.X, xyPt1.Y, 0))
                    End If
                    nLengthToOpenAxillaPt = nLengthToOpenAxillaPt + FN_CalcLength(xyPt1, xyPt2)
                    xyPt1 = xyPt2
                    aPrevAngle = aAngle
                    ''--------------ScanLine(sLine, "blank", & nLength, & aAngle)
                End While
                FileClose()
                If ptPlineColl.Count > 0 Then
                    Dim ii As Double
                    Dim PlineStart(ptPlineColl.Count), PlineEnd(ptPlineColl.Count) As Double
                    For ii = 0 To ptPlineColl.Count - 1
                        Dim ptPline As Point3d = ptPlineColl(ii)
                        PlineStart(ii + 1) = ptPline.X
                        PlineEnd(ii + 1) = ptPline.Y
                    Next
                    PR_DrawPoly(PlineStart, PlineEnd, ptPlineColl.Count)
                End If
                ARMDIA1.PR_SetLayer("Notes")
                If (sAxillaType.Equals("Open")) Then
                    ''// Locate 1/3 raglan length And add an arrow at that point
                    ''// Label axilla as open
                    nLengthToOpenAxillaPt = nLengthToOpenAxillaPt / 3
                    nTraversedLength = 0.0
                    ''------hCurve = Open("file", sPathJOBST + "\\TEMPLTS\\VESTCURV.DAT", "readonly")
                    ''--------GetLine(hCurve, & sLine)
                    ''--------ScanLine(sLine, "blank", & nLength, & aAngle)
                    fileNum = FreeFile()
                    FileOpen(fileNum, sSettingsPath & "\\VESTCURV.DAT", VB.OpenMode.Input)
                    aPrevAngle = (aVestCurve - aCurve)
                    xyPt1 = xyAxilla
                    Dim nError As Short
                    While Not EOF(fileNum)
                        sLine = LineInput(fileNum)
                        FN_GetNumber(sLine, nLength, aAngle)
                        aAngle = aAngle + aPrevAngle
                        ''----------xyPt2 = CalcXY("relpolar", xyPt1, nLength, aAngle)
                        PR_CalcPolar(xyPt1, nLength, aAngle, xyPt2)
                        nTraversedLength = nTraversedLength + nLength
                        If (nTraversedLength > nLengthToOpenAxillaPt) Then
                            nError = FN_CirLinInt(xyPt1, xyPt2, xyPt1, nTraversedLength - nLengthToOpenAxillaPt, xyInt)
                            ''-------------hEnt = AddEntity("marker", "medarrow",xyInt,0.2,0.1,Calc("angle", xyPt1, xyPt2) - 90)
                            ''------------SetDBData(hEnt, "Data", sID)
                            ''-----------SetDBData(hEnt, "curvetype", "vest" + sSide + "openaxillamarker")
                            ''// Add text
                            ''-----------AddEntity("text","OPEN",xyAxilla.X - 0.25,xyAxilla.Y - 0.5)
                            PR_MakeXY(xyTextIns, xyAxilla.X - 0.25, xyAxilla.Y - 0.5)
                            PR_DrawText("OPEN", xyTextIns, 0.125, 0, 2)
                            ''---------AddEntity("text","AXILLA",xyAxilla.X - 0.25,xyAxilla.Y - 0.7)
                            PR_MakeXY(xyTextIns, xyAxilla.X - 0.25, xyAxilla.Y - 0.7)
                            PR_DrawText("AXILLA", xyTextIns, 0.125, 0, 2)
                            Exit While
                        End If
                        xyPt1 = xyPt2
                        aPrevAngle = aAngle
                        ''----------ScanLine(sLine, "blank", & nLength, & aAngle)
                    End While
                    FileClose()
                Else
                    ''--------AddEntity("text", "LINING", xyAxilla.X - 0.25, xyAxilla.Y - 0.5)
                    PR_MakeXY(xyTextIns, xyAxilla.X - 0.25, xyAxilla.Y - 0.5)
                    PR_DrawText("LINING", xyTextIns, 0.125, 0, 2)
                End If
                If (sSide.Equals("Right")) Then
                    ARMDIA1.PR_SetLayer("TemplateRight")
                Else
                    ARMDIA1.PR_SetLayer("TemplateLeft")
                End If
            End If
            ''------------------Mesh Axilla -----------
            If (sAxillaType.Equals("Mesh")) Then
                Dim nMeshAxillaFac, nMeshAxillaGussetFac, nMeshAxillaGussetBoysFac, nMSLengthGusset As Double
                Dim nMeshSymbolLength, nMeshLength, nMSLengthGussetBoys, nDistanceAlongRaglan As Double
                nMeshAxillaGussetFac = 0.75
                nMeshAxillaGussetBoysFac = 0.8125
                nMSLengthGusset = 2.1875
                nMSLengthGussetBoys = 2.375
                If (Val(txtAge.Text) <= 14) Then
                    nMeshAxillaFac = nMeshAxillaGussetFac
                Else
                    nMeshAxillaFac = nMeshAxillaGussetBoysFac
                End If

                If (BRAGiven And FN_CirLinInt(xyBreast, xyAxilla, xyAxilla, nMeshAxillaFac, xyInt)) Then
                    xyAxillaLow = xyInt
                ElseIf (FN_CirLinInt(xyWaistOFF, xyAxilla, xyAxilla, nMeshAxillaFac, xyInt)) Then
                    xyAxillaLow = xyInt
                Else
                    ''-------------Display("message", "error", "Can't form Axilla with this data\n")
                    MsgBox("Can't form Axilla with this data", 48, "Vest Body - Dialog")
                End If

                If (Val(txtAge.Text) <= 14) Then
                    ''//	sMeshSymbol = "mesh0to14"
                    nMeshSymbolLength = nMSLengthGusset
                    nMeshLength = 2.58
                Else
                    ''//	sMeshSymbol = "mesh15andUP"
                    nMeshSymbolLength = nMSLengthGussetBoys
                    nMeshLength = 2.8
                End If
                ''// Open Curve for Reading
                ''// Assume that if the tests for Existance/Corruption are passed. Then no need to repeat them

                ''hCurve = Open("file", sPathJOBST + "\\TEMPLTS\\VESTCURV.DAT", "readonly");
                ''GetLine(hCurve, & sLine);
                ''ScanLine(sLine, "blank", & nLength, & aAngle);
                Dim fileNum As Object
                fileNum = FreeFile()
                FileOpen(fileNum, sSettingsPath & "\\VESTCURV.DAT", VB.OpenMode.Input)

                aPrevAngle = (aVestCurve - aCurve) ''     // Rotate curve To correct start angle
                xyCen = xyAxillaLow
                nRadius = nMeshSymbolLength - nSeamAllowance - 0.25
                nDistanceAlongRaglan = 0
                nCount = 1
                xyPt1 = xyAxilla
                DrawSegment = False             ''  // Set draw segments flag off
                Dim sLine As String
                Dim xyPt2, xyMeshAtSeamAllowance As xy
                ''--------------StartPoly("polyline")
                Dim ptPlineColl As Point3dCollection = New Point3dCollection
                While Not EOF(fileNum)
                    sLine = LineInput(fileNum)
                    FN_GetNumber(sLine, nLength, aAngle)
                    aAngle = aAngle + aPrevAngle
                    ''--------xyPt2 = CalcXY("relpolar", xyPt1, nLength, aAngle)
                    PR_CalcPolar(xyPt1, nLength, aAngle, xyPt2)
                    ''//	if (!DrawSegment) nDistanceAlongRaglan = nDistanceAlongRaglan + nLength ;

                    If (FN_CirLinInt(xyPt1, xyPt2, xyCen, nRadius, xyInt)) Then
                        If (nCount = 1) Then
                            DrawSegment = True          ''// Draw from seam allowance
                            xyRaglanAxilla = xyInt
                            nRadius = nMeshSymbolLength
                            ''// Check if seam allowance on same segment
                            If (FN_CirLinInt(xyPt1, xyPt2, xyCen, nRadius, xyInt)) Then
                                nCount = 2
                            End If
                            ''//			nDistanceAlongRaglan = nDistanceAlongRaglan + Calc ("length", xyPt1, xyPt2);
                            xyPt1 = xyRaglanAxilla
                        End If
                        If (nCount = 2) Then
                            xyMeshAtSeamAllowance = xyInt   ''// Mesh symbol End For angle
                            xyCen = xyBackNeckCen
                            nRadius = nBackNeckRadius
                        End If
                        If (nCount = 3) Then
                            xyRaglanNeck = xyInt            ''// Found Raglan End
                            ''//			AddEntity("line", xyPt1, xyRaglanNeck);
                            ''--------------AddVertex(xyPt1)
                            ''----------AddVertex(xyRaglanNeck)
                            ''---------------EndPoly()
                            ptPlineColl.Add(New Point3d(xyPt1.X, xyPt1.Y, 0))
                            ptPlineColl.Add(New Point3d(xyRaglanNeck.X, xyRaglanNeck.Y, 0))
                            ''------------hEnt = UID("find", UID("getmax"))
                            ''--------SetDBData(hEnt, "Data", sID)
                            ''---------SetDBData(hEnt, "curvetype", "vest" + sSide + "raglan")
                            Exit While
                        End If
                        nCount = nCount + 1
                    End If

                    ''//                if ( DrawSegment ) AddEntity("line",xyPt1,xyPt2);  ''// Draw raglan
                    If (DrawSegment) Then
                        ''-------AddVertex(xyPt1)   ''// Draw raglan
                        ptPlineColl.Add(New Point3d(xyPt1.X, xyPt1.Y, 0))
                    End If
                    xyPt1 = xyPt2
                    aPrevAngle = aAngle
                    ''------ScanLine(sLine, "blank", & nLength, & aAngle);
                End While
                FileClose()
                If ptPlineColl.Count > 0 Then
                    Dim ii As Double
                    Dim PlineStart(ptPlineColl.Count), PlineEnd(ptPlineColl.Count) As Double
                    For ii = 0 To ptPlineColl.Count - 1
                        Dim ptPline As Point3d = ptPlineColl(ii)
                        PlineStart(ii + 1) = ptPline.X
                        PlineEnd(ii + 1) = ptPline.Y
                    Next
                    PR_DrawPoly(PlineStart, PlineEnd, ptPlineColl.Count)
                End If

                ''// Insert Mesh axilla symbol at correct angle 

                aAngle = FN_CalcAngle(xyAxillaLow, xyMeshAtSeamAllowance)

                ''//   SetSymbolLibrary( sPathJOBST + "\\JOBST.SLB"); 
                ''//   if ( !Symbol("find", sMeshSymbol+"seam")) Exit(%cancel, "Can't find MESH Axilla symbol to insert\nCheck your installation, that JOBST.SLB exists");
                ''//   if ( !Symbol("find", sMeshSymbol+"pro")) Exit(%cancel, "Can't find MESH Axilla symbol to insert\nCheck your installation, that JOBST.SLB exists");
                ''// Insert  Seam
                ARMDIA1.PR_SetLayer("Notes")
                ''//   hMesh = AddEntity("symbol", sMeshSymbol + "seam", xyAxillaLow, 1, 1, aAngle);
                If (Val(txtAge.Text) <= 14) Then
                    ''---------AddEntity("text", "1-3/4\" "GUSSET", xyAxillaLow.X - 0.25, xyAxillaLow.Y - 0.5)
                    PR_MakeXY(xyTextIns, xyAxillaLow.X - 0.25, xyAxillaLow.Y - 0.5)
                    PR_DrawText("1-3/4" & Chr(34) & "GUSSET", xyTextIns, 0.125, 0, 0)
                Else
                    ''-----------AddEntity("text", "BOYS GUSSET", xyAxillaLow.X - 0.25, xyAxillaLow.Y - 0.5)
                    PR_MakeXY(xyTextIns, xyAxillaLow.X - 0.25, xyAxillaLow.Y - 0.5)
                    PR_DrawText("BOYS GUSSET", xyTextIns, 0.125, 0, 0)
                End If

                ''// Insert profile
                If (sSide.Equals("Right")) Then
                    ''----------AddEntity("text", "RIGHT", xyAxillaLow.X - 0.25, xyAxillaLow.Y - 0.75)
                    PR_MakeXY(xyTextIns, xyAxillaLow.X - 0.25, xyAxillaLow.Y - 0.75)
                    PR_DrawText("RIGHT", xyTextIns, 0.125, 0, 0)
                    ARMDIA1.PR_SetLayer("TemplateRight")
                Else
                    ARMDIA1.PR_SetLayer("TemplateLeft")
                End If
                ''//   hMesh = AddEntity("symbol", sMeshSymbol + "pro", xyAxillaLow, 1, 1, aAngle);

                ''// Revised code to write a datafile MESHDRAW Is started at the end of the macro BODY.D 
                fileNum = FreeFile()
                If (bPrevAxillaWasMesh) Then
                    ''---------SetData("UnitLinearType", 0)    ''// "Inches"
                    ''---------hCurve = Open("file", "C:\\JOBST\\MESHDRAW.DAT", "append")
                    FileOpen(fileNum, sSettingsPath & "\\MESHDRAW.DAT", VB.OpenMode.Append)
                Else
                    ''-----------hCurve = Open("file", "C:\\JOBST\\MESHDRAW.DAT", "write")
                    FileOpen(fileNum, sSettingsPath & "\\MESHDRAW.DAT", VB.OpenMode.Output)
                    ''-----------SetData("UnitLinearType", 0)    ''// "Inches"
                    ''------------PrintFile(hCurve, "vestmesh", "\n")
                    PrintLine(fileNum, "vestmesh", "\n")
                    ''-----------PrintFile(hCurve, UID("get", hBody), "\n")
                    PrintLine(fileNum, "get", "\n")
                End If

                ''----------PrintFile(hCurve, xyAxillaLow, "\n")
                PrintLine(fileNum, Str(xyAxillaLow.X), Str(xyAxillaLow.Y), "\n")
                ''---------PrintFile(hCurve, xyAxilla, "\n")
                PrintLine(fileNum, Str(xyAxilla.X), Str(xyAxilla.Y), "\n")
                ''-----------PrintFile(hCurve, xyAxilla, "\n")
                PrintLine(fileNum, Str(xyAxilla.X), Str(xyAxilla.Y), "\n")
                ''--------------PrintFile(hCurve, xyRaglanNeck, "\n")
                PrintLine(fileNum, Str(xyRaglanNeck.X), Str(xyRaglanNeck.Y), "\n")
                ''--------------PrintFile(hCurve, nMeshLength, "\n")
                PrintLine(fileNum, Str(nMeshLength), "\n")
                ''---------------PrintFile(hCurve, nDistanceAlongRaglan, "\n")
                PrintLine(fileNum, Str(nDistanceAlongRaglan), "\n")
                ''---------------PrintFile(hCurve, sSide, "\n")
                PrintLine(fileNum, sSide, "\n")
                ''---------PrintFile(hCurve, "vest", "\n")
                PrintLine(fileNum, "vest", "\n")
                ''---------------SetData("UnitLinearType", 6)    ''// "Inches/Fraction"

                FileClose()
                bPrevAxillaWasMesh = True
            End If
            ''----------------SleeveLess Axilla ---------------
            If (sAxillaType.Equals("Sleeveless")) Then
                Dim nSLessFac, nError, nAxillaShoulderRad As Double
                xyRaglanNeck = xyBackNeckConstruct
                ''// Use 1/2 figured shoulder circumference 
                ''// NB use of 1/2 scale
                If (sSide.Equals("Left")) Then
                    nSLessFac = FN_Round(nLtSLessCir / 2)
                Else
                    nSLessFac = FN_Round(nRtSLessCir / 2)
                End If
                nSLessFac = nSLessFac - 0.5 ''// 16.Oct.97 
                If (BRAGiven) Then
                    aAngle = FN_CalcAngle(xyBreast, xyAxilla)
                    xyAxillaLow = xyBreast  ''// Initail setting	 	
                Else
                    aAngle = FN_CalcAngle(xyWaistOFF, xyAxilla)
                    xyAxillaLow = xyWaistOFF     ''// Initail setting	  		 	
                End If
                ''// Revise xyAxillaLow
                xyAxillaLow.Y = xyAxillaLow.Y + (System.Math.Tan(aAngle) * System.Math.Abs(((nLowSLine + xyO.X) - nSLessFac) - xyAxillaLow.X))
                xyAxillaLow.X = nLowSLine - nSLessFac + xyO.X

                ''// Subtract for scoop necks
                If (sNeckType.Equals("Scoop") Or sNeckType.Equals("Measured Scoop")) Then
                    nSWidth = nSWidth - 1
                End If

                ''// Minimum width 1.5"
                If (nSWidth < 1.5) Then
                    nSWidth = 1.5
                End If

                PR_MakeXY(xyTempSt, (nLowSLine + 0.125) + xyO.X, xyO.Y)
                PR_MakeXY(xyTempEnd, (nLowSLine + 0.125) + xyO.X, 100.0 + xyO.Y)
                nError = FN_CirLinInt(xyTempSt, xyTempEnd, xyRaglanNeck, nSWidth, xyInt)
                xySleeveLess = xyInt

                ''// Shorten to accomodate body width
                If (xySleeveLess.Y > xyAxillaLow.Y) Then
                    xySleeveLess.Y = xyAxillaLow.Y
                End If
                ''//   AddEntity ("marker", "xmarker", xySleeveLess , 0.25, 0.25, 0);
                ''//   AddEntity ("marker", "xmarker", xyAxillaLow , 0.25, 0.25, 0);

                nAxillaShoulderRad = FN_CalcLength(xyAxillaLow, xySleeveLess)  ''// V.I.P.
                aCurve = FN_CurveAngle(xyAxillaLow, nAxillaShoulderRad)
                aVestCurve = FN_CalcAngle(xyAxillaLow, xySleeveLess) * (180 / VESTDIA1.PI)  ''// V.I.P.	

                Dim fileNum As Object
                fileNum = FreeFile()
                ''---------hCurve = Open("file", g_PathJOBST + "\\TEMPLTS\\VESTCURV.DAT", "readonly")
                ''-------------GetLine(hCurve, & sLine)
                ''---------ScanLine(sLine, "blank", & nLength, & aAngle)
                FileOpen(fileNum, sSettingsPath + "\\VESTCURV.DAT", VB.OpenMode.Input)
                aPrevAngle = (aVestCurve - aCurve)
                xyPt1 = xyAxillaLow
                DrawSegment = True                  ''// Set draw segments flag on
                Dim xyPt2 As xy
                Dim sLine As String
                While Not EOF(fileNum)
                    sLine = LineInput(fileNum)
                    FN_GetNumber(sLine, nLength, aAngle)
                    aAngle = aAngle + aPrevAngle
                    ''-----xyPt2 = CalcXY("relpolar", xyPt1, nLength, aAngle)
                    PR_CalcPolar(xyPt1, nLength, aAngle, xyPt2)
                    If (FN_CirLinInt(xyPt1, xyPt2, xyAxillaLow, nAxillaShoulderRad, xyInt)) Then
                        ''-------AddEntity("line", xyPt1, xyInt)
                        PR_DrawLine(xyPt1, xyInt)
                        xySleeveLess = xyInt
                        Exit While
                    End If
                    If (DrawSegment) Then
                        ''-----------AddEntity("line", xyPt1, xyPt2)
                        PR_DrawLine(xyPt1, xyPt2)
                    End If
                    xyPt1 = xyPt2
                    aPrevAngle = aAngle
                    ''----------ScanLine(sLine, "blank", & nLength, & aAngle);
                End While

                ''-------AddEntity("line", xySleeveLess, xyRaglanNeck)
                PR_DrawLine(xySleeveLess, xyRaglanNeck)
                FileClose()
            End If
            '' Store xyRaglanNeck for use in arm transfer
            Dim xyRaglanNeckOriginal As xy
            xyRaglanNeckOriginal = xyRaglanNeck
            '' Draw Profile
            '' On seperate Left and Right Layers,  Draw EOS Left hand only
            If sSide.Equals("Left") Then
                If SleeveLess Then
                    xyRaglanNeck = xyFrontNeckOFF
                End If
                '' BackNeck
                '' Draw for measured scoops
                If cboBackNeck.Text.Equals("Measured Scoop") Then
                    nX = xyRaglanNeck.X - xyBackNeckCL.X
                    nY = xyRaglanNeck.Y - xyBackNeckCL.Y
                    '' Draw a fitted polyline using 1/3 and 1/2 rule
                    '------------hEnt = AddEntity("poly", "openfitted", xyBackNeckCL, xyBackNeckCL.X + nX / 3, xyBackNeckCL.Y + nY / 2, xyRaglanNeck)
                    Dim ptColl As Point3dCollection = New Point3dCollection()
                    ptColl.Add(New Point3d(xyBackNeckCL.X, xyBackNeckCL.Y, 0))
                    ptColl.Add(New Point3d(xyBackNeckCL.X + nX / 3, xyBackNeckCL.Y + nY / 2, 0))
                    ptColl.Add(New Point3d(xyRaglanNeck.X, xyRaglanNeck.Y, 0))
                    PR_DrawSpline(ptColl)
                    'SetDBData(hEnt, "Data", sID)
                    'SetDBData(hEnt, "curvetype", "vestbackneck")
                    MsgBox("The BACK Measured Scoop Neck has been drawn As a POLYLINE" & Chr(13) & "Edit this line And make it OPEN FITTED, this will Then be a smooth line" & Chr(13), 35, "Vest Body - Dialog")
                Else
                    '' Draw for Regular and Scoop back neck 
                    nLength = FN_CalcLength(xyBackNeckCL, xyRaglanNeck)
                    aAngle = FN_CalcAngle(xyBackNeckCL, xyRaglanNeck) ' * (180 / VESTDIA1.PI)
                    nLength = (nLength / 2) / System.Math.Cos(aAngle)
                    xyCen.X = xyBackNeckCL.X + nLength
                    xyCen.Y = xyO.Y
                    ''xyCen.Y = xyInsertion.Y

                    aAngle = 2 * aAngle * (180 / VESTDIA1.PI) '(180 - aAngle - 90) '* (180 / VESTDIA1.PI)
                    aPrevAngle = 180.0 '- aAngle ' * (180 / VESTDIA1.PI)
                    ''----------------hEnt = AddEntity("arc", xyCen, nLength, aPrevAngle, aAngle)
                    PR_DrawArc(xyCen, nLength, (aAngle * (VESTDIA1.PI / 180)), (aPrevAngle * (VESTDIA1.PI / 180)))
                    'SetDBData(hEnt, "Data", sID)
                    'SetDBData(hEnt, "curvetype", "vestbackneck")
                End If
                ''
                '' End back neck section
                ''------------StartPoly("polyline")
                nCount = 3
                If EOSGiven Then
                    nCount = 4
                End If
                If BRAGiven Then
                    nCount = nCount + 1
                End If
                Dim PlineStart(nCount), PlineEnd(nCount) As Double
                '' EOS
                If EOSGiven Then
                    ''-------------AddVertex(xyBackNeckCL)
                    ''-------------AddVertex(xyEOSCL)
                    ''---------------AddVertex(xyEOSOFF)
                    ''---------------AddVertex(xyWaistOFF)
                    PlineStart(1) = xyBackNeckCL.X
                    PlineEnd(1) = xyBackNeckCL.Y
                    PlineStart(2) = xyEOSCL.X
                    PlineEnd(2) = xyEOSCL.Y
                    PlineStart(3) = xyEOSOFF.X
                    PlineEnd(3) = xyEOSOFF.Y
                    PlineStart(4) = xyWaistOFF.X
                    PlineEnd(4) = xyWaistOFF.Y
                Else
                    ''-------------AddVertex(xyBackNeckCL)
                    ''--------------AddVertex(xyWaistCL)
                    ''-----------------AddVertex(xyWaistOFF)
                    PlineStart(1) = xyBackNeckCL.X
                    PlineEnd(1) = xyBackNeckCL.Y
                    PlineStart(2) = xyWaistCL.X
                    PlineEnd(2) = xyWaistCL.Y
                    PlineStart(3) = xyWaistOFF.X
                    PlineEnd(3) = xyWaistOFF.Y
                End If
                '' Under Breast to Waist
                If BRAGiven Then
                    ''------------------AddVertex(xyBreast)
                    PlineStart(nCount) = xyBreast.X
                    PlineEnd(nCount) = xyBreast.Y
                End If
                ''--------------------EndPoly()
                PR_DrawPoly(PlineStart, PlineEnd, nCount)
                ''---------------------hEnt = UID("find", UID("getmax"))
                ''-------------------SetDBData(hEnt, "Data", sID)
                ''-------------------SetDBData(hEnt, "curvetype", "vestprofile")

                '' Detachable turtlenecks
                '' Draw in box 3.0 x 3.0
                '--------------------Dim xyPt1 As xy
                ''--------------If (StringCompare("Turtle Detach", sNeckType, 13)) Then
                If sNeckType.Contains("Turtle Detach") Then
                    xyPt1.Y = xyAxilla.Y + 0.25
                    xyPt1.X = xyO.X
                    'xyPt1.X = xyInsertion.X
                    '        hEnt = AddEntity("poly", "polyline", xyPt1, xyPt1.X, xyPt1.Y + 3,
                    '                         xyPt1.X, xyPt1.Y + 3, xyPt1.X + 3, xyPt1.Y + 3,
                    'xyPt1.X + 3, xyPt1.Y + 3, xyPt1.X + 3, xyPt1.Y,
                    'xyPt1.X + 3, xyPt1.Y, xyPt1,
                    'xyPt1, xyPt1.X, xyPt1.Y + 3)
                    Dim PolyStart(10), PolyEnd(10) As Double
                    PolyStart(1) = xyPt1.X
                    PolyEnd(1) = xyPt1.Y
                    PolyStart(2) = xyPt1.X
                    PolyEnd(2) = xyPt1.Y + 3
                    PolyStart(3) = xyPt1.X
                    PolyEnd(3) = xyPt1.Y + 3
                    PolyStart(4) = xyPt1.X + 3
                    PolyEnd(4) = xyPt1.Y + 3
                    PolyStart(5) = xyPt1.X + 3
                    PolyEnd(5) = xyPt1.Y + 3

                    PolyStart(6) = xyPt1.X + 3
                    PolyEnd(6) = xyPt1.Y
                    PolyStart(7) = xyPt1.X + 3
                    PolyEnd(7) = xyPt1.Y
                    PolyStart(8) = xyPt1.X
                    PolyEnd(8) = xyPt1.Y
                    PolyStart(9) = xyPt1.X
                    PolyEnd(9) = xyPt1.Y
                    PolyStart(10) = xyPt1.X
                    PolyEnd(10) = xyPt1.Y + 3
                    PR_DrawPoly(PolyStart, PolyEnd, 10)
                    ''------------SetDBData(hEnt, "Data", sID)
                End If
            End If
            '' Draw Axilla dependant profiles
            '' Lowest axilla point to next lowest point
            If BRAGiven Then
                ''------------------hEnt = AddEntity("line", xyAxillaLow, xyBreast)
                PR_DrawLine(xyAxillaLow, xyBreast)
            Else
                ''--------------------hEnt = AddEntity("line", xyAxillaLow, xyWaistOFF)
                PR_DrawLine(xyAxillaLow, xyWaistOFF)
            End If
            ''    SetDBData(hEnt, "ID",sID);

            '' Axilla profile and axilla inserts
            ''
            '------------If (StringCompare("Regular", sAxillaType, 7)) Then
            If sAxillaType.Contains("Regular") Then
                '' Draw profile 
                ''---------------------hEnt = AddEntity("line", xyAxillaConstruct_2, xyRaglanAxilla) 
                PR_DrawLine(xyAxillaConstruct_2, xyRaglanAxilla)

                ''	SetDBData(hEnt, "Data",sID);
                aPrevAngle = FN_CalcAngle(xyAxilla, xyAxillaLow)
                aAngle = FN_CalcAngle(xyAxilla, xyAxillaConstruct_2) ' - aPrevAngle
                ''-----------------------hEnt = AddEntity("arc", xyAxilla, nRegAxillaFac_1, aPrevAngle, aAngle)
                PR_DrawArc(xyAxilla, nRegAxillaFac_1, aPrevAngle, aAngle)
                ''	SetDBData(hEnt, "Data",sID);

                '' Draw insert
                aPrevAngle = aPrevAngle * (180 / VESTDIA1.PI)
                ''--------------xyInsertConstruct_4 = CalcXY("relpolar", xyAxillaLow, System.Math.Sqrt(2 * (nSeamAllowance * nSeamAllowance)), aPrevAngle + 45)
                PR_CalcPolar(xyAxillaLow, System.Math.Sqrt(2 * (nSeamAllowance * nSeamAllowance)), aPrevAngle + 45, xyInsertConstruct_4)
                aPrevAngle = FN_CalcAngle(xyAxilla, xyInsertConstruct_4) ' * (180 / VESTDIA1.PI)
                aAngle = (FN_CalcAngle(xyAxilla, xyAxillaConstruct_2)) ' - aPrevAngle

                '-------------hEnt = AddEntity("arc", TransXY(xyAxilla, "trs", nXInsert, nYInsert), nRegAxillaFac_1 + nSeamAllowance, aPrevAngle, aAngle) 
                Dim xyCenter As xy
                PR_MakeXY(xyCenter, xyAxilla.X + nXInsert, xyAxilla.Y + nYInsert)
                PR_DrawArc(xyCenter, nRegAxillaFac_1 + nSeamAllowance, aPrevAngle, aAngle)
                ''	SetDBData(hEnt, "Data",sID);
                '----------------hEnt = AddEntity("line", TransXY(xyInsertConstruct_4, "trs", nXInsert, nYInsert), TransXY(xyInsertConstruct_3, "trs", nXInsert, nYInsert)) 
                PR_MakeXY(xyStart, xyInsertConstruct_4.X + nXInsert, xyInsertConstruct_4.Y + nYInsert)
                PR_MakeXY(xyEnd, xyInsertConstruct_3.X + nXInsert, xyInsertConstruct_3.Y + nYInsert)
                PR_DrawLine(xyStart, xyEnd)
                ''	SetDBData(hEnt, "Data",sID);
                '--------------------hEnt = AddEntity("line", TransXY(xyInsertConstruct_1, "trs", nXInsert, nYInsert), TransXY(xyInsertConstruct_2, "trs", nXInsert, nYInsert)) 
                PR_MakeXY(xyStart, xyInsertConstruct_1.X + nXInsert, xyInsertConstruct_1.Y + nYInsert)
                PR_MakeXY(xyEnd, xyInsertConstruct_2.X + nXInsert, xyInsertConstruct_2.Y + nYInsert)
                PR_DrawLine(xyStart, xyEnd)
                ''	SetDBData(hEnt, "Data",sID);
                '---------------------hEnt = AddEntity("line", TransXY(xyInsertConstruct_2, "trs", nXInsert, nYInsert), TransXY(xyRaglanAxilla, "trs", nXInsert, nYInsert)) 	
                PR_MakeXY(xyStart, xyInsertConstruct_2.X + nXInsert, xyInsertConstruct_2.Y + nYInsert)
                PR_MakeXY(xyEnd, xyRaglanAxilla.X + nXInsert, xyRaglanAxilla.Y + nYInsert)
                PR_DrawLine(xyStart, xyEnd)
                ''	SetDBData(hEnt, "Data",sID);

                '' Draw seam line and stamp 
                '' Note recalculation because of drawing on the notes layer
                ARMDIA1.PR_SetLayer("Construct")
                '-----------------------hEnt = AddEntity("line", TransXY(xyAxillaConstruct_2, "trs", nXInsert, nYInsert), TransXY(xyRaglanAxilla, "trs", nXInsert, nYInsert)) 
                PR_MakeXY(xyStart, xyAxillaConstruct_2.X + nXInsert, xyAxillaConstruct_2.Y + nYInsert)
                PR_MakeXY(xyEnd, xyRaglanAxilla.X + nXInsert, xyRaglanAxilla.Y + nYInsert)
                PR_DrawLine(xyStart, xyEnd)
                ''	SetDBData(hEnt, "Data",sID);
                aPrevAngle = FN_CalcAngle(xyAxilla, xyInsertConstruct_4)
                aAngle = FN_CalcAngle(xyAxilla, xyAxillaConstruct_2) ' - aPrevAngle
                ''------------------hEnt = AddEntity("arc", TransXY(xyAxilla, "trs", nXInsert, nYInsert), nRegAxillaFac_1, aPrevAngle, aAngle)
                PR_MakeXY(xyCenter, xyAxilla.X + nXInsert, xyAxilla.Y + nYInsert)
                PR_DrawArc(xyCenter, nRegAxillaFac_1, aPrevAngle, aAngle)
                ''	SetDBData(hEnt, "Data",sID);
                Dim nAType As Double
                If sSide.Equals("Left") Then
                    nAType = 1
                Else
                    nAType = 2
                End If
                If nInitial_nAxilla = 1 Then
                    nAType = 0
                End If
                ''------------PR_DataStamp(TransXY(xyAxilla.X - 1, xyAxilla.Y - 1, "trs", nXInsert, nYInsert), 10 + nAType)
                PR_MakeXY(xyCenter, xyAxilla.X - 1 + nXInsert, xyAxilla.Y - 1 + nYInsert)
                PR_DataStamp(xyCenter, 10 + nAType)
            End If
            '' Restore original raglan neck 
            '' Edits w.r.t sleeveless vest cause this
            xyRaglanNeck = xyRaglanNeckOriginal

            Dim nAxillaBackNeckRad, nShoulderToBackRaglan As Double
            '' Update bodybox with data for drawing of sleeves
            If sAxillaType.Equals("Sleeveless") = False Then
                nAxillaBackNeckRad = FN_CalcLength(xyAxilla, xyRaglanNeck)
                nShoulderToBackRaglan = FN_CalcLength(xyRaglanNeck, xyBackNeckConstruct)
                If sSide.Equals("Right") And sHighestAxilla.Equals("None") = False Then
                    '' These are for the RIGHT axilla data transfer
                    '--------SetDBData(hBody, "AFNRadRight", MakeString("scalar", nAxillaFrontNeckRad))
                    ''---------------------SetDBData(hBody, "ABNRadRight", MakeString("scalar", nAxillaBackNeckRad))
                    ''------------------------SetDBData(hBody, "SBRaglanRight", MakeString("scalar", nShoulderToBackRaglan))
                    g_sSide = "Right"
                Else
                    '' These are for the LEFT or BOTH  axilla data transfer
                    ''-------------------SetDBData(hBody, "AxillaFrontNeckRad", MakeString("scalar", nAxillaFrontNeckRad))
                    ''----------------SetDBData(hBody, "AxillaBackNeckRad", MakeString("scalar", nAxillaBackNeckRad))
                    ''-------------------SetDBData(hBody, "ShoulderToBackRaglan", MakeString("scalar", nShoulderToBackRaglan))
                    g_sSide = "Left"
                End If
                g_nAxillaBackNeckRad = nAxillaBackNeckRad
                g_nAxillaFrontNeckRad = nAxillaFrontNeckRad
                g_nShoulderToBackRaglan = nShoulderToBackRaglan
            End If
            '' loop to next Axilla
            nAxilla = nAxilla - 1
        End While ''End Figure both axilla while loop

        '' Set up notes layer
        ARMDIA1.PR_SetLayer("Notes")

        '' Bra Cups
        '' DRAW_2.D created by visual basic
        Dim nBraFac_1 As Double = 0.25 + 0.5
        Dim nBraFac_SleeveLess As Double = 0.75 + 0.5
        Dim jj, nDiskLt, nDiskRt, nDisk, nBraCLOffsetLt, nBraCLOffsetRt, nBraCLOffset As Double
        Dim nDiskXoffLt, nDiskXoffRt, nDiskYoffLt, nDiskYoffRt, nDiskXoff, nDiskYoff As Double
        Dim sTmp, sDisk As String
        Dim xyDisk As xy
        nDiskLt = 1
        nDiskRt = 1
        nBraCLOffsetLt = 1.25
        nDiskXoffLt = 1.45
        nDiskYoffLt = 1.797
        nBraCLOffsetRt = 1.25
        nDiskXoffRt = 1.45
        nDiskYoffRt = 1.797
        Dim xyText, xyLnStart, xyLnEnd As xy
        If BRAGiven Then
            '-------------@C\JOBST\DRAW_2.D
            ''-----------------If (StringCompare(sLtAxillaType, sRtAxillaType) && StringCompare(sLtAxillaType, "Sleeveless")) Then
            If cboLeftAxilla.Text.Equals(cboRightAxilla.Text) And cboLeftAxilla.Text.Equals("Sleeveless") Then
                nBraFac_1 = nBraFac_SleeveLess
            End If
            ''---------------------------SetSymbolLibrary(VESTDIA1.g_PathJOBST + "\\JOBST.SLB")
            ''----------------If (Symbol("find", "bradisk1")) Then
            If PR_FindBlockExist("bradisk1") Then
                If nDiskLt = nDiskRt Then
                    jj = 1
                Else
                    jj = 2
                End If
                ''-----------------------------SetData("TextFont", 0)      ''//CAD Block
                ''----------------------SetData("TextAspect", 0.6)
                ''-----------------SetData("TextVertJust", 16)     ''// Center
                ''---------------------SetData("TextHorzJust", 2)      ''// Center
                ''--------------------------SetData("TextHeight", 0.125)
                While jj > 0
                    If jj = 2 Then
                        nDisk = nDiskLt
                        nBraCLOffset = nBraCLOffsetLt
                        nDiskXoff = nDiskXoffLt
                        nDiskYoff = nDiskYoffLt
                        sTmp = " (Left)"
                    End If
                    If jj = 1 Then
                        nDisk = nDiskRt
                        sTmp = " "
                        nBraCLOffset = nBraCLOffsetRt
                        nDiskXoff = nDiskXoffRt
                        nDiskYoff = nDiskYoffRt
                    End If
                    If nDisk > 0 Then
                        ''-----------------sDisk = "bradisk" + MakeString("Long", nDisk)
                        sDisk = "bradisk" + Str(nDisk)
                        ''------------If (Symbol("find", sDisk)) Then
                        If PR_FindBlockExist(sDisk) Then
                            xyDisk.X = nBraAxillaHt - nDiskXoff - nBraFac_1
                            xyDisk.Y = xyO.Y + nBraCLOffset
                            ''---------------Import("symbol", sDisk)
                            ''--------------------AddEntity("symbol", sDisk, xyDisk)
                        End If
                        '----------------AddEntity("text", "No. " + MakeString("Long", nDisk) + sTmp
                        '  ,nBraAxillaHt - nDiskXoff  - nBraFac_1
                        ',xyO.y + nBraCLOffset + nDiskYoff ) 	
                        PR_MakeXY(xyText, nBraAxillaHt - nDiskXoff - nBraFac_1, xyO.Y + nBraCLOffset + nDiskYoff)
                        PR_DrawText("No. " + Str(nDisk) + sTmp, xyText, 0.125, 0, 0)
                        ARMDIA1.PR_SetLayer("Construct")
                        '-------------------------------AddEntity("line", nBraAxillaHt
                        ',xyO.y + nBraCLOffset
                        ',nBraAxillaHt - (nDiskXoff  - nBraFac_1)*2
                        ',xyO.y + nBraCLOffset) 
                        PR_MakeXY(xyLnStart, nBraAxillaHt, xyO.Y + nBraCLOffset)
                        PR_MakeXY(xyLnEnd, nBraAxillaHt - (nDiskXoff - nBraFac_1) * 2, xyO.Y + nBraCLOffset)
                        PR_DrawLine(xyLnStart, xyLnEnd)
                        ''-----------------------------AddEntity("line", nBraAxillaHt - nBraFac_1
                        ',xyO.y + nBraCLOffset
                        ',nBraAxillaHt  - nBraFac_1
                        ',xyO.y + nBraCLOffset + (nDiskYoff) * 2 )
                        PR_MakeXY(xyLnStart, nBraAxillaHt - nBraFac_1, xyO.Y + nBraCLOffset)
                        PR_MakeXY(xyLnEnd, nBraAxillaHt - nBraFac_1, xyO.Y + nBraCLOffset + (nDiskYoff) * 2)
                        PR_DrawLine(xyLnStart, xyLnEnd)
                        ARMDIA1.PR_SetLayer("Notes")
                    End If
                    If nDisk < 0 Then
                        If jj = 2 Then
                            ''--------------------- AddEntity("text", "No Cup - Left"
                            '  ,nBraAxillaHt - 1.39   - nBraFac_1
                            ',xyO.y + 1.75  )
                            PR_MakeXY(xyText, nBraAxillaHt - 1.39 - nBraFac_1, xyO.Y + 1.75)
                            PR_DrawText("No Cup - Left", xyText, 0.125, 0, 0)
                        Else
                            ''---------------------------- AddEntity("text", "No Cup - Right"
                            '  ,nBraAxillaHt - 1.39  - nBraFac_1
                            ',xyO.y + 1.75 + 0.5 ) 
                            PR_MakeXY(xyText, nBraAxillaHt - 1.39 - nBraFac_1, xyO.Y + 1.75 + 0.5)
                            PR_DrawText("No Cup - Right", xyText, 0.125, 0, 0)
                        End If
                    End If
                    jj = jj - 1
                End While ''// Endwhile
            Else
                ''---------Exit(%cancel, "Can't find BRADISK symbols to insert\nCheck your installation, that JOBST.SLB exists")
                MsgBox("Can't find BRADISK symbols to insert" & Chr(13) & "Check your installation, that JOBST.SLB exists", 16, "Vest Body Dialog")
                Exit Sub
            End If

        End If
        '' If one side is sleevles then lable the other side
        ''------------------------------------- Dim xyPt1 As xy
        If SleeveLess Then
            ''---------------xyPt1 = CalcXY("relpolar", xyAxilla, FN_CalcLength(xyAxilla, xyBackNeckCL) / 2, FN_CalcAngle(xyAxilla, xyBackNeckCL))
            PR_CalcPolar(xyAxilla, FN_CalcLength(xyAxilla, xyBackNeckCL) / 2, (FN_CalcAngle(xyAxilla, xyBackNeckCL) * (180 / VESTDIA1.PI)), xyPt1)
            If cboRightAxilla.Text.Equals("Sleeveless") = False Then
                ''------------------AddEntity("text", "Right", xyPt1)
                PR_DrawText("Right", xyPt1, 0.125, 0, 0)
            End If
            If cboLeftAxilla.Text.Equals("Sleeveless") = False Then
                ''------------AddEntity("text", "Left", xyPt1)
                PR_DrawText("Left", xyPt1, 0.125, 0, 0)
            End If
        End If
        '' Front neck
        nNeckDimension = nNeckDimension * VESTDIA1.g_nUnitsFac
        If sNeckType.Equals("Measured Scoop") Then
            nX = xyFrontNeckOFF.X - xyFrontNeckCL.X + nNeckDimension
            nY = xyFrontNeckOFF.Y - xyFrontNeckCL.Y
            '' Draw a fitted polyline using 1/3 and 2/3 rule
            ''---------------          hEnt = AddEntity("poly", "openfitted", xyFrontNeckCL.x - nNeckDimension, xyFrontNeckCL.y
            ', xyFrontNeckCL.x - nNeckDimension + nX / 3,  xyFrontNeckCL.y + nY*2/3
            ', xyFrontNeckOFF) 
            Dim ptColl As Point3dCollection = New Point3dCollection()
            ptColl.Add(New Point3d(xyFrontNeckCL.X - nNeckDimension, xyFrontNeckCL.Y, 0))
            ptColl.Add(New Point3d(xyFrontNeckCL.X - nNeckDimension + nX / 3, xyFrontNeckCL.Y + nY * 2 / 3, 0))
            ptColl.Add(New Point3d(xyFrontNeckOFF.X, xyFrontNeckOFF.Y, 0))
            PR_DrawSpline(ptColl)
            '---------------------SetDBData(hEnt, "Data", sID)
            '---------------------------SetDBData(hEnt, "curvetype", "vestfrontneck")
            MsgBox("The FRONT Measured Scoop Neck has been drawn as a POLYLINE" & Chr(13) & "Edit this line and make it OPEN FITTED, this will then be a smooth line", 0, "Vest Body - Dialog")
        Else
            '' Revise xyFrontNeckCen	
            ''   	nLength =  Calc("length", xyFrontNeckCL, xyFrontNeckOFF) / 2;
            ''  	aAngle = Calc("angle", xyFrontNeckCL, xyFrontNeckOFF);
            ''	nLength = nLength /  cos(aAngle) ;
            '' 	xyFrontNeckCen.X = xyFrontNeckCL.X + nLength ; 
            '' 	xyFrontNeckCen.Y = xyO.Y ;
            nLength = FN_CalcLength(xyFrontNeckCL, xyFrontNeckOFF) / 2
            aAngle = FN_CalcAngle(xyFrontNeckCL, xyFrontNeckOFF) * (180 / VESTDIA1.PI)
            ''--------------xyFrontNeckCen = CalcXY("relpolar", xyFrontNeckCL, nLength, aAngle)
            PR_CalcPolar(xyFrontNeckCL, nLength, aAngle, xyFrontNeckCen)
            aAngle = aAngle - 90
            nNeckFac_5 = 1.3
            nLength = System.Math.Sqrt(((nNeckActualCir * nNeckFac_5) * (nNeckActualCir * nNeckFac_5)) - (nLength * nLength))
            ''--------------------xyFrontNeckCen = CalcXY("relpolar", xyFrontNeckCen, nLength, aAngle)
            PR_CalcPolar(xyFrontNeckCen, nLength, aAngle, xyFrontNeckCen)
            aPrevAngle = FN_CalcAngle(xyFrontNeckCen, xyFrontNeckOFF)
            aAngle = FN_CalcAngle(xyFrontNeckCen, xyFrontNeckCL) ' - aPrevAngle
            '--------------hEnt = AddEntity("arc", xyFrontNeckCen, nNeckActualCir * nNeckFac_5, aPrevAngle, aAngle)
            PR_DrawArc(xyFrontNeckCen, nNeckActualCir * nNeckFac_5, aPrevAngle, aAngle)
            '---------------SetDBData(hEnt, "Data", sID)
            '---------------SetDBData(hEnt, "curvetype", "vestfrontneck")
        End If
        '' Neck Notes
        ''-----------------------SetData("TextVertJust", 32)
        '-----------------If (StringCompare("Turtle Detach", sNeckType, 13)) Then '/* ie Detachable*/
        Dim xyTextInsert As xy
        If sNeckType.Contains("Turtle Detach") Then '/* ie Detachable*/
            '----------SetData("TextHorzJust", 1)
            xyPt1.X = xyO.X + 0.25
            xyPt1.Y = xyAxilla.Y + 0.1
            ''--------------------AddEntity("text", sNeckNotes, xyPt1.X, xyPt1.Y + 0.4)
            PR_MakeXY(xyText, xyPt1.X, xyPt1.Y + 0.5)
            ''PR_DrawText(sNeckNotes, xyText, 0.125, 0, 0)
            PR_DrawMText(sNeckNotes, xyText, False)
            ''---------------------------AddEntity("text", sPatient, xyPt1.X, xyPt1.Y + 1.0)
            PR_MakeXY(xyText, xyPt1.X, xyPt1.Y + 1)
            PR_DrawText(txtPatientName.Text, xyText, 0.125, 0, 0)
            ''----------------AddEntity("text", sWorkOrder, xyPt1.X, xyPt1.Y + 0.8)
            PR_MakeXY(xyText, xyPt1.X, xyPt1.Y + 0.8)
            PR_DrawText(txtWorkOrder.Text, xyText, 0.125, 0, 0)
            ''------------------AddEntity("text", sAge + " yrs", xyPt1.X, xyPt1.Y + 0.6)
            PR_MakeXY(xyText, xyPt1.X, xyPt1.Y + 0.6)
            PR_DrawText(txtAge.Text, xyText, 0.125, 0, 0)
            ''--------------------------SetData("TextHorzJust", 4)
            aAngle = 180 - (FN_CalcAngle(xyFrontNeckCen, xyFrontNeckOFF) * (180 / VESTDIA1.PI))
            ''---------------------------AddEntity("text", sNeckNotes, CalcXY("relpolar", xyFrontNeckCen, nNeckActualCir * nNeckFac_5 + 0.25, (180 - aAngle / 2)))
            PR_CalcPolar(xyFrontNeckCen, nNeckActualCir * nNeckFac_5 + 0.25, (180 - aAngle / 2), xyTextInsert)
            ''PR_DrawText(sNeckNotes, xyTextInsert, 0.125, 0, 0)
            PR_DrawMText(sNeckNotes, xyTextInsert, True)
        Else
            ''----------------------SetData("TextHorzJust", 4)
            aAngle = 180 - (FN_CalcAngle(xyFrontNeckCen, xyFrontNeckOFF) * (180 / VESTDIA1.PI))
            ''----------------------------AddEntity("text", sNeckNotes + "     ", CalcXY("relpolar", xyFrontNeckCen, nNeckActualCir * nNeckFac_5, (180 - aAngle / 2)))
            PR_CalcPolar(xyFrontNeckCen, nNeckActualCir * nNeckFac_5, (180 - aAngle / 2), xyTextInsert)
            PR_DrawText(sNeckNotes, xyTextInsert, 0.125, 0, 2)
        End If

        '' Closures
        ''-------------SetData("TextHorzJust", 2)

        '' Closure length factors
        ''sClosure = cboClosure.Text
        Dim nClosureAllowance As Double
        If sClosure.Equals("Velcro") Or sClosure.Equals("Front Velcro") Or sClosure.Equals("Front Velcro (Reversed)") Or sClosure.Equals("Back Velcro") Or sClosure.Equals("Back Velcro (Reversed)") Then
            nClosureAllowance = 0.375   '' Velcro
        Else
            nClosureAllowance = 0.125   '' Zippers only
        End If

        ''------------If (StringCompare("Front", sClosure, 5)) Then
        If sClosure.Contains("Front") Then
            If sNeckType.Equals("Measured Scoop") Then
                nLength = xyFrontNeckCL.X - xyO.X - nClosureAllowance - nNeckDimension
            Else
                nLength = xyFrontNeckCL.X - xyO.X - nClosureAllowance
            End If
        Else
            nLength = xyBackNeckCL.X - xyO.X - nClosureAllowance
        End If
        Dim strLength As String = ""
        nLength = FN_Round(nLength)
        ''  Compensate for turtle necks
        Dim nZipFac As Double = 0.95
        ''-------------If (StringCompare("Turtle", sNeckType, 6)) Then
        If sNeckType.Contains("Turtle") Then
            ''----------------If (StringCompare("Turtle Detach", sNeckType, 13)) Then
            If sNeckType.Contains("Turtle Detach") Then
                '' Detachable necks
                strLength = VESTDIA1.fnInchestoText(VESTDIA1.fnRoundInches(nLength / nZipFac))
                nLength = FN_Round(nLength / nZipFac)
            Else
                ''Ordinary and Fabric necks
                strLength = VESTDIA1.fnInchestoText(VESTDIA1.fnRoundInches((nLength + nTurtleWidth) / nZipFac))
                nLength = FN_Round((nLength + nTurtleWidth) / nZipFac)
            End If
        Else
            strLength = VESTDIA1.fnInchestoText(VESTDIA1.fnRoundInches(nLength / nZipFac))
            nLength = FN_Round(nLength / nZipFac)
        End If

        '--------------AddEntity("text",
        '  sClosure + ", " + Format("length", nLength),
        '  xyO.x + nLength / 3,
        '  xyO.y + 0.5)
        Dim xyTemp As xy
        PR_MakeXY(xyTemp, xyO.X + nLength / 3, xyO.Y + 0.5)
        ''Changed for #176 in the issue list on 25th June 2019
        ''PR_DrawText(sClosure & ", " & strLength & Chr(34), xyTemp, 0.125, 0, 1)
        PR_DrawText(sClosure, xyTemp, 0.125, 0, 1)

        '' Add Stamps at waist
        'xyWaistOFF.X = xyWaistOFF.X + 2
        'xyWaistOFF.Y = xyWaistOFF.Y - 2
        Dim xyDataStamp As xy
        PR_MakeXY(xyDataStamp, xyWaistOFF.X + 2, xyWaistOFF.Y - 2)
        PR_DataStamp(xyDataStamp, 1)  '' Add a body stamp

        '' Draw construction lines for checking purpose
        ARMDIA1.PR_SetLayer("Construct")
        ''---------------AddEntity("line", nHighSLine + xyO.x, xyO.y, nHighSLine + xyO.x, xyAxilla.Y)
        Dim xyLineSt, xyLineEnd As xy
        PR_MakeXY(xyLineSt, nHighSLine + xyO.X, xyO.Y)
        PR_MakeXY(xyLineEnd, nHighSLine + xyO.X, xyAxilla.Y)
        PR_DrawLine(xyLineSt, xyLineEnd)
        If BRAGiven Then
            ''----------------AddEntity("line", xyBreast, xyBreast.X, xyO.y)
            PR_MakeXY(xyLineEnd, xyBreast.X, xyO.Y)
            PR_DrawLine(xyBreast, xyLineEnd)
        End If
        If EOSGiven Then
            ''--------------AddEntity("line", xyEOSOFF, xyEOSOFF.X, xyAxilla.Y)
            PR_MakeXY(xyLineEnd, xyEOSOFF.X, xyAxilla.Y)
            PR_DrawLine(xyEOSOFF, xyLineEnd)
        End If
        ''-------------------------hEnt = AddEntity("line", nLowSLine + xyO.x, xyO.y, nLowSLine + xyO.x, xyAxilla.Y)       ''SetDBData(hEnt, "ID",sID);
        PR_MakeXY(xyLineSt, nLowSLine + xyO.X, xyO.Y)
        PR_MakeXY(xyLineEnd, nLowSLine + xyO.X, xyAxilla.Y)
        PR_DrawLine(xyLineSt, xyLineEnd)
        ''------------------------hEnt = AddEntity("line", xyWaistOFF, xyWaistOFF.X, xyAxilla.Y)              ''SetDBData(hEnt, "ID",sID);
        PR_MakeXY(xyLineEnd, xyWaistOFF.X, xyWaistOFF.Y + 0.625)
        PR_DrawLine(xyWaistOFF, xyLineEnd)
        ''----------------hEnt = AddEntity("marker", "xmarker", xyBackNeckConstruct, 0.25, 0.25, 0)
        PR_DrawXMarker(xyBackNeckConstruct, 0.1875)
        ''------------------SetDBData(hEnt, "Data", sID)
        ''-------------------SetDBData(hEnt, "curvetype", "vestbackneckconstruct")

        '' Use vestbody to carry the sID to the sleeve drawing
        ''------------------SetDBData(hBody, "ID", sID)

        '' Fix required if Vest details change
        ''If (StringCompare("None", sHighestAxilla)) Then
        If sHighestAxilla.Equals("None") Then
            ''-----------------SetDBData(hBody, "AFNRadRight", "")
            ''----------------------SetDBData(hBody, "ABNRadRight", "")
            ''------------------------SetDBData(hBody, "SBRaglanRight", "")
            ''---------------hEnt = AddEntity("line", xyAxilla, xyAxilla.X, xyO.y)
            PR_MakeXY(xyLineEnd, xyAxilla.X, xyO.Y)
            PR_DrawLine(xyAxilla, xyLineEnd)
            ''--------------------SetDBData(hEnt, "Data", sID)
            ''--------------------SetDBData(hEnt, "curvetype", "vest" + sSide + "axillaconstruct")

        Else
            ''----------------hEnt = AddEntity("line", xyHighestAxilla, xyHighestAxilla.x, xyO.y)
            PR_MakeXY(xyLineEnd, xyHighestAxilla.X, xyO.Y)
            PR_DrawLine(xyHighestAxilla, xyLineEnd)
            ''--------------------SetDBData(hEnt, "Data", sID)
            ''------------------------SetDBData(hEnt, "curvetype", "vest" + sHighestAxilla + "axillaconstruct")
            ''--------------------------hEnt = AddEntity("line", xyLowestAxilla, xyLowestAxilla.x, xyO.y)
            PR_MakeXY(xyLineEnd, xyLowestAxilla.X, xyO.Y)
            PR_DrawLine(xyLowestAxilla, xyLineEnd)
            ''--------------------SetDBData(hEnt, "Data", sID)
            ''---------------------If (StringCompare(sHighestAxilla, "Left")) Then
            If sHighestAxilla.Equals("Left") Then
                ''---------------------SetDBData(hEnt, "curvetype", "vestRightaxillaconstruct")
            Else
                ''-----------------------SetDBData(hEnt, "curvetype", "vestLeftaxillaconstruct")
            End If
            If SleeveLess = False Then
                '        Display("message", "OKquestion",
                '"WARNING!\n" +
                '"You might need to transfer the lowest axilla front neck\n" +
                '"to the sleeve raglan.\n" +
                '"Use the \"Vest To Slv.\" tool\n " )
                MsgBox("WARNING!" & Chr(13) & "You might need to transfer the lowest axilla front neck" & Chr(13) & "to the sleeve raglan." & Chr(13) &
                        "Use the" & Chr(34) & "Vest To Slv." & Chr(34) & "tool" & Chr(13), 16, "Vest Body Dialog")
            End If
        End If

        '' Do Mesh axilla.
        '' The axilla MACROS are set by the visual basic programme.
        '' I.E. DRAW_1.D points to the relevant MACRO/S

        ''---------------------------------------
        'If (bPrevAxillaWasMesh) Then
        '    '-----------------@C:\JOBST\DRAW_3.D
        'End If
        ''----------------------------

        '' End of BODY.D, Clean up and close down
        Dim xyBase As xy
        PR_DrawXMarker(xyBase, 0.0625)
    End Sub
    'Function to return the angle between two points in degrees
    'in the range 0 - 360
    'Zero is always 0 and is never 360
    Private Function FN_CalcAngle(ByRef xyStart As xy, ByRef xyEnd As xy) As Double
        Dim X, Y As Object
        Dim rAngle As Double
        Dim PI As Double
        PI = 3.141592654

        X = xyEnd.X - xyStart.X
        Y = xyEnd.Y - xyStart.Y

        'Horizomtal
        If X = 0 Then
            If Y > 0 Then
                FN_CalcAngle = 90 * (PI / 180)
            Else
                FN_CalcAngle = 270 * (PI / 180)
            End If
            Exit Function
        End If

        'Vertical (avoid divide by zero later)
        If Y = 0 Then
            If X > 0 Then
                FN_CalcAngle = 0 * (PI / 180)
            Else
                FN_CalcAngle = 180 * (PI / 180)
            End If
            Exit Function
        End If

        'All other cases
        'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rAngle = System.Math.Atan(Y / X) * (180 / PI) 'Convert to degrees

        If rAngle < 0 Then rAngle = rAngle + 180 'rAngle range is -PI/2 & +PI/2

        If Y > 0 Then
            FN_CalcAngle = rAngle
        Else
            FN_CalcAngle = rAngle + 180
        End If
        FN_CalcAngle = FN_CalcAngle * (PI / 180)
    End Function
    'Function to calculate the intersection between a line and a circle.
    'Note:-
    '    Returns true if intersection found.
    '    The first intersection (only) is found.
    '    Ported from DRAFIX CAD DLG version.
    Function FN_CirLinInt(ByRef xyStart As xy, ByRef xyEnd As xy, ByRef xyCen As xy, ByRef nRad As Double, ByRef xyInt As xy) As Short
        Static nM, nC, nA, nSlope, nB, nK, nCalcTmp As Object
        Static nRoot As Double
        Static nSign As Short

        Dim PI As Double = 3.141592654
        nSlope = FN_CalcAngle(xyStart, xyEnd) * (180 / PI)

        'Horizontal Line
        If nSlope = 0 Or nSlope = 180 Then
            nSlope = -1
            nC = nRad ^ 2 - (xyStart.Y - xyCen.Y) ^ 2
            If nC < 0 Then
                FN_CirLinInt = False 'no roots
                Exit Function
            End If
            nSign = 1 'test each root
            While nSign > -2
                nRoot = xyCen.X + System.Math.Sqrt(nC) * nSign
                If nRoot >= MANGLOVE1.min(xyStart.X, xyEnd.X) And nRoot <= max(xyStart.X, xyEnd.X) Then
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
        If nSlope = 90 Or nSlope = 270 Then
            nSlope = -1
            nC = nRad ^ 2 - (xyStart.X - xyCen.X) ^ 2
            If nC < 0 Then
                FN_CirLinInt = False 'no roots
                Exit Function
            End If
            nSign = 1 'test each root
            While nSign > -2
                nRoot = xyCen.Y + System.Math.Sqrt(nC) * nSign
                If nRoot >= MANGLOVE1.min(xyStart.Y, xyEnd.Y) And nRoot <= max(xyStart.Y, xyEnd.Y) Then
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
        If nSlope > 0 Then
            nM = (xyEnd.Y - xyStart.Y) / (xyEnd.X - xyStart.X) 'Slope
            nK = xyStart.Y - nM * xyStart.X 'Y-Axis intercept
            nA = (1 + nM ^ 2)
            nB = 2 * (-xyCen.X + (nM * nK) - (xyCen.Y * nM))
            nC = (xyCen.X ^ 2) + (nK ^ 2) + (xyCen.Y ^ 2) - (2 * xyCen.Y * nK) - (nRad ^ 2)
            nCalcTmp = (nB ^ 2) - (4 * nC * nA)

            If nCalcTmp < 0 Then
                FN_CirLinInt = False 'No Roots
                Exit Function
            End If
            nSign = 1
            While nSign > -2
                nRoot = (-nB + (System.Math.Sqrt(nCalcTmp) / nSign)) / (2 * nA)
                If nRoot >= MANGLOVE1.min(xyStart.X, xyEnd.X) And nRoot <= max(xyStart.X, xyEnd.X) Then
                    xyInt.X = nRoot
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
    Private Sub PR_MakeXY(ByRef xyReturn As xy, ByRef X As Double, ByRef Y As Double)
        'Utility to return a point based on the X and Y values given
        xyReturn.X = X
        xyReturn.Y = Y
    End Sub
    Function FN_CalcLength(ByRef xyStart As xy, ByRef xyEnd As xy) As Double
        'Fuction to return the length between two points
        'Greatfull thanks to Pythagorus
        FN_CalcLength = System.Math.Sqrt((xyEnd.X - xyStart.X) ^ 2 + (xyEnd.Y - xyStart.Y) ^ 2)
    End Function
    Sub PR_DrawPoly(ByRef ChinStrapX As Double(), ByRef ChinStrapY As Double(), ByRef nCount As Short)
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
        'If Profile.n <= 1 Then Exit Sub

        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "hEnt = AddEntity(" & QQ & "poly" & QCQ & "polyline" & QQ)
        'For ii = 1 To Profile.n
        '    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '    PrintLine(fNum, CC & "xyStart.x+" & Str(Profile.X(ii)) & CC & "xyStart.y+" & Str(Profile.y(ii)))
        'Next ii
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, ");")
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout),
                                            OpenMode.ForWrite)

            '' Create a polyline with two segments (3 points)
            Using acPoly As Polyline = New Polyline()
                For ii = 1 To nCount
                    acPoly.AddVertexAt(ii - 1, New Point2d(xyInsertion.X + ChinStrapX(ii), xyInsertion.Y + ChinStrapY(ii)), 0, 0, 0)
                Next ii
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acPoly.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.Y, 0)))
                End If

                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acPoly)
                acTrans.AddNewlyCreatedDBObject(acPoly, True)
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using

    End Sub
    Private Sub PR_DrawLine(ByRef xyStart As xy, ByRef xyFinish As xy)
        'To the DRAFIX macro file (given by the global txtfNum).
        'Write the syntax to draw a LINE between two points.
        'For this to work it assumes that the following DRAFIX variables
        'are defined
        '    HEADNECK1.XY      xyStart
        '    HANDLE  hEnt
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)
            ''Create a Line object
            'Dim acLine As Line = New Line(New Point3d(xyStart.X, xyStart.Y, 0), New Point3d(xyFinish.X, xyFinish.Y, 0))
            Dim acLine As Line = New Line(New Point3d(xyStart.X + xyInsertion.X, xyStart.Y + xyInsertion.Y, 0), New Point3d(xyFinish.X + xyInsertion.X, xyFinish.Y + xyInsertion.Y, 0))
            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                acLine.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.Y, 0)))
            End If

            '' Add the new object to the block table record and the transaction
            acBlkTblRec.AppendEntity(acLine)
            acTrans.AddNewlyCreatedDBObject(acLine, True)
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawArc(ByRef xyCen As xy, ByRef nRad As Double, ByRef nStartAng As Double, ByRef nEndAng As Double)

        ' this procedure draws an arc between two points

        Dim nDeltaAng As Object


        'UPGRADE_WARNING: Couldn't resolve default property of object nRad. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '------------------nRad = FN_CalcLength(xyCen, xyArcStart)
        'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '-------------------nStartAng = FN_CalcAngle(xyCen, xyArcStart)
        'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '----------------nEndAng = FN_CalcAngle(xyCen, xyArcEnd)
        'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nDeltaAng = nEndAng - nStartAng
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout),
                                        OpenMode.ForWrite)

            '' Create an arc that is at 6.25,9.125 with a radius of 6, and
            '' starts at 64 degrees and ends at 204 degrees
            Using acArc As Arc = New Arc(New Point3d(xyInsertion.X + xyCen.X, xyInsertion.Y + xyCen.Y, 0),
                                         nRad, nStartAng, nEndAng)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acArc.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.Y, 0)))
                End If

                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acArc)
                acTrans.AddNewlyCreatedDBObject(acArc, True)
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using

        'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nRad. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'

    End Sub
    Private Sub PR_DrawText(ByRef sText As Object, ByRef xyInsert As xy, ByRef nHeight As Object, ByRef nAngle As Object, ByVal nTextmode As Double)
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)

            '' Create a single-line text object
            Using acText As DBText = New DBText()
                acText.Position = New Point3d(xyInsert.X + xyInsertion.X, xyInsert.Y + xyInsertion.Y, 0)
                acText.Height = nHeight
                acText.TextString = sText
                acText.Rotation = nAngle
                acText.HorizontalMode = nTextmode
                If acText.HorizontalMode <> TextHorizontalMode.TextLeft Then
                    acText.AlignmentPoint = New Point3d(xyInsert.X + xyInsertion.X, xyInsert.Y + xyInsertion.Y, 0)
                End If
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acText.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.Y, 0)))
                End If
                acBlkTblRec.AppendEntity(acText)
                acTrans.AddNewlyCreatedDBObject(acText, True)
            End Using

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using
    End Sub

    '    Public Sub FN_CirLinInt()
    '        'Note - Returns only the First intersection
    '        Dim nSlope, nSign, nC As Double
    '        nSlope = Calc("angle", %1, %2, %3, %4)

    '        ' Horizontal Line
    '        If (nSlope = 0 Or nSlope = 180) Then
    '            nSlope = -1
    '            nC = %7^2 - (%2-%6)^2
    '   	If (nC < 0) Then
    '                Return         '' No Roots
    '            End If
    '            nSign = 1           ' Test Each root
    '            While (nSign > -2)
    '                nRoot = %5 + sqrt(nC) * nSign;		
    '  		If (nRoot >= min(%1,%3) && nRoot <= max(%1,%3)) Then
    '                    xyInt.x = nRoot;
    '			xyInt.y = %2;
    '			Return (%true);
    '			End If
    '                nSign = nSign - 2;
    '		End While
    '            Return
    '        End If

    '// Vertical line
    '   If (nSlope == 90 || nSlope == 270 ) {
    '	nSlope = -1;	
    '	nC = %7^2 - (%1-%5)^2;
    '	If (nC < 0) Then Return (%False);		// No Roots
    '	nSign = 1;			// Test Each root
    '	While (nSign > -2){
    '		nRoot = %6 + sqrt(nC) * nSign;
    '  		If (nRoot >= min(%2,%4) && nRoot <= max(%2,%4)) Then{
    '			xyInt.y = nRoot;
    '			xyInt.x = %1;
    '			Return (%true);
    '			}
    '		nSign = nSign - 2;
    '		}
    '	Return (%false);
    '	}

    '// Non-Orthogonal Line
    '   If (nSlope > 0) Then {
    '   	nM = (%4 - %2) / (%3 - %1) ;		// Slope
    '   	nK = %2 - nM*%1;			// Y-Axis intercept
    '   	nA = (1 + nM ^ 2) ;			
    '   	nB = 2 * (-%5 + nM*nK - %6*nM) ;
    '  	nC = %5^2 + nK^2 + %6^2 - 2*%6*nK - %7^2 ;
    '   	nCalcTmp = (nB ^ 2) - (4 * nC * nA);

    '   	If (nCalcTmp < 0) Then Return (%False);	// No Roots
    '	nSign = 1;
    '	While (nSign > -2) {
    '   		nRoot = (-nB + (sqrt(nCalcTmp) / nSign)) / (2 * nA);
    '  		If (nRoot >= min(%1,%3) && nRoot <= max(%1,%3)) Then {  
    '			xyInt.x = nRoot;
    '	   		xyInt.y = nM * nRoot + nK;
    '			Return (%true);
    '			}
    '		nSign = nSign - 2;
    '		}
    '	Return (%false);
    ' 	}

    '   End Sub

    Private Sub FN_GetNumber(ByVal sString As String, ByRef nLength As Double, ByRef nAngle As Double)
        Dim iPos As Short
        If Len(sString) = 0 Then
            nLength = -1
            nAngle = -1
            Exit Sub
        End If
        iPos = InStr(sString, " ")
        If iPos < 0 Then
            nLength = -1
            nAngle = -1
            Exit Sub
        End If
        Dim sLeftString As Double
        sLeftString = VB.Left(sString, iPos - 1)
        nLength = Val(sLeftString)
        sString = LTrim(Mid(sString, iPos))
        If Len(sString) = 0 Then
            nAngle = -1
            Exit Sub
        End If
        nAngle = Val(sString)
    End Sub
    'Procedure to return a point at a distance and an angle from a given point
    Sub PR_CalcPolar(ByRef xyStart As xy, ByRef nLength As Double, ByVal nAngle As Double, ByRef xyReturn As xy)
        Dim A, B As Double
        'Convert from degees to radians
        nAngle = nAngle * VESTDIA1.PI / 180
        B = System.Math.Sin(nAngle) * nLength
        A = System.Math.Cos(nAngle) * nLength
        xyReturn.X = xyStart.X + A
        xyReturn.Y = xyStart.Y + B
    End Sub

    ' Calculate the required vestcurve angle.
    ' Open the VESTCURVE data file
    ' Check for 1. Existance And 2. Corruption  
    '
    ' Function	aAngle = FN_CurveAngle ( xyStart, nRadiusFromStart)
    '
    ' Purpose	Get the angle of the curve w.r.t. the rotation of
    '		of the vest curve
    '
    Public Function FN_CurveAngle(ByRef xyStart As xy, ByRef nRadiusFromStart As Double) As Double
        Dim fileNum As Object
        fileNum = FreeFile()
        Dim sSettingsPath As String = fnGetSettingsPath("LookupTables")
        FileOpen(fileNum, sSettingsPath & "\\VESTCURV.DAT", VB.OpenMode.Input)
        Dim sLine As String
        Dim aCurveRotation As Double = 100000     ' Impossible value used To test For non-intersecton
        Dim aPrevAngle As Double = 0
        Dim xyPt1, xyPt2, xyInt As xy
        xyPt1.X = xyStart.X
        xyPt1.Y = xyStart.Y
        Dim aAngle, nLength As Double
        If fileNum Then
            Do While Not EOF(fileNum)
                sLine = LineInput(fileNum)
                If sLine.Contains(Chr(32)) = False Then
                    MsgBox("Can't open VESTCURV.DAT" & Chr(13) & "Check installation", 48, "CAD - Vest Dialogue")
                    FileClose()
                    Return 0
                End If
                FN_GetNumber(sLine, nLength, aAngle)
                aAngle = aAngle + aPrevAngle
                ''-------------------------xyPt2 = CalcXY("relpolar", xyPt1, nLength, aAngle)
                PR_CalcPolar(xyPt1, nLength, aAngle, xyPt2)
                If FN_CirLinInt(xyPt1, xyPt2, xyStart, nRadiusFromStart, xyInt) Then
                    aCurveRotation = FN_CalcAngle(xyStart, xyInt) * (180 / VESTDIA1.PI)  ' V.I.P.
                    Exit Do
                End If
                xyPt1 = xyPt2
                aPrevAngle = aAngle
                ''------------------------ScanLine(sLine, "blank", & nLength, & aAngle)
            Loop
        Else
            ''Exit (%abort, "Can't open VESTCURV.DAT\nCheck installation")
            MsgBox("Can't open VESTCURV.DAT" & Chr(13) & "Check installation", 48, "CAD - Vest Dialogue")
            Return 0
        End If
        FileClose()
        If aCurveRotation = 100000 Then
            MsgBox("Can't Find Curve angle with this data in FN_CurveAngle ", 48, "CAD - Vest Dialogue")
        End If
        Return (aCurveRotation)
    End Function
    'To Draw Spline
    Private Sub PR_DrawSpline(ByRef PointCollection As Point3dCollection)
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        '' Get the current document and database
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout),
                                        OpenMode.ForWrite)
            '' Get a 3D vector from the point (0.5,0.5,0)
            Dim vecTan As Vector3d = New Point3d(0, 0, 0).GetAsVector
            '' Create a spline through (0, 0, 0), (5, 5, 0), and (10, 0, 0) with a
            '' start and end tangency of (0.5, 0.5, 0.0)
            Using acSpline As Spline = New Spline(PointCollection, vecTan, vecTan, 4, 0.0)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acSpline.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.Y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acSpline)
                acTrans.AddNewlyCreatedDBObject(acSpline, True)
            End Using
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    '' Procedure PRDataStamp ( Origin.x, Origin.y, nStampType)
    '' Purpose:	To add a stamp containing the patient details 
    ''		at the given origin
    ''
    '' Arguments:	%1 = Origin.x
    ''		%2 = Origin.y
    ''		%3 = nStampType where :-
    ''			1    = Body, full stamp
    ''			11 = Body, Axilla insert left abbreviated stamp
    ''			12 = Body, Axilla insert right abbreviated stamp
    ''			2    = Sleeve, left full stamp
    ''			3    = Sleeve, right full stamp
    ''			4    = Work Order stamp
    ''			
    '' Notes:		The origin Is the top Of the text
    ''		text Is printed downwards
    Public Sub PR_DataStamp(ByRef xyInsert As xy, ByRef nStampType As Double)
        ''-------------------SetData("TextVertJust", 32)     '' Bottom
        ''----------------------------SetData("TextHorzJust", 1)      '' Left
        ''--------------------SetData("TextHeight", 0.125)
        ''----------------------SetData("TextFont", 0)      ''CAD Block
        ''-------------------SetData("TextAspect", 0.6)

        ''Store original layer And set to layer Notes
        ''----------------------------GetData("LayerNumber", &hOriginalLayer)
        ARMDIA1.PR_SetLayer("Notes")
        Dim sText As String
        If nStampType = 1 Then
            '----------------AddEntity("text", sPatient + "\n" + sWorkOrder + "\n" + sFabric, %1,%2)
            sText = txtPatientName.Text & Chr(10) & txtWorkOrder.Text & Chr(10) & cboFabric.Text
            PR_DrawMText(sText, xyInsert, False)
            xyInsert.Y = xyInsert.Y - 0.8
            ARMDIA1.PR_SetLayer("Construct")
            ''------------------AddEntity("text", sFileNo + "\n" + sDiagnosis + "\n" + sAge + "\n" + sSEX, %1,%2)
            sText = txtFileNo.Text & Chr(10) & txtDiagnosis.Text & Chr(10) & txtAge.Text & Chr(10) & txtSex.Text
            PR_DrawMText(sText, xyInsert, False)
        End If '' End if 1 Or  2 Or 3 ( body, Left And right sleeves )

        '' Axilla Inserts stamp
        ''
        If nStampType = 11 Or nStampType = 12 Then
            If nStampType = 11 Then
                ''-----------------------------AddEntity("text", "Vest Left" + "\n" + sPatient + "\n" + sWorkOrder,%1 ,%2)
                sText = "Vest Left" & Chr(10) & txtPatientName.Text & Chr(10) & txtWorkOrder.Text
                PR_DrawMText(sText, xyInsert, False)
            Else
                ''------------------------------AddEntity("text", "Vest Right" + "\n" + sPatient + "\n" + sWorkOrder,%1, %2)
                sText = "Vest Right" & Chr(10) & txtPatientName.Text & Chr(10) & txtWorkOrder.Text
                PR_DrawMText(sText, xyInsert, False)
            End If
        End If

        If nStampType = 10 Then
            ''-----------------AddEntity("text", "Vest" + "\n" + sPatient + "\n" + sWorkOrder,%1 ,%2)
            sText = "Vest" & Chr(10) & txtPatientName.Text & Chr(10) & txtWorkOrder.Text
            PR_DrawMText(sText, xyInsert, False)
        End If

        '' Restore original layer     
        ''-------------------------Execute("menu", "SetLayer", hOriginalLayer)

    End Sub '' // End PRDataStamp() 
    Private Sub PR_DrawMText(ByRef sText As Object, ByRef xyInsert As xy, ByRef bIsRight As Boolean)
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                     OpenMode.ForRead)
            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout),
                                        OpenMode.ForWrite)

            Dim mtx As New MText()
            mtx.Location = New Point3d(xyInsert.X + xyInsertion.X, xyInsert.Y + xyInsertion.Y, 0)
            mtx.SetDatabaseDefaults()
            mtx.TextStyleId = acCurDb.Textstyle
            ' current text size
            mtx.TextHeight = 0.1
            ' current textstyle
            mtx.Width = 0.0
            mtx.Rotation = 0
            mtx.Contents = sText
            mtx.Attachment = AttachmentPoint.TopLeft
            If bIsRight = True Then
                mtx.Attachment = AttachmentPoint.BottomRight
            End If
            mtx.SetAttachmentMovingLocation(mtx.Attachment)
            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                mtx.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.Y, 0)))
            End If
            acBlkTblRec.AppendEntity(mtx)
            acTrans.AddNewlyCreatedDBObject(mtx, True)

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using
    End Sub
    'Fuction to return the rounded value of a decimal number
    Function FN_Round(ByVal nNumber As Single) As Double
        'E.G.
        '    round(1.35) = 1
        '    round(1.55) = 2
        '    round(2.50) = 3
        '    round(-2.50) = -3
        '
        Dim nInt, nSign As Short
        nSign = System.Math.Sign(nNumber)
        nNumber = System.Math.Abs(nNumber)
        nInt = Int(nNumber)
        'If (nNumber - nInt) >= 0.5 Then
        '    FN_Round = (nInt + 1) * nSign
        'Else
        '    FN_Round = nInt * nSign
        'End If

        If nInt <> 0 Then
            Dim nDec As Double = (nNumber - nInt) * 10
            ''nDec = ARMDIA1.round(nDec) / 10
            Dim nSecDec = (nDec - Int(nDec)) * 10
            nSecDec = ARMDIA1.round(nSecDec) / 10
            nDec = (Int(nDec) + nSecDec) / 10
            nNumber = nInt + nDec
            FN_Round = nNumber
        Else
            FN_Round = nNumber
        End If
    End Function
    Function PR_FindBlockExist(ByRef sBlockName As String) As Boolean
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        '' Start a transaction
        Dim bIsExist As Boolean = False
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            If acBlkTbl.Has(sBlockName) Then
                bIsExist = True
            End If
            '' Save the new object to the database
            acTrans.Commit()
        End Using
        Return bIsExist
    End Function
    'To Get Insertion point from user
    Private Sub PR_GetInsertionPoint()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        Dim pPtRes As PromptPointResult
        Dim pPtOpts As PromptPointOptions = New PromptPointOptions("")
        pPtOpts.Message = vbLf & "Indicate Start Point "
        pPtRes = acDoc.Editor.GetPoint(pPtOpts)
        If pPtRes.Status = PromptStatus.Cancel Then
            Exit Sub
        End If
        Dim ptStart As Point3d = pPtRes.Value
        PR_MakeXY(xyInsertion, ptStart.X, ptStart.Y)
    End Sub
    Private Sub saveInfoToDWG()
        Try
            Dim _sClass As New SurroundingClass()
            If (_sClass.GetXrecord("VestBody", "VESTDIC") IsNot Nothing) Then
                _sClass.RemoveXrecord("VestBody", "VESTDIC")
            End If

            Dim resbuf As New ResultBuffer
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _cboRed_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _cboRed_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboLeftCup.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboRightCup.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtLeftDisk.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtRightDisk.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtFrontNeck.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtBackNeck.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboLeftAxilla.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboRightAxilla.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboFrontNeck.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboBackNeck.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboClosure.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboFabric.Text))
            _sClass.SetXrecord(resbuf, "VestBody", "VESTDIC")

            _sClass = New SurroundingClass()
            If (_sClass.GetXrecord("VestArmFabric", "VESTARMFABDIC") IsNot Nothing) Then
                _sClass.RemoveXrecord("VestArmFabric", "VESTARMFABDIC")
            End If
            resbuf = New ResultBuffer
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboFabric.Text))
            _sClass.SetXrecord(resbuf, "VestArmFabric", "VESTARMFABDIC")

            _sClass = New SurroundingClass()
            If (_sClass.GetXrecord("VestTorsoFabric", "VESTTORSOFABDIC") IsNot Nothing) Then
                _sClass.RemoveXrecord("VestTorsoFabric", "VESTTORSOFABDIC")
            End If
            resbuf = New ResultBuffer
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboFabric.Text))
            _sClass.SetXrecord(resbuf, "VestTorsoFabric", "VESTTORSOFABDIC")

        Catch ex As Exception

        End Try
    End Sub
    Private Sub readDWGInfo()
        Try

            Dim _sClass As New SurroundingClass()
            Dim resbuf As New ResultBuffer
            resbuf = _sClass.GetXrecord("VestBody", "VESTDIC")
            If (resbuf IsNot Nothing) Then
                Dim arr() As TypedValue = resbuf.AsArray()
                _txtCir_0.Text = arr(0).Value
                _txtCir_1.Text = arr(1).Value
                _txtCir_2.Text = arr(2).Value
                _txtCir_3.Text = arr(3).Value
                _txtCir_4.Text = arr(4).Value
                _txtCir_5.Text = arr(5).Value
                _txtCir_6.Text = arr(6).Value
                _lblCir_0.Text = arr(7).Value
                _lblCir_1.Text = arr(8).Value
                _lblCir_2.Text = arr(9).Value
                _lblCir_3.Text = arr(10).Value
                _lblCir_4.Text = arr(11).Value
                _lblCir_5.Text = arr(12).Value
                _lblCir_6.Text = arr(13).Value
                _cboRed_0.Text = arr(14).Value
                _cboRed_1.Text = arr(15).Value
                _txtCir_7.Text = arr(16).Value
                _txtCir_8.Text = arr(17).Value
                _lblCir_7.Text = arr(18).Value
                _lblCir_8.Text = arr(19).Value
                _txtCir_9.Text = arr(20).Value
                _txtCir_10.Text = arr(21).Value
                _txtCir_11.Text = arr(22).Value
                _lblCir_9.Text = arr(23).Value
                _lblCir_10.Text = arr(24).Value
                _lblCir_11.Text = arr(25).Value
                cboLeftCup.Text = arr(26).Value
                cboRightCup.Text = arr(27).Value
                txtLeftDisk.Text = arr(28).Value
                txtRightDisk.Text = arr(29).Value
                txtFrontNeck.Text = arr(30).Value
                txtBackNeck.Text = arr(31).Value
                cboLeftAxilla.Text = arr(32).Value
                cboRightAxilla.Text = arr(33).Value
                cboFrontNeck.Text = arr(34).Value
                cboBackNeck.Text = arr(35).Value
                cboClosure.Text = arr(36).Value
                cboFabric.Text = arr(37).Value
            End If
            resbuf = _sClass.GetXrecord("NewPatient", "NEWPATIENTDIC")
            If (resbuf IsNot Nothing) Then
                Dim arr() As TypedValue = resbuf.AsArray()
                Dim pattern As String = "dd-MM-yyyy"
                Dim parsedDate As DateTime
                DateTime.TryParseExact(arr(8).Value, pattern, System.Globalization.CultureInfo.CurrentCulture,
                                      System.Globalization.DateTimeStyles.None, parsedDate)
                Dim startTime As DateTime = Convert.ToDateTime(parsedDate)
                Dim endTime As DateTime = DateTime.Today
                Dim span As TimeSpan = endTime.Subtract(startTime)
                Dim totalDays As Double = span.TotalDays
                Dim totalYears As Double = Math.Truncate(totalDays / 365)
                txtAge.Text = totalYears.ToString()
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub PR_DrawXMarker(ByRef xyBlkInsert As xy, ByRef length As Double)
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim xyStart, xyEnd, xyBase, xySecSt, xySecEnd As xy
        PR_CalcPolar(xyBase, length, 135, xyStart)
        PR_CalcPolar(xyBase, length, -45, xyEnd)
        PR_CalcPolar(xyBase, length, 45, xySecSt)
        PR_CalcPolar(xyBase, length, -135, xySecEnd)

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has("X Marker") Then
                Dim blkTblRecCross As BlockTableRecord = New BlockTableRecord()
                blkTblRecCross.Name = "X Marker"
                Dim acLine As Line = New Line(New Point3d(xyStart.X, xyStart.Y, 0), New Point3d(xyEnd.X, xyEnd.Y, 0))
                blkTblRecCross.AppendEntity(acLine)
                acLine = New Line(New Point3d(xySecSt.X, xySecSt.Y, 0), New Point3d(xySecEnd.X, xySecEnd.Y, 0))
                blkTblRecCross.AppendEntity(acLine)
                acBlkTbl.UpgradeOpen()
                acBlkTbl.Add(blkTblRecCross)
                acTrans.AddNewlyCreatedDBObject(blkTblRecCross, True)
                blkRecId = blkTblRecCross.Id
            Else
                blkRecId = acBlkTbl("X Marker")
            End If
            ' Insert the block into the current space
            If blkRecId <> ObjectId.Null Then
                'Create new block reference 
                Dim blkRef As BlockReference = New BlockReference(New Point3d(xyBlkInsert.X + xyInsertion.X, xyBlkInsert.Y + xyInsertion.Y, 0), blkRecId)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.Y, 0)))
                End If
                '' Open the Block table record Model space for write
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)
                acBlkTblRec.AppendEntity(blkRef)
                acTrans.AddNewlyCreatedDBObject(blkRef, True)
            End If
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawVestDia()
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim blkId As ObjectId = New ObjectId()
        Dim obj As New BlockCreation.BlockCreation
        blkId = obj.LoadBlockInstance()
        Dim xyStart(5), xyEnd(5) As Double
        xyStart(1) = 0
        xyEnd(1) = 1.25
        xyStart(2) = 0
        xyEnd(2) = 1.4375
        xyStart(3) = 1.5
        xyEnd(3) = 1.4375
        xyStart(4) = 1.5
        xyEnd(4) = 1.25
        xyStart(5) = 0
        xyEnd(5) = 1.25

        Dim xyText As vestdia.xy
        PR_MakeXY(xyText, 0.71875, 1.25)
        Dim strTag(34), strTextString(34), strAxilla(6) As String
        strTag(1) = "fileno"
        strTag(2) = "LtSCir"
        strTag(3) = "RtSCir"
        strTag(4) = "NeckCir"
        strTag(5) = "SWidth"
        strTag(6) = "S_Waist"
        strTag(7) = "ChestCir"
        'strTag(8) = "ChestCirActual"
        strTag(8) = "WaistCir"
        strTag(9) = "S_EOS"
        strTag(10) = "EOSCir"
        strTag(11) = "S_Breast"
        strTag(12) = "BreastCir"
        strTag(13) = "NippleCir"
        strTag(14) = "BraLtCup"
        strTag(15) = "BraRtCup"
        strTag(16) = "BraLtDisk"
        strTag(17) = "BraRtDisk"
        strTag(18) = "LtAxillaType"
        strTag(19) = "RtAxillaType"
        strTag(20) = "NeckType"
        strTag(21) = "NeckDimension"
        strTag(22) = "BackNeckType"
        strTag(23) = "BackNeckDim"
        strTag(24) = "Closure"
        strTag(25) = "BreastCirUserFac"
        strTag(26) = "WaistCirUserFac"
        strTag(27) = "EOSCirUserFac"
        strTag(28) = "Fabric"
        strTag(29) = "AxillaFrontNeckRad"
        strTag(30) = "AxillaBackNeckRad"
        strTag(31) = "ShoulderToBackRaglan"
        strTag(32) = "AFNRadRight"
        strTag(33) = "ABNRadRight"
        strTag(34) = "SBRaglanRight"

        Dim n As Double
        For n = 1 To 6
            strAxilla(n) = ""
        Next n
        If g_sSide = "Right" Then
            strAxilla(4) = Str(g_nAxillaFrontNeckRad)
            strAxilla(5) = Str(g_nAxillaBackNeckRad)
            strAxilla(6) = Str(g_nShoulderToBackRaglan)
        ElseIf g_sSide = "Left" Then
            strAxilla(1) = Str(g_nAxillaFrontNeckRad)
            strAxilla(2) = Str(g_nAxillaBackNeckRad)
            strAxilla(3) = Str(g_nShoulderToBackRaglan)
        End If


        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRefPatient As BlockReference = acTrans.GetObject(blkId, OpenMode.ForRead)
            Dim ptPosition As Point3d = blkRefPatient.Position
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has("VESTBODY") Then
                Dim blkTblRecCross As BlockTableRecord = New BlockTableRecord()
                blkTblRecCross.Name = "VESTBODY"
                Dim acPoly As Polyline = New Polyline()
                Dim ii As Double
                For ii = 1 To 5
                    acPoly.AddVertexAt(ii - 1, New Point2d(xyStart(ii), xyEnd(ii)), 0, 0, 0)
                Next ii
                blkTblRecCross.AppendEntity(acPoly)

                Dim acText As DBText = New DBText()
                acText.Position = New Point3d(xyText.X, xyText.Y, 0)
                acText.Height = 0.1
                acText.TextString = "VEST-BODY"
                acText.Rotation = 0
                acText.Justify = AttachmentPoint.BottomCenter
                acText.AlignmentPoint = New Point3d(xyText.X, xyText.Y, 0)
                blkTblRecCross.AppendEntity(acText)

                Dim acAttDef As New AttributeDefinition
                For ii = 1 To 34
                    acAttDef = New AttributeDefinition
                    acAttDef.Position = New Point3d(0, 0, 0)
                    acAttDef.Prompt = strTag(ii)
                    acAttDef.Tag = strTag(ii)
                    acAttDef.Height = 1
                    acAttDef.Justify = AttachmentPoint.BaseLeft
                    acAttDef.Invisible = True
                    blkTblRecCross.AppendEntity(acAttDef)
                Next
                '' Set the layer DATA as current layer
                'Dim acLyrTbl As LayerTable
                'acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead)
                'If acLyrTbl.Has("DATA") = True Then
                '    acCurDb.Clayer = acLyrTbl("DATA")
                'End If
                ''Changed for SN-BLOCK issue on 31 July 2019
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForRead)
                For Each objID As ObjectId In acBlkTblRec
                    Dim dbObj As DBObject = acTrans.GetObject(objID, OpenMode.ForRead)
                    If TypeOf dbObj Is BlockReference Then
                        Dim blkTitleBox As BlockReference = dbObj
                        If blkTitleBox.Name = "MAINPATIENTDETAILS" Then
                            Dim strTitleBoxLayer As String = blkTitleBox.Layer
                            Dim acLyrTbl As LayerTable
                            acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead)
                            If acLyrTbl.Has(strTitleBoxLayer) = True Then
                                acCurDb.Clayer = acLyrTbl(strTitleBoxLayer)
                            End If
                            Exit For
                        End If
                    End If
                Next
                acBlkTbl.UpgradeOpen()
                acBlkTbl.Add(blkTblRecCross)
                acTrans.AddNewlyCreatedDBObject(blkTblRecCross, True)
                blkRecId = blkTblRecCross.Id
            Else
                blkRecId = acBlkTbl("VESTBODY")
            End If
            ' Insert the block into the current space
            If blkRecId <> ObjectId.Null Then
                'Create new block reference 
                Dim blkRef As BlockReference = New BlockReference(ptPosition, blkRecId)
                'If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                '    blkRef.TransformBy(Matrix3d.Scaling(2.54, ptPosition))
                'End If
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)

                ''--------------Added to set TITLEBOX Layer to all block references on 30-5-2019
                For Each objID As ObjectId In acBlkTblRec
                    Dim dbObj As DBObject = acTrans.GetObject(objID, OpenMode.ForRead)
                    If TypeOf dbObj Is BlockReference Then
                        Dim blkTitleBox As BlockReference = dbObj
                        If blkTitleBox.Name = "MAINPATIENTDETAILS" Then
                            Dim strTitleBoxLayer As String = blkTitleBox.Layer
                            Dim acLyrTbl As LayerTable
                            acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead)
                            If acLyrTbl.Has(strTitleBoxLayer) = True Then
                                acCurDb.Clayer = acLyrTbl(strTitleBoxLayer)
                            End If
                            Exit For
                        End If
                    End If
                Next
                ''-----------------------

                acBlkTblRec.AppendEntity(blkRef)
                acTrans.AddNewlyCreatedDBObject(blkRef, True)
                '' Open the Block table record Model space for write
                Dim blkTblRec As BlockTableRecord
                blkTblRec = acTrans.GetObject(blkRecId, OpenMode.ForRead)

                For Each objID As ObjectId In blkTblRec
                    Dim dbObj As DBObject = acTrans.GetObject(objID, OpenMode.ForRead)
                    If TypeOf dbObj Is AttributeDefinition Then
                        Dim acAttDef As AttributeDefinition = dbObj
                        If Not acAttDef.Constant Then
                            Dim acAttRef As New AttributeReference
                            acAttRef.SetAttributeFromBlock(acAttDef, blkRef.BlockTransform)
                            If acAttRef.Tag.ToUpper().Equals("fileno", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFileNo.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("LtSCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(0).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("RtSCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(1).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("NeckCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(2).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("SWidth", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(3).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("S_Waist", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(4).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("ChestCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(5).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("WaistCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(6).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("S_EOS", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(7).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("EOSCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(8).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("S_Breast", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(9).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BreastCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(10).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("NippleCir", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtCir(11).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BraLtCup", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboLeftCup.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BraRtCup", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboRightCup.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BraLtDisk", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtLeftDisk.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BraRtDisk", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtRightDisk.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("LtAxillaType", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboLeftAxilla.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("RtAxillaType", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboRightAxilla.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("NeckType", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboFrontNeck.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("NeckDimension", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFrontNeck.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BackNeckType", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboBackNeck.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BackNeckDim", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtBackNeck.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Closure", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboClosure.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("WaistCirUserFac", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboRed(0).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("EOSCirUserFac", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboRed(1).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("BreastCirUserFac", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboRed(0).Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Fabric", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = cboFabric.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("AxillaFrontNeckRad", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = strAxilla(1)
                            ElseIf acAttRef.Tag.ToUpper().Equals("AxillaBackNeckRad", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = strAxilla(2)
                            ElseIf acAttRef.Tag.ToUpper().Equals("ShoulderToBackRaglan", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = strAxilla(3)
                            ElseIf acAttRef.Tag.ToUpper().Equals("AFNRadRight", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = strAxilla(4)
                            ElseIf acAttRef.Tag.ToUpper().Equals("ABNRadRight", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = strAxilla(5)
                            ElseIf acAttRef.Tag.ToUpper().Equals("SBRaglanRight", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = strAxilla(6)
                            Else
                                Continue For
                            End If
                            blkRef.AttributeCollection.AppendAttribute(acAttRef)
                            acTrans.AddNewlyCreatedDBObject(acAttRef, True)
                        End If
                    End If
                Next
            End If
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub

    Private Sub VestToSlv_Click(sender As Object, e As EventArgs) Handles VestToSlv.Click
        ''BODTOSLV.D file
        Me.Hide()
        VestMain.VestMainDlg.Hide()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim ed As Editor = acDoc.Editor
        Dim ptEntOpts As PromptEntityOptions = New PromptEntityOptions("Select Vest Raglan Profile")
        ptEntOpts.AllowNone = True
        ptEntOpts.Keywords.Add("Line")
        ptEntOpts.Keywords.Add("Arc")
        ptEntOpts.Keywords.Add("Curve")
        ptEntOpts.Keywords.Add("Polyline")
        Dim ptEntRes As PromptEntityResult = ed.GetEntity(ptEntOpts)
        If ptEntRes.Status <> PromptStatus.OK Then
            MsgBox("Vest Raglan Profile not selected", 16, "VEST Body - Dialogue")
            Me.Show()
            VestMain.VestMainDlg.Show()
            Exit Sub
        End If
        Dim sLayer As String = ""
        Try
            Dim idObject As ObjectId = ptEntRes.ObjectId
            Using tr As Transaction = acCurDb.TransactionManager.StartTransaction()
                Dim ent As Entity = tr.GetObject(idObject, OpenMode.ForRead)
                sLayer = ent.Layer()
                tr.Commit()
            End Using
        Catch ex As Exception
        End Try

        Dim sSleeve As String = Mid(sLayer, 9, (sLayer.Length - 8))
        If (sSleeve.ToUpper().Equals("LEFT", StringComparison.InvariantCultureIgnoreCase) = False And sSleeve.ToUpper().Equals("RIGHT", StringComparison.InvariantCultureIgnoreCase) = False) Then
            MsgBox("Select a Right or Left Raglan profile only", 16, "VEST Body - Dialogue")
            Me.Show()
            VestMain.VestMainDlg.Show()
            Exit Sub
        End If

        Dim ptOpts As PromptPointOptions = New PromptPointOptions(vbCrLf + "Axilla Point")
        Dim ptRes As PromptPointResult = ed.GetPoint(ptOpts)
        If ptRes.Status <> PromptStatus.OK Then
            Me.Show()
            VestMain.VestMainDlg.Show()
            Exit Sub
        End If
        Dim ptAxilla As Point3d = ptRes.Value
        Dim xyAxilla As xy
        xyAxilla.X = ptAxilla.X
        xyAxilla.Y = ptAxilla.Y

        ptOpts = New PromptPointOptions(vbCrLf + "Front Neck and Raglan intesection")
        ptRes = ed.GetPoint(ptOpts)
        If ptRes.Status <> PromptStatus.OK Then
            Me.Show()
            VestMain.VestMainDlg.Show()
            Exit Sub
        End If
        Dim ptFrontNeck As Point3d = ptRes.Value
        Dim xyFrontNeck As xy
        xyFrontNeck.X = ptFrontNeck.X
        xyFrontNeck.Y = ptFrontNeck.Y

        ptOpts = New PromptPointOptions(vbCrLf + "Back Neck at end of Raglan")
        ptRes = ed.GetPoint(ptOpts)
        If ptRes.Status <> PromptStatus.OK Then
            Me.Show()
            VestMain.VestMainDlg.Show()
            Exit Sub
        End If
        Dim ptBackNeck As Point3d = ptRes.Value
        Dim xyBackNeck As xy
        xyBackNeck.X = ptBackNeck.X
        xyBackNeck.Y = ptBackNeck.Y

        ptOpts = New PromptPointOptions(vbCrLf + "Back Neck at Highest Shoulder line")
        ptRes = ed.GetPoint(ptOpts)
        If ptRes.Status <> PromptStatus.OK Then
            Me.Show()
            VestMain.VestMainDlg.Show()
            Exit Sub
        End If
        Dim ptBackNeckConstruct As Point3d = ptRes.Value
        Dim xyBackNeckConstruct As xy
        xyBackNeckConstruct.X = ptBackNeckConstruct.X
        xyBackNeckConstruct.Y = ptBackNeckConstruct.Y

        Dim nAxillaFrontNeckRad, nAxillaBackNeckRad, nShoulderToBackRaglan As Double
        ''Changed for #149 and #152 in issue list
        'nAxillaFrontNeckRad = FN_CalcLength(xyAxilla, xyFrontNeck)
        'nAxillaBackNeckRad = FN_CalcLength(xyAxilla, xyBackNeck)
        'nShoulderToBackRaglan = FN_CalcLength(xyBackNeck, xyBackNeckConstruct)
        Dim nInchToCM As Double = 1
        If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            nInchToCM = 2.54
        End If
        nAxillaFrontNeckRad = FN_CalcLength(xyAxilla, xyFrontNeck) / nInchToCM
        nAxillaBackNeckRad = FN_CalcLength(xyAxilla, xyBackNeck) / nInchToCM
        nShoulderToBackRaglan = FN_CalcLength(xyBackNeck, xyBackNeckConstruct) / nInchToCM

        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            If acBlkTbl.Has("VESTBODY") Then
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)
                For Each objID As ObjectId In acBlkTblRec
                    Dim dbObj As DBObject = acTrans.GetObject(objID, OpenMode.ForWrite)
                    If TypeOf dbObj Is BlockReference Then
                        Dim blkRef As BlockReference = dbObj
                        If blkRef.Name = "VESTBODY" Then
                            For Each attributeID As ObjectId In blkRef.AttributeCollection
                                Dim attRefObj As DBObject = acTrans.GetObject(attributeID, OpenMode.ForWrite)

                                If TypeOf attRefObj Is AttributeReference Then
                                    Dim acAttRef As AttributeReference = attRefObj
                                    If (sSleeve.ToUpper().Equals("RIGHT", StringComparison.InvariantCultureIgnoreCase)) Then
                                        If acAttRef.Tag.ToUpper().Equals("AFNRadRight", StringComparison.InvariantCultureIgnoreCase) Then
                                            acAttRef.TextString = Str(nAxillaFrontNeckRad)
                                        ElseIf acAttRef.Tag.ToUpper().Equals("ABNRadRight", StringComparison.InvariantCultureIgnoreCase) Then
                                            acAttRef.TextString = Str(nAxillaBackNeckRad)
                                        ElseIf acAttRef.Tag.ToUpper().Equals("SBRaglanRight", StringComparison.InvariantCultureIgnoreCase) Then
                                            acAttRef.TextString = Str(nShoulderToBackRaglan)
                                        End If

                                    ElseIf (sSleeve.ToUpper().Equals("LEFT", StringComparison.InvariantCultureIgnoreCase)) Then
                                        If acAttRef.Tag.ToUpper().Equals("AxillaFrontNeckRad", StringComparison.InvariantCultureIgnoreCase) Then
                                            acAttRef.TextString = Str(nAxillaFrontNeckRad)
                                        ElseIf acAttRef.Tag.ToUpper().Equals("AxillaBackNeckRad", StringComparison.InvariantCultureIgnoreCase) Then
                                            acAttRef.TextString = Str(nAxillaBackNeckRad)
                                        ElseIf acAttRef.Tag.ToUpper().Equals("ShoulderToBackRaglan", StringComparison.InvariantCultureIgnoreCase) Then
                                            acAttRef.TextString = Str(nShoulderToBackRaglan)
                                        End If
                                    Else
                                        Exit For
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
            End If
            acTrans.Commit()
        End Using
        Me.Close()
        VestMain.VestMainDlg.Close()
        MsgBox("Data Transfer Finished", 0, "VEST Body - Dialogue")
    End Sub
    Private Sub PR_UpdateAge()
        Try
            Dim _sClass As New SurroundingClass()
            Dim resbuf As New ResultBuffer
            resbuf = _sClass.GetXrecord("NewPatient", "NEWPATIENTDIC")
            If (resbuf IsNot Nothing) Then
                Dim arr() As TypedValue = resbuf.AsArray()
                Dim pattern As String = "dd-MM-yyyy"
                Dim parsedDate As DateTime
                DateTime.TryParseExact(arr(8).Value, pattern, System.Globalization.CultureInfo.CurrentCulture,
                                      System.Globalization.DateTimeStyles.None, parsedDate)
                Dim startTime As DateTime = Convert.ToDateTime(parsedDate)
                Dim endTime As DateTime = DateTime.Today
                Dim span As TimeSpan = endTime.Subtract(startTime)
                Dim totalDays As Double = span.TotalDays
                Dim totalYears As Double = Math.Truncate(totalDays / 365)
                txtAge.Text = totalYears.ToString()
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub cboFabric_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFabric.SelectedIndexChanged
        VestMain.g_sVestFabric = cboFabric.Text
    End Sub
End Class