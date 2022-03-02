Imports System.IO
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Control to choose an output file.
''' </summary>
Public Class ctlOutputFileChooser : Inherits ctlFileChooser

    ' The suggested filename for the output file
    Private mSuggestedFileName As String

    ' Flag to show a warning if the output file already exists
    Private mWarnOnOverwrite As Boolean

    ''' <summary>
    ''' Creates a new output file chooser control
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new output file chooser control using the given extension and
    ''' extension label
    ''' </summary>
    ''' <param name="ext">The extension to use for the output file.</param>
    ''' <param name="extLabel">The label used to describe the type of file.</param>
    Public Sub New(ByVal ext As String, ByVal extLabel As String)
        Me.New(GetSingleton.IDictionary(ext, extLabel))
    End Sub

    ''' <summary>
    ''' Creates a new output file chooser control using the given map of extension
    ''' labels to their extensions.
    ''' </summary>
    ''' <param name="extensions">The extensions as keys mapped with their labels
    ''' as values.</param>
    Public Sub New(ByVal extensions As IDictionary(Of String, String))
        MyBase.New(extensions)
        Me.Prompt = My.Resources.ctlOutputFileChooser_ChooseTheOutputFile
    End Sub

    ''' <summary>
    ''' The suggested filename for this file chooser.
    ''' This sanitizes the name on the way in to ensure that no invalid file
    ''' characters are used. All illegal characters are replaced with an underscore.
    ''' </summary>
    Public Property SuggestedFileName() As String
        Get
            Return mSuggestedFileName
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then ' Let's just get this out of the way
                mSuggestedFileName = value
                Return
            End If
            If value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
                For Each c As Char In Path.GetInvalidFileNameChars()
                    value = value.Replace(c, "_"c)
                Next
            End If
            Dim ext As String = PrimaryExtension
            If ext <> "" AndAlso Not value.EndsWith("." & ext, StringComparison.CurrentCultureIgnoreCase) Then
                value &= "." & ext
            End If

            mSuggestedFileName = value
            FileName = Path.Combine(PreferredDirectory, value)
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating whether a warning should be displayed if the chosen output
    ''' file already exists. By default, no warning is shown
    ''' </summary>
    Public Property WarnOnOverwrite() As Boolean
        Get
            Return mWarnOnOverwrite
        End Get
        Set(ByVal value As Boolean)
            mWarnOnOverwrite = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the file dialog used to choose the output file
    ''' </summary>
    ''' <returns>A newly initialised file dialog for capturing the file to write to
    ''' </returns>
    Protected Overrides Function GetFileDialog() As FileDialog
        Dim dia As New SaveFileDialog()
        dia.AddExtension = True
        dia.DefaultExt = PrimaryExtension
        dia.OverwritePrompt = mWarnOnOverwrite

        If mSuggestedFileName <> "" Then dia.FileName = mSuggestedFileName
        Return dia
    End Function

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlOutputFileChooser))
        Me.SuspendLayout()
        '
        'ctlOutputFileChooser
        '
        resources.ApplyResources(Me, My.Resources.ctlOutputFileChooser_This)
        Me.Name = "ctlOutputFileChooser"
        Me.ResumeLayout(False)

    End Sub
End Class
