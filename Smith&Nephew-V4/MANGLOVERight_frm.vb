﻿Option Strict Off
Option Explicit On
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports VB = Microsoft.VisualBasic
Friend Class MANGLOVERight_frm
    Inherits System.Windows.Forms.Form
    'Project:   MANGLOVE.MAK
    'Purpose:   Manual glove
    '
    '
    'Version:   3.01
    'Date:      17.Jan.96
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
    'Windows API Functions Declarations
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

    'MsgBox constant
    '    Const IDCANCEL = 2
    '    Const IDYES = 6
    '    Const IDNO = 7

    'Open tip constants
    Const AGE_CUTOFF As Short = 10 'Years
    Const ADULT_STD_OPEN_TIP As Double = 0.5 'Inches
    Const CHILD_STD_OPEN_TIP As Double = 0.375 'Inches
    Public g_iInsertSize As Short

    'Globals set by FN_Open
    Public CC As Object 'Comma
    Public QQ As Object 'Quote
    Public NL As Object 'Newline
    ''------------Public MANGLOVERight_bas.fNum As Object 'Macro file number
    ''--------Public g_RightOnFold As Short
    Public QCQ As Object 'Quote Comma Quote
    Public QC As Object 'Quote Comma
    Public CQ As Object 'Comma Quote

    Public Const g_sDialogID As String = "Glove Dialogue"
    Public g_sFileNo As String 'The patients file no
    Dim g_FingerThumbBlend As MANGLOVERight_bas.curve
    Dim UlnarProfile As MANGLOVERight_bas.curve
    Dim RadialProfile As MANGLOVERight_bas.curve
    Dim g_iLastTape As Short
    Public g_sChangeChecker As String

    'XY data type to represent points
    'Structure XY
    '    Dim X As Double
    '    Dim y As Double
    'End Structure

    'Public Structure curve
    '    Dim n As Short
    '    <VBFixedArray(100)> Dim X() As Double
    '    <VBFixedArray(100)> Dim y() As Double

    '    'UPGRADE_TODO: "Initialize" must be called to initialize instances of this structure. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"'
    '    Public Sub Initialize()
    '        'UPGRADE_WARNING: Lower bound of array X was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    '        ReDim X(100)
    '        'UPGRADE_WARNING: Lower bound of array y was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    '        ReDim y(100)
    '    End Sub
    'End Structure



    Private Sub Cancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Cancel.Click
        Dim Response As Short
        Dim sTask, sCurrentValues As String

        'Check if data has been modified
        ''If FN_GloveDataChanged() Then
        sCurrentValues = FN_GloveDataChanged()
        If sCurrentValues <> g_sChangeChecker Then
            Response = MsgBox("Changes have been made, Save changes before closing", 35, "Glove - Right")
            Select Case Response
                Case IDYES
                    PR_UpdateDDE()
                    PR_UpdateDDE_Extension()
                    Dim sDrawFile As String = fnGetSettingsPath("PathDRAW") & "\draw.d"
                    PR_CreateSaveMacro(sDrawFile)
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
        ManGlvMain.ManGlvMainDlg.Close()
    End Sub

    Public Function PR_CloseRightGlvDialog() As Boolean
        Dim Response As Short
        Dim sCurrentValues As String
        sCurrentValues = FN_GloveDataChanged()
        If sCurrentValues <> g_sChangeChecker Then
            Response = MsgBox("Changes have been made, Save changes before closing", 35, "Glove - Right")
            Select Case Response
                Case IDYES
                    PR_UpdateDDE()
                    PR_UpdateDDE_Extension()
                    Dim sDrawFile As String = fnGetSettingsPath("PathDRAW") & "\draw.d"
                    PR_CreateSaveMacro(sDrawFile)
                    saveInfoToDWG()
                Case IDCANCEL
                    Return False
            End Select
        End If
        Return True
    End Function

    'UPGRADE_WARNING: Event cboFlaps.SelectedIndexChanged may fire when form is initialized. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
    Private Sub cboFlaps_SelectedIndexChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cboFlaps.SelectedIndexChanged
        ''------------------------------------------------
        'If InStr(1, CType(MANGLOVERight_bas.MainForm.Controls("cboFlaps"), Object).Text, "D") > 0 Then
        '    CType(MANGLOVERight_bas.MainForm.Controls("lblFlap"), Object)(4).Enabled = True
        '    CType(MANGLOVERight_bas.MainForm.Controls("txtWaistCir"), Object).Enabled = True
        '    CType(MANGLOVERight_bas.MainForm.Controls("labWaistCir"), Object).Enabled = True
        'ElseIf CType(MANGLOVERight_bas.MainForm.Controls("lblFlap"), Object)(4).Enabled = True Then
        '    CType(MANGLOVERight_bas.MainForm.Controls("txtWaistCir"), Object).Text = ""
        '    CType(MANGLOVERight_bas.MainForm.Controls("labWaistCir"), Object).Text = ""
        '    CType(MANGLOVERight_bas.MainForm.Controls("lblFlap"), Object)(4).Enabled = False
        '    CType(MANGLOVERight_bas.MainForm.Controls("txtWaistCir"), Object).Enabled = False
        '    CType(MANGLOVERight_bas.MainForm.Controls("labWaistCir"), Object).Enabled = False
        'End If
        ''----------------------------------------------

        If InStr(1, cboFlaps.Text, "D") > 0 Then
            lblFlap(4).Enabled = True
            txtWaistCir.Enabled = True
            labWaistCir.Enabled = True
        ElseIf lblFlap(4).Enabled = True Then
            txtWaistCir.Text = ""
            labWaistCir.Text = ""
            lblFlap(4).Enabled = False
            txtWaistCir.Enabled = False
            labWaistCir.Enabled = False
        End If

        If MANGLOVERight_bas.MainForm.Visible Then PR_CalculateArmTapeReductions()

    End Sub

    Private Sub cmdCalculate_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCalculate.Click
        PR_CalculateArmTapeReductions()
    End Sub

    Private Sub cmdCalculateInsert_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCalculateInsert.Click
        Static iError, iIndex As Short
        If FN_ValidateData("Ignore Webs") Then
            'Common Dialogue elements
            If optFold(1).Checked = True Then g_RightOnFold = True Else g_RightOnFold = False
            PR_GetDlgHandData()
            MANGLOVERight_bas.PR_LoadReductionCharts((cboFabric.Text))
            'UPGRADE_WARNING: Couldn't resolve default property of object FN_CalculateInsert(g_iInsertSize, CALC). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If FN_CalculateInsert(g_iInsertSize, "CALC") Then
                'Translate and display
                Select Case g_iInsertSize
                    Case 1 To 5
                        iIndex = g_iInsertSize + 3
                    Case 6
                        iIndex = 3
                End Select

                'We need to force a calculation if the user changes
                'fold options but if this is done before we have enough
                'data we will confuse the user as they have not explititly
                'calculated.
                g_RightDataIsCalcuable = True
            Else
                'Set Drop down to calculate on fail
                iIndex = 0
            End If
            cboInsert.SelectedIndex = iIndex
        End If
    End Sub

    Private Function FN_CalcGlovePoints(ByRef iInsert As Short) As Short
        'Calculate the points on the glove based on the insert given
        'and the data in the dialogue
        'Note:-
        ' Figure both Left and Right the same.
        ' We then mirror the result in the vertical line
        ' given by X = xyRightDatum.x
        FN_CalcGlovePoints = True

        Dim nFiguredPalm, nFiguredWrist As Double

        Dim nPIPs, nMaxWeb As Double
        Dim nFiguredPIPs, nMissingPIPs As Double
        Dim ii, nn, Response As Short
        Dim nMinDist, nThumbSlantLength, nInsert, nSlantInsert, aPalmThumbEnd, nInc As Double
        Dim nMinRadius, nThumbLineToWeb, nLength As Double
        Dim xyThumbOriginal, xyMinRadius, xyMinRadiusC1, xyThumbPalm5 As MANGLOVERight_bas.XY
        Dim iMaxLoopCount, Success, ForcedFail, iLoopCount As Short
        Dim nCheckDistance As Double

        nInsert = g_iRightInsertValue(iInsert) * RIGHT_EIGHTH
        nSlantInsert = 0
        nInc = 0.03125 '1/32nd of an inch

        'Calculate Figured DIPs and PIPs for the fingers
        For nn = 1 To 4
            If g_iRightFINGER_CHART(nn) = RIGHT_THUMB Then
                g_RightFinger(nn).PIP = FN_RightFigureFinger(RIGHT_THUMB, g_nRightFingerPIPCir(nn), iInsert)
                g_RightFinger(nn).DIP = FN_RightFigureFinger(RIGHT_THUMB, g_nRightFingerDIPCir(nn), iInsert)
            Else
                g_RightFinger(nn).PIP = FN_RightFigureFinger(RIGHT_PIP, g_nRightFingerPIPCir(nn), iInsert)
                g_RightFinger(nn).DIP = FN_RightFigureFinger(RIGHT_DIP, g_nRightFingerDIPCir(nn), iInsert)
            End If
            'If not missing then minimun value is 3/8th"
            If g_RightFinger(nn).PIP > 0 Then
                g_RightFinger(nn).PIP = g_RightFinger(nn).PIP + g_nRightFINGER_FIGURE(nn)
                If g_RightFinger(nn).PIP < 0.375 And g_RightFinger(nn).PIP > 0 Then g_RightFinger(nn).PIP = 0.375
                g_RightFinger(nn).DIP = g_RightFinger(nn).DIP + g_nRightFINGER_FIGURE(nn)
                If g_RightFinger(nn).DIP < 0.375 And g_RightFinger(nn).DIP > 0 Then g_RightFinger(nn).DIP = 0.375
            End If
            nFiguredPIPs = nFiguredPIPs + g_RightFinger(nn).PIP
        Next nn

        'Figure palm and wrist
        nFiguredPalm = (FN_RightFigureTape(RIGHT_PALM, g_nRightPalm) + g_nRightPALM_FIGURE) / 2
        nFiguredWrist = (FN_RightFigureTape(RIGHT_WRIST, g_nRightWrist) + g_nRightPALM_FIGURE) / 2

        'Thumb
        If g_iRightThumbStyle = 2 Then
            'Setin thumb
            g_RightFinger(5).PIP = FN_RightFigureFinger(RIGHT_PIP, g_nRightFingerPIPCir(5), iInsert)
        ElseIf g_iRightThumbStyle = 0 Then
            'Normal thumbs
            g_RightFinger(5).PIP = FN_RightFigureFinger(RIGHT_THUMB, g_nRightFingerPIPCir(5), iInsert)
        Else
            'Edged thumbs
            'Do not figure
            g_RightFinger(5).PIP = g_nRightFingerPIPCir(5)
        End If
        g_RightFinger(5).DIP = 0

        'Check for missing fingers
        'If there are missing fingers then we need establish the width of the
        'RIGHT_PIP for the missing fingers, we use the Figured palm as the total
        'width of the PIPs, subtract the actual total PIPs and spread the rest
        'over the missing fingers
        'Note the exception for Fused Fingers
        If g_RightMissingFingers > 0 And Not g_RightFusedFingers Then
            nMissingPIPs = System.Math.Abs(nFiguredPalm - nFiguredPIPs) / g_RightMissingFingers
            For ii = 1 To 4
                If g_RightMissing(ii) = True Then g_RightFinger(ii).PIP = nMissingPIPs
            Next ii
        End If

        'Establish Datum point
        If g_RightOnFold Then
            MANGLOVERight_bas.PR_MakeXY(xyRightDatum, 0, MANGLOVERight_bas.FN_LengthWristToEOS())
        Else
            MANGLOVERight_bas.PR_MakeXY(xyRightDatum, 1, MANGLOVERight_bas.FN_LengthWristToEOS())
        End If

        'Missing web heights can represent amputated fingers etc
        'we need to account for these

        'Missing thumb web (use lowest - 0.375)
        If g_nRightWeb(4) = 0 Then
            g_nRightWeb(4) = 1000000 'Dummy high value
            For ii = 3 To 1 Step -1
                If g_nRightWeb(ii) < g_nRightWeb(4) Then g_nRightWeb(4) = g_nRightWeb(ii)
            Next ii
            g_nRightWeb(4) = g_nRightWeb(4) - 0.375
        End If

        'Missing middle
        If g_nRightWeb(3) = 0 Then g_nRightWeb(3) = g_nRightWeb(4) + 0.25

        'Missing Ring
        If g_nRightWeb(2) = 0 And g_nRightWeb(1) <> 0 Then g_nRightWeb(2) = g_nRightWeb(1) 'Use Little
        If g_nRightWeb(2) = 0 And g_nRightWeb(1) = 0 Then g_nRightWeb(2) = g_nRightWeb(3) 'Use Middle if no little

        'Missing Little
        If g_nRightWeb(1) = 0 Then g_nRightWeb(1) = g_nRightWeb(2) 'Use Ring

        'Little RightFinger (at web)
        nPIPs = g_RightFinger(1).PIP
        MANGLOVERight_bas.PR_MakeXY(xyRightLittle, xyRightDatum.X + nPIPs, xyRightDatum.y + g_nRightWeb(1))

        'Little RightFinger at side of hand
        MANGLOVERight_bas.PR_MakeXY(xyRightLFS, xyRightDatum.X, xyRightLittle.y)

        'Ring finger
        nPIPs = g_RightFinger(1).PIP + g_RightFinger(2).PIP
        MANGLOVERight_bas.PR_MakeXY(xyRightRing, xyRightDatum.X + nPIPs, xyRightDatum.y + g_nRightWeb(2))

        'Middle finger
        nPIPs = g_RightFinger(1).PIP + g_RightFinger(2).PIP + g_RightFinger(3).PIP
        MANGLOVERight_bas.PR_MakeXY(xyRightMiddle, xyRightDatum.X + nPIPs, xyRightDatum.y + g_nRightWeb(3))

        'Index finger
        nPIPs = g_RightFinger(1).PIP + g_RightFinger(2).PIP + g_RightFinger(3).PIP + g_RightFinger(4).PIP
        MANGLOVERight_bas.PR_MakeXY(xyRightIndex, xyRightDatum.X + nPIPs, xyRightMiddle.y)

        'Thumb
        If g_RightOnFold Then
            MANGLOVERight_bas.PR_MakeXY(xyRightThumb, xyRightDatum.X + nFiguredPalm, xyRightDatum.y + g_nRightWeb(4))
        Else
            If nPIPs <> nFiguredPalm Then
                'Avoid overflow
                MANGLOVERight_bas.PR_MakeXY(xyRightThumb, xyRightDatum.X + (((nPIPs - nFiguredPalm) / 2) + nFiguredPalm), xyRightDatum.y + g_nRightWeb(4))
            Else
                MANGLOVERight_bas.PR_MakeXY(xyRightThumb, xyRightDatum.X + nFiguredPalm, xyRightDatum.y + g_nRightWeb(4))
            End If
        End If

        'Set finger points
        g_RightFinger(1).xyUlnar = xyRightLFS
        g_RightFinger(1).xyRadial = xyRightLittle
        g_RightFinger(1).WebHt = xyRightLittle.y

        g_RightFinger(2).xyUlnar = xyRightLittle
        g_RightFinger(2).xyRadial = xyRightRing
        g_RightFinger(2).WebHt = xyRightRing.y

        g_RightFinger(3).xyUlnar = xyRightRing
        g_RightFinger(3).xyRadial = xyRightMiddle
        g_RightFinger(3).WebHt = xyRightMiddle.y

        g_RightFinger(4).xyUlnar = xyRightMiddle
        g_RightFinger(4).xyRadial = xyRightIndex
        g_RightFinger(4).WebHt = xyRightMiddle.y

        'Palm points
        'Wrist line
        If g_RightOnFold Then
            MANGLOVERight_bas.PR_MakeXY(xyRightPalm(1), xyRightLFS.X, xyRightDatum.y)
        Else
            If nPIPs <> nFiguredWrist Then
                'Avoids overflow
                MANGLOVERight_bas.PR_MakeXY(xyRightPalm(1), xyRightDatum.X + ((nPIPs - nFiguredWrist) / 2), xyRightDatum.y)
            Else
                MANGLOVERight_bas.PR_MakeXY(xyRightPalm(1), xyRightDatum.X, xyRightDatum.y)
            End If
        End If

        'Offset from wrist line
        If g_RightOnFold Then
            xyRightPalm(2).X = xyRightLFS.X
        Else
            If nPIPs <> nFiguredPalm Then
                'Avoids overflow
                xyRightPalm(2).X = xyRightLFS.X + (nPIPs - nFiguredPalm) / 2
            Else
                xyRightPalm(2).X = xyRightLFS.X
            End If
        End If

        'Establish maximum Web Height to get offset
        nMaxWeb = 0
        For ii = 1 To 3
            If g_nRightWeb(ii) > nMaxWeb Then nMaxWeb = g_nRightWeb(ii)
        Next ii

        If nMaxWeb > 3.5 Then
            xyRightPalm(2).y = xyRightDatum.y + 1
        Else
            xyRightPalm(2).y = xyRightDatum.y + 0.5
        End If

        'Offset from wrist at Thumb Side
        xyRightPalm(5).X = xyRightPalm(2).X + nFiguredPalm
        xyRightPalm(5).y = xyRightPalm(2).y

        'Wrist at Thumb Side
        xyRightPalm(6).X = xyRightPalm(1).X + nFiguredWrist
        xyRightPalm(6).y = xyRightPalm(1).y


        'Figure Lengths for fingers and thumbs
        Dim nTipCutBack As Double
        Dim iDigitType As Short

        If Val(txtAge.Text) <= AGE_CUTOFF Then
            nTipCutBack = CHILD_STD_OPEN_TIP
        Else
            nTipCutBack = ADULT_STD_OPEN_TIP
        End If

        iDigitType = RIGHT_FINGER_LEN
        For ii = 1 To 5
            If ii = 5 Then iDigitType = RIGHT_THUMB_LEN 'Swap for thumbs
            Select Case g_RightFinger(ii).Tip
                Case 0, 10
                    'Closed tips (Figured Length)
                    g_RightFinger(ii).Len_Renamed = FN_RightFigureLength(iDigitType, g_nRightFingerLen(ii))
                Case 1, 11
                    'Standard Cut back for open tips (GivenLength - StdCutBack)
                    g_RightFinger(ii).Len_Renamed = g_nRightFingerLen(ii) - nTipCutBack
                Case 2, 12
                    'Open at a desired given length
                    g_RightFinger(ii).Len_Renamed = g_nRightFingerLen(ii)
            End Select
        Next ii

        'Thumbpoints on palm
        'Angles
        aPalmThumbEnd = 0

        xyThumbOriginal = xyRightThumb 'Store original to give a message later
        xyRightPalmThumb(1) = xyRightThumb

        Dim nDrop As Double
        If g_iRightThumbStyle <> 1 Then
            'Normal or SetIn thumbs
            xyRightPalmThumb(1).y = xyRightThumb.y + 0.75

            'Ensure mininum distance of 3/8" from Index web
            If (xyRightIndex.y - xyRightPalmThumb(1).y) < 0.375 Then
                xyRightPalmThumb(1).y = xyRightIndex.y - 0.375
            End If
            'Ensure mininum distance of 1/8" from top of thumb curve to thumb web
            If xyRightPalmThumb(1).y - xyRightThumb.y < 0.125 Then
                xyRightThumb.y = xyRightPalmThumb(1).y - 0.125
            End If
            nThumbSlantLength = g_RightFinger(5).PIP / (1 / System.Math.Sqrt(2))

            'This loop counter ensures that the bottom of thumb curve does not go closer
            'than 0.25" to the EOS

            '        iMaxLoopCount = Int((((xyRightThumb.Y - xyRightPalm(6).Y) - nThumbSlantLength)) / .0625)
            iMaxLoopCount = MANGLOVERight_bas.min(25, Int((((xyRightThumb.y - 0.25) - nThumbSlantLength)) / 0.0625))
            iLoopCount = 0
            Do  'at least once
                'Get blending curve and revise xyRightPalmThumb(1) X value
                PR_CalcRightFingerBlendingProfile(xyRightIndex, xyRightThumb, xyRightMiddle, g_RightFinger(4), g_FingerThumbBlend, xyRightPalmThumb(1))

                xyRightPalmThumb(2).y = xyRightPalmThumb(1).y
                xyRightPalmThumb(2).X = xyRightPalmThumb(1).X - nInsert

                xyRightPalmThumb(5).y = xyRightPalmThumb(2).y
                '           xyRightPalmThumb(5).X = xyRightPalmThumb(2).X - .125
                xyRightPalmThumb(5).X = xyRightPalmThumb(2).X - 0.25

                xyRightPalmThumb(3).y = xyRightThumb.y
                xyRightPalmThumb(3).X = xyRightPalmThumb(1).X - ((nFiguredPalm / 2) - 0.125)

                'Check that Point 3 is always less than point 5, if it is not
                'then make it so by an eighth
                If xyRightPalmThumb(5).X - xyRightPalmThumb(3).X < 0.125 Then xyRightPalmThumb(3).X = xyRightPalmThumb(5).X - 0.125

                'Get top thumb curve
                'We find the top curve by iteration, accepting the first
                'by changing the X value of xyRightPalmThumb(5)
                nMinDist = MANGLOVERight_bas.min(System.Math.Abs(xyRightPalmThumb(3).y - xyRightPalmThumb(5).y), System.Math.Abs(xyRightPalmThumb(3).X - xyRightPalmThumb(5).X))
                xyThumbPalm5 = xyRightPalmThumb(5) 'retain for later use
                Success = False
                ForcedFail = False
                nCheckDistance = System.Math.Abs(xyRightPalmThumb(3).X - xyRightPalmThumb(5).X)
                'Try first moving towards the center of the palm away from xyRightPalmThumb(2)
                While Not Success And nCheckDistance > nMinDist And Not ForcedFail
                    ii = FN_RightBiArcCurve(xyRightPalmThumb(3), 90, xyRightPalmThumb(5), aPalmThumbEnd, g_RightThumbTopCurve)

                    If ii Then
                        'Check that the distance from the thumbline is more than 3/8th from xyRightMiddle
                        'allow adjustement to continue the other way if it is
                        nLength = MANGLOVERight_bas.FN_CalcLength(g_RightThumbTopCurve.xyR1, xyRightRing) - g_RightThumbTopCurve.nR1
                        nLength = ARMDIA1.round(nLength / 0.0625)
                        '                    If nLength < .375 Then
                        If nLength < 6 Then
                            ForcedFail = True
                        Else
                            Success = True
                        End If
                    Else
                        'Adjust and try again BUT!
                        'Check that xyRightPalmThumb(5).x - xyRightPalmThumb(3) is not silly
                        'IE stop any infinite loops.
                        '                    xyRightPalmThumb(5).X = xyRightPalmThumb(5).X + nInc
                        xyRightPalmThumb(5).X = xyRightPalmThumb(5).X - nInc
                        If System.Math.Abs(xyRightPalmThumb(3).X - xyRightPalmThumb(5).X) > nCheckDistance Then ForcedFail = True
                        nCheckDistance = System.Math.Abs(xyRightPalmThumb(3).X - xyRightPalmThumb(5).X)
                    End If
                End While

                'If the above does not work then try moving away from the center
                'of the palm towards (but not past) xyRightPalmThumb(2)
                If Not Success Then
                    xyRightPalmThumb(5) = xyThumbPalm5
                    For nLength = xyRightPalmThumb(5).X To xyRightPalmThumb(2).X Step (nInc / 2)
                        ii = FN_RightBiArcCurve(xyRightPalmThumb(3), 90, xyRightPalmThumb(5), aPalmThumbEnd, g_RightThumbTopCurve)
                        If ii Then
                            'Check that the distance from the thumbline is more than 3/8th from xyRightRing
                            'allow adjustement to continue until fail
                            If (MANGLOVERight_bas.FN_CalcLength(g_RightThumbTopCurve.xyR1, xyRightRing) - g_RightThumbTopCurve.nR1) >= 0.375 Then
                                Success = True
                                Exit For
                            Else
                                'Adjust and try again
                                xyRightPalmThumb(5).X = nLength
                            End If
                        Else
                            'Adjust and try again
                            xyRightPalmThumb(5).X = nLength
                        End If
                    Next nLength
                End If

                iLoopCount = iLoopCount + 1

                If Success Or iLoopCount >= iMaxLoopCount Then Exit Do

                'If we don't have a sucessfull top thumb line then lower
                'the top line or if this is not possible then drop xyRightThumb
                'Do this in increments of nDrop
                nDrop = 0.0625
                'Drop top thumb curve (Ensure a minimum of 1/8th inch)
                If (xyRightPalmThumb(1).y - xyRightThumb.y) >= 0.25 Then
                    'Drop top curve
                    xyRightPalmThumb(1).y = xyRightPalmThumb(1).y - 0.125
                Else
                    'Drop thumb
                    xyRightThumb.y = xyRightThumb.y - nDrop
                End If
            Loop

            If Not Success Then
                MsgBox("Can't form TOP part of thumb curve on palm!. Adjust the palm curve and use the Transfer tool to update the thumb", 48, g_sDialogID)
                'Degenerate "ThumbTopCurve" to a straight line
                xyRightPalmThumb(5) = xyThumbPalm5
                g_RightThumbTopCurve.xyStart = xyRightPalmThumb(3)
                g_RightThumbTopCurve.xyEnd = xyThumbPalm5
                g_RightThumbTopCurve.nR1 = 0
                g_RightThumbTopCurve.nR2 = 0
            Else
                'Check and warn
                If MANGLOVERight_bas.FN_CalcLength(g_RightThumbTopCurve.xyR1, xyRightLittle) - g_RightThumbTopCurve.nR1 < 0.375 Then
                    Response = MsgBox("The insert used creates a Top thumb curve on the palm that is closer than 3/8ths to the little finger web!" & Chr(13) & "Use YES to continue or NO to return to the dialogue.", 52, g_sDialogID)
                End If
                If xyThumbOriginal.y <> xyRightThumb.y Then
                    nLength = System.Math.Abs(xyThumbOriginal.y - xyRightThumb.y)
                    Response = MsgBox("Warnimg the thumb web position has been lowered by" & Str(nLength) & " inches, so that the thumb curve is 3/8ths away from closest web!" & Chr(13) & "Use YES to continue or NO to return to the dialogue.", 52, g_sDialogID)
                    'Save the drop for later use with Web spacers
                    g_iRightThumbWebDrop = Int(nLength / 0.0625) 'this will give the value in 1/16 ths
                End If
                Select Case Response
                    Case IDYES
                        FN_CalcGlovePoints = True
                    Case IDNO, IDCANCEL
                        FN_CalcGlovePoints = False
                        Exit Function
                End Select
            End If
        Else
            'Edged thumb
            nThumbSlantLength = g_RightFinger(5).PIP / 2
            PR_CalcRightFingerBlendingProfile(xyRightIndex, xyRightThumb, xyRightMiddle, g_RightFinger(4), g_FingerThumbBlend, xyRightPalmThumb(1))
        End If

        xyRightPalmThumb(4).X = xyRightThumb.X
        xyRightPalmThumb(4).y = xyRightThumb.y - nThumbSlantLength

        'Palm points (Points dependant on thumb y)
        'LFS at Thumb web height
        If g_RightOnFold Then
            xyRightPalm(3).X = xyRightLFS.X
        Else
            xyRightPalm(3).X = xyRightPalm(2).X
        End If

        xyRightPalm(3).y = xyRightThumb.y
        xyRightPalm(4) = xyRightThumb
    End Function

    Private Function FN_CalculateInsert(ByRef iInsert As Short, ByRef sType As String) As Object
        'This procedure uses the data for the finger RIGHT_PIP and the
        'Palm circumference to calculate the insert sizes
        'based on the procedure
        'GOP 01/02/26, Effective date 2nd June 1992
        '
        '
        'Notes:-
        '
        Dim ii, nn, Response As Short
        Dim nAge As Object
        Dim nFiguredPalm, nFiguredPIPs, nPermittedDiff As Double
        Dim sText As String
        nAge = Val(txtAge.Text)

        'Dim ifile%
        'Initially true
        'UPGRADE_WARNING: Couldn't resolve default property of object FN_CalculateInsert. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FN_CalculateInsert = True

        'CALCULATE an insert
        'setup the parameters based on radio buttons
        'ifile = FreeFile
        'Open "C:\TMP\ERR.DAT" For Output As ifile
        'If g_RightFusedFingers = True Then Print #ifile, "Fused Fingers"

        'Loop through the inserts starting at
        '1/2" (ii=1) to 1" (ii=5) then try as a last resort 3/8" (ii=6)
        'If we find an insert that satisfies our test criteria then break the
        'for next loop
        'NB addition wrt Fused Fingers
        '
        For ii = 1 To 4
            g_nRightFINGER_FIGURE(ii) = 0
        Next ii

        Dim iStart, iEnd As Short
        If sType = "CALC" Then
            iStart = 1
            iEnd = 6
        Else
            iStart = iInsert
            iEnd = iInsert
        End If

        For iInsert = iStart To iEnd
            Select Case g_iRightInsertStyle
                Case 0
                    'Normal
                    If g_RightOnFold Then
                        'Use thumb for little finger
                        g_iRightFINGER_CHART(1) = RIGHT_THUMB
                        g_nRightFINGER_FIGURE(1) = -(2 * RIGHT_SIXTEENTH)
                        For ii = 2 To 4
                            g_iRightFINGER_CHART(ii) = RIGHT_PIP
                        Next ii
                        g_nRightPALM_FIGURE = RIGHT_EIGHTH - (g_iRightInsertValue(iInsert) * RIGHT_EIGHTH)
                    Else
                        For ii = 1 To 4
                            g_iRightFINGER_CHART(ii) = RIGHT_PIP
                        Next ii
                        g_nRightPALM_FIGURE = (3 * RIGHT_EIGHTH) - ((g_iRightInsertValue(iInsert) * RIGHT_EIGHTH) * 2)
                    End If

                Case 1
                    g_iRightFINGER_CHART(1) = RIGHT_THUMB
                    For ii = 2 To 4
                        g_iRightFINGER_CHART(ii) = RIGHT_PIP
                    Next ii
                    g_nRightPALM_FIGURE = (3 * RIGHT_EIGHTH) - (g_iRightInsertValue(iInsert) * RIGHT_EIGHTH)
                Case 2
                    g_iRightFINGER_CHART(1) = RIGHT_THUMB
                    g_iRightFINGER_CHART(2) = RIGHT_PIP
                    g_iRightFINGER_CHART(3) = RIGHT_PIP
                    g_iRightFINGER_CHART(4) = RIGHT_THUMB
                    g_nRightPALM_FIGURE = (2 * RIGHT_EIGHTH)
                Case 3
                    For ii = 1 To 4
                        g_iRightFINGER_CHART(ii) = RIGHT_THUMB
                    Next ii
                    g_nRightPALM_FIGURE = (2 * RIGHT_EIGHTH)
            End Select

            'set permitted differencses
            If g_RightOnFold Then
                nPermittedDiff = RIGHT_EIGHTH
            Else
                nPermittedDiff = 2 * RIGHT_EIGHTH
            End If

            'Common figuring used latter
            g_iRightFINGER_CHART(5) = RIGHT_THUMB
            g_nRightWRIST_FIGURE = g_nRightPALM_FIGURE

            'Figure fingers
            nFiguredPIPs = 0
            For ii = 1 To 4
                nFiguredPIPs = nFiguredPIPs + FN_RightFigureFinger(g_iRightFINGER_CHART(ii), g_nRightFingerPIPCir(ii), iInsert) + g_nRightFINGER_FIGURE(ii)
            Next ii

            'Figure palm
            nFiguredPalm = (FN_RightFigureTape(RIGHT_PALM, g_nRightPalm) + g_nRightPALM_FIGURE) / 2
            'Dim nValue#
            'Print #ifile, "iInsert= "; iInsert
            'Print #ifile, "nFiguredPalm = "; nFiguredPalm
            'Print #ifile, "nFiguredPIPs = "; nFiguredPIPs
            'For ii = 1 To 4
            'nValue = FN_RightFigureFinger(g_iRightFINGER_CHART(ii), g_nRightFingerPIPCir(ii), iInsert) + g_nRightFINGER_FIGURE(ii)
            'Print #ifile, "RightFinger"; ii
            'Print #ifile, "Figured"; nValue; "Cir="; g_nRightFingerPIPCir(ii); "Figured= "; FN_RightFigureFinger(g_iRightFINGER_CHART(ii), g_nRightFingerPIPCir(ii), iInsert); "Figure all="; g_nRightFINGER_FIGURE(ii)
            'Next ii
            'Exit "FOR" if conditions met
            If System.Math.Abs(nFiguredPalm - nFiguredPIPs) <= nPermittedDiff Or (g_RightMissingFingers <> 0 And Not g_RightFusedFingers) Then Exit For
            If sType <> "CALC" Then Exit For 'Exit if we have been given an Insert size

        Next iInsert

        'Close #ifile
        'Check to see if all of the above have been tried
        'issue a warning message and exit this sub
        If iInsert >= 7 Then
            If g_RightOnFold Then
                sText = "Unable to calculate an insert. Try OFF the fold"
            Else
                sText = "Unable to calculate an insert. Try again using one of the other options."
            End If
            MsgBox(sText, 16, "Glove - Insert Calculation")
            FN_CalculateInsert = False
            Exit Function
        End If

        'Check and warn with respect to age and insert size
        If ((iInsert = 4 Or iInsert = 5) And nAge < 6) Then
            sText = "It is not recommended that an insert of 7/8"" or greater is given for children under 6 years old. Try using a ""one insert glove - on the fold""." & Chr(13) & "Do you wish to continue?"
            Response = MsgBox(sText, 36, "Glove - Insert Calculation")
            Select Case Response
                Case IDYES
                    FN_CalculateInsert = True
                Case IDNO, IDCANCEL
                    FN_CalculateInsert = False
            End Select
        End If

        If (sType <> "CALC" And System.Math.Abs(nFiguredPalm - nFiguredPIPs) > nPermittedDiff) And g_RightMissingFingers = 0 And Not g_RightFusedFingers Then
            sText = "Incorrect figuring with the insert specified." & Chr(13) & "Do you wish to continue?"
            Response = MsgBox(sText, 36, "Glove - Insert Calculation")
            Select Case Response
                Case IDYES
                    FN_CalculateInsert = True
                Case IDNO, IDCANCEL
                    FN_CalculateInsert = False
            End Select
        End If
    End Function

    Private Function FN_CompareWithOtherGlove(ByRef sFabric As String, ByRef Onfold As Short, ByRef iInsertSize As Short) As Short
        'Compares the current glove with the other glove
        '
        'Globals
        '     g_iRightInsertOtherGlv
        '     g_RightOnFoldOtherGlv
        '     txtFabric.Text
        '     g_sSide
        '
        Static sThisInsert, sText, sOtherInsert As String
        Static Response As Short

        FN_CompareWithOtherGlove = True
        sText = ""

        If g_iRightInsertOtherGlv = -1 Then Exit Function 'Other glove does not exist

        If sFabric <> txtFabric.Text Then
            sText = MANGLOVERight_bas.g_sSide & " glove fabric is " & sFabric & "." & Chr(13)
            sText = sText & "Other glove fabric is " & txtFabric.Text & "." & Chr(13)
        End If

        If g_iRightInsertOtherGlv <> iInsertSize Then
            sText = sText & "Insert sizes are different." & Chr(13)
            sThisInsert = Trim(ARMEDDIA1.fnInchesToText(g_iRightInsertValue(iInsertSize) * RIGHT_EIGHTH))
            If Mid(sThisInsert, 1, 1) = "-" Then sThisInsert = Mid(sThisInsert, 2)
            sOtherInsert = Trim(ARMEDDIA1.fnInchesToText(g_iRightInsertValue(g_iRightInsertOtherGlv) * RIGHT_EIGHTH))
            If Mid(sOtherInsert, 1, 1) = "-" Then sOtherInsert = Mid(sOtherInsert, 2)
            sText = sText & "This glove uses a " & sThisInsert & " insert, The other glove uses a " & sOtherInsert & " insert." & Chr(13)
        End If

        If Onfold <> g_RightOnFoldOtherGlv Then
            If MANGLOVERight_bas.g_sSide = "Left" And g_RightOnFoldOtherGlv Then sText = sText & "Right glove is Drawn on the fold" & Chr(13)
            If MANGLOVERight_bas.g_sSide = "Left" And Not g_RightOnFoldOtherGlv Then sText = sText & "Right glove is Drawn off the fold" & Chr(13)
            If MANGLOVERight_bas.g_sSide = "Right" And g_RightOnFoldOtherGlv Then sText = sText & "Left glove is Drawn on the fold" & Chr(13)
            If MANGLOVERight_bas.g_sSide = "Right" And Not g_RightOnFoldOtherGlv Then sText = sText & "Left glove is Drawn off the fold" & Chr(13)
        End If

        'Check if error has occured
        If sText <> "" Then
            If MANGLOVERight_bas.g_sSide = "Left" Then
                sText = "This glove differs from the RIGHT glove!" & Chr(13) & sText & Chr(13) & "Do you wish to continue and draw the glove anyway?"
            Else
                sText = "This glove differs from the LEFT glove!" & Chr(13) & sText & Chr(13) & "Do you wish to continue and draw the glove anyway?"
            End If
            Response = MsgBox(sText, 36, g_sDialogID)
            Select Case Response
                Case IDYES
                    FN_CompareWithOtherGlove = True
                Case IDNO, IDCANCEL
                    FN_CompareWithOtherGlove = False
            End Select
        End If
    End Function

    Private Function FN_GloveDataChanged() As String
        'Function that will be used to check if data has changed
        'The first Time that it is called the function will always
        'return False
        '
        'NOTE:-
        '    The change checked for is always the state of the data the
        '    first time the function is called
        '
        '    It is intendee to be run After Link Close to save the state
        '    at that point
        '    If the user uses Close then we can check if any changes have been
        '    made


        Static sCheck, sTmp As String
        Static ii As Short

        'Get data from the controls
        sTmp = ""
        '    MANGLOVERight_bas.MainForm!SSTab1.Tab = 0

        For ii = 0 To 12
            sTmp = sTmp & txtCir(ii).Text
        Next ii

        For ii = 0 To 8
            sTmp = sTmp & txtLen(ii).Text
        Next ii

        '    MANGLOVERight_bas.MainForm!SSTab1.Tab = 1

        For ii = 8 To 23
            sTmp = sTmp & txtExtCir(ii).Text & mms(ii).Text
        Next ii

        sTmp = sTmp & txtWristPleat1.Text & txtWristPleat2.Text & txtShoulderPleat1.Text & txtShoulderPleat2.Text

        sTmp = sTmp & cboFlaps.Text & txtStrapLength.Text & txtFrontStrapLength.Text & txtCustFlapLength.Text & txtWaistCir.Text

        sTmp = sTmp & Str(optExtendTo(0).Checked) & Str(optExtendTo(1).Checked) & Str(optExtendTo(2).Checked)

        sTmp = sTmp & Str(optThumbStyle(0).Checked) & Str(optThumbStyle(1).Checked) & Str(optThumbStyle(2).Checked)

        sTmp = sTmp & Str(optProximalTape(0).Checked) & Str(optProximalTape(1).Checked)

        sTmp = sTmp & Str(optFold(0).Checked) & Str(optFold(1).Checked)

        sTmp = sTmp & cboFabric.Text & cboDistalTape.Text & cboProximalTape.Text & cboPressure.Text & cboInsert.Text

        sTmp = sTmp & Str(chkSlantedInserts.CheckState) & Str(chkPalm.CheckState) & Str(chkDorsal.CheckState)

        'Check for changes
        'If sCheck = "" Then
        '    FN_GloveDataChanged = False
        '    sCheck = sTmp
        'Else
        '    If sTmp <> sCheck Then FN_GloveDataChanged = True
        'End If
        FN_GloveDataChanged = sTmp
    End Function

    Private Function FN_Open(ByRef sDrafixFile As String, ByRef sName As Object, ByRef sPatientFile As Object, ByRef sLeftorRight As Object) As Short
        'Open the DRAFIX macro file
        'Initialise Global variables

        'Open file
        MANGLOVERight_bas.fNum = FreeFile()
        FileOpen(MANGLOVERight_bas.fNum, sDrafixFile, VB.OpenMode.Output)
        FN_Open = MANGLOVERight_bas.fNum

        'Initialise String globals
        CC = Chr(44) 'The comma (,)
        NL = Chr(10) 'The new line character
        QQ = Chr(34) 'Double quotes (")
        QCQ = QQ & CC & QQ
        QC = QQ & CC
        CQ = CC & QQ

        'Initialise patient globals
        g_sFileNo = sPatientFile
        MANGLOVERight_bas.g_sSide = sLeftorRight
        MANGLOVERight_bas.g_sPatient = sName

        'Globals to reduced drafix code written to file
        MANGLOVERight_bas.g_sCurrentLayer = ""
        MANGLOVERight_bas.g_nCurrTextHt = 0.125
        MANGLOVERight_bas.g_nCurrTextAspect = 0.6
        MANGLOVERight_bas.g_nCurrTextHorizJust = 1 'Left
        MANGLOVERight_bas.g_nCurrTextVertJust = 32 'Bottom
        MANGLOVERight_bas.g_nCurrTextFont = 0 'BLOCK
        MANGLOVERight_bas.g_nCurrTextAngle = 0

        'Write header information etc. to the DRAFIX macro file
        '
        PrintLine(MANGLOVERight_bas.fNum, "//DRAFIX Macro created - " & DateString & "  " & TimeString)
        PrintLine(MANGLOVERight_bas.fNum, "//Patient - " & MANGLOVERight_bas.g_sPatient & ", " & g_sFileNo & ", Hand - " & MANGLOVERight_bas.g_sSide)
        PrintLine(MANGLOVERight_bas.fNum, "//by Visual Basic, GLOVES - Drawing")

        'Define DRAFIX variables
        PrintLine(MANGLOVERight_bas.fNum, "HANDLE hLayer, hChan, hEnt, hSym, hOrigin, hMPD;")
        PrintLine(MANGLOVERight_bas.fNum, "XY     xyStart, xyOrigin, xyScale, xyO;")
        PrintLine(MANGLOVERight_bas.fNum, "STRING sFileNo, sSide, sID, sName;")
        PrintLine(MANGLOVERight_bas.fNum, "ANGLE  aAngle;")

        'Text data
        PrintLine(MANGLOVERight_bas.fNum, "SetData(" & QQ & "TextHorzJust" & QC & MANGLOVERight_bas.g_nCurrTextHorizJust & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetData(" & QQ & "TextVertJust" & QC & MANGLOVERight_bas.g_nCurrTextVertJust & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetData(" & QQ & "TextHeight" & QC & MANGLOVERight_bas.g_nCurrTextHt & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetData(" & QQ & "TextAspect" & QC & MANGLOVERight_bas.g_nCurrTextAspect & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetData(" & QQ & "TextFont" & QC & MANGLOVERight_bas.g_nCurrTextFont & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "Data" & QCQ & "string" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "ZipperLength" & QCQ & "length" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "Zipper" & QCQ & "string" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "TapeLengths" & QCQ & "string" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "TapeLengths2" & QCQ & "string" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "TapeLengthPt1" & QCQ & "string" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "curvetype" & QCQ & "string" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "Leg" & QCQ & "string" & QQ & ");")

        'Clear user selections etc
        PrintLine(MANGLOVERight_bas.fNum, "UserSelection (" & QQ & "clear" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Execute (" & QQ & "menu" & QCQ & "SetStyle" & QC & "Table(" & QQ & "find" & QCQ & "style" & QCQ & "bylayer" & QQ & "));")
        PrintLine(MANGLOVERight_bas.fNum, "Execute (" & QQ & "menu" & QCQ & "SetColor" & QC & "Table(" & QQ & "find" & QCQ & "color" & QCQ & "bylayer" & QQ & "));")

        'Set values for use futher on by other macros
        PrintLine(MANGLOVERight_bas.fNum, "sSide = " & QQ & MANGLOVERight_bas.g_sSide & QQ & ";")
        PrintLine(MANGLOVERight_bas.fNum, "sFileNo = " & QQ & g_sFileNo & QQ & ";")

        'Get Start point
        PrintLine(MANGLOVERight_bas.fNum, "GetUser (" & QQ & "xy" & QCQ & "Indicate Start Point" & QC & "&xyStart);")

        'Place a marker at the start point for later use.
        'Get a UID and create the unique 4 character start to the ID code
        'Note this is a bit dogey if the drawing contains more than 9999 entities
        ARMDIA1.PR_SetLayer("Construct")
        PrintLine(MANGLOVERight_bas.fNum, "hOrigin = AddEntity(" & QQ & "marker" & QCQ & "xmarker" & QC & "xyStart" & CC & "0.125);")
        PrintLine(MANGLOVERight_bas.fNum, "if (hOrigin) {")
        PrintLine(MANGLOVERight_bas.fNum, "  sID=StringMiddle(MakeString(" & QQ & "long" & QQ & ",UID(" & QQ & "get" & QQ & ",hOrigin)), 1, 4) ; ")
        PrintLine(MANGLOVERight_bas.fNum, "  while (StringLength(sID) < 4) sID = sID + " & QQ & " " & QQ & ";")
        PrintLine(MANGLOVERight_bas.fNum, "  sID = sID + sFileNo + sSide ;")
        PrintLine(MANGLOVERight_bas.fNum, "  SetDBData(hOrigin," & QQ & "ID" & QQ & ",sID);")
        PrintLine(MANGLOVERight_bas.fNum, "  }")

        'Display Hour Glass Symbol
        PrintLine(MANGLOVERight_bas.fNum, "Display (" & QQ & "cursor" & QCQ & "wait" & QCQ & "Drawing" & QQ & ");")

        'Set values for use futher on by other macros
        PrintLine(MANGLOVERight_bas.fNum, "xyOrigin = xyStart" & ";")

        PrintLine(MANGLOVERight_bas.fNum, "hMPD = UID (" & QQ & "find" & QC & Val(txtUidMPD.Text) & ");")
        PrintLine(MANGLOVERight_bas.fNum, "if (hMPD)")
        PrintLine(MANGLOVERight_bas.fNum, "  GetGeometry(hMPD, &sName, &xyO, &xyScale, &aAngle);")
        PrintLine(MANGLOVERight_bas.fNum, "else")
        PrintLine(MANGLOVERight_bas.fNum, "  Exit(%cancel," & QQ & "Can't find > mainpatientdetails < symbol, Insert Patient Data" & QQ & ");")

        'Start drawing on correct side
        ARMDIA1.PR_SetLayer("Template" & MANGLOVERight_bas.g_sSide)
    End Function
    Private Function FN_TranslateInsert(ByRef iListIndex As Short, ByRef iInsert As Short) As Short
        'Function to translate the cboInsert value to one that can be
        'used in a manual glove
        'It will also attempt reconcile the two
        '
        '   cboInsert.AddItem "Calculate"   0
        '   cboInsert.AddItem "1/8"         1
        '   cboInsert.AddItem "1/4"         2
        '   cboInsert.AddItem "3/8"         3      Valid Manual
        '   cboInsert.AddItem "1/2"         4      Valid Manual
        '   cboInsert.AddItem "5/8"         5      Valid Manual
        '   cboInsert.AddItem "3/4"         6      Valid Manual
        '   cboInsert.AddItem "7/8"         7      Valid Manual
        '   cboInsert.AddItem "1"           8      Valid Manual
        '   cboInsert.AddItem "1-1/8"       9
        '   cboInsert.AddItem "1-1/4"       10
        '   cboInsert.AddItem "1-3/8"       11
        '   cboInsert.AddItem "1-1/2"       12
        '   cboInsert.AddItem "1-5/8"       13
        '   cboInsert.AddItem "1-3/4"       14
        '   cboInsert.AddItem "1-7/8"       15
        '
        FN_TranslateInsert = True
        Select Case iListIndex
            Case 0 To 2, 9 To 15
                MsgBox("Not valid insert size for MANUAL-Gloves. Use a value in the range 3/8th to 1 inch.  Or use Calculate to force a calculation ", 16, g_sDialogID)
                FN_TranslateInsert = False
                iInsert = 0
            Case 4 To 8
                iInsert = iListIndex - 3
            Case 3
                iInsert = 6
        End Select
    End Function

    Private Function FN_ValidateData(ByRef sOption As String) As Short
        'This function is used only to make gross checks
        'for missing data.
        'It does not perform any sensibility checks on the
        'data
        'The sOption allows for a less rigourous check
        '    "Check All"
        '    "Ignore Webs"
        '
        Dim NL, sError, sWebError As String
        Dim ii, nn As Short
        Dim nThumbWebHt As Double

        'Initialise
        FN_ValidateData = False
        sError = ""
        NL = Chr(13)

        Dim sCircum(12) As Object
        sCircum(0) = "Little RightFinger"
        sCircum(1) = "Little RightFinger"
        sCircum(2) = "Ring RightFinger"
        sCircum(3) = "Ring RightFinger"
        sCircum(4) = "Middle RightFinger"
        sCircum(5) = "Middle RightFinger"
        sCircum(6) = "Index RightFinger"
        sCircum(7) = "Index RightFinger"
        sCircum(8) = "Thumb"
        sCircum(9) = "Palm"
        sCircum(10) = "Wrist"
        sCircum(11) = "Tape 1½"
        sCircum(12) = "Tape 3"

        Dim nTotalCir(4) As Object
        Dim sLengths(8) As Object
        sLengths(0) = "Little RightFinger tip  to web"
        sLengths(1) = "Ring RightFinger tip to web"
        sLengths(2) = "Middle RightFinger tip to web"
        sLengths(3) = "Index RightFinger tip to web"
        sLengths(4) = "Thumb to tip web"
        sLengths(5) = "Wrist to web at Little RightFinger"
        sLengths(6) = "Wrist to web at Middle RightFinger"
        sLengths(7) = "Wrist to web at Index RightFinger"
        sLengths(8) = "Wrist to web at Thumb"

        'Check circumferences
        nn = 0
        For ii = 0 To 7 Step 2
            If Val(txtCir(ii).Text) <> 0 And Val(txtCir(ii + 1).Text) = 0 Then
                sError = sError & "Missing RIGHT_PIP for " & sCircum(ii) & "!" & NL
            End If
            nTotalCir(nn) = Val(txtCir(ii).Text) + Val(txtCir(ii + 1).Text)
            nn = nn + 1
        Next ii

        'Settings for thumb (as not set above)
        nTotalCir(4) = Val(txtCir(8).Text)

        For ii = 8 To 10
            If Val(txtCir(ii).Text) = 0 Then
                sError = sError & "Missing circumference for " & sCircum(ii) & "!" & NL
            End If
        Next ii

        'Check on lengths
        For ii = 0 To 3
            If Val(txtLen(ii).Text) = 0 And nTotalCir(ii) <> 0 Then
                sError = sError & "Missing length " & sLengths(ii) & "!" & NL
            End If
            If Val(txtLen(ii).Text) <> 0 And nTotalCir(ii) = 0 Then
                sError = sError & "Length given but missing circumference " & sCircum(ii * 2) & "!" & NL
            End If
        Next ii

        'Length not required for edged thumbs
        If optThumbStyle(1).Checked <> True Then
            If Val(txtLen(4).Text) = 0 And nTotalCir(4) <> 0 Then
                sError = sError & "Missing length " & sLengths(4) & "!" & NL
            End If
            If Val(txtLen(4).Text) <> 0 And nTotalCir(4) = 0 Then
                sError = sError & "Length given but missing circumference " & sCircum(4 * 2) & "!" & NL
            End If
        End If

        If sOption <> "Ignore Webs" Then
            sWebError = ""
            For ii = 5 To 8
                If Val(txtLen(ii).Text) = 0 Then
                    sWebError = sWebError & "Missing length for " & sLengths(ii) & "!" & NL
                End If
            Next ii
            If sWebError <> "" Then
                If MsgBox(sWebError & NL & "If the missing webs represent amputated fingers then, ""OK"" to continue or ""CANCEL"" to return to the dialogue to check measurments", 49, g_sDialogID) <> IDOK Then
                    FN_ValidateData = False
                    Exit Function
                End If
            End If

            'Check that the thumb web is lower that all the other webs
            sWebError = ""
            nThumbWebHt = Val(txtLen(8).Text)
            For ii = 5 To 7
                If (Val(txtLen(ii).Text)) <= nThumbWebHt And nThumbWebHt <> 0 Then
                    sWebError = sWebError & "Thumb web is higher than " & sLengths(ii) & "!" & NL
                End If
            Next ii
            If sWebError <> "" Then
                If MsgBox(sWebError & NL & "Use, ""OK"" to continue or ""CANCEL"" to return to the dialogue to check measurments", 49, g_sDialogID) <> IDOK Then
                    FN_ValidateData = False
                    Exit Function
                End If
            End If
        End If

        If cboFabric.Text = "" Then
            sError = sError & "Fabric not given! " & NL
        Else
            If UCase(Mid(cboFabric.Text, 1, 3)) <> "POW" Then
                sError = sError & "Fabric chosen is not Powernet"
            End If
            If Val(Mid(cboFabric.Text, 5, 3)) < RIGHT_LOW_MODULUS Or Val(Mid(cboFabric.Text, 5, 3)) > RIGHT_HIGH_MODULUS Then
                sError = sError & "Modulus of fabric chosen is not available on Gram / Tension reduction chart"
            End If
        End If

        'Validate extension data
        If sOption = "Check All" Then
            sError = sError & MANGLOVERight_bas.FN_ValidateExtensionData()
        End If

        If sError <> "" Then
            MsgBox(sError, 16, g_sDialogID)
            FN_ValidateData = False
        Else
            FN_ValidateData = True
        End If
    End Function


    'UPGRADE_ISSUE: Form event Form.LinkClose was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ABD9AF39-7E24-4AFF-AD8D-3675C1AA3054"'
    Private Sub Form_LinkClose()

        Dim nAge, ii, nn, jj As Short
        Dim nValue As Double
        Dim sTapesBeyondWrist, sValue As String

        'Disable timeout timer as a link close event has happened
        ''--------Timer1.Enabled = False

        'Check if MainPatientDetails exists
        'If it dosn't then exit
        'If txtUidMPD.Text = "" Then
        '    MsgBox("No Patient details found!", 16, g_sDialogID)
        '    Return
        'End If

        'Scaling factor
        If txtUnits.Text = "cm" Then
            MANGLOVERight_bas.g_nUnitsFac = 10 / 25.4
        Else
            MANGLOVERight_bas.g_nUnitsFac = 1
        End If

        'Hand Option buttons
        If txtSide.Text = "Left" Then
            optHand(0).Checked = True
            optHand(1).Enabled = False
            MANGLOVERight_bas.g_sSide = "Left"
        End If
        If txtSide.Text = "Right" Then
            optHand(1).Checked = True
            optHand(0).Enabled = False
            MANGLOVERight_bas.g_sSide = "Right"
        End If

        'Circumferences
        For ii = 0 To 10
            nValue = Val(Mid(txtTapeLengths.Text, (ii * 3) + 1, 3))
            If nValue > 0 Then
                txtCir(ii).Text = CStr(nValue / 10)
                txtCir_Leave(txtCir.Item(ii), New System.EventArgs())
            End If
        Next ii

        'Lengths
        For ii = 0 To 8
            nn = ii + 11
            nValue = Val(Mid(txtTapeLengths.Text, (nn * 3) + 1, 3))
            If nValue > 0 Then
                txtLen(ii).Text = CStr(nValue / 10)
                txtLen_Leave(txtLen.Item(ii), New System.EventArgs())
            End If
        Next ii

        'Tip Option Buttons
        '     0 => Closed tip
        '     1 => Standard Open Tip
        '     2 => Disired Open Tip
        If txtTapeLengths2.Text <> "" Then
            For ii = 0 To 6
                nValue = Val(Mid(txtTapeLengths2.Text, ii + 1, 1))
                If ii = 0 Then
                    optLittleTip(nValue).Checked = True
                ElseIf ii = 1 Then
                    optRingTip(nValue).Checked = True
                ElseIf ii = 2 Then
                    optMiddleTip(nValue).Checked = True
                ElseIf ii = 3 Then
                    optIndexTip(nValue).Checked = True
                ElseIf ii = 4 Then
                    optThumbTip(nValue).Checked = True
                ElseIf ii = 5 Then
                    chkSlantedInserts.CheckState = nValue
                ElseIf ii = 6 Then
                    chkPalm.CheckState = nValue
                End If
            Next ii
            'Tapes beyond wrist
            'normal glove

            ii = 0
            sTapesBeyondWrist = Mid(txtTapeLengths2.Text, 8)
            For nn = 11 To 12
                nValue = Val(Mid(sTapesBeyondWrist, (ii * 3) + 1, 3))
                If nValue > 0 Then
                    txtCir(nn).Text = CStr(nValue / 10)
                    txtCir_Leave(txtCir.Item(nn), New System.EventArgs())
                End If
                ii = ii + 1
            Next nn
        End If


        'Setup for specific insert size
        cboInsert.Items.Add("Calculate")
        cboInsert.Items.Add("1/8")
        cboInsert.Items.Add("1/4")
        cboInsert.Items.Add("3/8")
        cboInsert.Items.Add("1/2")
        cboInsert.Items.Add("5/8")
        cboInsert.Items.Add("3/4")
        cboInsert.Items.Add("7/8")
        cboInsert.Items.Add("1")
        cboInsert.Items.Add("1-1/8")
        cboInsert.Items.Add("1-1/4")
        cboInsert.Items.Add("1-3/8")
        cboInsert.Items.Add("1-1/2")
        cboInsert.Items.Add("1-5/8")
        cboInsert.Items.Add("1-3/4")
        cboInsert.Items.Add("1-7/8")

        'Stored insert value (This is stored in 8ths)
        Dim sInsert As String
        Dim nInsert As Short

        'This is a flag used to allow for automatic recalculation
        'if the user switches between on and off the fold
        'This stops confusing the user with warning messages about
        'missing data when they have not requested anything to happen
        'but have simply used the Fold option buttons
        g_RightDataIsCalcuable = False

        sInsert = Mid(txtTapeLengths2.Text, 15, 2)
        If sInsert <> "" Then
            nInsert = Val(sInsert)
            If nInsert <= cboInsert.Items.Count Then
                cboInsert.SelectedIndex = nInsert
            Else
                cboInsert.SelectedIndex = 0
            End If
        Else
            cboInsert.SelectedIndex = 0
        End If

        If cboInsert.SelectedIndex > 0 Then
            g_RightDataIsCalcuable = True
            g_iInsertSize = nInsert
        End If

        'Reinforced DORSAL
        nValue = Val(Mid(txtTapeLengths2.Text, 17, 1))
        chkDorsal.CheckState = nValue

        'Thumb Options
        nValue = Val(Mid(txtTapeLengths2.Text, 18, 1))
        optThumbStyle(nValue).Checked = True

        'Inset Options
        nValue = Val(Mid(txtTapeLengths2.Text, 19, 1))
        optInsertStyle(nValue).Checked = True

        'Print Fold Options
        sValue = Mid(txtTapeLengths2.Text, 20, 1)
        chkPrintFold.CheckState = System.Windows.Forms.CheckState.Checked
        If sValue = "0" Then chkPrintFold.CheckState = System.Windows.Forms.CheckState.Unchecked

        'Fused fingers options
        nValue = Val(Mid(txtTapeLengths2.Text, 21, 1))
        chkFusedFingers.CheckState = nValue

        'Fabric
        If txtFabric.Text <> "" Then cboFabric.Text = txtFabric.Text

        'Get DDE data for extension
        'This procedure is in the module GLVEXTEN.BAS
        g_RightCalculatedExtension = False

        PR_GetExtensionDDE_Data()

        'Update the values in the glove extension module
        'This takes the values from the dialogue and a sets it up for subsequent
        'procedures in this module
        PR_GetDlgAboveWrist()

        'Having set all the fields with raw data we NOW!
        'refine the display by executing the relevent procedure
        For ii = 0 To 2
            If optExtendTo(ii).Checked = True Then
                PR_ExtendTo_Click((ii))
                Exit For
            End If
        Next ii

        'Get data for other hand to ensure that the two gloves are
        'similar
        g_iRightInsertOtherGlv = -1
        g_RightOnFoldOtherGlv = False
        If MANGLOVERight_bas.g_sSide = "Right" Then
            If Val(Mid(txtDataGC.Text, 1, 2)) > 0 Then
                g_iRightInsertOtherGlv = Val(Mid(txtDataGC.Text, 1, 2))
                If Val(Mid(txtDataGC.Text, 3, 2)) > 0 Then g_RightOnFoldOtherGlv = True
            End If
        Else
            If Val(Mid(txtDataGC.Text, 5, 2)) <> 0 Then
                g_iRightInsertOtherGlv = Val(Mid(txtDataGC.Text, 5, 2))
                If Val(Mid(txtDataGC.Text, 7, 2)) > 0 Then g_RightOnFoldOtherGlv = True
            End If
        End If


        'Setup to test for changes
        ''ii = FN_GloveDataChanged()
        g_sChangeChecker = FN_GloveDataChanged()


        ''--------------CType(MANGLOVERight_bas.MainForm.Controls("SSTab1"), Object).Tab = 0
        SSTab1.SelectedIndex = 0

        'Show after link is closed
        Show()

    End Sub
    Sub PR_DisableFigureArm()

        Dim ii As Short

        For ii = 3 To 11
            lblArm(ii).Enabled = False
        Next ii

        'Pleats
        g_nRightPleats(1) = Val(txtWristPleat1.Text)
        g_nRightPleats(2) = Val(txtWristPleat2.Text)
        g_nRightPleats(3) = Val(txtShoulderPleat2.Text)
        g_nRightPleats(4) = Val(txtShoulderPleat1.Text)

        For ii = 0 To 3
            lblPleat(ii).Enabled = False
        Next ii


        txtShoulderPleat1.Enabled = False
        txtShoulderPleat2.Enabled = False
        '  MainForm!txtShoulderPleat1 = ""
        '  MainForm!txtShoulderPleat2 = ""
        txtWristPleat1.Enabled = False
        txtWristPleat2.Enabled = False
        '  MainForm!txtWristPleat1 = ""
        '  MainForm!txtWristPleat2 = ""

        frmCalculate.Enabled = False
        cboPressure.Enabled = False
        cmdCalculate.Enabled = False
    End Sub
    Sub PR_DisableGloveToAxilla()
        frmGloveToAxilla.Enabled = False
        optProximalTape(0).Checked = False
        optProximalTape(1).Checked = False
        optProximalTape(0).Enabled = False
        optProximalTape(1).Enabled = False
        PR_ProximalTape_Click((0))
    End Sub
    Sub PR_EnableFigureArm()

        'Converse of PR_DisableFigureArm

        Static ii As Short
        For ii = 3 To 11
            lblArm(ii).Enabled = True
        Next ii

        For ii = 0 To 3
            lblPleat(ii).Enabled = True
        Next ii

        txtShoulderPleat1.Enabled = True
        txtShoulderPleat2.Enabled = True
        ''------------txtWristPleat1.Text = CStr(g_nRightPleats(1))
        ''-------------txtWristPleat2.Text = CStr(g_nRightPleats(2))
        If g_nRightPleats(1) > 0 Then txtWristPleat1.Text = CStr(g_nRightPleats(1))
        If g_nRightPleats(2) > 0 Then txtWristPleat2.Text = CStr(g_nRightPleats(2))
        txtWristPleat1.Enabled = True
        txtWristPleat2.Enabled = True
        If g_nRightPleats(3) > 0 Then txtShoulderPleat2.Text = CStr(g_nRightPleats(3))
        If g_nRightPleats(4) > 0 Then txtShoulderPleat1.Text = CStr(g_nRightPleats(4))

        frmCalculate.Enabled = True
        cboPressure.Enabled = True
        cboPressure.SelectedIndex = g_iRightPressure
        cmdCalculate.Enabled = True

    End Sub
    Sub PR_ExtendTo_Click(ByRef INDEX As Short)

        Static iStart, iFirst, ii, iLast, iElbow As Short
        iLast = 23
        iFirst = 8
        iElbow = 16 'Elbow w.r.t. txtExtCir()
        iStart = 8

        Select Case INDEX
            Case 0 'Normal Glove
                'Disable tapes above those required
                For ii = iStart To iLast
                    txtExtCir(ii).Enabled = False
                    lblTape(ii).Enabled = False
                    ''---------------PR_GrdInchesDisplay(ii - 8, 0)
                    lblExtCir(ii - 8).Text = ""
                Next ii

                'Disable all MMs fields etc
                For ii = iFirst To iLast
                    mms(ii).Enabled = False
                    mms(ii).Text = ""
                    ''--------------------- PR_GramRedDisplay(ii - 8, 0, 0)
                    lblGms(ii - 8).Text = ""
                    lblRed(ii - 8).Text = ""
                Next ii

                PR_DisableFigureArm()
                PR_DisableGloveToAxilla()

                'set disable and disable
                cboDistalTape.SelectedIndex = -1
                cboDistalTape.Enabled = False
                cboProximalTape.SelectedIndex = -1
                cboProximalTape.Enabled = False

            Case 1 'Glove to Elbow
                'Enable MMs fields etc
                For ii = iFirst To iLast
                    If ii <= iElbow Then
                        txtExtCir(ii).Enabled = True
                        mms(ii).Enabled = True
                        lblTape(ii).Enabled = True
                    Else
                        txtExtCir(ii).Enabled = False
                        lblTape(ii).Enabled = False
                        mms(ii).Enabled = True
                        mms(ii).Text = ""
                        ''---------------------------PR_GramRedDisplay(ii - 8, 0, 0)
                        ''-----------------------PR_GrdInchesDisplay(ii - 8, 0)
                        lblGms(ii - 8).Text = ""
                        lblRed(ii - 8).Text = ""
                        lblExtCir(ii - 8).Text = ""
                    End If
                Next ii

                'Enable other bits
                PR_EnableFigureArm()
                PR_DisableGloveToAxilla()

                'Set Wrist and EOS pointers
                If g_iRightNumTapesWristToEOS > 0 Then
                    'Set wrist to given tape
                    If g_iRightWristPointer > MANGLOVERight_bas.g_iMGlvRightFirstTape Then
                        cboDistalTape.SelectedIndex = g_iRightWristPointer
                    Else
                        'set to first tape
                        cboDistalTape.SelectedIndex = 0
                    End If

                    'Set EOS to given tape or to Elbow if it extends
                    'past the elbow
                    'NB. The order of tapes is reversed in this list
                    'starting at 19-1/2 and finishing at 0
                    If g_iRightEOSPointer < RIGHT_ELBOW_TAPE Then
                        If g_iRightEOSPointer >= g_iLastTape Then
                            cboProximalTape.SelectedIndex = 0
                        Else
                            cboProximalTape.SelectedIndex = 17 - g_iRightEOSPointer
                        End If
                    ElseIf g_iRightEOSPointer > RIGHT_ELBOW_TAPE Or g_iLastTape > RIGHT_ELBOW_TAPE Then
                        cboProximalTape.SelectedIndex = 17 - RIGHT_ELBOW_TAPE
                    Else
                        'set to last tape
                        cboProximalTape.SelectedIndex = 0
                    End If
                Else
                    cboDistalTape.SelectedIndex = 0
                    cboProximalTape.SelectedIndex = 0
                End If

                cboDistalTape.Enabled = True
                cboProximalTape.Enabled = True
            Case 2 'Glove to Axilla
                For ii = iFirst To iLast
                    mms(ii).Enabled = True
                    txtExtCir(ii).Enabled = True
                    lblTape(ii).Enabled = True
                    '                MainForm!mms(ii) = ""
                    '                PR_GramRedDisplay ii - 8, 0, 0
                Next ii

                'Enable other bits
                PR_EnableFigureArm()

                frmGloveToAxilla.Enabled = True
                If g_RightEOSType = RIGHT_ARM_FLAP Then
                    optProximalTape(1).Checked = True
                Else
                    optProximalTape(0).Checked = True
                End If

                optProximalTape(0).Enabled = True
                optProximalTape(1).Enabled = True

                If g_iRightNumTapesWristToEOS > 0 Then
                    'Set wrist to given tape
                    If g_iRightWristPointer > MANGLOVERight_bas.g_iMGlvRightFirstTape Then
                        cboDistalTape.SelectedIndex = g_iRightWristPointer
                    Else
                        'set to first tape
                        cboDistalTape.SelectedIndex = 0
                    End If

                    'Set EOS to given tape
                    'NB. The order of tapes is reversed in this list
                    'starting at 19-1/2 and finishing at 0
                    If g_iRightEOSPointer < g_iLastTape Then
                        cboProximalTape.SelectedIndex = 17 - g_iRightEOSPointer
                    Else
                        'set to first tape
                        cboProximalTape.SelectedIndex = 0
                    End If
                Else
                    For ii = iFirst To iLast
                        mms(ii).Text = ""
                        ''--------------------PR_GramRedDisplay(ii - 8, 0, 0)
                        lblGms(ii - 8).Text = ""
                        lblRed(ii - 8).Text = ""
                    Next ii
                    cboDistalTape.SelectedIndex = 0
                    cboProximalTape.SelectedIndex = 0
                End If

                cboDistalTape.Enabled = True
                cboProximalTape.Enabled = True
        End Select
    End Sub

    'UPGRADE_ISSUE: Form event Form.LinkExecute was not upgraded. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ABD9AF39-7E24-4AFF-AD8D-3675C1AA3054"'
    Private Sub Form_LinkExecute(ByRef CmdStr As String, ByRef Cancel As Short)
        If CmdStr = "Cancel" Then
            Cancel = 0
            Return
        End If
    End Sub

    Private Sub manglove_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        Try
            Dim ii As Short
            'Hide during DDE transfer
            Hide()

            'Enable Timeout timer
            'Use 6 seconds (a 1/10th of a minute).
            'Timeout is disabled in Link close
            ''------------Timer1.Interval = 6000
            ''------------Timer1.Enabled = True

            MANGLOVERight_bas.MainForm = Me

            'Position to center of screen
            Left = VB6.TwipsToPixelsX((VB6.PixelsToTwipsX(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width) - VB6.PixelsToTwipsX(MANGLOVERight_bas.MainForm.Width)) / 2)
            Top = VB6.TwipsToPixelsY((VB6.PixelsToTwipsY(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height) - VB6.PixelsToTwipsY(MANGLOVERight_bas.MainForm.Height)) / 2)

            'Get the path to the JOBST system installation directory
            MANGLOVERight_bas.g_sPathJOBST = fnPathJOBST()

            'Set up fabric list
            ''------------------LGLEGDIA1.PR_GetComboListFromFile(cboFabric, MANGLOVERight_bas.g_sPathJOBST & "\FABRIC.DAT")
            Dim sSettingsPath As String = fnGetSettingsPath("LookupTables")
            LGLEGDIA1.PR_GetComboListFromFile(cboFabric, sSettingsPath & "\FABRIC.DAT")

            'Clear DDE transfer boxes
            cboFabric.Text = ""
            txtUidGlove.Text = ""
            txtTapeLengths.Text = ""
            txtTapeLengths2.Text = ""
            txtTapeLengthPt1.Text = ""
            txtSide.Text = "Right"
            txtWorkOrder.Text = ""
            txtUidGC.Text = ""
            txtFabric.Text = ""
            txtUidMPD.Text = ""
            txtGrams.Text = ""
            txtReduction.Text = ""
            txtDataGlove.Text = ""
            txtDataGC.Text = ""
            txtFlap.Text = ""
            txtTapeMMs.Text = ""
            txtWristPleat.Text = ""
            txtShoulderPleat.Text = ""

            g_iRightEOSPointer = -1
            g_iRightNumTotalTapes = 0
            g_iRightNumTapesWristToEOS = 0
            g_iMGlvRightFirstTape = -1
            g_iLastTape = -1
            g_iRightWristPointer = -1
            g_iRightPressure = 1
            MANGLOVERight_bas.g_idRgtLastZipper = New ObjectId

            ''Added for #213 in the issue list
            For ii = 1 To RIGHT_NOFF_ARMTAPES
                RightTapeNote(ii).nCir = 0
                RightTapeNote(ii).iMMs = 0
                RightTapeNote(ii).iRed = 0
                RightTapeNote(ii).iGms = 0
                RightTapeNote(ii).sNote = ""
                RightTapeNote(ii).iTapePos = 0
                RightTapeNote(ii).sTapeText = ""
            Next ii

            'cboInsert.Items.Add("Calculate")
            'cboInsert.Items.Add("1/8")
            'cboInsert.Items.Add("1/4")
            'cboInsert.Items.Add("3/8")
            'cboInsert.Items.Add("1/2")
            'cboInsert.Items.Add("5/8")
            'cboInsert.Items.Add("3/4")
            'cboInsert.Items.Add("7/8")
            'cboInsert.Items.Add("1")
            'cboInsert.Items.Add("1-1/8")
            'cboInsert.Items.Add("1-1/4")
            'cboInsert.Items.Add("1-3/8")
            'cboInsert.Items.Add("1-1/2")
            'cboInsert.Items.Add("1-5/8")
            'cboInsert.Items.Add("1-3/4")
            'cboInsert.Items.Add("1-7/8")

            'Default units
            'g_nUnitsFac = 1             'Metric
            MANGLOVERight_bas.g_nUnitsFac = 10 / 25.4 'Inches

            'Set Default Tips to Closed
            optLittleTip(0).Checked = 1
            optRingTip(0).Checked = 1
            optMiddleTip(0).Checked = 1
            optIndexTip(0).Checked = 1
            optThumbTip(0).Checked = 1


            'Setup display inches grid
            'grdInches.set_ColWidth(0, 615)
            'grdInches.set_ColAlignment(0, 2)
            'For ii = 0 To 15
            '    '        grdInches.RowHeight(ii) = 266
            '    grdInches.set_RowHeight(ii, 270)
            'Next ii

            ''Setup display of results grid
            'For ii = 0 To 1
            '    grdDisplay.set_ColWidth(ii, 488)
            '    grdDisplay.set_ColAlignment(ii, 2)
            'Next ii

            'For ii = 0 To 15
            '    '        grdDisplay.RowHeight(ii) = 266
            '    grdDisplay.set_RowHeight(ii, 270)
            'Next ii

            'ListBoxes
            'g_sRightTapeText, From GLVEXTEN.BAS

            cboDistalTape.Items.Add("1st")
            For ii = 2 To 17
                cboDistalTape.Items.Add(LTrim(Mid(g_sRightTapeText, (ii * 3) + 1, 3)))
            Next ii
            cboDistalTape.SelectedIndex = 0

            cboProximalTape.Items.Add("Last")
            For ii = 17 To 2 Step -1
                cboProximalTape.Items.Add(LTrim(Mid(g_sRightTapeText, (ii * 3) + 1, 3)))
            Next ii
            cboProximalTape.SelectedIndex = 0

            cboFlaps.Items.Add("Style A ")
            cboFlaps.Items.Add("Style B ")
            cboFlaps.Items.Add("Style C ")
            cboFlaps.Items.Add("Style D ")
            cboFlaps.Items.Add("Style E ")
            cboFlaps.Items.Add("Style F ")
            cboFlaps.Items.Add("Raglan A")
            cboFlaps.Items.Add("Raglan B")
            cboFlaps.Items.Add("Raglan C")
            cboFlaps.Items.Add("Raglan D")
            cboFlaps.Items.Add("Raglan E")
            cboFlaps.Items.Add("Raglan F")

            cboPressure.Items.Add("15")
            cboPressure.Items.Add("20")
            cboPressure.Items.Add("25")
            cboPressure.SelectedIndex = 0

            'Default side
            MANGLOVERight_bas.g_sSide = "Right"

            'load
            MANGLOVERight_bas.PR_LoadPowernetChart()
            Dim fileNo As String = "", patient As String = "", diagnosis As String = "", age As String = "", sex As String = ""
            Dim workOrder As String = "", tempDate As String = "", tempEng As String = "", units As String = ""
            Dim blkId As ObjectId = New ObjectId()
            Dim obj As New BlockCreation.BlockCreation
            blkId = obj.LoadBlockInstance()
            If (blkId.IsNull()) Then
                MsgBox("No Patient Details have been found in drawing!", 16, "Glove Details")
                Me.Close()
                Exit Sub
            End If
            obj.BindAttributes(blkId, fileNo, patient, diagnosis, age, sex, workOrder, tempDate, tempEng, units)
            txtFileNo.Text = fileNo
            txtPatientName.Text = patient
            txtDiagnosis.Text = diagnosis
            txtAge.Text = age
            txtSex.Text = sex
            txtUnits.Text = units
            txtWorkOrder.Text = workOrder

            PR_UpdateAge()
            Form_LinkClose()
            optLittleTip(2).Checked = True
            optRingTip(2).Checked = True
            optMiddleTip(2).Checked = True
            optIndexTip(2).Checked = True
            optThumbTip(2).Checked = True
            _optInsertStyle_2.Checked = True
            '_optFold_0.Checked = True

            readDWGInfo()
            g_sChangeChecker = FN_GloveDataChanged()
        Catch ex As Exception
            Me.Close()
            ManGlvMain.ManGlvMainDlg.Close()
        End Try
    End Sub

    Private Sub labCir_DoubleClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles labCir.DoubleClick
        Dim Index As Short = labCir.GetIndex(eventSender)
        'Procedure to disable the circumference fields
        'to enable the use to select only the relevent
        'data to be drawn
        'Saves having to delele and re-enter to get variations
        'on a glove
        Dim iDIP, iPIP As Short
        If System.Drawing.ColorTranslator.ToOle(labCir(Index).ForeColor) = &HC0C0C0 Then
            Select Case Index
                Case 0 To 7, 11 To 12
                    PR_EnableCir(Index)
                Case 8
                    PR_EnableCir(Index)
                    'Disable thumb length
                    labLen(4).ForeColor = System.Drawing.ColorTranslator.FromOle(&H0)
                    txtLen(4).Enabled = True
                    lblLen(4).Enabled = True
                Case 20 To 23
                    iDIP = (Index - 20) * 2
                    iPIP = iDIP + 1
                    labCir(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&H0)
                    PR_EnableCir(iDIP)
                    PR_EnableCir(iPIP)
                    'Enable finger length
                    labLen(Index - 20).ForeColor = System.Drawing.ColorTranslator.FromOle(&H0)
                    txtLen(Index - 20).Enabled = True
                    lblLen(Index - 20).Enabled = True
            End Select
        Else
            Select Case Index
                Case 0 To 7, 12
                    PR_DisableCir(Index)
                Case 8
                    PR_DisableCir(Index)
                    'Disable thumb length
                    labLen(4).ForeColor = System.Drawing.ColorTranslator.FromOle(&HC0C0C0)
                    txtLen(4).Enabled = False
                    lblLen(4).Enabled = False
                Case 11
                    PR_DisableCir(Index)
                    PR_DisableCir(Index + 1)
                Case 20 To 23
                    iDIP = (Index - 20) * 2
                    iPIP = iDIP + 1
                    labCir(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&HC0C0C0)
                    PR_DisableCir(iDIP)
                    PR_DisableCir(iPIP)
                    'Disable finger length
                    labLen(Index - 20).ForeColor = System.Drawing.ColorTranslator.FromOle(&HC0C0C0)
                    txtLen(Index - 20).Enabled = False
                    lblLen(Index - 20).Enabled = False
            End Select
        End If
    End Sub

    Private Sub labLen_DoubleClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles labLen.DoubleClick
        Dim Index As Short = labLen.GetIndex(eventSender)
        If Index = 5 Then Exit Sub
        If System.Drawing.ColorTranslator.ToOle(labLen(Index).ForeColor) = &HC0C0C0 Then
            labLen(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&H0)
            txtLen(Index).Enabled = True
            lblLen(Index).Enabled = True
        Else
            labLen(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&HC0C0C0)
            txtLen(Index).Enabled = False
            lblLen(Index).Enabled = False
        End If
    End Sub

    Private Sub lblPleat_DoubleClick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles lblPleat.DoubleClick
        Dim Index As Short = lblPleat.GetIndex(eventSender)
        'Procedure to disable the pleat fields
        'to enable the user to select only the relevent
        'data to be drawn
        'Saves having to delele and re-enter to get variations
        'on a glove
        Dim iDIP, iPIP As Short
        If System.Drawing.ColorTranslator.ToOle(lblPleat(Index).ForeColor) = &HC0C0C0 Then
            lblPleat(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&H0)
            Select Case Index
                Case 0
                    txtWristPleat1.Enabled = True
                Case 1
                    txtWristPleat2.Enabled = True
                Case 2
                    txtShoulderPleat2.Enabled = True
                Case 3
                    txtShoulderPleat1.Enabled = True
            End Select
        Else
            lblPleat(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&HC0C0C0)
            Select Case Index
                Case 0
                    txtWristPleat1.Enabled = False
                Case 1
                    txtWristPleat2.Enabled = False
                Case 2
                    txtShoulderPleat2.Enabled = False
                Case 3
                    txtShoulderPleat1.Enabled = False
            End Select
        End If

    End Sub

    Private Sub OK_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles OK.Click
        Dim sTask As String
        Dim iInsert, iError As Short
        'Don't allow multiple clicking
        '
        ''OK.Enabled = False

        'Get data from the Dialogue
        PR_GetDlgData()

        If FN_ValidateData("Check All") Then
            'Display and hourglass
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor
            MANGLOVERight_bas.PR_LoadReductionCharts((cboFabric.Text))

            'These functions, FN_CalculateInsert() and FN_TranslateInsert()
            'interact with the user.
            'If they returns false then either an error has occoured
            'or the user has chosen to make changes.
            'In either case the user is returned to the dialogue
            Me.Hide()
            ManGlvMain.ManGlvMainDlg.Hide()
            If cboInsert.Text = "Calculate" Then
                If Not FN_CalculateInsert(g_iInsertSize, "CALC") Then GoTo EXIT_OK
            Else
                'Translate cboInsert.Text to correct format
                'for use below
                If Not FN_TranslateInsert((cboInsert.SelectedIndex), g_iInsertSize) Then
                    GoTo EXIT_OK
                Else
                    'Use the given insertsize
                    If Not FN_CalculateInsert(g_iInsertSize, "USE") Then GoTo EXIT_OK
                End If
            End If

            'Check with the glove common
            If Not FN_CompareWithOtherGlove((cboFabric.Text), g_RightOnFold, g_iInsertSize) Then
                GoTo EXIT_OK
            End If

            'Calculate glove points based on insert given above and data.
            If Not FN_CalcGlovePoints(g_iInsertSize) Then GoTo EXIT_OK

            'Calculate glove extensions (if any)
            MANGLOVERight_bas.PR_CalculateExtension(xyRightPalm(1), xyRightPalm(6), g_iRightInsertValue(g_iInsertSize) * RIGHT_EIGHTH, UlnarProfile, RadialProfile)

            PR_UpdateDDE()

            PR_UpdateDDE_Extension()

            MANGLOVERight_bas.PR_GetInsertionPoint()
            Dim sDrawFile As String = fnGetSettingsPath("PathDRAW") & "\draw.d"
            PR_CreateDrawMacro(sDrawFile, g_iInsertSize)
            Dim xyBase As MANGLOVERight_bas.XY
            MANGLOVERight_bas.PR_DrawMarker(xyBase)
EXIT_OK:
            saveInfoToDWG()
            OK.Enabled = True
            Me.Close()
            ManGlvMain.ManGlvMainDlg.Close()
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default
            Exit Sub
        End If
    End Sub

    Private Sub optExtendTo_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optExtendTo.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optExtendTo.GetIndex(eventSender)
            'Use Procedure in module GLVEXTEN.BAS
            If MANGLOVERight_bas.MainForm.Visible Then PR_ExtendTo_Click(Index)
        End If
    End Sub

    Private Sub optFold_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optFold.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optFold.GetIndex(eventSender)
            If g_RightDataIsCalcuable Then cmdCalculateInsert_Click(cmdCalculateInsert, New System.EventArgs())
        End If
    End Sub

    Private Sub optHand_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optHand.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optHand.GetIndex(eventSender)
            If Index = 0 Then
                MANGLOVERight_bas.g_sSide = "Left"
            Else
                MANGLOVERight_bas.g_sSide = "Right"
            End If
        End If
    End Sub

    Private Sub optIndexTip_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optIndexTip.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optIndexTip.GetIndex(eventSender)
            PR_CutBackTip(3, Index)
        End If
    End Sub

    Private Sub optInsertStyle_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optInsertStyle.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optInsertStyle.GetIndex(eventSender)
            If Index = 1 Then
                optFold(0).Checked = True
                optFold(1).Enabled = False
            Else
                optFold(1).Enabled = True
            End If
        End If
    End Sub

    Private Sub optLittleTip_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optLittleTip.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optLittleTip.GetIndex(eventSender)
            PR_CutBackTip(0, Index)
        End If
    End Sub

    Private Sub optMiddleTip_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optMiddleTip.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optMiddleTip.GetIndex(eventSender)
            PR_CutBackTip(2, Index)
        End If
    End Sub

    Private Sub optProximalTape_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optProximalTape.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optProximalTape.GetIndex(eventSender)
            'Use Procedure in module GLVEXTEN.BAS
            PR_ProximalTape_Click(Index)
        End If
    End Sub

    Private Sub optRingTip_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optRingTip.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optRingTip.GetIndex(eventSender)
            PR_CutBackTip(1, Index)
        End If
    End Sub

    Private Sub optThumbTip_CheckedChanged(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles optThumbTip.CheckedChanged
        If eventSender.Checked Then
            Dim Index As Short = optThumbTip.GetIndex(eventSender)
            PR_CutBackTip(4, Index)
        End If
    End Sub

    Private Sub PR_CreateDrawMacro(ByRef sFileName As String, ByRef iInsertSize As Short)
        '
        ' GLOBALS
        '
        '    g_sSide
        '    g_FingerThumbBlend
        '
        Dim ii As Short
        Dim xyTextAlign, xyPt2, xyPt1, xyPt3, xyTmp As MANGLOVERight_bas.XY
        Dim nAge, iiStart As Short
        Dim TmpY, nInsert, TmpX, nOffSet As Double
        Dim FingerLFSBlend, WristLFSBlend As MANGLOVERight_bas.curve
        Dim WristThumbBlend As MANGLOVERight_bas.curve
        Dim ThumbSide, LFS, Reinforced As MANGLOVERight_bas.curve
        Dim nLength, aAngle, nTranslate As Double
        Dim sWorkOrder, sText As String

        'Initialise
        ''------MANGLOVERight_bas.fNum = FN_Open("C:\JOBST\DRAW.D", txtPatientName.Text, txtFileNo.Text, MANGLOVERight_bas.g_sSide)
        MANGLOVERight_bas.fNum = FN_Open(sFileName, txtPatientName.Text, txtFileNo.Text, MANGLOVERight_bas.g_sSide)

        'Draw on correct layer
        ARMDIA1.PR_SetLayer("Template" & MANGLOVERight_bas.g_sSide)

        'Get Blending profiles on thumbside
        'N.B. Modifications to xyRightPalmThumb(1) and xyRightPalmThumb(4)
        If RadialProfile.n = 1 Then iiStart = 1 Else iiStart = 2
        xyPt1.X = RadialProfile.X(iiStart)
        xyPt1.y = RadialProfile.y(iiStart)

        '    PR_CalcRightFingerBlendingProfile xyRightIndex, xyRightPalm(4), xyRightMiddle, g_RightFinger(4), FingerThumbBlend, xyRightPalmThumb(1)
        PR_CalcRightWristBlendingProfile(xyRightPalm(4), xyRightPalm(5), xyRightPalm(6), xyPt1, WristThumbBlend, xyRightPalmThumb(4))

        'Get Blending profiles on Little RightFinger Side
        If Not g_RightOnFold Then
            xyPt1.X = UlnarProfile.X(iiStart)
            xyPt1.y = UlnarProfile.y(iiStart)
            PR_CalcRightWristBlendingProfile(xyRightPalm(3), xyRightPalm(2), xyRightPalm(1), xyPt1, WristLFSBlend, xyTmp)
        End If
        MANGLOVERight_bas.PR_MakeXY(xyPt3, xyRightPalm(3).X, xyRightPalm(4).y)
        PR_CalcRightFingerBlendingProfile(xyRightLFS, xyPt3, xyRightLittle, g_RightFinger(1), FingerLFSBlend, xyTmp)

        'Calculate other PalmThumb points based on point xyRightPalmThumb(1)
        'Done here so it can be handed below
        '
        nInsert = g_iRightInsertValue(iInsertSize) * RIGHT_EIGHTH

        'Calculate a point that will be used to align text
        MANGLOVERight_bas.PR_MakeXY(xyTextAlign, xyRightPalm(1).X + 0.25, xyRightPalm(1).y + 1.625)

        'Mirror and translate for right hand side
        'To now all the data has been caculated for the LEFT
        'side
        If MANGLOVERight_bas.g_sSide = "Right" Then
            'Find farthest point from datum
            nTranslate = ARMDIA1.max(g_RightFinger(4).xyRadial.X, RadialProfile.X(RadialProfile.n))
            nTranslate = nTranslate - xyRightDatum.X
            'Fingers
            For ii = 1 To 4
                MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, g_RightFinger(ii).xyRadial)
                MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, g_RightFinger(ii).xyUlnar)
            Next ii
            'Curves
            MANGLOVERight_bas.PR_MirrorCurveInYaxis(xyRightDatum.X, nTranslate, g_FingerThumbBlend)
            MANGLOVERight_bas.PR_MirrorCurveInYaxis(xyRightDatum.X, nTranslate, WristThumbBlend)
            If Not g_RightOnFold Then
                MANGLOVERight_bas.PR_MirrorCurveInYaxis(xyRightDatum.X, nTranslate, WristLFSBlend)
            End If
            MANGLOVERight_bas.PR_MirrorCurveInYaxis(xyRightDatum.X, nTranslate, FingerLFSBlend)
            MANGLOVERight_bas.PR_MirrorCurveInYaxis(xyRightDatum.X, nTranslate, RadialProfile)
            MANGLOVERight_bas.PR_MirrorCurveInYaxis(xyRightDatum.X, nTranslate, UlnarProfile)
            'Points
            For ii = 1 To 6
                MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightPalm(ii))
            Next ii
            For ii = 1 To 5
                MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightPalmThumb(ii))
            Next ii
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightLFS)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightLittle)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightRing)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightMiddle)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightIndex)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, xyRightThumb)

            'Top of thumb curve
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, g_RightThumbTopCurve.xyStart)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, g_RightThumbTopCurve.xyTangent)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, g_RightThumbTopCurve.xyEnd)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, g_RightThumbTopCurve.xyR1)
            MANGLOVERight_bas.PR_MirrorPointInYaxis(xyRightDatum.X, nTranslate, g_RightThumbTopCurve.xyR2)

            'Text alignment
            nLength = ARMDIA1.max((Len(txtPatientName.Text) * 0.075) + 0.125, 1.5)
            MANGLOVERight_bas.PR_MakeXY(xyTextAlign, xyRightPalm(1).X - nLength, xyRightPalm(1).y + 1.625)
        End If

        'Draw Fingers (For missing see later)
        For ii = 1 To 4
            PR_DrawFingers(g_RightFinger(ii))
        Next ii
        'For ii = 1 To 5
        '    MANGLOVERight_bas.PR_DrawMarkerNamed "detail", xyRightPalmThumb(ii), .1, .1, 0
        '    PR_DrawText Trim$(Str$(ii)), xyRightPalmThumb(ii), .075
        'Next ii
        'Draw Thumb
        If g_nRightFingerLen(5) <> 0 Then
            Select Case g_iRightThumbStyle
                Case 0
                    PR_DrawNormalThumb(nInsert)
                Case 1
                    'do nothing as this is an edged thumb which does not require
                    'a length, drawn below
                Case 2
                    PR_DrawSetInThumb(nInsert)
            End Select
        Else
            If g_iRightThumbStyle = 1 Then PR_DrawEdgedThumb(nInsert)
        End If

        'Draw arm extension
        'Create profile for LitteFingerSide and ThumbSide
        'Little RightFinger Side
        If Not g_RightOnFold Then
            'Join blending profiles to extension
            LFS.n = 0
            Dim nCount1 As Integer = FingerLFSBlend.n + (WristLFSBlend.n - 2) + (UlnarProfile.n - 1)
            If RadialProfile.n = 1 Then
                nCount1 = nCount1 + 1
            End If
            Dim X(nCount1), Y(nCount1) As Double
            LFS.X = X
            LFS.y = Y
            For ii = 1 To FingerLFSBlend.n
                LFS.n = LFS.n + 1
                LFS.X(LFS.n) = FingerLFSBlend.X(ii)
                LFS.y(LFS.n) = FingerLFSBlend.y(ii)
            Next ii
            For ii = 2 To WristLFSBlend.n - 1
                LFS.n = LFS.n + 1
                LFS.X(LFS.n) = WristLFSBlend.X(ii)
                LFS.y(LFS.n) = WristLFSBlend.y(ii)
            Next ii
            'Add wrist point, only if RadialProfile.n = 1
            If RadialProfile.n = 1 Then
                LFS.n = LFS.n + 1
                LFS.X(LFS.n) = WristLFSBlend.X(WristLFSBlend.n)
                LFS.y(LFS.n) = WristLFSBlend.y(WristLFSBlend.n)
            End If
            For ii = 2 To UlnarProfile.n
                LFS.n = LFS.n + 1
                LFS.X(LFS.n) = UlnarProfile.X(ii)
                LFS.y(LFS.n) = UlnarProfile.y(ii)
            Next ii
        Else
            'To reduce code in the Zipper drawing module we
            'artificially create a polyline of three points
            'We know that the macro language always creates a polyline
            'if there are only 3 points in a fitted curve.
            'This is a bug in Drafix 3.01, which we take care of here
            'however this may be fixed for a subsequent release so beware!

            LFS.n = 0
            Dim nCount2 As Integer = FingerLFSBlend.n + 1
            Dim PolyX(nCount2), PolyY(nCount2) As Double
            LFS.X = PolyX
            LFS.y = PolyY
            For ii = 1 To FingerLFSBlend.n
                LFS.n = LFS.n + 1
                LFS.X(LFS.n) = FingerLFSBlend.X(ii)
                LFS.y(LFS.n) = FingerLFSBlend.y(ii)
            Next ii
            LFS.X(LFS.n) = xyRightLFS.X
            LFS.y(LFS.n) = UlnarProfile.y(UlnarProfile.n) + ((xyRightLFS.y - UlnarProfile.y(UlnarProfile.n)) / 2)

            LFS.n = LFS.n + 1
            LFS.X(LFS.n) = UlnarProfile.X(UlnarProfile.n)
            LFS.y(LFS.n) = UlnarProfile.y(UlnarProfile.n)
        End If

        'Draw
        ARMDIA1.PR_SetLayer("Template" & MANGLOVERight_bas.g_sSide)
        MANGLOVERight_bas.PR_DrawFitted(LFS)
        MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "LFS")
        MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", "%Length")
        'Only add this data base field if this is an editable curve
        If g_RightExtendTo <> RIGHT_GLOVE_NORMAL Then MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "LFS")

        'Thumb side
        'Join blending profiles to extension
        ThumbSide.n = 0
        Dim nCount As Integer = g_FingerThumbBlend.n + (WristThumbBlend.n - 2) + RadialProfile.n
        Dim X1(nCount), Y1(nCount) As Double
        ThumbSide.X = X1
        ThumbSide.y = Y1
        For ii = 1 To g_FingerThumbBlend.n
            ThumbSide.n = ThumbSide.n + 1
            ThumbSide.X(ThumbSide.n) = g_FingerThumbBlend.X(ii)
            ThumbSide.y(ThumbSide.n) = g_FingerThumbBlend.y(ii)
        Next ii
        For ii = 2 To WristThumbBlend.n - 1
            ThumbSide.n = ThumbSide.n + 1
            ThumbSide.X(ThumbSide.n) = WristThumbBlend.X(ii)
            ThumbSide.y(ThumbSide.n) = WristThumbBlend.y(ii)
        Next ii
        'Add wrist point, only if RadialProfile.n = 1
        If RadialProfile.n = 1 Then
            ThumbSide.n = ThumbSide.n + 1
            ThumbSide.X(ThumbSide.n) = WristThumbBlend.X(WristThumbBlend.n)
            ThumbSide.y(ThumbSide.n) = WristThumbBlend.y(WristThumbBlend.n)
        End If
        For ii = 2 To RadialProfile.n
            ThumbSide.n = ThumbSide.n + 1
            ThumbSide.X(ThumbSide.n) = RadialProfile.X(ii)
            ThumbSide.y(ThumbSide.n) = RadialProfile.y(ii)
        Next ii
        'Draw
        MANGLOVERight_bas.PR_DrawFitted(ThumbSide)
        MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "ETS")
        MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", "%Length")
        'Only add this data base field if this is an editable curve
        If g_RightExtendTo <> RIGHT_GLOVE_NORMAL Then MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "ETS")

        'Add labels and construction lines
        ARMDIA1.PR_SetLayer("Construct")

        'Draw notes for each tape
        'If on the fold draw notes at a fixed distance
        If g_RightOnFold Then
            nLength = System.Math.Abs(RadialProfile.X(1) - UlnarProfile.X(1)) / 2
        Else
            nLength = -1
        End If
        ''Added for #224 in the issue list
        Dim nIndex As Short = 0
        If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            While (nIndex + 8 <= 23)
                If (txtExtCir(nIndex + 8).Text <> "") Then
                    Exit While
                End If
                nIndex = nIndex + 1
            End While
        End If
        For ii = 1 To RadialProfile.n
            MANGLOVERight_bas.PR_MakeXY(xyPt1, UlnarProfile.X(ii), UlnarProfile.y(ii))
            MANGLOVERight_bas.PR_MakeXY(xyPt2, RadialProfile.X(ii), RadialProfile.y(ii))
            ''Changed for #224 in the issue list
            Dim nTextBoxIndex As Short = nIndex + ii
            If ii = RadialProfile.n - 1 And g_RightEOSType = RIGHT_ARM_FLAP Then
                'Provide a dummy EOS for the zipper programme
                ''PR_DrawTapeNotes(ii, xyPt1, xyPt2, nLength, "EOS")
                PR_DrawTapeNotes(ii, xyPt1, xyPt2, nLength, "EOS", nTextBoxIndex)
            Else
                ''PR_DrawTapeNotes(ii, xyPt1, xyPt2, nLength, "")
                PR_DrawTapeNotes(ii, xyPt1, xyPt2, nLength, "", nTextBoxIndex)
            End If
            If RightTapeNote(ii).iTapePos = RIGHT_ELBOW_TAPE And RightTapeNote(RadialProfile.n).iTapePos <> RIGHT_ELBOW_TAPE Then
                'Draw elbow line
                ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_BOTTOM_, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)
                ARMDIA1.PR_SetLayer("Notes")
                MANGLOVERight_bas.PR_DrawLine(xyPt1, xyPt2)
                MANGLOVERight_bas.PR_CalcMidPoint(xyPt1, xyPt2, xyPt3)
                xyPt3.y = xyPt3.y + 0.0625
                MANGLOVERight_bas.PR_DrawText("E", xyPt3, 0.1, 0)
            End If
        Next ii

        If g_RightEOSType = RIGHT_ARM_FLAP Then
            MANGLOVERight_bas.PR_DrawShoulderFlaps(xyPt1, xyPt2, cboFlaps.Text, txtCustFlapLength.Text, txtStrapLength.Text, txtFrontStrapLength.Text, txtWaistCir.Text)
        Else
            'Draw EOS, account for elastic if last tape is at elbow
            'N.B. our elbow is fixed at tape no 9
            ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_BOTTOM_, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)
            ARMDIA1.PR_SetLayer("Notes")
            If RightTapeNote(RadialProfile.n).iTapePos = RIGHT_ELBOW_TAPE Then
                MANGLOVERight_bas.PR_MakeXY(xyPt3, RadialProfile.X(RadialProfile.n - 1), RadialProfile.y(RadialProfile.n - 1))
                aAngle = System.Math.Abs(90 - MANGLOVERight_bas.FN_CalcAngle(xyPt2, xyPt3))
                If Val(txtAge.Text) > 10 Then nOffSet = 0.75 Else nOffSet = 0.375
                nLength = nOffSet / System.Math.Cos(aAngle * (MANGLOVERight_bas.PI / 180))
                MANGLOVERight_bas.PR_CalcPolar(xyPt2, MANGLOVERight_bas.FN_CalcAngle(xyPt2, xyPt3), nLength, xyPt2)
                If Not g_RightOnFold Then
                    MANGLOVERight_bas.PR_MakeXY(xyPt3, UlnarProfile.X(UlnarProfile.n - 1), UlnarProfile.y(UlnarProfile.n - 1))
                    MANGLOVERight_bas.PR_CalcPolar(xyPt1, MANGLOVERight_bas.FN_CalcAngle(xyPt1, xyPt3), nLength, xyPt1)
                Else
                    xyPt1.y = xyPt1.y + nOffSet
                End If
                MANGLOVERight_bas.PR_CalcMidPoint(xyPt1, xyPt2, xyPt3)
                xyPt3.y = xyPt3.y + 0.0625
                MANGLOVERight_bas.PR_DrawText("E", xyPt3, 0.1, 0)
            End If

            'Elastic for children
            If Val(txtAge.Text) <= 10 Then
                ARMDIA1.PR_SetLayer("Notes")
                MANGLOVERight_bas.PR_CalcMidPoint(xyPt1, xyPt2, xyPt3)
                xyPt3.y = xyPt3.y + 0.25
                ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_VERT_CENTER, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)
                MANGLOVERight_bas.PR_DrawText("1/2" & Chr(34) & " Elastic", xyPt3, 0.1, 0)
            End If

            'Draw EOS line
            ARMDIA1.PR_SetLayer("Template" & MANGLOVERight_bas.g_sSide)
            MANGLOVERight_bas.PR_DrawLine(xyPt1, xyPt2)
            MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "EOS")
            MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "EOS")
        End If

        'Draw Centre line
        If Not g_RightOnFold Then
            ARMDIA1.PR_SetLayer("Construct")
            aAngle = MANGLOVERight_bas.FN_CalcAngle(xyPt1, xyPt2)
            nLength = MANGLOVERight_bas.FN_CalcLength(xyPt1, xyPt2)
            MANGLOVERight_bas.PR_CalcPolar(xyPt1, aAngle, nLength / 2, xyPt1)
            MANGLOVERight_bas.PR_MakeXY(xyPt1, xyPt1.X, UlnarProfile.y(1))
            MANGLOVERight_bas.PR_MakeXY(xyPt2, xyPt1.X, UlnarProfile.y(UlnarProfile.n))
            MANGLOVERight_bas.PR_DrawLine(xyPt1, xyPt2)
        End If

        'Insert Size (Text)
        'Position Insert text Size w.r.t Little RightFinger web
        ARMDIA1.PR_SetLayer("Notes")
        If nInsert = 0.375 Then
            'Substitute 1/2" for 3/8" inch for printing only
            sText = Trim(ARMEDDIA1.fnInchesToText(0.5))
        Else
            sText = Trim(ARMEDDIA1.fnInchesToText(nInsert))
        End If
        If Mid(sText, 1, 1) = "-" Then sText = Mid(sText, 2) 'Strip leading "-" sign
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sText = sText & QQ
        If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            sText = Trim(fnDisplayToCM(nInsert)) & "CM"
        End If
        sText = "INSERT " & sText
        MANGLOVERight_bas.PR_MakeXY(xyPt3, xyRightLittle.X, xyRightLittle.y - (nInsert + 0.25))
        ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_VERT_CENTER, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)
        MANGLOVERight_bas.PR_DrawText(sText, xyPt3, 0.1, 0)

        'Inserts
        Select Case g_iRightInsertStyle
            Case 1 'One insert, Little RightFinger Only, Off the Fold
                ''-------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightLittle, 1, 0)
                ''--------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightRing, 1, 0)
                ''--------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightMiddle, 1, 0)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightLittle)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightRing)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightMiddle)
                If MANGLOVERight_bas.g_sSide = "Left" Then
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, xyRightPalmThumb(1).X - 0.0625, xyRightPalmThumb(1).y + 0.03125)
                Else
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, xyRightPalmThumb(1).X + 0.0625, xyRightPalmThumb(1).y + 0.03125)
                End If
                MANGLOVERight_bas.PR_MakeXY(xyPt2, xyPt1.X, xyRightPalmThumb(1).y + 0.25)
                MANGLOVERight_bas.PR_DrawLine(xyPt1, xyPt2)
                MANGLOVERight_bas.PR_DrawMarkerNamed("Open Short Arrow", xyPt1, 0.1, 0.125, 90)
                MANGLOVERight_bas.PR_DrawMarkerNamed("Open Short Arrow", xyPt2, 0.1, 0.125, 270)
            Case 2 'One insert, Little and Index RightFinger, On or Off the Fold
                ''-------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightLittle, 1, 0)
                ''------------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightRing, 1, 0)
                ''---------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightMiddle, 1, 0)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightLittle)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightRing)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightMiddle)
            Case 3 'One insert Glove, On or Off the Fold
                ''----------------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightLittle, 1, 0)
                ''-------------------ARMDIA1.PR_InsertSymbol("GloveInsertU", xyRightMiddle, 1, 0)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightLittle)
                MANGLOVERight_bas.PR_DrawGloveInsertU(xyRightMiddle)
        End Select

        'Add wrist line on notes
        MANGLOVERight_bas.PR_DrawLine(xyRightPalm(1), xyRightPalm(6))
        ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_BOTTOM_, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)
        MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "RIGHT_WRIST")
        MANGLOVERight_bas.PR_CalcMidPoint(xyRightPalm(1), xyRightPalm(6), xyPt3)
        xyPt3.y = xyPt3.y + 0.0625
        MANGLOVERight_bas.PR_DrawText("W", xyPt3, 0.1, 0)

        'print fold line if drawn on the fold and print requested
        If g_RightPrintFold And g_RightOnFold Then
            xyPt3 = xyRightPalm(1)
            xyPt3.y = xyRightPalm(1).y - 0.25
            If MANGLOVERight_bas.g_sSide = "Right" Then
                MANGLOVERight_bas.g_nCurrTextAngle = 90
            Else
                MANGLOVERight_bas.g_nCurrTextAngle = -90
            End If
            MANGLOVERight_bas.PR_DrawText("Fold", xyPt3, 0.1, (MANGLOVERight_bas.g_nCurrTextAngle * (MANGLOVERight_bas.PI / 180)))
            MANGLOVERight_bas.g_nCurrTextAngle = 0
        End If

        'Slanted inserts
        If chkSlantedInserts.CheckState = 1 Then
            nInsert = nInsert - 0.125
            For ii = 2 To 4
                If g_RightMissing(ii) <> True Then
                    If ii = 2 Then
                        TmpX = xyRightLittle.X
                        TmpY = xyRightLittle.y
                    ElseIf ii = 3 Then
                        TmpX = xyRightRing.X
                        TmpY = xyRightRing.y
                    Else
                        TmpX = xyRightMiddle.X
                        TmpY = xyRightMiddle.y
                    End If
                    'Vertical line
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, TmpX, TmpY)
                    MANGLOVERight_bas.PR_MakeXY(xyPt2, TmpX, TmpY - nInsert)
                    MANGLOVERight_bas.PR_DrawLine(xyPt1, xyPt2)

                    'Horizontal line
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, TmpX + RIGHT_EIGHTH, TmpY - nInsert)
                    MANGLOVERight_bas.PR_MakeXY(xyPt2, TmpX - RIGHT_EIGHTH, TmpY - nInsert)
                    MANGLOVERight_bas.PR_DrawLine(xyPt1, xyPt2)
                End If
            Next ii
        End If

        'Reinforced Palms
        If chkPalm.CheckState = 1 Or chkDorsal.CheckState = 1 Then
            Reinforced.n = 5
            Dim X(5), Y(5) As Double
            Reinforced.X = X
            Reinforced.y = Y
            Reinforced.X(1) = xyRightLFS.X
            Reinforced.y(1) = xyRightLFS.y - 0.25
            Reinforced.X(2) = xyRightLittle.X
            Reinforced.y(2) = xyRightLittle.y - 0.25
            Reinforced.X(3) = xyRightRing.X
            Reinforced.y(3) = xyRightRing.y - 0.25
            Reinforced.X(4) = xyRightMiddle.X
            Reinforced.y(4) = xyRightMiddle.y - 0.25
            Reinforced.X(5) = xyRightIndex.X
            Reinforced.y(5) = xyRightMiddle.y - 0.25

            'Draw reinforcing lines using Long Dash line type
            MANGLOVERight_bas.PR_SetLineStyle("Long Dash")
            ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_VERT_CENTER, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)

            'Establish a text insert point
            MANGLOVERight_bas.PR_CalcMidPoint(xyRightPalm(3), xyRightPalm(4), xyPt3)
            xyPt3.y = xyPt3.y - 0.75

            If chkPalm.CheckState = 1 Then
                MANGLOVERight_bas.PR_DrawFitted(Reinforced)
                'Add REINFORCED text
                ''MANGLOVERight_bas.PR_DrawText("REINFORCED" & Chr(10) & "PALM", xyPt3, 0.1, 0)
                MANGLOVERight_bas.PR_DrawMText("REINFORCED" & Chr(10) & "PALM", xyPt3, 0, True)
            End If

            If chkDorsal.CheckState = 1 Then
                If chkSlantedInserts.CheckState = 1 Then
                    For ii = 1 To 5
                        Reinforced.y(ii) = Reinforced.y(ii) - nInsert
                    Next ii
                End If
                MANGLOVERight_bas.PR_DrawFitted(Reinforced)
                'Add REINFORCED text
                If chkPalm.CheckState = 1 Then
                    'Shift text so that it misses RIGHT_PALM Text
                    If txtSide.Text = "Right" Then
                        xyPt3.y = xyPt3.y - 0.4
                    Else
                        xyPt3.y = xyPt3.y + 0.4
                    End If
                End If
                ''MANGLOVERight_bas.PR_DrawText("REINFORCED" & Chr(10) & "DORSAL", xyPt3, 0.1, 0)
                MANGLOVERight_bas.PR_DrawMText("REINFORCED" & Chr(10) & "DORSAL", xyPt3, 0, True)
            End If

            'Close at wrist
            MANGLOVERight_bas.PR_DrawLine(xyRightPalm(1), xyRightPalm(6))
            MANGLOVERight_bas.PR_SetLineStyle("bylayer")
        End If

        'Missing Fingers
        PR_DrawMissingFingers()

        'Patient Details
        ARMDIA1.PR_SetTextData(1, 32, -1, -1, -1) 'Horiz-Left, Vertical-Bottom
        xyPt3 = xyTextAlign
        If txtWorkOrder.Text = "" Then
            sWorkOrder = "-"
        Else
            sWorkOrder = txtWorkOrder.Text
        End If
        sText = txtSide.Text & Chr(10) & txtPatientName.Text & Chr(10) & sWorkOrder & Chr(10) & Trim(Mid(cboFabric.Text, 4))
        ''--------MANGLOVERight_bas.PR_DrawText(sText, xyPt3, 0.1, 0)
        MANGLOVERight_bas.PR_DrawMText(sText, xyPt3, 0)

        'Other patient details in black on layer construct
        ARMDIA1.PR_SetLayer("Construct")
        MANGLOVERight_bas.PR_MakeXY(xyPt3, xyPt3.X, xyPt3.y - 0.8)
        sText = txtFileNo.Text & Chr(10) & txtDiagnosis.Text & Chr(10) & txtAge.Text & Chr(10) & txtSex.Text
        ''---------MANGLOVERight_bas.PR_DrawText(sText, xyPt3, 0.1, 0)
        MANGLOVERight_bas.PR_DrawMText(sText, xyPt3, 0)

        'Markers for zippers
        'Place markers at lowest Web positions
        'For latter use with Zips, if a slant insert given then
        'supply the length of the slant insert
        'Added here capture mods to nInsert and drawn on layer construct

        MANGLOVERight_bas.PR_DrawMarker(xyRightPalm(1))
        MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "PALMER")
        MANGLOVE1.PR_AddXDataValToLast("Age", txtAge.Text)
        MANGLOVE1.PR_AddXDataValToLast("Data", txtDataGlove.Text)
        ARMDIA1.PR_NamedHandle("hPalmer") 'Save a HANDLE for later use

        MANGLOVERight_bas.PR_DrawMarker(xyRightLittle)
        MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "PALMER-WEB")
        If chkSlantedInserts.CheckState = 1 Then
            MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", Str(nInsert))
        Else
            MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", "0")
        End If

        MANGLOVERight_bas.PR_DrawMarker(xyRightRing)
        MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "DORSAL-WEB")
        If chkSlantedInserts.CheckState = 1 Then
            MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", Str(nInsert))
        Else
            MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", "0")
        End If

        MANGLOVERight_bas.PR_DrawMarker(xyRightPalmThumb(3))
        MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "DORSAL")

        MANGLOVERight_bas.PR_DrawMarker(xyRightMiddle)
        MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", "OUTSIDE-WEB")
        If chkSlantedInserts.CheckState = 1 Then
            MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", Str(nInsert))
        Else
            MANGLOVERight_bas.PR_AddXDataValToLast("ZipperLength", "0")
        End If

        'Markers for editors
        For ii = 2 To 6
            MANGLOVERight_bas.PR_DrawMarker(xyRightPalm(ii))
            MANGLOVERight_bas.PR_AddXDataValToLast("Data", "RIGHT_PALM" & Trim(Str(ii)))
        Next ii

        'Update symbol gloveglove, glovecommon and marker palmer with data
        PR_DrawGloveCommonBlock()
        PR_DrawGloveRightBlock()
        PR_UpdateDBFields("Draw")
        ARMDIA1.PR_SetLayer("1")

