
Imports System.IO

''' <summary>
''' Stage class for choosing an output file
''' </summary>
Public Class OutputFileStage : Inherits FileStage

    ''' <summary>
    ''' The ID of this stage.
    ''' </summary>
    Public Const StageId As String = "output-file"

    ' The suggested name for the output file
    Private mSuggestedName As String

    ''' <summary>
    ''' Creates a new output file stage with no specified name or directory
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new output file stage with no specified name, using the given pref
    ''' for the initial directory
    ''' </summary>
    ''' <param name="outputDirPref">The name of the preference which details the
    ''' directory to use.</param>
    Public Sub New(ByVal outputDirPref As String)
        Me.New(outputDirPref, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new output file stage with the given suggested name and the
    ''' specified pref name which holds the preferred initial directory.
    ''' </summary>
    ''' <param name="outputDirPref">The pref name for the preferred output directory
    ''' </param>
    ''' <param name="suggestedName">The suggested name to use in this stage.</param>
    Public Sub New(ByVal outputDirPref As String, ByVal suggestedName As String)
        MyBase.New(
         My.Resources.OutputFileStage_ChooseTheOutputFile, My.Resources.OutputFileStage_ChooseWhereTheReleaseShouldBeExportedTo, outputDirPref)
        Me.SuggestedName = suggestedName
    End Sub

    ''' <summary>
    ''' The ID of this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' Gets the control in this stage as an output file chooser.
    ''' </summary>
    Private Shadows ReadOnly Property Chooser() As ctlOutputFileChooser
        Get
            Return DirectCast(mControl, ctlOutputFileChooser)
        End Get
    End Property

    ''' <summary>
    ''' Gets a newly initialised file chooser control for this stage.
    ''' </summary>
    ''' <returns>An output file chooser, initialised with this stage's current values
    ''' </returns>
    Protected Overrides Function GetFileChooser() As ctlFileChooser
        Dim ofc As New ctlOutputFileChooser()
        ' We handle the warning within this stage - no need for the control to do it
        ofc.WarnOnOverwrite = False
        ofc.SuggestedFileName = mSuggestedName
        Return ofc
    End Function

    ''' <summary>
    ''' The suggested name for the file represented by this stage.
    ''' </summary>
    Public Property SuggestedName() As String
        Get
            Return mSuggestedName
        End Get
        Set(ByVal value As String)
            If value = "" Then mSuggestedName = Nothing Else mSuggestedName = value.Trim()
            If Chooser IsNot Nothing Then Chooser.SuggestedFileName = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the committing of this stage, checking that a file has been chosen
    ''' and that it is okay to overwrite it if it already exists.
    ''' </summary>
    Protected Overrides Sub OnCommitting(ByVal e As StageCommittingEventArgs)
        If Chooser.FileName <> "" Then ' Empty filename is handled by FileStage class...
            Dim f As FileInfo = New FileInfo(Chooser.FileName)
            If f.Exists Then
                Dim res As DialogResult = MessageBox.Show(String.Format(
                 My.Resources.OutputFileStage_TheFile0AlreadyExistsDoYouWantToOverwriteThisFile, f.FullName),
                 My.Resources.OutputFileStage_FileAlreadyExists, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
                If res = DialogResult.Cancel Then e.Cancel = True : Return
            End If
            Dim dir As DirectoryInfo = f.Directory
            If Not dir.Exists Then
                Dim res As DialogResult = MessageBox.Show(String.Format(
                 My.Resources.OutputFileStage_TheDirectory0DoesNotExistDoYouWantToCreateIt, dir.FullName),
                 My.Resources.OutputFileStage_PathDoesnTExist, MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                If res = DialogResult.Cancel Then e.Cancel = True : Return
                dir.Create()
            End If
        End If
        MyBase.OnCommitting(e)
    End Sub

End Class
