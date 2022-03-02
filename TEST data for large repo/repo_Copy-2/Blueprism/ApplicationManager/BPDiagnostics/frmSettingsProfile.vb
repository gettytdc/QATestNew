Imports microsoft.Win32

''' <summary>
''' A simple settings profiler form.
''' </summary>
''' <remarks></remarks>
Public Class frmSettingsProfile

    ''' <summary>
    ''' The common builder interface for RegistryBuilder and ReadableBuilder
    ''' </summary>
    Private Interface Builder
        Sub WriteValue(ByVal name As String, ByVal value As Object)
        Sub WriteHeader(ByVal path As String)
        Sub WriteFooter()
        Sub Close()
    End Interface

    ''' <summary>
    ''' Encapsulates the code nessacery to build regedit compatible output.
    ''' </summary>
    Private Class RegistryBuilder
        Implements Builder

        Private Result As IO.StreamWriter

        Public Sub New(ByVal path As String)
            Result = New IO.StreamWriter(path, False)
            Result.WriteLine("Windows Registry Editor Version 5.00")
            Result.WriteLine()
        End Sub

        Private Function RegFormat(ByVal o As Object) As String

            Dim s As String
            If TypeOf o Is Integer Then
                s = CInt(o).ToString("X8")
            Else
                s = o.ToString
            End If

            Select Case o.GetType.Name
                Case "Int32"
                    Return "dword:" & s
                Case "String"
                    Return """" & s & """"
            End Select

            Return Nothing
        End Function

        Public Sub Close() Implements Builder.Close
            Result.Close()
        End Sub

        Public Sub WriteFooter() Implements Builder.WriteFooter
            Result.WriteLine()
        End Sub

        Public Sub WriteHeader(ByVal path As String) Implements Builder.WriteHeader
            Result.WriteLine("[HKEY_LOCAL_MACHINE\" & path & "]")
        End Sub

        Public Sub WriteValue(ByVal name As String, ByVal value As Object) Implements Builder.WriteValue
            If name = "" Then
                Result.WriteLine("@=" & RegFormat(value))
            Else
                Result.WriteLine("""" & name & """=" & RegFormat(value))
            End If
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            Result.Dispose()
            Result = Nothing
        End Sub

    End Class

    ''' <summary>
    ''' Encapsulates the code nessacery to build readable output.
    ''' </summary>
    Private Class ReadableBuilder
        Implements Builder

        Private LookupTable As Hashtable
        Private Delegate Sub AppendTextDelegate(ByVal s As String)
        Private AppendTextInstance As New AppendTextDelegate(AddressOf AppendTextInvoke)
        Private AppendInvokeForm As frmSettingsProfile

        Protected Sub AppendText(ByVal s As String)
            If Not s Is Nothing Then
                AppendInvokeForm.Invoke(AppendTextInstance, s)
            End If
        End Sub

        Private Sub AppendTextInvoke(ByVal s As String)
            AppendInvokeForm.TextBox1.AppendText(s)
        End Sub

        Public Sub New(ByVal form As frmSettingsProfile)
            AppendInvokeForm = form
            LookupTable = New Hashtable
            'This list was taken from http://support.microsoft.com/kb/182569
            LookupTable.Add("1001", "ActiveX controls and plug-ins: Download signed ActiveX controls")
            LookupTable.Add("1004", "ActiveX controls and plug-ins: Download unsigned ActiveX controls")
            LookupTable.Add("1200", "ActiveX controls and plug-ins: Run ActiveX controls and plug-ins")
            LookupTable.Add("1201", "ActiveX controls and plug-ins: Initialize and script ActiveX controls not marked as safe for scripting")
            LookupTable.Add("1206", "Miscellaneous: Allow scripting of Internet Explorer Web browser control ")
            LookupTable.Add("1207", "Reserved #1")
            LookupTable.Add("1208", "ActiveX controls and plug-ins: Allow previously unused ActiveX controls to run without prompt ")
            LookupTable.Add("1209", "ActiveX(Controls And plug - ins) : Allow(Scriptlets)")
            LookupTable.Add("120A", "ActiveX controls and plug-ins: ActiveX controls and plug-ins: Override Per-Site (domain-based) ActiveX restrictions")
            LookupTable.Add("120B", "ActiveX controls and plug-ins: Override Per-Site (domain-based) ActiveX restrictions")
            LookupTable.Add("1400", "Scripting() : Active(scripting)")
            LookupTable.Add("1402", "Scripting: Scripting of Java applets")
            LookupTable.Add("1405", "ActiveX controls and plug-ins: Script ActiveX controls marked as safe for scripting")
            LookupTable.Add("1406", "Miscellaneous: Access data sources across domains")
            LookupTable.Add("1407", "Scripting: Allow Programmatic clipboard access")
            LookupTable.Add("1408", "Reserved #2")
            LookupTable.Add("1601", "Miscellaneous: Submit non-encrypted form data")
            LookupTable.Add("1604", "Downloads() : Font(download)")
            LookupTable.Add("1605", "Run Java ")
            LookupTable.Add("1606", "Miscellaneous: Userdata persistence ")
            LookupTable.Add("1607", "Miscellaneous: Navigate sub-frames across different domains")
            LookupTable.Add("1608", "Miscellaneous: Allow META REFRESH  ")
            LookupTable.Add("1609", "Miscellaneous: Display mixed content ")
            LookupTable.Add("160A", "Miscellaneous: Include local directory path when uploading files to a server ")
            LookupTable.Add("1800", "Miscellaneous: Installation of desktop items")
            LookupTable.Add("1802", "Miscellaneous: Drag and drop or copy and paste files")
            LookupTable.Add("1803", "Downloads: File Download ")
            LookupTable.Add("1804", "Miscellaneous: Launching programs and files in an IFRAME")
            LookupTable.Add("1805", "Launching programs and files in webview ")
            LookupTable.Add("1806", "Miscellaneous: Launching applications and unsafe files")
            LookupTable.Add("1807", "Reserved #3")
            LookupTable.Add("1808", "Reserved #4")
            LookupTable.Add("1809", "Miscellaneous: Use Pop-up Blocker  ")
            LookupTable.Add("180A", "Reserved #5")
            LookupTable.Add("180B", "Reserved #6")
            LookupTable.Add("180C", "Reserved #7")
            LookupTable.Add("180D", "Reserved #8")
            LookupTable.Add("1A00", "User Authentication: Logon")
            LookupTable.Add("1A02", "Allow persistent cookies that are stored on your computer ")
            LookupTable.Add("1A03", "Allow per-session cookies (not stored) ")
            LookupTable.Add("1A04", "(Miscellaneous) : Don() 't prompt for client certificate selection when no certificates or only one certificate exists  ")
            LookupTable.Add("1A05", "Allow 3rd party persistent cookies ")
            LookupTable.Add("1A06", "Allow 3rd party session cookies ")
            LookupTable.Add("1A10", "Privacy Settings ")
            LookupTable.Add("1C00", "Java permissions ")
            LookupTable.Add("1E05", "Miscellaneous: Software channel permissions")
            LookupTable.Add("1F00", "Reserved  #9")
            LookupTable.Add("2000", "ActiveX controls and plug-ins: Binary and script behaviors")
            LookupTable.Add("2001", ".NET Framework-reliant components: Run components signed with Authenticode")
            LookupTable.Add("2004", ".NET Framework-reliant components: Run components not signed with Authenticode")
            LookupTable.Add("2100", "Miscellaneous: Open files based on content, not file extension  ")
            LookupTable.Add("2101", "Miscellaneous: Web sites in less privileged web content zone can navigate into this zone ")
            LookupTable.Add("2102", "Miscellaneous: Allow script initiated windows without size or position constraints  ")
            LookupTable.Add("2103", "Scripting: Allow status bar updates via script ")
            LookupTable.Add("2104", "Miscellaneous: Allow websites to open windows without address or status bars ")
            LookupTable.Add("2105", "Scripting: Allow websites to prompt for information using scripted windows ")
            LookupTable.Add("2200", "Downloads: Automatic prompting for file downloads  ")
            LookupTable.Add("2201", "ActiveX controls and plug-ins: Automatic prompting for ActiveX controls  ")
            LookupTable.Add("2300", "Miscellaneous: Allow web pages to use restricted protocols for active content ")
            LookupTable.Add("2301", "Miscellaneous: Use Phishing Filter ")
            LookupTable.Add("2400", ".NET Framework: XAML browser applications")
            LookupTable.Add("2401", ".NET(Framework): XPS(documents)")
            LookupTable.Add("2402", ".NET(Framework): Loose(XAML)")
            LookupTable.Add("2500", "Turn on Protected Mode [Vista only setting] ")
            LookupTable.Add("2600", "Enable .NET Framework setup ")
        End Sub

        Private Function GetSetting(ByVal o As Object) As String
            Select Case CInt(o)
                Case 0
                    Return "Permitted"
                Case 1
                    Return "Prompt"
                Case 3
                    Return "Prohibited"
            End Select

            Return "Unknown"
        End Function

        Private Delegate Sub ButtonEnableDelegate()

        Private Sub ButtonEnableInvoke()
            AppendInvokeForm.btnSave.Enabled = True
        End Sub

        Public Sub Close() Implements Builder.Close
            Dim d As New ButtonEnableDelegate(AddressOf ButtonEnableInvoke)
            AppendInvokeForm.Invoke(d)
        End Sub

        Public Sub WriteFooter() Implements Builder.WriteFooter
            AppendText(vbCrLf)
        End Sub

        Public Sub WriteHeader(ByVal path As String) Implements Builder.WriteHeader
            AppendText("Settings for: " & path & vbCrLf & vbCrLf)
        End Sub

        Public Sub WriteValue(ByVal name As String, ByVal value As Object) Implements Builder.WriteValue
            If name = "" Then
                AppendText(vbTab & "Default = " & value.ToString & vbCrLf)
            Else
                Dim up As Object = LookupTable(name)
                If Not TypeOf value Is Integer OrElse up Is Nothing Then
                    AppendText(vbTab & name & " = " & value.ToString & vbCrLf)
                Else
                    AppendText(vbTab & up & " = " & GetSetting(value) & vbCrLf)
                End If
            End If
        End Sub
    End Class

    ''' <summary>
    ''' Gets the settings from the registry
    ''' </summary>
    Public Sub GetSettings()

        Const VersionPath As String = "SOFTWARE\Microsoft\Internet Explorer"
        Dim InternetVersion As RegistryKey = Registry.LocalMachine.OpenSubKey(VersionPath)
        OutputBuilder.WriteHeader(VersionPath)
        Dim v As Object = InternetVersion.GetValue("Version")
        OutputBuilder.WriteValue("Version", v)
        OutputBuilder.WriteFooter()

        Const SettingsPath As String = "SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings"
        Dim InternetSettings As RegistryKey = Registry.LocalMachine.OpenSubKey(SettingsPath)
        Dim Zones As RegistryKey = InternetSettings.OpenSubKey("Zones")
        For Each zonename As String In Zones.GetSubKeyNames
            Dim Zone As RegistryKey = Zones.OpenSubKey(zonename)
            OutputBuilder.WriteHeader(SettingsPath & "\Zones\" & zonename)

            For Each name As String In Zone.GetValueNames
                Dim o As Object = Zone.GetValue(name)
                OutputBuilder.WriteValue(name, o)
            Next

            OutputBuilder.WriteFooter()
        Next

        OutputBuilder.Close()
    End Sub

    ''' <summary>
    ''' Holds a reference to a builder that is used either to build the Registry style
    ''' output to file or readable output to screen
    ''' </summary>
    Private OutputBuilder As Builder

    ''' <summary>
    ''' The click event handler for the Get Settings button
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnGetSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetSettings.Click
        OutputBuilder = New ReadableBuilder(Me)
        Me.TextBox1.Clear()
        Dim t As New Threading.Thread(AddressOf GetSettings)
        t.Start()
    End Sub

    ''' <summary>
    ''' The click event handler for the Save button
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        If SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim s As String = SaveFileDialog1.FileName
            OutputBuilder = New RegistryBuilder(s)
            GetSettings()
            TextBox1.Text = String.Format(My.Resources.SettingsSavedTo0, s)
        End If
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Close()
    End Sub
End Class
