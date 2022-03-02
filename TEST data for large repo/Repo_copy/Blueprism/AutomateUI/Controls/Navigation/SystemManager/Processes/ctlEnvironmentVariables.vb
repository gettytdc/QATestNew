Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore

Public Class ctlEnvironmentVariables : Implements IChild, IMode, IPermission

    ' List keeping track of environment variables which have been deleted.
    ' Cleared after successful handling of the 'Apply' button (which deletes the
    ' vars in this list from the database)
    Private mEnvironmentRowsDeleted As New List(Of clsEnvironmentVariable)

    ' List keeping track of existing environment variables
    Private mEnvironmentRowsExisting As New List(Of clsEnvironmentVariable)

    ' List of environment variables when the page was loaded
    Private ReadOnly mEnvironmentRowsOriginalRecords As New List(Of clsEnvironmentVariable)

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        lvEnvVars.MinimumColumnWidth = 50
        With lvEnvVars.Columns
            .Add(My.Resources.ctlEnvironmentVariables_Name).Width = 100
            .Add(My.Resources.ctlEnvironmentVariables_Type).Width = 110
            .Add(My.Resources.ctlEnvironmentVariables_Description).Width = 200
            .Add(My.Resources.ctlEnvironmentVariables_Value)
        End With
        lvEnvVars.LastColumnAutoSize = True
    End Sub

    Public Property Mode() As ProcessType Implements IMode.Mode
        Get
            Return mMode
        End Get
        Set(ByVal value As ProcessType)
            mMode = value
        End Set
    End Property
    Private mMode As ProcessType

    ''' <summary>
    ''' Checks if the current user can configure environment variables for the
    ''' control in its current mode
    ''' </summary>
    Private ReadOnly Property CanEditEnvVars() As Boolean
        Get
            Return User.Current.HasPermission(mMode.GetPermissionString("Configure Environment Variables"))
        End Get
    End Property

    ''' <summary>
    ''' Populates the environment vars table using data from the db.
    ''' </summary>
    Private Sub PopulateEnvironmentVars()
        Dim canEdit As Boolean = CanEditEnvVars
        lvEnvVars.Rows.Clear()
        For Each var In gSv.GetEnvironmentVariables()
            lvEnvVars.Rows.Add(
             New clsEnvironmentVariableListRow(lvEnvVars, var))
            mEnvironmentRowsExisting.Add(var)
            mEnvironmentRowsOriginalRecords.Add(New clsEnvironmentVariable(var.Name,
                                                                       var.Value,
                                                                       var.Description))
        Next
        lvEnvVars.Readonly = Not canEdit
        btnAddEnvironmentVariable.Enabled = canEdit
        btnEnvironmentVariableApply.Enabled = canEdit
        If canEdit AndAlso lvEnvVars.Rows.Count > 0 Then
            btnRemoveEnvVar.Enabled = True
            btnFindReferences.Enabled = True
        Else
            btnRemoveEnvVar.Enabled = False
            btnFindReferences.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' Handles changes to the currently selected environment variable.
    ''' </summary>
    Private Sub lvEnvVars_EditableRowChanged(sender As Object, e As ListRowChangedEventArgs) _
     Handles lvEnvVars.EditableRowChanged
        Dim lvRow As clsEnvironmentVariableListRow =
         TryCast(lvEnvVars.CurrentEditableRow, clsEnvironmentVariableListRow)
        'Ensure a variable is selected
        If lvRow Is Nothing Then Return
        btnFindReferences.Enabled =
            mEnvironmentRowsExisting.Contains(lvRow.EnvironmentVariable)
    End Sub

    ''' <summary>
    ''' Handles the adding of an environment variable
    ''' </summary>
    Private Sub btnAddEnvironmentVariable_LinkClicked(ByVal sender As System.Object,
     ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) _
     Handles btnAddEnvironmentVariable.LinkClicked

        lvEnvVars.Rows.Add(
         New clsEnvironmentVariableListRow(lvEnvVars, New clsEnvironmentVariable()))
        lvEnvVars.CurrentEditableRow = lvEnvVars.Rows.Last
        btnRemoveEnvVar.Enabled = True
    End Sub

    Private Sub btnRemoveEnvVar_LinkClicked(ByVal sender As System.Object,
     ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) _
     Handles btnRemoveEnvVar.LinkClicked
        Dim lvRow As clsEnvironmentVariableListRow =
         TryCast(lvEnvVars.CurrentEditableRow, clsEnvironmentVariableListRow)
        'Ensure a variable is selected
        If lvRow Is Nothing Then Return

        If mEnvironmentRowsExisting.Contains(lvRow.EnvironmentVariable) Then
            'Build dependency list to check references for
            Dim deps As New clsProcessDependencyList()
            deps.Add(New clsProcessEnvironmentVarDependency(lvRow.EnvironmentVariable.Name))

            'Return if user cancelled deletion
            If Not mParent.ConfirmDeletion(deps) Then Return
        End If

        'If ok to proceed then delete the variable
        mEnvironmentRowsDeleted.Add(lvRow.EnvironmentVariable)
        lvEnvVars.Rows.Remove(lvRow)
        If lvEnvVars.Rows.Count = 0 Then
            btnRemoveEnvVar.Enabled = False
            btnFindReferences.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' Handles the finding of references
    ''' </summary>
    Private Sub btnFindReferences_LinkClicked(ByVal sender As System.Object,
     ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) _
     Handles btnFindReferences.LinkClicked
        Dim lvRow As clsEnvironmentVariableListRow =
         TryCast(lvEnvVars.CurrentEditableRow, clsEnvironmentVariableListRow)
        If lvRow Is Nothing Then Return

        Dim varName As String = lvRow.EnvironmentVariable.Name
        mParent.FindReferences(New clsProcessEnvironmentVarDependency(varName))
    End Sub

    Private Sub btnEnvironmentVariableApply_Click(ByVal sender As Object,
     ByVal e As EventArgs) Handles btnEnvironmentVariableApply.Click
        lvEnvVars.FocusFirstGridItem()

        Dim duplicates As New clsSet(Of String)
        Dim inserted As New List(Of clsEnvironmentVariable)
        Dim updated As New List(Of clsEnvironmentVariable)

        For Each row As clsEnvironmentVariableListRow In lvEnvVars.Rows
            Dim variable As clsEnvironmentVariable = row.EnvironmentVariable
            If variable.Name = "" Then
                UserMessage.Show(My.Resources.ctlEnvironmentVariables_PleaseRemoveAnyBlankEnvironmentVariablesBeforeApplyingChanges)
                Return
            End If

            If variable.DataType = DataType.unknown Then
                UserMessage.Show(My.Resources.ctlEnvironmentVariables_PleaseProvideDataTypesForEachEnvironmentVariableBeforeApplyingChanges)
                Return
            End If

            If variable.DataType = DataType.image AndAlso variable.Value.EncodedValue Is Nothing Then
                UserMessage.Show(My.Resources.ctlEnvironmentVariables_PleaseImportAnImage)
                Return
            End If

            'Ensure maximum variable name length is not exceeded
            If variable.Name.Length > 64 Then
                UserMessage.Err(String.Format(
                 My.Resources.ctlEnvironmentVariables_VariableNameLengthCannotExceed64Characters0,
                 variable.Name))
                Return
            End If

            If Not duplicates.Contains(variable.Name.ToLower()) Then
                If mEnvironmentRowsExisting.Contains(variable) _
                 And Not mEnvironmentRowsDeleted.Contains(variable) Then
                    If Not mEnvironmentRowsOriginalRecords.Contains(variable) Then _
                        updated.Add(variable)
                Else
                    inserted.Add(variable)
                End If
                duplicates.Add(variable.Name.ToLower())
            Else
                UserMessage.Err(My.Resources.ctlEnvironmentVariables_YouHaveMoreThanOneEnvironmentVariableWithTheSameName0, variable.Name)
                Return
            End If
        Next

        Try
            gSv.UpdateEnvironmentVariables(inserted, updated, mEnvironmentRowsDeleted)
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
            Return
        End Try

        For Each variable As clsEnvironmentVariable In inserted
            variable.OldName = variable.Name
            mEnvironmentRowsExisting.Add(variable)
        Next
        For Each variable As clsEnvironmentVariable In updated
            variable.OldName = variable.Name
            mEnvironmentRowsExisting.First(Function(x) x.OldName = variable.Name).OldName = variable.Name
        Next
        mEnvironmentRowsDeleted.Clear()
        If lvEnvVars.Rows.Count > 0 Then btnFindReferences.Enabled = True
    End Sub

    Private Sub ctlEnvironmentVariables_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        PopulateEnvironmentVars()
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(
                mMode.GetPermissionString("Configure Environment Variables"),
                mMode.GetPermissionString("View Environment Variables"))
        End Get
    End Property

End Class
