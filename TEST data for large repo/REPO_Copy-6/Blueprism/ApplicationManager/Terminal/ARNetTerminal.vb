Option Strict Off
Imports System.Reflection
Imports System.Collections.Generic

Public Class ARNetTerminal
    Inherits AbstractTerminal

    Private ReadOnly mCreateMethod As Object

    Private mReflectionApp As Object
    Private mTerminal As Object
    Private mScreen As Object

    Private mIsConnected As Boolean

    Public Enum ControlKeyCode
        InvalidKey
        Transmit
        Erase_Eof
        Tab
        BackTab
        Down
        Up
        Right
        Left
        Insert
        F1
        F2
        F3
        F4
        F5
        F6
        F7
        F8
        F9
        F10
        F11
        F12
        F13
        F14
        F15
        F16
        F17
        F18
        F19
        F20
        F21
        F22
        F23
        F24
        PA1
        PA2
        PA3
        FieldExit
        Clear
        BackSpace
        DestructiveBackSpace
        Delete
        DeleteChar
        DeleteWord
        NextWord
        EraseInput
        RuleLine
        CursorSelect
        FieldMark
        ReverseField
        EndOfField
        Reset
        NewLine
        Home
        Attention
        SystemRequest
        Print
        AltCursor
        Dup
        ClearPartition
        PartitionJump
        PanRight
        CursorBlink
        FieldDelimiter
        LeftDouble
        RightDouble
        ScrollDown
        ScrollUp
        PanLeft
        DownDouble
        ExtGr
        FieldMinus
        FieldPlus
        Help
        Hex
        PageDown
        PageUp
        PlusCr
        RollDown
        RollUp
        TextAssistBeginBold
        TextAssistBeginOfLine
        TextAssistBeginUnderline
        TextAssistBottomOfPage
        TextAssistCarrierReturn
        TextAssistCenter
        TextAssistEndBold
        TextAssistEndOfLine
        TextAssistHalfIndexDown
        TextAssistHalfIndexUp
        TextAssistInsertSymbols
        TextAssistNextStop
        TextAssistNextTextColumn
        TextAssistPageEnd
        TextAssistRequiredSpace
        TextAssistRequiredTab
        TextAssistStop
        TextAssistTextTabAdvance
        TextAssistTopOfPage
        TextAssistWordUnderline
        Test
        UpDouble
        Duplicate
        PreviousWord
    End Enum

    Public Class KeyCodeAttribute : Inherits Attribute
        Public Property Key As ControlKeyCode
        Public Sub New(key As ControlKeyCode)
            Me.Key = key
        End Sub
    End Class

    Private Shared mKeyCodeMap As IDictionary(Of KeyCodeMappings, ControlKeyCode)

    Shared Sub New()
        mKeyCodeMap = New Dictionary(Of KeyCodeMappings, ControlKeyCode)
        For Each k As KeyCodeMappings In [Enum].GetValues(GetType(KeyCodeMappings))
            Dim attr = KeyCode.GetAttribute(Of KeyCodeAttribute)(k)
            If attr IsNot Nothing Then
                mKeyCodeMap.Add(k, attr.Key)
            End If
        Next
    End Sub

    Public Sub New()
        Dim a = Assembly.Load("Attachmate.Reflection.Framework, Version=1.2.0.0, Culture=neutral, PublicKeyToken=13bff1b6907eadcf")
        Dim t = a.GetType("Attachmate.Reflection.Framework.MyReflection")
        mCreateMethod = t.GetMethod("CreateApplication", New Type() {GetType(String), GetType(Boolean)})
        mIsConnected = False
    End Sub


    Public Overrides Function ConnectToHostOrSession(SessionProfile As String, SessionShortName As String, ByRef sErr As String) As Boolean

        Try

            mReflectionApp = mCreateMethod.Invoke(Nothing, New Object() {"myWorkspace", True})

            Dim terminals As Object() = mReflectionApp.GetControlsByFilePath(SessionProfile)
            If terminals IsNot Nothing AndAlso terminals.Length > 0 Then
                mTerminal = terminals(0)
            Else
                mTerminal = mReflectionApp.CreateControl(SessionProfile)
            End If

            Dim frame = mReflectionApp.GetObject("Frame")
            frame.CreateView(mTerminal)

            mScreen = mTerminal.Screen

            mIsConnected = True
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    Public Overrides Function DisconnectFromHost(ByRef sErr As String) As Boolean
        Try
            mTerminal.Disconnect()
            mIsConnected = False
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function Launch(sessionProfile As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Attach(sessionShortName As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Detach(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Terminate(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean
        Try
            col = mScreen.CursorColumn
            row = mScreen.CursorRow
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function GetSessionSize(ByRef numRows As Integer, ByRef numColumns As Integer, ByRef sErr As String) As Boolean
        Try
            numRows = mScreen.Rows
            numColumns = mScreen.Columns
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function GetText(StartRow As Integer, StartColumn As Integer, Length As Integer, ByRef Value As String, ByRef sErr As String) As Boolean
        Try
            Value = mScreen.GetText(StartRow, StartColumn, Length)
            Return True
        Catch ex As Exception
            sErr = ex.Message
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

    Public Overrides Function IsConnected() As Boolean
        Return mIsConnected
    End Function

    Public Overrides Function RunEmulatorMacro(macroName As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotImplemented
        Return False
    End Function

    Public Overrides Function SelectArea(StartRow As Integer, StartColumn As Integer, EndRow As Integer, EndColumn As Integer, Type As SelectionType, ByRef sErr As String) As Boolean
        Try
            mScreen.SetSelectionStartPos(StartRow, StartColumn)
            mScreen.ExtendSelection(EndRow, EndColumn)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SendKeystroke(Key As String, ByRef sErr As String) As Boolean
        Try
            mScreen.SendKeys(Key)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Try
            Dim sequence = Terminal.ParseKeySequence(keys)

            For Each k In sequence
                If k.IsCharacter Then
                    mScreen.SendKeys(k.Character)
                Else
                    Dim c = mKeyCodeMap(k.Code)
                    mScreen.SendControlKey(c)
                End If
            Next

            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function


    Public Overrides Function SetCursorPosition(row As Integer, col As Integer, ByRef sErr As String) As Boolean
        Try
            mScreen.MoveCursorTo(row, col)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SetText(StartRow As Integer, StartColumn As Integer, Value As String, ByRef sErr As String) As Boolean
        Try
            mScreen.PutText(Value, StartRow, StartColumn)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Property SleepTime As Integer

    Public Overrides Property WaitTimeout As Integer

    Protected Overrides Sub Dispose(disposing As Boolean)
        If mReflectionApp IsNot Nothing Then
            mReflectionApp.Close(1)
            mReflectionApp = Nothing
        End If

        MyBase.Dispose(disposing)
    End Sub

End Class
