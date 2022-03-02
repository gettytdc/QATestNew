Imports System.Drawing
Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.WindowSpy
Imports BluePrism.Server.Domain.Models
Imports System.Windows.Forms
Imports BluePrism.ApplicationManager.ClientCommsI

Imports SpyMode = BluePrism.ApplicationManager.WindowSpy.clsWindowSpy.SpyMode
Imports TerminalTypes = BluePrism.BPCoreLib.SessionStartInfo.TerminalTypes
Imports BluePrism.BPCoreLib

''' <summary>
''' The interface with a terminal target application. This is an abstract
''' interface which wraps an underlying terminal emulator of whatever type from
''' the BPATerminalEmulation solution
''' </summary>
Public Class Terminal
    Implements IDisposable

#Region "Members"

    ''' <summary>
    ''' The terminal session.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moTerminal As AbstractTerminal


    ''' <summary>
    ''' The number of rows in the session.
    ''' </summary>
    ''' <remarks></remarks>
    Private miRows As Integer

    ''' <summary>
    ''' The number of columns in the session.
    ''' </summary>
    ''' <remarks></remarks>
    Private miColumns As Integer

    ''' <summary>
    ''' The handle of the terminal session window.
    ''' </summary>
    ''' <remarks></remarks>
    Private miTargetHandle As IntPtr

    ''' <summary>
    ''' The size of the session window.
    ''' </summary>
    ''' <remarks></remarks>
    Private mstrTargetSize As Size

    ''' <summary>
    ''' A flag to indicate that a session has started successfully.
    ''' </summary>
    ''' <remarks></remarks>
    Private mbSessionStarted As Boolean

    ''' <summary>
    ''' The grid overlay form used to define session fields.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents moGrid As frmSpy

    ''' <summary>
    ''' Event raised when a field is selected.
    ''' </summary>
    ''' <param name="field"></param>
    ''' <remarks></remarks>
    Public Event FieldSelected(ByVal field As clsTerminalField)

    ''' <summary>
    ''' Indicates whether the mouse is depressed on the
    ''' target win32 window.
    ''' </summary>
    ''' <remarks></remarks>
    Private mbMouseDownOnTarget As Boolean

    ''' <summary>
    ''' Inidicates whether the mouse is dragging on
    ''' the target win32 window.
    ''' </summary>
    ''' <remarks></remarks>
    Private mbMouseDraggingTarget As Boolean


#End Region

#Region "Construction & Disposal"

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not moTerminal Is Nothing Then
                moTerminal.Dispose()
                moTerminal = Nothing
            End If
            If Not moGrid Is Nothing Then
                moGrid.Dispose()
                moGrid = Nothing
            End If
        End If
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub
#End Region

#Region "Properties"

    ''' <summary>
    ''' Gets the size of the connected session, in rows and
    ''' columns.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Returns the size of the connected session,
    ''' eg 80x24 or returns (0,0) if not connected.</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property SessionSize() As SessionDimensions
        Get
            Return New SessionDimensions(miRows, miColumns)
        End Get
    End Property

