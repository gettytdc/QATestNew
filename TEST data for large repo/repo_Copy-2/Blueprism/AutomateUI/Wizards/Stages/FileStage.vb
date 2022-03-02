Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' A stage, ie. a single part of a wizard, represented by a control which sits in
''' the wizard.
''' </summary>
Public MustInherit Class FileStage : Inherits WizardStage

    ' map of extensions in form {ext:label}
    Private mExtensions As IDictionary(Of String, String)

    ' The pref name for the initial directory to use
    Private mDirPref As String

    ' The filename chosen for the output file
    Private mFilename As String

    ''' <summary>
    ''' Creates a new description stage.
    ''' </summary>
    Public Sub New(ByVal title As String, ByVal subtitle As String)
        Me.New(title, subtitle, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new output file stage with no specified name, using the given pref
    ''' for the initial directory
    ''' </summary>
    ''' <param name="dirPref">The name of the preference which details the
    ''' directory to use.</param>
    Public Sub New(ByVal title As String, ByVal subtitle As String, ByVal dirPref As String)
        MyBase.New(title, subtitle)
        If dirPref = "" Then mDirPref = Nothing Else mDirPref = dirPref
        mExtensions = New clsOrderedDictionary(Of String, String)
    End Sub

    ''' <summary>
    ''' The unique ID for this stage.
    ''' </summary>
    Public MustOverride Overrides ReadOnly Property Id() As String

    ''' <summary>
    ''' Gets the file chooser which can accept the file name in this stage.
    ''' </summary>
    ''' <returns>A newly initialised file chooser control for accepting a file name
    ''' on behalf of this stage.</returns>
    Protected MustOverride Function GetFileChooser() As ctlFileChooser

    ''' <summary>
    ''' Adds an exteions to this output file stage, to be accepted in the file
    ''' chooser.
    ''' </summary>
    ''' <param name="extensions">The extensions to check for, separated by
    ''' semi-colons, eg. <c>AddExtensionEntry("bprelease;xml", "Blue Prism
    ''' Releases")</c> would add an entry looking for all *.bprelease and *.xml files
    ''' with the label "Blue Prism Releases".</param>
    ''' <param name="label">The label for the extension</param>
    Public Sub AddExtensionEntry(ByVal extensions As String, ByVal label As String)
        mExtensions(extensions) = label
    End Sub

    ''' <summary>
    ''' Gets the control in this stage as a file chooser.
    ''' </summary>
    Protected ReadOnly Property Chooser() As ctlFileChooser
        Get
            Return DirectCast(mControl, ctlFileChooser)
        End Get
    End Property

    ''' <summary>
    ''' The currently set file name.
    ''' </summary>
    Public Property FileName() As String
        Get
            Return mFilename
        End Get
        Set(ByVal value As String)
            mFilename = value
            If Chooser IsNot Nothing Then Chooser.FileName = value
        End Set
    End Property

    ''' <summary>
    ''' Creates the control necessary to accept the file name for this stage.
    ''' </summary>
    ''' <returns>The control which will display / accept the output filename.
    ''' </returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim fc As ctlFileChooser = GetFileChooser()
        fc.DirectoryPref = mDirPref
        fc.Extensions = mExtensions
        fc.FileName = mFilename
        Return fc
    End Function

    ''' <summary>
    ''' Handles the committing of this stage, checking that a file has been chosen
    ''' and that it is okay to overwrite it if it already exists.
    ''' </summary>
    Protected Overrides Sub OnCommitting(ByVal e As StageCommittingEventArgs)
        If Chooser.FileName = "" Then
            UserMessage.Show(My.Resources.FileStage_YouMustChooseAFile)
            e.Cancel = True
        End If
    End Sub

    ''' <summary>
    ''' Handles this stage being committed by saving the filename to the member
    ''' variable so that the control can be disposed of.
    ''' </summary>
    Protected Overrides Sub OnCommitted(ByVal e As StageCommittedEventArgs)
        mFilename = Chooser.FileName
        MyBase.OnCommitted(e)
    End Sub

End Class
