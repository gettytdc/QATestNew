
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Control to choose an input file.
''' </summary>
Public Class ctlInputFileChooser : Inherits ctlFileChooser

    Private mMultiSelect As Boolean = false
    Public Property MultiSelect As Boolean
    Get
        Return mMultiSelect 
    End Get
        Set(value As Boolean)
            Me.Prompt = if(value, My.Resources.InputFileStage_ChooseTheInputFileS,My.Resources.ctlInputFileChooser_ChooseTheInputFile)
            mMultiSelect = value
        End Set
    End Property
    ''' <summary>
    ''' Creates a new input file chooser control
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new input file chooser control using the given extension and
    ''' extension label
    ''' </summary>
    ''' <param name="ext">The extension to use for the input file.</param>
    ''' <param name="extLabel">The label used to describe the type of file.</param>
    Public Sub New(ByVal ext As String, ByVal extLabel As String)
        Me.New(GetSingleton.IDictionary(ext, extLabel))
    End Sub

    ''' <summary>
    ''' Creates a new input file chooser control using the given map of extension
    ''' labels to their extensions.
    ''' </summary>
    ''' <param name="extensions">The extensions as keys mapped with their labels
    ''' as values.</param>
    Public Sub New(ByVal extensions As IDictionary(Of String, String))
        MyBase.New(extensions)
        Me.Prompt = My.Resources.ctlInputFileChooser_ChooseTheInputFile
    End Sub

    ''' <summary>
    ''' Gets the file dialog used to choose the input file
    ''' </summary>
    ''' <returns>A newly initialised file dialog for capturing the file to read from
    ''' </returns>
    Protected Overrides Function GetFileDialog() As FileDialog
        Dim dia As New OpenFileDialog()
        dia.Multiselect = MultiSelect
        dia.AddExtension = True
        dia.DefaultExt = PrimaryExtension
        Return dia
    End Function

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlInputFileChooser))
        Me.SuspendLayout()
        '
        'ctlInputFileChooser
        '
        resources.ApplyResources(Me, My.Resources.ctlInputFileChooser_This)
        Me.Name = "ctlInputFileChooser"
        Me.ResumeLayout(False)

End Sub
End Class