#End Region

    Public Shared Function CreateTerminal(ByVal info As SessionStartInfo) As AbstractTerminal
        Select Case info.TerminalType
            Case TerminalTypes.GEN
                Return New HLLAPITerminal(info.SessionDLLName, info.SessionDLLEntryPoint, info.SessionType, info.Convention, info.Encoding)
            Case TerminalTypes.ATM
                Return New ATMTerminal()
            Case TerminalTypes.HUM
                Return New HUMTerminal()
            Case TerminalTypes.IBM
                Return New IBMTerminal()
            Case TerminalTypes.PSS
                Return New PassportTerminal()
            Case TerminalTypes.RMD
                Return New RMDTerminal()
            Case TerminalTypes.ART
                Return New ARTerminal()
            Case TerminalTypes.ARN
                Return New ARNetTerminal()
            Case TerminalTypes.RUM
                'Rumba
                Return New HLLAPITerminal("EHLAPI32.DLL", "hllapi", SessionStartInfo.SessionTypes.Enhanced, CallingConvention.StdCall, Encoding.Default)
            Case TerminalTypes.TMT
                'TeemTalk
                Return New HLLAPITerminal("PCSHLL32.DLL", "hllapi", SessionStartInfo.SessionTypes.Normal, CallingConvention.Cdecl, Encoding.Default)
            Case TerminalTypes.PCH
                'IBMPComHLL
                Return New HLLAPITerminal("PCSHLL32.DLL", "hllapi", SessionStartInfo.SessionTypes.Enhanced, CallingConvention.StdCall, Encoding.Default)
            Case TerminalTypes.IAC
                'IBMiAccess
                Return New HLLAPITerminal("PCSHLL32.DLL", "hllapi", SessionStartInfo.SessionTypes.Enhanced, CallingConvention.Cdecl, Encoding.Default)
            Case TerminalTypes.PWT
                'PowerTerm
                Return New HLLAPITerminal("HLLAPI32.DLL", "hllapi", SessionStartInfo.SessionTypes.Enhanced, CallingConvention.Cdecl, Encoding.Default)
            Case TerminalTypes.CON
                'Conductor
                Return New HLLAPITerminal("HLLAPI.DLL", "hllapi", SessionStartInfo.SessionTypes.NotImplemented, CallingConvention.Cdecl, Encoding.Default)
            Case TerminalTypes.HAT
                'HostAccess
                Return New HLLAPITerminal("HA7EHLL.DLL", "WinHLLAPI", SessionStartInfo.SessionTypes.Enhanced, CallingConvention.Cdecl, Encoding.Default)
            Case TerminalTypes.HEE
                'HostExplorer (EHLAPPI)
                Return New HLLAPITerminal("EHLAPI32.DLL", "hllapi", SessionStartInfo.SessionTypes.Enhanced, CallingConvention.StdCall, Encoding.Default)
            Case TerminalTypes.INF
                'InfoConnect (WinHLAPPI)
                Return New HLLAPITerminal("IHLAPI32.DLL", "WinHLLAPI", SessionStartInfo.SessionTypes.Normal, CallingConvention.StdCall, Encoding.Default)
            Case Else
                Throw New ArgumentException(My.Resources.UnrecognisedTerminalTypeRequested)
        End Select
    End Function

    ''' <summary>
    ''' This method is obsolete, use Launch or Attach instead. Provided for backwards
    ''' compatability this method connects to the specified host or session.
    ''' </summary>
    ''' <exception cref="System.Exception">Failure to connect.</exception>
    ''' <exception cref="System.Exception">Failure to get session size.</exception>
    ''' <exception cref="System.Exception">License does not permit the use of
    ''' mainframe applications.</exception>
    <Obsolete>
    Public Sub ConnectToHostOrSession(ByVal info As SessionStartInfo)

        Dim sErr As String = Nothing

        moTerminal = CreateTerminal(info)
        If info.WaitSleepTime > 0 Then
            moTerminal.SleepTime = info.WaitSleepTime
        End If
        If info.WaitTimeout > 0 Then
            moTerminal.WaitTimeout = info.WaitTimeout
        End If

        If Not String.IsNullOrEmpty(info.MainframeSpecificInfo) Then
            Select Case info.TerminalType
                Case TerminalTypes.ATM
                    Dim VariantType As ATMTerminal.HostAccessValues
                    Select Case info.MainframeSpecificInfo
                        Case "KEA"
                            VariantType = ATMTerminal.HostAccessValues.ATM_KEA
                        Case "KEA_HP"
                            VariantType = ATMTerminal.HostAccessValues.ATM_HP
                        Case "IRMA"
                            VariantType = ATMTerminal.HostAccessValues.ATM_IRMA
                        Case "RALLY"
                            VariantType = ATMTerminal.HostAccessValues.ATM_RALLY
                        Case "ICONN"
                            VariantType = ATMTerminal.HostAccessValues.ATM_ICONN
                        Case Else
                            VariantType = ATMTerminal.HostAccessValues.ATM_EXTRA
                    End Select
                    CType(moTerminal, ATMTerminal).VariantType = VariantType
            End Select
        End If

        If moTerminal.ConnectToHostOrSession(info.SessionFile, info.SessionID, sErr) Then

            'Wait for terminal to become connected, for max of 10 seconds.
            'Thereafter carry on blindly
            Dim StartWaitingTime As DateTime = DateTime.Now
            Dim MaxWaitingTime As TimeSpan = New TimeSpan(0, 0, 10) '10 seconds
            While Not moTerminal.IsConnected
                Thread.Sleep(50)
                If DateTime.Now.Subtract(StartWaitingTime) > MaxWaitingTime Then
                    Exit While
                End If
            End While

            If moTerminal.GetSessionSize(miRows, miColumns, sErr) Then
                mbSessionStarted = True
            Else
                moTerminal.Dispose()
                moTerminal = Nothing
                Throw New InvalidOperationException(String.Format(My.Resources.ErrorFromGetSessionSize0, sErr))
            End If
        Else
            moTerminal.Dispose()
            moTerminal = Nothing
            Throw New ApiException(String.Format(My.Resources.ErrorFromConnectToHostOrSession0, sErr))
        End If

    End Sub


    ''' <summary>
    ''' Determines if the target win32 window on which to lay the grid has been found.
    ''' </summary>
    ''' <returns>True if found; false if not.</returns>
    Private Function IsTerminalWindowFound() As Boolean
        Return Me.miTargetHandle <> IntPtr.Zero
    End Function


