Option Strict Off
Option Explicit On
Imports VB = Microsoft.VisualBasic
Friend Class armeddia
	Inherits System.Windows.Forms.Form
	'Project:   ARMEDDIA
	'Purpose:   Editor for Arms and Vest sleeves.
	'
	'Version:   3.01
	'Date:      28.Mar.95
	'Author:    Gary George
	'-------------------------------------------------------
	'REVISIONS:
	'Date       By      Action
	'-------------------------------------------------------
	'Dec 98     GG      Ported to VB 5
	'
	'Notes:-
	'
	' Known Bugs
	'
	' 1 With a gauntlet extra or missing points above the wrist
	'   are not checked for.
	'
	' 2 The vertex mapping that is currently used only with the
	'   gaunlets between the palm and wrist should be extended
	'   to included the whole arm.
	'
	' 3 Fails if DRAFIX Help is in use.
	'
	'
	
	'UPGRADE_WARNING: Event cboContracture.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
	Private Sub cboContracture_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboContracture.SelectedIndexChanged
		Dim sType As String
		
		If g_NoElbowTape Then Exit Sub
		
		If cboContracture.Text <> txtContracture.Text Then
			txtContracture.Text = cboContracture.Text
			sType = VB.Left(txtContracture.Text, 2)
			Select Case sType
				Case "No"
					'Reset mms at elbow to standard and recalculate
					Select Case txtMM.Text
						Case "15mm"
							EditMMs(g_iElbowTape).Text = CStr(12)
						Case "20mm"
							EditMMs(g_iElbowTape).Text = CStr(16)
						Case "25mm"
							EditMMs(g_iElbowTape).Text = CStr(20)
					End Select
					PR_CalculateFromMMs(g_iElbowTape)
					cboContracture.Text = ""
					txtContracture.Text = ""
				Case "10"
					'set the reduction and back calculate grams and mms
					PR_CalculateFromReduction(g_iElbowTape, 12)
				Case "36"
					'set the reduction and back calculate grams and mms
					PR_CalculateFromReduction(g_iElbowTape, 15)
				Case "71"
					'set the reduction and back calculate grams and mms
					PR_CalculateFromReduction(g_iElbowTape, 17)
			End Select
			
		End If
	End Sub
	
	'UPGRADE_WARNING: Event cboLining.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
	Private Sub cboLining_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboLining.SelectedIndexChanged
		If cboLining.Text <> txtLining.Text Then
			If cboLining.Text = "None" Then cboLining.Text = ""
			txtLining.Text = cboLining.Text
		End If
	End Sub
	
	Private Sub cboLinings_Change()
		If g_NoElbowTape Then Exit Sub
	End Sub
	
	Private Sub cmdCancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCancel.Click
		'This cancel will delete the tempory curve then exit.
		'Checks to see if there is an Available drafix instance to
		'sendkeys to.
		Dim sTask As String
		If g_ReDrawn = True Then
			sTask = fnGetDrafixWindowTitleText()
			If sTask <> "" Then
				PR_CancelProfileEdits()
				AppActivate(sTask)
				System.Windows.Forms.SendKeys.SendWait("@C:\JOBST\DRAW.D{enter}")
			End If
		End If
        'End
    End Sub
	
	Private Sub cmdClose_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdClose.Click
		'Check that data is all present and commit into drafix
		If FN_Validate_Data() Then
			PR_DrawAndCommitProfileEdits()
			AppActivate(fnGetDrafixWindowTitleText())
			System.Windows.Forms.SendKeys.SendWait("@C:\JOBST\DRAW.D{enter}")
            Return
        End If
	End Sub
	
	Private Sub cmdDraw_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdDraw.Click
		'Check that data is all present and insert into drafix
		If FN_Validate_Data() Then
			PR_DrawProfileEdits()
			AppActivate(fnGetDrafixWindowTitleText())
			g_ReDrawn = True
			System.Windows.Forms.SendKeys.SendWait("@C:\JOBST\DRAW.D{enter}")
		End If
	End Sub
	
	Private Sub EditMMs_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles EditMMs.Enter
		Dim Index As Short = EditMMs.GetIndex(eventSender)
		ARMEDDIA1.Select_Text_In_Box(EditMMs(Index))
	End Sub
	
	Private Sub EditMMs_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles EditMMs.Leave
		Dim Index As Short = EditMMs.GetIndex(eventSender)
		If FN_CheckValue(EditMMs(Index), "MM's") Then
			PR_CalculateFromMMs(Index)
		End If
	End Sub
	
	Private Sub EditTapes_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles EditTapes.Enter
		Dim Index As Short = EditTapes.GetIndex(eventSender)
		ARMEDDIA1.Select_Text_In_Box(EditTapes(Index))
	End Sub
	
	Private Sub EditTapes_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles EditTapes.Leave
		Dim Index As Short = EditTapes.GetIndex(eventSender)
		'Check length given is valid
		'Display value in inches
		If FN_CheckValue(EditTapes(Index), "Tape Length") Then
			InchText(Index).Text = ARMEDDIA1.fnInchesToText(ARMEDDIA1.fnDisplayToInches(Val(EditTapes(Index).Text)))
			PR_CalculateFromMMs(Index)
		End If
	End Sub
	
	Private Function FN_CheckValue(ByRef TextBox As System.Windows.Forms.Control, ByRef sMessage As String) As Short
		'Check that a valid numeric value has been Entered
		Dim sChar, sText As String
		Dim iLen, nn As Short
		sText = TextBox.Text
		iLen = Len(sText)
		FN_CheckValue = True
		For nn = 1 To iLen
			sChar = Mid(sText, nn, 1)
			If Asc(sChar) > 57 Or Asc(sChar) < 46 Or Asc(sChar) = 47 Then
				MsgBox("Invalid " & sMessage & " has been entered", 48, "Arm Details")
				TextBox.Focus()
				FN_CheckValue = False
				Exit For
			End If
		Next nn
	End Function
	
	Private Function FN_DrawOpen(ByRef sDrafixFile As String, ByRef sName As Object, ByRef sPatientFile As Object, ByRef sLeftorRight As Object) As Short
        'Open the DRAFIX macro file
        'Initialise Global variables

        'Open file
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMEDDIA1.fNum = FreeFile()
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileOpen(ARMEDDIA1.fNum, sDrafixFile, OpenMode.Output)
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FN_DrawOpen = ARMEDDIA1.fNum

        'Initialise String globals
        'UPGRADE_WARNING: Couldn't resolve default property of object cc. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMDIA1.CC = Chr(44) 'The comma ( , )
        'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMDIA1.NL = Chr(10) 'The new line character
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMDIA1.QQ = Chr(34) 'Double quotes ( " )
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object cc. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMDIA1.QCQ = ARMDIA1.QQ & ARMDIA1.CC & ARMDIA1.QQ 'Quote Comma Quote ( "," )
        'UPGRADE_WARNING: Couldn't resolve default property of object cc. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMDIA1.QC = ARMDIA1.QQ & ARMDIA1.CC 'Quote Comma ( ", )
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object cc. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMDIA1.CQ = ARMDIA1.CC & ARMDIA1.QQ 'Comma Quote ( ," )

        'Globals to reduced drafix code written to file
        ARMDIA1.g_sCurrentLayer = ""
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMDIA1.g_nCurrTextAspect = 0.6

        'Write header information etc. to the DRAFIX macro file
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "//DRAFIX Arm Editing Macro created - " & DateString & "  " & TimeString)
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "//by Visual Basic")

    End Function

    Private Function FN_Validate_Data() As Short

        Dim sError, NL As String
        Dim nFirstTape, ii, nLastTape As Short
        Dim iError As Short
        Dim nValue As Double

        NL = Chr(10) 'new line


        'Display Error message (if required) and return
        If Len(sError) > 0 Then
            MsgBox(sError, 48, "Errors in Data")
            FN_Validate_Data = False
            Exit Function
        Else
            FN_Validate_Data = True
        End If


    End Function

    'UPGRADE_ISSUE: Form event Form.LinkClose was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ABD9AF39-7E24-4AFF-AD8D-3675C1AA3054"'
    Private Sub Form_LinkClose()
        Dim iValue, ii, nn, filenum As Short
        Dim nValue As Double
        Dim textline As String
        Dim sDistalStyle, sProximalStyle As String

        'Units
        If txtUnits.Text = "cm" Then
            g_nUnitsFac = 10 / 25.4
        Else
            g_nUnitsFac = 1
        End If

        ARMEDDIA1.MainForm = Me
        'Other globals
        ARMDIA1.g_sID = txtID.Text & txtArm.Text
        ARMDIA1.g_sFileNo = Mid(txtID.Text, 5)
        g_sStyle = Mid(txtID.Text, 1, 4)
        ARMDIA1.g_sSide = txtArm.Text

        'Proximal and Distal end tape Styles
        sDistalStyle = Mid(txtID.Text, 1, 2)
        sProximalStyle = Mid(txtID.Text, 3, 2)

        'Get gauntlet Details
        g_Gauntlet = ARMEDDIA1.fnGetNumber(txtGauntlet.Text, 1)
        If g_Gauntlet Then
            g_iWristNo = ARMEDDIA1.fnGetNumber(txtGauntlet.Text, 5) - 1
            g_iPalmNo = ARMEDDIA1.fnGetNumber(txtGauntlet.Text, 6) - 1
            g_nPalmWristDist = ARMEDDIA1.fnDisplayToInches(ARMEDDIA1.fnGetNumber(txtGauntlet.Text, 9))
        End If

        'Title bar display
        If txtArm.Text = "Left" Then
            Me.Text = "ARM Edit - Left [" & ARMDIA1.g_sFileNo & "]"
        End If
        If txtArm.Text = "Right" Then
            Me.Text = "ARM Edit - Right [" & ARMDIA1.g_sFileNo & "]"
        End If

        'Display - Given Length, Length in inches, MMs, Grams and Reduction at each tape
        'Store values into working arrays. Store also the initial values for change checking
        For ii = 0 To 17
            nValue = Val(Mid(txtTapeLengths.Text, (ii * 3) + 1, 3)) / 10
            If nValue > 0 Then

                EditTapes(ii).Text = CStr(nValue)
                g_nLengths(ii) = nValue
                g_nLengthsInit(ii) = nValue
                InchText(ii).Text = ARMEDDIA1.fnInchesToText(ARMEDDIA1.fnDisplayToInches(nValue))

                iValue = Val(Mid(txtTapeMMs.Text, (ii * 3) + 1, 3))
                EditMMs(ii).Text = CStr(iValue)
                g_iMMs(ii) = iValue
                g_iMMsInit(ii) = iValue

                iValue = Val(Mid(txtGrams.Text, (ii * 3) + 1, 3))
                EditGrams(ii).Text = Str(iValue)
                g_iGms(ii) = iValue
                g_iGmsInit(ii) = iValue

                iValue = Val(Mid(txtReduction.Text, (ii * 3) + 1, 3))
                EditReductions(ii).Text = Str(iValue)
                g_iRed(ii) = iValue
                g_iRedInit(ii) = iValue

            End If
        Next ii

        'Establish First tape
        'Disable unused tapes
        ARMDIA1.g_iFirstTape = -1
        nValue = 0
        For ii = 0 To 17
            nValue = Val(EditTapes(ii).Text)
            If nValue > 0 Then Exit For
            EditTapes(ii).Enabled = False
            EditMMs(ii).Enabled = False
            EditMMs(ii).BackColor = System.Drawing.ColorTranslator.FromOle(&HFFFFFF)
        Next ii
        ARMDIA1.g_iFirstTape = ii


        'If the first tape is on the elbow then disable
        'first tape editing
        If ARMDIA1.g_iFirstTape = 10 Then
            EditTapes(ARMDIA1.g_iFirstTape).Enabled = False
            EditMMs(ARMDIA1.g_iFirstTape).Enabled = False
            EditMMs(ARMDIA1.g_iFirstTape).BackColor = System.Drawing.ColorTranslator.FromOle(&HFFFFFF)
        End If

        'Establish Last tape (DISABLE down to Last Tape)
        ARMDIA1.g_iLastTape = -1
        nValue = 0
        For ii = 17 To 0 Step -1
            nValue = Val(EditTapes(ii).Text)
            If nValue > 0 Then Exit For
            EditTapes(ii).Enabled = False
            EditMMs(ii).Enabled = False
            EditMMs(ii).BackColor = System.Drawing.ColorTranslator.FromOle(&HFFFFFF)
        Next ii
        ARMDIA1.g_iLastTape = ii

        'If the Proximal style is Flap or is a Vest Raglan
        'then disable last tape editing
        If sProximalStyle = "FP" Or sProximalStyle = "VR" Then
            EditTapes(ARMDIA1.g_iLastTape).Enabled = False
            EditMMs(ARMDIA1.g_iLastTape).Enabled = False
            EditMMs(ARMDIA1.g_iLastTape).BackColor = System.Drawing.ColorTranslator.FromOle(&HFFFFFF)
        End If

        'Set redraw flag to False
        g_ReDrawn = False
        g_ShortArm = False

        'Load the original profile from file
        'Setup profile vertex to tape mappings
        PR_GetProfileFromFile("C:\JOBST\ARMCURVE.DAT")


        'Load fabric conversion charts from file
        'Establish Fabric
        If Mid(txtFabric.Text, 1, 3) = "Pow" Then
            ARMEDDIA1.PR_LoadFabricFromFile(g_MATERIAL, ARMDIA1.g_sPathJOBST & "\TEMPLTS\POWERNET.DAT")
        Else
            ARMEDDIA1.PR_LoadFabricFromFile(g_MATERIAL, ARMDIA1.g_sPathJOBST & "\TEMPLTS\BOBINNET.DAT")
        End If

        'Establish Fabric modulus
        g_iModulus = Val(Mid(txtFabric.Text, 5, 3))

        'If there is no elbow tape or elbow tape is the last tape
        'then disable linings and contractures
        g_iElbowTape = 10
        g_sOriginalContracture = txtContracture.Text
        g_sOriginalLining = txtLining.Text
        If Val(EditTapes(g_iElbowTape).Text) = 0 Or g_iElbowTape = ARMDIA1.g_iFirstTape Or g_iElbowTape = ARMDIA1.g_iLastTape Then
            cboContracture.Enabled = False
            cboLining.Enabled = False
            g_NoElbowTape = True
        Else
            cboContracture.Text = txtContracture.Text
            cboLining.Text = txtLining.Text
        End If

        Show()
        'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default 'Change pointer to default.
        cmdCancel.Focus()

    End Sub

    'UPGRADE_ISSUE: Form event Form.LinkExecute was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ABD9AF39-7E24-4AFF-AD8D-3675C1AA3054"'
    Private Sub Form_LinkExecute(ByRef CmdStr As String, ByRef Cancel As Short)
        If CmdStr = "Cancel" Then
            Cancel = 0
            'End
        End If
    End Sub

    Private Sub armeddia_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        Dim filenum As Short
        Dim textline As String
        'Hide form while loading
        Hide()

        'Check if a previous instance is running
        'If it is warn user and exit
        'UPGRADE_ISSUE: App property App.PrevInstance was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="076C26E5-B7A9-4E77-B69C-B4448DF39E58"'
        'If App.PrevInstance Then
        '    MsgBox("The Arm Edit Module is already running!" & Chr(13) & "Use ALT-TAB to access it .", 16, "Leg Edit Warning")
        '    Return 'End
        'End If

        'Position to center of screen
        Left = VB6.TwipsToPixelsX((VB6.PixelsToTwipsX(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width) - VB6.PixelsToTwipsX(Me.Width)) / 10)
        Top = VB6.TwipsToPixelsY((VB6.PixelsToTwipsY(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height) - VB6.PixelsToTwipsY(Me.Height)) / 2)

        'Set Tape Numbers
        num(0).Text = "-6"
        num(1).Text = "-4" & Chr(189)
        num(2).Text = "-3"
        num(3).Text = "-1" & Chr(189)
        num(4).Text = "0"
        num(5).Text = "1" & Chr(189)
        num(6).Text = "3"
        num(7).Text = "4" & Chr(189)
        num(8).Text = "6"
        num(9).Text = "7" & Chr(189)
        num(10).Text = "9"
        num(11).Text = "10" & Chr(189)
        num(12).Text = "12"
        num(13).Text = "13" & Chr(189)
        num(14).Text = "15"
        num(15).Text = "16" & Chr(189)
        num(16).Text = "18"
        num(17).Text = "19" & Chr(189)

        txtUnits.Text = ""
        txtUIDArm.Text = ""
        txtUIDTempArm.Text = ""
        txtUIDCurve.Text = ""
        txtArm.Text = ""
        txtID.Text = ""
        txtTapeLengths.Text = ""
        txtTapeMMs.Text = ""
        txtGrams.Text = ""
        txtReduction.Text = ""
        txtContracture.Text = ""
        txtLining.Text = ""
        txtStump.Text = ""
        txtFabric.Text = ""

        'Linings Combo
        cboLining.Items.Add("Inside Lining")
        cboLining.Items.Add("Outside Lining")
        cboLining.Items.Add("Lining")
        cboLining.Items.Add("Full Lining")
        cboLining.Items.Add("Reinforced Elbow")
        cboLining.Items.Add("None")

        'Contractures Combo
        cboContracture.Items.Add("10-35 Degrees")
        cboContracture.Items.Add("36-70 Degrees")
        cboContracture.Items.Add("71 Degrees and Over")
        cboContracture.Items.Add("None")

        g_nUnitsFac = 1 'Default to inches
        g_ShortArm = False
        g_NoElbowTape = False


        ARMDIA1.g_sPathJOBST = fnPathJOBST()

    End Sub

    Private Sub PR_CalculateFromMMs(ByRef Index As Short)
        'Calculate the reduction based on the given lengths
        'and the given MMs

        'If no change has been made to either length or MMs then exit
        If Val(EditTapes(Index).Text) = g_nLengths(Index) And Val(EditMMs(Index).Text) = g_iMMs(Index) Then Exit Sub

        Dim sConversion As String
        Dim nLength As Double
        Dim iPrevVal, ii, iVal As Short
        Dim iGrams, iReduction, iMMs As Short

        'Calculate grams
        'NB allows use of decimal MMs to fudge value
        '
        nLength = ARMEDDIA1.fnDisplayToInches(CDbl(EditTapes(Index).Text))
        iGrams = Int(nLength * Val(EditMMs(Index).Text))
        iMMs = Int(Val(EditMMs(Index).Text))

        'Get conversion string based on modulus
        sConversion = ""
        For ii = 0 To 17
            If g_iModulus = Val(g_MATERIAL.Modulus(ii)) Then
                sConversion = g_MATERIAL.Conversion_Renamed(ii)
                Exit For
            End If
        Next ii

        If sConversion = "" Then
            MsgBox("Fabric Modulus not found in conversion chart", 16, "ARM Edit")
            cmdCancel_Click(cmdCancel, New System.EventArgs())
            Return 'End
        End If

        iPrevVal = 0
        For ii = 0 To 22
            iVal = Val(Mid(sConversion, (ii * 4) + 1, 4))
            If iVal >= iGrams Then Exit For
            iPrevVal = iVal
        Next ii

        Select Case ii
            Case 0
                'Minimum reduction
                iReduction = 10
            Case 23
                'Maximum reduction
                iReduction = 32
            Case Else
                'Get reduction closest to given grams
                If (iGrams - iPrevVal) < (iVal - iGrams) Then
                    iReduction = ii + 9
                Else
                    iReduction = ii + 10
                End If
        End Select

        'Modify stored values
        g_iMMs(Index) = iMMs
        g_nLengths(Index) = Val(EditTapes(Index).Text)
        g_iRed(Index) = iReduction
        g_iGms(Index) = iGrams

        'Change display
        EditMMs(Index).Text = CStr(iMMs) 'Reset MMs to integer value
        EditGrams(Index).Text = Str(iGrams)
        EditReductions(Index).Text = Str(iReduction)

        'Show that this tape has been modified
        g_iChanged(Index) = 1

    End Sub

    Private Sub PR_CalculateFromReduction(ByRef Index As Short, ByRef iReduction As Short)

        'Back calculate from the given reduction revised grams and mms

        Dim sConversion As String
        Dim nLength As Double
        Dim iPrevVal, ii, iVal As Short
        Dim iGrams, iMMs As Short

        'Quiet exit for bad data
        nLength = ARMEDDIA1.fnDisplayToInches(CDbl(EditTapes(Index).Text))
        If nLength <= 0 Then Exit Sub

        'Get conversion string based on modulus
        sConversion = ""
        For ii = 0 To 17
            If g_iModulus = Val(g_MATERIAL.Modulus(ii)) Then
                sConversion = g_MATERIAL.Conversion_Renamed(ii)
                Exit For
            End If
        Next ii

        If sConversion = "" Then
            MsgBox("Fabric Modulus not found in conversion chart", 16, "ARM Edit")
            cmdCancel_Click(cmdCancel, New System.EventArgs())
            Return 'End
        End If


        'Get grams from conversion string based on given reduction
        'N.B. Reductions only run from 10 to 32
        '
        Select Case iReduction
            Case Is < 10
                'Use lowest available grams for those less than 10
                iGrams = Val(Mid(sConversion, 1, 4))
            Case Is > 32
                'Use highest available grams for those greater than 32
                iGrams = Val(Mid(sConversion, (22 * 4) + 1, 4))
            Case Else
                iGrams = Val(Mid(sConversion, ((iReduction - 10) * 4) + 1, 4))
        End Select

        'Back calculate MMs from grams and length
        iMMs = iGrams / nLength

        'Modify stored values
        g_iMMs(Index) = iMMs
        g_nLengths(Index) = Val(EditTapes(Index).Text)
        g_iRed(Index) = iReduction
        g_iGms(Index) = iGrams

        'Change display
        EditMMs(Index).Text = CStr(iMMs) 'Reset MMs to integer value
        EditGrams(Index).Text = Str(iGrams)
        EditReductions(Index).Text = Str(iReduction)

        'Show that this tape has been modified
        g_iChanged(Index) = 1

    End Sub

    Private Sub PR_CancelProfileEdits()
        Dim ii As Short

        If g_ReDrawn = True Then
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ARMEDDIA1.fNum = FN_DrawOpen("C:\JOBST\DRAW.D", "EDIT", "Cancel", txtArm)

            If g_ShortArm = True Then
                'Restore original curve (Short Arms only)
                PR_PutLine("HANDLE  hCurv;")
                'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(ARMEDDIA1.fNum, "hCurv = UID (" & ARMDIA1.QQ & "find" & ARMDIA1.QC & Val(txtUIDTempArm.Text) & ");")
                For ii = 0 To g_iProfile
                    'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(ARMEDDIA1.fNum, "SetVertex(hCurv," & ii + 1 & "," & xyOriginal(ii).X & "," & xyOriginal(ii).y & ");")
                Next ii
            Else
                'Delete Curve Copy
                PR_PutLine("HANDLE hTempCurv;")
                'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(ARMEDDIA1.fNum, "hTempCurv = UID (" & ARMDIA1.QQ & "find" & ARMDIA1.QC & Val(txtUIDTempArm.Text) & ");")
                'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(ARMEDDIA1.fNum, "DeleteEntity(hTempCurv);")
            End If
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            FileClose(ARMEDDIA1.fNum)
        End If
    End Sub

    Private Sub PR_DeleteTapeLabel(ByRef xyPt As ARMDIA1.XY, ByRef nTape As Object)
        'Deletes the tape label and the text at the given point
        Dim slayer As String

        Dim y1, x1, x2, y2 As Double

        x1 = xyPt.X
        x2 = xyPt.X + 1
        y1 = xyPt.y
        y2 = xyPt.y + 2
        'UPGRADE_WARNING: Couldn't resolve default property of object nTape. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        slayer = ARMDIA1.g_sFileNo & Mid(ARMDIA1.g_sSide, 1, 1) & nTape

        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "hChan=Open(" & ARMDIA1.QQ & "selection" & ARMDIA1.QCQ & "(type = 'Text' OR type = 'circle') AND layer = '" & slayer & "' AND TOTally INside " & x1 & y1 & x2 & y2 & ARMDIA1.QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "if(hChan)")
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "{ResetSelection(hChan);while(hEnt=GetNextSelection(hChan))DeleteEntity(hEnt);}")
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "Close(" & ARMDIA1.QQ & "selection" & ARMDIA1.QC & "hChan);")

    End Sub

    Private Sub PR_DrawAndCommitProfileEdits()

        'Create a macro to commit the changes to the Original profile

        Dim xyPt As ARMDIA1.XY
        Dim ii, iProfileVertex As Short
        Dim ProfileChanged As Short
        Dim nSeam As Double
        Dim sGms, sLen, sMM, sRed As String
        Dim sPackedGms, sPackedLengths, sPackedMMs, sPackedRed As String
        Dim iGms, iLen, iMM, iRed As Short

        nSeam = 0.1875

        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMEDDIA1.fNum = FN_DrawOpen("C:\JOBST\DRAW.D", "EDIT", "Test", txtArm)
        PR_PutLine("HANDLE  hCurv, hTempCurv, hArm, hChan, hEnt;")

        ARMDIA1.PR_SetLayer("Construct")

        'Delete Curve Copy (Only if redraw has been used)
        If g_ReDrawn = True And g_ShortArm = False Then
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "hTempCurv = UID (" & ARMDIA1.QQ & "find" & ARMDIA1.QC & Val(txtUIDTempArm.Text) & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "DeleteEntity(hTempCurv);")
        End If

        'Modify Original curve  (need not have been redrawn first)
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "hCurv = UID (" & ARMDIA1.QQ & "find" & ARMDIA1.QC & Val(txtUIDCurve.Text) & ");")
        ProfileChanged = False
        For ii = 0 To 17
            If g_iChanged(ii) < 0 Or g_iChanged(ii) > 0 Then
                iProfileVertex = g_iVertexMap(ii)
                xyPt.X = xyProfile(iProfileVertex - 1).X
                xyPt.y = ((ARMEDDIA1.fnDisplayToInches(g_nLengths(ii)) * (System.Math.Abs(g_iRed(ii) - 100) / 100)) / 2) + nSeam + xyOtemplate.y
                'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(ARMEDDIA1.fNum, "SetVertex(hCurv," & iProfileVertex & "," & xyPt.X & "," & xyPt.y & ");")
                xyPt.y = xyOtemplate.y
                PR_DeleteTapeLabel(xyPt, ii + 1)
                ARMDIA1.PR_PutTapeLabel(ii + 1, xyPt, ARMEDDIA1.fnDisplayToInches(g_nLengths(ii)), g_iMMs(ii), g_iGms(ii), g_iRed(ii))
                ProfileChanged = True
            End If
        Next ii
        'Update contracture if it has been changed
        If txtContracture.Text <> g_sOriginalContracture And g_NoElbowTape = False Then
            ARMEUTIL.PR_DeleteByID(ARMDIA1.g_sID & "Contracture")
            PR_DrawContracture()
        End If

        'Add revised lining if it has been changed
        If txtLining.Text <> g_sOriginalLining And g_NoElbowTape = False Then
            PR_DrawLining()
        End If

        'Update Arm Box or Origin Marker (only if Profile has been changed)
        If ProfileChanged = True Then
            For ii = 0 To 17
                iLen = g_nLengths(ii) * 10 'Shift decimal place
                iMM = g_iMMs(ii)
                iRed = g_iRed(ii)
                iGms = g_iGms(ii)

                If iLen <> 0 Then
                    sLen = New String(" ", 3)
                    sLen = RSet(Trim(Str(iLen)), Len(sLen))
                    sMM = New String(" ", 3)
                    sMM = RSet(Trim(Str(iMM)), Len(sMM))
                    sRed = New String(" ", 3)
                    sRed = RSet(Trim(Str(iRed)), Len(sRed))
                    sGms = New String(" ", 3)
                    sGms = RSet(Trim(Str(iGms)), Len(sGms))
                Else
                    sLen = New String(" ", 3)
                    sMM = New String(" ", 3)
                    sRed = New String(" ", 3)
                    sGms = New String(" ", 3)
                End If

                sPackedLengths = sPackedLengths & sLen
                sPackedMMs = sPackedMMs & sMM
                sPackedRed = sPackedRed & sRed
                sPackedGms = sPackedGms & sGms

            Next ii


            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "hArm = UID (" & ARMDIA1.QQ & "find" & ARMDIA1.QC & Val(txtUIDArm.Text) & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "if (!hArm)Exit(%cancel," & ARMDIA1.QQ & "Can't find ARMBOX to Update" & ARMDIA1.QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "SetDBData( hArm, " & ARMDIA1.QQ & "TapeLengths" & ARMDIA1.QCQ & sPackedLengths & ARMDIA1.QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "SetDBData( hArm, " & ARMDIA1.QQ & "TapeMMs" & ARMDIA1.QCQ & sPackedMMs & ARMDIA1.QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "SetDBData( hArm, " & ARMDIA1.QQ & "Grams" & ARMDIA1.QCQ & sPackedGms & ARMDIA1.QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "SetDBData( hArm, " & ARMDIA1.QQ & "Reduction" & ARMDIA1.QCQ & sPackedRed & ARMDIA1.QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "SetDBData( hArm, " & ARMDIA1.QQ & "Contracture" & ARMDIA1.QCQ & txtContracture.Text & ARMDIA1.QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "SetDBData( hArm, " & ARMDIA1.QQ & "Lining" & ARMDIA1.QCQ & txtLining.Text & ARMDIA1.QQ & ");")

        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "Execute (" & ARMDIA1.QQ & "menu" & ARMDIA1.QCQ & "ViewRedraw" & ARMDIA1.QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileClose(ARMEDDIA1.fNum)

    End Sub

    Private Sub PR_DrawContracture()
        Dim xyContractTop, xyContractBott As ARMDIA1.XY
        Dim iElbowVertex As Short
        Dim nSeam, nContractureWidth, nProfileOffset As Double
        Dim sContracture As String
        'UPGRADE_WARNING: Arrays in structure Contracture may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        Dim Contracture As ARMDIA1.curve

        If g_NoElbowTape Or txtContracture.Text = "" Then Exit Sub

        'Get elbow position on profile
        nSeam = 0.1875
        nProfileOffset = 0.5
        iElbowVertex = g_iVertexMap(g_iElbowTape)
        xyContractTop.X = xyProfile(iElbowVertex - 1).X
        xyContractTop.y = ((ARMEDDIA1.fnDisplayToInches(g_nLengths(g_iElbowTape)) * (System.Math.Abs(g_iRed(g_iElbowTape) - 100) / 100)) / 2) + nSeam + xyOtemplate.y
        xyContractTop.y = xyContractTop.y - nProfileOffset

        'Elbow position on the edge of the template at the fold line
        xyContractBott.X = xyContractTop.X
        xyContractBott.y = xyOtemplate.y

        'Contracture width
        sContracture = VB.Left(txtContracture.Text, 2)
        Select Case sContracture
            Case "No"
                'Just in case
                Exit Sub
            Case "10"
                nContractureWidth = 0.5
            Case "36"
                nContractureWidth = 1
            Case "71"
                nContractureWidth = 1.5
        End Select

        'Create contracture polyline
        Contracture.n = 5

        'Start at bottom
        Contracture.X(1) = xyContractBott.X
        Contracture.y(1) = xyContractBott.y

        'To the left
        Contracture.X(2) = xyContractBott.X - (nContractureWidth / 2)
        Contracture.y(2) = xyContractBott.y + ((xyContractTop.y - xyContractBott.y) / 2)

        'To the top
        Contracture.X(3) = xyContractTop.X
        Contracture.y(3) = xyContractTop.y

        'To the right
        Contracture.X(4) = xyContractBott.X + (nContractureWidth / 2)
        Contracture.y(4) = Contracture.y(2)

        'Close to the bottom
        Contracture.X(5) = xyContractBott.X
        Contracture.y(5) = xyContractBott.y

        'Draw contracture
        ARMDIA1.PR_SetLayer("Notes")
        ARMDIA1.PR_DrawPoly(Contracture)
        ARMDIA1.PR_AddEntityID(ARMEDDIA1.g_sID, "", "Contracture")
        ARMDIA1.PR_SetTextData(1, 32, -1, -1, -1)
        ARMDIA1.PR_DrawText("Remove For Contracture", xyContractTop, 0.125)

    End Sub

    Private Sub PR_DrawLining()
        Dim xyElbow As ARMDIA1.XY
        Dim iElbowVertex As Short
        Dim nProfileOffset, nSeam As Double

        If g_NoElbowTape Or txtLining.Text = "" Then Exit Sub

        'Get elbow position on profile
        nProfileOffset = 1
        nSeam = 0.1875
        iElbowVertex = g_iVertexMap(g_iElbowTape)
        xyElbow.X = xyProfile(iElbowVertex - 1).X
        xyElbow.y = ((ARMEDDIA1.fnDisplayToInches(g_nLengths(g_iElbowTape)) * (System.Math.Abs(g_iRed(g_iElbowTape) - 100) / 100)) / 2) + nSeam + xyOtemplate.y
        xyElbow.y = xyElbow.y - nProfileOffset

        'Draw Lining Text
        ARMDIA1.PR_SetLayer("Notes")
        ARMDIA1.PR_SetTextData(2, 32, -1, -1, -1)
        ARMDIA1.PR_DrawText(txtLining, xyElbow, 0.125)

    End Sub

    Private Sub PR_DrawProfileEdits()
        'Create a macro to copy part of the arm profile and
        'modify this copy
        'Modifications will only be committed to the original profile on "Finish"
        'The exception to this is the ShortArm where due to DRAFIX bugs the edits
        'have to be done directly on the profile.

        Dim xyPt As ARMDIA1.XY
        Dim ii, iProfileVertex As Short
        Dim nSeam As Double

        nSeam = 0.1875

        If g_ShortArm = True Then txtUIDTempArm.Text = txtUIDCurve.Text

        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ARMEDDIA1.fNum = FN_DrawOpen("C:\JOBST\DRAW.D", "EDIT", "Profile Edits", txtArm)
        PR_PutLine("HANDLE  hCurv;")

        If g_ReDrawn <> True And g_ShortArm = False Then
            'Make Copy of curve (First time through only)
            'Not for short arms (3 tapes)

            ARMDIA1.PR_SetLayer("Construct")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "hCurv = AddEntity(" & ARMDIA1.QQ & "poly" & ARMDIA1.QCQ & "fitted" & ARMDIA1.QQ)

            For ii = 0 To g_iProfile
                'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                Print(ARMEDDIA1.fNum, ARMDIA1.CC & xyProfile(ii).X & ARMDIA1.CC & xyProfile(ii).y)
            Next ii

            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, ");")

        Else
            'Modify Curve Copy
            If txtUIDTempArm.Text <> "" Then
                'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(ARMEDDIA1.fNum, "hCurv = UID (" & ARMDIA1.QQ & "find" & ARMDIA1.QC & Val(txtUIDTempArm.Text) & ");")
            Else

            End If
        End If

        'Loop through all tape values.  If changed then modify profile copy
        For ii = 0 To 17
            If g_iChanged(ii) > 0 Then
                g_iChanged(ii) = -1
                iProfileVertex = g_iVertexMap(ii)
                xyPt.X = xyProfile(iProfileVertex - 1).X
                xyPt.y = ((ARMEDDIA1.fnDisplayToInches(g_nLengths(ii)) * (System.Math.Abs(g_iRed(ii) - 100) / 100)) / 2) + nSeam + xyOtemplate.y
                'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(ARMEDDIA1.fNum, "SetVertex(hCurv," & iProfileVertex & "," & xyPt.X & "," & xyPt.y & ");")
            End If
        Next ii

        If g_ReDrawn <> True And g_ShortArm = False Then
            PR_PutLine("HANDLE  hDDE;")

            'Poke Curve UID back to the VB program
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "hDDE = Open (" & ARMDIA1.QQ & "dde" & ARMDIA1.QCQ & "armeddia" & ARMDIA1.QCQ & "armeddia" & ARMDIA1.QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(ARMEDDIA1.fNum, "Poke ( hDDE, " & ARMDIA1.QQ & "txtUIDTempArm" & ARMDIA1.QC & "MakeString(" & ARMDIA1.QQ & "long" & ARMDIA1.QC & "UID(" & ARMDIA1.QQ & "get" & ARMDIA1.QQ & ",hCurv)));")

        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "Execute (" & ARMDIA1.QQ & "menu" & ARMDIA1.QCQ & "ViewRedraw" & ARMDIA1.QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileClose(ARMEDDIA1.fNum)

    End Sub

    Private Sub PR_GetProfileFromFile(ByRef sFileName As String)
        'Procedure to read curve data from file
        'Data is a series of x, y values.
        'In the following format
        '
        '    Line    Type
        '-----------------------------------------------
        '    1       Template Origin (xyOtemplate)
        '    2  }
        '    .  }}
        '    .  }}}  Vertices of profile (Profile)
        '    .  }}
        '    N  }
        '
        '
        'The editable vetices are setup by use of an array that maps the
        'vertex of the profile to the tapes.

        Dim iPalm, ii, iVertex As Short
        Dim fFileNum As Short
        Dim nTolerance As Double

        fFileNum = FreeFile()

        If FileLen(sFileName) = 0 Then
            MsgBox(sFileName & "Not found", 48)
            Exit Sub
        End If

        FileOpen(fFileNum, sFileName, OpenMode.Input)

        'Get control points
        Input(fFileNum, xyOtemplate.X)
        Input(fFileNum, xyOtemplate.y)

        'Get profile points
        g_iProfile = 0
        Do While Not EOF(fFileNum)
            Input(fFileNum, xyProfile(g_iProfile).X)
            Input(fFileNum, xyProfile(g_iProfile).y)
            'Store original profile for use in cancel for short arms
            xyOriginal(g_iProfile).X = xyProfile(g_iProfile).X
            xyOriginal(g_iProfile).y = xyProfile(g_iProfile).y
            g_iProfile = g_iProfile + 1
        Loop
        g_iProfile = g_iProfile - 1
        FileClose(fFileNum)

        'If a gauntlet has been given then the user may have added additional
        'Vertices to smooth the flow of the curve into the gaunlet end.  These
        'will be between the WristTape and the PalmTape.
        '
        'For editing purposes we must find the profile vertex that maps to
        'a tape. We therefor create an array that contains the profile vertex
        'number for each tape
        '
        If g_Gauntlet Then
            'One to One mapping up to Palm
            iVertex = 1
            ii = ARMDIA1.g_iFirstTape
            Do
                g_iVertexMap(ii) = iVertex
                iVertex = iVertex + 1
                ii = ii + 1
            Loop Until ii > g_iPalmNo
            iPalm = iVertex - 1

            'Skip Vertex between palm and wrist
            'NB Use of tolerance
            nTolerance = 0.01
            For ii = iPalm To g_iProfile
                If xyProfile(ii).X > xyProfile(iPalm - 1).X + (g_nPalmWristDist + nTolerance) Then Exit For
            Next ii
            iVertex = ii

            'One to One mapping from wrist onwards
            For ii = g_iWristNo To ARMDIA1.g_iLastTape
                g_iVertexMap(ii) = iVertex
                iVertex = iVertex + 1
            Next ii

        Else
            'For ordinary arms we assume a 1 to 1 mapping
            iVertex = 1
            For ii = ARMDIA1.g_iFirstTape To ARMDIA1.g_iLastTape
                g_iVertexMap(ii) = iVertex
                iVertex = iVertex + 1
            Next ii
            'Check to see if the user has inserted extra points
            'Warn and exit, as this will cause errors
            If (ARMDIA1.g_iLastTape - ARMDIA1.g_iFirstTape) > g_iProfile Then
                MsgBox("There are missing points in the Plain Arm profile.  The editor will not work!  Redraw arm or insert missing point", 16, "Arm EDIT")
                Return
            End If
            If (ARMDIA1.g_iLastTape - ARMDIA1.g_iFirstTape) < g_iProfile Then
                MsgBox("There are extra points in the Plain Arm profile.  The editor will not work!  Remove extra points and try again", 16, "Arm EDIT")
                Return
            End If
        End If



        'DRAFIX only allows the creation by a macro of an Open Fitted curve if the curve
        'has more than 3 points.
        'Therefor the method of duplicating the curve to display the results is
        'not available.  Edits will be applied directly to the curve
        If g_iProfile = 2 Then g_ShortArm = True


    End Sub

    Private Sub PR_PutLine(ByRef sLine As String)
        'Puts the contents of sLine to the opened "Macro" file
        'Puts the line with no translation or additions
        '    ARMEDDIA1.fNum is global variable
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, sLine)
    End Sub

    Private Sub PR_SetLayer(ByRef sNewLayer As String)
        'To the DRAFIX macro file (given by the global ARMEDDIA1.fNum).
        'Write the syntax to set the current LAYER.
        'For this to work it assumes that hLayer is defined in DRAFIX as
        'a HANDLE.
        '
        'Note:-
        '    ARMEDDIA1.fNum, CC, QQ, NL, g_sCurrentLayer are globals initialised by FN_Open
        '
        'To reduce unessesary writing of DRAFIX code check that the new layer
        'is different from the Current layer, change only if it is different.
        '

        If ARMDIA1.g_sCurrentLayer = sNewLayer Then Exit Sub
        ARMDIA1.g_sCurrentLayer = sNewLayer

        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "hLayer = Table(" & ARMDIA1.QQ & "find" & ARMDIA1.QCQ & "layer" & ARMDIA1.QCQ & sNewLayer & ARMDIA1.QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object ARMEDDIA1.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(ARMEDDIA1.fNum, "if ( hLayer != %badtable)" & "Execute (" & ARMDIA1.QQ & "menu" & ARMDIA1.QCQ & "SetLayer" & ARMDIA1.QC & "hLayer);")

    End Sub
End Class