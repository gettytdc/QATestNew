Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Collections.CollectionUtil
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

Friend Class frmStagePropertiesCode
    Implements IDataItemTreeRefresher
#Region "Members"

    ''' <summary>
    ''' The original stage at the time that the form was opened. This is cloned from
    ''' the stage at the point it is set into the form.
    ''' </summary>
    ''' <seealso cref="OnClosing" />
    Private mOriginal As clsProcessStage

#End Region

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mParamsControl.InputsList.SuppressedDataTypes =
         clsProcessDataTypes.GetFrameworkIncompatibleDataTypes
        mParamsControl.OutputsList.SuppressedDataTypes =
         clsProcessDataTypes.GetFrameworkIncompatibleDataTypes
    End Sub

    ''' <summary>
    ''' Overrides base property so that we can capture the original stage rather than
    ''' the clone.
    ''' </summary>
    Public Overrides Property ProcessStage() As clsProcessStage
        Get
            Return MyBase.ProcessStage
        End Get
        Set(ByVal value As clsProcessStage)
            MyBase.ProcessStage = value

            'Capture the original details
            mOriginal = value.Clone()
        End Set

    End Property

    ''' <summary>
    ''' Closing event handler to undo any changes when the OK
    ''' button has not been pressed.
    ''' </summary>
    Protected Overrides Sub OnClosing(ByVal e As CancelEventArgs)
        MyBase.OnClosing(e)
        ' If the close is cancelled, or the user is OK'ing, leave everything as is
        If e.Cancel OrElse DialogResult = DialogResult.OK Then Return
        ' Otherwise reset the stage in the process to the original
        mProcessStage.Process.SetStage(mProcessStage.Id, mOriginal)
    End Sub

    ''' <summary>
    ''' Strongly typed reference to the code stage represented by this form
    ''' </summary>
    Private ReadOnly Property CodeStage() As clsCodeStage
        Get
            Return DirectCast(mProcessStage, clsCodeStage)
        End Get
    End Property

    Public ReadOnly Property Stage As clsProcessStage Implements IDataItemTreeRefresher.Stage
        Get
            Return ProcessStage
        End Get
    End Property

    ''' <summary>
    ''' Populates user interface with stage data.
    ''' </summary>
    Protected Overrides Sub PopulateStageData()

        MyBase.PopulateStageData()

        'Populate treeview
        mDataItemTree.Populate(mProcessStage)

        'This populates the inputs/outputs tabs
        mParamsControl.SetStage(mProcessStage, Me.ProcessViewer, Nothing)
        mParamsControl.RefreshControls(
         mProcessStage.GetInputs(), mProcessStage.GetOutputs())
        mParamsControl.Treeview = mDataItemTree

        'Populate the code editor...
        mParamsControl.CodeEditor.Populate(CodeStage.CodeText, CodeStage.Language)

        mDataItemTree.ProcessViewer = Me.ProcessViewer
    End Sub

    ''' <summary>
    ''' Updates the list of I/O parameters maintained in this form
    ''' </summary>
    Private Sub UpdateParameterList()
        ' Setting the stage has the effect of resetting the list to that which is
        ' now in the stage
        mParamList.Stage = CodeStage
    End Sub

    ''' <summary>
    ''' Checks the code fragment for errors.
    ''' </summary>
    Private Sub HandleCodeCheck(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mParamsControl.CheckCode

        Dim proc As clsProcess = mProcessStage.Process

        If proc.CompilerRunner Is Nothing Then
            UserMessage.Show(My.Resources.UnableToCheckCode)
            Return
        End If
        Try
            CodeStage.CodeText = mParamsControl.CodeEditor.Code
        Catch ex As Exception
            UserMessage.Err(ex, ex.Message)
        End Try
        'This goes against the way properties forms usually work.
        'Normally the process is updated after the properties form
        'closes with DialogResult.OK. In this case the process is 
        'updated now so that the code fragment can be tested. The
        'FormClosing event handler will undo any changes if the OK
        'button has not been pressed.
        proc.SetStage(mProcessStage.Id, mProcessStage)

        Try
            Using valForm As New frmValidateResults(
             proc, mProcessStage, ValidateProcessResult.SourceTypes.Code)

                valForm.Owner = Me
                valForm.StartPosition = FormStartPosition.CenterParent
                valForm.ShowInTaskbar = False
                valForm.ShowDialog()
            End Using
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.AnErrorOccurredWhileValidating0, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Checks if the given parameter has a name set in it or not.
    ''' </summary>
    ''' <param name="param">The parameter to test</param>
    ''' <returns>True if the parameter is named; False otherwise</returns>
    Private Function HasName(ByVal param As clsProcessParameter) As Boolean
        Return param.Name <> ""
    End Function

    ''' <summary>
    ''' Checks if the given parameter has no name set in it.
    ''' </summary>
    ''' <param name="param">The parameter to test</param>
    ''' <returns>True if the parameter has no name; False otherwise</returns>
    Private Function HasNoName(ByVal param As clsProcessParameter) As Boolean
        Return param.Name = ""
    End Function

    ''' <summary>
    ''' Handles the changing of tabs on the inputs outputs code.
    ''' </summary>
    Private Sub HandleTabChanged(
     ByVal sender As Object, ByVal e As TabControlEventArgs) _
     Handles mParamsControl.TabSelected
        If DirectCast(e.TabPage.Tag, CodeStageTabs.TabTypes) = CodeStageTabs.TabTypes.Code Then
            ' Set the named input and output params into the code stage
            mProcessStage.ClearParameters()
            mProcessStage.AddParameters(MergeList(
             Filter(mParamsControl.GetInputParameters(False), AddressOf HasName),
             Filter(mParamsControl.GetOutputParameters(False), AddressOf HasName))
            )
            UpdateParameterList()
            mSwitcher.SelectedTab = tpCodeParams
        Else
            mSwitcher.SelectedTab = tpDataItemTree
        End If
    End Sub

    ''' <summary>
    ''' Applies changes to underlying stage.
    ''' </summary>
    ''' <returns>See base class.</returns>
    Protected Overrides Function ApplyChanges() As Boolean

        ' Get all the parameters - inputs first
        Dim allParams As IList(Of clsProcessParameter) = MergeList(
         mParamsControl.GetInputParameters(False),
         mParamsControl.GetOutputParameters(False)
        )

        ' Check that all params have names
        Dim noNames As ICollection(Of clsProcessParameter) =
         Filter(allParams, AddressOf HasNoName)

        If noNames.Count > 0 Then
            Dim inputOutputParam As String = CStr(IIf(First(noNames).Direction = ParamDirection.In,
                                       My.Resources.ThereIsAnInputParameterWithNoName,
                                       My.Resources.ThereIsAnOutputParameterWithNoName))
            UserMessage.Err(
             inputOutputParam &
             My.Resources.PleaseEnsureAllParametersAreNamed)
            Return False
        End If

        ' Check for parameter name clashes
        Dim nameCounter As New clsCounterMap(Of String)
        For Each param As clsProcessParameter In allParams
            nameCounter(param.Name) += 1
        Next
        Dim sb As New StringBuilder()
        For Each kv As KeyValuePair(Of String, Integer) In nameCounter
            If kv.Value > 1 Then
                If sb.Length > 0 Then sb.Append(My.Resources.frmStagePropertiesCode_ApplyChanges_Comma)
                sb.Append(""""c).Append(kv.Key).Append(""""c)
            End If
        Next
        If sb.Length > 0 Then
            UserMessage.Err(
             My.Resources.ThereAreTooManyParametersWithTheSameNames0PleaseEnsureAllParameterNamesAreUnique, sb)
            Return False
        End If

        ' Check uniqueness of data item name
        Dim conflictStage As clsProcessStage =
         CollectionUtil.First(mProcessStage.FindNamingConflicts(txtName.Text))

        If conflictStage IsNot Nothing Then
            UserMessage.Err(
             My.Resources.frmStagePropertiesCode_TheChosenNameForThisStageConflictsWithTheStage0OnPage1PleaseChooseAnother,
             conflictStage.Name, conflictStage.GetSubSheetName())
            txtName.Focus()
            Return False
        End If

        ' Apply the other changes
        If Not MyBase.ApplyChanges() Then Return False

        Dim code As String = mParamsControl.CodeEditor.Code
        If CodeStage.CodeText <> code Then
            CodeStage.CodeText = code
            CodeStage.Compiled = False
        End If

        'Update the object's parameters
        mProcessStage.Parameters = allParams

        If SomeParametersChanged() Then CodeStage.Compiled = False

        Return True

    End Function


    ''' <summary>
    ''' Checks to see if parameters have changed but doesn't care if parameter values
    ''' have changed only the name and datatype
    ''' </summary>
    ''' <returns>True if parameters have changed, otherwise False</returns>
    Private Function SomeParametersChanged() As Boolean
        Dim stg As clsProcessStage = mProcessStage.Process.GetStage(mProcessStage.Id)

        Dim origCount As Integer = stg.Parameters.Count
        If origCount <> mProcessStage.Parameters.Count Then Return True

        For i As Integer = 0 To origCount - 1
            Dim oldParam As clsProcessParameter = stg.GetParameters(i)
            Dim newParam As clsProcessParameter = mProcessStage.GetParameters(i)
            If oldParam.Name <> newParam.Name _
             OrElse oldParam.ParamType <> newParam.ParamType _
             OrElse oldParam.Direction <> newParam.Direction Then Return True
        Next

        Return False
    End Function

    ''' <summary>
    ''' Gets the filename of the relevant help file.
    ''' </summary>
    ''' <returns>The filename.</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesCode.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Public Sub Repopulate(displayStage As clsDataStage) Implements IDataItemTreeRefresher.Repopulate
        mDataItemTree.Repopulate(displayStage)
    End Sub

    Public Sub Remove(stage As clsDataStage) Implements IDataItemTreeRefresher.Remove
        mDataItemTree.RemoveDataItemTreeNode(stage)
    End Sub
End Class