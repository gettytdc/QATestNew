Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Xml
Imports BluePrism.BPCoreLib
Imports BluePrism.Utilities.Functional
Imports My.Resources
Imports MyResource = My.Resources

Namespace BluePrism.ApplicationManager.AMI
    ''' <summary>
    ''' This class is used to return information to the client about a particular
    ''' application type.
    ''' </summary>
    Public Class clsApplicationTypeInfo

        ''' <summary>
        ''' Constants for the IDs of various application types, for the cases where
        ''' they need to be referenced directly in the code.
        ''' </summary>
        Public Const Win32ApplicationID As String = "Win32"
        Public Const Win32LaunchID As String = "Win32Launch"
        Public Const Win32AttachID As String = "Win32Attach"
        Public Const JavaApplicationID As String = "Java"
        Public Const JavaAttachID As String = "JavaAttach"
        Public Const JavaLaunchID As String = "JavaLaunch"
        Public Const HTMLApplicationID As String = "HTML"
        Public Const HTMLAttachID As String = "HTMLAttach"
        Public Const HTMLLaunchID As String = "HTMLLaunch"
        Public Const CitrixApplicationId As String = "Citrix"
        Public Const CitrixLaunchID As String = "CitrixLaunch"
        Public Const CitrixAttachID As String = "CitrixAttach"
        Public Const CitrixJavaLaunchID As String = "CitrixJavaLaunch"
        Public Const CitrixJavaAttachID As String = "CitrixJavaAttach"
        Public Const CitrixBrowserLaunchID As String = "CitrixBrowserLaunch"
        Public Const CitrixBrowserAttachID As String = "CitrixBrowserAttach"

        Public Const BrowserApplicationId As String = "Browser"
        Public Const BrowserAttachId As String = "BrowserAttach"
        Public Const BrowserLaunchId As String = "BrowserLaunch"

        Public Const MainframeGEN As String = "MainframeGEN"
        Public Const MainframeHEE As String = "MainframeHEE"
        Public Const MainframePCH As String = "MainframePCH"
        Public Const MainframePWT As String = "MainframePWT"
        Public Const MainframeIAC As String = "MainframeIAC"
        Public Const MainframeCON As String = "MainframeCON"
        Public Const MainframeHAT As String = "MainframeHAT"
        Public Const MainframeINF As String = "MainframeINF"
        Public Const MainframeHUM As String = "MainframeHUM"
        Public Const MainframeATM As String = "MainframeATM"
        Public Const MainframePSS As String = "MainframePSS"
        Public Const MainframeRUM As String = "MainframeRUM"
        Public Const MainframeIBM As String = "MainframeIBM"
        Public Const MainframeTMT As String = "MainframeTMT"
        Public Const MainframeRMD As String = "MainframeRMD"
        Public Const MainframeART As String = "MainframeART"
        Public Const MainframeARN As String = "MainframeARN"

        ''' <summary>
        ''' Returns true for all Mainframe ID's that represent HLLAPI mainframes.
        ''' </summary>
        ''' <param name="appInfo">The appinfo to check</param>
        Public Shared Function IsHLLAPIMainframe(ByVal appInfo As clsApplicationTypeInfo) As Boolean
            Return MainframeType(appInfo) = MainframeTypes.HLLAPI
        End Function

        ''' <summary>
        ''' Returns true for the generic hllapi mainframe type
        ''' </summary>
        ''' <param name="appInfo"></param>
        Public Shared Function IsGenericHLLAPIMainframe(ByVal appInfo As clsApplicationTypeInfo) As Boolean
            If appInfo IsNot Nothing Then
                Return appInfo.ID = MainframeGEN
            End If
        End Function

        Public Shared Function IsAttachmateMainframe(ByVal appInfo As clsApplicationTypeInfo) As Boolean
            If appInfo IsNot Nothing Then
                Return appInfo.ID = MainframeATM
            End If
        End Function

        ''' <summary>
        ''' Returns true for all application ids that represent mainframes.
        ''' </summary>
        ''' <param name="appInfo">The appinfo to check</param>
        Public Shared Function IsMainframe(ByVal appInfo As clsApplicationTypeInfo) As Boolean
            Return MainframeType(appInfo) <> MainframeTypes.Unrecognised
        End Function

        ''' <summary>
        ''' Returns the mainframe id if it is a valid mainframe id.
        ''' </summary>
        ''' <param name="appinfo"></param>
        Public Shared Function ValidMainframeId(appinfo As clsApplicationTypeInfo) As String
            If IsMainframe(appinfo) Then
                Return appinfo.ID
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' Returns the mainframe id if it is a mainframe that supports running macros.
        ''' currently just Attachmate (ATMAPI)
        ''' </summary>
        ''' <param name="appinfo"></param>
        Public Shared Function HasMacroSupport(appinfo As clsApplicationTypeInfo) As Boolean
            Return IsAttachmateMainframe(appinfo)
        End Function


        ''' <summary>
        ''' Returns the mainframe id if it is a mainframe that supports getting the
        ''' parent window title, currently just IBM (COM API)
        ''' </summary>
        ''' <param name="appinfo"></param>
        Public Shared Function HasParentWindowSupport(appinfo As clsApplicationTypeInfo) As Boolean
            If appinfo IsNot Nothing Then
                Return appinfo.ID = MainframeIBM
            End If
            Return False
        End Function

        ''' <summary>
        ''' Returns true if the mainframe supports Launch
        ''' </summary>
        Public Shared Function HasMainframeLaunchSupport(appInfo As clsApplicationTypeInfo) As Boolean
            If appInfo IsNot Nothing Then
                Return appInfo.ID = MainframeHUM
            End If
            Return False
        End Function

        Public Shared Function HasMainframeSendKeySupport(appInfo As clsApplicationTypeInfo) As Boolean
            If IsHLLAPIMainframe(appInfo) Then Return True
            If appInfo IsNot Nothing Then
                Return appInfo.ID = MainframeARN
            End If
            Return False
        End Function

        ''' <summary>
        ''' Returns true if the mainframe supports Attach
        ''' </summary>
        Public Shared Function HasMainframeAttachSupport(appInfo As clsApplicationTypeInfo) As Boolean
            If IsHLLAPIMainframe(appInfo) Then Return True
            If appInfo IsNot Nothing Then
                Return appInfo.ID = MainframeHUM
            End If
            Return False
        End Function

        ''' <summary>
        ''' Returns true if the mainframe supports Detach
        ''' </summary>
        Public Shared Function HasMainframeDetachSupport(appInfo As clsApplicationTypeInfo) As Boolean
            If appInfo IsNot Nothing Then
                Return appInfo.ID = MainframeHUM
            End If
            Return False
        End Function

        ''' <summary>
        ''' Returns true if the mainframe supports Terminate
        ''' </summary>
        Public Shared Function HasMainframeTerminateSupport(appInfo As clsApplicationTypeInfo) As Boolean
            If appInfo IsNot Nothing Then
                Return appInfo.ID = MainframeHUM
            End If
            Return False
        End Function


        ''' <summary>
        ''' Mainframe Types
        ''' </summary>
        Private Enum MainframeTypes
            Unrecognised
            HLLAPI
            Other
        End Enum


        Public Shared Function ParseTerminalType(appType As clsApplicationTypeInfo) As SessionStartInfo.TerminalTypes
            'Use the last three letters of the ID as the terminal type
            Dim ts As String = appType.ID.Substring(appType.ID.Length - 3)
            Return CType(System.Enum.Parse(GetType(SessionStartInfo.TerminalTypes), ts), SessionStartInfo.TerminalTypes)
        End Function


        ''' <summary>
        ''' Gets the mainframe type.
        ''' </summary>
        Private Shared Function MainframeType(appInfo As clsApplicationTypeInfo) As MainframeTypes
            If appInfo IsNot Nothing AndAlso appInfo.ID IsNot Nothing Then
                Select Case appInfo.ID
                    Case MainframeGEN, MainframeHEE, MainframePCH, MainframePWT, MainframeIAC, MainframeCON, MainframeHAT, MainframeINF
                        Return MainframeTypes.HLLAPI
                    Case MainframeHUM, MainframeATM, MainframePSS, MainframeRUM, MainframeIBM, MainframeTMT, MainframeRMD, MainframeART, MainframeARN
                        Return MainframeTypes.Other
                End Select
            End If
            Return MainframeTypes.Unrecognised
        End Function

        ''' <summary>
        ''' Get a list of the supported application types. This is a fully populated
        ''' canonical list of all types supported. Additionally, the list and all
        ''' instances in it are freshly created, so can be modified and re-used by
        ''' the caller.
        ''' </summary>
        ''' <returns>A collection of clsApplicationTypeInfo objects, each detailing a
        ''' particular application type.</returns>
        Public Shared Function GetApplicationTypes(globalInfo As clsGlobalInfo) As List(Of clsApplicationTypeInfo)

            Dim all As New List(Of clsApplicationTypeInfo)
            Dim parameter As clsApplicationParameter
            Dim desc As String

            all.Add(GetWindowsApplicationType(Win32LaunchID, Win32AttachID))
            all.Add(GetJavaApplicationType(JavaLaunchID, JavaAttachID))
            all.Add(GetBrowserApplicationType(BrowserLaunchId, BrowserAttachId))

            'Browser-Automation, base application type
            Dim app = New clsApplicationTypeInfo(HTMLApplicationID, BrowserBasedApplicationInternetExplorer)
            app.Enabled = True

            'browser app for attaching to
            Dim subApp = New clsApplicationTypeInfo(HTMLAttachID, ABrowserWhichIsAlreadyRunning)
            subApp.Enabled = True
            subApp.ParentType = app
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "WindowTitle"
            parameter.FriendlyName = WindowTitle_1
            parameter.HelpText = EnterTheWindowTitleOfTheTargetApplicationTheWildcardsAreValid
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True 'Only one of window title or process name is required
            parameter.HelpReference = "helpApplicationParameters.htm#WindowTitle"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "ProcessName"
            parameter.FriendlyName = ProcessName_1
            parameter.HelpText = EnterTheWindowsProcessNameOfTheTargetApplication
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True 'Only one of window title or process name is required
            parameter.HelpReference = "helpApplicationParameters.htm#ProcessName"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = IfNotFoundForAttachingYourBrowserMayBeLaunchedAutomaticallyIfDesiredEnterThePat
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = ExecutableFilesExeExe
            parameter.Value = "C:\Program Files\Internet Explorer\iexplore.exe"
            parameter.HelpReference = "helpApplicationParameters.htm#ExecutablePath"
            parameter.AcceptNullValue = True
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "CommandLineParams"
            parameter.FriendlyName = CommandLineParams
            parameter.HelpText = PleaseEnterTheURLOfYourStartPageAndAnyOtherCommandLineParametersToBeUsedWhenThe
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#CommandLineParams"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "ActiveTabOnly"
            parameter.FriendlyName = ActiveTabOnly
            parameter.HelpText = OnlyInteractWithElementsOnTheActiveBrowserTab
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.Value = "True"
            parameter.LegacyValue = "False"
            parameter.AcceptNullValue = False
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Name = "UseJavaInBrowser"
            parameter.FriendlyName = UseJavaInBrowser
            parameter.Enabled = True
            parameter.HelpText = EnableJavaIntegrationTechniques
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.AcceptNullValue = False
            parameter.HelpReference = "helpApplicationParameters.htm#UseJavaInBrowser"
            parameter.Value = "False"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "ExcludeHTC"
            parameter.FriendlyName = ExcludeHTC
            parameter.HelpText = ExcludeHTCComponentsImprovesPerformance
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.Value = "True"
            parameter.LegacyValue = "False"
            parameter.AcceptNullValue = False
            subApp.Parameters.Add(parameter)
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            app.SubTypes.Add(subApp)

            'html sub application for launching directly
            subApp = New clsApplicationTypeInfo(HTMLLaunchID, ABrowserThatIsLaunchedFromAnExecutableFile)
            subApp.Enabled = True
            subApp.ParentType = app
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = PleaseEnterThePathToYourBrowser
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = ExecutableFilesExeExe
            parameter.Value = "C:\Program Files\Internet Explorer\iexplore.exe"
            parameter.HelpReference = "helpApplicationParameters.htm#ExecutablePath"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "CommandLineParams"
            parameter.FriendlyName = CommandLineParams
            parameter.HelpText = PleaseEnterTheURLOfYourStartPageAndAnyOtherCommandLineParameters
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#CommandLineParams"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "ActiveTabOnly"
            parameter.FriendlyName = ActiveTabOnly
            parameter.LegacyValue = "False"
            parameter.HelpText = OnlyInteractWithElementsOnTheActiveBrowserTab
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.Value = "True"
            parameter.AcceptNullValue = False
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Name = "UseJavaInBrowser"
            parameter.FriendlyName = UseJavaInBrowser
            parameter.Enabled = True
            parameter.HelpText = EnableJavaIntegrationTechniques
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.AcceptNullValue = False
            parameter.HelpReference = "helpApplicationParameters.htm#UseJavaInBrowser"
            parameter.Value = "False"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "ExcludeHTC"
            parameter.FriendlyName = ExcludeHTC
            parameter.HelpText = ExcludeHTCComponentsImprovesPerformance
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.Value = "True"
            parameter.LegacyValue = "False"
            parameter.AcceptNullValue = False
            subApp.Parameters.Add(parameter)
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            app.SubTypes.Add(subApp)

            all.Add(app)

            'Mainframe applications
            app = New clsApplicationTypeInfo("Mainframe", MainframeApplication)
            app.Enabled = True
            all.Add(app)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeGEN, GenericHLLAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "CodePage"
            parameter.FriendlyName = CodePage
            parameter.HelpText = APICodePage
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.Values = New List(Of String)
            parameter.FriendlyValues = New List(Of String)
            parameter.Values.Add("0")
            parameter.FriendlyValues.Add(LocaleVariant0)
            Dim encodings = From enc In System.Text.Encoding.GetEncodings Select enc Order By enc.DisplayName
            Dim defCodePage = System.Text.Encoding.Default.CodePage
            For Each enc In encodings
                Dim cp = enc.CodePage
                Dim val = String.Format("{0} [{1}]", enc.DisplayName, cp)
                parameter.Values.Add(cp)
                parameter.FriendlyValues.Add(val)
                If cp = defCodePage Then parameter.Value = cp
            Next
            parameter.LegacyValue = "20127" 'US-ASCII
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "DLL Name"
            parameter.FriendlyName = DLLName
            parameter.HelpText = DLLName
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.Value = "EHLAPI32.DLL"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "DLL Entry Point"
            parameter.FriendlyName = DLLEntryPoint
            parameter.HelpText = DLLEntryPoint
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.Value = "hllapi"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Calling Convention"
            parameter.FriendlyName = CallingConvention
            parameter.HelpText = CallingConvention
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.Values = New List(Of String)
            parameter.FriendlyValues = New List(Of String)
            parameter.Values.Add("C Declaration")
            parameter.FriendlyValues.Add(CDeclaration)
            parameter.Values.Add("Windows API")
            parameter.FriendlyValues.Add(WindowsAPI)
            parameter.Value = "C Declaration"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session Type"
            parameter.FriendlyName = SessionType
            parameter.HelpText = SessionType
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.Values = New List(Of String)
            parameter.FriendlyValues = New List(Of String)
            parameter.Values.Add("Standard")
            parameter.FriendlyValues.Add(Standard)
            parameter.Values.Add("Enhanced")
            parameter.FriendlyValues.Add(Enhanced)
            parameter.LegacyValue = "Standard"
            parameter.Value = "Enhanced"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeIAC, IBMIAccessEHLLAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeIBM, IBMPersonalCommunicatorCOMAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = PathToSessionFile
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = IBMMainframeSessionFilesWsWs
            parameter.HelpReference = "helpApplicationParameters.htm#SessionFile"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            app.SubTypes.Add(subApp)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "WaitSleepTime"
            parameter.FriendlyName = WaitSleepTime
            parameter.HelpText = PollingIntervalInMillisecondsDuringWaitOperations
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Number
            parameter.HelpReference = "helpApplicationParameters.htm#WaitSleepTime"
            parameter.AcceptNullValue = False
            parameter.Value = "250"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "WaitTimeout"
            parameter.FriendlyName = WaitTimeout
            parameter.HelpText = TimeoutIntervalInSecondsDuringWaitOperations
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Number
            parameter.HelpReference = "helpApplicationParameters.htm#WaitTimeout"
            parameter.AcceptNullValue = False
            parameter.Value = "30"
            subApp.Parameters.Add(parameter)
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframePCH, IBMPersonalCommunicatorHLLAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeATM, MicroFocusAttachmateExtraXTreme93ATMAPI32)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Attachmate Variant"
            parameter.FriendlyName = AttachmateVariant
            parameter.HelpText = PleaseChooseTheAppropriateVariantOfYourAttachmateEmulatorBelow
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#AttachmateVariant"
            parameter.Values = New List(Of String)
            parameter.FriendlyValues = New List(Of String)
            parameter.Values.Add("Extra!")
            parameter.FriendlyValues.Add(Extra)
            parameter.Values.Add("Rally!")
            parameter.FriendlyValues.Add(Rally)
            parameter.Values.Add("Kea!")
            parameter.FriendlyValues.Add(Kea)
            parameter.Values.Add("Kea! for HP")
            parameter.FriendlyValues.Add(KeaForHP)
            parameter.Values.Add("Irma")
            parameter.FriendlyValues.Add(Irma)
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeINF, MicroFocusInfoconnectWinHLLAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeART, MicroFocusReflection9XAndEarlierCOMAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = PathToSessionFile
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = AttachmateReflectionSettingsFilesR2wR2w
            parameter.HelpReference = HelpApplicationParametersHtmSessionFile
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeARN, MicroFocusReflectionForDesktop16NETAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = PathToSessionFile
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = AttachmateReflectionSettingsFilesRd3xRd5xRd3xRd5x
            parameter.HelpReference = "helpApplicationParameters.htm#SessionFile"
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            desc = NeoWareLaterWallDataRumbaTerminalEmulatorViaTheHLLAPIInterfaceWithVersion6Rumba
            subApp = New clsApplicationTypeInfo(MainframeRUM, MicroFocusRumbaEHLLAPI, desc)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeHUM, OpenTextHostExplorerFormerlyHummingbirdCOMAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = PathToSessionFile
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = HummingbirdMainframeSessionFilesHepHep
            parameter.HelpReference = "helpApplicationParameters.htm#SessionFile"
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeHEE, OpenTextHostExplorerFormerlyHummingbirdEHLLAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            desc = EricomPowerTermTerminalEmulatorViaStandardHLLAPIInterfaceVersion6IsNotSupported
            subApp = New clsApplicationTypeInfo(MainframePWT, EricomPowerTermInterConnectHLLAPI, desc)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            desc = PericomLaterHPTeemTalkTerminalEmulatorViaTheHLLAPIInterfaceVersion4OfTeemTalkDo
            subApp = New clsApplicationTypeInfo(MainframeTMT, HPTeemtalkEHLLAPI, desc)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeCON, NDLActiveConductorHLLAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeRMD, RMDHTTPXML)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Port"
            parameter.FriendlyName = Port
            parameter.HelpText = RMDInterfacePort
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Number
            parameter.HelpReference = "helpApplicationParameters.htm#Port"
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframePSS, RocketPASSPORTCOMAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = PathToSessionFile
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = PassportMainframeSessionFilesZwsZws
            parameter.HelpReference = "helpApplicationParameters.htm#SessionFile"
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            '## DISABLED #############################################################
            'New Mainframe Sub-type
            subApp = New clsApplicationTypeInfo(MainframeHAT, HostAccessWinHLLAPI)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Session ID"
            parameter.FriendlyName = SessionID
            parameter.HelpText = SessionIdentifier
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.List
            parameter.HelpReference = "helpApplicationParameters.htm#SessionID"
            parameter.Values = New List(Of String)
            For i As Integer = 0 To 25
                parameter.Values.Add(Chr(i + Asc("A")))
            Next
            subApp.Parameters.Add(parameter)
            subApp.ParentType = app
            subApp.Enabled = True
            parameter = GetProcessModeParameter()
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())

            'HostAccess (WinHLLAPI) is disabled for now.
            'a.SubTypes.Add(subApp)

            app = GetCitrixApplicationType()
            app.Enabled = True
            all.Add(app)

            Return all
        End Function

        Private Shared Function GetWindowsApplicationType(launchId As String, attachId As String, Optional parentApplicationType As clsApplicationTypeInfo = Nothing) As clsApplicationTypeInfo
            'Win32 applications
            Dim app = New clsApplicationTypeInfo(Win32ApplicationID, WindowsApplication)
            app.Enabled = True 'win32 always allowed, regardless of license
            app.ParentType = parentApplicationType

            Dim isCitrix = parentApplicationType?.ID = CitrixApplicationId

            'Windows sub-application for launching directly
            Dim subApp As New clsApplicationTypeInfo(launchId, MyApplicationIsLaunchedFromAnExecutableFile)
            subApp.Enabled = True 'win32 always allowed, regardless of license
            subApp.ParentType = app
            Dim parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = EnterThePathToTheApplicationExecutableOrUseTheBrowseButton
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.CheckFileExists = Not isCitrix
            parameter.FileExtensionFilter = ExecutableFilesExeExe
            parameter.HelpReference = "helpApplicationParameters.htm#ExecutablePath"
            subApp.Parameters.Add(parameter)

            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "CommandLineParams"
            parameter.FriendlyName = CommandLineParams
            parameter.HelpText = EnterTheCommandLineParametersIfAnyToBePassedToTheTargetApplication
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#CommandLineParams"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "WorkingDirectory"
            parameter.FriendlyName = WorkingDirectory
            parameter.HelpText = EnterTheWorkingDirectoryWhichTheTargetApplicationWillUse
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#WorkingDirectory"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "NonInvasive"
            parameter.FriendlyName = NonInvasive
            parameter.HelpText = RestrictToNonInvasiveAutomationTechniques
            parameter.HelpReference = "helpApplicationParameters.htm#NonInvasive"
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.Value = "True"
            parameter.AcceptNullValue = False
            subApp.Parameters.Add(parameter)
            parameter = GetProcessModeParameter(isCitrix)
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            'Windows sub-application for attaching to at runtime
            subApp = New clsApplicationTypeInfo(attachId, MyApplicationWillAlreadyBeRunningIWantToAttachToAnExistingInstance)
            subApp.Enabled = True 'win32 always allowed, regardless of license
            subApp.ParentType = app
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "WindowTitle"
            parameter.FriendlyName = WindowTitle_1
            parameter.HelpText = EnterTheWindowTitleOfTheTargetApplicationTheWildcardsAreValid
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True 'Only one of window title or process name is required
            parameter.HelpReference = "helpApplicationParameters.htm#WindowTitle"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "ProcessName"
            parameter.FriendlyName = ProcessName_1
            parameter.HelpText = EnterTheWindowsProcessNameOfTheTargetApplication
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True 'Only one of window title or process name is required
            parameter.HelpReference = "helpApplicationParameters.htm#ProcessName"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = IfNotFoundForAttachingYourApplicationMayBeLaunchedAutomaticallyIfDesiredEnterTh
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = ExecutableFilesExeExe
            parameter.HelpReference = "helpApplicationParameters.htm#ExecutablePath"
            parameter.CheckFileExists = Not isCitrix
            parameter.AcceptNullValue = True
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "CommandLineParams"
            parameter.FriendlyName = CommandLineParams
            parameter.HelpText = EnterTheCommandLineParametersIfAnyToBePassedWhenTheSpecifiedExecutableIsLaunched
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#CommandLineParams"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "NonInvasive"
            parameter.FriendlyName = NonInvasive
            parameter.HelpText = RestrictToNonInvasiveAutomationTechniques
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.Boolean
            parameter.Value = "True"
            parameter.AcceptNullValue = False
            subApp.Parameters.Add(parameter)
            parameter = GetProcessModeParameter(isCitrix)
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())
            app.SubTypes.Add(subApp)

            Return app
        End Function

        Private Shared Function GetJavaApplicationType(launchId As String, attachId As String, Optional parentApplicationType As clsApplicationTypeInfo = Nothing) As clsApplicationTypeInfo
            'Java applications
            Dim app As New clsApplicationTypeInfo(JavaApplicationID, JavaBasedApplication)
            app.Enabled = True
            app.ParentType = parentApplicationType

            Dim isCitrix = parentApplicationType?.ID = CitrixApplicationId

            'java sub application for launching directly
            Dim subApp = New clsApplicationTypeInfo(launchId, MyApplicationIsLaunchedFromAnExecutableFile)
            subApp.Enabled = True
            subApp.ParentType = app
            Dim parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = EnterThePathToTheApplicationSJARFileOrUseTheBrowseButton
            parameter.CheckFileExists = Not isCitrix
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = JavaApplicationsJarJar
            parameter.HelpReference = "helpApplicationParameters.htm#ExecutablePath"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "CommandLineParams"
            parameter.FriendlyName = CommandLineParams
            parameter.HelpText = EnterTheCommandLineParametersIfAnyToBePassedToTheTargetApplication
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#CommandLineParams"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "WorkingDirectory"
            parameter.FriendlyName = WorkingDirectory
            parameter.HelpText = EnterTheWorkingDirectoryWhichTheTargetApplicationWillUse
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#WorkingDirectory"
            subApp.Parameters.Add(parameter)
            parameter = GetProcessModeParameter(isCitrix)
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())

            app.SubTypes.Add(subApp)

            'java sub application for attaching to at runtime
            subApp = New clsApplicationTypeInfo(attachId, MyApplicationWillAlreadyBeRunningIWantToAttachToAnExistingInstance)
            subApp.Enabled = True
            subApp.ParentType = app
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "WindowTitle"
            parameter.FriendlyName = WindowTitle_1
            parameter.HelpText = EnterTheWindowTitleOfTheTargetApplicationTheWildcardsAreValid
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True 'Only one of window title or process name is required
            parameter.HelpReference = "helpApplicationParameters.htm#WindowTitle"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "ProcessName"
            parameter.FriendlyName = ProcessName_1
            parameter.HelpText = EnterTheWindowsProcessNameOfTheTargetApplication
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True 'Only one of window title or process name is required
            parameter.HelpReference = "helpApplicationParameters.htm#ProcessName"
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "Path"
            parameter.FriendlyName = MyResource.Path
            parameter.HelpText = IfNotFoundForAttachingYourApplicationMayBeLaunchedAutomaticallyIfDesiredEn_java
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.File
            parameter.FileExtensionFilter = JavaApplicationsJarJar
            parameter.HelpReference = "helpApplicationParameters.htm#ExecutablePath"
            parameter.CheckFileExists = Not isCitrix
            parameter.AcceptNullValue = True
            subApp.Parameters.Add(parameter)
            parameter = New clsApplicationParameter()
            parameter.Enabled = True
            parameter.Name = "CommandLineParams"
            parameter.FriendlyName = CommandLineParams
            parameter.HelpText = EnterTheCommandLineParametersIfAnyToBePassedWhenTheSpecifiedExecutableIsLaunched
            parameter.ParameterType = clsApplicationParameter.ParameterTypes.String
            parameter.AcceptNullValue = True
            parameter.HelpReference = "helpApplicationParameters.htm#CommandLineParams"
            subApp.Parameters.Add(parameter)
            parameter = GetProcessModeParameter(isCitrix)
            subApp.Parameters.Add(parameter)
            subApp.Parameters.Add(GetExternalProcessTimeoutParameter(parameter))
            subApp.Parameters.Add(GetOptionsParameter())

            app.SubTypes.Add(subApp)

            Return app
        End Function

        Private Shared Function GetCitrixApplicationType() As clsApplicationTypeInfo

            'Citrix applications
            Dim app As New clsApplicationTypeInfo(CitrixApplicationId, CitrixApplication)

            app.SubTypes.Add(GetWindowsApplicationType(CitrixLaunchID, CitrixAttachID, app))

            app.SubTypes.Add(GetJavaApplicationType(CitrixJavaLaunchID, CitrixJavaAttachID, app))

            app.SubTypes.Add(GetBrowserApplicationType(CitrixBrowserLaunchID, CitrixBrowserAttachID, app))

            Return app
        End Function

        Private Shared Function GetBrowserApplicationType(launchId As String, attachId As String, Optional parentApplicationType As clsApplicationTypeInfo = Nothing) As clsApplicationTypeInfo

            Dim app = New clsApplicationTypeInfo(BrowserApplicationId, BrowserBasedApplicationChromeFirefox)
            app.Enabled = True
            app.ParentType = parentApplicationType

            Dim isCitrix = parentApplicationType?.ID = CitrixApplicationId

            app.SubTypes.Add(GetBrowserApplicationAttachApp(app, attachId, isCitrix))
            app.SubTypes.Add(GetBrowserApplicationLaunchApp(app, launchId, isCitrix))

            Return app
        End Function

        Private Shared Function GetBrowserApplicationAttachApp(parentApp As clsApplicationTypeInfo, attachId As String, Optional isCitrix As Boolean = False) As clsApplicationTypeInfo

            Dim subApp = New clsApplicationTypeInfo(attachId, ABrowserWhichIsAlreadyRunning) With
            {
                .Enabled = True,
                .ParentType = parentApp
            }

            subApp.Parameters.Add(
            New clsApplicationParameter() With
            {
                .Enabled = True,
                .Name = "WindowTitle",
                .FriendlyName = WindowTitle_1,
                .HelpText = EnterTheTitleOfTheTargetPageTheWildcardsAreValidAWildcardWillAutomaticallyBeAdd,
                .ParameterType = clsApplicationParameter.ParameterTypes.String,
                .AcceptNullValue = False
            })

            subApp.Parameters.Add(
            New clsApplicationParameter() With
            {
                .Enabled = True,
                .Name = "Path",
                .FriendlyName = MyResource.Path,
                .HelpText = IfNotFoundForAttachingYourBrowserMayBeLaunchedAutomaticallyIfDesiredEnterThePat,
                .ParameterType = clsApplicationParameter.ParameterTypes.File,
                .FileExtensionFilter = ExecutableFilesExeExe,
                .Value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google\Chrome\Application\chrome.exe"),
                .CheckFileExists = Not isCitrix,
                .HelpReference = "helpApplicationParameters.htm#ExecutablePath",
                .AcceptNullValue = True
            })

            subApp.Parameters.Add(
            New clsApplicationParameter() With
            {
                .Enabled = True,
                .Name = "CommandLineParams",
                .FriendlyName = CommandLineParams,
                .HelpText = PleaseEnterTheURLOfYourStartPageAndAnyOtherCommandLineParametersToBeUsedWhenThe,
                .ParameterType = clsApplicationParameter.ParameterTypes.String,
                .HelpReference = "helpApplicationParameters.htm#CommandLineParams",
                .AcceptNullValue = True
            })
            subApp.Outputs.Add(
                New clsArgumentInfo("trackingid", TrackingId, "text", TrackingIdDescription, True)
                )

            GetProcessModeParameter(isCitrix).
            Tee(AddressOf subApp.Parameters.Add).
            Map(AddressOf GetExternalProcessTimeoutParameter).
            Tee(AddressOf subApp.Parameters.Add)

            Return subApp
        End Function

        Private Shared Function GetBrowserApplicationLaunchApp(parentApp As clsApplicationTypeInfo, launchId As String, Optional isCitrix As Boolean = False) As clsApplicationTypeInfo

            Dim subApp = New clsApplicationTypeInfo(launchId, ABrowserThatIsLaunchedFromAnExecutableFile) With
            {
                .Enabled = True,
                .ParentType = parentApp
            }

            subApp.Parameters.Add(
            New clsApplicationParameter() With
            {
                .Enabled = True,
                .Name = "Path",
                .FriendlyName = MyResource.Path,
                .HelpText = PleaseEnterThePathToYourBrowser,
                .ParameterType = clsApplicationParameter.ParameterTypes.File,
                .CheckFileExists = Not isCitrix,
                .FileExtensionFilter = ExecutableFilesExeExe,
                .Value = "",
                .HelpReference = "helpApplicationParameters.htm#ExecutablePath",
                .AcceptNullValue = True
            })

            subApp.Parameters.Add(
            New clsApplicationParameter() With
            {
                .Enabled = True,
                .Name = "CommandLineParams",
                .FriendlyName = CommandLineParams,
                .HelpText = PleaseEnterTheURLOfYourStartPageAndAnyOtherCommandLineParametersToBeUsedWhenThe,
                .ParameterType = clsApplicationParameter.ParameterTypes.String,
                .HelpReference = "helpApplicationParameters.htm#CommandLineParams",
                .AcceptNullValue = True
            })

            subApp.Outputs.Add(
                New clsArgumentInfo("trackingid", TrackingId, "text", TrackingIdDescription, True)
                )

            GetProcessModeParameter(isCitrix).
            Tee(AddressOf subApp.Parameters.Add).
            Map(AddressOf GetExternalProcessTimeoutParameter).
            Tee(AddressOf subApp.Parameters.Add)

            Return subApp

        End Function

        ''' <summary>
        ''' Get a clsApplicationParameter describing the 'ProcessMode' parameter.
        ''' This is encapsulated here because it's used by all application types.
        ''' </summary>
        ''' <returns>A new clsApplicationParameter instance.</returns>
        Private Shared Function GetProcessModeParameter(Optional isCitrix As Boolean = False) As clsApplicationParameter

            Dim p As New clsApplicationParameter()
            p.Enabled = True
            p.Name = "ProcessMode"
            p.FriendlyName = MyResource.ProcessMode
            p.HelpText = SelectTheApplicationManagerMode
            p.HelpReference = "helpApplicationParameters.htm#ProcessMode"
            p.ParameterType = clsApplicationParameter.ParameterTypes.List
            If isCitrix Then
                p.Value = ProcessMode.Citrix32.ToString()
            Else
                p.Value = ProcessMode.Internal.ToString()
            End If
            p.Values = New List(Of String)
            p.FriendlyValues = New List(Of String)
            If isCitrix Then
                p.Values.Add(ProcessMode.Citrix32.ToString())
                p.FriendlyValues.Add(ExternalCitrix32BitMode)
                p.Values.Add(ProcessMode.Citrix64.ToString())
                p.FriendlyValues.Add(ExternalCitrix64BitMode)
            Else
                p.Values.Add(ProcessMode.Internal.ToString())
                p.FriendlyValues.Add(EmbeddedDefault)
                p.Values.Add(ProcessMode.Ext32bit.ToString())
                p.FriendlyValues.Add(External32BitMode)
                p.Values.Add(ProcessMode.Ext64bit.ToString())
                p.FriendlyValues.Add(External64BitMode)
                p.Values.Add(ProcessMode.ExtNativeArch.ToString())
                p.FriendlyValues.Add(ExternalOSAddressSize)
                p.Values.Add(ProcessMode.ExtSameArch.ToString())
                p.FriendlyValues.Add(ExternalBluePrismAddressSize)
            End If
            p.AcceptNullValue = False
            Return p
        End Function

        ''' <summary>
        ''' Gets the external application manager timeout parameter.
        ''' </summary>
        ''' <param name="processModeParameter">
        ''' The application manager process mode parameter for the application. This is 
        ''' used to determine if the timeout parameter is enabled.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function GetExternalProcessTimeoutParameter(
        processModeParameter As clsApplicationParameter) As clsApplicationParameter

            Dim p As New clsApplicationParameter()
            'This parameter should only be enabled if the process mode parameter value is
            ' not 'Internal'.
            ' Every time p.Enabled is used to check whether this parameter is enabled, 
            ' this will check the value of the process mode parameter to determine this.
            p.UpdateEnabled = Function() processModeParameter.Value <> "Internal"
            p.Name = "ExternalProcessTimeout"
            p.FriendlyName = ExternalProcessTimeout
            p.HelpText = ProvideTheTimeThatTheObjectWillWaitForTheApplicationToRespondBeforeThrowingAnEx
            p.HelpReference = "helpApplicationParameters.htm#ExternalProcessTimeout"
            p.ParameterType = clsApplicationParameter.ParameterTypes.Timespan
            p.Value = 0
            p.AcceptNullValue = False
            Return p
        End Function
        ''' <summary>
        ''' Get a clsApplicationParameter describing the 'Options' parameter that is
        ''' used to specifiy a comma-separated list of options to various application
        ''' types to change behaviour in various ways. This is encapsulated here
        ''' because it's repeated in so many places.
        ''' </summary>
        ''' <returns>A new clsApplicationParameter instance.</returns>
        Private Shared Function GetOptionsParameter() As clsApplicationParameter

            Dim p As New clsApplicationParameter()
            p.Enabled = True
            p.Name = "Options"
            p.FriendlyName = Options
            p.HelpText = IfNecessaryEnterAnyOptionsSpecificToThisTargetApplicationAsDirectedByBluePrismT
            p.ParameterType = clsApplicationParameter.ParameterTypes.String
            p.AcceptNullValue = True
            p.HelpReference = "helpApplicationParameters.htm#Options"
            Return p
        End Function


        ''' <summary>
        ''' The identifier for this application type.
        ''' </summary>
        Public ReadOnly Property ID As String

        ''' <summary>
        ''' The name of this application type, which can be used for presentation
        ''' to the user. e.g. "Win32 Application", "Mainframe Application"
        ''' </summary>
        Public ReadOnly Property Name As String

        ''' <summary>
        ''' A description of the application type. This can be lengthy, including
        ''' detailed usage notes or other information as required. It will be
        ''' auto-extracted for use in documentation. Paragraph breaks can be included
        ''' by using two line breaks - i.e. VbCrLf x 2
        ''' </summary>
        Public ReadOnly Property Description As String

        Public ReadOnly Property Parameters As List(Of clsApplicationParameter)

        Public ReadOnly Property Outputs As List(Of clsArgumentInfo)

        ''' <summary>
        ''' The parent type of application. Relevant only when contained in another
        ''' applicationtype's subtypes collection. May be null.
        ''' </summary>
        Public Property ParentType As clsApplicationTypeInfo

        ''' <summary>
        ''' The root type of this application - this will get the application type which
        ''' is the root ancestor of this application type, returning this application
        ''' type if it has no parents (and is thus, by definition, a root type).
        ''' </summary>
        Public ReadOnly Property RootType As clsApplicationTypeInfo
            Get
                If ParentType Is Nothing Then Return Me
                Return ParentType.RootType
            End Get
        End Property

        ''' <summary>
        ''' A list of sub application types. E.g. Hummingbird is a subtype of Mainframe.
        ''' </summary>
        Public ReadOnly Property SubTypes As List(Of clsApplicationTypeInfo)

        ''' <summary>
        ''' Constructor, to be used only within AMI itself. Clients of AMI receive
        ''' instances of this class only by calling the relevant clsAMI methods.
        ''' </summary>
        ''' <param name="id">Identifier for the application type</param>
        ''' <param name="name">Name of the application type</param>
        ''' <param name="desc">Description of the application type. If Nothing is
        ''' supplied, the name is used. This is intended only for temporary backward
        ''' compatibility - ultimately all application types should have a proper
        ''' description. See also the documentation for the Description property.</param>
        Friend Sub New(ByVal id As String, ByVal name As String, Optional ByVal desc As String = Nothing)
            Me.ID = id
            Me.Name = name
            If desc Is Nothing Then
                Description = name
            Else
                Description = desc
            End If
            Me.Parameters = New List(Of clsApplicationParameter)
            Me.SubTypes = New List(Of clsApplicationTypeInfo)
            Me.Outputs = New List(Of clsArgumentInfo)
        End Sub

        ''' <summary>
        ''' Returns a deep clone, except for the parent, and subtype, which are shallow
        ''' clones.
        ''' </summary>
        ''' <returns>As summary.</returns>
        Public Function Clone() As clsApplicationTypeInfo
            Dim copy As New clsApplicationTypeInfo(Me.ID, Me.Name)
            For Each p As clsApplicationParameter In Me.Parameters
                copy.Parameters.Add(p.Clone)
            Next
            copy.ParentType = Me.ParentType
            copy.SubTypes.AddRange(Me.SubTypes)

            Return copy
        End Function

        ''' <summary>
        ''' Serialises the apptype info object to xml for storage. All that is actually
        ''' serialised is the type ID and the parameter values. This stored version is
        ''' intended to be restored to fully populated instance by the FromXML method.
        ''' </summary>
        ''' <param name="parentDocument">The parent document to use in order to create
        ''' the return value (an xml element).</param>
        ''' <returns>Returns a new xml element, representing the current apptype info
        ''' object. Root node is "apptypeinfo".</returns>
        Public Function ToXML(ByVal parentDocument As XmlDocument) As XmlElement

            Dim pElement As XmlElement = parentDocument.CreateElement("apptypeinfo")
            Dim cElement As XmlElement, c2Element As XmlElement, c3Element As XmlElement

            'Add ID
            cElement = parentDocument.CreateElement("id")
            cElement.InnerText = ID.ToString
            pElement.AppendChild(cElement)

            'Add parameters
            cElement = parentDocument.CreateElement("parameters")
            For Each p As clsApplicationParameter In Parameters
                c2Element = parentDocument.CreateElement("parameter")
                cElement.AppendChild(c2Element)
                c3Element = parentDocument.CreateElement("name")
                c3Element.InnerText = p.Name
                c2Element.AppendChild(c3Element)
                If p.Value IsNot Nothing Then
                    c3Element = parentDocument.CreateElement("value")
                    c3Element.InnerText = p.Value
                    c2Element.AppendChild(c3Element)
                End If
            Next
            pElement.AppendChild(cElement)

            Return pElement

        End Function


        ''' <summary>
        ''' Indicates whether the application type is to be made available or not,
        ''' perhaps according to licensing restrictions.
        ''' </summary>
        Public Property Enabled() As Boolean
            Get
                Return mbEnabled
            End Get
            Set(ByVal value As Boolean)
                mbEnabled = value
            End Set
        End Property
        Private mbEnabled As Boolean

        Public Property Hidden As Boolean

        ''' <summary>
        ''' Creates a new clsApplicationTypeInfo object, as represented by the supplied
        ''' xml element.
        ''' </summary>
        ''' <param name="Element">The xml element representing an application type info
        ''' object. Root node name is "apptypeinfo"</param>
        ''' <returns>Returns a new clsApplicationTypeInfo object, providing the supplied
        ''' xml element is sound, returns Nothing otherwise.</returns>
        ''' <remarks>Legacy processes may have all kinds of additional information in the
        ''' supplied XML (see bug #4907) but we are only interested in the application
        ''' type (ID) and the parameter values.</remarks>
        Public Shared Function FromXML(ByVal element As XmlElement) _
        As clsApplicationTypeInfo

            If element.Name <> "apptypeinfo" Then Return Nothing

            'Find the ID of the application type...
            Dim id As String = Nothing
            For Each child As XmlElement In element.ChildNodes
                If child.Name = "id" Then
                    id = child.InnerText
                    Exit For
                End If
            Next
            If id Is Nothing Then _
            Throw New InvalidOperationException(MissingApplicationTypeID)

            'Get a fresh clsApplicationTypeInfo of the required type...
            Dim appInfo As clsApplicationTypeInfo = FindMatchingApplicationType(GetApplicationTypes(New clsGlobalInfo), id)

            If appInfo Is Nothing Then _
            Throw New InvalidOperationException(
                String.Format(UnableToLoadUnrecognisedApplicationType0, id))

            'Set the value of the parameter to be the legacy value if this exists (the 
            'value will only be used if the xml doesn't contain the parameter). The 
            'legacy value is used for applications created before the
            'parameter existed, that need to have a different value than the default 
            'value.
            For Each p As clsApplicationParameter In appInfo.Parameters
                If p.LegacyValue <> "" Then p.Value = p.LegacyValue
            Next

            'Fill in the parameter values. Any that are missing will use the default 
            'value that was loaded with application types (or the legacy value if that 
            'has been set)
            For Each child As XmlElement In element.ChildNodes
                If child.Name = "parameters" Then
                    For Each c2 As XmlElement In child.ChildNodes
                        If c2.Name = "parameter" Then
                            Dim name As String = Nothing, value As String = Nothing
                            For Each c3 As XmlElement In c2.ChildNodes
                                If c3.Name = "value" Then
                                    value = c3.InnerText
                                ElseIf c3.Name = "name" Then
                                    name = c3.InnerText
                                End If
                            Next
                            If name IsNot Nothing Then
                                For Each p As clsApplicationParameter In appInfo.Parameters
                                    If p.Name = name Then
                                        p.Value = value
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If
            Next

            Return appInfo

        End Function

        Private Shared Function FindMatchingApplicationType(appTypes As ICollection(Of clsApplicationTypeInfo), appId As String) As clsApplicationTypeInfo
            For Each appType In appTypes
                If appType.ID = appId Then
                    Return appType
                Else
                    Dim matchingType = FindMatchingApplicationType(appType.SubTypes, appId)
                    If matchingType IsNot Nothing Then Return matchingType
                End If
            Next
            Return Nothing
        End Function


        ''' <summary>
        ''' The String representation is the Name, allowing it to be used in combo boxes.
        ''' </summary>
        Public Overrides Function ToString() As String
            Return Name
        End Function

        ''' <summary>
        ''' Checks if this application type is equal to the given object. It is
        ''' considered equal if it is an application type with the same ID as this one.
        ''' </summary>
        ''' <param name="obj">The object to check for equality against.</param>
        ''' <returns>True if the given object is a non-null application type info object
        ''' with the same ID as this one; False otherwise.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim appType As clsApplicationTypeInfo = TryCast(obj, clsApplicationTypeInfo)
            Return appType IsNot Nothing AndAlso appType.ID = ID
        End Function

        ''' <summary>
        ''' Gets an integer hash based on this object. The hash is based on the ID only,
        ''' as this is the only member used for equality checks for this type.
        ''' </summary>
        ''' <returns>An integer hash to use for this object.</returns>
        Public Overrides Function GetHashCode() As Integer
            If ID = "" Then Return 0
            Return ID.GetHashCode() Xor 7714
        End Function

    End Class
End Namespace
