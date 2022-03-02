Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Utility

Public Class frmDeleteDependencies
    Implements IEnvironmentColourManager

#Region "Properties"

    Public ReadOnly Property Deletions As clsProcessDependencyList
        Get
            Return mDeletions
        End Get
    End Property
    Private mDeletions As clsProcessDependencyList

#End Region

#Region "Constructor"

    Public Sub New(deps As clsProcessDependencyList)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ImageList1.Images.Add("OK", BluePrism.Images.ToolImages.Tick_16x16)
        ImageList1.Images.Add("INUSE", BluePrism.Images.ToolImages.Warning_16x16)

        Me.tBar.SubTitle = My.Resources.frmDeleteDependencies_AreYouSureThatYouWantToDeleteTheFollowingItems
        Me.tBar.Title = My.Resources.frmDeleteDependencies_ConfirmDeletion

        Dim referencedDeps As clsProcessDependencyList = gSv.FilterUnReferenced(deps)

        'Add referenced items first
        Dim i As Integer
        For Each d As clsProcessDependency In referencedDeps.Dependencies
            Dim name = d.GetValues().Values.First().ToString()
            If TypeOf d Is IIdBasedDependency Then name = CType(d, IIdBasedDependency).Name
            Dim comment As String

            If TypeOf (d) Is clsProcessCredentialsDependency Then
                If CType(d, clsProcessCredentialsDependency).GatewaysCredential AndAlso CType(d, clsProcessCredentialsDependency).SharedCredential Then
                    dgvItems.DefaultCellStyle.WrapMode = DataGridViewTriState.True
                    dgvItems.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
                    comment = My.Resources.ObjectsProcessesReferenceThisItem + vbCrLf + My.Resources.DataGatewaysReferenceThisCredential
                ElseIf CType(d, clsProcessCredentialsDependency).GatewaysCredential Then
                    comment = My.Resources.DataGatewaysReferenceThisCredential
                Else
                    comment = My.Resources.ObjectsProcessesReferenceThisItem
                End If
            Else
                comment = My.Resources.ObjectsProcessesReferenceThisItem
            End If

            i = dgvItems.Rows.Add(ImageList1.Images.Item("INUSE"),
                                   name,
                                   comment,
                                   False)
            dgvItems.Rows(i).Tag = d
        Next

        'Add unreferenced items last
        For Each d As clsProcessDependency In deps.Dependencies
            If Not referencedDeps.Has(d) Then
                Dim name = d.GetValues().Values.First().ToString()
                If TypeOf d Is IIdBasedDependency Then name = CType(d, IIdBasedDependency).Name
                i = dgvItems.Rows.Add(ImageList1.Images.Item("OK"),
                                  name,
                                  My.Resources.NoReferencesFound,
                                  True)
                dgvItems.Rows(i).Tag = d
            End If
        Next
    End Sub

#End Region

#Region "Event handlers"

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        DialogResult = System.Windows.Forms.DialogResult.Cancel
        Close()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        mDeletions = New clsProcessDependencyList()
        For Each row As DataGridViewRow In dgvItems.Rows
            If CBool(CType(row.Cells("cbDelete"), DataGridViewCheckBoxCell).Value) Then
                mDeletions.Add(CType(row.Tag, clsProcessDependency))
            End If
        Next
        DialogResult = System.Windows.Forms.DialogResult.OK
        Close()
    End Sub

#End Region

#Region "Help and Environment colour implementations"

    Public Overrides Function GetHelpFile() As String
        Return "frmReferenceChecker.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub


    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return tBar.BackColor
        End Get
        Set(value As Color)
            tBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return tBar.TitleColor
        End Get
        Set(value As Color)
            tBar.TitleColor = value
            tBar.SubtitleColor = value
        End Set
    End Property

#End Region

End Class