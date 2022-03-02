Imports BluePrism.AutomateAppCore.Groups

Friend Class frmProcessCompare
    Inherits frmStagedWizard

    Private mLocation As FileOrDatabaseStage.ProcessLocationType
    Private mSelectedMember As ProcessBackedGroupMember
    Private mFileName As String

    Private WithEvents mLocationChooser As FileOrDatabaseStage
    Private WithEvents mFileChooser As InputFileStage
    Private WithEvents mProcessChooser As SelectProcessStage
    Private WithEvents mMessage As MessageStage

    Public Sub New(proctype As ProcessType)
        MyBase.New(proctype.FormatString(My.Resources.frmProcessCompare_SelectA0ToCompare), CreateStages(proctype))

        mLocationChooser = DirectCast(GetStage(FileOrDatabaseStage.StageId), FileOrDatabaseStage)
        mFileChooser = DirectCast(GetStage(InputFileStage.StageId), InputFileStage)
        mProcessChooser = DirectCast(GetStage(SelectProcessStage.StageId), SelectProcessStage)
        mMessage = DirectCast(GetStage(MessageStage.StageId), MessageStage)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    Private Shared Function CreateStages(proctype As ProcessType) As IList(Of WizardStage)
        Dim stages As New List(Of WizardStage)
        ' Include the 'choose package' stage if one is not chosen
        With stages
            .Add(New FileOrDatabaseStage(proctype.FormatString(My.Resources.frmProcessCompare_CompareThe0ToAAnother0InAFileOrTheDatabase)))
            .Add(New InputFileStage(proctype.FormatString(My.Resources.frmProcessCompare_ChooseA0FromAnExternalFile)))
            .Add(New SelectProcessStage(proctype.FormatString(My.Resources.frmProcessCompare_ChooseA0FromTheDatabase), proctype))
            .Add(New MessageStage(My.Resources.frmProcessCompare_ProcessesCompared)) 'This stage should never appear.
        End With
        Return stages
    End Function

    Protected Overrides Sub OnSteppingNext(ByVal e As WizardSteppingEventArgs)
        If e.Stage Is mFileChooser _
            AndAlso mLocationChooser.ProcessLocation = FileOrDatabaseStage.ProcessLocationType.Database Then
            e.Skip = True
        End If
        If e.Stage Is mProcessChooser _
            AndAlso mLocationChooser.ProcessLocation = FileOrDatabaseStage.ProcessLocationType.File Then
            e.Skip = True
        End If
        If e.Stage Is mMessage Then
            Me.Completed = True
            Me.Close()
        End If
    End Sub

    Protected Overrides Sub OnInitStage(stg As WizardStage)
        MyBase.OnInitStage(stg)
        Select Case True
            Case stg Is mFileChooser
                btnNext.Text = My.Resources.frmProcessCompare_Finish
            Case stg Is mProcessChooser
                btnBack.Enabled = True
        End Select
    End Sub

    Private Sub HandleLocationChosen(sender As WizardStage, e As StageCommittedEventArgs) _
        Handles mLocationChooser.Committed
        mLocation = mLocationChooser.ProcessLocation
    End Sub

    Private Sub HandleProcessChosen(sender As WizardStage, e As StageCommittedEventArgs) _
        Handles mProcessChooser.Committed
        mSelectedMember = mProcessChooser.SelectedMember
    End Sub

    Private Sub HandleFileChosen(sender As WizardStage, e As StageCommittedEventArgs) _
        Handles mFileChooser.Committed
        mFileName = mFileChooser.FileName
    End Sub

    Public ReadOnly Property ProcessLocation As FileOrDatabaseStage.ProcessLocationType
        Get
            Return mLocation
        End Get
    End Property

    Public ReadOnly Property SelectedMember As ProcessBackedGroupMember
        Get
            Return mSelectedMember
        End Get
    End Property

    Public ReadOnly Property FileName As String
        Get
            Return mFileName
        End Get
    End Property

    Public Overrides Function GetHelpFile() As String
        Return "frmProcessCompare.html"
    End Function

End Class