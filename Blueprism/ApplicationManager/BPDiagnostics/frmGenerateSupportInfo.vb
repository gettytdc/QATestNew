Option Strict On

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Environment
Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Management
Imports System.Runtime.InteropServices
Imports System.Text
Imports BluePrism.BPCoreLib
Imports Microsoft.Win32

''' <summary>
''' The functionality in this form is directly ported from a VB-script created by customer 
''' services to allow customers to quickly gather logs and other information required to 
''' diagnose problems.
''' The resulting documents are copied into a folder and zipped so the zip file can be sent 
''' to customer services.
''' </summary>
Public Class frmGenerateSupportInfo
#Region "Variables"

    Private mBluePrismProgramDataFolder As String
    Private mSupportFolder As String
    Private mResultsFolder As String
    Private mInstallInfo As String
    Private mClosePending As Boolean

#End Region

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
    End Sub

#Region "Events"

    Private Sub frmGenerateSupportInfo_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        btnClose.Enabled = False

        Dim installed As Boolean = GetBPInstallPathAndVersion()
        If Not installed Then
            btnClose.Enabled = True
            Exit Sub
        End If

        ' Produce Message Box with information and option to cancel
        Dim messageTemplate As String =
            My.Resources.frmGenerateSupportInfo_Resources.StartInfoMessage_Template

        Dim result As DialogResult = MessageBox.Show(
            Me,
            String.Format(messageTemplate, mSupportFolder),
            My.Resources.frmGenerateSupportInfo_Resources.GenerateSupportInformation,
            MessageBoxButtons.OKCancel)

        If result = DialogResult.Cancel Then
            Me.Close()
            Exit Sub
        End If

        ' Otherwise start the processing
        If Not generatorWorker.IsBusy Then generatorWorker.RunWorkerAsync()
    End Sub


    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        If Not generatorWorker.IsBusy Then Me.Close()
    End Sub

    Private Sub generatorWorker_DoWork(sender As Object, e As DoWorkEventArgs) _
        Handles generatorWorker.DoWork

        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        ' Start outputting informtion to window
        WriteLineToUI("Blue Prism support information processing started at " & Now)
        WriteLineToUI("")
        WriteLineToUI("The results will be stored in the following location: """ & mResultsFolder & """")
        WriteLineToUI("")

        If HasAdminAccess() Then
            GenerateSupportInfo(sender, e)
        Else
            WriteLineToUI(vbCrLf & "It appears that you do not have administrative rights on this machine")
            WriteLineToUI("Could not generate support information.")
        End If
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If generatorWorker.IsBusy Then
            mClosePending = True
            generatorWorker.CancelAsync()
            e.Cancel = True
            Me.Enabled = False
            Return

        End If
        MyBase.OnFormClosing(e)
    End Sub


    Private Sub BackgroundWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) _
        Handles generatorWorker.RunWorkerCompleted

        btnClose.Enabled = True

        If mClosePending Then Me.Close()
        mClosePending = False
    End Sub


#End Region


#Region "Methods"

    ''' <summary>
    ''' Determines whether BluePrism is installed and populates the Blue Prism install path 
    ''' and version so the details can be used in the UI and in methods which collect the
    ''' support information.
    ''' </summary>
    ''' <returns>A boolean value indicating whether Blue Prism is installed</returns>
    Private Function GetBPInstallPathAndVersion() As Boolean

        Dim regKeys As String() = New String() { 
            "HKEY_LOCAL_MACHINE\SOFTWARE\Blue Prism Limited\Automate\",
            "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Blue Prism Limited\Automate\",
            "HKLM\SOFTWARE\Blue Prism Limited\Automate\",
            "HKLM\SOFTWARE\WOW6432Node\Blue Prism Limited\Automate\"
        }

        Dim version As String = RegistryHelper.GetValueAsString(regKeys, "Version")
        Dim nativeInstallPath As String = RegistryHelper.GetValueAsString(regKeys, "InstallDir")
        
        If nativeInstallPath = "" Then
            WriteLineToUI("Blue Prism is not installed or could not be found.")
            Return False
        End If

        Dim installDirectoryBitness As String = GetInstallDirectoryBitness(nativeInstallPath)
        mInstallInfo = $"Blue Prism Path : {nativeInstallPath}
Blue Prism Version : {version} {installDirectoryBitness}"

        ' get the correct ProgramData folder path, this is where we will store the output
        mBluePrismProgramDataFolder = BPUtil.CombinePaths(GetFolderPath(SpecialFolder.CommonApplicationData),
                                      "Blue Prism Limited", "Automate V3")
        mSupportFolder = mBluePrismProgramDataFolder & "\Support"

        ' Pad single digits to two with leading zero
        Dim machineName As String = Environment.MachineName
        Dim directoryName As String = String.Format("BP_{0}_{1:yyyyMMdd}_{1:HH-mm-ss}", machineName, DateTime.Now)
        mResultsFolder = Path.Combine(mSupportFolder, directoryName)
        Return True

    End Function

    Private Function GetInstallDirectoryBitness(nativeInstallPath As String) As String
        Dim standardDirectory As String = GetFolderPath(SpecialFolder.ProgramFiles)
        Dim x86Directory As String = GetFolderPath(SpecialFolder.ProgramFilesX86)

        Dim separateX86Directory As Boolean = (Not String.IsNullOrWhiteSpace(x86Directory)) _
            AndAlso (Not standardDirectory.Equals(x86Directory, StringComparison.OrdinalIgnoreCase))
        If Not separateX86Directory Then
            ' We only have ProgramFiles - assume 32-bit
            Return "32-bit"
        Else
            ' We have both ProgramFiles and ProgramFilesX86 directories
            Dim installedInX86Directory As Boolean = nativeInstallPath.StartsWith(x86Directory, StringComparison.OrdinalIgnoreCase)
            Return If(installedInX86Directory, "32-bit", "64-bit")
        End If
    End Function


    ''' <summary>
    ''' Main method to gather information, write to files and create and zip the containing 
    ''' folder.
    ''' </summary>
    Private Sub GenerateSupportInfo(sender As Object, e As DoWorkEventArgs)
        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        ' Create Folder and file and set as current directory
        Directory.CreateDirectory(mResultsFolder)
        Directory.SetCurrentDirectory(mResultsFolder)

        WriteSummary()

        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        ' Produce and Copy Necessary Files
        RunStep("Copying userenv log files ...", Sub()
            Dim windowsDirectory As String = Environment.GetFolderPath(SpecialFolder.Windows)
            CopyFileIfExists(Path.Combine(windowsDirectory, "Debug\UserMode\userenv.log"), "EnvironmentUser.log")
            CopyFileIfExists(Path.Combine(windowsDirectory, "Debug\UserMode\userenv.bak"), "EnvironmentUser.bak")
            CopyFileIfExists(Path.Combine(windowsDirectory, "system32\GroupPolicy\gpt.ini"), "GroupPolicyGPT.ini")
        End Sub)

        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        RunStep("Dumping event logs ...", AddressOf ExportEventLog)

        RunStep("Running ipconfig ...", Sub() ExecuteAndRecord("ipconfig.exe", "/all", "IPConfig.txt"))

        RunStep("Running systeminfo ...", Sub() ExecuteAndRecord("systeminfo.exe", Nothing, "SysInfo.txt"))

        RunStep("Running tasklist ...", Sub()
                                            ExecuteAndRecord("tasklist.exe", "/v /fo csv", "tasklist.csv")
                                            ExecuteAndRecord("tasklist.exe", "/svc /fo csv", "tasklist-svc.csv")
                                        End Sub)

        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        RunStep("Running gpresult ...", Sub() ExecuteAndRecord("gpresult.exe", "/scope computer /z", "GPResult.txt"))

        RunStep("Running netstat ...", Sub() ExecuteAndRecord("netstat", "-a -b -f -n -o -p tcp -r", "Netstat.txt"))

        RunStep("Running netsh firewall ...", Sub() ExecuteAndRecord("netsh.exe", "firewall show config", "NetshFireWall.txt"))
        If generatorWorker.CancellationPending Then e.Cancel = True : Return

        RunStep("Writing out registry keys ...", Sub()
                                                     SaveKeys("Software\Blue Prism Limited", "RegKey-BluePrism.txt")
                                                     SaveKeys("software\Citrix", "RegKey-Citrix.txt")

                                                     ' Now do for x64 - if on x86 then will just fail
                                                     SaveKeys("Software\Wow6432node\Blue Prism Limited", "RegKey-BluePrism64.txt")
                                                     SaveKeys("software\Wow6432node\Citrix", "RegKey-Citrix64.txt")

                                                     Shell("reg.exe save " & """HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall""" & " RegKey-Uninstall.hiv", 0, True)
                                                 End Sub)

        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        RunStep("Enumerating installed products & patches ...", Sub()
                                                                    Dim errors As ICollection(Of String) =
                    InstalledProductEnumerator.EnumerateInstalledProducts("Installed-Products.csv")

                                                                    Dim errorList As List(Of String) = DirectCast(errors, List(Of String))

                                                                    If errorList.Count > 0 Then
                                                                        For Each err As String In errorList
                                                                            WriteLineToUI(err)
                                                                        Next
                                                                    End If

                                                                End Sub)
        If generatorWorker.CancellationPending Then e.Cancel = True : Return

        ' Enumerate all processes with details
        RunStep("Enumerating running processes ...", AddressOf ExportRunningProcesses)

        ' Get Configs
        RunStep("Copying Blue Prism configurations ...", Sub()
                                                             Dim automatePath As String = BPUtil.CombinePaths(mBluePrismProgramDataFolder, "Automate.config")
                                                             Dim loginAgentPath As String = BPUtil.CombinePaths(mBluePrismProgramDataFolder, "LoginAgentService.config")

                                                             CopyFileIfExists(automatePath, "Automate.config")
                                                             CopyFileIfExists(loginAgentPath, "LoginAgentService.config")
                                                         End Sub)

        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        ' Find BluePrism agent services so we can look at what files we have on the system
        RunStep("Examining services and files ...", AddressOf ExportServices)

        RunStep("Logging output ...", Sub()
                                          ' Write the output from the UI to a text file to capture any errors encountered 
                                          ' during the information gathering
                                          Using fstm As FileStream = File.OpenWrite("BPDiagnosticsOutput.txt")
                                              Using sw As StreamWriter = New StreamWriter(fstm)
                                                  sw.WriteLine(txtOutput.Text)
                                              End Using
                                          End Using
                                      End Sub)

        If generatorWorker.CancellationPending Then e.Cancel = True : Return
        ' Write Zip File
        RunStep("Creating zip file ...", Sub()
                                             Dim fullZipPath As String = mResultsFolder & ".zip"
                                             Try
                                                 ZipFile.CreateFromDirectory(mResultsFolder, fullZipPath)

                                                 WriteLineToUI(vbCrLf & "Completed at " & Now)
                                                 WriteLineToUI("")
                                                 WriteLineToUI("Created zip file: """ & fullZipPath & vbCrLf)

                                             Catch ex As Exception
                                                 WriteLineToUI(vbCrLf & "Error creating zip file """ & fullZipPath & """")
                                                 WriteLineToUI("Error: " & ex.Message)

                                             End Try
                                         End Sub)
        Dim explorerPath As String = Path.Combine(GetFolderPath(SpecialFolder.Windows), "explorer.exe")
        RunStep("Opening results folder ...", Sub() Process.Start(explorerPath, mSupportFolder))

        WriteLineToUi("Export finished.")

    End Sub

    Private Sub ExportEventLog()

        If File.Exists("BluePrism.evtx") Then File.Delete("BluePrism.evtx")
        EvtExportLog(IntPtr.Zero,
                     "Blue Prism",
                     "
<QueryList>
    <Query Id=""0"" Path=""Blue Prism"">
        <Select Path=""Blue Prism"">*[System[TimeCreated[timediff(@SystemTime)&lt;=" & TimeSpan.FromDays(7).TotalMilliseconds & "]]]</Select>
    </Query>
</QueryList>",
                     "BluePrism.evtx",
                     EventExportLogFlags.ChannelPath)
    End Sub

    ''' <summary>
    ''' Enumerates Windows Services and writes their properties out, appending them to the 
    ''' existing summary file.
    ''' </summary>
    Private Sub ExportServices()

        Try
            Dim writer As ManagementObjectCsvWriter = New ManagementObjectCsvWriter
            writer.AddProperty("Name") _
            .AddProperty("PathName") _
            .AddProperty("ServiceType") _
            .AddProperty("StartMode") _
            .AddProperty("StartName") _
            .AddProperty("State")

            Dim services As ManagementObjectCollection = GetManagementObjects( _
                "Select * from Win32_Service where DisplayName Like ""Blue Prism%""") 
            Dim content As String = writer.Write(services, False)

            Using sw As StreamWriter = File.AppendText("_Summary.txt")
                If services IsNot Nothing Then
                    sw.WriteLine("Blue Prism Services" & vbCrLf & "===================")
                End If
                sw.WriteLine(content)
            End Using
        Catch ex As Exception
            WriteLineToUI("Error enumerating services: " & ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Enumerates all the running processes and writes their properties to a text file
    ''' </summary>
    Private Sub ExportRunningProcesses()
        Try
            Dim writer As New ManagementObjectCsvWriter
            writer.AddProperty("CreationDate") _
            .AddProperty("Name") _
            .AddProperty("ProcessId") _
            .AddProperty("ParentProcessId") _
            .AddProperty("SessionId") _
            .AddProperty("Priority") _
            .AddProperty("CommandLine") _
            .AddProperty("ExecutablePath") _
            .AddProperty("KernelModeTime", AddressOf ManagementObjectFormatter.Time) _
            .AddProperty("UserModeTime", AddressOf ManagementObjectFormatter.Time) _
            .AddProperty("HandleCount") _
            .AddProperty("ThreadCount") _
            .AddProperty("MaximumWorkingSetSize") _
            .AddProperty("MinimumWorkingSetSize") _
            .AddProperty("WorkingSetSize") _
            .AddProperty("PeakWorkingSetSize") _
            .AddProperty("PageFaults") _
            .AddProperty("PageFileUsage") _
            .AddProperty("PeakPageFileUsage") _
            .AddProperty("PeakVirtualSize") _
            .AddProperty("QuotaPagedPoolUsage") _
            .AddProperty("QuotaPeakPagedPoolUsage") _
            .AddProperty("QuotaNonPagedPoolUsage") _
            .AddProperty("QuotaPeakNonPagedPoolUsage") _
            .AddProperty("ReadOperationCount") _
            .AddProperty("WriteOperationCount") _
            .AddProperty("OtherOperationCount") _
            .AddProperty("ReadTransferCount") _
            .AddProperty("WriteTransferCount") _
            .AddProperty("OtherTransferCount") _
            .AddProperty("VirtualSize")

            Dim processes As ManagementObjectCollection = GetManagementObjects("select * from Win32_Process")
            Dim content As String = writer.Write(processes, True)
            File.WriteAllText("processes.csv", content)
            Return

        Catch ex As Exception
            WriteLineToUI("Error enumerating processes: " & ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Gets MAPIEx version 
    ''' </summary>
    ''' <returns>String representing MAPIEx version</returns>
    Private Function GetMapiExVersion() As String
        Dim result As String = ""
        Dim temp1 As String
        Dim temp2 As String
        Dim mapidata As String()

        Try
            Dim features As ManagementObjectCollection = GetManagementObjects("select * from Win32_ProductSoftwareFeatures")
            For Each feature As ManagementObject In features
                temp1 = feature.ToString
                If CBool(InStr(1, temp1, "ProductName=\""Blue Prism MAPIEx", vbTextCompare)) Then
                    mapidata = Split(temp1, ",")
                    temp1 = mapidata(UBound(mapidata) - 1)
                    temp1 = Replace(temp1, "ProductName=""", "", 1, -1, vbTextCompare)
                    temp1 = temp1 + New String(" "c, 27)
                    temp1 = temp1.Left(27)
                    temp2 = mapidata(UBound(mapidata))
                    temp2 = Replace(temp2, "Version=""", "Version: ", 1, -1, vbTextCompare)
                    result = result + temp1 & " - " & temp2 & vbCrLf
                    result = Replace(result, """", "")
                End If
            Next
            If result = "" Then
                Return "MAPIEx is not installed."
            Else
                Return result.Left(Len(result) - 2)
            End If
        Catch ex As Exception
            WriteLineToUI("Error obtaining MAPIEx version: " & ex.Message)
            Return ("Unable to determine whether MAPIEx is installed")
        End Try

    End Function


    ''' <summary>
    ''' Save the specified keys called from the Registry
    ''' </summary>
    ''' <param name="baseKey"></param>
    ''' <param name="outputFile"></param>
    Private Sub SaveKeys(baseKey As String, outputFile As String)
        ' Create a temporary location to save the file as we cannot write to Program Data 
        ' folder in Win 8 upwards
        Dim tempFilePath As String = Path.GetTempPath & outputFile
        Dim input As String = BPUtil.BuildCommandLineArgString(
           "/e", tempFilePath, "HKEY_LOCAL_MACHINE\" & baseKey)
        ' Check the key actually exists first
        Using key As RegistryKey = Registry.LocalMachine.OpenSubKey(baseKey)
            If key Is Nothing Then Return
        End Using

        ' if the key does exist, then try to write it
        Try
            Dim regeditPath As String = Path.Combine(GetFolderPath(SpecialFolder.Windows), "regedit.exe")
            Dim regeditProcess As Process = Process.Start(regeditPath, input)
            regeditProcess.WaitForExit() 'wait until regedit.exe process is finished importing the file 
            ' now move the file to the output folder
            If File.Exists(tempFilePath) Then File.Move(tempFilePath, mResultsFolder & "\" & outputFile)
        Catch e As Exception
            WriteLineToUi(String.Format("Error writing registry key {0}: {1}", baseKey, e.Message))
        End Try

    End Sub

    ''' <summary>
    ''' Copies a file to the directory
    ''' </summary>
    ''' <param name="sourceFile">String path representing the file to be copied</param>
    ''' <param name="destinationFile">String path representing the destination to copy the 
    ''' file to</param>
    Private Sub CopyFileIfExists(sourceFile As String, destinationFile As String)
        If Directory.Exists(Directory.GetParent(sourceFile).ToString) AndAlso
            File.Exists(sourceFile) Then
            File.Copy(sourceFile, destinationFile, True)
        End If
    End Sub

    ''' <summary>
    ''' Runs the requested program and records the results
    ''' </summary>
    ''' <param name="program">String representing the program to execute</param>
    ''' <param name="arguments">Stringrepresenting the arguments to pass</param>
    ''' <param name="outputFile">A string representing the file to record the output to.</param>
    Private Sub ExecuteAndRecord(program As String, arguments As String, outputFile As String)

        Dim p As New Process
        With p.StartInfo
            .UseShellExecute = False
            .FileName = program
            .Arguments = arguments
            .RedirectStandardOutput = True
            .WindowStyle = ProcessWindowStyle.Hidden
            .CreateNoWindow = True
        End With
        Try
            Using fs As FileStream = File.OpenWrite(outputFile)
                ' Create a file to write to if it doesn't already exist
                Using sw As StreamWriter = New StreamWriter(fs)
                    p.Start()
                    Using reader As StreamReader = p.StandardOutput
                        sw.WriteLine(reader.ReadToEnd())
                        p.WaitForExit()
                    End Using
                End Using
            End Using
        Catch e As Exception
            WriteLineToUI("Error executing process: " & program &
                                 vbCrLf & e.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Gets Java version as a string
    ''' </summary>
    ''' <returns>A string representing the installed Java version</returns>
    Private Function Java() As String
        Dim result As String = ""
        Dim javadata As String()
        Dim temp1 As String
        Dim temp2 As String
        Try
            Dim features As ManagementObjectCollection = GetManagementObjects("select * from Win32_ProductSoftwareFeatures")
            For Each feature As ManagementObject In features
                temp1 = feature.ToString
                If CBool(InStr(1, temp1, "ProductName=\""Java", vbTextCompare)) Then
                    javadata = Split(temp1, ",")
                    temp1 = javadata(UBound(javadata) - 1)
                    temp1 = Replace(temp1, "ProductName=\""", "", 1, -1, vbTextCompare)
                    temp1 = temp1 + New String(" "c,27)
                    temp1 = temp1.Left(27)
                    temp2 = javadata(UBound(javadata))
                    temp2 = Replace(temp2, "Version=""", "Version: ", 1, -1, vbTextCompare)
                    result = result + temp1 & " - " & temp2 & vbCrLf
                    result = Replace(result, """", "")
                End If
            Next
        Catch ex As Exception
            Return "Error detecting Java version: " & ex.Message
        End Try

        If result = "" Then
            Return "Java is not installed"
        Else
            Return result.Left(Len(result) - 2)
        End If
    End Function

    ''' <summary>
    ''' Get NET Install Path and Version
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDotNetInstallPathAndVersion() As String
        Dim regPath1 As String = "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"
        Dim regPath2 As String = "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"
        Dim version1 As String = "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"
        Dim version2 As String = "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"
        Dim dotNetVersion As String = ""
        Dim dotNetInstallPath As String = ""

        Try
            dotNetVersion = Registry.GetValue(version1, "Version", Nothing).ToString
            dotNetInstallPath = Registry.GetValue(regPath1, "InstallPath", Nothing).ToString
        Catch e As ArgumentException
            dotNetInstallPath = ""
        End Try

        If dotNetInstallPath = "" Then
            Try
                dotNetVersion = Registry.GetValue(version2, "Version", Nothing).ToString
                dotNetInstallPath = Registry.GetValue(regPath2, "InstallPath", Nothing).ToString
            Catch e As ArgumentException
                dotNetInstallPath = ""
            End Try
        End If

        If dotNetInstallPath = "" Then
            dotNetInstallPath = "Looks like .NET is not installed."
        End If

        Return ".Net Path : " & dotNetInstallPath & vbCrLf & ".Net Version : " & dotNetVersion
    End Function


    ''' <summary>
    ''' Helper method which writes the imput to the textbox in the UI on a new line.
    ''' </summary>
    ''' <param name="message">The text to write to the UI</param>
    Private Sub WriteLineToUi(message As String)
        If InvokeRequired Then Invoke(Sub() WriteLineToUI(message)) : Return
        txtOutput.AppendText(vbCrLf & message)
    End Sub

    ''' <summary>
    ''' Method which writes a summary of the machine name and installed product versions to 
    ''' a text file in the current directory.
    ''' </summary>
    Private Sub WriteSummary()

        Dim path As String = "_Summary.txt"
        Dim compName As String = Environment.MachineName

        Dim sb As StringBuilder = New StringBuilder
        sb.AppendLine("Computer")
        sb.AppendLine("========")
        sb.AppendLine("Name : " & compName)
        sb.AppendLine()

        sb.AppendLine("Blue Prism")
        sb.AppendLine("========")
        sb.AppendLine(mInstallInfo)
        sb.AppendLine()
        sb.AppendLine(GetMapiExVersion())
        sb.AppendLine()

        sb.AppendLine(".Net")
        sb.AppendLine("========")
        sb.AppendLine(GetDotNetInstallPathAndVersion())
        sb.AppendLine()

        sb.AppendLine("Java")
        sb.AppendLine("========")
        sb.AppendLine(Java())
        sb.AppendLine()

        Dim stringToWrite As String = sb.ToString

        Using fstm As FileStream = File.OpenWrite(path)
            Using sw As StreamWriter = New StreamWriter(fstm)
                sw.WriteLine(stringToWrite)
            End Using
        End Using

    End Sub


    ''' <summary>
    ''' Function which checks whether the user has administrative access to the machine.
    ''' </summary>
    ''' <returns>True if the user has administrative privileges.</returns>
    Private Function HasAdminAccess() As Boolean

        Dim shouldExit As Boolean
        Dim key As String = ""
        Try
            key = Registry.GetValue("HKEY_USERS\S-1-5-19\Environment\", "TEMP", Nothing).ToString
        Catch e As Exception
            shouldExit = True
        End Try
        If key Is Nothing Then shouldExit = True

        If shouldExit Then Return False

        Return True

    End Function

    Private Sub RunStep(message As String, action As Action)
        WriteLineToUI(message)
        Try
            action()
        Catch ex As Exception
            WriteLineToUI("Error running step: " & ex.ToString)
        End Try
    End Sub

    Private Function GetManagementObjects(query As String) As ManagementObjectCollection
        Return New ManagementObjectSearcher(query).Get
    End Function

    Public Enum EventExportLogFlags
        ChannelPath = 1
        LogFilePath = 2
        TolerateQueryErrors = &H1000
    End Enum

    <DllImport("wevtapi.dll",
               CallingConvention:=CallingConvention.Winapi,
               CharSet:=CharSet.Auto,
               SetLastError:=True)>
    Public Shared Function EvtExportLog(
                                       sessionHandle As IntPtr,
                                       path As String,
                                       query As String,
                                       targetPath As String,
                                       <MarshalAs(UnmanagedType.I4)> flags As EventExportLogFlags) As Boolean

    End Function


#End Region

    Private Class ManagementObjectCsvWriter

        Private ReadOnly mProperties As New List(Of PropertyFormatter)

        Private Class PropertyFormatter
            Private mFormatFunc As Func(Of String, String)

            Public Sub New(name As String, format As Func(Of String, String))
                If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))
                If format Is Nothing Then Throw New ArgumentNullException(NameOf(format))
                Me.Name = name
                Me.mFormatFunc = format
            End Sub

            Property Name As String
            
            Public Function FormatValue(obj As ManagementObject) As String
                Dim rawValue As String
                Try
                    rawValue = CType(obj(Name), String)
                Catch ex As Exception

                    Dim availableProperties As IEnumerable(Of String) = obj.Properties.OfType(Of PropertyData) _
                        .Select(Function(pd) pd.Name)
                    Dim availablePropertiesSummary As String = String.Join(Environment.NewLine, availableProperties)
                    Throw New ArgumentException($"Error accessing property ""{Name}"". Available properties:
{availablePropertiesSummary}", ex)
                End Try
                Dim formattedValue As String = mFormatFunc(rawValue)

                Return CType(EscapeAndQuote(formattedValue), String)
            End Function

            Private Function EscapeAndQuote(value As String) As Object
                If value Is Nothing Then Return ""

                If Not value.Contains(",") Then Return value

                Dim escaped As String = value.Replace("""", """""")
                Return """" & escaped & """"
            End Function
        End Class

        ''' <summary>
        ''' Adds details of property to format
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="format"></param>
        ''' <returns>The current ManagementObjectCsvWriter instance (allows method chaining)</returns>
        Public Function AddProperty(name As String, Optional format As Func(Of String, String) = Nothing) _
            As ManagementObjectCsvWriter
            If format Is Nothing Then
                format = Function(value) value
            End If
            mProperties.Add(New PropertyFormatter(name, format))
            Return Me
        End Function

        Public Function Write(managementObjects As ManagementObjectCollection, includeHeader As Boolean) As String

            Dim csv As New StringBuilder
            If includeHeader Then AppendHeader(csv)
            For Each managementObject As ManagementObject In managementObjects
                AppendRow(csv, managementObject)
            Next
            Return csv.ToString

        End Function

        Private Sub AppendRow(csv As StringBuilder, managementObject As ManagementObject)
            Dim values As IEnumerable(Of String) = mProperties.Select(Function(p) p.FormatValue(managementObject))
            Dim row As String = String.Join(",", values)
            csv.AppendLine(row)
        End Sub

        Private Sub AppendHeader(csv As StringBuilder)
            Dim header As String = String.Join(",", mProperties.Select(Function(p) p.Name))
            csv.AppendLine(header)
        End Sub

    End Class

    Public Class ManagementObjectFormatter

        Public Shared Function Time(value As String) As String
            Return Math.Round(CDbl(CDbl(value) / 10000000)).ToString(CultureInfo.InvariantCulture)
        End Function

    End Class

    Public Class RegistryHelper
        
        ''' <summary>
        ''' Gets value of specified name within registry key as a string value. 
        ''' </summary>
        ''' <param name="keyName">The registry key</param>
        ''' <param name="valueName">The name of the value</param>
        ''' <returns>Value converted to a string or an empty string if the value is not found</returns>
        Public Shared Function GetValueAsString(keyName As String, valueName As String) As String
            Dim value As Object = Registry.GetValue(keyName, valueName, Nothing)
            Return If(value IsNot Nothing, value.ToString, "")
        End Function

        ''' <summary>
        ''' Searches a sequence of registry keys for a value, takes the first value found and returns
        ''' the string value. Exceptions while reading values will be ignored and the next key used.
        ''' </summary>
        ''' <param name="keyNames">The registry key names</param>
        ''' <param name="valueName">The name of the value</param>
        ''' <returns>Value converted to a string or an empty string if the value is not found</returns>
        Public Shared Function GetValueAsString(keyNames As String(), valueName As String) As String
            Dim value As Object = keyNames _
                .Select(Function(keyName)
                            Try
                                Return Registry.GetValue(keyName, valueName, Nothing)
                            Catch 
                                Return Nothing
                            End Try
                        End Function) _
                .FirstOrDefault(Function(result) result IsNot Nothing)
            Return If(value IsNot Nothing, value.ToString, "")
        End Function

    End Class
End Class

