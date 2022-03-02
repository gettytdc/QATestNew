Imports System.Collections.Generic
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Xml
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery
Imports BluePrism.BPCoreLib
Imports BluePrism.TerminalEmulation

''' <summary>
''' Query Command Handlers - Terminal-specific
''' </summary>
Partial Public Class clsLocalTargetApp

    <Category(Category.Terminal)> _
    <Command("Starts the session.")> _
    <Parameters("Options (optional), SessionFile, SessionID, TerminalType and ATMVariant.")>
    <Obsolete>
    Private Function ProcessCommandStartSession(ByVal objQuery As clsQuery) As Reply

        Dim si As SessionStartInfo = GetSessionStartInfo(objQuery)

        SyncLock mApplicationStateLock
            Try
                mTerminalApp = New Terminal
                mTerminalApp.ConnectToHostOrSession(si)
                Return Reply.Ok
            Catch e As Exception
                mTerminalApp = Nothing
                Throw New InvalidOperationException(String.Format(My.Resources.LaunchFailed0, e.Message))
            End Try
        End SyncLock
    End Function

    ''' <summary>
    ''' Gets the session start info from the query parameters
    ''' </summary>
    Private Function GetSessionStartInfo(objQuery As clsQuery) As SessionStartInfo

        Dim si As New SessionStartInfo

        Dim sCodePage = objQuery.GetParameter(ParameterNames.CodePage)
        Dim codePage As Integer
        If Integer.TryParse(sCodePage, codePage) Then
            si.Encoding = Encoding.GetEncoding(codePage)
        Else
            si.Encoding = Encoding.Default
        End If

        si.SessionFile = objQuery.GetParameter(ParameterNames.SessionFile)
        si.SessionID = objQuery.GetParameter(ParameterNames.SessionID)
        si.SessionDLLName = objQuery.GetParameter(ParameterNames.SessionDLLName)
        si.SessionDLLEntryPoint = objQuery.GetParameter(ParameterNames.SessionDLLEntryPoint)

        Dim convention As CallingConvention
        If Not [Enum].TryParse(Of CallingConvention)(objQuery.GetParameter(ParameterNames.SessionConvention), convention) Then
            convention = CallingConvention.Cdecl
        End If
        si.Convention = convention

        Dim waitTimeout As String = objQuery.GetParameter(ParameterNames.WaitTimeout)
        If Integer.TryParse(waitTimeout, si.WaitTimeout) Then
            'supplied value is in seconds - convert to milliseconds here
            si.WaitTimeout *= 1000
        Else
            'don't care if this fails since zero means use default
        End If

        Dim waitSleepTime As String = objQuery.GetParameter(ParameterNames.WaitSleepTime)
        Integer.TryParse(waitSleepTime, si.WaitSleepTime)   'don't care if this fails since zero means use default


        Dim terminalType As String = objQuery.GetParameter(ParameterNames.TerminalType)
        If terminalType IsNot Nothing Then
            si.TerminalType = CType(System.Enum.Parse(GetType(SessionStartInfo.TerminalTypes), terminalType), SessionStartInfo.TerminalTypes)
        End If
        Select Case si.TerminalType
            Case SessionStartInfo.TerminalTypes.ATM
                si.MainframeSpecificInfo = objQuery.GetParameter(ParameterNames.ATMVariant)
            Case Else
                si.MainframeSpecificInfo = ""
        End Select

        Dim sessionType As String = objQuery.GetParameter(ParameterNames.SessionType)
        If sessionType IsNot Nothing Then
            si.SessionType = CType(System.Enum.Parse(GetType(SessionStartInfo.SessionTypes), sessionType), SessionStartInfo.SessionTypes)
        End If

        'Set the application options.
        Dim s As String = objQuery.GetParameter(ParameterNames.Options)
        If s IsNot Nothing Then SetOptions(s)
        Return si
    End Function

    <Category(Category.Terminal)>
    <Command("Ends the session.")>
    <Obsolete>
    Private Function ProcessCommandEndSession(ByVal objQuery As clsQuery) As Reply

        Try
            OnExpectDisconnect()
        Finally
            SyncLock mApplicationStateLock
                Dim changed = False
                If mTerminalApp IsNot Nothing Then
                    If Not OptionSet("nodisconnect") Then
                        mTerminalApp.DisconnectFromHost(Nothing)
                    End If
                    mTerminalApp.Dispose()
                    mTerminalApp = Nothing
                    changed = True
                End If
                Connected = False
                If changed Then OnDisconnected()
            End SyncLock
        End Try
        Return Reply.Ok

    End Function

    <Category(Category.Terminal)>
    <Command("Launch a terminal emulator")>
    Private Function ProcessCommandLaunchMainframe(ByVal objQuery As clsQuery) As Reply

        Dim si = GetSessionStartInfo(objQuery)

        Dim sErr As String = Nothing
        mTerminalApp = New Terminal
        If mTerminalApp.Launch(si, sErr) Then _
            Return Reply.Ok

        Throw New InvalidOperationException(sErr)
    End Function

    <Category(Category.Terminal)>
    <Command("Attach to an existing terminal emulator")>
    Private Function ProcessCommandAttachMainframe(ByVal objQuery As clsQuery) As Reply
        Dim sErr As String = Nothing

        Dim si = GetSessionStartInfo(objQuery)

        If mTerminalApp Is Nothing Then
            mTerminalApp = New Terminal
        End If
        If mTerminalApp.Attach(si, sErr) Then _
            Return Reply.Ok

        Throw New InvalidOperationException(sErr)
    End Function

    <Category(Category.Terminal)>
    <Command("Detach from the terminal emulator")>
    Private Function ProcessCommandDetachMainframe(ByVal objQuery As clsQuery) As Reply
        Dim sErr As String = Nothing
        If mTerminalApp.Detach(sErr) Then
            Disconnect()
            Return Reply.Ok
        End If

        Throw New InvalidOperationException(sErr)
    End Function

    <Category(Category.Terminal)>
    <Command("Terminate the terminal emulator")>
    Private Function ProcessCommandTerminateMainframe(ByVal objQuery As clsQuery) As Reply
        Dim sErr As String = Nothing
        If mTerminalApp.Terminate(sErr) Then
            Disconnect()
            Return Reply.Ok
        End If

        Throw New InvalidOperationException(sErr)
    End Function

    <Category(Category.Terminal)>
    <Command("Check that a field exists, and identifies by comparing the value read to a reference value.")>
    <Parameters("StartX, StartY, EndX, EndY define the bounds of the field. FieldType is the type. FieldText contains the text that should be found in the field.")>
    Private Function ProcessCommandCheckField(ByVal objQuery As clsQuery) As Reply

        If mTerminalApp Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NotConnected_1)
        End If

        Dim matchText As clsIdentifierMatchTarget
        Try
            matchText = objQuery.GetIdentifier(IdentifierTypes.FieldText)
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToParseInputs0, ex.Message))
        End Try

        Dim field As clsTerminalField = GetTerminalField(mTerminalApp, objQuery)
        Dim result As String = Nothing
        Dim sErr As String = Nothing
        If Not mTerminalApp.GetField(field, result, sErr) Then
            Throw New InvalidOperationException(sErr)
        End If
        If matchText.IsMatch(result) Then
            Return Reply.Result(True)
        Else
            Return Reply.Result(False)
        End If

    End Function


    <Category(Category.Terminal)>
    <Command("Search the whole terminal for some text.")>
    <Parameters("* NewText is the text to search for. ")>
    <Response("The XML of an Automate collection, with fields ""Row"" and ""Column"" whose values are indexed from 1. Each row contains information about a found instance of the text.")>
    Private Function ProcessCommandSearchTerminal(ByVal objQuery As clsQuery) As Reply
        Dim searchText As String = objQuery.GetParameter(ParameterNames.NewText)
        If searchText Is Nothing Then Throw New InvalidOperationException(My.Resources.SpecifyTheTextToSearchFor)
        If mTerminalApp Is Nothing Then Throw New InvalidOperationException(My.Resources.NotConnected_1)
        Dim pp As List(Of Point) = mTerminalApp.SearchTerminal(searchText)
        Dim xdoc As New XmlDocument
        Dim root As XmlElement = xdoc.CreateElement("collection")
        For Each p As Point In pp
            Dim row As XmlElement = xdoc.CreateElement("row")
            row.AppendChild(CreateCollectionFieldXML(xdoc, p.Y.ToString, "number", "Row"))
            row.AppendChild(CreateCollectionFieldXML(xdoc, p.X.ToString, "number", "Column"))
            root.AppendChild(row)
        Next
        Return Reply.Result(root.OuterXml)

    End Function


    <Category(Category.Terminal)>
    <Command("Read the contents of a field.")>
    <Parameters("* StartX, StartY, EndX, EndY define the bounds of the field." & vbCrLf &
    "* FieldType is the type, which can be either ""Rectangular"" or ""MultilineWrapped"", with the case being significant." & vbCrLf &
    "* FieldText is accepted as a parameter, for compatibility, but ignored. ")>
    Private Function ProcessCommandGetField(ByVal objQuery As clsQuery) As Reply

        If mTerminalApp Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NotConnected_1)
        End If

        Dim field As clsTerminalField = GetTerminalField(mTerminalApp, objQuery)
        Dim sErr As String = Nothing
        Dim result As String = Nothing
        If Not mTerminalApp.GetField(field, result, sErr) Then
            Throw New InvalidOperationException(sErr)
        End If
        Return Reply.Result(result)

    End Function

    <Category(Category.Terminal)>
    <Command("Sets the contents of a field.")>
    <Parameters("StartX, StartY, EndX, EndY define the bounds of the field. FieldType is the type. FieldText is accepted as a parameter, for compatibility, but ignored. NewText is the text to set.")>
    Private Function ProcessCommandSetField(ByVal objQuery As clsQuery) As Reply

        If mTerminalApp Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NotConnected_1)
        End If

        Dim newText As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim field As clsTerminalField = GetTerminalField(mTerminalApp, objQuery)
        Dim sErr As String = Nothing
        If mTerminalApp.SetField(field, newText, sErr) Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(sErr)
        End If
    End Function

    <Category(Category.Highlighting)>
    <Command("Highlights a Terminal field.")>
    <Parameters("The terminal field bounding rectangle specified by 'StartX' 'StartY' 'EndX' and 'EndY' as well as those required to uniquely identify the HTML element.")>
    Private Function ProcessCommandHighlightTerminalField(ByVal objQuery As clsQuery) As Reply

        If mTerminalApp Is Nothing Then
            Throw New InvalidOperationException(My.Resources.NotConnected_1)
        End If

        Dim sErr As String = Nothing
        Dim field As clsTerminalField = GetTerminalField(mTerminalApp, objQuery)
        If mTerminalApp.HighlightField(field, sErr) Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(sErr)
        End If

    End Function

    ''' <summary>
    ''' Gets the terminal field details from the query.
    ''' </summary>
    ''' <param name="objQuery">The query</param>
    ''' <returns>A new clsTerminalFeild containing the details</returns>
    Private Shared Function GetTerminalField(terminalApp As Terminal, objQuery As clsQuery) As clsTerminalField

        Dim startCol, startRow, endCol, endRow As Integer
        Dim fieldType As clsTerminalField.FieldTypes
        Try
            startCol = objQuery.GetIdentifierIntValue(IdentifierTypes.StartX)
            startRow = objQuery.GetIdentifierIntValue(IdentifierTypes.StartY)
            endCol = objQuery.GetIdentifierIntValue(IdentifierTypes.EndX)
            endRow = objQuery.GetIdentifierIntValue(IdentifierTypes.EndY)
            fieldType = clsEnum(Of clsTerminalField.FieldTypes).Parse(
                objQuery.GetIdentifierValue(IdentifierTypes.FieldType))

        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToParseInputs0, ex.Message))
        End Try

        Return New clsTerminalField(terminalApp.SessionSize, fieldType, startRow, startCol, endRow, endCol)

    End Function

    <Category(Category.Terminal)>
    <Command("Gets the coordinates of the cursor position in the presentation space. Currently only implemented for IBM mainframes.")>
    <Parameters("None.")>
    <Response("The XML of an Automate collection, with fields ""Row"" and ""Column"" whose values are indexed from 1.")>
    Private Function ProcessCommandGetMainframeCursorPos(ByVal objQuery As clsQuery) As Reply
        Dim row, col As Integer
        Dim sErr As String = Nothing
        If mTerminalApp.GetCursorPosition(row, col, sErr) Then
            Dim xdoc As New XmlDocument
            Dim rootNode As XmlElement = xdoc.CreateElement("collection")
            Dim rowNode As XmlElement = xdoc.CreateElement("row")
            rowNode.AppendChild(CreateCollectionFieldXML(xdoc, row.ToString, "number", "Row"))
            rowNode.AppendChild(CreateCollectionFieldXML(xdoc, col.ToString, "number", "Column"))
            rootNode.AppendChild(rowNode)
            Return Reply.Result(rootNode.OuterXml)
        Else
            Throw New InvalidOperationException(sErr)
        End If
    End Function

    <Category(Category.Terminal)>
    <Command("Sets the coordinates of the cursor position in the presentation space. Currently only implemented for IBM mainframes.")>
    <Parameters("TargX, TargY. The new cursor position, corresponding to column and row (respectively), indexed from 1.")>
    Private Function ProcessCommandSetMainframeCursorPos(ByVal objQuery As clsQuery) As Reply

        Dim colVal As String = objQuery.GetParameter(ParameterNames.TargX)
        Dim col As Integer
        If Not Integer.TryParse(colVal, col) Then
            Throw New InvalidOperationException(My.Resources.FailedToParseValueForColumnCoordinate)
        End If

        Dim rowVal As String = objQuery.GetParameter(ParameterNames.TargY)
        Dim row As Integer
        If Not Integer.TryParse(rowVal, row) Then
            Throw New InvalidOperationException(My.Resources.FailedToParseValueForRowCoordinate)
        End If

        Dim sErr As String = Nothing
        If Me.mTerminalApp.SetCursorPosition(row, col, sErr) Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(sErr)
        End If
    End Function

    <Category(Category.Terminal)>
    <Command("Gets the text of the emulator window title. Currently only implemented for IBM mainframes.")>
    <Parameters("None.")>
    Private Function ProcessCommandGetMainframeParentWindowTitle(ByVal objQuery As clsQuery) As Reply
        Dim Title As String = Nothing
        Dim sErr As String = Nothing
        If Me.mTerminalApp.GetParentWindowTitle(Title, sErr) Then
            Return Reply.Result(Title)
        Else
            Throw New InvalidOperationException(sErr)
        End If
    End Function

    <Category(Category.Terminal)>
    <Command("Sets the text of the emulator window title. Currently only implemented for IBM mainframes.")>
    <Parameters("NewText. The new text to set in the window title.")>
    Private Function ProcessCommandSetMainframeParentWindowTitle(ByVal objQuery As clsQuery) As Reply
        Dim Title As String = objQuery.GetParameter(ParameterNames.NewText)
        Dim sErr As String = Nothing
        If Me.mTerminalApp.SetParentWindowTitle(Title, sErr) Then
            Return Reply.Ok
        Else
            Throw New InvalidOperationException(sErr)
        End If
    End Function

    <Category(Category.Terminal)>
    <Command("Runs the named macro defined in the target emulator environment. Currently only implemented for Attachmate mainframes.")>
    <Parameters("NewText. The name of the macro to be run, only. Full file paths are not supported.")>
    Private Function ProcessCommandRunMainframeMacro(ByVal objQuery As clsQuery) As Reply
        Dim MacroName As String = objQuery.GetParameter(ParameterNames.NewText)
        If String.IsNullOrEmpty(MacroName) Then
            Throw New InvalidOperationException(My.Resources.NoMacroNameSpecified)
        Else
            Dim sErr As String = Nothing
            If Me.mTerminalApp.RunMacro(MacroName, sErr) Then
                Return Reply.Ok
            Else
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToRunMacro0, sErr))
            End If
        End If
    End Function

    <Category(Category.Terminal)>
    <Command("Sends keypresses to the active mainframe")>
    <Parameters("'newtext' to specify the characters to type 'special' characters can be specified using control codes.
    * For Terminal applications - the text is processed directly by the terminal emulator. Codes such as @E for Enter are common. Full documentation to be located. ")>
    Private Function ProcessCommandMainframeSendKeys(q As clsQuery) As Reply
        If mTerminalApp IsNot Nothing Then
            Dim sErr As String = Nothing
            If Not mTerminalApp.SendControlKeys(q.GetParameter(ParameterNames.NewText), sErr) Then
                Throw New InvalidOperationException(sErr)
            End If
        End If
        Return Reply.Ok
    End Function

End Class
