Imports AutomateUI
Imports BluePrism.AutomateProcessCore

''' <summary>
''' A stage, ie. a single part of a wizard, represented by a control which sits in
''' the wizard.
''' </summary>
Public Class InputFileStage : Inherits FileStage

    Public event FileSelected(sender As Object, e As EventArgs)
    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "input-file"

    Public Const BpFontFileExtension As String = ".bpfont"

    Public Shared Property MultiSelect As Boolean = false
    ''' <summary>
    ''' Creates a new input file stage
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new input file stage.
    ''' </summary>
    ''' <param name="dirPref">The pref name for the directory to open initially
    ''' </param>
    Public Sub New(ByVal dirPref As String)
        MyBase.New(My.Resources.InputFileStage_ChooseTheInputFileS, My.Resources.InputFileStage_SelectTheFilesYouWantToProcess, dirPref)
    End Sub

    Public Sub New(ByVal dirPref As String, multiSelect As Boolean)
        MyBase.New(if(multiSelect,My.Resources.InputFileStage_ChooseTheInputFileS,My.Resources.ctlInputFileChooser_ChooseTheInputFile), My.Resources.InputFileStage_SelectTheFilesYouWantToProcess, dirPref)
    End Sub
    ''' <summary>
    ''' The unique ID for this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.InputFileStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    Protected Overrides Sub OnCommitting(e As StageCommittingEventArgs)
        If Chooser.FileName.EndsWith(BpFontFileExtension, StringComparison.OrdinalIgnoreCase) Then
            UserMessage.Show(My.Resources.frmImportRelease_ToImportAFontGoToSystemFonts)
            e.Cancel = True
        End If
        If Not Chooser.FileName.Contains(",") AndAlso IO.File.Exists(Chooser.FileName) AndAlso
            (Chooser.FileName.EndsWith(clsProcess.ObjectFileExtension, StringComparison.OrdinalIgnoreCase) OrElse
              Chooser.FileName.EndsWith(clsProcess.ProcessFileExtension, StringComparison.OrdinalIgnoreCase)) AndAlso
             clsProcess.CheckValidExtensionForType(Chooser.FileName) <> clsProcess.IsValidForType.Valid Then
            UserMessage.Show(
                String.Format(My.Resources.TheSelectedFileIsNotAValidBluePrism0,
                              If(Chooser.FileName.EndsWith(clsProcess.ObjectFileExtension,
                                                               StringComparison.OrdinalIgnoreCase),
                                     My.Resources.BusinessObject.ToLowerInvariant(),
                                     My.Resources.Process.ToLowerInvariant())))
            e.Cancel = True
        End If
        MyBase.OnCommitting(e)
    End Sub

    ''' <summary>
    ''' Gets a newly initialised file chooser control for this stage.
    ''' </summary>
    ''' <returns>An output file chooser, initialised with this stage's current values
    ''' </returns>
    Protected Overrides Function GetFileChooser() As ctlFileChooser
        Dim inputFileChooser = New ctlInputFileChooser
        inputFileChooser.MultiSelect = MultiSelect
        Return inputFileChooser
    End Function

End Class
