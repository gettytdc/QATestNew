Option Strict Off

Public Class HUMTerminal
    Inherits AbstractTerminal

    ''' <summary>
    ''' Reference to the hostexplorer COM object.
    ''' </summary>
    Private mHostEx As Object

    ''' <summary>
    ''' The session with which this class communicates.
    ''' </summary>
    ''' <remarks>There is a one-one correspondence between
    ''' each instance of this class, and each host.</remarks>
    Private mCurrentSession As Object

    ''' <summary>
    ''' This method is obsolete, use Launch or Attach instead. Provided for backwards
    ''' compatability this method connects to the specified host or session. As
    ''' such there is no need to modify this function, so just leave it as is
    ''' </summary>
    <Obsolete>
    Public Overrides Function ConnectToHostOrSession(sessionProfile As String, sessionShortName As String, ByRef sErr As String) As Boolean
        'The parameter SessionShortName is not used. It appears because it is part of 
        'the union of interfaces with other similar APIs

        Try
            If mHostEx Is Nothing Then
                mHostEx = CreateObject("HostExplorer")
            End If

            If mCurrentSession IsNot Nothing Then
                sErr = My.Resources.AlreadyConnectedToASessionYouMustDisconnectItFirst
                Return False
            End If

            Dim sessionId As Short = mHostEx.StartSession(sessionProfile)
            Me.mCurrentSession = mHostEx.Hosts(sessionId)

            If mCurrentSession Is Nothing Then
                sErr = My.Resources.FailedToRetrieveHostAfterConnecting
                Return False
            End If

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' This method is obsolete, use Detach or Terminate instead. Provided for
    ''' backwards compatability this method disconnects from the target. As
    ''' such there is no need to modify this function, so just leave it as is
    ''' </summary>
    <Obsolete>
    Public Overrides Function DisconnectFromHost(ByRef sErr As String) As Boolean

        Try
            If mCurrentSession Is Nothing Then
                sErr = My.Resources.NotConnectedToASessionCallConnectToHostFirst
                Return False
            End If

            mCurrentSession.Disconnect()
            mCurrentSession.Close()
            mCurrentSession = Nothing
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Property that provides lazy load access to the HostExplorer Object.
    ''' </summary>
    Private ReadOnly Property HostExplorer As Object
        Get
            If mHostEx Is Nothing Then
                mHostEx = CreateObject("HostExplorer")
            End If
            Return mHostEx
        End Get
    End Property

    ''' <summary>
    ''' Launch the terminal emulator
    ''' </summary>
    ''' <param name="sessionProfile">The path to a file on disk, representing the
    ''' desired session. This value must be appropriate to the type of target terminal,
    ''' which means that the callee must know about the type. Not relevant for all
    ''' terminal types, however at least one of either SessionProfile or
    ''' SessionShortName must be supplied in each case.</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Overrides Function Launch(sessionProfile As String, ByRef sErr As String) As Boolean
        Try

            If mCurrentSession IsNot Nothing Then
                sErr = My.Resources.AlreadyConnectedToASessionYouMustDetachOrTerminateFirst
                Return False
            End If

            Dim sessionId As Short = HostExplorer.StartSession(sessionProfile)
            mCurrentSession = HostExplorer.Hosts(sessionId)

            If mCurrentSession Is Nothing Then
                sErr = My.Resources.FailedToRetrieveHostAfterConnecting
                Return False
            End If

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True

    End Function

    ''' <summary>
    ''' Attach to an existing terminal emulator session.
    ''' </summary>
    ''' <param name="SessionShortName">A letter from A..Z representing the session ID.
    ''' Not relevant for all terminal types - see above.</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Overrides Function Attach(sessionShortName As String, ByRef sErr As String) As Boolean
        Try

            mCurrentSession = HostExplorer.HostFromShortName(sessionShortName)

            If mCurrentSession Is Nothing Then
                sErr = String.Format(My.Resources.FailedToRetrieveSessionAfterAttachingToSession0, sessionShortName)
                Return False
            End If

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Dttach the terminal emulator session.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Overrides Function Detach(ByRef sErr As String) As Boolean
        Try
            If Not CheckSessionConnected(sErr) Then Return False

            mCurrentSession = Nothing

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Terminate the terminal emulator.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    Public Overrides Function Terminate(ByRef sErr As String) As Boolean
        Try
            If Not CheckSessionConnected(sErr) Then Return False

            mCurrentSession.Disconnect()
            mCurrentSession.Close()
            mCurrentSession = Nothing

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    Public Overrides Function GetText(startRow As Integer, startColumn As Integer, length As Integer, ByRef value As String, ByRef sErr As String) As Boolean

        Try
            If Not CheckSessionConnected(sErr) Then Return False

            If length < 0 Then
                sErr = My.Resources.LengthOfTextToRetrieveMustBeAtLeastZero
                Return False
            End If

            value = Space(length)

            value = mCurrentSession.TextRC(startRow, startColumn, length)

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    Public Overrides Function SendKeystroke(key As String, ByRef sErr As String) As Boolean

        Try
            If Not CheckSessionConnected(sErr) Then Return False

            Me.mCurrentSession.Keys(key)
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function SetText(startRow As Integer, startColumn As Integer, value As String, ByRef sErr As String) As Boolean

        Try
            If Not CheckSessionConnected(sErr) Then Return False

            Me.mCurrentSession.PutText(value, startRow, startColumn)
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

        Return True
    End Function

    Public Overrides Function GetSessionSize(ByRef numRows As Integer, ByRef numColumns As Integer, ByRef sErr As String) As Boolean

        Try
            If Not CheckSessionConnected(sErr) Then Return False

            numRows = mCurrentSession.Rows
            numColumns = mCurrentSession.Columns
            Return True
        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function GetWindowTitle(ByRef value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function SetWindowTitle(value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean


        Try
            If Not CheckSessionConnected(sErr) Then Return False

            'Code from http://mimage.hummingbird.com/alt_content/binary/pdf/support/nc/automatinghostapplications.pdf

            'Use this method to set or get the cursor position
            'as an absolute number. To calculate the actual
            'Row and Column values, use the sample code
            'below:
            '  row = ((Host.Cursor - 1) / Host.Columns) + 1
            '  col = ((Host.Cursor-1) Mod Host.Columns) + 1

            Dim pos As Integer = mCurrentSession.Cursor()
            Dim cols As Integer = mCurrentSession.Columns
            row = ((pos - 1) \ cols) + 1
            col = ((pos - 1) Mod cols) + 1

            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try

    End Function

    Public Overrides Function SetCursorPosition(row As Integer, col As Integer, ByRef sErr As String) As Boolean


        Try
            If Not CheckSessionConnected(sErr) Then Return False

            mCurrentSession.CursorRC(row, col)
            Return True

        Catch e As Exception
            sErr = e.ToString
            Return False
        End Try
    End Function

    Public Overrides Function RunEmulatorMacro(macroName As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.NotImplementedForHummingbirdTerminals
        Return False
    End Function

    Public Overrides Function IsConnected() As Boolean
        Try
            Return (Me.mCurrentSession IsNot Nothing) AndAlso Me.mCurrentSession.isconnected()
        Catch e As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Function to provide a nice error message if the session doesn't exist
    ''' </summary>
    Public Function CheckSessionConnected(ByRef sErr As String) As Boolean
        If mCurrentSession Is Nothing Then
            sErr = My.Resources.NotConnectedToASessionLaunchOrAttachFirst
            Return False
        End If

        Return True
    End Function

    Public Overrides Function SelectArea(startRow As Integer, startColumn As Integer, endRow As Integer, endColumn As Integer, selectionType As SelectionType, ByRef sErr As String) As Boolean

        Try
            If Me.mCurrentSession Is Nothing Then
                sErr = My.Resources.NotConnectedToASessionCallConnectToHostFirst
                Return False
            End If

            Dim areaType As Integer
            Select Case selectionType
                Case SelectionType.Continuous
                    areaType = 2
                Case SelectionType.Block
                    areaType = 3
            End Select

            Dim area As Object = Me.mCurrentSession.Area(startRow, startColumn, endRow, endColumn, 0, areaType)
            If area Is Nothing Then
                sErr = My.Resources.FailedToRetrieveAreaSpecified
                Return False
            Else
                area.Select()
            End If

            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Property SleepTime() As Integer
        Get
            'do nothing - not relevant to hummingbird terminals
        End Get
        Set(value As Integer)
            'do nothing - not relevant to hummingbird terminals
        End Set
    End Property

    Public Overrides Property WaitTimeout() As Integer
        Get
            'do nothing - not relevant to hummingbird terminals
        End Get
        Set(value As Integer)
            'do nothing - not relevant to hummingbird terminals
        End Set
    End Property

End Class
