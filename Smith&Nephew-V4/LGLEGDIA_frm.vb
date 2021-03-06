Option Strict Off
Option Explicit On
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports VB = Microsoft.VisualBasic
Friend Class lglegdia
    Inherits System.Windows.Forms.Form
    'Module :   LGLEGDIA.MAK
    'Purpose:   Input and Figure leg
    '
    'Version:   3.01
    'Date:      1995
    'Author:    Gary George
    '
    'Used in:
    '
    '-------------------------------------------------------
    'REVISIONS:
    'Date       By      Action
    'Oct/Nov 95 GG      Modifications w.r.t Triton / Imageable
    '
    '19.Dec.95  GG      Bug fix
    '                   Close button always assumed a change
    '                   had been made.
    '                   FN_ConcatData() added
    '
    'Jan 99     GG      Ported to VB5
    '-------------------------------------------------------
    '
    'Notes:-
    '    Much of the code and form has been hacked from
    '    WHFIGURE.FRM and WHLEGDIA.FRM
    '    As both of these are proven, there has been little
    '    to no changes made except to disable JOBSTEX_FL
    '
    '    Reference to the left leg should be taken to mean
    '    the current leg
    '

    'MsgBox constants
    '    Const IDYES = 6
    '    Const IDNO = 7
    '    Const IDCANCEL = 2

    'Other constants
    Dim g_sTextList As String = " -6-4� -3-1�  0 1�  3 4�  6 7�  910� 1213� 1516� 1819� 2122� 2425� 2728� 3031� 3334� 36"
    Const NEWLINE As Short = 13
    Public g_JOBSTEX_FL As Short
    Public g_sChangeChecker As String
    Public g_iStyleLastTape As Short
    Public g_iStyleFirstTape As Short
    Public g_iFirstTape As Short
    Public g_iLastTape As Short
    Public g_nFrontStrapLength As Double
    Public g_nGauntletExtension As Double
    Public g_nLtLastAnkle As Double
    Public g_iLtAnkle As Short
    Public g_nLtLastHeel As Double
    Public g_iLtLastMM As Double
    Public g_iLtLastStretch As Short
    Public g_iLtLastZipper As Short
    Public g_iLtLastFabric As Short
    Public g_sPathJOBST As String
    Public g_iLtStretch(29) As Short
    Public g_iLtRed(29) As Short
    Public g_iLtMM(29) As Short
    'Public g_nUnitsFac As Double
    Public g_nCurrTextVertJust As Object
    Public g_nLtLengths(29) As Double

    'Constants to define the drive and the root directory
    Public g_sFileNo As String 'The patients file no
    Public g_sSide As String 'The side Left or right
    Public g_sPatient As String 'The patients name
    Public g_sFlapLength As String
    Public g_sFabric As String
    Public g_sFabnam As String
    Public g_sPressureChange As String
    Public g_sFabricChange As String
    Public g_sWpleat1 As String
    Public g_sWpleat2 As String
    Public g_sSpleat1 As String
    Public g_sSpleat2 As String
    Public g_sFlapStrap As String
    Public g_sFlapChk As Object
    Public g_sinchflag As String
    Public g_sFlap As String
    Public g_sMM As String
    Public g_sGaunt As String
    Public g_sDetGaunt As String
    Public g_sNoThumb As String
    Public g_sPalmNo As String
    Public g_sWristNo As String
    Public g_sPalmWristDist As String

    Public g_sCurrentLayer As String
    Public g_nCurrTextHt As Object
    Public g_nCurrTextAspect As Object
    Public g_nCurrTextHorizJust As Object
    Public g_nCurrTextFont As Object
    Public g_nCurrTextAngle As Object

    'Globals set by FN_Open
    Public CC As Object 'Comma
    Public QQ As Object 'Quote
    Public NL As Object 'Newline
    Public fNum As Object 'Macro file number
    Public QCQ As Object 'Quote Comma Quote
    Public QC As Object 'Quote Comma
    Public CQ As Object 'Comma Quote

    Public SmallHeel As Boolean
    Public xyToeSeam, xyProfileLast, xyPrevProfileLast, xyProfileStart As LGLEGDIA1.XY
    Public nLegStyle As Short
    Dim nThighPltXoff, nThighPltYoff, nThighTopExtension As Double
    Dim idLastCreated As ObjectId
    Dim g_sXMarkerHandle As String
    Private Sub Cancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Cancel.Click
        Dim Response As Short
        Dim sTask As String

        'Check if data has been modified
        If g_sChangeChecker <> FN_ConcatData() Then
            Response = MsgBox("Changes have been made, Save changes before closing", 35, "Left Leg")
            Select Case Response
                Case IDYES
                    Update_DDE_Text_Boxes()
                    ''--------fNum = FN_SaveOpen("c:\jobst\draw.d", txtPatientName, txtFileNo, txtLeg)
                    Dim sDrawFile As String = fnGetSettingsPath("PathDRAW") & "\draw.d"
                    fNum = FN_SaveOpen(sDrawFile, txtPatientName.Text, txtFileNo.Text, txtLeg.Text)
                    PR_SaveLeg()
                    FileClose(fNum)
                    'sTask = fnGetDrafixWindowTitleText()
                    'If sTask <> "" Then
                    '	AppActivate(fnGetDrafixWindowTitleText())
                    '	System.Windows.Forms.SendKeys.SendWait("@c:\jobst\draw.d{enter}")
                    '                   Return
                    '               Else
                    '	MsgBox("Can't find a Drafix Drawing to update!", 16, "LEG Details Dialogue")
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
        LegMain.LegMainDlg.Close()
    End Sub
    Public Function PR_CloseLeftLegDialog() As Boolean
        Dim Response As Short
        'Check if data has been modified
        If g_sChangeChecker <> FN_ConcatData() Then
            Response = MsgBox("Changes have been made, Save changes before closing", 35, "Left Leg")
            Select Case Response
                Case IDYES
                    Update_DDE_Text_Boxes()
                    Dim sDrawFile As String = fnGetSettingsPath("PathDRAW") & "\draw.d"
                    fNum = FN_SaveOpen(sDrawFile, txtPatientName.Text, txtFileNo.Text, txtLeg.Text)
                    PR_SaveLeg()
                    FileClose(fNum)
                    saveInfoToDWG()
                Case IDCANCEL
                    Return False
            End Select
        End If
        Return True
    End Function

    'UPGRADE_WARNING: Event cboFabric.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
    Private Sub cboFabric_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboFabric.SelectedIndexChanged
        PR_FigureLeftAnkle()
    End Sub

    'UPGRADE_WARNING: Event chkLeftZipper.CheckStateChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
    Private Sub chkLeftZipper_CheckStateChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles chkLeftZipper.CheckStateChanged
        If LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True Then
            If chkLeftZipper.CheckState = 1 Then
                cboLeftTemplate.SelectedIndex = 1 '9DS
            Else
                cboLeftTemplate.SelectedIndex = 0 '13DS
            End If
        End If
        PR_FigureLeftAnkle()
    End Sub

    Private Sub Draw_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Draw.Click
        Dim sCAD_App As String
        'Check that data is all present and insert into drafix
        Update_DDE_Text_Boxes()
        If Validate_Data() Then
            'Update_DDE_Text_Boxes()
            Me.Hide()
            LegMain.LegMainDlg.Hide()
            PR_GetInsertionPoint()
            PR_DrawAndSaveLeg()
            PR_DrawLegCommonBlock()
            PR_DrawLegLeftBlock()
            Dim sLegStyle As String = ""
            Select Case nLegStyle
                Case 1 'Knee High
                    sLegStyle = "KLN"
                Case 2 'Thigh Length
                    sLegStyle = "TLN"
                Case 3 'Knee Band
                    sLegStyle = "KBN"
                Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                    'Elastic
                    sLegStyle = "TBB" 'Thigh Band (B/K)
                    If nLegStyle = 4 Then
                        sLegStyle = "TBA" 'Thigh Band (A/K)
                    End If
                Case Else 'Anklet,
                    sLegStyle = "ANK"
            End Select
            PR_AddDBValueToLast("MarkerID", sLegStyle + txtFileNo.Text + txtLeg.Text + "LegCurve")
            'sCAD_App = fnGetDrafixWindowTitleText()
            'If sCAD_App <> "" Then
            '	AppActivate(sCAD_App)
            '	System.Windows.Forms.SendKeys.SendWait("@C:\JOBST\DRAW.D{enter}")
            '             Return
            '         Else
            '	MsgBox("Can't find drafix", 16, "Leg Dialogue")
            'End If
            saveInfoToDWG()
            Me.Close()
            LegMain.LegMainDlg.Close()
        End If
    End Sub

    Private Sub ExtendLegTapes_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ExtendLegTapes.Click
        'Takes the last tape given and add the required value
        'GOP - 01-02/12, 6.2.1
        '
        Dim ii As Short
        Dim nValue As Double

        'Locate last tape
        For ii = 29 To 0 Step -1
            If Val(txtLeft(ii).Text) > 0 Then Exit For
        Next ii

        'Check that there are some tapes.
        'Check that there is room to add a new tape
        If ii = 0 Or ii = 29 Then
            Beep()
            Exit Sub
        End If

        'Convert given value to inches
        nValue = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(ii).Text))

        'Extend tape
        If txtSex.Text = "Male" Then
            nValue = nValue + 0.5
        Else
            nValue = nValue + 1
        End If

        If g_iStyleLastTape = ii Then PR_SetLastTape(ii + 1)

        txtLeft(ii + 1).Text = CStr(LGLEGDIA1.fnInchesToDisplay(nValue))
        '----grdLeftInches.Row = ii + 1
        '-----grdLeftInches.Text = LGLEGDIA1.fnInchesToText(nValue)

    End Sub

    Private Function FN_BuildStyleString() As String

        Dim sString, sFabricClass As String
        Dim sFootLength As String
        Dim nSave As Short

        If LGLEGDIA1.g_POWERNET = True Then sFabricClass = "0"
        If LGLEGDIA1.g_JOBSTEX = True Then sFabricClass = "1"
        If g_JOBSTEX_FL = True Then sFabricClass = "2"

        nSave = 0

        'Transfer to the "legbox" the following set of data in blank delimited format
        'these will be stored in the fields relevent data fields
        '
        '                            DATA - Field
        '                            ~~~~~~~~~~~~
        '                LegStyle                            (1)
        '                First Tape of style                 (2)
        '                Last Tape of style                  (3)
        '                AnkleTape#                          (4)
        '                Pressure                            (5)
        '                [Grams|Stretch]                     (6)
        '                Reduction                           (7)
        '                AnkleLength                         (8)
        '                HeelLength                          (9)
        '                Zipper Status                       (10)
        '                FabricClass                         (11)
        '                Toe Style %%%                       (12)
        '                Foot Length                         (13)
        '                Template                            (14)
        '                Heel Style  ***                     (15)
        '                Heel Reinforcement ***              (16)
        '
        '          *** = Not yet implemented
        '          %%% = Above Knee / Below Knee for Footless styles
        '
        'Where Leg Style has the following meanings :-
        '
        '    0 = Anklet
        '    1 = Knee High
        '    2 = Thigh High
        '    3 = Knee band
        '    4 = Thigh band
        '
        'The First and Last tape positions are style dependant.
        If txtFootLength.Text = "" Then
            sFootLength = "0"
        Else
            sFootLength = Str(CDbl(txtFootLength.Text))
        End If

        sString = Str(g_iLegStyle) & " "
        If g_iLegStyle < 3 Then
            sString = sString & Str(g_iFirstTape + nSave) & " "
        Else
            sString = sString & Str(g_iStyleFirstTape + nSave) & " "
        End If
        sString = sString & Str(g_iStyleLastTape + nSave) & " "

        Select Case g_iLegStyle
            Case 0
                If g_iLtAnkle <> 0 Then
                    sString = sString & Str(g_iLtAnkle + 1) & " "
                    sString = sString & "0 "
                    sString = sString & "0 "
                    sString = sString & "0 "
                    sString = sString & Str(g_nLtLastAnkle) & " "
                    sString = sString & Str(g_nLtLastHeel) & " "
                    sString = sString & Str(g_iLtLastZipper) & " "
                    sString = sString & sFabricClass & " "
                    sString = sString & Str(cboToeStyle.SelectedIndex) & " "
                    sString = sString & sFootLength & " "
                    sString = sString & Str(cboLeftTemplate.SelectedIndex) & " -1 -1"
                Else
                    'Set to reflect the footless state
                    sString = sString & "-1 0 0 0 0 0 0 "
                    sString = sString & sFabricClass
                    sString = sString & " 0 0 "
                    sString = sString & Str(cboLeftTemplate.SelectedIndex) & " -1 -1"
                End If
            Case 1, 2
                If g_iLtAnkle <> 0 Then
                    sString = sString & Str(g_iLtAnkle + 1) & " "
                    sString = sString & Str(g_iLtMM(g_iLtAnkle)) & " "
                    sString = sString & Str(g_iLtStretch(g_iLtAnkle)) & " "
                    sString = sString & Str(g_iLtRed(g_iLtAnkle)) & " "
                    sString = sString & Str(g_nLtLastAnkle) & " "
                    sString = sString & Str(g_nLtLastHeel) & " "
                    sString = sString & Str(g_iLtLastZipper) & " "
                    sString = sString & sFabricClass & " "
                    sString = sString & Str(cboToeStyle.SelectedIndex) & " "
                    sString = sString & sFootLength & " "
                    sString = sString & Str(cboLeftTemplate.SelectedIndex) & " -1 -1"
                Else
                    'Set to reflect the footless state
                    sString = sString & "-1 0 0 0 0 0 0 "
                    sString = sString & sFabricClass
                    sString = sString & " 0 0 "
                    sString = sString & Str(cboLeftTemplate.SelectedIndex) & " -1 -1"
                End If

            Case 3, 4, 5
                sString = sString & "-1 0 0 0 0 0 0 "
                sString = sString & sFabricClass
                sString = sString & " 0 0 "
                sString = sString & Str(cboLeftTemplate.SelectedIndex) & " -1 -1"
        End Select

        FN_BuildStyleString = sString

    End Function

    Private Function FN_ConcatData() As String
        'Concatenates the displayed data
        'this can then be used to check if any modifications have been made
        '
        Dim ii As Short
        Dim vData As Object

        'Initialise to blank string
        'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vData = ""

        For ii = 0 To 29
            'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vData = vData + txtLeft(ii).Text
            'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If ii >= 6 Then vData = vData + txtLeftMM(ii).Text
        Next ii
        For ii = 0 To 1
            'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vData = vData + Str(optFabric(ii).Checked)
        Next ii

        For ii = 0 To 5
            'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vData = vData + Str(optType(ii).Checked)
        Next ii

        'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vData = vData + cboFabric.Text + cboToeStyle.Text + txtFootLength.Text + txtFirstTape.Text + txtLastTape.Text + cboLeftTemplate.Text + Str(chkLeftZipper.CheckState)
        'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vData = vData + txtFootPleat1.Text + txtFootPleat2.Text + txtTopLegPleat1.Text + txtTopLegPleat1.Text

        'UPGRADE_WARNING: Couldn't resolve default property of object vData. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FN_ConcatData = vData

    End Function

    Private Function FN_DrawOpen(ByRef sDrafixFile As String, ByRef sName As Object, ByRef sPatientFile As Object, ByRef sLeftorRight As Object) As Short
        'Open the DRAFIX macro file
        'Initialise Global variables

        'Open file
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FreeFile()
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileOpen(fNum, sDrafixFile, VB.OpenMode.Output)
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FN_DrawOpen = fNum

        'Initialise String globals
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        CC = Chr(44) 'The comma ( , )
        'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        NL = Chr(10) 'The new line character
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        QQ = Chr(34) 'Double quotes ( " )
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        QCQ = QQ & CC & QQ 'Quote Comma Quote ( "," )
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        QC = QQ & CC 'Quote Comma ( ", )
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        CQ = CC & QQ 'Comma Quote ( ," )

        'Initialise patient globals
        'UPGRADE_WARNING: Couldn't resolve default property of object sPatientFile. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sFileNo = sPatientFile
        'UPGRADE_WARNING: Couldn't resolve default property of object sLeftorRight. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sSide = sLeftorRight
        'UPGRADE_WARNING: Couldn't resolve default property of object sName. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sPatient = sName

        'Globals to reduced drafix code written to file
        g_sCurrentLayer = ""
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextHt = 0.125
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextAspect = 0.6
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHorizJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextHorizJust = 1 'Left
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextVertJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextVertJust = 8 'Bottom
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextFont = 0 'BLOCK

        'Write header information etc. to the DRAFIX macro file
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//DRAFIX Leg Drawing Macro created - " & DateString & "  " & TimeString)
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//Patient - " & g_sPatient & CC & " " & g_sFileNo & CC & " SIDE - " & g_sSide)
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//by Visual Basic")

        'Text data
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextHorzJust" & QC & g_nCurrTextHorizJust & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextVertJust" & QC & g_nCurrTextVertJust & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextHeight" & QC & g_nCurrTextHt & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextAspect" & QC & g_nCurrTextAspect & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextFont" & QC & g_nCurrTextFont & ");")

        PR_PutLine("STRING sTitleName, sLeg, sTmp, sWorkOrder, sID, sPathJOBST;")

        'Path to JOBST installed directory
        PR_PutStringAssign("sPathJOBST", ARMDIA1.FN_EscapeSlashesInString(g_sPathJOBST))

    End Function

    Private Function FN_EscapeSlashesInString(ByRef sAssignedString As Object) As String
        'Search through the string looking for " (double quote characater)
        'If found use \ (Backslash) to escape it
        '
        Dim ii As Short
        'UPGRADE_NOTE: Char was upgraded to Char_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
        Dim Char_Renamed As String
        Dim sEscapedString As String

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

    End Function


    Private Function FN_POWERNET_Pressure(ByRef iReduction As Short, ByRef nAnkleCir As Double, ByRef iModulus As Short) As Short
        'nMMHg = FN_POWERNET_Pressure(iReduction, nAnkleCir, iModulus)
        'This fuction is the reverse of FN_POWERNET_reduction
        '
        'Input
        '    iReduction  a Reduction in the range 10 to 32
        '    iModulus    Modulus of chosen fabric
        '                Eg Fabric = "Pow 210-3B Cream" => Modulus = 210.
        '    nAnkleCir   Ankle circumferance used with the derved grams to
        '                back calculat the pressue at the given reductio
        '
        'Globals
        '    POWERNET    The Fabric conversion chart loaded from file.
        '                Maps modulus to reduction at the given grams.
        '
        'Output
        '    nMMHg       Pressure reverse calclated from the conversion chart.
        '
        '
        'NOTE:-
        '    No range checking is done on the reduction values.
        '    Similarly the modulus is not checked.
        '    So it had better be right.
        '
        Dim sConversion As String
        Dim iGrams, iVal, ii As Short

        'Get conversion string based on modulus
        sConversion = ""
        For ii = 0 To 17
            If iModulus = Val(LGLEGDIA1.POWERNET.Modulus(ii)) Then
                sConversion = LGLEGDIA1.POWERNET.Conversion_Renamed(ii)
                Exit For
            End If
        Next ii

        iVal = iReduction - 10
        iGrams = Val(Mid(sConversion, (iVal * 4) + 1, 4))

        FN_POWERNET_Pressure = ARMDIA1.round(iGrams / nAnkleCir)

    End Function

    Private Function FN_POWERNET_Reduction(ByRef iGrams As Short, ByRef iModulus As Short) As Short
        'nReduction = FN_POWERNET_Reduction(nMMHg, nGrams, iModulus)
        'Input
        '    iGrams      Grams at Ankle
        '    iModulus    Modulus of chosen fabric
        '                Eg Fabric = "Pow 210-3B Cream" => Modulus = 210.
        '
        'Globals
        '    POWERNET    The Fabric conversion chart loaded from file.
        '                Maps modulus to reduction at the given grams.
        '
        'Output
        '    iReduction  Reduction established from the conversion chart.
        '                In the range 10 to 32
        '
        '
        'NOTE
        '    This fuction is derived from the DRAFIX function
        '    FNCalcReduction()
        '
        Dim sConversion As String
        Dim iPrevVal, ii, iVal As Short

        'Get conversion string based on modulus
        sConversion = ""
        For ii = 0 To 17
            If iModulus = Val(LGLEGDIA1.POWERNET.Modulus(ii)) Then
                sConversion = LGLEGDIA1.POWERNET.Conversion_Renamed(ii)
                Exit For
            End If
        Next ii

        If sConversion = "" Then
            FN_POWERNET_Reduction = -1000
            Exit Function
        End If

        iPrevVal = 0
        For ii = 0 To 22
            iVal = Val(Mid(sConversion, (ii * 4) + 1, 4))
            If iVal >= iGrams Then Exit For
            iPrevVal = iVal
        Next ii

        'Default to a 10 reduction
        If ii = 0 Then
            FN_POWERNET_Reduction = 10
            Exit Function
        End If

        'Get reduction closest to given grams
        If (iGrams - iPrevVal) < (iVal - iGrams) Then
            FN_POWERNET_Reduction = ii + 9
        Else
            FN_POWERNET_Reduction = ii + 10
        End If

    End Function

    Private Function FN_SaveOpen(ByRef sDrafixFile As String, ByRef sName As Object, ByRef sPatientFile As Object, ByRef sLeftorRight As Object) As Short

        'Open the DRAFIX macro file
        'Initialise Global variables

        'Open file
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FreeFile()
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileOpen(fNum, sDrafixFile, VB.OpenMode.Output)
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FN_SaveOpen = fNum

        'Initialise String globals
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        CC = Chr(44) 'The comma ( , )
        'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        NL = Chr(10) 'The new line character
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        QQ = Chr(34) 'Double quotes ( " )
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QCQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        QCQ = QQ & CC & QQ 'Quote Comma Quote ( "," )
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        QC = QQ & CC 'Quote Comma ( ", )
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        CQ = CC & QQ 'Comma Quote ( ," )

        'Initialise patient globals
        'UPGRADE_WARNING: Couldn't resolve default property of object sPatientFile. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sFileNo = sPatientFile
        'UPGRADE_WARNING: Couldn't resolve default property of object sLeftorRight. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sSide = sLeftorRight
        'UPGRADE_WARNING: Couldn't resolve default property of object sName. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sPatient = sName

        'Write header information etc. to the DRAFIX macro file
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//DRAFIX Leg Drawing Macro created - " & DateString & "  " & TimeString)
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//Patient - " & g_sPatient & CC & " " & g_sFileNo & CC & " SIDE - " & g_sSide)
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//by Visual Basic")
        PR_PutLine("STRING sTitleName, sLeg, sTmp, sWorkOrder, sID, sPathJOBST;")

        'Path to JOBST installed directory
        PR_PutStringAssign("sPathJOBST", ARMDIA1.FN_EscapeSlashesInString(g_sPathJOBST))

    End Function

    Private Function fnGetNumber(ByVal sString As String, ByRef iIndex As Short) As Double
        'Function to return as a numerical value the iIndexth item in a string
        'that uses blanks (spaces) as delimiters.
        'EG
        '    sString = "12.3 65.1 45"
        '    fnGetNumber( sString, 2) = 65.1
        '
        'If the iIndexth item is not found then return -1 to indicate an error.
        'This assumes that the string will not be used to store -ve numbers.
        'Indexing starts from 1

        Dim ii, iPos As Short
        Dim sItem As String

        'Initial error checking
        sString = Trim(sString) 'Remove leading and trailing blanks

        If Len(sString) = 0 Then
            fnGetNumber = -1
            Exit Function
        End If

        'Prepare string
        sString = sString & " " 'Trailing blank as stopper for last item

        'Get iIndexth item
        For ii = 1 To iIndex
            iPos = InStr(sString, " ")
            If ii = iIndex Then
                sString = VB.Left(sString, iPos - 1)
                fnGetNumber = Val(sString)
                Exit Function
            Else
                sString = LTrim(Mid(sString, iPos))
                If Len(sString) = 0 Then
                    fnGetNumber = -1
                    Exit Function
                End If
            End If
        Next ii

        'The function should have exited befor this, however just in case
        '(iIndex = 0) we indicate an error,
        fnGetNumber = -1

    End Function

    'UPGRADE_ISSUE: Form event Form.LinkClose was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ABD9AF39-7E24-4AFF-AD8D-3675C1AA3054"'
    Private Sub Form_LinkClose()

        Dim nAge, ii, nn, iFabricClass As Short
        Dim iZipper, iMMHg, iValue As Short
        Dim nValue, nStretch As Double
        Dim sMessage As String
        Dim iLastStyleFound As Short
        Dim sSpacer As Object

        'Disable timeout timer
        Timer1.Enabled = False

        'Units
        If txtUnits.Text = "cm" Then
            LGLEGDIA1.g_nUnitsFac = 10 / 25.4
        Else
            LGLEGDIA1.g_nUnitsFac = 1
        End If

        'Use Title fabric if a fabric is not already given
        If txtLegTitleFabric.Text <> "" And txtFabric.Text = "" Then txtFabric.Text = txtLegTitleFabric.Text

        'Set dropdown combo boxes
        'Toe Style
        cboToeStyle.Items.Add("")
        cboToeStyle.Items.Add("Curved")
        cboToeStyle.Items.Add("Cut-Back")
        cboToeStyle.Items.Add("Straight")
        cboToeStyle.Items.Add("Soft Enclosed")
        cboToeStyle.Items.Add("Soft Enclosed B/M")
        cboToeStyle.Items.Add("Self Enclosed")

        'Set value
        'For ii = 0 To (cboToeStyle.Items.Count - 1)
        '    If VB6.GetItemString(cboToeStyle, ii) = txtToeStyle.Text Then
        '        cboToeStyle.SelectedIndex = ii
        '    End If
        'Next ii

        'Leg Option buttons
        If txtLeg.Text = "Left" Then
            Me.Text = "LEG Details - Left"
            optLeftLeg.Checked = True
            optRightLeg.Enabled = False
        End If
        If txtLeg.Text = "Right" Then
            Me.Text = "LEG Details - Right"
            optRightLeg.Checked = True
            optLeftLeg.Enabled = False
        End If

        'Update tape boxes
        'Get first and last tapes in the text boxes
        g_iFirstTape = -1
        g_iLastTape = -1

        g_iStyleFirstTape = -1
        g_iStyleLastTape = 30

        nValue = 0
        '------grdLeftInches.Col = 0
        For ii = 0 To 29
            nValue = Val(Mid(txtLeftLengths.Text, (ii * 4) + 1, 4)) / 10
            If nValue > 0 Then
                txtLeft(ii).Text = CStr(nValue)
                nValue = LGLEGDIA1.fnDisplaytoInches(nValue)
                '-------------grdLeftInches.Row = ii
                '--------------grdLeftInches.Text = LGLEGDIA1.fnInchesToText(nValue)
                lblLeft(ii).Text = Str(nValue)
            End If
            If g_iFirstTape < 0 And nValue > 0 Then g_iFirstTape = ii
            If g_iLastTape < 0 And g_iFirstTape > 0 And nValue = 0 Then g_iLastTape = ii - 1
        Next ii
        If nValue > 0 Then g_iLastTape = 29

        'Set leg style options
        'The chosen leg style is given when the user has selected a
        'pattern from the drawing rather than using one of the buttons.

        g_iLegStyle = fnGetNumber(txtChosenStyle.Text, 1)

        If g_iLegStyle >= 0 Then
            optType_CheckedChanged(optType.Item(g_iLegStyle), New System.EventArgs())
            optType(g_iLegStyle).Checked = True
            PR_SetLastTape(g_iStyleLastTape)
        Else
            PR_LastTapeDisplay("Disabled")
            PR_FirstTapeDisplay("Disabled")
        End If

        If g_iLegStyle >= 3 Then PR_SetFirstTape(g_iStyleFirstTape)


        'Establish how the leg is to be figured
        'Base this on chosen fabric and availability of mm
        'If it is not given explicitly

        'Check for an explititly given fabric class from the Style field
        'This Field will be emplty if no previous figuring has been done
        'Note This is a multi data. I the first number is -ve then there is no ankle.
        '     Also the function fnGetNumber returns -1 if the number does not exist.
        '
        iFabricClass = -1
        If fnGetNumber(g_sStyleString, 4) <> -1 Then
            iFabricClass = fnGetNumber(g_sStyleString, 11)
        End If

        'Get fabric class from other data
        If iFabricClass < 0 Then
            If txtFabric.Text = "" Then
                If txtDiagnosis.Text = "Burns" Then
                    LGLEGDIA1.g_POWERNET = True
                Else
                    '                If Left$(txtDiagnosis.Text, 5) = "Lymph" Then
                    '                    g_JOBSTEX_FL = True
                    '                Else
                    '                    g_JOBSTEX = True
                    '                End If
                    LGLEGDIA1.g_JOBSTEX = True
                End If
            Else
                If VB.Left(txtFabric.Text, 3) = "Pow" Then
                    LGLEGDIA1.g_POWERNET = True
                Else
                    '                If txtLeftMMs.Text <> Then
                    '                    g_JOBSTEX_FL = True
                    '                Else
                    '                    g_JOBSTEX = True
                    '                End If
                    LGLEGDIA1.g_JOBSTEX = True
                End If
            End If
        Else
            If iFabricClass = 0 Then LGLEGDIA1.g_POWERNET = True
            If iFabricClass = 1 Then LGLEGDIA1.g_JOBSTEX = True
            If iFabricClass = 2 Then LGLEGDIA1.g_JOBSTEX = True
            '        If iFabricClass = 2 Then g_JOBSTEX_FL = True
        End If

        If LGLEGDIA1.g_JOBSTEX = True Then optFabric(1).Checked = True
        '   If g_JOBSTEX_FL = True Then optFabric(2).Value = True

        'Setup fabric dropdown box
        Dim sSettingsPath As String = fnGetSettingsPath("LookupTables")
        If LGLEGDIA1.g_POWERNET = True Then
            optFabric(0).Checked = True
            chkStretch.Enabled = True
            ''-------LGLEGDIA1.PR_GetComboListFromFile(cboFabric, g_sPathJOBST & "\WHFABRIC.DAT")
            ''-------PR_LoadFabricFromFile(g_sPathJOBST & "\TEMPLTS\POWERNET.DAT")
            ''LGLEGDIA1.PR_GetComboListFromFile(cboFabric, sSettingsPath & "\WHFABRIC.DAT")
            cboFabric.Items.Clear()
            LGLEGDIA1.PR_GetComboListFromFile(cboFabric, sSettingsPath & "\FABRIC.DAT")
            PR_LoadFabricFromFile(sSettingsPath & "\POWERNET.DAT")
        End If

        If LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True Then
            'Jobstex fabric
            cboFabric.Items.Clear()
            'cboFabric.Items.Add("53 - JOBSTEX")
            'cboFabric.Items.Add("55 - JOBSTEX")
            'cboFabric.Items.Add("57 - JOBSTEX")
            'cboFabric.Items.Add("63 - JOBSTEX")
            'cboFabric.Items.Add("65 - JOBSTEX")
            'cboFabric.Items.Add("67 - JOBSTEX")
            'cboFabric.Items.Add("73 - JOBSTEX")
            'cboFabric.Items.Add("75 - JOBSTEX")
            'cboFabric.Items.Add("77 - JOBSTEX")
            'cboFabric.Items.Add("83 - JOBSTEX")
            'cboFabric.Items.Add("85 - JOBSTEX")
            'cboFabric.Items.Add("87 - JOBSTEX")

            cboFabric.Items.Add("53 - POWERTEX")
            cboFabric.Items.Add("55 - POWERTEX")
            cboFabric.Items.Add("57 - POWERTEX")
            cboFabric.Items.Add("63 - POWERTEX")
            cboFabric.Items.Add("65 - POWERTEX")
            cboFabric.Items.Add("67 - POWERTEX")
            cboFabric.Items.Add("73 - POWERTEX")
            cboFabric.Items.Add("75 - POWERTEX")
            cboFabric.Items.Add("77 - POWERTEX")
            cboFabric.Items.Add("83 - POWERTEX")
            cboFabric.Items.Add("85 - POWERTEX")
            cboFabric.Items.Add("87 - POWERTEX")
        End If

        'Set fabric value
        'NB   It might be that the fabric originally used is not one on the
        '     current fabric list.  In this case add the given fabric to
        '     the start of the list
        For ii = 0 To (cboFabric.Items.Count - 1)
            If VB6.GetItemString(cboFabric, ii) = txtFabric.Text Then
                cboFabric.SelectedIndex = ii
            End If
        Next ii
        If txtFabric.Text <> "" And cboFabric.SelectedIndex = -1 Then
            cboFabric.Items.Insert(0, txtFabric.Text)
            cboFabric.SelectedIndex = 0
        End If
        For ii = 0 To (cboToeStyle.Items.Count - 1)
            If VB6.GetItemString(cboToeStyle, ii) = txtToeStyle.Text Then
                cboToeStyle.SelectedIndex = ii
            End If
        Next ii


        'Set up depending on leg style chosen
        'if only a single style available then display the values for that
        'style only
        'other wise display as blank untill the user selects an option

        PR_EstablishAnkles()

        If LGLEGDIA1.g_POWERNET = True Then PR_EnablePOWERNET()
        If LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True Then PR_EnableJOBSTEX()
        '   If g_JOBSTEX_FL = True Then PR_EnableFL_JOBSTEX


        'From the g_sStyleString set above extract the saved ankle figuring
        'We have to be careful in the order in which this is set up as in the case
        'of JOBSTEX_FL we do not want to cause recaculation on the ankle.
        'We also must take care only to add values only if they exist
        '(thus we check that the returned value from fnGetNumber is not -ve)
        '
        Dim iFabric As Short
        If g_sStyleString <> "" And g_iLtAnkle <> 0 Then
            iZipper = fnGetNumber(g_sStyleString, 10)
            If iZipper >= 0 Then chkLeftZipper.CheckState = iZipper 'Order is v.important here
            iMMHg = fnGetNumber(g_sStyleString, 5)
            If iMMHg >= 0 Then
                g_iLtMM(g_iLtAnkle) = iMMHg
                txtLeftMM(g_iLtAnkle).Text = CStr(iMMHg)
            End If
            If g_JOBSTEX_FL = True Then
                nStretch = fnGetNumber(g_sStyleString, 6)
                iFabric = Val(VB.Left(cboFabric.Text, 2))
                If nStretch >= 0 Then
                    PR_DisplayFiguredAnkle("Left", nStretch, g_nLtLastAnkle, g_nLtLastHeel, iFabric)
                    g_iLtStretch(g_iLtAnkle) = nStretch
                End If
            End If
        End If


        'Don't figure ankle for fabric class JOBSTEX_FL as the user can modify the pressures
        'at each leg tape manually and these are stored.  Figuring would overwrite
        'any manual changes
        If g_JOBSTEX_FL <> True Then
            PR_FigureLeftAnkle()
        Else
            If txtLeftMMs.Text <> "" And g_iLtAnkle <> 0 Then
                For ii = g_iLtAnkle + 1 To 29
                    iValue = Val(Mid(txtLeftMMs.Text, (ii * 3) + 1, 3))
                    If iValue > 0 Then
                        txtLeftMM(ii).Text = CStr(iValue)
                        PR_FigureLeftTape(ii)
                    End If
                Next ii
            End If
        End If

        'Disable anklet and knee if no ankle tape
        If g_iLtAnkle = 0 Then
            optType(0).Enabled = False
            optType(1).Enabled = False
            optType(2).Enabled = False
        End If

        'Find existing styles display last found
        sMessage = ""
        'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sSpacer = ""
        iLastStyleFound = -1

        If txtAnklet.Text <> "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sMessage = sMessage + sSpacer + "Anklet"
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sSpacer = ", "
            iLastStyleFound = 0
        End If

        If txtKneeLength.Text <> "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sMessage = sMessage + sSpacer + "Knee Length"
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sSpacer = ", "
            iLastStyleFound = 1
        End If
        If txtThighLength.Text <> "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sMessage = sMessage + sSpacer + "Thigh Length"
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sSpacer = ", "
            iLastStyleFound = 2
        End If
        If txtKneeBand.Text <> "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sMessage = sMessage + sSpacer + "Knee Band"
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sSpacer = ", "
            iLastStyleFound = 3
        End If
        If txtThighBandAK.Text <> "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sMessage = sMessage + sSpacer + "Thigh (A/K)"
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sSpacer = ", "
            iLastStyleFound = 4
        End If
        If txtThighBandBK.Text <> "" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sMessage = sMessage + sSpacer + "Thigh (B/K)"
            'UPGRADE_WARNING: Couldn't resolve default property of object sSpacer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sSpacer = ", "
            iLastStyleFound = 5
        End If

        If iLastStyleFound >= 0 Then
            optType_CheckedChanged(optType.Item(iLastStyleFound), New System.EventArgs())
            optType(iLastStyleFound).Checked = True
            labMessage.Text = "Existing Styles: " & Chr(13) & sMessage
        End If

        g_sChangeChecker = FN_ConcatData()
        Show()
        'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default 'Change pointer to default.
        Me.Activate()

    End Sub

    'UPGRADE_ISSUE: Form event Form.LinkExecute was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ABD9AF39-7E24-4AFF-AD8D-3675C1AA3054"'
    Private Sub Form_LinkExecute(ByRef CmdStr As String, ByRef Cancel As Short)
        If CmdStr = "Cancel" Then
            Cancel = 0
            Return
        End If
    End Sub

    Private Sub lglegdia_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        Try
            Dim ii As Short
            Hide()
            'Check if a previous instance is running
            'If it is warn user and exit
            'UPGRADE_ISSUE: App property App.PrevInstance was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="076C26E5-B7A9-4E77-B69C-B4448DF39E58"'
            'if app.previnstance then
            '	msgbox("the leg input module is already running!" & chr(13) & "use alt-tab and cancel it.", 16, "error starting figure")
            '          return
            '      end if

            'Maintain while loading DDE data
            'UPGRADE_WARNING: Screen property Screen.MousePointer has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6BA9B8D2-2A32-4B6E-8D36-44949974A5B4"'
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor ' Change pointer to hourglass.
            'Reset in Form_LinkClose

            'Position to center of screen
            Left = CStr((VB6.PixelsToTwipsX(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width) - VB6.PixelsToTwipsX(Me.Width)) / 2) ' Center form horizontally.
            Top = VB6.TwipsToPixelsY((VB6.PixelsToTwipsY(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height) - VB6.PixelsToTwipsY(Me.Height)) / 2) ' Center form vertically.

            LGLEGDIA1.MainForm = Me

            LGLEGDIA1.g_nUnitsFac = 1 'Default to inches
            g_iLegStyle = -1
            LGLEGDIA1.g_JOBSTEX = False
            g_JOBSTEX_FL = False
            LGLEGDIA1.g_POWERNET = False
            g_sTextList = "-7� -6-4� -3-1�  0 1�  3 4�  6 7�  910� 1213� 1516� 1819� 2122� 2425� 2728� 3031� 3334� 36"
            txtLeg.Text = "Left"

            ''Added for #159 in the issue list
            For ii = 1 To 30
                Dim strTape As String = LTrim(Mid(g_sTextList, ((ii - 1) * 3) + 1, 3))
                cboLastTape.Items.Add(strTape)
                cboFirstTape.Items.Add(strTape)
            Next

            'Setup display inches grid
            '-----grdLeftInches.set_ColWidth(0, 880)
            '------grdLeftInches.set_ColAlignment(0, 2)
            ''---------------------------
            'For ii = 0 To 29
            '    grdLeftInches.set_RowHeight(ii, 266)
            'Next ii

            'Setup display of results grid
            'For ii = 0 To 1
            '    grdLeftDisplay.set_ColWidth(ii, 488)
            '    grdLeftDisplay.set_ColAlignment(ii, 2)
            'Next ii

            'For ii = 0 To 23
            '    grdLeftDisplay.set_RowHeight(ii, 266)
            'Next ii
            ''-------------------------------

            g_sPathJOBST = fnPathJOBST()
            Dim strMod(18), strConv(18) As String
            LGLEGDIA1.POWERNET.Modulus = strMod
            LGLEGDIA1.POWERNET.Conversion_Renamed = strConv
            idLastCreated = New ObjectId
            g_sXMarkerHandle = ""

            txtAnklet.Text = ""
            txtKneeLength.Text = ""
            txtThighLength.Text = ""
            txtKneeBand.Text = ""
            txtThighBandAK.Text = ""
            txtThighBandBK.Text = ""

            'Enable time out timer
            'Timer1.Interval = 6000
            'Timer1.Enabled = True

            Dim fileNo As String = "", patient As String = "", diagnosis As String = "", age As String = "", sex As String = ""
            Dim workOrder As String = "", tempDate As String = "", tempEng As String = "", units As String = ""
            Dim obj As New BlockCreation.BlockCreation
            Dim blkId As ObjectId = New ObjectId()
            blkId = obj.LoadBlockInstance()
            If (blkId.IsNull()) Then
                MsgBox("Can't find Patient Details", 48, "LEG Details Dialog")
                Me.Close()
                Exit Sub
            End If

            obj.BindAttributes(blkId, fileNo, patient, diagnosis, age, sex, workOrder, tempDate, tempEng, units)

            txtDiagnosis.Text = diagnosis
            txtFileNo.Text = fileNo
            txtPatientName.Text = patient
            txtUnits.Text = units
            txtAge.Text = age
            txtSex.Text = sex
            txtWorkOrder.Text = workOrder
            If txtUnits.Text = "cm" Then
                LGLEGDIA1.g_nUnitsFac = 10 / 25.4
            Else
                LGLEGDIA1.g_nUnitsFac = 1
            End If
            chkStretch.Enabled = False
            chkStretch.Checked = False
            chkHeelContracture.Checked = True
            readDWGInfo()
            Form_LinkClose()
        Catch ex As Exception
            Me.Close()
            LegMain.LegMainDlg.Close()
        End Try
    End Sub

    'UPGRADE_WARNING: Event optFabric.CheckedChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
    Private Sub optFabric_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optFabric.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optFabric.GetIndex(eventSender)
            'Allows the selection of fabric class
            'Used to override the fabric class selection established at link close

            'Clean Message box
            ''labMessage.Text = ""

            'Do nothing if same fabric class selected
            If LGLEGDIA1.g_POWERNET = True And Index = 0 Then Exit Sub
            If LGLEGDIA1.g_JOBSTEX = True And Index = 1 Then Exit Sub
            If g_JOBSTEX_FL = True And Index = 2 Then Exit Sub

            'Restablish ankles just in case they have changed
            PR_EstablishAnkles()

            'Set the display for the different fabric classes
            'We must first reset the form to the blank form that we use on form load
            PR_ResetFormLGLEGDIA()

            LGLEGDIA1.g_POWERNET = False
            LGLEGDIA1.g_JOBSTEX = False
            g_JOBSTEX_FL = False

            Select Case Index
                Case 0
                    'POWERNET
                    LGLEGDIA1.g_POWERNET = True
                    PR_EnablePOWERNET()
                    chkStretch.Enabled = True
                Case 1
                    'JOBSTEX
                    LGLEGDIA1.g_JOBSTEX = True
                    PR_EnableJOBSTEX()
                    chkStretch.Enabled = False
                    chkStretch.Checked = False
                Case 2
                    'JOBSTEX_FL, Pressure calulated at every leg tape
                    g_JOBSTEX_FL = True
                    PR_EnableJOBSTEX()
                    'PR_EnableFL_JOBSTEX
            End Select

            'Setup fabric dropdown box
            cboFabric.Items.Clear()
            Dim sSettingsPath As String = fnGetSettingsPath("LookupTables")
            If LGLEGDIA1.g_POWERNET = True Then
                ''-----------LGLEGDIA1.PR_GetComboListFromFile(cboFabric, g_sPathJOBST & "\WHFABRIC.DAT")
                ''-----------PR_LoadFabricFromFile(g_sPathJOBST & "\TEMPLTS\POWERNET.DAT")
                ''LGLEGDIA1.PR_GetComboListFromFile(cboFabric, sSettingsPath & "\WHFABRIC.DAT")
                cboFabric.Items.Clear()
                LGLEGDIA1.PR_GetComboListFromFile(cboFabric, sSettingsPath & "\FABRIC.DAT")
                PR_LoadFabricFromFile(sSettingsPath & "\POWERNET.DAT")
            End If

            If LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True Then
                'Jobstex fabric
                cboFabric.Items.Clear()
                'cboFabric.Items.Add("53 - JOBSTEX")
                'cboFabric.Items.Add("55 - JOBSTEX")
                'cboFabric.Items.Add("57 - JOBSTEX")
                'cboFabric.Items.Add("63 - JOBSTEX")
                'cboFabric.Items.Add("65 - JOBSTEX")
                'cboFabric.Items.Add("67 - JOBSTEX")
                'cboFabric.Items.Add("73 - JOBSTEX")
                'cboFabric.Items.Add("75 - JOBSTEX")
                'cboFabric.Items.Add("77 - JOBSTEX")
                'cboFabric.Items.Add("83 - JOBSTEX")
                'cboFabric.Items.Add("85 - JOBSTEX")
                'cboFabric.Items.Add("87 - JOBSTEX")

                cboFabric.Items.Add("53 - POWERTEX")
                cboFabric.Items.Add("55 - POWERTEX")
                cboFabric.Items.Add("57 - POWERTEX")
                cboFabric.Items.Add("63 - POWERTEX")
                cboFabric.Items.Add("65 - POWERTEX")
                cboFabric.Items.Add("67 - POWERTEX")
                cboFabric.Items.Add("73 - POWERTEX")
                cboFabric.Items.Add("75 - POWERTEX")
                cboFabric.Items.Add("77 - POWERTEX")
                cboFabric.Items.Add("83 - POWERTEX")
                cboFabric.Items.Add("85 - POWERTEX")
                cboFabric.Items.Add("87 - POWERTEX")
            End If

            'If we have values for Ankle Pressure and fabric lets use them
            If g_iLtLastFabric < cboFabric.Items.Count Then
                cboFabric.SelectedIndex = g_iLtLastFabric
                g_iLtLastFabric = -1000 'This will force re-figuring
                If g_iLtMM(g_iLtAnkle) > 0 Then
                    txtLeftMM(g_iLtAnkle).Text = CStr(g_iLtMM(g_iLtAnkle))
                    PR_FigureLeftAnkle()
                End If
                g_iLtLastFabric = -1
            End If

        End If
    End Sub

    'UPGRADE_WARNING: Event optType.CheckedChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
    Private Sub optType_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optType.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optType.GetIndex(eventSender)

            Dim ii As Short
            Dim nValue As Double

            g_iLastLegStyle = g_iLegStyle
            g_iLegStyle = Index

            'get the Style String g_sStyleString from the relevent DDE box
            'or rebuild string from existing displayed data
            Select Case g_iLegStyle
                Case 0
                    g_sStyleString = txtAnklet.Text
                    '            g_sBestCatNo = "0105"   'Anklet
                    '            g_nOptions = 0
                Case 1
                    g_sStyleString = txtKneeLength.Text
                    '            g_sBestCatNo = "0101"   'Knee length
                    '            g_nOptions = 0
                Case 2
                    g_sStyleString = txtThighLength.Text
                    '            g_sBestCatNo = "0201"   'Thigh length
                    '            g_nOptions = 0
                Case 3
                    g_sStyleString = txtKneeBand.Text
                    '            g_sBestCatNo = "0015"   'Knee band
                    '            g_nOptions = 3
                    '            g_sCatNo(1) = "0015"    'Knee band
                    '            g_sCatNo(2) = "1131"    'Stump Support Below Knee
                    '            g_sCatNo(3) = "0101"    'Footless Knee Length
                Case 4
                    g_sStyleString = txtThighBandAK.Text
                    '            g_sBestCatNo = "0019"   'Thigh band
                    '            g_nOptions = 3
                    '            g_sCatNo(1) = "0019"    'Thigh band
                    '            g_sCatNo(2) = "1130"    'Stump Support above Knee
                    '            g_sCatNo(3) = "0201"    'Footless thigh Length

                Case 5
                    g_sStyleString = txtThighBandBK.Text
                    '            g_sBestCatNo = "0019"    'Thigh band
                    '            g_nOptions = 3
                    '            g_sCatNo(1) = "0019"    'Thigh band
                    '            g_sCatNo(2) = "1131"    'Stump Support Below Knee
                    '            g_sCatNo(3) = "0201"    'Footless thigh Length

            End Select

            'Find First and last Tapes as displayed
            'Don't worry about holes for now
            g_iFirstTape = -1
            For ii = 0 To 29
                If Val(txtLeft(ii).Text) > 0 Then Exit For
            Next ii
            If ii < 30 Then g_iFirstTape = ii

            g_iLastTape = -1
            For ii = 29 To 0 Step -1
                If Val(txtLeft(ii).Text) > 0 Then Exit For
            Next ii
            If ii >= 0 Then g_iLastTape = ii

            If g_iLastTape = g_iFirstTape Then
                g_iFirstTape = -1
                g_iLastTape = -1
            End If

            PR_LastTapeDisplay("Enabled")

            Select Case Index
                Case 0 'Anklet
                    'Disable FirstTape display
                    PR_FirstTapeDisplay("Disabled")
                    'Disable heel figuring
                    'Set last tape to ankle + 1
                    If g_iLtAnkle <> 0 Then
                        PR_SetLastTape(g_iLtAnkle + 1)
                        txtLeftMM(g_iLtAnkle).Enabled = False
                        txtLeftMM(g_iLtAnkle).Text = ""
                        '--------grdLeftDisplay.Row = g_iLtAnkle - 6
                        '--------grdLeftDisplay.Col = 0
                        lblGms(g_iLtAnkle - 6).Text = ""
                        '--------grdLeftDisplay.Text = ""
                        '--------grdLeftDisplay.Col = 1
                        '--------grdLeftDisplay.Text = ""
                        lblRed(g_iLtAnkle - 6).Text = ""
                        cboToeStyle.SelectedIndex = fnGetNumber(g_sStyleString, 12)
                    End If

                    If LGLEGDIA1.g_POWERNET = True Then cboLeftTemplate.SelectedIndex = 0

                Case 1 To 2 'Knee Length, Thigh Length
                    'Disable FirstTape display
                    PR_FirstTapeDisplay("Disabled")
                    'Enable and figure ankle
                    If g_iLtAnkle <> 0 Then
                        txtLeftMM(g_iLtAnkle).Enabled = True
                        cboToeStyle.SelectedIndex = fnGetNumber(g_sStyleString, 12)
                        If g_iLtLastMM <> 0 Then
                            txtLeftMM(g_iLtAnkle).Text = CStr(g_iLtLastMM)
                            g_iLtLastFabric = -1000 'Use this to force a refigure
                            PR_FigureLeftAnkle()
                        End If
                    End If

                    'Set Last style tape from StyleString
                    If g_sStyleString <> "" Then
                        PR_SetLastTape(fnGetNumber(g_sStyleString, 3) - 1)
                    End If

                    'Ensure that the style last tape is valid, reset if not
                    'If g_iStyleLastTape > 0 And g_iStyleLastTape <= g_iLastTape Then
                    '    PR_SetLastTape(g_iStyleLastTape)
                    'Else
                    '    PR_SetLastTape(g_iLastTape)
                    'End If
                    PR_SetLastTape(g_iLastTape)

                Case 3, 4, 5 'Knee Band, Thigh Band AK and Thigh Band BK
                    'Enable FirstTape display
                    PR_FirstTapeDisplay("Enabled")

                    'Disable Heel figuring
                    If g_iLtAnkle <> 0 Then
                        txtLeftMM(g_iLtAnkle).Text = ""
                        txtLeftMM(g_iLtAnkle).Enabled = False
                        'grdLeftDisplay.Row = g_iLtAnkle - 6
                        'grdLeftDisplay.Col = 0
                        lblGms(g_iLtAnkle - 6).Text = ""
                        'grdLeftDisplay.Text = ""
                        'grdLeftDisplay.Col = 1
                        'grdLeftDisplay.Text = ""
                        lblRed(g_iLtAnkle - 6).Text = ""
                    End If

                    'Set Style first tape
                    'Use existing if available else use first tape as above
                    'don't go past ankle
                    'Set First and Last style tapes from StyleString
                    If g_sStyleString <> "" Then
                        PR_SetFirstTape(fnGetNumber(g_sStyleString, 2) - 1)
                        PR_SetLastTape(fnGetNumber(g_sStyleString, 3) - 1)
                    End If

                    'Ensure that the style last tape is valid, reset if not
                    'If g_iStyleFirstTape > 0 And g_iStyleFirstTape >= g_iFirstTape Then
                    '    PR_SetFirstTape(g_iStyleFirstTape)
                    'Else
                    '    If g_iLtAnkle <> 0 Then
                    '        PR_SetFirstTape(g_iLtAnkle)
                    '    Else
                    '        PR_SetFirstTape(g_iFirstTape)
                    '    End If
                    'End If
                    PR_SetFirstTape(g_iFirstTape)

                    'If g_iStyleLastTape > 0 And g_iStyleLastTape <= g_iLastTape Then
                    '    PR_SetLastTape(g_iStyleLastTape)
                    'Else
                    '    PR_SetLastTape(g_iLastTape)
                    'End If
                    PR_SetLastTape(g_iLastTape)

                    cboLeftTemplate.SelectedIndex = 0

            End Select

            If g_sStyleString <> "" Then
                'Restore values from style string
                chkLeftZipper.CheckState = fnGetNumber(g_sStyleString, 10)

                nValue = fnGetNumber(g_sStyleString, 13)
                If nValue <> 0 Then
                    txtFootLength.Text = CStr(nValue)
                Else
                    txtFootLength.Text = ""
                End If

                txtFootLength_Leave(txtFootLength, New System.EventArgs())

                If fnGetNumber(g_sStyleString, 14) <= cboLeftTemplate.Items.Count - 1 Then
                    cboLeftTemplate.SelectedIndex = fnGetNumber(g_sStyleString, 14)
                Else
                    cboLeftTemplate.SelectedIndex = -1
                End If
                If g_iLtAnkle <> 0 And g_iLegStyle > 0 And g_iLegStyle < 3 And fnGetNumber(g_sStyleString, 5) <> 0 Then
                    txtLeftMM(g_iLtAnkle).Text = CStr(fnGetNumber(g_sStyleString, 5))
                    g_iLtLastFabric = -1000 'Use this to force a refigure
                    PR_FigureLeftAnkle()
                End If
            Else
                'Build style string from exisisting data
                g_sStyleString = FN_BuildStyleString()
            End If

        End If
    End Sub

    Private Sub PR_DisplayErrorMessage(ByRef iErrorNum As Object, ByRef sContext As Object)
        'Procedure to display error messages
        '
        '    iErrorNum   Local error number
        '    sContext    User supplied string to be displayed in the Message Label
        '
        'NOTE
        '    In many cases values returned fron functions are -ve to indicate that
        '    an error has occured.
        '    This meams that an error can be quickly established without having to
        '    know the actual error details, mearly by checking if the returned value
        '    is -ve.
        '    This works well in this particular set of modules as therv are very few
        '    time that a -ve value is required to be returned.
        '
        '    All error numbers are local to this Module.
        '
        '
        '

        Dim iError As Short
        Dim sError, NL As String

        NL = Chr(13) 'New Line character
        'Within this procedure the error number is always +ve.
        'UPGRADE_WARNING: Couldn't resolve default property of object iErrorNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        iError = System.Math.Abs(iErrorNum)

        sError = ""
        Select Case iError
            'Messages with repect to available Styles
            Case 500
                sError = "Anklet"
            Case 501
                sError = "Knee Length"
            Case 502
                sError = "Thigh Length"
            Case 503
                sError = "Knee Band"
            Case 504
                sError = "Thigh Band A/K"
            Case 505
                sError = "Thigh Band B/K"
                'Ankle figuring
            Case 1000
                sError = "The modulus for the chosen fabric " & VB6.GetItemString(cboFabric, cboFabric.SelectedIndex) & NL
                sError = sError & "Not listed in the POWERNET conversion chart."
            Case 1001
                sError = "The given Pressure resulted in a reduction of less than 14. "
                sError = sError & "Using a 14 reduction and back calculating the Pressure"
            Case 1002
                sError = "The given Pressure resulted in a reduction of over 26. "
                sError = sError & "As the Diagnosis is for Burns, "
                sError = sError & "using a 26 reduction and back calculating the Pressure"
            Case 1003
                sError = "The given Pressure resulted in a reduction of over 32. "
                sError = sError & "As the Diagnosis is other than Burns, "
                sError = sError & "using a 32 reduction and back calculating the Pressure"
            Case 1004
                sError = "At this reduction the 100% stretch is exceeded. "
                sError = sError & "Decrease the reduction."
            Case 2001
                sError = "Maximum Stretch exceeded. "
            Case 2002
                sError = "Minimum Stretch exceeded. "
            Case 2003
                sError = "Donning Stretch of 110% exceeded. "
            Case 2004
                sError = "Reduction exceeds 32. "

            Case Else
                sError = "Unknown Error" & NL
        End Select

        'If a message is in the box then add
        If labMessage.Text = "" Then
            labMessage.Text = sError
        Else
            labMessage.Text = labMessage.Text & NL & NL & sError
        End If
        Beep()

    End Sub

    Private Sub PR_DisplayFiguredAnkle(ByRef sLeg As String, ByRef nStretch As Double, ByRef nAnkleCir As Double, ByRef nHeelCir As Double, ByRef iFabric As Short)
        Dim nPatternDim As Double
        Dim iReduction, iMaxStretch As Short
        Dim sContext As String
        Dim iDonningStretch, iMinStretch As Short

        nPatternDim = (nAnkleCir / (1 + (0.01 * nStretch))) / LGLEGDIA1.g_nUnitsFac
        iReduction = ARMDIA1.round((1 - 1 / (0.01 * nStretch + 1)) * 100)
        iDonningStretch = ARMDIA1.round((((nHeelCir / LGLEGDIA1.g_nUnitsFac) - nPatternDim) / nPatternDim) * 100)

        'grdLeftDisplay.Row = g_iLtAnkle - 6
        'grdLeftDisplay.Col = 0
        lblGms(g_iLtAnkle - 6).Text = Str(nStretch)
        'Stretch
        'grdLeftDisplay.Text = Str(nStretch)
        'Reduction
        '      grdLeftDisplay.Col = 1
        'grdLeftDisplay.Text = Str(iReduction)
        lblRed(g_iLtAnkle - 6).Text = Str(iReduction)
        g_iLtRed(g_iLtAnkle) = iReduction
        'Max Stretch
        labLeftMaxStr.Text = CStr(iDonningStretch)


        'Set Max and Min stretches
        iMaxStretch = 44
        If iFabric <= 55 Then iMinStretch = 16 Else iMinStretch = 18

        If nStretch < iMinStretch Then
            PR_DisplayErrorMessage(2002, "Ankle Figuring")
        End If

        If nStretch > iMaxStretch Then
            PR_DisplayErrorMessage(2001, "Ankle Figuring")
        End If

        If iDonningStretch > 110 Then
            PR_DisplayErrorMessage(2003, "Ankle Figuring")
        End If

        If iReduction > 32 Then
            PR_DisplayErrorMessage(2004, "Ankle Figuring")
        End If



    End Sub

    Private Sub PR_DrawAndSaveLeg()
        'Procedure to create a Macro to draw a leg lower options
        'Anklet,  Knee High, Thigh High, Knee Band and Thigh Band

        Dim sFile, sBody As String
        Dim sData, sLengths, sLegStyle As String
        Dim nStyleLastRed, nReductionAnkle As Short
        Dim nLastTape, nFirstTape, nFabricClass As Short
        Dim nStyleLastTape, nStyleFirstTape, nAge As Short
        Dim FootLess, ii, itemplate As Short

        Dim nThighPltRad, nTopThigh As Double
        Dim nThighPltDeltaAngle As Double
        Dim nHeel, nThighPltLen As Double
        Dim nB, nA, nThighPltStartAngle As Double

        Dim nFoldHt, nValue As Double

        'Open Macro file (fNum is declared as Global)
        ''-------sFile = "C:\JOBST\DRAW.D"
        sFile = fnGetSettingsPath("PathDRAW") & "\draw.d"
        fNum = FN_DrawOpen(sFile, txtPatientName.Text, txtFileNo.Text, txtLeg.Text)

        PR_SaveLeg()

        PR_PutLine("@" & g_sPathJOBST & "\LEG\LG_LEG.D;")


        'Patient Details
        PR_PutStringAssign("sPatient", txtPatientName.Text)
        PR_PutStringAssign("sFileNo", txtFileNo.Text)

        Dim sWorkOrder As String = ""
        If txtWorkOrder.Text = "" Then
            PR_PutStringAssign("sWorkOrder", "-")
            sWorkOrder = "-"
        Else
            PR_PutStringAssign("sWorkOrder", txtWorkOrder.Text)
            sWorkOrder = txtWorkOrder.Text
        End If

        PR_PutStringAssign("sAge", txtAge.Text)
        nAge = Val(txtAge.Text)
        PR_PutNumberAssign("nAge", nAge)

        PR_PutStringAssign("sSEX", txtSex.Text)
        If txtSex.Text = "Male" Then
            PR_PutLine("Male = %true;")
            PR_PutLine("Female = %false;")
        Else
            PR_PutLine("Male = %false;")
            PR_PutLine("Female = %true;")
        End If

        PR_PutStringAssign("sUnits", txtUnits.Text)
        PR_PutNumberAssign("nUnitsFac", LGLEGDIA1.g_nUnitsFac)

        PR_PutStringAssign("sDiagnosis", txtDiagnosis.Text)

        'Body details
        PR_PutStringAssign("sFabric", txtFabric.Text)

        'Leg Details
        sLengths = txtLeftLengths.Text 'Use later to establish 1st & Last tapes

        If Len(txtFabric.Text) > 0 Then PR_PutStringAssign("sFabric", txtFabric.Text)

        If fnGetNumber(g_sStyleString, 4) < 0 Or fnGetNumber(g_sStyleString, 1) > 2 Or g_iLtAnkle = 0 Then
            PR_PutLine("FootLess = %true;")
            FootLess = True
        Else
            PR_PutLine("FootLess = %false;")
            'Ankle tape values
            PR_PutNumberAssign("nAnkleTape", fnGetNumber(g_sStyleString, 4))
            PR_PutStringAssign("sAnkleTape", Str(fnGetNumber(g_sStyleString, 4)))
            PR_PutStringAssign("sMMAnkle", Str(fnGetNumber(g_sStyleString, 5)))
            PR_PutStringAssign("sGramsAnkle", Str(fnGetNumber(g_sStyleString, 6)))
            PR_PutStringAssign("sReductionAnkle", Str(fnGetNumber(g_sStyleString, 7)))
            PR_PutNumberAssign("nReductionAnkle", fnGetNumber(g_sStyleString, 7))
            nReductionAnkle = fnGetNumber(g_sStyleString, 7)
            nHeel = fnGetNumber(g_sStyleString, 9)
        End If

        nFabricClass = fnGetNumber(g_sStyleString, 11)
        If nFabricClass = 2 Then
            PR_PutStringAssign("sReduction", txtLeftRed.Text)
            PR_PutStringAssign("sTapeMMs", txtLeftMMs.Text)
            PR_PutStringAssign("sStretch", txtLeftStr.Text)
        End If

        PR_PutStringAssign("sLeg", txtLeg.Text)

        PR_PutStringAssign("sPressure", txtLeftTemplate.Text)
        itemplate = CShort(VB.Left(txtLeftTemplate.Text, 2))

        PR_PutStringAssign("sTapeLengths", txtLeftLengths.Text)
        PR_PutStringAssign("sToeStyle", txtToeStyle.Text)

        Dim nFootPleat1, nFootPleat2 As Double
        nFootPleat1 = LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat1.Text))
        nFootPleat2 = LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat2.Text))
        PR_PutNumberAssign("nFootPleat1", LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat1.Text)))
        PR_PutNumberAssign("nFootPleat2", LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat2.Text)))

        PR_PutStringAssign("sFootLength", txtFootLength.Text)

        'First and LastTapes
        nFirstTape = -1
        nLastTape = 30
        For ii = 0 To 29
            nValue = Val(Mid(sLengths, (ii * 4) + 1, 4)) / 10
            'Set first and last tape (assumes no holes in data)
            If nFirstTape < 0 And nValue > 0 Then nFirstTape = ii + 1
            If nLastTape = 30 And nFirstTape > 0 And nValue = 0 Then nLastTape = ii
        Next ii
        PR_PutNumberAssign("nFirstTape", nFirstTape)
        PR_PutNumberAssign("nLastTape", nLastTape)

        'First and last tapes for style
        nStyleFirstTape = fnGetNumber(g_sStyleString, 2)
        PR_PutNumberAssign("nStyleFirstTape", nStyleFirstTape)

        nStyleLastTape = fnGetNumber(g_sStyleString, 3)
        PR_PutNumberAssign("nStyleLastTape", nStyleLastTape)

        'Get last tape value for use in calculating thigh length ending
        nValue = LGLEGDIA1.fnDisplaytoInches(Val(Mid(sLengths, ((nStyleLastTape - 1) * 4) + 1, 4)) / 10)

        'Style type etc
        'NOTE for a footless thigh length treat as a thigh band (BK)
        nLegStyle = fnGetNumber(g_sStyleString, 1)
        If nLegStyle = 2 And FootLess Then nLegStyle = 5

        PR_PutNumberAssign("nLegStyle", nLegStyle)
        PR_PutStringAssign("sType", g_sStyleString)

        Dim nTopLegPleat1, nTopLegPleat2 As Double
        nTopLegPleat1 = LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat1.Text))
        nTopLegPleat2 = LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat2.Text))
        PR_PutNumberAssign("nTopLegPleat1", LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat1.Text)))
        PR_PutNumberAssign("nTopLegPleat2", LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat2.Text)))

        Select Case nLegStyle
            Case 1 'Knee High
                sLegStyle = "KLN"

                If nFabricClass = 0 And nValue < 15 Then
                    nStyleLastRed = 10
                Else
                    nStyleLastRed = 14
                End If

            Case 2 'Thigh Length
                sLegStyle = "TLN"

                If nFabricClass = 0 Then
                    nStyleLastRed = 14
                Else
                    nStyleLastRed = 16
                End If

                'Calculate parameters for Thigh Plate Template
                nTopThigh = ((nValue * ((100 - nStyleLastRed) / 100)) / 2) + 0.1875
                nThighPltRad = 23

                Select Case nTopThigh 'Get closest thigh plate
                    Case 0 To 7
                        nThighPltXoff = 0.52
                        nThighPltLen = 6
                    Case 7 To 9
                        nThighPltXoff = 0.73
                        nThighPltLen = 8
                    Case 9 To 11
                        nThighPltXoff = 1
                        nThighPltLen = 10
                    Case 11 To 13
                        nThighPltXoff = 1.27
                        nThighPltLen = 12
                    Case Is > 13
                        nThighPltXoff = 1.5
                        nThighPltLen = 14
                End Select

                'Depending on Heel set top of thigh extension
                If nHeel < 9 Then
                    nThighTopExtension = 0.5
                    PR_PutNumberAssign("nThighTopExtension", nThighTopExtension)
                Else
                    nThighTopExtension = 1
                    PR_PutNumberAssign("nThighTopExtension", nThighTopExtension)
                End If

                'Revise thigh plate to make it fit (if required)
                'Note the thigh plate line must land on the profile, on or
                'in front of the last tape
                If nThighPltXoff < nThighTopExtension Then
                    nThighPltXoff = 1
                End If

                PR_PutNumberAssign("nThighPltRad", nThighPltRad)
                PR_PutNumberAssign("nThighPltXoff", nThighPltXoff)

            Case 3 'Knee Band
                sLegStyle = "KBN"

                PR_PutNumberAssign("nStyleFirstRed", 8)
                nStyleLastRed = 8

                'Elastic at distal always
                PR_PutNumberAssign("nElastic", 1)

            Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                If nFabricClass = 0 Then
                    nStyleLastRed = 14
                Else
                    nStyleLastRed = 16
                End If

                'Release the distal tape to a 95% reduction,  92% reduction if +3 or +1-1/2 tape
                If nStyleFirstTape > 8 Then
                    PR_PutNumberAssign("nStyleFirstRed", 5)
                Else
                    PR_PutNumberAssign("nStyleFirstRed", 8)
                End If

                'Calculate parameters for Thigh Plate Template
                nTopThigh = ((nValue * ((100 - nStyleLastRed) / 100)) / 2) + 0.1875
                nThighPltRad = 23

                Select Case nTopThigh 'Get closest thigh plate
                    Case 0 To 7
                        nThighPltXoff = 0.52
                        nThighPltLen = 6
                    Case 7 To 9
                        nThighPltXoff = 0.73
                        nThighPltLen = 8
                    Case 9 To 11
                        nThighPltXoff = 1
                        nThighPltLen = 10
                    Case 11 To 13
                        nThighPltXoff = 1.27
                        nThighPltLen = 12
                    Case Is > 13
                        nThighPltXoff = 1.5
                        nThighPltLen = 14
                End Select

                'Depending on age set top of thigh extension
                If nAge < 10 Then
                    nThighTopExtension = 0.5
                    PR_PutNumberAssign("nThighTopExtension", nThighTopExtension)
                Else
                    nThighTopExtension = 1
                    PR_PutNumberAssign("nThighTopExtension", nThighTopExtension)
                End If

                'Revise thigh plate to make it fit (if required)
                'Note the thigh plate line must land on the profile, on or
                'in front of the last tape
                If nThighPltXoff < nThighTopExtension Then
                    nThighPltXoff = 1
                End If

                PR_PutNumberAssign("nThighPltRad", nThighPltRad)
                PR_PutNumberAssign("nThighPltXoff", nThighPltXoff)

                'Elastic
                If nLegStyle = 4 Then
                    PR_PutNumberAssign("nElastic", 1)
                    sLegStyle = "TBA" 'Thigh Band (A/K)
                Else
                    PR_PutNumberAssign("nElastic", 0)
                    sLegStyle = "TBB" 'Thigh Band (B/K)
                End If


            Case Else 'Anklet,
                sLegStyle = "ANK"
                'Anklet reductions done by counting and therefor done in the
                'macro LGLEGDWG.D
                nStyleLastRed = -1
                PR_PutNumberAssign("nReductionAnkle", 14)
                nReductionAnkle = 14
        End Select

        PR_PutNumberAssign("nStyleLastRed", nStyleLastRed)
        PR_PutStringAssign("sLegStyle", sLegStyle)

        'Fabric Class (Load JOBSTEX_FL Procedures and Defaults if class = 2 )
        'For a footless style the only valid classes are 0 and 1
        If FootLess And nFabricClass = 2 Then
            nFabricClass = 1
        End If
        PR_PutNumberAssign("nFabricClass", nFabricClass)

        'Load universal Procedures and Defaults
        ''------- PR_PutLine("@" & g_sPathJOBST & "\LEG\LGLEGDEF.D;")

        'Get Origin & Draw template
        ''----------PR_PutLine("@" & g_sPathJOBST & "\LEG\LGLEGTMP.D;")

        ARMDIA1.PR_SetLayer("Construct")
        ''Load template data file
        ''--------PROpenTemplateFile ()
        If nFabricClass = 0 Then
            ''--------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
        Else
            ''---------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
        End If

        Dim hChan As Short
        hChan = FreeFile()
        FileOpen(hChan, sFile, VB.OpenMode.Input)
        Dim sLine As String = ""
        Dim nNo, nSpace, n20Len, nReduction As Double
        Dim sScale As String = ""
        If (hChan) Then
            'If (Input(hChan, sLine)) Then
            '    ScanLine(sLine, "blank", & nNo, & sScale, & nSpace, & n20Len, & nReduction)

            'Else
            '    Exit (%abort, "Can't read " + sFile + "\nFile maybe corrupted")
            '        End If
            sLine = LineInput(hChan)
            nNo = FN_GetNumber(sLine, 1)
            sScale = FN_GetString(sLine, 2)
            nSpace = FN_GetNumber(sLine, 3)
            n20Len = FN_GetNumber(sLine, 4)
            nReduction = FN_GetNumber(sLine, 5)
        Else
            ''-------Exit (%abort, "Can't open "+ sFile + "\nCheck installation")
            MsgBox("Can't open " + sFile + "\nCheck installation", 48, "Leg Dialog")
        End If
        Dim nOffset As Double = 0.5
        Dim nSeam As Double = 0.1875
        Dim xyPt1 As LGLEGDIA1.XY '' = LGLEGDIA1.xyLegInsertion ''LGLEGDIA1.xyLegInsertion is insertion pt
        ''-----xyPt1.y = LGLEGDIA1.xyLegInsertion.y + nOffset + nSeam
        xyPt1.y = 0 + nOffset + nSeam
        ''Add a marker at the Origin for later use in drawing the foot 
        ''--------hEnt = AddEntity("marker", "xmarker", LGLEGDIA1.xyLegInsertion, 0.1, 0.1)
        Dim xyBase As LGLEGDIA1.XY
        PR_DrawXMarker(xyBase, True)
        PR_AddDBValueToLast("Handle", g_sXMarkerHandle)
        ''------------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "Origin")
        PR_AddDBValueToLast("OriginID", sLegStyle + txtFileNo.Text + txtLeg.Text + "Origin")
        ''----------SetDBData(hEnt, "units", sUnits)
        PR_AddDBValueToLast("units", txtUnits.Text)
        ''--------SetDBData(hEnt, "Data", sType)
        PR_AddDBValueToLast("Data", g_sStyleString)

        '' Loop Through data file to get to start of leg tapes
        '' Ignoring the none relevent ones 
        Dim nn As Double = 1
        While (nn < nStyleFirstTape)
            ''------GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            nn = nn + 1
        End While
        ScanLine(sLine, nNo, sScale, nSpace, n20Len, nReduction)
        nSpace = 0

        '' Loop through displaying points And scales
        Dim nLoop As Double
        Dim nAnkleTape As Double = fnGetNumber(g_sStyleString, 4)
        If (nFabricClass = 2) Then
            nLoop = nAnkleTape - 1 ''  // JOBSTEX Gradient
        Else
            nLoop = nStyleLastTape
        End If
        While (nn <= nLoop)
            If (FNGetTape(nn)) Then
                ''Pleats, NB nSpace Is distance of current tape from the previous tape
                Dim nPleatGiven As Double = 0
                If nn = nStyleFirstTape + 1 And nFootPleat1 <> 0 Then
                    nSpace = nFootPleat1
                    nPleatGiven = 1
                End If
                If nn = nStyleFirstTape + 2 And nFootPleat2 <> 0 Then
                    nSpace = nFootPleat2
                    nPleatGiven = 1
                End If
                If nn = nStyleLastTape - 1 And nTopLegPleat2 <> 0 Then
                    nSpace = nTopLegPleat2
                    nPleatGiven = 1
                End If
                If nn = nStyleLastTape And nTopLegPleat1 <> 0 Then
                    nSpace = nTopLegPleat1
                    nPleatGiven = 1
                End If
                xyPt1.X = xyPt1.X + nSpace
                ''------------Changed for #65 and #77 in issue list---------------
                If chkStretch.Checked = True And nNo > 6 Then
                    xyPt1.X = xyPt1.X + 0.125
                End If
                Dim xyTmp As LGLEGDIA1.XY
                xyTmp.X = xyPt1.X
                Dim nTapeLen, nLength, jj, nTickStep, nTickHt As Double
                ''nTapeLen = FN_Round(FN_Decimalise(FNGetTape(nn)) * LGLEGDIA1.g_nUnitsFac)
                nTapeLen = FN_Decimalise(FNGetTape(nn)) * LGLEGDIA1.g_nUnitsFac
                nLength = n20Len / 20 * nTapeLen
                ''------xyTmp.y = LGLEGDIA1.xyLegInsertion.y + nSeam + nLength
                xyTmp.y = 0 + nSeam + nLength
                ''--------AddEntity("marker", "xmarker", xyTmp, 0.2, 0.2)
                PR_DrawXMarker(xyTmp)

                '' Add Pleat indication
                Dim xyText As LGLEGDIA1.XY
                If (nPleatGiven = 1) Then
                    ''---------SetData("TextHorzJust", 2)
                    ''---------AddEntity("text", "PLEAT", xyTmp.X - (nSpace / 2), xyTmp.y + 0.5)
                    PR_MakeXY(xyText, xyTmp.X - (nSpace / 2), xyTmp.y + 0.5)
                    PR_DrawText("PLEAT", xyText, 0.1, 0, 2)
                End If

                '' Add Ticks And Text at each scale
                '' Start ticks 2 below And 2 above
                ii = Int(nLength / (n20Len / 20)) - 1
                jj = ii + 2

                nTickStep = n20Len / (20 * 8)
                ''--------nTickHt = LGLEGDIA1.xyLegInsertion.y + nSeam + ii * nTickStep * 8
                nTickHt = 0 + nSeam + ii * nTickStep * 8
                '' Add Reduction Text
                ''------SetData("TextHeight", 0.125)
                ''--------SetData("TextHorzJust", 1)
                ''--------AddEntity("text", MakeString("long", nReduction), xyPt1.x + 0.2, nTickHt - 0.4)
                PR_MakeXY(xyText, xyPt1.X + 0.2, nTickHt - 0.4)
                PR_DrawText(Str(nReduction), xyText, 0.125, 0, 1)
                ''---------AddEntity("text", Format("length", nTapeLen), xyPt1.x + 0.2, nTickHt - nSeam)
                PR_MakeXY(xyText, xyPt1.X + 0.2, nTickHt - nSeam)
                ''---------PR_DrawText(Str(nTapeLen), xyText, 0.125, 0, 1)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    ''PR_DrawText(fnDisplayToCM(nTapeLen), xyText, 0.125, 0, 1)
                    PR_DrawText(txtLeft(nn - 1).Text, xyText, 0.125, 0, 1)
                Else
                    PR_DrawText(LGLEGDIA1.fnInchesToText(nTapeLen), xyText, 0.125, 0, 1)
                End If

                '' Print revised reduction at ankle tape except for anklets
                If (nn = nAnkleTape And nLegStyle <> 0) Then
                    ''--------AddEntity("text", Str(fnGetNumber(g_sStyleString, 7)), xyPt1.X + 0.4, nTickHt - 0.4)
                    PR_MakeXY(xyText, xyPt1.X + 0.4, nTickHt - 0.4)
                    PR_DrawText(Str(fnGetNumber(g_sStyleString, 7)), xyText, 0.125, 0, 1)
                    If (nFabricClass = 0) Then
                        ''------AddEntity("text", Str(fnGetNumber(g_sStyleString, 6)) + " grams", xyPt1.X + 0.4, nTickHt - 0.6)
                        PR_MakeXY(xyText, xyPt1.X + 0.4, nTickHt - 0.6)
                        PR_DrawText(Str(fnGetNumber(g_sStyleString, 6)) + " grams", xyText, 0.125, 0, 1)
                    Else
                        ''---------AddEntity("text", Str(fnGetNumber(g_sStyleString, 6)) + " stretch", xyPt1.X + 0.4, nTickHt - 0.6)
                        PR_MakeXY(xyText, xyPt1.X + 0.4, nTickHt - 0.6)
                        PR_DrawText(Str(fnGetNumber(g_sStyleString, 6)) + " stretch", xyText, 0.125, 0, 1)
                    End If
                    ''--------AddEntity("text", Str(fnGetNumber(g_sStyleString, 5)) + " mm", xyPt1.X + 0.4, nTickHt - 0.8)
                    PR_MakeXY(xyText, xyPt1.X + 0.4, nTickHt - 0.8)
                    PR_DrawText(Str(fnGetNumber(g_sStyleString, 5)) + " mm", xyText, 0.125, 0, 1)
                    ''-----------AddEntity("marker", "xmarker", xyPt1.X + 0.3, nTickHt - 0.5, 0.2, 0.2)
                    PR_MakeXY(xyText, xyPt1.X + 0.3, nTickHt - 0.5)
                    PR_DrawXMarker(xyText)
                End If
                ''-----------------
                '' Add tape ID
                ''------Dim sSymbol As String = MakeString("long", nNo) + "tape"
                Dim sSymbol As String = Str(nNo) + "tape"
                Dim strLab As String = Label1(nn - 1).Text
                If nNo = 0 Then
                    strLab = "Pleat"
                End If
                Dim xyTape As LGLEGDIA1.XY
                PR_MakeXY(xyTape, xyPt1.X, nTickHt)
                PR_DrawRuler(sSymbol, xyTape, strLab)
                'If (!Symbol("find", sSymbol)) Then
                '    Exit(%cancel, "Can't find a symbol to insert\nCheck your installation, that JOBST.SLB exists")
                'End If               
                'AddEntity("symbol", sSymbol, xyPt1.X, nTickHt)
                ''-----------------------
                While (ii <= jj)
                    Dim nTickLength As Double = 0.22
                    ''---------AddEntity("line", xyPt1.x, nTickHt, xyPt1.x + nTickLength, nTickHt)
                    Dim xyStart, xyEnd As LGLEGDIA1.XY
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt)
                    PR_DrawLine(xyStart, xyEnd)
                    '' Add Tick Text
                    Dim nTextMode As Double
                    If (ii < 10) Then
                        ''--------SetData("TextHorzJust", 4)
                        nTextMode = 3
                    Else
                        ''------SetData("TextHorzJust", 2)
                        nTextMode = 1
                    End If
                    ''--------AddEntity("text", MakeString("long", ii), xyPt1.x + nSeam, nTickHt - 0.01, 0.06, 0.1, 90)
                    PR_MakeXY(xyText, xyPt1.X + nSeam, nTickHt - 0.01)
                    PR_DrawText(Str(ii), xyText, 0.1, (90 * (LGLEGDIA1.PI / 180)), nTextMode)
                    nTickLength = 0.05
                    ''-----AddEntity("line", xyPt1.x, nTickHt + nTickStep, xyPt1.x + nTickLength, nTickHt + nTickStep)
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt + nTickStep)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt + nTickStep)
                    PR_DrawLine(xyStart, xyEnd)
                    ''---------AddEntity("line", xyPt1.x, nTickHt + nTickStep * 3, xyPt1.x + nTickLength, nTickHt + nTickStep * 3)
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt + nTickStep * 3)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt + nTickStep * 3)
                    PR_DrawLine(xyStart, xyEnd)
                    ''------AddEntity("line", xyPt1.x, nTickHt + nTickStep * 5, xyPt1.x + nTickLength, nTickHt + nTickStep * 5)
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt + nTickStep * 5)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt + nTickStep * 5)
                    PR_DrawLine(xyStart, xyEnd)
                    ''------AddEntity("line", xyPt1.x, nTickHt + nTickStep * 7, xyPt1.x + nTickLength, nTickHt + nTickStep * 7)
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt + nTickStep * 7)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt + nTickStep * 7)
                    PR_DrawLine(xyStart, xyEnd)
                    nTickLength = 0.08
                    ''--------AddEntity("line", xyPt1.x, nTickHt + nTickStep * 2, xyPt1.x + nTickLength, nTickHt + nTickStep * 2)
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt + nTickStep * 2)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt + nTickStep * 2)
                    PR_DrawLine(xyStart, xyEnd)
                    ''--------AddEntity("line", xyPt1.x, nTickHt + nTickStep * 6, xyPt1.x + nTickLength, nTickHt + nTickStep * 6)
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt + nTickStep * 6)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt + nTickStep * 6)
                    PR_DrawLine(xyStart, xyEnd)
                    nTickLength = 0.12
                    ''---------AddEntity("line", xyPt1.x, nTickHt + nTickStep * 4, xyPt1.x + nTickLength, nTickHt + nTickStep * 4)
                    PR_MakeXY(xyStart, xyPt1.X, nTickHt + nTickStep * 4)
                    PR_MakeXY(xyEnd, xyPt1.X + nTickLength, nTickHt + nTickStep * 4)
                    PR_DrawLine(xyStart, xyEnd)
                    ii = ii + 1
                    ''------------nTickHt = LGLEGDIA1.xyLegInsertion.y + nSeam + ii * nTickStep * 8
                    nTickHt = 0 + nSeam + ii * nTickStep * 8
                End While
            End If
            nn = nn + 1
            ''-----GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            ScanLine(sLine, nNo, sScale, nSpace, n20Len, nReduction)

            '' draw patient data at HEEL tape
            '' NOTE Heel tape Is tape #7 in this case
            Dim nTxtY, nTxtX As Double
            If ((nn = 7) Or (FootLess And nn = nStyleFirstTape + 2)) Then
                ARMDIA1.PR_SetLayer("Notes")
                ''------------SetData("TextVertJust", 32)     ''// Bottom
                ''------------SetData("TextHorzJust", 1)      ''// Left
                ''------------SetData("TextHeight", 0.1)
                ''----------nTxtY = LGLEGDIA1.xyLegInsertion.y + nSeam + 2.25
                nTxtY = 0 + nSeam + 2.25
                nTxtX = xyPt1.X
                Dim sText As String
                If (txtLeg.Text.Equals("Left")) Then
                    sText = "Left Leg  " + Chr(10)
                Else
                    sText = "Right Leg  " + Chr(10)
                End If
                '' Data
                sText = sText + " " + txtPatientName.Text + Chr(10) + " " + sWorkOrder
                If (nFabricClass = 0) Then
                    sText = sText + Chr(10) + " " + StringMiddle(txtFabric.Text, 5, txtFabric.Text.Length - 4)
                Else
                    sText = sText + Chr(10) + " " + txtFabric.Text
                End If
                ''------AddEntity("text", sText, nTxtX, nTxtY)
                Dim xyText1 As LGLEGDIA1.XY
                PR_MakeXY(xyText1, nTxtX, nTxtY)
                PR_DrawMText(sText, xyText1, False)

                ARMDIA1.PR_SetLayer("Construct")
                sText = " " + txtFileNo.Text + Chr(10) + " " + txtDiagnosis.Text + Chr(10) + " " + txtAge.Text + Chr(10) +
                    " " + txtSex.Text + Chr(10) + " " + txtLeftTemplate.Text
                ''--------AddEntity("text", sText, nTxtX, nTxtY - 0.75)
                PR_MakeXY(xyText1, nTxtX, nTxtY - 0.75)
                PR_DrawMText(sText, xyText1, False)

                '' Reset Setup text defaults 
                ''--------SetData("TextFont", 0)
                ''---------SetData("TextVertJust", 8)
                ''--------SetData("TextHorzJust", 1)
                ''---------SetData("TextHeight", 0.125)
                ''------SetData("TextAspect", 0.6)
            End If
        End While
        '' xyO Is the extreme right of drawing at this stage
        Dim xyO As LGLEGDIA1.XY
        xyO.X = xyPt1.X
        ''--------xyO.y = LGLEGDIA1.xyLegInsertion.y
        ''-----Close("file", hChan) 
        FileClose()


        'Calculate Foot Points (If a foot exists)
        'Draw Leg and foot profile

        If Not FootLess Then
            ''--------PR_PutLine("@" & g_sPathJOBST & "\WAIST\WHFTPNTS.D;")
            ''----------PR_PutLine("@" & g_sPathJOBST & "\LEG\LGLEGDWG.D;")
            PR_CalculateFootPoints(nFabricClass, nFirstTape, nStyleLastRed, nReductionAnkle)
        End If

        'Footless ThighLength and Thigh/Knee Bands
        If FootLess Then
            ''-----------PR_PutLine("@" & g_sPathJOBST & "\LEG\LGBNDDWG.D;")
            PR_DrawLegForThighLand(nFabricClass, nStyleLastRed, xyO, nB, nA)
        End If

        'Draw closing lines
        Select Case nLegStyle
            Case 0
                ''------------PR_PutLine("@" & g_sPathJOBST & "\LEG\LGANKCLS.D;")
                ''-------------Execute("menu", "SetLayer", hTemplateLayer)
                If (SmallHeel) Then
                    xyO.X = xyO.X - 0.375
                Else
                    xyO.X = xyO.X - 0.75
                End If

                ''----------xyPt1 = CalcXY("relpolar", xyProfileLast, 0.25, 270)
                PR_CalcPolar(xyProfileLast, 0.25, 270, xyPt1)
                ''---------AddEntity("line", xyPt1, xyProfileLast)
                PR_DrawLine(xyPt1, xyProfileLast)
                ''----------hEnt = AddEntity("line", xyPt1, xyO) ;
                ''-------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "ClosingLine")
                PR_DrawLine(xyPt1, xyO)
                PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "ClosingLine")
                Dim xyEnd As LGLEGDIA1.XY
                ''-------If (LGLEGDIA1.xyLegInsertion.X < xyToeSeam.X) Then
                If (xyEnd.X < xyToeSeam.X) Then
                    ''-------AddEntity("line", xyO, xyToeSeam.x, xyOtemplate.y)
                    PR_MakeXY(xyEnd, xyToeSeam.X, 0)
                    PR_DrawLine(xyO, xyEnd)
                Else
                    ''------AddEntity("line", xyO, xyOtemplate)
                    PR_MakeXY(xyEnd, 0, 0)
                    PR_DrawLine(xyO, xyEnd)
                End If

                ''// Seam TRAM Lines
                ARMDIA1.PR_SetLayer("Notes")
                ''--------AddEntity("line", xyToeSeam.x, xyO.y + nSeam + 0.5, xyO.X, xyO.y + nSeam + 0.5)
                Dim xyStart As LGLEGDIA1.XY
                PR_MakeXY(xyStart, xyToeSeam.X, xyO.y + nSeam + 0.5)
                PR_MakeXY(xyEnd, xyO.X, xyO.y + nSeam + 0.5)
                PR_DrawLine(xyStart, xyEnd)
                ''----AddEntity("line", xyToeSeam.x, xyO.y + nSeam, xyO.X, xyO.y + nSeam);
                PR_MakeXY(xyStart, xyToeSeam.X, xyO.y + nSeam)
                PR_MakeXY(xyEnd, xyO.X, xyO.y + nSeam)
                PR_DrawLine(xyStart, xyEnd)
            Case 1, 2
                ''-------PR_PutLine("@" & g_sPathJOBST & "\LEG\LGLEGCLS.D;")
                PR_DrawClosingLines(xyO, xyProfileLast, nB, nA)
        End Select

        'Close macro file
        FileClose(fNum)
        If chkHeelContracture.Checked = True Then
            ''PR_DrawKneeContracture()
            PR_DrawHeelContracture()
        End If
        If chkStump.Checked = True Then
            PR_DrawLegStump()
        End If

    End Sub

    Private Sub PR_EnableJOBSTEX()
        'Set up the display for JOBSTEX fabric
        '
        Dim iDisable, ii, nn, iEnable As Short
        Dim nValue As Double

        cboLeftTemplate.Items.Clear()
        cboLeftTemplate.Items.Add("13 DS")
        cboLeftTemplate.Items.Add("09 DS")
        cboLeftTemplate.Enabled = False

        'Set value
        For ii = 0 To (cboLeftTemplate.Items.Count - 1)
            If VB6.GetItemString(cboLeftTemplate, ii) = txtLeftTemplate.Text Then
                cboLeftTemplate.SelectedIndex = ii
            End If
        Next ii

        'Set the display for leg depending on the ankle and fabric and options.
        'g_JOBSTEX_FL is a special case where all of the stretch at tapes above the
        'ankle are calculated
        '
        'Panty Legs
        'If a template is not given then
        'then set 13DS for JOBSTEX, this is first item in list
        '
        If g_iLtAnkle = 0 Then
            If cboLeftTemplate.SelectedIndex = -1 Then cboLeftTemplate.SelectedIndex = 0
            cboLeftTemplate.Enabled = False
            chkLeftZipper.Enabled = False
            chkLeftZipper.CheckState = System.Windows.Forms.CheckState.Unchecked
        End If


        If g_iLtAnkle <> 0 Then
            For ii = 0 To 2
                labLeftDisp(ii).Visible = True
            Next ii
            labLeftMaxStr.Visible = True
            labLeftDisp(1).Text = "Str"
            For ii = 0 To 3
                labLeftDisp(ii).Visible = True
            Next
            'Set display value depending on Ankle position
            iEnable = g_iLtAnkle
            If iEnable = 6 Then
                iDisable = 7
            Else
                iDisable = 6
            End If
            chkLeftZipper.Enabled = True
            txtLeftMM(iEnable).Visible = True
            txtLeftMM(iDisable).Visible = False
            lblGms(iDisable - 6).Text = ""
            lblRed(iDisable - 6).Text = ""
        End If


    End Sub

    Private Sub PR_EnablePOWERNET()

        Dim iDisable, ii, nn, iEnable As Short
        Dim nValue As Double

        cboLeftTemplate.Items.Clear()
        cboLeftTemplate.Items.Add("30mm Hg")
        cboLeftTemplate.Items.Add("35mm Hg")
        cboLeftTemplate.Items.Add("40mm Hg")
        cboLeftTemplate.Items.Add("50mm Hg")

        cboLeftTemplate.Enabled = True

        'Set value
        For ii = 0 To (cboLeftTemplate.Items.Count - 1)
            If VB6.GetItemString(cboLeftTemplate, ii) = txtLeftTemplate.Text Then
                cboLeftTemplate.SelectedIndex = ii
            End If
        Next ii

        'Set the display for each leg depending on the ankle and fabric and options.
        '     g_XtAnkle = 0   => Panty leg
        '     g_XtAnkle = 6   => Ankle at +1-1/2
        '     g_XtAnkle = 0   => Ankle at +3
        '

        'Panty Legs
        'If a template is not given then
        'then set to 35mm Hg for Powernet

        If g_iLtAnkle = 0 Then
            If cboLeftTemplate.SelectedIndex = -1 Then cboLeftTemplate.SelectedIndex = 1
            chkLeftZipper.Enabled = False
            chkLeftZipper.CheckState = System.Windows.Forms.CheckState.Unchecked
        End If

        If g_iLtAnkle <> 0 Then
            For ii = 0 To 2
                labLeftDisp(ii).Visible = True
            Next ii
            labLeftDisp(1).Text = "Gms"
            labLeftDisp(3).Visible = False
            labLeftMaxStr.Visible = False
            For ii = 0 To 2
                labLeftDisp(ii).Visible = True
            Next

            'Set display value depending on Ankle position
            iEnable = g_iLtAnkle
            If iEnable = 6 Then
                iDisable = 7
            Else
                iDisable = 6
            End If

            chkLeftZipper.Enabled = True

            txtLeftMM(iEnable).Visible = True
            txtLeftMM(iDisable).Visible = False
            lblGms(iDisable - 6).Text = ""
            lblRed(iDisable - 6).Text = ""
        End If

    End Sub

    Private Sub PR_EstablishAnkles()
        'For both legs
        'Setup global variables.

        Dim ii, nn As Short
        Dim nValue As Double

        'Find ankle tapes
        'Note:
        '     This depends on the heel dimension.
        '     For a heel of less than 9" the ankle is at the +1-1/2 tape
        '     otherwise it is at the +3 tape.
        '     The exception to this rule is that if one leg is at 1-1/2 and one
        '     is at +3, then both legs must be figured at the +3 tape.
        '
        '     Later a check is made to see if there are enough tapes to draw the
        '     foot properley, at this point we are not interested we just want the
        '     ankles.
        '
        '     g_XtAnkle = 0   => Panty leg
        '     g_XtAnkle = 6   => Ankle at +1-1/2
        '     g_XtAnkle = 7   => Ankle at +3
        '
        ' Note also use of LAST global variables

        g_iLtAnkle = 0
        g_nLtLastAnkle = 0
        g_nLtLastHeel = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(5).Text))
        If g_nLtLastHeel <> 0 Then
            If g_nLtLastHeel < 9 Then
                g_iLtAnkle = 6 'Small heel Ankle at +1 1/2 tape
            Else
                g_iLtAnkle = 7 'Large heel Ankle at +3 tape
            End If
        End If

        g_nLtLastAnkle = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(g_iLtAnkle).Text))

    End Sub

    Private Sub PR_FigureLeftAnkle()
        'Figures the Pressure/Stretch at the LEFT ankle
        'Input from form:-
        '   Left(5-7) where     Left(5) = Heel Tape
        '                       Left(6) = Ankle Tape (Heel < 9")
        '                       Left(7) = Ankle Tape (Heel > 9")
        '   cboFabric
        '   chkLeftZipper
        '   txtLeftMM(g_iLtAnkle)
        '   txtLeftStretch(g_iLtAnkle)   JOBSTEX fabric only
        '
        'Input from globals
        '   g_JOBSTEX           Flag to indicate use of JOBSTEX fabric
        '   g_JOBSTEX_FL        Flag to indicate that JOBSTEX fabric in use
        '                       and that the stretch will be calculated at all
        '                       leg tape
        '   g_POWERNET          Flag to indicate use of Powernet fabric
        '
        '   g_iLtAnkle          Indicates the Left ankle tape
        '   g_nLtLastHeel
        '   g_nLtLastAnkle
        '   g_iLtLastMM
        '   g_iLtLastStretch    JOBSTEX fabric only
        '   g_iLtLastZipper
        '   g_iLastFabric
        '
        'JOBSTEX fabric Input and Output
        '   txtLeftMM(g_iLtAnkle)        User Input, if txtLeftStretch has been modified.
        '   txtLeftStretch(g_iLtAnkle)   User Input, if txtLeftMM has been modified.
        '   labLeftRed(g_iLtAnkle)       Displays Reduction
        '   labLeftMaxStr(2)             Displays MaxStretch
        '
        'Note:-
        '   The user inputs a required pressure in MMHg the stretch is then calculated
        '   and the stretch dependant values displayed.
        '   Alternately the stretch can be entered and the pressure in MMHg calculated.
        '   Given input of both (eg at link close) then MMHg takes precidence
        '
        'POWERNET fabric Input and Output
        '   txtLeftMM(g_iLtAnkle)            User Input
        '   txtLeftStretch(g_iLtAnkle)       not used (disabled)
        '   labLeftFiguredGrams(g_iLtAnkle)  Displays Grams
        '   labLeftRed(g_iLtAnkle)           Displays Reduction
        'Note:-
        '   The user inputs a required pressure in MMHg the reduction is then calculated
        '   and the reduction dependant values displayed.
        '
        '
        Dim nMMHg, nStretch As Double
        Dim nAnkleCir, nHeelCir As Double
        Dim nTapeCir As Double
        Dim iFabric, iZipper, iPrevMMHg As Short
        Dim iGrams, iModulus, iReduction As Short
        Dim ii As Short
        Dim nLastTape, nLastTapeMMHg As Short
        Dim nLastTapeCir As Double

        'Clean Messaage box
        labMessage.Text = ""

        'Establish if enough data exists to calculate.
        'If not then EXIT quietly

        'Leg tapes at heel and ankles and Fabric
        If Val(txtLeft(5).Text) = 0 Or Val(txtLeft(g_iLtAnkle).Text) = 0 Or cboFabric.SelectedIndex = -1 Then
            Exit Sub
        End If

        'MMHg for POWERNET fabric
        If LGLEGDIA1.g_POWERNET = True And Val(txtLeftMM(g_iLtAnkle).Text) = 0 Then Exit Sub

        'MMHg and Stretch for JOBSTEX fabric
        If (LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True) And Val(txtLeftMM(g_iLtAnkle).Text) = 0 Then Exit Sub

        'Fabric Selection
        'NOTE:-
        '    The POWERNET fabric is based on the file g_sPathJOBST & "\WHFABRIC.DAT
        '    For clarity this file can contain blank lines.  Therefor we must check
        '    that the fabric selected is not blank.
        If VB6.GetItemString(cboFabric, cboFabric.SelectedIndex) = "" Then Exit Sub

        'Establish if any of the inputs have changed compared to the last stored
        'values.
        'If they have changed then recalculate and redisplay
        'NB  Use of GOTO, (structured programmers get stuffed!)

        'Heel Tape
        If LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(5).Text)) <> g_nLtLastHeel Then GoTo CALC

        'Ankle Tape
        If LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(g_iLtAnkle).Text)) <> g_nLtLastAnkle Then GoTo CALC

        'Pressure MMHg
        If Val(txtLeftMM(g_iLtAnkle).Text) <> g_iLtLastMM Then GoTo CALC

        'Zippers
        If chkLeftZipper.CheckState <> g_iLtLastZipper Then GoTo CALC

        'Fabric
        If cboFabric.SelectedIndex <> g_iLtLastFabric Then GoTo CALC

        'If nothing has changed then Exit this sub
        Exit Sub

        'If we get to here we can assume that there is enough data to process and
        'we can calculate the relevant values.
