Imports System.IO
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Control to choose an output file.
''' </summary>
Public Class ctlFileChooser : Inherits ctlWizardStageControl

    Public event FileSelected(sender As Object, e As EventArgs)

    ' The extension used by this chooser. In the form: {extension}:{label}
    Private mExtensions As IDictionary(Of String, String)

    ' The pref name detailing which directory to open initially
    Private mDirectoryPref As String

    ' The directory to open initially (lazily loaded)
    Private mDirectory As String

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

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mExtensions = extensions

    End Sub

    ''' <summary>
    ''' The extensions being handled by this file chooser. The keys should represent
    ''' the extensions to look for, separated by semi-colons (eg <c>"xml;txt;log"</c>
    ''' and each corresponding label should represent the label for that set of
    ''' extensions (eg. <c>Text File</c>). A set of extensions with no label (ie. a
    ''' map entry value of <c>null</c>) will display as just a list of extensions.
    ''' </summary>
    Public Property Extensions() As IDictionary(Of String, String)
        Get
            Return mExtensions
        End Get
        Set(ByVal value As IDictionary(Of String, String))
            mExtensions = value
        End Set
    End Property

    ''' <summary>
    ''' The file name currently selected in this control
    ''' </summary>
    Public Property FileName() As String
        Get
            Return txtFilePath.Text
        End Get
        Set(ByVal value As String)
            txtFilePath.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The prompt for the file location
    ''' </summary>
    Public Property Prompt() As String
        Get
            Return lblPrompt.Text
        End Get
        Set(ByVal value As String)
            lblPrompt.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The primary extension(s) for this output file chooser. This is the first
    ''' extension set in the map of extensions, if one is set. Otherwise null.
    ''' </summary>
    Public ReadOnly Property PrimaryExtension() As String
        Get
            If mExtensions Is Nothing Then Return Nothing
            Return CollectionUtil.First(Of String)(mExtensions.Keys)
        End Get
    End Property

    ''' <summary>
    ''' The preferred directory to open initially in this control
    ''' </summary>
    Public Property PreferredDirectory() As String
        Get
            ' Try getting a directory from the text box first - that overrides
            ' anything else.
            Dim enteredDir As String = ExtractBestFitDir(txtFilePath.Text)
            If enteredDir IsNot Nothing Then Return enteredDir

            ' If it's not there (or cannot be made to represent a directory)...
            If mDirectory Is Nothing Then

                ' Check the directory pref for the last used directory.
                If mDirectoryPref <> "" Then _
                 mDirectory = ExtractBestFitDir(gSv.GetPref(mDirectoryPref, ""))

                ' If that doesn't exist either, default to the Desktop.
                If mDirectory = "" Then _
                 mDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)

            End If
            Return mDirectory
        End Get
        Set(ByVal value As String)
            If mDirectoryPref <> "" Then gSv.SetUserPref(mDirectoryPref, value)
            mDirectory = value
        End Set
    End Property

    ''' <summary>
    ''' The name of the pref which indicates where the preferred directory for this
    ''' file chooser exists.
    ''' </summary>
    Public Property DirectoryPref() As String
        Get
            Return mDirectoryPref
        End Get
        Set(ByVal value As String)
            mDirectoryPref = value
        End Set
    End Property

    ''' <summary>
    ''' The filter string for this file chooser.
    ''' </summary>
    Protected ReadOnly Property FileFilter() As String
        Get
            Return BuildFilter(mExtensions)
        End Get
    End Property

    ''' <summary>
    ''' Builds the filter string from the given map.
    ''' </summary>
    ''' <param name="map">The maps of labels to their extensions. The keys should
    ''' represent the extensions to look for (eg. <c>"xml;txt;log"</c> and each
    ''' corresponding label should represent the label for that set of extensions
    ''' (eg. <c>Text File</c>). A set of extensions with no label (ie. a map entry
    ''' value of <c>null</c>) will display as just a list of extensions.</param>
    ''' <returns>A filter string representing the given map</returns>
    Protected Function BuildFilter(ByVal map As IDictionary(Of String, String)) As String
        If map Is Nothing OrElse map.Count = 0 Then Return Nothing

        Dim sb As New StringBuilder()
        Dim allIncluded As Boolean = False
        For Each extList As String In map.Keys
            If sb.Length > 0 Then sb.Append("|"c)
            Dim lbl As String = Nothing
            map.TryGetValue(extList, lbl)
            ' Build up the extension string
            Dim extSb As New StringBuilder()
            For Each ext As String In extList.Split(";"c)
                If extSb.Length > 0 Then extSb.Append(";"c)
                extSb.AppendFormat("*.{0}", ext)
            Next
            If extSb.Length = 0 Then Continue For ' ... though quite why it would be, I don't know
            If lbl Is Nothing Then
                sb.AppendFormat("{0}|{0}", extSb)
            Else
                sb.AppendFormat("{1} ({0})|{0}", extSb, lbl)
            End If
            If extList = "*" Then allIncluded = True
        Next
        If sb.Length = 0 Then Return Nothing

        ' Ensure that the user can select all files if they want to
        If allIncluded Then sb.AppendFormat(My.Resources.ctlFileChooser_AllFiles)

        Return sb.ToString()

    End Function

    ''' <summary>
    ''' Gets the file dialog to use when the browse button is pressed on this file
    ''' chooser.
    ''' </summary>
    ''' <returns>The file dialog for this chooser.</returns>
    ''' <exception cref="NotImplementedException">If the subclass of this file
    ''' chooser does not implement this function. This would be an abstract class if
    ''' the visual designer could handle such things</exception>
    Protected Overridable Function GetFileDialog() As FileDialog
        Throw New NotImplementedException( _
         "Subclasses of ctlFileChooser must implement GetFileDialog()")
    End Function

    ''' <summary>
    ''' Handles the browse button being clicked
    ''' </summary>
    Private Sub HandleBrowseClicked(ByVal sender As Object, ByVal e As EventArgs) Handles btnBrowse.Click
        OnBrowse(e)
    End Sub

    ''' <summary>
    ''' Handles a Browse operation on this file chooser.
    ''' </summary>
    Protected Overridable Sub OnBrowse(ByVal e As EventArgs)
        Using dia As FileDialog = GetFileDialog()
            dia.Filter = Me.FileFilter
            dia.Title = Me.Prompt
            dia.ValidateNames = True
            dia.ShowHelp = False
            dia.InitialDirectory = Me.PreferredDirectory
            If dia.ShowDialog() = DialogResult.OK Then
                If dia.FileNames.Count > 1 Then
                    txtFilePath.Text = String.Join(",", dia.FileNames)
                Else
                    txtFilePath.Text = dia.FileName
                    Me.PreferredDirectory = Directory.GetParent(dia.FileName).FullName
                End If
                RaiseEvent FileSelected(me, e)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Handles the text box being actioned - this checks for a carriage return char
    ''' and fires an <see cref="Activated"/> event if it discovers that the enter
    ''' key was pressed.
    ''' </summary>
    Private Sub HandleTextActioned(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtFilePath.Activated
        OnActivated(New ActivationEventArgs())
    End Sub

    ''' <summary>
    ''' Extracts the directory which best fits the given text or returns null if a
    ''' meaningful directory could not be guessed from it.
    ''' </summary>
    ''' <param name="dirText">The text from which to try and intuit a directory
    ''' path.</param>
    ''' <returns>The full path to the directory which matches closest to the given
    ''' text or null if no such directory could be gleaned from the given text.
    ''' </returns>
    Protected Function ExtractBestFitDir(ByVal dirText As String) As String

        If dirText IsNot Nothing Then dirText = dirText.Trim()

        ' First, simplest check - if dirText is empty, we have nothing...
        If dirText = "" Then Return Nothing

        Try

            ' Now let's see if the text does actually represent a directory
            If Directory.Exists(dirText) Then Return New DirectoryInfo(dirText).FullName

            ' Failing that, see if it's a filename - if it is return the directory it's in.
            If File.Exists(dirText) Then Return New FileInfo(dirText).DirectoryName

            ' So it either doesn't exist or it's invalid. Check for invalid chars first
            Dim invalidCharIndex As Integer = dirText.IndexOfAny(Path.GetInvalidPathChars())
            ' Substring up to the offending character and attempt to extract a
            ' best fit directory from that instead
            If invalidCharIndex >= 0 Then _
             Return ExtractBestFitDir(dirText.Substring(0, invalidCharIndex))

            ' At this point, it just doesn't exist, so go up the directory tree until
            ' a directory exists, or we have no more string to play with
            Dim dir As String = dirText
            Do
                dir = Path.GetDirectoryName(dir)
                If dir Is Nothing Then Return Nothing Else dir = dir.Trim()
            Loop While dir <> "" AndAlso Not Directory.Exists(dir)

            If dir = "" Then Return Nothing
            Return New DirectoryInfo(dir).FullName

        Catch
            ' Any exceptions are caused by one of the IO methods getting an invalid
            ' form of file / directory path. No point in passing it further up - just
            ' treat it as 'no directory name'
            Return Nothing

        End Try
    End Function

End Class
