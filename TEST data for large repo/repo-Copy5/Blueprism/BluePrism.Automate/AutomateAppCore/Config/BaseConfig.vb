Imports System.IO
Imports System.Security
Imports System.Threading
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Common.Security.Exceptions
Imports NLog
Imports BluePrism.BPCoreLib

''' <summary>
''' Base config object which provides the basics for user or machine specific
''' configuration.
''' </summary>
<Serializable()>
Public MustInherit Class BaseConfig

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    ' The number of attempts to open the config file
    Private Const RetryAttempts As Integer = 5

    ' The period in between attempts to open the config file
    Private Const RetryPeriodMillis As Integer = 500

    Protected mLocation As IConfigLocator

    ' Flag indicating if this config has loaded correctly (ever in its lifetime)
    Private mLoaded As Boolean

    ''' <summary>
    ''' Creates a base config object based on the given arguments.
    ''' </summary>
    Public Sub New(location As IConfigLocator)
        mLocation = location
    End Sub

    Public Sub TryLoad(throwNotFoundException As Boolean)
        Try
            Load()

            ' Throw if file or directory are not there so that default values
            ' can be applied, if reset is needed.
        Catch dnfe As DirectoryNotFoundException
            If throwNotFoundException Then Throw
        Catch fnfe As FileNotFoundException
            If throwNotFoundException Then Throw
            ' Any other errors (most likely to be permissions errors);
            ' log them to the event log
        Catch cetex As CertificateException
            mLoaded = False
            Throw
        Catch ex As Exception
            Log.Error(ex, "Error loading {0} config from {1}",
                     mLocation.LocationTypeName,
                     ConfigFile.FullName)
        End Try
    End Sub

    Protected MustOverride ReadOnly Property FileName As String

    ''' <summary>
    ''' The file object representing the backing file for this config object.
    ''' </summary>
    Protected MustOverride ReadOnly Property ConfigFile As FileInfo


    ''' <summary>
    ''' Flag indicating if this config has been loaded successfully or not.
    ''' </summary>
    Public Property Loaded() As Boolean
        Get
            Return mLoaded
        End Get
        Protected Set(ByVal value As Boolean)
            mLoaded = value
        End Set
    End Property

    ''' <summary>
    ''' Creates the directory in which the backing file for this object is stored,
    ''' if it doesn't already exist.
    ''' </summary>
    Private Sub CreateDirectory(directory As DirectoryInfo)
        With directory
            If Not .Exists Then .Create()
        End With
    End Sub

    ''' <summary>
    ''' Opens the backing file for this config object for reading.
    ''' The file is opened with a <see cref="FileMode.Open"/> mode, and with
    ''' <see cref="FileAccess.Read"/> access.
    ''' </summary>
    ''' <returns>A file stream around the config file backing this object, opened
    ''' for reading.</returns>
    ''' <remarks>Note that this can thrown any of the exceptions that the
    ''' <see cref="FileStream"/> constructor can throw. Notable ones are recorded
    ''' in this comment, but the list is not exhaustive.</remarks>
    ''' <exception cref="FileNotFoundException">If the config file representing
    ''' this object does not exist.</exception>
    ''' <exception cref="DirectoryNotFoundException">If the specified path is invalid
    ''' </exception>
    ''' <exception cref="UnauthorizedAccessException">If the process does not have
    ''' required permissions to open the file.</exception>
    Public Overridable Function OpenReadFile() As FileStream
        Return New FileStream(ConfigFile.FullName, FileMode.Open, FileAccess.Read)
    End Function

    ''' <summary>
    ''' Opens the backing file for this config object for reading.
    ''' The file is opened with a <see cref="FileMode.Open"/> mode, and with
    ''' <see cref="FileAccess.Read"/> access. It also specifies that the file must
    ''' be opened in exclusive mode.
    ''' </summary>
    ''' <returns>A file stream around the config file backing this object, opened
    ''' for writing.</returns>
    ''' <remarks>Note that this can thrown any of the exceptions that the
    ''' <see cref="FileStream"/> constructor can throw. Notable ones are recorded
    ''' in this comment, but the list is not exhaustive.</remarks>
    ''' <exception cref="DirectoryNotFoundException">If the specified path is invalid
    ''' </exception>
    ''' <exception cref="UnauthorizedAccessException">If the process does not have
    ''' required permissions to open the file for writing.</exception>
    Public Function OpenWriteFile() As FileStream
        ' Test for the directory and create it if necessary
        Return OpenWriteFile(False)
    End Function

    ''' <summary>
    ''' Opens the configuration file for editing, but makes a copy of the file first.
    ''' </summary>
    ''' <param name="backup"></param>
    ''' <returns></returns>
    Public Function OpenWriteFile(backup As Boolean) As FileStream

        Dim file = ConfigFile
        CreateDirectory(file.Directory)
        If backup Then BackupFile()
        Return New FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None)
    End Function

    ''' <summary>
    ''' Attempt to restore the backup if something failed in the process.
    ''' </summary>
    ''' <returns></returns>
    Public Function RestoreBackup() As Boolean
        Dim backupFile = BackupConfigFileName()
        Try
            If File.Exists(backupFile) Then
                File.Copy(backupFile, ConfigFile.FullName, True)
                File.Delete(backupFile)
            End If
        Catch ex As PermissionException
            'If the user can't access this file, then we don't want to cancel execution
        End Try

    End Function

    ''' <summary>
    ''' After the configuration has been successfully written to disk, this file is no longer needed, so delete it.
    ''' </summary>
    Public Sub RemoveBackup()
        Try
            Dim backupFile = BackupConfigFileName()
            If File.Exists(backupFile) Then
                File.Delete(backupFile)
            End If
        Catch ex As PermissionException
            'If the user can't access this file, then we don't want to cancel execution
        End Try
    End Sub

    ''' <summary>
    ''' Construct filename, replacing the extenstion with .backup
    ''' </summary>
    ''' <returns>File path with extenstion replaced with .backup</returns>
    Private Function BackupConfigFileName() As String
        Return Path.ChangeExtension(ConfigFile.FullName, ".backup")
    End Function

    ''' <summary>
    ''' Backup the file specified by ConfigFile
    ''' </summary>
    Private Sub BackupFile()
        Dim configFileName = ConfigFile.FullName
        Try
            If File.Exists(configFileName) AndAlso ConfigFile.Length > 0 Then
                File.Copy(configFileName, BackupConfigFileName(), True)
            End If
        Catch ex As PermissionException
            'If the user can't access this file, then we don't want to cancel execution
        End Try
    End Sub


    ''' <summary>
    ''' Checks if the current process has privileges enough to save the config
    ''' file to its current configured location.
    ''' Note that if the file is not writable for other reasons (eg. if it is locked
    ''' by another process), this property will not detect that - it just checks the
    ''' process owner's current privileges.
    ''' </summary>
    Public Function HasWritePrivileges() As Boolean

        ' We should be able to save the configuration if we have elevated
        ' privileges or we have write access to the target directory
        If UacHelper.IsProcessElevated Then Return True

        ' We can't (realistically) test any safe way until .net 4 (using
        ' the permission set from the current app domain), so all we can
        ' do is attempt to write the file and see what happens.
        Try
            Dim file = ConfigFile
            CreateDirectory(file.Directory)

            Dim mode As FileMode
            If file.Exists Then mode = FileMode.Append Else mode = FileMode.CreateNew
            Using str As FileStream = file.Open(mode, FileAccess.Write)
                ' We don't actually want to write anything... we just wanted
                ' to find out if we could open a file for writing.
            End Using

            ' We don't want to fail if we don't have delete privileges
            Try
                ' Clean up the file if we just created it for this test
                If mode = FileMode.CreateNew Then file.Delete()
            Catch
            End Try

            Return True

            ' Any of these are "No we don't have write privileges"
        Catch se As SecurityException
        Catch uae As UnauthorizedAccessException
        Catch ioe As IOException
        End Try

        Return False

    End Function

    ''' <summary>
    ''' Loads the configuration from its backing file.
    ''' </summary>
    ''' 
    ''' <remarks>Note that this can throw any of the exceptions that the
    ''' <see cref="FileStream"/> constructor can throw. Notable ones are recorded
    ''' in this comment, but the list is not exhaustive. Also note that subclasses
    ''' which override this method should ensure that the <see cref="Loaded"/>
    ''' property is set appropriately.</remarks>
    ''' <exception cref="FileNotFoundException">If the config file representing
    ''' this object does not exist.</exception>
    ''' <exception cref="DirectoryNotFoundException">If the specified path is invalid
    ''' </exception>
    ''' <exception cref="UnauthorizedAccessException">If the process does not have
    ''' required permissions to open the file.</exception>
    Public Overridable Sub Load()
        Dim attempt As Integer = 1
        While True
            Try
                Using reader As New StreamReader(OpenReadFile())
                    Load(reader)
                End Using
                mLoaded = True
                Return

            Catch ioe As IOException
                attempt += 1
                If attempt <= RetryAttempts Then
                    Thread.Sleep(RetryPeriodMillis)
                Else
                    Log.Warn("Failed to open ""{0}"" for reading - tried {1} time(s)",
                        ConfigFile.FullName, RetryAttempts)
                    Throw
                End If

            End Try
        End While
    End Sub

    ''' <summary>
    ''' Loads the configuration from the given stream reader.
    ''' </summary>
    ''' <param name="reader">The stream reader from which to read the config</param>
    Protected MustOverride Sub Load(ByVal reader As TextReader)

    ''' <summary>
    ''' Attempts to save the configuration while disregarding the stack trace that
    ''' might help track down any problems which occur while saving it.
    ''' </summary>
    ''' <param name="sErr">The error message if an error occurs</param>
    ''' <returns>True to indicate success; False to indicate failure</returns>
    Public Function TrySave(Optional ByRef sErr As String = Nothing) As Boolean
        Try
            Save()
            Return True
        Catch ex As Exception
            Log.Error(ex, "Failed to save the config to ""{0}""", ConfigFile.FullName)
            sErr = ex.Message
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Saves the configuration to its backing file
    ''' </summary>
    ''' <remarks>Note that this can thrown any of the exceptions that the
    ''' <see cref="FileStream"/> constructor can throw. Notable ones are recorded
    ''' in this comment, but the list is not exhaustive.</remarks>
    ''' <exception cref="DirectoryNotFoundException">If the specified path is invalid
    ''' </exception>
    ''' <exception cref="UnauthorizedAccessException">If the process does not have
    ''' required permissions to open the file for writing.</exception>
    Public Overridable Sub Save()
        Dim attempt As Integer = 1
        While True
            Try
                Using writer As New StreamWriter(OpenWriteFile(True))
                    Save(writer)
                End Using
                mLoaded = True
                RemoveBackup()
                Return
            Catch ioe As IOException
                attempt += 1
                If attempt <= RetryAttempts Then
                    Thread.Sleep(RetryPeriodMillis)
                Else
                    Log.Warn("Failed to open ""{0}"" for writing - tried {1} time(s)",
                        ConfigFile.FullName, RetryAttempts)
                    Throw
                End If
            Catch e As Exception
                'catch other exception and restore current configuration file.
                Log.Warn($"General Failed saving file {ConfigFile.FullName}, restoring backip.",
                         ConfigFile.FullName, RetryAttempts)
                RestoreBackup()
                Throw
            End Try
        End While
    End Sub

    ''' <summary>
    ''' Saves the configuration to the given stream writer.
    ''' </summary>
    ''' <param name="writer">The writer to which the config should be saved</param>
    Protected MustOverride Sub Save(ByVal writer As TextWriter)

End Class
