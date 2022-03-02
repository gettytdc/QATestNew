Imports BluePrism.AutomateAppCore
Imports System.Text.RegularExpressions

Public Class ctlPathFinder

    ''' <summary>
    ''' The regular expression determining which characters to replace with
    ''' underscores when sanitising a filename.
    ''' </summary>
    Private Shared ReadOnly FilenameSanitiserRegex As New Regex("[\\/*?]")

    Public Event PathTextChanged As EventHandler

    ''' <summary>
    ''' The text displayed above the textbox, hinting
    ''' to the user what information is required.
    ''' </summary>
    ''' <value></value>
    Public Property HintText() As String
        Get
            Return Me.lblHintText.Text
        End Get
        Set(ByVal value As String)
            Me.lblHintText.Text = value
            Me.UpdateHintLabelSize()
        End Set
    End Property

    ''' <summary>
    ''' Updates the size of the hint label according to
    ''' its text
    ''' </summary>
    Private Sub UpdateHintLabelSize()
        'Me.BackColor = Color.Red
        'Me.lblHintText.BackColor = Color.Blue
        Me.lblHintText.AutoSize = True
        Me.lblHintText.MaximumSize = New Size(Me.txtFile.Width, Integer.MaxValue)
        Me.txtFile.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        Me.txtFile.Top = lblHintText.Bottom
        Me.btnBrowse.Top = Me.txtFile.Top
        Me.Height = txtFile.Bottom + 2
    End Sub


    ''' <summary>
    ''' Private member to store public property InitialDirectory()
    ''' </summary>
    Private msInitialDirectory As String

    ''' <summary>
    ''' The directory in which the browse dialog should begin.
    ''' </summary>
    ''' <value></value>
    Public Property InitialDirectory() As String
        Get
            Return msInitialDirectory
        End Get
        Set(ByVal value As String)
            msInitialDirectory = value
        End Set
    End Property


    ''' <summary>
''' Private member to store public property SuggestedFilename()
''' </summary>
    Private mSuggestedFilename As String
    ''' <summary>
    ''' The text to be displayed by default in the file browser dialog.
    ''' This can act as a suggested filename when saving or creating
    ''' a new file.
    ''' </summary>
    Public Property SuggestedFilename() As String
        Get
            Return mSuggestedFilename
        End Get
        Set(ByVal value As String)
            mSuggestedFilename = value
        End Set
    End Property


    ''' <summary>
    ''' Private member to store public property Filter()
    ''' </summary>
    Private msFilter As String

    ''' <summary>
    ''' The filter to apply in the browse dialog.
    ''' </summary>
    ''' <value></value>
    Public Property Filter() As String
        Get
            Return msFilter
        End Get
        Set(ByVal value As String)
            msFilter = value
        End Set
    End Property




    ''' <summary>
    ''' Private member to store public property BrowseWindowTitle()
    ''' </summary>
    Private msBrowseWindowTitle As String = My.Resources.ctlPathFinder_LocateFile

    ''' <summary>
    ''' The title to display in the popup window when the 
    ''' user clicks the browse button.
    ''' </summary>
    ''' <value>Defaults to "Locate file ..."</value>
    Public Property BrowseWindowTitle() As String
        Get
            Return msBrowseWindowTitle
        End Get
        Set(ByVal value As String)
            msBrowseWindowTitle = value
        End Set
    End Property

    ''' <summary>
    ''' The modes available for this path finder.
    ''' </summary>
    Public Enum PathModes
        ''' <summary>
        ''' An existing file is to be located.
        ''' </summary>
        Open
        ''' <summary>
        ''' A path is to be specified for the purposes of saving;
        ''' this path may or may not exist (yet).
        ''' </summary>
        Save
    End Enum

    ''' <summary>
    ''' The mode in which this path finder operates.
    ''' </summary>
    Public Property Mode() As PathModes
        Get
            Return mMode
        End Get
        Set(ByVal value As PathModes)
            mMode = value
        End Set
    End Property
    Private mMode As PathModes

    ''' <summary>
    ''' Strips any characters which are not allowed in a file / path name in
    ''' Windows, replacing them with underscores.
    ''' </summary>
    ''' <param name="fn">The filename to sanitise.</param>
    ''' <returns>The filename with any illegal file characters replaced by
    ''' underscores.</returns>
    Private Function SanitiseFilename(ByVal fn As String) As String
        If fn Is Nothing Then Return Nothing
        Return FilenameSanitiserRegex.Replace(fn, "_")
    End Function

    Public Property CheckFileExists As Boolean = True

    Private Sub btnBrowse_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnBrowse.Click
        'Create the dialog
        Dim fd As FileDialog = Nothing
        Select Case Me.Mode
            Case PathModes.Open
                Dim ofd As New OpenFileDialog()
                ofd.Multiselect = False
                ofd.CheckFileExists = CheckFileExists
                fd = ofd
            Case PathModes.Save
                Dim sfd As New SaveFileDialog()
                sfd.CheckFileExists = False
                sfd.OverwritePrompt = False
                fd = sfd
        End Select

        'Set the initial directory, which is either taken explicitly
        'from the public property, or inferred from the text in the textbox
        If msInitialDirectory <> "" Then
            fd.InitialDirectory = msInitialDirectory
        Else
            Dim TestPath As String = txtFile.Text
            If Not System.IO.Directory.Exists(TestPath) Then
                Dim LastSlash As Integer = txtFile.Text.LastIndexOf("\")
                If LastSlash > -1 Then
                    TestPath = txtFile.Text.Substring(0, LastSlash)
                End If
            End If
            If System.IO.Directory.Exists(TestPath) Then
                fd.InitialDirectory = txtFile.Text
            Else
                fd.InitialDirectory = clsFileSystem.GetProcessesPath()
            End If
        End If

        'Set file filter
        If Not String.IsNullOrEmpty(Me.msFilter) Then
            fd.Filter = msFilter
        Else
            fd.Filter = My.Resources.ctlPathFinder_AllFiles
        End If

        fd.AddExtension = True
        fd.DereferenceLinks = True
        fd.ShowHelp = False
        fd.Title = Me.BrowseWindowTitle
        fd.FileName = SanitiseFilename(SuggestedFilename)

        If fd.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            txtFile.Text = fd.FileName()
        End If
        fd.Dispose()
    End Sub

    ''' <summary>
    ''' The file chosen by the user
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ChosenFile() As String
        Get
            Return Me.txtFile.Text
        End Get
    End Property

    Private Sub txtFile_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtFile.SizeChanged, MyBase.SizeChanged, lblHintText.SizeChanged, lblHintText.LocationChanged
        Me.UpdateHintLabelSize()
    End Sub

    Private Sub txtFile_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFile.TextChanged
        RaiseEvent PathTextChanged(sender, e)
    End Sub
End Class