debugclose:
        FileClose(MANGLOVERight_bas.fNum)
    End Sub

    Private Sub PR_CreateSaveMacro(ByRef sDrafixFile As String)
        'Open the DRAFIX macro file
        'Initialise Global variables

        'Open file
        MANGLOVERight_bas.fNum = FreeFile()
        FileOpen(MANGLOVERight_bas.fNum, sDrafixFile, VB.OpenMode.Output)

        'Initialise String globals
        CC = Chr(44) 'The comma (,)
        NL = Chr(10) 'The new line character
        QQ = Chr(34) 'Double quotes (")
        QCQ = QQ & CC & QQ
        QC = QQ & CC
        CQ = CC & QQ

        'Write header information etc. to the DRAFIX macro file
        '
        PrintLine(MANGLOVERight_bas.fNum, "//DRAFIX Macro created - " & DateString & "  " & TimeString)
        PrintLine(MANGLOVERight_bas.fNum, "//Patient " & txtPatientName.Text & ", " & txtFileNo.Text & ", Hand - " & MANGLOVERight_bas.g_sSide)
        PrintLine(MANGLOVERight_bas.fNum, "//by Visual Basic, GLOVES - Save only")

        'Define DRAFIX variables
        PrintLine(MANGLOVERight_bas.fNum, "HANDLE hLayer, hChan, hSym, hEnt, hOrigin, hMPD;")
        PrintLine(MANGLOVERight_bas.fNum, "XY     xyO, xyScale;")
        PrintLine(MANGLOVERight_bas.fNum, "STRING sFileNo, sSide, sID, sName;")
        PrintLine(MANGLOVERight_bas.fNum, "ANGLE  aAngle;")

        'Clear user selections etc
        PrintLine(MANGLOVERight_bas.fNum, "UserSelection (" & QQ & "clear" & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "Execute (" & QQ & "menu" & QCQ & "SetStyle" & QC & "Table(" & QQ & "find" & QCQ & "style" & QCQ & "bylayer" & QQ & "));")

        'Display Hour Glass symbol
        PrintLine(MANGLOVERight_bas.fNum, "Display (" & QQ & "cursor" & QCQ & "wait" & QCQ & "Saving" & QQ & ");")

        'Get Handle and Origin of "mainpatientdetails symbol"
        PrintLine(MANGLOVERight_bas.fNum, "hMPD = UID (" & QQ & "find" & QC & Val(txtUidMPD.Text) & ");")
        PrintLine(MANGLOVERight_bas.fNum, "if (hMPD)")
        PrintLine(MANGLOVERight_bas.fNum, "  GetGeometry(hMPD, &sName, &xyO, &xyScale, &aAngle);")
        PrintLine(MANGLOVERight_bas.fNum, "else")
        PrintLine(MANGLOVERight_bas.fNum, "  Exit(%cancel," & QQ & "Can't find > mainpatientdetails < symbol, Insert Patient Data" & QQ & ");")

        'Save DB fileds
        PR_UpdateDBFields("Save")
        FileClose(MANGLOVERight_bas.fNum)
    End Sub

    Private Sub PR_CutBackTip(ByRef iFinger As Short, ByRef iOption As Object)
        'Function to cut down code w.r.t.
        ' optLittleTip_Click
        ' optMiddleTip_Click
        ' optRingTip_Click
        ' optIndexTip_Click
        ' optThumbTip_Click

        Dim nLen As Double
        Dim nTipCutBack As Double

        If txtLen(iFinger).Enabled = False Then Exit Sub

        nLen = MANGLOVERight_bas.FN_InchesValue(txtLen(iFinger))
        If nLen <= 0 Then Exit Sub

        If Val(txtAge.Text) <= AGE_CUTOFF Then
            nTipCutBack = CHILD_STD_OPEN_TIP
        Else
            nTipCutBack = ADULT_STD_OPEN_TIP
        End If

        If iOption = 1 Then nLen = nLen - nTipCutBack

        If nLen > 0 Then lblLen(iFinger).Text = MANGLOVERight_bas.fnInchestoText(nLen)
    End Sub

    Private Sub PR_DisableCir(ByRef Index As Short)
        'Read as part of labCir_DblClick
        labCir(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&HC0C0C0)
        txtCir(Index).Enabled = False
        lblCir(Index).Enabled = False
    End Sub

    Private Sub PR_DrawEdgedThumb(ByRef nInsert As Object)
        'Procedure to draw an edged thumb
        Static aAngle As Short
        Static nFillet As Single
        Static xyTmp3, xyTmp1, xyTmp2, xyKeep As MANGLOVERight_bas.XY

        'Set angle depending on side
        If MANGLOVERight_bas.g_sSide = "Right" Then aAngle = 360 Else aAngle = 180

        nFillet = 0.125

        'Thumb on layer Notes
        ARMDIA1.PR_SetLayer("Notes")

        MANGLOVERight_bas.PR_CalcPolar(xyRightPalmThumb(1), aAngle, 0.25 - nFillet, xyTmp1)
        MANGLOVERight_bas.PR_DrawLine(xyRightPalmThumb(1), xyTmp1)
        MANGLOVERight_bas.PR_CalcPolar(xyTmp1, 270, nFillet, xyTmp2)
        MANGLOVERight_bas.PR_CalcPolar(xyTmp2, aAngle, nFillet, xyTmp3)
        MANGLOVERight_bas.PR_DrawArc(xyTmp2, xyTmp1, xyTmp3)
        xyKeep = xyTmp3

        MANGLOVERight_bas.PR_CalcPolar(xyRightPalmThumb(4), aAngle, 0.25 - nFillet, xyTmp1)
        MANGLOVERight_bas.PR_DrawLine(xyRightPalmThumb(4), xyTmp1)
        MANGLOVERight_bas.PR_CalcPolar(xyTmp1, 90, nFillet, xyTmp2)
        MANGLOVERight_bas.PR_CalcPolar(xyTmp2, aAngle, nFillet, xyTmp3)
        If MANGLOVERight_bas.g_sSide = "Right" Then
            MANGLOVERight_bas.PR_DrawArc(xyTmp2, xyTmp3, xyTmp1)
        Else
            MANGLOVERight_bas.PR_DrawArc(xyTmp2, xyTmp1, xyTmp3)
        End If
        MANGLOVERight_bas.PR_DrawLine(xyKeep, xyTmp3)

        MANGLOVERight_bas.PR_CalcMidPoint(xyKeep, xyTmp3, xyTmp1)
        MANGLOVERight_bas.PR_CalcPolar(xyTmp1, aAngle, 0.05, xyTmp1)
        MANGLOVERight_bas.g_nCurrTextAngle = aAngle - 90
        ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_BOTTOM_, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)
        MANGLOVERight_bas.PR_DrawText("EDGED", xyTmp1, 0.1, 0)
        MANGLOVERight_bas.g_nCurrTextAngle = 0
    End Sub

    Private Sub PR_DrawFingers(ByRef Digit As RightFinger)
        'Procedure to draw a RightFinger based on the data given in the structure
        'RightFinger
        'NB
        '    NOT USED TO DRAW A RIGHT_THUMB
        '
        ' Fingers are drawn vertically
        ' All dimensions assummed to be figured
        '
        '
        Static xyPt3, xyPt1, xyPt2, xyPt4 As MANGLOVERight_bas.XY
        Static xyPt7, xyPt5, xyPt6, xyPt8 As MANGLOVERight_bas.XY
        Static xyPt9, xyMid As MANGLOVERight_bas.XY
        Static nX, nY As Double
        Static nA, aTheta, nRadius As Double
        Static rTheta As Double
        Static nHt, nWidth As Double
        Static nTipWidth, nBulge As Double
        Static Sign As Short

        'Simple case for missing fingers
        'Don't draw anything
        If Digit.Len_Renamed <= 0 Then Exit Sub

        'Establish if handing is required
        Sign = 1
        If Digit.xyUlnar.X > Digit.xyRadial.X Then Sign = -1

        'Case for missing RIGHT_DIP
        If Digit.DIP <= 0 Then
            MANGLOVERight_bas.PR_MakeXY(xyPt1, Digit.xyRadial.X, Digit.WebHt + Digit.Len_Renamed)
            If Sign > 0 Then
                MANGLOVERight_bas.PR_CalcPolar(xyPt1, 180, Digit.PIP, xyPt2)
            Else
                MANGLOVERight_bas.PR_CalcPolar(xyPt1, 0, Digit.PIP, xyPt2)
            End If
            Select Case Digit.Tip
                Case 0, 10 'Closed tip (ON and OFF Fold)
                    nHt = MANGLOVERight_bas.FN_CalcLength(xyPt1, xyPt2) / 2 'Bug fix 04/02/97
                    If xyPt1.y - nHt <= Digit.WebHt Then '     "    "
                        xyPt1.y = Digit.WebHt '     "    "
                        xyPt2.y = xyPt1.y '     "    "
                    Else '     "    "
                        xyPt1.y = xyPt1.y - nHt '     "    "
                        xyPt2.y = xyPt1.y '     "    "
                    End If '     "    "
                    PR_StartPoly()
                    PR_AddVertex(Digit.xyRadial, 0)
                    PR_AddVertex(xyPt1, (Sign))
                    PR_AddVertex(xyPt2, 0)
                    PR_AddVertex(Digit.xyUlnar, 0)
                    PR_EndPoly()
                    Dim Polycurve As MANGLOVERight_bas.curve
                    Dim X(4), Y(4), Bulge(4) As Double
                    X(1) = Digit.xyRadial.X
                    Y(1) = Digit.xyRadial.y
                    Bulge(1) = 0
                    X(2) = xyPt1.X
                    Y(2) = xyPt1.y
                    Bulge(2) = Sign
                    X(3) = xyPt2.X
                    Y(3) = xyPt2.y
                    Bulge(3) = 0
                    X(4) = Digit.xyUlnar.X
                    Y(4) = Digit.xyUlnar.y
                    Bulge(4) = 0
                    Polycurve.n = 4
                    Polycurve.X = X
                    Polycurve.y = Y
                    MANGLOVERight_bas.PR_DrawPoly(Polycurve, Bulge)
                Case 1, 11, 2, 12 'Standard open tip & Desired length open tip (ON Fold)
                    'The cutback for standard open tips has been done in
                    'FN_CalculateGlove
                    PR_StartPoly()
                    PR_AddVertex(Digit.xyRadial, 0)
                    PR_AddVertex(xyPt1, 0)
                    PR_AddVertex(xyPt2, 0)
                    PR_AddVertex(Digit.xyUlnar, 0)
                    PR_EndPoly()
                    Dim Polycurve As MANGLOVERight_bas.curve
                    Dim X(4), Y(4) As Double
                    X(1) = Digit.xyRadial.X
                    Y(1) = Digit.xyRadial.y
                    X(2) = xyPt1.X
                    Y(2) = xyPt1.y
                    X(3) = xyPt2.X
                    Y(3) = xyPt2.y
                    X(4) = Digit.xyUlnar.X
                    Y(4) = Digit.xyUlnar.y
                    Polycurve.n = 4
                    Polycurve.X = X
                    Polycurve.y = Y
                    Dim Bulge(4), ii As Double
                    For ii = 1 To 4
                        Bulge(ii) = 0
                    Next ii
                    MANGLOVERight_bas.PR_DrawPoly(Polycurve, Bulge)
            End Select
        Else
            Select Case Digit.Tip
                Case 0 'Closed tip (OFF Fold)
                    nHt = Digit.Len_Renamed / 3
                    nTipWidth = Digit.DIP * 0.8
                    If nTipWidth < 0.375 Then nTipWidth = 0.375

                    'RIGHT_PIP
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, Digit.xyUlnar.X, Digit.WebHt + nHt)

                    'RIGHT_DIP
                    xyPt2.y = xyPt1.y + nHt
                    xyPt2.X = xyPt1.X + (Sign * (System.Math.Abs(Digit.PIP - Digit.DIP) / 2))

                    'At Tip
                    xyPt3.y = xyPt2.y + nHt
                    xyPt3.X = xyPt2.X + (Sign * (System.Math.Abs(nTipWidth - Digit.DIP) / 2))

                    'Find center of arc
                    rTheta = System.Math.Atan((System.Math.Abs(nTipWidth - Digit.DIP) / 2) / nHt)

                    aTheta = rTheta * (180 / MANGLOVERight_bas.PI)
                    rTheta = (rTheta + (MANGLOVERight_bas.PI / 2)) / 2
                    nA = (nTipWidth / 2) / System.Math.Cos(rTheta)
                    nRadius = nA * System.Math.Sin(rTheta)

                    If Sign > 0 Then
                        MANGLOVERight_bas.PR_CalcPolar(xyPt3, 315 - (aTheta / 2), nA, xyPt4)
                        MANGLOVERight_bas.PR_CalcPolar(xyPt4, 180 - aTheta, nRadius, xyPt5)
                        MANGLOVERight_bas.PR_CalcPolar(xyPt4, aTheta, nRadius, xyPt6)
                    Else
                        MANGLOVERight_bas.PR_CalcPolar(xyPt3, 225 + (aTheta / 2), nA, xyPt4)
                        MANGLOVERight_bas.PR_CalcPolar(xyPt4, aTheta, nRadius, xyPt5)
                        MANGLOVERight_bas.PR_CalcPolar(xyPt4, 180 - aTheta, nRadius, xyPt6)
                    End If

                    MANGLOVERight_bas.PR_MakeXY(xyPt7, xyPt3.X + (Sign * nTipWidth), xyPt3.y)
                    MANGLOVERight_bas.PR_MakeXY(xyPt8, xyPt2.X + (Sign * Digit.DIP), xyPt2.y)
                    MANGLOVERight_bas.PR_MakeXY(xyPt9, xyPt1.X + (Sign * Digit.PIP), xyPt1.y)

                    'Draw RightFinger
                    PR_StartPoly()
                    PR_AddVertex(Digit.xyUlnar, 0)
                    PR_AddVertex(xyPt1, 0)
                    PR_AddVertex(xyPt2, 0)
                    'Calculate bulge
                    nBulge = ((nRadius - System.Math.Abs(xyPt5.y - xyPt4.y)) / (MANGLOVERight_bas.FN_CalcLength(xyPt5, xyPt6) / 2)) * (Sign * -1)
                    PR_AddVertex(xyPt5, nBulge)
                    PR_AddVertex(xyPt6, 0)
                    PR_AddVertex(xyPt8, 0)
                    PR_AddVertex(xyPt9, 0)
                    PR_AddVertex(Digit.xyRadial, 0)
                    PR_EndPoly()
                    Dim Polycurve As MANGLOVERight_bas.curve
                    Dim X(8), Y(8), Bulge(8) As Double
                    X(1) = Digit.xyUlnar.X
                    Y(1) = Digit.xyUlnar.y
                    Bulge(1) = 0
                    X(2) = xyPt1.X
                    Y(2) = xyPt1.y
                    Bulge(2) = 0
                    X(3) = xyPt2.X
                    Y(3) = xyPt2.y
                    Bulge(3) = 0
                    X(4) = xyPt5.X
                    Y(4) = xyPt5.y
                    Bulge(4) = nBulge
                    X(5) = xyPt6.X
                    Y(5) = xyPt6.y
                    Bulge(5) = 0
                    X(6) = xyPt8.X
                    Y(6) = xyPt8.y
                    Bulge(6) = 0
                    X(7) = xyPt9.X
                    Y(7) = xyPt9.y
                    Bulge(7) = 0
                    X(8) = Digit.xyRadial.X
                    Y(8) = Digit.xyRadial.y
                    Bulge(8) = 0
                    Polycurve.n = 8
                    Polycurve.X = X
                    Polycurve.y = Y
                    MANGLOVERight_bas.PR_DrawPoly(Polycurve, Bulge)

                Case 1, 2 'Standard open tip & Desired length open tip (OFF Fold
                    'The cutback for standard open tips has been done in
                    'FN_CalculateGlove
                    nHt = Digit.Len_Renamed / 2

                    'RIGHT_PIP
                    ' xyPt1.x = Digit.xyUlnar.x
                    ' xyPt1.y = Digit.xyUlnar.y + nHt
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, Digit.xyUlnar.X, Digit.WebHt + nHt)

                    xyPt4 = xyPt1
                    xyPt4.X = xyPt1.X + (Sign * Digit.PIP)

                    'RIGHT_DIP
                    xyPt2.y = xyPt1.y + nHt
                    xyPt2.X = xyPt1.X + (Sign * (System.Math.Abs(Digit.PIP - Digit.DIP) / 2))

                    'UPGRADE_WARNING: Couldn't resolve default property of object xyPt3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    xyPt3 = xyPt2
                    xyPt3.X = xyPt2.X + (Sign * Digit.DIP)

                    'Draw RightFinger
                    PR_StartPoly()
                    PR_AddVertex(Digit.xyUlnar, 0)
                    PR_AddVertex(xyPt1, 0)
                    PR_AddVertex(xyPt2, 0)
                    PR_AddVertex(xyPt3, 0)
                    PR_AddVertex(xyPt4, 0)
                    PR_AddVertex(Digit.xyRadial, 0)
                    PR_EndPoly()
                    Dim Polycurve As MANGLOVERight_bas.curve
                    Dim X(6), Y(6) As Double
                    X(1) = Digit.xyUlnar.X
                    Y(1) = Digit.xyUlnar.y
                    X(2) = xyPt1.X
                    Y(2) = xyPt1.y
                    X(3) = xyPt2.X
                    Y(3) = xyPt2.y
                    X(4) = xyPt3.X
                    Y(4) = xyPt3.y
                    X(5) = xyPt4.X
                    Y(5) = xyPt4.y
                    X(6) = Digit.xyRadial.X
                    Y(6) = Digit.xyRadial.y
                    Polycurve.n = 6
                    Polycurve.X = X
                    Polycurve.y = Y
                    Dim Bulge(6), ii As Double
                    For ii = 1 To 6
                        Bulge(ii) = 0
                    Next ii
                    MANGLOVERight_bas.PR_DrawPoly(Polycurve, Bulge)

                Case 10 'Closed tip (ON Fold)
                    nHt = Digit.Len_Renamed / 3
                    nTipWidth = Digit.DIP * 0.8
                    If nTipWidth < 0.375 Then nTipWidth = 0.375

                    'RIGHT_PIP
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, Digit.xyUlnar.X, Digit.WebHt + nHt)

                    'RIGHT_DIP
                    xyPt2.y = xyPt1.y + nHt
                    xyPt2.X = Digit.xyUlnar.X

                    'At Tip
                    xyPt3.y = xyPt2.y + nHt
                    xyPt3.X = Digit.xyUlnar.X

                    'Center of arc
                    nRadius = nTipWidth / 2
                    MANGLOVERight_bas.PR_MakeXY(xyPt4, xyPt3.X + (Sign * nRadius), xyPt3.y - nRadius)

                    'Find Ends of arc
                    rTheta = System.Math.Atan(System.Math.Abs(nTipWidth - Digit.DIP) / nHt)
                    aTheta = rTheta * (180 / MANGLOVERight_bas.PI)

                    MANGLOVERight_bas.PR_MakeXY(xyPt5, xyPt3.X, xyPt3.y - nRadius)

                    If Sign > 0 Then
                        MANGLOVERight_bas.PR_CalcPolar(xyPt4, aTheta, nRadius, xyPt6)
                    Else
                        MANGLOVERight_bas.PR_CalcPolar(xyPt4, 180 - aTheta, nRadius, xyPt6)
                    End If

                    MANGLOVERight_bas.PR_MakeXY(xyPt7, xyPt3.X + (Sign * nTipWidth), xyPt3.y)
                    MANGLOVERight_bas.PR_MakeXY(xyPt8, xyPt2.X + (Sign * Digit.DIP), xyPt2.y)
                    MANGLOVERight_bas.PR_MakeXY(xyPt9, xyPt1.X + (Sign * Digit.PIP), xyPt1.y)

                    'Draw finger
                    PR_StartPoly()
                    PR_AddVertex(Digit.xyUlnar, 0)
                    PR_AddVertex(xyPt5, 0)
                    'Calculate bulge
                    ''------------------------------BDYUTILS.PR_CalcMidPoint(xyPt5, xyPt6, xyMid)
                    MANGLOVERight_bas.PR_CalcMidPoint(xyPt5, xyPt6, xyMid)
                    nBulge = ((nRadius - MANGLOVERight_bas.FN_CalcLength(xyMid, xyPt4)) / (MANGLOVERight_bas.FN_CalcLength(xyPt5, xyPt6) / 2)) * (Sign * -1)
                    PR_AddVertex(xyPt5, nBulge)
                    PR_AddVertex(xyPt6, 0)
                    PR_AddVertex(xyPt8, 0)
                    PR_AddVertex(xyPt9, 0)
                    PR_AddVertex(Digit.xyRadial, 0)
                    PR_EndPoly()
                    Dim Polycurve As MANGLOVERight_bas.curve
                    Dim X(7), Y(7), Bulge(7) As Double
                    X(1) = Digit.xyUlnar.X
                    Y(1) = Digit.xyUlnar.y
                    Bulge(1) = 0
                    X(2) = xyPt5.X
                    Y(2) = xyPt5.y
                    Bulge(2) = 0
                    X(3) = xyPt5.X  ''At bulge value - nBulge
                    Y(3) = xyPt5.y
                    Bulge(3) = nBulge
                    X(4) = xyPt6.X
                    Y(4) = xyPt6.y
                    Bulge(4) = 0
                    X(5) = xyPt8.X
                    Y(5) = xyPt8.y
                    Bulge(5) = 0
                    X(6) = xyPt9.X
                    Y(6) = xyPt9.y
                    Bulge(6) = 0
                    X(7) = Digit.xyRadial.X
                    Y(7) = Digit.xyRadial.y
                    Bulge(7) = 0
                    Polycurve.n = 7
                    Polycurve.X = X
                    Polycurve.y = Y
                    MANGLOVERight_bas.PR_DrawPoly(Polycurve, Bulge)

                Case 11, 12 'Standard open tip & Desired length open tip (ON Fold)
                    'The cutback for standard open tips has been done in
                    'FN_CalculateGlove
                    nHt = Digit.Len_Renamed / 2

                    'RIGHT_PIP
                    MANGLOVERight_bas.PR_MakeXY(xyPt1, Digit.xyUlnar.X, Digit.WebHt + nHt)

                    xyPt4 = xyPt1
                    xyPt4.X = xyPt1.X + (Sign * Digit.PIP)

                    'RIGHT_DIP
                    xyPt2.y = xyPt1.y + nHt
                    xyPt2.X = xyPt1.X

                    xyPt3 = xyPt2
                    xyPt3.X = xyPt2.X + (Sign * Digit.DIP)

                    'Draw RightFinger
                    PR_StartPoly()
                    PR_AddVertex(Digit.xyUlnar, 0)
                    PR_AddVertex(xyPt2, 0)
                    PR_AddVertex(xyPt3, 0)
                    PR_AddVertex(xyPt4, 0)
                    PR_AddVertex(Digit.xyRadial, 0)
                    PR_EndPoly()
                    Dim Polycurve As MANGLOVERight_bas.curve
                    Dim X(5), Y(5) As Double
                    X(1) = Digit.xyUlnar.X
                    Y(1) = Digit.xyUlnar.y
                    X(2) = xyPt2.X
                    Y(2) = xyPt2.y
                    X(3) = xyPt3.X
                    Y(3) = xyPt3.y
                    X(4) = xyPt4.X
                    Y(4) = xyPt4.y
                    X(5) = Digit.xyRadial.X
                    Y(5) = Digit.xyRadial.y
                    Polycurve.n = 5
                    Polycurve.X = X
                    Polycurve.y = Y
                    Dim Bulge(5), ii As Double
                    For ii = 1 To 5
                        Bulge(ii) = 0
                    Next ii
                    MANGLOVERight_bas.PR_DrawPoly(Polycurve, Bulge)
            End Select
        End If
    End Sub

    Private Sub PR_DrawMissing(ByRef iFingers As Short, ByRef iTip As Short, ByRef xy1 As MANGLOVERight_bas.XY, ByRef xy2 As MANGLOVERight_bas.XY, ByRef xy3 As MANGLOVERight_bas.XY, ByRef xy4 As MANGLOVERight_bas.XY)

        Dim xyMidPoint As MANGLOVERight_bas.XY
        ARMDIA1.PR_SetLayer("Template" & MANGLOVERight_bas.g_sSide)

        Select Case iFingers
            Case 1
                MANGLOVERight_bas.PR_CalcMidPoint(xy1, xy2, xyMidPoint)
                ''------------------------------BDYUTILS.PR_StartPoly()
                ''------------------------------BDYUTILS.PR_AddVertex(xy1, 0)
                ''------------------------------BDYUTILS.PR_AddVertex(xyMidPoint, 0)
                ''------------------------------BDYUTILS.PR_AddVertex(xy2, 0)
                ''------------------------------BDYUTILS.PR_EndPoly()
            Case 2
                ''------------------------------BDYUTILS.PR_StartPoly()
                ''------------------------------ BDYUTILS.PR_AddVertex(xy1, 0)
                ''------------------------------BDYUTILS.PR_AddVertex(xy2, 0)
                ''------------------------------BDYUTILS.PR_AddVertex(xy3, 0)
                ''------------------------------BDYUTILS.PR_EndPoly()
                xyMidPoint = xy2
            Case 3
                ''------------------------------BDYUTILS.PR_StartPoly()
                ''------------------------------BDYUTILS.PR_AddVertex(xy1, 0)
                ''------------------------------BDYUTILS.PR_AddVertex(xy2, 0)
                ''------------------------------BDYUTILS.PR_AddVertex(xy3, 0)
                ''------------------------------BDYUTILS.PR_AddVertex(xy4, 0)
                ''------------------------------BDYUTILS.PR_EndPoly()
                MANGLOVERight_bas.PR_CalcMidPoint(xy2, xy3, xyMidPoint)
            Case Is < 1
                'Add note only
                MANGLOVERight_bas.PR_CalcMidPoint(xy1, xy2, xyMidPoint)
                xyMidPoint.y = xyMidPoint.y - (RIGHT_EIGHTH * 1)
            Case Else
                Exit Sub
        End Select

        ARMDIA1.PR_SetLayer("Notes")
        xyMidPoint.y = xyMidPoint.y - (RIGHT_EIGHTH * 2)
        If iTip = 0 Or iTip = 10 Then
            MANGLOVERight_bas.PR_DrawText("CLOSED", xyMidPoint, 0.1, 0)
        Else
            MANGLOVERight_bas.PR_DrawText("OPEN", xyMidPoint, 0.1, 0)
        End If

    End Sub

    Private Sub PR_DrawMissingFingers()
        Dim ClosedLittle, iMissingFingers, ClosedIndex As Short
        Dim ii As Short
        Dim xyNull As MANGLOVERight_bas.XY

        If g_RightFusedFingers Then Exit Sub

        ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_VERT_CENTER, RIGHT_CURRENT, RIGHT_CURRENT, RIGHT_CURRENT)

        'Check for missing fingers
        iMissingFingers = 0
        For ii = 0 To 3
            If g_RightFinger(ii + 1).Len_Renamed <= 0 Then iMissingFingers = iMissingFingers + (2 ^ ii)
        Next ii

        ClosedLittle = False
        ClosedIndex = False
        If g_RightFinger(1).Tip = 0 Or g_RightFinger(1).Tip = 10 Then ClosedLittle = True
        If g_RightFinger(4).Tip = 0 Or g_RightFinger(4).Tip = 10 Then ClosedIndex = True

        Select Case iMissingFingers
            'NB -1 => text only
            Case 1
                If ClosedLittle Then
                    PR_DrawMissing(-1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                End If
            Case 2
                PR_DrawMissing(1, g_RightFinger(2).Tip, xyRightLittle, xyRightRing, xyNull, xyNull)
            Case 3
                If ClosedLittle Then
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLittle, xyRightRing, xyNull, xyNull)
                Else
                    PR_DrawMissing(2, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyRightRing, xyNull)
                End If
            Case 4
                PR_DrawMissing(1, g_RightFinger(3).Tip, xyRightRing, xyRightMiddle, xyNull, xyNull)
            Case 5
                If ClosedLittle Then
                    PR_DrawMissing(-1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                End If
                PR_DrawMissing(1, g_RightFinger(3).Tip, xyRightRing, xyRightMiddle, xyNull, xyNull)
            Case 6
                PR_DrawMissing(2, g_RightFinger(2).Tip, xyRightLittle, xyRightRing, xyRightMiddle, xyNull)
            Case 7
                If ClosedLittle Then
                    PR_DrawMissing(2, g_RightFinger(1).Tip, xyRightLittle, xyRightRing, xyRightMiddle, xyNull)
                Else
                    PR_DrawMissing(3, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyRightRing, xyRightMiddle)
                End If
            Case 8
                If ClosedIndex Then
                    PR_DrawMissing(-1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                End If
            Case 9
                If ClosedLittle Then
                    PR_DrawMissing(-1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                End If
                If ClosedIndex Then
                    PR_DrawMissing(-1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                End If
            Case 10
                PR_DrawMissing(1, g_RightFinger(2).Tip, xyRightLittle, xyRightRing, xyNull, xyNull)
                If ClosedIndex Then
                    PR_DrawMissing(-1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                End If
            Case 11
                If ClosedLittle Then
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLittle, xyRightRing, xyNull, xyNull)
                Else
                    PR_DrawMissing(2, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyRightRing, xyNull)
                End If
                If ClosedIndex Then
                    PR_DrawMissing(-1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(4).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                End If
            Case 12
                If ClosedIndex Then
                    PR_DrawMissing(1, g_RightFinger(3).Tip, xyRightRing, xyRightMiddle, xyNull, xyNull)
                Else
                    PR_DrawMissing(2, g_RightFinger(3).Tip, xyRightRing, xyRightMiddle, xyRightIndex, xyNull)
                End If
            Case 13
                If ClosedLittle Then
                    PR_DrawMissing(-1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                Else
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                End If
                If ClosedIndex Then
                    PR_DrawMissing(1, g_RightFinger(3).Tip, xyRightRing, xyRightMiddle, xyNull, xyNull)
                Else
                    PR_DrawMissing(2, g_RightFinger(3).Tip, xyRightRing, xyRightMiddle, xyRightIndex, xyNull)
                End If
            Case 14
                If ClosedIndex Then
                    PR_DrawMissing(2, g_RightFinger(2).Tip, xyRightLittle, xyRightRing, xyRightMiddle, xyNull)
                Else
                    PR_DrawMissing(3, g_RightFinger(3).Tip, xyRightLittle, xyRightRing, xyRightMiddle, xyRightIndex)
                End If
            Case 15
                If ClosedIndex And ClosedLittle Then
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLittle, xyRightMiddle, xyNull, xyNull)
                ElseIf ClosedLittle Then
                    PR_DrawMissing(-1, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyNull, xyNull)
                    PR_DrawMissing(3, g_RightFinger(1).Tip, xyRightLittle, xyRightRing, xyRightMiddle, xyRightIndex)
                ElseIf ClosedIndex Then
                    PR_DrawMissing(-1, g_RightFinger(1).Tip, xyRightMiddle, xyRightIndex, xyNull, xyNull)
                    PR_DrawMissing(3, g_RightFinger(1).Tip, xyRightLFS, xyRightLittle, xyRightRing, xyRightMiddle)
                Else
                    PR_DrawMissing(1, g_RightFinger(1).Tip, xyRightLFS, xyRightIndex, xyNull, xyNull)
                End If
            Case Else
                '=> No missing fingers so "DO NOTHING"
        End Select

    End Sub

    Private Sub PR_DrawNormalThumb(ByRef nInsert As Double)
        'UPGRADE_WARNING: Lower bound of array xyRightT was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim xyRightT(12)
        Static xyTmpStart, xyTmp, xyTmpEnd, xyTranslate As MANGLOVERight_bas.XY
        Static xyThumbPalm5 As MANGLOVERight_bas.XY
        Static aAngle, nLength, aThumbAngle As Double
        Static nJ_Radius, nSeam, nInc As Double
        Static nSlantInsert, nThumbSlantLength As Double
        Static aPalmThumbStart, aPalmThumbEnd As Double
        Static ThumbTopCurve, ThumbBottCurve As RightBiArc
        Static iThumbDirection, ii, iAngleDirection, Success As Short
        Static nFiguredPalm, nMinDist, nOffSet, nBulge As Double
        Static sText As String

        'Globals
        '     g_RightThumbTopCurve, from FN_CalculateGlove
        '

        'Angles
        aPalmThumbStart = MANGLOVERight_bas.FN_CalcAngle(xyRightPalmThumb(4), xyRightPalmThumb(3)) + 20
        aPalmThumbEnd = 0
        iAngleDirection = 1
        iThumbDirection = -1
        nOffSet = 3

        'Mirror Thumb angles for right side
        If MANGLOVERight_bas.g_sSide = "Right" Then
            'Angles
            aPalmThumbStart = aPalmThumbStart - 40
            aPalmThumbEnd = 180
            iAngleDirection = -1
            iThumbDirection = 1
            nOffSet = 6
        End If

        nInc = 0.03125 '1/32nd of an inch

        'Thumb curve on palm
        ARMDIA1.PR_SetLayer("Notes")

        'BOTTOM Curve
        'We find by iteration the first curve that fits the data.  We change the
        'start angle as part of the iteration
        Success = False
        While Not Success And (aPalmThumbStart < 180 Or aPalmThumbStart > 0)
            ii = MANGLOVERight_bas.FN_RightBiArcCurve(xyRightPalmThumb(4), aPalmThumbStart, xyRightPalmThumb(3), 90, ThumbBottCurve)
            If ii Then
                Success = True
            Else
                'Adjust and try again
                aPalmThumbStart = aPalmThumbStart + (iAngleDirection * 1)
            End If
        End While

        If Not Success Then
            MsgBox("Can't form BOTTOM part of thumb curve on palm! Adjust the palm curve and use the Transfer tool to update the thumb", 48, g_sDialogID)
            'Degenerate "ThumbBottCurve" to a straight line
            ThumbBottCurve.xyStart = xyRightPalmThumb(4)
            ThumbBottCurve.xyEnd = xyRightPalmThumb(3)
            ThumbBottCurve.nR1 = 0
            ThumbBottCurve.nR2 = 0
        End If

        'Draw thumb curve on palm
        PR_DrawThumbCurve(ThumbBottCurve, g_RightThumbTopCurve)
        MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "ThumbPalmCurve")

        '    PR_DrawLine xyRightPalmThumb(5), xyRightPalmThumb(1)
        MANGLOVERight_bas.PR_DrawLine(g_RightThumbTopCurve.xyEnd, xyRightPalmThumb(1))

        'Draw the actual thumb
        'Translate thumb curves
        nSeam = 0.125
        nJ_Radius = 0.125

        ThumbBottCurve.xyR1.X = ThumbBottCurve.xyR1.X + nOffSet
        ThumbBottCurve.xyR2.X = ThumbBottCurve.xyR2.X + nOffSet
        ThumbBottCurve.xyTangent.X = ThumbBottCurve.xyTangent.X + nOffSet
        ThumbBottCurve.xyEnd.X = ThumbBottCurve.xyEnd.X + nOffSet
        ThumbBottCurve.xyStart.X = ThumbBottCurve.xyStart.X + nOffSet
        g_RightThumbTopCurve.xyR1.X = g_RightThumbTopCurve.xyR1.X + nOffSet
        g_RightThumbTopCurve.xyR2.X = g_RightThumbTopCurve.xyR2.X + nOffSet
        g_RightThumbTopCurve.xyTangent.X = g_RightThumbTopCurve.xyTangent.X + nOffSet
        g_RightThumbTopCurve.xyEnd.X = g_RightThumbTopCurve.xyEnd.X + nOffSet
        g_RightThumbTopCurve.xyStart.X = g_RightThumbTopCurve.xyStart.X + nOffSet

        'Draw thumb curve (Seam line)
        PR_DrawThumbCurve(ThumbBottCurve, g_RightThumbTopCurve)
        MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "ThumbPieceSeam")

        aAngle = MANGLOVERight_bas.FN_CalcAngle(xyRightPalmThumb(4), xyRightPalmThumb(2))
        aThumbAngle = aAngle + (iThumbDirection * 45)
        'UPGRADE_WARNING: Couldn't resolve default property of object xyRightT(1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        xyRightT(1) = ThumbBottCurve.xyStart

        nLength = g_RightFinger(5).PIP / (1 / System.Math.Sqrt(2))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(1), aAngle, nLength, xyRightT(4))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(4), aThumbAngle, g_RightFinger(5).Len_Renamed, xyRightT(3))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(3), aThumbAngle + (iThumbDirection * 90), g_RightFinger(5).PIP, xyRightT(2))

        nLength = nJ_Radius / System.Math.Tan((45 / 2) * (MANGLOVERight_bas.PI / 180))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(4), aThumbAngle, nLength, xyRightT(5))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(4), aAngle, nLength, xyRightT(6))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(6), aAngle + (iThumbDirection * 90), nJ_Radius, xyRightT(7))

        nLength = MANGLOVERight_bas.FN_CalcLength(xyRightPalmThumb(4), xyRightPalmThumb(2))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(1), aAngle, nLength, xyRightT(10))
        MANGLOVERight_bas.PR_DrawLine(g_RightThumbTopCurve.xyEnd, xyRightT(10))

        nLength = nSeam / System.Math.Abs(System.Math.Sin(aAngle * (MANGLOVERight_bas.PI / 180)))
        nLength = MANGLOVERight_bas.FN_CalcLength(xyRightPalmThumb(4), xyRightPalmThumb(2)) + nLength
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(1), aAngle, nLength, xyRightT(8))

        MANGLOVERight_bas.PR_CalcPolar(xyRightT(1), aThumbAngle + 180, 1, xyTmp)
        ii = MANGLOVERight_bas.FN_CirLinInt(xyRightT(1), xyTmp, ThumbBottCurve.xyR1, ThumbBottCurve.nR1 + nSeam, xyRightT(9))

        MANGLOVERight_bas.PR_DrawLine(xyRightT(6), xyRightT(1))

        'Offset line
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(6), aAngle + (iAngleDirection * 90), 0.25, xyTmp)
        MANGLOVERight_bas.PR_CalcPolar(xyTmp, aAngle, 10, xyRightT(11))
        If Not MANGLOVERight_bas.FN_CirLinInt(xyRightT(11), xyTmp, g_RightThumbTopCurve.xyR2, g_RightThumbTopCurve.nR2 + nSeam, xyRightT(11)) Then
            'make a best guess
            MANGLOVERight_bas.PR_CalcPolar(xyRightT(8), aAngle + (iAngleDirection * 90), 0.25, xyRightT(11))
        End If

        MANGLOVERight_bas.PR_CalcPolar(xyTmp, aAngle + 180, 10, xyRightT(12))
        If Not MANGLOVERight_bas.FN_CirLinInt(xyRightT(12), xyTmp, ThumbBottCurve.xyR1, ThumbBottCurve.nR1 + nSeam, xyRightT(12)) Then
            'make a best guess
            MANGLOVERight_bas.PR_CalcPolar(xyRightT(1), aAngle + (iAngleDirection * 90), 0.25, xyRightT(12))
        End If
        MANGLOVERight_bas.PR_DrawLine(xyRightT(11), xyRightT(12))

        'Draw Text
        MANGLOVERight_bas.PR_CalcMidPoint(xyRightT(1), xyRightT(2), xyTmp)

        MANGLOVERight_bas.PR_CalcPolar(xyTmp, aThumbAngle + (iAngleDirection * 90), g_RightFinger(5).PIP * 2 / 3, xyTmp)
        ARMDIA1.PR_SetTextData(RIGHT_HORIZ_CENTER, RIGHT_VERT_CENTER, RIGHT_EIGHTH, RIGHT_CURRENT, RIGHT_CURRENT)
        If MANGLOVERight_bas.g_sSide = "Left" Then
            MANGLOVERight_bas.g_nCurrTextAngle = aThumbAngle
        Else
            MANGLOVERight_bas.g_nCurrTextAngle = 180 + aThumbAngle
        End If
        sText = txtSide.Text & Chr(10) & txtPatientName.Text & Chr(10) & txtWorkOrder.Text
        ''--------MANGLOVERight_bas.PR_DrawText(sText, xyTmp, 0.1, (MANGLOVERight_bas.g_nCurrTextAngle * (MANGLOVERight_bas.PI / 180)))
        MANGLOVERight_bas.PR_DrawMText(sText, xyTmp, (MANGLOVERight_bas.g_nCurrTextAngle * (MANGLOVERight_bas.PI / 180)))
        MANGLOVERight_bas.g_nCurrTextAngle = 0

        'Draw Outside profile
        ARMDIA1.PR_SetLayer("Template" & MANGLOVERight_bas.g_sSide)
        ''------------------------------MANGLOVERight_bas.PR_StartPoly()
        ''------------------------------MANGLOVERight_bas.PR_AddVertex(xyRightT(9), 0)
        Dim Polycurve As MANGLOVERight_bas.curve
        Dim X(6), Y(6), Bulge(6) As Double
        X(1) = xyRightT(9).X
        Y(1) = xyRightT(9).y
        Bulge(1) = 0

        'Draw tip
        If g_RightFinger(5).Tip = 0 Then
            'Closed tip
            MANGLOVERight_bas.PR_CalcPolar(xyRightT(2), aThumbAngle + 180, g_RightFinger(5).PIP / 2, xyRightT(2))
            MANGLOVERight_bas.PR_CalcPolar(xyRightT(3), aThumbAngle + 180, g_RightFinger(5).PIP / 2, xyRightT(3))
            MANGLOVERight_bas.PR_CalcMidPoint(xyRightT(2), xyRightT(3), xyTmp)
            nBulge = 1 * (iThumbDirection * -1)
        Else
            'Open tip
            nBulge = 0
        End If

        ''------------------------------MANGLOVERight_bas.PR_AddVertex(xyRightT(2), nBulge)
        ''------------------------------MANGLOVERight_bas.PR_AddVertex(xyRightT(3), 0)
        X(2) = xyRightT(2).X
        Y(2) = xyRightT(2).y
        Bulge(2) = nBulge
        X(3) = xyRightT(3).X
        Y(3) = xyRightT(3).y
        Bulge(3) = 0
        'Calculate bulge
        MANGLOVERight_bas.PR_CalcMidPoint(xyRightT(5), xyRightT(6), xyTmp)
        nBulge = ((nJ_Radius - MANGLOVERight_bas.FN_CalcLength(xyTmp, xyRightT(7))) / (MANGLOVERight_bas.FN_CalcLength(xyRightT(5), xyRightT(6)) / 2)) * iThumbDirection
        ''------------------------------ MANGLOVERight_bas.PR_AddVertex(xyRightT(5), nBulge)

        ''------------------------------MANGLOVERight_bas.PR_AddVertex(xyRightT(6), 0)
        ''------------------------------MANGLOVERight_bas.PR_AddVertex(xyRightT(8), 0)

        ''------------------------------MANGLOVERight_bas.PR_EndPoly()
        X(4) = xyRightT(5).X
        Y(4) = xyRightT(5).y
        Bulge(4) = nBulge
        X(5) = xyRightT(6).X
        Y(5) = xyRightT(6).y
        Bulge(5) = 0
        X(6) = xyRightT(8).X
        Y(6) = xyRightT(8).y
        Bulge(6) = 0
        Polycurve.n = 6
        Polycurve.X = X
        Polycurve.y = Y
        MANGLOVERight_bas.PR_DrawPoly(Polycurve, Bulge)

        If ThumbBottCurve.nR1 > 0 And ThumbBottCurve.nR1 > 0 Then
            'Offset curves
            ThumbBottCurve.nR1 = ThumbBottCurve.nR1 + nSeam
            ThumbBottCurve.nR2 = ThumbBottCurve.nR2 + nSeam
            aAngle = MANGLOVERight_bas.FN_CalcAngle(ThumbBottCurve.xyR1, ThumbBottCurve.xyStart)
            MANGLOVERight_bas.PR_CalcPolar(ThumbBottCurve.xyR1, aAngle, ThumbBottCurve.nR1, ThumbBottCurve.xyStart)
            aAngle = MANGLOVERight_bas.FN_CalcAngle(ThumbBottCurve.xyR1, ThumbBottCurve.xyTangent)
            MANGLOVERight_bas.PR_CalcPolar(ThumbBottCurve.xyR1, aAngle, ThumbBottCurve.nR1, ThumbBottCurve.xyTangent)
            aAngle = MANGLOVERight_bas.FN_CalcAngle(ThumbBottCurve.xyR2, ThumbBottCurve.xyEnd)
            MANGLOVERight_bas.PR_CalcPolar(ThumbBottCurve.xyR2, aAngle, ThumbBottCurve.nR2, ThumbBottCurve.xyEnd)
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object ThumbBottCurve.xyStart. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ThumbBottCurve.xyStart = xyRightT(9)
            ThumbBottCurve.xyEnd.X = ThumbBottCurve.xyEnd.X + (iThumbDirection * nSeam)
        End If

        If g_RightThumbTopCurve.nR1 > 0 And g_RightThumbTopCurve.nR1 > 0 Then
            g_RightThumbTopCurve.nR1 = g_RightThumbTopCurve.nR1 + nSeam
            g_RightThumbTopCurve.nR2 = g_RightThumbTopCurve.nR2 + nSeam
            aAngle = MANGLOVERight_bas.FN_CalcAngle(g_RightThumbTopCurve.xyR1, g_RightThumbTopCurve.xyStart)
            MANGLOVERight_bas.PR_CalcPolar(g_RightThumbTopCurve.xyR1, aAngle, g_RightThumbTopCurve.nR1, g_RightThumbTopCurve.xyStart)
            aAngle = MANGLOVERight_bas.FN_CalcAngle(g_RightThumbTopCurve.xyR1, g_RightThumbTopCurve.xyTangent)
            MANGLOVERight_bas.PR_CalcPolar(g_RightThumbTopCurve.xyR1, aAngle, g_RightThumbTopCurve.nR1, g_RightThumbTopCurve.xyTangent)
            aAngle = MANGLOVERight_bas.FN_CalcAngle(g_RightThumbTopCurve.xyR2, g_RightThumbTopCurve.xyEnd)
            MANGLOVERight_bas.PR_CalcPolar(g_RightThumbTopCurve.xyR2, aAngle, g_RightThumbTopCurve.nR2, g_RightThumbTopCurve.xyEnd)
        Else
            g_RightThumbTopCurve.xyEnd = xyRightT(8)
            g_RightThumbTopCurve.xyStart = ThumbBottCurve.xyEnd
        End If

        'Draw thumb curve
        PR_DrawThumbCurve(ThumbBottCurve, g_RightThumbTopCurve)
        MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "ThumbPieceOutsideEdge")
        MANGLOVERight_bas.PR_DrawLine(g_RightThumbTopCurve.xyEnd, xyRightT(8))
    End Sub

    Private Sub PR_DrawSetInThumb(ByRef nInsert As Object)
        'Procedure to draw a Radial Set In thumb

        ReDim xyRightT(12)
        Dim xyTmpStart, xyTmp, xyTmpEnd, xyTranslate As MANGLOVERight_bas.XY
        Dim aAngle, nLength, aThumbAngle As Double
        Dim nSeam, nJ_Radius As Double
        Dim nSlantInsert, nThumbSlantLength As Double
        Dim aPalmThumbStart, aPalmThumbEnd As Double
        Dim ThumbTopCurve, ThumbBottCurve As RightBiArc
        Dim iThumbDirection, ii, iAngleDirection, Success As Short
        Dim nMinDist, nOffSet As Double

        'Angles
        iAngleDirection = 1
        iThumbDirection = -1
        aThumbAngle = 45

        'Mirror Thumb angles for right side
        If MANGLOVERight_bas.g_sSide = "Right" Then
            'Angles
            iAngleDirection = -1
            iThumbDirection = 1
            aThumbAngle = 135
        End If

        'Calculate thumb points
        'Draw profile
        ARMDIA1.PR_SetLayer("Template" & MANGLOVERight_bas.g_sSide)
        xyRightT(4) = xyRightThumb
        xyRightT(1) = xyRightThumb
        xyRightT(1).y = xyRightThumb.y - (System.Math.Sqrt(2) * g_RightFinger(5).PIP)

        MANGLOVERight_bas.PR_CalcPolar(xyRightT(4), aThumbAngle, g_RightFinger(5).Len_Renamed, xyRightT(3))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(3), aThumbAngle + (iThumbDirection * 90), g_RightFinger(5).PIP, xyRightT(2))

        nJ_Radius = 0.125
        nLength = nJ_Radius * System.Math.Tan((22.5) * (MANGLOVERight_bas.PI / 180))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(4), aThumbAngle, nJ_Radius, xyRightT(5))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(4), 90, nJ_Radius, xyRightT(6))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(6), 90 + (iThumbDirection * 90), nLength, xyRightT(7))

        nJ_Radius = 0.25
        nLength = nJ_Radius * System.Math.Cos((67.5) * (MANGLOVERight_bas.PI / 180))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(1), aThumbAngle, nLength, xyRightT(8))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(1), 270, nLength, xyRightT(9))
        MANGLOVERight_bas.PR_CalcPolar(xyRightT(9), 90 + (iThumbDirection * 90), nJ_Radius, xyRightT(10))

        'Draw tip
        If g_RightFinger(5).Tip = 0 Then
            'Closed tip
            MANGLOVERight_bas.PR_CalcPolar(xyRightT(2), aThumbAngle + 180, g_RightFinger(5).PIP / 2, xyRightT(2))
            MANGLOVERight_bas.PR_CalcPolar(xyRightT(3), aThumbAngle + 180, g_RightFinger(5).PIP / 2, xyRightT(3))
            MANGLOVERight_bas.PR_CalcMidPoint(xyRightT(2), xyRightT(3), xyTmp)
            MANGLOVERight_bas.PR_DrawArc(xyTmp, xyRightT(2), xyRightT(3))
        Else
            'Open tip
            MANGLOVERight_bas.PR_DrawLine(xyRightT(2), xyRightT(3))
        End If

        MANGLOVERight_bas.PR_DrawLine(xyRightT(8), xyRightT(2))
        MANGLOVERight_bas.PR_DrawLine(xyRightT(3), xyRightT(5))
        MANGLOVERight_bas.PR_DrawArc(xyRightT(7), xyRightT(6), xyRightT(5))
        MANGLOVERight_bas.PR_DrawArc(xyRightT(10), xyRightT(8), xyRightT(9))
    End Sub

    Private Sub PR_DrawTapeNotes(ByRef iVertex As Short, ByRef xyStart As MANGLOVERight_bas.XY, ByRef xyEnd As MANGLOVERight_bas.XY, ByRef nOffSet As Double, ByRef sZipperID As String, ByVal nIndex As Short)
        'Draws the the notes at ecah tape
        Dim xyRightDatum, xyInsert As MANGLOVERight_bas.XY
        Dim nLength, aAngle, nDec As Double
        Dim sText As String
        Dim iInt As Short

        'Exit from sub if no Tape position Given as
        'this implies that the RightTapeNote is Empty
        '(EG with RIGHT_GLOVE_NORMAL)
        If RightTapeNote(iVertex).iTapePos = 0 Then Exit Sub

        'Draw Construction line
        ARMDIA1.PR_SetLayer("Construct")
        If g_RightExtendTo <> GLOVE_NORMAL Then
            MANGLOVERight_bas.PR_DrawLine(xyStart, xyEnd)

            If sZipperID <> "" Then
                If sZipperID <> "EOS" Then MANGLOVERight_bas.PR_AddXDataValToLast("curvetype", "EOS")
                MANGLOVERight_bas.PR_AddXDataValToLast("Zipper", sZipperID)
            End If
        End If

        'Get center point on which text positions are based
        'Doing it this way accounts for case when xyStart.x > xyEnd.x
        If nOffSet > 0 Then
            aAngle = MANGLOVERight_bas.FN_CalcAngle(xyStart, xyEnd)
            MANGLOVERight_bas.PR_CalcPolar(xyStart, aAngle, nOffSet, xyRightDatum)
        Else
            MANGLOVERight_bas.PR_CalcMidPoint(xyStart, xyEnd, xyRightDatum)
        End If

        'Insert Symbol
        ''---------------------ARMDIA1.PR_InsertSymbol("glvTapeNotes", xyRightDatum, 1, 0)

        'Setup for Tape Lable
        ''MANGLOVERight_bas.PR_AddXDataValToLast("Data", RightTapeNote(iVertex).sTapeText)

        'MMs
        '' MANGLOVERight_bas.PR_AddXDataValToLast("MM", Trim(Str(RightTapeNote(iVertex).iMMs)) & "mm")

        'Grams
        ''MANGLOVERight_bas.PR_AddXDataValToLast("Grams", Trim(Str(RightTapeNote(iVertex).iGms)) & "gm")

        'Reduction
        ''MANGLOVERight_bas.PR_AddXDataValToLast("Reduction", Trim(Str(RightTapeNote(iVertex).iRed)))

        'Circumference  in Inches and Eighths format
        Dim dValue As Double = RightTapeNote(iVertex).nCir
        Dim nInchToCM As Double = 1
        Dim nDecFromText As Double = 0
        If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
            nInchToCM = 2.54
            ''dValue = Val(txtExtCir(iVertex + 7).Text)            
            ''dValue = dValue * nInchToCM
            ''Changed for #224 in the issue list
            If nIndex + 7 <= 23 Then
                dValue = Val(txtExtCir(nIndex + 7).Text)
                ''Added for #196 in the issue list
                If txtExtCir(nIndex + 7).Text.Contains(".") Then
                    Dim strText As String = txtExtCir(nIndex + 7).Text
                    strText = Mid(strText, InStr(strText, ".") + 1)
                    nDecFromText = Val(strText)
                End If
            Else
                dValue = RightTapeNote(iVertex).nCir * nInchToCM
            End If
        End If
            ''iInt = Int(RightTapeNote(iVertex).nCir * nInchToCM)
            iInt = Int(dValue)
        '' MANGLOVERight_bas.PR_AddXDataValToLast("TapeLengths", Trim(Str(iInt)))
        Dim strTapeLen As String = Trim(Str(iInt))
        Dim bIsTapeLen2 As Boolean = False

        ''nDec = ((RightTapeNote(iVertex).nCir * nInchToCM) - iInt)
        nDec = (dValue - iInt)
        Dim isho As Short = dValue - iInt
        If nDec > 0 Then
            iInt = ARMDIA1.round((nDec / RIGHT_EIGHTH) * 10) / 10
            ''Added for #196 in the issue list
            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                If nDecFromText > 0 Then
                    iInt = nDecFromText
                End If
            End If
            If iInt <> 0 Then
                ''MANGLOVERight_bas.PR_AddXDataValToLast("TapeLengths2", Trim(Str(iInt)))
                bIsTapeLen2 = True
            End If
        End If

        If g_RightExtendTo <> GLOVE_NORMAL Then
            MANGLOVERight_bas.PR_DrawRightGlvTapeNotes(xyRightDatum, RightTapeNote(iVertex).sTapeText, Trim(Str(RightTapeNote(iVertex).iMMs)) & "mm", Trim(Str(RightTapeNote(iVertex).iGms)) & "gm",
                                      Trim(Str(RightTapeNote(iVertex).iRed)), strTapeLen, bIsTapeLen2, Trim(Str(iInt)))
            MANGLOVERight_bas.PR_AddXDataValToLast("ID", g_sFileNo + "Right")
        End If

        'Check for Notes
        If RightTapeNote(iVertex).sNote <> "" Then
            'set text justification etc.
            ARMDIA1.PR_SetTextData(RIGHT_RIGHT_, RIGHT_VERT_CENTER, RIGHT_EIGHTH, RIGHT_CURRENT, RIGHT_CURRENT)
            MANGLOVERight_bas.PR_MakeXY(xyInsert, xyRightDatum.X - 0.1, xyRightDatum.y + 0.35)
            If RightTapeNote(iVertex).sNote <> "PLEAT" Then ARMDIA1.PR_SetLayer("Notes")
            MANGLOVERight_bas.PR_DrawText(RightTapeNote(iVertex).sNote, xyInsert, 0.1, 0)
        End If
    End Sub

    Private Sub PR_DrawThumbCurve(ByRef BottCurve As RightBiArc, ByRef TopCurve As RightBiArc)
        'Procedure to draw the specific thumb curve as a fitted polyline.
        'Created to reduce code in PR_DrawThumb.
        '
        'N.B. (Very Carefully)
        '
        '    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        '    This is not a general procedure it is specific to the
        '    thumb case defined by the Two bi-arcs.  It is order
        '    and data dependant.
        '    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        '
        '
        'UPGRADE_WARNING: Arrays in structure Profile may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        Dim Profile As MANGLOVERight_bas.curve
        Dim ii, iVertex As Short
        Dim xyVertex As MANGLOVERight_bas.XY
        Dim aStep, aEnd, aFirst, aTangent, aDiff As Double

        iVertex = 1
        Dim nCount As Integer = 3
        If BottCurve.nR1 > 0 And BottCurve.nR2 > 0 Then
            nCount = nCount + 7
        End If
        If TopCurve.nR1 > 0 And TopCurve.nR2 > 0 Then
            nCount = nCount + 7
        End If
        Dim X(nCount), Y(nCount) As Double
        Profile.X = X
        Profile.y = Y
        Profile.X(iVertex) = BottCurve.xyStart.X
        Profile.y(iVertex) = BottCurve.xyStart.y

        If BottCurve.nR1 > 0 And BottCurve.nR2 > 0 Then

            aFirst = MANGLOVERight_bas.FN_CalcAngle(BottCurve.xyR1, BottCurve.xyStart)
            aTangent = MANGLOVERight_bas.FN_CalcAngle(BottCurve.xyR1, BottCurve.xyTangent)
            aEnd = MANGLOVERight_bas.FN_CalcAngle(BottCurve.xyR2, BottCurve.xyEnd)
            aStep = (aTangent - aFirst) / 4
            For ii = 1 To 3
                iVertex = iVertex + 1
                MANGLOVERight_bas.PR_CalcPolar(BottCurve.xyR1, aFirst + (ii * aStep), BottCurve.nR1, xyVertex)
                Profile.X(iVertex) = xyVertex.X
                Profile.y(iVertex) = xyVertex.y
            Next ii

            iVertex = iVertex + 1
            Profile.X(iVertex) = BottCurve.xyTangent.X
            Profile.y(iVertex) = BottCurve.xyTangent.y

            aDiff = aEnd - aTangent
            If System.Math.Abs(aDiff) >= 180 Then aDiff = 360 - System.Math.Abs(aDiff)
            aStep = aDiff / 4
            For ii = 1 To 3
                iVertex = iVertex + 1
                MANGLOVERight_bas.PR_CalcPolar(BottCurve.xyR2, aTangent + (ii * aStep), BottCurve.nR2, xyVertex)
                Profile.X(iVertex) = xyVertex.X
                Profile.y(iVertex) = xyVertex.y
            Next ii

        End If

        iVertex = iVertex + 1
        Profile.X(iVertex) = BottCurve.xyEnd.X
        Profile.y(iVertex) = BottCurve.xyEnd.y

        If TopCurve.nR1 > 0 And TopCurve.nR2 > 0 Then
            aFirst = MANGLOVERight_bas.FN_CalcAngle(TopCurve.xyR1, TopCurve.xyStart)
            aTangent = MANGLOVERight_bas.FN_CalcAngle(TopCurve.xyR1, TopCurve.xyTangent)
            aEnd = MANGLOVERight_bas.FN_CalcAngle(TopCurve.xyR2, TopCurve.xyEnd)

            If aFirst > 270 Then
                aStep = (aTangent + (360 - aFirst)) / 4
            Else
                aStep = (aTangent - aFirst) / 4
            End If
            For ii = 1 To 3
                iVertex = iVertex + 1
                MANGLOVERight_bas.PR_CalcPolar(TopCurve.xyR1, aFirst + (ii * aStep), TopCurve.nR1, xyVertex)
                Profile.X(iVertex) = xyVertex.X
                Profile.y(iVertex) = xyVertex.y
            Next ii

            iVertex = iVertex + 1
            Profile.X(iVertex) = TopCurve.xyTangent.X
            Profile.y(iVertex) = TopCurve.xyTangent.y

            aStep = (aEnd - aTangent) / 4
            For ii = 1 To 3
                iVertex = iVertex + 1
                MANGLOVERight_bas.PR_CalcPolar(TopCurve.xyR2, aTangent + (ii * aStep), TopCurve.nR2, xyVertex)
                Profile.X(iVertex) = xyVertex.X
                Profile.y(iVertex) = xyVertex.y
            Next ii

        End If

        iVertex = iVertex + 1
        Profile.X(iVertex) = TopCurve.xyEnd.X
        Profile.y(iVertex) = TopCurve.xyEnd.y

        Profile.n = iVertex
        MANGLOVERight_bas.PR_DrawFitted(Profile)
    End Sub

    Private Sub PR_EnableCir(ByRef Index As Short)
        'Read as part of labCir_DblClick
        labCir(Index).ForeColor = System.Drawing.ColorTranslator.FromOle(&H0)
        txtCir(Index).Enabled = True
        lblCir(Index).Enabled = True
    End Sub

    'Private Sub PR_GetComboListFromFile(ByRef Combo_Name As System.Windows.Forms.Control, ByRef sFileName As String)
    '    'General procedure to create the list section of
    '    'a combo box reading the data from a file

    '    Dim sLine As String
    '    Dim fFileNum As Short

    '    fFileNum = FreeFile()

    '    If FileLen(sFileName) = 0 Then
    '        MsgBox(sFileName & "Not found", 48, g_sDialogID)
    '        Exit Sub
    '    End If

    '    FileOpen(fFileNum, sFileName, OpenMode.Input)
    '    Do While Not EOF(fFileNum)
    '        sLine = LineInput(fFileNum)
    '        'UPGRADE_WARNING: Couldn't resolve default property of object Combo_Name.AddItem. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '        Combo_Name.AddItem(sLine)
    '    Loop
    '    FileClose(fFileNum)

    'End Sub

    Private Sub PR_GetDlgData()
        'Subroutine to extract from the Dialogue the data
        'putting it into global variables
        '
        'Common Dialogue elements
        ''-------If optFold(1).Checked = True Then g_RightOnFold = True Else g_RightOnFold = False

        'Get data from tabbed
        PR_GetDlgHandData()
        PR_GetDlgAboveWrist()
        If optFold(1).Checked = True Then g_RightOnFold = True Else g_RightOnFold = False
    End Sub

    Private Sub PR_GetDlgHandData()
        'Procedure to setup Module level variables based on the data in the
        'dialogue
        'Save having to do this more than once

        Dim ii As Short

        ''---------------If CType(MANGLOVERight_bas.MainForm.Controls("SSTab1"), Object).Tab <> 0 Then CType(MANGLOVERight_bas.MainForm.Controls("SSTab1"), Object).Tab = 0
        If SSTab1.SelectedIndex <> 0 Then SSTab1.SelectedIndex = 0

        'Insert values in Eighths
        g_iRightInsertValue(1) = 4
        g_iRightInsertValue(2) = 5
        g_iRightInsertValue(3) = 6
        g_iRightInsertValue(4) = 7
        g_iRightInsertValue(5) = 8
        g_iRightInsertValue(6) = 3

        'Missing RightFinger
        For ii = 1 To 5
            g_RightMissing(ii) = False
        Next ii

        'Given Dimensions and missing fingers
        g_RightMissingFingers = 0

        If txtCir(0).Enabled Then g_nRightFingerDIPCir(1) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(0).Text)) 'Little finger RIGHT_DIP
        If txtCir(1).Enabled Then g_nRightFingerPIPCir(1) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(1).Text)) 'Little finger RIGHT_PIP
        If g_nRightFingerPIPCir(1) = 0 Then
            g_RightMissingFingers = g_RightMissingFingers + 1
            g_RightMissing(1) = True
        End If

        If txtCir(2).Enabled Then g_nRightFingerDIPCir(2) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(2).Text)) 'Ring finger RIGHT_DIP
        If txtCir(3).Enabled Then g_nRightFingerPIPCir(2) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(3).Text)) 'Ring finger RIGHT_PIP
        If g_nRightFingerPIPCir(2) = 0 Then
            g_RightMissingFingers = g_RightMissingFingers + 1
            g_RightMissing(2) = True
        End If

        If txtCir(4).Enabled Then g_nRightFingerDIPCir(3) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(4).Text)) 'Middle RightFinger RIGHT_DIP
        If txtCir(5).Enabled Then g_nRightFingerPIPCir(3) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(5).Text)) 'Middle RightFinger RIGHT_PIP
        If g_nRightFingerPIPCir(3) = 0 Then
            g_RightMissingFingers = g_RightMissingFingers + 1
            g_RightMissing(3) = True
        End If

        If txtCir(6).Enabled Then g_nRightFingerDIPCir(4) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(6).Text)) 'Index RightFinger RIGHT_DIP
        If txtCir(7).Enabled Then g_nRightFingerPIPCir(4) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(7).Text)) 'Index RightFinger RIGHT_PIP
        If g_nRightFingerPIPCir(4) = 0 Then
            g_RightMissingFingers = g_RightMissingFingers + 1
            g_RightMissing(4) = True
        End If

        If txtCir(8).Enabled Then g_nRightFingerPIPCir(5) = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(8).Text)) 'Thumb
        If g_nRightFingerPIPCir(5) = 0 Then
            g_RightMissing(5) = True
        End If

        g_nRightPalm = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(9).Text)) 'Palm
        g_nRightWrist = MANGLOVERight_bas.fnDisplayToInches(Val(txtCir(10).Text)) 'Wrist

        'RightFinger Lengths
        For ii = 0 To 4
            If txtLen(ii).Enabled Then g_nRightFingerLen(ii + 1) = MANGLOVERight_bas.fnDisplayToInches(Val(txtLen(ii).Text))
        Next ii

        'Webs
        For ii = 1 To 4
            g_nRightWeb(ii) = MANGLOVERight_bas.fnDisplayToInches(Val(txtLen(ii + 4).Text))
        Next ii

        'Set finger tips
        For ii = 0 To 2
            If optLittleTip(ii).Checked = True Then
                If g_RightOnFold Then
                    g_RightFinger(1).Tip = ii + 10
                Else
                    g_RightFinger(1).Tip = ii
                End If
            End If
            If optRingTip(ii).Checked = True Then g_RightFinger(2).Tip = ii
            If optMiddleTip(ii).Checked = True Then g_RightFinger(3).Tip = ii
            If optIndexTip(ii).Checked = True Then g_RightFinger(4).Tip = ii
            If optThumbTip(ii).Checked = True Then g_RightFinger(5).Tip = ii
        Next ii

        ''---------g_nRightCir(1) = MANGLOVERight_bas.FN_InchesValue(txtCir(10))
        g_nRightCir(1, 1) = MANGLOVERight_bas.FN_InchesValue(txtCir(10))
        ''---------If txtCir(11).Enabled Then g_nRightCir(2) = MANGLOVERight_bas.FN_InchesValue(txtCir(11))
        If txtCir(11).Enabled Then g_nRightCir(1, 2) = MANGLOVERight_bas.FN_InchesValue(txtCir(11))
        ''-------------If txtCir(12).Enabled Then g_nRightCir(3) = MANGLOVERight_bas.FN_InchesValue(txtCir(12))
        If txtCir(12).Enabled Then g_nRightCir(1, 3) = MANGLOVERight_bas.FN_InchesValue(txtCir(12))

        'Thumb styles
        For ii = 0 To 2
            If optThumbStyle(ii).Checked = True Then g_iRightThumbStyle = ii
        Next ii

        'Insert styles
        For ii = 0 To 3
            If optInsertStyle(ii).Checked = True Then g_iRightInsertStyle = ii
        Next ii

        'print fold
        g_RightPrintFold = False
        ''-------------If CType(MANGLOVERight_bas.MainForm.Controls("chkPrintFold"), Object).Value <> 0 Then g_RightPrintFold = True
        If chkPrintFold.Checked <> False Then g_RightPrintFold = True

        'Fused fingers
        g_RightFusedFingers = False
        ''-----------If CType(MANGLOVERight_bas.MainForm.Controls("chkFusedFingers"), Object).Value <> 0 Then g_RightFusedFingers = True
        If chkFusedFingers.Checked <> False Then g_RightFusedFingers = True

    End Sub

    Private Sub PR_UpdateDBFields(ByRef sType As String)
        'sType = "Save" for use with save only
        'sType = "Draw" for use with a drawing macro

        Dim sSymbol As String

        'Glove common
        sSymbol = "glovecommon"
        If txtUidGC.Text = "" Then
            'Insert a new symbol
            PrintLine(MANGLOVERight_bas.fNum, "if ( Symbol(" & QQ & "find" & QCQ & sSymbol & QQ & ")){")
            PrintLine(MANGLOVERight_bas.fNum, "  Execute (" & QQ & "menu" & QCQ & "SetLayer" & QC & "Table(" & QQ & "find" & QCQ & "layer" & QCQ & "Data" & QQ & "));")
            PrintLine(MANGLOVERight_bas.fNum, "  hSym = AddEntity(" & QQ & "symbol" & QCQ & sSymbol & QC & "xyO.x, xyO.y);")
            PrintLine(MANGLOVERight_bas.fNum, "  }")
            PrintLine(MANGLOVERight_bas.fNum, "else")
            PrintLine(MANGLOVERight_bas.fNum, "  Exit(%cancel, " & QQ & "Can't find >" & sSymbol & "< symbol to insert\nCheck your installation, that JOBST.SLB exists!" & QQ & ");")
        Else
            'Use existing symbol
            PrintLine(MANGLOVERight_bas.fNum, "hSym = UID (" & QQ & "find" & QC & Val(txtUidGC.Text) & ");")
            PrintLine(MANGLOVERight_bas.fNum, "if (!hSym) Exit(%cancel," & QQ & "Can't find >" & sSymbol & "< symbol to update!" & QQ & ");")
        End If

        'Update DB fields
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "Fabric" & QCQ & txtFabric.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "Data" & QCQ & txtDataGC.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "fileno" & QCQ & txtFileNo.Text & QQ & ");")

        'Glove
        sSymbol = "gloveglove"
        If txtUidGlove.Text = "" Then
            'Insert a new symbol
            PrintLine(MANGLOVERight_bas.fNum, "if ( Symbol(" & QQ & "find" & QCQ & sSymbol & QQ & ")){")
            PrintLine(MANGLOVERight_bas.fNum, "  Execute (" & QQ & "menu" & QCQ & "SetLayer" & QC & "Table(" & QQ & "find" & QCQ & "layer" & QCQ & "Data" & QQ & "));")
            If optHand(0).Checked = True Then
                'Left Hand
                PrintLine(MANGLOVERight_bas.fNum, "  hSym = AddEntity(" & QQ & "symbol" & QCQ & sSymbol & QC & "xyO.x+1.5 , xyO.y);")
            Else
                'Right Hand
                PrintLine(MANGLOVERight_bas.fNum, "  hSym = AddEntity(" & QQ & "symbol" & QCQ & sSymbol & QC & "xyO.x+3 , xyO.y);")
            End If
            PrintLine(MANGLOVERight_bas.fNum, "  }")
            PrintLine(MANGLOVERight_bas.fNum, "else")
            PrintLine(MANGLOVERight_bas.fNum, "  Exit(%cancel, " & QQ & "Can't find >" & sSymbol & "< symbol to insert\nCheck your installation, that JOBST.SLB exists!" & QQ & ");")
        Else
            'Use existing symbol
            PrintLine(MANGLOVERight_bas.fNum, "hSym = UID (" & QQ & "find" & QC & Val(txtUidGlove.Text) & ");")
            PrintLine(MANGLOVERight_bas.fNum, "if (!hSym) Exit(%cancel," & QQ & "Can't find >" & sSymbol & "< symbol to update!" & QQ & ");")
        End If

        'Update DB fields
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "TapeLengths" & QCQ & txtTapeLengths.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "TapeLengths2" & QCQ & txtTapeLengths2.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "TapeLengthPt1" & QCQ & txtTapeLengthPt1.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "fileno" & QCQ & txtFileNo.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "Sleeve" & QCQ & txtSide.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "Grams" & QCQ & txtGrams.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "Reduction" & QCQ & txtReduction.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "TapeMMs" & QCQ & txtTapeMMs.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "Data" & QCQ & txtDataGlove.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "WristPleat" & QCQ & txtWristPleat.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "ShoulderPleat" & QCQ & txtShoulderPleat.Text & QQ & ");")
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hSym" & CQ & "Flap" & QCQ & txtFlap.Text & QQ & ");")

        If sType = "Draw" Then
            'Update the fields associated with the PALMER marker
            'This was given an explicit handle in the procedure PR_CreateDrawMacro
            '
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "TapeLengths" & QCQ & txtTapeLengths.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "TapeLengths2" & QCQ & txtTapeLengths2.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "TapeLengthPt1" & QCQ & txtTapeLengthPt1.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "TapeMMs" & QCQ & txtTapeMMs.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "fileno" & QCQ & txtFileNo.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "Sleeve" & QCQ & txtSide.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "Data" & QCQ & txtDataGlove.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "age" & QCQ & txtAge.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "units" & QCQ & txtUnits.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "Grams" & QCQ & txtGrams.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "Reduction" & QCQ & txtReduction.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "TapeMMs" & QCQ & txtTapeMMs.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "Data" & QCQ & txtDataGlove.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "WristPleat" & QCQ & txtWristPleat.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "ShoulderPleat" & QCQ & txtShoulderPleat.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "Flap" & QCQ & txtFlap.Text & QQ & ");")
            PrintLine(MANGLOVERight_bas.fNum, "SetDBData( hPalmer" & CQ & "Fabric" & QCQ & txtFabric.Text & QQ & ");")
        End If
    End Sub

    Private Sub PR_UpdateDDE()

        'Procedure to update the fields used when data is transfered
        'from DRAFIX using DDE.
        'Although the transfer back to DRAFIX is not via DDE we use the same controls
        'simply to illustrate the method by which the data is packed into the
        'fields

        'The decisons on format for each is not very neat!
        'The reason is the iteritive development process and the 63 char length
        'limitation on DRAFIX DB Fields.

        'Backward compatibility with the CAD-Glove is required in this
        'procedure

        ''-------------------------CType(MANGLOVERight_bas.MainForm.Controls("SSTab1"), Object).Tab = 0
        SSTab1.SelectedIndex = 0

        Dim iLen, ii As Short
        Dim sLen, sPacked As String

        'Initialise
        sPacked = ""

        'Pack Circumferences
        For ii = 0 To 10
            iLen = Val(txtCir(ii).Text) * 10 'Shift decimal place

            If iLen <> 0 Then
                sLen = New String(" ", 3)
                sLen = RSet(Trim(Str(iLen)), Len(sLen))
            Else
                sLen = New String(" ", 3)
            End If
            sPacked = sPacked & sLen
        Next ii

        'Pack Lengths
        For ii = 0 To 8
            iLen = Val(txtLen(ii).Text) * 10 'Shift decimal place

            If iLen <> 0 Then
                sLen = New String(" ", 3)
                sLen = RSet(Trim(Str(iLen)), Len(sLen))
            Else
                sLen = New String(" ", 3)
            End If
            sPacked = sPacked & sLen
        Next ii

        'Store to DDE text boxes
        txtTapeLengths.Text = sPacked

        'Tip options
        Dim Closed_Renamed(4) As Object
        Dim StdOpen(4) As Object
        Closed_Renamed(0) = optLittleTip(0).Checked
        Closed_Renamed(1) = optRingTip(0).Checked
        Closed_Renamed(2) = optMiddleTip(0).Checked
        Closed_Renamed(3) = optIndexTip(0).Checked
        Closed_Renamed(4) = optThumbTip(0).Checked

        StdOpen(0) = optLittleTip(1).Checked
        StdOpen(1) = optRingTip(1).Checked
        StdOpen(2) = optMiddleTip(1).Checked
        StdOpen(3) = optIndexTip(1).Checked
        StdOpen(4) = optThumbTip(1).Checked

        sPacked = ""
        For ii = 0 To 4
            If Closed_Renamed(ii) = True Then
                sPacked = sPacked & "0"
            ElseIf StdOpen(ii) = True Then
                sPacked = sPacked & "1"
            Else
                sPacked = sPacked & "2"
            End If
        Next ii

        'CheckBoxes
        sPacked = sPacked & Trim(Str(chkSlantedInserts.CheckState))
        sPacked = sPacked & Trim(Str(chkPalm.CheckState))

        'Tapes past wrist (Store here due to limitations on DB field length
        'Pack Circumferences
        For ii = 11 To 12
            iLen = Val(txtCir(ii).Text) * 10 'Shift decimal place

            If iLen <> 0 Then
                sLen = New String(" ", 3)
                sLen = RSet(Trim(Str(iLen)), Len(sLen))
            Else
                sLen = New String(" ", 3)
            End If
            sPacked = sPacked & sLen
        Next ii

        'Insert size
        'Translate from Manual glove format to Eighths
        'NOTE:-
        '    This is rather stupid but it made life easier earlier
        If g_iInsertSize = 6 Then
            iLen = 3
        ElseIf g_iInsertSize > 0 Then
            iLen = 3 + g_iInsertSize
        End If
        sLen = New String(" ", 3)
        sLen = RSet(Trim(Str(iLen)), Len(sLen))
        sPacked = sPacked & sLen

        'Reinforced dorsal
        sPacked = sPacked & Trim(Str(chkDorsal.CheckState))

        'Thumb Options
        For ii = 0 To 2
            If optThumbStyle(ii).Checked = True Then sPacked = sPacked & Trim(Str(ii))
        Next ii
        'Insert Options
        For ii = 0 To 3
            If optInsertStyle(ii).Checked = True Then sPacked = sPacked & Trim(Str(ii))
        Next ii

        'Print Fold
        sPacked = sPacked & Trim(Str(chkPrintFold.CheckState))

        'Fused Fingers
        sPacked = sPacked & Trim(Str(chkFusedFingers.CheckState))

        'Store to DDE text boxes
        txtTapeLengths2.Text = sPacked

        'Hand & Glove common
        'Glove common is used to carry the fabric and insert size and
        'on\off fold data, so that both gloves can be harmonized
        sLen = New String(" ", 2)
        sLen = RSet(Trim(Str(g_iInsertSize)), Len(sLen))
        ''-------------If CType(MANGLOVERight_bas.MainForm.Controls("optFold"), Object)(0).Value = True Then sPacked = sLen & " 0" Else sPacked = sLen & " 1"
        If optFold(0).Checked = True Then sPacked = sLen & " 0" Else sPacked = sLen & " 1"

        If optHand(0).Checked = True Then
            txtSide.Text = "Left"
            sPacked = sPacked & Mid(txtDataGC.Text, 5, 4)
        Else
            txtSide.Text = "Right"
            If Mid(txtDataGC.Text, 1, 4) <> "" Then
                sPacked = Mid(txtDataGC.Text, 1, 4) & sPacked
            Else
                sPacked = "    " & sPacked
            End If
        End If
        txtDataGC.Text = sPacked

        'Fabric
        If cboFabric.Text <> "" Then txtFabric.Text = cboFabric.Text
    End Sub

    Private Sub Tab_Renamed_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Tab_Renamed.Click
        'Allows the user to use enter as a tab

        System.Windows.Forms.SendKeys.Send("{TAB}")

    End Sub

    'Private Sub Timer1_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles Timer1.Tick
    '    'End as the link close event has not
    '    'occured.
    '    Return
    'End Sub

    Private Sub txtCir_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtCir.Enter
        Dim Index As Short = txtCir.GetIndex(eventSender)
        MANGLOVERight_bas.PR_Select_Text(txtCir(Index))
    End Sub

    Private Sub txtCir_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtCir.Leave
        Dim Index As Short = txtCir.GetIndex(eventSender)

        Dim nLen As Double
        Dim sString As String

        'Check that number is valid (-ve value => invalid)
        nLen = MANGLOVERight_bas.FN_InchesValue(txtCir(Index))
        'Display value in inches if Valid
        If nLen <> -1 Then
            sString = Trim(ARMEDDIA1.fnInchesToText(nLen))
            If VB.Left(sString, 1) = "-" Then sString = Mid(sString, 2)
            lblCir(Index).Text = sString
        End If
    End Sub

    Private Sub txtCustFlapLength_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtCustFlapLength.Enter

        MANGLOVERight_bas.PR_Select_Text(txtCustFlapLength)
    End Sub

    Private Sub txtCustFlapLength_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtCustFlapLength.Leave

        Dim nLen As Double

        'Check that number is valid (-ve value => invalid)
        nLen = MANGLOVERight_bas.FN_InchesValue(txtCustFlapLength)

        'Display value in inches if Valid
        If nLen <> -1 Then labCustFlapLength.Text = ARMEDDIA1.fnInchesToText(nLen)
    End Sub

    Private Sub txtExtCir_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtExtCir.Enter
        Dim Index As Short = txtExtCir.GetIndex(eventSender)
        MANGLOVERight_bas.PR_Select_Text(txtExtCir(Index))
    End Sub

    Private Sub txtExtCir_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtExtCir.Leave
        Dim Index As Short = txtExtCir.GetIndex(eventSender)

        Dim nLen As Double

        'Check that number is valid (-ve value => invalid)
        nLen = MANGLOVERight_bas.FN_InchesValue(txtExtCir(Index))

        'Display value in inches if Valid
        Dim sString As String
        If nLen <> -1 Then
            'Note we use a grid here that starts at 0
            ''-------------------------------- MANGLOVERight_bas.PR_GrdInchesDisplay(Index - 8, nLen)
            sString = Trim(MANGLOVERight_bas.fnInchestoText(nLen))
            If VB.Left(sString, 1) = "-" Then sString = Mid(sString, 2)
            lblExtCir(Index - 8).Text = sString
        End If

    End Sub

    Private Sub txtFrontStrapLength_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtFrontStrapLength.Enter

        MANGLOVERight_bas.PR_Select_Text(txtFrontStrapLength)

    End Sub

    Private Sub txtFrontStrapLength_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtFrontStrapLength.Leave
        Dim nLen As Double

        'Check that number is valid (-ve value => invalid)
        nLen = MANGLOVERight_bas.FN_InchesValue(txtFrontStrapLength)

        'Display value in inches if Valid
        If nLen <> -1 Then labFrontStrapLength.Text = ARMEDDIA1.fnInchesToText(nLen)

    End Sub

    Private Sub txtLen_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLen.Enter
        Dim Index As Short = txtLen.GetIndex(eventSender)
        MANGLOVERight_bas.PR_Select_Text(txtLen(Index))
    End Sub

    Private Sub txtLen_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtLen.Leave
        Dim Index As Short = txtLen.GetIndex(eventSender)
        Dim nLen As Double
        Dim nTipCutBack As Double
        Dim sString As String

        'Check that number is valid (-ve value => invalid)
        nLen = MANGLOVERight_bas.FN_InchesValue(txtLen(Index))
        If nLen < 0 Then
            lblLen(Index).Text = ""
            Exit Sub
        End If

        If Index <= 4 Then
            'Special case for finger lengths w.r.t. tips
            If Val(txtAge.Text) <= AGE_CUTOFF Then
                nTipCutBack = CHILD_STD_OPEN_TIP
            Else
                nTipCutBack = ADULT_STD_OPEN_TIP
            End If

            Select Case Index
                Case 0
                    'Little finger
                    If optLittleTip(1).Checked Then nLen = nLen - nTipCutBack
                Case 1
                    'Ring finger
                    If optRingTip(1).Checked Then nLen = nLen - nTipCutBack
                Case 2
                    'Middle finger
                    If optMiddleTip(1).Checked Then nLen = nLen - nTipCutBack
                Case 3
                    'Index finger
                    If optIndexTip(1).Checked Then nLen = nLen - nTipCutBack
                Case 4
                    'Thumb finger
                    If optThumbTip(1).Checked Then nLen = nLen - nTipCutBack
            End Select
        End If

        'Display length text
        If nLen > 0 Then
            sString = Trim(ARMEDDIA1.fnInchesToText(nLen))
            If VB.Left(sString, 1) = "-" Then sString = Mid(sString, 2)
            lblLen(Index).Text = sString
        Else
            lblLen(Index).Text = ""
        End If

    End Sub

    Private Sub txtStrapLength_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtStrapLength.Enter

        MANGLOVERight_bas.PR_Select_Text(txtStrapLength)

    End Sub

    Private Sub txtStrapLength_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtStrapLength.Leave
        Dim nLen As Double

        'Check that number is valid (-ve value => invalid)
        nLen = MANGLOVERight_bas.FN_InchesValue(txtStrapLength)

        'Display value in inches if Valid
        If nLen <> -1 Then labStrap.Text = ARMEDDIA1.fnInchesToText(nLen)

    End Sub

    Private Sub txtWaistCir_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtWaistCir.Enter

        MANGLOVERight_bas.PR_Select_Text(txtWaistCir)

    End Sub

    Private Sub txtWaistCir_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles txtWaistCir.Leave
        Dim nLen As Double

        'Check that number is valid (-ve value => invalid)
        nLen = MANGLOVERight_bas.FN_InchesValue(txtWaistCir)

        'Display value in inches if Valid
        If nLen <> -1 Then labWaistCir.Text = ARMEDDIA1.fnInchesToText(nLen)
    End Sub
    Sub PR_UpdateDDE_Extension()
        'Procedure to update the fields used when data is transfered
        'from DRAFIX using DDE.
        'Although the transfer back to DRAFIX is not via DDE we use the same controls
        'simply to illustrate the method by which the data is packed into the
        'fields

        'This routine is similar to PR_UpdateDDE in MANGLOVE.FRM#
        'it is split so that we can acccess the Module level variables
        Dim iLen, ii As Short
        Dim sLen, sPacked As String

        ''--------------CType(MANGLOVERight_bas.MainForm.Controls("SSTab1"), Object).Tab = 1
        SSTab1.SelectedIndex = 1


        'Pack Glove extensions
        sPacked = "   " 'Allow for possible extension to -4 tape
        For ii = 8 To 23
            ''-----------iLen = Val(CType(MANGLOVERight_bas.MainForm.Controls("txtExtCir"), Object)(ii).Text) * 10 'Shift decimal place
            iLen = Val(txtExtCir(ii).Text) * 10 'Shift decimal place

            If iLen <> 0 Then
                sLen = New String(" ", 3)
                sLen = RSet(Trim(Str(iLen)), Len(sLen))
            Else
                sLen = New String(" ", 3)
            End If

            sPacked = sPacked & sLen

        Next ii
        ''-------------CType(MANGLOVERight_bas.MainForm.Controls("txtTapeLengthPt1"), Object).Text = sPacked
        txtTapeLengthPt1.Text = sPacked

        'Pack Grams, MMs and Reductions
        Dim sTapeMMs, sGrams, sReduction As String

        sGrams = "   " 'Allow for possible extension to -4 tape
        sTapeMMs = "   " 'Allow for possible extension to -4 tape
        sReduction = "   " 'Allow for possible extension to -4 tape

        For ii = 1 To 16
            'Grams
            If MANGLOVERight_bas.g_iGms(ii) <> 0 Then
                sLen = New String(" ", 3)
                sLen = RSet(Trim(Str(MANGLOVERight_bas.g_iGms(ii))), Len(sLen))
            Else
                sLen = New String(" ", 3)
            End If
            sGrams = sGrams & sLen

            'Tape pressures
            If MANGLOVERight_bas.g_iMMs(ii) <> 0 Then
                sLen = New String(" ", 3)
                sLen = RSet(Trim(Str(MANGLOVERight_bas.g_iMMs(ii))), Len(sLen))
            Else
                sLen = New String(" ", 3)
            End If
            sTapeMMs = sTapeMMs & sLen

            'Reductions
            If MANGLOVERight_bas.g_iRed(ii) <> 0 Then
                sLen = New String(" ", 3)
                sLen = RSet(Trim(Str(MANGLOVERight_bas.g_iRed(ii))), Len(sLen))
            Else
                sLen = New String(" ", 3)
            End If
            sReduction = sReduction & sLen
        Next ii

        ''------------------CType(MANGLOVERight_bas.MainForm.Controls("txtReduction"), Object).Text = sReduction
        ''------------------CType(MANGLOVERight_bas.MainForm.Controls("txtTapeMMs"), Object).Text = sTapeMMs
        ''------------------CType(MANGLOVERight_bas.MainForm.Controls("txtGrams"), Object).Text = sGrams
        txtReduction.Text = sReduction
        txtTapeMMs.Text = sTapeMMs
        txtGrams.Text = sGrams

        'Fold, variations etc etc.
        sPacked = ""

        'Fold
        ''---------If CType(MANGLOVERight_bas.MainForm.Controls("optFold"), Object)(0).Value = True Then sPacked = " 0" Else sPacked = " 1"
        If optFold(0).Checked = True Then sPacked = " 0" Else sPacked = " 1"

        'Pressure
        sLen = New String(" ", 2)
        ''------------sLen = RSet(Trim(Str(CType(MANGLOVERight_bas.MainForm.Controls("cboPressure"), Object).SelectedIndex)), Len(sLen))
        sLen = RSet(Trim(Str(cboPressure.SelectedIndex)), Len(sLen))
        sPacked = sPacked & sLen

        'Wrist Tape
        ''-----------sLen = RSet(Trim(Str(CType(MANGLOVERight_bas.MainForm.Controls("cboDistalTape"), Object).SelectedIndex)), Len(sLen))
        sLen = RSet(Trim(Str(cboDistalTape.SelectedIndex)), Len(sLen))
        sPacked = sPacked & sLen

        'EOS Tape
        ''-----------sLen = RSet(Trim(Str(CType(MANGLOVERight_bas.MainForm.Controls("cboProximalTape"), Object).SelectedIndex)), Len(sLen))
        sLen = RSet(Trim(Str(cboProximalTape.SelectedIndex)), Len(sLen))
        sPacked = sPacked & sLen

        'Variations
        ''---------If CType(MANGLOVERight_bas.MainForm.Controls("optExtendTo"), Object)(0).Value = True Then
        If optExtendTo(0).Checked = True Then
            sPacked = sPacked & " 0 0" 'Second Zero is w.r.t Flap
            ''-------------ElseIf CType(MANGLOVERight_bas.MainForm.Controls("optExtendTo"), Object)(1).Value = True Then
        ElseIf optExtendTo(1).Checked = True Then
            sPacked = sPacked & " 1 0" 'Second Zero is w.r.t Flap
        Else
            sPacked = sPacked & " 2"
            'Check for flaps
            ''-------------If CType(MANGLOVERight_bas.MainForm.Controls("optProximalTape"), Object)(1).Checked = True Then sPacked = sPacked & " 1" Else sPacked = sPacked & " 0"
            If optProximalTape(1).Checked = True Then sPacked = sPacked & " 1" Else sPacked = sPacked & " 0"
        End If

        ''---------------CType(MANGLOVERight_bas.MainForm.Controls("txtDataGlove"), Object).Text = sPacked
        txtDataGlove.Text = sPacked

        'Flap
        sPacked = ""
        ''-----------If CType(MANGLOVERight_bas.MainForm.Controls("optExtendTo"), Object)(2).Value = True And CType(MANGLOVERight_bas.MainForm.Controls("optProximalTape"), Object)(1).Checked = True Then
        If optExtendTo(2).Checked = True And optProximalTape(1).Checked = True Then
            'Flap Type
            sLen = New String(" ", 3)
            ''-----------sLen = RSet(Trim(Str(CType(MANGLOVERight_bas.MainForm.Controls("cboFlaps"), Object).SelectedIndex)), Len(sLen))
            sLen = RSet(Trim(Str(cboFlaps.SelectedIndex)), Len(sLen))
            sPacked = sPacked & sLen

            ''-----------sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtStrapLength"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
            sLen = RSet(Trim(Str(Val(txtStrapLength.Text) * 10)), Len(sLen)) 'Shift decimal place
            sPacked = sPacked & sLen

            ''-----------sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtFrontStrapLength"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
            sLen = RSet(Trim(Str(Val(txtFrontStrapLength.Text) * 10)), Len(sLen)) 'Shift decimal place
            sPacked = sPacked & sLen

            ''-----------sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtCustFlapLength"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
            sLen = RSet(Trim(Str(Val(txtCustFlapLength.Text) * 10)), Len(sLen)) 'Shift decimal place
            sPacked = sPacked & sLen

            ''-----------sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtWaistCir"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
            sLen = RSet(Trim(Str(Val(txtWaistCir.Text) * 10)), Len(sLen)) 'Shift decimal place
            sPacked = sPacked & sLen
        End If

        ''----------CType(MANGLOVERight_bas.MainForm.Controls("txtFlap"), Object).Text = sPacked
        txtFlap.Text = sPacked

        'Pleats
        sLen = New String(" ", 3)
        ''-----------If Val(CType(MANGLOVERight_bas.MainForm.Controls("txtWristPleat1"), Object).Text) > 0 Then sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtWristPleat1"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
        If Val(txtWristPleat1.Text) > 0 Then sLen = RSet(Trim(Str(Val(txtWristPleat1.Text) * 10)), Len(sLen)) 'Shift decimal place
        sPacked = sLen

        sLen = New String(" ", 3)
        ''---------If Val(CType(MANGLOVERight_bas.MainForm.Controls("txtWristPleat2"), Object).Text) Then sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtWristPleat2"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
        If Val(txtWristPleat2.Text) Then sLen = RSet(Trim(Str(Val(txtWristPleat2.Text) * 10)), Len(sLen)) 'Shift decimal place
        ''-------------CType(MANGLOVERight_bas.MainForm.Controls("txtWristPleat"), Object).Text = sPacked & sLen
        txtWristPleat.Text = sPacked & sLen

        sLen = New String(" ", 3)
        ''--------------If Val(CType(MANGLOVERight_bas.MainForm.Controls("txtShoulderPleat1"), Object).Text) > 0 Then sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtShoulderPleat1"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
        If Val(txtShoulderPleat1.Text) > 0 Then sLen = RSet(Trim(Str(Val(txtShoulderPleat1.Text) * 10)), Len(sLen)) 'Shift decimal place
        sPacked = sLen

        sLen = New String(" ", 3)
        ''--------------If Val(CType(MANGLOVERight_bas.MainForm.Controls("txtShoulderPleat2"), Object).Text) Then sLen = RSet(Trim(Str(Val(CType(MANGLOVERight_bas.MainForm.Controls("txtShoulderPleat2"), Object).Text) * 10)), Len(sLen)) 'Shift decimal place
        If Val(txtShoulderPleat2.Text) Then sLen = RSet(Trim(Str(Val(txtShoulderPleat2.Text) * 10)), Len(sLen)) 'Shift decimal place
        ''--------------CType(MANGLOVERight_bas.MainForm.Controls("txtShoulderPleat"), Object).Text = sPacked & sLen
        txtShoulderPleat.Text = sPacked & sLen
    End Sub
    Sub PR_CalculateArmTapeReductions()
        'Procedure to calculate the reductions
        'at each arm tape
        Dim ii As Short

        'Don't Calculate if normal glove
        ''----------If CType(MANGLOVERight_bas.MainForm.Controls("optExtendTo"), Object)(0).Value = True Then Exit Sub
        If optExtendTo(0).Checked = True Then Exit Sub

        ''------------If CType(MANGLOVERight_bas.MainForm.Controls("cboFabric"), Object).Text = "" Then
        If cboFabric.Text = "" Then
            MsgBox("Fabric not given! ", 48, "Glove - Calculate ARM Button")
            Exit Sub
        Else
            ''----------If UCase(Mid(CType(MANGLOVERight_bas.MainForm.Controls("cboFabric"), Object).Text, 1, 3)) <> "POW" Then
            If UCase(Mid(cboFabric.Text, 1, 3)) <> "POW" Then
                MsgBox("Fabric chosen is not Powernet!", 48, "Glove - Calculate ARM Button")
                Exit Sub
            End If
            ''----------If Val(Mid(CType(MANGLOVERight_bas.MainForm.Controls("cboFabric"), Object).Text, 5, 3)) < RIGHT_LOW_MODULUS Or Val(Mid(CType(MANGLOVERight_bas.MainForm.Controls("cboFabric"), Object).Text, 5, 3)) > RIGHT_HIGH_MODULUS Then
            If Val(Mid(cboFabric.Text, 5, 3)) < RIGHT_LOW_MODULUS Or Val(Mid(cboFabric.Text, 5, 3)) > RIGHT_HIGH_MODULUS Then
                MsgBox("Modulus of fabric chosen is not available on Gram / Tension reduction chart!", 48, "Glove - Calculate ARM Button")
                Exit Sub
            End If
        End If

        'Update from dialogue
        PR_GetDlgAboveWrist()

        If g_iRightPressure < 0 Then
            MsgBox("Can't Calculate, missing Pressure", 48, "CAD Glove - Calculate ARM Button")
            Exit Sub
        End If

        If g_iRightNumTapesWristToEOS <= 1 Then
            MsgBox("Can't Calculate, Only one tape between given Wrist and EOS", 48, "CAD Glove - Calculate ARM Button")
            Exit Sub
        End If

        'Check that there are no holes in the data
        For ii = g_iRightWristPointer To g_iRightEOSPointer
            ''------If g_nRightCir(ii) = 0 Then
            If g_nRightCir(1, ii) = 0 Then
                MsgBox("Can't Calculate, missing Tape Circumferences between given Wrist and EOS", 48, "CAD Glove - Calculate ARM Button")
                Exit Sub
            End If
        Next ii


        'Set the MMs based on the selected pressure
        PR_SetMMs(cboPressure.Text)

        'Set the modulus based on the fabric
        PR_SetModulusIndex(cboFabric.Text)

        'Calculate grams etc
        For ii = 1 To RIGHT_NOFF_ARMTAPES
            If ii >= g_iRightWristPointer And ii <= g_iRightEOSPointer Then

                ''-----------MANGLOVERight_bas.g_iGms(ii) = ARMDIA1.round(g_nRightCir(ii) * MANGLOVERight_bas.g_iMMs(ii))
                MANGLOVERight_bas.g_iGms(ii) = ARMDIA1.round(g_nRightCir(1, ii) * MANGLOVERight_bas.g_iMMs(ii))
                MANGLOVERight_bas.g_iRed(ii) = FN_GetRightReduction(MANGLOVERight_bas.g_iGms(ii))

                'Adjust at wrist
                If ii = g_iRightEOSPointer Then
                    If ii < RIGHT_ELBOW_TAPE Then
                        'The EOS must lie between 10 and 14
                        'From the given reduction we back calculate the grams and mms
                        If MANGLOVERight_bas.g_iRed(ii) > 14 Then MANGLOVERight_bas.g_iRed(ii) = 14
                    Else
                        'For Flaps always use a 12
                        'From the given reduction we back calculate the grams and mms
                        If g_RightEOSType = RIGHT_ARM_PLAIN Then
                            MANGLOVERight_bas.g_iRed(ii) = 10
                        Else
                            MANGLOVERight_bas.g_iRed(ii) = 12
                        End If

                    End If
                    'Back calculate
                    MANGLOVERight_bas.g_iGms(ii) = FN_GetRightGrams(MANGLOVERight_bas.g_iRed(ii))
                    ''------------MANGLOVERight_bas.g_iMMs(ii) = ARMDIA1.round(MANGLOVERight_bas.g_iGms(ii) / g_nRightCir(ii))
                    MANGLOVERight_bas.g_iMMs(ii) = ARMDIA1.round(MANGLOVERight_bas.g_iGms(ii) / g_nRightCir(1, ii))
                End If

                'Adjust at EOS  g_iRightWristPointer
                If ii = g_iRightWristPointer Then
                    'Always use a 10 reduction for wrist
                    MANGLOVERight_bas.g_iRed(ii) = 10
                    MANGLOVERight_bas.g_iGms(ii) = FN_GetRightGrams(MANGLOVERight_bas.g_iRed(ii))
                    ''-------------MANGLOVERight_bas.g_iMMs(ii) = ARMDIA1.round(MANGLOVERight_bas.g_iGms(ii) / g_nRightCir(ii))
                    MANGLOVERight_bas.g_iMMs(ii) = ARMDIA1.round(MANGLOVERight_bas.g_iGms(ii) / g_nRightCir(1, ii))
                End If

                'Display Results
                ''----------------------------PR_GrdInchesDisplay(ii - 1, g_nRightCir(ii))
                ''----------------------- PR_GramRedDisplay(ii - 1, MANGLOVERight_bas.g_iGms(ii), MANGLOVERight_bas.g_iRed(ii))
                ''------------CType(MANGLOVERight_bas.MainForm.Controls("mms"), Object)(ii + 7).Text = CStr(MANGLOVERight_bas.g_iMMs(ii))
                lblExtCir(ii - 1).Text = MANGLOVERight_bas.fnInchestoText(g_nRightCir(1, ii))
                lblGms(ii - 1).Text = CStr(MANGLOVERight_bas.g_iGms(ii))
                lblRed(ii - 1).Text = CStr(MANGLOVERight_bas.g_iRed(ii))
                mms(ii + 7).Text = CStr(MANGLOVERight_bas.g_iMMs(ii))

            Else
                'Blank the displayed values
                ''-------------------------PR_GrdInchesDisplay(ii - 1, 0)
                ''-----------------------PR_GramRedDisplay(ii - 1, 0, 0)
                lblGms(ii - 1).Text = ""
                lblRed(ii - 1).Text = ""
                ''------------CType(MANGLOVERight_bas.MainForm.Controls("mms"), Object)(ii + 7).Text = ""
                lblExtCir(ii - 1).Text = ""
                mms(ii + 7).Text = ""
                MANGLOVERight_bas.g_iMMs(ii) = 0
                MANGLOVERight_bas.g_iRed(ii) = 0
                MANGLOVERight_bas.g_iGms(ii) = 0
            End If
        Next ii
    End Sub
    Sub PR_SetMMs(ByRef sPressure As String)
        'Set the mms based on the pressure and the wrist and EOS
        'tapes given
        'REF:    GOP 01-02/16, Section 1.4
        '
        Dim ii As Short
        For ii = g_iRightWristPointer To g_iRightEOSPointer
            Select Case sPressure
                Case "15"
                    If ii = g_iRightWristPointer Then
                        MANGLOVERight_bas.g_iMMs(ii) = 12
                    ElseIf ii < RIGHT_ELBOW_TAPE - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 15
                    ElseIf ii = RIGHT_ELBOW_TAPE - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 12
                    ElseIf ii = RIGHT_ELBOW_TAPE Then
                        MANGLOVERight_bas.g_iMMs(ii) = 8
                    ElseIf ii = RIGHT_ELBOW_TAPE + 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 10
                    ElseIf (ii > RIGHT_ELBOW_TAPE + 1) And (ii <> g_iRightEOSPointer - 1) And (ii <> g_iRightEOSPointer) Then
                        MANGLOVERight_bas.g_iMMs(ii) = 12
                    ElseIf ii = g_iRightEOSPointer - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 10
                    ElseIf ii = g_iRightEOSPointer Then
                        MANGLOVERight_bas.g_iMMs(ii) = 8
                    End If
                Case "20"
                    If ii = g_iRightWristPointer Then
                        MANGLOVERight_bas.g_iMMs(ii) = 16
                    ElseIf ii < RIGHT_ELBOW_TAPE - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 20
                    ElseIf ii = RIGHT_ELBOW_TAPE - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 16
                    ElseIf ii = RIGHT_ELBOW_TAPE Then
                        MANGLOVERight_bas.g_iMMs(ii) = 10
                    ElseIf ii = RIGHT_ELBOW_TAPE + 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 13
                    ElseIf (ii > RIGHT_ELBOW_TAPE + 1) And (ii <> g_iRightEOSPointer - 1) And (ii <> g_iRightEOSPointer) Then
                        MANGLOVERight_bas.g_iMMs(ii) = 16
                    ElseIf ii = g_iRightEOSPointer - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 13
                    ElseIf ii = g_iRightEOSPointer Then
                        MANGLOVERight_bas.g_iMMs(ii) = 10
                    End If
                Case "25"
                    If ii = g_iRightWristPointer Then
                        MANGLOVERight_bas.g_iMMs(ii) = 20
                    ElseIf ii < RIGHT_ELBOW_TAPE - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 25
                    ElseIf ii = RIGHT_ELBOW_TAPE - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 20
                    ElseIf ii = RIGHT_ELBOW_TAPE Then
                        MANGLOVERight_bas.g_iMMs(ii) = 13
                    ElseIf ii = RIGHT_ELBOW_TAPE + 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 17
                    ElseIf (ii > RIGHT_ELBOW_TAPE + 1) And (ii <> g_iRightEOSPointer - 1) And (ii <> g_iRightEOSPointer) Then
                        MANGLOVERight_bas.g_iMMs(ii) = 20
                    ElseIf ii = g_iRightEOSPointer - 1 Then
                        MANGLOVERight_bas.g_iMMs(ii) = 17
                    ElseIf ii = g_iRightEOSPointer Then
                        MANGLOVERight_bas.g_iMMs(ii) = 13
                    End If
            End Select
        Next ii
    End Sub
    Sub PR_SetModulusIndex(ByRef sFabric As String)
        'Set the module level variable iModulus Index based on
        'the chosen fabric
        '
        ' Where sFabric = Pow 230-2B
        '
        '    g_iRightModulusIndex Start = RIGHT_LOW_MODULUS  = 160
        '    g_iRightModulusIndex End   = RIGHT_HIGH_MODULUS = 340
        '
        ' hence
        '    g_iRightModulusIndex = ((230 - 160) + 10) / 10
        '
        '
        Dim iMod As Short

        iMod = Val(Mid(sFabric, 5, 3))
        If iMod < RIGHT_LOW_MODULUS Then
            g_iRightModulusIndex = RIGHT_LOW_MODULUS
        ElseIf iMod > RIGHT_HIGH_MODULUS Then
            g_iRightModulusIndex = RIGHT_HIGH_MODULUS
        Else
            g_iRightModulusIndex = ((iMod - RIGHT_LOW_MODULUS) + 10) / 10
        End If
    End Sub
    Sub PR_GetDlgAboveWrist()
        'General procedure to read the data given in the
        'dialogue controls and copy to module level variables.
        'Saves having to do this more than once.
        '
        'Updates:-
        '

        'NOTE:
        '    We ignore the fact that there may be missing tapes.
        '
        '    g_iRightWristPointer and g_iRightEOSPointer are used to indicate the
        '    start and finish tapes in the arrays and not in the
        '    txtExtCir() array

        Dim nCir As Double
        Dim ii As Short

        ''----------------If CType(MainForm.Controls("SSTab1"), Object).Tab <> 1 Then CType(MainForm.Controls("SSTab1"), Object).Tab = 1
        If SSTab1.SelectedIndex <> 1 Then SSTab1.SelectedIndex = 1

        ''----------Dim g_iMGlvRightFirstTape As Integer = -1
        ''--------Dim g_iLastTape As Integer = -1
        ''----------Dim g_iRightWristPointer As Integer = -1
        ''------------Dim g_iRightEOSPointer As Integer = -1
        ''---------Dim g_iRightNumTotalTapes As Integer = 0
        ''-----------Dim g_iRightNumTapesWristToEOS As Integer = 0
        ''--------Dim g_iRightPressure As Integer = 1
        ''-------Dim g_RightExtendTo As Integer
        'Glove type
        If optExtendTo(0).Checked = True Then
            g_RightExtendTo = RIGHT_GLOVE_NORMAL
        ElseIf optExtendTo(1).Checked = True Then
            g_RightExtendTo = RIGHT_GLOVE_ELBOW
        Else
            g_RightExtendTo = RIGHT_GLOVE_AXILLA
        End If

        Dim iIndex As Short
        If g_RightExtendTo = RIGHT_GLOVE_NORMAL Then
            'Get values from Hand Data TAB
            'We will need to update this w.r.t CAD Glove later
            'Assummes no holes in data

            ''-----------If g_nRightCir(2) <= 0 Then
            If g_nRightCir(1, 2) <= 0 Then
                g_iRightNumTotalTapes = 1
                g_iRightNumTapesWristToEOS = 1
            ElseIf g_nRightCir(1, 3) > 0 Then
                ''-----------ElseIf g_nRightCir(3) > 0 Then
                g_iRightNumTotalTapes = 3
                g_iRightNumTapesWristToEOS = 3
            Else
                g_iRightNumTotalTapes = 2
                g_iRightNumTapesWristToEOS = 2
            End If
            '        g_iRightWristPointer = 1
            g_iRightWristPointer = 0
            g_iRightEOSPointer = 1000 'Stupid value to fool stupid code
            'in PR_ExtendTo_Click
        Else
            'Get First and last tape (if any)
            For ii = 8 To 23
                nCir = MANGLOVERight_bas.FN_InchesValue(txtExtCir(ii))
                If nCir > 0 And g_iMGlvRightFirstTape = -1 Then g_iMGlvRightFirstTape = ii - 7
                ''---------g_nRightCir(ii - 7) = nCir
                g_nRightCir(1, ii - 7) = nCir
            Next ii

            For ii = 23 To 8 Step -1
                If Val(txtExtCir(ii).Text) > 0 Then
                    g_iLastTape = ii - 7
                    Exit For
                End If
            Next ii

            'Total number of tapes
            If (g_iLastTape = g_iMGlvRightFirstTape) Then
                If g_iLastTape <> -1 Then g_iRightNumTotalTapes = 1
            Else
                g_iRightNumTotalTapes = (g_iLastTape - g_iMGlvRightFirstTape) + 1
            End If
            'Get values from "Above Wrist" TAB
            'Wrist tape
            'cboDistalTape.ListIndex = 0 => use first (Defaults to this)
            iIndex = cboDistalTape.SelectedIndex
            If iIndex = 0 Or iIndex = -1 Then
                g_iRightWristPointer = g_iMGlvRightFirstTape
            Else
                g_iRightWristPointer = iIndex
            End If

            'EOS tape
            'cboProximalTape.ListIndex = 0 => use last (Defaults to this)
            iIndex = cboProximalTape.SelectedIndex
            If iIndex = 0 Or iIndex = -1 Then
                g_iRightEOSPointer = g_iLastTape
            Else
                'NB. The order of tapes is reversed in this list
                'starting at 19-1/2 and finishing at 0
                g_iRightEOSPointer = 17 - iIndex
            End If

            'Number of tapes between
            If (g_iRightWristPointer = g_iRightEOSPointer) Then
                'Just in case there are no tapes
                'And "first" and "last" are used for Wrist and EOS
                If g_iLastTape <> -1 Then g_iRightNumTapesWristToEOS = 1
            Else
                g_iRightNumTapesWristToEOS = (g_iRightEOSPointer - g_iRightWristPointer) + 1
            End If

            'Pleats
            If txtWristPleat1.Enabled = True Then g_nRightPleats(1) = Val(txtWristPleat1.Text) Else g_nRightPleats(1) = 0
            If txtWristPleat2.Enabled = True Then g_nRightPleats(2) = Val(txtWristPleat2.Text) Else g_nRightPleats(2) = 0
            If txtShoulderPleat2.Enabled = True Then g_nRightPleats(3) = Val(txtShoulderPleat2.Text) Else g_nRightPleats(3) = 0
            If txtShoulderPleat1.Enabled = True Then g_nRightPleats(4) = Val(txtShoulderPleat1.Text) Else g_nRightPleats(4) = 0

            g_iRightPressure = cboPressure.SelectedIndex
        End If

        'End of support type
        g_RightEOSType = RIGHT_ARM_PLAIN
        If g_RightExtendTo = RIGHT_GLOVE_AXILLA And optProximalTape(1).Checked Then
            g_RightEOSType = RIGHT_ARM_FLAP
            g_sRightFlapType = cboFlaps.Text
            g_iRightFlapType = cboFlaps.SelectedIndex
            If txtStrapLength.Text <> "" Then g_nRightStrapLength = MANGLOVERight_bas.FN_InchesValue(txtStrapLength)
            If txtFrontStrapLength.Text <> "" Then MANGLOVERight_bas.g_nFrontStrapLength = MANGLOVERight_bas.FN_InchesValue(txtFrontStrapLength)
            If txtCustFlapLength.Text <> "" Then g_nRightCustFlapLength = MANGLOVERight_bas.FN_InchesValue(txtCustFlapLength)
            If txtWaistCir.Text <> "" Then MANGLOVERight_bas.g_nWaistCir = MANGLOVERight_bas.FN_InchesValue(txtWaistCir)
        End If

        'On or off fold
        g_RightOnFold = False
    End Sub
    Sub PR_GetExtensionDDE_Data()
        'Procedure to use the data given in the Form for the
        'glove extension (if any) along the arm
        '
        Dim nAge, Flap As Short
        Dim jj, ii, nn As Short
        Dim nValue As Double
        Dim iGms, iMMs, iRed As Short
        Dim sDiag As String

        Dim g_nRightCir(1, RIGHT_NOFF_ARMTAPES) As Double
        ''------Dim g_iMGlvRightFirstTape As Integer = -1
        Dim g_iLastTape As Integer = -1
        ''----------Dim g_iRightWristPointer As Integer = -1
        ''-------------Dim g_iRightEOSPointer As Integer = -1
        ''-----------Dim g_iRightNumTotalTapes As Integer = 0
        ''-----------Dim g_iRightNumTapesWristToEOS As Integer = 0
        ''----------Dim g_iRightPressure As Integer = 1

        ''------------CType(MainForm.Controls("SSTab1"), Object).Tab = 1

        'Defaults
        optExtendTo(0).Checked = True 'Normal Glove
        cboDistalTape.SelectedIndex = 0 '1st Tape
        cboProximalTape.SelectedIndex = 0 'Last Tape
        Flap = False

        'Set default pressure w.r.t diagnosis
        nAge = Val(txtAge.Text)

        ' if pressure empty set from diagnosis
        sDiag = UCase(Mid(txtDiagnosis.Text, 1, 6))
        'Arterial Insufficiency
        If sDiag = "ARTERI" Then
            g_iRightPressure = 0
            'Assist fluid dynamics
        ElseIf sDiag = "ASSIST" Then
            g_iRightPressure = 1
            'Blood Clots
        ElseIf sDiag = "BLOOD " Then
            g_iRightPressure = 1
            'Burns
        ElseIf sDiag = "BURNS " Or sDiag = "BURNS" Then
            g_iRightPressure = 0
            'Cancer
        ElseIf sDiag = "CANCER" Then
            g_iRightPressure = 1
            'Cardial Vascular Arrest
        ElseIf sDiag = "CARDIA" Then
            g_iRightPressure = 0
            'Carpal Tunnel Syndrome
        ElseIf sDiag = "CARPAL" Then
            g_iRightPressure = 1
            'Cellulitis
        ElseIf sDiag = "CELLUL" Then
            g_iRightPressure = 1
            'Chronic Venous Insufficiency
        ElseIf sDiag = "CHRONI" Then
            g_iRightPressure = 1
            'Heart Condition
        ElseIf sDiag = "HEART " Then
            g_iRightPressure = 0
            'Hemangioma
        ElseIf sDiag = "HEMANG" Then
            g_iRightPressure = 0
            'Lymphedema, Lymphedema 1+, Lymphedema 2+
            'N.B. Lymphedema <=> Lymphedema 1+
        ElseIf sDiag = "LYMPHE" Then
            If InStr(txtDiagnosis.Text, "2") > 0 Then
                If nAge <= 70 Then
                    g_iRightPressure = 2
                Else
                    g_iRightPressure = 1
                End If
            Else
                If nAge <= 70 Then
                    g_iRightPressure = 1
                Else
                    g_iRightPressure = 0
                End If
            End If
            'Night Wear
        ElseIf sDiag = "NIGHT " Then
            g_iRightPressure = 0
            'Post Fracture
        ElseIf sDiag = "POST F" Then
            g_iRightPressure = 1
            'Post Mastectomy
        ElseIf sDiag = "POST M" Then
            g_iRightPressure = 1
            'Postphlebetic Syndrome
        ElseIf sDiag = "POSTPH" Then
            g_iRightPressure = 1
            'Renal Disease (Kidney)
        ElseIf sDiag = "RENAL " Then
            g_iRightPressure = 1
            'Skin Graft
        ElseIf sDiag = "SKIN G" Then
            g_iRightPressure = 0
            'Stroke
        ElseIf sDiag = "STROKE" Then
            g_iRightPressure = 0
            'Tendonitis
        ElseIf sDiag = "TENDON" Then
            g_iRightPressure = 0
            'Thrombophlebitis
        ElseIf sDiag = "THROMB" Then
            g_iRightPressure = 1
            'Trauma
        ElseIf sDiag = "TRAUMA" Then
            g_iRightPressure = 1
            'Varicose Veins
        ElseIf sDiag = "VARICO" Then
            g_iRightPressure = 1
            'Request for 30 m/m, 40 m/m and 50 m/m
        ElseIf sDiag = "REQUES" Then
            If InStr(txtDiagnosis.Text, "30") > 0 Then
                g_iRightPressure = 0
            ElseIf InStr(txtDiagnosis.Text, "40") > 0 Then
                g_iRightPressure = 1
            ElseIf InStr(txtDiagnosis.Text, "50") > 0 Then
                g_iRightPressure = 2
            End If
            'Vein Removal
        ElseIf sDiag = "VEIN R" Then
            g_iRightPressure = 1
        End If

        'Glove extensions
        'Code for extended glove
        'Glove Option Buttons
        Dim nLen As Double

        If txtDataGlove.Text <> "" Then
            For ii = 0 To 5
                nValue = Val(Mid(txtDataGlove.Text, (ii * 2) + 1, 2))
                If ii = 0 Then
                    'Fold options
                    'Do nothing Off fold is default
                ElseIf ii = 1 And nValue >= 0 Then
                    'Pressure w.r.t Figuring and MMs
                    g_iRightPressure = nValue
                ElseIf ii = 2 Then
                    'Wrist Tape
                    cboDistalTape.SelectedIndex = nValue
                ElseIf ii = 3 Then
                    'Proximal Tape
                    cboProximalTape.SelectedIndex = nValue
                ElseIf ii = 4 Then
                    optExtendTo(nValue).Checked = True
                    If nValue = 2 Then Flap = True
                ElseIf ii = 5 Then
                    'Only if glove to axilla is set do we
                    'do anything
                    If Flap And nValue = 1 Then
                        'Flap, break up flap multiple field
                        optProximalTape(1).Checked = True
                        PR_ProximalTape_Click((1))
                        For jj = 0 To 4
                            nValue = Val(Mid(txtFlap.Text, (jj * 3) + 1, 3))
                            If jj = 0 Then
                                cboFlaps.SelectedIndex = nValue
                            ElseIf jj = 1 And nValue > 0 Then
                                g_nRightStrapLength = nValue / 10
                                txtStrapLength.Text = CStr(g_nRightStrapLength)
                                PR_DisplayRightTextInches(txtStrapLength, labStrap)
                            ElseIf jj = 2 And nValue > 0 Then
                                MANGLOVERight_bas.g_nFrontStrapLength = nValue / 10
                                txtFrontStrapLength.Text = CStr(MANGLOVERight_bas.g_nFrontStrapLength)
                                PR_DisplayRightTextInches(txtFrontStrapLength, labFrontStrapLength)
                            ElseIf jj = 3 And nValue > 0 Then
                                g_nRightCustFlapLength = nValue / 10
                                txtCustFlapLength.Text = CStr(g_nRightCustFlapLength)
                                PR_DisplayRightTextInches(txtCustFlapLength, labCustFlapLength)
                            ElseIf jj = 4 And nValue > 0 Then
                                MANGLOVERight_bas.g_nWaistCir = nValue / 10
                                txtWaistCir.Text = CStr(MANGLOVERight_bas.g_nWaistCir)
                                PR_DisplayRightTextInches(txtWaistCir, labWaistCir)
                            End If
                        Next jj
                    Else
                        'Disable flaps
                        PR_ProximalTape_Click((0))
                    End If
                End If
            Next ii
        End If

        'Set value for pressure
        cboPressure.SelectedIndex = g_iRightPressure

        'Glove to elbow and Glove to axilla
        'These can start at -3 (However to allow for possible
        'changes later we save up to -4-1/2 but ignore -4-1/2 for the
        'mean time)
        If txtTapeLengthPt1.Text <> "" Then
            ii = 1
            For nn = 8 To 23
                nValue = Val(Mid(txtTapeLengthPt1.Text, (ii * 3) + 1, 3))
                If nValue > 0 Then
                    txtExtCir(nn).Text = nValue / 10
                    nLen = MANGLOVERight_bas.FN_InchesValue(txtExtCir(nn))
                    ''------------------If nLen <> -1 Then PR_GrdInchesDisplay(nn - 8, nLen)
                    ''--------g_nRightCir(0, ii) = nLen
                    If nLen <> -1 Then lblExtCir(nn - 8).Text = MANGLOVERight_bas.fnInchestoText(nLen)
                    g_nRightCir(1, ii) = nLen
                Else
                    ''-------------------PR_GrdInchesDisplay(nn - 8, 0)
                    lblExtCir(nn - 8).Text = ""
                End If
                iMMs = Val(Mid(txtTapeMMs.Text, (ii * 3) + 1, 3))
                If iMMs > 0 Then
                    iGms = Val(Mid(txtGrams.Text, (ii * 3) + 1, 3))
                    iRed = Val(Mid(txtReduction.Text, (ii * 3) + 1, 3))
                    mms(nn).Text = CStr(iMMs)
                    ''--------------PR_GramRedDisplay(nn - 8, iGms, iRed)
                    lblGms(nn - 8).Text = CStr(iGms)
                    lblRed(nn - 8).Text = CStr(iRed)
                    MANGLOVERight_bas.g_iMMs(ii) = iMMs
                    MANGLOVERight_bas.g_iRed(ii) = iRed
                    MANGLOVERight_bas.g_iGms(ii) = iGms
                End If
                ii = ii + 1
            Next nn
        End If

        'Pleats
        For ii = 0 To 1
            nValue = Val(Mid(txtWristPleat.Text, (ii * 3) + 1, 3))
            If ii = 0 And nValue > 0 Then
                txtWristPleat1.Text = CStr(nValue / 10)
            ElseIf ii = 1 And nValue > 0 Then
                txtWristPleat2.Text = CStr(nValue / 10)
            End If
            nValue = Val(Mid(txtShoulderPleat.Text, (ii * 3) + 1, 3))
            If ii = 0 And nValue > 0 Then
                txtShoulderPleat1.Text = CStr(nValue / 10)
            ElseIf ii = 1 And nValue > 0 Then
                txtShoulderPleat2.Text = CStr(nValue / 10)
            End If
        Next ii
    End Sub
    Sub PR_ProximalTape_Click(ByRef INDEX As Short)
        Dim ii As Short
        If INDEX = 0 Then
            'Disable flaps
            g_iRightFlapType = cboFlaps.SelectedIndex
            cboFlaps.Enabled = False
            cboFlaps.SelectedIndex = -1
            For ii = 0 To 4
                lblFlap(ii).Enabled = False
            Next ii
            txtStrapLength.Enabled = False
            g_nRightStrapLength = Val(txtStrapLength.Text)
            txtStrapLength.Text = ""
            labStrap.Text = ""

            txtFrontStrapLength.Enabled = False
            MANGLOVERight_bas.g_nFrontStrapLength = Val(txtFrontStrapLength.Text)
            txtFrontStrapLength.Text = ""
            labFrontStrapLength.Text = ""

            txtCustFlapLength.Enabled = False
            g_nRightCustFlapLength = Val(txtCustFlapLength.Text)
            txtCustFlapLength.Text = ""
            labCustFlapLength.Text = ""

            txtWaistCir.Enabled = False
            MANGLOVERight_bas.g_nWaistCir = Val(txtWaistCir.Text)
            txtWaistCir.Text = ""
            labWaistCir.Text = ""
        Else
            'Enable flaps
            cboFlaps.Enabled = True
            cboFlaps.SelectedIndex = g_iRightFlapType

            For ii = 0 To 3
                lblFlap(ii).Enabled = True
            Next ii
            txtStrapLength.Enabled = True
            If g_nRightStrapLength > 0 Then
                txtStrapLength.Text = CStr(g_nRightStrapLength)
                PR_DisplayRightTextInches(txtStrapLength, labStrap)
            End If

            txtFrontStrapLength.Enabled = True
            If MANGLOVERight_bas.g_nFrontStrapLength > 0 Then
                txtFrontStrapLength.Text = CStr(MANGLOVERight_bas.g_nFrontStrapLength)
                PR_DisplayRightTextInches(txtFrontStrapLength, labFrontStrapLength)
            End If

            txtCustFlapLength.Enabled = True
            If g_nRightCustFlapLength > 0 Then
                txtCustFlapLength.Text = CStr(g_nRightCustFlapLength)
                PR_DisplayRightTextInches(txtCustFlapLength, labCustFlapLength)
            End If

            If InStr(1, cboFlaps.Text, "D") > 0 Then
                lblFlap(4).Enabled = True
                txtWaistCir.Enabled = True
                If MANGLOVERight_bas.g_nWaistCir > 0 Then
                    txtWaistCir.Text = CStr(MANGLOVERight_bas.g_nWaistCir)
                    PR_DisplayRightTextInches(txtWaistCir, labWaistCir)
                End If
            End If
        End If
    End Sub
    Private Sub saveInfoToDWG()
        Try
            Dim _sClass As New SurroundingClass()
            If (_sClass.GetXrecord("ManGloveRight", "RIGHTGLOVEDIC") IsNot Nothing) Then
                _sClass.RemoveXrecord("ManGloveRight", "RIGHTGLOVEDIC")
            End If

            Dim resbuf As New ResultBuffer

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtCir_12.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtLen_8.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboFabric.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboInsert.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkSlantedInserts.CheckState))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkFusedFingers.CheckState))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkPalm.CheckState))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkDorsal.CheckState))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), chkPrintFold.CheckState))

            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optInsertStyle_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optInsertStyle_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optInsertStyle_2.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optInsertStyle_3.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optFold_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optFold_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optExtendTo_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optExtendTo_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optExtendTo_2.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optThumbStyle_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optThumbStyle_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optThumbStyle_2.Checked))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblCir_12.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_0.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_3.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_4.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_5.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_6.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_7.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _lblLen_8.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optLittleTip_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optLittleTip_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optLittleTip_2.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optRingTip_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optRingTip_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optRingTip_2.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optMiddleTip_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optMiddleTip_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optMiddleTip_2.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optIndexTip_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optIndexTip_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optIndexTip_2.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optThumbTip_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optThumbTip_1.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optThumbTip_2.Checked))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_12.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_13.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_14.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_15.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_16.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_17.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_18.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_19.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_20.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_21.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_22.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _txtExtCir_23.Text))

            Dim ii As Short
            For ii = 0 To 15
                resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblExtCir(ii).Text))
            Next ii

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_8.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_9.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_10.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_11.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_12.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_13.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_14.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_15.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_16.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_17.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_18.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_19.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_20.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_21.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_22.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), _mms_23.Text))

            For ii = 0 To 15
                resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblGms(ii).Text))
            Next ii
            For ii = 0 To 15
                resbuf.Add(New TypedValue(CInt(DxfCode.Text), lblRed(ii).Text))
            Next ii

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtWristPleat1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtWristPleat2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtShoulderPleat1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtShoulderPleat2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboDistalTape.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboProximalTape.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboPressure.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optProximalTape_0.Checked))
            resbuf.Add(New TypedValue(CInt(DxfCode.Bool), _optProximalTape_1.Checked))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), cboFlaps.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtStrapLength.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtFrontStrapLength.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtCustFlapLength.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtWaistCir.Text))

            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtFabric.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtTapeLengths.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtTapeLengths2.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtTapeLengthPt1.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtDataGlove.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtWristPleat.Text))
            resbuf.Add(New TypedValue(CInt(DxfCode.Text), txtShoulderPleat.Text))

            _sClass.SetXrecord(resbuf, "ManGloveRight", "RIGHTGLOVEDIC")
        Catch ex As Exception
        End Try
    End Sub
    Private Sub readDWGInfo()
        Try

            Dim _sClass As New SurroundingClass()
            Dim resbuf As New ResultBuffer
            resbuf = _sClass.GetXrecord("ManGloveRight", "RIGHTGLOVEDIC")
            If (resbuf IsNot Nothing) Then
                Dim arr() As TypedValue = resbuf.AsArray()
                _txtCir_0.Text = arr(0).Value
                _txtCir_1.Text = arr(1).Value
                _txtCir_2.Text = arr(2).Value
                _txtCir_3.Text = arr(3).Value
                _txtCir_4.Text = arr(4).Value
                _txtCir_5.Text = arr(5).Value
                _txtCir_6.Text = arr(6).Value
                _txtCir_7.Text = arr(7).Value
                _txtCir_8.Text = arr(8).Value
                _txtCir_9.Text = arr(9).Value
                _txtCir_10.Text = arr(10).Value
                _txtCir_11.Text = arr(11).Value
                _txtCir_12.Text = arr(12).Value
                _txtLen_0.Text = arr(13).Value
                _txtLen_1.Text = arr(14).Value
                _txtLen_2.Text = arr(15).Value
                _txtLen_3.Text = arr(16).Value
                _txtLen_4.Text = arr(17).Value
                _txtLen_5.Text = arr(18).Value
                _txtLen_6.Text = arr(19).Value
                _txtLen_7.Text = arr(20).Value
                _txtLen_8.Text = arr(21).Value
                cboFabric.Text = arr(22).Value
                cboInsert.Text = arr(23).Value
                chkSlantedInserts.CheckState = arr(24).Value
                chkFusedFingers.CheckState = arr(25).Value
                chkPalm.CheckState = arr(26).Value
                chkDorsal.CheckState = arr(27).Value
                chkPrintFold.CheckState = arr(28).Value
                _optInsertStyle_0.Checked = arr(29).Value
                _optInsertStyle_1.Checked = arr(30).Value
                _optInsertStyle_2.Checked = arr(31).Value
                _optInsertStyle_3.Checked = arr(32).Value
                _optFold_1.Checked = arr(33).Value
                _optFold_0.Checked = arr(34).Value
                _optExtendTo_0.Checked = arr(35).Value
                _optExtendTo_1.Checked = arr(36).Value
                _optExtendTo_2.Checked = arr(37).Value
                _optThumbStyle_0.Checked = arr(38).Value
                _optThumbStyle_1.Checked = arr(39).Value
                _optThumbStyle_2.Checked = arr(40).Value

                _lblCir_0.Text = arr(41).Value
                _lblCir_1.Text = arr(42).Value
                _lblCir_2.Text = arr(43).Value
                _lblCir_3.Text = arr(44).Value
                _lblCir_4.Text = arr(45).Value
                _lblCir_5.Text = arr(46).Value
                _lblCir_6.Text = arr(47).Value
                _lblCir_7.Text = arr(48).Value
                _lblCir_8.Text = arr(49).Value
                _lblCir_9.Text = arr(50).Value
                _lblCir_10.Text = arr(51).Value
                _lblCir_11.Text = arr(52).Value
                _lblCir_12.Text = arr(53).Value

                _lblLen_0.Text = arr(54).Value
                _lblLen_1.Text = arr(55).Value
                _lblLen_2.Text = arr(56).Value
                _lblLen_3.Text = arr(57).Value
                _lblLen_4.Text = arr(58).Value
                _lblLen_5.Text = arr(59).Value
                _lblLen_6.Text = arr(60).Value
                _lblLen_7.Text = arr(61).Value
                _lblLen_8.Text = arr(62).Value

                _optLittleTip_0.Checked = arr(63).Value
                _optLittleTip_1.Checked = arr(64).Value
                _optLittleTip_2.Checked = arr(65).Value

                _optRingTip_0.Checked = arr(66).Value
                _optRingTip_1.Checked = arr(67).Value
                _optRingTip_2.Checked = arr(68).Value

                _optMiddleTip_0.Checked = arr(69).Value
                _optMiddleTip_1.Checked = arr(70).Value
                _optMiddleTip_2.Checked = arr(71).Value

                _optIndexTip_0.Checked = arr(72).Value
                _optIndexTip_1.Checked = arr(73).Value
                _optIndexTip_2.Checked = arr(74).Value

                _optThumbTip_0.Checked = arr(75).Value
                _optThumbTip_1.Checked = arr(76).Value
                _optThumbTip_2.Checked = arr(77).Value

                _txtExtCir_8.Text = arr(78).Value
                _txtExtCir_9.Text = arr(79).Value
                _txtExtCir_10.Text = arr(80).Value
                _txtExtCir_11.Text = arr(81).Value
                _txtExtCir_12.Text = arr(82).Value
                _txtExtCir_13.Text = arr(83).Value
                _txtExtCir_14.Text = arr(84).Value
                _txtExtCir_15.Text = arr(85).Value
                _txtExtCir_16.Text = arr(86).Value
                _txtExtCir_17.Text = arr(87).Value
                _txtExtCir_18.Text = arr(88).Value
                _txtExtCir_19.Text = arr(89).Value
                _txtExtCir_20.Text = arr(90).Value
                _txtExtCir_21.Text = arr(91).Value
                _txtExtCir_22.Text = arr(92).Value
                _txtExtCir_23.Text = arr(93).Value

                Dim ii As Short
                For ii = 0 To 15
                    lblExtCir(ii).Text = arr(ii + 94).Value
                Next ii

                _mms_8.Text = arr(110).Value
                _mms_9.Text = arr(111).Value
                _mms_10.Text = arr(112).Value
                _mms_11.Text = arr(113).Value
                _mms_12.Text = arr(114).Value
                _mms_13.Text = arr(115).Value
                _mms_14.Text = arr(116).Value
                _mms_15.Text = arr(117).Value
                _mms_16.Text = arr(118).Value
                _mms_17.Text = arr(119).Value
                _mms_18.Text = arr(120).Value
                _mms_19.Text = arr(121).Value
                _mms_20.Text = arr(122).Value
                _mms_21.Text = arr(123).Value
                _mms_22.Text = arr(124).Value
                _mms_23.Text = arr(125).Value

                For ii = 0 To 15
                    lblGms(ii).Text = arr(ii + 126).Value
                    MANGLOVERight_bas.g_iGms(ii + 1) = Val(lblGms(ii).Text)
                    MANGLOVERight_bas.g_iMMs(ii + 1) = Val(mms(ii + 8).Text)
                Next ii
                For ii = 0 To 15
                    lblRed(ii).Text = arr(ii + 142).Value
                    MANGLOVERight_bas.g_iRed(ii + 1) = Val(lblRed(ii).Text)
                Next ii

                txtWristPleat1.Text = arr(158).Value
                txtWristPleat2.Text = arr(159).Value
                txtShoulderPleat1.Text = arr(160).Value
                txtShoulderPleat2.Text = arr(161).Value
                cboDistalTape.Text = arr(162).Value
                cboProximalTape.Text = arr(163).Value
                cboPressure.Text = arr(164).Value
                _optProximalTape_0.Checked = arr(165).Value
                _optProximalTape_1.Checked = arr(166).Value

                cboFlaps.Text = arr(167).Value
                txtStrapLength.Text = arr(168).Value
                txtFrontStrapLength.Text = arr(169).Value
                txtCustFlapLength.Text = arr(170).Value
                txtWaistCir.Text = arr(171).Value
            End If

        Catch ex As Exception

        End Try
    End Sub
    Sub PR_StartPoly()
        'To the DRAFIX macro file (given by the global MANGLOVERight_bas.fNum)
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
        '    MANGLOVERight_bas.fNum, CC, QQ, NL are globals initialised by FN_Open
        '
        '

        'UPGRADE_WARNING: Couldn't resolve default property of object MANGLOVERight_bas.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(MANGLOVERight_bas.fNum, "StartPoly (""PolyLine"");")


    End Sub
    Sub PR_AddVertex(ByRef xyPoint As MANGLOVERight_bas.XY, ByRef nBulge As Double)
        'To the DRAFIX macro file (given by the global MANGLOVERight_bas.fNum)
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
        '    MANGLOVERight_bas.fNum, CC, QQ, NL,  are globals initialised by FN_Open
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object MANGLOVERight_bas.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(MANGLOVERight_bas.fNum, "  AddVertex(" & "xyStart.x+" & xyPoint.X & CC & "xyStart.y+" & xyPoint.y & CC & "0,0," & nBulge & ");")

    End Sub
    Sub PR_EndPoly()
        'To the DRAFIX macro file (given by the global MANGLOVERight_bas.fNum)
        'write the syntax to end a POLYLINE
        'For this to work it assumes that the following DRAFIX variables
        'are defined and initialised
        '    XY      xyStart
        '    HANDLE  hEnt
        '    STRING  sID
        '
        'Note:-
        '    MANGLOVERight_bas.fNum, CC, QQ, NL,  are globals initialised by FN_Open
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object MANGLOVERight_bas.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(MANGLOVERight_bas.fNum, "EndPoly();")
        'UPGRADE_WARNING: Couldn't resolve default property of object MANGLOVERight_bas.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(MANGLOVERight_bas.fNum, "hEnt = UID (""find"", UID (""getmax"")) ;")
        'UPGRADE_WARNING: Couldn't resolve default property of object MANGLOVERight_bas.fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(MANGLOVERight_bas.fNum, "SetDBData(hEnt," & QQ & "ID" & QQ & ",sID);")

    End Sub
    Private Sub PR_DrawGloveCommonBlock()
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
        xyEnd(1) = 0.6875
        xyStart(2) = 0
        xyEnd(2) = 0.875
        xyStart(3) = 1.5
        xyEnd(3) = 0.875
        xyStart(4) = 1.5
        xyEnd(4) = 0.6875
        xyStart(5) = 0
        xyEnd(5) = 0.6875
        Dim xyText As MANGLOVERight_bas.XY
        MANGLOVERight_bas.PR_MakeXY(xyText, 0.71875, 0.6875)
        Dim strTag(3), strTextString(3) As String
        strTag(1) = "Fabric"
        strTag(2) = "Data"
        strTag(3) = "fileno"
        strTextString(1) = txtFabric.Text
        strTextString(2) = txtDataGC.Text
        strTextString(3) = txtFileNo.Text

        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRefPatient As BlockReference = acTrans.GetObject(blkId, OpenMode.ForRead)
            Dim ptPosition As Point3d = blkRefPatient.Position
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has("GLOVECOMMON") Then
                Dim blkTblRecCross As BlockTableRecord = New BlockTableRecord()
                blkTblRecCross.Name = "GLOVECOMMON"
                Dim acPoly As Polyline = New Polyline()
                Dim ii As Double
                For ii = 1 To 5
                    acPoly.AddVertexAt(ii - 1, New Point2d(xyStart(ii), xyEnd(ii)), 0, 0, 0)
                Next ii
                blkTblRecCross.AppendEntity(acPoly)

                Dim acText As DBText = New DBText()
                acText.Position = New Point3d(xyText.X, xyText.y, 0)
                acText.Height = 0.1
                acText.TextString = "GLOVES"
                acText.Rotation = 0
                acText.Justify = AttachmentPoint.BottomCenter
                acText.AlignmentPoint = New Point3d(xyText.X, xyText.y, 0)
                blkTblRecCross.AppendEntity(acText)

                Dim acAttDef As New AttributeDefinition
                For ii = 1 To 3
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
                blkRecId = acBlkTbl("GLOVECOMMON")
            End If
            ' Insert the block into the current space
            If blkRecId <> ObjectId.Null Then
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
                            If acAttRef.Tag.ToUpper().Equals("Fabric", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFabric.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Data", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtDataGC.Text
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
    Private Sub PR_DrawGloveRightBlock()
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
        xyEnd(1) = 0.6875
        xyStart(2) = 0
        xyEnd(2) = 0.875
        xyStart(3) = 1.5
        xyEnd(3) = 0.875
        xyStart(4) = 1.5
        xyEnd(4) = 0.6875
        xyStart(5) = 0
        xyEnd(5) = 0.6875
        Dim xyText As MANGLOVERight_bas.XY
        MANGLOVERight_bas.PR_MakeXY(xyText, 0.71875, 0.6875)
        Dim strTag(12), strTextString(12) As String
        strTag(1) = "TapeLengths"
        strTag(2) = "TapeLengths2"
        strTag(3) = "TapeLengthPt1"
        strTag(4) = "fileno"
        strTag(5) = "Sleeve"
        strTag(6) = "Grams"
        strTag(7) = "Reduction"
        strTag(8) = "Tape MMs"
        strTag(9) = "Data"
        strTag(10) = "WristPleat"
        strTag(11) = "ShoulderPleat"
        strTag(12) = "Flap"

        strTextString(1) = txtTapeLengths.Text
        strTextString(2) = txtTapeLengths2.Text
        strTextString(3) = txtTapeLengthPt1.Text
        strTextString(4) = txtFileNo.Text
        strTextString(5) = txtSide.Text
        strTextString(6) = txtGrams.Text
        strTextString(7) = txtReduction.Text
        strTextString(8) = txtTapeMMs.Text
        strTextString(9) = txtDataGlove.Text
        strTextString(10) = txtWristPleat.Text
        strTextString(11) = txtShoulderPleat.Text
        strTextString(12) = txtFlap.Text

        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            Dim blkRefPatient As BlockReference = acTrans.GetObject(blkId, OpenMode.ForRead)
            Dim ptPosition As Point3d = blkRefPatient.Position
            Dim blkRecId As ObjectId = ObjectId.Null
            If Not acBlkTbl.Has("GLOVERIGHT") Then
                Dim blkTblRecCross As BlockTableRecord = New BlockTableRecord()
                blkTblRecCross.Name = "GLOVERIGHT"
                Dim acPoly As Polyline = New Polyline()
                Dim ii As Double
                For ii = 1 To 5
                    acPoly.AddVertexAt(ii - 1, New Point2d(xyStart(ii), xyEnd(ii)), 0, 0, 0)
                Next ii
                blkTblRecCross.AppendEntity(acPoly)

                Dim acText As DBText = New DBText()
                acText.Position = New Point3d(xyText.X, xyText.y, 0)
                acText.Height = 0.1
                acText.TextString = "GLOVE"
                acText.Rotation = 0
                acText.Justify = AttachmentPoint.BottomCenter
                acText.AlignmentPoint = New Point3d(xyText.X, xyText.y, 0)
                blkTblRecCross.AppendEntity(acText)

                Dim acAttDef As New AttributeDefinition
                For ii = 1 To 12
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
                blkRecId = acBlkTbl("GLOVERIGHT")
            End If
            ' Insert the block into the current space
            If blkRecId <> ObjectId.Null Then
                Dim blkRef As BlockReference = New BlockReference(New Point3d(ptPosition.X + 3.0, ptPosition.Y, 0), blkRecId)
                'If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                '    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(ptPosition.X + 3.0, ptPosition.Y, 0)))
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
                            If acAttRef.Tag.ToUpper().Equals("TapeLengths", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtTapeLengths.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("TapeLengths2", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtTapeLengths2.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("TapeLengthPt1", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtTapeLengthPt1.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("fileno", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFileNo.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Sleeve", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtSide.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Grams", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtGrams.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Reduction", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtReduction.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Tape MMs", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtTapeMMs.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Data", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtDataGlove.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("WristPleat", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtWristPleat.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("ShoulderPleat", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtShoulderPleat.Text
                            ElseIf acAttRef.Tag.ToUpper().Equals("Flap", StringComparison.InvariantCultureIgnoreCase) Then
                                acAttRef.TextString = txtFlap.Text
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
    Function fnDisplayToCM(ByRef nInches As Double) As String
        Dim nCMVal, nDec As Double
        Dim iInt As Short
        nCMVal = nInches * 2.54
        nCMVal = System.Math.Abs(nCMVal)
        iInt = Int(nCMVal)
        If iInt <> 0 Then
            nDec = (nCMVal - iInt) * 10
            nDec = ARMDIA1.round(nDec) / 10
            nCMVal = iInt + nDec
            fnDisplayToCM = Str(nCMVal)
        Else
            fnDisplayToCM = Str(1)
        End If
    End Function
End Class