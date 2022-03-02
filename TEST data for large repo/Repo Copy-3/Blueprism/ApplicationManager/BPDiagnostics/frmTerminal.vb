
Imports System.Collections.Generic
Imports System.IO

Imports BluePrism.TerminalEmulation
Imports BluePrism.AMI
Imports BluePrism.BPCoreLib
Imports BluePrism.ApplicationManager.AMI

Public Class frmTerminal

    ''' <summary>
    ''' The terminal with which we are interacting.
    ''' </summary>
    Private mTerminal As Terminal

    Private mFieldCount As Integer = 0


    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click

        Dim filter As String
        Select Case CType(cmbTerminalType.SelectedItem, clsApplicationTypeInfo).ID
            Case "MainframeIBM"
                filter = My.Resources.IBMMainframeSessionFilesWsWs
            Case "MainframeHUM"
                filter = My.Resources.HummingbirdMainframeSessionFilesHepHep
            Case Else
                filter = My.Resources.AllFiles
        End Select

        Dim ofd As New OpenFileDialog
        ofd.Filter = filter
        ofd.AddExtension = True
        ofd.CheckFileExists = True
        ofd.CheckPathExists = True
        ofd.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)
        ofd.Multiselect = False
        ofd.RestoreDirectory = True
        ofd.ShowHelp = False
        ofd.Title = My.Resources.ChooseSessionFile

        If ofd.ShowDialog = Windows.Forms.DialogResult.OK Then
            txtSessionFile.Text = ofd.FileName
        End If
    End Sub

    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        If Not mTerminal Is Nothing Then
            If lvFields.SelectedItems.Count > 0 Then
                Try
                    lvFields.SelectedItems(0).Remove()
                Catch ex As Exception
                    MessageBox.Show(String.Format(My.Resources.OperationFailed0, ex.Message))
                End Try
            End If
        End If
    End Sub

    Private Sub btnSpyNewField_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSpyNewField.Click
        If Not mTerminal Is Nothing Then
            Dim F As clsTerminalField = Nothing
            Dim Result As String = mTerminal.DoSpy(F, Nothing)
            Select Case True
                Case Result.StartsWith("ERROR") OrElse Result.StartsWith("EXCEPTION")
                    MessageBox.Show(String.Format(My.Resources.OperationFailed0, Result))
                Case Result.StartsWith("CANCEL")
                    'Do nothing
                    Exit Sub
                Case Result = "TERMINALFIELD"
                    'Add new field row to listview, and select it
                    mFieldCount += 1
                    Dim LVI As New ListViewItem(mFieldCount.ToString)
                    LVI.SubItems.Add(F.StartColumn.ToString)
                    LVI.SubItems.Add(F.StartRow.ToString)
                    LVI.SubItems.Add(F.EndColumn.ToString)
                    LVI.SubItems.Add(F.EndRow.ToString)
                    LVI.SubItems.Add(F.FieldType.ToString)
                    LVI.Tag = F
                    lvFields.Items.Add(LVI)
                    LVI.Selected = True
            End Select
        Else
            MessageBox.Show(My.Resources.NotConnectedToATerminal)
        End If
    End Sub

    Private Sub btnSetText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetText.Click
        If Not mTerminal Is Nothing Then
            If lvFields.SelectedItems.Count > 0 Then
                Try
                    Dim NewText As String = Me.txtFieldText.Text
                    Dim sErr As String = Nothing
                    Dim CurrentField As clsTerminalField = CType(lvFields.SelectedItems(0).Tag, clsTerminalField)

                    Dim field As New clsTerminalField(mTerminal.SessionSize, CurrentField.FieldType, CurrentField.StartRow, CurrentField.StartColumn, CurrentField.EndRow, CurrentField.EndColumn)
                    If Not mTerminal.SetField(field, NewText, sErr) Then
                        MessageBox.Show(String.Format(My.Resources.OperationFailed0, sErr))
                    End If
                Catch ex As Exception
                    MessageBox.Show(String.Format(My.Resources.OperationFailed0, ex.Message))
                End Try
            Else
                MessageBox.Show(My.Resources.NoFieldIsSelectedFromTheListPleaseFirstSelectAField)
            End If
        Else
            MessageBox.Show(My.Resources.NotConnectedToATerminal)
        End If
    End Sub

    Private Sub btnGetText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetText.Click
        If Not mTerminal Is Nothing Then
            If lvFields.SelectedItems.Count > 0 Then
                Try
                    Dim NewText As String = ""
                    Dim sErr As String = Nothing
                    Dim CurrentField As clsTerminalField = CType(lvFields.SelectedItems(0).Tag, clsTerminalField)

                    Dim field As New clsTerminalField(mTerminal.SessionSize, clsTerminalField.FieldTypes.MultilineWrapped, CurrentField.StartRow, CurrentField.StartColumn, CurrentField.EndRow, CurrentField.EndColumn)
                    If Not mTerminal.GetField(field, NewText, sErr) Then
                        MessageBox.Show(String.Format(My.Resources.OperationFailed0, sErr))
                    Else
                        Me.txtFieldText.Text = NewText
                    End If
                Catch ex As Exception
                    MessageBox.Show(String.Format(My.Resources.OperationFailed0, ex.Message))
                End Try
            Else
                MessageBox.Show(My.Resources.NoFieldIsSelectedFromTheListPleaseFirstSelectAField)
            End If
        Else
            MessageBox.Show(My.Resources.NotConnectedToATerminal)
        End If
    End Sub

    <Obsolete>
    Private Sub btnLaunch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLaunch.Click

        Dim si As New SessionStartInfo

        Dim apptype As clsApplicationTypeInfo = CType(cmbTerminalType.SelectedItem, clsApplicationTypeInfo)

        si.TerminalType = clsApplicationTypeInfo.ParseTerminalType(apptype)

        'Get Session ID, if at all
        Dim sessionID As String = Nothing
        If txtSessionID.Text.Length > 0 Then
            If txtSessionID.Text.Length = 1 AndAlso Char.IsLetter(txtSessionID.Text(0)) Then
                sessionID = txtSessionID.Text
            Else
                MessageBox.Show(My.Resources.SessionIDMustBeASingleLetterFromAZ)
                Exit Sub
            End If
        End If
        si.SessionID = sessionID

        'Get session file, if any
        Dim sessionFile As String
        If txtSessionFile.Text.Length = 0 Then
            sessionFile = ""
        ElseIf File.Exists(txtSessionFile.Text) Then
            sessionFile = txtSessionFile.Text
        Else
            MessageBox.Show(My.Resources.TheSpecifiedSessionFileDoesNotSeemToExistOnDisk)
            Exit Sub
        End If
        si.SessionFile = sessionFile

        'Make sure either session file, or session ID is given
        If sessionFile = "" AndAlso sessionID = "" Then
            MessageBox.Show(My.Resources.YouMustChooseEitherASessionFileOrASessionID)
            Exit Sub
        End If

        Dim SessionDLLName As String = Nothing
        If txtSessionDLLName.Text.Length > 0 Then
            SessionDLLName = txtSessionDLLName.Text
        End If
        si.SessionDLLName = SessionDLLName

        Dim SessionDLLEntryPoint As String = Nothing
        If txtSessionDLLEntryPoint.Text.Length > 0 Then
            SessionDLLEntryPoint = txtSessionDLLEntryPoint.Text
        End If
        si.SessionDLLEntryPoint = SessionDLLEntryPoint
        si.Convention = Runtime.InteropServices.CallingConvention.Cdecl

        si.WaitSleepTime = 0
        si.WaitTimeout = 0
        si.MainframeSpecificInfo = ""

        'Do the launch
        mTerminal = New Terminal()
        Try
            Cursor = Cursors.WaitCursor
            mTerminal.ConnectToHostOrSession(si)
        Catch ex As Exception
            mTerminal = Nothing
            MessageBox.Show(String.Format(My.Resources.FailedToConnect0, ex.Message))
        End Try
        Cursor = Cursors.Default

    End Sub


    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        PopulateTerminalTypes()
        cmbTerminalType.SelectedIndex = 0
    End Sub


    ''' <summary>
    ''' Populate the list of terminal types. We use AMI's data to do this.
    ''' </summary>
    Private Sub PopulateTerminalTypes()

        Dim ami As New clsAMI(New clsGlobalInfo)
        Dim apptypes = ami.GetApplicationTypes()

        For Each apptype As clsApplicationTypeInfo In apptypes
            If apptype.ID = "Mainframe" Then
                For Each subtype As clsApplicationTypeInfo In apptype.SubTypes
                    cmbTerminalType.Items.Add(subtype)
                Next
            End If
        Next

    End Sub

End Class
