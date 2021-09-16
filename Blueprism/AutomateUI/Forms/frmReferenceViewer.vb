Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmReferenceViewer
    Implements IEnvironmentColourManager

    Public Event OpenProcess(procid As Guid, dep As clsProcessDependency)

#Region "Properties"

    Public Property Dependency As clsProcessDependency
        Get
            Return mDependency
        End Get
        Set(value As clsProcessDependency)
            mDependency = value
            If mDependency IsNot Nothing Then
                Findreferences()
            End If
        End Set
    End Property
    Private mDependency As clsProcessDependency
    Private mAlternateDependency As clsProcessDependency

    'The current process (if applicable)
    Public Property Process As clsProcess

    'Indicates whether any references for the passed item were found
    Public ReadOnly Property ReferencesFound As Boolean
        Get
            Return (dgvReferences.Rows.Count > 0)
        End Get
    End Property

#End Region

#Region "Constructor"

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        imgList.Images.Add("O", BluePrism.Images.ComponentImages.Class_16x16)
        imgList.Images.Add("P", BluePrism.Images.ToolImages.Procedure_16x16)

    End Sub

#End Region

#Region "Methods"

    Private Sub Findreferences()
        dgvReferences.Rows.Clear()
        tBar.Title = String.Format(My.Resources.ReferencesTo0, mDependency.GetLocalizedFriendlyName())
        If Process Is Nothing Then
            If TypeOf (mDependency) Is IIdBasedDependency Then
                tBar.SubTitle = CType(mDependency, IIdBasedDependency).Name
            Else
                tBar.SubTitle = mDependency.GetValues().Values.First().ToString()
            End If
        ElseIf TypeOf (mDependency) Is clsProcessActionDependency Then
            Dim actionName = CType(mDependency, clsProcessActionDependency).RefActionName
            tBar.SubTitle = actionName
            ' This is a special case for when published actions are referenced by their
            ' owning Objects via a Page stage
            mAlternateDependency = New clsProcessPageDependency(Process.GetSubSheetID(actionName))
        ElseIf TypeOf (mDependency) Is clsProcessElementDependency Then
            Dim el As clsApplicationElement = Process.ApplicationDefinition.FindElement(CType(mDependency, clsProcessElementDependency).RefElementID)
            tBar.SubTitle = el?.Name
        Else
            tBar.SubTitle = Process.Name
        End If

        Dim hiddenItems As Boolean
        For Each p In gSv.GetReferences(mDependency, hiddenItems)
            AddRow(p)
        Next

        If hiddenItems Then
            lblWarning.Image = BluePrism.Images.ToolImages.Warning_16x16
            lblWarning.Text = String.Format(
                    My.Resources.This0IsReferencedByObjectsProcessesThatYouDonTHaveAccessTo,
                    mDependency.GetLocalizedFriendlyName())
        Else
            ssWarning.Visible = False
            dgvReferences.Dock = DockStyle.Fill
        End If

        'Add the current object/process if also exists as an internal dependency
        If Process IsNot Nothing Then
            Dim dependencies = Process.GetDependencies(True)
            If dependencies.Has(mDependency) OrElse (mAlternateDependency IsNot Nothing AndAlso dependencies.Has(mAlternateDependency)) Then
                Dim procid As Guid = Process.Id
                If procid = Guid.Empty Then procid = gSv.GetProcessIDByName(Process.Name, True)

                AddRow(New ProcessInfo() With {
                   .Id = procid,
                   .Type = Process.ProcessType,
                   .Name = Process.Name,
                   .Description = Process.Description,
                   .CanViewDefinition = True})
            End If
        End If
        dgvReferences.Columns(3).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        dgvReferences.Columns(3).FillWeight = 25.0!
    End Sub

    Private Sub AddRow(process As ProcessInfo)
        Dim i As Integer = dgvReferences.Rows.Add(
            imgList.Images(If(process.Type = DiagramType.Object, "O", "P")),
            process.Name,
            process.Description)

        If process.CanViewDefinition Then
            dgvReferences.Rows(i).Cells(3).Value = My.Resources.View
            If process.Type = DiagramType.Object Then
                dgvReferences.Rows(i).Cells(0).ToolTipText = My.Resources.BusinessObject
                dgvReferences.Rows(i).Cells(3).ToolTipText = My.Resources.ViewBusinessObject
            Else
                dgvReferences.Rows(i).Cells(0).ToolTipText = My.Resources.Process
                dgvReferences.Rows(i).Cells(3).ToolTipText = My.Resources.ViewProcess
            End If
        End If

        'Add process ID as tag for this row
        dgvReferences.Rows(i).Tag = process
    End Sub

#End Region

#Region "Event handlers"

    Private Sub dgvReferences_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) _
     Handles dgvReferences.CellContentClick
        If e.ColumnIndex <> 3 Then Return
        'If link clicked then raise Open Process request
        Dim process = CType(dgvReferences.Rows(e.RowIndex).Tag, ProcessInfo)
        If process.CanViewDefinition Then RaiseEvent OpenProcess(process.Id, mDependency)
    End Sub

#End Region

#Region "Help and Environment colour implementations"

    Public Overrides Function GetHelpFile() As String
        Return "frmReferenceViewer.htm"
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