#Region "CreateForm"

    ''' <summary>
    ''' Creates a form over the window identified by the 
    ''' member target handle.</summary>
    Private Sub CreateForm()
        'Get size of target window
        Dim WindowRect As New RECT
        GetWindowRect(miTargetHandle, WindowRect)

        'Find out where the grid seems to lie within that rectangle
        Dim LocOffset As Size
        Dim SizeOffset As Size
        AdjustGridSize(Me.miTargetHandle, LocOffset, SizeOffset)

        'Create and show grid overlay, taking into account the apparent
        'bounds of the grid within the target window
        moGrid = New frmSpy(miTargetHandle, Point.Add(WindowRect.Location, LocOffset), Size.Subtract(WindowRect.Size, SizeOffset), miColumns, miRows)
        moGrid.LocationOffset = LocOffset
        moGrid.Customsize = True
        moGrid.Show()
    End Sub


    Private Sub AdjustGridSize(ByVal handle As IntPtr, ByRef LocationOffset As Size, ByRef SizeAdjustment As Size)
        Dim sErr As String = Nothing
        Dim Success As Boolean = False
        Success = moTerminal.SelectArea(1, 1, Me.miRows, Me.miColumns, AbstractTerminal.SelectionType.Block, sErr)
        Dim imgScreenShot As Image = (New clsScreenShot()).GetScreenShot(handle)
        Success = moTerminal.SelectArea(1, 1, 1, 1, AbstractTerminal.SelectionType.Block, sErr)

        If Success Then
            Dim TSA As New clsTerminalScreenshotAnalyser(moTerminal, CType(imgScreenShot, Bitmap))
            LocationOffset = New Size(TSA.LeftMarginEstimate, TSA.TopMarginEstimate)
            SizeAdjustment = New Size(TSA.LeftMarginEstimate + TSA.RightMarginEstimate, TSA.TopMarginEstimate + TSA.BottomMarginEstimate)
        Else
            LocationOffset = Size.Empty
            SizeAdjustment = Size.Empty
        End If
    End Sub

#End Region



    ''' <summary>
    ''' Text on terminal screen, last time it was observed.
    ''' </summary>
    ''' <remarks>Used in conjunction with the method UpdateFields.</remarks>
    Private mCachedTerminalText As String

    ''' <summary>
    ''' Updates the fields made visible.
    ''' </summary>
    ''' <remarks>This is achieved by keeping track of the text on the whole screen.
    ''' If the whole screen has changed, then the likelihood is that all fields
    ''' should be cleared and the native fields automatically added. If the text
    ''' remains the same, then all field should be kept, including native and user
    ''' drawn fields.</remarks>
    Private Sub UpdateFields()
        Dim ScreenText As String = Me.GetTerminalText
        If Not ScreenText = mCachedTerminalText Then
            moGrid.ClearFields()
        End If
        mCachedTerminalText = ScreenText
    End Sub

    ''' <summary>
    ''' Gets the text on the entire terminal screen.
    ''' </summary>
    ''' <returns>Returns the text as a string</returns>
    ''' <remarks>The members micolumns, mirows and moTerminal should be populated
    ''' first.</remarks>
    Private Function GetTerminalText() As String
        Dim sResult As String = Nothing
        Dim sErr As String = Nothing
        Dim field As New clsTerminalField(SessionSize, clsTerminalField.FieldTypes.Rectangular, 1, 1, miRows, Me.miColumns)
        Me.GetField(field, sResult, sErr)
        Return sResult
    End Function

    ''' <summary>
    ''' Runs the specified macro defined in the target emulator instance.
    ''' </summary>
    ''' <param name="MacroName">The name of the macro only. Full paths are not
    ''' supported.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>True on success, false otherwise. When false, refere to the sErr
    ''' parameter.</returns>
    Public Function RunMacro(ByVal MacroName As String, ByRef sErr As String) As Boolean
        Return Me.moTerminal.RunEmulatorMacro(MacroName, sErr)
    End Function

    ''' <summary>
    ''' The field spied by the spying form.
    ''' </summary>
    ''' <remarks>This is null except immediately after a spy.
    ''' After being read it should be made null.</remarks>
    Private mSpiedField As clsTerminalField

    ''' <summary>
    ''' Handles selection events from the grid form and invokes the
    ''' selection delegate function.
    ''' </summary>
    ''' <param name="f"></param>
    Private Sub moGrid_FieldSelected(ByVal f As clsTerminalField) Handles moGrid.FieldSelected
        mSpiedField = CType(f, clsTerminalField)
    End Sub

    Private Sub HandleTerminalWindowMissing() Handles moGrid.TerminalWindowMissing
        RaiseEvent Terminated()
    End Sub

    Public Sub BringToFront()
        If Me.IsTerminalWindowFound Then
            SetForegroundWindow(miTargetHandle)
        End If
    End Sub

    ''' <summary>
    ''' This method is obsolete, use Detach or Terminate instead. Provided for backwards
    ''' compatability this method disconnects from the host.
    ''' </summary>
    ''' <param name="sErr"></param>
    <Obsolete>
    Public Function DisconnectFromHost(ByRef sErr As String) As Boolean
        If moTerminal Is Nothing Then Return True
        Try
            Dim bResult As Boolean = moTerminal.DisconnectFromHost(sErr)
            moTerminal.Dispose()
            moTerminal = Nothing
            Return bResult
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Launch the terminal emulator
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Function Launch(info As SessionStartInfo, ByRef sErr As String) As Boolean
        moTerminal = CreateTerminal(info)
        If moTerminal.Launch(info.SessionFile, sErr) Then
            Return moTerminal.GetSessionSize(miRows, miColumns, sErr)
        End If
    End Function

    ''' <summary>
    ''' Attach to an existing terminal emulator session.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Function Attach(info As SessionStartInfo, ByRef sErr As String) As Boolean
        If moTerminal Is Nothing Then
            moTerminal = CreateTerminal(info)
        End If
        If moTerminal.Attach(info.SessionID, sErr) Then
            Return moTerminal.GetSessionSize(miRows, miColumns, sErr)
        End If
    End Function

    ''' <summary>
    ''' Dttach the terminal emulator session.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Function Detach(ByRef sErr As String) As Boolean
        Return moTerminal.Detach(sErr)
    End Function

    ''' <summary>
    ''' Terminate the terminal emulator.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Function Terminate(ByRef sErr As String) As Boolean
        Return moTerminal.Terminate(sErr)
    End Function

    <Obsolete>
    Public Function SendKeystroke(ByVal sKeys As String, ByRef sErr As String) As Boolean
        Return moTerminal.SendKeystroke(sKeys, sErr)
    End Function

    Public Function SendControlKeys(ByVal sKeys As String, ByRef sErr As String) As Boolean
        Return moTerminal.SendControlKeys(sKeys, sErr)
    End Function

    ''' <summary>
    ''' Sets the text in the specified field.
    ''' </summary>
    ''' <param name="field">The field.</param>
    ''' <param name="value">The text to set.</param>
    ''' <param name="sErr">Carries back an error message in the event of an error.
    ''' Relevant only when the return value is False.</param>
    ''' <returns>Returns true on success.</returns>
    Public Function SetField(field As clsTerminalField, value As String, ByRef sErr As String) As Boolean

        Select Case field.FieldType

            Case clsTerminalField.FieldTypes.Rectangular
                Dim FieldWidth As Integer = field.Width
                Dim RowCount As Integer = field.Height
                Dim RowsSet As Integer = 0

                While (RowsSet < RowCount) AndAlso ((RowsSet * FieldWidth) <= value.Length - 1)
                    Dim Length As Integer = Math.Min(value.Length - (RowsSet * FieldWidth), FieldWidth)
                    If Not moTerminal.SetText(field.StartRow + RowsSet, field.StartColumn, value.Substring(RowsSet * FieldWidth, Length), sErr) Then
                        Return False
                    End If
                    RowsSet += 1
                End While

                Return True

            Case Else
                If value.Length > field.Length Then
                    value = value.Substring(0, field.Length)
                End If
                Return moTerminal.SetText(field.StartRow, field.StartColumn, value, sErr)
        End Select
    End Function


    ''' <summary>
    ''' Search the terminal for instances of the given text.
    ''' </summary>
    ''' <param name="searchText">The text to search for.</param>
    ''' <returns>A List(Of Point) with an entry for each instance found.</returns>
    Public Function SearchTerminal(ByVal searchText As String) As List(Of Point)

        Dim screen As String = Nothing
        Dim sErr As String = Nothing
        Dim field As New clsTerminalField(SessionSize, clsTerminalField.FieldTypes.MultilineWrapped, 1, 1, miRows, miColumns)
        If Not GetField(field, screen, sErr) Then
            Throw New InvalidOperationException(String.Format(My.Resources.FailedToReadTerminalScreen0, sErr))
        End If

        screen = screen.ToLower()
        searchText = searchText.ToLower()

        Dim pp As New List(Of Point)
        Dim index As Integer = 0
        While True
            Dim found As Integer = screen.IndexOf(searchText, index)
            If found = -1 Then Exit While
            pp.Add(New Point((found Mod miColumns) + 1, CInt(Math.Floor(found / miColumns)) + 1))
            index = found + 1
        End While
        Return pp

    End Function

    ''' <summary>
    ''' Gets the text of the supplied terminal field.
    ''' </summary>
    ''' <param name="field">The field from which to retrieve text.</param>
    ''' <param name="result">String to carry back
    ''' the text found in the field.</param>
    ''' <param name="sErr">String to carry back an error
    ''' message. Relevant only when return value is false.</param>
    ''' <returns>Returns true on success. On error, a message is
    ''' supplied in the argument to the parameter sErr.</returns>
    ''' <exception cref="System.Exception">License does not permit the use of mainframe applications.</exception>
    Public Function GetField(field As clsTerminalField, ByRef result As String, ByRef sErr As String) As Boolean

        Select Case field.FieldType
            Case clsTerminalField.FieldTypes.MultilineWrapped
                Dim fieldText As String = Nothing
                If Not moTerminal.GetText(field.StartRow, field.StartColumn, field.Length, fieldText, sErr) Then
                    Return False
                End If

                result = fieldText
                Return True
            Case clsTerminalField.FieldTypes.Rectangular
                Dim size As Integer = field.Width * field.Height
                Dim fieldText As New StringBuilder(size)

                For i As Integer = field.StartRow To field.EndRow
                    Dim tempString As String = Nothing
                    If Not moTerminal.GetText(i, field.StartColumn, field.Width, tempString, sErr) Then
                        Return False
                    End If
                    fieldText.Append(tempString)
                Next

                result = fieldText.ToString
                Return True
            Case Else
                sErr = My.Resources.UnknownFieldType
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Gets the title of the parent window of the terminal emulator
    ''' owning the presentation space. Not available for all terminal
    ''' types. Such terminal return a "not implemented" error in this case.
    ''' </summary>
    ''' <param name="Value">The string in the window title.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns true on success.</returns>
    Public Function GetParentWindowTitle(ByRef Value As String, ByRef sErr As String) As Boolean
        Return Me.moTerminal.GetWindowTitle(Value, sErr)
    End Function


    ''' <summary>
    ''' Sets the title of the parent window of the terminal emulator
    ''' owning the presentation space. Not available for all terminal
    ''' types. Such terminal return a "not implemented" error in this case.
    ''' </summary>
    ''' <param name="Value">The new window title.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns true on success.</returns>
    Public Function SetParentWindowTitle(ByVal Value As String, ByRef sErr As String) As Boolean
        Return Me.moTerminal.SetWindowTitle(Value, sErr)
    End Function

    ''' <summary>
    ''' Gets the position of the cursor within the presentation space.
    ''' Not available for all terminal
    ''' types. Such terminal return a "not implemented" error in this case.
    ''' </summary>
    ''' <param name="col">Carries back the value of the column at which
    ''' the cursor is located. Indexed from 1; max value is the number of columns.</param>
    ''' <param name="row">Carries back the value of the row at which
    ''' the cursor is located. Indexed from 1; max value is the number of rows.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns true on succes. When false, see the sErr parameter.</returns>
    Public Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean
        Return Me.moTerminal.GetCursorPosition(row, col, sErr)
    End Function

    ''' <summary>
    ''' Sets the position of the cursor in the presentation space.
    ''' Not available for all terminal
    ''' types. Such terminal return a "not implemented" error in this case.
    ''' </summary>
    ''' <param name="col">The x coordinate of the new cursor position, indexed from 1.
    ''' Maximum allowable value is the number of columns.</param>
    ''' <param name="row">The y coordinate of the new cursor position, indexed from 1.
    ''' Maximum allowable value is the number of rows.</param>
    ''' <param name="sErr">Carries back a message in the event of an error.</param>
    ''' <returns>Returns true on succes. When false, see the sErr parameter.</returns>
    Public Function SetCursorPosition(ByVal row As Integer, ByVal col As Integer, ByRef sErr As String) As Boolean

        SessionSize.CheckContains(row, col)

        Return Me.moTerminal.SetCursorPosition(row, col, sErr)

    End Function


    ''' <summary>
    ''' Highlights the specified field on the grid, if one
    ''' exists. </summary>
    ''' <param name="field">The field</param>
    ''' <param name="sErr">Carries back an error message in the
    ''' event of an error. Relevant only when the return
    ''' value is false.</param>
    ''' <returns>Returns true on success.</returns>
    ''' <remarks>If the grid does not exist, then an error
    ''' is returned.</remarks>
    ''' <exception cref="System.Exception">License does not permit the use of mainframe applications.</exception>
    Public Function HighlightField(field As clsTerminalField, ByRef sErr As String) As Boolean
        If (Me.moGrid IsNot Nothing) Then

            Me.moGrid.TopMost = True
            Me.moGrid.Visible = True
            Me.moGrid.AddField(field)
            field.PermanentlyHighlighted = True
            moGrid.Invalidate(True)

            Dim EndWait As DateTime = DateTime.Now.Add(New TimeSpan(0, 0, 2))
            Do
                Application.DoEvents()
            Loop Until DateTime.Now > EndWait
            Me.moGrid.RemoveField(field)

            Me.moGrid.TopMost = False
            Return True
        Else
            sErr = My.Resources.NoGridPresent
            Return False
        End If
    End Function


    Public Event Terminated()

    ''' <summary>
    ''' Indicates whether any window spy operation currently underway has since been
    ''' cancelled via a separate call.
    ''' </summary>
    Private mbWindowSpyCancelled As Boolean

    ''' <summary>
    ''' Cancels any spy operation currently underway, otherwise has no effect.
    ''' </summary>
    Public Sub CancelSpy()
        mbWindowSpyCancelled = True
        If Not moGrid Is Nothing Then
            Me.moGrid.EndSpy()
        End If
    End Sub

    ''' <summary>
    ''' Performs the spy operation.
    ''' </summary>
    ''' <param name="SpiedField">The resulting field</param>
    ''' <param name="targetapp">The target app to spy</param>
    ''' <returns>True when a field has been spied, False if the user canceled</returns>
    ''' <exception cref="System.Exception">License does not permit the use of mainframe applications.</exception>
    Public Function DoSpy(ByRef SpiedField As clsTerminalField, ByVal targetapp As ITargetApp) As Boolean

        'If terminal window not yet identified, need to do that first.
        If Not Me.IsTerminalWindowFound Then

            Using s As New clsWindowSpy(SpyMode.Win32, SpyMode.Win32, True)
                s.StartSpy(targetapp)
                mbWindowSpyCancelled = False
                Do
                    MessagePump(25)
                Loop Until mbWindowSpyCancelled OrElse s.Ended

                If s.WindowChosen Then
                    Dim WI As clsWindowSpy.WindowInformation = s.CurrentWindow
                    miTargetHandle = WI.WindowHandle
                End If

                If mbWindowSpyCancelled Then
                    SpiedField = Nothing
                    Return False
                End If
            End Using
        End If

        'Create the grid if it does not yet exist
        If Me.moGrid Is Nothing Then
            CreateForm()
        End If

        'Next we spy for a terminal field
        Me.UpdateFields()

        Me.moGrid.UpdateScreenShot()
        Me.moGrid.TopMost = True
        Me.moGrid.Visible = True
        Me.moGrid.StartSpy()
        Do
            Application.DoEvents()
            Thread.Sleep(25)
        Loop Until (moGrid Is Nothing) OrElse (Not moGrid.Spying)   'test for null in case form is closed and disposed by user

        If Not moGrid Is Nothing Then
            Me.moGrid.TopMost = False
            'Me.moGrid.Visible = False
        End If

        'At this point the user has either cancelled (so we just return false)
        'or we need to populate the spied element parameter
        If (Not mSpiedField Is Nothing) AndAlso (Not mSpiedField.IsEmpty) Then
            Dim sFieldText As String = ""
            Dim sErr As String = Nothing
            If Not Me.GetField(mSpiedField, sFieldText, sErr) Then
                Throw New InvalidOperationException(String.Format(My.Resources.FailedToRetrieveFieldText0, sErr))
            End If

            mSpiedField.FieldText = sFieldText
            SpiedField = mSpiedField
            Return True
        Else
            Return False
        End If
    End Function


    ''' <summary>
    ''' Simple finite state machine based parser.
    ''' </summary>
    ''' <param name="keys">The key sequence to parse</param>
    ''' <returns>A enumerable keycode list</returns>
    Public Shared Function ParseKeySequence(keys As String) As IEnumerable(Of KeyCode)
        Dim list As New List(Of KeyCode)
        If keys Is Nothing Then Return list

        Dim inKeyCode = False
        Dim expectClose = False
        Dim currentKeyCode As StringBuilder = Nothing
        Dim previous As Char

        For Each c As Char In keys

            If expectClose AndAlso c <> "}"c Then
                Throw New InvalidFormatException(String.Format(My.Resources.AClosingBraceWasExpectedBut0WasFound, c))
            End If

            Select Case c

                Case "{"c

                    If inKeyCode Then
                        If previous = "{"c Then
                            expectClose = True
                        Else
                            Throw New InvalidFormatException(My.Resources.UnexpectedOpenBrace)
                        End If
                    Else
                        currentKeyCode = New StringBuilder()
                    End If

                    inKeyCode = True

                Case "}"c

                    If expectClose Then
                        AddCharacter(list, previous)
                        expectClose = False
                        inKeyCode = False
                        Exit Select
                    End If

                    If inKeyCode Then
                        If previous = "{"c Then
                            expectClose = True
                        Else
                            AddKeyCode(list, currentKeyCode)
                        End If
                    Else
                        Throw New InvalidFormatException(My.Resources.UnexpectedClosingBrace)
                    End If

                    inKeyCode = False

                Case "+"c, "^"c, "%"c, "~"c, "("c, ")"c, "["c, "]"c

                    If inKeyCode AndAlso previous = "{"c Then
                        expectClose = True
                    Else
                        Throw New NotImplementedException(My.Resources.SpecialCharactersAreNotImplemented)
                    End If

                Case Else

                    If inKeyCode Then
                        currentKeyCode.Append(c)
                    Else
                        AddCharacter(list, c)
                    End If

            End Select

            previous = c
        Next

        If inKeyCode OrElse expectClose Then
            Throw New InvalidFormatException(My.Resources.MissingClosingBrace)
        End If

        Return list
    End Function

    Private Shared Sub AddKeyCode(list As ICollection(Of KeyCode), currentKeyCode As StringBuilder)
        Dim code As KeyCodeMappings
        If currentKeyCode IsNot Nothing Then
            Dim text = currentKeyCode.ToString()
            If [Enum].TryParse(text, True, code) Then
                Dim keyCode As New KeyCode(code)
                list.Add(keyCode)
            Else
                Throw New InvalidFormatException(String.Format(My.Resources.InvalidKeycode0, text))
            End If
        End If
    End Sub

    Private Shared Sub AddCharacter(list As ICollection(Of KeyCode), currentChar As Char)
        Dim keyCode As New KeyCode(currentChar)
        list.Add(keyCode)
    End Sub
End Class
