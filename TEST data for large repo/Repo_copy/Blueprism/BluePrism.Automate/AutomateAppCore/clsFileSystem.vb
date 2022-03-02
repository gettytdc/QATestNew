Imports System.IO
Imports System.Windows.Forms
Imports System.Environment
Imports BluePrism.BPCoreLib


''' Project  : Automate
''' Class    : clsFileSystem
''' 
''' <summary>
''' Class to provide shared methods for retrieving file paths used in Automate.
''' </summary>
Public Class clsFileSystem

    ''' <summary>
    ''' True if this is a development installation, False otherwise. Set by the
    ''' shared constructory.
    ''' </summary>
    Private Shared mIsDev As Boolean

    ''' <summary>
    ''' Shared constructor.
    ''' </summary>
    Shared Sub New()

        'Determine if this is a development installation, and save the result for
        'use later.
        Dim sPath As String
        sPath = Path.Combine(Application.StartupPath(), "..")
        sPath = Path.Combine(sPath, "Automate.sln")
        mIsDev = File.Exists(sPath)

    End Sub

    ''' <summary>
    ''' Get the location of the help file.
    ''' </summary>
    ''' <returns>The full path to the help file.</returns>
    Public Shared Function GetHelpFilePath() As String
        Return ApplicationProperties.HelpFilePath
    End Function


    ''' <summary>
    ''' Returns the path where 'graphics' should be loaded from.
    ''' </summary>
    ''' <returns>The 'graphics' folder path</returns>
    Public Shared Function GetGraphicsPath() As String

        If mIsDev Then
            Dim p As String = Path.Combine(Application.StartupPath(), "..")
            p = Path.Combine(p, "Graphics")
            Return p
        End If

        Return Path.Combine(Application.StartupPath, "Graphics")
    End Function

    ''' <summary>
    ''' Returns the path where processes should be loaded from.
    ''' </summary>
    ''' <returns>The 'processes' folder path.</returns>
    Public Shared Function GetProcessesPath() As String

        If mIsDev Then
            Dim p As String = Path.Combine(Application.StartupPath(), "..")
            p = Path.Combine(p, "Processes")
            Return p
        End If

        Return Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), "Processes")
    End Function

    ''' <summary>
    ''' Gets the directory to be used by Automate for temporary files on the local
    ''' disk.
    ''' </summary>
    Public Shared ReadOnly Property TempDirectory() As String
        Get
            Return Path.Combine(AppDataDirectory, "Temp")
        End Get
    End Property

    ''' <summary>
    ''' Gets the directory to be used by Automate for application data on local disk.
    ''' </summary>
    Public Shared ReadOnly Property AppDataDirectory() As String
        Get
            Return GetFolderPath(SpecialFolder.ApplicationData) & "\Blue Prism Limited\Automate V3\"
        End Get
    End Property

    ''' <summary>
    ''' Gets the directory to be used by Automate for common application data on local disk.
    ''' </summary>
    ''' <remarks>This needs to be consistent with the COMMONAPPDATAPRODUCTFOLDER directory defined 
    ''' in the Setup project (Product.wxs)</remarks>
    Public Shared ReadOnly Property CommonAppDataDirectory() As String
        Get
            Return GetFolderPath(SpecialFolder.CommonApplicationData) & "\Blue Prism Limited\Blue Prism\"
        End Get
    End Property
    
    ''' <summary>
    ''' Returns the directory where exported logs should stored.
    ''' </summary>
    ''' <returns>The 'exported logs' folder path</returns>
    Public Shared Function GetExportedLogsPath() As String
        Return GetFolderPath(SpecialFolder.Desktop)
    End Function


    ''' <summary>
    ''' Different file extensions, for which Automate can provide a file filter.
    ''' </summary>
    Public Enum FileExtensions
        XML
        TXT
        EXE
        CSV
        bpprocess
        bpobject
    End Enum

    ''' <summary>
    ''' Gets the filter string, appropriate for use in a file browser dialog,
    ''' using the requested file extension.
    ''' </summary>
    ''' <param name="Filter">The file extension for which a filter string is required.</param>
    ''' <returns>Returns a string, suitable for use in the Forms.OpenFileDialog,
    ''' or similar, for filtering the filetype.</returns>
    Public Shared Function GetFileFilterString(ByVal Filter As FileExtensions) As String
        Select Case Filter
            Case FileExtensions.EXE
                Return My.Resources.clsFileSystem_ExecutableFilesExeExe
            Case FileExtensions.TXT
                Return My.Resources.clsFileSystem_TextFilesTxtTxt
            Case FileExtensions.XML
                Return My.Resources.clsFileSystem_XMLFilesXmlXml
            Case FileExtensions.CSV
                Return My.Resources.clsFileSystem_CSVFilesCsvCsv
            Case FileExtensions.bpprocess
                Return My.Resources.clsFileSystem_bpprocessFilesbpprocess
            Case FileExtensions.bpobject
                Return My.Resources.clsFileSystem_bpobjectFilesbpobject 
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.clsFileSystem_InternalConfigurationErrorNoFilterStringExistsForTheReqestedTypeOf0, Filter.ToString))
        End Select
    End Function

    ''' <summary>
    ''' Gets the path of the program files directory.
    ''' </summary>
    Public Shared ReadOnly Property ProgramFilesDirectory() As String
        Get
            Return GetFolderPath(SpecialFolder.ProgramFiles)
        End Get
    End Property

    ''' <summary>
    ''' Gets the path of the My Documents directory.
    ''' </summary>
    Public Shared ReadOnly Property MyDocumentsDirectory() As String
        Get
            Return GetFolderPath(SpecialFolder.MyDocuments)
        End Get
    End Property

End Class
