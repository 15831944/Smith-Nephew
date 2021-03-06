Option Strict Off
Option Explicit On
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Public Module ARMDIA1
    'XY data type to represent points
    Structure XY
        Dim X As Double
        Dim y As Double
    End Structure

    Public Structure curve
        Dim n As Short
        <VBFixedArray(100)> Dim X() As Double
        <VBFixedArray(100)> Dim y() As Double

        'UPGRADE_TODO: "Initialize" must be called to initialize instances of this structure. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"'
        Public Sub Initialize()
            'UPGRADE_WARNING: Lower bound of array X was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim X(100)
            'UPGRADE_WARNING: Lower bound of array y was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim y(100)
        End Sub
    End Structure

    'PI as a Global Constant
    Public Const PI As Double = 3.141592654

    Public MainForm As armdia

    'Globals set by FN_Open
    Public CC As Object 'Comma
    Public QQ As Object 'Quote
    Public NL As Object 'Newline
    Public fNum As Object 'Macro file number
    Public QCQ As Object 'Quote Comma Quote
    Public QC As Object 'Quote Comma
    Public CQ As Object 'Comma Quote

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
    Public g_sThumbCircum As String
    Public g_sThumbLength As String
    Public g_sEnclosedThumb As String
    Public g_sSecondLastTape As String
    Public g_sSecondTape As String
    Public g_sFirstTape As String
    Public g_sLastTape As String
    Public g_sModulus As String
    Public g_sStump As String
    Public g_sTapeLengths As String
    Public g_sTapeMMs As String
    Public g_sGrams As String
    Public g_sReduction As String
    Public g_sAmm As Object
    Public g_sBmm As Object
    Public g_sCmm As Object
    Public g_sDmm As Object
    Public g_sWaistCir As Object

    Public g_sWorkOrder As String

    'Store current layer and text setings to reduce DRAFIX code
    'this value is checked in PR_SetLayer
    Public g_sCurrentLayer As String
    Public g_nCurrTextHt As Object
    Public g_nCurrTextAspect As Object
    Public g_nCurrTextHorizJust As Object
    Public g_nCurrTextVertJust As Object
    Public g_nCurrTextFont As Object
    Public g_nCurrTextAngle As Object

    Public g_sHoleCheck As String
    Public g_sOld As String

    Public g_sID As String
    Public g_sEditLengths As String
    Public g_sUnits As String


    Public g_iStyleFirstTape As Short
    Public g_iStyleLastTape As Short
    Public g_iFirstTape As Short
    Public g_iLastTape As Short
    Public g_nFrontStrapLength As Double
    Public g_nGauntletExtension As Double

    Public g_Modified As Short
    Public g_sPathJOBST As String

    Public g_bDrawVestMesh As Short
    Public g_bDrawBodyMesh As Short

    Public Detachable, WristNo, PalmNo, ThumLen, EnclosedThm As Object
    Public xyInsertion As XY

    Public g_iRightStyleFirstTape As Short
    Public g_iRightStyleLastTape As Short
    Public g_iRightFirstTape As Short
    Public g_iRightLastTape As Short
    Public g_sRightModulus As String
    Public g_sRightAmm As Object
    Public g_sRightBmm As Object
    Public g_sRightCmm As Object
    Public g_sRightDmm As Object
    Public g_sRightHoleCheck As String
    Public g_sRightPressureChange As String




    Sub DataBaseDataUpDate(ByRef sType As String)
        Dim sSymbol As Object
        Dim sString As String
        Dim sGauntlet, sFlap As String
        Dim sBoxType As String
        Dim nYoffset As Single

        Return '11-June-2018
        Dim _frmarmdia As New armdia()      'Choose data box depending on type
        If _frmarmdia.txtType.Text = "ARM" Then
            sBoxType = "armarm"
        Else
            sBoxType = "vestarm"
        End If

        'Get sleevebox handle
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "Close(" & QQ & "selection" & QC & "hChan);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "hChan = Open(" & QQ & "selection" & QCQ & "DB SymbolName = '" & sBoxType & "' AND DB Sleeve ='" & g_sSide & "'" & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "ResetSelection(hChan);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "hSleeve = GetNextSelection(hChan);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "Close(" & QQ & "selection" & QC & "hChan);")

        'If sleevebox not found then insert one
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "if(!hSleeve){")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    Close(" & QQ & "selection" & QC & "hChan);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    hChan = Open(" & QQ & "selection" & QCQ & "DB SymbolName = 'mainpatientdetails'" & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    ResetSelection(hChan);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    hTitle = GetNextSelection(hChan);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    Close(" & QQ & "selection" & QC & "hChan);")
        'Get title box origin
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    GetGeometry(hTitle,&sTitleName, &xyTitleOrigin,&xyTitleScale,&aTitleAngle);")

        'Insert arm or sleeve box
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    if ( !Symbol(" & QQ & "find" & QCQ & sBoxType & QQ & ")) Exit(%cancel," & QQ & "Cant find SLEEVEBOX or ARMBOX symbol to insert|nCheck your installation, that JOBST.SLB exists" & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    if(StringCompare(" & QQ & "Left" & QCQ & g_sSide & QQ & ")) xyTitleOrigin.x = xyTitleOrigin.x + 1.5;")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "        else xyTitleOrigin.x = xyTitleOrigin.x + 3;")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    Execute(" & QQ & "menu" & QCQ & "SetLayer" & QC & "Table(" & QQ & "find" & QCQ & "layer" & QCQ & "Data" & QQ & "));")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "    hSleeve = AddEntity(" & QQ & "symbol" & QCQ & sBoxType & QC & "xyTitleOrigin);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "}")

        'Insert Arm common
        'UPGRADE_WARNING: Couldn't resolve default property of object sSymbol. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sSymbol = "armcommon"
        If _frmarmdia.txtUidAC.Text = "" Then
            'Insert a new symbol
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "if ( Symbol(" & QQ & "find" & QCQ & sSymbol & QQ & ")){")
            '        Print #fNum, "if(!hSleeve){"
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  Close(" & QQ & "selection" & QC & "hChan);")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  hChan = Open(" & QQ & "selection" & QCQ & "DB SymbolName = 'mainpatientdetails'" & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  ResetSelection(hChan);")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  hTitle = GetNextSelection(hChan);")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  Close(" & QQ & "selection" & QC & "hChan);")
            'Get title box origin
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  GetGeometry(hTitle,&sTitleName, &xyTitleOrigin,&xyTitleScale,&aTitleAngle);")

            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  Execute (" & QQ & "menu" & QCQ & "SetLayer" & QC & "Table(" & QQ & "find" & QCQ & "layer" & QCQ & "Data" & QQ & "));")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  hSym = AddEntity(" & QQ & "symbol" & QCQ & sSymbol & QC & "xyTitleOrigin.x, xyTitleOrigin.y);")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  }")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "else")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  Exit(%cancel, " & QQ & "Can't find >" & sSymbol & "< symbol to insert\nCheck your installation, that JOBST.SLB exists!" & QQ & ");")
        Else
            'Use existing symbol
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "hSym = UID (" & QQ & "find" & QC & Val(_frmarmdia.txtUidAC.Text) & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "if (!hSym) Exit(%cancel," & QQ & "Can't find >" & sSymbol & "< symbol to update!" & QQ & ");")
        End If
        'Update DB fields
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hSym" & CQ & "Fabric" & QCQ & _frmarmdia.txtFabric.Text & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData( hSym" & CQ & "fileno" & QCQ & _frmarmdia.txtFileNo.Text & QQ & ");")






        'Flap multiple field
        sString = New String(" ", 35)
        sString = LSet(g_sFlap, Len(sString))
        If Val(_frmarmdia.txtCustFlapLength.Text) = 0 Then _frmarmdia.txtCustFlapLength.Text = CStr(-1)
        If Val(_frmarmdia.txtWaistCir.Text) = 0 Then _frmarmdia.txtWaistCir.Text = "-1"
        If Val(_frmarmdia.txtFrontStrapLength.Text) = 0 Then _frmarmdia.txtFrontStrapLength.Text = "-1"
        sFlap = sString & _frmarmdia.txtStrap.Text & " " & _frmarmdia.txtCustFlapLength.Text & " " & _frmarmdia.txtWaistCir.Text & " " & _frmarmdia.txtFrontStrapLength.Text

        'Gauntlet Multiple field
        If g_sGaunt = "False" Then
            sGauntlet = "0"
        Else
            'Set falgs
            sString = "1 "
            If g_sEnclosedThumb = "True" Then
                sString = sString & "1 "
            Else
                sString = sString & "0 "
            End If

            If g_sDetGaunt = "True" Then
                sString = sString & "1 "
            Else
                sString = sString & "0 "
            End If

            If g_sNoThumb = "True" Then
                sString = sString & "1 "
            Else
                sString = sString & "0 "
            End If
            If Val(_frmarmdia.txtGauntletExtension.Text) = 0 Then _frmarmdia.txtGauntletExtension.Text = "-1"
            'Data
            sGauntlet = sString & Val(g_sWristNo) & " " & Val(g_sPalmNo) & " " & Val(g_sThumbLength) & " " & Val(g_sThumbCircum) & " " & Val(g_sPalmWristDist) & " " & _frmarmdia.txtGauntletExtension.Text
        End If

        'Update data base fields
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Sleeve" & QCQ & g_sSide & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Fileno" & QCQ & g_sFileNo & QQ & ");")

        'Wrist and Shoulder pleat multiple fields
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "WristPleat" & QCQ & g_sWpleat1 & " " & g_sWpleat2 & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "ShoulderPleat" & QCQ & g_sSpleat1 & " " & g_sSpleat2 & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Flap" & QCQ & sFlap & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Gauntlet" & QCQ & sGauntlet & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "MM" & QCQ & g_sMM & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Modulus" & QCQ & g_sModulus & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Fabric" & QCQ & g_sFabric & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Stump" & QCQ & g_sStump & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "TapeMMs" & QCQ & g_sTapeMMs & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Reduction" & QCQ & g_sReduction & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "Grams" & QCQ & g_sGrams & QQ & ");")

        'Tape lengths are universal, Store the actually used lengths with the
        'Profile Origin Marker
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "TapeLengths" & QCQ & g_sTapeLengths & QQ & ");")
        'Store ID of last drawn
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetDBData(hSleeve," & QQ & "ID" & QCQ & g_sID & QQ & ");")


        If sType = "Draw" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "Fabric" & QCQ & g_sFabric & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "Gauntlet" & QCQ & sGauntlet & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "TapeLengths" & QCQ & g_sEditLengths & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "units" & QCQ & g_sUnits & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "WristPleat" & QCQ & g_sWpleat1 & " " & g_sWpleat2 & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "ShoulderPleat" & QCQ & g_sSpleat1 & " " & g_sSpleat2 & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "Flap" & QCQ & sFlap & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "Grams" & QCQ & g_sGrams & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "Reduction" & QCQ & g_sReduction & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "TapeMMs" & QCQ & g_sTapeMMs & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "Stump" & QCQ & g_sStump & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "Modulus" & QCQ & g_sModulus & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "MM" & QCQ & g_sMM & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetDBData(hOrigin," & QQ & "age" & QCQ & CType(_frmarmdia.Controls("txtAge"), Object).Text & QQ & ");")
        End If

    End Sub



    Function FN_CalcAngle(ByRef xyStart As XY, ByRef xyEnd As XY) As Double
        'Function to return the angle between two points in degrees
        'in the range 0 - 360
        'Zero is always 0 and is never 360

        Dim X, y As Object
        Dim rAngle As Double

        'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        X = xyEnd.X - xyStart.X
        'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        y = xyEnd.y - xyStart.y

        'Horizomtal
        'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If X = 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If y > 0 Then
                FN_CalcAngle = 90
            Else
                FN_CalcAngle = 270
            End If
            Exit Function
        End If

        'Vertical (avoid divide by zero later)
        'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If y = 0 Then
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
        'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rAngle = System.Math.Atan(y / X) * (180 / PI) 'Convert to degrees

        If rAngle < 0 Then rAngle = rAngle + 180 'rAngle range is -PI/2 & +PI/2

        'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If y > 0 Then
            FN_CalcAngle = rAngle
        Else
            FN_CalcAngle = rAngle + 180
        End If

    End Function

    Function FN_CalcLength(ByRef xyStart As XY, ByRef xyEnd As XY) As Double
        'Fuction to return the length between two points
        'Greatfull thanks to Pythagorus

        FN_CalcLength = System.Math.Sqrt((xyEnd.X - xyStart.X) ^ 2 + (xyEnd.y - xyStart.y) ^ 2)

    End Function

    Function FN_EscapeQuotesInString(ByRef sAssignedString As Object) As String
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
            If Char_Renamed = """" Then
                sEscapedString = sEscapedString & "\" & Char_Renamed
            Else
                sEscapedString = sEscapedString & Char_Renamed
            End If
        Next ii

        FN_EscapeQuotesInString = sEscapedString

    End Function

    Function FN_EscapeSlashesInString(ByRef sAssignedString As Object) As String
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
        Dim sItem As String

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
                sString = Left(sString, iPos - 1)
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

    Function FN_Open(ByRef sDrafixFile As String, ByRef sName As Object, ByRef sPatientFile As Object, ByRef sLeftorRight As Object, ByRef sType As String) As Short
        'Open the DRAFIX macro file
        'Initialise Global variables
        Dim sID, sAxillaType As String
        Dim iPos As Double
        Dim sProximalStyle, sStyle, sDistalStyle As String

        'Open file
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        fNum = FreeFile()
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FileOpen(fNum, sDrafixFile, Microsoft.VisualBasic.OpenMode.Output)
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        FN_Open = fNum

        'Initialise String globals
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        CC = Chr(44) 'The comma (,)
        'UPGRADE_WARNING: Couldn't resolve default property of object NL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        NL = Chr(10) 'The new line character
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        QQ = Chr(34) 'Double quotes (")
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
        Dim _frmarmdia As New armdia()
        'Initialise patient globals
        'UPGRADE_WARNING: Couldn't resolve default property of object sPatientFile. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sFileNo = sPatientFile
        'UPGRADE_WARNING: Couldn't resolve default property of object sLeftorRight. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sSide = sLeftorRight
        'UPGRADE_WARNING: Couldn't resolve default property of object sName. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_sPatient = sName
        If CType(_frmarmdia.Controls("txtWorkOrder"), Object).Text = "" Then
            g_sWorkOrder = "-"
        Else
            g_sWorkOrder = CType(_frmarmdia.Controls("txtWorkOrder"), Object).Text
        End If


        'Globals to reduced drafix code written to file
        g_sCurrentLayer = ""
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextHt = 0.125
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAspect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextAspect = 0.6
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextHorizJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextHorizJust = 1 'Left
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextVertJust. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextVertJust = 32 'Bottom
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextFont. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextFont = 0 'BLOCK
        'UPGRADE_WARNING: Couldn't resolve default property of object g_nCurrTextAngle. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        g_nCurrTextAngle = 0

        'Create the 4 character string to identify the type
        PR_GetStyle(sStyle)


        'Write header information etc. to the DRAFIX macro file
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//DRAFIX Macro created - " & DateString & "  " & TimeString)
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//Patient - " & g_sPatient & CC & " " & g_sFileNo & CC & " Sleeve-" & g_sSide)
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "//by Visual Basic")

        'Define DRAFIX variables
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "HANDLE hLayer, hSleeve, hTitle, hChan, hEnt, hOrigin, hSym;")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "XY     xyAxilla, xyAxillaLow, xyTitleOrigin, xyStart, xyTitleScale, xyOrigin, xyElbow;")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "ANGLE  aTitleAngle;")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "STRING sTitleName, sFileNo, sSleeve, sID, sWorkOrder, sName, sVestID, sData, sDate, sPathJOBST;")

        'Set path to JOBST installed directory
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sPathJOBST = " & QQ & FN_EscapeSlashesInString(g_sPathJOBST) & QQ & ";")

        'Set up ID  data base field
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "Table(" & QQ & "add" & QCQ & "field" & QCQ & "ID" & QCQ & "string" & QQ & ");")

        'Text data
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextHorzJust" & QC & g_nCurrTextHorizJust & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextVertJust" & QC & g_nCurrTextVertJust & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextHeight" & QC & g_nCurrTextHt & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextAspect" & QC & g_nCurrTextAspect & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetData(" & QQ & "TextFont" & QC & g_nCurrTextFont & ");")

        'Clear user selections etc
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "UserSelection (" & QQ & "clear" & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "Execute (" & QQ & "menu" & QCQ & "SetStyle" & QC & "Table(" & QQ & "find" & QCQ & "style" & QCQ & "bylayer" & QQ & "));")

        g_sID = sStyle & g_sFileNo & g_sSide

        'Find axilla type
        If CType(_frmarmdia.Controls("txtType"), Object).Text <> "ARM" Then
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            iPos = InStr(CType(_frmarmdia.Controls("txtVestRaglan"), Object).Text, QQ) 'Regular axilla are awkward as they
            'contain a space, look for quote (")
            'QQ is a constant set to quote (")
            If iPos <> 0 Then
                'Escape quote for use in drafix
                'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                sAxillaType = Left(CType(_frmarmdia.Controls("txtVestRaglan"), Object).Text, iPos - 1) & "\" & QQ
            Else
                iPos = InStr(CType(_frmarmdia.Controls("txtVestRaglan"), Object).Text, " ")
                If iPos <> 0 Then
                    sAxillaType = Left(CType(_frmarmdia.Controls("txtVestRaglan"), Object).Text, iPos - 1)
                End If
            End If

            End If

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sSleeve = " & QQ & g_sSide & QQ & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sFileNo = " & QQ & g_sFileNo & QQ & ";")

        'Get Start point
        If sType = "Draw" Then
            'Get Start point
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "SetSymbolLibrary( sPathJOBST + " & QQ & "\\JOBST.SLB" & QQ & ");")

            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "GetUser (" & QQ & "xy" & QCQ & "Indicate Start Point" & QC & "&xyStart);")

            'Place a marker at the start point for later use.
            'Get a UID and create the unique 4 character start to the ID code
            'Note this is a bit dogey if the drawing contains more than 9999 entities
            PR_SetLayer("Construct")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "hOrigin = AddEntity(" & QQ & "marker" & QCQ & "xmarker" & QC & "xyStart" & CC & "0.125);")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "if (hOrigin) {")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  sData=StringMiddle(MakeString(" & QQ & "long" & QQ & ",UID(" & QQ & "get" & QQ & ",hOrigin)), 1, 4) ; ")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  while (StringLength(sData) < 4) sData = sData + " & QQ & " " & QQ & ";")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  sData = sData + sFileNo + sSleeve ;")
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  SetDBData( hOrigin," & QQ & "ID" & QQ & CC & QQ & g_sID & "originmark" & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  SetDBData( hOrigin," & QQ & "Data" & QQ & CC & "sData" & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  SetDBData( hOrigin," & QQ & "curvetype" & QQ & CC & QQ & "sleeveoriginmark" & QQ & ");")
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "  }")
        End If

        'Display Hour Glass symbol
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "Display (" & QQ & "cursor" & QCQ & "wait" & QCQ & "Drawing" & QQ & ");")

        'Set values for use futher on by other macros
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "xyOrigin = xyStart" & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sID = " & QQ & g_sID & QQ & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sName = " & QQ & g_sPatient & QQ & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sWorkOrder = " & QQ & _frmarmdia.txtWorkOrder.Text & QQ & ";")

    End Function

    Function fnGetString(ByVal sString As String, ByRef iIndex As Short, ByRef sDelimiter As String) As String
        'Function to return as a string the iIndexth item in a string
        'that using the given string sDelimiter as the delimiter.
        'EG
        '    sString = "Sam Spade Hello"
        '    sDelimiter = " " {SPACE}
        '    fnGetNumber( sString, 2) = "Spade"
        '
        'If the iIndexth item is not found then return "" to indicate an error.
        'Indexing starts from 1

        Dim ii, iPos As Short
        Dim sItem As String

        'Initial error checking
        sString = Trim(sString) 'Remove leading and trailing blanks

        If Len(sString) = 0 Then
            fnGetString = ""
            Exit Function
        End If

        'Prepare string
        sString = sString & sDelimiter 'Trailing sDelimiter as stopper for last item

        'Get iIndexth item
        For ii = 1 To iIndex
            iPos = InStr(sString, sDelimiter)
            If ii = iIndex Then
                sString = Left(sString, iPos - 1)
                fnGetString = sString
                Exit Function
            Else
                sString = LTrim(Mid(sString, iPos + 1))
                If Len(sString) = 0 Then
                    fnGetString = ""
                    Exit Function
                End If
            End If
        Next ii

        'The function should have exited befor this, however just in case
        '(iIndex = 0) we indicate an error,
        fnGetString = ""

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

    Sub PR_AddEntityID(ByRef sFileNo As String, ByRef sSide As String, ByRef sType As Object)
        'To the DRAFIX macro file (given by the global fNum)
        'write the syntax to add to an ENTITY the database information
        'in the DB variable "ID" that will allow the identity of an entity
        'to be retrieved, by other parts of the system.
        '
        'For this to work it assumes that the following DRAFIX variables
        'are defined.
        '    HANDLE  hEnt
        '
        'Note:-
        '    fNum, CC, QQ, NL are globals initialised by FN_Open
        '
        Return '11-June-2018
        Dim sID As String

        'UPGRADE_WARNING: Couldn't resolve default property of object sType. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sID = sFileNo & sSide & sType

        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "if (hEnt) SetDBData( hEnt," & QQ & "ID" & QQ & CC & QQ & sID & QQ & ");")

        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "if (hEnt) SetDBData( hEnt," & QQ & "Data" & QQ & CC & "sData" & ");")

    End Sub

    Sub PR_CalcPolar(ByRef xyStart As XY, ByVal nAngle As Double, ByRef nLength As Double, ByRef xyReturn As XY)
        'Procedure to return a point at a distance and an angle from a given point
        '
        Dim A, B As Double

        'Convert from degees to radians
        nAngle = nAngle * PI / 180

        B = System.Math.Sin(nAngle) * nLength
        A = System.Math.Cos(nAngle) * nLength

        xyReturn.X = xyStart.X + A
        xyReturn.y = xyStart.y + B

    End Sub

    Sub PR_CreateTapeLayer(ByRef sFileNo As String, ByRef sSide As String, ByRef nTape As Object)
        'To the DRAFIX macro file (given by the global fNum).
        'Write the syntax to create a layer for a sleeve tape label
        'This will be named by the following convention
        '    <"FileNo"> + <"L"|"R"> + <nTapeNo>
        'E.g. A1234567L10
        '
        'For this to work it assumes that the following DRAFIX variables
        'are defined.
        '    HANDLE  hLayer
        '
        'Note:-
        '    fNum, CC, QQ, NL, QCQ are globals initialised by FN_Open
        '
        Dim slayer As String

        'UPGRADE_WARNING: Couldn't resolve default property of object nTape. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        slayer = sFileNo & Mid(sSide, 1, 1) & nTape



        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Layer table for read
            Dim acLyrTbl As LayerTable
            acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                     OpenMode.ForRead)

            'Dim sLayerName As String = "Center"

            If acLyrTbl.Has(slayer) = False Then
                Using acLyrTblRec As LayerTableRecord = New LayerTableRecord()

                    '' Assign the layer the ACI color 3 and a name
                    acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, 3)
                    acLyrTblRec.Name = slayer

                    '' Upgrade the Layer table for write
                    acLyrTbl.UpgradeOpen()

                    '' Append the new layer to the Layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec)
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec, True)
                End Using
            End If

            '' Open the Block table for read
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                     OpenMode.ForRead)

            '' Open the Block table record Model space for write
            Dim acBlkTblRec As BlockTableRecord
            acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout),
                                        OpenMode.ForWrite)

            '' Create a circle object
            Using acCirc As Circle = New Circle()
                acCirc.Center = New Point3d(2, 2, 0)
                acCirc.Radius = 1
                acCirc.Layer = slayer

                'acBlkTblRec.AppendEntity(acCirc)
                'acTrans.AddNewlyCreatedDBObject(acCirc, True)
            End Using

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using

        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "hLayer = Table(" & QQ & "find" & QCQ & "layer" & QCQ & slayer & QQ & ");")
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "if ( hLayer != %badtable)" & "Execute (" & QQ & "menu" & QCQ & "SetLayer" & QC & "hLayer);")
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "else")
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "Table (" & QQ & "add" & QCQ & "layer" & QCQ & slayer & QCQ & "Tape Layer Data" & QCQ & "current" & QC & "Table(" & QQ & "find" & QCQ & "color" & QCQ & "DarkCyan" & QQ & "));")

    End Sub

    Sub PR_DeleteText(ByRef sFileNo As Object, ByRef sSleeve As Object, ByRef nTape As Object)
        'For this to work it assumes that the following DRAFIX variables
        'are defined
        '    XY      xyStart
        '
        'Note:-
        '    fNum, CC, QQ, NL, g_nCurrTextAspect are globals initialised by FN_Open
        '

        Return '11-June -2018
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "nTape = " & nTape & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sFileNo = " & QQ & sFileNo & QQ & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sSleeve = " & QQ & sSleeve & QQ & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "sLayer=" & QQ & "layer = '" & QQ & "+ sFileNo + StringMiddle(sSleeve, 1, 1) + MakeString(" & QQ & "long" & QQ & ", nTape) + " & QQ & "'" & QQ & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "hChan = Open(" & QQ & "selection" & QC & "sLayer);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "UserSelection(" & QQ & "clear" & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "if (hChan){")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "ResetSelection(hChan);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "while(hEnt = GetNextSelection (hChan)) DeleteEntity(hEnt);")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "}")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "Close(" & QQ & "selection" & QC & "hChan);")
    End Sub

    Sub PR_DrawArc(ByRef xyCen As XY, ByRef xyArcStart As XY, ByRef xyArcEnd As XY)

        Dim nEndAng, nRad, nStartAng, nDeltaAng As Object

        'UPGRADE_WARNING: Couldn't resolve default property of object nRad. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nRad = FN_CalcLength(xyCen, xyArcStart)

        'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nStartAng = FN_CalcAngle(xyCen, xyArcStart) * (PI / 180)

        'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nEndAng = FN_CalcAngle(xyCen, xyArcEnd) * (PI / 180)

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
            Using acArc As Arc = New Arc(New Point3d(xyCen.X + xyInsertion.X, xyCen.y + xyInsertion.y, 0),
                                     nRad, nStartAng, nEndAng)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acArc.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
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
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '' PrintLine(fNum, "hEnt = AddEntity(" & QQ & "arc" & QC & "xyStart.x +" & Str(xyCen.X) & CC & "xyStart.y +" & Str(xyCen.y) & CC & Str(nRad) & CC & Str(nStartAng) & CC & Str(nDeltaAng) & ");")

    End Sub

    Sub PR_DrawAssignDrafixVariable(ByRef sName As String, ByRef nValue As Double)
        'To the DRAFIX macro file (given by the global fNum).
        'Write the syntax to do a variable assignment
        '
        'For this to work it assumes that the following DRAFIX variables
        'are defined
        '    XY      xyStart
        '
        'Note:-
        '    fNum, CC, QQ, NL, g_nCurrTextAspect are globals initialised by FN_Open
        '
        '
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Return '11-06-2018    PrintLine(fNum, sName & "=" & nValue & ";")


    End Sub

    Sub PR_DrawCircle(ByRef xyCen As XY, ByRef nRadius As Object)
        'To the DRAFIX macro file (given by the global fNum).
        'Write the syntax to draw a CIRCLE at the point given.
        'For this to work it assumes that the following DRAFIX variables
        'are defined
        '    XY      xyStart
        '    HANDLE  hEnt
        '
        'Note:-
        '    fNum, CC, QQ, NL are globals initialised by FN_Open
        '
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
                acCirc.Center = New Point3d(xyCen.X + xyInsertion.X, xyCen.y + xyInsertion.y, 0)
                acCirc.Radius = nRadius
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acCirc.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If

                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acCirc)
                acTrans.AddNewlyCreatedDBObject(acCirc, True)
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "hEnt = AddEntity(" & QQ & "circle" & QC & "xyStart.x+" & Str(xyCen.X) & CC & "xyStart.y+" & Str(xyCen.y) & CC & nRadius & ");")
    End Sub

    Sub PR_DrawCircularStump(ByRef xyStart As XY, ByRef nFiguredLength As Double, ByRef sAge As String)
        'To the DRAFIX macro file (given by the global fNum)
        'Write the syntax to draw a CIRCULAR STUMP
        '
        'For this to work it assumes that the following DRAFIX variables
        'are defined
        '    XY      xyStart
        '    HANDLE  hEnt
        '
        'Note:-
        '    g_sFileno, g_sPatient, g_sSide are globals initialised by FN_Open
        '

        Dim xyPt As XY
        Dim nTextHt, nSeamAllowance As Single
        Dim nRadius As Single

        nTextHt = 0.125
        nSeamAllowance = 0.125 ' 1/8th Inch
        nRadius = (nFiguredLength / 3.14)

        PR_SetLayer("Template" & g_sSide)

        PR_DrawCircle(xyStart, nRadius + nSeamAllowance) 'Add seam allowance
        PR_AddEntityID(g_sFileNo, g_sSide, "CirStump") 'sFileNo, g_sSide from FN_Open

        PR_SetLayer("Notes")
        PR_DrawCircle(xyStart, nRadius)
        PR_AddEntityID(g_sFileNo, g_sSide, "CirStumpSeam") 'sFileNo, g_sSide from FN_Open

        PR_SetTextData(2, 16, -1, -1, -1) 'Horiz center, Vertical center
        ''-----PR_DrawText(g_sPatient & "\n" & g_sWorkOrder & "\n" & g_sSide, xyStart, nTextHt, 0)
        PR_DrawMText(g_sPatient & Chr(10) & sAge & Chr(10) & g_sSide, xyStart, True)

    End Sub

    Sub PR_DrawFitted(ByRef Profile As curve, Optional ByVal bIsSetXDATA As Boolean = False)
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
        'Return '11-June-2018
        'Draw the profile
        '    If there is no vertex or only one vertex then exit.
        '    For two vertex draw as a polyline (this degenerates to a single line).
        '    For three vertex draw as a polyline (as no fitted curve can be drawn
        '    by a macro).
        '
        Select Case Profile.n
            Case 0 To 1
                Exit Sub
            Case 3
                PR_DrawPoly(Profile)
                'Warn the user to smooth the curve
                'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'PrintLine(fNum, "Display (" & QQ & "message" & QCQ & "OKquestion" & QCQ & "The Profile has been drawn as a POLYLINE\nEdit this line and make it OPEN FITTED,\n this will then be a smooth line" & QQ & ");")
            Case Else
                ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'PrintLine(fNum, "hEnt = AddEntity(" & QQ & "poly" & QCQ & "fitted" & QQ)
                'For ii = 1 To Profile.n
                '    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                '    PrintLine(fNum, CC & "xyStart.x+" & Str(Profile.X(ii)) & CC & "xyStart.y+" & Str(Profile.y(ii)))
                'Next ii
                ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'PrintLine(fNum, ");")



                'Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
                'Dim acCurDb As Database = acDoc.Database

                '' Start a transaction
                'Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

                '    '' Open the Block table for read
                '    Dim acBlkTbl As BlockTable
                '    acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

                '    '' Open the Block table record Model space for write
                '    Dim acBlkTblRec As BlockTableRecord
                '    acBlkTblRec = acTrans.GetObject(acBlkTbl(BlockTableRecord.ModelSpace),
                '                                    OpenMode.ForWrite)

                '    '' Create a polyline with two segments (3 points)
                '    Using acPoly As Polyline = New Polyline()
                '        For ii = 1 To Profile.n
                '            acPoly.AddVertexAt(ii - 1, New Point2d(Profile.X(ii) + xyInsertion.X, Profile.y(ii) + xyInsertion.y), 0, 0, 0)
                '        Next ii
                '        '' Add the new object to the block table record and the transaction
                '        acBlkTblRec.AppendEntity(acPoly)
                '        acTrans.AddNewlyCreatedDBObject(acPoly, True)
                '    End Using

                '    '' Save the new object to the database
                '    acTrans.Commit()
                'End Using
                Dim ptColl As Point3dCollection = New Point3dCollection()
                For ii = 1 To Profile.n
                    ptColl.Add(New Point3d(Profile.X(ii) + xyInsertion.X, Profile.y(ii) + xyInsertion.y, 0))
                Next
                PR_DrawSpline(ptColl, bIsSetXDATA)

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

            '' Create a line that starts at 5,5 and ends at 12,3
            Dim acLine As Line = New Line(New Point3d(xyStart.X + xyInsertion.X, xyStart.y + xyInsertion.y, 0),
                                    New Point3d(xyFinish.X + xyInsertion.X, xyFinish.y + xyInsertion.y, 0))

            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                acLine.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
            End If
            '' Add the new object to the block table record and the transaction
            acBlkTblRec.AppendEntity(acLine)
            acTrans.AddNewlyCreatedDBObject(acLine, True)

            '' Save the new object to the database
            acTrans.Commit()
        End Using
        '
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "hEnt = AddEntity(")
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, QQ & "line" & QC)
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "xyStart.x+" & Str(xyStart.X) & CC & "xyStart.y+" & Str(xyStart.y) & CC)
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, "xyStart.x+" & Str(xyFinish.X) & CC & "xyStart.y+" & Str(xyFinish.y))
        ''UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'PrintLine(fNum, ");")


    End Sub

    Sub PR_DrawLineOffset(ByRef xyStart As XY, ByRef xyFinish As XY, ByRef nOffset As Double)
        'To the DRAFIX macro file (given by the global fNum)
        'write the syntax to draw a line between two points offset a given distance.
        'For this to work it assumes that xyStart is defined in DRAFIX
        'as an XY
        '
        'Note:-
        '    fNum, CC, QQ, NL are globals initialised by FN_Open.
        '    The line is always offset to the LEFT of the direction from start
        '    to finish.
        '    If nOffset is -ve then the line is offset to the RIGHT

        Dim nAngle As Double
        Dim xyPt1, xyPt2 As XY

        nAngle = FN_CalcAngle(xyStart, xyFinish)
        If nOffset < 0 Then
            nAngle = nAngle - 90
        Else
            nAngle = nAngle + 90
        End If

        PR_CalcPolar(xyStart, nAngle, System.Math.Abs(nOffset), xyPt1)
        PR_CalcPolar(xyFinish, nAngle, System.Math.Abs(nOffset), xyPt2)

        PR_DrawLine(xyPt1, xyPt2)

    End Sub

    Sub PR_DrawMesh()
        'Invokes external Mesh drawing routine
        'As such it must be the last line called by the DRAW Macro
        If g_bDrawBodyMesh Then
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "Execute (" & QQ & "application" & QC & "sPathJOBST + " & QQ & "\\raglan\\meshdraw" & QCQ & "normal" & QQ & " );")
        End If
        If g_bDrawVestMesh Then
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "Execute (" & QQ & "application" & QC & "sPathJOBST + " & QQ & "\\raglan\\meshvest" & QCQ & "normal" & QQ & " );")
        End If
    End Sub

    Sub PR_DrawPoly(ByRef Profile As curve)
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
                For ii = 1 To Profile.n
                    acPoly.AddVertexAt(ii - 1, New Point2d(Profile.X(ii) + xyInsertion.X, Profile.y(ii) + xyInsertion.y), 0, 0, 0)
                Next ii

                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acPoly.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acPoly)
                acTrans.AddNewlyCreatedDBObject(acPoly, True)
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using

    End Sub

    Sub PR_DrawRaglan(ByRef sType As String, ByRef sVestRaglan As Object, ByRef sAge As Object, ByRef sVestID As Object)
        'To the DRAFIX macro file (given by the global fNum).
        'Write the syntax to add a Raglan to the end of the sleeve
        'profile based on the data given in the variable sVestRaglan
        '
        'Note:-
        ' This routine is used to draw the raglan part of the
        ' sleeve based on the data from the vest.
        ' It is called VestRaglan to distinguish it from the
        ' raglan flap.
        '
        'Input:-
        ' sVestRaglan  A composite string consisting of the following:
        '
        '              sAxillaType, either Regular 2"
        '                                  Regular 1�"
        '                                  Regular 2�"
        '                                  Open
        '                                  Mesh
        '                                  Lining
        '                                  Sleeveless
        '              nAxillaFrontNeckRad
        '              nAxillaBackNeckRad
        '              nShoulderToBackRaglan
        '
        '              E.G. "Regular 1�" 5.565656 6.123344 0.3456
        '
        'Output:-
        ' Calls to 3 different macro files written to fNum
        '    Open macro file.
        '
        '        nAxillaFrontNeckRad   }
        '        nAxillaBackNeckRad    } Data from sVestRaglan
        '        nShoulderToBackRaglan } (For vest only)
        '        sAxillaType           }
        '
        '    Axilla macro file (depending on axilla given).
        '    Closing macro file.
        '
        '
        'Note: This routine has been amended to support Bodysuits
        '      For vest read Vest & BodySuit
        Dim sAxillaType, sString As String
        Dim iPos As Short

        'Flag to draw the mesh used by PR_DrawMesh
        g_bDrawBodyMesh = False
        g_bDrawVestMesh = False


        'Find axilla type
        'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object sVestRaglan. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        iPos = InStr(sVestRaglan, QQ) 'Regular axilla are awkward as they
        'contain a space, look for quote (")
        'QQ is a constant set to quote (")
        If iPos <> 0 Then
            'Escape quote for use in drafix
            'UPGRADE_WARNING: Couldn't resolve default property of object QQ. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object sVestRaglan. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sAxillaType = Left(sVestRaglan, iPos - 1) & "\" & QQ
            'UPGRADE_WARNING: Couldn't resolve default property of object sVestRaglan. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sString = Mid(sVestRaglan, iPos + 1)
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object sVestRaglan. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            iPos = InStr(sVestRaglan, " ")
            'UPGRADE_WARNING: Couldn't resolve default property of object sVestRaglan. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sAxillaType = Left(sVestRaglan, iPos - 1)
            'UPGRADE_WARNING: Couldn't resolve default property of object sVestRaglan. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sString = Mid(sVestRaglan, iPos)
        End If

        'Check that an axilla has been given
        If sAxillaType = "None" Or sAxillaType = "Sleeveless" Then Exit Sub

        'NB the order of the following is very important
        '
        'Data from vest used in drawing the sleeve
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "nAxillaFrontNeckRad= " & FN_GetNumber(sString, 1) & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "nAxillaBackNeckRad= " & FN_GetNumber(sString, 2) & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "nShoulderToBackRaglan = " & FN_GetNumber(sString, 3) & ";")

        If sType = "Vest" Then
            'Initialise raglan drawing
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\VR_INIT.D;")

            'Draw for the particular axilla type
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "sAxillaType = " & QQ & sAxillaType & QQ & ";")
            Dim _frmarmdia As New armdia()
            Select Case sAxillaType
                Case "Open", "Lining"
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\VR_OPEN.D;")
                Case "Mesh"
                    g_bDrawVestMesh = True
                    'UPGRADE_WARNING: Couldn't resolve default property of object sAge. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "nAge = " & Val(sAge) & ";")
                    If g_sSide = "Left" Then
                        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        PrintLine(fNum, "nMeshLength = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 2, ",")) & ";")
                        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        PrintLine(fNum, "nDistanceAlongRaglan = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 1, ",")) & ";")
                    Else
                        If Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 4, ",")) > 0 And Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 3, ",")) > 0 Then
                            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            PrintLine(fNum, "nMeshLength = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 4, ",")) & ";")
                            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            PrintLine(fNum, "nDistanceAlongRaglan = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 3, ",")) & ";")
                        Else
                            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            PrintLine(fNum, "nMeshLength = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 2, ",")) & ";")
                            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            PrintLine(fNum, "nDistanceAlongRaglan = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 1, ",")) & ";")
                        End If
                    End If
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\VR_MESH.D;")
                Case Else 'Regular
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\VR_REGLR.D;")
            End Select

            'Close raglan drawing
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\VR_CLOSE.D;")
        Else
            'Initialise raglan drawing
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\BR_INIT.D;")

            'Draw for the particular axilla type
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "sAxillaType = " & QQ & sAxillaType & QQ & ";")
            Dim _frmarmdia As New armdia
            Select Case sAxillaType
                Case "Open", "Lining"
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\BR_OPEN.D;")
                Case "Mesh"
                    g_bDrawBodyMesh = True
                    'UPGRADE_WARNING: Couldn't resolve default property of object sAge. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "nAge = " & Val(sAge) & ";")
                    If g_sSide = "Left" Then
                        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        PrintLine(fNum, "nMeshLength = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 2, ",")) & ";")
                        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        PrintLine(fNum, "nDistanceAlongRaglan = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 1, ",")) & ";")
                    Else
                        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        PrintLine(fNum, "nMeshLength = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 4, ",")) & ";")
                        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        PrintLine(fNum, "nDistanceAlongRaglan = " & Val(fnGetString(CType(_frmarmdia.Controls("txtMeshData"), Object).Text, 3, ",")) & ";")
                    End If
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\BR_MESH.D;")
                Case Else 'Regular
                    'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\BR_REGLR.D;")
            End Select

            'Close raglan drawing
            'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            PrintLine(fNum, "@" & g_sPathJOBST & "\RAGLAN\BR_CLOSE.D;")

        End If
    End Sub

    Sub PR_DrawText(ByRef sText As Object, ByRef xyInsert As XY, ByRef nHeight As Object, ByRef nAngle As Double)
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

            '' Create a single-line text object
            Using acText As DBText = New DBText()
                acText.Position = New Point3d(xyInsert.X + xyInsertion.X, xyInsert.y + xyInsertion.y, 0)
                acText.Height = nHeight
                acText.TextString = sText
                acText.Rotation = nAngle
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acText.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If
                acBlkTblRec.AppendEntity(acText)
                acTrans.AddNewlyCreatedDBObject(acText, True)
            End Using

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ' PrintLine(fNum, "AddEntity(" & QQ & "text" & QCQ & sText & QC & "xyStart.x+" & Str(xyInsert.X) & CC & "xyStart.y+" & Str(xyInsert.y) & CC & nWidth & CC & nHeight & CC & g_nCurrTextAngle & ");")

    End Sub

    Sub PR_GetStyle(ByRef sStyle As String)
        'This function returns a value of the following
        'in sWO
        'WorkOrder, ItemNo
        'eg
        '    12345,10
        '
        Dim nOptions As Short
        Dim sDistalStyle, sProximalStyle As String
        Dim sBestCatNo, sStyleWO As String
        Dim nResult As Short

        'Distal
        sDistalStyle = "XX"
        If g_iStyleFirstTape = g_iFirstTape Then
            'Plain
            sDistalStyle = "PL"
        Else
            'Start at tape
            If g_iStyleFirstTape < 10 Then
                sDistalStyle = "0" & LTrim(Str(g_iStyleFirstTape))
            Else
                sDistalStyle = LTrim(Str(g_iStyleFirstTape))
            End If
        End If
        If g_sGaunt = "True" Then
            'Gauntlet
            sDistalStyle = "GT"
        End If
        If g_sStump = "True" Then
            'Stump
            sDistalStyle = "ST"
        End If

        'Proximal
        sProximalStyle = "XX"
        If g_iStyleLastTape = g_iLastTape Then
            'Plain
            sProximalStyle = "PL"
        Else
            'Start at tape
            If g_iStyleLastTape < 10 Then
                sProximalStyle = "0" & LTrim(Str(g_iStyleLastTape))
            Else
                sProximalStyle = LTrim(Str(g_iStyleLastTape))
            End If
        End If
        If g_sDetGaunt = "True" And g_iStyleLastTape <= 6 Then
            'Detachable Gauntlet only, if drawn from or less than tape +1-1/2
            sProximalStyle = "GT"
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object g_sFlapChk. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If g_sFlapChk = "True" Then
            'Flap
            sProximalStyle = "FP"
        End If
        Dim _frmarmdia As New armdia
        If Left(_frmarmdia.cboFlaps.Text, 4) = "Vest" Then
            'Vest raglan
            sProximalStyle = "VR"
        End If
        If Left(_frmarmdia.cboFlaps.Text, 4) = "Body" Then
            'Bodysuit raglan
            sProximalStyle = "BR"
        End If

        'Join distal and proximal styles
        sStyle = sDistalStyle & sProximalStyle


    End Sub

    Sub PR_InsertSymbol(ByRef sSymbol As String, ByRef xyInsert As XY, ByRef nScale As Single, ByRef nRotation As Single, ByRef slayer As String)
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
        '    The DRAFIX symbol library "C:\JOBST\JOBST.SLB" exists
        '
        'Note:-
        '    fNum, CC, QQ, NL, QCQ are globals initialised by FN_Open
        '
        PR_SetLayer("Notes")
        Dim blkIdCollection As ObjectIdCollection = New ObjectIdCollection()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim OpenDb As Database = New Database()
        Try
            OpenDb.ReadDwgFile(g_sPathJOBST & QQ & "\\JOBST.SLB", System.IO.FileShare.Read, True, "")
        Catch ex As Autodesk.AutoCAD.Runtime.Exception
            ''MsgBox("Bad Tape File")
            Exit Sub
        End Try
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        Using acTrans As Transaction = OpenDb.TransactionManager.StartTransaction()
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(OpenDb.BlockTableId, OpenMode.ForRead)
            If acBlkTbl.Has(sSymbol) Then
                blkIdCollection.Add(acBlkTbl(sSymbol))
            End If
            acTrans.Commit()
        End Using
        If blkIdCollection.Count > 0 Then
            Dim iMap As IdMapping = New IdMapping()
            acCurDb.WblockCloneObjects(blkIdCollection, acCurDb.BlockTableId, iMap, DuplicateRecordCloning.Ignore, False)
        End If

        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()
            Dim blkRecId As ObjectId = ObjectId.Null
            Dim acBlkTbl As BlockTable
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)
            If acBlkTbl.Has(sSymbol) Then
                Dim blkRef As BlockReference = New BlockReference(New Point3d(xyInsert.X + xyInsertion.X, xyInsert.y + xyInsertion.y, 0), acBlkTbl(sSymbol))
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)
                acBlkTblRec.AppendEntity(blkRef)
                acTrans.AddNewlyCreatedDBObject(blkRef, True)
            End If
            acTrans.Commit()
        End Using






        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "SetSymbolLibrary(sPathJOBST +" & QQ & "\\JOBST.SLB" & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "Symbol(" & QQ & "find" & QCQ & sSymbol & QQ & ");")

        'PR_SetLayer slayer

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "hEnt = AddEntity(" & QQ & "symbol" & QCQ & sSymbol & QC)
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "xyStart.x+" & Str(xyInsert.X) & CC & "xyStart.y+" & Str(xyInsert.y) & CC)
        'UPGRADE_WARNING: Couldn't resolve default property of object CC. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, Str(nScale) & CC & Str(nScale) & CC & Str(nRotation) & ");")

    End Sub

    Sub PR_MakeXY(ByRef xyReturn As XY, ByRef X As Double, ByRef y As Double)
        'Utility to return a point based on the X and Y values
        'given
        xyReturn.X = X
        xyReturn.y = y
    End Sub

    Sub PR_NamedHandle(ByRef sHandleName As String)
        'To the DRAFIX macro file (given by the global fNum).
        'Write the syntax to retain the entity handle of a previously
        'added entity.
        '
        'Assumes that hEnt is the entity handle to the just inserted entity.
        Return '11-June-2018
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, "HANDLE " & sHandleName & ";")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PrintLine(fNum, sHandleName & " = hEnt;")

    End Sub

    Sub PR_PutTapeLabel(ByRef nTape As Object, ByRef xyStart As XY, ByRef nLength As Object, ByRef nMM As Object, ByRef nGrm As Object, ByRef nRed As Object)
        Dim nDec As Object
        Dim nInt As Object
        'To the DRAFIX macro file (given by the global fNum).
        'Write the syntax to add Sleeve Tape details,
        'these details are given explicitly as arguments.
        'Where:-
        '    nTape       Index into sTextList below
        '    xyStart     Position of tape label on fold
        '    nLength     Tape length to be displayed, decimal inches
        '    nMM         MMs to be displayed
        '    nGrm        Grams to be displayed
        '    nRed        Reduction to be displayed
        '
        '
        'For this to work it assumes that the following DRAFIX variables
        'are defined
        '    XY      xyStart
        '    HANDLE  hEnt
        '
        'Note:-
        '    fNum, g_sFileNo, g_sSide are globals initialised by FN_Open
        '
        '
        Dim sTextList, sSymbol As String
        Dim xyPt As XY
        Dim nSymbolOffSet, nTextHt As Single

        'sTextList = "  6 4�  3 1�  0 1�  3 4�  6 7�  910� 1213� 1516� 1819� 2122� 2425� 2728� 3031� 3334� 36"

        'UPGRADE_WARNING: Couldn't resolve default property of object nTape. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sSymbol = nTape & "tape"
        nSymbolOffSet = 0.6877
        nTextHt = 0.125

        PR_MakeXY(xyPt, xyStart.X, xyStart.y + nSymbolOffSet) 'Offset because symbol point is at top
        ''PR_InsertSymbol(sSymbol, xyPt, 1, 0, "Notes") '' Commented on 23-06-2018

        PR_CreateTapeLayer(g_sFileNo, g_sSide, nTape)

        PR_SetTextData(1, 32, -1, -1, 0)

        'Length text
        'N.B. format as Inches and eighths. With eighths offset up and left
        'UPGRADE_WARNING: Couldn't resolve default property of object nInt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nInt = Int(CDbl(nLength)) 'Integer part of the length (before decimal point)

        'Decimal part of the length (after decimal point)
        'convert to 1/8ths and get nearest by rounding
        'UPGRADE_WARNING: Couldn't resolve default property of object nInt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nLength. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nDec. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nDec = round((nLength - nInt) / 0.125)
        'UPGRADE_WARNING: Couldn't resolve default property of object nDec. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If nDec = 8 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object nDec. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            nDec = 0
            'UPGRADE_WARNING: Couldn't resolve default property of object nInt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            nInt = nInt + 1
        End If

        'Draw Integer part
        PR_MakeXY(xyPt, xyStart.X + 0.0625, xyStart.y + 0.75)
        'UPGRADE_WARNING: Couldn't resolve default property of object nInt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_DrawText(Trim(nInt), xyPt, nTextHt, 0)

        'Draw eights part
        'UPGRADE_WARNING: Couldn't resolve default property of object nInt. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_MakeXY(xyPt, xyStart.X + 0.0625 + (Len(Trim(nInt)) * nTextHt * 0.8), xyStart.y + 0.75 + nTextHt / 1.5)
        'UPGRADE_WARNING: Couldn't resolve default property of object nDec. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If nDec <> 0 Then PR_DrawText(Trim(nDec), xyPt, nTextHt / 1.5, 0)

        'MMs text
        PR_MakeXY(xyPt, xyStart.X + 0.0625, xyStart.y + 1)
        'UPGRADE_WARNING: Couldn't resolve default property of object nMM. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_DrawText(DirectCast(nMM, TextBox).Text & "mm", xyPt, nTextHt, 0)

        'Grams text
        PR_MakeXY(xyPt, xyStart.X + 0.0625, xyStart.y + 1.25)
        'UPGRADE_WARNING: Couldn't resolve default property of object nGrm. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_DrawText(DirectCast(nGrm, Label).Text & "gm", xyPt, nTextHt, 0)

        'Reduction text and circle round the text
        PR_SetTextData(2, 16, -1, -1, -1)
        PR_MakeXY(xyPt, xyStart.X + 0.25, xyStart.y + 1.625)
        'UPGRADE_WARNING: Couldn't resolve default property of object nRed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PR_DrawTextCenter(Trim(DirectCast(nRed, Label).Text), xyPt, nTextHt, 0)
        PR_DrawCircle(xyPt, 0.125)



    End Sub

    Sub PR_RightThumbHole(ByRef xyCen As XY, ByRef xyArcStart As XY, ByRef xyArcEnd As XY)
        ' This Calculates and Draws the curve at the
        ' right of the ThumbHole on the template

        Dim nEndAng, nRad, nStartAng, nDeltaAng As Object
        'UPGRADE_WARNING: Couldn't resolve default property of object nRad. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nRad = FN_CalcLength(xyCen, xyArcStart)
        'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nStartAng = FN_CalcAngle(xyCen, xyArcStart) * (PI / 180)
        'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nEndAng = FN_CalcAngle(xyCen, xyArcEnd) * (PI / 180)
        'UPGRADE_WARNING: Couldn't resolve default property of object nStartAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nEndAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nDeltaAng = nEndAng - nStartAng
        'UPGRADE_WARNING: Couldn't resolve default property of object nDeltaAng. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        nDeltaAng = -nDeltaAng
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
            Using acArc As Arc = New Arc(New Point3d(xyCen.X + xyInsertion.X, xyCen.y + xyInsertion.y, 0),
                                         nRad, nStartAng, nEndAng)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acArc.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acArc)
                acTrans.AddNewlyCreatedDBObject(acArc, True)
            End Using

            '' Save the new object to the database
            acTrans.Commit()
        End Using
        'PrintLine(fNum, "hEnt = AddEntity(" & QQ & "arc" & QC & "xyStart.x +" & Str(xyCen.X) & CC & "xyStart.y +" & Str(xyCen.y) & CC & Str(nRad) & CC & Str(nStartAng) & CC & Str(nDeltaAng) & ");")
    End Sub

    Sub PR_SetLayer(ByRef sNewLayer As String)
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

        ' If g_sCurrentLayer = sNewLayer Then Exit Sub
        'g_sCurrentLayer = sNewLayer

        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        '' Start a transaction
        Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

            '' Open the Layer table for read
            Dim acLyrTbl As LayerTable
            acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                     OpenMode.ForRead)

            ' Dim sLayerName As String = "Center"

            If acLyrTbl.Has(sNewLayer) = True Then
                '' Set the layer Center current
                acCurDb.Clayer = acLyrTbl(sNewLayer)

                '' Save the changes
                acTrans.Commit()
            End If

            '' Dispose of the transaction
        End Using

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ' PrintLine(fNum, "hLayer = Table(" & QQ & "find" & QCQ & "layer" & QCQ & sNewLayer & QQ & ");")
        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ' PrintLine(fNum, "if ( hLayer != %badtable)" & "Execute (" & QQ & "menu" & QCQ & "SetLayer" & QC & "hLayer);")

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
        Return '11-June-2018

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

    Function round(ByVal nNumber As Single) As Short
        'Fuction to return the rounded value of a decimal number
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
            round = (nInt + 1) * nSign
        Else
            round = nInt * nSign
        End If

    End Function
    Sub PR_DrawTextCenter(ByRef sText As Object, ByRef xyInsert As XY, ByRef nHeight As Object, ByRef nAngle As Double)
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

            '' Create a single-line text object
            Using acText As DBText = New DBText()
                acText.Position = New Point3d(xyInsert.X + xyInsertion.X, xyInsert.y + xyInsertion.y, 0)
                acText.Height = nHeight
                acText.TextString = sText
                acText.Rotation = nAngle
                acText.HorizontalMode = TextHorizontalMode.TextMid
                acText.AlignmentPoint = New Point3d(xyInsert.X + xyInsertion.X, xyInsert.y + xyInsertion.y, 0)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    acText.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If
                acBlkTblRec.AppendEntity(acText)
                acTrans.AddNewlyCreatedDBObject(acText, True)
            End Using

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using

        'UPGRADE_WARNING: Couldn't resolve default property of object fNum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ' PrintLine(fNum, "AddEntity(" & QQ & "text" & QCQ & sText & QC & "xyStart.x+" & Str(xyInsert.X) & CC & "xyStart.y+" & Str(xyInsert.y) & CC & nWidth & CC & nHeight & CC & g_nCurrTextAngle & ");")

    End Sub
    'To Draw Spline
    Private Sub PR_DrawSpline(ByRef PointCollection As Point3dCollection, Optional ByVal bIsSetXDATA As Boolean = False)
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
                    acSpline.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If
                '' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acSpline)
                acTrans.AddNewlyCreatedDBObject(acSpline, True)
                If bIsSetXDATA = True Then
                    Dim acRegAppTbl As RegAppTable
                    acRegAppTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead)
                    Dim acRegAppTblRec As RegAppTableRecord
                    Dim appName As String = "ProfileID"
                    Dim xdataStr As String = "ARM" & ARMDIA1.g_sID & "Profile"
                    If acRegAppTbl.Has(appName) = False Then
                        acRegAppTblRec = New RegAppTableRecord
                        acRegAppTblRec.Name = appName
                        acRegAppTbl.UpgradeOpen()
                        acRegAppTbl.Add(acRegAppTblRec)
                        acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, True)
                    End If
                    Using rb As New ResultBuffer
                        rb.Add(New TypedValue(DxfCode.ExtendedDataRegAppName, appName))
                        rb.Add(New TypedValue(DxfCode.ExtendedDataAsciiString, xdataStr))
                        acSpline.XData = rb
                    End Using
                End If
            End Using
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_DrawMText(ByRef sText As Object, ByRef xyInsert As ARMDIA1.XY, ByRef bIsCenter As Boolean)
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
            mtx.Location = New Point3d(xyInsert.X + xyInsertion.X, xyInsert.y + xyInsertion.y, 0)
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
                mtx.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
            End If
            acBlkTblRec.AppendEntity(mtx)
            acTrans.AddNewlyCreatedDBObject(mtx, True)

            '' Save the changes and dispose of the transaction
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_DrawXMarker()
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim xyStart, xyEnd, xyBase, xySecSt, xySecEnd As XY
        PR_CalcPolar(xyBase, 135, 0.0625, xyStart)
        PR_CalcPolar(xyBase, -45, 0.0625, xyEnd)
        PR_CalcPolar(xyBase, 45, 0.0625, xySecSt)
        PR_CalcPolar(xyBase, -135, 0.0625, xySecEnd)
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
                Dim blkRef As BlockReference = New BlockReference(New Point3d(xyInsertion.X, xyInsertion.y, 0), blkRecId)
                If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                    blkRef.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
                End If
                '' Open the Block table record Model space for write
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(strLayout), OpenMode.ForWrite)
                acBlkTblRec.AppendEntity(blkRef)
                acTrans.AddNewlyCreatedDBObject(blkRef, True)

                '' Set XData to this block reference for Arm Edit
                Dim acRegAppTbl As RegAppTable
                acRegAppTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead)
                Dim acRegAppTblRec As RegAppTableRecord
                Dim appName As String = "ProfileID"
                Dim xdataStr As String = "ARM" & ARMDIA1.g_sID & "Profile"
                If acRegAppTbl.Has(appName) = False Then
                    acRegAppTblRec = New RegAppTableRecord
                    acRegAppTblRec.Name = appName
                    acRegAppTbl.UpgradeOpen()
                    acRegAppTbl.Add(acRegAppTblRec)
                    acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, True)
                End If
                Using rb As New ResultBuffer
                    rb.Add(New TypedValue(DxfCode.ExtendedDataRegAppName, appName))
                    rb.Add(New TypedValue(DxfCode.ExtendedDataAsciiString, xdataStr))
                    blkRef.XData = rb
                End Using
            End If
            '' Save the new object to the database
            acTrans.Commit()
        End Using
    End Sub
    Sub PR_DrawClosedArrow(ByRef xyPoint As ARMDIA1.XY, ByRef nAngle As Object)
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database
        Dim strLayout As String = BlockTableRecord.ModelSpace
        If fnGetSettingsPath("DRAW_IN_LAYOUT").ToUpper().Equals("YES", StringComparison.InvariantCultureIgnoreCase) Then
            strLayout = BlockTableRecord.PaperSpace
        End If

        Dim xyStart As ARMDIA1.XY
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
            acPoly.AddVertexAt(0, New Point2d(xyStart.X + xyInsertion.X, xyStart.y + xyInsertion.y), 0, 0, 0)
            acPoly.AddVertexAt(0, New Point2d(xyStart.X + xyInsertion.X + 0.125, xyStart.y + xyInsertion.y + 0.0625), 0, 0, 0)
            acPoly.AddVertexAt(0, New Point2d(xyStart.X + xyInsertion.X + 0.125, xyStart.y + xyInsertion.y - 0.0625), 0, 0, 0)
            acPoly.AddVertexAt(0, New Point2d(xyStart.X + xyInsertion.X, xyStart.y + xyInsertion.y), 0, 0, 0)
            acPoly.TransformBy(Matrix3d.Rotation((nAngle * (BODYSUIT1.PI / 180)), Vector3d.ZAxis, New Point3d(xyStart.X + xyInsertion.X, xyStart.y + xyInsertion.y, 0)))
            If fnGetSettingsPath("UNITS").ToUpper().Equals("CM", StringComparison.InvariantCultureIgnoreCase) Then
                acPoly.TransformBy(Matrix3d.Scaling(2.54, New Point3d(xyInsertion.X, xyInsertion.y, 0)))
            End If

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
End Module