CALC:
        'Calculate stretch based on given pressure (MMHg).
        'NB  All dimensions are converted to in inches except that last dimesions
        '    are stored using their display values.

        nHeelCir = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(5).Text))
        g_nLtLastHeel = nHeelCir

        nAnkleCir = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(g_iLtAnkle).Text))
        g_nLtLastAnkle = nAnkleCir

        iZipper = chkLeftZipper.CheckState
        g_iLtLastZipper = iZipper

        g_iLtLastFabric = cboFabric.SelectedIndex
        nLastTape = -1

        nMMHg = Val(txtLeftMM(g_iLtAnkle).Text)
        If LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True Then
            'JOBSTEX - Fabric
            iFabric = Val(VB.Left(VB6.GetItemString(cboFabric, cboFabric.SelectedIndex), 2))
            nStretch = ARMDIA1.round(FN_JOBSTEX_Stretch(nAnkleCir, nHeelCir, nMMHg, iZipper, iFabric))

            'Calculate and Display the other stretch dependent values
            PR_DisplayFiguredAnkle("Left", nStretch, nAnkleCir, nHeelCir, iFabric)

            g_iLtLastStretch = nStretch

            'g_iLtRed(g_iLtAnkle) this is set in PR_DisplayFiguredAnkle above
            g_iLtStretch(g_iLtAnkle) = nStretch
            g_iLtMM(g_iLtAnkle) = nMMHg


            'Check if template is set
            If cboLeftTemplate.SelectedIndex = -1 Then cboLeftTemplate.SelectedIndex = iZipper

        Else
            'POWERNET - Fabric
            iGrams = ARMDIA1.round(nAnkleCir * nMMHg)
            iModulus = Val(Mid(VB6.GetItemString(cboFabric, cboFabric.SelectedIndex), 5, 3))
            iReduction = FN_POWERNET_Reduction(iGrams, iModulus)
            'Exit if error
            If iReduction < 0 Then
                PR_DisplayErrorMessage(iReduction, "Error - Left Ankle Figuring")
                Exit Sub
            End If

            'Minimum Reduction for all diagnosis
            If iReduction < 14 Then
                iReduction = 14
                nMMHg = FN_POWERNET_Pressure(iReduction, nAnkleCir, iModulus)
                iGrams = ARMDIA1.round(nAnkleCir * nMMHg)
                PR_DisplayErrorMessage(1001, "Left Ankle Figuring")
            End If

            'Maximum Reduction for Burns
            If iReduction > 26 And txtDiagnosis.Text = "Burns" Then
                iReduction = 26
                nMMHg = FN_POWERNET_Pressure(iReduction, nAnkleCir, iModulus)
                iGrams = ARMDIA1.round(nAnkleCir * nMMHg)
                PR_DisplayErrorMessage(1002, "Left Ankle Figuring")
            End If

            'Maximum reduction for all other diagnosis
            If iReduction > 32 Then
                iReduction = 32
                nMMHg = FN_POWERNET_Pressure(iReduction, nAnkleCir, iModulus)
                iGrams = ARMDIA1.round(nAnkleCir * nMMHg)
                PR_DisplayErrorMessage(1003, "Left Ankle Figuring")
            End If

            'Display Calculations
            txtLeftMM(g_iLtAnkle).Text = Str(nMMHg)
            'grdLeftDisplay.Row = g_iLtAnkle - 6
            'grdLeftDisplay.Col = 0
            lblGms(g_iLtAnkle - 6).Text = Str(iGrams)
            'grdLeftDisplay.Text = Str(iGrams)
            'grdLeftDisplay.Col = 1
            'grdLeftDisplay.Text = Str(iReduction)
            lblRed(g_iLtAnkle - 6).Text = Str(iReduction)

            'Set Template
            Select Case iReduction
                Case 0 To 14, 14 To 18
                    cboLeftTemplate.SelectedIndex = 0 '30 mmhg
                Case 19 To 21
                    cboLeftTemplate.SelectedIndex = 1 '35 mmhg
                Case 21 To 23
                    cboLeftTemplate.SelectedIndex = 2 '40 mmhg
                Case 24 To 32, Is > 32
                    cboLeftTemplate.SelectedIndex = 3 '50 mmhg
            End Select

            'Max Stretch (Only if no zipper given)
            If chkLeftZipper.CheckState = 0 Then
                nHeelCir = nHeelCir / 4
                nAnkleCir = (nAnkleCir * ((100 - iReduction) / 100)) / 2
                If nAnkleCir < nHeelCir Then PR_DisplayErrorMessage(1004, "Left Ankle Figuring")
            End If

            g_iLtStretch(g_iLtAnkle) = iGrams 'Bit of a misnomer but what the Hell!
            g_iLtRed(g_iLtAnkle) = iReduction
            g_iLtMM(g_iLtAnkle) = nMMHg

        End If
        g_iLtLastMM = Val(txtLeftMM(g_iLtAnkle).Text)

        'Calculate the pressure at all leg tapes
        '
        If g_JOBSTEX_FL = True Then
            'iZipper = 0     'Don't use zipper

            'Establish Last tape position
            'This allows for the addition of leg tapes to extend up the leg
            'It's not very efficient but what the hell!!!!
            nLastTape = 0
            For ii = g_iLtAnkle + 1 To 29
                If txtLeft(ii).Text = "" Then Exit For
            Next ii
            If ii = 29 Then
                nLastTape = 29
            Else
                nLastTape = ii - 1
            End If

            'Establish pressure at last tape
            'As the last tape is at a 20 reduction we know that the stretch must
            'be 25%
            nLastTapeCir = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(nLastTape).Text))
            nLastTapeMMHg = LGLEGDIA1.round(FN_JOBSTEX_Pressure(nLastTapeCir, nHeelCir, 25, iZipper, iFabric))

            txtLeftMM(nLastTape).Text = CStr(nLastTapeMMHg)
            g_iLtStretch(nLastTape) = 25
            g_iLtRed(nLastTape) = 20
            g_iLtMM(nLastTape) = nLastTapeMMHg

            'First pass from Ankle to LastTape
            nStretch = g_iLtLastStretch
            iPrevMMHg = g_iLtLastMM
            For ii = g_iLtAnkle + 1 To nLastTape - 1
                nTapeCir = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(ii).Text))
                nMMHg = 1000 'Force While
                While nMMHg > iPrevMMHg And nStretch > 1
                    nMMHg = ARMDIA1.round(FN_JOBSTEX_Pressure(nTapeCir, nHeelCir, nStretch, iZipper, iFabric))
                    If nMMHg > iPrevMMHg Then nStretch = nStretch - 1
                End While
                txtLeftMM(ii).Text = CStr(nMMHg)
                g_iLtMM(ii) = nMMHg
                iPrevMMHg = nMMHg
                g_iLtStretch(ii) = nStretch
                g_iLtRed(ii) = ARMDIA1.round((1 - 1 / (0.01 * nStretch + 1)) * 100)
            Next ii

            'Second pass from LastTape to Ankle
            'Based on last tape Pressure recalculate the tapes until the pressures
            'become the same
            iPrevMMHg = nLastTapeMMHg

            For ii = nLastTape - 1 To g_iLtAnkle + 1 Step -1
                nTapeCir = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(ii).Text))

                nMMHg = Val(txtLeftMM(ii).Text)
                nStretch = g_iLtStretch(ii)
                If nMMHg >= iPrevMMHg Then Exit For
                While nMMHg < iPrevMMHg
                    nMMHg = ARMDIA1.round(FN_JOBSTEX_Pressure(nTapeCir, nHeelCir, nStretch, iZipper, iFabric))
                    If nMMHg < iPrevMMHg Then nStretch = nStretch + 1
                End While
                txtLeftMM(ii).Text = CStr(nMMHg)
                g_iLtMM(ii) = nMMHg
                iPrevMMHg = nMMHg
                g_iLtStretch(ii) = nStretch
                g_iLtRed(ii) = ARMDIA1.round((1 - 1 / (0.01 * nStretch + 1)) * 100)
            Next ii

            'Display Values
            'STRETCH
            'grdLeftDisplay.Col = 0
            For ii = g_iLtAnkle + 1 To nLastTape
                'grdLeftDisplay.Row = ii - 6
                'grdLeftDisplay.Text = Str(g_iLtStretch(ii))
                lblGms(ii - 6).Text = Str(g_iLtStretch(ii))
            Next ii

            'REDUCTION
            'grdLeftDisplay.Col = 1
            For ii = g_iLtAnkle + 1 To nLastTape
                'grdLeftDisplay.Row = ii - 6
                'grdLeftDisplay.Text = Str(g_iLtRed(ii))
                lblRed(ii - 6).Text = Str(g_iLtRed(ii))
            Next ii

        End If

    End Sub

    Private Sub PR_FigureLeftTape(ByRef Index As Short)
        'Figures the Pressure/Stretch at a LEFT LegTape
        'This is only available to JOBSTEX_FL
        'See PR_FigureLeftTape for details

        Dim nMMHg, nStretch As Double
        Dim nTapeCir As Double
        Dim iZipper, iFabric As Short

        'Establish if enough data exists to calculate.
        'If not then EXIT quietly
        If Val(txtLeftMM(Index).Text) <= 0 Then Exit Sub

        'Establish if pressure has changed from last time
        'If it hasn't then exit
        'This is used to cure a problem in that the stretch for a tape is not based not on the
        'pressure but on the previous or subsequent stretch, depending if it was found
        'on the forward pass or the backward pass.
        'The pressure was then back calculated fron the stretch.
        'We had a problem in that the same pressue can give a different stretch
        'To stop this happening when tabbing through the pressures this check has
        'been introduced
        '
        If Val(txtLeftMM(Index).Text) = g_iLtMM(Index) Then Exit Sub


        'MMHg and Stretch for JOBSTEX fabric
        'Don't Calculate if no values for Ankle tape
        If (LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True) And Val(txtLeftMM(g_iLtAnkle).Text) = 0 Or g_iLtStretch(g_iLtAnkle) = 0 Then Exit Sub


        'If we get to here we can assume that there is enough data to process and
        'we can calculate the relevant values.

        nTapeCir = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(Index).Text))
        iZipper = chkLeftZipper.CheckState
        iFabric = Val(VB.Left(VB6.GetItemString(cboFabric, cboFabric.SelectedIndex), 2))
        nMMHg = Val(txtLeftMM(Index).Text)
        g_iLtMM(Index) = nMMHg

        'Calculate stretch
        nStretch = JOBSTEX.FN_JOBSTEX_Stretch(nTapeCir, g_nLtLastHeel, nMMHg, iZipper, iFabric)

        'Store values
        g_iLtStretch(Index) = ARMDIA1.round(nStretch)
        g_iLtRed(Index) = ARMDIA1.round((1 - 1 / (0.01 * nStretch + 1)) * 100)

        'Display values
        'Note, the display grid displays from the ankle only
        '
        'grdLeftDisplay.Row = Index - 6 'Display from ankle
        'grdLeftDisplay.Col = 0
        'grdLeftDisplay.Text = Str(g_iLtStretch(Index))
        lblGms(Index - 6).Text = Str(g_iLtStretch(Index))
        'grdLeftDisplay.Col = 1
        'grdLeftDisplay.Text = Str(g_iLtRed(Index))
        lblRed(Index - 6).Text = Str(g_iLtRed(Index))

    End Sub

    Private Sub PR_FirstTapeDisplay(ByRef sType As String)

        If sType = "Disabled" Then
            txtFirstTape.Text = ""
            txtFirstTape.Enabled = False
            labFirstTape.Enabled = False
            If g_iStyleFirstTape >= 0 And g_iStyleFirstTape <= 29 Then Label1(g_iStyleFirstTape).ForeColor = System.Drawing.ColorTranslator.FromOle(RGB(0, 0, 0))
            '---------spnFirstTape.Enabled = False
            ''Added for #159 in the issue list
            cboFirstTape.SelectedIndex = -1
            cboFirstTape.Enabled = False
        Else
            txtFirstTape.Enabled = True
            labFirstTape.Enabled = True
            '----------spnFirstTape.Enabled = True
            ''Added for #159 in the issue list
            cboFirstTape.Enabled = True
        End If

    End Sub

    Private Sub PR_HeelChange(ByRef sLeg As String)
        'Procedure to modify the display if heel changed

        Dim iLtExistingAnkle, iRtExistingAnkle As Short
        'Clean Message box
        ''labMessage.Text = ""

        'Exit if heel has not been changed
        If LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(5).Text)) = g_nLtLastHeel Then Exit Sub

        'Store existing ankle position and get new ones
        iLtExistingAnkle = g_iLtAnkle

        PR_EstablishAnkles()

        'If revised the same as existing then exit
        If iLtExistingAnkle = g_iLtAnkle Then Exit Sub

        'Modify display
        PR_ResetFormLGLEGDIA()

        'Disable / Enable Anklet | Knee
        If g_iLtAnkle = 0 Then
            optType(0).Enabled = False
            optType(1).Enabled = False
            optType(2).Enabled = False
        Else
            optType(0).Enabled = True
            optType(1).Enabled = True
            optType(2).Enabled = True
        End If

        If LGLEGDIA1.g_POWERNET = True Then PR_EnablePOWERNET()
        If LGLEGDIA1.g_JOBSTEX = True Or g_JOBSTEX_FL = True Then PR_EnableJOBSTEX()
        ' If g_JOBSTEX_FL = True Then PR_EnableFL_JOBSTEX

    End Sub

    Private Sub PR_LastTapeDisplay(ByRef sType As String)
        If sType = "Disabled" Then
            txtLastTape.Text = ""
            txtLastTape.Enabled = False
            labLastTape.Enabled = False
            If g_iStyleLastTape >= 0 And g_iStyleLastTape <= 29 Then Label1(g_iStyleLastTape).ForeColor = System.Drawing.ColorTranslator.FromOle(RGB(0, 0, 0))
            '-------spnLastTape.Enabled = False
            ''Added for #159 in the issue list
            cboLastTape.SelectedIndex = -1
            cboLastTape.Enabled = False
        Else
            txtLastTape.Enabled = True
            labLastTape.Enabled = True
            '--------spnLastTape.Enabled = True
            ''Added for #159 in the issue list
            cboLastTape.Enabled = True
        End If

    End Sub

    Private Sub PR_LoadFabricFromFile(ByRef sFileName As String)
        'Procedure to load the POWERNET conversion chart from file
        'N.B. File opening Errors etc. are not handled (so tough titty!)

        Dim fnum, ii As Short
        fnum = FreeFile()
        FileOpen(fnum, sFileName, VB.OpenMode.Input)
        ii = 0
        Dim sstr As String = ""
        Do Until EOF(fnum)
            'Input(fnum, sstr)
            Input(fnum, LGLEGDIA1.POWERNET.Modulus(ii))
            Input(fnum, LGLEGDIA1.POWERNET.Conversion_Renamed(ii))
            ii = ii + 1
        Loop
        FileClose(fnum)

    End Sub

    Private Sub PR_PutLine(ByRef sLine As String)
        'Puts the contents of sLine to the opened "Macro" file
        'Puts the line with no translation or additions
        '    fNum is global variable
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, sLine)
    End Sub

    Private Sub PR_PutNumberAssign(ByRef sVariableName As String, ByRef nAssignedNumber As Object)

        'Procedure to put a number assignment
        'Adds a semi-colon

        'UPGRADE_WARNING: Couldn't resolve default property of object nAssignedNumber. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, sVariableName & "=" & Str(nAssignedNumber) & ";")


    End Sub

    Private Sub PR_PutStringAssign(ByRef sVariableName As String, ByRef sAssignedString As Object)
        'Procedure to put a string assignment
        'Encloses String in quotes and adds a semi-colon

        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, sVariableName & "=" & QQ & sAssignedString & QQ & ";")

    End Sub

    Private Sub PR_ResetFormLGLEGDIA()
        'Reset display of form to a clean state
        Dim ii As Short

        'Reset Max Stretch
        labLeftMaxStr.Text = ""
        labLeftMaxStr.Visible = False
        labLeftDisp(3).Visible = False

        'Clean Display grid
        For ii = 0 To 1 'First two rows only, used in this case

            'grdLeftDisplay.Row = ii
            'grdLeftDisplay.Col = 0
            'grdLeftDisplay.Text = ""
            'lblGms(ii).Text = ""
            'grdLeftDisplay.Col = 1
            'grdLeftDisplay.Text = ""
            'lblRed(ii).Text = ""

        Next ii

        'Switch off MM text boxes
        For ii = 6 To 7
            txtLeftMM(ii).Text = ""
            txtLeftMM(ii).Visible = False
            lblGms(ii - 6).Text = ""
            lblRed(ii - 6).Text = ""
        Next ii

        '
        For ii = 0 To 2
            labLeftDisp(ii).Visible = False
        Next ii

    End Sub

    Private Sub PR_SaveLeg()
        'Procedure to create a macro to save the leg data
        If txtUidLeftLeg.Text <> "" Then
            PR_PutLine("HANDLE  hLeg;")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "hLeg = UID (" & QQ & "find" & QC & Val(txtUidLeftLeg.Text) & ");")
        Else
            PR_PutLine("HANDLE  hLeg, hTitle, hLegTitle;")
            PR_PutLine("XY xyTitleOrigin, xyTitleScale;")
            PR_PutLine("ANGLE aTitleAngle;")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "hTitle = UID (" & QQ & "find" & QC & Val(txtUidTitle.Text) & ");")
            PR_PutStringAssign("sLeg", txtLeg.Text)

            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetSymbolLibrary( sPathJOBST + " & QQ & "\\JOBST.SLB" & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "if ( !Symbol(" & QQ & "find" & QCQ & "legleg" & QQ & "))")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "Exit(%cancel, " & QQ & "Can't find >legleg< symbol to insert\nCheck your installation, that JOBST.SLB exists" & QQ & ");")

            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "if ( hTitle )")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "GetGeometry( hTitle, &sTitleName, &xyTitleOrigin, &xyTitleScale, &aTitleAngle);")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "else")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "Exit(%cancel," & QQ & "Can't find > mainpatientdetails <, Use TITLE to insert Patient Data" & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "Execute(" & QQ & "menu" & QCQ & "SetLayer" & QC & "Table(" & QQ & "find" & QCQ & "layer" & QCQ & "Data" & QQ & "));")

            If txtUidLegTitle.Text = "" Then
                'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(fNum, "if ( !Symbol(" & QQ & "find" & QCQ & "legcommon" & QQ & "))")
                'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(fNum, "Exit(%cancel, " & QQ & "Can't find > legcommon < symbol to insert\nCheck your installation, that JOBST.SLB exists" & QQ & ");")
                'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(fNum, "hLegTitle = AddEntity(" & QQ & "symbol" & QCQ & "legcommon" & QC & "xyTitleOrigin);")
                'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(fNum, "SetDBData( hLegTitle" & CQ & "Fabric" & QCQ & txtFabric.Text & QQ & ");")
                'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(fNum, "SetDBData( hLegTitle" & CQ & "fileno" & QCQ & txtFileNo.Text & QQ & ");")
                'Run macro to setup DB fields
                'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                PrintLine(fNum, "@" & g_sPathJOBST & "\LEG\LGFIELDS.D;")

            End If

            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "if (StringCompare(" & QQ & "Left" & QC & "sLeg))")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "xyTitleOrigin.x = xyTitleOrigin.x + 1.5;")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "else")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "xyTitleOrigin.x = xyTitleOrigin.x + 3;")

            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "hLeg = AddEntity(" & QQ & "symbol" & QCQ & "legleg" & QC & "xyTitleOrigin);")
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData( hLeg, " & QQ & "fileno" & QCQ & txtFileNo.Text & QQ & ");")

        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "if (!hLeg)Exit(%cancel," & QQ & "Can't find LEGBOX to Update" & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "Leg" & QCQ & txtLeg.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "TapeLengthsPt1" & QCQ & Mid(txtLeftLengths.Text, 1, 60) & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "TapeLengthsPt2" & QCQ & Mid(txtLeftLengths.Text, 61, 60) & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "Pressure" & QCQ & txtLeftTemplate.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "FootLength" & QCQ & txtFootLength.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "Fabric" & QCQ & txtFabric.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "FootPleat1" & QCQ & txtFootPleat1.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "TopLegPleat1" & QCQ & txtTopLegPleat1.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "FootPleat2" & QCQ & txtFootPleat2.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "TopLegPleat2" & QCQ & txtTopLegPleat2.Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "ToeStyle" & QCQ & txtToeStyle.Text & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "Anklet" & QCQ & txtAnklet.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "ThighLength" & QCQ & txtThighLength.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "KneeLength" & QCQ & txtKneeLength.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "ThighBand" & QCQ & txtThighBandAK.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "ThighBandBK" & QCQ & txtThighBandBK.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hLeg, " & QQ & "KneeBand" & QCQ & txtKneeBand.Text & QQ & ");")

    End Sub

    Private Sub PR_SetFirstTape(ByRef iNewTape As Short)

        If iNewTape >= 0 And iNewTape <= 29 Then
            If g_iStyleFirstTape >= 0 And g_iStyleFirstTape <= 29 Then
                Label1(g_iStyleFirstTape).ForeColor = System.Drawing.ColorTranslator.FromOle(RGB(0, 0, 0))
            End If
            g_iStyleFirstTape = iNewTape
            Label1(iNewTape).ForeColor = System.Drawing.ColorTranslator.FromOle(RGB(255, 0, 0))
            txtFirstTape.Text = LTrim(Mid(g_sTextList, (iNewTape * 3) + 1, 3))
            ''Added for #159 in the issue list
            cboFirstTape.Text = txtFirstTape.Text
        End If

    End Sub

    Private Sub PR_SetLastTape(ByRef iNewTape As Short)

        If iNewTape >= 0 And iNewTape <= 29 Then
            If g_iStyleLastTape >= 0 And g_iStyleLastTape <= 29 Then
                Label1(g_iStyleLastTape).ForeColor = System.Drawing.ColorTranslator.FromOle(RGB(0, 0, 0))
            End If
            g_iStyleLastTape = iNewTape
            Label1(iNewTape).ForeColor = System.Drawing.ColorTranslator.FromOle(RGB(255, 0, 0))
            txtLastTape.Text = LTrim(Mid(g_sTextList, (iNewTape * 3) + 1, 3))
            ''Added for #159 in the issue list
            cboLastTape.Text = txtLastTape.Text
        End If

    End Sub

    Private Sub PR_SetTextData(ByRef nHoriz As Object, ByRef nVert As Object, ByRef nHt As Object, ByRef nAspect As Object, ByRef nFont As Object)
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
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetData(" & QQ & "TextHorzJust" & QC & nHoriz & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object nHoriz. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHorizJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            g_nCurrTextHorizJust = nHoriz
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object nVert. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextVertJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If nVert >= 0 And g_nCurrTextVertJust <> nVert Then
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetData(" & QQ & "TextVertJust" & QC & nVert & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object nVert. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextVertJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            g_nCurrTextVertJust = nVert
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object nHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If nHt >= 0 And g_nCurrTextHt <> nHt Then
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetData(" & QQ & "TextHeight" & QC & nHt & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object nHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            g_nCurrTextHt = nHt
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object nAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If nAspect >= 0 And g_nCurrTextAspect <> nAspect Then
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetData(" & QQ & "TextAspect" & QC & nAspect & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object nAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            g_nCurrTextAspect = nAspect
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object nFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If nFont >= 0 And g_nCurrTextFont <> nFont Then
            'UPGRADE_WARNING: Couldn't resolve default property of object fnum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetData(" & QQ & "TextFont" & QC & nFont & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object nFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            g_nCurrTextFont = nFont
        End If


    End Sub

    Private Sub PR_StripBadChar(ByRef Text_Box_Name As System.Windows.Forms.Control)
        'Strip the excess characters from the text boxes.
        'This is due to a DRAFIX bug with DDE Poke.
        Dim sString As String
        Dim iLength, ii As Short
        sString = Text_Box_Name.Text
        For ii = 1 To 2
            iLength = Len(sString)
            If iLength = 0 Then Exit For
            Select Case Asc(VB.Right(sString, 1))
                Case 32 To 126, 160 To 255
                    Exit For
                Case Else
                    sString = VB.Left(sString, iLength - 1)
            End Select
        Next ii
        Text_Box_Name.Text = sString
    End Sub

    Private Sub ShiftTapesDown_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ShiftTapesDown.Click
        'The effect of a single click on this button
        'is to shift all of the tapes in the direction of the
        'arrow
        Dim ii As Short
        Dim nValue As Double

        'Check that last tape is empty so that the previous
        'tape can be shifted into it
        If Len(txtLeft(29).Text) > 0 Then
            'Beep and exit function
            Beep()
            Exit Sub
        End If

        'Shift all tapes down 1
        For ii = 29 To 1 Step -1
            txtLeft(ii).Text = txtLeft(ii - 1).Text
            nValue = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(ii - 1).Text))
            '-----grdLeftInches.Row = ii
            '------grdLeftInches.Text = LGLEGDIA1.fnInchesToText(nValue)
        Next ii

        'Initiate Heel Change (if Required)
        If Val(txtLeft(5).Text) > 0 And g_iLtAnkle = 0 Then PR_HeelChange("Left")
        If Val(txtLeft(5).Text) = 0 And g_iLtAnkle <> 0 Then PR_HeelChange("Left")

        'For Thigh and Knee Bands move style first and last tapes
        If g_iLegStyle = 3 Or g_iLegStyle = 4 Then
            PR_SetFirstTape(g_iStyleFirstTape + 1)
            PR_SetLastTape(g_iStyleLastTape + 1)
        End If

        'Clean first position
        txtLeft(0).Text = ""
        '------grdLeftInches.Row = 0
        '-------grdLeftInches.Text = ""

    End Sub

    Private Sub ShiftTapesUp_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ShiftTapesUp.Click
        'The effect of a single click on this button
        'is to shift all of the tapes in the direction of the
        'arrow
        Dim ii As Short
        Dim nValue As Double

        'Check that first tape is empty so that the following
        'tape can be shifted into it
        If Len(txtLeft(0).Text) > 0 Then
            'Beep and exit Procedure
            Beep()
            Exit Sub
        End If

        'Shift all tapes up 1
        For ii = 1 To 29
            txtLeft(ii - 1).Text = txtLeft(ii).Text
            nValue = LGLEGDIA1.fnDisplaytoInches(Val(txtLeft(ii).Text))
            '------grdLeftInches.Row = ii - 1
            '------grdLeftInches.Text = LGLEGDIA1.fnInchesToText(nValue)
        Next ii

        'Initiate Heel Change (if Required)
        If Val(txtLeft(5).Text) > 0 And g_iLtAnkle = 0 Then PR_HeelChange("Left")
        If Val(txtLeft(5).Text) = 0 And g_iLtAnkle <> 0 Then PR_HeelChange("Left")

        'For Thigh and Knee Bands move style first and last tapes
        If g_iLegStyle = 3 Or g_iLegStyle = 4 Then
            If g_iLtAnkle = 0 Then
                PR_SetFirstTape(g_iStyleFirstTape - 1)
            Else
                If g_iStyleFirstTape - 1 >= g_iLtAnkle Then
                    PR_SetFirstTape(g_iStyleFirstTape - 1)
                Else
                    PR_SetFirstTape(g_iLtAnkle)
                End If
            End If
            PR_SetLastTape(g_iStyleLastTape - 1)
        End If

        'Clean last position
        txtLeft(29).Text = ""
        '-----grdLeftInches.Row = 29
        '-----grdLeftInches.Text = ""
    End Sub

    'Private Sub spnFirstTape_DownClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles spnFirstTape.DownClick

    '	Dim ii As Short
    '	If g_iStyleFirstTape <= 0 Or g_iStyleFirstTape - 1 < g_iFirstTape Then
    '		Beep()
    '		Exit Sub
    '	End If

    '	PR_SetFirstTape(g_iStyleFirstTape - 1)

    'End Sub

    'Private Sub spnFirstTape_UpClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles spnFirstTape.UpClick
    '	Dim ii As Short

    '	If g_iStyleFirstTape >= 28 Or g_iStyleFirstTape + 1 >= g_iStyleLastTape Then
    '		Beep()
    '		Exit Sub
    '	End If

    '	PR_SetFirstTape(g_iStyleFirstTape + 1)

    'End Sub

    'Private Sub spnLastTape_DownClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles spnLastTape.DownClick
    '	Dim ii As Short

    '	If g_iStyleLastTape <= 1 Or g_iStyleLastTape - 1 <= g_iStyleFirstTape Then
    '		Beep()
    '		Exit Sub
    '	End If

    '	PR_SetLastTape(g_iStyleLastTape - 1)

    'End Sub

    '   Private Sub spnLastTape_UpClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles spnLastTape.UpClick

    '	Dim ii As Short

    '	If g_iStyleLastTape >= 29 Or g_iStyleLastTape + 1 > g_iLastTape Then
    '		Beep()
    '		Exit Sub
    '	End If

    '	PR_SetLastTape(g_iStyleLastTape + 1)

    'End Sub

    Private Sub Tab_Renamed_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Tab_Renamed.Click
        'Allows the user to use enter as a tab

        System.Windows.Forms.SendKeys.Send("{TAB}")

    End Sub

    Private Sub Timer1_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Timer1.Tick
        'Timeout timer
        'If DRAFIX to VB DDE link fails then time out
        'after 6 seconds
        '-------------End
    End Sub

    Private Sub txtFootLength_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtFootLength.Enter
        LGLEGDIA1.Select_Text_In_Box(txtFootLength)
    End Sub

    Private Sub txtFootLength_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtFootLength.Leave
        ''-------LGLEGDIA1.Validate_And_Display_Text_In_Box(txtFootLength, labFootLengthInInches, -1)
        Dim sInchVal As String = ""
        LGLEGDIA1.Validate_And_Display_Text_In_Box(txtFootLength, sInchVal)
        If sInchVal <> "" Then
            labFootLengthInInches.Text = sInchVal
        End If
    End Sub

    Private Sub txtLeft_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLeft.Enter
        Dim Index As Short = txtLeft.GetIndex(eventSender)
        LGLEGDIA1.Select_Text_In_Box(txtLeft(Index))
    End Sub

    Private Sub txtLeft_KeyPress(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.KeyPressEventArgs) Handles txtLeft.KeyPress
        Dim KeyAscii As Short = Asc(eventArgs.KeyChar)
        Dim Index As Short = txtLeft.GetIndex(eventSender)
        'Use enter to act as a TAB
        '  If KeyAscii = NEWLINE Then txtLeft((Index + 1) Mod 30).SetFocus

        eventArgs.KeyChar = Chr(KeyAscii)
        If KeyAscii = 0 Then
            eventArgs.Handled = True
        End If
    End Sub

    Private Sub txtLeft_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLeft.Leave
        Dim Index As Short = txtLeft.GetIndex(eventSender)
        '--------LGLEGDIA1.Validate_And_Display_Text_In_Box(txtLeft(Index), grdLeftInches, Index)
        Dim sInchVal As String = ""
        LGLEGDIA1.Validate_And_Display_Text_In_Box(txtLeft(Index), sInchVal)
        If sInchVal <> "" Then
            lblLeft(Index).Text = sInchVal
        End If
        If Index = g_iLtAnkle Then PR_FigureLeftAnkle()
        If Index = 5 Then PR_HeelChange("Left")
        ''Added for #229 in the issue list
        Dim nLastTape As Short = -1
        Dim ii As Short
        For ii = 29 To 0 Step -1
            If Val(txtLeft(ii).Text) > 0 Then Exit For
        Next ii
        If ii >= 0 Then nLastTape = ii
        PR_SetLastTape(nLastTape)
    End Sub

    Private Sub txtLeftMM_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLeftMM.Leave
        Dim Index As Short = txtLeftMM.GetIndex(eventSender)
        If Index = g_iLtAnkle Then
            PR_FigureLeftAnkle()
        Else
            PR_FigureLeftTape(Index)
        End If
    End Sub

    Private Sub Update_DDE_Text_Boxes()
        'Called from OK_Click and Draw
        'Update the text boxes used for DDE transfer
        Dim ii As Short
        Dim sString, sJustifiedString As String
        Dim sFabricClass As String
        Dim iStr, iRed, iMM As Short
        Dim sStr, sRed, sMM As String
        Dim nValue As Single

        txtFabric.Text = VB6.GetItemString(cboFabric, cboFabric.SelectedIndex)
        txtToeStyle.Text = VB6.GetItemString(cboToeStyle, cboToeStyle.SelectedIndex)

        'LEFT Leg
        'Assume that data has been validated earlier

        'Set initial values
        txtLeftLengths.Text = ""
        txtLeftRed.Text = ""
        txtLeftStr.Text = ""
        txtLeftMMs.Text = ""
        g_iLastTape = -1
        g_iFirstTape = -1
        txtLeftTemplate.Text = VB6.GetItemString(cboLeftTemplate, cboLeftTemplate.SelectedIndex)

        For ii = 0 To 29
            nValue = Val(txtLeft(ii).Text)
            If nValue <> 0 Then
                nValue = nValue * 10 'Shift decimal place to right
                sJustifiedString = New String(" ", 4)
                sJustifiedString = RSet(Trim(Str(nValue)), Len(sJustifiedString))
            Else
                sJustifiedString = New String(" ", 4)
            End If

            'Tape values
            txtLeftLengths.Text = txtLeftLengths.Text & sJustifiedString

            'Set first and last tape (assumes no holes in data)
            If g_iFirstTape < 0 And nValue > 0 Then g_iFirstTape = ii + 1
            If g_iLastTape < 0 And g_iFirstTape > 0 And nValue = 0 Then g_iLastTape = ii
        Next ii

        If g_iLastTape < 0 Then g_iLastTape = 30

        'Where JOBSTEX_FL has been choosen then we update the MMs, Reduction and Stretch
        'These will then be picked up by the Leg Drawing modules
        If g_JOBSTEX_FL = True And g_iLtAnkle <> 0 Then
            For ii = 0 To 29
                iMM = g_iLtMM(ii)
                iRed = g_iLtRed(ii)
                iStr = g_iLtStretch(ii)
                If iMM <> 0 Then
                    sMM = New String(" ", 3)
                    sMM = RSet(Trim(Str(iMM)), Len(sMM))
                    sRed = New String(" ", 3)
                    sRed = RSet(Trim(Str(iRed)), Len(sRed))
                    sStr = New String(" ", 3)
                    sStr = RSet(Trim(Str(iStr)), Len(sStr))
                Else
                    sMM = New String(" ", 3)
                    sRed = New String(" ", 3)
                    sStr = New String(" ", 3)
                End If
                txtLeftMMs.Text = txtLeftMMs.Text & sMM
                txtLeftRed.Text = txtLeftRed.Text & sRed
                txtLeftStr.Text = txtLeftStr.Text & sStr
            Next ii
        End If

        ''Changed for #159 in the issue list
        Dim iFirstTape, iLastTape As Double
        For iFirstTape = 0 To 29
            Dim sFirstTape As String = LTrim(Mid(g_sTextList, (iFirstTape * 3) + 1, 3))
            If sFirstTape.Equals(txtFirstTape.Text) Then
                If g_iStyleFirstTape <> iFirstTape Then
                    g_iStyleFirstTape = iFirstTape
                End If
            End If
        Next
        For iLastTape = 0 To 29
            Dim sLastTape As String = LTrim(Mid(g_sTextList, (iLastTape * 3) + 1, 3))
            If sLastTape.Equals(txtLastTape.Text) Then
                If g_iStyleLastTape <> iLastTape Then
                    g_iStyleLastTape = iLastTape
                End If
            End If
        Next

        g_iStyleLastTape = g_iStyleLastTape + 1
        g_iStyleFirstTape = g_iStyleFirstTape + 1

        g_sStyleString = FN_BuildStyleString()

        Select Case g_iLegStyle
            Case 0
                txtAnklet.Text = g_sStyleString
            Case 1
                txtKneeLength.Text = g_sStyleString
            Case 2
                txtThighLength.Text = g_sStyleString
            Case 3
                txtKneeBand.Text = g_sStyleString
            Case 4
                txtThighBandAK.Text = g_sStyleString
            Case 5
                txtThighBandBK.Text = g_sStyleString
        End Select
        txtChosenStyle.Text = Str(g_iLegStyle)
    End Sub

    Private Function Validate_Data() As Short
        'Called from OK_Click

        Dim NL, sError, sTextList As String
        Dim sLeftError, sRightError As String
        Dim nFirstTape, ii, nLastTape As Short
        Dim iError As Short
        Dim nLargeHeelX As Double
        Dim nFootLength, nHeelLength, nTapeX As Double
        Dim nValue As Double

        NL = Chr(10) 'new line

        'LEFT LEG Checks
        'Check Tape length data text boxes for holes (ie missing values)
        'Establish First and last tape

        sError = ""

        For ii = 0 To 29 Step 1
            If Val(txtLeft(ii).Text) > 0 Then Exit For
        Next ii
        nFirstTape = ii

        For ii = 29 To 0 Step -1
            If Val(txtLeft(ii).Text) > 0 Then Exit For
        Next ii
        nLastTape = ii

        For ii = nFirstTape To nLastTape
            If Val(txtLeft(ii).Text) = 0 Then
                sError = sError & "Missing Tape length - " & LTrim(Mid(g_sTextList, (ii * 3) + 1, 3)) & NL
            End If
        Next ii

        'Check that a minimum of 3 tapes are given
        If (nLastTape - nFirstTape) < 2 Then
            sError = sError & "Minimum of 3 Tapes must be given." & NL
        End If

        'Check if -3 tape exists for Large Heel
        If g_iLtAnkle = 7 And Val(txtLeft(3).Text) = 0 And g_iLegStyle < 3 Then
            sError = sError & "As the Heel is 9 inches and over." & NL
            sError = sError & "and there is no -3 tape the foot will not draw properly " & NL
        End If

        'Check if -1 1/2 tape exists for Small Heel
        If g_iLtAnkle = 6 And Val(txtLeft(4).Text) = 0 And g_iLegStyle < 3 Then
            sError = sError & "As the Heel is smaller than 9 inches." & NL
            sError = sError & "and there is no -1 1/2 tape the foot will not draw properly " & NL
        End If

        'Check that figuring has been done at the ankle
        'Does not apply to Anklets or Bands
        If g_iLegStyle > 0 And g_iLegStyle < 3 And g_iLtAnkle <> 0 And (g_iLtStretch(g_iLtAnkle) = 0 Or g_iLtRed(g_iLtAnkle) = 0 Or g_iLtMM(g_iLtAnkle) = 0) Then
            sError = sError & "The Ankle has not been figured!." & NL
            sError = sError & "Figure the Ankle or use CANCEL to exit" & NL
        End If


        'Toe style need only be given if a heel tape is given, Heel is at tape 5
        If (g_iFirstTape < 5) And (cboToeStyle.SelectedIndex <= 0) And g_iLegStyle < 3 Then
            sError = sError & "Missing Toe Style." & NL
        End If

        'Check that a foot length has been given for self enclosed
        If VB6.GetItemString(cboToeStyle, cboToeStyle.SelectedIndex) = "Self Enclosed" And Val(txtFootLength.Text) = 0 Then
            sError = sError & "A foot length must be given for Self Enclosed toes." & NL
        End If

        'Check that a leg style has been chosen
        If g_iLegStyle < 0 Then
            sError = sError & "A Leg Style has not been chosen!." & NL
            sError = sError & "Choose a Style or use CANCEL to exit" & NL
        End If

        'General check
        If VB6.GetItemString(cboFabric, cboFabric.SelectedIndex) = "" Then
            sError = sError & "A Fabric has not been chosen!." & NL
        End If

        'General check for atemplate
        If cboLeftTemplate.SelectedIndex = -1 Then
            sError = sError & "A template has not been chosen!." & NL
        End If


        'Display Error message (if required) and return
        If Len(sError) > 0 Then
            MsgBox(sError, 48, "Errors in Data")
            Validate_Data = False
            Exit Function
        Else
            Validate_Data = True
        End If

        'For the combo boxes check if a toe selection has been made
        'and a heel tape is not given.
        'This is a none fatal error, the data will be ammended and
        'an information message given
        'will be given

        sError = ""
        If nFirstTape > 5 Then
            If cboToeStyle.SelectedIndex > 0 Then
                sError = sError & "Toe style, " & VB6.GetItemString(cboToeStyle, cboToeStyle.SelectedIndex) & NL
            End If
            cboToeStyle.SelectedIndex = 0
        End If

        'Display Warning message (if required)
        If Len(sError) > 0 Then
            sError = "The TOE style given can't apply. As there is no Heel." & NL
            sError = sError & "Removing TOE style to reflect this."
            MsgBox(sError, 64, "Warning - Errors in Data")
        End If



        'Yes / No type errors
        'In this case we warn the user that there is a problem!
        'They can continue or they can return to the dialog to make changes

        'Initialize error variables
        sError = ""
        iError = False

        'Make a stab at seeing if the foot length will be long enough.
        'For Toe = "Self enclosed" or Toe = "Soft enclosed" and a foot length is given
        'This checks that the given foot length is longer than the distance
        'from the heel to the -3 tape at the seam (-1 1/2 for heels less than 9 ")
        'it is a rough giude only.
        If g_nLtLastHeel > 0 And (VB6.GetItemString(cboToeStyle, cboToeStyle.SelectedIndex) = "Self Enclosed" Or (VB6.GetItemString(cboToeStyle, cboToeStyle.SelectedIndex) = "Soft Enclosed" And Val(txtFootLength.Text) <> 0)) Then

            'Set checking parameters based on fabric and template
            If LGLEGDIA1.g_POWERNET = True Then
                nLargeHeelX = 2.71875
                'Scale heel to the 0 tape of a 30mmHg template
                'This is the worst case for POWERNET fabric
                nHeelLength = g_nLtLastHeel * 0.402
                'Reduce heel by 3
                nHeelLength = nHeelLength + (3 * 0.05025)
            Else
                Select Case cboLeftTemplate.SelectedIndex
                    Case 0 ' 13 DS
                        nLargeHeelX = 2.73
                    Case 1 ' 09 DS
                        nLargeHeelX = 2.88
                End Select
                'Scale heel to the 0 tape of a JOBSTEX template
                nHeelLength = g_nLtLastHeel * 0.391
                'Reduce heel by 3
                nHeelLength = nHeelLength + (3 * 0.048875)
            End If

            If g_nLtLastHeel >= 9 Then
                nTapeX = nLargeHeelX 'Large heel, distance from heel to -3 tape
            Else
                nTapeX = 1.5 'Small heel, distance from heel to -1 1/2 tape
            End If

            nFootLength = LGLEGDIA1.fnDisplaytoInches(Val(txtFootLength.Text))

            nValue = System.Math.Sqrt(nHeelLength ^ 2 + nTapeX ^ 2)

            If nValue > nFootLength Then
                iError = True
                sError = sError & "The given foot length may be too small to correctly" & NL
                sError = sError & "position the toe!" & NL & NL
                sError = sError & "Do you wish to continue anyway ?"
            End If
        End If


        'Display Error message (if required) and return
        'These are non-fatal errors
        If iError = True Then
            iError = MsgBox(sError, 36, "Warning, Problems with data")
            If iError = IDYES Then
                Validate_Data = True
            Else
                Validate_Data = False
            End If
        Else
            Validate_Data = True
        End If

    End Function
    Function FN_GetNumber(ByVal sString As String, ByRef iIndex As Short) As Double
        'Function to return as a numerical value the iIndexth item in a string
        'that uses blanks (spaces) as delimiters.
        'EG
        '    sString = "12.3 65.1 45"
        '    FN_GetNumber( sString, 2) = 65.1
        '
        'If the iIndexth item is not found then return -1 to indicate an error.
        'This assumes that the string will not be used to store -ve numbers.
        'Indexing starts from 1

        Dim ii, iPos As Short

        'Initial error checking
        sString = Trim(sString) 'Remove leading and trailing blanks

        If Len(sString) = 0 Then
            FN_GetNumber = -1
            Exit Function
        End If

        'Prepare string
        sString = sString & " " 'Trailing blank as stopper for last item

        'Get iIndexth item
        For ii = 1 To iIndex
            iPos = InStr(sString, " ")
            If ii = iIndex Then
                sString = VB.Left(sString, iPos - 1)
                FN_GetNumber = Val(sString)
                Exit Function
            Else
                sString = LTrim(Mid(sString, iPos))
                If Len(sString) = 0 Then
                    FN_GetNumber = -1
                    Exit Function
                End If
            End If
        Next ii

        'The function should have exited befor this, however just in case
        '(iIndex = 0) we indicate an error,
        FN_GetNumber = -1

    End Function

    Function FN_GetString(ByVal sString As String, ByRef iIndex As Short) As String

        Dim ii, iPos As Short

        'Initial error checking
        sString = Trim(sString) 'Remove leading and trailing blanks

        If Len(sString) = 0 Then
            FN_GetString = ""
            Exit Function
        End If

        'Prepare string
        sString = sString & " " 'Trailing blank as stopper for last item

        'Get iIndexth item
        For ii = 1 To iIndex
            iPos = InStr(sString, " ")
            If ii = iIndex Then
                sString = VB.Left(sString, iPos - 1)
                FN_GetString = sString
                Exit Function
            Else
                sString = LTrim(Mid(sString, iPos))
                If Len(sString) = 0 Then
                    FN_GetString = ""
                    Exit Function
                End If
            End If
        Next ii

        'The function should have exited befor this, however just in case
        '(iIndex = 0) we indicate an error,
        FN_GetString = ""

    End Function
    Private Sub ScanLine(ByRef sLine As String, ByRef nNo As Double, ByRef sScale As String,
                         ByRef nSpace As Double, ByRef n20Len As Double, ByRef nReduction As Double)
        nNo = FN_GetNumber(sLine, 1)
        sScale = FN_GetString(sLine, 2)
        nSpace = FN_GetNumber(sLine, 3)
        n20Len = FN_GetNumber(sLine, 4)
        nReduction = FN_GetNumber(sLine, 5)
    End Sub
    Private Function FNGetTape(ByRef nIndex As Double) As Double
        ''-----------Dim nInt As Double = Value("scalar", StringMiddle(txtLeftLengths.Text, ((nIndex - 1) * 4) + 1, 3))
        Dim nInt As Double = Val(StringMiddle(txtLeftLengths.Text, ((nIndex - 1) * 4) + 1, 3))
        ''----------Dim nDec As Double = Value("scalar", StringMiddle(txtLeftLengths.Text, ((nIndex - 1) * 4) + 4, 1))
        Dim nDec As Double = Val(StringMiddle(txtLeftLengths.Text, ((nIndex - 1) * 4) + 4, 1))
        Return (nInt + (nDec * 0.1))
    End Function
    Function FN_Round(ByVal nNumber As Single) As Short
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
        If (nNumber - nInt) >= 0.5 Then
            FN_Round = (nInt + 1) * nSign
        Else
            FN_Round = nInt * nSign
        End If
    End Function
    Function FN_Decimalise(ByRef nDisplay As Double) As Double
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
        '        The returned value is usually +ve. Unless it can't
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
        If LGLEGDIA1.g_nUnitsFac <> 1 Then
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
    Sub PR_CalcPolar(ByRef xyStart As LGLEGDIA1.XY, ByRef nLength As Double, ByVal nAngle As Double, ByRef xyReturn As LGLEGDIA1.XY)
        Dim A, B As Double
        'Convert from degees to radians
        nAngle = nAngle * LGLEGDIA1.PI / 180
        B = System.Math.Sin(nAngle) * nLength
        A = System.Math.Cos(nAngle) * nLength
        xyReturn.X = xyStart.X + A
        xyReturn.y = xyStart.y + B
    End Sub
    Sub PR_DrawXMarker(ByRef xyBlkInsert As LGLEGDIA1.XY, Optional ByRef bIsOriginXMarker As Boolean = False)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim xyStart, xyEnd, xyBase, xySecSt, xySecEnd As LGLEGDIA1.XY
        PR_CalcPolar(xyBase, 0.0625, 135, xyStart)
        PR_CalcPolar(xyBase, 0.0625, -45, xyEnd)
        PR_CalcPolar(xyBase, 0.0625, 45, xySecSt)
        PR_CalcPolar(xyBase, 0.0625, -135, xySecEnd)

        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has("X Marker") Then
                Dim blkTblRecCross As BlockTableRecord = New BlockTableRecord()
                blkTblRecCross.Name = "X Marker"
                Dim acLine As Line = New Line(New Point3d(xyStart.X, xyStart.y, 0), New Point3d(xyEnd.X, xyEnd.y, 0))
                blkTblRecCross.AppendEntity(acLine)
                acLine = New Line(New Point3d(xySecSt.X, xySecSt.y, 0), New Point3d(xySecEnd.X, xySecEnd.y, 0))
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
                Dim blkRef As BlockReference = New BlockReference(New Point3d(xyBlkInsert.X + LGLEGDIA1.xyLegInsertion.X, xyBlkInsert.y + LGLEGDIA1.xyLegInsertion.y, 0), blkRecId)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If
                '' Open the Block table record Model space for write
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)
                acBlkTblRec.AppendEntity(blkRef)
                acTrans.AddNewlyCreatedDBObject(blkRef, True)
                idLastCreated = blkRef.ObjectId()
                If bIsOriginXMarker = True Then
                    g_sXMarkerHandle = blkRef.Handle.ToString()
                End If
            End If
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawText(ByRef sText As Object, ByRef xyInsert As LGLEGDIA1.XY, ByRef nHeight As Object, ByRef nAngle As Object, ByVal nTextmode As Double)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

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
                acText.Position = New Point3d(xyInsert.X + LGLEGDIA1.xyLegInsertion.X, xyInsert.y + LGLEGDIA1.xyLegInsertion.y, 0)
                acText.Height = nHeight
                acText.TextString = sText
                acText.Rotation = nAngle
                acText.WidthFactor = 0.6
                ''acText.HorizontalMode = nTextmode
                acText.Justify = nTextmode
                ''If acText.HorizontalMode <> TextHorizontalMode.TextLeft Then
                acText.AlignmentPoint = New Point3d(xyInsert.X + LGLEGDIA1.xyLegInsertion.X, xyInsert.y + LGLEGDIA1.xyLegInsertion.y, 0)
                ''End If
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acText.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If
                acBlkTblRec.AppendEntity(acText)
                acTrans.AddNewlyCreatedDBObject(acText, True)
            End Using

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_MakeXY(ByRef xyReturn As LGLEGDIA1.XY, ByRef X As Double, ByRef y As Double)
        'Utility to return a point based on the X and Y values
        'given
        xyReturn.X = X
        xyReturn.y = y
    End Sub
    Sub PR_DrawLine(ByRef xyStart As LGLEGDIA1.XY, ByRef xyFinish As LGLEGDIA1.XY)
        'To the DRAFIX macro file (given by the global fNum).
        'Write the syntax to draw a LINE between two points.
        'For this to work it assumes that the following DRAFIX variables
        'are defined
        '    XY      xyStart
        '    HANDLE  hEnt
        '
        'Note:-
        '    fNum, CC, QQ, NL are globals initialised by FN_Open

        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout),
                                      OpenMode.ForWrite)

            '' Create a line that starts at 5,5 and ends at 12,3
            Dim acLine As Line = New Line(New Point3d(xyStart.X + LGLEGDIA1.xyLegInsertion.X, xyStart.y + LGLEGDIA1.xyLegInsertion.y, 0),
                                    New Point3d(xyFinish.X + LGLEGDIA1.xyLegInsertion.X, xyFinish.y + LGLEGDIA1.xyLegInsertion.y, 0))

            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                acLine.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
            End If
            '' Add the new object to the block table record and the transaction
            acBlkTblRec.AppendEntity(acLine)
            acTrans.AddNewlyCreatedDBObject(acLine, True)
            idLastCreated = acLine.ObjectId()

            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_DrawMText(ByRef sText As Object, ByRef xyInsert As LGLEGDIA1.XY, ByRef bIsCenter As Boolean)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

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
            mtx.Location = New Point3d(xyInsert.X + LGLEGDIA1.xyLegInsertion.X, xyInsert.y + LGLEGDIA1.xyLegInsertion.y, 0)
            mtx.SetDatabaseDefaults()
            mtx.TextStyleId = acCurDb.Textstyle
            ' current text size
            mtx.TextHeight = 0.1
            ' current textstyle
            mtx.Width = 0.0
            mtx.Rotation = 0
            mtx.Contents = sText
            mtx.Attachment = AttachmentPoint.TopLeft
            If bIsCenter = True Then
                mtx.Attachment = AttachmentPoint.TopCenter
            End If
            mtx.SetAttachmentMovingLocation(mtx.Attachment)
            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                mtx.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
            End If
            acBlkTblRec.AppendEntity(mtx)
            acTrans.AddNewlyCreatedDBObject(mtx, True)

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_CalculateFootPoints(ByRef nFabricClass As Short, ByRef nFirstTape As Short, ByRef nStyleLastRed As Short,
                                       ByRef nReductionAnkle As Short)
        ''--------@ g_sPathJOBST & "\WAIST\WHFTPNTS.D;"

        ''Load template data file
        Dim sFile, sLine As String
        If nFabricClass = 0 Then
            ''------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
        Else
            ''------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
        End If
        Dim hChan As Short
        hChan = FreeFile()
        FileOpen(hChan, sFile, VB.OpenMode.Input)
        Dim nNo, nSpace, n20Len, nReduction, nn As Double
        Dim sScale As String = ""
        If (hChan) Then
            'If (Input(hChan, sLine)) Then
            '    ScanLine(sLine, "blank", & nNo, & sScale, & nSpace, & n20Len, & nReduction)

            'Else
            '    Exit (%abort, "Can't read " + sFile + "\nFile maybe corrupted")
            '        End If
            sLine = LineInput(hChan)
            nNo = FN_GetNumber(sLine, 1)
            sScale = FN_GetString(sLine, 2)
            nSpace = FN_GetNumber(sLine, 3)
            n20Len = FN_GetNumber(sLine, 4)
            nReduction = FN_GetNumber(sLine, 5)
        Else
            ''-------Exit (%abort, "Can't open "+ sFile + "\nCheck installation")
        End If
        ''From data file retain reduction data of first scale for use with
        '' Straight toes (where a 14 reduction Is required)
        Dim xyLeg As LGLEGDIA1.XY
        PR_MakeXY(xyLeg, 0, 0)
        Dim nFirst20Len, nFirstReduction, nHeelTape, nAnkleToHeelOffset, nHt, nTmpltReduction As Double
        nFirst20Len = n20Len
        nFirstReduction = nReduction
        nHeelTape = 6
        '' Establish type of heel given the ankle position
        '' NB this comes via WH_FIGUR.D And the Data Base Field AnkleTape 

        If (fnGetNumber(g_sStyleString, 4) = 7) Then
            SmallHeel = True
            nAnkleToHeelOffset = 1
        Else
            SmallHeel = False
            nAnkleToHeelOffset = 2
        End If
        '' Get data from both file And tapes for FOOT
        '' Actual position depends on ankle position ( this Is based on heel size)
        '' 	1. First Tape
        ''	2. AnkleM ( -1.5 tape Or -3 tape) (AnkleM stands for Ankle Minus)
        ''	2a. Previous tape to AnkleM ( -3 Or - 4.5)
        ''	3. Heel 	( 0 tape)
        ''	4. Ankle ( +1.5 tape Or +3 Tape)
        ''---------nHt = LGLEGDIA1.xyLegInsertion.X
        nHt = 0

        '' Loop Through data file to get to start of leg tapes
        '' Ignoring the none relevent ones 
        nn = 1
        Dim nAnkleTape As Double = fnGetNumber(g_sStyleString, 4)
        Dim nFootPleat1, nFootPleat2, nLastTape20Len, nLastTapeTmpltReduction As Double
        nFootPleat1 = LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat1.Text))
        nFootPleat2 = LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat2.Text))
        While (nn < nFirstTape)
            ''-----GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            nn = nn + 1
        End While
        ScanLine(sLine, nNo, sScale, nSpace, n20Len, nTmpltReduction)
        nSpace = 0
        Dim xyFirstTape, xyAnkleM, xyHeel, xyAnkle As LGLEGDIA1.XY
        Dim nAnkleMTape, nAnkleM20Len, nAnkleMTmpltReduction, nAnkleMPrev20Len, nAnkleMPrevTmpltReduction, nAnkleMPrevHt As Double
        Dim nPrev20Len, nPrevTmpltReduction, nPrevHt, nHeel20Len, nHeelTmpltReduction, nAnkle20Len, nAnkleTmpltReduction As Double
        While (nn <= nAnkleTape)
            If ((nn = nFirstTape + 1) And (nFootPleat1 <> 0)) Then
                nSpace = nFootPleat1
            End If
            If ((nn = nFirstTape + 2) And (nFootPleat2 <> 0)) Then
                nSpace = nFootPleat2
            End If
            nHt = nHt + nSpace
            If (nn = nFirstTape) Then
                xyFirstTape.X = nHt
                nLastTape20Len = n20Len
                nLastTapeTmpltReduction = nTmpltReduction
            End If
            If (nn = nHeelTape - nAnkleToHeelOffset) Then
                xyAnkleM.X = nHt
                nAnkleMTape = nHeelTape - nAnkleToHeelOffset
                nAnkleM20Len = n20Len
                nAnkleMTmpltReduction = nTmpltReduction
                If (nn = nFirstTape) Then
                    nAnkleMPrev20Len = n20Len
                    nAnkleMPrevTmpltReduction = nTmpltReduction
                    nAnkleMPrevHt = nHt
                Else
                    nAnkleMPrev20Len = nPrev20Len
                    nAnkleMPrevTmpltReduction = nPrevTmpltReduction
                    nAnkleMPrevHt = nPrevHt
                End If
            End If
            If (nn = nHeelTape) Then
                xyHeel.X = nHt
                nHeel20Len = n20Len
                nHeelTmpltReduction = nTmpltReduction
            End If
            If (nn = nAnkleTape) Then
                xyAnkle.X = nHt
                nAnkle20Len = n20Len
                nAnkleTmpltReduction = nTmpltReduction
            End If
            ''---------GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            nPrevHt = nHt   ''// Store wrt AnklePrevM  & Foot Pleats
            nPrev20Len = n20Len
            nPrevTmpltReduction = nTmpltReduction
            ScanLine(sLine, nNo, sScale, nSpace, n20Len, nTmpltReduction)
            nn = nn + 1
        End While
        ''---Close("file", hChan) 
        FileClose(hChan)
        '' Foot Reductions Chart
        '' Quick And Dirty

        Dim nHeelChartReduction, nAnkleMChartReduction, nAnkleMPrevChartReduction, nToeCutBackReduction, nToeCurvedReduction As Double
        If (nReductionAnkle >= 14 And nReductionAnkle <= 18) Then
            nHeelChartReduction = 17
            nAnkleMChartReduction = 14
        End If
        If (nReductionAnkle >= 19 And nReductionAnkle <= 23) Then
            nHeelChartReduction = 19
            nAnkleMChartReduction = 17
        End If
        If (nReductionAnkle >= 24) Then
            nHeelChartReduction = 21
            nAnkleMChartReduction = 17
        End If
        nAnkleMPrevChartReduction = 16
        nToeCutBackReduction = 34
        nToeCurvedReduction = 34
        '' Choose heel plate
        Dim nHeelR1, nHeelR2, nHeelR3, nHeelD1, nHeelD2, nBackHeelOff, nFrontHeelOff, nTapeLen As Double
        If (SmallHeel) Then
            '' Heel plate #1
            nHeelR1 = 1.1760709
            nHeelR2 = 0.8716718
            nHeelR3 = 1.1760709 ''//Heel Plate now symetrical 27.Sept.94
            nHeelD1 = 2.096372
            nHeelD2 = 2.1843981
            nBackHeelOff = 0.05
            nFrontHeelOff = 0.1875

        Else
            '' Heel plate #2
            nHeelR1 = 1.6601037
            nHeelR2 = 1.0876233
            nHeelR3 = 1.5053069
            nHeelD1 = 2.7812341
            nHeelD2 = 2.6294854
            nBackHeelOff = 0.25
            nFrontHeelOff = nBackHeelOff
        End If
        '' Calculate Control points
        '' For Heel
        '' At Ankle (ie +3 Or + 1.5 Tape)
        Dim nLength, nRedStep, nAnkleMTapeLen As Double
        Dim nSeam As Double = 0.1875
        ''---------nTapeLen = FN_Round(FN_Decimalise(FNGetTape(nAnkleTape)) * LGLEGDIA1.g_nUnitsFac)
        nTapeLen = FN_Decimalise(FNGetTape(nAnkleTape)) * LGLEGDIA1.g_nUnitsFac
        If (nFabricClass = 2) Then
            nLength = (nTapeLen * ((100 - nReductionAnkle) / 100)) / 2
        Else
            nLength = nAnkle20Len / 20 * nTapeLen
            nRedStep = nAnkle20Len / (20 * 8)
            nLength = nLength + ((nAnkleTmpltReduction - nReductionAnkle) * nRedStep)
        End If
        ''--------xyAnkle.y = LGLEGDIA1.xyLegInsertion.y + nSeam + nLength
        xyAnkle.y = 0 + nSeam + nLength
        ''// At Heel (ie at 0 Tape)

        ''---------nTapeLen = FN_Round(FN_Decimalise(FNGetTape(nHeelTape)) * LGLEGDIA1.g_nUnitsFac)
        nTapeLen = FN_Decimalise(FNGetTape(nHeelTape)) * LGLEGDIA1.g_nUnitsFac
        nLength = nHeel20Len / 20 * nTapeLen
        nRedStep = nHeel20Len / (20 * 8)
        nLength = nLength + ((nHeelTmpltReduction - nHeelChartReduction) * nRedStep)
        ''----------xyHeel.y = LGLEGDIA1.xyLegInsertion.y + nSeam + nLength
        xyHeel.y = 0 + nSeam + nLength

        ''// At Minus Ankle ( ie -3 Or -1.5 Tape)

        ''----------nAnkleMTapeLen = FN_Round(FN_Decimalise(FNGetTape(nAnkleMTape)) * LGLEGDIA1.g_nUnitsFac)
        nAnkleMTapeLen = FN_Decimalise(FNGetTape(nAnkleMTape)) * LGLEGDIA1.g_nUnitsFac
        nLength = nAnkleM20Len / 20 * nAnkleMTapeLen
        nRedStep = nAnkleM20Len / (20 * 8)
        nLength = nLength + ((nAnkleMTmpltReduction - nAnkleMChartReduction) * nRedStep)
        ''-------------xyAnkleM.y = LGLEGDIA1.xyLegInsertion.y + nSeam + nLength
        xyAnkleM.y = 0 + nSeam + nLength

        Dim xyAnkleMPrev, xyHeelCntrMidDistal As LGLEGDIA1.XY
        xyAnkleMPrev.X = nAnkleMPrevHt
        nLength = nAnkleMPrev20Len / 20 * nAnkleMTapeLen   ''; // NB "nAnkleMTapeLen" from above
        nRedStep = nAnkleMPrev20Len / (20 * 8)
        nLength = nLength + ((nAnkleMPrevTmpltReduction - nAnkleMPrevChartReduction) * nRedStep)
        ''-----------xyAnkleMPrev.y = LGLEGDIA1.xyLegInsertion.y + nSeam + nLength
        xyAnkleMPrev.y = 0 + nSeam + nLength

        ''// At Last tape using Minus Ankle Tape Value
        ''// N.B. Quick And dirty
        nLength = nAnkleMTapeLen * 0.66 / 2 '';  // NB "nAnkleMTapeLen" from above
        ''---------xyFirstTape.y = LGLEGDIA1.xyLegInsertion.y + nLength + nSeam
        xyFirstTape.y = 0 + nLength + nSeam

        ''// Heel Circles
        ''// Control points

        xyHeelCntrMidDistal.X = xyHeel.X - nFrontHeelOff
        xyHeelCntrMidDistal.y = xyHeel.y - nHeelR2
        Dim nCalc, nAngle, nToeOffset As Double
        Dim xyHeelCntrDistal, xyHeelCntrMidProximal, xyHeelCntrProximal As LGLEGDIA1.XY
        Dim nError As Short
        Dim xyStart, xyEnd, xyInt As LGLEGDIA1.XY
        If (SmallHeel) Then
            nCalc = FN_CalcLength(xyAnkleM, xyHeelCntrMidDistal)
            nAngle = FN_CalcAngle(xyHeelCntrMidDistal, xyAnkleM) - (System.Math.Acos((nHeelR1 ^ 2 - nHeelD1 ^ 2 - nCalc ^ 2) / (-2 * nHeelD1 * nCalc)) * (180 / LGLEGDIA1.PI))
            ''----------xyHeelCntrDistal = CalcXY("relpolar", xyHeelCntrMidDistal, nHeelD1, nAngle)
            PR_CalcPolar(xyHeelCntrMidDistal, nHeelD1, nAngle, xyHeelCntrDistal)
        Else
            ''--------------
            'nError = FN_CirLinInt(xyHeelCntrMidDistal.X, xyAnkleM.y + nHeelR1,
            '           xyHeelCntrMidDistal.X - 10, xyAnkleM.y + nHeelR1,
            '           xyHeelCntrMidDistal,
            '                nHeelD1)
            PR_MakeXY(xyStart, xyHeelCntrMidDistal.X, xyAnkleM.y + nHeelR1)
            PR_MakeXY(xyEnd, xyHeelCntrMidDistal.X - 10, xyAnkleM.y + nHeelR1)
            nError = FN_CirLinInt(xyStart, xyEnd, xyHeelCntrMidDistal, nHeelD1, xyInt)
            xyHeelCntrDistal = xyInt
        End If

        xyHeelCntrMidProximal.X = xyHeel.X + nBackHeelOff
        xyHeelCntrMidProximal.y = xyHeel.y - nHeelR2

        Dim BigAnkle As Boolean = False '';	//Flag to indicate Ankle of Lymphdema proportions
        If (SmallHeel) Then
            nCalc = FN_CalcLength(xyAnkle, xyHeelCntrMidProximal)
            nAngle = FN_CalcAngle(xyHeelCntrMidProximal, xyAnkle) + (System.Math.Acos((nHeelR3 ^ 2 - nHeelD2 ^ 2 - nCalc ^ 2) / (-2 * nHeelD2 * nCalc)) * (180 / LGLEGDIA1.PI))
            ''---xyHeelCntrProximal = CalcXY("relpolar", xyHeelCntrMidProximal, nHeelD2, nAngle)
            PR_CalcPolar(xyHeelCntrMidProximal, nHeelD2, nAngle, xyHeelCntrProximal)
        Else
            ''--------------
            'nError = FN_CirLinInt(xyHeelCntrMidProximal.X, xyAnkle.y + nHeelR3,
            '           xyHeelCntrMidProximal.X + 10, xyAnkle.y + nHeelR3,
            '           xyHeelCntrMidProximal,
            '          nHeelD2)
            PR_MakeXY(xyStart, xyHeelCntrMidProximal.X, xyAnkle.y + nHeelR3)
            PR_MakeXY(xyEnd, xyHeelCntrMidProximal.X + 10, xyAnkle.y + nHeelR3)
            nError = FN_CirLinInt(xyStart, xyEnd, xyHeelCntrMidProximal, nHeelD2, xyInt)
            xyHeelCntrProximal = xyInt
            If (nError = False) Then
                BigAnkle = True '';	// IE no intersection found
            End If
        End If

        ''// Toes
        ''// Quick and dirty ;
        ''-----------xyToeSeam.y = LGLEGDIA1.xyLegInsertion.y + nSeam
        xyToeSeam.y = 0 + nSeam
        Dim sFootLabel As String = " "
        Dim nAge As Short = Val(txtAge.Text)
        If (nAge <= 10) Then
            If (nAge <= 2) Then nToeOffset = 2.75
            If (nAge = 3) Then nToeOffset = 3.0 '';	// Fax 9.Sept.94, Item2
            If (nAge = 4) Then nToeOffset = 3.25
            If (nAge = 5) Then nToeOffset = 3.375
            If (nAge = 6) Then nToeOffset = 3.625
            If (nAge = 7) Then nToeOffset = 3.875
            If (nAge = 8) Then nToeOffset = 4.0
            If (nAge = 9) Then nToeOffset = 4.25
            If (nAge = 10) Then nToeOffset = 4.5
        End If
        Dim sToeStyle As String = txtToeStyle.Text
        Dim nOtherAnkleMTapeLen As Double
        If (sToeStyle.Equals("Curved")) Then
            ''-----If (Male) Then
            If txtSex.Text = "Male" Then
                xyToeSeam.X = xyHeel.X - 4.75
            Else
                xyToeSeam.X = xyHeel.X - 4.5
            End If
            If (nFirstTape <= 2 Or nAnkleMTapeLen >= 10 Or nOtherAnkleMTapeLen >= 10) Then
                ''//Figuring as a LONG Curved Toe
                xyToeSeam.X = xyHeel.X - 5.5
            End If
            If (nAge <= 10) Then
                ''// Figuring as a EXTRA SHORT  Curved Toe"
                xyToeSeam.X = xyHeel.X - nToeOffset
            End If
        End If
        If (sToeStyle.Equals("Cut-Back") Or sToeStyle.Equals("Soft Enclosed")) Then
            ''--------If (Male) Then
            If txtSex.Text = "Male" Then
                xyToeSeam.X = xyHeel.X - 4.75
            Else
                xyToeSeam.X = xyHeel.X - 4.5
            End If
            If (nAnkleMTapeLen >= 10 Or nOtherAnkleMTapeLen >= 10) Then
                xyToeSeam.X = xyHeel.X - 5.5
            End If
        End If

        If (sToeStyle.Equals("Cut-Back") Or sToeStyle.Equals("Soft Enclosed")) Then
            If sToeStyle.Equals("Soft Enclosed") Then
                sFootLabel = "CAP"
            End If
            If (nAge <= 10) Then
                xyToeSeam.X = xyHeel.X - nToeOffset
            End If
        End If
        Dim xyToeOFF As LGLEGDIA1.XY
        If (sToeStyle.Equals("Straight") Or sToeStyle.Equals("Soft Enclosed B/M")) Then
            xyToeSeam.X = xyHeel.X - 3.5
            nLength = nFirst20Len / 20 * nAnkleMTapeLen
            nRedStep = nFirst20Len / (20 * 8)
            nLength = nLength + ((nFirstReduction - 14) * nRedStep)
            ''-----------xyToeOFF.y = LGLEGDIA1.xyLegInsertion.y + nSeam + nLength
            xyToeOFF.y = 0 + nSeam + nLength
            xyToeOFF.X = xyToeSeam.X
            If (sToeStyle.Equals("Soft Enclosed B/M")) Then
                sFootLabel = "CAP"
            End If
        End If
        Dim sFootLength As String = txtFootLength.Text
        Dim nFootLength As Double
        ''-----------If (sToeStyle.Equals("Self Enclosed") Or (sToeStyle.Equals("Soft Enclosed") And Value("scalar", sFootLength))) Then
        If (sToeStyle.Equals("Self Enclosed") Or (sToeStyle.Equals("Soft Enclosed") And Val(sFootLength))) Then
            If (sToeStyle.Equals("Self Enclosed")) Then
                sFootLabel = "ENCLOSED"
            Else
                sFootLabel = "CAP"
            End If
            ''----------If (nFootLength = FN_Round(FN_Decimalise(Value("scalar", sFootLength)) * LGLEGDIA1.g_nUnitsFac)) Then
            ''----If (nFootLength = FN_Round(FN_Decimalise(Val(sFootLength)) * LGLEGDIA1.g_nUnitsFac)) Then
            nFootLength = FN_Decimalise(Val(sFootLength)) * LGLEGDIA1.g_nUnitsFac
            If (nFootLength <> 0) Then
                If (SmallHeel) Then
                    ''-------nFootLength = FN_Round(nFootLength * 0.9)
                    nFootLength = nFootLength * 0.9
                Else
                    ''----------nFootLength = FN_Round(nFootLength * 0.83)
                    nFootLength = nFootLength * 0.83
                End If
                ''---------nLength = (nHeel20Len / 20 * 12.5) + LGLEGDIA1.xyLegInsertion.y + nSeam
                nLength = (nHeel20Len / 20 * 12.5) + 0 + nSeam
                If (nLength < xyHeel.y) Then
                    ''-------------
                    'nError = FN_CirLinInt(LGLEGDIA1.xyLegInsertion.X - 20, LGLEGDIA1.xyLegInsertion.y + nSeam,
                    '   xyHeel.X, LGLEGDIA1.xyLegInsertion.y + nSeam,
                    '          xyHeel.X, nLength,
                    '          nFootLength)
                    ''------------PR_MakeXY(xyStart, xyLegInsertion.X - 20, xyLegInsertion.y + nSeam)
                    PR_MakeXY(xyStart, 0 - 20, 0 + nSeam)
                    ''------------PR_MakeXY(xyEnd, xyHeel.X, xyLegInsertion.y + nSeam)
                    PR_MakeXY(xyEnd, xyHeel.X, 0 + nSeam)
                    Dim xyCen As LGLEGDIA1.XY
                    PR_MakeXY(xyCen, xyHeel.X, nLength)
                    nError = FN_CirLinInt(xyStart, xyEnd, xyCen, nFootLength, xyInt)

                Else
                    'nError = FN_CirLinInt(LGLEGDIA1.xyLegInsertion.X - 20, LGLEGDIA1.xyLegInsertion.y + nSeam,
                    '                   xyHeel.X, LGLEGDIA1.xyLegInsertion.y + nSeam,
                    '                          xyHeel,
                    '                          nFootLength)
                    ''-------------PR_MakeXY(xyStart, xyLegInsertion.X - 20, xyLegInsertion.y + nSeam)
                    PR_MakeXY(xyStart, 0 - 20, 0 + nSeam)
                    ''-------------PR_MakeXY(xyEnd, xyHeel.X, xyLegInsertion.y + nSeam)
                    PR_MakeXY(xyEnd, xyHeel.X, 0 + nSeam)
                    nError = FN_CirLinInt(xyStart, xyEnd, xyHeel, nFootLength, xyInt)
                End If
                If (nError) Then
                    xyToeSeam = xyInt
                Else
                    'Display("message", "error", "Can't position Toe with given Foot length")
                    'Exit (%cancel, "Error forming foot") 
                    MsgBox("Can't position Toe with given Foot length", 48, "Leg Dialogue")
                    MsgBox("Error forming foot", 48, "Leg Dialogue")
                    Exit Sub
                End If

            Else
                'Display("message", "error", "A Foot length is required for Self Enclosed Toes")
                'Exit (%cancel, "No Foot length given") 
                MsgBox("A Foot length is required for Self Enclosed Toes", 48, "Leg Dialogue")
                MsgBox("No Foot length given", 48, "Leg Dialogue")
                Exit Sub
            End If
        End If
        ''// Toe Points to position toe arcs
        ''// 

        ''// Toe circle constants
        ''// Quick And Dirty
        Dim nToeCntrMidToCntrLowY, nToeCntrMidToCntrLowX, nToeCntrMidToCntrHighY, nToeCntrMidToCntrHighX, nToeMidR, nToeLowR, nToeHighR As Double
        nToeCntrMidToCntrLowY = 3.3359296
        nToeCntrMidToCntrLowX = 8.0276805
        nToeCntrMidToCntrHighY = 3.1684888
        nToeCntrMidToCntrHighX = 2.1960337
        nToeMidR = 0.2528866
        nToeLowR = 8.4403592
        nToeHighR = 4.1080042

        ''// Establish Low Toe Arc center

        nLength = xyFirstTape.y - xyToeSeam.y  ''; 	// Toe Point line To seam

        Dim xyToeCntrLow, xyToeCntrMid, xyToeCntrHigh As LGLEGDIA1.XY
        If (nToeCntrMidToCntrLowY < nLength) Then
            ''// Simple case
            ''// End of toe curve Is above seam line
            xyToeCntrLow.y = xyFirstTape.y - nToeCntrMidToCntrLowY
            xyToeCntrLow.X = xyToeSeam.X - nToeLowR

        Else
            ''// More Complex case
            ''// End of toe curve Is intersected by the seam line
            xyToeCntrLow.y = xyFirstTape.y - nToeCntrMidToCntrLowY
            nLength = nToeCntrMidToCntrLowY - nLength
            nLength = System.Math.Sqrt((nToeLowR * nToeLowR) - (nLength * nLength))
            xyToeCntrLow.X = xyToeSeam.X - nLength
        End If

        ''// Having established TOE Low circle center the rest follows 
        ''//
        xyToeCntrMid.y = xyToeCntrLow.y + nToeCntrMidToCntrLowY
        xyToeCntrMid.X = xyToeCntrLow.X + nToeCntrMidToCntrLowX
        xyToeCntrHigh.y = xyToeCntrMid.y - nToeCntrMidToCntrHighY
        xyToeCntrHigh.X = xyToeCntrMid.X + nToeCntrMidToCntrHighX

        ''//AddEntity ("circle", xyToeCntrHigh, nToeHighR ) ;
        ''//AddEntity ("circle", xyToeCntrMid, nToeMidR ) ;
        ''//AddEntity ("circle", xyToeCntrLow, nToeLowR ) ;
        ''//AddEntity ("circle",   xyHeelCntrDistal, nHeelR1) ;
        ''//AddEntity ("circle",   xyHeelCntrMidDistal, nHeelR2) ;
        ''//AddEntity ("circle",   xyHeelCntrProximal, nHeelR3) ;
        ''//AddEntity ("circle",   xyHeelCntrMidProximal, nHeelR2) ;

        ''----------@ & g_sPathJOBST & "\LEG\LGLEGDWG.D;"
        ''// Establish layer
        Dim sLeg As String = txtLeg.Text
        Dim strLayer As String = ""
        If (sLeg.Equals("Left")) Then
            ARMDIA1.PR_SetLayer("TemplateLeft")
            ''--------hTemplateLayer = Table("find", "layer", "TemplateLeft")
            strLayer = "TemplateLeft"
        Else
            ARMDIA1.PR_SetLayer("TemplateRight")
            ''-----------hTemplateLayer = Table("find", "layer", "TemplateRight")
            strLayer = "TemplateRight"
        End If

        '' // draw on layer construct
        ''// 'cause drafix is �"^%%%&*&(*&@@ 
        ARMDIA1.PR_SetLayer("Construct")
        ''// Draw for Straight & Straight types
        Dim aAngle, aPrevAngle, aAngleInc, ii As Double
        Dim ptPolyColl As Point3dCollection = New Point3dCollection
        Dim xyToePnt As LGLEGDIA1.XY
        If (sToeStyle.Equals("Straight") Or sToeStyle.Equals("Soft Enclosed B/M")) Then
            ''// Draw for straight & Straight types
            ''---------StartPoly("fitted")
            ''---------AddVertex(xyToeOFF)
            ptPolyColl.Add(New Point3d(xyToeOFF.X, xyToeOFF.y, 0))
        Else
            ''// Draw Toe Curve
            ''---------StartPoly("fitted")
            ''---------AddVertex(xyToeSeam)
            ptPolyColl.Add(New Point3d(xyToeSeam.X, xyToeSeam.y, 0))
            ''// First toe curve
            aAngle = FN_CalcAngle(xyToeCntrLow, xyToeCntrMid)
            aPrevAngle = FN_CalcAngle(xyToeCntrLow, xyToeSeam)
            If (aAngle > aPrevAngle) Then
                aAngleInc = (aAngle - aPrevAngle) / 3
            Else
                aAngleInc = ((aAngle + 360) - aPrevAngle) / 3
            End If
            ii = 1
            Dim xyPlr As LGLEGDIA1.XY
            While (ii <= 3)
                ''------AddVertex(CalcXY("relpolar", xyToeCntrLow, nToeLowR, aPrevAngle + aAngleInc * ii))
                PR_CalcPolar(xyToeCntrLow, nToeLowR, aPrevAngle + aAngleInc * ii, xyPlr)
                ptPolyColl.Add(New Point3d(xyPlr.X, xyPlr.y, 0))
                ii = ii + 1
            End While
            ''// Toe Point curve
            aAngle = FN_CalcAngle(xyToeCntrMid, xyToeCntrLow)
            aPrevAngle = FN_CalcAngle(xyToeCntrHigh, xyToeCntrMid)
            If (aAngle > aPrevAngle) Then
                aAngleInc = (aAngle - aPrevAngle) / 3
            Else
                aAngleInc = ((aAngle + 360) - aPrevAngle) / 3
            End If
            ii = 1

            ''------xyToePnt = CalcXY("relpolar", xyToeCntrMid, nToeMidR, aAngle - aAngleInc * ii)
            PR_CalcPolar(xyToeCntrMid, nToeMidR, aAngle - aAngleInc * ii, xyToePnt)
            While (ii <= 3)
                ''----------AddVertex(CalcXY("relpolar", xyToeCntrMid, nToeMidR, aAngle - aAngleInc * ii))
                PR_CalcPolar(xyToeCntrMid, nToeMidR, aAngle - aAngleInc * ii, xyPlr)
                ptPolyColl.Add(New Point3d(xyPlr.X, xyPlr.y, 0))
                ii = ii + 1
            End While

            ''// Top of toe curve
            aAngle = FN_CalcAngle(xyToeCntrHigh, xyToeCntrMid)
            aPrevAngle = FN_CalcAngle(xyToeCntrHigh, xyAnkleMPrev)
            If (aPrevAngle <= 90) Then
                aPrevAngle = 90
            End If
            If (aAngle > aPrevAngle) Then
                aAngleInc = (aAngle - aPrevAngle) / 3
            Else
                aAngleInc = ((aAngle + 360) - aPrevAngle) / 3
            End If
            ii = 1
            While (ii <= 2)
                ''-------AddVertex(CalcXY("relpolar", xyToeCntrHigh, nToeHighR, aAngle - aAngleInc * ii))
                PR_CalcPolar(xyToeCntrHigh, nToeHighR, aAngle - aAngleInc * ii, xyPlr)
                ptPolyColl.Add(New Point3d(xyPlr.X, xyPlr.y, 0))
                ii = ii + 1
            End While
            If (SmallHeel = False) Then
                ''----------AddVertex(xyAnkleMPrev)
                ptPolyColl.Add(New Point3d(xyAnkleMPrev.X, xyAnkleMPrev.y, 0))
            End If
        End If

        ''// Draw Heel
        ''// Add Start of heel
        If (xyAnkleMPrev.y <> 0 And SmallHeel = False) Then
            ''---------AddVertex(xyAnkleM)
            ptPolyColl.Add(New Point3d(xyAnkleM.X, xyAnkleM.y, 0))
        End If

        aPrevAngle = 270
        aAngle = FN_CalcAngle(xyHeelCntrDistal, xyHeelCntrMidDistal)
        If (aAngle > aPrevAngle) Then
            aAngleInc = (aAngle - aPrevAngle) / 3
        Else
            aAngleInc = ((aAngle + 360) - aPrevAngle) / 3
        End If
        ii = 1
        Dim xyPlr1 As LGLEGDIA1.XY
        While (ii <= 2)
            ''------------AddVertex(CalcXY("relpolar", xyHeelCntrDistal, nHeelR1, aPrevAngle + aAngleInc * ii))
            PR_CalcPolar(xyHeelCntrDistal, nHeelR1, aPrevAngle + aAngleInc * ii, xyPlr1)
            ptPolyColl.Add(New Point3d(xyPlr1.X, xyPlr1.y, 0))
            ii = ii + 1
        End While

        aPrevAngle = 90
        aAngle = FN_CalcAngle(xyHeelCntrMidDistal, xyHeelCntrDistal)
        If (aAngle > aPrevAngle) Then
            aAngleInc = (aAngle - aPrevAngle) / 3
        Else
            aAngleInc = ((aAngle + 360) - aPrevAngle) / 3
        End If
        ii = 1
        While (ii <= 3)
            ''---------AddVertex(CalcXY("relpolar", xyHeelCntrMidDistal, nHeelR2, aAngle - aAngleInc * ii))
            ''------Dim xyPlr2 As LGLEGDIA1.XY
            PR_CalcPolar(xyHeelCntrMidDistal, nHeelR2, aAngle - aAngleInc * ii, xyPlr1)
            ptPolyColl.Add(New Point3d(xyPlr1.X, xyPlr1.y, 0))
            ii = ii + 1
        End While

        If (SmallHeel = False Or BigAnkle = True) Then
            ''---------AddVertex(xyHeel)
            ptPolyColl.Add(New Point3d(xyHeel.X, xyHeel.y, 0))
        End If

        aAngle = 90
        aPrevAngle = FN_CalcAngle(xyHeelCntrMidProximal, xyHeelCntrProximal)
        If (aAngle > aPrevAngle) Then
            aAngleInc = (aAngle - aPrevAngle) / 3
        Else
            aAngleInc = ((aAngle + 360) - aPrevAngle) / 3
        End If
        Dim nPts As Double
        If (BigAnkle) Then
            nPts = 0 ''; //**
        Else
            nPts = 2 ''; //**
        End If
        ii = 0
        While (ii <= nPts)
            ''-----------AddVertex(CalcXY("relpolar", xyHeelCntrMidProximal, nHeelR2, aAngle - aAngleInc * ii))
            ''-----------Dim xyPlr3 As LGLEGDIA1.XY
            PR_CalcPolar(xyHeelCntrMidProximal, nHeelR2, aAngle - aAngleInc * ii, xyPlr1)
            ptPolyColl.Add(New Point3d(xyPlr1.X, xyPlr1.y, 0))
            ii = ii + 1
        End While
        aAngle = 270
        aPrevAngle = FN_CalcAngle(xyHeelCntrProximal, xyHeelCntrMidProximal)
        If (aAngle > aPrevAngle) Then
            aAngleInc = (aAngle - aPrevAngle) / 3
        Else
            aAngleInc = ((aAngle + 360) - aPrevAngle) / 3
        End If
        If (BigAnkle) Then
            nPts = 0 ''; //**
        Else
            nPts = 2 ''; //**
        End If
        ii = 1
        Dim xyTmp As LGLEGDIA1.XY
        While (ii <= nPts)
            ''-------xyTmp = CalcXY("relpolar", xyHeelCntrProximal, nHeelR3, aPrevAngle + aAngleInc * ii)
            PR_CalcPolar(xyHeelCntrProximal, nHeelR3, aPrevAngle + aAngleInc * ii, xyTmp)
            If (xyTmp.X < xyAnkle.X) Then
                ''--------AddVertex(xyTmp)
                ptPolyColl.Add(New Point3d(xyTmp.X, xyTmp.y, 0))
            End If
            ii = ii + 1
        End While
        ''// End of drawing of Foot	
        ''//
        ''// Draw Leg 
        ''//
        ''------PROpenTemplateFile()
        ''Load template data file
        If nFabricClass = 0 Then
            ''----------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
        Else
            ''--------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
        End If

        hChan = FreeFile()
        FileOpen(hChan, sFile, VB.OpenMode.Input)
        If (hChan) Then
            'If (Input(hChan, sLine)) Then
            '    ScanLine(sLine, "blank", & nNo, & sScale, & nSpace, & n20Len, & nReduction)

            'Else
            '    Exit (%abort, "Can't read " + sFile + "\nFile maybe corrupted")
            '        End If
            sLine = LineInput(hChan)
            nNo = FN_GetNumber(sLine, 1)
            sScale = FN_GetString(sLine, 2)
            nSpace = FN_GetNumber(sLine, 3)
            n20Len = FN_GetNumber(sLine, 4)
            nReduction = FN_GetNumber(sLine, 5)
        Else
            ''-------Exit (%abort, "Can't open "+ sFile + "\nCheck installation")
            MsgBox("Can't open " + sFile + "\nCheck installation", 48, "Leg Dialogue")
        End If
        ''// Skip to FirstTape
        nn = 1
        While (nn < nFirstTape)
            ''--------GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            nn = nn + 1
        End While

        ''// Skip to ankletape 
        Dim nStyleFirstTape As Double = fnGetNumber(g_sStyleString, 2)
        ''------------xyTmp = LGLEGDIA1.xyLegInsertion
        PR_MakeXY(xyTmp, 0, 0)
        While (nn < nAnkleTape)
            ''-------GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            ScanLine(sLine, nNo, sScale, nSpace, n20Len, nReduction)
            If ((nn = nStyleFirstTape) And (nFootPleat1 <> 0)) Then
                nSpace = nFootPleat1
            End If
            If ((nn = nStyleFirstTape + 1) And (nFootPleat2 <> 0)) Then
                nSpace = nFootPleat2
            End If
            xyTmp.X = xyTmp.X + nSpace
            ''------------Changed for #162 in issue list---------------
            If chkStretch.Checked = True And nNo > 6 Then
                xyTmp.X = xyTmp.X + 0.125
            End If
            nn = nn + 1
        End While

        nn = nAnkleTape
        Dim nStyleLastTape As Double = fnGetNumber(g_sStyleString, 3)
        Dim nLegStyle As Double = fnGetNumber(g_sStyleString, 1)
        While (nn <= nStyleLastTape)
            ''----------nTapeLen = FN_Round(FN_Decimalise(FNGetTape(nn)) * LGLEGDIA1.g_nUnitsFac)
            nTapeLen = FN_Decimalise(FNGetTape(nn)) * LGLEGDIA1.g_nUnitsFac
            If (nFabricClass = 2) Then
                nLength = (nTapeLen * (100 - Val(StringMiddle(txtLeftRed.Text, ((nn - 1) * 3) + 1, 3))) / 100) / 2
            Else
                nLength = n20Len / 20 * nTapeLen
            End If
            nRedStep = n20Len / (20 * 8)

            If (nLegStyle = 0 And nn = nStyleLastTape) Then
                ''// For Anklets release last tape to a 14 reduction
                ''// by counting out
                nLength = nLength + ((nReduction - 14) * nRedStep)
            End If
            If (nLegStyle = 1 And nn = nStyleLastTape) Then
                ''// For Knee length
                ''// Release last tape to a given reduction
                nLength = (nTapeLen * ((100 - nStyleLastRed) / 100)) / 2
            End If

            If (nLegStyle = 2 And nn = nStyleLastTape) Then
                ''// For Thigh bands release last tape to a given reduction
                ''// by counting out
                nLength = nLength + ((nReduction - nStyleLastRed) * nRedStep)
            End If

            If (nn = nAnkleTape And nFabricClass <> 2) Then
                ''// Release the ANKLE tape to the CALCULATED reduction
                nLength = nLength + ((nReduction - nReductionAnkle) * nRedStep)
            End If

            ''-------xyTmp.y = xyOtemplate.y + nSeam + nLength
            xyTmp.y = 0 + nSeam + nLength
            ''---------AddVertex(xyTmp)
            ptPolyColl.Add(New Point3d(xyTmp.X, xyTmp.y, 0))
            If (nFabricClass = 2) Then
                ''-------SetData("TextHorzJust", 2) '';		// Center
                ''--------sSymbol = MakeString("long", nNo) + "tape" '';
                Dim sSymbol As String = Str(nNo) + "tape"
                'If (!Symbol("find", sSymbol)) Then
                '    Exit(%cancel, "Can't find a symbol to insert\nCheck your installation, that JOBST.SLB exists")
                '        End If
                ''-------AddEntity("symbol", sSymbol, xyTmp)
                ''-----AddEntity("text", Format("length", nTapeLen), xyTmp.X, xyTmp.y - 0.5)
                Dim xyText As LGLEGDIA1.XY
                PR_MakeXY(xyText, xyTmp.X, xyTmp.y - 0.5)
                PR_DrawText(Str(nTapeLen), xyText, 0.1, 0, 2)
                ''----AddEntity("text", StringMiddle(txtLeftRed.Text, ((nn - 1) * 3) + 1, 3), xyTmp.X, xyTmp.y - 0.7)
                PR_MakeXY(xyText, xyTmp.X, xyTmp.y - 0.7)
                PR_DrawText(StringMiddle(txtLeftRed.Text, ((nn - 1) * 3) + 1, 3), xyText, 0.1, 0, 2)
                ''---------AddEntity("text", StringMiddle(sStretch, ((nn - 1) * 3) + 1, 3), xyTmp.X, xyTmp.y - 0.9)
                PR_MakeXY(xyText, xyTmp.X, xyTmp.y - 0.9)
                PR_DrawText(StringMiddle(txtLeftStr.Text, ((nn - 1) * 3) + 1, 3), xyText, 0.1, 0, 2)
                ''----------AddEntity("text", StringMiddle(sTapeMMs, ((nn - 1) * 3) + 1, 3), xyTmp.X, xyTmp.y - 1.1)
                PR_MakeXY(xyText, xyTmp.X, xyTmp.y - 1.1)
                PR_DrawText(StringMiddle(txtLeftMMs.Text, ((nn - 1) * 3) + 1, 3), xyText, 0.1, 0, 2)
            End If

            If (nn = nStyleLastTape) Then
                xyProfileLast = xyTmp
            End If
            If (nn = nStyleLastTape - 1) Then
                xyPrevProfileLast = xyTmp
            End If
            If (nn = nStyleFirstTape) Then
                xyProfileStart = xyTmp
            End If

            nn = nn + 1
            ''-------GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            ScanLine(sLine, nNo, sScale, nSpace, n20Len, nReduction)
            If ((nn = nStyleFirstTape + 1) And (nFootPleat1 <> 0)) Then
                nSpace = nFootPleat1
            End If
            If ((nn = nStyleFirstTape + 2) And (nFootPleat2 <> 0)) Then
                nSpace = nFootPleat2
            End If
            Dim nTopLegPleat1, nTopLegPleat2 As Double
            nTopLegPleat1 = LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat1.Text))
            nTopLegPleat2 = LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat2.Text))
            If ((nn = nStyleLastTape) And (nTopLegPleat1 <> 0)) Then
                nSpace = nTopLegPleat1
            End If
            If ((nn = nStyleLastTape - 1) And (nTopLegPleat2 <> 0)) Then
                nSpace = nTopLegPleat2
            End If
            xyTmp.X = xyTmp.X + nSpace
            ''------------Changed for #65 and #77 in issue list---------------
            If chkStretch.Checked = True And nNo > 6 Then
                xyTmp.X = xyTmp.X + 0.125
            End If
        End While
        ''------Close("file", hChan)
        FileClose(hChan)

        ''--------EndPoly()
        ''---------PR_DrawPoly(ptPolyColl)
        Dim ptCollSpline As Point3dCollection = New Point3dCollection
        For ii = 0 To ptPolyColl.Count - 1
            ptCollSpline.Add(New Point3d(LGLEGDIA1.xyLegInsertion.X + ptPolyColl(ii).X, LGLEGDIA1.xyLegInsertion.y + ptPolyColl(ii).Y, 0))
        Next ii
        ARMDIA1.PR_SetLayer("TemplateLeft")
        PR_DrawSpline(ptCollSpline)
        PR_AddDBValueToLast("Handle", g_sXMarkerHandle)
        Dim sLegStyle As String = ""
        Select Case nLegStyle
            Case 1 'Knee High
                sLegStyle = "KLN"
            Case 2 'Thigh Length
                sLegStyle = "TLN"
            Case 3 'Knee Band
                sLegStyle = "KBN"
            Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                'Elastic
                sLegStyle = "TBB" 'Thigh Band (B/K)
                If nLegStyle = 4 Then
                    sLegStyle = "TBA" 'Thigh Band (A/K)
                End If
            Case Else 'Anklet,
                sLegStyle = "ANK"
        End Select
        PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + sLeg + "LegCurve")
        PR_AddDBValueToLast("Insertion", Str(LGLEGDIA1.xyLegInsertion.X) & " " & Str(LGLEGDIA1.xyLegInsertion.y))
        ''---------------------------
        ''// Get polyline entity handle
        ''// Change layer And set DB values

        'hChan = Open("selection", "layer = 'Construct' AND type = 'Curve'")
        'If (hChan) Then
        '    ResetSelection(hChan)
        '    hCurv = GetNextSelection(hChan)
        '    SetEntityData(hCurv, "layer", hTemplateLayer)
        '    SetDBData(hCurv, "ID", sLegStyle + sFileNo + sLeg + "LegCurve")
        'End If
        'Close("selection", hChan)
        ''----------------

        ''// Draw foot points
        ARMDIA1.PR_SetLayer("Construct")
        ''---------hEnt = AddEntity("marker", "xmarker", xyAnkle, 0.2, 0.2)
        PR_DrawXMarker(xyAnkle)
        ''----------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "Ankle")
        PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + sLeg + "Ankle")
        ''-------- sTmp = MakeString("scalar", xyHeelCntrProximal.X - xyAnkle.X) + " " + MakeString("scalar", xyHeelCntrProximal.y - xyAnkle.y)
        Dim sTmp As String = Str(xyHeelCntrProximal.X - xyAnkle.X) + " " + Str(xyHeelCntrProximal.y - xyAnkle.y)
        ''-------------SetDBData(hEnt, "Data", sTmp)
        PR_AddDBValueToLast("Data", sTmp)
        ''---------hEnt = AddEntity("marker", "xmarker", xyHeel, 0.2, 0.2)
        PR_DrawXMarker(xyHeel)
        PR_AddDBValueToLast("Handle", g_sXMarkerHandle)
        ''---------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "Heel")
        PR_AddDBValueToLast("HeelID", sLegStyle + txtFileNo.Text + sLeg + "Heel")
        ''--------- SetDBData(hEnt, "Data", MakeString("long", SmallHeel))
        Dim nSHeel As Integer = 0
        If SmallHeel = True Then nSHeel = 1
        PR_AddDBValueToLast("Data", Str(nSHeel))
        ''---------hEnt = AddEntity("marker", "xmarker", xyAnkleM, 0.2, 0.2)
        PR_DrawXMarker(xyAnkleM)
        ''-----------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "AnkleM")
        PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + sLeg + "AnkleM")
        ''--------sTmp = MakeString("scalar", xyHeelCntrDistal.X - xyAnkleM.X) + " " + MakeString("scalar", xyHeelCntrDistal.y - xyAnkleM.y)
        sTmp = Str(xyHeelCntrDistal.X - xyAnkleM.X) + " " + Str(xyHeelCntrDistal.y - xyAnkleM.y)
        ''----------SetDBData(hEnt, "Data", sTmp)
        PR_AddDBValueToLast("Data", sTmp)
        If (nAge > 10) Then
            ''--------AddEntity("marker", "xmarker", xyAnkleMPrev, 0.2, 0.2)
            PR_DrawXMarker(xyAnkleMPrev)
        End If
        If (SmallHeel = True) Then
            ''-----AddEntity("arc", xyHeelCntrProximal, nHeelR3, 180, 90)
            PR_DrawArc(xyHeelCntrProximal, nHeelR3, (180 * (LGLEGDIA1.PI / 180)), (270 * (LGLEGDIA1.PI / 180)))
        End If

        '' // Draw rest of it
        ''----------Execute("menu", "SetLayer", hTemplateLayer)
        ARMDIA1.PR_SetLayer(strLayer)

        ''// Add Closing lines at TOE
        ''------------If (LGLEGDIA1.xyLegInsertion.X > xyToeSeam.X) Then
        If (xyLeg.X > xyToeSeam.X) Then
            ''-------AddEntity("line", xyOtemplate, xyToeSeam.X, xyOtemplate.y)
            PR_MakeXY(xyStart, 0, 0)
            PR_MakeXY(xyEnd, xyToeSeam.X, 0)
            PR_DrawLine(xyStart, xyEnd)
        End If
        ''---AddEntity("line", xyToeSeam.X, xyOtemplate.y, xyToeSeam)
        PR_MakeXY(xyEnd, xyToeSeam.X, 0)
        PR_DrawLine(xyEnd, xyToeSeam)

        ''// Toe endings
        Dim xyToeCL As LGLEGDIA1.XY
        xyToeCL.X = xyToeSeam.X
        ''------xyToeCL.y = xyOtemplate.y
        xyToeCL.y = 0

        If (sToeStyle.Equals("Soft Enclosed") And nFootLength = 0 And (nAge <= 10)) Then
            ''-----------AddEntity("line", xyToeSeam, xyToeSeam.X, xyAnkleM.y)
            PR_MakeXY(xyEnd, xyToeSeam.X, xyAnkleM.y)
            PR_DrawLine(xyToeSeam, xyEnd)
        End If
        Dim nLeftOffset, nRightOffset As Double
        If (sToeStyle.Contains("Cut-Back") Or (sToeStyle.Equals("Soft Enclosed") And nFootLength = 0 And (nAge > 10))) Then
            nRightOffset = 0.75
            nLeftOffset = 0.25
            xyTmp.X = xyToeCntrMid.X - nToeMidR
            xyTmp.y = xyToeCntrMid.y
            aAngle = FN_CalcAngle(xyToeCntrLow, xyTmp)
            aPrevAngle = FN_CalcAngle(xyToeCntrLow, xyToeCL)
            If (aAngle > aPrevAngle) Then
                aAngleInc = (aAngle - aPrevAngle) / 2
            Else
                aAngleInc = ((aAngle + 360) - aPrevAngle) / 2
            End If
            '    AddEntity("poly",
            '"openfitted",
            ' xyTmp,
            'CalcXY("relpolar", xyToeCntrLow, nToeLowR - nLeftOffset, aPrevAngle + aAngleInc),
            'xyToeCL)
            Dim xyPolar As LGLEGDIA1.XY
            PR_CalcPolar(xyToeCntrLow, nToeLowR - nLeftOffset, aPrevAngle + aAngleInc, xyPolar)
            Dim ptColl As Point3dCollection = New Point3dCollection()
            ptColl.Add(New Point3d(xyTmp.X, xyTmp.y, 0))
            ptColl.Add(New Point3d(xyPolar.X, xyPolar.y, 0))
            ptColl.Add(New Point3d(xyToeCL.X, xyToeCL.y, 0))
            PR_DrawPoly(ptColl)
            ARMDIA1.PR_SetLayer("Notes")
            '    AddEntity("poly",
            '"openfitted",
            ' xyTmp,
            'CalcXY("relpolar", xyToeCntrLow, nToeLowR + nRightOffset, aPrevAngle + aAngleInc),
            'xyToeCL)
            PR_CalcPolar(xyToeCntrLow, nToeLowR + nRightOffset, aPrevAngle + aAngleInc, xyPolar)
            Dim ptColl1 As Point3dCollection = New Point3dCollection()
            ptColl1.Add(New Point3d(xyTmp.X, xyTmp.y, 0))
            ptColl1.Add(New Point3d(xyPolar.X, xyPolar.y, 0))
            ptColl1.Add(New Point3d(xyToeCL.X, xyToeCL.y, 0))
            PR_DrawPoly(ptColl1)
        End If

        If (sToeStyle.Equals("Soft Enclosed") And nFootLength <> 0) Then
            If (nAge > 10) Then
                nRightOffset = 2.25
            Else
                nRightOffset = 1.75
            End If
            ''sFootLabel = Format("length", nRightOffset) + " Soft Enclosed"
            sFootLabel = LGLEGDIA1.fnInchesToText(nRightOffset) + Chr(34) + " Soft Enclosed"
            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                ''Changed for the new change #194 in the issue list
                ''sFootLabel = fnDisplayToCM(nRightOffset) + " Soft Enclosed"
                sFootLabel = fnDisplayToCM(nRightOffset) + "CM" + " Soft Enclosed"
            End If
            xyTmp.X = xyToeCntrMid.X - nToeMidR
            xyTmp.y = xyToeCntrMid.y
            aAngle = FN_CalcAngle(xyToeCntrLow, xyTmp)
            aPrevAngle = FN_CalcAngle(xyToeCntrLow, xyToeCL)
            If (aAngle > aPrevAngle) Then
                aAngleInc = (aAngle - aPrevAngle) / 2
            Else
                aAngleInc = ((aAngle + 360) - aPrevAngle) / 2
            End If

            ''----------Execute("menu", "SetLayer", hTemplateLayer)
            ARMDIA1.PR_SetLayer(strLayer)
            ''-----------xyTmp = CalcXY("relpolar", xyToeCntrLow, nToeLowR, aPrevAngle + aAngleInc)
            PR_CalcPolar(xyToeCntrLow, nToeLowR, aPrevAngle + aAngleInc, xyTmp)
            'AddEntity("line", xyTmp.X + nRightOffset,
            '         xyTmp.y,
            '         xyTmp.X + nRightOffset,
            '         xyAnkleM.y)
            PR_MakeXY(xyStart, xyTmp.X + nRightOffset, xyTmp.y)
            PR_MakeXY(xyEnd, xyTmp.X + nRightOffset, xyAnkleM.y)
            PR_DrawLine(xyStart, xyEnd)
            'AddEntity("line", xyTmp.X + nRightOffset,
            '         xyTmp.y,
            '         xyTmp)
            PR_DrawLine(xyStart, xyTmp)
        End If
        If (sToeStyle.Equals("Straight") Or sToeStyle.Equals("Soft Enclosed B/M")) Then
            ''--------AddEntity("Line", xyToeSeam, xyToeOFF)
            PR_DrawLine(xyToeSeam, xyToeOFF)
        Else
            ARMDIA1.PR_SetLayer("Notes")
            ''---------AddEntity("line", xyToeSeam.X, xyFirstTape.y, xyToePnt.x, xyFirstTape.y)
            PR_MakeXY(xyStart, xyToeSeam.X, xyFirstTape.y)
            PR_MakeXY(xyEnd, xyToePnt.X, xyFirstTape.y)
            PR_DrawLine(xyStart, xyEnd)
        End If

        ''// Foot lable
        ARMDIA1.PR_SetLayer("Notes")
        ''-----AddEntity("text", sFootLabel, xyToeSeam.X + 1.5, xyToeSeam.y + 1)
        PR_MakeXY(xyStart, xyToeSeam.X + 1.5, xyToeSeam.y + 1)
        ''PR_DrawText(sFootLabel, xyStart, 0.1, 0, 1)
        PR_DrawText(sFootLabel, xyStart, 0.1, 0, 3)
    End Sub
    Private Sub PR_DrawArc(ByRef xyCen As LGLEGDIA1.XY, ByRef nRad As Double, ByRef nStartAng As Double, ByRef nEndAng As Double)
        ' this procedure draws an arc between two points

        Dim nDeltaAng As Object
        nDeltaAng = nEndAng - nStartAng

        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
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
            Using acArc As Arc = New Arc(New Point3d(LGLEGDIA1.xyLegInsertion.X + xyCen.X, LGLEGDIA1.xyLegInsertion.y + xyCen.y, 0),
                                         nRad, nStartAng, nEndAng)

                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acArc.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acArc)
                acTrans.AddNewlyCreatedDBObject(acArc, True)
                idLastCreated = acArc.ObjectId()
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Function FN_CalcAngle(ByRef xyStart As LGLEGDIA1.XY, ByRef xyEnd As LGLEGDIA1.XY) As Double
        'Function to return the angle between two points in degrees
        'in the range 0 - 360
        'Zero is always 0 and is never 360

        Dim X, y As Object
        Dim rAngle As Double

        X = xyEnd.X - xyStart.X
        y = xyEnd.y - xyStart.y

        'Horizomtal
        If X = 0 Then
            If y > 0 Then
                FN_CalcAngle = 90
            Else
                FN_CalcAngle = 270
            End If
            Exit Function
        End If

        'Vertical (avoid divide by zero later)
        If y = 0 Then
            If X > 0 Then
                FN_CalcAngle = 0
            Else
                FN_CalcAngle = 180
            End If
            Exit Function
        End If

        'All other cases
        rAngle = System.Math.Atan(y / X) * (180 / LGLEGDIA1.PI) 'Convert to degrees

        If rAngle < 0 Then rAngle = rAngle + 180 'rAngle range is -PI/2 & +PI/2

        If y > 0 Then
            FN_CalcAngle = rAngle
        Else
            FN_CalcAngle = rAngle + 180
        End If

    End Function

    Function FN_CalcLength(ByRef xyStart As LGLEGDIA1.XY, ByRef xyEnd As LGLEGDIA1.XY) As Double
        'Fuction to return the length between two points
        'Greatfull thanks to Pythagorus
        FN_CalcLength = System.Math.Sqrt((xyEnd.X - xyStart.X) ^ 2 + (xyEnd.y - xyStart.y) ^ 2)
    End Function
    Function FN_CirLinInt(ByRef xyStart As LGLEGDIA1.XY, ByRef xyEnd As LGLEGDIA1.XY, ByRef xyCen As LGLEGDIA1.XY, ByRef nRad As Double, ByRef xyInt As LGLEGDIA1.XY) As Short
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

        nSlope = FN_CalcAngle(xyStart, xyEnd)

        'Horizontal Line
        If nSlope = 0 Or nSlope = 180 Then
            nSlope = -1
            nC = nRad ^ 2 - (xyStart.y - xyCen.y) ^ 2
            If nC < 0 Then
                FN_CirLinInt = False 'no roots
                Exit Function
            End If
            nSign = 1 'test each root
            While nSign > -2
                nRoot = xyCen.X + System.Math.Sqrt(nC) * nSign
                If nRoot >= MANGLOVE1.min(xyStart.X, xyEnd.X) And nRoot <= max(xyStart.X, xyEnd.X) Then
                    xyInt.X = nRoot
                    xyInt.y = xyStart.y
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
                nRoot = xyCen.y + System.Math.Sqrt(nC) * nSign
                If nRoot >= MANGLOVE1.min(xyStart.y, xyEnd.y) And nRoot <= max(xyStart.y, xyEnd.y) Then
                    xyInt.y = nRoot
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
            nM = (xyEnd.y - xyStart.y) / (xyEnd.X - xyStart.X) 'Slope
            nK = xyStart.y - nM * xyStart.X 'Y-Axis intercept
            nA = (1 + nM ^ 2)
            nB = 2 * (-xyCen.X + (nM * nK) - (xyCen.y * nM))
            nC = (xyCen.X ^ 2) + (nK ^ 2) + (xyCen.y ^ 2) - (2 * xyCen.y * nK) - (nRad ^ 2)
            nCalcTmp = (nB ^ 2) - (4 * nC * nA)

            If (nCalcTmp < 0) Then
                FN_CirLinInt = False 'No Roots
                Exit Function
            End If
            nSign = 1
            While nSign > -2
                nRoot = (-nB + (System.Math.Sqrt(nCalcTmp) / nSign)) / (2 * nA)
                If nRoot >= MANGLOVE1.min(xyStart.X, xyEnd.X) And nRoot <= max(xyStart.X, xyEnd.X) Then
                    xyInt.X = nRoot
                    xyInt.y = nM * nRoot + nK
                    FN_CirLinInt = True
                    Exit Function 'Return first root found
                End If
                nSign = nSign - 2
            End While
            FN_CirLinInt = False 'Should never get to here
        End If
        FN_CirLinInt = False
    End Function
    Function StringMiddle(ByRef strText As String, ByRef nStart As Integer, ByRef nLength As Integer) As String
        Return Mid(strText, nStart, nLength)
    End Function
    'To Draw Spline
    Private Sub PR_DrawSpline(ByRef PointCollection As Point3dCollection)
        '' Get the current document and database
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

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
                    acSpline.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acSpline)
                acTrans.AddNewlyCreatedDBObject(acSpline, True)
                idLastCreated = acSpline.ObjectId
            End Using
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawClosingLines(ByRef xyO As LGLEGDIA1.XY, ByRef xyProfileLast As LGLEGDIA1.XY,
                                    ByRef nB As Double, ByRef nA As Double)
        ''-------PR_PutLine("@" & g_sPathJOBST & "\LEG\LGLEGCLS.D;")
        ''---------Execute("menu", "SetLayer", hTemplateLayer);   
        Dim sLegStyle As String = ""
        Select Case nLegStyle
            Case 1 'Knee High
                sLegStyle = "KLN"
            Case 2 'Thigh Length
                sLegStyle = "TLN"
            Case 3 'Knee Band
                sLegStyle = "KBN"
            Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                'Elastic
                sLegStyle = "TBB" 'Thigh Band (B/K)
                If nLegStyle = 4 Then
                    sLegStyle = "TBA" 'Thigh Band (A/K)
                End If
            Case Else 'Anklet,
                sLegStyle = "ANK"
        End Select
        If (nLegStyle = 1) Then
            ''--------hEnt = AddEntity("line", xyO, xyProfileLast)
            PR_DrawLine(xyO, xyProfileLast)
            ''-------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "ClosingLine")
            PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "ClosingLine")
        Else
            ''// All nThighPlt values are calculated in LBLEGDIA VB program
            xyO.X = xyO.X + nThighTopExtension

            ''// Template end point
            Dim aAngle, nLength As Double
            aAngle = FN_CalcAngle(xyPrevProfileLast, xyProfileLast) * (LGLEGDIA1.PI / 180)
            nB = (xyO.X - nThighPltXoff) - xyPrevProfileLast.X
            nA = System.Math.Tan(aAngle) * nB
            Dim xyThighPlt, xyTmp As LGLEGDIA1.XY
            xyThighPlt.X = xyPrevProfileLast.X + nB
            xyThighPlt.y = xyPrevProfileLast.y + nA

            ''// Center of template arc (normal case)
            Dim nThighPltRad, nThighPltStartAngle, nThighPltDeltaAngle As Double
            Select Case nLegStyle
                Case 2
                    nThighPltRad = 23
            End Select
            nB = nThighPltRad - nThighPltXoff
            nA = System.Math.Sqrt(nThighPltRad ^ 2 - nB ^ 2)

            xyTmp.X = xyThighPlt.X - nB ''; 	// Arc Center X
            xyTmp.y = xyThighPlt.y - nA '';		// Arc Center Y
            Dim xyPolar As LGLEGDIA1.XY
            If (xyTmp.y < xyO.y) Then
                ''// Special case where center point Is below fold line
                Dim xyPt1 As LGLEGDIA1.XY
                xyPt1.y = xyO.y + 0.5
                xyPt1.X = xyO.X
                nLength = FN_CalcLength(xyPt1, xyThighPlt)
                aAngle = FN_CalcAngle(xyPt1, xyThighPlt)
                nA = System.Math.Sqrt(nThighPltRad ^ 2 - (nLength / 2) ^ 2)
                ''------xyTmp = CalcXY("relpolar", CalcXY("relpolar", xyPt1, nLength / 2, aAngle), nA, aAngle + 90)
                PR_CalcPolar(xyPt1, nLength / 2, aAngle, xyPolar)
                PR_CalcPolar(xyPolar, nA, aAngle + 90, xyTmp)
                nThighPltStartAngle = FN_CalcAngle(xyTmp, xyPt1) * (LGLEGDIA1.PI / 180)
                nThighPltDeltaAngle = (FN_CalcAngle(xyTmp, xyThighPlt) - nThighPltStartAngle) * (LGLEGDIA1.PI / 180)

            Else
                ''// Normal case
                nThighPltStartAngle = 0
                nThighPltDeltaAngle = (FN_CalcAngle(xyTmp, xyThighPlt) * (LGLEGDIA1.PI / 180)) - nThighPltStartAngle
            End If
            nThighPltDeltaAngle = nThighPltDeltaAngle + (1 * (LGLEGDIA1.PI / 180))
            '  hEnt = AddEntity("arc", xyTmp,
            'nThighPltRad,
            '           nThighPltStartAngle,
            '           nThighPltDeltaAngle)
            '  SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "ClosingLine")
            PR_DrawArc(xyTmp, nThighPltRad, nThighPltStartAngle, nThighPltDeltaAngle)
            PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "ClosingLine")

            ''// Bottom closing line
            ''--------hEnt = AddEntity("line", xyO, CalcXY("relpolar", xyTmp, nThighPltRad, nThighPltStartAngle))
            ''--------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "ClosingLine")
            PR_CalcPolar(xyTmp, nThighPltRad, nThighPltStartAngle * (180 / LGLEGDIA1.PI), xyPolar)
            PR_DrawLine(xyO, xyPolar)
            PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "ClosingLine")

            ''// Modify polyline by moving last vertex
            ''//	nn = GetVertexCount( hCurv) ;
            ''//	SetVertex( hCurv, nn, xyThighPlt) ;
        End If
        ''// Closing line to TOE
        Dim xyStart, xyEnd, xyLeg As LGLEGDIA1.XY
        PR_MakeXY(xyLeg, 0, 0)
        ''-----------If (xyOtemplate.X < xyToeSeam.X) Then
        ''--------------If (LGLEGDIA1.xyLegInsertion.X < xyToeSeam.X) Then
        If (xyLeg.X < xyToeSeam.X) Then
            ''------AddEntity("line", xyO, xyToeSeam.X, xyOtemplate.y)
            PR_MakeXY(xyEnd, xyToeSeam.X, 0)
            PR_DrawLine(xyO, xyEnd)
        Else
            ''----AddEntity("line", xyO, xyOtemplate)
            PR_MakeXY(xyEnd, 0, 0)
            PR_DrawLine(xyO, xyEnd)
        End If

        ''// Seam TRAM Lines
        ARMDIA1.PR_SetLayer("Notes")
        ''------AddEntity("line", xyToeSeam.X, xyO.y + nSeam + 0.5, xyO.X, xyO.y + nSeam + 0.5)
        Dim nSeam As Double = 0.1875
        PR_MakeXY(xyStart, xyToeSeam.X, xyO.y + nSeam + 0.5)
        PR_MakeXY(xyEnd, xyO.X, xyO.y + nSeam + 0.5)
        PR_DrawLine(xyStart, xyEnd)
        ''-------AddEntity("line", xyToeSeam.X, xyO.y + nSeam, xyO.X, xyO.y + nSeam)
        PR_MakeXY(xyStart, xyToeSeam.X, xyO.y + nSeam)
        PR_MakeXY(xyEnd, xyO.X, xyO.y + nSeam)
        PR_DrawLine(xyStart, xyEnd)
    End Sub
    Private Sub PR_DrawLegForThighLand(ByRef nFabricClass As Short, ByRef nStyleLastRed As Short, ByRef xyO As LGLEGDIA1.XY,
                                      ByRef nB As Double, ByRef nA As Double)
        ''-----------PR_PutLine("@" & g_sPathJOBST & "\LEG\LGBNDDWG.D;")
        ''// Establish layer
        Dim strLayer As String = ""
        If (txtLeg.Text.Equals("Left")) Then
            ARMDIA1.PR_SetLayer("TemplateLeft")
            ''--------hTemplateLayer = Table("find", "layer", "TemplateLeft")
            strLayer = "TemplateLeft"
        Else
            ARMDIA1.PR_SetLayer("TemplateRight")
            ''---------hTemplateLayer = Table("find", "layer", "TemplateRight")
            strLayer = "TemplateRight"
        End If
        ''// draw on layer construct
        ''// 'cause drafix is �"^%%%&*&(*&@@ 
        ARMDIA1.PR_SetLayer("Construct")

        ''// Draw Leg 
        ''//
        ''--------PROpenTemplateFile();
        ''Load template data file
        Dim sFile, sLine As String
        If nFabricClass = 0 Then
            ''----------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "MMHG.DAT"
        Else
            ''----------sFile = g_sPathJOBST + "\\TEMPLTS\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
            sFile = fnGetSettingsPath("LookupTables") + "\\WH" + StringMiddle(txtLeftTemplate.Text, 1, 2) + "DS.DAT"
        End If

        Dim hChan As Short
        hChan = FreeFile()
        hChan = FreeFile()
        FileOpen(hChan, sFile, VB.OpenMode.Input)
        Dim nNo, nSpace, n20Len, nReduction, nn As Double
        Dim sScale As String = ""
        sLine = ""
        If (hChan) Then
            'If (Input(hChan, sLine)) Then
            '    ScanLine(sLine, "blank", & nNo, & sScale, & nSpace, & n20Len, & nReduction)

            'Else
            '    Exit (%abort, "Can't read " + sFile + "\nFile maybe corrupted")
            '        End If
            sLine = LineInput(hChan)
            nNo = FN_GetNumber(sLine, 1)
            sScale = FN_GetString(sLine, 2)
            nSpace = FN_GetNumber(sLine, 3)
            n20Len = FN_GetNumber(sLine, 4)
            nReduction = FN_GetNumber(sLine, 5)
        Else
            ''-------Exit (%abort, "Can't open "+ sFile + "\nCheck installation")
            MsgBox("Can't open " + sFile + "\nCheck installation", 48, "Leg Dialog")
        End If

        ''// Skip to FirstTape
        nn = 1
        Dim nStyleFirstTape As Double = fnGetNumber(g_sStyleString, 2)
        While (nn < nStyleFirstTape)
            ''-------GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            nn = nn + 1
        End While
        ScanLine(sLine, nNo, sScale, nSpace, n20Len, nReduction)
        Dim xyTmp As LGLEGDIA1.XY
        ''--------xyTmp = xyOtemplate

        ''// Start drawing profile
        ''// Note, use of polyline for bands with only 3 tapes
        ''//
        Dim nStyleLastTape As Double = fnGetNumber(g_sStyleString, 3)
        Dim bIsSpline As Boolean = False
        If (nStyleLastTape - nStyleFirstTape = 2) Then
            ''-----------StartPoly("polyline")
            bIsSpline = False
        Else
            ''---------StartPoly("fitted")
            bIsSpline = True
        End If

        Dim nTapeLen, nLength, nRedStep, nStyleFirstRed, nThighPltRad As Double
        Dim nElastic As Integer
        Select Case nLegStyle
            Case 2
                nThighPltRad = 23
            Case 3
                nStyleFirstRed = 8
                nElastic = 1
            Case 4, 5
                nThighPltRad = 23
                nElastic = 1
                If nLegStyle = 5 Then
                    nElastic = 0
                End If
                If nStyleFirstTape > 8 Then
                    nStyleFirstRed = 5
                Else
                    nStyleFirstRed = 8
                End If
        End Select

        Dim nSeam As Double = 0.1875
        Dim nFootPleat1, nFootPleat2, nTopLegPleat1, nTopLegPleat2 As Double
        nFootPleat1 = LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat1.Text))
        nFootPleat2 = LGLEGDIA1.fnDisplaytoInches(Val(txtFootPleat2.Text))
        nTopLegPleat1 = LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat1.Text))
        nTopLegPleat2 = LGLEGDIA1.fnDisplaytoInches(Val(txtTopLegPleat2.Text))
        Dim ptPolyColl As Point3dCollection = New Point3dCollection
        While (nn <= nStyleLastTape)
            ''-------nTapeLen = FN_Round(FN_Decimalise(FNGetTape(nn)) * LGLEGDIA1.g_nUnitsFac)
            nTapeLen = FN_Decimalise(FNGetTape(nn)) * LGLEGDIA1.g_nUnitsFac
            nLength = n20Len / 20 * nTapeLen
            nRedStep = n20Len / (20 * 8)
            If (nn = nStyleFirstTape And nStyleFirstRed > 0) Then
                ''// Release last tape to a given reduction
                nLength = (nTapeLen * ((100 - nStyleFirstRed) / 100)) / 2
            End If

            If (nn = nStyleLastTape And nStyleLastRed > 0) Then
                ''// Release last tape to a given reduction
                nLength = (nTapeLen * ((100 - nStyleLastRed) / 100)) / 2
            End If

            ''---------xyTmp.y = xyOtemplate.y + nSeam + nLength
            xyTmp.y = 0 + nSeam + nLength
            ''-------AddVertex(xyTmp)
            ptPolyColl.Add(New Point3d(xyTmp.X, xyTmp.y, 0))
            If (nn = nStyleLastTape) Then
                xyProfileLast = xyTmp
            End If
            If (nn = nStyleLastTape - 1) Then
                xyPrevProfileLast = xyTmp
            End If
            If (nn = nStyleFirstTape) Then
                xyProfileStart = xyTmp
            End If

            nn = nn + 1
            ''----------GetLine(hChan, & sLine)
            sLine = LineInput(hChan)
            ScanLine(sLine, nNo, sScale, nSpace, n20Len, nReduction)
            If ((nn = nStyleFirstTape + 1) And (nFootPleat1 <> 0)) Then
                nSpace = nFootPleat1
            End If
            If ((nn = nStyleFirstTape + 2) And (nFootPleat2 <> 0)) Then
                nSpace = nFootPleat2
            End If
            If ((nn = nStyleLastTape) And (nTopLegPleat1 <> 0)) Then
                nSpace = nTopLegPleat1
            End If
            If ((nn = nStyleLastTape - 1) And (nTopLegPleat2 <> 0)) Then
                nSpace = nTopLegPleat2
            End If
            xyTmp.X = xyTmp.X + nSpace
            ''------------Changed for #65 and #77 in issue list---------------
            If chkStretch.Checked = True And nNo > 6 Then
                xyTmp.X = xyTmp.X + 0.125
            End If
        End While
        ''------Close("file", hChan)
        FileClose(hChan)
        ''---------EndPoly()
        ARMDIA1.PR_SetLayer("TemplateLeft")
        If bIsSpline = True Then
            Dim ptColl As Point3dCollection = New Point3dCollection
            Dim ii As Double
            For ii = 0 To ptPolyColl.Count - 1
                ptColl.Add(New Point3d(LGLEGDIA1.xyLegInsertion.X + ptPolyColl(ii).X, LGLEGDIA1.xyLegInsertion.y + ptPolyColl(ii).Y, 0))
            Next ii
            PR_DrawSpline(ptColl)
        Else
            PR_DrawPoly(ptPolyColl)
        End If
        PR_AddDBValueToLast("Handle", g_sXMarkerHandle)
        Dim sLegStyle As String = ""
        Select Case nLegStyle
            Case 1 'Knee High
                sLegStyle = "KLN"
            Case 2 'Thigh Length
                sLegStyle = "TLN"
            Case 3 'Knee Band
                sLegStyle = "KBN"
            Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                'Elastic
                sLegStyle = "TBB" 'Thigh Band (B/K)
                If nLegStyle = 4 Then
                    sLegStyle = "TBA" 'Thigh Band (A/K)
                End If
            Case Else 'Anklet,
                sLegStyle = "ANK"
        End Select
        PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "LegCurve")
        PR_AddDBValueToLast("Insertion", Str(LGLEGDIA1.xyLegInsertion.X) & " " & Str(LGLEGDIA1.xyLegInsertion.y))

        ''// Get polyline entity handle
        ''// Change layer And set DB values
        Dim sTmp As String = ""
        If (nStyleLastTape - nStyleFirstTape = 2) Then
            sTmp = "layer = 'Construct' AND type = 'Polyline'"
        Else
            sTmp = "layer = 'Construct' AND type = 'Curve'"
        End If

        'hChan = Open("selection", sTmp)
        'If (hChan) Then
        '    ResetSelection(hChan)
        '    hCurv = GetNextSelection(hChan)
        '    SetEntityData(hCurv, "layer", hTemplateLayer)
        '    SetDBData(hCurv, "ID", sLegStyle + sFileNo + sLeg + "LegCurve")
        'End If
        'Close("selection", hChan)

        ''-------Execute("menu", "SetLayer", hTemplateLayer)
        ARMDIA1.PR_SetLayer(strLayer)
        Dim aAngle, nThighPltStartAngle, nThighPltDeltaAngle As Double
        If (nLegStyle = 2 Or nLegStyle = 4 Or nLegStyle = 5) Then
            ''// All nThighPlt values are calculated in LBLEGDIA VB program
            ''// All nThighPlt values are calculated in LBLEGDIA VB program
            xyO.X = xyO.X + nThighTopExtension

            ''// Template end point
            aAngle = FN_CalcAngle(xyPrevProfileLast, xyProfileLast) * (LGLEGDIA1.PI / 180)
            nB = (xyO.X - nThighPltXoff) - xyPrevProfileLast.X
            nA = System.Math.Tan(aAngle) * nB

            Dim xyThighPlt As LGLEGDIA1.XY
            xyThighPlt.X = xyPrevProfileLast.X + nB
            xyThighPlt.y = xyPrevProfileLast.y + nA

            ''// Center of template arc (normal case)
            nB = nThighPltRad - nThighPltXoff
            nA = System.Math.Sqrt(nThighPltRad ^ 2 - nB ^ 2)

            xyTmp.X = xyThighPlt.X - nB ''; 	// Arc Center X
            xyTmp.y = xyThighPlt.y - nA '';		// Arc Center Y

            Dim xyPolar As LGLEGDIA1.XY
            If (xyTmp.y < xyO.y) Then
                ''// Special case where center point Is below fold line
                Dim xyPt1 As LGLEGDIA1.XY
                xyPt1.y = xyO.y + 0.5
                xyPt1.X = xyO.X
                nLength = FN_CalcLength(xyPt1, xyThighPlt)
                aAngle = FN_CalcAngle(xyPt1, xyThighPlt)
                nA = System.Math.Sqrt(nThighPltRad ^ 2 - (nLength / 2) ^ 2)
                ''-------xyTmp = CalcXY("relpolar", CalcXY("relpolar", xyPt1, nLength / 2, aAngle), nA, aAngle + 90)
                PR_CalcPolar(xyPt1, nLength / 2, aAngle, xyPolar)
                PR_CalcPolar(xyPolar, nA, aAngle + 90, xyTmp)
                nThighPltStartAngle = FN_CalcAngle(xyTmp, xyPt1)
                nThighPltDeltaAngle = FN_CalcAngle(xyTmp, xyThighPlt) - nThighPltStartAngle

            Else
                ''// Normal case
                nThighPltStartAngle = 0
                nThighPltDeltaAngle = FN_CalcAngle(xyTmp, xyThighPlt) - nThighPltStartAngle
            End If

            'hEnt = AddEntity("arc", xyTmp,
            '      nThighPltRad,
            '                 nThighPltStartAngle,
            '                 nThighPltDeltaAngle)
            'SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "ClosingLine")
            PR_DrawThighArc(xyTmp, nThighPltRad, (nThighPltStartAngle * (LGLEGDIA1.PI / 180)), (nThighPltDeltaAngle * (LGLEGDIA1.PI / 180)))
            PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "ClosingLine")

            ''// Bottom closing line
            ''------hEnt = AddEntity("line", xyO, CalcXY("relpolar", xyTmp, nThighPltRad, nThighPltStartAngle))
            PR_CalcPolar(xyTmp, nThighPltRad, nThighPltStartAngle, xyPolar)
            PR_DrawLine(xyO, xyPolar)
            ''----------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "ClosingLine")
            PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "ClosingLine")

            ''// Modify polyline by moving last vertex
            'nn = GetVertexCount(hCurv)
            'SetVertex(hCurv, nn, xyThighPlt)

        Else
            xyO.X = xyProfileLast.X
            ''-------xyO.y = xyOtemplate.y
            xyO.y = 0
            ''-------hEnt = AddEntity("line", xyProfileLast, xyO)
            ''--------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "ClosingLine")
            PR_DrawLine(xyProfileLast, xyO)
            PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "ClosingLine")
        End If

        ''// Closing line to Template start And start
        ''--------AddEntity("line", xyO, xyOtemplate)
        Dim xyStart, xyEnd As LGLEGDIA1.XY
        PR_MakeXY(xyEnd, 0, 0)
        PR_DrawLine(xyO, xyEnd)
        ''------------hEnt = AddEntity("line", xyProfileStart, xyOtemplate)
        ''----------SetDBData(hEnt, "ID", sLegStyle + sFileNo + sLeg + "DistalClosingLine")
        PR_DrawLine(xyProfileStart, xyEnd)
        PR_AddDBValueToLast("ID", sLegStyle + txtFileNo.Text + txtLeg.Text + "DistalClosingLine")

        ''// Seam TRAM Lines
        ARMDIA1.PR_SetLayer("Notes")
        ''-------AddEntity("line", xyOtemplate.X, xyO.y + nSeam + 0.5, xyO.X, xyO.y + nSeam + 0.5)
        PR_MakeXY(xyStart, 0, xyO.y + nSeam + 0.5)
        PR_MakeXY(xyEnd, xyO.X, xyO.y + nSeam + 0.5)
        PR_DrawLine(xyStart, xyEnd)
        ''--------AddEntity("line", xyOtemplate.X, xyO.y + nSeam, xyO.X, xyO.y + nSeam)
        PR_MakeXY(xyStart, 0, xyO.y + nSeam)
        PR_MakeXY(xyEnd, xyO.X, xyO.y + nSeam)
        PR_DrawLine(xyStart, xyEnd)

        ''// Notes about elastic 	
        ''---------SetData("TextAngle", 90)
        If (nElastic = 1) Then
            ''----------AddEntity("text", "ELASTIC", xyProfileStart.X + 0.25, xyOtemplate.y + (xyProfileStart.y - xyOtemplate.y) / 2)
            PR_MakeXY(xyStart, xyProfileStart.X + 0.25, xyProfileStart.y / 2)
            PR_DrawText("ELASTIC", xyStart, 0.1, (90 * (LGLEGDIA1.PI / 180)), 1)
        Else
            ''-----------AddEntity("text", "NO ELASTIC", xyProfileStart.X + 0.25, xyOtemplate.y + (xyProfileStart.y - xyOtemplate.y) / 2)
            PR_MakeXY(xyStart, xyProfileStart.X + 0.25, xyProfileStart.y / 2)
            PR_DrawText("NO ELASTIC", xyStart, 0.1, (90 * (LGLEGDIA1.PI / 180)), 1)
        End If

        If (nLegStyle = 3) Then ''// Knee  Bands
            ''----------AddEntity("text", "ELASTIC", xyProfileLast.X - 0.375, xyO.y + (xyProfileLast.y - xyO.y) / 2)
            PR_MakeXY(xyStart, xyProfileLast.X - 0.375, xyO.y + (xyProfileLast.y - xyO.y) / 2)
            PR_DrawText("ELASTIC", xyStart, 0.1, (90 * (LGLEGDIA1.PI / 180)), 1)
        End If

        ''-------SetData("TextAngle", 0)

    End Sub
    Sub PR_DrawPoly(ByRef PointCollection As Point3dCollection)
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

        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

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
                For ii = 0 To PointCollection.Count - 1
                    acPoly.AddVertexAt(ii, New Point2d(LGLEGDIA1.xyLegInsertion.X + PointCollection(ii).X, LGLEGDIA1.xyLegInsertion.y + PointCollection(ii).Y), 0, 0, 0)
                Next ii

                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acPoly.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acPoly)
                acTrans.AddNewlyCreatedDBObject(acPoly, True)
                idLastCreated = acPoly.ObjectId
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using

    End Sub
    Private Sub readDWGInfo()
        Try
            Dim _sClass As New SurroundingClass()
            Dim resbuf As New ResultBuffer
            resbuf = _sClass.GetXrecord("LegInfo", "LEGDIC")
            If (resbuf IsNot Nothing) Then
                Dim arr() As TypedValue = resbuf.AsArray()
                Lbl_0.Text = arr(0).Value
                lbl_1.Text = arr(1).Value
                lbl_2.Text = arr(2).Value
                lbl_3.Text = arr(3).Value
                lbl_4.Text = arr(4).Value
                lbl_5.Text = arr(5).Value
                lbl_6.Text = arr(6).Value
                lbl_7.Text = arr(7).Value
                lbl_8.Text = arr(8).Value
                lbl_9.Text = arr(9).Value
                lbl_10.Text = arr(10).Value
                lbl_11.Text = arr(11).Value
                lbl_12.Text = arr(12).Value
                lbl_13.Text = arr(13).Value
                lbl_14.Text = arr(14).Value
                lbl_15.Text = arr(15).Value
                lbl_16.Text = arr(16).Value
                lbl_17.Text = arr(17).Value
                lbl_18.Text = arr(18).Value
                lbl_19.Text = arr(19).Value
                lbl_20.Text = arr(20).Value
                lbl_21.Text = arr(21).Value
                lbl_22.Text = arr(22).Value
                lbl_23.Text = arr(23).Value
                lbl_24.Text = arr(24).Value
                lbl_25.Text = arr(25).Value
                lbl_26.Text = arr(26).Value
                lbl_27.Text = arr(27).Value
                lbl_28.Text = arr(28).Value
                lbl_29.Text = arr(29).Value

                _txtLeft_0.Text = arr(30).Value
                _txtLeft_1.Text = arr(31).Value
                _txtLeft_2.Text = arr(32).Value
                _txtLeft_3.Text = arr(33).Value
                _txtLeft_4.Text = arr(34).Value
                _txtLeft_5.Text = arr(35).Value
                _txtLeft_6.Text = arr(36).Value
                _txtLeft_7.Text = arr(37).Value
                _txtLeft_8.Text = arr(38).Value
                _txtLeft_9.Text = arr(39).Value
                _txtLeft_10.Text = arr(40).Value
                _txtLeft_11.Text = arr(41).Value
                _txtLeft_12.Text = arr(42).Value
                _txtLeft_13.Text = arr(43).Value
                _txtLeft_14.Text = arr(44).Value
                _txtLeft_15.Text = arr(45).Value
                _txtLeft_16.Text = arr(46).Value
                _txtLeft_17.Text = arr(47).Value
                _txtLeft_18.Text = arr(48).Value
                _txtLeft_19.Text = arr(49).Value
                _txtLeft_20.Text = arr(50).Value
                _txtLeft_21.Text = arr(51).Value
                _txtLeft_22.Text = arr(52).Value
                _txtLeft_23.Text = arr(53).Value
                _txtLeft_24.Text = arr(54).Value
                _txtLeft_25.Text = arr(55).Value
                _txtLeft_26.Text = arr(56).Value
                _txtLeft_27.Text = arr(57).Value
                _txtLeft_28.Text = arr(58).Value
                _txtLeft_29.Text = arr(59).Value

                lblgms_0.Text = arr(60).Value
                lblgms_1.Text = arr(61).Value
                lblgms_2.Text = arr(62).Value
                lblgms_3.Text = arr(63).Value
                lblgms_4.Text = arr(64).Value
                lblgms_5.Text = arr(65).Value
                lblgms_6.Text = arr(66).Value
                lblgms_7.Text = arr(67).Value
                lblgms_8.Text = arr(68).Value
                lblgms_9.Text = arr(69).Value
                lblgms_10.Text = arr(70).Value
                lblgms_11.Text = arr(71).Value
                lblgms_12.Text = arr(72).Value
                lblgms_13.Text = arr(73).Value
                lblgms_14.Text = arr(74).Value
                lblgms_15.Text = arr(75).Value
                lblgms_16.Text = arr(76).Value
                lblgms_17.Text = arr(77).Value
                lblgms_18.Text = arr(78).Value
                lblgms_19.Text = arr(79).Value
                lblgms_20.Text = arr(80).Value
                lblgms_21.Text = arr(81).Value
                lblgms_22.Text = arr(82).Value
                lblgms_23.Text = arr(83).Value

                lblred_0.Text = arr(84).Value
                lblred_1.Text = arr(85).Value
                lblred_2.Text = arr(86).Value
                lblred_3.Text = arr(87).Value
                lblred_4.Text = arr(88).Value
                lblred_5.Text = arr(89).Value
                lblred_6.Text = arr(90).Value
                lblred_7.Text = arr(91).Value
                lblred_8.Text = arr(92).Value
                lblred_9.Text = arr(93).Value
                lblred_10.Text = arr(94).Value
                lblred_11.Text = arr(95).Value
                lblred_12.Text = arr(96).Value
                lblred_13.Text = arr(97).Value
                lblred_14.Text = arr(98).Value
                lblred_15.Text = arr(99).Value
                lblred_16.Text = arr(100).Value
                lblred_17.Text = arr(101).Value
                lblred_18.Text = arr(102).Value
                lblred_19.Text = arr(103).Value
                lblred_20.Text = arr(104).Value
                lblred_21.Text = arr(105).Value
                lblred_22.Text = arr(106).Value
                lblred_23.Text = arr(107).Value

                _txtLeftMM_6.Text = arr(108).Value
                _txtLeftMM_7.Text = arr(109).Value
                _txtLeftMM_8.Text = arr(110).Value
                _txtLeftMM_9.Text = arr(111).Value
                _txtLeftMM_10.Text = arr(112).Value
                _txtLeftMM_11.Text = arr(113).Value
                _txtLeftMM_12.Text = arr(114).Value
                _txtLeftMM_13.Text = arr(115).Value
                _txtLeftMM_14.Text = arr(116).Value
                _txtLeftMM_15.Text = arr(117).Value
                _txtLeftMM_16.Text = arr(118).Value
                _txtLeftMM_17.Text = arr(119).Value
                _txtLeftMM_18.Text = arr(120).Value
                _txtLeftMM_19.Text = arr(121).Value
                _txtLeftMM_20.Text = arr(122).Value
                _txtLeftMM_21.Text = arr(123).Value
                _txtLeftMM_22.Text = arr(124).Value
                _txtLeftMM_23.Text = arr(125).Value
                _txtLeftMM_24.Text = arr(126).Value
                _txtLeftMM_25.Text = arr(127).Value
                _txtLeftMM_26.Text = arr(128).Value
                _txtLeftMM_27.Text = arr(129).Value
                _txtLeftMM_28.Text = arr(130).Value
                _txtLeftMM_29.Text = arr(131).Value
                Dim ii As Double
                For ii = 6 To 29
                    g_iLtMM(ii) = Val(arr(102 + ii).Value)
                Next ii

                '_optType_0.Checked = arr(132).Value
                '_optType_3.Checked = arr(133).Value
                '_optType_1.Checked = arr(134).Value
                '_optType_4.Checked = arr(135).Value
                '_optType_2.Checked = arr(136).Value
                '_optType_5.Checked = arr(137).Value
                _optFabric_0.Checked = arr(138).Value
                _optFabric_1.Checked = arr(139).Value

                cboLeftTemplate.Text = arr(140).Value
                txtFabric.Text = arr(141).Value
                txtToeStyle.Text = arr(142).Value

                chkLeftZipper.CheckState = arr(143).Value
                g_iLtLastZipper = arr(143).Value

                txtFirstTape.Text = arr(144).Value
                txtLastTape.Text = arr(145).Value
                ''-------Added for #159 in the issue list
                cboLastTape.Text = txtLastTape.Text
                cboFirstTape.Text = txtFirstTape.Text
                ''---------------
                txtFootLength.Text = arr(146).Value
                txtChosenStyle.Text = arr(147).Value
                txtLeftTemplate.Text = arr(148).Value

                txtAnklet.Text = arr(149).Value
                txtKneeLength.Text = arr(150).Value
                txtThighLength.Text = arr(151).Value
                txtKneeBand.Text = arr(152).Value
                txtThighBandAK.Text = arr(153).Value
                txtThighBandBK.Text = arr(154).Value

                chkHeelContracture.Checked = arr(155).Value
                chkStump.Checked = arr(156).Value

                If arr.Length > 156 Then
                    txtFootPleat1.Text = arr(157).Value
                    txtFootPleat2.Text = arr(158).Value
                    txtTopLegPleat1.Text = arr(159).Value
                    txtTopLegPleat2.Text = arr(160).Value
                    chkStretch.Checked = arr(161).Value
                End If

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
    Private Sub saveInfoToDWG()
        Try
            Dim _sClass As New SurroundingClass()
            If (_sClass.GetXrecord("LegInfo", "LEGDIC") IsNot Nothing) Then
                _sClass.RemoveXrecord("LegInfo", "LEGDIC")
            End If

            Dim resbuf As New ResultBuffer

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), Lbl_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_12.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_13.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_14.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_15.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_16.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_17.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_18.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_19.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_20.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_21.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_22.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_23.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_24.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_25.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_26.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_27.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_28.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lbl_29.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_12.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_13.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_14.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_15.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_16.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_17.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_18.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_19.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_20.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_21.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_22.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_23.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_24.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_25.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_26.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_27.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_28.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeft_29.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_12.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_13.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_14.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_15.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_16.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_17.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_18.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_19.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_20.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_21.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_22.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblgms_23.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_12.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_13.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_14.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_15.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_16.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_17.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_18.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_19.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_20.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_21.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_22.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblred_23.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_12.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_13.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_14.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_15.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_16.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_17.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_18.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_19.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_20.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_21.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_22.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_23.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_24.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_25.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_26.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_27.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_28.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLeftMM_29.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optType_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optType_3.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optType_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optType_4.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optType_2.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optType_5.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optFabric_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optFabric_1.Checked))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboLeftTemplate.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboFabric.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboToeStyle.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkLeftZipper.CheckState))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtFirstTape.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtLastTape.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtFootLength.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtChosenStyle.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtLeftTemplate.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtAnklet.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtKneeLength.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtThighLength.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtKneeBand.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtThighBandAK.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtThighBandBK.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkHeelContracture.CheckState))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkStump.CheckState))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtFootPleat1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtFootPleat2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtTopLegPleat1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtTopLegPleat2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkStretch.CheckState))

            _sClass.SetXrecord(resbuf, "LegInfo", "LEGDIC")

        Catch ex As Exception

        End Try
    End Sub
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
        PR_MakeXY(LGLEGDIA1.xyLegInsertion, ptStart.X, ptStart.Y)
    End Sub
    Private Sub PR_DrawRuler(ByRef sSymbol As String, ByRef xyStart As LGLEGDIA1.XY, ByRef sTape As String)
        Dim xyPt As LGLEGDIA1.XY
        Dim xyTapeFst, xyTapeSec, xyTapeEnd, xyTapeText As LGLEGDIA1.XY
        Dim sTapeSymbol As String = sSymbol
        sTapeSymbol = Trim(sTapeSymbol)
        PR_MakeXY(xyPt, 0, 0)
        PR_MakeXY(xyTapeFst, xyPt.X, xyPt.y - 0.05)
        PR_MakeXY(xyTapeSec, xyPt.X, xyPt.y - 0.5)
        PR_MakeXY(xyTapeEnd, xyPt.X, xyTapeSec.y + 0.05)
        PR_MakeXY(xyTapeText, xyPt.X, xyPt.y - 0.25)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has(sTapeSymbol) Then
                Dim blkTblRecTape As BlockTableRecord = New BlockTableRecord()
                blkTblRecTape.Name = sTapeSymbol

                Dim acLine As Line = New Line(New Point3d(xyPt.X, xyPt.y, 0), New Point3d(xyTapeFst.X, xyTapeFst.y, 0))
                blkTblRecTape.AppendEntity(acLine)

                acLine = New Line(New Point3d(xyTapeSec.X, xyTapeSec.y, 0), New Point3d(xyTapeEnd.X, xyTapeEnd.y, 0))
                blkTblRecTape.AppendEntity(acLine)

                '' Create a single-line text object
                Dim acText As DBText = New DBText()
                acText.Position = New Point3d(xyTapeText.X, xyTapeText.y, 0)
                acText.Height = 0.125
                acText.TextString = sTape
                acText.Rotation = 0
                acText.Justify = AttachmentPoint.MiddleCenter
                acText.AlignmentPoint = New Point3d(xyTapeText.X, xyTapeText.y, 0)
                blkTblRecTape.AppendEntity(acText)

                acBlkTbl.UpgradeOpen()
                acBlkTbl.Add(blkTblRecTape)
                acTrans.AddNewlyCreatedDBObject(blkTblRecTape, True)
                blkRecId = blkTblRecTape.Id
            Else
                blkRecId = acBlkTbl(sTapeSymbol)
            End If
            ' Insert the block into the current space
            If blkRecId <> ObjectId.Null Then
                Dim blkTblRec As BlockTableRecord = acTrans.GetObject(blkRecId, OpenMode.ForWrite)
                For Each objID As ObjectId In blkTblRec
                    Dim dbObj As Entity = acTrans.GetObject(objID, OpenMode.ForWrite)
                    dbObj.Layer = "Construct"
                Next
                'Create new block reference 
                Dim blkRef As BlockReference = New BlockReference(New Point3d(xyStart.X + LGLEGDIA1.xyLegInsertion.X, xyStart.y + LGLEGDIA1.xyLegInsertion.y, 0), blkRecId)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
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
    Private Sub PR_DrawLegCommonBlock()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim blkId As ObjectId = New ObjectId()
        Dim obj As New BlockCreation.BlockCreation
        blkId = obj.LoadBlockInstance()

        Dim xyStart(5), xyEnd(5) As Double
        xyStart(1) = 0
        xyEnd(1) = 0.875
        xyStart(2) = 0
        xyEnd(2) = 1.0625
        xyStart(3) = 1.5
        xyEnd(3) = 1.0625
        xyStart(4) = 1.5
        xyEnd(4) = 0.875
        xyStart(5) = 0
        xyEnd(5) = 0.875
        Dim xyText As LGLEGDIA1.XY
        PR_MakeXY(xyText, 0.71875, 0.875)
        Dim strTag(2) As String
        strTag(1) = "Fabric"
        strTag(2) = "fileno"

        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRefPatient As BlockReference = acTrans.GetObject(blkId, OpenMode.ForRead)
            Dim ptPosition As Point3d = blkRefPatient.Position
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has("LEGCOMMON") Then
                Dim blkTblRecCross As BlockTableRecord = New BlockTableRecord()
                blkTblRecCross.Name = "LEGCOMMON"
                Dim acPoly As Polyline = New Polyline()
                Dim ii As Double
                For ii = 1 To 5
                    acPoly.AddVertexAt(ii - 1, New Point2d(xyStart(ii), xyEnd(ii)), 0, 0, 0)
                Next ii
                blkTblRecCross.AppendEntity(acPoly)

                Dim acText As DBText = New DBText()
                acText.Position = New Point3d(xyText.X, xyText.y, 0)
                acText.Height = 0.1
                acText.TextString = "LEGS"
                acText.Rotation = 0
                acText.Justify = AttachmentPoint.BottomCenter
                acText.AlignmentPoint = New Point3d(xyText.X, xyText.y, 0)
                blkTblRecCross.AppendEntity(acText)

                Dim acAttDef As New AttributeDefinition
                For ii = 1 To 2
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
                Exit Sub
            End If
            ' Insert the block into the current space
            If blkRecId <> ObjectId.Null Then
                'Create new block reference 
                Dim blkRef As BlockReference = New BlockReference(ptPosition, blkRecId)
                'If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                '    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(ptPosition.X, ptPosition.Y, 0)))
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
                            If acAttRef.Tag.ToUpper().Equals("Fabric", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFabric.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("fileno", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFileNo.Text
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
    Private Sub PR_DrawLegLeftBlock()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim blkId As ObjectId = New ObjectId()
        Dim obj As New BlockCreation.BlockCreation
        blkId = obj.LoadBlockInstance()

        Dim xyStart(5), xyEnd(5) As Double
        xyStart(1) = 0
        xyEnd(1) = 0.875
        xyStart(2) = 0
        xyEnd(2) = 1.0625
        xyStart(3) = 1.5
        xyEnd(3) = 1.0625
        xyStart(4) = 1.5
        xyEnd(4) = 0.875
        xyStart(5) = 0
        xyEnd(5) = 0.875
        Dim xyText As LGLEGDIA1.XY
        PR_MakeXY(xyText, 0.71875, 0.875)
        Dim strTag(18), strTextString(18) As String
        strTag(1) = "fileno"
        strTag(2) = "Leg"
        strTag(3) = "TapeLengthsPt1"
        strTag(4) = "TapeLengthsPt2"
        strTag(5) = "Pressure"
        strTag(6) = "Footlength"
        strTag(7) = "Fabric"
        strTag(8) = "FootPleat1"
        strTag(9) = "TopLegPleat1"
        strTag(10) = "FootPleat2"
        strTag(11) = "TopLegPleat2"
        strTag(12) = "ToeStyle"
        strTag(13) = "Anklet"
        strTag(14) = "ThighLength"
        strTag(15) = "KneeLength"
        strTag(16) = "ThighBand"
        strTag(17) = "ThighBandBK"
        strTag(18) = "KneeBand"

        strTextString(1) = txtFileNo.Text
        strTextString(2) = txtLeg.Text
        strTextString(3) = Mid(txtLeftLengths.Text, 1, 60)
        strTextString(4) = Mid(txtLeftLengths.Text, 61, 60)
        strTextString(5) = txtLeftTemplate.Text
        strTextString(6) = txtFootLength.Text
        strTextString(7) = txtFabric.Text
        strTextString(8) = txtFootPleat1.Text
        strTextString(9) = txtTopLegPleat1.Text
        strTextString(10) = txtFootPleat2.Text
        strTextString(11) = txtTopLegPleat2.Text
        strTextString(12) = txtToeStyle.Text
        strTextString(13) = txtAnklet.Text
        strTextString(14) = txtThighLength.Text
        strTextString(15) = txtKneeLength.Text
        strTextString(16) = txtThighBandAK.Text
        strTextString(17) = txtThighBandBK.Text
        strTextString(18) = txtKneeBand.Text

        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRefPatient As BlockReference = acTrans.GetObject(blkId, OpenMode.ForRead)
            Dim ptPosition As Point3d = blkRefPatient.Position
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has("LEGLEFT") Then
                Dim blkTblRecCross As BlockTableRecord = New BlockTableRecord()
                blkTblRecCross.Name = "LEGLEFT"
                Dim acPoly As Polyline = New Polyline()
                Dim ii As Double
                For ii = 1 To 5
                    acPoly.AddVertexAt(ii - 1, New Point2d(xyStart(ii), xyEnd(ii)), 0, 0, 0)
                Next ii
                blkTblRecCross.AppendEntity(acPoly)

                Dim acText As DBText = New DBText()
                acText.Position = New Point3d(xyText.X, xyText.y, 0)
                acText.Height = 0.1
                acText.TextString = "LEG"
                acText.Rotation = 0
                acText.Justify = AttachmentPoint.BottomCenter
                acText.AlignmentPoint = New Point3d(xyText.X, xyText.y, 0)
                blkTblRecCross.AppendEntity(acText)

                Dim acAttDef As New AttributeDefinition
                For ii = 1 To 18
                    acAttDef = New AttributeDefinition
                    acAttDef.Position = New Point3d(0, 0, 0)
                    acAttDef.Prompt = strTag(ii)
                    acAttDef.Tag = strTag(ii)
                    acAttDef.TextString = strTextString(ii)
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
                blkRecId = acBlkTbl("LEGLEFT")
            End If
            ' Insert the block into the current space
            If blkRecId <> ObjectId.Null Then
                Dim blkRef As BlockReference = New BlockReference(New Point3d(ptPosition.X + 1.5, ptPosition.Y, 0), blkRecId)
                'If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                '    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(ptPosition.X + 1.5, ptPosition.Y, 0)))
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
                idLastCreated = blkRef.ObjectId
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
                            ElseIf acAttRef.Tag.ToUpper().Equals("Leg", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtLeg.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("TapeLengthsPt1", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = Mid(txtLeftLengths.Text, 1, 60)
                            ElseIf acAttRef.Tag.ToUpper().Equals("TapeLengthsPt2", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = Mid(txtLeftLengths.Text, 61, 60)
                            ElseIf acAttRef.Tag.ToUpper().Equals("Pressure", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtLeftTemplate.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("FootLength", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFootLength.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Fabric", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFabric.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("FootPleat1", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFootPleat1.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("TopLegPleat1", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtTopLegPleat1.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("FootPleat2", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFootPleat2.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("TopLegPleat2", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtTopLegPleat2.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("ToeStyle", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtToeStyle.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Anklet", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtAnklet.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("ThighLength", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtThighLength.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("KneeLength", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtKneeLength.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("ThighBand", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtThighBandAK.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("ThighBandBK", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtThighBandBK.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("KneeBand", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtKneeBand.Text
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
    Private Sub PR_DrawThighArc(ByRef xyCen As LGLEGDIA1.XY, ByRef nRad As Double, ByRef nStartAng As Double, ByRef nDeltaAng As Double)
        ' this procedure draws an arc between two points

        Dim nEndAng As Object
        nEndAng = nStartAng + nDeltaAng

        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If
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
            Using acArc As Arc = New Arc(New Point3d(LGLEGDIA1.xyLegInsertion.X + xyCen.X, LGLEGDIA1.xyLegInsertion.y + xyCen.y, 0),
                                         nRad, nStartAng, nEndAng)

                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acArc.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acArc)
                acTrans.AddNewlyCreatedDBObject(acArc, True)
                idLastCreated = acArc.ObjectId
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Function fnDisplayToCM(ByRef nInches As Double) As String
        Dim nCMVal, nDec As Double
        Dim iInt As Short
        iInt = Int(nInches)
        nDec = nInches - iInt
        Dim iEighths As Double = 0
        Dim nPrecision As Double = 0.125
        If nDec <> 0 Then 'Avoid overflow
            iEighths = Int(nDec / nPrecision)
        Else
            iEighths = 0
        End If
        If iEighths <> 0 Then
            Select Case iEighths
                Case 2, 6
                    Dim nVal As Double = iEighths / 2
                    iEighths = nVal / 4
                Case 4
                    iEighths = iEighths + 0.5
                Case Else
                    iEighths = iEighths / 8
            End Select
        End If

        nCMVal = (iInt + iEighths) * 2.54
        nCMVal = System.Math.Abs(nCMVal)
        iInt = Int(nCMVal)
        ''Changed for #165 in the issue list
        If iInt <> 0 Then
            nDec = (nCMVal - iInt) * 10
            nDec = ARMDIA1.round(nDec) / 10
            nCMVal = iInt + nDec
            fnDisplayToCM = Str(nCMVal)
        Else
            fnDisplayToCM = Str(1)
        End If
        ''fnDisplayToCM = Str(nCMVal)
    End Function
    Sub PR_AddDBValueToLast(ByRef sDBName As String, ByRef sDBValue As String)
        'The last entity is given by hEnt
        ''PrintLine(BODYSUIT1.fNum, "if (hEnt) SetDBData( hEnt," & BODYSUIT1.QQ & sDBName & BODYSUIT1.QQ & BODYSUIT1.CC & BODYSUIT1.QQ & sDBValue & BODYSUIT1.QQ & ");")
        If (idLastCreated.IsValid = False) Then
            Exit Sub
        End If
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            Dim acEnt As DBObject = acTrans.GetObject(idLastCreated, OpenMode.ForWrite)
            Dim acRegAppTbl As RegAppTable
            acRegAppTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead)
            Dim acRegAppTblRec As RegAppTableRecord
            If acRegAppTbl.Has(sDBName) = False Then
                acRegAppTblRec = New RegAppTableRecord
                acRegAppTblRec.Name = sDBName
                acRegAppTbl.UpgradeOpen()
                acRegAppTbl.Add(acRegAppTblRec)
                acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, True)
            End If
            Using rb As New ResultBuffer
                rb.Add(New TypedValue(DxfCode.ExtendedDataRegAppName, sDBName))
                rb.Add(New TypedValue(DxfCode.ExtendedDataAsciiString, sDBValue))
                acEnt.XData = rb
            End Using
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_DrawKneeContracturePoly(ByRef PointCollection As Point3dCollection, Optional ByVal bIsSetScale As Boolean = False)
        Dim ii As Short

        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

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
                For ii = 0 To PointCollection.Count - 1
                    acPoly.AddVertexAt(ii, New Point2d(PointCollection(ii).X, PointCollection(ii).Y), 0, 0, 0)
                Next ii
                If bIsSetScale = True Then
                    If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                        acPoly.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                    End If
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acPoly)
                acTrans.AddNewlyCreatedDBObject(acPoly, True)
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawKneeContractureText(ByRef sText As Object, ByRef xyInsert As LGLEGDIA1.XY, ByRef nHeight As Object, ByRef nAngle As Object, ByVal nTextmode As Double)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

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
                acText.Position = New Point3d(xyInsert.X, xyInsert.y, 0)
                acText.Height = nHeight
                acText.TextString = sText
                acText.Rotation = nAngle
                acText.WidthFactor = 0.6
                ''acText.HorizontalMode = nTextmode
                acText.Justify = nTextmode
                ''If acText.HorizontalMode <> TextHorizontalMode.TextLeft Then
                acText.AlignmentPoint = New Point3d(xyInsert.X, xyInsert.y, 0)
                ''End If
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acText.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsert.X, xyInsert.y, 0)))
                End If
                acBlkTblRec.AppendEntity(acText)
                acTrans.AddNewlyCreatedDBObject(acText, True)
            End Using

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawKneeContracture()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim ed As Editor = acDoc.Editor
        Dim sLegStyle As String = ""
        Select Case nLegStyle
            Case 1 'Knee High
                sLegStyle = "KLN"
            Case 2 'Thigh Length
                sLegStyle = "TLN"
            Case 3 'Knee Band
                sLegStyle = "KBN"
            Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                'Elastic
                sLegStyle = "TBB" 'Thigh Band (B/K)
                If nLegStyle = 4 Then
                    sLegStyle = "TBA" 'Thigh Band (A/K)
                End If
            Case Else 'Anklet,
                sLegStyle = "ANK"
        End Select
        Dim sLeg As String = "Left"

        Dim curveFilterType(1) As TypedValue
        curveFilterType.SetValue(New TypedValue(DxfCode.Start, "Spline"), 0)
        curveFilterType.SetValue(New TypedValue(DxfCode.ExtendedDataRegAppName, "ID"), 1)
        Dim curveFilter As SelectionFilter = New SelectionFilter(curveFilterType)
        Dim Result As PromptSelectionResult = ed.SelectAll(curveFilter)
        If Result.Status <> PromptStatus.OK Then
            MsgBox("Origin Marker not found", 16, "Leg Dialog")
            Exit Sub
        End If
        Dim curveSet As SelectionSet = Result.Value
        Dim fitPtsCount As Integer = 0
        Dim ptCurveColl As New Point3dCollection
        Using tr As Transaction = acCurDb.TransactionManager.StartTransaction()
            For Each idSpline As ObjectId In curveSet.GetObjectIds()
                Dim acSpline As Spline = tr.GetObject(idSpline, OpenMode.ForRead)
                Dim rb As ResultBuffer = acSpline.GetXDataForApplication("ID")
                Dim strProfile As String = ""
                If Not IsNothing(rb) Then
                    ' Get the values in the xdata
                    For Each typeVal As TypedValue In rb
                        If typeVal.TypeCode = DxfCode.ExtendedDataRegAppName Then
                            Continue For
                        End If
                        strProfile = strProfile & typeVal.Value
                    Next
                End If
                If strProfile.Contains(sLegStyle + txtFileNo.Text + sLeg + "LegCurve") = False Then
                    Continue For
                End If
                fitPtsCount = acSpline.NumFitPoints
                Dim i As Integer = 0
                While (i < fitPtsCount)
                    ptCurveColl.Add(acSpline.GetFitPointAt(i))
                    i = i + 1
                End While
            Next
            ''tr.Commit()
        End Using

        Dim filterType(1) As TypedValue
        filterType.SetValue(New TypedValue(DxfCode.BlockName, "X Marker"), 0)
        filterType.SetValue(New TypedValue(DxfCode.ExtendedDataRegAppName, "OriginID"), 1)
        Dim selFilter As SelectionFilter = New SelectionFilter(filterType)
        Dim selResult As PromptSelectionResult = ed.SelectAll(selFilter)
        If selResult.Status <> PromptStatus.OK Then
            MsgBox("Origin Marker not found", 16, "Leg Dialog")
            Exit Sub
        End If
        Dim selectionSet As SelectionSet = selResult.Value
        Dim xyOtemplate As LGLEGDIA1.XY
        Dim OriginFound As Boolean = False
        Using acTr As Transaction = acCurDb.TransactionManager.StartTransaction()
            For Each idObject As ObjectId In selectionSet.GetObjectIds()
                Dim blkXMarker As BlockReference = acTr.GetObject(idObject, OpenMode.ForRead)
                Dim position As Point3d = blkXMarker.Position
                If position.X <> 0 Or position.Y <> 0 Then
                    OriginFound = True
                    PR_MakeXY(xyOtemplate, position.X, position.Y)
                    If xyOtemplate.X = LGLEGDIA1.xyLegInsertion.X And xyOtemplate.y = LGLEGDIA1.xyLegInsertion.y Then
                        Exit For
                    End If
                End If
            Next
        End Using
        ''// Check if that the markers have been found, otherwise exit
        If (OriginFound = False) Then
            MsgBox("Origin Marker not found!", 16, "Leg Dialog")
            Exit Sub
        End If
        ''// Start dialog wrt contractures
        Dim oKneeCtrFrm As New KNEECTR_frm
        oKneeCtrFrm.g_sCaption = sLeg + " Knee Contractures"
        oKneeCtrFrm.ShowDialog()
        If oKneeCtrFrm.g_bIsCancel = True Then
            Exit Sub
        End If
        Dim nContracture As Double = 0
        Dim sTmp As String = Mid(oKneeCtrFrm.g_sContracture, 1, 2)
        If (sTmp.Equals("10")) Then nContracture = 0.5
        If (sTmp.Equals("36")) Then nContracture = 1
        If (sTmp.Equals("71")) Then nContracture = 1.5
        Dim nInchToCM As Double = 1
        If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            nInchToCM = 2.54
        End If
        nContracture = nContracture * nInchToCM
        Dim xyKneeDip As LGLEGDIA1.XY
        While (True)
            Dim ptEntOpts As PromptEntityOptions = New PromptEntityOptions(Chr(10) & "Select the Knee Dip tape symbol")
            ptEntOpts.AllowNone = True
            ''ptEntOpts.Keywords.Add("INSERT")
            ptEntOpts.SetRejectMessage(Chr(10) & "Only block")
            ptEntOpts.AddAllowedClass(GetType(BlockReference), True)
            Dim ptEntRes As PromptEntityResult = ed.GetEntity(ptEntOpts)
            If ptEntRes.Status <> PromptStatus.OK Then
                MsgBox("A Leg Profile was not selected", 16, "Leg Dialog")
                Exit Sub
            End If
            Dim idObject As ObjectId = ptEntRes.ObjectId
            Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
                Dim blkXMarker As BlockReference = acTrans.GetObject(idObject, OpenMode.ForRead)
                Dim strBlkName As String = blkXMarker.Name
                If strBlkName.Contains("tape") = False Then
                    Continue While
                End If
                Dim position As Point3d = blkXMarker.Position
                PR_MakeXY(xyKneeDip, position.X, position.Y)
                Exit While
            End Using
        End While
        Dim nn As Integer = 1
        While (nn <= fitPtsCount)
            Dim xyTmp As LGLEGDIA1.XY
            PR_MakeXY(xyTmp, ptCurveColl(nn).X, ptCurveColl(nn).Y)
            Dim nTolerance As Double = System.Math.Abs(xyTmp.X - xyKneeDip.X)
            If (nTolerance < 0.1) Then
                xyKneeDip = xyTmp
                Exit While
            End If
            nn = nn + 1
        End While
        If (nn = fitPtsCount + 1) Then
            MsgBox("Can't find Knee Dip vertex on selected profile.", 16, "Leg Dialog")
            Exit Sub
        End If
        ''// Get next vertex to allow construction of contracture.
        ''// Check availability of vertices to avoid nasty failure And error messages.
        ''// Note:-
        ''//       Special case where contracture width Is longer than the distance between the
        ''//       initially selected next vertex And the knee dip.
        If (nn + 1 > fitPtsCount) Then
            MsgBox("Can't calculate Knee contracture on selected profile.", 16, "Leg Dialog")
            Exit Sub
        End If
        Dim xyPt1, xyPt2, xyInt As LGLEGDIA1.XY
        PR_MakeXY(xyPt1, ptCurveColl(nn + 1).X, ptCurveColl(nn + 1).Y)
        Dim nError As Short = FN_CirLinInt(xyKneeDip, xyPt1, xyKneeDip, nContracture, xyInt)
        If (nError = False) Then
            If (nn + 1 > fitPtsCount) Then
                MsgBox("Can't calculate Knee contracture on selected profile.", 16, "Leg Dialog")
                Exit Sub
            End If
            PR_MakeXY(xyPt2, ptCurveColl(nn + 2).X, ptCurveColl(nn + 2).Y)
            nError = FN_CirLinInt(xyPt1, xyPt2, xyKneeDip, nContracture, xyInt)
            If (nError = False) Then
                MsgBox("Can't calculate Knee contracture on selected profile.", 16, "Leg Dialog")
                Exit Sub
            End If
        End If
        xyPt2 = xyInt
        Dim nKneeCir, aAngle, nA As Double
        nKneeCir = xyKneeDip.y - xyOtemplate.y
        aAngle = FN_CalcAngle(xyKneeDip, xyPt2)
        nA = System.Math.Sqrt((0.5 * nKneeCir) ^ 2 - (0.5 * nContracture) ^ 2)
        ''----------xyPt1 = CalcXY("relpolar", CalcXY("relpolar", xyKneeDip, nContracture * 0.5, aAngle), nA, aAngle + 270)
        PR_CalcPolar(xyKneeDip, nContracture * 0.5, aAngle, xyInt)
        PR_CalcPolar(xyInt, nA, aAngle + 270, xyPt1)

        ''// Draw contracture
        ''//
        ARMDIA1.PR_SetLayer("Notes")
        'StartPoly("polyline")
        'AddVertex(xyKneeDip)
        'AddVertex(xyPt1)
        'AddVertex(xyPt2)
        'EndPoly()
        Dim ptPolyColl As Point3dCollection = New Point3dCollection
        ptPolyColl.Add(New Point3d(xyKneeDip.X, xyKneeDip.y, 0))
        ptPolyColl.Add(New Point3d(xyPt1.X, xyPt1.y, 0))
        ptPolyColl.Add(New Point3d(xyPt2.X, xyPt2.y, 0))
        PR_DrawKneeContracturePoly(ptPolyColl)

        ''------SetData("TextHorzJust", 4) ;
        ''------AddEntity("text", "CONTRACTURE", xyKneeDip.X, xyKneeDip.y - ((xyKneeDip.y - xyPt1.y) / 2));
        xyKneeDip.y = xyKneeDip.y - ((xyKneeDip.y - xyPt1.y) / 2)
        PR_DrawKneeContractureText("CONTRACTURE", xyKneeDip, 0.125, 0, 3)

        ''// Display Contracture size on layer construct (for info only)
        ARMDIA1.PR_SetLayer("Construct")
        ''-------SetData("TextHorzJust", 2) ;
        ''-------AddEntity("text", sContracture, xyPt1.X, xyPt1.y - 0.25); 
        xyPt1.y = xyPt1.y - (0.25 * nInchToCM)
        PR_DrawKneeContractureText(oKneeCtrFrm.g_sContracture, xyPt1, 0.125, 0, 2)
    End Sub
    Sub PR_DrawStumpLine(ByRef xyStart As LGLEGDIA1.XY, ByRef xyFinish As LGLEGDIA1.XY)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout),
                                      OpenMode.ForWrite)

            '' Create a line that starts at 5,5 and ends at 12,3
            Dim acLine As Line = New Line(New Point3d(xyStart.X, xyStart.y, 0),
                                    New Point3d(xyFinish.X, xyFinish.y, 0))

            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                acLine.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
            End If
            '' Add the new object to the block table record and the transaction
            acBlkTblRec.AppendEntity(acLine)
            acTrans.AddNewlyCreatedDBObject(acLine, True)
            idLastCreated = acLine.ObjectId()

            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_DrawStumpCircle(ByRef xyCen As LGLEGDIA1.XY, ByRef nRadius As Object)
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

            '' Create a circle that is at 2,3 with a radius of 4.25
            Using acCirc As Circle = New Circle()
                acCirc.Center = New Point3d(xyCen.X, xyCen.y, 0)
                acCirc.Radius = nRadius
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acCirc.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If

                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acCirc)
                acTrans.AddNewlyCreatedDBObject(acCirc, True)
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_DrawClosedArrow(ByRef xyPoint As LGLEGDIA1.XY, ByRef nAngle As Object)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        Dim xyStart As LGLEGDIA1.XY
        ''PR_MakeXY(xyStart, xyPoint.X, xyPoint.Y + 0.125)
        xyStart = xyPoint
        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)

            '' Create a polyline with two segments (3 points)
            Dim acPoly As Polyline = New Polyline()
            acPoly.AddVertexAt(0, New Point2d(xyStart.X, xyStart.y), 0, 0, 0)
            acPoly.AddVertexAt(0, New Point2d(xyStart.X + 0.125, xyStart.y + 0.0625), 0, 0, 0)
            acPoly.AddVertexAt(0, New Point2d(xyStart.X + 0.125, xyStart.y - 0.0625), 0, 0, 0)
            acPoly.AddVertexAt(0, New Point2d(xyStart.X, xyStart.y), 0, 0, 0)
            acPoly.TransformBy(Matrix3d.Rotation((nAngle * (BODYSUIT1.PI / 180)), Vector3d.ZAxis, New Point3d(xyStart.X, xyStart.y, 0)))
            'If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            '    acPoly.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
            'End If

            '' Add the new object to the block table record and the transaction
            Dim idPolyline As ObjectId = New ObjectId
            idPolyline = acBlkTblRec.AppendEntity(acPoly)
            acTrans.AddNewlyCreatedDBObject(acPoly, True)
            ''Create Hatch entity
            Dim ObjIds As ObjectIdCollection = New ObjectIdCollection
            ObjIds.Add(idPolyline)
            Dim oHatch As Hatch = New Hatch()
            Dim normal As Vector3d = New Vector3d(0.0, 0.0, 1.0)
            oHatch.Normal = normal
            oHatch.Elevation = 0.0
            oHatch.PatternScale = 2.0
            oHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID")
            oHatch.ColorIndex = 1
            acBlkTblRec.AppendEntity(oHatch)
            acTrans.AddNewlyCreatedDBObject(oHatch, True)
            oHatch.Associative = True
            oHatch.AppendLoop(CInt(HatchLoopTypes.Default), ObjIds)
            oHatch.Color = acPoly.Color
            oHatch.EvaluateHatch(True)

            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawStumpText(ByRef sText As Object, ByRef xyInsert As LGLEGDIA1.XY, ByRef nHeight As Object, ByRef nAngle As Object, ByVal nTextmode As Double)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

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
                acText.Position = New Point3d(xyInsert.X, xyInsert.y, 0)
                acText.Height = nHeight
                acText.TextString = sText
                acText.Rotation = nAngle
                acText.WidthFactor = 0.6
                ''acText.HorizontalMode = nTextmode
                acText.Justify = nTextmode
                ''If acText.HorizontalMode <> TextHorizontalMode.TextLeft Then
                acText.AlignmentPoint = New Point3d(xyInsert.X, xyInsert.y, 0)
                ''End If
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acText.TransformBy(Matrix3d.Scaling(2.54, New Point3d(LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y, 0)))
                End If
                acBlkTblRec.AppendEntity(acText)
                acTrans.AddNewlyCreatedDBObject(acText, True)
            End Using

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using
    End Sub
    Private Sub PR_DrawLegStump()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim ed As Editor = acDoc.Editor
        Dim sLegStyle As String = ""
        Select Case nLegStyle
            Case 1 'Knee High
                sLegStyle = "KLN"
            Case 2 'Thigh Length
                sLegStyle = "TLN"
            Case 3 'Knee Band
                sLegStyle = "KBN"
            Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                'Elastic
                sLegStyle = "TBB" 'Thigh Band (B/K)
                If nLegStyle = 4 Then
                    sLegStyle = "TBA" 'Thigh Band (A/K)
                End If
            Case Else 'Anklet,
                sLegStyle = "ANK"
        End Select
        Dim sLeg As String = "Left"

        Dim curveFilterType(1) As TypedValue
        curveFilterType.SetValue(New TypedValue(DxfCode.Start, "Spline"), 0)
        curveFilterType.SetValue(New TypedValue(DxfCode.ExtendedDataRegAppName, "ID"), 1)
        Dim curveFilter As SelectionFilter = New SelectionFilter(curveFilterType)
        Dim Result As PromptSelectionResult = ed.SelectAll(curveFilter)
        If Result.Status <> PromptStatus.OK Then
            MsgBox("A Leg profile was not selected", 16, "Leg Stump")
            Exit Sub
        End If
        Dim curveSet As SelectionSet = Result.Value
        Dim fitPtsCount As Integer = 0
        Dim ptCurveColl As New Point3dCollection
        Dim idSpline As New ObjectId
        Dim nCurveFound As Integer = 0
        Using tr As Transaction = acCurDb.TransactionManager.StartTransaction()
            For Each idObject As ObjectId In curveSet.GetObjectIds()
                Dim acSpline As Spline = tr.GetObject(idObject, OpenMode.ForRead)
                Dim rb As ResultBuffer = acSpline.GetXDataForApplication("ID")
                Dim strProfile As String = ""
                If Not IsNothing(rb) Then
                    ' Get the values in the xdata
                    For Each typeVal As TypedValue In rb
                        If typeVal.TypeCode = DxfCode.ExtendedDataRegAppName Then
                            Continue For
                        End If
                        strProfile = strProfile & typeVal.Value
                    Next
                End If
                If strProfile.Contains(sLegStyle + txtFileNo.Text + sLeg + "LegCurve") = False Then
                    Continue For
                End If
                fitPtsCount = acSpline.NumFitPoints
                Dim i As Integer = 0
                While (i < fitPtsCount)
                    ptCurveColl.Add(acSpline.GetFitPointAt(i))
                    i = i + 1
                End While
                nCurveFound = nCurveFound + 1
                idSpline = acSpline.ObjectId
            Next
        End Using
        If nCurveFound > 1 Then
            ptCurveColl.Clear()
        End If

        ''// Get leg box data
        Dim sSymbol As String = "LEGLEFT"
        If sLegStyle.Equals("ANK") Or sLegStyle.Equals("TLN") Or sLegStyle.Equals("KLN") Then
            MsgBox("This Leg is Not FOOTLESS" & Chr(10) & "Can't draw Stump for this Leg", 16, "Leg Stump")
            Exit Sub
        End If

        Dim sTapeLengths As String = txtLeftLengths.Text
        Dim sStyleString As String = ""
        If sLegStyle.Equals("TBA") Then
            sStyleString = txtThighBandAK.Text
        ElseIf sLegStyle.Equals("TBB") Then
            sStyleString = txtThighBandBK.Text
        ElseIf sLegStyle.Equals("KBN") Then
            sStyleString = txtKneeBand.Text
        End If

        ''// Extract first and last tapes from StyleString
        If sStyleString = "" Then
            MsgBox("Can't extract data from Style String!", 16, "Leg Stump")
            Exit Sub
        End If
        Dim nn As Double = fnGetNumber(sStyleString, 1)
        Dim nFirstTape As Double = fnGetNumber(sStyleString, 2)
        Dim nLastTape As Double = fnGetNumber(sStyleString, 3)

        ''// Release nFirstTape to a .78 reduction
        Dim nTapeLen As Double = ARMDIA1.round(FN_Decimalise(FNGetTape(nFirstTape)) * LGLEGDIA1.g_nUnitsFac)
        Dim nSeam As Double = 0.1875
        Dim nLength As Double = ((nTapeLen * 0.78) / 2) + nSeam
        Dim nTol As Double = System.Math.Abs(nLength - ((nTapeLen * 0.95) + nSeam)) + 0.125
        Dim xyDistal, xyOtemplate, xyTmp As LGLEGDIA1.XY
        PR_MakeXY(xyOtemplate, LGLEGDIA1.xyLegInsertion.X, LGLEGDIA1.xyLegInsertion.y)
        xyDistal.X = xyOtemplate.X
        xyDistal.y = xyOtemplate.y + nLength
        If ptCurveColl.Count > 0 Then
            PR_MakeXY(xyTmp, ptCurveColl(0).X, ptCurveColl(0).Y)
        End If

        Dim nInchToCM As Double = 1
        If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            nInchToCM = 2.54
        End If
        Dim nXVal As Double = 0.125 * nInchToCM
        nTol = nTol * nInchToCM
        If (System.Math.Abs(xyDistal.X - xyTmp.X) > nXVal) Then
            MsgBox("Origin marker and Profile start not within tolerance in X !", 16, "Leg Stump")
            Exit Sub
        End If
        If (System.Math.Abs(xyDistal.y - xyTmp.y) > nTol) Then
            MsgBox("Origin marker and Profile start not within tolerance in Y !", 16, "Leg Stump")
            Exit Sub
        End If
        ''// Draw Stumps
        Dim xyFirstProfile As LGLEGDIA1.XY = xyDistal
        If txtAge.Text <= 10 Then
            xyDistal.X = xyDistal.X - 0.5
        Else
            xyDistal.X = xyDistal.X - 0.75
        End If
        ARMDIA1.PR_SetLayer("TemplateLeft")
        nLength = FN_CalcLength(xyOtemplate, xyDistal)
        Dim aAngle As Double = FN_CalcAngle(xyOtemplate, xyDistal)
        If nLength > 3 Then
            ''// STAR stump
            ''StartPoly("polyline");
            ''// First Point	
            Dim nA As Double = System.Math.Sqrt(((nLength / 3) + 0.125) ^ 2 - (nLength / 6) ^ 2)
            Dim xyPt2, xyMidPt, xyApexPt, xyInt, xyPt3 As LGLEGDIA1.XY
            ''-----xyPt2 = CalcXY("relpolar", xyOtemplate, nLength / 3, aAngle)
            ''---------- xyMidPt = CalcXY("relpolar", xyOtemplate, nLength / 6, aAngle)
            ''----------xyApexPt = CalcXY("relpolar", xyMidPt, nA, aAngle + 90)
            PR_CalcPolar(xyOtemplate, nLength / 3, aAngle, xyPt2)
            PR_CalcPolar(xyOtemplate, nLength / 6, aAngle, xyMidPt)
            PR_CalcPolar(xyMidPt, nA, aAngle + 90, xyApexPt)

            Dim nError As Short = FN_CirLinInt(xyApexPt, xyOtemplate, xyApexPt, 0.125, xyInt)
            Dim ptStumpColl As New Point3dCollection
            ''--------AddVertex(xyOtemplate)
            ''-----------AddVertex(xyInt)
            ptStumpColl.Add(New Point3d(xyOtemplate.X, xyOtemplate.y, 0))
            ptStumpColl.Add(New Point3d(xyInt.X, xyInt.y, 0))

            nError = FN_CirLinInt(xyApexPt, xyPt2, xyApexPt, 0.125, xyInt)
            ''-------AddVertex(xyInt)
            ''-----------AddVertex(xyPt2)
            ptStumpColl.Add(New Point3d(xyInt.X, xyInt.y, 0))
            ptStumpColl.Add(New Point3d(xyPt2.X, xyPt2.y, 0))

            ''// Middle Point
            nA = System.Math.Sqrt(((nLength / 3) + 0.25) ^ 2 - (nLength / 6) ^ 2)
            ''----------xyPt3 = CalcXY("relpolar", xyOtemplate, (nLength / 3) * 2, aAngle)
            ''-----------xyMidPt = CalcXY("relpolar", xyOtemplate, nLength / 2, aAngle)
            ''------------xyApexPt = CalcXY("relpolar", xyMidPt, nA, aAngle + 90)
            PR_CalcPolar(xyOtemplate, (nLength / 3) * 2, aAngle, xyPt3)
            PR_CalcPolar(xyOtemplate, nLength / 2, aAngle, xyMidPt)
            PR_CalcPolar(xyMidPt, nA, aAngle + 90, xyApexPt)

            nError = FN_CirLinInt(xyApexPt, xyPt2, xyApexPt, 0.25, xyInt)
            ''-------AddVertex(xyInt);
            ptStumpColl.Add(New Point3d(xyInt.X, xyInt.y, 0))

            nError = FN_CirLinInt(xyApexPt, xyPt3, xyApexPt, 0.25, xyInt)
            ''---------AddVertex(xyInt);
            ''-----------AddVertex(xyPt3);
            ptStumpColl.Add(New Point3d(xyInt.X, xyInt.y, 0))
            ptStumpColl.Add(New Point3d(xyPt3.X, xyPt3.y, 0))

            ''// Last Point	
            nA = System.Math.Sqrt(((nLength / 3) + 0.125) ^ 2 - (nLength / 6) ^ 2)
            ''---------xyMidPt = CalcXY("relpolar", xyOtemplate, (nLength / 6) * 5, aAngle) 	
            ''-----------xyApexPt = CalcXY("relpolar", xyMidPt, nA, aAngle + 90)
            PR_CalcPolar(xyOtemplate, (nLength / 6) * 5, aAngle, xyMidPt)
            PR_CalcPolar(xyMidPt, nA, aAngle + 90, xyApexPt)

            nError = FN_CirLinInt(xyApexPt, xyPt3, xyApexPt, 0.125, xyInt)
            ''--------AddVertex(xyInt);
            ptStumpColl.Add(New Point3d(xyInt.X, xyInt.y, 0))

            nError = FN_CirLinInt(xyApexPt, xyDistal, xyApexPt, 0.125, xyInt)
            ''------------AddVertex(xyInt);
            ''-----------AddVertex(xyDistal);
            ''-----------AddVertex(xyFirstProfile)
            ptStumpColl.Add(New Point3d(xyInt.X, xyInt.y, 0))
            ptStumpColl.Add(New Point3d(xyDistal.X, xyDistal.y, 0))
            ptStumpColl.Add(New Point3d(xyFirstProfile.X, xyFirstProfile.y, 0))
            PR_DrawKneeContracturePoly(ptStumpColl, True)

            ''//Adjust profile
            ''//There Is a DRAFIX bug that causes drafix to fail if there are only 3
            ''//vertex in the curve when using SetVertex, hence:-
            If (ptCurveColl.Count > 3) Then
                ''--------SetVertex(hCurv, 1, xyFirstProfile)
                Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
                    Dim acSpline As Spline = acTrans.GetObject(idSpline, OpenMode.ForWrite)
                    If acSpline.IsNull <> True Then
                        Dim nLen As Double = System.Math.Abs(xyFirstProfile.y - xyOtemplate.y)
                        xyFirstProfile.y = xyOtemplate.y + (nLen * nInchToCM)
                        acSpline.SetFitPointAt(0, New Point3d(xyFirstProfile.X, xyFirstProfile.y, 0))
                    End If
                    acTrans.Commit()
                End Using
            Else
                MsgBox("As the Curve only has 3 points! it can't be adjusted automatically." & Chr(10) & "You will need to move the end point to the top of the stump manually", 0, "Leg Stump")
            End If

            ''// Ticks And lines
            ARMDIA1.PR_SetLayer("Notes")
            ''----------xyTmp = CalcXY("relpolar", xyPt2, 0.25, aAngle - 90) ;
            ''----------AddEntity("line", xyPt2, xyTmp);
            ''----------AddEntity("line", CalcXY("relpolar", xyTmp, 0.125, aAngle) ,CalcXY("relpolar", xyTmp,  0.125, aAngle+180));
            PR_CalcPolar(xyPt2, 0.25, aAngle - 90, xyTmp)
            PR_DrawStumpLine(xyPt2, xyTmp)
            Dim xyStart, xyEnd, xyArrow As LGLEGDIA1.XY
            PR_CalcPolar(xyTmp, 0.125, aAngle, xyStart)
            PR_CalcPolar(xyTmp, 0.125, aAngle + 180, xyEnd)
            PR_DrawStumpLine(xyStart, xyEnd)

            ''--------xyTmp = CalcXY("relpolar", xyPt3, 0.25, aAngle - 90) ;
            ''------------AddEntity("line", xyPt3, xyTmp);
            ''------------AddEntity("line", CalcXY("relpolar", xyTmp, 0.125, aAngle) ,CalcXY("relpolar", xyTmp,  0.125, aAngle+180))
            PR_CalcPolar(xyPt3, 0.25, aAngle - 90, xyTmp)
            PR_DrawStumpLine(xyPt3, xyTmp)
            PR_CalcPolar(xyTmp, 0.125, aAngle, xyStart)
            PR_CalcPolar(xyTmp, 0.125, aAngle + 180, xyEnd)
            PR_DrawStumpLine(xyStart, xyEnd)
            ''--------AddEntity("marker", "closed arrow", xyDistal.X + 0.5, xyDistal.y, 0.5, 0.125, 270)
            PR_MakeXY(xyArrow, xyDistal.X + 0.5, xyDistal.y)
            PR_DrawClosedArrow(xyArrow, 270)
        Else
            ''// Circular stump
            'StartPoly("polyline") ;
            '  AddVertex(xyOtemplate);
            '  AddVertex(xyDistal);
            '  AddVertex(xyFirstProfile);
            'EndPoly() ;
            Dim ptPolyColl As New Point3dCollection
            ptPolyColl.Add(New Point3d(xyOtemplate.X, xyOtemplate.y, 0))
            ptPolyColl.Add(New Point3d(xyDistal.X, xyDistal.y, 0))
            ptPolyColl.Add(New Point3d(xyFirstProfile.X, xyFirstProfile.y, 0))
            PR_DrawKneeContracturePoly(ptPolyColl, True)

            ''//Adjust profile
            ''//There Is a DRAFIX bug that causes drafix to fail if there are only 3
            ''//vertex in the curve when using SetVertex, hence:-
            If (ptCurveColl.Count > 3) Then
                ''--------SetVertex(hCurv, 1, xyFirstProfile)
                Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
                    Dim acSpline As Spline = acTrans.GetObject(idSpline, OpenMode.ForWrite)
                    If acSpline.IsNull <> True Then
                        Dim nLen As Double = System.Math.Abs(xyFirstProfile.y - xyOtemplate.y)
                        xyFirstProfile.y = xyOtemplate.y + (nLen * nInchToCM)
                        acSpline.SetFitPointAt(0, New Point3d(xyFirstProfile.X, xyFirstProfile.y, 0))
                    End If
                    acTrans.Commit()
                End Using
            Else
                MsgBox("As the Curve only has 3 points! it can't be adjusted automatically." & Chr(10) & "You will need to move the end point to the top of the stump manually", 0, "Leg Stump")
            End If

            ''// Circle
            nLength = FN_CalcLength(xyOtemplate, xyDistal) - nSeam
            nLength = nLength / 3.1416
            xyTmp.y = xyOtemplate.y + nLength + 0.125
            xyTmp.X = xyDistal.X - (nLength + 0.125)
            ''---------AddEntity("circle", xyTmp, nLength + 0.125) ;
            PR_DrawStumpCircle(xyTmp, nLength + 0.125)

            ARMDIA1.PR_SetLayer("Notes")
            PR_DrawStumpCircle(xyTmp, nLength)

            ''----------SetData("TextHorzJust", 2) ;
            ''----------SetData("TextVertJust", 8) ;
            ''----------GetDBData(hTitle, "patient", & sTmp);
            PR_DrawStumpText(txtPatientName.Text, xyTmp, 0.125, 0, 2)
            ''------------SetData("TextVertJust", 32) ;
            ''-----------GetDBData(hTitle, "fileno", & sTmp);
            PR_DrawStumpText(txtFileNo.Text, xyTmp, 0.125, 0, 8)
        End If
        ''// Reset And exit
        ''Execute("menu", "SetLayer", Table("find", "layer", "1")) ;
        ''Exit (%ok, "Stump drawing Complete");
    End Sub
    Private Sub PR_DrawHeelContracture()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim ed As Editor = acDoc.Editor
        Dim sLegStyle As String = ""
        Select Case nLegStyle
            Case 1 'Knee High
                sLegStyle = "KLN"
            Case 2 'Thigh Length
                sLegStyle = "TLN"
            Case 3 'Knee Band
                sLegStyle = "KBN"
            Case 4, 5 'Thigh Band Above Knee and ThighBand Below Knee
                'Elastic
                sLegStyle = "TBB" 'Thigh Band (B/K)
                If nLegStyle = 4 Then
                    sLegStyle = "TBA" 'Thigh Band (A/K)
                End If
            Case Else 'Anklet,
                sLegStyle = "ANK"
        End Select
        Dim sLeg As String = "Left"

        Dim sOriginXdata As String = sLegStyle + txtFileNo.Text + sLeg + "Origin"
        Dim filterType(1) As TypedValue
        filterType.SetValue(New TypedValue(DxfCode.BlockName, "X Marker"), 0)
        filterType.SetValue(New TypedValue(DxfCode.ExtendedDataRegAppName, "OriginID"), 1)
        Dim selFilter As SelectionFilter = New SelectionFilter(filterType)
        Dim selResult As PromptSelectionResult = ed.SelectAll(selFilter)
        If selResult.Status <> PromptStatus.OK Then
            MsgBox("A Leg Profile was not selected", 16, "Heel Contracture")
            Exit Sub
        End If
        Dim selectionSet As SelectionSet = selResult.Value
        Dim sHeelXdata As String = sLegStyle + txtFileNo.Text + sLeg + "Heel"
        Dim HeelfilterType(1) As TypedValue
        HeelfilterType.SetValue(New TypedValue(DxfCode.BlockName, "X Marker"), 0)
        HeelfilterType.SetValue(New TypedValue(DxfCode.ExtendedDataRegAppName, "HeelID"), 1)
        ''HeelfilterType.SetValue(New TypedValue(DxfCode.ExtendedDataAsciiString, sHeelXdata), 2)
        Dim selHeelFilter As SelectionFilter = New SelectionFilter(HeelfilterType)
        selResult = ed.SelectAll(selHeelFilter)
        If selResult.Status <> PromptStatus.OK Then
            MsgBox("A Leg Profile was not selected", 16, "Heel Contracture")
            Exit Sub
        End If
        Dim heelSelectionSet As SelectionSet = selResult.Value

        Dim nMarkersFound As Integer = 0
        Dim xyOtemplate, xyHeel As LGLEGDIA1.XY
        Dim SmallHeel As Boolean = False
        Using acTr As Transaction = acCurDb.TransactionManager.StartTransaction()
            For Each idObject As ObjectId In selectionSet.GetObjectIds()
                Dim blkXMarker As BlockReference = acTr.GetObject(idObject, OpenMode.ForRead)
                Dim rbOrigin As ResultBuffer = blkXMarker.GetXDataForApplication("OriginID")
                Dim strOrigin As String = ""
                If Not IsNothing(rbOrigin) Then
                    ' Get the values in the xdata
                    For Each typeVal As TypedValue In rbOrigin
                        If typeVal.TypeCode = DxfCode.ExtendedDataRegAppName Then
                            Continue For
                        End If
                        strOrigin = strOrigin & typeVal.Value
                    Next
                End If
                If strOrigin.Equals(sOriginXdata) = False Then
                    Continue For
                End If
                If g_sXMarkerHandle <> "" And g_sXMarkerHandle.Contains(blkXMarker.Handle.ToString) = False Then
                    Continue For
                End If
                Dim position As Point3d = blkXMarker.Position
                If position.X <> 0 Or position.Y <> 0 Then
                    nMarkersFound = nMarkersFound + 1
                    ''------PR_MakeXY(xyOtemplate, position.X, position.Y)
                    xyOtemplate.X = position.X
                    xyOtemplate.y = position.Y
                End If
            Next
            For Each idObject As ObjectId In heelSelectionSet.GetObjectIds()
                Dim blkXMarker As BlockReference = acTr.GetObject(idObject, OpenMode.ForRead)
                Dim rbHeel As ResultBuffer = blkXMarker.GetXDataForApplication("HeelID")
                Dim strHeel As String = ""
                If Not IsNothing(rbHeel) Then
                    ' Get the values in the xdata
                    For Each typeVal As TypedValue In rbHeel
                        If typeVal.TypeCode = DxfCode.ExtendedDataRegAppName Then
                            Continue For
                        End If
                        strHeel = strHeel & typeVal.Value
                    Next
                End If
                If strHeel.Equals(sHeelXdata) = False Then
                    Continue For
                End If
                Dim rbHandle As ResultBuffer = blkXMarker.GetXDataForApplication("Handle")
                Dim strXMarkerHandle As String = ""
                If Not IsNothing(rbHandle) Then
                    ' Get the values in the xdata
                    For Each typeVal As TypedValue In rbHandle
                        If typeVal.TypeCode = DxfCode.ExtendedDataRegAppName Then
                            Continue For
                        End If
                        strXMarkerHandle = strXMarkerHandle & typeVal.Value
                    Next
                End If
                If g_sXMarkerHandle <> "" And g_sXMarkerHandle.Contains(strXMarkerHandle) = False Then
                    Continue For
                End If

                Dim resbuf As ResultBuffer = blkXMarker.GetXDataForApplication("Data")
                Dim strXMarkerData As String = ""
                If Not IsNothing(resbuf) Then
                    For Each typeVal As TypedValue In resbuf
                        If typeVal.TypeCode = DxfCode.ExtendedDataRegAppName Then
                            Continue For
                        End If
                        strXMarkerData = typeVal.Value
                        If strXMarkerData.Contains("1") Then
                            SmallHeel = True
                        End If
                    Next
                End If
                Dim position As Point3d = blkXMarker.Position
                If position.X <> 0 Or position.Y <> 0 Then
                    nMarkersFound = nMarkersFound + 1
                    ''------PR_MakeXY(xyHeel, position.X, position.Y)
                    xyHeel.X = position.X
                    xyHeel.y = position.Y
                End If
            Next
        End Using
        ''----------Close("selection", hChan)
        ''// Check if the markers have been found, otherwise exit
        If (nMarkersFound < 2) Then
            MsgBox("Missing markers for selected foot, data not found!", 16, "Heel Contracture")
            Exit Sub
        End If
        If (nMarkersFound > 2) Then
            MsgBox("Two or more drawings of the same style exist!" & Chr(10) & "Delete the extra drawing/s and try again.", 16, "Heel Contracture")
            Exit Sub
        End If

        ''// Calculate contracture points
        Dim nHeelOff As Double
        If (SmallHeel) Then
            nHeelOff = 0.25
            ''-------Changed for #79 � sent 22/02/2019-------------
            ''nHeelOff = nHeelOff + 0.25
            nHeelOff = nHeelOff + 0.183
        Else
            nHeelOff = 0.5
            ''-------Changed for #79 � sent 22/02/2019-------------
            nHeelOff = nHeelOff + 0.325
        End If
        Dim nInchToCM As Double = 1
        Dim nCmToInch As Double = 2.54
        If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            nInchToCM = 2.54
            nCmToInch = 1
        End If
        Dim nHeelCir As Double = xyHeel.y - xyOtemplate.y
        Dim xyPt1, xyPt2, xyPt3 As LGLEGDIA1.XY
        xyPt1.X = xyHeel.X - (nHeelOff * nInchToCM)
        xyPt1.y = xyOtemplate.y

        Dim nSeam As Double = 0.1875 * nInchToCM
        xyPt2.X = xyHeel.X
        ''-------Changed for #79 � sent 21/03/2019-------------
        ''xyPt2.y = xyOtemplate.y + (nSeam + (nHeelCir / 3))
        ''xyPt2.y = xyOtemplate.y + (nSeam + (nHeelCir / 2.75))

        ''-------Changed for #79 � sent 18/04/2019-------------
        ''nHeelCir = Val(txtLeft(5).Text) / 2.7
        ''xyPt2.y = xyOtemplate.y + (nHeelCir / 3)
        nHeelCir = (Val(txtLeft(5).Text) / nCmToInch) / 2.75
        xyPt2.y = xyOtemplate.y + (nHeelCir / 2)

        xyPt3.X = xyHeel.X + (nHeelOff * nInchToCM)
        xyPt3.y = xyOtemplate.y

        ''// Draw contracture
        ARMDIA1.PR_SetLayer("Notes")
        Dim ptCurveColl As Point3dCollection = New Point3dCollection
        ptCurveColl.Add(New Point3d(xyPt1.X, xyPt1.y, 0))
        ptCurveColl.Add(New Point3d(xyPt2.X, xyPt2.y, 0))
        ptCurveColl.Add(New Point3d(xyPt3.X, xyPt3.y, 0))
        PR_DrawKneeContracturePoly(ptCurveColl)

        ''-----------SetData("TextHorzJust", 1) 
        ''-----------AddEntity("text", "CONTRACTURE", xyPt3.X, xyPt3.y + 0.5)
        xyPt3.y = xyPt3.y + (0.5 * nInchToCM)
        PR_DrawKneeContractureText("CONTRACTURE", xyPt3, 0.125, 0, 1)

        ''// Reset And exit
        ''------------Execute("menu", "SetLayer", Table("find", "layer", "1")) ;
        ''MsgBox("Contracture drawing Complete", 0, "Heel Contracture")
    End Sub

    Private Sub cboLastTape_TextChanged(sender As Object, e As EventArgs) Handles cboLastTape.TextChanged
        txtLastTape.Text = cboLastTape.Text
    End Sub

    Private Sub cboFirstTape_TextChanged(sender As Object, e As EventArgs) Handles cboFirstTape.TextChanged
        txtFirstTape.Text = cboFirstTape.Text
    End Sub
End Class