Imports BluePrism.AutomateAppCore.Groups

Public Class ctlProcessChooser : Inherits ctlWizardStageControl

    Public Sub New(procType As ProcessType)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If procType.IsBusinessObject Then
            tvGroups.TreeType = GroupTreeType.Objects
        Else
            tvGroups.TreeType = GroupTreeType.Processes
        End If

        tvGroups.Filter = ProcessBackedGroupMember.NotRetired
    End Sub

    Public ReadOnly Property SelectedMember As ProcessBackedGroupMember
        Get
            Dim mems = tvGroups.SelectedMembers
            If mems.Count > 0 Then
                Return TryCast(mems(0), ProcessBackedGroupMember)
            End If
            Return Nothing
        End Get
    End Property

End Class
