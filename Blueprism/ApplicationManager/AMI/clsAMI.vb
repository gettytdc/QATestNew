
Option Strict On

Imports System.Reflection
Imports System.Collections.Generic
Imports System.Xml
Imports System.IO
Imports System.Drawing
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports BluePrism.ApplicationManager
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.CharMatching
Imports AppTypes = BluePrism.ApplicationManager.AMI.clsElementTypeInfo.AppTypes
Imports ParamType = BluePrism.ApplicationManager.AMI.clsApplicationParameter.ParameterTypes
Imports ParamNames =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.ParameterNames
Imports BluePrism.Core.Xml
Imports System.Linq
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UIAutomation
Imports LocaleTools
Imports BluePrism.ApplicationManager.AMI
Imports NLog

Namespace BluePrism.AMI

    ''' Project  : AMI
    ''' Class    : clsAMI
    ''' 
    ''' <summary>
    ''' The clsAMI class provides the only point of contact with Application Manager
    ''' for all Automate modules that use it. Its purpose is to supply information
    ''' needed by the GUI and provide transparent translation to the internal
    ''' interfaces while completely hiding the details of the underlying implementation.
    ''' 
    ''' Scope/Visibility
    ''' 
    ''' * Automate and Automate Process Core are aware ONLY of AMI, and nothing
    '''   else within Application Manager
    ''' * Nothing else in Application Manager is aware of the existence of AMI.
    ''' 
    ''' </summary>
    Public Class clsAMI
        Implements IDisposable

        ''' <summary>
        ''' The ID which signifies a 'write' action - this is a hardcoded special
        ''' case action which is not registered within AMI proper, but is used by
        ''' write stages in APC.
        ''' </summary>
        Public Const WriteActionID As String = "Write"

        Private Shared mFontStore As IFontStore

        Private ReadOnly mLogger As Logger = LogManager.GetCurrentClassLogger()

        Private Shared ReadOnly BrowserAppTypes As New List(Of String)({clsApplicationTypeInfo.BrowserAttachId, clsApplicationTypeInfo.BrowserLaunchId,
                                                                 clsApplicationTypeInfo.CitrixBrowserLaunchID, clsApplicationTypeInfo.CitrixBrowserAttachID})

        ''' <summary>
        ''' Set an IFontLoader than can be used by Application Manager to retrieve
        ''' font definitions. This IFontLoader may be ignored if Application Manager
        ''' is configured to read fonts from local files.
        ''' </summary>
        ''' <param name="store">The IFontLoader to use.</param>
        Public Shared Sub SetFontLoader(ByVal store As IFontStore)
            mFontStore = store
            FontReader.SetFontStore(store)
        End Sub


        ''' <summary>
        ''' Get the definition of the specified font.
        ''' </summary>
        ''' <param name="name">The name of the font.</param>
        ''' <returns>The font's XML definition, or Nothing if the font doesn't exist.
        ''' </returns>
        Public Shared Function GetFont(ByVal name As String) As String
            If mFontStore Is Nothing Then Return Nothing
            Dim f As BPFont = mFontStore.GetFont(name)
            If f Is Nothing Then Return Nothing
            Return f.Data.GetXML()
        End Function


        Public Const MessageDelimiter As String = "|||"

        ''' <summary>
        ''' The id of the identifier containing screenshot information for an
        ''' element after a Win32Region spy. This will also match the name of
        ''' the corresponding attribute within an application model element.
        ''' </summary>
        Public Const ScreenshotIdentifierId As String = "Screenshot"

        Public Enum ApplicationStatus
            ''' <summary>
            ''' The application is running.
            ''' </summary>
            Launched
            ''' <summary>
            ''' The application has not yet been launched,
            ''' or it has since been closed.
            ''' </summary>
            NotLaunched
        End Enum

        ''' <summary>
        ''' Event raised when an application is closed,
        ''' eg by the user.
        ''' </summary>
        ''' <param name="appInfo">Info about the application
        ''' which has closed.</param>
        Public Event ApplicationStatusChanged(ByVal appInfo As clsApplicationTypeInfo, ByVal status As ApplicationStatus)


        ''' <summary>
        ''' The target application for this instance, or Nothing if not yet connected.
        ''' </summary>
        Private WithEvents mTargetApp As clsTargetApp


        ''' <summary>
        ''' Get the PID of the target application we're currently connected to.
        ''' </summary>
        ''' <returns>The PID, or 0 if there is no connection.</returns>
        Public ReadOnly Property TargetPID As Integer
            Get
                If mTargetApp Is Nothing Then Return 0
                Return mTargetApp.PID
            End Get
        End Property


        ''' <summary>
        ''' Get the current target application interface. For use only by internal
        ''' (to Application Manager) diagnostics tools.
        ''' </summary>
        ''' <returns>The clsTargetApp instance, or Nothing if not connected.</returns>
        <CLSCompliant(False)>
        Public Function GetTargetApp() As clsTargetApp
            Return mTargetApp
        End Function

        ''' <summary>
        ''' Global information that can be used to configure settings of the 
        ''' application modeller interface
        ''' </summary>
        Private mGlobalInfo As clsGlobalInfo

        ''' <summary>
        ''' Information about the application type we're currently running, or
        ''' intending to run. Must be set using SetTargetAppInfo() before attempting
        ''' to launch the application.
        ''' </summary>
        Private mTargetAppInfo As clsApplicationTypeInfo

        ''' <summary>
        ''' A dictionary (keyed by ID) of all the action types we support. We build
        ''' this in the clsAMI constructor and it remains as a static reference
        ''' throughout the lifetime of the object.
        ''' </summary>
        Private Shared mActionTypes As Dictionary(Of String, clsActionTypeInfo)

        ''' <summary>
        ''' A dictionary (keyed by ID) of all the action types we support. We build
        ''' this in the clsAMI constructor and it remains as a static reference
        ''' throughout the lifetime of the object.
        ''' </summary>
        Private Shared mConditionTypes As Dictionary(Of String, clsConditionTypeInfo)

        ''' <summary>
        ''' A dictionary (keyed by ID) of all the element types we support. We build
        ''' this in the clsAMI constructor and it remains as a static reference
        ''' throughout the lifetime of the object.
        ''' </summary>
        Private Shared mElementTypes As Dictionary(Of String, clsElementTypeInfo)


        ''' <summary>
        ''' A dictionary (keyed by ID) of all the Identifiers we support. We build
        ''' this in the clsAMI constructor and it remains as a static reference
        ''' throughout the lifetime of the object.
        ''' </summary>
        Private Shared mIdentifiers As Dictionary(Of String, clsIdentifierInfo)

        ''' <summary>
        ''' A list of errors found when parsing external data. In the course of
        ''' processing it, items that cannot be dealt with are dropped, but a
        ''' record of the error is added to this list for use by validators etc.
        ''' </summary>
        Public Shared ReadOnly Property ExtDataErrors() As List(Of String)
            Get
                Return mExtDataErrors
            End Get
        End Property
        Private Shared mExtDataErrors As New List(Of String)

        ''' <summary>
        ''' Shared constructor for initialising static data.
        ''' </summary>
        Shared Sub New()
            StaticBuildTypes()
        End Sub

        Public Shared Sub StaticBuildTypes()

            'Load external definitions...
            Dim extdata As New List(Of XmlDocument)
            Dim datapath As String = Path.GetDirectoryName(GetType(clsAMI).Assembly.Location)
            Dim xml As XmlDocument
            For Each docname As String In New String() {"SAPElements.xml", "Actions.xml"}
                Try
                    xml = New ReadableXmlDocument()
                    xml.Load(Path.Combine(datapath, docname))
                    extdata.Add(xml)
                Catch ex As Exception
                    mExtDataErrors.Add(String.Format(My.Resources.CouldNotLoad01, docname, ex.Message))
                End Try
            Next

            BuildActionTypes(extdata)
            BuildConditionTypes()
            BuildElementTypes(extdata)
            BuildIdentifierInfo()
        End Sub

        ''' <summary>
        ''' The required attributes typically needed for a list region.
        ''' </summary>
        Private Shared sReqdAttrsListRegion As ICollection(Of String) =
         GetReadOnly.ICollection(New String() {"ListDirection", "Padding"})

        ''' <summary>
        ''' The required attributes typically needed for a grid region
        ''' </summary>
        Private Shared sReqdAttrsGridRegion As ICollection(Of String) =
         GetReadOnly.ICollection(New String() {"GridSchema"})

        ''' <summary>
        ''' The required attributes typically needed for a character matching
        ''' operation
        ''' </summary>
        Private Shared sReqdAttrsCharMatching As ICollection(Of String) =
         GetReadOnly.ICollection(New String() {"FontName"})

        ''' <summary>
        ''' Adds an action with the given arguments
        ''' </summary>
        ''' <param name="id">The ID of the action</param>
        ''' <param name="name">The (display) name of the action</param>
        ''' <param name="helptext">A description of the action</param>
        ''' <param name="requiresFocus">True to ensure the target element gets
        ''' focus before the action is performed; False to indicate that this is
        ''' not required.</param>
        ''' <param name="args">The argument descriptors for the action</param>
        ''' <exception cref="ArgumentException">If the given action ID already exists
        ''' in the registered action types</exception>
        Private Shared Function AddAction(
         ByVal id As String, ByVal name As String, ByVal helptext As String,
         ByVal requiresFocus As Boolean, ByVal ParamArray args() As clsArgumentInfo) _
         As clsActionTypeInfo
            Return AddAction(id, Nothing, name, helptext, requiresFocus,
             "", GetEmpty.ICollection(Of String), args)
        End Function

        ''' <summary>
        ''' Adds an action with the given arguments
        ''' </summary>
        ''' <param name="id">The ID of the action</param>
        ''' <param name="name">The (display) name of the action</param>
        ''' <param name="helptext">A description of the action</param>
        ''' <param name="requiresFocus">True to ensure the target element gets
        ''' focus before the action is performed; False to indicate that this is
        ''' not required.</param>
        ''' <param name="returnDataType">The type of data returned by the action - an
        ''' empty string indicates that no data is returned.</param>
        ''' <param name="args">The argument descriptors for the action</param>
        ''' <exception cref="ArgumentException">If the given action ID already exists
        ''' in the registered action types</exception>
        Private Shared Function AddAction(
         ByVal id As String, ByVal name As String, ByVal helptext As String,
         ByVal requiresFocus As Boolean, ByVal returnDataType As String,
         ByVal ParamArray args() As clsArgumentInfo) As clsActionTypeInfo
            Return AddAction(id, Nothing, name, helptext, requiresFocus,
             returnDataType, GetEmpty.ICollection(Of String), args)
        End Function

        ''' <summary>
        ''' Adds an action with the given arguments
        ''' </summary>
        ''' <param name="id">The ID of the action</param>
        ''' <param name="name">The (display) name of the action</param>
        ''' <param name="helptext">A description of the action</param>
        ''' <param name="requiresFocus">True to ensure the target element gets
        ''' focus before the action is performed; False to indicate that this is
        ''' not required.</param>
        ''' <param name="returnDataType">The type of data returned by the action - an
        ''' empty string indicates that no data is returned.</param>
        ''' <param name="args">The argument descriptors for the action</param>
        ''' <exception cref="ArgumentException">If the given action ID already exists
        ''' in the registered action types</exception>
        Private Shared Function AddAction(
         ByVal id As String, ByVal name As String, ByVal helptext As String,
         ByVal requiresFocus As Boolean, ByVal returnDataType As String,
         ByVal reqdAttributes As ICollection(Of String),
         ByVal ParamArray args() As clsArgumentInfo) As clsActionTypeInfo
            Return AddAction(id, Nothing, name, helptext, requiresFocus,
             returnDataType, reqdAttributes, args)
        End Function

        ''' <summary>
        ''' Adds an action with the given arguments
        ''' </summary>
        ''' <param name="id">The ID of the action</param>
        ''' <param name="name">The (display) name of the action</param>
        ''' <param name="helptext">A description of the action</param>
        ''' <param name="requiresFocus">True to ensure the target element gets
        ''' focus before the action is performed; False to indicate that this is
        ''' not required.</param>
        ''' <param name="returnDataType">The type of data returned by the action - an
        ''' empty string indicates that no data is returned.</param>
        ''' <param name="args">The argument descriptors for the action</param>
        ''' <exception cref="ArgumentException">If the given action ID already exists
        ''' in the registered action types</exception>
        Private Shared Function AddAction(
         ByVal id As String, ByVal commandId As String, ByVal name As String,
         ByVal helptext As String, ByVal requiresFocus As Boolean,
         ByVal returnDataType As String, ByVal reqdAttributes As ICollection(Of String),
         ByVal ParamArray args() As clsArgumentInfo) As clsActionTypeInfo

            Dim action As New clsActionTypeInfo(
             id, commandId, name, helptext, requiresFocus, returnDataType,
             reqdAttributes, args)
            mActionTypes.Add(id, action)
            Return action

        End Function

        ''' <summary>
        ''' Adds region actions based on the given action - it adds one for each
        ''' type of region, ie. 'basic' region, a list region and a grid region
        ''' </summary>
        ''' <param name="idFormat">The format string for the action ID. Each region
        ''' type creates a new action with a function of this format string - Basic
        ''' regions replace the {0} placeholder with an empty string, List regions
        ''' replace it with the word "List", Grid reginos replace it with the word
        ''' "Grid".</param>
        ''' <param name="name">The display name of the action - this is the same
        ''' across each action type.</param>
        ''' <param name="helptext">The help text describing the action</param>
        ''' <param name="args">The base arguments to use for the action - list
        ''' actions get an additional argument: 'Element Number', and grid actions
        ''' get two extra : 'Column Number' and 'Row Number'.</param>
        ''' <exception cref="ArgumentException">If the given action ID or any of the
        ''' derived IDs already exist in the registered action types</exception>
        Private Shared Sub AddRegionActions(
         ByVal idFormat As String, ByVal name As String, ByVal helptext As String,
         ByVal ParamArray args() As clsArgumentInfo)
            AddRegionActions(idFormat, name, helptext, "",
             GetEmpty.ICollection(Of String), args)
        End Sub

        ''' <summary>
        ''' Adds region actions based on the given action - it adds one for each
        ''' type of region, ie. 'basic' region, a list region and a grid region
        ''' </summary>
        ''' <param name="idFormat">The format string for the action ID. Each region
        ''' type creates a new action with a function of this format string - Basic
        ''' regions replace the {0} placeholder with an empty string, List regions
        ''' replace it with the word "List", Grid reginos replace it with the word
        ''' "Grid".</param>
        ''' <param name="name">The display name of the action - this is the same
        ''' across each action type.</param>
        ''' <param name="helptext">The help text describing the action</param>
        ''' <param name="returnDataType">The return data type of the action.</param>
        ''' <param name="args">The base arguments to use for the action - list
        ''' actions get an additional argument: 'Element Number', and grid actions
        ''' get two extra : 'Column Number' and 'Row Number'.</param>
        ''' <exception cref="ArgumentException">If the given action ID or any of the
        ''' derived IDs already exist in the registered action types</exception>
        Private Shared Sub AddRegionActions(
         ByVal idFormat As String, ByVal name As String, ByVal helptext As String,
         ByVal returnDataType As String, ByVal ParamArray args() As clsArgumentInfo)
            AddRegionActions(idFormat, name, helptext, returnDataType,
             GetEmpty.ICollection(Of String), args)
        End Sub

        ''' <summary>
        ''' Adds region actions based on the given action - it adds one for each
        ''' type of region, ie. 'basic' region, a list region and a grid region
        ''' </summary>
        ''' <param name="idFormat">The format string for the action ID. Each region
        ''' type creates a new action with a function of this format string - Basic
        ''' regions replace the {0} placeholder with an empty string, List regions
        ''' replace it with the word "List", Grid reginos replace it with the word
        ''' "Grid".</param>
        ''' <param name="name">The display name of the action - this is the same
        ''' across each action type.</param>
        ''' <param name="helptext">The help text describing the action</param>
        ''' <param name="returnDataType">The return data type of the action.</param>
        ''' <param name="args">The base arguments to use for the action - list
        ''' actions get an additional argument: 'Element Number', and grid actions
        ''' get two extra : 'Column Number' and 'Row Number'.</param>
        ''' <exception cref="ArgumentException">If the given action ID or any of the
        ''' derived IDs already exist in the registered action types</exception>
        ''' <remarks>All of the actions generated by this method will use the same
        ''' command action - that of the basic region action - eg. adding actions
        ''' with an idFormat of "{0}DoThing" will create "DoThing", "ListDoThing"
        ''' and "GridDoThing" actions, all of which have a command ID indicating that
        ''' 'DoThing' is the command passed in the query into client comms.</remarks>
        Private Shared Sub AddRegionActions(
         ByVal idFormat As String, ByVal name As String, ByVal helptext As String,
         ByVal returnDataType As String,
         ByVal reqdAttributes As ICollection(Of String),
         ByVal ParamArray args() As clsArgumentInfo)


            ' Basic region. Note that the basic region command is called for all
            ' actions which act across all region types (ie. any action which are
            ' added via this method)
            Dim basicId As String = String.Format(idFormat, "")
            AddAction(
             basicId, name, helptext, True, returnDataType, reqdAttributes, args)

            ' List region
            Dim listId As String = String.Format(idFormat, "List")
            ' Insert 'Element Number' as the first arg for a list region
            Dim listArgs(args.Length) As clsArgumentInfo
            listArgs(0) = New clsArgumentInfo(
             "ElementNumber", My.Resources.ElementNumber, "number", My.Resources.The1basedNumberIndicatingWhichElementOfTheListRegionToActOnDefaultIs1,
             True)

            ' Copy the remaining arguments (clsArgumentInfo is semantically immutable
            ' so it is safe to use the same references)
            Array.Copy(args, 0, listArgs, 1, args.Length)

            AddAction(listId, basicId, name, helptext, True, returnDataType,
             CollectionUtil.MergeSet(reqdAttributes, sReqdAttrsListRegion), listArgs)

            ' action.CommandID = basicId

            ' Grid region
            Dim gridId As String = String.Format(idFormat, "Grid")
            Dim gridArgs(args.Length + 1) As clsArgumentInfo
            gridArgs(0) = New clsArgumentInfo("ColumnNumber", My.Resources.ColumnNumber,
             "number", My.Resources.The1basedColumnNumberIndicatingWhichColumnOfTheGridRegionToActOnDefaultIs1, False)
            gridArgs(1) = New clsArgumentInfo("RowNumber", My.Resources.RowNumber, "number",
             My.Resources.The1basedRowNumberIndicatingWhichRowOfTheGridRegionToActOnDefaultIs1, False)
            Array.Copy(args, 0, gridArgs, 2, args.Length)

            AddAction(gridId, name, helptext, True, returnDataType,
             CollectionUtil.MergeSet(reqdAttributes, sReqdAttrsGridRegion), gridArgs)

        End Sub


        ''' <summary>
        ''' Builds the map of action types availabe in AMI
        ''' </summary>
        ''' <param name="extdata">A list of XML documentats that contain external
        ''' element type definitions, to be used in addition to the ones hard-coded
        ''' here.</param>
        Private Shared Sub BuildActionTypes(ByVal extdata As List(Of XmlDocument))
            'Build the complete list of action types...
            mActionTypes = New Dictionary(Of String, clsActionTypeInfo)
                      
            'In the following group, pay attention to the fact that the things we refer
            'to as identifiers internally are called "Attributes" in the Application
            'Modeller user interface, so documentation and friendly names are set
            'accordingly. (see bug #6883)
            AddAction("GetWindowIdentifier", My.Resources.GetWindowAttribute, My.Resources.GetTheCurrentValueOfAWindowAttribute, False, "text",
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToGet, False))
            AddAction("GetAAIdentifier", My.Resources.GetAAAttribute, My.Resources.GetTheCurrentValueOfAnActiveAccessibilityAttribute, False, "text",
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToGet, False))
            AddAction("GetHTMLIdentifier", My.Resources.GetHTMLAttribute, My.Resources.GetTheCurrentValueOfAnHTMLAttribute, False, "text",
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToGet, False))
            AddAction("GetJABIdentifier", My.Resources.GetJABAttribute, My.Resources.GetTheCurrentValueOfAJavaAccessBridgeAttribute, False, "text",
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToGet, False))
            AddAction("GetUIAIdentifier", My.Resources.GetUIAAttribute, My.Resources.GetTheCurrentValueOfAUIAutomationAttribute, False, "text",
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToGet, False))

            With AddAction("GetElementBounds", My.Resources.GetBounds,
             My.Resources.GetsInformationAboutTheBoundingRectangleOfTheElementInCoordinatesRelativeToThePa, False, "collection")
                ' 'Get Bounds' is deprecated for Win32 elements
                ' We know this won't be a region because regions have their own
                ' actions for getting bounds and thus don't use this action
                .DeprecatedChecker = Function(e) e.AppType = AppTypes.Win32
            End With

            AddAction("GetRelativeElementBounds", My.Resources.GetRelativeBounds,
             My.Resources.GetsInformationAboutTheBoundingRectangleOfTheElementInCoordinatesRelativeToThePa, False, "collection")

            AddAction("GetElementScreenBounds", My.Resources.GetScreenBounds,
             My.Resources.GetsInformationAboutTheBoundingRectangleOfTheElementInScreenCoordinates, False, "collection")

            AddRegionActions("{0}RegionGetElementBounds", My.Resources.GetBounds,
             My.Resources.GetsInformationAboutTheBoundingRectangleOfTheElementInCoordinatesRelativeToThePa, "collection")
            AddRegionActions("{0}RegionGetElementScreenBounds", My.Resources.GetScreenBounds,
             My.Resources.GetsInformationAboutTheBoundingRectangleOfTheElementInScreenCoordinates, "collection")

            AddAction("ClickWindow", My.Resources.ClickWindow, My.Resources.SendAClickMessageToTheWindowAtTheSpecifiedPositionThePositionIsRelativeToTheTopL, False,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text", My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("ClickWindowCentre", My.Resources.ClickWindowCentre, My.Resources.SendAClickMessageToTheCentreOfTheWindow, False,
            New clsArgumentInfo("newtext", My.Resources.MouseButton, "text", My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("MouseClick", My.Resources.GlobalMouseClick, My.Resources.ClickTheElementAtTheSpecifiedPositionUsingAGlobalMouseClickThePositionIsRelative, True,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text", My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("MouseDoubleClick", My.Resources.GlobalDoubleMouseClick, My.Resources.DoubleClickTheElementAtTheSpecifiedPositionUsingAGlobalMouseClickThePositionIsRe, True,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text", My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("MouseClickCentre", My.Resources.GlobalMouseClickCentre, My.Resources.ClickTheElementAtItsCentreUsingAGlobalMouseClick, True,
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text", My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("MouseDoubleClickCentre", My.Resources.GlobalDoubleMouseClickCentre, My.Resources.DoubleClickTheElementAtItsCentreUsingAGlobalMouseClick, True,
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text", My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddRegionActions("{0}RegionMouseClick", My.Resources.GlobalMouseClick,
             My.Resources.ClickTheRegionAtTheSpecifiedPositionUsingAGlobalMouseClickThePositionIsRelativeT,
             New clsArgumentInfo("targx", My.Resources.X, "number",
              My.Resources.TheXCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number",
              My.Resources.TheYCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text",
              My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddRegionActions("{0}RegionMouseClickCentre", My.Resources.GlobalMouseClickCentre,
             My.Resources.ClickTheRegionAtItsCentreUsingAGlobalMouseClickTheMouseCursorWillAppearToJumpAcr,
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text",
              My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddRegionActions("{0}RegionParentClickCentre", My.Resources.ClickCentre,
             My.Resources.ClickTheParentWindowOfTheRegionAtTheCentreOfTheRegion)

            AddRegionActions("{0}RegionStartDrag", My.Resources.StartDrag,
             My.Resources.StartADragdropOperationByDraggingFromTheCentreOfTheRegionMustBeFollowedByADropOp)
            AddRegionActions("{0}RegionDropOnto", My.Resources.DropOnto,
             My.Resources.EndADragdropOperationByDroppingOntoTheRegionAtItsCentreMustBePrecededByADragOper)

            AddAction("Drag", My.Resources.Drag, My.Resources.StartDraggingFromAGivenPositionOverTheWindow, True,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfThePointAtWhichToStartDraggingRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfThePointAtWhichToStartDraggingRelativeToTheWindow, False))

            AddAction("Drop", My.Resources.Drop, My.Resources.DropAtTheGivenPositionOverTheWindowMustFollowADrag, True,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfThePointAtWhichToPerformTheDropRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfThePointAtWhichToPerformTheDropRelativeToTheWindow, False))

            AddAction("DragItem", My.Resources.DragListviewItem, My.Resources.StartDraggingTheListviewItemWithTheSpecifiedText, True,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheListviewItemToBeDraggedIfNotUniqueThenTheFirstMatchingItemFoundWillB, False))

            AddAction("DropOntoItem", My.Resources.DropOntoListviewItem, My.Resources.DropOntoTheListviewItemWithTheSpecifiedTextMustFollowADrag, True,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheListviewItemOntoWhichTheDropShouldBePerformedIfNotUniqueThenTheFirst, False))

            AddAction("CloseWindow", My.Resources.Close, My.Resources.AsksAWindowToClose, False)
            AddAction("HTMLFocus", My.Resources.Focus, My.Resources.FocusesAnHTMLElement, False)
            AddAction("HTMLHover", My.Resources.Hover, My.Resources.HoversOverAnHTMLElement, False)
            'Note: pressing a button only seems to require focus for .NET-based apps??
            AddAction("Press", My.Resources.Press, My.Resources.PressAButton, True)

            AddAction("SetField", My.Resources.SetField, My.Resources.SetTheContentsOfATerminalField, False,
             New clsArgumentInfo("newtext", My.Resources.Text, "text", My.Resources.TheNewTextToBeEnteredIntoTheField, False))

            AddAction("GetField", My.Resources.ReadField, My.Resources.ReadTheContentsOfATerminalField, False, "text")

            AddAction("SearchTerminal", My.Resources.SearchTerminal, My.Resources.SearchTheWholeTerminalForInstancesOfTheGivenTextCaseInsensitive, False, "Collection",
             New clsArgumentInfo("newtext", My.Resources.Text, "text", My.Resources.TheTextToSearchFor, False))

            AddAction("GetMainframeCursorPos", My.Resources.GetCursorPosition, My.Resources.GetsTheRowcolumnPositionOfTheCursorInThePresentationSpace, False, "Collection")


            AddAction("SetMainframeCursorPos", My.Resources.SetCursorPosition, My.Resources.SetsTheRowcolumnPositionOfTheCursorInThePresentationSpace, False,
             New clsArgumentInfo("TargY", My.Resources.RowIndex, "number", My.Resources.The1basedIndexOfTheRowAtWhichTheCursorShouldBeLocatedThisValueMustNotExceedTheNu, False),
             New clsArgumentInfo("TargX", My.Resources.ColumnIndex, "number", My.Resources.The1basedIndexOfTheColumnAtWhichTheCursorShouldBeLocatedThisValueMustNotExceedTh, False))

            AddAction("GetMainframeParentWindowTitle", My.Resources.GetWindowTitle, My.Resources.GetsTheTitleOfTheWindowInWhichTheTerminalEmulatorResides, False, "text")

            AddAction("SetMainframeParentWindowTitle", My.Resources.SetWindowTitle, My.Resources.SetsTheTitleOfTheWindowInWhichTheTerminalEmulatorResides, False,
             New clsArgumentInfo("newtext", My.Resources.NewTitle, "text", My.Resources.TheNewTitleOfTheMainframeEmulatorWindow, False))

            AddAction("RunMainframeMacro", My.Resources.RunMacro, My.Resources.RunsTheSpecifiedMacroDefinedOnTheEmulatorInstance, False,
             New clsArgumentInfo("newtext", My.Resources.MacroName, "text", My.Resources.TheNameOfTheMacroToBeRunOnlyFilePathsAreNotSupported, False))

            AddAction("SetText", My.Resources.SetText, My.Resources.SetTheWindowtextForAWindow, False,
             New clsArgumentInfo("newtext", My.Resources.Text, "text", My.Resources.TheNewTextToBeSentToTheWindow, False))

            ' Region based actions - anything that reads text has an implicit
            ' requirement to include the char matching attributes - ie. "FontName"
            ' List and grid regions have further requirements - see AddRegionActions

            AddRegionActions("{0}ReadTextOCR", My.Resources.ReadTextWithOCR,
             My.Resources.ReadTextUsingOCRFromARectangularAreaOnAWindow,
             "text",
             New clsArgumentInfo("language", My.Resources.Language, "text",
              My.Resources.TheTesseractLanguageCodeToUseDefaultIsEng, True),
             New clsArgumentInfo("pagesegmode", My.Resources.PageSegmentationMode, "text",
              My.Resources.TheTesseractPageSegmentationModeToUseDefaultIsAuto, True),
             New clsArgumentInfo("charwhitelist", My.Resources.CharacterWhitelist, "text",
              My.Resources.WhitelistOfCharactersToRecognise, True),
             New clsArgumentInfo("diagspath", My.Resources.DiagnosticsPath, "text",
              My.Resources.OptionalCanBeUsedToSpecifyADirectoryWhereDiagnosticsFilesWillBeSavedEgIntermedia, True),
             New clsArgumentInfo("scale", My.Resources.Scale, "number",
               My.Resources.SetTheInternalImageScaleFactorDefaultIs4, True)
            )

            AddRegionActions("{0}GetText", "Read Text (Legacy)",
             My.Resources.ReadTextDiscoveredUsingInvasiveTechniquesWithinARectangularAreaOfAWindow, "text", sReqdAttrsCharMatching)

            AddRegionActions("{0}GetTextCenter", My.Resources.ReadText,
             "Read text discovered values. The text was found based on the centre of " &
             "the text being contained within the specified region within the window. " &
             "(Method used: Invasive)", "text", sReqdAttrsCharMatching)

            ' RecogniseXXXText actions are for back-compat only. Replaced by ReadChars
            AddRegionActions("{0}RecogniseText", My.Resources.RecogniseText,
             My.Resources.ReadTextUsingCharacterMatchingFromARectangularAreaOnAWindow,
             "text",
             sReqdAttrsCharMatching,
             New clsArgumentInfo("font", My.Resources.Font, "text",
              My.Resources.TheNameOfTheFontToUseDefaultIsSystem, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True)
            )

            AddRegionActions("{0}RecogniseSingleLineText", My.Resources.RecogniseSingleLineText,
             My.Resources.ReadTextUsingCharacterMatchingFromARectangularAreaOnAWindowWhichIsKnownToReprese, "text",
             sReqdAttrsCharMatching,
             New clsArgumentInfo("font", My.Resources.Font, "text", My.Resources.TheNameOfTheFontToUse, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True)
            )

            AddRegionActions("{0}RecogniseMultiLineText", My.Resources.RecogniseMultilineText,
             My.Resources.ReadTextUsingCharacterMatchingFromARectangularAreaOnAWindowWhichIsKnownToReprese_1, "text",
             sReqdAttrsCharMatching,
             New clsArgumentInfo("font", My.Resources.Font, "text", My.Resources.TheNameOfTheFontToUse, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True),
             New clsArgumentInfo("EraseBlocks", My.Resources.EraseBlocks, "flag",
              My.Resources.OptionalDefaultFalseSetToTrueToAutomaticallyDetectAndEraseColouredBlocksSurround, True)
            )

            AddRegionActions("{0}ReadChars", My.Resources.RecogniseText,
             My.Resources.ReadTextUsingCharacterMatchingFromARectangularAreaOnAWindow,
             "text",
             sReqdAttrsCharMatching,
             New clsArgumentInfo("font", My.Resources.Font, "text",
              My.Resources.TheNameOfTheFontToUseDefaultIsSystem, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True),
             New clsArgumentInfo("Multiline", My.Resources.SplitLines, "flag",
               My.Resources.TrueToSplitEachSubregionIntoLinesBeforeMatchingDefaultFalse, True),
             New clsArgumentInfo("OrigAlgorithm", My.Resources.UseOriginalAlgorithm, "flag",
               My.Resources.TrueToUseTheBackwardsCompatibleAlgorithmForReadingCharactersWhichScansAcrossThen, True),
             New clsArgumentInfo("EraseBlocks", My.Resources.EraseBlocks, "flag",
              My.Resources.OptionalDefaultFalseSetToTrueToAutomaticallyDetectAndEraseColouredBlocksSurround, True)
            )

            AddRegionActions("{0}ReadBitmap", My.Resources.ReadImage,
             My.Resources.ReadImageFromARectangularAreaOnAWindow, "image")

            ' Actions specific to particular region types

            AddAction("ListReadCharsInRange", My.Resources.RecogniseTextInRange,
             My.Resources.ReadTextUsingCharacterMatchingFromASeriesOfRectangularAreasOnAWindowWhichAreKnow,
             True, "collection", sReqdAttrsCharMatching,
             New clsArgumentInfo("FirstElement", My.Resources.FirstElement, "number",
              My.Resources.TheFirstElementInTheListRegionToReadDefaultIs1, True),
             New clsArgumentInfo("LastElement", My.Resources.LastElement, "number",
              My.Resources.TheLastElementInTheListRegionToReadDefaultIs1, True),
             New clsArgumentInfo("Multiline", My.Resources.SplitLines, "flag",
               My.Resources.TrueToSplitEachSubregionIntoLinesBeforeMatchingDefaultFalse, True),
             New clsArgumentInfo("font", My.Resources.Font, "text", My.Resources.TheNameOfTheFontToUse, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True),
             New clsArgumentInfo("EraseBlocks", My.Resources.EraseBlocks, "flag",
              My.Resources.OptionalDefaultFalseSetToTrueToAutomaticallyDetectAndEraseColouredBlocksSurround, True)
            )

            AddAction("GridReadTable", My.Resources.RecogniseTextInTable,
             My.Resources.ReadsTheTextInEachCellDefinedInTheGridRegion, True,
             "collection", sReqdAttrsCharMatching,
             New clsArgumentInfo("Multiline", My.Resources.SplitLines, "flag",
               My.Resources.TrueToSplitEachSubregionIntoLinesBeforeMatchingDefaultFalse, True),
             New clsArgumentInfo("font", My.Resources.Font, "text", My.Resources.TheNameOfTheFontToUse, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True),
             New clsArgumentInfo("EraseBlocks", My.Resources.EraseBlocks, "flag",
              My.Resources.OptionalDefaultFalseSetToTrueToAutomaticallyDetectAndEraseColouredBlocksSurround, True)
             )

            AddAction("GetConflictingFontCharacters", My.Resources.GetConflictingFontCharacters,
             My.Resources.GetACollectionOfConflictingCharacterGroupsEachRowContainsAStringUnderTheCharacte, False, "collection",
             New clsArgumentInfo("font", My.Resources.Font, "text", My.Resources.TheNameOfTheFontOfInterest, True))

            AddAction("GetWindowText", My.Resources.GetText, My.Resources.GetTheTextForAWindow, False, "text")
            AddAction("GetRegionWindowText", My.Resources.GetWin32ParentText, My.Resources.GetTheTextForARegionsWin32ParentControlwindow, False, "text")
            AddAction("IsWindowActive", My.Resources.IsWindowActive, My.Resources.IndicatesWhetherAWindowIsTheActiveWindowTheWindowWhichReceivesUserInput, False, "flag")
            AddAction("GetChecked", My.Resources.GetChecked, My.Resources.ReadsTheCheckedValueFromACheckboxRadioButtonEtc, False, "flag")
            AddAction("NetGetChecked", My.Resources.GetChecked, My.Resources.ReadsTheCheckedValueFromACheckboxRadioButtonEtc, False, "flag")
            AddAction("ShowDropdown", My.Resources.ShowDropdown, My.Resources.ShowTheDropdownListOnAComboboxOrMenu, False)
            AddAction("HideDropdown", My.Resources.HideDropdown, My.Resources.HideTheDropdownListOnAComboboxOrMenu, False)
            AddAction("JABGetText", My.Resources.GetText, My.Resources.ReadTheTextOfAJavaElement, False, "text")
            AddAction("JABGetSelectedText", My.Resources.GetSelectedText, My.Resources.ReadTheSelectedTextOfAJavaElement, False, "text")

            AddAction("JABSelectText", My.Resources.SelectText, My.Resources.SelectAPortionOfTheTextInAnEditableJavaElement, False,
             New clsArgumentInfo("Position", My.Resources.StartPosition, "number", My.Resources.TheOnebasedStartIndexOfTheTextToBeSelectedThisMustBeWithinTheRangeOfAvailableCha, True),
             New clsArgumentInfo("Length", My.Resources.Length, "number", My.Resources.TheNumberOfCharactersToBeSelectedThisMustBeWithinTheRangeOfAvailableCharactersRe, True))

            AddAction("JABSelectAllText", My.Resources.SelectAllText, My.Resources.SelectAllOfTheTextInAnEditableJavaElement, False)
            AddAction("JABGetChecked", My.Resources.GetChecked, My.Resources.ReadsTheCheckedValueFromAJavaCheckboxRadioButtonToggleButtonEtc, False, "flag")
            AddAction("JABSetChecked", My.Resources.SetChecked, My.Resources.WritesTheCheckedValueToAJavaCheckboxRadioButtonToggleButtonEtc, False)
            AddAction("JABIsSelected", My.Resources.IsSelected, My.Resources.ReadsTheSelectedValueFromAJavaElementSuchAsAListItemATabPageEtc, False, "flag")
            AddAction("JABIsExpanded", My.Resources.IsExpanded, My.Resources.ReadsTheExpandedValueFromAJavaElementSuchAsATreeNodeAComboBoxEtc, False, "flag")
            AddAction("JABHideDropdown", My.Resources.HideDropDown_1, My.Resources.HidesTheDropdownPortionOfAJavaElementSuchAsAMenuComboboxEtc, False)
            AddAction("JABShowDropdown", My.Resources.ShowDropDown_1, My.Resources.ShowsTheDropdownPortionOfAJavaElementSuchAsAMenuComboboxEtc, False)
            AddAction("JABCollapseTreeNode", My.Resources.CollapseItem, My.Resources.CollapseAJavaTreeNodeIfItIsExpanded, False)
            AddAction("JABExpandTreeNode", My.Resources.ExpandItem, My.Resources.ExpandAJavaTreeNodeIfItIsExpanded, False)
            AddAction("JABToggleTreeNode", My.Resources.ToggleItemExpansion, My.Resources.TogglesTheExpandedStateOfAJavaTreeNodeIfItHasChildren, False)

            AddAction("JABSelectItem", My.Resources.SelectItem, My.Resources.SelectsAJavaItemIdentifiedByItsTextOrItsPositionEgFromAMenuDropdownAComboBoxATab, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToBeSelectedIfNotUniqueThenTheFirstMatchingItemFoundWillBeSelect, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemToBeSelectedThisParameterIsIgnoredUnlessTheItemTextParame, True))

            AddAction("JABEnsureItemVisible", My.Resources.EnsureItemVisible, My.Resources.SelectsAJavaItemIdentifiedByItsTextOrItsPositionEgFromAMenuDropdownAComboBoxATab, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOnlyUsedWhenNoValueIsSuppliedToThePositionParameter, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemIfThisParameterIsUsedThenTheTextParameterWillBeIgnored, True))

            AddAction("JABSelectAllItems", My.Resources.SelectAllItems, My.Resources.SelectsAllChildItemsEgInAList, False)
            AddAction("JABClearSelection", My.Resources.ClearSelection, My.Resources.ClearsTheSelectionOfChildItemsEgInAList, False)
            AddAction("JABGetSelectedItemCount", My.Resources.CountSelectedItems, My.Resources.CountsAndReturnsTheNumberOfSelectedItemsContainedInAJavaListviewTreeviewListBoxE, False, "number")
            AddAction("GetAllItems", My.Resources.GetAllItems, My.Resources.GetsAllRowsAndColumnsOfAListviewComboboxDatagridEtcAsACollectionForSimpleControl, False, "collection")

            AddAction("GetTreenodeChildItems", My.Resources.GetTreenodeChildItems, My.Resources.GetsAllChildItemsOfTheSpecifiedItemInATreeviewAsACollection, False, "collection",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheTreenodeOfInterestIfNotUniqueThenTheFirstMatchingItemWillBeRetrieved, True))

            AddAction("GetTreenodeSiblingItems", My.Resources.GetTreenodeSiblingItems, My.Resources.GetsAllSiblingItemsOfTheSpecifiedItemInATreeviewAsACollectionTheReturnedCollecti, False, "collection",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheTreenodeOfInterestIfNotUniqueThenTheFirstMatchingItemWillBeRetrieved, True))

            AddAction("GoToCell", My.Resources.GoToCell, My.Resources.SetTheCurrentPositionToAParticularCell, False,
             New clsArgumentInfo("starty", My.Resources.Row, "number", My.Resources.The1basedRowNumber, False),
             New clsArgumentInfo("startx", My.Resources.Column, "number", My.Resources.The1basedColumnNumber, False))

            AddAction("SetTopRow", My.Resources.SetTopRow, My.Resources.SetTheTopVisibleRowThisRowMayNotActuallyGoToTheTopIfThereAreNotEnoughRowsBelowIt, False,
             New clsArgumentInfo("starty", My.Resources.Row, "number", My.Resources.The1basedRowNumber, False))

            AddAction("GetRowOffset", My.Resources.GetRowOffset, My.Resources.GetTheOffsetInPixelsOfTheTopOfTheGivenRowFromTheTopOfTheGrid, False, "Number",
             New clsArgumentInfo("starty", My.Resources.Row, "number", My.Resources.The1basedRowNumber, False))

            AddAction("SelectRange", My.Resources.SelectRange, My.Resources.SelectARangeOfCells, False,
             New clsArgumentInfo("starty", My.Resources.Row, "number", My.Resources.The1basedRowNumber, False),
             New clsArgumentInfo("startx", My.Resources.Column, "number", My.Resources.The1basedColumnNumber, False),
             New clsArgumentInfo("endy", My.Resources.EndRow, "number", My.Resources.TheEndColumnNumber, False),
             New clsArgumentInfo("endx", My.Resources.EndColumn, "number", My.Resources.TheEndColumnNumber, False))

            AddAction("GetItem", My.Resources.GetItem, My.Resources.GetsAllDataAssociatedWithAnItemAsACollectionEgEveryColumnOfAListviewForTheItemIn, False, "collection",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheMainTextOfTheItemOfInterestEgTheTextOfTheFirstColumnInAListviewItemIfNotUniqu, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemOfInterestThisParameterIsIgnoredUnlessTheItemTextParamete, True))

            AddAction("GetSelectedItems", My.Resources.GetSelectedItems, My.Resources.GetsAllSelectedRowsAndColumnsOfAListviewComboboxDatagridEtcAsACollection, False, "collection")

            AddAction("IsItemSelected", My.Resources.IsItemSelected, My.Resources.ReadsTheSelectedValueFromAChildOfTheChosenElementEgFromAnItemInAListviewEtc, False, "flag",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOnlyUsedWhenNoValueIsSuppliedToThePositionParameter, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemIfThisParameterIsUsedThenTheTextParameterWillBeIgnored, True))

            AddAction("IsItemChecked", My.Resources.IsItemChecked, My.Resources.ReadsTheCheckedValueFromAChildOfTheChosenElementEgFromAnItemInAListviewEtc, False, "flag",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOnlyUsedWhenNoValueIsSuppliedToThePositionParameter, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemIfThisParameterIsUsedThenTheTextParameterWillBeIgnored, True))

            AddAction("IsItemExpanded", My.Resources.IsItemExpanded, My.Resources.ReadsTheExpandedValueFromAChildOfTheChosenElementEgFromAnItemInATreeviewEtc, False, "flag",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOnlyUsedWhenNoValueIsSuppliedToThePositionParameter, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemIfThisParameterIsUsedThenTheTextParameterWillBeIgnored, True))

            AddAction("IsItemFocused", My.Resources.IsItemFocused, My.Resources.ReadsTheFocusedValueFromAChildOfTheChosenElementEgFromAnItemInATreeviewEtc, False, "flag",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOnlyUsedWhenNoValueIsSuppliedToThePositionParameter, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemIfThisParameterIsUsedThenTheTextParameterWillBeIgnored, True))

            AddAction("GetItemImageIndex", My.Resources.GetItemImageIndex, My.Resources.ReadsTheImageIndexOfAChildOfTheChosenElementEgFromAnItemInAListviewTreeviewEtc, False, "number",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOnlyUsedWhenNoValueIsSuppliedToThePositionParameter, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemIfThisParameterIsUsedThenTheTextParameterWillBeIgnored, True))

            AddAction("ReadCurrentValue", My.Resources.GetCurrentValue, My.Resources.GetsTheCurrentValueOfATextboxComboboxListviewCurrentRowCheckboxEtcTheDataTypeWil, False, "text")

            AddAction("EnsureItemVisible", My.Resources.EnsureItemVisible, My.Resources.MakesSureThatTheSpecifiedItemIsVisibleInAListviewOrTreeviewByAdjustingTheScrollb, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOnlyUsedWhenNoValueIsSuppliedToTheIndexParameter, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemIfThisParameterIsUsedThenTheTextParameterWillBeIgnored, True))

            AddAction("ScrollListviewToTop", My.Resources.ScrollToTop, My.Resources.ScrollsAListviewToTheTopWhenInDetailsMode, False)
            AddAction("ScrollListviewToBottom", My.Resources.ScrollToBottom, My.Resources.ScrollsAListviewToTheBottomWhenInDetailsMode, False)
            AddAction("GetItemCount", My.Resources.CountItems, My.Resources.CountsAndReturnsTheNumberOfItemsContainedInAListviewOrTreeview, False, "number")
            AddAction("GetSelectedItemCount", My.Resources.CountSelectedItems, My.Resources.CountsAndReturnsTheNumberOfSelectedItemsContainedInAListviewOrTreeview, False, "number")
            AddAction("GetPageCapacity", My.Resources.GetPageCapacity, My.Resources.GetsTheNumberOfItemsThatCanBeDisplayedOnOnePageInAListviewInDetailsModeOrATreevi, False, "number")
            AddAction("GetSelectedItemText", My.Resources.GetSelectedItemText, My.Resources.GetsTheTextOfTheFirstSelectedItemReturnsAnErrorIfNoItemsAreSelected, False, "text")

            ' TREEVIEW STUFF
            AddAction("ExpandTreeNode", My.Resources.ExpandItem, My.Resources.ExpandsTheSpecifiedTreeviewItem, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheTreenodeToBeExpandedIfNotUniqueThenTheFirstMatchingNodeWillBeExpande, False))

            AddAction("CollapseTreeNode", My.Resources.CollapseItem, My.Resources.CollapsesTheSpecifiedTreeviewItem, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheTreenodeToBeCollapsedIfNotUniqueThenTheFirstMatchingNodeWillBeCollap, False))

            AddAction("ToggleTreeNode", My.Resources.ToggleItemExpansion, My.Resources.TogglesTheExpandedStateOfTheSpecifiedTreeviewItem, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheTreenodeToBeToggledIfNotUniqueThenTheFirstMatchingNodeWillBeToggled, False))

            ' Numeric value stuff
            AddAction("GetNumericValue", My.Resources.GetValue, My.Resources.GetsTheNumericValueOfAControlSuchAsThePositionOnATrackbarAScrollbarOrTheValueInA, False, "number")

            AddAction("SetNumericValue", My.Resources.SetValue, My.Resources.SetsTheNumericValueOfAControlSuchAsThePositionOnATrackbarAScrollbarOrTheValueInA, False, "number",
             New clsArgumentInfo("NumericValue", My.Resources.Value, "number", My.Resources.TheNewValueToBeSetInTheControl, False))

            AddAction("GetMaxNumericValue", My.Resources.GetMaxValue, My.Resources.GetsTheMaximumNumericValueAllowedByAControlSuchAsATrackbarAScrollbarOrANumericUp, False, "number")
            AddAction("GetMinNumericValue", My.Resources.GetMinValue, My.Resources.GetsTheMinimumNumericValueAllowedByAControlSuchAsATrackbarAScrollbarOrANumericUp, False, "number")
            AddAction("JABGetNumericValue", My.Resources.GetValue, My.Resources.GetsTheNumericValueOfAControlSuchAsAJavaTrackbarAJavaScrollbarOrAJavaNumericUpdo, False, "number")

            AddAction("JABSetNumericValue", My.Resources.SetValue, My.Resources.SetsTheNumericValueOfAControlSuchAsAJavaTrackbarAJavaScrollbarOrAJavaNumericUpdo, False, "number",
             New clsArgumentInfo("NumericValue", My.Resources.Value, "number", My.Resources.TheNewValueToBeSetInTheControl, False))

            AddAction("JABGetMaxNumericValue", My.Resources.GetMaxValue, My.Resources.GetsTheMaximumNumericValueAllowedByAControlSuchAsAJavaTrackbarAJavaScrollbarOrAJ, False, "number")
            AddAction("JABGetMinNumericValue", My.Resources.GetMinValue, My.Resources.GetsTheMinimumNumericValueAllowedByAControlSuchAsAJavaTrackbarAJavaScrollbarOrAJ, False, "number")

            'DATE PICKER STUFF
            AddAction("GetDTPickerDateTime", My.Resources.GetValue, My.Resources.GetsTheDatetimeValueOfAVB6DatePickerControl, False, "datetime")
            AddAction("GetDateTimeValue", My.Resources.GetValue, My.Resources.GetsTheDatetimeValueOfAControlSuchAsADatePicker, False, "datetime")

            AddAction("SetDateTimeValue", My.Resources.SetValue, My.Resources.SetsTheDatetimeValueOfAControlSuchAsADatePicker, False, "datetime",
             New clsArgumentInfo("DateTimeValue", My.Resources.Value, "datetime", My.Resources.TheNewValueToBeSetInTheControl, False))

            AddAction("GetMaxDateTimeValue", My.Resources.GetMaxValue, My.Resources.GetsTheMaximumAllowableDatetimeValueOfAControlSuchAsADatePicker, False, "datetime")
            AddAction("GetMinDateTimeValue", My.Resources.GetMinValue, My.Resources.SetsTheMinimumAllowableDatetimeValueOfAControlSuchAsADatePicker, False, "datetime")
            AddAction("GetMaxSelectedDateTimeValue", My.Resources.GetMaxSelectedValue, My.Resources.GetsTheMaximumSelectedDatetimeValueOfAControlSuchAsADatePicker, False, "datetime")
            AddAction("GetMinSelectedDateTimeValue", My.Resources.GetMinSelectedValue, My.Resources.GetsTheMinimumSelectedDatetimeValueOfAControlSuchAsADatePicker, False, "datetime")

            AddAction("ScrollToMinimum", My.Resources.ScrollToMinimum, My.Resources.SetsAScrollbarToItsMinimumValueIeEitherTheExtremeLeftOrToTheTopDependingOnItsOri, False)
            AddAction("ScrollToMaximum", My.Resources.ScrollToMaximum, My.Resources.SetsAScrollbarToItsMaximumValueIeEitherTheExtremeRightOrToTheBottomDependingOnIt, False)

            AddAction("ScrollByAmount", My.Resources.ScrollByAmount, My.Resources.AdjustsTheValueOfAScrollBarByTheSpecifiedAmountInPagesWhereOnePageIsTheSizeRepre, False,
             New clsArgumentInfo("NumericValue", My.Resources.NumberOfPages, "number", My.Resources.TheNumberOfPagesToScrollFractionalValuesSuchAs05AreValidAsWellAsWholeValuesSuchA, False))

            AddAction("JABScrollToMinimum", My.Resources.ScrollToMinimum, My.Resources.SetsAJavaScrollbarToItsMinimumValueIeEitherTheExtremeLeftOrToTheTopDependingOnIt, False)
            AddAction("JABScrollToMaximum", My.Resources.ScrollToMaximum, My.Resources.SetsAJavaScrollbarToItsMaximumValueIeEitherTheExtremeRightOrToTheBottomDepending, False)

            AddAction("JABScrollByAmount", My.Resources.ScrollByAmount, My.Resources.AdjustsTheValueOfAJavaScrollBarByTheSpecifiedAmountInPagesWhereOnePageIsTheSizeR, False,
             New clsArgumentInfo("NumericValue", My.Resources.NumberOfPages, "number", My.Resources.TheNumberOfPagesToScrollFractionalValuesSuchAs05AreValidAsWellAsWholeValuesSuchA, False))

            AddAction("MultiSelectItem", My.Resources.MultiSelectItem, My.Resources.AddsTheItemToAMultipleSelectionInAWindowsListBoxThatMatchesTheGivenText, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToBeSelectedIfNotUniqueThenTheFirstMatchingItemWillBeSelected, False))

            AddAction("SelectItem", My.Resources.SelectItem, My.Resources.SelectsTheItemInAWindowsComboBoxListBoxListviewOrTreeviewThatMatchesTheGivenText, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToBeSelectedIfNotUniqueThenTheFirstMatchingItemFoundWillBeSelect, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemToBeModifiedThisParameterIsIgnoredUnlessTheItemTextParame, True))

            AddAction("ClickItem", My.Resources.ClickItem, My.Resources.ClicksTheItemInAWindowsListviewThatMatchesTheGivenText, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToBeClickedIfNotUniqueThenTheFirstMatchingItemFoundWillBeSelecte, False),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemToBeClickedThisParameterIsIgnoredUnlessTheItemTextParamet, True),
             New clsArgumentInfo("mousebutton", My.Resources.MouseButton, "text", My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("GetItemBoundsAsCollection", My.Resources.GetItemBounds, My.Resources.GetsTheBoundsOfAnItemWithinAListviewRelativeToTheListviewsTopLeftCorner, False, "collection",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOfInterestIfNotUniqueThenTheFirstMatchingItemFoundWillBeUsed, False),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemOfInterestThisParameterIsIgnoredUnlessTheItemTextParamete, True))

            AddAction("GetItemScreenBoundsAsCollection", My.Resources.GetItemScreenBounds, My.Resources.GetsTheBoundsOfAnItemWithinAListviewInScreenCoordinates, False, "collection",
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemOfInterestIfNotUniqueThenTheFirstMatchingItemFoundWillBeUsed, False),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemOfInterestThisParameterIsIgnoredUnlessTheItemTextParamete, True))

            AddAction("SelectTreeNode", My.Resources.SelectTreeNode, My.Resources.SelectsANodeInATreeView, False,
             New clsArgumentInfo("newtext", My.Resources.NodeText, "text", My.Resources.TheTextOfTheNodeToBeSelectedIfNotUniqueThenTheFirstMatchingItemFoundWillBeSelect, False))

            AddAction("ClickToolbarButton", My.Resources.ClickItem, My.Resources.ClicksTheItemInAWindowsToolbarOrSimilarThatMatchesTheGivenText, False,
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemToBeClickedThisParameterIsIgnoredUnlessTheItemTextParamet, True))

            AddAction("IsToolbarButtonEnabled", My.Resources.IsButtonEnabled, My.Resources.ReadsTheEnabledValueOfAToolbarButton, False, "flag",
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemOfInterestThisParameterIsIgnoredUnlessTheItemTextParamete, True))

            AddAction("IsToolbarButtonChecked", My.Resources.IsButtonChecked, My.Resources.ReadsTheCheckedValueOfAToolbarButton, False, "flag",
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemOfInterestThisParameterIsIgnoredUnlessTheItemTextParamete, True))

            AddAction("IsToolbarButtonPressed", My.Resources.IsButtonPressed, My.Resources.ReadsThePressedValueOfAToolbarButton, False, "flag",
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemOfInterestThisParameterIsIgnoredUnlessTheItemTextParamete, True))

            AddAction("SelectMenuItem", My.Resources.SelectMenuItem, My.Resources.SelectsTheSpecifiedMenuItemFromTheMainMenu, False,
             New clsArgumentInfo("Value", My.Resources.MenuPath, "text", My.Resources.ThePathToTheMenuItemOfInterestAsADescentThroughTheMenuTreestructureEgFileBackupO, False))

            AddAction("IsMenuItemChecked", My.Resources.IsMenuItemChecked, My.Resources.ReadsTheCheckedValueOfAMenuItem, False, "flag",
             New clsArgumentInfo("Value", My.Resources.MenuPath, "text", My.Resources.ThePathToTheMenuItemOfInterestAsADescentThroughTheMenuTreestructureEgFileBack_1, False))

            AddAction("IsMenuItemEnabled", My.Resources.IsMenuItemEnabled, My.Resources.ReadsTheEnabledValueOfAMenuItem, False, "flag",
             New clsArgumentInfo("Value", My.Resources.MenuPath, "text", My.Resources.ThePathToTheMenuItemOfInterestAsADescentThroughTheMenuTreestructureEgFileBack_2, False))

            AddAction("ClickTab", My.Resources.ClickTab, My.Resources.SendsAClickMessageToATabControlLocatedAtTheCentreOfTheSpecifiedTab, False,
             New clsArgumentInfo("newtext", My.Resources.TabText, "text", My.Resources.TheTextOfTheTabToReceiveTheClickMessagesIfNotUniqueThenTheFirstMatchingTabFoundW, False),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheTabToBeUsedThisParameterIsIgnoredUnlessTheTabTextParameterIsB, True))

            AddAction("MouseClickTab", My.Resources.GlobalMouseClickTab, My.Resources.ClicksATabControlAtTheCentreOfTheSpecifiedTabUsingAGlobalMouseClick, True,
             New clsArgumentInfo("newtext", My.Resources.TabText, "text", My.Resources.TheTextOfTheTabToBeClickedIfNotUniqueThenTheFirstMatchingTabFoundWillBeUsed, False),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheTabToBeClickedThisParameterIsIgnoredUnlessTheTabTextParameter, True))

            AddAction("SetItemChecked", My.Resources.SetItemChecked, My.Resources.SetsTheCheckedValueOfTheItemInAWindowsListviewOrTreeviewThatMatchesTheGivenText, True,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToBeModifiedIfNotUniqueThenTheFirstMatchingItemFoundWillBeUsed, False),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemToBeModifiedThisParameterIsIgnoredUnlessTheItemTextParame, True),
             New clsArgumentInfo("Value", My.Resources.Value, "flag", My.Resources.AFlagValueIndicatingWhetherTheItemSpecifiedShouldBeCheckedOrUnchecked, True))

            AddAction("AASelectItem", My.Resources.SelectItem, My.Resources.SelectsAChildItemIdentifiedByItsTextOrByItsIndexEgFromAMenuDropdownOrAComboBoxEt, False,
             New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToBeSelectedIfNotUniqueThenTheFirstMatchingItemFoundWillBeSelect, True),
             New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemToBeSelectedThisParameterIsIgnoredUnlessTheItemTextParame, True))

            AddAction("AAGetItemCount", My.Resources.CountItems, My.Resources.CountsAndReturnsTheNumberOfItemsContainedInAnActiveAccessibilityElementSuchAsALi, False, "number")
            AddAction("AAGetAllItems", My.Resources.GetAllItems, My.Resources.GetsAllRowsAndColumnsOfAListviewComboboxDatagridListboxEtcAsACollectionForSimple, False, "collection")
            AddAction("AAGetSelectedItems", My.Resources.GetSelectedItems, My.Resources.GetsAllSelectedRowsAndColumnsOfAListviewComboboxDatagridListboxEtcAsACollection, False, "collection")
            AddAction("AAGetSelectedItemText", My.Resources.GetSelectedItemText, My.Resources.GetsTheTextOfThePrincipalSelectionOfAListviewComboboxDatagridListboxEtc, False, "text")
            AddAction("AAGetValue", My.Resources.GetCurrentValue, My.Resources.ReadsTheCurrentValueFromAnActiveAccessibilityElement, False, "text")
            AddAction("AAGetChecked", My.Resources.GetChecked, My.Resources.ReadsTheCheckedValueFromAnActiveAccessibilityCheckboxRadioButtonEtc, False, "flag")
            AddAction("AAShowDropdown", My.Resources.ShowDropdown, My.Resources.ShowTheDropdownListOnAComboboxOrMenuEtc, False)
            AddAction("AAHideDropdown", My.Resources.HideDropdown, My.Resources.HideTheDropdownListOnAComboboxOrMenuEtc, False)
            AddAction("ClearSelection", My.Resources.ClearSelection, My.Resources.ClearsTheSelectionOfChildItemsWhereAppropriateEgInAEditFieldListviewListboxEtc, False)

            AddAction("SetChecked", My.Resources.SetChecked, My.Resources.SetsTheCheckedStatusOfARadioButtonOrCheckbox, False,
             New clsArgumentInfo("newtext", My.Resources.Checked, "flag", My.Resources.TheNewValueToBeApplied, False))

            AddAction("LaunchMainframe", My.Resources.LaunchMainframe, My.Resources.LaunchTheMainframeTheParametersSuppliedInTheApplicationModellerWizardMayBeOption, False)
            AddAction("AttachMainframe", My.Resources.AttachMainframe, My.Resources.AttachToTheTargetMainframe, False,
                      New clsArgumentInfo("Session ID", My.Resources.SessionIdentifier, "text", My.Resources.OftenCalledTheSessionShortNameThisIsASingleLetterInTheRangeAZ, True))
            AddAction("DetachMainframe", My.Resources.DetachMainframe, My.Resources.DetachFromTheMainframe, False)
            AddAction("TerminateMainframe", My.Resources.TerminateMainframe, My.Resources.TerminateTheMainframe, False)

            AddAction("MainframeSendKeys", My.Resources.SendKeysMainframe, My.Resources.SendKeysToTheMainframeSeeTheMainframeIntegrationHelpPageForMoreDetails, False,
             New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text", My.Resources.TheKeycodesToBeSentToTheTargetApplication, False))

            AddAction("Launch", My.Resources.Launch, My.Resources.LaunchTheTargetApplicationTheParametersSuppliedInTheApplicationModellerWizardMay, False)

            AddAction("AttachApplication", My.Resources.Attach, My.Resources.AttachesToAnExistingInstanceOfTheTargetApplicationTheParametersSuppliedInTheAppl, False,
             New clsArgumentInfo("WindowTitlesCollection", My.Resources.WindowTitlesAsCollection, "collection", My.Resources.OptionalParameterACollectionOfDifferentPatternsDesignedToMatchAnyOneOfANumberOfW, True),
             New clsArgumentInfo("WindowTitle", My.Resources.WindowTitle, "text", My.Resources.OptionalParameterAPatternSpecifyingASingleWindowTitleToBeMatchedTheWildcardChara, True),
             New clsArgumentInfo("ProcessName", My.Resources.ProcessName, "text", My.Resources.OptionalParameterAPatternSpecifyingTheProcessNameOfTheApplicationTheWildcardChar, True),
             New clsArgumentInfo("ProcessID", My.Resources.ProcessID, "number", My.Resources.OptionalParameterTheProcessIdentifierPidOfTheProcess, True),
             New clsArgumentInfo("Username", My.Resources.UserName, "text", My.Resources.OptionalParameterTheUsernameOfTheUserWhoTheProcessIsRunningAs, True),
             New clsArgumentInfo("ChildIndex", My.Resources.ChildIndex, "number", My.Resources.OptionalParameterTheChildProcessIndex, True))

            AddAction("DetachApplication", My.Resources.Detach, My.Resources.DetachesFromTheCurrentlyConnectedApplication, False)
            AddAction("WebDetachApplication", My.Resources.Detach, My.Resources.DetachesFromTheCurrentlyConnectedApplication, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))

            AddAction("Terminate", My.Resources.Terminate, My.Resources.TerminateTheTargetApplicationForEmergencyUseOnly, False)
            AddAction("IsConnected", My.Resources.IsConnected, My.Resources.DetectsWhetherTheBusinessObjectIsCurrentlyConnectedToTheApplicationBeItThroughLa, False, "flag")

            AddAction("WebIsConnected", My.Resources.IsConnected, My.Resources.DetectsWhetherTheBusinessObjectIsCurrentlyConnectedToTheApplicationBeItThroughLa, False, "flag",
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))

            AddAction("WebCloseApplication", My.Resources.Terminate, My.Resources.ClosesTheTabs, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))

            AddAction("SendKeys", My.Resources.GlobalSendKeys, My.Resources.SendKeysToTheActiveApplication, True,
             New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text", My.Resources.TheKeycodesToBeSentToTheTargetApplicationFullDetailsAtHttpmsdn2microsoftcomenusl, False),
             New clsArgumentInfo(ParamNames.Interval, My.Resources.Interval, "number", My.Resources.TheNumberOfSecondsToWaitBetweenEachKeypressNoteThatIfThisIsSetToANonzeroValueTex, True)
            )

            AddAction("SendKeyEvents", My.Resources.GlobalSendKeyEvents, My.Resources.SendKeysToTheApplicationUsingEventsTheseEventsTakePlaceAtTheLowestLevelAndAreRec, True,
             New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text", String.Format(My.Resources.TheKeyEventsToBeSentToTheTargetApplicationSpecialCharactersShouldBeEnclosedInCur, vbCrLf), False),
             New clsArgumentInfo(ParamNames.Interval, My.Resources.Interval, "number", My.Resources.TheNumberOfSecondsToWaitBetweenEachKeypressDefaultIs01Ie100ms, True)
            )

            AddAction("ActivateApp", My.Resources.ActivateApplication, My.Resources.ActivateTheApplicationIeBringsToTheForegroundTargetElementShouldBeTheAppsMainWin, False)

            AddAction("TypeText", My.Resources.WindowPressKeys, My.Resources.SendKeypressesToTheGivenWindowNoteThatCareMustSometimesBeTakenToDirectTheKeypres, False,
             New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text", My.Resources.TheTextToBeSentEachLetterRepresentsADifferentKeystrokeWhichWillBeSentOneByOne, False),
             New clsArgumentInfo(ParamNames.Interval, My.Resources.Interval, "number", My.Resources.TheNumberOfSecondsToWaitBetweenEachKeystroke, True)
            )

            AddAction("TypeTextAlt", My.Resources.WindowPressKeysWithAlt, My.Resources.SendKeypressesToTheGivenWindowWithTheAltKeyDown, True,
             New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text", My.Resources.TheTextToBeSentEachLetterRepresentsADifferentKeystrokeWhichWillBeSentOneByOne, False),
             New clsArgumentInfo(ParamNames.Interval, My.Resources.Interval, "number", My.Resources.TheNumberOfSecondsToWaitBetweenEachKeystroke, True)
            )

            AddAction("Default", My.Resources.xDefault, My.Resources.PerformTheDefaultActionForTheApplicationElement, False)

            AddAction("MoveWindow", My.Resources.Move, My.Resources.MoveAWindowToTheSpecifiedLocation, False,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfTheNewWindowLocationRelativeToTheTopleftCornerOfTheScreen, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfTheNewWindowLocationRelativeToTheTopleftCornerOfTheScreen, False))

            AddAction("ResizeWindow", My.Resources.Resize, My.Resources.ResizeAWindow, False,
             New clsArgumentInfo("targwidth", My.Resources.Width, "number", My.Resources.TheNewWidthOfTheWindowInPixels, False),
             New clsArgumentInfo("targheight", My.Resources.Height, "number", My.Resources.TheNewWidthOfTheWindowInPixels, False))

            AddAction("MaximiseWindow", My.Resources.Maximise, My.Resources.MaximisesTheWindowToFillTheScreen, False)
            AddAction("MinimiseWindow", My.Resources.Minimise, My.Resources.MinimisesTheWindowToTheTaskTray, False)
            AddAction("RestoreWindow", My.Resources.Restore, My.Resources.RestoresTheWindowFromItsMinimisedOrMaximisedState, False)
            AddAction("HideAllWindows", My.Resources.HideAllWindows, My.Resources.HidesAllToplevelWindowsBelongingToAnApplicationUseHideWindowToHideAnIndividualWi, False)
            AddAction("HideWindow", My.Resources.HideWindow, My.Resources.HidesAToplevelWindowByMovingItOffscreenAndRemovingFromTheTaskbar, False)
            AddAction("UnhideWindow", My.Resources.UnhideWindow, My.Resources.UnhidesAToplevelWindowWhichHasPreviouslyBeenHidden, False)
            AddAction("IsWindowHidden", My.Resources.IsWindowHidden, My.Resources.DeterminesWhetherAWindowHasPreviouslyBeenHiddenUsingACallToHideWindow, False, "flag")

            AddAction("DoJava", My.Resources.xDo, My.Resources.PerformsAnAction, False,
             New clsArgumentInfo("action", My.Resources.Action, "text", "", False))

            AddAction("JABFocus", My.Resources.Focus, My.Resources.BringsAJavaElementIntoFocusReadyToReceiveKeyboardInput, False)
            AddAction("JABSelectTab", My.Resources.SelectTab, My.Resources.SelectsAJavaTabFromAJavaTabControl, False)
            AddAction("JABGetItemCount", My.Resources.CountItems, My.Resources.GetsTheNumberOfItemsContainedInAJavaComponentEgTheNumberOfRowsInATableEgTheNumbe, False, "number")
            AddAction("JABGetAllItems", My.Resources.GetAllItems, My.Resources.GetsAllItemsFromAJavaTableComboBoxEtcForSimpleControlsTheCollectionHasASingleFie, False, "collection")
            AddAction("JABGetSelectedItems", My.Resources.GetSelectedItems, My.Resources.GetsAllSelectedItemsFromAJavaTableComboBoxEtc, False, "collection")
            AddAction("AAClickCentre", My.Resources.GlobalMouseClickCentre,
             My.Resources.ClickAtTheCentreOfTheActiveAccessibilityElement, True,
              New clsArgumentInfo("newtext", My.Resources.MouseButton, "text",
              My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("AAMouseClick", My.Resources.GlobalMouseClick,
             My.Resources.ClickTheElementAtTheSpecifiedPositionUsingAGlobalMouseClickThePositionIsRelative, True,
             New clsArgumentInfo("targx", My.Resources.X, "number",
              My.Resources.TheXCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number",
              My.Resources.TheYCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text",
              My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("AAGetName", My.Resources.GetName, My.Resources.GetsTheNameOfTheAccessibilityElementSomeApplicationsExposeCurrentValueInformatio, False, "text")

            AddAction("AAGetDescription", My.Resources.GetDescription, My.Resources.GetsTheDescriptionOfTheAccessibilityElementSomeApplicationsExposeCurrentValueInf, False, "text")

            AddAction("AAGetTable", My.Resources.GetTable, My.Resources.GetTheActiveAccessibilityTableSpecified, False, "collection")

            AddAction("AAFocus", My.Resources.Focus, My.Resources.FocusTheActiveAccessibilityElement, False)

            AddAction("AASendKeys", My.Resources.GlobalSendKeys, My.Resources.SendKeysToTheActiveApplication, True,
             New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text", My.Resources.TheKeycodesToBeSentToTheTargetApplicationFullDetailsAtHttpmsdn2microsoftcomenusl, False),
             New clsArgumentInfo(ParamNames.Interval, My.Resources.Interval, "number", My.Resources.TheNumberOfSecondsToWaitBeforeEachKeypressNoteThatIfThisIsSetToANonzeroValueText, True)
            )

            '==HTML stuff==
            AddAction("HTMLGetDocumentURL", My.Resources.GetDocumentURL, My.Resources.RetrievesTheURLOfTheCurrentlyLoadedDocument, False, "text")
            AddAction("HTMLGetDocumentURLDomain", My.Resources.GetDocumentURLDomain, My.Resources.RetrievesTheURLDomainOfTheCurrentlyLoadedDocument, False, "text")
            AddAction("HTMLNavigate", My.Resources.Navigate, My.Resources.NavigateToAURL, False,
             New clsArgumentInfo("NewText", My.Resources.URL, "text", My.Resources.TheURLToNavigateTo, True))

            AddAction("HTMLClickCentre", My.Resources.ClickCentre, My.Resources.ClickTheCentreOfAnHTMLElement, False)
            AddAction("HTMLDoubleClickCentre", My.Resources.DoubleClickCentre, My.Resources.DoubleClickTheCentreOfAnHTMLElement, False)
            AddAction("HTMLGetAllItems", My.Resources.GetAllItems, My.Resources.GetAllChildItemsOfAnHTMLElementAsACollectionForSimpleControlsTheCollectionHasASi, False, "collection")
            AddAction("HTMLGetTable", My.Resources.GetTable, My.Resources.GetsAllElementsOfAHTMLTableAsACollection, False, "collection")
            AddAction("HTMLGetSelectedItems", My.Resources.GetSelectedItems, My.Resources.GetAllChildItemsOfAnHTMLElementWhichAreSelectedAsACollection, False, "collection")
            AddAction("HTMLGetSelectedItemText", My.Resources.GetSelectedItemText, My.Resources.GetsTheTextOfTheFirstSelectedItemReturnsAnErrorIfNoItemsAreSelected, False, "text")
            AddAction("HTMLSelectItem", My.Resources.SelectItem, My.Resources.SelectsTheItemInAComboBoxThatMatchesTheGivenParameters, False,
            New clsArgumentInfo("newtext", My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToBeSelectedIfNotUniqueThenTheFirstMatchingItemFoundWillBeSelect, True),
            New clsArgumentInfo("position", My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheItemToBeModifiedThisParameterIsIgnoredUnlessTheItemTextParame, True),
            New clsArgumentInfo("propname", My.Resources.ItemValue, "text", My.Resources.TheValueOfTheItemToSelectedThisParameterIsIgnoredUnlessTheItemTextAndItemPositio, True))
            AddAction("HTMLCountItems", My.Resources.CountItems, My.Resources.GetTheNumberOfChildItemsOfAnHTMLElement, False, "number")
            AddAction("HTMLCountSelectedItems", My.Resources.CountSelectedItems, My.Resources.GetTheNumberOfChildItemsOfAnHTMLElementWhichAreSelected, False, "number")
            AddAction("HTMLInvokeJavascriptMethod", My.Resources.InvokeJavascriptFunction, My.Resources.CallsTheSpecifiedJavascriptFunctionThisCanBeAStandardJavascriptFunctionOrOneWhic, False,
             New clsArgumentInfo("MethodName", My.Resources.FunctionName, "text", My.Resources.TheNameOfTheFunctionToBeCalled, False),
             New clsArgumentInfo("Arguments", My.Resources.Arguments, "text", My.Resources.TheArgumentsToPassToTheFunctionAsAnArrayOfObjectsInJSONFormat, True))

            AddAction("HTMLInsertJavascriptFragment", My.Resources.InsertJavascriptFragment, My.Resources.InsertsTheSuppliedJavascriptFragmentIntoTheTargetDocumentThisCanBeAMixtureOfMeth, False,
             New clsArgumentInfo("FragmentText", My.Resources.Fragment, "text", String.Format(My.Resources.TheJavascriptFragmentToBeInsertedEg0varSuccess0functionDoSomethingAlerthello0Suc, vbCrLf), False))

            AddAction("HTMLUpdateCookie", My.Resources.UpdateCookie, My.Resources.UpdatesTheGivenCookieOnTheTargetDocumentCookiesNeedToBeSpecifiedInTheFormatNamev, False,
   New clsArgumentInfo("newtext", My.Resources.Cookie, "text", My.Resources.TheCookieDataToBeSet, False))

            'Diagnostic Actions
            AddAction("HTMLSnapshot", My.Resources.HTMLSnapshot, My.Resources.TakeASnapshotOfAllHTMLDocumentElements, False, "text")
            AddAction("HTMLSourceCap", My.Resources.SourceCapture, My.Resources.CaptureTheSourceOfTheHTMLDocument, False, "text")
            AddAction("HTMLGetOuterHTML", My.Resources.GetHTML, My.Resources.GetsTheHTMLOfAnElement, False, "text")
            AddAction("HTMLGetPath", My.Resources.GetPath, My.Resources.GetsPathOfTheHTMLElement, False, "text")
            AddAction("WindowsSnapshot", My.Resources.Snapshot, My.Resources.TakeASnapshotOfAllElements, False, "text")
            'For backwards compatibility only...
            AddAction("JABSnapshot", My.Resources.Snapshot, My.Resources.TakeASnapshotOfAllElements, False, "text")

            '.Net Control Specific Actions
            AddAction("ClickLink", My.Resources.ClickLink, My.Resources.ClicksANetLinkLabel, False)

            '==DDE stuff==
            AddAction("DDEGetText", My.Resources.ReadTextDDE, My.Resources.ReadsTheValueOfADDEField, False, "text")

            AddAction("ExecuteDDECommand", My.Resources.ExecuteCommandDDE, My.Resources.ExecutesTheCommandRepresentedByTheCurrentDDEElement, False,
             New clsArgumentInfo("newtext", My.Resources.Value, "text", My.Resources.TheValueIfAnyToSupplyToTheDDECommand, True),
             New clsArgumentInfo("NoCheck", My.Resources.NoCheck, "flag", My.Resources.IfTrueThenNoCheckWillBeMadeAsToTheSuccessOfTheOperationThisProvidesAWorkaroundFo, True))

            AddAction("Verify", My.Resources.Verify, My.Resources.VerifiesThatAnElementExistsAndOptionallyHighlightsTheElement, False, "flag",
             New clsArgumentInfo("highlight", My.Resources.Highlight, "flag", My.Resources.SpecifiesThatTheElementShouldBeHighlighted, True))

            AddRegionActions("{0}RegionVerify", My.Resources.Verify,
             My.Resources.VerifiesThatAnElementExistsAndOptionallyHighlightsTheElement, "flag",
             New clsArgumentInfo("highlight", My.Resources.Highlight, "flag",
              My.Resources.SpecifiesThatTheElementShouldBeHighlighted, True))

            '==Web==            
            AddAction("WebWrite", My.Resources.Click,My.Resources.WritesToTheGivenElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebClick", My.Resources.Click, My.Resources.ClicksOnTheGivenElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebDoubleClick", My.Resources.DoubleClick, My.Resources.DoubleClickTheGivenElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebSelect", My.Resources.xSelect, My.Resources.SelectsTheCurrentElementAndAnyDescendantElements, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebFocus", My.Resources.Focus, My.Resources.SetsTheKeyboardFocusToTheGivenElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebHover", My.Resources.Hover, My.Resources.HoversOverAnHTMLElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebHoverMouseOnElement", My.Resources.HoverMouseOnElement, My.Resources.HoverMouseOnAnHTMLElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebSetAttribute", My.Resources.SetAttribute, My.Resources.SetsTheGivenAttributeOnTheElement, False,
                      New clsArgumentInfo(ParamNames.PropName, My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToSet, False),
                      New clsArgumentInfo(ParamNames.Value, My.Resources.Value, "text", My.Resources.TheValueToSetTheAttributeTo, False),
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebSetCheckState", My.Resources.SetChecked, My.Resources.SetsTheCheckedStateOfTheElement, False,
                      New clsArgumentInfo(ParamNames.NewText, My.Resources.Checked, "flag", My.Resources.WhetherTheControlShouldBeChecked, False),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebSendKeys", My.Resources.GlobalSendKeys, My.Resources.SendKeysToTheActiveApplication, True,
                      New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text", My.Resources.TheKeycodesToBeSentToTheTargetApplicationFullDetailsAtHttpmsdn2microsoftcomenusl, False),
                      New clsArgumentInfo(ParamNames.Interval, My.Resources.Interval, "number", My.Resources.TheNumberOfSecondsToWaitBeforeEachKeypressNoteThatIfThisIsSetToANonzeroValueText, True),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebScrollTo", My.Resources.ScrollTo, My.Resources.ScrollTheParentToTheElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebNavigate", My.Resources.SetAddress, My.Resources.NavigatesToTheGivenAddress, False,
                      New clsArgumentInfo(ParamNames.NewText, My.Resources.Address, "text", My.Resources.TheWebAddressToNavigateTo, False),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebInvokeJavascript", My.Resources.InvokeJavascriptFunction, My.Resources.CallsTheSpecifiedJavascriptFunctionThisCanBeAStandardJavascriptFunctionOrOneWhic, False,
                      New clsArgumentInfo(ParamNames.MethodName, My.Resources.FunctionName, "text", My.Resources.TheNameOfTheFunctionToBeCalled, False),
                      New clsArgumentInfo(ParamNames.Arguments, My.Resources.Arguments, "text", My.Resources.TheArgumentsToPassToTheFunctionAsAnArrayOfObjectsInJSONFormat, True),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebInjectJavascript", My.Resources.InsertJavascriptFragment, My.Resources.InsertsTheSuppliedJavascriptFragmentIntoTheTargetDocumentThisCanBeAMixtureOfMeth, False,
                      New clsArgumentInfo(ParamNames.FragmentText, My.Resources.Fragment, "text", String.Format(My.Resources.TheJavascriptFragmentToBeInsertedEg0varSuccess0functionDoSomethingAlerthello0Suc, vbCrLf), False),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebUpdateCookie", My.Resources.UpdateCookie, My.Resources.UpdatesTheGivenCookieOnTheTargetDocumentCookiesNeedToBeSpecifiedInTheFormatNa_1, False,
                      New clsArgumentInfo(ParamNames.NewText, My.Resources.Cookie, "text", My.Resources.TheCookieDataToBeSet, False),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebSubmit", My.Resources.Submit, My.Resources.SubmitsTheForm, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebSelectListItem", My.Resources.SelectListItem, My.Resources.SelectsAnItemInTheListByNameOrIndex, False,
                      New clsArgumentInfo(ParamNames.ItemIndex, My.Resources.ItemIndex, "number", My.Resources.TheIndexOfTheItemToSelect, True),
                      New clsArgumentInfo(ParamNames.ItemText, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToSelect, True),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebAddToListSelection", My.Resources.AddToSelection, My.Resources.AddsAnItemToTheListsSelectedItemsByNameOrIndex, False,
                      New clsArgumentInfo(ParamNames.ItemIndex, My.Resources.ItemIndex, "number", My.Resources.TheIndexOfTheItem, True),
                      New clsArgumentInfo(ParamNames.ItemText, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItem, True),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebRemoveFromListSelection", My.Resources.RemoveFromSelection, My.Resources.RemovesAnItemFromTheListsSelectedItemsByNameOrIndex, False,
                      New clsArgumentInfo(ParamNames.ItemIndex, My.Resources.ItemIndex, "number", My.Resources.TheIndexOfTheItem, True),
                      New clsArgumentInfo(ParamNames.ItemText, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItem, True),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebClickMenuItem", My.Resources.ClickMenuItem, My.Resources.ClicksTheMenuItemIdentifiedByNameOrIndex, False,
                      New clsArgumentInfo(ParamNames.ItemIndex, My.Resources.ItemIndex, "number", My.Resources.TheIndexOfTheItemToClick, True),
                      New clsArgumentInfo(ParamNames.ItemText, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToClick, True),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebToggleExpandCollapse", My.Resources.ToggleExpandCollapse, My.Resources.TogglesWhetherTheElementIsExpandedOrCollapsed, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebExpand", My.Resources.Expand, My.Resources.ExpandsTheElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebCollapse", My.Resources.Collapse, My.Resources.CollapsesTheElement, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebCheckRadio", My.Resources.CheckRadio, My.Resources.SetsTheRadioButtonToBeTheOneInItsGroupThatsChecked, False,
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebSelectTextRange", My.Resources.SelectTextRange, My.Resources.SelectsARangeOfTextWithinTheElement, False,
                      New clsArgumentInfo(ParamNames.Index, My.Resources.StartIndex, "number", My.Resources.TheCharacterToStartTheSelectionAt),
                      New clsArgumentInfo(ParamNames.Length, My.Resources.Length, "number", My.Resources.TheNumberOfCharactersToSelect),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetAttribute", My.Resources.GetAttribute, My.Resources.GetsTheGivenAttributeOfTheElement, False, "text",
                      New clsArgumentInfo(ParamNames.PropName, My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToGet, False),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetBounds", My.Resources.GetBounds, My.Resources.GetsTheBoundsOfTheElementRelativeToThePage, False, "collection",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetCheckState", My.Resources.GetCheckState, My.Resources.GetsTheCheckedStateOfTheElement, False, "flag",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetColumnCount", My.Resources.GetColumnCount, My.Resources.GetsTheNumberOfColumnsInTheTable, False, "number",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetFormValues", My.Resources.GetFormValues, My.Resources.GetsTheValuesFromEachInputElementInAForm, False, "collection",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetId", My.Resources.GetID, My.Resources.GetsTheIDOfTheElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetIsOnScreen", My.Resources.GetIsOnScreen, My.Resources.GetsAValueIndicatingWhetherTheElementIsVisibleOnScreen, False, "flag",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetIsSelected", My.Resources.GetIsSelected, My.Resources.GetsAValueIndicatingWhetherTheListItemIsSelected, False, "flag",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetIsVisible", My.Resources.GetIsVisible, My.Resources.GetsAValueIndicatingWhetherTheElementIsVisible, False, "flag",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetItems", My.Resources.GetItems, My.Resources.GetsAllItemsFromAList, False, "collection",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetLabel", My.Resources.GetLabel, My.Resources.GetsTheLabelAssociatedWithAnElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetLinkAddress", My.Resources.GetLinkAddress, My.Resources.GetsTheWebAddressThatALinkPointsTo, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetMaxValue", My.Resources.GetMaxValue, My.Resources.GetsTheMaximumValueOfANumericElement, False, "number",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetMinValue", My.Resources.GetMinValue, My.Resources.GetsTheMinimumValueOfANumericElement, False, "number",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetPageUrl", My.Resources.GetPageURL, My.Resources.GetsTheURLOfTheWebPageThatContainsThisElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetRowCount", My.Resources.GetRowCount, My.Resources.GetsTheNumberOfRowsInTheTable, False, "number",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetSelectedItems", My.Resources.GetSelectedItems, My.Resources.GetsTheItemsInAListWhichAreSelected, False, "collection",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetSelectedItemsText", My.Resources.GetSelectedItemsText, My.Resources.GetsTheItemsInAListWhichAreSelected, False, "collection",
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetSelectedText", My.Resources.GetSelectedText, My.Resources.GetsTheTextSelectedInAnElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetTableItem", My.Resources.GetTableItem, My.Resources.GetsTheItemInATableAtTheGivenRowAndColumn, False, "text",
                      New clsArgumentInfo(ParamNames.ColumnNumber, My.Resources.Column, "number", My.Resources.ColumnIndex_1, False),
                      New clsArgumentInfo(ParamNames.RowNumber, My.Resources.Row, "number", My.Resources.RowIndex_1, False),
                      New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetTableItems", My.Resources.GetTableItems, My.Resources.GetsAllItemsInATable, False, "collection",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetText", My.Resources.GetText, My.Resources.GetsTheTextContainedInAnElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetValue", My.Resources.GetCurrentValue, My.Resources.ReadsTheCurrentValueFromAWebElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetPath", My.Resources.GetPath, My.Resources.GetsPathOfTheWebElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            AddAction("WebGetHtml", My.Resources.GetHTML, My.Resources.GetsTheHTMLOfAnElement, False, "text",
                       New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))

            '==UIA==
            AddAction("UIAGetName", My.Resources.GetName, My.Resources.GetsTheNameOfTheUIAElementSomeApplicationsExposeCurrentValueInformationInTheName, False, "text")
            AddAction("UIAFocus", My.Resources.Focus, My.Resources.FocusTheUIAElement, False)
            AddAction("UIAButtonPress", My.Resources.Press, My.Resources.PressTheUIAutomationButtonElementThisWillInvokeToggleOrExpandcollapseTheButtonDe, False)
            AddAction("UIAPress", My.Resources.Press, My.Resources.PressTheUIAutomationElement, False)
            AddAction("UIASelect", My.Resources.xSelect, My.Resources.DeselectsAnySelectedItemsAndThenSelectsTheUIAutomationElement, False)
            AddAction("UIAMenuItemPress", My.Resources.PressMenuItem, My.Resources.PressTheUIAutomationMenuItemElementEitherByExpandingASubmenuOrInvokingTheMenuIte, False)
            AddAction("UIAToggle", My.Resources.Toggle, My.Resources.ChangeTheToggleStateOfTheUIAutomationElement, False)
            AddAction("UIAGetToggleState", My.Resources.GetChecked, My.Resources.GetTheCheckedStateOfTheUIAutomationElement, False, "flag")
            ' overload below allows the above action to be named differently for a button control
            AddAction("UIAGetPressedState", "UIAGetToggleState", My.Resources.GetPressed, My.Resources.GetThePressedStateOfTheUIAutomationElement, False, "flag", Nothing)
            AddAction("UIASetToggleState", My.Resources.SetChecked, My.Resources.SetsTheCheckedStateOfTheUIAutomationElement, False,
                      New clsArgumentInfo(ParamNames.NewText, My.Resources.Checked, "flag", My.Resources.WhetherTheControlShouldBeChecked, False))
            AddAction("UIAGetValue", My.Resources.GetCurrentValue, My.Resources.ReadsTheCurrentValueFromAUIAutomationElement, False, "text")
            AddAction("UIASetValue", My.Resources.SetCurrentValue, My.Resources.WritesTheCurrentValueToAUIAutomationElement, False, "text")
            AddAction("UIAExpandCollapse", My.Resources.ExpandCollapse, My.Resources.ExpandsOrCollapsesTheUIAutomationElement, False)
            AddAction("UIARadioSetChecked", My.Resources.SetChecked, My.Resources.SetsTheCheckedStatusOfAUIARadioButton, False,
                      New clsArgumentInfo("newtext", My.Resources.Checked, "flag", My.Resources.WhetherTheControlShouldBeChecked, False))
            AddAction("UIAGetSelectedItemText", My.Resources.GetSelectedItemText, My.Resources.GetsTheTextOfTheSelectedItem, False, "text")
            AddAction("UIAGetSelectedText", My.Resources.GetSelectedText, My.Resources.GetsTheTextSelectedInTheElement, False, "text")
            AddAction("UIAGetExpanded", My.Resources.IsExpanded,
                My.Resources.ReadsTheCurrentStateOfWhetherTheElementIsExpandedOrCollapsedIfTheElementCannotBe, False, "flag")
            AddAction("UIAGetRadioCheckedState", My.Resources.GetChecked, My.Resources.GetTheCheckedStateOfTheUIAutomationElement, False, "flag")
            AddAction("UIAGetSelectedItems", My.Resources.GetSelectedItems, My.Resources.GetsTheSelectedItemsInTheList, False, "collection")
            AddAction("UIAGetItemCount", My.Resources.GetItemCount, My.Resources.GetsTheNumberOfItemsInTheList, False, "number")
            AddAction("UIAGetAllItems", My.Resources.GetAllItems, My.Resources.GetsAllItemsInTheList, False, "collection")
            AddAction("UIAListSelect", My.Resources.SelectItem,
                      My.Resources.DeselectsAnySelectedItemsAndThenSelectsTheCurrentElementIfTheItemTextParameterIs,
                      False,
                New clsArgumentInfo(ParamNames.Index, My.Resources.ItemPosition, "number", My.Resources.TheOnebasedIndexOfTheItemToSelect, True),
                New clsArgumentInfo(ParamNames.IDName, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToSelect, True))
            AddAction("UIAComboSelect", My.Resources.SelectItem,
                      My.Resources.SelectsTheSpecifiedItemInAUIAComboBoxIfTheItemTextParameterIsSuppliedThisTakesPr,
                      False,
                New clsArgumentInfo(ParamNames.Index, My.Resources.ItemPosition, "number", My.Resources.TheOnebasedIndexOfTheItemToSelect, True),
                New clsArgumentInfo(ParamNames.IDName, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToSelect, True))
            AddAction("UIAComboGetItemCount", My.Resources.GetItemCount, My.Resources.GetsTheNumberOfItemsInTheListInAUIAComboBox, False, "number")
            AddAction("UIAComboGetAllItems", My.Resources.GetAllItems, My.Resources.GetsAllItemsInTheListInAUIAComboBox, False, "collection")
            AddAction("UIAScrollVertical", My.Resources.ScrollVertically, My.Resources.ScrollsAnElementVertically, False,
                New clsArgumentInfo(ParamNames.BigStep, My.Resources.BigStep, "flag", My.Resources.WhetherTheScrollShouldBeABigStep, False),
                New clsArgumentInfo(ParamNames.ScrollUp, My.Resources.ScrollUp, "flag", My.Resources.WhetherTheScrollShouldBeInTheUpwardsDirection, False))
            AddAction("UIAScrollHorizontal", My.Resources.ScrollHorizontally, My.Resources.ScrollsAnElementHorizontally, False,
                New clsArgumentInfo(ParamNames.BigStep, My.Resources.BigStep, "flag", My.Resources.WhetherTheScrollShouldBeABigStep, False),
                New clsArgumentInfo(ParamNames.ScrollUp, My.Resources.ScrollUp, "flag", My.Resources.WhetherTheScrollShouldBeInTheUpwardsDirection, False))
            AddAction("UIAAddToSelection", My.Resources.AddToSelection, My.Resources.AddsAnItemToItsParentsSelectedItems, False)
            AddAction("UIARemoveFromSelection", My.Resources.RemoveFromSelection, My.Resources.RemovesAnItemFromItsParentsSelectedItems, False)
            AddAction("UIAScrollIntoView", My.Resources.ScrollIntoView, My.Resources.ScrollsTheElementIntoView, False)
            AddAction("UIAListAddToSelection", My.Resources.AddToSelection, My.Resources.AddsTheSpecfiedItemToTheListsSelectedItemsIfTheItemTextParameterIsSuppliedThisTa, False,
                New clsArgumentInfo(ParamNames.Index, My.Resources.ItemPosition, "number", My.Resources.TheOnebasedIndexOfTheItemToSelect, True),
                New clsArgumentInfo(ParamNames.IDName, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToSelect, True))
            AddAction("UIAListRemoveFromSelection", My.Resources.RemoveFromSelection, My.Resources.RemovesTheGivenItemFromTheListsSelectedItemsIfTheItemTextParameterIsSuppliedThis, False,
                New clsArgumentInfo(ParamNames.Index, My.Resources.ItemPosition, "number", My.Resources.TheOnebasedIndexOfTheItemToDeselect, True),
                New clsArgumentInfo(ParamNames.IDName, My.Resources.ItemText, "text", My.Resources.TheTextOfTheItemToDeselect, True))
            AddAction("UIAGetIsItemSelected", My.Resources.IsItemSelected, My.Resources.WhetherTheItemIsCurrentlySelected, False, "flag")
            AddAction("UIAGetAllTabsText", My.Resources.GetAllTabsText, My.Resources.ReturnsACollectionOfTheTextOfAllTheTabsContainedWithinTheTabControl, False, "collection")
            AddAction("UIASelectTabItem", My.Resources.SelectItem, My.Resources.SelectsTheTabItem, False)
            AddAction("UIASelectTab", My.Resources.SelectTab,
                      My.Resources.SelectsTheTabWithinAUIATabControlIfTheTabTextParameterIsSuppliedThisTakesPrecede, False,
                      New clsArgumentInfo("TabIndex", My.Resources.TabPosition, "number", My.Resources.TheOnebasedIndexOfTheTabToSelect, True),
                      New clsArgumentInfo("TabText", My.Resources.TabText, "text", My.Resources.TheTextValueOfTheTabToSelect, True))

            Dim idName = New clsArgumentInfo(ParamNames.IDName, My.Resources.ItemText, "text", My.Resources.TheTextOfTheTreeViewItemOfInterest, False)
            Dim itemPosition = New clsArgumentInfo(ParamNames.Position, My.Resources.ItemPosition, "number", My.Resources.The1basedIndexOfTheTreeViewItemOfInterestThisParameterIsIgnoredUnlessTheItemText, True)
            AddAction("UIATreeSelect", My.Resources.SelectItem, My.Resources.SelectsAnItemInATree, False, idName, itemPosition)
            AddAction("UIATreeAddToSelection", My.Resources.AddToSelection, My.Resources.AddsTheGivenItemToTheTreesSelectedItems, False, idName, itemPosition)
            AddAction("UIATreeRemoveFromSelection", My.Resources.RemoveFromSelection, My.Resources.RemovesTheGivenItemFromTheTreesSelectedItems, False, idName, itemPosition)
            AddAction("UIATreeExpandCollapse", My.Resources.ExpandCollapse, My.Resources.ExpandsOrCollapsesTheUIAutomationElement, False, idName, itemPosition)
            AddAction("UIATreeIsExpanded", My.Resources.IsItemExpanded,
                      My.Resources.ReadsTheCurrentStateOfWhetherTheElementIsExpandedOrCollapsedIfTheElementCannotBe,
                      False, "flag", idName, itemPosition)

            Dim columnNumberArg = New clsArgumentInfo(ParamNames.ColumnNumber, My.Resources.ColumnNumber, "number",
                My.Resources.The1basedColumnNumberIndicatingWhichColumnToActOn, False)
            Dim rowNumberArg = New clsArgumentInfo(ParamNames.RowNumber, My.Resources.RowNumber, "number",
                My.Resources.The1basedRowNumberIndicatingWhichRowToActOn, False)
            Dim elementNumber = New clsArgumentInfo(ParamNames.ElementNumber, My.Resources.ElementNumber, "number",
                My.Resources.ComponentIndexWithinTheCell0IndicatesTheCellItself)
            AddAction("UIATableReadCellText", My.Resources.ReadCellText, My.Resources.ReadsTheTextFromTheCell, False, "text",
                columnNumberArg, rowNumberArg, elementNumber)
            AddAction("UIATableSetCellText", My.Resources.WriteCellText, My.Resources.SetsTheTextIntoTheSpecifiedCell, False,
                New clsArgumentInfo("NewText", My.Resources.CellText, "text", My.Resources.TheNewTextForTheCellDefaultIsBlank, True),
                columnNumberArg, rowNumberArg, elementNumber)
            AddAction("UIATableScrollIntoView", My.Resources.ScrollIntoView, My.Resources.ScrollsTheTableCellIntoView, False,
                columnNumberArg, rowNumberArg)
            AddAction("UIATableRows", My.Resources.GetRows, My.Resources.GetsTheDataInTheSpecifiedRows, False, "collection",
                New clsArgumentInfo(ParamNames.FirstRowNumber, My.Resources.FirstRowNumber, "number",
                                    My.Resources.The1basedRowNumberIndicatingTheFirstRowToIncludeInTheCollectionDefaultsTo1IfNotS,
                                    False),
                New clsArgumentInfo(ParamNames.LastRowNumber, My.Resources.LastRowNumber, "number",
                                    My.Resources.The1basedRowNumberIndicatingTheLastRowToIncludeInTheCollectionDefaultsToTheLastR,
                                    False))

            AddAction("UIATableAddRowToSelection", My.Resources.AddRowToSelection, My.Resources.AddsARowToTheCurrentSelection, False, rowNumberArg)
            AddAction("UIATableRemoveRowFromSelection", My.Resources.RemoveRowFromSelection, My.Resources.RemovesARowFromTheCurrentSelection, False, rowNumberArg)
            AddAction("UIATableClearSelection", My.Resources.ClearSelection, My.Resources.ClearsTheCurrentSelection, False)
            AddAction("UIATableGetSelectedRows", My.Resources.GetSelectedRows, My.Resources.GetsTheSelectedRowsAsACollection, False, "collection")
            AddAction("UIATableRowCount", My.Resources.GetRowCount, My.Resources.GetTheNumberOfRowsInTheTable, False, "number")
            AddAction("UIATableColumnCount", My.Resources.GetColumnCount, My.Resources.GetTheNumberOfColumnsInTheTable, False, "number")
            AddAction("UIATableSelectedRowNumber", My.Resources.GetSelectedRowNumber, My.Resources.GetsTheNumberOfTheFirstSelectedRowInTheTable, False, "number")
            AddAction("UIATableSelectedColumnNumber", My.Resources.GetSelectedColumnNumber, My.Resources.GetsNumberOfTheFirstSelectedColumnInTheTable, False, "number")

            AddAction("UIATableToggleCell", My.Resources.ToggleCell, My.Resources.ChangesTheToggleStateOfTheTableCell, False,
                columnNumberArg, rowNumberArg, elementNumber)

            AddAction("UIATableReadToggleState", My.Resources.GetChecked, My.Resources.GetsCheckedStateOfTheTableCell, False, "flag",
                columnNumberArg, rowNumberArg, elementNumber)

            AddAction("UIATableExpandCollapse", My.Resources.ExpandCollapseCell, My.Resources.ExpandsOrCollapsesTheTableCell, False,
                columnNumberArg, rowNumberArg, elementNumber)

            AddAction("UIATableExpanded", My.Resources.GetExpandedCell, My.Resources.ReadsTheCurrentStateOfWhetherTheCellElementIsExpandedOrCollapsed, False, "flag",
                columnNumberArg, rowNumberArg, elementNumber)

            AddAction("UIATableSelectComboboxItem", My.Resources.SelectComboboxItem,
                      My.Resources.SelectsAnItemFromAComboboxWithinTheTableCellElementIfTheItemTextParameterIsSuppl,
                      False, columnNumberArg, rowNumberArg, elementNumber,
                      New clsArgumentInfo("ItemIndex", My.Resources.ItemPosition, "number", My.Resources.TheOnebasedIndexOfTheItemToSelect, True),
                      New clsArgumentInfo("ItemText", My.Resources.ItemText, "text", My.Resources.TheTextValueOfTheItemToSelect, True))

            AddAction("UIATableCountComboboxItems", My.Resources.CountComboboxItems,
                      My.Resources.TheNumberOfItemsWithinAComboboxInsideTheUIAutomationElementCell,
                      False, "number", columnNumberArg, rowNumberArg, elementNumber)

            AddAction("UIATableGetAllComboboxItems", My.Resources.GetAllComboboxItems,
                      My.Resources.GetAllItemsFromAComboboxWithinTheCellElementAsACollection,
                      False, "collection", columnNumberArg, rowNumberArg, elementNumber)

            AddAction("UIATableGetSelectedComboboxItem", My.Resources.GetSelectedComboboxItem,
                      My.Resources.GetsTheValueOfTheSelectedItemFromAComboboxInATableCell,
                      False, "text", columnNumberArg, rowNumberArg, elementNumber)

            AddAction("UIADrag", My.Resources.Drag, My.Resources.StartDraggingFromAGivenPositionOverTheWindow, True,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfThePointAtWhichToStartDraggingRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfThePointAtWhichToStartDraggingRelativeToTheWindow, False))

            AddAction("UIADrop", My.Resources.Drop, My.Resources.DropAtTheGivenPositionOverTheWindowMustFollowADrag, True,
             New clsArgumentInfo("targx", My.Resources.X, "number", My.Resources.TheXCoordinateOfThePointAtWhichToPerformTheDropRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number", My.Resources.TheYCoordinateOfThePointAtWhichToPerformTheDropRelativeToTheWindow, False))

            AddAction("UIAGetRelativeElementBounds", My.Resources.GetRelativeBounds,
             My.Resources.GetsInformationAboutTheBoundingRectangleOfTheElementInCoordinatesRelativeToThePa, False, "collection")

            AddAction("UIAGetElementScreenBounds", My.Resources.GetScreenBounds,
             My.Resources.GetsInformationAboutTheBoundingRectangleOfTheElementInScreenCoordinates, False, "collection")

            AddAction("UIAMouseClick", My.Resources.GlobalMouseClick,
             My.Resources.ClickTheElementAtTheSpecifiedPositionUsingAGlobalMouseClickThePositionIsRelative, True,
             New clsArgumentInfo("targx", My.Resources.X, "number",
              My.Resources.TheXCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("targy", My.Resources.Y, "number",
              My.Resources.TheYCoordinateOfThePointAtWhichToClickRelativeToTheWindow, False),
             New clsArgumentInfo("newtext", My.Resources.MouseButton, "text",
              My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("UIAClickCentre", My.Resources.GlobalMouseClickCentre,
             My.Resources.ClickAtTheCentreOfTheUIAElement, True,
              New clsArgumentInfo("newtext", My.Resources.MouseButton, "text",
              My.Resources.OptionalTheMouseButtonToUseValidValuesAreLeftAndRightANullValueImpliesLeft, True))

            AddAction("UIASendKeys", My.Resources.GlobalSendKeys, My.Resources.SendKeysToTheActiveApplication, True,
             New clsArgumentInfo(ParamNames.NewText, My.Resources.Text, "text",
                                 My.Resources.TheKeycodesToBeSentToTheTargetApplicationFullDetailsAtHttpmsdn2microsoftcomenusl,
                                 False),
             New clsArgumentInfo(ParamNames.Interval, My.Resources.Interval, "number",
                                 My.Resources.TheNumberOfSecondsToWaitBeforeEachKeypressNoteThatIfThisIsSetToANonzeroValueText,
                                 True)
            )

            AddAction("UIAComboExpandCollapse", My.Resources.ExpandCollapse, My.Resources.ExpandsOrCollapsesTheUIAutomationElement, False)

            'Add in actions from external data...
            For Each x As XmlDocument In extdata
                If x.DocumentElement.Name = "actions" Then
                    Dim err As String = Nothing
                    For Each el As XmlElement In x.DocumentElement.ChildNodes
                        If el.Name = "action" Then
                            Dim id As String = Nothing
                            Dim name As String = Nothing
                            Dim helptext As String = Nothing
                            Dim returntype As String = ""
                            Dim requiresfocus As Boolean = False
                            Dim args As New List(Of clsArgumentInfo)

                            id = el.GetAttribute("id")
                            For Each el2 As XmlElement In el.ChildNodes
                                Select Case el2.Name
                                    Case "name"
                                        name = LTools.GetC(el2.InnerText, "actions", "name")
                                    Case "helptext"
                                        helptext = LTools.GetC(el2.InnerText, "actions", "helptext")
                                    Case "returntype"
                                        returntype = el2.InnerText
                                    Case "requiresfocus"
                                        requiresfocus = True
                                    Case "argument"
                                        Dim argid As String = el2.GetAttribute("id")
                                        Dim argname As String = Nothing
                                        Dim argdesc As String = Nothing
                                        Dim argdatatype As String = Nothing
                                        Dim argopt As Boolean = False
                                        For Each el3 As XmlElement In el2.ChildNodes
                                            Select Case el3.Name
                                                Case "name"
                                                    argname = LTools.GetC(el3.InnerText, "actions", "name")
                                                Case "description"
                                                    argdesc = LTools.GetC(el3.InnerText, "actions", "description")
                                                Case "datatype"
                                                    argdatatype = el3.InnerText
                                                Case "optional"
                                                    argopt = True
                                            End Select
                                        Next
                                        If argdatatype Is Nothing Then
                                            mExtDataErrors.Add(String.Format(My.Resources.MissingDataTypeForArgument0, argid))
                                        ElseIf argdesc Is Nothing Then
                                            mExtDataErrors.Add(String.Format(My.Resources.MissingDescriptionForArgument0, argid))
                                        ElseIf argname Is Nothing Then
                                            mExtDataErrors.Add(String.Format(My.Resources.MissingNameForArgument0, argid))
                                        Else
                                            args.Add(New clsArgumentInfo(argid, argname, argdatatype, argdesc, argopt))
                                        End If
                                End Select
                            Next

                            If err IsNot Nothing Then
                                If id Is Nothing Then
                                    err = My.Resources.MissingIDForAction
                                ElseIf name Is Nothing Then
                                    err = String.Format(My.Resources.MissingNameForAction0, id)
                                ElseIf helptext Is Nothing Then
                                    err = String.Format(My.Resources.MissingHelptextForAction0, id)
                                End If
                            End If

                            If err Is Nothing Then
                                If args.Count = 0 Then
                                    AddAction(id, name, helptext, requiresfocus, returntype)
                                Else
                                    AddAction(id, name, helptext, requiresfocus, returntype, args.ToArray())
                                End If
                            End If
                        End If
                    Next
                End If
            Next

        End Sub


        ''' <summary>
        ''' Adds a condition with the given arguments.
        ''' </summary>
        ''' <param name="id">The ID of the condition</param>
        ''' <param name="name">The (display) name of the condition</param>
        ''' <param name="datatype">The datatype of the tested attribute</param>
        ''' <param name="desc">A description of the condition</param>
        ''' <param name="args">The arguments required for the condition</param>
        Private Shared Sub AddCondition(ByVal id As String, ByVal name As String, ByVal datatype As String, ByVal desc As String, ByVal ParamArray args() As clsArgumentInfo)
            mConditionTypes(id) = New clsConditionTypeInfo(id, name, datatype, desc, args)
        End Sub

        ''' <summary>
        ''' Adds a condition with the given arguments.
        ''' </summary>
        ''' <param name="id">The ID of the condition</param>
        ''' <param name="name">The (display) name of the condition</param>
        ''' <param name="datatype">The datatype of the tested attribute</param>
        ''' <param name="desc">A description of the condition</param>
        ''' <param name="defaultVal">The default value set for the condition, if one
        ''' exists. This is used to pre-populate the condition when it is added to a
        ''' stage.</param>
        ''' <param name="args">The arguments required for the condition</param>
        Private Shared Sub AddCondition(ByVal id As String, ByVal name As String, ByVal datatype As String, ByVal desc As String, ByVal defaultVal As String, ByVal ParamArray args() As clsArgumentInfo)
            mConditionTypes(id) = New clsConditionTypeInfo(id, name, datatype, desc, defaultVal, args)
        End Sub

        ''' <summary>
        ''' Builds the conditions that can be waited for within AMI
        ''' </summary>
        Private Shared Sub BuildConditionTypes()
            'Now do the conditiontype info
            mConditionTypes = New Dictionary(Of String, clsConditionTypeInfo)

            AddCondition("GetText", My.Resources.GetText, "text",
             My.Resources.ComparesAgainstDrawnTextRetrievedFromARectangularRegion)

            AddCondition("ListGetText", My.Resources.GetText, "text", My.Resources.ComparesAgainstDrawnTextRetrievedFromAListElementInARectangularRegion,
             New clsArgumentInfo("ElementNumber", My.Resources.ElementNumber, "text", My.Resources.TheElementNumberToCheckDefaultIs1, True))

            AddCondition("GridGetText", My.Resources.GetText, "text", My.Resources.ComparesAgainstDrawnTextRetrievedFromAGridCellInARectangularRegion,
             New clsArgumentInfo("ColumnNumber", My.Resources.ColumnNumber, "text", My.Resources.TheColumnNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("RowNumber", My.Resources.RowNumber, "text", My.Resources.TheRowNumberOfTheGridToCheckDefaultIs1, True))

            AddCondition("MatchImage", My.Resources.MatchesImage, "flag",
             My.Resources.ComparesAgainstAnImageRetrievedFromARectangularRegion, "True",
             New clsArgumentInfo("ImageValue", My.Resources.Image, "image",
              My.Resources.TheImageToCompareAgainstIfEmptyItWillCheckForAnElementSnapshotOnTheImageIfNoneFo,
              True))

            AddCondition("ListMatchImage", My.Resources.MatchesImage, "flag",
            My.Resources.ComparesAgainstAnImageRetrievedFromAListElementInARectangularRegion, "True",
             New clsArgumentInfo("ElementNumber", My.Resources.ElementNumber, "text",
              My.Resources.TheElementNumberToCheckDefaultIs1, True),
             New clsArgumentInfo("ImageValue", My.Resources.Image, "image",
              My.Resources.TheImageToCompareAgainstIfEmptyItWillCheckForAnElementSnapshotOnTheImageIfNoneFo,
              True))

            AddCondition("GridMatchImage", My.Resources.MatchesImage, "flag",
             My.Resources.ComparesAgainstAnImageRetrievedFromAGridElementInARectangularRegion, "True",
             New clsArgumentInfo("ColumnNumber", My.Resources.ColumnNumber, "text",
              My.Resources.TheColumnNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("RowNumber", My.Resources.RowNumber, "text",
              My.Resources.TheRowNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("ImageValue", My.Resources.Image, "image",
              My.Resources.TheImageToCompareAgainstIfEmptyItWillCheckForAnElementSnapshotOnTheImageIfNoneFo,
              True))

            AddCondition("ContainsImage", My.Resources.ContainsImage, "flag",
             My.Resources.DeterminesIfTheRegionContainsAParticularImage, "True",
             New clsArgumentInfo("ImageValue", My.Resources.Image, "image",
              My.Resources.TheImageToCompareAgainstIfEmptyItWillCheckForAnElementSnapshotOnTheImageIfNoneFo,
              True))

            AddCondition("ListContainsImage", My.Resources.ContainsImage, "flag",
             My.Resources.DeterminesIfTheSpecifiedElementInTheListRegionContainsAParticularImage, "True",
             New clsArgumentInfo("ElementNumber", My.Resources.ElementNumber, "text",
              My.Resources.TheElementNumberToCheckDefaultIs1, True),
             New clsArgumentInfo("ImageValue", My.Resources.Image, "image",
              My.Resources.TheImageToCompareAgainstIfEmptyItWillCheckForAnElementSnapshotOnTheImageIfNoneFo,
              True))

            AddCondition("GridContainsImage", My.Resources.ContainsImage, "flag",
             My.Resources.DeterminesIfTheSpecifiedElementInTheGridRegionContainsAParticularImage, "True",
             New clsArgumentInfo("ColumnNumber", My.Resources.ColumnNumber, "text",
              My.Resources.TheColumnNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("RowNumber", My.Resources.RowNumber, "text",
              My.Resources.TheRowNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("ImageValue", My.Resources.Image, "image",
              My.Resources.TheImageToCompareAgainstIfEmptyItWillCheckForAnElementSnapshotOnTheImageIfNoneFo,
              True))

            AddCondition("ContainsColour", My.Resources.ContainsColour, "flag",
             My.Resources.DeterminesIfTheRegionContainsAParticularColour, "True",
             New clsArgumentInfo("Colour", My.Resources.Colour, "Text",
              My.Resources.TheColourToCheckForThisCanBeEnteredAsAColourNameEgRedOrAHexColourCodeEgFF0000, False))

            AddCondition("ListContainsColour", My.Resources.ContainsColour, "flag",
             My.Resources.DeterminesIfTheSpecifiedElementInTheListRegionContainsAParticularColour, "True",
             New clsArgumentInfo("ElementNumber", My.Resources.ElementNumber, "text",
              My.Resources.TheElementNumberToCheckDefaultIs1, True),
             New clsArgumentInfo("Colour", My.Resources.Colour, "Text",
              My.Resources.TheColourToCheckForThisCanBeEnteredAsAColourNameEgRedOrAHexColourCodeEgFF0000, False))

            AddCondition("GridContainsColour", My.Resources.ContainsColour, "flag",
             My.Resources.DeterminesIfTheSpecifiedElementInTheGridRegionContainsAParticularColour, "True",
             New clsArgumentInfo("ColumnNumber", My.Resources.ColumnNumber, "text",
              My.Resources.TheColumnNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("RowNumber", My.Resources.RowNumber, "text",
              My.Resources.TheRowNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("Colour", My.Resources.Colour, "Text",
              My.Resources.TheColourToCheckForThisCanBeEnteredAsAColourNameEgRedOrAHexColourCodeEgFF0000, False))

            AddCondition("UniformColour", My.Resources.UniformColour, "flag",
             My.Resources.DeterminesIfTheRegionConsistsOfAUniformColour, "True",
             New clsArgumentInfo("Colour", My.Resources.Colour, "Text",
              My.Resources.TheColourToCheckForThisCanBeEnteredAsAColourNameEgRedOrAHexColourCodeEgFF0000IfN, True))

            AddCondition("ListUniformColour", My.Resources.UniformColour, "flag",
             My.Resources.DeterminesIfTheSpecifiedElementInTheListRegionConsistsOfAUniformColour, "True",
             New clsArgumentInfo("ElementNumber", My.Resources.ElementNumber, "text",
              My.Resources.TheElementNumberToCheckDefaultIs1, True),
             New clsArgumentInfo("Colour", My.Resources.Colour, "Text",
              My.Resources.TheColourToCheckForThisCanBeEnteredAsAColourNameEgRedOrAHexColourCodeEgFF0000IfN, True))

            AddCondition("GridUniformColour", My.Resources.UniformColour, "flag",
             My.Resources.DeterminesIfTheSpecifiedElementInTheGridRegionConsistsOfAUniformColour, "True",
             New clsArgumentInfo("ColumnNumber", My.Resources.ColumnNumber, "text",
              My.Resources.TheColumnNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("RowNumber", My.Resources.RowNumber, "text",
              My.Resources.TheRowNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("Colour", My.Resources.Colour, "Text",
              My.Resources.TheColourToCheckForThisCanBeEnteredAsAColourNameEgRedOrAHexColourCodeEgFF0000IfN, True))

            AddCondition("ReadChars", My.Resources.RecogniseText, "text", My.Resources.ComparesTextUsingCharacterMatchingFromARectangularAreaOnAWindow,
             New clsArgumentInfo("font", My.Resources.Font, "text",
              My.Resources.TheNameOfTheFontToUseDefaultIsSystem, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True),
             New clsArgumentInfo("Multiline", My.Resources.SplitLines, "flag",
              My.Resources.TrueToSplitEachSubregionIntoLinesBeforeMatchingDefaultFalse, True),
             New clsArgumentInfo("OrigAlgorithm", My.Resources.UseOriginalAlgorithm, "flag",
              My.Resources.TrueToUseTheBackwardsCompatibleAlgorithmForReadingCharactersWhichScansAcrossThen, True),
             New clsArgumentInfo("EraseBlocks", My.Resources.EraseBlocks, "flag",
              My.Resources.OptionalDefaultFalseSetToTrueToAutomaticallyDetectAndEraseColouredBlocksSurround, True)
            )

            AddCondition("ListReadChars", My.Resources.RecogniseText, "text", My.Resources.ComparesTextUsingCharacterMatchingFromAListElementInARectangularAreaOnAWindow,
             New clsArgumentInfo("ElementNumber", My.Resources.ElementNumber, "text",
              My.Resources.TheElementNumberToCheckDefaultIs1, True),
             New clsArgumentInfo("font", My.Resources.Font, "text",
              My.Resources.TheNameOfTheFontToUseDefaultIsSystem, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True),
             New clsArgumentInfo("Multiline", My.Resources.SplitLines, "flag",
              My.Resources.TrueToSplitEachSubregionIntoLinesBeforeMatchingDefaultFalse, True),
             New clsArgumentInfo("OrigAlgorithm", My.Resources.UseOriginalAlgorithm, "flag",
              My.Resources.TrueToUseTheBackwardsCompatibleAlgorithmForReadingCharactersWhichScansAcrossThen, True),
             New clsArgumentInfo("EraseBlocks", My.Resources.EraseBlocks, "flag",
              My.Resources.OptionalDefaultFalseSetToTrueToAutomaticallyDetectAndEraseColouredBlocksSurround, True)
            )

            AddCondition("GridReadChars", My.Resources.RecogniseText, "text", My.Resources.ComparesTextUsingCharacterMatchingFromAGridCellInARectangularAreaOnAWindow,
             New clsArgumentInfo("ColumnNumber", My.Resources.ColumnNumber, "text",
              My.Resources.TheColumnNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("RowNumber", My.Resources.RowNumber, "text",
              My.Resources.TheRowNumberOfTheGridToCheckDefaultIs1, True),
             New clsArgumentInfo("font", My.Resources.Font, "text",
              My.Resources.TheNameOfTheFontToUseDefaultIsSystem, True),
             New clsArgumentInfo("colour", My.Resources.Colour, "text",
              My.Resources.TheTextColourEgFF0000DefaultIsBlack, True),
             New clsArgumentInfo("backgroundcolour", My.Resources.BackgroundColour, "text",
              My.Resources.OptionalUsedToSpecifyTheBackgroundColourAgainstWhichTheTextIsSetIfUsedTheColourO, True),
             New clsArgumentInfo("Multiline", My.Resources.SplitLines, "flag",
              My.Resources.TrueToSplitEachSubregionIntoLinesBeforeMatchingDefaultFalse, True),
             New clsArgumentInfo("OrigAlgorithm", My.Resources.UseOriginalAlgorithm, "flag",
              My.Resources.TrueToUseTheBackwardsCompatibleAlgorithmForReadingCharactersWhichScansAcrossThen, True),
             New clsArgumentInfo("EraseBlocks", My.Resources.EraseBlocks, "flag",
              My.Resources.OptionalDefaultFalseSetToTrueToAutomaticallyDetectAndEraseColouredBlocksSurround, True)
            )

            AddCondition("GetText", My.Resources.GetText, "text", My.Resources.ComparesAgainstDrawnTextRetrievedFromARectangularRegion)
            AddCondition("JABGetText", My.Resources.GetText, "text", My.Resources.ComparesAgainstTextRetrievedFromAJavaElement)
            AddCondition("GetItemCount", My.Resources.CountItems, "number", My.Resources.CountsAndReturnsTheNumberOfItemsContainedInAListviewOrTreeview, "0")
            AddCondition("JABIsFocused", My.Resources.IsFocused, "flag", My.Resources.DeterminesWhetherAJavaElementIsFocused, "True")
            AddCondition("GetField", My.Resources.GetField, "text", My.Resources.ComparesContentsOfATerminalField)
            AddCondition("GetWindowText", My.Resources.GetWindowText, "text", My.Resources.ComparesMainTextOfAWindowOrControl)
            AddCondition("CheckExists", My.Resources.CheckExists, "flag", My.Resources.CheckTheElementExistsTrueOrFalse, "True")
            AddCondition("AACheckExists", My.Resources.CheckExists, "flag", My.Resources.CheckThatTheActiveAccessibilityElementExistsTrueOrFalse, "True")
            AddCondition("JABCheckExists", My.Resources.CheckExists, "flag", My.Resources.CheckThatTheJavaElementExistsTrueOrFalse, "True")
            AddCondition("CheckField", My.Resources.CheckExists, "flag", My.Resources.CheckIfTheTerminalFieldExistsWithTheTextContainedWhenSpiedTrueOrFalse, "True")
            AddCondition("Checked", My.Resources.Checked, "flag", My.Resources.CheckIfTheItemEgACheckboxIsCheckedTrueOrFalse, "True")
            AddCondition("NetChecked", My.Resources.Checked, "flag", My.Resources.CheckIfTheItemEgACheckboxIsCheckedTrueOrFalse, "True")
            AddCondition("DocumentLoaded", My.Resources.DocumentLoaded, "flag", My.Resources.CheckIfTheCurrentDocumentHasLoadedTrueOrFalseRelevantOnlyForBrowserApplicationsD, "True")
            AddCondition("HTMLCheckExistsAndDocumentLoaded", My.Resources.ParentDocumentLoaded, "flag", My.Resources.CheckTheElementExistsAndThatTheEntirePageAndAllOfItsChildFramesAreFullyLoadedPar, "True")
            AddCondition("HTMLGetDocumentURL", My.Resources.CheckURL, "text", My.Resources.CheckIfTheURLOfTheCurrentlyLoadedDocumentEqualsAParticularValueRelevantOnlyForBr)
            AddCondition("HTMLGetDocumentURLDomain", My.Resources.CheckURLDomain, "text", My.Resources.CheckIfTheDomainOfTheURLOfTheCurrentlyLoadedDocumentEqualsAParticularValueReleva)
            AddCondition("HTMLGetValue", My.Resources.CheckValue, "text", My.Resources.CheckIfTheHTMLElementIsASpecificValue)
            AddCondition("CheckDDEElementReadable", My.Resources.CheckDDEElementReadable, "flag", My.Resources.ChecksWhetherTheSuppliedDDEElementCanBeLocatedAndItsValueCanBeRead, "True")
            AddCondition("CheckDDEServerAndTopicAvailable", My.Resources.CheckDDETopicAvailable, "flag", My.Resources.ChecksWhetherADDEConversationCanBeInitiatedWithTheSpecifiedServerTopicPairTheIte, "True")
            AddCondition("CheckButtonClicked", My.Resources.Pressed, "flag", My.Resources.CheckIfTheButtonHasBeenPressed, "True")
            AddCondition("MouseLeftDown", My.Resources.MouseLeftDown, "flag", My.Resources.CheckIfTheLeftMouseButtonHasBeenPressed, "True")
            AddCondition("IsConnected", My.Resources.IsConnected, "flag", My.Resources.ChecksWhetherTheBusinessObjectIsCurrentlyConnectedToTheApplicationBeItThroughLau, "True")
            AddCondition("WebIsConnected", My.Resources.IsConnected, "flag", My.Resources.ChecksWhetherTheBusinessObjectIsCurrentlyConnectedToTheApplicationBeItThroughLau, "True")
            AddCondition("CheckWindowActive", My.Resources.CheckWindowActive, "flag", My.Resources.ChecksWhetherAWindowIsTheCurrentActiveWindowTheWindowWhichReceivesUserInput, "True")

            AddCondition("UIAGetItemCount", My.Resources.CountItems, "number", My.Resources.GetsTheNumberOfItemsInTheList, "0")
            AddCondition("UIAGetIsFocused", My.Resources.Focused, "flag", My.Resources.ChecksIfTheElementHasFocus, "True")
            AddCondition("UIAComboGetItemCount", My.Resources.CountItems, "number", My.Resources.GetsTheNumberOfItemsInTheComboBox, "0")
            AddCondition("UIAGetToggleState", My.Resources.Checked, "flag", My.Resources.GetsIfTheElementIsChecked, "True")
            AddCondition("UIAGetRadioCheckedState", My.Resources.Checked, "flag", My.Resources.GetsIfTheElementIsChecked, "True")

            AddCondition("WebCheckExists", My.Resources.CheckExists, "flag", My.Resources.CheckThatTheWebElementExistsTrueOrFalse, "True",
                         New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            
            AddCondition("WebWrite", My.Resources.WebWrite, "text", My.Resources.WriteTheValueToTheGivenWebElement, "True",
                         New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))

            AddCondition("WebCheckParentDocumentLoaded", My.Resources.ParentDocumentLoaded,"flag", My.Resources.CheckTheElementExistsAndThatTheEntirePageAndAllOfItsChildFramesAreFullyLoadedPar, "True",
                         New clsArgumentInfo(ParamNames.TrackingId, My.Resources.TrackingId, "text", My.Resources.TrackingIdDescription, True))
            'In the following group, pay attention to the fact that the things we refer
            'to as identifiers internally are called "Attributes" in the Application
            'Modeller user interface, so documentation and friendly names are set
            'accordingly. (see bug #6883)
            AddCondition("CheckWindowIdentifier", My.Resources.CheckWindowAttribute, "text", My.Resources.CheckTheCurrentValueOfAWindowAttribute,
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToCheck, False))
            AddCondition("CheckAAIdentifier", My.Resources.CheckAAAttribute, "text", My.Resources.GetTheCurrentValueOfAnActiveAccessibilityAttribute,
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToCheck, False))
            AddCondition("CheckHTMLIdentifier", My.Resources.CheckHTMLAttribute, "text", My.Resources.GetTheCurrentValueOfAnHTMLAttribute,
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToCheck, False))
            AddCondition("CheckJABIdentifier", My.Resources.CheckJABAttribute, "text", My.Resources.GetTheCurrentValueOfAJavaAccessBridgeAttribute,
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToCheck, False))
            AddCondition("CheckUIAIdentifier", My.Resources.CheckUIAutomationAttribute, "text", My.Resources.GetTheCurrentValueOfAUIAutomationAttribute,
             New clsArgumentInfo("idname", My.Resources.AttributeName, "text", My.Resources.TheNameOfTheAttributeToCheck, False))
        End Sub

        ''' <summary>
        ''' Gets an element object representing the given query.
        ''' </summary>
        ''' <param name="identifiers">The list of identifiers representing a
        ''' UIAutomation element.</param>
        ''' <returns>A <see cref="clsElement"/> encapsulating a UIAutomation element
        ''' made up of the given identifiers.</returns>
        Public Function GetUIAutomationElement(identifiers As String) As clsElement
            Return New clsElement(
                Me, ElementCategory.UIA, GetIdentifierMapFromResult(identifiers))
        End Function

        ''' <summary>
        ''' Gets the element types identified by the given strings
        ''' </summary>
        ''' <param name="ids">The IDs for which the types are required.</param>
        ''' <returns>The collection of element types corresponding to the given
        ''' IDs</returns>
        Private Shared Function GetElementTypes(ByVal ParamArray ids() As String) As ICollection(Of clsElementTypeInfo)
            Dim types As New List(Of clsElementTypeInfo)
            For Each id As String In ids
                types.Add(mElementTypes(id))
            Next
            Return types
        End Function

        ''' <summary>
        ''' Adds the allowed alternative types for a specific element type
        ''' </summary>
        ''' <param name="targetId">The ID of the element type whose alternative types
        ''' should be appended to</param>
        ''' <param name="ids">The IDs of the alternative element types suitable for
        ''' the target type</param>
        Private Shared Sub AddAltTypes(ByVal targetId As String, ByVal ParamArray ids() As String)
            mElementTypes(targetId).AlternateTypes.AddRange(GetElementTypes(ids))
        End Sub

        ''' <summary>
        ''' Attribute defining the allowed alternatives to an element type, ie.
        ''' those types which are available for the user to set the element to,
        ''' given the type of element detected by the spy operation.
        ''' </summary>
        Private Class AllowedAlternativesAttribute : Inherits Attribute
            ' The relevant alternatives
            Private mAlts As ICollection(Of ElementType)
            ''' <summary>
            ''' Creates a new set of allowed alternatives containing the given
            ''' element types
            ''' </summary>
            ''' <param name="alts">The alternative types</param>
            Public Sub New(ByVal ParamArray alts() As ElementType)
                mAlts = alts
            End Sub

            ''' <summary>
            ''' Gets the alternative types allowed for the given element type.
            ''' </summary>
            ''' <param name="el">The element type for which the alternatives are
            ''' required.</param>
            ''' <returns>A collection of alternative element types appropriate to the
            ''' specified element type.</returns>
            Public Shared Function GetAlternatives(ByVal el As ElementType) As ICollection(Of ElementType)
                With el.GetType().GetMember(el.ToString())(0)
                    Dim attr = .GetCustomAttributes(Of AllowedAlternativesAttribute)(False).FirstOrDefault()
                    If attr IsNot Nothing Then
                        Return GetReadOnly.ICollection(attr.mAlts)
                    End If
                End With
                Return GetEmpty.ICollection(Of ElementType)()
            End Function
        End Class

        ''' <summary>
        ''' Enumeration of the supported element types within AMI. A type can have a
        ''' set of 'allowed alternatives' defined on it - these are other element
        ''' types which the user can choose to use in place of the base element type.
        ''' eg. if the spy tool recognises a 'Button', the user can specify that it
        ''' is actually a radio button.
        ''' </summary>
        Public Enum ElementType

            Application
            <AllowedAlternatives(Password)>
            Edit
            <AllowedAlternatives(Edit)>
            Password
            ListBox
            RadioButton
            CheckBox
            NetCheckBox

            <AllowedAlternatives(RadioButton, CheckBox, NetCheckBox)>
            Button

            ComboBox
            ListView
            ListViewAx
            Treeview
            TreeviewAx
            StatusBarAx
            TabControl
            TrackBar
            UpDown
            DateTimePicker
            MonthCalPicker
            ScrollBar
            Label
            NetLinkLabel
            Toolbar
            DataGrid
            DataGridView

            <AllowedAlternatives(DataGrid, DataGridView, RadioButton, CheckBox, Button, Edit,
             ListBox, ComboBox, ListView, Treeview, TabControl, TrackBar, UpDown, DateTimePicker,
             MonthCalPicker, ScrollBar, Label, Toolbar)>
            Window

            MSFlexGrid
            ApexGrid
            DTPicker

            WindowRect
            Win32ListRegion
            Win32GridRegion

            TerminalField

            <AllowedAlternatives(AAElement)>
            AAButton
            <AllowedAlternatives(AAElement)>
            AACheckBox
            <AllowedAlternatives(AAElement)>
            AAComboBox
            <AllowedAlternatives(AAElement)>
            AAListBox
            <AllowedAlternatives(AAElement)>
            AARadioButton
            <AllowedAlternatives(AAElement)>
            AAEdit
            <AllowedAlternatives(AAButton, AACheckBox, AAComboBox, AAListBox, AARadioButton, AAEdit)>
            AAElement

            <AllowedAlternatives(UIAElement)>
            UIAWindow
            <AllowedAlternatives(UIAElement)>
            UIAButton
            <AllowedAlternatives(UIAElement)>
            UIACheckBox
            <AllowedAlternatives(UIAElement)>
            UIARadio
            <AllowedAlternatives(UIAElement)>
            UIAEdit
            <AllowedAlternatives(UIAElement)>
            UIAMenu
            <AllowedAlternatives(UIAElement)>
            UIAMenuItem
            <AllowedAlternatives(UIAElement)>
            UIAComboBox
            <AllowedAlternatives(UIAElement)>
            UIAList
            <AllowedAlternatives(UIAElement)>
            UIAListItem
            <AllowedAlternatives(UIAElement)>
            UIATabControl
            <AllowedAlternatives(UIAElement)>
            UIATabItem
            <AllowedAlternatives(UIAElement)>
            UIAHyperlink
            <AllowedAlternatives(UIAElement)>
            UIATable
            <AllowedAlternatives(UIAElement)>
            UIATreeView
            <AllowedAlternatives(UIAElement)>
            UIATreeViewItem
            <AllowedAlternatives(UIAButton, UIACheckBox, UIARadio, UIAEdit, UIAMenu,
                                 UIAMenuItem, UIAComboBox, UIAList, UIAListItem,
                                 UIATable, UIAHyperlink, UIATreeView, UIATreeViewItem,
                                 UIAWindow, UIATabControl, UIATabItem)>
            UIAElement

            <AllowedAlternatives(WebElement)>
            WebButton
            <AllowedAlternatives(WebElement)>
            WebCheckBox
            <AllowedAlternatives(WebElement)>
            WebForm
            <AllowedAlternatives(WebElement, WebButton)>
            WebLink
            <AllowedAlternatives(WebElement)>
            WebList
            <AllowedAlternatives(WebElement)>
            WebListItem
            <AllowedAlternatives(WebElement)>
            WebMenu
            <AllowedAlternatives(WebElement)>
            WebMenuItem
            <AllowedAlternatives(WebElement)>
            WebRadio
            <AllowedAlternatives(WebElement)>
            WebSlider
            <AllowedAlternatives(WebElement)>
            WebTable
            <AllowedAlternatives(WebElement)>
            WebTableItem
            <AllowedAlternatives(WebElement, WebTextEdit)>
            WebText
            <AllowedAlternatives(WebElement)>
            WebTextEdit
            <AllowedAlternatives(WebElement)>
            WebProgressBar

            <AllowedAlternatives(WebButton, WebCheckBox, WebForm, WebLink, WebList,
                                 WebListItem, WebMenu, WebMenuItem, WebRadio,
                                 WebSlider, WebTable, WebTableItem, WebText,
                                 WebTextEdit, WebProgressBar)>
            WebElement

            <AllowedAlternatives(HTML)>
            HTMLButton
            <AllowedAlternatives(HTML)>
            HTMLCheckBox
            <AllowedAlternatives(HTML)>
            HTMLRadioButton
            <AllowedAlternatives(HTML)>
            HTMLEdit
            <AllowedAlternatives(HTML)>
            HTMLCombo
            <AllowedAlternatives(HTML)>
            HTMLTable
            <AllowedAlternatives(HTMLButton, HTMLCheckBox, HTMLRadioButton, HTMLEdit, HTMLCombo, HTMLTable)>
            HTML

            <AllowedAlternatives(JavaPasswordEdit)>
            JavaEdit
            <AllowedAlternatives(JavaEdit)>
            JavaPasswordEdit

            JavaCheckBox
            JavaRadioButton
            JavaToggleButton
            JavaMenuItem
            JavaMenu

            <AllowedAlternatives(JavaRadioButton, JavaCheckBox, JavaToggleButton, JavaMenuItem, JavaMenu)>
            JavaButton
            JavaScrollBar
            JavaComboBox
            JavaDialog
            JavaTabSelector
            JavaProgressBar
            JavaTrackBar
            JavaUpDown
            JavaTable
            JavaTreeView
            JavaTreeNode
            JavaListBox
            JavaTabControl
            JavaToolBar
            JavaPopupMenu

            <AllowedAlternatives(JavaEdit, JavaPasswordEdit, JavaCheckBox, JavaRadioButton,
             JavaToggleButton, JavaMenuItem, JavaMenu, JavaButton, JavaScrollBar, JavaComboBox,
             JavaDialog, JavaTabSelector, JavaProgressBar, JavaTrackBar, JavaUpDown, JavaTable,
             JavaTreeView, JavaTreeNode, JavaListBox, JavaTabControl, JavaToolBar, JavaPopupMenu)>
            Java

            DDEElement
        End Enum

        ''' <summary>
        ''' Adds the given writable element type to the map maintained by AMI
        ''' </summary>
        ''' <param name="app">The application type for the element.</param>
        ''' <param name="tp">The ID of the element</param>
        ''' <param name="name">The (display) name of the element</param>
        ''' <param name="defaultDatatype">The default data type for the element</param>
        ''' <param name="desc">The description of the element</param>
        Private Shared Sub AddElementType(
         ByVal map As IDictionary(Of ElementType, clsElementTypeInfo), ByVal app As AppTypes,
         ByVal tp As ElementType, ByVal name As String, ByVal defaultDatatype As String,
         ByVal desc As String)
            AddElementType(map, app, tp, name, defaultDatatype, desc, False)
        End Sub

        ''' <summary>
        ''' Adds the given element type to the map maintained by AMI
        ''' </summary>
        ''' <param name="app">The application type for the element.</param>
        ''' <param name="tp">The ID of the element</param>
        ''' <param name="name">The (display) name of the element</param>
        ''' <param name="defaultDatatype">The default data type for the element</param>
        ''' <param name="desc">The description of the element</param>
        ''' <param name="isReadOnly">True to indicate that the element is read only,
        ''' False to indicate that it is writable</param>
        Private Shared Sub AddElementType(
         ByVal map As IDictionary(Of ElementType, clsElementTypeInfo), ByVal app As AppTypes,
         ByVal tp As ElementType, ByVal name As String, ByVal defaultDatatype As String,
         ByVal desc As String, ByVal isReadOnly As Boolean)
            map(tp) = New clsElementTypeInfo(app, tp.ToString(), name, defaultDatatype, desc, isReadOnly)
        End Sub

        ''' <summary>
        ''' Builds the element types for use within AMI
        ''' </summary>
        ''' <param name="extdata">A list of XML documentats that contain external
        ''' element type definitions, to be used in addition to the ones hard-coded
        ''' here.</param>
        Private Shared Sub BuildElementTypes(ByVal extdata As List(Of XmlDocument))

            'Build the map of element types...
            Dim map As New Dictionary(Of ElementType, clsElementTypeInfo)

            'The application itself...
            AddElementType(map, AppTypes.Application, ElementType.Application, My.Resources.Application, "", My.Resources.AnElementRepresentingTheTargetApplicationAsAWhole)
            AddElementType(map, AppTypes.Win32, ElementType.Edit, My.Resources.Edit, "text", My.Resources.AStandardWindowsTextEditField)
            AddElementType(map, AppTypes.Win32, ElementType.Password, My.Resources.Password, "password", My.Resources.AWindowsPasswordTextBox)

            AddElementType(map, AppTypes.Win32, ElementType.ListBox, My.Resources.ListBox, "collection", My.Resources.AStandardWindowsListBox)
            AddElementType(map, AppTypes.Win32, ElementType.RadioButton, My.Resources.RadioButton, "flag", My.Resources.AStandardWindowsRadioButton)
            AddElementType(map, AppTypes.Win32, ElementType.CheckBox, My.Resources.CheckBox, "flag", My.Resources.AStandardWindowsCheckBox)
            AddElementType(map, AppTypes.NET, ElementType.NetCheckBox, My.Resources.CheckBoxNET, "flag", My.Resources.ANETCheckBox)
            AddElementType(map, AppTypes.Win32, ElementType.Button, My.Resources.Button, "text", My.Resources.AStandardWindowsButton, True)
            AddElementType(map, AppTypes.Win32, ElementType.ComboBox, My.Resources.ComboBox, "text", My.Resources.AStandardWindowsComboBox)

            'A standard Windows listview (SysListView32)...
            AddElementType(map, AppTypes.Win32, ElementType.ListView, My.Resources.ListView, "collection", My.Resources.AStandardWindowsListView)

            'An ActiveX listview (ListViewWndClass and ListView20WndClass, which
            'correspond to the ActiveX controls CLSID_MsListView50 and CLSID_MsListView60
            'for which the GUIDs can be found in BPInjAgent)
            AddElementType(map, AppTypes.Win32, ElementType.ListViewAx, My.Resources.ListView, "collection", My.Resources.AnActiveXListView)

            'A standard windows treeview (SysTreeview32)...
            AddElementType(map, AppTypes.Win32, ElementType.Treeview, My.Resources.TreeView, "collection", My.Resources.AStandardWindowsTreeView)

            'An ActiveX treeview (TreeView20WndClass)...
            AddElementType(map, AppTypes.Win32, ElementType.TreeviewAx, My.Resources.TreeView, "collection", My.Resources.AnActiveXTreeView)

            'An ActiveX statusbar (StatusBar20WndClass)...
            AddElementType(map, AppTypes.Win32, ElementType.StatusBarAx, My.Resources.StatusBar, "collection", My.Resources.AnActiveXStatusBar)

            'A standard Windows tab control (SysTabControl32)...
            AddElementType(map, AppTypes.Win32, ElementType.TabControl, My.Resources.TabControl, "number", My.Resources.AStandardWindowsTabControl, True)

            'A standard windows trackbar control (msctls_trackbar32) ...
            AddElementType(map, AppTypes.Win32, ElementType.TrackBar, My.Resources.TrackBar, "number", My.Resources.AStandardWindowsTrackBarControl)

            'A standard windows updown control (msctls_updown32) ...
            AddElementType(map, AppTypes.Win32, ElementType.UpDown, My.Resources.UpDownBox, "", My.Resources.AStandardWindowsUpDownControl)

            'A standard windows date picker control (SysDateTimePick32) ...
            AddElementType(map, AppTypes.Win32, ElementType.DateTimePicker, My.Resources.DateTimePicker, "datetime", My.Resources.AStandardWindowsDateTimePickerControl)

            'A standard windows month calendar picker control (SysMonthCal32) ...
            AddElementType(map, AppTypes.Win32, ElementType.MonthCalPicker, My.Resources.MonthCalendarPicker, "date", My.Resources.AStandardWindowsMonthCalendarPickerControl)

            'A standard windows scrollbar (SCROLLBAR) ...
            AddElementType(map, AppTypes.Win32, ElementType.ScrollBar, My.Resources.ScrollBar, "number", My.Resources.AStandardWindowsScrollBar)

            'A standard windows Label (STATIC) ...
            AddElementType(map, AppTypes.Win32, ElementType.Label, My.Resources.Label, "text", My.Resources.AStandardWindowsLabel)

            'A .NET Link Label.
            AddElementType(map, AppTypes.NET, ElementType.NetLinkLabel, My.Resources.LinkLabelNET, "text", My.Resources.AStandardNETLinkLabel)

            'A standard windows toolbar (TOOLBARWINDOW32 or MSVB_LIB_TOOLBAR) ...
            AddElementType(map, AppTypes.Win32, ElementType.Toolbar, My.Resources.Toolbar, "text", My.Resources.AStandardWindowsToolbar, True)

            'A .NET DataGrid control...
            AddElementType(map, AppTypes.Win32, ElementType.DataGrid, My.Resources.NETDataGrid, "collection", My.Resources.ANETDataGridControl, True)

            'A .NET DataGridView control...
            AddElementType(map, AppTypes.Win32, ElementType.DataGridView, My.Resources.NETDataGridView, "collection", My.Resources.ANETDataGridViewControl, True)

            'A generic window...
            AddElementType(map, AppTypes.Win32, ElementType.Window, My.Resources.Window, "text", My.Resources.AStandardWindowsWindow)

            'An MSFlexGrid (ActiveX Control)...
            AddElementType(map, AppTypes.Win32, ElementType.MSFlexGrid, My.Resources.MSFlexGrid, "collection", My.Resources.AMicrosoftFlexGridActiveXControl)

            'An ApexGrid (ActiveX Control) from dbgrid32.ocx...
            AddElementType(map, AppTypes.Win32, ElementType.ApexGrid, My.Resources.ApexGrid, "collection", My.Resources.AnApexGridActiveXControl)

            'A DTPicker (ActiveX Control) as supplied with VB6...
            AddElementType(map, AppTypes.Win32, ElementType.DTPicker, My.Resources.DTPicker, "datetime", My.Resources.AVB6DatePickerControl)

            'A defined rectangular area within a window...
            AddElementType(map, AppTypes.Win32, ElementType.WindowRect, My.Resources.Region, "text", My.Resources.ARectangularRegionWithinAWindowIdentifiedUsingEitherCoordinatesOrAnImageForRegio)
            AddElementType(map, AppTypes.Win32, ElementType.Win32ListRegion, My.Resources.ListRegion, "collection", My.Resources.AListOfContiguousRegionsWithinAWindow)
            AddElementType(map, AppTypes.Win32, ElementType.Win32GridRegion, My.Resources.GridRegion, "collection", My.Resources.ATableOfRegionsWithinAWindow)

            'A field on a terminal screen, identified by X,Y and Length...
            AddElementType(map, AppTypes.Mainframe, ElementType.TerminalField, My.Resources.Field, "text", My.Resources.ATerminalEmulatorField)

            'An Active Accessibility element.
            AddElementType(map, AppTypes.AA, ElementType.AAElement, My.Resources.ActiveAccessibility, "text", My.Resources.AnActiveAccessibilityElement)
            AddElementType(map, AppTypes.AA, ElementType.AAButton, My.Resources.ButtonAA, "text", My.Resources.AnActiveAccessibilityButtonElement, True)
            AddElementType(map, AppTypes.AA, ElementType.AACheckBox, My.Resources.CheckBoxAA, "flag", My.Resources.AnActiveAccessibilityCheckboxElement)
            AddElementType(map, AppTypes.AA, ElementType.AAComboBox, My.Resources.ComboBoxAA, "text", My.Resources.AnActiveAccessibilityComboBoxElement)
            AddElementType(map, AppTypes.AA, ElementType.AAListBox, My.Resources.ListBoxAA, "text", My.Resources.AnActiveAccessibilityListBoxElement)
            AddElementType(map, AppTypes.AA, ElementType.AARadioButton, My.Resources.RadioButtonAA, "flag", My.Resources.AnActiveAccessibilityRadioButtonElement)
            AddElementType(map, AppTypes.AA, ElementType.AAEdit, My.Resources.EditAA, "text", My.Resources.AnActiveAccessibilityEditBoxElement)

            'A UIA element.
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAElement, My.Resources.UIAutomation, "text", My.Resources.AGenericUIAutomationElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAButton, My.Resources.ButtonUIA, "text", My.Resources.AUIAutomationButtonElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIACheckBox, My.Resources.CheckBoxUIA, "flag", My.Resources.AUIAutomationCheckBoxElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIARadio, My.Resources.RadioButtonUIA, "flag", My.Resources.AUIAutomationRadioButtonElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAComboBox, My.Resources.ComboBoxUIA, "text", My.Resources.AUIAutomationComboBoxElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAEdit, My.Resources.EditBoxUIA, "text", My.Resources.AUIAutomationEditBoxElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAMenu, My.Resources.MenuUIA, "text", My.Resources.AUIAutomationMenuElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAMenuItem, My.Resources.MenuItemUIA, "text", My.Resources.AUIAutomationMenuItemElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAList, My.Resources.ListUIA, "text", My.Resources.AUIAutomationListElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAListItem, My.Resources.ListItemUIA, "text", My.Resources.AUIAutomationListItemElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIATable, My.Resources.TableUIA, "collection", My.Resources.AUIAutomationTableElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIATabControl, My.Resources.TabControlUIA, "text", My.Resources.AUIAutomationTabControl)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIATabItem, My.Resources.TabItemUIA, "text", My.Resources.AUIAutomationTabItem)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAHyperlink, My.Resources.HyperlinkUIA, "text", My.Resources.AUIAutomationHyperlinkElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIATreeView, My.Resources.TreeViewUIA, "text", My.Resources.AUIAutomationTreeViewElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIATreeViewItem, My.Resources.TreeViewItemUIA, "text", My.Resources.AUIAutomationTreeViewItemElement)
            AddElementType(map, AppTypes.UIAutomation, ElementType.UIAWindow, My.Resources.WindowUIA, "text", My.Resources.AUIAutomationWindowElement)

            AddElementType(map, AppTypes.Web, ElementType.WebElement, My.Resources.WebElement, "text", My.Resources.AGenericWebElement)
            AddElementType(map, AppTypes.Web, ElementType.WebButton, My.Resources.ButtonWeb, "text", My.Resources.AWebPageButtonElement)
            AddElementType(map, AppTypes.Web, ElementType.WebCheckBox, My.Resources.CheckBoxWeb, "flag", My.Resources.AWebPageCheckBoxElement)
            AddElementType(map, AppTypes.Web, ElementType.WebForm, My.Resources.FormWeb, "text", My.Resources.AWebFormElement)
            AddElementType(map, AppTypes.Web, ElementType.WebLink, My.Resources.HyperlinkWeb, "text", My.Resources.AWebHyperlinkElement)
            AddElementType(map, AppTypes.Web, ElementType.WebList, My.Resources.ListWeb, "text", My.Resources.AWebListElement)
            AddElementType(map, AppTypes.Web, ElementType.WebListItem, My.Resources.ListItemWeb, "text", My.Resources.AWebListItemElement)
            AddElementType(map, AppTypes.Web, ElementType.WebMenu, My.Resources.MenuWeb, "text", My.Resources.AWebMenuElement)
            AddElementType(map, AppTypes.Web, ElementType.WebMenuItem, My.Resources.MenuItemWeb, "text", My.Resources.AWebMenuItemElement)
            AddElementType(map, AppTypes.Web, ElementType.WebProgressBar, My.Resources.ProgressBarWeb, "number", My.Resources.AWebProgressBarElement)
            AddElementType(map, AppTypes.Web, ElementType.WebRadio, My.Resources.RadioButtonWeb, "text", My.Resources.AWebRadioButtonElement)
            AddElementType(map, AppTypes.Web, ElementType.WebSlider, My.Resources.SliderWeb, "number", My.Resources.AWebSliderElement)
            AddElementType(map, AppTypes.Web, ElementType.WebTable, My.Resources.TableWeb, "text", My.Resources.AWebTableElement)
            AddElementType(map, AppTypes.Web, ElementType.WebTableItem, My.Resources.TableItemWeb, "text", My.Resources.AWebTableItemElement)
            AddElementType(map, AppTypes.Web, ElementType.WebText, My.Resources.TextWeb, "text", My.Resources.AWebTextElement)
            AddElementType(map, AppTypes.Web, ElementType.WebTextEdit, My.Resources.TextEditWeb, "text", My.Resources.AWebTextEditElement)


            'A HTML DOM element.
            AddElementType(map, AppTypes.HTML, ElementType.HTML, My.Resources.HTMLElement, "text", My.Resources.AnHTMLDOMElement)
            AddElementType(map, AppTypes.HTML, ElementType.HTMLButton, My.Resources.HTMLButton, "text", My.Resources.AnHTMLButtonElement, True)
            AddElementType(map, AppTypes.HTML, ElementType.HTMLCheckBox, My.Resources.HTMLCheckBox, "flag", My.Resources.AnHTMLCheckboxElement)
            AddElementType(map, AppTypes.HTML, ElementType.HTMLRadioButton, My.Resources.HTMLRadioButton, "flag", My.Resources.AnHTMLRadioButtonElement)
            AddElementType(map, AppTypes.HTML, ElementType.HTMLEdit, My.Resources.HTMLEdit, "text", My.Resources.AnHTMLEditBoxElement)
            AddElementType(map, AppTypes.HTML, ElementType.HTMLCombo, My.Resources.HTMLComboBox, "text", My.Resources.AnHTMLComboBoxElement)
            AddElementType(map, AppTypes.HTML, ElementType.HTMLTable, My.Resources.HTMLTable, "text", My.Resources.AnHTMLTableElement)

            'Java edit boxes
            AddElementType(map, AppTypes.Java, ElementType.JavaEdit, My.Resources.EditJava, "text", My.Resources.AJavaEditBox)
            AddElementType(map, AppTypes.Java, ElementType.JavaPasswordEdit, My.Resources.PasswordEditJava, "password", My.Resources.AJavaPasswordEditField)

            'Various types of java button. All of these inherit from swing.AbstractButton
            AddElementType(map, AppTypes.Java, ElementType.JavaCheckBox, My.Resources.CheckBoxJava, "flag", My.Resources.AJavaCheckBox)
            AddElementType(map, AppTypes.Java, ElementType.JavaRadioButton, My.Resources.RadioButtonJava, "flag", My.Resources.AJavaRadioButton)
            AddElementType(map, AppTypes.Java, ElementType.JavaToggleButton, My.Resources.ToggleButtonJava, "flag", My.Resources.AToggleButtonIeAButtonThatRetainsItsPressedStateUntilItIsPressedAgain)
            AddElementType(map, AppTypes.Java, ElementType.JavaMenuItem, My.Resources.MenuItemJava, "text", My.Resources.AJavaMenuItem, True)
            AddElementType(map, AppTypes.Java, ElementType.JavaMenu, My.Resources.MenuJava, "text", My.Resources.AJavaMenuOftenFoundAtTheTopOfAMainApplicationWindow, True)

            'The base button type, with its alternatives
            AddElementType(map, AppTypes.Java, ElementType.JavaButton, My.Resources.ButtonJava, "text", My.Resources.AJavaButton, True)

            'Misc java types
            AddElementType(map, AppTypes.Java, ElementType.JavaScrollBar, My.Resources.ScrollbarJava, "number", My.Resources.AJavaScrollbar, True) 'At the moment, no API support for writing. See bug 2993
            AddElementType(map, AppTypes.Java, ElementType.JavaComboBox, My.Resources.ComboBoxJava, "text", My.Resources.AJavaCombobox)
            AddElementType(map, AppTypes.Java, ElementType.JavaDialog, My.Resources.DialogWindowJava, "", My.Resources.ADialogWindowAsOftenUsedInPopupForms, True)
            AddElementType(map, AppTypes.Java, ElementType.JavaTabSelector, My.Resources.TabPageSelectorJava, "", My.Resources.ATabInATabControlWhichWhenSelectedChangesTheCurrentlyViewedPage, True)
            AddElementType(map, AppTypes.Java, ElementType.JavaProgressBar, My.Resources.ProgessBarJava, "number", My.Resources.AJavaProgressBar, True)
            AddElementType(map, AppTypes.Java, ElementType.JavaTrackBar, My.Resources.TrackBarJava, "number", My.Resources.ATrackBarControlWhichConsistsOfAPointerOnASlidingScaleToIndicateANumericValue, True)    'At the moment, no API support for writing. See bug 2993
            AddElementType(map, AppTypes.Java, ElementType.JavaUpDown, My.Resources.UpDownBoxJava, "number", My.Resources.AnUpdownBoxConsistingOfATextboxWithSomeUpdownButtonsForAdjustingTheValueInTheBox)
            AddElementType(map, AppTypes.Java, ElementType.JavaTable, My.Resources.TableJava, "text", My.Resources.AJavaTableAnAnalogousControlToTheWindowsListviewWhenViewedInDetailsMode)
            AddElementType(map, AppTypes.Java, ElementType.JavaTreeView, My.Resources.TreeViewJava, "", My.Resources.AJavaTreeView)
            AddElementType(map, AppTypes.Java, ElementType.JavaTreeNode, My.Resources.TreeNodeJava, "text", My.Resources.AJavaTreeViewNode, True)
            AddElementType(map, AppTypes.Java, ElementType.JavaListBox, My.Resources.ListBoxJava, "text", My.Resources.AJavaListBox)
            AddElementType(map, AppTypes.Java, ElementType.JavaTabControl, My.Resources.TabControlJava, "", My.Resources.AJavaTabControl, True)
            AddElementType(map, AppTypes.Java, ElementType.JavaToolBar, My.Resources.ToolBarJava, "", My.Resources.AJavaToolBar, True)
            AddElementType(map, AppTypes.Java, ElementType.JavaPopupMenu, My.Resources.PopupMenuJava, "", My.Resources.AJavaPopupMenu, True)

            'The generic Java element (via Java Access Bridge)
            AddElementType(map, AppTypes.Java, ElementType.Java, My.Resources.Java, "text", My.Resources.AJavaElement)

            'Dynamic Data Exchange (DDE) support
            AddElementType(map, AppTypes.DDE, ElementType.DDEElement, My.Resources.DDEElement, "text", My.Resources.ADynamicDataExchangeDDEField)

            ' We now have a map of all (internal) element types mapped against their
            ' element type value. We need to transfer this into a dictionary keyed on
            ' a string representation of their ID - This is still necessary because we
            ' need to support element types imported from outside automate which can
            ' thus not be defined within an automate enumeration
            mElementTypes = New Dictionary(Of String, clsElementTypeInfo)

            ' Configure all the alternate types from values set in the element types' attributes
            For Each entry As KeyValuePair(Of ElementType, clsElementTypeInfo) In map
                Dim tp As ElementType = entry.Key
                Dim el As clsElementTypeInfo = entry.Value
                For Each alt As ElementType In AllowedAlternativesAttribute.GetAlternatives(tp)
                    el.AlternateTypes.Add(map(alt))
                Next
                ' And move the element types into the string-based map
                mElementTypes(el.ID) = el
            Next

            'Elements from external data... These go directly into the string ID based map
            For Each x As XmlDocument In extdata
                If x.DocumentElement.Name = "elements" Then
                    For Each el As XmlElement In x.DocumentElement.ChildNodes
                        If el.Name = "element" Then
                            Dim isreadonly As Boolean = True
                            Dim apptype As clsElementTypeInfo.AppTypes
                            Dim apptypes As String = ""
                            Dim id As String = Nothing
                            Dim name As String = Nothing
                            Dim helptext As String = Nothing
                            Dim alternates As New List(Of String)
                            Dim sapidentification As String = Nothing
                            Dim actionqueries As New Dictionary(Of String, String)
                            Dim readqueries As New Dictionary(Of String, String)
                            Dim writequery As String = Nothing
                            Dim datatype As String = "text"

                            id = el.GetAttribute("id")
                            For Each el2 As XmlElement In el.ChildNodes
                                Select Case el2.Name
                                    Case "name"
                                        name = LTools.GetC(el2.InnerText, "elements", "name")
                                    Case "helptext"
                                        helptext = LTools.GetC(el2.InnerText, "elements", "helptext")
                                    Case "apptype"
                                        apptypes = el2.InnerText
                                    Case "alternate"
                                        alternates.Add(el2.InnerText)
                                    Case "sapidentification"
                                        sapidentification = el2.InnerText
                                    Case "datatype"
                                        datatype = el2.InnerText
                                    Case "writequery"
                                        isreadonly = False
                                        writequery = el2.InnerText
                                    Case "readquery"
                                        Dim actionid As String = el2.GetAttribute("action")
                                        If mActionTypes.ContainsKey(actionid) Then
                                            readqueries.Add(actionid, el2.InnerText)
                                        Else
                                            mExtDataErrors.Add(String.Format(My.Resources.InvalidActionID0, actionid))
                                        End If
                                    Case "actionquery"
                                        Dim actionid As String = el2.GetAttribute("action")
                                        If mActionTypes.ContainsKey(actionid) Then
                                            actionqueries.Add(actionid, el2.InnerText)
                                        Else
                                            mExtDataErrors.Add(String.Format(My.Resources.InvalidActionID0, actionid))
                                        End If
                                End Select
                            Next

                            Dim err As String = Nothing
                            If id Is Nothing Then
                                err = My.Resources.MissingIDForElement
                            ElseIf name Is Nothing Then
                                err = String.Format(My.Resources.MissingNameForElement0, id)
                            ElseIf helptext Is Nothing Then
                                err = String.Format(My.Resources.MissingHelptextForElement0, id)
                            ElseIf Not clsEnum.TryParse(apptypes, apptype) Then
                                err = String.Format(My.Resources.InvalidApplicationTypeForElement0, id)
                            End If

                            If err Is Nothing Then
                                Dim eltype As New clsElementTypeInfo(apptype, id, name, datatype, helptext, isreadonly)
                                eltype.SAPIdentification = sapidentification
                                eltype.WriteQuery = writequery
                                If readqueries.Count <> 0 Then
                                    eltype.ReadQueries = readqueries
                                End If
                                If actionqueries.Count <> 0 Then
                                    eltype.ActionQueries = actionqueries
                                End If
                                mElementTypes.Add(eltype.ID, eltype)
                                For Each altid As String In alternates
                                    If mElementTypes.ContainsKey(altid) Then
                                        eltype.AlternateTypes.Add(mElementTypes(altid))
                                    Else
                                        mExtDataErrors.Add(String.Format(My.Resources.MissingAlternate0For1, altid, id))
                                    End If
                                Next
                            Else
                                mExtDataErrors.Add(err)
                            End If

                        End If
                    Next
                End If
            Next

        End Sub

        ''' <summary>
        ''' Adds a 'normal' identifier info object to the shared identifiers map
        ''' representing the given values.
        ''' </summary>
        ''' <param name="id">The ID of the  info to add</param>
        ''' <param name="name">The (user presentable) name of the info</param>
        ''' <param name="datatype">The datatype represented by the identifier info
        ''' </param>
        Private Shared Sub AddIdentifierInfo(
         ByVal id As String, ByVal name As String, ByVal datatype As String)
            AddIdentifierInfo(id, name, datatype, False)
        End Sub

        ''' <summary>
        ''' Adds a 'normal' identifier info object to the shared identifiers map
        ''' representing the given values.
        ''' </summary>
        ''' <param name="id">The ID of the  info to add</param>
        ''' <param name="name">The (user presentable) name of the info</param>
        ''' <param name="datatype">The datatype represented by the identifier info
        ''' </param>
        ''' <param name="multiple">True to indicate that the identifier in question
        ''' can contain multiple values - ie. that the spy operation can return
        ''' multiple instances of this identifier. Typically, multiple identifier
        ''' values are not displayed in the UI without some special handling.</param>
        Private Shared Sub AddIdentifierInfo(ByVal id As String,
         ByVal name As String, ByVal datatype As String, ByVal multiple As Boolean)
            mIdentifiers(id) = New clsIdentifierInfo(
             id, IdentifierType.Normal, name, datatype, multiple)
        End Sub

        ''' <summary>
        ''' Adds a 'parent' identifier info object to the shared identifiers map
        ''' representing the given values.
        ''' </summary>
        ''' <param name="id">The ID of the identifier info to add</param>
        ''' <param name="name">The (user presentable) name of the info</param>
        ''' <param name="datatype">The datatype represented by the identifier info
        ''' </param>
        Private Shared Sub AddParentIdentifierInfo(
         ByVal id As String, ByVal name As String, ByVal datatype As String)
            mIdentifiers(id) =
             New clsIdentifierInfo(id, IdentifierType.Parent, name, datatype)
        End Sub

        ''' <summary>
        ''' Builds the shared map of identifier info objects mapped against their
        ''' IDs, used for spied elements.
        ''' </summary>
        Private Shared Sub BuildIdentifierInfo()

            'Identifiers
            mIdentifiers = New Dictionary(Of String, clsIdentifierInfo)

            'Win32
            AddIdentifierInfo("WindowText", My.Resources.WindowText, "text")
            AddIdentifierInfo("ClassName", My.Resources.ClassName, "text")
            AddIdentifierInfo("WindowText", My.Resources.WindowText, "text")
            AddIdentifierInfo("ClassName", My.Resources.ClassName, "text")
            AddIdentifierInfo("CtrlID", My.Resources.ControlID, "number")
            AddIdentifierInfo("X", My.Resources.X, "number")
            AddIdentifierInfo("Y", My.Resources.Y, "number")
            AddIdentifierInfo("Width", My.Resources.Width, "number")
            AddIdentifierInfo("Height", My.Resources.Height, "number")
            AddIdentifierInfo("Visible", My.Resources.Visible, "flag")
            AddIdentifierInfo("ScreenVisible", My.Resources.ScreenVisible, "flag")
            AddIdentifierInfo("Enabled", My.Resources.Enabled, "flag")
            AddIdentifierInfo("Active", My.Resources.Active, "flag")
            AddIdentifierInfo("Ordinal", My.Resources.Ordinal, "number")
            AddIdentifierInfo("ChildCount", My.Resources.ChildCount, "number")
            AddIdentifierInfo("Style", My.Resources.Style, "number")
            AddIdentifierInfo("AncestorsText", My.Resources.AncestorsText, "text")
            AddIdentifierInfo("TypeName", My.Resources.TypeName, "text")
            AddIdentifierInfo("Screenshot", My.Resources.Screenshot, "image")
            ' Needs to be added for back compatibility when fixing Win32.Checked 
            ' being searched in a query instead of AA Checked
            AddIdentifierInfo("Checked", My.Resources.Checked, "flag")

            'Win32-Parent
            AddParentIdentifierInfo("pWindowText", My.Resources.WindowText, "text")
            AddParentIdentifierInfo("pCtrlID", My.Resources.ControlID, "number")
            AddParentIdentifierInfo("pX", My.Resources.X, "number")
            AddParentIdentifierInfo("pY", My.Resources.Y, "number")
            AddParentIdentifierInfo("pWidth", My.Resources.Width, "number")
            AddParentIdentifierInfo("pHeight", My.Resources.Height, "number")
            AddParentIdentifierInfo("pActive", My.Resources.Active, "flag")
            AddParentIdentifierInfo("pOrdinal", My.Resources.Ordinal, "number")
            AddParentIdentifierInfo("pChildCount", My.Resources.ChildCount, "number")
            AddParentIdentifierInfo("pStyle", My.Resources.Style, "number")
            AddParentIdentifierInfo("pClassName", My.Resources.ClassName, "text")
            AddParentIdentifierInfo("pVisible", My.Resources.Visible, "flag")
            AddParentIdentifierInfo("pScreenVisible", My.Resources.ScreenVisible, "flag")
            AddParentIdentifierInfo("pEnabled", My.Resources.Enabled, "flag")

            ' WindowRect, Win32ListRegion, Win32GridRegion
            AddIdentifierInfo("StartX", My.Resources.StartX, "number")
            AddIdentifierInfo("StartY", My.Resources.StartY, "number")
            AddIdentifierInfo("EndX", My.Resources.EndX, "number")
            AddIdentifierInfo("EndY", My.Resources.EndY, "number")
            AddIdentifierInfo("RetainImage", My.Resources.RetainImage, "flag")
            AddIdentifierInfo("ElementSnapshot", My.Resources.ElementSnapshot, "image")
            AddIdentifierInfo("FontName", My.Resources.FontName, "text")
            AddIdentifierInfo("ImageValue", My.Resources.ImageValue, "image")
            AddIdentifierInfo("LocationMethod", My.Resources.LocationMethod, "text")
            AddIdentifierInfo("RegionPosition", My.Resources.RegionPosition, "text")
            AddIdentifierInfo("ImageSearchPadding", My.Resources.ImageSearchPadding, "text")
            AddIdentifierInfo("RelativeParentID", My.Resources.RelativeParent, "Guid")
            AddIdentifierInfo("ColourTolerance", My.Resources.ColourTolerance, "number")
            AddIdentifierInfo("Greyscale", My.Resources.Greyscale, "flag")

            ' Win32ListRegion
            AddIdentifierInfo("ListDirection", My.Resources.ListDirection, "text")
            AddIdentifierInfo("Padding", My.Resources.Padding, "number")

            ' Win32GridRegion
            AddIdentifierInfo("GridSchema", My.Resources.GridSchema, "text")

            'SAP
            AddIdentifierInfo("ID", My.Resources.ID, "text")
            'note that the following are never actually used directly as an identifier,
            'but they do get returned from spys/snapshots for element identification
            'purposes.
            AddIdentifierInfo("ComponentType", My.Resources.ComponentType, "text")
            AddIdentifierInfo("SubType", My.Resources.SubType, "text")

            'AA
            AddIdentifierInfo("MatchIndex", My.Resources.MatchIndex, "number")
            AddIdentifierInfo("MatchReverse", My.Resources.MatchReverse, "flag")
            AddIdentifierInfo("aX", My.Resources.aX, "number")
            AddIdentifierInfo("aY", My.Resources.aY, "number")
            AddIdentifierInfo("aWidth", My.Resources.aWidth, "number")
            AddIdentifierInfo("aHeight", My.Resources.aHeight, "number")
            AddIdentifierInfo("Name", My.Resources.Name, "text")
            AddIdentifierInfo("Description", My.Resources.Description, "text")
            AddIdentifierInfo("Role", My.Resources.Role, "text")
            AddIdentifierInfo("State", My.Resources.State, "text")
            AddIdentifierInfo("Value", My.Resources.Value, "text")
            AddIdentifierInfo("Value2", My.Resources.Value, "text")
            AddIdentifierInfo("KeyboardShortcut", My.Resources.KeyboardShortcut, "text")
            AddIdentifierInfo("DefaultAction", My.Resources.DefaultAction, "text")
            AddIdentifierInfo("ElementCount", My.Resources.ElementCount, "number")
            AddIdentifierInfo("Unavailable", My.Resources.Unavailable, "flag")
            AddIdentifierInfo("Selected", My.Resources.Selected, "flag")
            AddIdentifierInfo("Focused", My.Resources.Focused, "flag")
            AddIdentifierInfo("Pressed", My.Resources.Pressed, "flag")
            AddIdentifierInfo("aChecked", My.Resources.aChecked, "flag")
            AddIdentifierInfo("Mixed", My.Resources.Mixed, "flag")
            AddIdentifierInfo("ReadOnly", My.Resources.xReadOnly, "flag")
            AddIdentifierInfo("Hottracked", My.Resources.Hottracked, "flag")
            AddIdentifierInfo("Default", My.Resources.xDefault, "flag")
            AddIdentifierInfo("Expanded", My.Resources.Expanded, "flag")
            AddIdentifierInfo("Collapsed", My.Resources.Collapsed, "flag")
            AddIdentifierInfo("Busy", My.Resources.Busy, "flag")
            AddIdentifierInfo("Floating", My.Resources.Floating, "flag")
            AddIdentifierInfo("Marqueed", My.Resources.Marqueed, "flag")
            AddIdentifierInfo("Animated", My.Resources.Animated, "flag")
            AddIdentifierInfo("Invisible", My.Resources.Invisible, "flag")
            AddIdentifierInfo("Offscreen", My.Resources.Offscreen, "flag")
            AddIdentifierInfo("Sizeable", My.Resources.Sizeable, "flag")
            AddIdentifierInfo("Moveable", My.Resources.Moveable, "flag")
            AddIdentifierInfo("SelfVoicing", My.Resources.SelfVoicing, "flag")
            AddIdentifierInfo("Focusable", My.Resources.Focusable, "flag")
            AddIdentifierInfo("Selectable", My.Resources.Selectable, "flag")
            AddIdentifierInfo("Linked", My.Resources.Linked, "flag")
            AddIdentifierInfo("Traversed", My.Resources.Traversed, "flag")
            AddIdentifierInfo("Multiselectable", My.Resources.Multiselectable, "flag")
            AddIdentifierInfo("Extselectable", My.Resources.Extselectable, "flag")
            AddIdentifierInfo("Alert_low", My.Resources.AlertLow, "flag")
            AddIdentifierInfo("Alert_medium", My.Resources.AlertMedium, "flag")
            AddIdentifierInfo("Alert_high", My.Resources.AlertHigh, "flag")

            'AA-Parent
            AddParentIdentifierInfo("paX", My.Resources.aX, "number")
            AddParentIdentifierInfo("paY", My.Resources.aY, "number")
            AddParentIdentifierInfo("paWidth", My.Resources.aWidth, "number")
            AddParentIdentifierInfo("paHeight", My.Resources.aHeight, "number")
            AddParentIdentifierInfo("pName", My.Resources.Name, "text")
            AddParentIdentifierInfo("pDescription", My.Resources.Description, "text")
            AddParentIdentifierInfo("pRole", My.Resources.Role, "text")
            AddParentIdentifierInfo("pID", My.Resources.ID, "text")
            AddParentIdentifierInfo("pState", My.Resources.State, "text")
            AddParentIdentifierInfo("pValue", My.Resources.Value, "text")
            AddParentIdentifierInfo("pValue2", My.Resources.Value, "text")
            AddParentIdentifierInfo("pKeyboardShortcut", My.Resources.KeyboardShortcut, "text")
            AddParentIdentifierInfo("pDefaultAction", My.Resources.DefaultAction, "text")
            AddParentIdentifierInfo("pElementCount", My.Resources.ElementCount, "number")
            AddParentIdentifierInfo("pUnavailable", My.Resources.Unavailable, "flag")
            AddParentIdentifierInfo("pSelected", My.Resources.Selected, "flag")
            AddParentIdentifierInfo("pFocused", My.Resources.Focused, "flag")
            AddParentIdentifierInfo("pPressed", My.Resources.Pressed, "flag")
            AddParentIdentifierInfo("pChecked", My.Resources.Checked, "flag")
            AddParentIdentifierInfo("paChecked", My.Resources.Checked, "flag")
            AddParentIdentifierInfo("pMixed", My.Resources.Mixed, "flag")
            AddParentIdentifierInfo("pReadOnly", My.Resources.xReadOnly, "flag")
            AddParentIdentifierInfo("pHottracked", My.Resources.Hottracked, "flag")
            AddParentIdentifierInfo("pDefault", My.Resources.xDefault, "flag")
            AddParentIdentifierInfo("pExpanded", My.Resources.Expanded, "flag")
            AddParentIdentifierInfo("pCollapsed", My.Resources.Collapsed, "flag")
            AddParentIdentifierInfo("pBusy", My.Resources.Busy, "flag")
            AddParentIdentifierInfo("pFloating", My.Resources.Floating, "flag")
            AddParentIdentifierInfo("pMarqueed", My.Resources.Marqueed, "flag")
            AddParentIdentifierInfo("pAnimated", My.Resources.Animated, "flag")
            AddParentIdentifierInfo("pInvisible", My.Resources.Invisible, "flag")
            AddParentIdentifierInfo("pOffscreen", My.Resources.OffScreen_1, "flag")
            AddParentIdentifierInfo("pSizeable", My.Resources.Sizeable, "flag")
            AddParentIdentifierInfo("pMoveable", My.Resources.Moveable, "flag")
            AddParentIdentifierInfo("pSelfVoicing", My.Resources.SelfVoicing, "flag")
            AddParentIdentifierInfo("pFocusable", My.Resources.Focusable, "flag")
            AddParentIdentifierInfo("pSelectable", My.Resources.Selectable, "flag")
            AddParentIdentifierInfo("pLinked", My.Resources.Linked, "flag")
            AddParentIdentifierInfo("pTraversed", My.Resources.Traversed, "flag")
            AddParentIdentifierInfo("pMultiselectable", My.Resources.Multiselectable, "flag")
            AddParentIdentifierInfo("pExtselectable", My.Resources.Extselectable, "flag")
            AddParentIdentifierInfo("pAlert_low", My.Resources.AlertLow, "flag")
            AddParentIdentifierInfo("pAlert_medium", My.Resources.AlertMedium, "flag")
            AddParentIdentifierInfo("pAlert_high", My.Resources.AlertHigh, "flag")

            'UIA
            AddIdentifierInfo("uX", My.Resources.UIAX, "number")
            AddIdentifierInfo("uY", My.Resources.UIAY, "number")
            AddIdentifierInfo("uWidth", My.Resources.UIAWidth, "number")
            AddIdentifierInfo("uHeight", My.Resources.UIAHeight, "number")
            AddIdentifierInfo("uClassName", My.Resources.UIAClassName, "text")
            AddIdentifierInfo("uAutomationId", My.Resources.UIAAutomationId, "text")
            AddIdentifierInfo("uLocalizedControlType", My.Resources.UIALocalizedControlType, "text")
            AddIdentifierInfo("uControlType", My.Resources.UIAControlType, "text")
            AddIdentifierInfo("uName", My.Resources.UIAName, "text")
            AddIdentifierInfo("uIsPassword", My.Resources.UIAPassword, "flag")
            AddIdentifierInfo("uIsRequiredForForm", My.Resources.UIARequired, "flag")
            AddIdentifierInfo("uOrientation", My.Resources.UIAOrientation, "text")
            AddIdentifierInfo("uItemStatus", My.Resources.UIAItemStatus, "text")
            AddIdentifierInfo("uItemType", My.Resources.UIAItemType, "text")
            AddIdentifierInfo("uLabeledBy", My.Resources.UIALabeledBy, "text")
            AddIdentifierInfo("uOffscreen", My.Resources.UIAOffscreen, "flag")
            AddIdentifierInfo("uTopLevelWindowId", My.Resources.UIATopLevelWindowID, "text")
            AddIdentifierInfo("uProcessId", My.Resources.UIAProcessId, "text")
            AddIdentifierInfo("uEnabled", My.Resources.UIAEnabled, "text")
            AddIdentifierInfo("uHelpText", My.Resources.UIAHelpText, "text")
            AddIdentifierInfo("uHasKeyboardFocus", My.Resources.UIAHasKeyboardFocus, "text")
            AddIdentifierInfo("uAcceleratorKey", My.Resources.UIAAcceleratorKey, "text")
            AddIdentifierInfo("uAccessKey", My.Resources.UIAAccessKey, "text")

            ' FIXME: Probably need a few more things here?
            AddParentIdentifierInfo("puControlType", My.Resources.UIAControlType, "text")
            AddParentIdentifierInfo("puLocalizedControlType", My.Resources.UIALocalizedControlType, "text")
            AddParentIdentifierInfo("puClassName", My.Resources.UIAClassName, "text")
            AddParentIdentifierInfo("puName", My.Resources.UIAName, "text")
            AddParentIdentifierInfo("puAutomationId", My.Resources.UIAAutomationId, "text")

            'Web
            AddIdentifierInfo("wX", My.Resources.WebX, "number")
            AddIdentifierInfo("wY", My.Resources.WebY, "number")
            AddIdentifierInfo("wWidth", My.Resources.WebWidth, "number")
            AddIdentifierInfo("wHeight", My.Resources.WebHeight, "number")
            AddIdentifierInfo("wName", My.Resources.WebName, "text")
            AddIdentifierInfo("wId", My.Resources.WebID, "text")
            AddIdentifierInfo("wXPath", My.Resources.WebPath, "text")
            AddIdentifierInfo("wCssSelector", My.Resources.WebCssSelector, "text")
            AddIdentifierInfo("wElementType", My.Resources.WebElementType, "text")
            AddIdentifierInfo("wValue", My.Resources.WebValue, "text")
            AddIdentifierInfo("wPageAddress", My.Resources.WebPageAddress, "text")
            AddIdentifierInfo("wClass", My.Resources.WebClass, "text")
            AddIdentifierInfo("wClientX", My.Resources.WebClientX, "number")
            AddIdentifierInfo("wClientY", My.Resources.WebClientY, "number")
            AddIdentifierInfo("wClientWidth", My.Resources.WebClientWidth, "number")
            AddIdentifierInfo("wClientHeight", My.Resources.WebClientHeight, "number")
            AddIdentifierInfo("wOffsetX", My.Resources.WebOffsetX, "number")
            AddIdentifierInfo("wOffsetY", My.Resources.WebOffsetY, "number")
            AddIdentifierInfo("wOffsetWidth", My.Resources.WebOffsetWidth, "number")
            AddIdentifierInfo("wOffsetHeight", My.Resources.WebOffsetHeight, "number")
            AddIdentifierInfo("wScrollX", My.Resources.WebScrollX, "number")
            AddIdentifierInfo("wScrollY", My.Resources.WebScrollY, "number")
            AddIdentifierInfo("wScrollWidth", My.Resources.WebScrollWidth, "number")
            AddIdentifierInfo("wScrollHeight", My.Resources.WebScrollHeight, "number")
            AddIdentifierInfo("wChildCount", My.Resources.WebChildCount, "number")
            AddIdentifierInfo("wIsEditable", My.Resources.WebElementIsEditable, "flag")
            AddIdentifierInfo("wStyle", My.Resources.WebStyle, "text")
            AddIdentifierInfo("wTabIndex", My.Resources.WebTabIndex, "number")
            AddIdentifierInfo("wInputType", My.Resources.WebInputType, "text")
            AddIdentifierInfo("wAccessKey", My.Resources.WebAccessKey, "text")
            AddIdentifierInfo("wInnerText", My.Resources.WebText, "text")
            AddIdentifierInfo("wSource", My.Resources.WebSource, "text")
            AddIdentifierInfo("wTargetAddress", My.Resources.WebTargetAddress, "text")
            AddIdentifierInfo("wAlt", My.Resources.WebAltText, "text")
            AddIdentifierInfo("wPattern", My.Resources.WebInputPattern, "text")
            AddIdentifierInfo("wRel", My.Resources.WebLinkRelationship, "text")
            AddIdentifierInfo("wLinkTarget", My.Resources.WebLinkTarget, "text")
            AddIdentifierInfo("wPlaceholder", My.Resources.WebPlaceholderText, "text")

            'HTML
            AddIdentifierInfo("Path", My.Resources.Path, "text")
            AddIdentifierInfo("TagName", My.Resources.TagName, "text")
            AddIdentifierInfo("Title", My.Resources.Title, "text")
            AddIdentifierInfo("Link", My.Resources.Link, "text")
            AddIdentifierInfo("InputType", My.Resources.InputType, "text")

            'This identifier is for backwards compatability with old processes, 
            'InputIdentifier2 will be used in newly spied elements.
            AddIdentifierInfo("InputIdentifier", My.Resources.InputIdentifier, "text")
            AddIdentifierInfo("InputIdentifier2", My.Resources.InputIdentifier, "text")
            AddIdentifierInfo("pURL", My.Resources.ParentURL, "text")

            'JAB
            AddIdentifierInfo("VirtualName", My.Resources.VirtualName, "text")
            AddIdentifierInfo("AllowedActions", My.Resources.AllowedActions, "text")
            AddIdentifierInfo("Armed", My.Resources.Armed, "flag")
            AddIdentifierInfo("AncestorCount", My.Resources.AncestorCount, "number")
            AddIdentifierInfo("aAncestorCount", My.Resources.aAncestorCount, "number")
            AddIdentifierInfo("Editable", My.Resources.Editable, "flag")
            AddIdentifierInfo("Expandable", My.Resources.Expandable, "flag")
            AddIdentifierInfo("Horizontal", My.Resources.Horizontal, "flag")
            AddIdentifierInfo("Iconified", My.Resources.Iconified, "flag")
            AddIdentifierInfo("JavaText", My.Resources.JavaText, "text")
            AddIdentifierInfo("KeyBindings", My.Resources.KeyBindings, "text")
            AddIdentifierInfo("Modal", My.Resources.Modal, "flag")
            AddIdentifierInfo("MultipleLine", My.Resources.MultipleLine, "flag")
            AddIdentifierInfo("MultiSelectable", My.Resources.MultiSelectable_1, "flag")
            AddIdentifierInfo("Opaque", My.Resources.Opaque, "flag")
            AddIdentifierInfo("Resizable", My.Resources.Resizeable, "flag")
            AddIdentifierInfo("Showing", My.Resources.Showing, "flag")
            AddIdentifierInfo("SingleLine", My.Resources.SingleLine, "flag")
            AddIdentifierInfo("Transient", My.Resources.Transient, "flag")
            AddIdentifierInfo("Vertical", My.Resources.Vertical, "flag")

            'Terminal
            AddIdentifierInfo("FieldType", My.Resources.FieldType, "text")
            AddIdentifierInfo("FieldText", My.Resources.FieldText, "text")

            'DDE
            AddIdentifierInfo("DDEServerName", My.Resources.DDEServerName, "text")
            AddIdentifierInfo("DDETopicName", My.Resources.DDETopicName, "text")
            AddIdentifierInfo("DDEItemName", My.Resources.DDEItemName, "text")

            'Generic informational note returned by spy/snapshot tools, but
            'almost certainly not usable as an actual identifier.
            'Unlike other idenfitiers, tools might return multiple instances
            'of this. They are intended only for use in diagnostics, etc.
            AddIdentifierInfo("Note", My.Resources.InformationalNote, "text", True)
            AddIdentifierInfo("ScreenBounds", My.Resources.ScreenBounds, "text")

        End Sub


        ''' <summary>
        ''' Constructor
        ''' </summary>
        Public Sub New(info As clsGlobalInfo)
            mGlobalInfo = info
            mTargetApp = Nothing
            mTargetAppInfo = Nothing
        End Sub


        ''' <summary>
        ''' Initialise the configuration. Any application which makes use of AMI
        ''' should call this once on startup. Any error should be dealt with by
        ''' informing the user in an appropriate manner.
        ''' </summary>
        Public Shared Function Init(ByRef sErr As String) As Boolean
            Return ApplicationManagerUtilities.clsConfig.Init(sErr)
        End Function


        ''' <summary>
        ''' The formats of document available
        ''' </summary>
        Public Enum DocumentFormats
            ''' <summary>
            ''' Mediawiki-compatible wiki markup document
            ''' </summary>
            Wiki = 0
            ''' <summary>
            ''' XHTML-strict html document.
            ''' </summary>
            HTML = 1
        End Enum


        ''' <summary>
        ''' Get a list of Element types that are compatible with the given action
        ''' </summary>
        ''' <param name="action">The action in question.</param>
        ''' <param name="read">If True, compatibility is determined for Read stages.
        ''' Otherwise, for Navigate stages.</param>
        ''' <returns>A list of the compatible types.</returns>
        ''' <remarks>This is for documentation generation purposes only.</remarks>
        Private Function GetCompatibleElements(ByVal action As clsActionTypeInfo, ByVal read As Boolean) As List(Of clsElementTypeInfo)
            Dim compat As New List(Of clsElementTypeInfo)

            For Each el As clsElementTypeInfo In mElementTypes.Values
                Dim testActions As List(Of clsActionTypeInfo)
                If read Then
                    testActions = GetAllowedReadActions(el, Me.mTargetAppInfo)
                Else
                    testActions = GetAllowedActions(el, Me.mTargetAppInfo)
                End If
                If testActions.Count > 0 Then
                    For Each testAction As clsActionTypeInfo In testActions
                        If testAction.ID = action.ID Then
                            compat.Add(el)
                            Exit For
                        End If
                    Next
                End If
            Next

            Return compat
        End Function


        Private Function GetCompatibleElements(ByVal condition As clsConditionTypeInfo) As List(Of clsElementTypeInfo)
            Dim compatibleElements As New List(Of clsElementTypeInfo)

            For Each el As clsElementTypeInfo In mElementTypes.Values
                For Each testCondition In GetAllowedConditions(el, Nothing)
                    If testCondition.ID = condition.ID Then
                        compatibleElements.Add(el)
                        Exit For
                    End If
                Next
            Next

            Return compatibleElements
        End Function

        Public Function GetDocumentation(ByVal format As DocumentFormats) As String

            Dim docBuilder As ApplicationManagerUtilities.clsWikiDocumentBuilder
            Select Case format
                Case DocumentFormats.HTML
                    docBuilder = New ApplicationManagerUtilities.clsHTMLDocumentBuilder
                Case Else
                    docBuilder = New ApplicationManagerUtilities.clsWikiDocumentBuilder
            End Select
            docBuilder.BeginDocument(My.Resources.ApplicationManagerOperations)

            'Action definitions...
            docBuilder.CreateHeader(My.Resources.Actions, 1)
            For Each a As clsActionTypeInfo In mActionTypes.Values
                docBuilder.CreateHeader(a.Name, 2, a.ID)
                docBuilder.AppendParagraph(a.HelpText)

                'Add table of arguments if necessary
                If a.Arguments.Count > 0 Then
                    docBuilder.BeginTable(My.Resources.Parameters)
                    docBuilder.BeginTableRow()
                    docBuilder.AppendTableHeader(My.Resources.Name)
                    docBuilder.AppendTableHeader(My.Resources.DataType)
                    docBuilder.AppendTableHeader(My.Resources.Description)
                    docBuilder.EndTableRow()
                    For Each arg As clsArgumentInfo In a.Arguments.Values
                        docBuilder.BeginTableRow(String.Format("BPADataType{0}", arg.DataType)) 'These classes are defined manually at http://cagney:8086/index.php?title=MediaWiki:Common.css
                        docBuilder.AppendTableData(arg.Name, ApplicationManagerUtilities.clsWikiDocumentBuilder.HorizontalAlignment.Center)
                        docBuilder.AppendTableData(arg.DataType, ApplicationManagerUtilities.clsWikiDocumentBuilder.HorizontalAlignment.Center)

                        If Not String.IsNullOrEmpty(arg.Description) Then
                            docBuilder.AppendTableData(arg.Description, ApplicationManagerUtilities.clsWikiDocumentBuilder.HorizontalAlignment.Left)
                        Else
                            docBuilder.AppendLiteralText("<td><span style=""color:red"">" & My.Resources.NoDescriptionFoundPleaseFixThisProblem & "</span></td>")
                        End If
                        docBuilder.EndTableRow()
                    Next
                    docBuilder.EndTable()
                Else
                    docBuilder.AppendParagraph(My.Resources.ThisActionTakesNoParameters)
                End If
                docBuilder.AppendParagraph(String.Format(My.Resources.TheInternalIDForThisActionIs0, a.ID))

                'Add a list of elements for which this action is valid
                docBuilder.AppendParagraph(My.Resources.WorksWithTheFollowingElementTypes)
                Dim compat As List(Of clsElementTypeInfo) = GetCompatibleElements(a, False)
                Dim compatr As List(Of clsElementTypeInfo) = GetCompatibleElements(a, True)
                If compat.Count > 0 Or compatr.Count > 0 Then
                    docBuilder.BeginList()
                    For Each el As clsElementTypeInfo In compat
                        docBuilder.BeginListItem()
                        docBuilder.CreateLink("#" & ApplicationManagerUtilities.clsHTMLDocumentBuilder.EspapeHeaderCharacters(el.ID), el.Name)
                        docBuilder.AppendLiteralText(My.Resources.xElements)
                        docBuilder.EndListItem()
                    Next
                    For Each el As clsElementTypeInfo In compatr
                        docBuilder.BeginListItem()
                        docBuilder.CreateLink("#" & ApplicationManagerUtilities.clsHTMLDocumentBuilder.EspapeHeaderCharacters(el.ID), el.Name)
                        docBuilder.AppendLiteralText(My.Resources.ElementsReadStage)
                        docBuilder.EndListItem()
                    Next
                    docBuilder.EndList()
                End If

                'Create notes
                If a.RequiresFocus Then
                    docBuilder.AppendParagraph(My.Resources.GlobalActionTheTargetApplicationMustHaveFocus)
                End If

            Next

            'Condition definitions...
            docBuilder.CreateHeader(My.Resources.Conditions, 1)
            For Each c As clsConditionTypeInfo In mConditionTypes.Values
                docBuilder.CreateHeader(c.Name, 2, c.ID)
                docBuilder.AppendParagraph(c.HelpText)

                docBuilder.AppendParagraph(My.Resources.WorksWithTheFollowingElementTypes)
                Dim CompatibleElements As List(Of clsElementTypeInfo) = Me.GetCompatibleElements(c)
                If CompatibleElements.Count > 0 Then
                    docBuilder.BeginList()
                    For Each CompatibleElement As clsElementTypeInfo In CompatibleElements
                        docBuilder.BeginListItem()
                        docBuilder.CreateLink("#" & ApplicationManagerUtilities.clsHTMLDocumentBuilder.EspapeHeaderCharacters(CompatibleElement.ID), CompatibleElement.Name)
                        docBuilder.AppendLiteralText(My.Resources.xElements)
                        docBuilder.EndListItem()
                    Next
                    docBuilder.EndList()
                End If
            Next

            'Element type defintions...
            docBuilder.CreateHeader(My.Resources.ElementTypes, 1)
            For Each el As clsElementTypeInfo In mElementTypes.Values
                docBuilder.CreateHeader(el.Name, 2, el.ID)
                docBuilder.AppendParagraph(el.HelpText)
                Dim acts As List(Of clsActionTypeInfo)
                acts = GetAllowedActions(el, Me.mTargetAppInfo)
                If acts.Count > 0 Then
                    docBuilder.AppendParagraph(My.Resources.SupportsTheFollowingActions)
                    docBuilder.BeginList()
                    For Each a As clsActionTypeInfo In acts
                        docBuilder.BeginListItem()
                        docBuilder.CreateLink("#" & ApplicationManagerUtilities.clsHTMLDocumentBuilder.EspapeHeaderCharacters(a.ID), a.Name)
                        docBuilder.EndListItem()
                    Next
                    docBuilder.EndList()
                End If
                acts = GetAllowedReadActions(el, Me.mTargetAppInfo)
                If acts.Count > 0 Then
                    docBuilder.AppendParagraph(My.Resources.SupportsTheFollowingReadActions)
                    docBuilder.BeginList()
                    For Each a As clsActionTypeInfo In acts
                        docBuilder.BeginListItem()
                        docBuilder.CreateLink("#" & ApplicationManagerUtilities.clsHTMLDocumentBuilder.EspapeHeaderCharacters(a.ID), a.Name)
                        docBuilder.EndListItem()
                    Next
                    docBuilder.EndList()
                End If
                Dim conds = GetAllowedConditions(el, Nothing)
                If conds.Count > 0 Then
                    docBuilder.AppendParagraph(My.Resources.SupportsTheFollowingConditions)
                    docBuilder.BeginList()
                    For Each c As clsConditionTypeInfo In conds
                        docBuilder.BeginListItem()
                        docBuilder.CreateLink("#" & ApplicationManagerUtilities.clsHTMLDocumentBuilder.EspapeHeaderCharacters(c.ID), c.Name)
                        docBuilder.EndListItem()
                    Next
                    docBuilder.EndList()
                End If
                If el.AlternateTypes.Count > 0 Then
                    docBuilder.AppendParagraph(My.Resources.TheFollowingAlternateTypesCanBeSelected)
                    Dim bFirst As Boolean = True
                    docBuilder.BeginList()
                    For Each alt_el As clsElementTypeInfo In el.AlternateTypes
                        docBuilder.BeginListItem()
                        docBuilder.CreateLink("#" & ApplicationManagerUtilities.clsHTMLDocumentBuilder.EspapeHeaderCharacters(alt_el.ID), alt_el.Name)
                        docBuilder.EndListItem()
                    Next
                    docBuilder.EndList()
                End If
                docBuilder.AppendParagraph(String.Format(My.Resources.InternalID0, el.ID))
            Next

            'Application types...
            docBuilder.CreateHeader(My.Resources.ApplicationTypes, 1)
            For Each apptype As clsApplicationTypeInfo In GetApplicationTypes()
                docBuilder.CreateHeader(apptype.Name, 2)
                docBuilder.AppendParagraph(apptype.Description)
                docBuilder.AppendParagraph(String.Format(My.Resources.InternalID0, apptype.ID))
                If apptype.SubTypes.Count > 0 Then
                    docBuilder.CreateHeader(My.Resources.Subtypes, 3)
                    For Each apptype2 As clsApplicationTypeInfo In apptype.SubTypes
                        docBuilder.CreateHeader(apptype2.Name, 4)
                        docBuilder.AppendParagraph(apptype2.Description)
                        docBuilder.AppendParagraph(String.Format(My.Resources.InternalID0, apptype2.ID))
                    Next
                End If
            Next

            docBuilder.EndDocument()
            Return docBuilder.ToString()
        End Function


        ''' <summary>
        ''' Handler for the Terminated and Disconnected events from the target
        ''' application.
        ''' </summary>
        Private Sub TargetAppNoLongerConnected() Handles mTargetApp.Disconnected
            If mTargetApp Is Nothing Then Return
            mTargetApp.Dispose()
            mTargetApp = Nothing
            RaiseEvent ApplicationStatusChanged(
             mTargetAppInfo, ApplicationStatus.NotLaunched)
        End Sub

        ''' <summary>
        ''' Get information about the currently selected target application type.
        ''' </summary>
        ''' <returns>The clsApplicationTypeInfo that represents details of the
        ''' application type, or Nothing if one has not been set.</returns>
        Public Function GetTargetAppInfo() As clsApplicationTypeInfo
            Return mTargetAppInfo
        End Function


        ''' <summary>
        ''' Set details of the target application we are working with. These are the
        ''' settings that will be used by both Launch() and DoAction().
        ''' </summary>
        ''' <param name="appInfo">A fully populated clsApplicationTypeInfo class,
        ''' including all the parameters.</param>
        ''' <param name="Err">Carries error message in the event of
        ''' an error.</param>
        ''' <returns>Returns true on success; returns false in the event
        ''' of an error (eg the license does not permit the use of this
        ''' application type).</returns>
        Public Function SetTargetApplication(ByRef appInfo As clsApplicationTypeInfo, ByRef Err As clsAMIMessage) As Boolean

            '*****************************************************
            'Temporary backwards compatibility hack for
            'developer convenience. Remove at any time
            'if you wish, but be aware that if you do then
            'existing Demo processes using this application type
            'will break. PJW 18-07-07
            If appInfo IsNot Nothing Then
                If appInfo.ID = clsApplicationTypeInfo.Win32ApplicationID Then
                    Dim NewInfo As New clsApplicationTypeInfo(clsApplicationTypeInfo.Win32LaunchID, appInfo.Name)
                    NewInfo.Parameters.AddRange(appInfo.Parameters)
                    NewInfo.ParentType = appInfo.ParentType
                    NewInfo.SubTypes.AddRange(appInfo.SubTypes)
                    NewInfo.Enabled = appInfo.Enabled
                    appInfo = NewInfo
                End If
            End If
            '*****************************************************

            mTargetAppInfo = appInfo
            Return True
        End Function


        ''' <summary>
        ''' Gets the appropriate UI string to describe the launching of the
        ''' current application.
        ''' </summary>
        ''' <returns>Returns a string suitable for display on a button which is to
        ''' launch/attach the application.</returns>
        Public Function GetLaunchCommandUIString(Optional appInfo As clsApplicationTypeInfo = Nothing) As String
            If appInfo Is Nothing Then appInfo = mTargetAppInfo

            If appInfo IsNot Nothing Then
                Select Case appInfo.ID
                    Case clsApplicationTypeInfo.Win32AttachID, clsApplicationTypeInfo.HTMLAttachID, clsApplicationTypeInfo.JavaAttachID,
                        clsApplicationTypeInfo.BrowserAttachId, clsApplicationTypeInfo.CitrixAttachID, clsApplicationTypeInfo.CitrixJavaAttachID,
                        clsApplicationTypeInfo.CitrixBrowserAttachID
                        Return My.Resources.Attach
                End Select
            End If

            Return My.Resources.Launch
        End Function

        ''' <summary>
        ''' Determines whether a special launch of internet explorer is required for
        ''' windows Vista.
        ''' </summary>
        ''' <returns>Returns true if a special launch is required; false otherwise.</returns>
        Private Function IsVistaIELaunchRequired(ByVal Path As String) As Boolean
            If System.Environment.OSVersion.Version.Major >= 6 Then
                If Path.EndsWith("iexplore.exe") Then
                    Return True
                End If
            End If
            Return False
        End Function

        ''' <summary>
        ''' Launch the target application.
        ''' </summary>
        ''' <param name="Err">On failure, this will contain a description of the
        ''' error</param>
        ''' <param name="args">The arguments supplied which override any parameter
        ''' values set via a call to <see cref="SetTargetApplication">
        ''' SetTargetApplication</see>. Any value which is either absent or null
        ''' within these arguments will be ignored, and the corresponding value will
        ''' be taken from the class data instead. To blank a value, provide an empty
        ''' rather than a null string.
        ''' 
        ''' This dictionary may be a null reference if no values are not to be
        ''' overridden. CG, 1-Oct-2008: If what????</param>
        ''' <returns>Returns True on success</returns>
        Public Function LaunchApplication(ByRef Err As clsAMIMessage, ByVal args As Dictionary(Of String, String), Optional ByRef outputs As Dictionary(Of String, String) = Nothing) As Boolean

            Try

                If mTargetAppInfo Is Nothing Then
                    Err = New clsAMIMessage(clsAMIMessage.CommonMessages.NoTargetAppInformation)
                    Return False
                End If

                'Browsers may have multiple instances so don't worry if they are already running
                If Not mTargetApp Is Nothing AndAlso mTargetAppInfo.ParentType.ID <> "Browser" Then
                    Err = New clsAMIMessage(My.Resources.ApplicationAlreadyLaunched, 32778)
                    Return False
                End If

                Dim optionsval As String = GetOptions(args)

                Dim mode As ProcessMode = GetProcessMode(args)
                Dim externalAppmanTimeout = TimeSpan.Zero
                If Not TryGetApplicationManagerTimeout(args, externalAppmanTimeout) Then
                    Err = New clsAMIMessage(My.Resources.UnableToParseExternalApplicationManagerTimeoutParameter)
                    Return False
                End If
                Select Case mTargetAppInfo.ID
                    Case clsApplicationTypeInfo.Win32LaunchID,
                         clsApplicationTypeInfo.HTMLLaunchID,
                         clsApplicationTypeInfo.CitrixLaunchID
                        Return LaunchApplicationGeneral(args, optionsval, mode, Err, externalAppmanTimeout)

                    Case clsApplicationTypeInfo.BrowserLaunchId,
                         clsApplicationTypeInfo.CitrixBrowserLaunchID
                        Return LaunchApplicationBrowser(args, optionsval, mode, Err, externalAppmanTimeout, outputs)

                    Case clsApplicationTypeInfo.Win32AttachID,
                         clsApplicationTypeInfo.JavaAttachID,
                         clsApplicationTypeInfo.HTMLAttachID,
                         clsApplicationTypeInfo.CitrixAttachID,
                         clsApplicationTypeInfo.CitrixJavaAttachID
                        Return LaunchApplicationAttachFirst(args, optionsval, mode, Err, externalAppmanTimeout)

                    Case clsApplicationTypeInfo.BrowserAttachId,
                         clsApplicationTypeInfo.CitrixBrowserAttachID
                        Return LaunchApplicationBrowserAttachFirst(args, optionsval, mode, Err, externalAppmanTimeout, outputs)

                    Case clsApplicationTypeInfo.ValidMainframeId(mTargetAppInfo)

                        If clsApplicationTypeInfo.HasMainframeLaunchSupport(mTargetAppInfo) Then
                            LaunchMainframe(args, optionsval, mode, Err, externalAppmanTimeout)
                        Else
                            'For backwards compatibility we use startsession
                            Return LaunchOrAttachMainframe(args, optionsval, mode, Err, "startsession", externalAppmanTimeout)
                        End If

                    Case clsApplicationTypeInfo.JavaLaunchID,
                         clsApplicationTypeInfo.CitrixJavaLaunchID

                        Return LaunchApplicationJava(args, optionsval, mode, Err, externalAppmanTimeout)

                    Case Else
                        Err = New clsAMIMessage(String.Format(My.Resources.CantLaunchApplicationType0, mTargetAppInfo.ID), 32788)
                        Return False
                End Select

            Catch ex As Exception
                Err = New clsAMIMessage(String.Format(My.Resources.FailedToLaunch0, ex.Message))
                Return False
            End Try

            Return True
        End Function

        ''' <summary>
        ''' Gets the options argument and generates a query argument
        ''' </summary>
        Private Function GetOptions(args As Dictionary(Of String, String)) As String

            Dim optionsval As String
            optionsval = GetArgumentValue(args, ParamType.String, "Options")
            If optionsval.Length <> 0 Then
                optionsval = " options=" & optionsval
            End If
            Return optionsval
        End Function

        ''' <summary>
        ''' Gets the process mode
        ''' </summary>
        Private Function GetProcessMode(args As Dictionary(Of String, String)) As ProcessMode
            Dim mode = ProcessMode.Internal
            Dim modeval As String = GetArgumentValue(args, ParamType.List, "ProcessMode")
            If modeval.Length > 0 Then clsEnum.TryParse(modeval, mode)
            Return mode
        End Function

        ''' <summary>
        ''' Attach to the target application.
        ''' </summary>
        ''' <param name="args">A list of arguments. This should include the necessary
        ''' info such as the window title, process name, etc.</param>
        ''' <param name="err"></param>
        ''' <returns></returns>
        Private Function AttachApplication(ByVal args As Dictionary(Of String, String), ByRef err As clsAMIMessage, Optional ByRef outputs As Dictionary(Of String, String) = Nothing) As Boolean

            If Not mTargetApp Is Nothing Then
                err = New clsAMIMessage(My.Resources.AlreadyConnectedToAnApplication, 32778)
                Return False
            End If

            Try

                'Do some ad hoc parsing of the collection of window titles.
                'We would prefer not to do it in this manner. See bugs 3631, 3739.
                Dim windowTitles As New List(Of String)
                Dim collectionXml As String = GetArgumentValue(args, ParamType.String, "WindowTitlesCollection")
                If Not String.IsNullOrEmpty(collectionXml) Then
                    Dim xdoc As XmlDocument
                    Try
                        xdoc = New ReadableXmlDocument(collectionXml)
                    Catch ex As Exception
                        err = New clsAMIMessage(String.Format(My.Resources.FailedToParseCollectionOfWindowTitlesXMLEngineReportedError0, ex.Message))
                        Return False
                    End Try
                    For Each toplevelel As XmlElement In xdoc.ChildNodes
                        For Each rowel As XmlElement In toplevelel.ChildNodes
                            If rowel.Name = "row" Then
                                For Each fieldel As XmlElement In rowel.ChildNodes
                                    If fieldel.Name = "field" AndAlso fieldel.GetAttribute("name") = "Window Title" Then
                                        windowTitles.Add(fieldel.GetAttribute("value"))
                                    End If
                                Next
                            End If
                        Next
                    Next
                End If

                'Get Process Name and Window Title parameters
                Dim windowTitle As String = GetArgumentValue(args, ParamType.String, "WindowTitle")
                Dim processName As String = GetArgumentValue(args, ParamType.String, "ProcessName")
                Dim processId As String = GetArgumentValue(args, ParamType.Number, "ProcessID")
                Dim userName As String = GetArgumentValue(args, ParamType.String, "Username")
                Dim browserAttach As Boolean
                Dim trackingId As String = String.Empty

                If IsBrowserApplication(mTargetAppInfo.ID) Then
                    If Not windowTitle.EndsWith("*") Then
                        windowTitle = windowTitle & "*"
                    End If
                    trackingId = Guid.NewGuid().ToString()
                    browserAttach = True
                End If
                If Not String.IsNullOrEmpty(windowTitle) Then windowTitles.Add(windowTitle)

                'We must have something meaningful to work with
                If windowTitles.Count = 0 Then
                    If String.IsNullOrEmpty(processName) AndAlso String.IsNullOrEmpty(processId) Then
                        err = New clsAMIMessage(My.Resources.AtLeastOneOfWindowTitleProcessNameOrProcessIdMustBeSpecified)
                        Return False
                    End If
                End If

                Dim optionsval As String
                optionsval = GetArgumentValue(args, ParamType.String, "Options")
                If optionsval.Length <> 0 Then
                    optionsval = " options=" & optionsval
                End If

                Dim mode As ProcessMode
                clsEnum.TryParse(GetArgumentValue(args, ParamType.List, "ProcessMode"), mode)

                'Prepare to send query string
                Dim bUseJab = UseJab(mTargetAppInfo.ID)
                If mTargetAppInfo.ID = clsApplicationTypeInfo.HTMLAttachID Then
                    Dim useJavaInBrowser As Boolean = Boolean.Parse(GetArgumentValue(args, ParamType.Boolean, "UseJavaInBrowser"))
                    bUseJab = bUseJab OrElse useJavaInBrowser
                End If

                mTargetApp = clsTargetApp.GetTargetApp(mode)

                'Try each window title one at a time until one of them works
                If windowTitles.Count = 0 Then windowTitles.Add("")
                Dim sResType As String = Nothing
                Dim sResult As String = ""
                For Each wt As String In windowTitles

                    Dim sQuery As String
                    sQuery = "attachapplication" & optionsval
                    If Not String.IsNullOrEmpty(wt) Then
                        sQuery &= " windowtitle=" & clsQuery.EncodeValue(wt)
                    End If
                    If Not String.IsNullOrEmpty(processName) Then
                        sQuery &= " processname=" & clsQuery.EncodeValue(processName)
                    End If
                    If Not String.IsNullOrEmpty(processId) Then
                        sQuery &= " processid=" & processId
                    End If
                    If Not String.IsNullOrEmpty(userName) Then
                        sQuery &= " username=" & clsQuery.EncodeValue(userName)
                    End If
                    sQuery &= " jab=" & bUseJab.ToString
                    sQuery &= " browserattach=" & browserAttach.ToString()
                    sQuery &= " trackingId=" & trackingId

                    'Get the "non-invasive" flag from the arguments, if it exists...
                    Dim nonInvasive = GetBooleanArgument(args, "NonInvasive", True)

                    'Never hook browser app.
                    If {clsApplicationTypeInfo.HTMLAttachID, clsApplicationTypeInfo.HTMLLaunchID, clsApplicationTypeInfo.BrowserAttachId, clsApplicationTypeInfo.BrowserLaunchId}.Contains(mTargetAppInfo.ID) Then
                        nonInvasive = True
                    End If

                    sQuery &= $" hook={Not nonInvasive}"

                    Dim childIndex As Integer = 0
                    Dim childIndexString As String = GetArgumentValue(args, ParamType.Number, "ChildIndex")
                    If Not String.IsNullOrEmpty(childIndexString) Then
                        childIndex = Integer.Parse(childIndexString)
                        sQuery &= " childindex=" & childIndex.ToString
                    End If

                    sResult = mTargetApp.ProcessQuery(sQuery)
                    clsQuery.ParseResponse(sResult, sResType, sResult)
                    Dim bRes As Boolean = (sResType = "OK")

                    If bRes Then

                        If outputs IsNot Nothing AndAlso Not String.IsNullOrEmpty(trackingId) Then
                            outputs.Add("trackingid", trackingId)
                        End If

                        RaiseEvent ApplicationStatusChanged(mTargetAppInfo, ApplicationStatus.Launched)
                        Return True
                    End If
                Next

                If sResType = "ERROR" Then
                    err = clsAMIMessage.Parse(sResult)
                Else
                    err = clsAMIMessage.Parse(String.Format("{0} - {1}", sResType, sResult))
                End If
                'If we get here then all window titles have failed
                TargetAppNoLongerConnected()

            Catch ex As Exception
                err = New clsAMIMessage(String.Format(My.Resources.FailedToAttach0, ex.Message))
            End Try

            Return False
        End Function

        Private Shared Function UseJab(appInfoId As String) As Boolean
            Select Case appInfoId
                Case clsApplicationTypeInfo.JavaAttachID, clsApplicationTypeInfo.JavaLaunchID,
                    clsApplicationTypeInfo.CitrixJavaAttachID, clsApplicationTypeInfo.CitrixJavaLaunchID
                    Return True
            End Select
            Return False
        End Function

        Public Shared Function IsBrowserApplication(appInfoId As String) As Boolean
            Return BrowserAppTypes.Any(Function(x) x = appInfoId)
        End Function

        ''' <summary>
        ''' Launches a Java application
        ''' </summary>
        Private Function LaunchApplicationJava(args As Dictionary(Of String, String), optionsval As String, mode As ProcessMode, ByRef Err As clsAMIMessage, timeout As TimeSpan) As Boolean
            Dim bRes As Boolean
            mLogger.Debug("Launching a java application")
            'Get the path, and command line arguments
            Dim PathValue As String = Me.GetArgumentValue(args, ParamType.File, "Path")
            Dim CommandLineParamsValue As String = Me.GetArgumentValue(args, ParamType.String, "CommandLineParams")
            If String.IsNullOrEmpty(PathValue) Then
                Err = New clsAMIMessage(My.Resources.NoExecutableSpecified)
                Return False
            End If

            'Some paths may contain spaces which need to be escaped by enclosing in quotes
            If Not (PathValue.StartsWith("""") AndAlso PathValue.EndsWith("""")) Then
                PathValue = """" & PathValue
                PathValue = PathValue & """"
            End If

            'Get the working directory
            Dim WorkingDirectory As String = Me.GetArgumentValue(args, ParamType.String, "WorkingDirectory")


            mTargetApp = clsTargetApp.GetTargetApp(mode)
            Dim sResult As String, sQuery As String
            sQuery = "startapplication path=" & clsQuery.EncodeValue("java -jar " & PathValue & " " & CommandLineParamsValue)
            sQuery &= " workingdir=" & clsQuery.EncodeValue(WorkingDirectory)
            sQuery &= optionsval & " hook=False jab=True"

            Dim sResType As String = Nothing
            sResult = mTargetApp.ProcessQuery(sQuery, timeout)
            clsQuery.ParseResponse(sResult, sResType, sResult)
            bRes = (sResType = "OK")
            If Not bRes Then
                TargetAppNoLongerConnected()
                If sResType = "ERROR" Then
                    Err = clsAMIMessage.Parse(sResult)
                Else
                    Err = clsAMIMessage.Parse(String.Format("{0} - {1}", sResType, sResult))
                End If
            Else
                RaiseEvent ApplicationStatusChanged(mTargetAppInfo, ApplicationStatus.Launched)
            End If
            Return bRes
        End Function

        ''' <summary>
        ''' Launches a mainframe
        ''' </summary>
        Private Function LaunchMainframe(args As Dictionary(Of String, String), optionsval As String,
                                                    mode As ProcessMode, ByRef Err As clsAMIMessage,
                                                    timeout As TimeSpan) As Boolean
            If mTargetAppInfo Is Nothing Then
                Err = New clsAMIMessage(clsAMIMessage.CommonMessages.NoTargetAppInformation)
                Return False
            End If

            If Not mTargetApp Is Nothing Then
                Err = New clsAMIMessage(My.Resources.MainframeAlreadyLaunched, 32778)
                Return False
            End If

            Return LaunchOrAttachMainframe(args, optionsval, mode, Err, "launchmainframe", timeout)
        End Function

        ''' <summary>
        ''' Attaches to a mainframe
        ''' </summary>
        Private Function AttachMainframe(args As Dictionary(Of String, String), optionsval As String,
                                                    mode As ProcessMode, ByRef Err As clsAMIMessage,
                                                    timeout As TimeSpan) As Boolean
            If Not mTargetApp Is Nothing Then
                Err = New clsAMIMessage(My.Resources.AlreadyAttachedToAMainframe, 32778)
                Return False
            End If

            Return LaunchOrAttachMainframe(args, optionsval, mode, Err, "attachmainframe", timeout)
        End Function

        ''' <summary>
        ''' Launches or attaches to a mainframe application depending on the value
        ''' passed in the sQuery parameter.
        ''' </summary>
        Private Function LaunchOrAttachMainframe(args As Dictionary(Of String, String), optionsval As String,
                                                    mode As ProcessMode, ByRef Err As clsAMIMessage,
                                                    sQuery As String, timeout As TimeSpan) As Boolean

            'Get parameters - session ID, Session File Path, Port
            Dim sessionID = GetArgumentValue(args, ParamType.List, "Session ID")
            Dim sessionFilePath = GetArgumentValue(args, ParamType.File, "Path")
            'The RMD emulator uses a port parameter, but we just pass that
            'in as the session file path!
            Dim port = GetArgumentValue(args, ParamType.Number, "Port")
            If String.IsNullOrEmpty(sessionFilePath) AndAlso Not String.IsNullOrEmpty(port) Then
                sessionFilePath = port
            End If

            'reject bad parameters
            If String.IsNullOrEmpty(sessionID) AndAlso String.IsNullOrEmpty(sessionFilePath) Then
                Err = New clsAMIMessage(My.Resources.NoSessionIdOrSessionFileSpecified)
                Return False
            End If

            'Take last three letters to parse as TerminalType
            Dim terminalType = clsApplicationTypeInfo.ParseTerminalType(mTargetAppInfo)

            'Wait parameters
            Dim waitTimeout = GetArgumentValue(args, ParamType.Number, "WaitTimeout")
            Dim waitSleepTime = GetArgumentValue(args, ParamType.Number, "WaitSleepTime")


            'Attachmate-specific info
            Dim ATMVariant = ""
            If clsApplicationTypeInfo.IsAttachmateMainframe(mTargetAppInfo) Then
                ATMVariant = GetArgumentValue(args, ParamType.List, "Attachmate Variant")
                Select Case ATMVariant
                    Case "Extra!"
                        ATMVariant = "EXTRA"
                    Case "Kea!"
                        ATMVariant = "KEA"
                    Case "Kea! for HP"
                        ATMVariant = "KEA_HP"
                    Case "Irma"
                        ATMVariant = "IRMA"
                    Case "InfoConnect"
                        ATMVariant = "ICONN"
                    Case "Rally!"
                        ATMVariant = "RALLY"
                    Case Else
                        ATMVariant = "EXTRA"
                End Select
            End If

            'Launch application using session type
            mTargetApp = clsTargetApp.GetTargetApp(mode)

            sQuery &= " sessionfile=" & clsQuery.EncodeValue(sessionFilePath)
            sQuery &= " sessionid=" & clsQuery.EncodeValue(sessionID)
            sQuery &= " terminaltype=" & clsQuery.EncodeValue(terminalType.ToString())
            sQuery &= " waittimeout=" & clsQuery.EncodeValue(waitTimeout)
            sQuery &= " waitsleeptime=" & clsQuery.EncodeValue(waitSleepTime)
            sQuery &= " atmvariant=" & clsQuery.EncodeValue(ATMVariant)

            'Generic EHLLAPI parameters
            If clsApplicationTypeInfo.IsGenericHLLAPIMainframe(mTargetAppInfo) Then
                Dim codepage As String = GetArgumentValue(args, ParamType.List, "Code Page")

                Dim sessionDllName As String = GetArgumentValue(args, ParamType.String, "DLL Name")
                Dim sessionDllEntryPoint As String = GetArgumentValue(args, ParamType.String, "DLL Entry Point")

                Dim convention As String = Nothing
                Select Case GetArgumentValue(args, ParamType.List, "Calling Convention")
                    Case "Windows API"
                        convention = Runtime.InteropServices.CallingConvention.Winapi.ToString
                    Case "C Declaration"
                        convention = Runtime.InteropServices.CallingConvention.Cdecl.ToString
                    Case "Standard Call"
                        convention = Runtime.InteropServices.CallingConvention.StdCall.ToString
                End Select

                Dim sessionType As String
                Select Case GetArgumentValue(args, ParamType.List, "Session Type")
                    Case "Standard"
                        sessionType = SessionStartInfo.SessionTypes.Normal.ToString
                    Case "Enhanced"
                        sessionType = SessionStartInfo.SessionTypes.Enhanced.ToString
                    Case Else
                        sessionType = SessionStartInfo.SessionTypes.NotImplemented.ToString
                End Select

                sQuery &= " codepage=" & codepage
                sQuery &= " sessiondllname=" & clsQuery.EncodeValue(sessionDllName)
                sQuery &= " sessiondllentrypoint=" & clsQuery.EncodeValue(sessionDllEntryPoint)
                sQuery &= " sessionconvention=" & clsQuery.EncodeValue(convention)
                sQuery &= " sessiontype=" & clsQuery.EncodeValue(sessionType)
            End If

            sQuery &= optionsval
            Dim sResType As String = Nothing, sResult As String
            sResult = mTargetApp.ProcessQuery(sQuery, timeout)
            clsQuery.ParseResponse(sResult, sResType, sResult)
            Dim bRes = (sResType = "OK")
            If Not bRes Then
                Err = clsAMIMessage.Parse(sResult)
                TargetAppNoLongerConnected()
                Return False
            Else
                RaiseEvent ApplicationStatusChanged(mTargetAppInfo, ApplicationStatus.Launched)
            End If

            Return True
        End Function

        ''' <summary>
        ''' Attaches to an application but if attaching fails then launches it.
        ''' </summary>
        Private Function LaunchApplicationAttachFirst(args As Dictionary(Of String, String), optionsval As String, mode As ProcessMode, ByRef Err As clsAMIMessage, timeout As TimeSpan) As Boolean
            Dim bRes As Boolean

            If Me.AttachApplication(args, Err) Then
                Return True
            Else
                'Attach failed so try launching instead

                'Get path of application to launch
                Dim PathValue As String = Me.GetArgumentValue(args, ParamType.File, "Path")
                Dim CommandLineParamsValue As String = Me.GetArgumentValue(args, ParamType.String, "CommandLineParams")
                If String.IsNullOrEmpty(PathValue) Then
                    'Attach failed and no application to launch given,
                    'so we throw the error from the attaching
                    TargetAppNoLongerConnected()
                    Return False
                End If

                'Formulate launch query
                mTargetApp = clsTargetApp.GetTargetApp(mode)
                Dim bUseJab As Boolean = (mTargetAppInfo.ID = clsApplicationTypeInfo.JavaLaunchID OrElse
                    mTargetAppInfo.ID = clsApplicationTypeInfo.CitrixJavaLaunchID OrElse
                    mTargetAppInfo.ID = clsApplicationTypeInfo.JavaAttachID OrElse
                    mTargetAppInfo.ID = clsApplicationTypeInfo.CitrixJavaAttachID)

                If bUseJab Then
                    Return LaunchApplicationJava(args, optionsval, mode, Err, timeout)
                End If

                Dim sQuery As String
                sQuery = "startapplication path=" & clsQuery.EncodeValue(PathValue & " " & CommandLineParamsValue)
                sQuery &= " jab=" & bUseJab.ToString()

                'Get the "non-invasive" flag from the arguments, if it exists...
                Dim nonInvasive = GetBooleanArgument(args, "NonInvasive", True)
                sQuery &= $" hook={Not nonInvasive}"

                Dim excludehtcval As String
                excludehtcval = GetArgumentValue(args, ParamType.Boolean, "ExcludeHTC")
                If excludehtcval.Length <> 0 Then
                    sQuery &= " excludehtc=" & excludehtcval
                End If

                Dim activetabonlyval As String
                activetabonlyval = GetArgumentValue(args, ParamType.Boolean, "ActiveTabOnly")
                If activetabonlyval.Length <> 0 Then
                    sQuery &= " activetabonly=" & activetabonlyval
                End If

                sQuery &= optionsval

                Dim sResType As String = Nothing
                Dim sResult As String = mTargetApp.ProcessQuery(sQuery, timeout)
                clsQuery.ParseResponse(sResult, sResType, sResult)
                bRes = (sResType = "OK")
                If Not bRes Then
                    TargetAppNoLongerConnected()
                    If sResType = "ERROR" Then
                        Err = clsAMIMessage.Parse(sResult)
                    Else
                        Err = clsAMIMessage.Parse(String.Format("{0} - {1}", sResType, sResult))
                    End If
                Else
                    RaiseEvent ApplicationStatusChanged(mTargetAppInfo, ApplicationStatus.Launched)
                End If
                Return bRes
            End If
        End Function

        Private Function LaunchApplicationBrowserAttachFirst(args As Dictionary(Of String, String), optionsValue As String, mode As ProcessMode,
                                                             ByRef [error] As clsAMIMessage, timeout As TimeSpan, ByRef outputs As Dictionary(Of String, String)) As Boolean

            If AttachApplication(args, [error]) Then
                Return True
            Else
                Return LaunchApplicationBrowser(args, optionsValue, mode, [error], timeout, outputs)
            End If
        End Function

        ''' -------------------------------------------------------------------------
        ''' <summary>
        ''' Launches the application this is for internal and only used for clsApplicationTypeInfo.Win32LaunchID
        ''' clsApplicationTypeInfo.HTMLLaunchID, the more general version of this function is <cref>LaunchApplication</cref>
        ''' </summary>
        Private Function LaunchApplicationGeneral(args As Dictionary(Of String, String), optionsval As String, mode As ProcessMode, ByRef Err As clsAMIMessage, timeout As TimeSpan) As Boolean
            Dim bRes As Boolean

            'Get the path
            Dim PathValue As String = Me.GetArgumentValue(args, ParamType.File, "Path")
            If String.IsNullOrEmpty(PathValue) Then
                Err = New clsAMIMessage(My.Resources.NoExecutableSpecified)
                Return False
            End If

            'get the command line arguments
            Dim CommandLineParamsValue As String = Me.GetArgumentValue(args, ParamType.String, "CommandLineParams")

            'Get the working directory
            Dim WorkingDirectory As String = Me.GetArgumentValue(args, ParamType.String, "WorkingDirectory")

            mTargetApp = clsTargetApp.GetTargetApp(mode)
            Dim sResult As String, sQuery As String
            If mTargetAppInfo.ID = clsApplicationTypeInfo.HTMLLaunchID AndAlso IsVistaIELaunchRequired(PathValue) Then
                sQuery = "StartIExploreOnVista HTMLCommandline=" & clsQuery.EncodeValue(CommandLineParamsValue)
            Else
                sQuery = "startapplication path=" & clsQuery.EncodeValue(PathValue & " " & CommandLineParamsValue) & " workingdir=" & clsQuery.EncodeValue(WorkingDirectory)
            End If

            'Get the "non-invasive" flag from the arguments
            Dim nonInvasive = GetBooleanArgument(args, "NonInvasive", True)

            If mTargetAppInfo.ID = clsApplicationTypeInfo.HTMLLaunchID Then
                'Never use hooking in browser application
                nonInvasive = True

                Dim JavaValue As Boolean = Boolean.Parse(Me.GetArgumentValue(args, ParamType.Boolean, "UseJavaInBrowser"))
                sQuery &= " jab=" & JavaValue.ToString
            End If

            sQuery &= $" hook={Not nonInvasive}"

            Dim excludehtcval As String
            excludehtcval = GetArgumentValue(args, ParamType.Boolean, "ExcludeHTC")
            If excludehtcval.Length <> 0 Then
                sQuery &= " excludehtc=" & excludehtcval
            End If

            Dim activetabonlyval As String
            activetabonlyval = GetArgumentValue(args, ParamType.Boolean, "ActiveTabOnly")
            If activetabonlyval.Length <> 0 Then
                sQuery &= " activetabonly=" & activetabonlyval
            End If

            sQuery &= optionsval

            Dim sResType As String = Nothing
            sResult = mTargetApp.ProcessQuery(sQuery, timeout)
            clsQuery.ParseResponse(sResult, sResType, sResult)
            bRes = (sResType = "OK")
            If Not bRes Then
                TargetAppNoLongerConnected()
                If sResType = "ERROR" Then
                    Err = clsAMIMessage.Parse(sResult)
                Else
                    Err = clsAMIMessage.Parse(String.Format("{0} - {1}", sResType, sResult))
                End If
            Else
                RaiseEvent ApplicationStatusChanged(mTargetAppInfo, ApplicationStatus.Launched)
            End If
            Return bRes
        End Function


        Private Function LaunchApplicationBrowser(args As Dictionary(Of String, String), optionsValue As String, mode As ProcessMode,
                                                  ByRef [error] As clsAMIMessage, timeout As TimeSpan, ByRef outputs As Dictionary(Of String, String)) As Boolean
            Dim result As Boolean

            'Get the path
            Dim pathValue As String = GetArgumentValue(args, ParamType.File, "Path")
            If String.IsNullOrEmpty(pathValue) Then
                [error] = New clsAMIMessage(My.Resources.NoExecutableSpecified  & Environment.NewLine & Environment.NewLine & [error].Message )
                Return False
            End If

            'get the command line arguments
            Dim commandLineParameters As String = GetArgumentValue(args, ParamType.String, "CommandLineParams")

            'Get the working directory
            Dim workingDirectory As String = GetArgumentValue(args, ParamType.String, "WorkingDirectory")
            Dim trackingId = Guid.NewGuid()

            mTargetApp = clsTargetApp.GetTargetApp(mode)
            Dim resultString As String, query As String
            query = "LaunchBrowserApplication path=" & clsQuery.EncodeValue(pathValue & " " & commandLineParameters) & " workingdir=" & clsQuery.EncodeValue(workingDirectory) & " trackingid=" & clsQuery.EncodeValue(trackingId.ToString())

            Dim browserLaunchTimeout = GetArgumentValue(args, ParamType.String, "BrowserLaunchTimeout")
            If browserLaunchTimeout <> String.Empty Then
                query &= " browserlaunchtimeout=" & clsQuery.EncodeValue(browserLaunchTimeout)
            End If
            query &= optionsValue

            Dim resultType As String = Nothing
            resultString = mTargetApp.ProcessQuery(query, timeout)
            clsQuery.ParseResponse(resultString, resultType, resultString)
            result = (resultType = "OK")
            If Not result Then
                TargetAppNoLongerConnected()
                If resultType = "ERROR" Then
                    [error] = clsAMIMessage.Parse(resultString)
                Else
                    [error] = clsAMIMessage.Parse(String.Format("{0} - {1}", resultType, resultString))
                End If
            Else
                If outputs IsNot Nothing Then
                    outputs.Add("trackingid", trackingId.ToString())
                End If
                RaiseEvent ApplicationStatusChanged(mTargetAppInfo, ApplicationStatus.Launched)
            End If
            Return result
        End Function


        ''' <summary>
        ''' Gets the specified argument, (if provided) from the supplied arguments,
        ''' or from class data where necessary.
        ''' </summary>
        ''' <param name="args">The list of arguments supplied. A value is taken from
        ''' here by preference when it is present in the dictionary exists and its
        ''' value is not null. The dictionary may be a null reference to simply use
        ''' the class data instead.</param>
        ''' <param name="parameterName">The name of the parameter sought.</param>
        ''' <param name="parameterType">The expected type of the parameter. The value
        ''' will only be returned if both the name and type match.</param>
        ''' <returns>Returns the value from the supplied dictionary of arguments,
        ''' if it is present and not null. Otherwise looks up the value in the class
        ''' data, provided it is not null. Failing this, returns an empty (but not
        ''' null) string.</returns>
        Private Function GetArgumentValue(args As IDictionary(Of String, String), parameterType As ParamType, parameterName As String) As String
            'Get the value from the arguments, if it exists. An absent or null value
            'here indicates that it should be taken from the class data instead.
            'This policy is described at http://cagney:8086/index.php?title=AMI_Runtime_Application_Parameters
            If (args IsNot Nothing) AndAlso args.ContainsKey(parameterName) AndAlso (args(parameterName) IsNot Nothing) Then
                Return args(parameterName)
            Else
                For Each p As clsApplicationParameter In mTargetAppInfo.Parameters
                    If p.ParameterType = parameterType AndAlso p.Name = parameterName AndAlso (Not String.IsNullOrEmpty(p.Value)) Then
                        Return p.Value
                    End If
                Next
            End If

            Return String.Empty
        End Function

        Private Function GetBooleanArgument(args As IDictionary(Of String, String), parameterName As String, defaultValue As Boolean) As Boolean
            Dim value = GetArgumentValue(args, ParamType.Boolean, parameterName)
            If String.IsNullOrEmpty(value) Then Return defaultValue
            Dim result As Boolean
            If Not Boolean.TryParse(value, result) Then Return defaultValue
            Return result
        End Function

        ''' <summary>
        ''' Detaches from the target application.
        ''' </summary>
        ''' <param name="Err">Carries back error information when the return
        ''' value is false.</param>
        ''' <returns>True on success, False otherwise.</returns>
        Public Function DetachApplication(ByRef Err As clsAMIMessage) As Boolean
            If mTargetApp Is Nothing Then
                Err = New clsAMIMessage(clsAMIMessage.CommonMessages.NotConnected)
                Return False
            End If

            Dim sQuery = If(clsApplicationTypeInfo.HasMainframeDetachSupport(mTargetAppInfo), "DetachMainframe", "DetachApplication")
            Dim sResType As String = Nothing
            Dim sResult As String = mTargetApp.ProcessQuery(sQuery)
            clsQuery.ParseResponse(sResult, sResType, sResult)
            Dim Success As Boolean = (sResType = "OK")
            If Not Success Then
                If sResType = "ERROR" Then
                    Err = clsAMIMessage.Parse(sResult)
                Else
                    Err = clsAMIMessage.Parse(String.Format("{0} - {1}", sResType, sResult))
                End If
            Else
                Me.TargetAppNoLongerConnected()
            End If

            Return Success
        End Function

        ''' <summary>
        ''' Determines if the two applications are equal, from the point of view of
        ''' the application being launched and operated on.
        ''' 
        ''' <example>If the path of the application executable is not the same, then
        ''' the applications are not equal.</example>
        ''' <example>If the two applications are both terminal applications, but use
        ''' different session identifiers then the applications are not equal.
        ''' </example>
        ''' </summary>
        ''' <param name="App1">The first application.</param>
        ''' <param name="App2">The second application.</param>
        ''' <returns>Returns true if the important features of the applications are
        ''' the same.</returns>
        Public Function ApplicationsAreEqual(ByVal App1 As clsApplicationTypeInfo, ByVal App2 As clsApplicationTypeInfo) As Boolean
            If App1.ID <> App2.ID Then Return False
            If App1.Parameters.Count <> App2.Parameters.Count Then Return False

            For Each p1 As clsApplicationParameter In App1.Parameters
                For Each p2 As clsApplicationParameter In App2.Parameters
                    If (p1.Name = p2.Name) AndAlso (p1.ParameterType = p2.ParameterType) Then
                        If Not p1.Value = p2.Value Then Return False
                    End If
                Next
            Next

            Return True
        End Function


        ''' <summary>
        ''' Determines if the application has been launched already.
        ''' </summary>
        ''' <returns>True if the application is running, false
        ''' otherwise.</returns>
        Public Function ApplicationIsLaunched() As Boolean
            Return Not (mTargetApp Is Nothing)
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
            p.FriendlyName = My.Resources.Options
            p.HelpText = My.Resources.IfNecessaryEnterAnyOptionsSpecificToThisTargetApplicationAsDirectedByBluePrismT
            p.ParameterType = ParamType.String
            p.AcceptNullValue = True
            p.HelpReference = "helpApplicationParameters.htm#Options"
            Return p

        End Function

        ''' <summary>
        ''' Get a list of the types of application supported by AMI.
        ''' </summary>
        ''' <returns>A collection of clsApplicationTypeInfo objects, each
        ''' detailing a particular application type.</returns>
        Public Function GetApplicationTypes() As List(Of clsApplicationTypeInfo)
            Return clsApplicationTypeInfo.GetApplicationTypes(mGlobalInfo)
        End Function


        ''' <summary>
        ''' Get a list of the types of Element supported by AMI.
        ''' </summary>
        ''' <returns>A collection of clsElementTypeInfo objects, each detailing a
        ''' particlar element type.</returns>
        Public Shared Function GetElementTypes() As Dictionary(Of String, clsElementTypeInfo)
            Return mElementTypes
        End Function

        ''' <summary>
        ''' Get full information about an element type, given its ID.
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns>An ElementTypeInfo instance, or Nothing if the ID was not recognised
        ''' </returns>
        Public Shared Function GetElementTypeInfo(ByVal ID As String) As clsElementTypeInfo
            If mElementTypes.ContainsKey(ID) Then
                Return mElementTypes.Item(ID)
            Else
                Return Nothing
            End If
        End Function


        ''' <summary>
        ''' Get information about an identifier, and set the value.
        ''' </summary>
        ''' <param name="ID">The ID of the identifier</param>
        ''' <param name="value">The value to set.</param>
        ''' <param name="bEnableByDefault">True if the identifier should be enabled
        ''' by default.</param>
        ''' <param name="customId">An optional custom value to assign to the
        ''' identifier's ID property</param>
        ''' <returns>A clsIdentifierInfo instance containing the details of the
        ''' identifier, and populated with the given value.</returns>
        ''' <remarks>Throws an exception if given a duff ID - this should never
        ''' happen.</remarks>
        Public Shared Function GetIdentifierInfo(ByVal id As String,
         ByVal value As String, Optional ByVal bEnableByDefault As Boolean = False,
         Optional customId As String = Nothing) _
         As clsIdentifierInfo
            If Not mIdentifiers.ContainsKey(id) Then _
             Throw New NoSuchElementException("Identifier {0} does not exist", id)

            ' Create a clone of the referenced identifier, overwrite the value and
            ' enabled value with the given arguments
            Dim newId As clsIdentifierInfo = mIdentifiers(id).Clone(customId)
            newId.Value = value
            newId.EnableByDefault = bEnableByDefault
            Return newId

        End Function

        ''' <summary>
        ''' Get information about an identifier.
        ''' </summary>
        ''' <param name="ID">The ID of the identifier</param>
        ''' <returns>A clsIdentifierInfo instance containing the details of the
        ''' identifier.</returns>
        Public Shared Function GetIdentifierInfo(ByVal id As String) As clsIdentifierInfo
            If mIdentifiers.ContainsKey(id) Then Return mIdentifiers(id)
            Return Nothing
        End Function

        ''' <summary>
        ''' Get full information about an action type, given its ID.
        ''' </summary>
        ''' <param name="ID">The ID of the action whose info is sought.</param>
        ''' <param name="appInfo">Optional - may be a null reference if not of interest.
        ''' The application type of interest. When used, more specific info is returned
        ''' for that application type. Eg the launch action gains parameters specific
        ''' to that application type.</param>
        ''' <returns>An ActionTypeInfo instance, or Nothing if the ID was not recognised.
        ''' </returns>
        Public Shared Function GetActionTypeInfo(ByVal ID As String, Optional ByVal appInfo As clsApplicationTypeInfo = Nothing) As clsActionTypeInfo
            If mActionTypes.ContainsKey(ID) Then
                If ID = "Launch" Then
                    Return GetLaunchAction(appInfo, ID)
                ElseIf ID = "AttachApplication" Then
                    Dim action = mActionTypes.Item(ID)
                    If appInfo Is Nothing Then Return action
                    For Each p As clsArgumentInfo In appInfo.Outputs
                        action.AddOutput(p)
                    Next
                    Return action
                Else
                    Return mActionTypes.Item(ID)
                End If
            Else
                Return Nothing
            End If
        End Function


        ''' <summary>
        ''' Get full information about an condition type, given its ID.
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <returns>An ElementTypeInfo instance, or Nothing if the ID was not recognised
        ''' </returns>
        Public Shared Function GetConditionTypeInfo(ByVal ID As String) As clsConditionTypeInfo
            If mConditionTypes.ContainsKey(ID) Then
                Return mConditionTypes.Item(ID)
            Else
                Return Nothing
            End If
        End Function


        ''' <summary>
        ''' Cancels any spy operation currently underway.
        ''' Otherwise does nothing.
        ''' </summary>
        ''' <param name="Err">String to carry error message.</param>
        ''' <returns>Returns true on succes.</returns>
        Public Function CancelSpy(ByRef Err As clsAMIMessage) As Boolean

            If mTargetApp Is Nothing Then
                Err = New clsAMIMessage(clsAMIMessage.CommonMessages.NotConnected)
                Return False
            End If

            'Perform the spy query and parse the response...
            Dim sResult As String, sResType As String = Nothing
            sResult = mTargetApp.ProcessQuery("CancelSpy")
            clsQuery.ParseResponse(sResult, sResType, sResult)

            Select Case sResType
                Case "ERROR"
                    Err = clsAMIMessage.Parse(sResult)
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>
        ''' Detects the element type for the given category and set of IDs
        ''' </summary>
        ''' <param name="cat">The category for the element</param>
        ''' <param name="ids">The collection of identifiers</param>
        ''' <returns>The element type deduced from the identifiers, given the broad
        ''' category that the element belongs to</returns>
        ''' <exception cref="InvalidTypeException">If the element category value was
        ''' unrecognised.</exception>
        Private Function DetectElementType(ByVal cat As ElementCategory,
         ByVal ids As ICollection(Of clsIdentifierInfo)) _
         As clsElementTypeInfo
            Select Case cat
                Case ElementCategory.Window
                    Return DetectWin32ElementType(ids)
                Case ElementCategory.AAElement
                    Return DetectAAElementType(ids)
                Case ElementCategory.Html
                    Return DetectHTMLElementType(ids)
                Case ElementCategory.WEB
                    Return DetectWebElementType(ids)
                Case ElementCategory.JAB
                    Return DetectJavaElementType(ids)
                Case ElementCategory.SAP
                    Return DetectSAPElementType(ids)
                Case ElementCategory.UIA
                    Return DetectUIAElementType(ids)
                Case Else
                    Throw New InvalidTypeException(
                     My.Resources.UnhandledElementType0Detected, cat)
            End Select

        End Function

        ''' <summary>
        ''' Detects an appropriate element type for the AA element described by the
        ''' given identifiers.
        ''' </summary>
        ''' <param name="ids">The collection of identifiers for which an AA element
        ''' type is required.</param>
        ''' <returns>The element type corresponding to the given set of IDs</returns>
        ''' <exception cref="ArgumentException">If the role in the given collection
        ''' of identifiers is not recognised - ie. does not exist in the enum of
        ''' <see cref="AccessibleRole">Accessible Roles</see></exception>
        Private Function DetectAAElementType(
         ByVal ids As ICollection(Of clsIdentifierInfo)) As clsElementTypeInfo

            Dim role As AccessibleRole = AccessibleRole.Default
            For Each id As clsIdentifierInfo In ids
                If id.ID = "Role" Then
                    If Not clsEnum.TryParse(id.Value, True, role) Then Throw New _
                     ArgumentException(String.Format(My.Resources.UnrecognisedRole0, id.Value))
                    Exit For
                End If
            Next
            Select Case role
                Case AccessibleRole.PushButton
                    Return GetElementTypeInfo("AAButton")
                Case AccessibleRole.CheckButton
                    Return GetElementTypeInfo("AACheckBox")
                Case AccessibleRole.RadioButton
                    Return GetElementTypeInfo("AARadioButton")
                Case AccessibleRole.ComboBox
                    Return GetElementTypeInfo("AAComboBox")
                Case AccessibleRole.List
                    Return GetElementTypeInfo("AAListBox")
                Case AccessibleRole.Text
                    Return GetElementTypeInfo("AAEdit")
                Case Else
                    Return GetElementTypeInfo("AAElement")
            End Select

        End Function

        ''' <summary>
        ''' Detects of the specific UIAElement type to use for a given set of
        ''' identifiers.
        ''' </summary>
        ''' <param name="ids">The collection of identifiers identifying a UIA element
        ''' </param>
        ''' <returns>The specific UIA element type derived from the given identifiers
        ''' </returns>
        ''' <remarks>This is by way of a placeholder at the moment until more
        ''' specific UIA element types are implemented.</remarks>
        Private Function DetectUIAElementType(
         ByVal ids As ICollection(Of clsIdentifierInfo)) As clsElementTypeInfo

            Dim uiaControlType =
                ids.
                FirstOrDefault(Function(x) x.ID = clsQuery.IdentifierTypes.uControlType.ToString()).
                Value

            Select Case uiaControlType.ParseEnum(Of ControlType)()
                Case ControlType.Button
                    Return GetElementTypeInfo("UIAButton")
                Case ControlType.CheckBox
                    Return GetElementTypeInfo("UIACheckBox")
                Case ControlType.RadioButton
                    Return GetElementTypeInfo("UIARadio")
                Case ControlType.ComboBox
                    Return GetElementTypeInfo("UIAComboBox")
                Case ControlType.Edit, ControlType.Text, ControlType.Document
                    Return GetElementTypeInfo("UIAEdit")
                Case ControlType.Menu, ControlType.MenuBar
                    Return GetElementTypeInfo("UIAMenu")
                Case ControlType.MenuItem
                    Return GetElementTypeInfo("UIAMenuItem")
                Case ControlType.List
                    Return GetElementTypeInfo("UIAList")
                Case ControlType.ListItem
                    Return GetElementTypeInfo("UIAListItem")
                Case ControlType.Tab
                    Return GetElementTypeInfo("UIATabControl")
                Case ControlType.TabItem
                    Return GetElementTypeInfo("UIATabItem")
                Case ControlType.Table, ControlType.DataGrid
                    Return GetElementTypeInfo("UIATable")
                Case ControlType.Hyperlink
                    Return GetElementTypeInfo("UIAHyperlink")
                Case ControlType.Tree
                    Return GetElementTypeInfo("UIATreeView")
                Case ControlType.TreeItem
                    Return GetElementTypeInfo("UIATreeViewItem")
                Case ControlType.Window
                    Return GetElementTypeInfo("UIAWindow")
                Case ControlType.SplitButton
                    Return GetElementTypeInfo("UIAButton")
                Case Else
                    Return GetElementTypeInfo("UIAElement")
            End Select

        End Function

        Private Function DetectWebElementType(
         ByVal ids As ICollection(Of clsIdentifierInfo)) As clsElementTypeInfo

            Dim tag = If(
                    ids.
                    FirstOrDefault(Function(x) x.ID = clsQuery.IdentifierTypes.wElementType.ToString())?.
                    Value,
                    String.Empty)

            Select Case tag.ToLowerInvariant()

                Case "button"
                    Return GetElementTypeInfo(ElementType.WebElement.ToString())

                Case "form"
                    Return GetElementTypeInfo(ElementType.WebForm.ToString())

                Case "a", "area"
                    Return GetElementTypeInfo(ElementType.WebLink.ToString())

                Case "dl", "ol", "ul", "datalist", "select"
                    Return GetElementTypeInfo(ElementType.WebList.ToString())

                Case "option"
                    Return GetElementTypeInfo(ElementType.WebListItem.ToString())

                Case "nav", "menu"
                    Return GetElementTypeInfo(ElementType.WebMenu.ToString())

                Case "menuitem"
                    Return GetElementTypeInfo(ElementType.WebMenuItem.ToString())

                Case "progress"
                    Return GetElementTypeInfo(ElementType.WebProgressBar.ToString())

                Case "table"
                    Return GetElementTypeInfo(ElementType.WebTable.ToString())

                Case "td", "th"
                    Return GetElementTypeInfo(ElementType.WebTableItem.ToString())

                Case "address", "article", "aside", "footer", "header", "h1", "h2", "h3", "h4",
                     "h5", "h6", "hgroup", "section", "blockquote", "figcaption", "p", "pre",
                     "abbr", "bdi", "bdo", "code", "dfn", "em", "i", "kbd", "mark", "q", "s",
                     "samp", "small", "strong", "sub", "sup", "tt", "u", "var", "del", "ins",
                     "caption", "label", "legend"
                    Return GetElementTypeInfo(ElementType.WebText.ToString())

                Case "textedit"
                    Return GetElementTypeInfo(ElementType.WebTextEdit.ToString())

                Case "input"
                    Dim inputType = If(
                        ids.
                        FirstOrDefault(Function(x) x.ID = clsQuery.IdentifierTypes.wInputType.ToString())?.
                        Value,
                        String.Empty)

                    Select Case inputType.ToLowerInvariant()
                        Case "button", "submit", "reset"
                            Return GetElementTypeInfo(ElementType.WebButton.ToString())

                        Case "checkbox"
                            Return GetElementTypeInfo(ElementType.WebCheckBox.ToString())

                        Case "radio"
                            Return GetElementTypeInfo(ElementType.WebRadio.ToString())

                        Case "range"
                            Return GetElementTypeInfo(ElementType.WebSlider.ToString())

                        Case "date", "datetime-local", "email", "month", "number", "password", "search",
                             "tel", "text", "url", "week"
                            Return GetElementTypeInfo(ElementType.WebTextEdit.ToString())

                        Case Else
                            Return GetElementTypeInfo(ElementType.WebElement.ToString())
                    End Select

                Case Else
                    Return GetElementTypeInfo(ElementType.WebElement.ToString())
            End Select

        End Function

        ''' <summary>
        ''' Detects an appropriate element type for the HTML element described by the
        ''' given identifiers.
        ''' </summary>
        ''' <param name="ids">The collection of identifiers for which an HTML element
        ''' type is required.</param>
        ''' <returns>The element type corresponding to the given set of IDs</returns>
        Private Function DetectHTMLElementType(
         ByVal ids As ICollection(Of clsIdentifierInfo)) As clsElementTypeInfo
            Dim tagname As String = Nothing, inputtype As String = Nothing
            For Each id As clsIdentifierInfo In ids
                Select Case id.ID
                    Case "TagName" : tagname = id.Value
                    Case "InputType" : inputtype = id.Value
                End Select
            Next

            If tagname = "table" Then Return GetElementTypeInfo("HTMLTable")

            Select Case inputtype.ToLower()
                Case "submit" : Return GetElementTypeInfo("HTMLButton")
                Case "checkbox" : Return GetElementTypeInfo("HTMLCheckBox")
                Case "radio" : Return GetElementTypeInfo("HTMLRadioButton")
                Case "text" : Return GetElementTypeInfo("HTMLEdit")
                Case "select" : Return GetElementTypeInfo("HTMLCombo")
                Case Else : Return GetElementTypeInfo("HTML")
            End Select

        End Function

        ''' <summary>
        ''' Detects an appropriate element type for the java element described by the
        ''' given identifiers.
        ''' </summary>
        ''' <param name="ids">The collection of identifiers for which a java element
        ''' type is required.</param>
        ''' <returns>The element type corresponding to the given set of IDs</returns>
        Public Function DetectJavaElementType(
         ByVal ids As ICollection(Of clsIdentifierInfo)) As clsElementTypeInfo
            Dim role As String = Nothing, allowedactions As String = Nothing
            For Each id As clsIdentifierInfo In ids
                Select Case id.ID
                    Case "Role" : role = id.Value
                    Case "AllowedActions" : allowedactions = id.Value
                End Select
            Next
            'Determine the element type based on the role
            Select Case role
                Case "push button"
                    Return GetElementTypeInfo("JavaButton")
                Case "check box"
                    Return GetElementTypeInfo("JavaCheckBox")
                Case "radio button"
                    Return GetElementTypeInfo("JavaRadioButton")
                Case "text"
                    Return GetElementTypeInfo("JavaEdit")
                Case "scroll bar"
                    Return GetElementTypeInfo("JavaScrollBar")
                Case "combo box"
                    Return GetElementTypeInfo("JavaComboBox")
                Case "menu", "menu bar"
                    Return GetElementTypeInfo("JavaMenu")
                Case "menu item"
                    Return GetElementTypeInfo("JavaMenuItem")
                Case "dialog"
                    Return GetElementTypeInfo("JavaDialog")
                Case "page tab"
                    Return GetElementTypeInfo("JavaTabSelector")
                Case "toggle button"
                    Return GetElementTypeInfo("JavaToggleButton")
                Case "progress bar"
                    Return GetElementTypeInfo("JavaProgressBar")
                Case "password text"
                    Return GetElementTypeInfo("JavaPasswordEdit")
                Case "slider"
                    Return GetElementTypeInfo("JavaTrackBar")
                Case "spinner", "spinbox"
                    Return GetElementTypeInfo("JavaUpDown")
                Case "table"
                    Return GetElementTypeInfo("JavaTable")
                Case "list"
                    Return GetElementTypeInfo("JavaListBox")
                Case "page tab list"
                    Return GetElementTypeInfo("JavaTabControl")
                Case "tool bar"
                    Return GetElementTypeInfo("JavaToolBar")
                Case "tree"
                    Return GetElementTypeInfo("JavaTreeView")
                Case "popup menu"
                    Return GetElementTypeInfo("JavaPopupMenu")
                Case "label"
                    Dim aa As ICollection(Of String) = allowedactions.Split(","c)
                    If aa.Contains("toggle expand") Then
                        Return GetElementTypeInfo("JavaTreeNode")
                    Else
                        Return GetElementTypeInfo("Java")
                    End If
                Case Else
                    Return GetElementTypeInfo("Java")
            End Select

        End Function

        ''' <summary>
        ''' Detect an appropriate element type for the given SAP element.
        ''' </summary>
        ''' <param name="identifiers">The identifiers for the SAP element.</param>
        ''' <returns>The clsElementTypeInfo describing the detected element type.
        ''' </returns>
        Private Function DetectSAPElementType(ByVal identifiers As ICollection(Of clsIdentifierInfo)) As clsElementTypeInfo
            Dim etype As clsElementTypeInfo = Nothing
            Dim compType As String = Nothing
            Dim subType As String = Nothing
            For Each id As clsIdentifierInfo In identifiers
                If id.ID = "ComponentType" Then
                    compType = id.Value
                ElseIf id.ID = "SubType" Then
                    subType = id.Value
                End If
            Next
            'Look at all element types that have a SAPIdentification field
            'set and pick one if it matches.
            Dim matchedType As clsElementTypeInfo = Nothing
            Dim wildcardType As clsElementTypeInfo = Nothing
            For Each el As clsElementTypeInfo In mElementTypes.Values
                If el.SAPIdentification IsNot Nothing Then
                    If el.SAPIdentification = "*" Then
                        wildcardType = el
                    Else
                        Dim cts() As String = el.SAPIdentification.Split(","c)
                        For Each ct As String In cts
                            If ct.IndexOf("/") <> -1 Then
                                If subType IsNot Nothing Then
                                    Dim ctss() As String = ct.Split("/"c)
                                    If ctss(0) = compType AndAlso ctss(1) = subType Then
                                        matchedType = el
                                        Exit For
                                    End If
                                End If
                            Else
                                If ct = compType Then
                                    matchedType = el
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End If
                If matchedType IsNot Nothing Then Exit For
            Next
            If matchedType IsNot Nothing Then
                etype = matchedType
            ElseIf wildcardType IsNot Nothing Then
                etype = wildcardType
            Else
                Throw New InvalidOperationException(My.Resources.TheSAPElementTypeCannotBeIndentifiedSpecifcallyAndNoWildcardTypeHasBeenDefined)
            End If
            Return etype
        End Function


        ''' <summary>
        ''' Detect an appropriate element type for the given window. This is done based
        ''' on the class name.
        ''' </summary>
        ''' <param name="identifiers">The identifiers for the window.</param>
        ''' <returns>The clsElementTypeInfo describing the detected element type.
        ''' </returns>
        Private Function DetectWin32ElementType(
         ByVal identifiers As ICollection(Of clsIdentifierInfo)) As clsElementTypeInfo

            Dim elementType As clsElementTypeInfo = GetElementTypeInfo("Window")    'Default

            Dim classname As String = Nothing,
                typename As String = Nothing,
                style As ES

            For Each id As clsIdentifierInfo In identifiers
                Select Case id.ID
                    Case "ClassName" : classname = id.Value
                    Case "TypeName" : typename = id.Value
                    Case "Style" : style = CType(id.Value, ES)
                End Select
            Next

            If classname IsNot Nothing Then
                Select Case True
                    Case classname.Contains("ThunderRT6CheckBox")
                        'Note that 'normal' checkboxes will get identified
                        'as buttons, but the user will be allowed to convert
                        'them.
                        elementType = GetElementTypeInfo("CheckBox")
                    Case classname.Contains("ComboBox"), classname.Contains("COMBOBOX"), classname.Contains("ThunderRT6ComboBox")
                        elementType = GetElementTypeInfo("ComboBox")
                    Case classname.Contains("ListBox"),
                       classname.StartsWith("WindowsForms10.LISTBOX")
                        elementType = GetElementTypeInfo("ListBox")
                    Case classname.Contains("Button"),
                     classname.Contains("ThunderRT6CommandButton"),
                      classname.Contains("ThunderRT6OptionButton"),
                      classname.Contains("WindowsForms10.BUTTON")
                        elementType = GetElementTypeInfo("Button")
                    Case classname.Contains("Edit"),
                      classname.Contains("ThunderRT6TextBox"),
                       classname.StartsWith("WindowsForms10.EDIT")
                        If style.HasFlag(ES.ES_PASSWORD) Then
                            elementType = GetElementTypeInfo("Password")
                        Else
                            elementType = GetElementTypeInfo("Edit")
                        End If

                    Case classname.Contains("SysListView32"), classname.Contains("PBListView32"), classname.Contains("TListView"), classname.Contains("PBListView32")
                        elementType = GetElementTypeInfo("ListView")
                    Case classname = "ListViewWndClass" OrElse classname = "ListView20WndClass"
                        elementType = GetElementTypeInfo("ListViewAx")
                    Case classname = "TreeView20WndClass"
                        elementType = GetElementTypeInfo("TreeviewAx")
                    Case classname = "StatusBarWndClass" Or classname = "StatusBar20WndClass"
                        elementType = GetElementTypeInfo("StatusBarAx")
                    Case classname.Contains("MSFlexGridWndClass")
                        elementType = GetElementTypeInfo("MSFlexGrid")
                    Case classname.Contains("ApexGrid")
                        elementType = GetElementTypeInfo("ApexGrid")
                    Case classname.Contains("DTPicker20WndClass")
                        elementType = GetElementTypeInfo("DTPicker")
                    Case classname.Contains("SysTabControl32"), classname.Contains("SSTabCtlWndClass"), classname.Contains("PBTabControl32")
                        elementType = GetElementTypeInfo("TabControl")
                    Case classname.Contains("SysTreeView32")
                        elementType = GetElementTypeInfo("Treeview")
                    Case classname.Contains("msctls_trackbar32"), classname.Contains("TTrackBar")
                        elementType = GetElementTypeInfo("TrackBar")
                    Case classname.Contains("msctls_updown32"), classname.Contains("TUpDown")
                        elementType = GetElementTypeInfo("UpDown")
                    Case classname.Contains("SysDateTimePick32")
                        elementType = GetElementTypeInfo("DateTimePicker")
                    Case classname.Contains("SysMonthCal32")
                        elementType = GetElementTypeInfo("MonthCalPicker")
                    Case classname.Contains("SCROLLBAR")
                        elementType = GetElementTypeInfo("ScrollBar")
                    Case classname.Contains("STATIC"), classname.Contains("Static")
                        If classname.StartsWith("WindowsForms10") Then
                            Select Case typename
                                Case "System.Windows.Forms.LinkLabel"
                                    elementType = GetElementTypeInfo("NetLinkLabel")
                                Case Else
                                    elementType = GetElementTypeInfo("Label")
                            End Select
                        Else
                            elementType = GetElementTypeInfo("Label")
                        End If
                    Case classname.Contains("msvb_lib_toolbar"), classname.Contains("ToolbarWindow32")
                        elementType = GetElementTypeInfo("Toolbar")
                End Select
            End If
            Return elementType
        End Function


        ''' <summary>
        ''' Perform a bitmap spying operation. If there is no current target application
        ''' then the operation is done using 'any application'.
        ''' </summary>
        ''' <returns>The spied bitmap, or Nothing.</returns>
        Public Function SpyBitmap() As Bitmap

            'Select the current target application, or 'any application'...
            Dim t As clsTargetApp = mTargetApp
            If t Is Nothing Then t = New clsLocalTargetApp()

            'Perform the spybitmap query and parse the response...
            Dim result As String, restype As String = Nothing
            result = t.ProcessQuery("spybitmap")
            clsQuery.ParseResponse(result, restype, result)
            If restype <> "BITMAP" Then Return Nothing

            Return clsPixRect.ParseBitmap(result)

        End Function


        ''' <summary>
        ''' Perform a 'spy' operation and return the results for use by Application
        ''' Modeller. The target application must be online for this operation.
        ''' An exception is thrown if an error occurs.
        ''' </summary>
        ''' <param name="identifiers">A list of identifying features
        ''' of the spied window.</param>
        ''' <param name="elementType">The type of window spied.</param>
        ''' <returns>Returns true if a window is identified, false
        ''' otherwise. When true, information about the spied element is in
        ''' colIdentifiers.</returns>
        Public Function Spy(ByRef elementType As clsElementTypeInfo, ByRef identifiers As List(Of clsIdentifierInfo)) As Boolean

            If mTargetApp Is Nothing Then Return False

            Dim spyMode As String
            Select Case mTargetAppInfo.ID
                Case clsApplicationTypeInfo.JavaLaunchID, clsApplicationTypeInfo.JavaAttachID,
                     clsApplicationTypeInfo.CitrixJavaLaunchID, clsApplicationTypeInfo.CitrixJavaAttachID
                    spyMode = "Java"
                Case clsApplicationTypeInfo.HTMLLaunchID, clsApplicationTypeInfo.HTMLAttachID
                    spyMode = "Html"
                Case clsApplicationTypeInfo.BrowserLaunchId, clsApplicationTypeInfo.BrowserAttachId,
                     clsApplicationTypeInfo.CitrixBrowserLaunchID, clsApplicationTypeInfo.CitrixBrowserAttachID
                    spyMode = "Web"
                Case Else
                    spyMode = "Win32"
            End Select

            'Perform the spy query and parse the response...
            Dim res As String, sResType As String = Nothing
            res = mTargetApp.ProcessQuery("spy initialspymode=" & spyMode)
            clsQuery.ParseResponse(res, sResType, res)

            Select Case sResType
                Case "ERROR"
                    Throw New InvalidOperationException(res)

                Case "CANCEL"
                    Return False

                Case "WINDOW", "WINDOWRECT"
                    identifiers = GetIdentifiersFromResult(res)
                    If sResType = "WINDOWRECT" Then
                        elementType = GetElementTypeInfo("WindowRect")
                    Else
                        elementType = DetectWin32ElementType(identifiers)
                    End If
                    Return True

                Case "SAP"
                    identifiers = GetIdentifiersFromResult(res)
                    elementType = DetectSAPElementType(identifiers)
                    Return True

                Case "AAELEMENT"
                    identifiers = GetIdentifiersFromResult(res)
                    elementType = DetectAAElementType(identifiers)
                    Return True

                Case "UIAELEMENT"
                    identifiers = GetIdentifiersFromResult(res)
                    elementType = DetectUIAElementType(identifiers)
                    Return True

                Case "WEBELEMENT"
                    identifiers = GetIdentifiersFromResult(res)
                    elementType = DetectWebElementType(identifiers)
                    Return True

                Case "HTML"
                    identifiers = GetIdentifiersFromResult(res)
                    elementType = DetectHTMLElementType(identifiers)
                    Return True

                Case "JAB"
                    identifiers = GetIdentifiersFromResult(res)
                    elementType = DetectJavaElementType(identifiers)
                    Return True

                Case "TERMINALFIELD"
                    identifiers = GetIdentifiersFromResult(res)
                    elementType = GetElementTypeInfo("TerminalField")
                    Return True

                Case Else
                    Throw New InvalidOperationException(String.Format(My.Resources.UnrecognisedSpyResultOf0, res))
            End Select

        End Function

        ''' <summary>
        ''' Parses a spy result into a dictionary of identifiers, keyed on the
        ''' identifier ID.
        ''' </summary>
        ''' <param name="result">A map of spied identifiers, keyed on their ID.
        ''' </param>
        ''' <returns>The map of identifiers gleaned from the given result.</returns>
        Private Function GetIdentifierMapFromResult(ByVal result As String) _
         As IDictionary(Of String, clsIdentifierInfo)

            ' Create a case-insensitive dictionary for identifiers
            Dim map As New Dictionary(Of String, clsIdentifierInfo)(
             StringComparer.CurrentCultureIgnoreCase)

            For Each id As clsIdentifierInfo In GetIdentifiersFromResult(result)
                ' If this identifier supports multiple values and we have one in the
                ' map already, add the value to that, otherwise, just set the value
                ' into the map
                Dim existing As clsIdentifierInfo = Nothing
                If id.SupportsMultiple AndAlso map.TryGetValue(id.ID, existing) Then
                    existing.Values.Add(id.Value)
                Else
                    map(id.ID) = id
                End If
            Next
            Return map
        End Function

        ''' <summary>
        ''' Parse a spy result into a list of identifiers.
        ''' </summary>
        ''' <param name="result">The spy results.</param>
        ''' <returns>A List(Of clsIdentifierInfo) containing the identifiers.</returns>
        Private Function GetIdentifiersFromResult(ByVal result As String) As List(Of clsIdentifierInfo)
            Dim identifiers As New List(Of clsIdentifierInfo)
            While result.Length() > 0
                Dim index As Integer = result.IndexOf("=")
                If index = -1 Then Throw New InvalidOperationException(String.Format(My.Resources.InvalidSpyResponseAt0, result))
                Dim idname As String = result.Substring(0, index)
                result = result.Substring(index + 1)
                Dim idvalue As String = clsQuery.ParseValue(result)
                Dim def As Boolean = False
                If idname.StartsWith("+"c) Then
                    def = True
                    idname = idname.Substring(1)
                End If
                If Not identifiers.Any(Function(x) x.ID = idname) Then
                    identifiers.Add(GetIdentifierInfo(idname, idvalue, def))
                End If
            End While
            Return identifiers
        End Function


        ''' <summary>
        ''' The category defining the broad type of element (ie. what engine was
        ''' used to determine the identifiers for it)
        ''' </summary>
        Public Enum ElementCategory
            Unknown = 0
            Window
            AAElement
            Html
            JAB
            SAP
            SAPSESSION = SAP
            SAPCONNECTION = SAP
            UIA
            WEB
        End Enum

        ''' <summary>
        ''' Represents an element in the tree. Used to return information from
        ''' GetElementTree.
        ''' </summary>
        Public Class clsElement

            ' The AMI instance which generated this element
            Private mOwner As clsAMI

            ' The category of this element
            Private mCategory As ElementCategory

            ' The element type of the element
            Private mElemType As clsElementTypeInfo

            ' The identifiers detailing the element
            Private mIdentifiers As IDictionary(Of String, clsIdentifierInfo)

            ' The children of this element
            Private mChildren As ICollection(Of clsElement)

            ''' <summary>
            ''' The identifiers associated with this element
            ''' </summary>
            Public Property Identifiers() As IDictionary(Of String, clsIdentifierInfo)
                Get
                    If mIdentifiers Is Nothing Then _
                     mIdentifiers = New Dictionary(Of String, clsIdentifierInfo)
                    Return mIdentifiers
                End Get
                Set(ByVal value As IDictionary(Of String, clsIdentifierInfo))
                    mIdentifiers = value
                    mElemType =
                     mOwner.DetectElementType(mCategory, Identifiers.Values)
                End Set
            End Property

            ''' <summary>
            ''' The element type associated with this element.
            ''' </summary>
            Public Property ElementType() As clsElementTypeInfo
                Get
                    Return mElemType
                End Get
                Set(ByVal value As clsElementTypeInfo)
                    mElemType = value
                End Set
            End Property

            ''' <summary>
            ''' Formats the given format string with the value of the identifier with
            ''' the given name.
            ''' </summary>
            ''' <param name="str">The format string to return with the identifier
            ''' value embedded in the placeholder in the string represented by '{0}'.
            ''' If the identifier is not found in this element, the specified
            ''' fallback string is returned with no formatting performed.</param>
            ''' <param name="fallback">The fallback string to return if the
            ''' desired identifier was not found in this element</param>
            ''' <param name="id">The ID of the identifier whose value should be
            ''' embedded into the format string</param>
            ''' <returns>The formatted string with the identifier value embedded.
            ''' </returns>
            Private Function FormatIdValue(
             ByVal str As String, ByVal fallback As String, ByVal id As String) _
             As String
                Dim ident As clsIdentifierInfo = Nothing
                If Not Identifiers.TryGetValue(id, ident) Then Return fallback
                Return String.Format(str, ident.Value)
            End Function

            ''' <summary>
            ''' Gets a type label suitable to display the type of this element. This
            ''' may be a defined type within AMI, or it may be a guide to the type of
            ''' element which has not been fully registered with AMI (eg. it will
            ''' append the HTML tag name, the java role etc)
            ''' </summary>
            Public ReadOnly Property TypeLabel As String
                Get
                    Select Case mElemType.ID
                        Case "Java"
                            Return FormatIdValue(My.Resources.x0Java, My.Resources.Java, "Role")
                        Case "AAElement"
                            Return FormatIdValue(My.Resources.x0AA, My.Resources.AAElement, "Role")
                        Case "HTML"
                            Return FormatIdValue(My.Resources.x0HTML, My.Resources.HTML, "TagName")
                        Case "WebElement"
                            Return FormatIdValue(My.Resources.x0WebElement, My.Resources.WebElement, "wElementType")
                        Case Else : Return mElemType.Name
                    End Select
                End Get
            End Property

            ''' <summary>
            ''' The elements which represent children of this element
            ''' </summary>
            Public Property Children() As ICollection(Of clsElement)
                Get
                    If mChildren Is Nothing Then mChildren = New List(Of clsElement)
                    Return mChildren
                End Get
                Set(ByVal value As ICollection(Of clsElement))
                    mChildren = value
                End Set
            End Property

            ''' <summary>
            ''' Creates a new element owned by the given AMI instance
            ''' </summary>
            ''' <param name="owner">The owner of this element</param>
            ''' <param name="cat">The string value of the category for this
            ''' element; typically "WIN32", "AAELEMENT", "HTML" or "JAVA"</param>
            ''' <param name="idents">The collection of identifiers which makes up
            ''' this element</param>
            Friend Sub New(ByVal owner As clsAMI, ByVal cat As ElementCategory,
             ByVal idents As IDictionary(Of String, clsIdentifierInfo))
                If owner Is Nothing Then Throw New ArgumentNullException(NameOf(owner))
                mOwner = owner
                mCategory = cat
                Identifiers = idents
            End Sub

            ''' <summary>
            ''' Highlights this element. No effect if any errors occur (ie. no errors
            ''' are reported if the highlighting fails)
            ''' </summary>
            Public Sub Highlight()
                mOwner.HighlightWindow(ElementType, Identifiers.Values,
                 GetEmpty.IDictionary(Of String, String), Nothing)
            End Sub

            ''' <summary>
            ''' Gets the screen bounds for this element.
            ''' </summary>
            <DebuggerBrowsable(DebuggerBrowsableState.Never)>
            Public ReadOnly Property ScreenBounds() As Rectangle
                Get
                    ' First, let's see if we have a screenbounds identifier which we
                    ' can use.
                    Dim ident As clsIdentifierInfo = Nothing
                    If Identifiers.TryGetValue("ScreenBounds", ident) Then
                        Dim r As RECT
                        If RECT.TryParse(ident.Value, r) Then Return r
                    End If

                    Return mOwner.GetScreenBounds(ElementType, Identifiers.Values,
                     GetEmpty.IDictionary(Of String, String), Nothing)
                End Get
            End Property

        End Class

        ''' <summary>
        ''' Get the element tree for the target application, ignoring any elements
        ''' set as 'invisible' (typically only affects AA or JAB elements). Throws an
        ''' exception if anything goes wrong.
        ''' </summary>
        ''' <param name="mode">The mode, determining the method used to interact with
        ''' the target application. This corresponds to a spy mode, but only "Html"
        ''' and "Win32" are relevant modes here.</param>
        ''' <returns>A List of clsElement instances representing the top level
        ''' elements, with each potentially having a tree of child elements below it.
        ''' </returns>
        ''' <exception cref="InvalidFormatException">If the returned snapshot data
        ''' was in an unrecognised format</exception>
        ''' <exception cref="InvalidModeException">If the specified
        ''' <paramref name="mode"/> was not recognised</exception>
        ''' <exception cref="NotConnectedException">If this instance of AMI is not
        ''' connected to any application with which to retrieve the tree</exception>
        Public Function GetElementTree(
         ByVal mode As String, ByVal monitor As clsProgressMonitor) _
         As ICollection(Of clsElement)
            Return GetElementTree(mode, False, monitor)
        End Function

        ''' <summary>
        ''' Get the element tree for the target application, ignoring any elements
        ''' set as 'invisible' (typically only affects AA or JAB elements). Throws an
        ''' exception if anything goes wrong.
        ''' </summary>
        ''' <param name="mode">The mode, determining the method used to interact with
        ''' the target application. This should be an Application Type ID - see
        ''' clsApplicationTypeInfo for more information. If a snapshot is specified,
        ''' this parameter is not used and should be passed as Nothing.</param>
        ''' <returns>A List of clsElement instances representing the top level
        ''' elements, with each potentially having a tree of child elements below it.
        ''' </returns>
        ''' <exception cref="InvalidFormatException">If the returned snapshot data
        ''' was in an unrecognised format</exception>
        ''' <exception cref="InvalidModeException">If the specified
        ''' <paramref name="mode"/> was not recognised</exception>
        ''' <exception cref="NotConnectedException">If this instance of AMI is not
        ''' connected to any application with which to retrieve the tree</exception>
        Public Function GetElementTree(ByVal mode As String) _
         As ICollection(Of clsElement)
            Return GetElementTree(mode, False, Nothing)
        End Function

        ''' <summary>
        ''' Get the element tree for the target application. Throws an exception if
        ''' anything goes wrong.
        ''' </summary>
        ''' <param name="mode">The mode, determining the method used to interact with
        ''' the target application. This should be an Application Type ID - see
        ''' clsApplicationTypeInfo for more information. If a snapshot is specified,
        ''' this parameter is not used and should be passed as Nothing.</param>
        ''' <param name="includeInvisible">True to include invisible elements and
        ''' their descendants where appropriate. This is ignored for HTML trees (all
        ''' elements in the DOM are returned) and Win32 trees (I don't think
        ''' invisible windows are exposed to us), so is primarily of interest for
        ''' AA and JAB elements.</param>
        ''' <param name="snapshot">If passed, the element tree is constructed from
        ''' this snapshot instead of being queried from the application.</param>
        ''' <returns>A List of clsElement instances representing the top level
        ''' elements, with each potentially having a tree of child elements below it.
        ''' </returns>
        ''' <exception cref="InvalidFormatException">If the returned snapshot data
        ''' was in an unrecognised format</exception>
        ''' <exception cref="InvalidModeException">If the specified
        ''' <paramref name="mode"/> was not recognised</exception>
        ''' <exception cref="NotConnectedException">If this instance of AMI is not
        ''' connected to any application with which to retrieve the tree</exception>
        Public Function GetElementTree(ByVal mode As String,
         ByVal includeInvisible As Boolean, ByVal monitor As clsProgressMonitor,
         Optional ByVal snapshot As String = Nothing) _
         As ICollection(Of clsElement)

            ' Just a placeholder so we don't have to put null checks all over
            ' the place
            If monitor Is Nothing Then monitor = New clsProgressMonitor()

            If snapshot Is Nothing AndAlso mTargetApp Is Nothing Then
                Throw New ApplicationManager.AMI.NotConnectedException()
            End If

            Dim elements As New List(Of clsElement)

            If snapshot Is Nothing Then
                Dim query As String

                Select Case mode
                    Case clsApplicationTypeInfo.Win32LaunchID, clsApplicationTypeInfo.Win32AttachID, clsApplicationTypeInfo.JavaAttachID,
                         clsApplicationTypeInfo.JavaLaunchID, clsApplicationTypeInfo.CitrixJavaLaunchID, clsApplicationTypeInfo.CitrixJavaAttachID
                        query = "windowssnapshot"
                        If includeInvisible Then query &= " includeinvisible=True"
                    Case clsApplicationTypeInfo.HTMLLaunchID, clsApplicationTypeInfo.HTMLAttachID
                        query = "htmlsnapshot"
                    Case clsApplicationTypeInfo.BrowserLaunchId, clsApplicationTypeInfo.BrowserAttachId, clsApplicationTypeInfo.CitrixBrowserAttachID,
                         clsApplicationTypeInfo.CitrixBrowserLaunchID
                        query = "websnapshot"
                    Case Else
                        Throw New InvalidModeException("Invalid mode: {0}", mode)
                End Select

                monitor.FireProgressChange(10, My.Resources.ProcessingQuery)
                If monitor.IsCancelRequested Then _
                 Return GetEmpty.ICollection(Of clsElement)()

                Dim result As String = mTargetApp.ProcessQuery(query)

                monitor.FireProgressChange(75, My.Resources.ParsingResponse)
                If monitor.IsCancelRequested Then _
                 Return GetEmpty.ICollection(Of clsElement)()

                Dim resType As String = Nothing
                clsQuery.ParseResponse(result, resType, snapshot)
                If resType <> "RESULT" Then Throw New BluePrismException(
                 My.Resources.FailedToGetSnapshot01, resType, result)
            End If

            Dim parents As New List(Of clsElement)
            For Each line As String In snapshot.Split(Chr(10))
                line = line.TrimEnd()
                If line.Length = 0 Then Continue For

                Dim thislevel As Integer = 0
                While line.StartsWith("  ")
                    line = line.Substring(2)
                    thislevel += 1
                End While
                While parents.Count > thislevel
                    parents.RemoveAt(parents.Count - 1)
                End While

                Dim i As Integer = line.IndexOf(":")
                If i = -1 Then _
                 Throw New InvalidFormatException(My.Resources.BadSnapshotResultFormat)
                Dim etype As String = line.Substring(0, i)
                Dim data As String = line.Substring(i + 1)
                Dim cat = clsEnum.Parse(etype, True, ElementCategory.Unknown)

                Dim e As New clsElement(Me, cat, GetIdentifierMapFromResult(data))

                If parents.Count = 0 _
                 Then elements.Add(e) _
                 Else parents(parents.Count - 1).Children.Add(e)

                parents.Add(e)

            Next

            monitor.FireProgressChange(99, My.Resources.ElementTreeComplete)
            If monitor.IsCancelRequested Then _
             Return GetEmpty.ICollection(Of clsElement)()

            Return elements

        End Function

        ''' <summary>
        ''' Gets the highlight command which is to be used to highlight the specified
        ''' element.
        ''' </summary>
        ''' <param name="el">The element to be highlighted</param>
        ''' <returns>The highlight command which should be used to highlight the
        ''' given element.</returns>
        Private Function GetHighlightCommand(ByVal el As clsElementTypeInfo) _
         As String
            ' If it's a "special" app type, return the specific function
            Select Case el.AppType
                Case AppTypes.Java : Return "HighlightJABElement"
                Case AppTypes.AA : Return "HighlightElement"
                Case AppTypes.UIAutomation : Return "HighlightUIAElement"
                Case AppTypes.Web : Return "HighlightWebElement"
                Case AppTypes.HTML : Return "HighlightHTMLElement"
                Case AppTypes.SAP : Return "highlightsap"
            End Select

            ' Otherwise, deal with the 'special' element types
            Select Case el.ID
                Case "TerminalField" : Return "HighlightTerminalField"
                Case "WindowRect", "Win32ListRegion",
                 "Win32GridRegion" : Return "HighlightRegion"
            End Select

            ' Otherwise, it's a window.
            Return "HighlightWindow"
        End Function

        ''' <summary>
        ''' Gets the application manager timeout parameter value 
        ''' </summary>
        ''' <param name="args">Dictionary of application parameters</param>
        ''' <param name="timeout">OUT: The value of the timeout parameter</param>
        ''' <returns>True on success</returns>
        ''' <remarks></remarks>
        Private Function TryGetApplicationManagerTimeout(
                args As Dictionary(Of String, String), ByRef timeout As TimeSpan) _
                    As Boolean
            Dim mode As ProcessMode = ProcessMode.Internal
            Dim modeval As String = GetArgumentValue(args, ParamType.List,
                                                     "ProcessMode")
            If modeval.Length > 0 Then clsEnum.TryParse(modeval, mode)

            timeout = TimeSpan.Zero

            If mode <> ProcessMode.Internal Then
                Dim timeoutString = GetArgumentValue(args, ParamType.Timespan,
                                                     "ExternalProcessTimeout")
                Dim timeoutSeconds As Integer
                If Not Integer.TryParse(timeoutString, timeoutSeconds) Then
                    Return False
                End If
                timeout = TimeSpan.FromSeconds(timeoutSeconds)
            End If
            Return True
        End Function

        ''' <summary>
        ''' Gets the command to get the screen bounds of an element of particular
        ''' type
        ''' </summary>
        ''' <param name="el">The element type for which the screen bounds command
        ''' is required</param>
        ''' <returns>The name of the command with which the screen bounds can be
        ''' retrieved.</returns>
        Private Function GetScreenBoundsCommand(ByVal el As clsElementTypeInfo) _
         As String
            Dim prefix As String
            Select Case el.AppType
                Case AppTypes.AA : prefix = "AA"
                Case AppTypes.HTML : prefix = "HTML"
                Case AppTypes.Java : prefix = "JAB"
                Case AppTypes.Web : prefix = "Web"
                Case AppTypes.SAP : prefix = "SAP"
                Case Else
                    Select Case el.ID
                        Case "WindowRect", "Win32ListRegion", "Win32GridRegion"
                            prefix = "Region"
                        Case Else
                            prefix = ""
                    End Select
            End Select
            Return prefix & "GetElementScreenBounds"
        End Function

        ''' <summary>
        ''' Gets a snapshot in the form of a <see cref="clsPixRect"/> from the given
        ''' set of identifiers and arguments. Only Win32 elements are supported for
        ''' this function.
        ''' </summary>
        ''' <param name="el">The type of element</param>
        ''' <param name="idents">The list of identifiers used to recognise the
        ''' element</param>
        ''' <param name="args">The arguments describing the element</param>
        ''' <returns>The PixRect representing the screenshot from the given element
        ''' information.</returns>
        ''' <exception cref="NotSupportedException">If any element type other than
        ''' <see cref="AppTypes.Win32"/> is given.</exception>
        ''' <exception cref="NotConnectedException">If AMI is not currently connected
        ''' to an application</exception>
        ''' <exception cref="AMIException">If any other errors occur while attempting
        ''' to get the snapshot from the specified element.</exception>
        Public Function GetSnapshot(ByVal el As clsElementTypeInfo,
         ByVal idents As ICollection(Of clsIdentifierInfo),
         ByVal args As IDictionary(Of String, String)) As clsPixRect
            If el.AppType <> AppTypes.Win32 Then Throw New NotSupportedException(
             String.Format(My.Resources.OnlyWin32ElementsSupportSnapshotsNot0, el.AppType))

            If mTargetApp Is Nothing Then Throw New ApplicationManager.AMI.NotConnectedException()

            Dim query As String = "ReadBitmap"
            AppendArgumentsToQuery(query, args)
            AppendIdentifiersToQuery(query, idents)

            Dim resp As String = mTargetApp.ProcessQuery(query)

            Dim resultType As String = Nothing
            Dim result As String = ""
            clsQuery.ParseResponse(resp, resultType, result)
            Select Case resultType
                Case "ERROR", "WARNING" : Throw New AMIException(resp)
                Case "OK", "RESULT" : Return New clsPixRect(result)
                Case Else
                    Throw New AMIException(String.Format(My.Resources.UnknownResponseType0, resultType))

            End Select

        End Function

        ''' <summary>
        ''' Gets the screen bounds of the element described by the given element
        ''' type and arguments.
        ''' </summary>
        ''' <param name="el">The type of element</param>
        ''' <param name="idents">The list of identifiers used to recognise the
        ''' element</param>
        ''' <param name="args">The arguments describing the element</param>
        ''' <param name="err">Any messages from AMI</param>
        ''' <returns>A rectangle describing the bounds of the specified element, or
        ''' <see cref="Rectangle.Empty"/> if any errors occur</returns>
        Public Function GetScreenBounds(ByVal el As clsElementTypeInfo,
         ByVal idents As ICollection(Of clsIdentifierInfo),
         ByVal args As IDictionary(Of String, String), ByRef err As clsAMIMessage) _
         As Rectangle
            Try

                'Fail if not connected
                If mTargetApp Is Nothing Then
                    err = New clsAMIMessage(clsAMIMessage.CommonMessages.NotConnected)
                    Return Rectangle.Empty
                End If

                'Perform the highlight query.

                Dim sQuery As String = GetScreenBoundsCommand(el)
                AppendArgumentsToQuery(sQuery, args)
                AppendIdentifiersToQuery(sQuery, idents)

                'Do the highlighting
                Dim fullResp As String = mTargetApp.ProcessQuery(sQuery)

                Dim resultType As String = Nothing
                Dim result As String = ""
                clsQuery.ParseResponse(fullResp, resultType, result)
                Select Case resultType
                    Case "ERROR", "WARNING"
                        err = clsAMIMessage.Parse(fullResp)
                        Return Rectangle.Empty
                    Case "OK", "RESULT"
                        Return clsTargetApp.CreateRectangleFromCollectionXML(result)
                    Case Else
                        err = clsAMIMessage.Parse(
                         String.Format(My.Resources.UnknownResponseType0, resultType))
                        Return Rectangle.Empty
                End Select

            Catch
                Return Rectangle.Empty

            End Try

        End Function

        ''' <summary>
        ''' Highlights the specified window in the target application, if one exists.
        ''' </summary>
        ''' <param name="el">The type of element to highlight, where
        ''' relevant.</param>
        ''' <param name="idents">The identifiers for the window to highlight, where
        ''' relevant.</param>
        ''' <param name="args">The arguments for the window to highlight.</param>
        ''' <param name="err">On failure, contains details of the error.</param>
        ''' <returns>Returns True if successful, False otherwise.</returns>
        Public Function HighlightWindow(ByVal el As clsElementTypeInfo,
         ByVal idents As ICollection(Of clsIdentifierInfo),
         ByVal args As IDictionary(Of String, String), ByRef err As clsAMIMessage) _
         As Boolean
            Try
                'Fail if not connected
                If mTargetApp Is Nothing Then
                    err = New clsAMIMessage(clsAMIMessage.CommonMessages.NotConnected)
                    Return False
                End If

                'Perform the highlight query.

                Dim sQuery As String = GetHighlightCommand(el)
                AppendArgumentsToQuery(sQuery, args)
                AppendIdentifiersToQuery(sQuery, idents)

                'Do the highlighting
                Dim sResponse As String = Nothing, sResType As String = Nothing
                sResponse = mTargetApp.ProcessQuery(sQuery)
                Dim sErr As String = ""
                clsQuery.ParseResponse(sResponse, sResType, sErr)
                Select Case sResType
                    Case "ERROR", "WARNING"
                        err = clsAMIMessage.Parse(sResponse)
                        Return False
                    Case "OK", "RESULT"
                        Return True
                    Case Else
                        err = clsAMIMessage.Parse(String.Format(My.Resources.UnknownResponseType0, sResType))
                        Return False
                End Select

            Catch
                Return False

            End Try

            Return True
        End Function

#Region "Action Validation"

        ''' <summary>
        ''' Determines whether the specified action is appropriate for the specified
        ''' element type. This is used during Process Validation, where it can
        ''' highlight the use of deprecated actions as validation warnings.
        ''' </summary>
        ''' <param name="actionID">The ID of the action of interest.</param>
        ''' <param name="elem">The type of element with which the specified
        ''' action is to be used.</param>
        ''' <param name="app">The type of application owning the supplied
        ''' element. If Nothing, then a more liberal acceptance policy will be used.
        ''' e.g. SetMainframeWindowTitle is valid for a mainframe "Application"
        ''' element, but not for an HTML "Application" element.</param>
        ''' <param name="alternative">If True is returned, this will contain
        ''' Nothing unless the action is deprecated, in which case it will contain
        ''' the ID of a suggested alternative action to use. If False is returned,
        ''' the value is undefined.</param>
        ''' <param name="helpMsg">If a suggestedAlternative is returned, this
        ''' will contain a user-friendly string to explain any issues
        ''' arising. e.g. "PressButton is no longer valid. Please use TapButton
        ''' instead"</param>
        ''' <param name="helpTopic">If an <paramref name="alternative"/> is returned,
        ''' this will contain a reference to a related help topic, or 0 if one is not
        ''' available. Envisaged for situations where complicated queries are
        ''' deprecated and complicated replacements or migrations are necessary.
        ''' </param>
        ''' <returns>Returns True if the specified action will be accepted; False
        ''' otherwise.</returns>
        ''' <remarks>The correct way to use this function is to first check the
        ''' return value, then if relevant check the <paramref name="alternative"/>.
        ''' </remarks>
        Public Shared Function IsValidAction(ByVal actionID As String,
         ByVal elem As clsElementTypeInfo, ByVal app As clsApplicationTypeInfo,
         ByRef alternative As String,
         ByRef helpMsg As String, ByRef helpTopic As Integer) As Boolean

            Dim verifyAction As clsActionTypeInfo = Nothing

            'Any action which is advertised in the official list is always valid
            For Each action As clsActionTypeInfo In
             clsAMI.GetAllowedActions(elem, app)
                If action.ID = actionID Then
                    alternative = Nothing
                    Return True

                ElseIf action.Name = "Verify" Then
                    ' Save the verify action for this element type
                    verifyAction = action

                ElseIf app IsNot Nothing AndAlso IsBrowserApplication(app.ID) Then
                    If actionID = "DetachApplication" Then
                        alternative = "WebDetachApplication"
                        Return True
                    End If
                    If actionID = "Terminate" Then
                        alternative = "WebCloseApplication"
                        Return True
                    End If

                End If
            Next

            ' If they are requesting a 'Verify' action and we have an alternative,
            ' pass that back
            If actionID = "Verify" AndAlso verifyAction IsNot Nothing Then _
             alternative = verifyAction.ID : Return True

            ' Otherwise, it's not valid for this element type
            Return False
        End Function

        ''' <summary>
        ''' Determines whether the specified read action is appropriate for the
        ''' specified element type. This is used during Process Validation, where
        ''' it can highlight the use of deprecated actions as validation warnings.
        ''' It's also called at load time, and the 'suggested' value is automatically
        ''' used.
        ''' </summary>
        ''' <param name="actionID">The ID of the action of interest.</param>
        ''' <param name="elementType">The type of element with which the 
        ''' specified action is to be used.</param>
        ''' <param name="appInfo">The type of application owning the supplied
        ''' element. If Nothing, then a more liberal acceptance policy will be used.
        ''' e.g. SetMainframeWindowTitle is valid for a mainframe "Application"
        ''' element, but not for an HTML "Application" element.</param>
        ''' <param name="suggestedAltActionId">Will return null unless the action is
        ''' deprecated, in which case it will contain the ID of a suggested
        ''' alternative action to use.</param>
        ''' <param name="suggestedAltArgs">Will return null unless the action is
        ''' deprecated, in which case it will contain a dictionary of argument IDs
        ''' and their suggested values, encoded. ie. this will be non-null if and
        ''' only if <paramref name="suggestedAltActionId"/> is non-null. It may not
        ''' contain all arguments for the returned action - only those which should
        ''' be set with a particular value, which is encoded in the value of the
        ''' dictionary.</param>
        ''' <param name="helpMessage">If a suggestedAlternative is returned, this
        ''' will contain a user-friendly string to explain any issues
        ''' arising. e.g. "PressButton is no longer valid. Please use TapButton
        ''' instead"</param>
        ''' <param name="helpTopic">If a suggestedAlternative is returned, this will
        ''' contain a reference to a related help topic, or 0 if one is not
        ''' available. Envisaged for situations where complicated queries are
        ''' deprecated and complicated replacements or migrations are necessary.</param>
        ''' <returns>Returns True if the specified action will be accepted; False
        ''' otherwise.</returns>
        ''' <remarks>The correct way to use this function is to first check the
        ''' return value, then if relevant check the suggestedAlternative.</remarks>
        Public Shared Function IsValidReadAction(ByVal actionID As String,
         ByVal elementType As clsElementTypeInfo,
         ByVal appInfo As clsApplicationTypeInfo,
         ByRef suggestedAltActionId As String,
         ByRef suggestedAltArgs As IDictionary(Of String, String),
         ByRef helpMessage As String, ByRef helpTopic As Integer) As Boolean

            suggestedAltActionId = Nothing
            suggestedAltArgs = Nothing
            helpMessage = Nothing
            helpTopic = 0

            'Any action which is advertised in the official list is always valid
            If GetAllowedReadActionIds(elementType, appInfo).Contains(actionID) Then _
             Return True

            'We used to have a single 'readcurrentvalue' but this was replaced by
            'specific read actions. We still support it by allowing it through here
            'but suggesting a 'modern' alternative...
            If actionID.ToLower() = "readcurrentvalue" Then
                suggestedAltActionId = GetDefaultReadAction(elementType.ID)
                suggestedAltArgs = GetEmpty.IDictionary(Of String, String)()
                helpMessage = My.Resources.TheReadCurrentValueActionIsObsoletePleaseUseTheSuggestedAlternative
                Return True
            End If

            'The JABSnapshot action is now deprecated...
            If actionID = "JABSnapshot" Then
                suggestedAltActionId = "WindowsSnapshot"
                suggestedAltArgs = GetEmpty.IDictionary(Of String, String)()
                helpMessage = My.Resources.TheJABSnapshotActionIsReplacedByTheMoreDetailedWindowsSnapshotActionWhichWillInc
                Return True
            End If

            ' GetWindowText for regions => GetRegionWindowText
            If actionID = "GetWindowText" AndAlso elementType.ID = "WindowRect" Then
                suggestedAltActionId = "GetRegionWindowText"
                suggestedAltArgs = GetEmpty.IDictionary(Of String, String)()
                helpMessage =
                 My.Resources.GetWindowTextForRegionsHasBeenSlightlyAlteredToGiveBetterInformationToTheEndUser
                Return True
            End If

            ' Recognise*Text => ReadChars(Multiline:flag, OrigAlgorithm:flag)
            If Regex.IsMatch(actionID, "Recognise.*Text") Then
                suggestedAltActionId = "ReadChars"
                suggestedAltArgs = New Dictionary(Of String, String)
                helpMessage =
                 My.Resources.TheRecogniseTextActionsHaveBeenCoalescedIntoASingleActionWithParametersDetailing
                Select Case actionID
                    Case "RecogniseSingleLineText"
                        suggestedAltArgs("Multiline") = "False"
                        suggestedAltArgs("OrigAlgorithm") = "False"

                    Case "RecogniseMultiLineText"
                        suggestedAltArgs("Multiline") = "True"
                        suggestedAltArgs("OrigAlgorithm") = "False"

                    Case "RecogniseText"
                        suggestedAltArgs("Multiline") = "False"
                        suggestedAltArgs("OrigAlgorithm") = "True"

                    Case Else ' Shouldn't really be here - "RecogniseWhatText"?
                        suggestedAltActionId = Nothing
                        suggestedAltArgs = Nothing
                        helpMessage = Nothing

                End Select
                ' We found a suitable alternative, return it
                If suggestedAltActionId IsNot Nothing Then Return True
            End If

            If actionID.ToLowerInvariant = "isconnected" AndAlso appInfo IsNot Nothing AndAlso appInfo.ID = clsApplicationTypeInfo.BrowserLaunchId Then
                suggestedAltActionId = "WebIsConnected"
                suggestedAltArgs = New Dictionary(Of String, String)
                suggestedAltArgs("TrackingId") = "True"
                Return True
            End If

            Return False
        End Function

        Public Shared Function IsValidWriteAction(ByVal actionID As String, ByVal elementType As clsElementTypeInfo) As Boolean
            If GetAllowedWriteAction(elementType).Any(Function(x) x.ID = actionID) Then Return True
        End Function

        ''' <summary>
        ''' Determines whether the specified diagnostic action is appropriate for the
        ''' specified element type. This is used during Process Validation, where
        ''' it can highlight the use of deprecated actions as validation warnings.
        ''' </summary>
        ''' <param name="actionID">The ID of the action of interest.</param>
        ''' <param name="appInfo">The type of application owning the supplied
        ''' element. If Nothing, then a more liberal acceptance policy will be used.
        ''' e.g. SetMainframeWindowTitle is valid for a mainframe "Application"
        ''' element, but not for an HTML "Application" element.</param>
        ''' <param name="suggestedAlternative">If True is returned, this will contain
        ''' Nothing unless the action is deprecated, in which case it will contain
        ''' the ID of a suggested alternative action to use. If False is returned,
        ''' the value is undefined.</param>
        ''' <param name="helpMessage">If a suggestedAlternative is returned, this
        ''' will contain a user-friendly string to explain any issues
        ''' arising. e.g. "PressButton is no longer valid. Please use TapButton
        ''' instead"</param>
        ''' <param name="helpTopic">If a suggestedAlternative is returned, this will
        ''' contain a reference to a related help topic, or 0 if one is not
        ''' available. Envisaged for situations where complicated queries are
        ''' deprecated and complicated replacements or migrations are necessary.</param>
        ''' <returns>Returns True if the specified action will be accepted; False
        ''' otherwise.</returns>
        ''' <remarks>The correct way to use this function is to first check the
        ''' return value, then if relevant check the suggestedAlternative.</remarks>
        Public Shared Function IsValidDiagnosticAction(ByVal actionID As String, ByVal appInfo As clsApplicationTypeInfo, ByRef suggestedAlternative As String, ByRef helpMessage As String, ByRef helpTopic As Integer) As Boolean

            'Any action which is advertised in the official list is always valid
            Dim allowedActions As List(Of clsActionTypeInfo) = clsAMI.GetAllowedDiagnosticActions(appInfo)
            For Each action As clsActionTypeInfo In allowedActions
                If action.ID = actionID Then
                    suggestedAlternative = Nothing
                    Return True
                End If
            Next

            'The JABSnapshot action is now deprecated...
            If actionID = "JABSnapshot" Then
                suggestedAlternative = "WindowsSnapshot"
                helpMessage = My.Resources.TheJABSnapshotActionIsReplacedByTheMoreDetailedWindowsSnapshotActionWhichWillInc
                helpTopic = 0
                Return True
            End If

            Return False
        End Function


        ''' <summary>
        ''' Determines whether the specified condition is appropriate for the
        ''' specified element type. This is used during Process Validation, where
        ''' it can highlight the use of deprecated actions as validation warnings.
        ''' </summary>
        ''' <param name="conditionID">The ID of the condition of interest.</param>
        ''' <param name="elementType">The type of element with which the 
        ''' specified condition is to be used.</param>
        ''' <param name="appInfo">The type of application owning the supplied
        ''' element. If Nothing, then a more liberal acceptance policy will be used.
        ''' e.g. SetMainframeWindowTitle is valid for a mainframe "Application"
        ''' element, but not for an HTML "Application" element.</param>
        ''' <param name="suggestedAlternative">If True is returned, this will contain
        ''' Nothing unless the action is deprecated, in which case it will contain
        ''' the ID of a suggested alternative action to use. If False is returned,
        ''' the value is undefined.</param>
        ''' <param name="helpMessage">If a suggestedAlternative is returned, this
        ''' will contain a user-friendly string to explain any issues
        ''' arising. e.g. "PressButton is no longer valid. Please use TapButton
        ''' instead"</param>
        ''' <param name="helpTopic">If a suggestedAlternative is returned, this will
        ''' contain a reference to a related help topic, or 0 if one is not
        ''' available. Envisaged for situations where complicated queries are
        ''' deprecated and complicated replacements or migrations are necessary.</param>
        ''' <returns>Returns True if the specified action will be accepted; False
        ''' otherwise.</returns>
        ''' <remarks>The correct way to use this function is to first check the
        ''' return value, then if relevant check the suggestedAlternative.</remarks>
        Public Shared Function IsValidCondition(
         conditionID As String,
         elementType As clsElementTypeInfo,
         appInfo As clsApplicationTypeInfo,
         ByRef suggestedAlternative As String,
         ByRef helpMessage As String,
         ByRef helpTopic As Integer) As Boolean

            'Any condition which is advertised in the official list is always valid
            For Each condition In GetAllowedConditions(elementType, appInfo)
                If condition.ID = conditionID Then
                    suggestedAlternative = Nothing
                    Return True
                End If
            Next

            'Check for deprecated conditions...
            Select Case conditionID
                ' These should always have been exposed simply as "CheckExists"
                Case "HTMLCheckExists", "JABCheckExists", "AACheckExists"
                    suggestedAlternative = "CheckExists"
                    helpMessage = My.Resources.AllCheckExistsActionsAreNowCombined
                    helpTopic = 0
                    Return True
                Case "CheckExists"
                    If appInfo IsNot Nothing AndAlso appInfo.ID = clsApplicationTypeInfo.BrowserLaunchId Then
                        suggestedAlternative = "WebCheckExists"
                        Return True
                    End If
            End Select

            Return False
        End Function

#End Region

        Public Shared Function GetAllowedWriteAction(elementType As clsElementTypeInfo) As List(Of clsActionTypeInfo)
            Dim list As New List(Of clsActionTypeInfo)
            Select Case elementType.ID
                Case "WebForm", "WebList", "WebMenu", "WebMenuItem", "WebRadio", "WebTextEdit", "WebText", "WebCheckBox",
                     "WebLink","WebButton","WebElement","WebTable","WebSlider","WebProgressBar"
                    list.Add(mActionTypes("WebWrite"))
            End Select
            Return list
        End Function

        ''' <summary>
        ''' Gets a list of allowed actions for a navigate stage, using given the
        ''' element type, and the internal application info (as set using the
        ''' <see cref="SetTargetApplication">SetTargetApplication</see> method).
        ''' </summary>
        ''' <param name="elementType">The element type</param>
        ''' <returns>A collection of action types (clsActionTypeInfo)</returns>
        Public Function GetAllowedActions(ByVal elementType As clsElementTypeInfo) As List(Of clsActionTypeInfo)
            Return GetAllowedActions(elementType, mTargetAppInfo)
        End Function


        ''' <summary>
        ''' Gets a list of allowed actions for a navigate stage, given the element type.
        ''' </summary>
        ''' <param name="elementType">The element type</param>
        ''' <param name="appInfo">The type of application owning the supplied
        ''' element. This may be a Nothing, but in this case less specific
        ''' information may be returned.</param>
        ''' <returns>A List of action types (clsActionTypeInfo)</returns>
        ''' <remarks>See also IsValidAction() method. GetAllowedActions
        ''' advertises an official list, whereas the IsValid method indicates which
        ''' conditions are acceptable (both legacy and current/official).</remarks>
        Public Shared Function GetAllowedActions(ByVal elementType As clsElementTypeInfo, ByVal appInfo As clsApplicationTypeInfo) As List(Of clsActionTypeInfo)

            Dim list As New List(Of clsActionTypeInfo)

            'Deal with element definitions that are data driven first...
            If elementType.ActionQueries IsNot Nothing Then
                For Each action As String In elementType.ActionQueries.Keys
                    If Not mActionTypes.ContainsKey(action) Then
                        Throw New InvalidOperationException(String.Format(My.Resources.MissingActionType0, action))
                    End If
                    list.Add(mActionTypes(action))
                Next
                Return list
            End If

            Select Case elementType.ID
                Case "DDEElement"
                    list.Add(mActionTypes("ExecuteDDECommand"))
                Case "TerminalField"
                    'No navigate actions for a terminal field
                Case "Button", "JavaButton"
                    list.Add(mActionTypes("Press"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                Case "CheckBox", "RadioButton"
                    list.Add(mActionTypes("Press"))
                Case "NetCheckBox"
                Case "JavaCheckBox", "JavaRadioButton", "JavaToggleButton"
                    list.Add(mActionTypes("Press"))
                Case "ComboBox"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("SelectItem"))
                    list.Add(mActionTypes("ShowDropdown"))
                    list.Add(mActionTypes("HideDropdown"))
                    list.Add(mActionTypes("TypeText"))
                    list.Add(mActionTypes("TypeTextAlt"))
                Case "AAComboBox"
                    list.Add(mActionTypes("AAClickCentre"))
                    list.Add(mActionTypes("AAFocus"))
                    list.Add(mActionTypes("AAMouseClick"))
                    list.Add(mActionTypes("AASelectItem"))
                    list.Add(mActionTypes("AAShowDropdown"))
                    list.Add(mActionTypes("AASendKeys"))
                    list.Add(mActionTypes("AAHideDropdown"))
                    list.Add(mActionTypes("TypeText"))
                    list.Add(mActionTypes("TypeTextAlt"))
                Case "Edit", "Password"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                    list.Add(mActionTypes("TypeText"))
                    list.Add(mActionTypes("TypeTextAlt"))
                Case "JavaEdit"
                    list.Add(mActionTypes("JABSelectText"))
                    list.Add(mActionTypes("JABSelectAllText"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "ListBox"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                    list.Add(mActionTypes("SelectItem"))
                    list.Add(mActionTypes("TypeText"))
                Case "ListView"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                    list.Add(mActionTypes("DragItem"))
                    list.Add(mActionTypes("DropOntoItem"))
                    list.Add(mActionTypes("MultiSelectItem"))
                    list.Add(mActionTypes("SelectItem"))
                    list.Add(mActionTypes("ClickItem"))
                    list.Add(mActionTypes("SetItemChecked"))
                    list.Add(mActionTypes("ClearSelection"))
                    list.Add(mActionTypes("TypeText"))
                    list.Add(mActionTypes("EnsureItemVisible"))
                Case "Toolbar"
                    list.Add(mActionTypes("ClickToolbarButton"))
                Case "DataGrid", "DataGridView"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "MSFlexGrid"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("GoToCell"))
                    list.Add(mActionTypes("SelectRange"))
                    list.Add(mActionTypes("SetTopRow"))
                Case "ApexGrid"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("GoToCell"))
                Case "DTPicker"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "TreeviewAx"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("SelectTreeNode"))
                Case "Treeview"
                    list.Add(mActionTypes("SelectItem"))
                    list.Add(mActionTypes("SetItemChecked"))
                    list.Add(mActionTypes("EnsureItemVisible"))
                    list.Add(mActionTypes("ExpandTreeNode"))
                    list.Add(mActionTypes("CollapseTreeNode"))
                    list.Add(mActionTypes("ToggleTreeNode"))
                Case "JavaTreeView"
                    list.Add(mActionTypes("JABSelectItem"))
                    list.Add(mActionTypes("JABEnsureItemVisible"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "JavaTreeNode"
                    list.Add(mActionTypes("JABExpandTreeNode"))
                    list.Add(mActionTypes("JABCollapseTreeNode"))
                    list.Add(mActionTypes("JABToggleTreeNode"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "JavaProgressBar"
                    list.Add(mActionTypes("JABGetNumericValue"))
                    list.Add(mActionTypes("JABGetMaxNumericValue"))
                    list.Add(mActionTypes("JABGetMinNumericValue"))
                Case "JavaMenu"
                    list.Add(mActionTypes("JABHideDropdown"))
                    list.Add(mActionTypes("JABShowDropdown"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "JavaPopupMenu"
                    list.Add(mActionTypes("JABSelectItem"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "JavaMenuItem"
                    list.Add(mActionTypes("Press"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "JavaTable"
                    list.Add(mActionTypes("JABSelectAllItems"))
                    list.Add(mActionTypes("JABClearSelection"))
                    list.Add(mActionTypes("JABSelectItem"))
                Case "JavaListBox"
                    list.Add(mActionTypes("JABSelectItem"))
                    list.Add(mActionTypes("JABSelectAllItems"))
                    list.Add(mActionTypes("JABClearSelection"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "JavaTabControl", "JavaToolBar"
                    list.Add(mActionTypes("JABSelectItem"))
                Case "JavaComboBox"
                    list.Add(mActionTypes("JABShowDropdown"))
                    list.Add(mActionTypes("JABHideDropdown"))
                    list.Add(mActionTypes("JABSelectItem"))
                Case "JavaTabSelector"
                    list.Add(mActionTypes("JABSelectTab"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                Case "MonthCalPicker"
                    list.Add(mActionTypes("GetDateTimeValue"))
                    list.Add(mActionTypes("SetDateTimeValue"))
                    list.Add(mActionTypes("GetMaxDateTimeValue"))
                    list.Add(mActionTypes("GetMinDateTimeValue"))
                    list.Add(mActionTypes("GetMaxSelectedDateTimeValue"))
                    list.Add(mActionTypes("GetMinSelectedDateTimeValue"))
                Case "ScrollBar"
                    list.Add(mActionTypes("ScrollToMinimum"))
                    list.Add(mActionTypes("ScrollToMaximum"))
                    list.Add(mActionTypes("ScrollByAmount"))
                Case "JavaScrollBar"
                    'Would like to advertise such actions, but no API support for
                    'writing values to java controls. See bug 2993
                    '  list.Add(mActionTypes("JABScrollToMinimum"))
                    '  list.Add(mActionTypes("JABScrollToMaximum"))
                    '  list.Add(mActionTypes("JABScrollByAmount"))
                Case "TabControl"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("SelectItem"))
                    list.Add(mActionTypes("ClickTab"))
                    list.Add(mActionTypes("MouseClickTab"))
                Case "NetLinkLabel"
                    list.Add(mActionTypes("ClickLink"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                Case "Label"
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                Case "Window"
                    list.Add(mActionTypes("ClickWindow"))
                    list.Add(mActionTypes("ClickWindowCentre"))
                    list.Add(mActionTypes("MouseClick"))
                    list.Add(mActionTypes("MouseClickCentre"))
                    list.Add(mActionTypes("MouseDoubleClick"))
                    list.Add(mActionTypes("MouseDoubleClickCentre"))
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                    list.Add(mActionTypes("TypeText"))
                    list.Add(mActionTypes("TypeTextAlt"))
                    list.Add(mActionTypes("ActivateApp"))
                    list.Add(mActionTypes("CloseWindow"))
                    list.Add(mActionTypes("MoveWindow"))
                    list.Add(mActionTypes("ResizeWindow"))
                    list.Add(mActionTypes("MaximiseWindow"))
                    list.Add(mActionTypes("MinimiseWindow"))
                    list.Add(mActionTypes("RestoreWindow"))
                    list.Add(mActionTypes("HideWindow"))
                    list.Add(mActionTypes("UnhideWindow"))
                    list.Add(mActionTypes("SelectMenuItem"))
                Case "Application"

                    If clsApplicationTypeInfo.IsMainframe(appInfo) Then

                        If clsApplicationTypeInfo.HasMainframeLaunchSupport(appInfo) Then
                            list.Add(GetLaunchAction(appInfo, "LaunchMainframe"))
                        Else
                            list.Add(GetLaunchAction(appInfo, "Launch"))
                        End If
                        If clsApplicationTypeInfo.HasMainframeAttachSupport(appInfo) Then
                            list.Add(mActionTypes("AttachMainframe"))
                        End If
                        If clsApplicationTypeInfo.HasMainframeDetachSupport(appInfo) Then
                            list.Add(mActionTypes("DetachMainframe"))
                        End If
                        If clsApplicationTypeInfo.HasMainframeTerminateSupport(appInfo) Then
                            list.Add(mActionTypes("TerminateMainframe"))
                        Else
                            list.Add(mActionTypes("Terminate"))
                        End If

                        If clsApplicationTypeInfo.HasMainframeSendKeySupport(appInfo) Then
                            list.Add(mActionTypes("MainframeSendKeys"))
                        Else
                            list.Add(mActionTypes("SendKeys"))
                        End If
                    Else

                        list.Add(GetLaunchAction(appInfo, "Launch"))

                        'Only add these if not a mainframe
                        If appInfo IsNot Nothing AndAlso IsBrowserApplication(appInfo.ID) Then
                            list.Add(GetAttachAction(appInfo, "AttachApplication"))
                        Else
                            list.Add(mActionTypes("AttachApplication"))
                        End If

                        If appInfo IsNot Nothing Then
                            If IsBrowserApplication(appInfo.ID) Then
                                list.Add(mActionTypes("WebDetachApplication"))
                                list.Add(mActionTypes("WebCloseApplication"))
                            Else
                                list.Add(mActionTypes("DetachApplication"))
                                list.Add(mActionTypes("Terminate"))
                            End If
                        End If

                        list.Add(mActionTypes("SendKeys"))

                    End If

                    list.Add(mActionTypes("SendKeyEvents"))

                    If clsApplicationTypeInfo.HasParentWindowSupport(appInfo) Then
                        list.Add(mActionTypes("SetMainframeParentWindowTitle"))
                    End If
                    If clsApplicationTypeInfo.IsMainframe(appInfo) Then
                        list.Add(mActionTypes("SetMainframeCursorPos"))
                    End If
                    If clsApplicationTypeInfo.HasMacroSupport(appInfo) Then
                        list.Add(mActionTypes("RunMainframeMacro"))
                    End If
                    If appInfo IsNot Nothing Then
                        Select Case appInfo.ID
                            Case clsApplicationTypeInfo.HTMLAttachID, clsApplicationTypeInfo.HTMLLaunchID
                                list.Add(mActionTypes("HTMLNavigate"))
                                list.Add(mActionTypes("HTMLInvokeJavascriptMethod"))
                                list.Add(mActionTypes("HTMLInsertJavascriptFragment"))
                                list.Add(mActionTypes("HTMLUpdateCookie"))
                            Case clsApplicationTypeInfo.Win32AttachID, clsApplicationTypeInfo.Win32LaunchID, clsApplicationTypeInfo.JavaAttachID,
                                 clsApplicationTypeInfo.JavaLaunchID, clsApplicationTypeInfo.HTMLAttachID, clsApplicationTypeInfo.HTMLLaunchID,
                                 clsApplicationTypeInfo.CitrixJavaLaunchID, clsApplicationTypeInfo.CitrixJavaAttachID
                                list.Add(mActionTypes("HideAllWindows"))
                        End Select
                    End If
                Case "WindowRect"
                    list.Add(mActionTypes("RegionMouseClickCentre"))
                    list.Add(mActionTypes("RegionMouseClick"))
                    list.Add(mActionTypes("RegionParentClickCentre"))
                    list.Add(mActionTypes("RegionStartDrag"))
                    list.Add(mActionTypes("RegionDropOnto"))
                Case "Win32ListRegion"
                    list.Add(mActionTypes("ListRegionMouseClickCentre"))
                    list.Add(mActionTypes("ListRegionMouseClick"))
                    list.Add(mActionTypes("ListRegionParentClickCentre"))
                    list.Add(mActionTypes("ListRegionStartDrag"))
                    list.Add(mActionTypes("ListRegionDropOnto"))
                Case "Win32GridRegion"
                    list.Add(mActionTypes("GridRegionMouseClickCentre"))
                    list.Add(mActionTypes("GridRegionMouseClick"))
                    list.Add(mActionTypes("GridRegionParentClickCentre"))
                    list.Add(mActionTypes("GridRegionStartDrag"))
                    list.Add(mActionTypes("GridRegionDropOnto"))

                Case "AAElement"
                    list.Add(mActionTypes("AAClickCentre"))
                    list.Add(mActionTypes("AAMouseClick"))
                    list.Add(mActionTypes("Default"))
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                    list.Add(mActionTypes("AAFocus"))
                    list.Add(mActionTypes("AASendKeys"))
                Case "AAEdit"
                    list.Add(mActionTypes("AAClickCentre"))
                    list.Add(mActionTypes("AAFocus"))
                    list.Add(mActionTypes("AAMouseClick"))
                    list.Add(mActionTypes("AASendKeys"))
                Case "AAButton"
                    list.Add(mActionTypes("AAFocus"))
                    list.Add(mActionTypes("Press"))
                    list.Add(mActionTypes("AAClickCentre"))
                    list.Add(mActionTypes("AAMouseClick"))
                    list.Add(mActionTypes("AASendKeys"))
                Case "AAListBox"
                    list.Add(mActionTypes("AAFocus"))
                    list.Add(mActionTypes("AASelectItem"))
                    list.Add(mActionTypes("AAClickCentre"))
                    list.Add(mActionTypes("AAMouseClick"))
                    list.Add(mActionTypes("AASendKeys"))
                Case "HTML"
                    list.Add(mActionTypes("Drag"))
                    list.Add(mActionTypes("Drop"))
                    list.Add(mActionTypes("SelectItem"))
                    list.Add(mActionTypes("HTMLFocus"))
                    list.Add(mActionTypes("HTMLHover"))
                    If (appInfo IsNot Nothing) AndAlso (appInfo.ID = clsApplicationTypeInfo.HTMLLaunchID OrElse appInfo.ID = clsApplicationTypeInfo.HTMLAttachID) Then
                        list.Add(mActionTypes("HTMLNavigate"))
                        list.Add(mActionTypes("HTMLInvokeJavascriptMethod"))
                        list.Add(mActionTypes("HTMLInsertJavascriptFragment"))
                    End If
                Case "HTMLCheckBox", "HTMLRadioButton"
                    list.Add(mActionTypes("SetChecked"))
                Case "HTMLCombo"
                    list.Add(mActionTypes("HTMLSelectItem"))

                Case "WebForm"
                    list.Add(mActionTypes("WebSubmit"))
                Case "WebList"
                    list.Add(mActionTypes("WebSelectListItem"))
                    list.Add(mActionTypes("WebAddToListSelection"))
                    list.Add(mActionTypes("WebRemoveFromListSelection"))
                Case "WebMenu"
                    list.Add(mActionTypes("WebClickMenuItem"))
                Case "WebMenuItem"
                    list.Add(mActionTypes("WebToggleExpandCollapse"))
                    list.Add(mActionTypes("WebExpand"))
                    list.Add(mActionTypes("WebCollapse"))
                Case "WebRadio"
                    list.Add(mActionTypes("WebCheckRadio"))
                Case "WebTextEdit"
                    list.Add(mActionTypes("WebSelectTextRange"))
                Case "WebText"
                    list.Add(mActionTypes("WebSelectTextRange"))
                Case "UIAElement"
                    list.Add(mActionTypes("UIADrag"))
                    list.Add(mActionTypes("UIADrop"))
                Case "UIAButton"
                    list.Add(mActionTypes("UIAButtonPress"))
                Case "UIAComboBox"
                    list.Add(mActionTypes("UIAComboSelect"))
                    list.Add(mActionTypes("UIAComboExpandCollapse"))
                Case "UIAMenuItem"
                    list.Add(mActionTypes("UIAMenuItemPress"))
                Case "UIAList"
                    list.Add(mActionTypes("UIAListSelect"))
                    list.Add(mActionTypes("UIAListAddToSelection"))
                    list.Add(mActionTypes("UIAListRemoveFromSelection"))
                Case "UIAListItem"
                    list.Add(mActionTypes("UIAAddToSelection"))
                    list.Add(mActionTypes("UIARemoveFromSelection"))
                    list.Add(mActionTypes("UIAPress"))
                    list.Add(mActionTypes("UIAExpandCollapse"))
                    list.Add(mActionTypes("UIAScrollIntoView"))
                Case "UIATabControl"
                    list.Add(mActionTypes("UIASelectTab"))
                Case "UIATabItem"
                    list.Add(mActionTypes("UIASelectTabItem"))
                Case "UIATable"
                    list.Add(mActionTypes("UIAScrollHorizontal"))
                    list.Add(mActionTypes("UIAScrollVertical"))
                    list.Add(mActionTypes("UIATableSetCellText"))
                    list.Add(mActionTypes("UIATableScrollIntoView"))
                    list.Add(mActionTypes("UIATableToggleCell"))
                    list.Add(mActionTypes("UIATableExpandCollapse"))
                    list.Add(mActionTypes("UIATableSelectComboboxItem"))
                    list.Add(mActionTypes("UIATableAddRowToSelection"))
                    list.Add(mActionTypes("UIATableRemoveRowFromSelection"))
                    list.Add(mActionTypes("UIATableClearSelection"))
                Case "UIAHyperlink"
                    list.Add(mActionTypes("UIAPress"))

                Case "UIATreeView"
                    list.Add(mActionTypes("UIATreeSelect"))
                    list.Add(mActionTypes("UIATreeAddToSelection"))
                    list.Add(mActionTypes("UIATreeRemoveFromSelection"))
                    list.Add(mActionTypes("UIATreeExpandCollapse"))
                Case "UIATreeViewItem"
                    list.Add(mActionTypes("UIASelect"))
                    list.Add(mActionTypes("UIAExpandCollapse"))
                    list.Add(mActionTypes("UIADrag"))
                    list.Add(mActionTypes("UIADrop"))
                    list.Add(mActionTypes("UIASetToggleState"))
                Case "UIAWindow"
                    list.Add(mActionTypes("UIADrag"))
                    list.Add(mActionTypes("UIADrop"))
            End Select

            If elementType.AppType = AppTypes.UIAutomation Then
                list.Add(mActionTypes("UIAClickCentre"))
                list.Add(mActionTypes("UIAMouseClick"))
                list.Add(mActionTypes("UIAFocus"))
                list.Add(mActionTypes("UIASendKeys"))
            End If

            If elementType.AppType = AppTypes.Web Then
                list.Add(mActionTypes("WebClick"))
                If elementType.ID <> "WebRadio" AndAlso elementType.ID <> "WebCheckBox" Then
                    list.Add(mActionTypes("WebDoubleClick"))
                End If
                list.Add(mActionTypes("WebSelect"))
                list.Add(mActionTypes("WebFocus"))
                list.Add(mActionTypes("WebHover"))
                list.Add(mActionTypes("WebHoverMouseOnElement"))
                list.Add(mActionTypes("WebSetAttribute"))
                list.Add(mActionTypes("WebSendKeys"))
                list.Add(mActionTypes("WebScrollTo"))
                list.Add(mActionTypes("WebNavigate"))
                list.Add(mActionTypes("WebInvokeJavascript"))
                list.Add(mActionTypes("WebInjectJavascript"))
                list.Add(mActionTypes("WebUpdateCookie"))
            End If

            'Add generic actions to all HTML elements
            If elementType.AppType = clsElementTypeInfo.AppTypes.HTML Then
                list.Add(mActionTypes("HTMLClickCentre"))
                list.Add(mActionTypes("HTMLDoubleClickCentre"))
            End If

            'Add a generic 'do' to all Java Access Bridge controls, to allow use of
            'additional functionality we haven't supported directly...
            If elementType.AppType = clsElementTypeInfo.AppTypes.Java Then
                list.Add(mActionTypes("DoJava"))
                list.Add(mActionTypes("JABFocus"))

                ' Make mouse-clicking available on all java elements...
                Dim clicker As clsActionTypeInfo = mActionTypes("MouseClick")
                If Not list.Contains(clicker) Then
                    list.Add(clicker)
                    list.Add(mActionTypes("MouseClickCentre"))
                End If

            End If

            Select Case elementType.AppType
                Case AppTypes.AA, AppTypes.HTML, AppTypes.Java, AppTypes.Mainframe
                    list.Add(mActionTypes("Verify"))

                Case AppTypes.Application, AppTypes.DDE, AppTypes.SAP
                    ' These app types have no 'Verify' function

                Case Else
                    Select Case elementType.ID
                        Case "WindowRect"
                            list.Add(mActionTypes("RegionVerify"))
                        Case "Win32ListRegion"
                            list.Add(mActionTypes("ListRegionVerify"))
                        Case "Win32GridRegion"
                            list.Add(mActionTypes("GridRegionVerify"))
                        Case Else
                            list.Add(mActionTypes("Verify"))
                    End Select

            End Select

            Return list
        End Function

        ''' <summary>
        ''' Gets the allowed diagnostic actions that can be performed for a given
        ''' application type
        ''' </summary>
        ''' <param name="appInfo">The application type in question.</param>
        ''' <returns>A List of ActionTypeInfo instances detailing the allowed
        ''' diagnostic actions.</returns>
        Public Shared Function GetAllowedDiagnosticActions(ByVal appInfo As clsApplicationTypeInfo) As List(Of clsActionTypeInfo)
            Dim list As New List(Of clsActionTypeInfo)

            Select Case appInfo.ID
                Case clsApplicationTypeInfo.HTMLLaunchID, clsApplicationTypeInfo.HTMLAttachID
                    list.Add(mActionTypes("HTMLSnapshot"))
                    list.Add(mActionTypes("HTMLSourceCap"))
                Case clsApplicationTypeInfo.Win32LaunchID, clsApplicationTypeInfo.Win32AttachID, clsApplicationTypeInfo.JavaLaunchID,
                     clsApplicationTypeInfo.JavaAttachID, clsApplicationTypeInfo.CitrixJavaLaunchID, clsApplicationTypeInfo.CitrixJavaAttachID
                    list.Add(mActionTypes("WindowsSnapshot"))
            End Select

            Return list
        End Function

        ''' <summary>
        ''' Gets a launch/attach action appropriate to the supplied application type.
        ''' </summary>
        ''' <param name="AppInfo">The application type for which the launch (or
        ''' attach) action is required. May be null if desired, in which case the
        ''' return value corresponds to the default parameterless launch action.
        ''' </param>
        ''' <returns>Gets a launch/attach action with parameters appropriate to the
        ''' current application type, as set in SetApplicationInfo(). The return
        ''' value is based on a clone of the action stored in the shared default
        ''' instance.</returns>
        Private Shared Function GetLaunchAction(appInfo As clsApplicationTypeInfo, id As String) As clsActionTypeInfo
            Return mActionTypes(id).CreateSpecificLaunchAction(appInfo)
        End Function

        Private Shared Function GetAttachAction(appInfo As clsApplicationTypeInfo, id As String) As clsActionTypeInfo
            Return mActionTypes(id).CreateSpecificAttachAction(appInfo)
        End Function

        ''' <summary>
        ''' Gets a list of conditions to be used with wait stages.
        ''' </summary>
        ''' <param name="elType">The type of element for which the conditions must be
        ''' fetched</param>
        ''' <param name="AppInfo">The type of application owning the supplied element.
        ''' If nothing, then all conditions matching the element from all application
        ''' types will be returned.</param>
        ''' <returns>A list of clsConditionTypeInfo instances, each representing an
        ''' available condition for the given element type.</returns>
        ''' <remarks>See also IsValidCondition() method. GetAllowedConditions
        ''' advertises an official list, whereas the IsValid method indicates which
        ''' conditions are acceptable (both legacy and current/official).</remarks>
        Public Shared Function GetAllowedConditions(
         elType As clsElementTypeInfo,
         appInfo As clsApplicationTypeInfo) As ICollection(Of clsConditionTypeInfo)

            Dim list As New List(Of clsConditionTypeInfo)

            'All Java, AA, UIA, SAP and HTML elements get a CheckExists condition...
            Select Case elType.AppType
                Case AppTypes.Java, AppTypes.AA, AppTypes.UIAutomation, AppTypes.HTML, AppTypes.SAP
                    list.Add(mConditionTypes("CheckExists"))
                Case AppTypes.Web
                    list.Add(mConditionTypes("WebCheckExists"))
            End Select

            'All HTML elements have the HTMLCheckExistsAndDocumentLoaded condition
            If elType.AppType = AppTypes.HTML Then
                list.Add(mConditionTypes("HTMLCheckExistsAndDocumentLoaded"))
            End If

            If elType.AppType = AppTypes.Web Then
                list.Add(mConditionTypes("WebCheckParentDocumentLoaded"))
            End If

            If elType.AppType = AppTypes.UIAutomation Then
                list.Add(mConditionTypes("UIAGetIsFocused"))
            End If

            'Then we add additional possible conditions based on the specific element...
            Select Case elType.ID
                Case "WindowRect"
                    list.Add(mConditionTypes("MatchImage"))
                    list.Add(mConditionTypes("ContainsImage"))
                    list.Add(mConditionTypes("ContainsColour"))
                    list.Add(mConditionTypes("UniformColour"))
                    list.Add(mConditionTypes("ReadChars"))
                    list.Add(mConditionTypes("CheckExists"))
                Case "Win32ListRegion"
                    list.Add(mConditionTypes("ListMatchImage"))
                    list.Add(mConditionTypes("ListContainsImage"))
                    list.Add(mConditionTypes("ListContainsColour"))
                    list.Add(mConditionTypes("ListUniformColour"))
                    list.Add(mConditionTypes("ListReadChars"))
                Case "Win32GridRegion"
                    list.Add(mConditionTypes("GridMatchImage"))
                    list.Add(mConditionTypes("GridContainsImage"))
                    list.Add(mConditionTypes("GridContainsColour"))
                    list.Add(mConditionTypes("GridUniformColour"))
                    list.Add(mConditionTypes("GridReadChars"))

                Case "DDEElement"
                    list.Add(mConditionTypes("CheckDDEServerAndTopicAvailable"))
                    list.Add(mConditionTypes("CheckDDEElementReadable"))
                Case "CheckBox", "RadioButton"
                    list.Add(mConditionTypes("CheckExists"))
                    list.Add(mConditionTypes("Checked"))
                Case "NetCheckBox"
                    list.Add(mConditionTypes("CheckExists"))
                    list.Add(mConditionTypes("NetChecked"))
                Case "Button"
                    list.Add(mConditionTypes("CheckExists"))
                    'TODO: Should only enable the following if we're hooked, OR, we need
                    '      to make it work when we're not hooked!!
                    list.Add(mConditionTypes("CheckButtonClicked"))
                    list.Add(mConditionTypes("MouseLeftDown"))
                Case "ComboBox"
                    list.Add(mConditionTypes("CheckExists"))
                Case "ListBox", "ListView", "Treeview", "TreeviewAx", "TabControl"
                    list.Add(mConditionTypes("CheckExists"))
                    list.Add(mConditionTypes("GetItemCount"))
                Case "ListViewAx"
                    list.Add(mConditionTypes("CheckExists"))
                    list.Add(mConditionTypes("GetItemCount"))
                Case "Edit", "Password"
                    list.Add(mConditionTypes("CheckExists"))
                Case "Window"
                    list.Add(mConditionTypes("CheckExists"))
                    list.Add(mConditionTypes("CheckWindowActive"))
                Case "Label"
                    list.Add(mConditionTypes("CheckExists"))
                Case "NetLinkLabel"
                    list.Add(mConditionTypes("CheckExists"))
                Case "TerminalField"
                    list.Add(mConditionTypes("CheckField"))
                    list.Add(mConditionTypes("GetField"))
                Case "Java", "JavaEdit", "JavaCheckBox", "JavaRadioButton", "JavaButton", "JavaScrollBar", "JavaComboBox", "JavaMenu", "JavaMenuItem", "JavaDialog", "JavaToggleButton", "JavaTabSelector", "JavaProgressBar", "JavaPasswordEdit", "JavaTrackBar", "JavaUpDown", "JavaTable", "JavaTreeView", "JavaTreeNode"
                    list.Add(mConditionTypes("JABGetText"))
                    list.Add(mConditionTypes("JABIsFocused"))
                Case "HTML", "Application"
                    If appInfo Is Nothing OrElse
                     appInfo.ID = clsApplicationTypeInfo.HTMLLaunchID OrElse appInfo.ID = clsApplicationTypeInfo.HTMLAttachID Then
                        list.Add(mConditionTypes("DocumentLoaded"))
                        list.Add(mConditionTypes("HTMLGetDocumentURLDomain"))
                        list.Add(mConditionTypes("HTMLGetDocumentURL"))
                    End If
                Case "DTPicker", "DateTimePicker", "ApexGrid", "MSFlexGrid"
                    list.Add(mConditionTypes("CheckExists"))
                Case "Toolbar"
                    list.Add(mConditionTypes("CheckExists"))
                Case "HTMLEdit"
                    list.Add(mConditionTypes("HTMLGetValue"))
                Case "UIAList"
                    list.Add(mConditionTypes("UIAGetItemCount"))
                Case "UIAComboBox"
                    list.Add(mConditionTypes("UIAComboGetItemCount"))
                Case "UIACheckBox", "UIAListItem"
                    list.Add(mConditionTypes("UIAGetToggleState"))
                Case "UIARadio"
                    list.Add(mConditionTypes("UIAGetRadioCheckedState"))
            End Select


            If elType.AppType = AppTypes.Win32 AndAlso elType.AppType <> AppTypes.DDE Then
                    list.Add(mConditionTypes("GetWindowText"))
            End If

            'Direct checking of identifiers...
            If elType.AppType = AppTypes.Win32 Then
                list.Add(mConditionTypes("CheckWindowIdentifier"))
            ElseIf elType.AppType = AppTypes.AA Then
                'AA elements can have both these kinds of identifier. They need
                'to be specified and retrieved separately.
                list.Add(mConditionTypes("CheckWindowIdentifier"))
                list.Add(mConditionTypes("CheckAAIdentifier"))
            ElseIf elType.AppType = AppTypes.HTML Then
                list.Add(mConditionTypes("CheckHTMLIdentifier"))
            ElseIf elType.AppType = AppTypes.Java Then
                list.Add(mConditionTypes("CheckJABIdentifier"))
            ElseIf elType.AppType = AppTypes.UIAutomation Then
                list.Add(mConditionTypes("CheckUIAIdentifier"))
            End If

            Return list
        End Function

        ''' <summary>
        ''' Gets a list of read action IDs available for the specified element.
        ''' </summary>
        ''' <param name="elementType">The type of element for which the read actions
        ''' must be fetched</param>
        ''' <returns>Returns a list of the IDs of the actions available for getting
        ''' data from the specified element.</returns>
        Public Shared Function GetAllowedReadActionIds(
         ByVal elementType As clsElementTypeInfo,
         ByVal appInfo As clsApplicationTypeInfo) As ICollection(Of String)
            Dim ids As New clsOrderedSet(Of String)

            'Deal with element definitions that are data driven first...
            If elementType.ReadQueries IsNot Nothing Then
                For Each action As String In elementType.ReadQueries.Keys
                    ids.Add(action)
                Next
            End If

            If appInfo IsNot Nothing Then
                If clsApplicationTypeInfo.IsMainframe(appInfo) Then
                    ids.Add("SearchTerminal")
                End If
            End If

            Select Case elementType.ID
                Case "WindowRect"
                    ids.Add("GetText")
                    ids.Add("GetTextCenter")
                    ids.Add("GetRegionWindowText")
                    ids.Add("ReadChars")
                    ids.Add("ReadBitmap")
                    ids.Add("ReadTextOCR")
                Case "Win32ListRegion"
                    ids.Add("ListGetText")
                    ids.Add("ListReadChars")
                    ids.Add("ListReadBitmap")
                    ids.Add("ListReadCharsInRange")
                    ids.Add("ListReadTextOCR")
                Case "Win32GridRegion"
                    ids.Add("GridGetText")
                    ids.Add("GridReadChars")
                    ids.Add("GridReadBitmap")
                    ids.Add("GridReadTable")
                    ids.Add("GridReadTextOCR")
                Case "CheckBox", "RadioButton"
                    ids.Add("GetChecked")
                    ids.Add("GetWindowText")
                Case "NetCheckBox"
                    ids.Add("NetGetChecked")
                    ids.Add("GetWindowText")
                Case "TabControl"
                    ids.Add("GetAllItems")
                    ids.Add("GetSelectedItemText")
                    ids.Add("GetItemCount")
                Case "ListViewAx"
                    ids.Add("GetItemCount")
                    ids.Add("GetSelectedItemText")
                    ids.Add("GetAllItems")
                Case "StatusBarAx"
                    ids.Add("GetAllItems")
                Case "ListView"
                    ids.Add("GetItemCount")
                    ids.Add("GetSelectedItemCount")
                    ids.Add("GetAllItems")
                    ids.Add("GetItem")
                    ids.Add("GetSelectedItems")
                    ids.Add("GetSelectedItemText")
                    ids.Add("GetPageCapacity")
                    ids.Add("IsItemSelected")
                    ids.Add("IsItemChecked")
                    ids.Add("IsItemFocused")
                    ids.Add("GetItemBoundsAsCollection")
                    ids.Add("GetItemScreenBoundsAsCollection")
                    ids.Add("GetItemImageIndex")
                Case "DataGrid", "DataGridView"
                    ids.Add("GetAllItems")
                Case "MSFlexGrid", "ApexGrid"
                    ids.Add("GetAllItems")
                    ids.Add("GetRowOffset")
                Case "ListBox"
                    ids.Add("GetItemCount")
                    ids.Add("GetSelectedItemCount")
                    ids.Add("GetAllItems")
                    ids.Add("GetSelectedItems")
                    ids.Add("GetSelectedItemText")
                    ids.Add("IsItemSelected")
                Case "TreeviewAx"
                    ids.Add("GetItemCount")
                    ids.Add("GetTreenodeChildItems")
                    ids.Add("GetTreenodeSiblingItems")
                Case "Treeview"
                    ids.Add("GetItemCount")
                    ids.Add("GetTreenodeChildItems")
                    ids.Add("GetTreenodeSiblingItems")
                    ids.Add("GetSelectedItemCount")
                    ids.Add("GetSelectedItemText")
                    ids.Add("GetPageCapacity")
                    ids.Add("IsItemChecked")
                    ids.Add("IsItemSelected")
                    ids.Add("IsItemExpanded")
                    ids.Add("IsItemFocused")
                Case "Toolbar"
                    ids.Add("GetItemCount")
                    ids.Add("IsToolbarButtonEnabled")
                    ids.Add("IsToolbarButtonChecked")
                    ids.Add("IsToolbarButtonPressed")
                Case "ComboBox"
                    ids.Add("GetItemCount")
                    ids.Add("GetAllItems")
                    ids.Add("GetSelectedItemText")
                Case "TrackBar", "ScrollBar", "UpDown"
                    ids.Add("GetMaxNumericValue")
                    ids.Add("GetNumericValue")
                    ids.Add("GetMinNumericValue")
                Case "DTPicker"
                    ids.Add("GetDTPickerDateTime")
                Case "DateTimePicker"
                    ids.Add("GetDateTimeValue")
                    ids.Add("GetMaxDateTimeValue")
                    ids.Add("GetMinDateTimeValue")
                Case "JavaRadioButton", "JavaCheckBox"
                    ids.Add("JABGetChecked")
                    ids.Add("JABGetText")
                Case "JavaTrackBar", "JavaScrollBar"
                    ids.Add("JABGetMaxNumericValue")
                    ids.Add("JABGetNumericValue")
                    ids.Add("JABGetMinNumericValue")
                Case "JavaUpDown"
                    ids.Add("JABGetMaxNumericValue")
                    ids.Add("JABGetNumericValue")
                    ids.Add("JABGetText")
                    ids.Add("JABGetMinNumericValue")
                Case "JavaTable"
                    ids.Add("JABGetItemCount")
                    ids.Add("JABGetSelectedItemCount")
                    ids.Add("JABGetAllItems")
                    ids.Add("JABGetSelectedItems")
                Case "JavaTreeView"
                    ids.Add("JABGetItemCount")
                    ids.Add("JABGetSelectedItemCount")
                    ids.Add("JABGetSelectedText")
                Case "JavaTreeNode"
                    ids.Add("JABGetText")
                    ids.Add("JABIsExpanded")
                    ids.Add("JABIsSelected")
                    ids.Add("JABGetItemCount")
                    ids.Add("JABGetSelectedItemCount")
                Case "JavaComboBox"
                    ids.Add("JABGetText")
                    ids.Add("JABGetItemCount")
                    ids.Add("JABGetAllItems")
                    ids.Add("JABGetSelectedItems")
                    ids.Add("JABIsExpanded")
                Case "JavaPopupMenu"
                    ids.Add("JABGetItemCount")
                    ids.Add("JABGetAllItems")
                Case "JavaProgressBar"
                    ids.Add("JABGetMaxNumericValue")
                    ids.Add("JABGetNumericValue")
                    ids.Add("JABGetMinNumericValue")
                Case "JavaListBox"
                    ids.Add("JABGetSelectedItemCount")
                    ids.Add("JABGetItemCount")
                    ids.Add("JABGetAllItems")
                    ids.Add("JABGetSelectedItems")
                    ids.Add("JABGetText")
                Case "JavaButton", "JavaToggleButton", "JavaLabel"
                    ids.Add("JABGetText")
                Case "JavaEdit"
                    ids.Add("JABGetText")
                    ids.Add("JABGetSelectedText")
                Case "JavaTabSelector"
                    ids.Add("JABGetText")
                    ids.Add("JABIsSelected")
                Case "AAElement", "AAEdit", "AAButton"
                    ids.Add("AAGetName")
                    ids.Add("AAGetValue")
                    ids.Add("AAGetDescription")
                    ids.Add("AAGetTable")
                Case "JavaMenu"
                    ids.Add("JABIsExpanded")
                    ids.Add("JABGetText")
                    ids.Add("JABGetItemCount")
                    ids.Add("JABGetAllItems")
                Case "AACheckBox", "AARadioButton"
                    ids.Add("AAGetName")
                    ids.Add("AAGetChecked")
                    ids.Add("AAGetDescription")
                Case "AAComboBox", "AAListBox"
                    ids.Add("AAGetAllItems")
                    ids.Add("AAGetSelectedItems")
                    ids.Add("AAGetItemCount")
                    ids.Add("AAGetSelectedItemText")
                    ids.Add("AAGetValue")
                    ids.Add("AAGetName")
                    ids.Add("AAGetDescription")
                Case "HTML"
                    If (appInfo IsNot Nothing) AndAlso (appInfo.ID = clsApplicationTypeInfo.HTMLLaunchID OrElse appInfo.ID = clsApplicationTypeInfo.HTMLAttachID) Then
                        ids.Add("HTMLGetDocumentURL")
                        ids.Add("HTMLGetDocumentURLDomain")
                    End If
                    ids.Add("ReadCurrentValue")
                    ids.Add("HTMLGetOuterHTML")
                    ids.Add("HTMLGetPath")
                    ids.Add("HTMLGetTable")
                Case "HTMLCombo"
                    ids.Add("HTMLCountItems")
                    ids.Add("HTMLCountSelectedItems")
                    ids.Add("HTMLGetAllItems")
                    ids.Add("HTMLGetSelectedItems")
                    ids.Add("HTMLGetSelectedItemText")
                    ids.Add("HTMLGetOuterHTML")
                    ids.Add("HTMLGetPath")
                    ids.Add("HTMLGetTable")
                Case "HTMLTable"
                    ids.Add("HTMLGetTable")
                    ids.Add("HTMLGetOuterHTML")
                    ids.Add("HTMLGetPath")
                Case "HTMLEdit"
                    ids.Add("HTMLGetOuterHTML")
                    ids.Add("ReadCurrentValue")
                    ids.Add("HTMLGetPath")
                    ids.Add("HTMLGetTable")
                Case "HTMLButton"
                    ids.Add("HTMLGetOuterHTML")
                    ids.Add("ReadCurrentValue")
                    ids.Add("HTMLGetPath")
                    ids.Add("HTMLGetTable")
                Case "HTMLCheckBox"
                    ids.Add("HTMLGetOuterHTML")
                    ids.Add("ReadCurrentValue")
                    ids.Add("HTMLGetPath")
                    ids.Add("HTMLGetTable")
                Case "HTMLRadioButton"
                    ids.Add("HTMLGetOuterHTML")
                    ids.Add("ReadCurrentValue")
                    ids.Add("HTMLGetPath")
                    ids.Add("HTMLGetTable")
                Case "TerminalField"
                    ids.Add("GetField")
                Case "Application"
                    ids.AddAll(GetAllowedReadActionIdsForApplication(appInfo))
                Case "Window"
                    ids.Add("IsMenuItemChecked")
                    ids.Add("IsMenuItemEnabled")
                    ids.Add("IsWindowActive")
                    ids.Add("IsWindowHidden")
                Case "DDEElement"
                    ids.Add("DDEGetText")

                Case "WebCheckBox"
                    ids.Add("WebGetCheckState")
                    ids.Add("WebGetLabel")
                Case "WebForm"
                    ids.Add("WebGetFormValues")
                Case "WebLink"
                    ids.Add("WebGetLinkAddress")
                    ids.Add("WebGetSelectedText")
                Case "WebList"
                    ids.Add("WebGetSelectedItems")
                    ids.Add("WebGetSelectedItemsText")
                    ids.Add("WebGetItems")
                Case "WebListItem"
                    ids.Add("WebGetIsSelected")
                Case "WebRadio"
                    ids.Add("WebGetCheckState")
                    ids.Add("WebGetLabel")
                Case "WebProgressBar"
                    ids.Add("WebGetMaxValue")
                    ids.Add("WebGetValue")
                Case "WebSlider"
                    ids.Add("WebGetValue")
                    ids.Add("WebGetMaxValue")
                    ids.Add("WebGetMinValue")
                    ids.Add("WebGetLabel")
                Case "WebTable"
                    ids.Add("WebGetTableItems")
                    ids.Add("WebGetTableItem")
                    ids.Add("WebGetColumnCount")
                    ids.Add("WebGetRowCount")
                    ids.Add("WebGetSelectedText")
                Case "WebText"
                    ids.Add("WebGetSelectedText")
                Case "WebTextEdit"
                    ids.Add("WebGetSelectedText")
                    ids.Add("WebGetLabel")

                Case "UIAElement"
                    ids.Add("UIAGetValue")
                    ' Plus default actions added below
                Case "UIAButton"
                    ids.Add("UIAGetPressedState")
                    ids.Add("UIAGetExpanded")
                Case "UIACheckBox"
                    ids.Add("UIAGetToggleState")
                Case "UIARadio"
                    ids.Add("UIAGetRadioCheckedState")
                Case "UIAEdit"
                    ids.Add("UIAGetValue")
                    ids.Add("UIAGetSelectedText")
                Case "UIAMenuItem"
                    ids.Add("UIAGetExpanded")
                Case "UIAComboBox"
                    ids.Add("UIAGetExpanded")
                    ids.Add("UIAComboGetAllItems")
                    ids.Add("UIAComboGetItemCount")
                    ids.Add("UIAGetValue")
                Case "UIAList"
                    ids.Add("UIAGetSelectedItemText")
                    ids.Add("UIAGetSelectedItems")
                    ids.Add("UIAGetItemCount")
                    ids.Add("UIAGetAllItems")
                Case "UIAListItem"
                    ids.Add("UIAGetToggleState")
                    ids.Add("UIAGetExpanded")
                Case "UIATable"
                    ids.Add("UIATableReadCellText")
                    ids.Add("UIATableRows")
                    ids.Add("UIATableRowCount")
                    ids.Add("UIATableColumnCount")
                    ids.Add("UIATableSelectedRowNumber")
                    ids.Add("UIATableSelectedColumnNumber")
                    ids.Add("UIATableReadToggleState")
                    ids.Add("UIATableExpanded")
                    ids.Add("UIATableCountComboboxItems")
                    ids.Add("UIATableGetAllComboboxItems")
                    ids.Add("UIATableGetSelectedComboboxItem")
                    ids.Add("UIATableGetSelectedRows")
                Case "UIATabControl"
                    ids.Add("UIAGetAllTabsText")
                    ids.Add("UIAGetSelectedItemText")
                Case "UIATabItem"
                    ids.Add("UIAGetIsItemSelected")
                Case "UIATreeView"
                    ids.Add("UIATreeIsExpanded")
                    ids.Add("UIAGetSelectedItems")
                    ids.Add("UIAGetSelectedItemText")
                Case "UIATreeViewItem"
                    ids.Add("UIAGetIsItemSelected")
                    ids.Add("UIAGetToggleState")
                    ids.Add("UIAGetExpanded")
            End Select

            If elementType.AppType = AppTypes.UIAutomation Then
                ids.Add("UIAGetName")
            End If

            If elementType.AppType = AppTypes.Web Then
                ids.Add("WebGetAttribute")
                ids.Add("WebGetIsVisible")
                ids.Add("WebGetIsOnScreen")
                ids.Add("WebGetText")
                ids.Add("WebGetValue")
                ids.Add("WebGetId")
                ids.Add("WebGetPageUrl")
                ids.Add("WebGetPath")
                ids.Add("WebGetHtml")
            End If

            'Every Win32 element can have its window text read, even though it might
            'not be meaniningful (eg on a listview)
            If elementType.AppType = AppTypes.Win32 AndAlso
             Not ids.Contains("GetRegionWindowText") Then
                ids.Add("GetWindowText")
            End If

            'FallBack
            If ids.Count = 0 AndAlso elementType.AppType <> AppTypes.SAP Then
                ids.Add("ReadCurrentValue")
            End If

            'Direct retrieval of identifiers...
            If elementType.AppType = clsElementTypeInfo.AppTypes.Win32 Then
                ids.Add("GetWindowIdentifier")
            ElseIf elementType.AppType = clsElementTypeInfo.AppTypes.AA Then
                'AA elements can have both these kinds of identifier. They need
                'to be specified and retrieved separately.
                ids.Add("GetWindowIdentifier")
                ids.Add("GetAAIdentifier")
            ElseIf elementType.AppType = clsElementTypeInfo.AppTypes.HTML Then
                ids.Add("GetHTMLIdentifier")
            ElseIf elementType.AppType = clsElementTypeInfo.AppTypes.Java Then
                ids.Add("GetJABIdentifier")
            End If

            ids.AddAll(GetAllowedReadActionIdsForBounds(elementType)) 
            
            Return ids
        End Function

        Private Shared Function GetAllowedReadActionIdsForBounds(elementType As clsElementTypeInfo) As clsOrderedSet(Of String)
             Dim ids As clsOrderedSet(Of String) = New clsOrderedSet(Of String)

            'Almost Everything can have its bounds read.
            '(NB this comes after the "fallback" check, otherwise
            'the count == 0 condition becomes harder to maintain)
            Select Case elementType.AppType
                Case AppTypes.Mainframe, AppTypes.DDE
                    ' Can't retrieve the bounds for mainframe or DDE elements
                Case AppTypes.Web
                    ids.Add("WebGetBounds")
                Case AppTypes.Win32
                    Dim pre As String = Nothing
                    Select Case elementType.ID
                        Case "WindowRect" : pre = "Region"
                        Case "Win32ListRegion" : pre = "ListRegion"
                        Case "Win32GridRegion" : pre = "GridRegion"
                        Case Else : pre = ""
                    End Select
                    ' Non-region Win32 elements use 'GetRelativeElementBounds';
                    ' Region elements use '<prefix>GetElementBounds'
                    If pre = "" Then ids.Add("GetRelativeElementBounds")
                    ids.Add(pre + "GetElementBounds")
                    ids.Add(pre + "GetElementScreenBounds")
                Case AppTypes.UIAutomation
                    ids.Add("UIAGetElementScreenBounds")
                    ids.Add("UIAGetRelativeElementBounds")
                    ids.Add("GetUIAIdentifier")
                Case AppTypes.Web
                    ids.Add("WebGetBounds")
                Case Else
                    ids.Add("GetElementBounds")
                    ids.Add("GetElementScreenBounds")

            End Select

            Return ids
        End Function
        Private Shared Function GetAllowedReadActionIdsForApplication(appInfo As clsApplicationTypeInfo) As clsOrderedSet(Of String)
            Dim ids As clsOrderedSet(Of String) = New clsOrderedSet(Of String)
            If clsApplicationTypeInfo.HasParentWindowSupport(appInfo) Then
                ids.Add("GetMainframeParentWindowTitle")
            End If
            If clsApplicationTypeInfo.IsMainframe(appInfo) Then
                ids.Add("GetMainframeCursorPos")
            End If
            If (appInfo IsNot Nothing) Then
                Select Case appInfo.ID

                    Case clsApplicationTypeInfo.HTMLLaunchID, clsApplicationTypeInfo.HTMLAttachID
                        ids.Add("HTMLGetDocumentURL")
                        ids.Add("HTMLGetDocumentURLDomain")
                        ids.Add("HTMLSnapshot")
                        ids.Add("WindowsSnapshot")
                    Case clsApplicationTypeInfo.Win32LaunchID, clsApplicationTypeInfo.Win32AttachID, clsApplicationTypeInfo.JavaLaunchID,
                         clsApplicationTypeInfo.JavaAttachID, clsApplicationTypeInfo.CitrixJavaLaunchID, clsApplicationTypeInfo.CitrixJavaAttachID
                        ids.Add("WindowsSnapshot")
                End Select
            End If
            If appInfo IsNot Nothing AndAlso appInfo.ID = "BrowserLaunch" Then
                ids.Add("WebIsConnected")
            Else
                ids.Add("IsConnected")
            End If

            ids.Add("GetConflictingFontCharacters")

            Return ids
        End function
        ''' <summary>
        ''' Gets a list of read actions available for the specified element.
        ''' </summary>
        ''' <param name="elementType">The type of element for which the read actions
        ''' must be fetched</param>
        ''' <returns>Returns a list of the actions available for getting data
        ''' from the specified element.</returns>
        Public Shared Function GetAllowedReadActions(
         ByVal elementType As clsElementTypeInfo,
         ByVal appInfo As clsApplicationTypeInfo) As List(Of clsActionTypeInfo)
            Dim list As New List(Of clsActionTypeInfo)
            For Each id As String In GetAllowedReadActionIds(elementType, appInfo)
                list.Add(mActionTypes(id))
            Next
            Return list
        End Function

        ''' <summary>
        ''' Wait for a certain condition to be true based on a list of condition criteria,
        ''' or if the specified timeout elapses return a timeout indicator. This function
        ''' returns -1 for error, 0 for timeout, or the 1 based index of the choice
        ''' condition that matches the criteria.
        ''' </summary>
        ''' <param name="conditions">A list of clsWaitInfo objects that specify the
        ''' element, condition, and expected reply to wait for</param>
        ''' <param name="timeout">The value in milliseconds to wait until timeout.</param>
        ''' <param name="Err">In the event of an error, this contains an error
        ''' description</param>
        ''' <returns>-1 for error, 0 for timeout, or the 1 based index of the choice
        ''' condition that matches the criteria.</returns>
        Public Function DoWait(ByVal conditions As List(Of clsWaitInfo), ByVal timeout As Integer, ByRef Err As clsAMIMessage) As Integer

            If mTargetAppInfo Is Nothing Then
                Err = New clsAMIMessage(clsAMIMessage.CommonMessages.NoTargetAppInformation)
                Return -1
            End If

            Dim sQuery As String = "wait timeout=" & timeout.ToString

            Dim externalApplicationManagerTimeout As TimeSpan = TimeSpan.Zero
            If Not TryGetApplicationManagerTimeout(Nothing, externalApplicationManagerTimeout) Then
                Err = New clsAMIMessage(My.Resources.UnableToParseExternalApplicationManagerTimeoutParameter)
            End If

            For Each c As clsWaitInfo In conditions
                Dim sSubQuery As String
                Select Case c.Condition.CommandID
                    Case "CheckButtonClicked"
                        sSubQuery = "checkevent eventtype=ButtonPressed"
                    Case "MouseLeftDown"
                        sSubQuery = "checkevent eventtype=MouseLeftDown"
                    Case "CheckExists"
                        'Select the correct query, which depends on the type of element.
                        'The element type IDs are appropriately prefixed to make this
                        'easy.
                        If c.ElementType.AppType = AppTypes.HTML Then
                            sSubQuery = "HTMLCheckExists"
                        ElseIf c.ElementType.AppType = AppTypes.AA Then
                            sSubQuery = "AACheckExists"
                        ElseIf c.ElementType.AppType = AppTypes.UIAutomation Then
                            sSubQuery = "UIACheckExists"
                        ElseIf c.ElementType.AppType = AppTypes.Web Then
                            sSubQuery = "WebCheckExists"
                        ElseIf c.ElementType.AppType = AppTypes.Java Then
                            sSubQuery = "JABCheckExists"
                        ElseIf c.ElementType.AppType = AppTypes.SAP Then
                            sSubQuery = "sapcheckexists"
                        ElseIf c.ElementType.AppType = AppTypes.Win32 AndAlso
                         c.ElementType.ID = "WindowRect" Then
                            sSubQuery = "RegionCheckExists"
                        Else
                            sSubQuery = "checkwindow"
                        End If
                    Case "Checked"
                        sSubQuery = "getchecked"
                    Case "NetChecked"
                        sSubQuery = "getdotnetcontrolproperty newtext=Checked"
                    Case "DocumentLoaded"
                        sSubQuery = "HTMLDocumentLoaded"
                    Case "CheckWindowIdentifier"
                        sSubQuery = "getidentifier idtype=window"
                        TranslateIDName(c.Arguments)
                    Case "CheckHTMLIdentifier"
                        sSubQuery = "getidentifier idtype=html"
                        TranslateIDName(c.Arguments)
                    Case "CheckJABIdentifier"
                        sSubQuery = "getidentifier idtype=jab"
                        TranslateIDName(c.Arguments)
                    Case "CheckAAIdentifier"
                        sSubQuery = "getidentifier idtype=aa"
                        TranslateIDName(c.Arguments)
                    Case "CheckUIAIdentifier"
                        sSubQuery = "getidentifier idtype=uia"
                        TranslateIDName(c.Arguments)
                    Case "MatchImage", "ListMatchImage", "GridMatchImage"
                        sSubQuery = "matchimage"
                    Case "ContainsImage", "ListContainsImage", "GridContainsImage"
                        sSubQuery = "containsimage"
                    Case "ContainsColour", "ListContainsColour", "GridContainsColour"
                        sSubQuery = "containscolour"
                    Case "UniformColour", "ListUniformColour", "GridUniformColour"
                        sSubQuery = "uniformcolour"
                    Case "ReadChars", "ListReadChars", "GridReadChars"
                        sSubQuery = "readtext"
                    Case Else
                        sSubQuery = c.Condition.CommandID
                End Select
                'Add the identifiers...
                AppendIdentifiersToQuery(sSubQuery, c.Identifiers)
                'Add the arguments.
                AppendArgumentsToQuery(sSubQuery, c.Arguments)
                sQuery &= " {" & sSubQuery & "}" &
                 GetComparisonTypeSymbol(c.ComparisonType) &
                 clsQuery.EncodeValue(c.ExpectedReply)
            Next

            If mTargetApp Is Nothing Then
                Err = New clsAMIMessage(clsAMIMessage.CommonMessages.NotConnected)
                Return -1
            End If

            'Execute and return result...
            Dim sResult As String = mTargetApp.ProcessQuery(sQuery, externalApplicationManagerTimeout)
            Dim sResType As String = Nothing
            clsQuery.ParseResponse(sResult, sResType, sResult)
            Select Case sResType
                Case "RESULT"
                    Return Integer.Parse(sResult)
                Case "ERROR"
                    Err = clsAMIMessage.Parse(sResult)
                    Return -1
                Case Else
                    Err = New clsAMIMessage(My.Resources.UnrecognisedResponse)
                    Return -1
            End Select

        End Function


        ''' <summary>
        ''' Subtract one from some numeric arguments.
        ''' </summary>
        ''' <param name="args">A dictionary of arguments for the action. The key is
        ''' the argument ID (as retrieved from clsArgumentInfo), and the value is the
        ''' Automate-encoded data value, matching the data type of the argument.</param>
        ''' <param name="ids">The IDs of the arguments to alter. They must all be
        ''' numbers, or bad things will happen.</param>
        Private Sub SubtractOneFromArgs(
         ByVal args As Dictionary(Of String, String), ByVal ParamArray ids() As String)
            For Each arg As String In ids
                If args.ContainsKey(arg) Then
                    args(arg) = (Integer.Parse(args(arg)) - 1).ToString()
                End If
            Next
        End Sub


        ''' <summary>
        ''' Get the final query for a 'data driven' (from external XML) definition,
        ''' which allows for argument substitution, and "pre-queries" which can generate
        ''' additional arguments.
        ''' </summary>
        ''' <param name="query">The query being processed. This will be amended
        ''' accordingly, such that on return, it contains a single 'normal' query to
        ''' be executed.</param>
        ''' <param name="args">A dictionary of arguments for the action. The key is
        ''' the argument ID (as retrieved from clsArgumentInfo), and the value is the
        ''' Automate-encoded data value, matching the data type of the argument.</param>
        ''' <param name="idents">Identifiers for the element</param>
        ''' <param name="err">In the event of an error, this will contain an error
        ''' description.</param>
        ''' <returns>True if successful, False otherwise</returns>
        Private Function GetFinalDDQuery(ByRef query As String, ByVal args As Dictionary(Of String, String), ByVal idents As List(Of clsIdentifierInfo), ByRef err As clsAMIMessage, timeout As TimeSpan) As Boolean

            'Process any pre-queries, and add their outputs into the arguments...
            While query.StartsWith("@")
                query = query.Substring(1)
                Dim index As Integer = query.IndexOf("=")
                If index = -1 Then
                    err = New clsAMIMessage(My.Resources.PrequerySyntaxErrorMissing)
                    Return False
                End If
                Dim newargname As String = query.Substring(0, index)
                query = query.Substring(index + 1)
                If Not query.StartsWith("{") Then
                    err = New clsAMIMessage(My.Resources.PrequerySyntaxErrorMissing_1)
                    Return False
                End If
                query = query.Substring(1)
                index = query.IndexOf("}")
                If index = -1 Then
                    err = New clsAMIMessage(My.Resources.PrequerySyntaxErrorMissing_2)
                    Return False
                End If
                Dim prequery As String = query.Substring(0, index)
                query = query.Substring(index + 1)
                query = query.TrimStart()

                prequery = SubstituteArguments(prequery, args)
                AppendArgumentsToQuery(prequery, args)
                AppendIdentifiersToQuery(prequery, idents)
                Dim response As String = Nothing, resType As String = Nothing
                Dim result As String = Nothing
                If mTargetApp Is Nothing Then
                    err = New clsAMIMessage(My.Resources.NotConnected)
                    Return False
                End If
                response = mTargetApp.ProcessQuery(prequery, timeout)
                clsQuery.ParseResponse(response, resType, result)
                Select Case resType
                    Case "ERROR", "WARNING"
                        err = clsAMIMessage.Parse(response)
                        Return False
                    Case "OK"
                        args.Add(newargname, "OK")
                    Case "RESULT"
                        args.Add(newargname, result)
                    Case Else
                        err = clsAMIMessage.Parse(String.Format(My.Resources.UnknownResponseType0, resType))
                        Return False
                End Select

            End While

            query = SubstituteArguments(query, args)
            Return True

        End Function

        ''' <summary>
        ''' Substitute arguments into a query string, where the argument names are
        ''' delimited by $ - e.g. "mycommand something=$arg$" would have the 'arg'
        ''' replaced by the argument value.
        ''' </summary>
        ''' <param name="query">The source query string.</param>
        ''' <param name="args">The arguments to use for substitution.</param>
        ''' <returns>A new query string with the substitution performed.</returns>
        Private Function SubstituteArguments(ByVal query As String, ByVal args As Dictionary(Of String, String)) As String
            For Each arg As String In args.Keys
                query = Regex.Replace(query, "\$" & arg & "\$", args(arg), RegexOptions.IgnoreCase)
            Next
            Return query
        End Function

        ''' <summary>
        ''' Gets the default read action to use for a given element type in place of
        ''' the obsolete 'ReadCurrentValue' action.
        ''' </summary>
        ''' <param name="elementTypeId">The ID of the element type for which the
        ''' default read action is required.</param>
        ''' <returns>The read action to use in place of the 'ReadCurrentValue' action
        ''' for elements of the specified type.</returns>
        Private Shared Function GetDefaultReadAction(ByVal elementTypeId As String) _
         As String

            'Backwards compatibility for old read methods. Where possible we prefer
            'a specific read action such as 'Get Window Text' or 'Count Items'
            Select Case elementTypeId
                Case "TerminalField"
                    Return "GetField"
                Case "WindowRect"
                    Return "GetText"
                Case "AAElement", "AAButton", "AAEdit"
                    Return "AAGetValue"
                Case "AACheckBox", "AARadioButton"
                    Return "AAGetChecked"
                Case "HTML", "HTMLEdit"
                    Return "HTMLGetValue"
                Case "HTMLCombo"
                    Return "HTMLGetSelectedItemText"
                Case "Java", "JavaEdit", "JavaPasswordEdit"
                    Return "JABGetText"
                Case "JavaCheckBox", "JavaRadioButton"
                    Return "JABGetChecked"
                Case "CheckBox", "RadioButton"
                    Return "GetChecked"
                Case "HTMLCheckBox", "HTMLRadioButton"
                    Return "HTMLGetChecked"
                Case "TrackBar", "UpDown", "ScrollBar"
                    Return "GetNumericValue"
                Case "JavaScrollBar", "JavaProgressBar", "JavaUpDown", "JavaTrackBar"
                    Return "JABGetValue"
                Case "DateTimePicker", "MonthCalPicker"
                    Return "GetDatetimeValue"
                Case Else
                    Return "GetWindowText"
            End Select
        End Function



        ''' <summary>
        ''' Perform an action, by translating to a query and passing that down to the
        ''' object model.
        ''' </summary>
        ''' <param name="actionID">The ID of the action type to perform. This must be
        ''' the .ID property from an clsActionTypeInfo instance previously returned from
        ''' this object via GetAllowedActions(). There is also a 'special case' action ID
        ''' also supported here - "Write" which is used by Write stages (see <see
        ''' cref="WriteActionID"/>).</param>
        ''' <param name="elementType">The element type</param>
        ''' <param name="idents">Identifiers for the element</param>
        ''' <param name="args">A dictionary of arguments for the action. The key is
        ''' the argument ID (as retrieved from clsArgumentInfo), and the value is the
        ''' Automate-encoded data value, matching the data type of the argument.</param>
        ''' <param name="result">If successful, this will contain the result of the
        ''' action.</param>
        ''' <param name="err">In the event of an error, this will contain an error
        ''' description.</param>
        ''' <returns>True if successful, False otherwise</returns>
        Public Function DoAction(ByVal actionID As String, ByVal elementType As clsElementTypeInfo, ByVal idents As List(Of clsIdentifierInfo), ByVal args As Dictionary(Of String, String), ByRef result As String, ByRef err As clsAMIMessage) As Boolean

            If mTargetAppInfo Is Nothing Then
                err = New clsAMIMessage(clsAMIMessage.CommonMessages.NoTargetAppInformation)
                Return False
            End If

            Dim sErr As String = Nothing

            Dim sQuery As String = Nothing

            Dim externalAppmanTimeout As TimeSpan = TimeSpan.Zero
            If Not TryGetApplicationManagerTimeout(args, externalAppmanTimeout) Then
                err = New clsAMIMessage(My.Resources.UnableToParseExternalApplicationManagerTimeoutParameter)
            End If

            'Write is a special case - it's not even a real action ID!
            If actionID = "Write" Then

                If elementType.Readonly Then
                    err = New clsAMIMessage(
                        String.Format(My.Resources.CanNotWriteToElementsOfType0BecauseTheyAreReadonly, elementType.ID))
                    Return False
                End If

                If elementType.WriteQuery IsNot Nothing Then
                    sQuery = elementType.WriteQuery
                    If Not GetFinalDDQuery(sQuery, args, idents, err, externalAppmanTimeout) Then Return False
                Else
                    Select Case elementType.ID
                        Case "SAPTextBox"
                            sQuery = "sapsetproperty propname=text arguments=" & args("NewText")
                        Case "TerminalField"
                            sQuery = "setfield"
                        Case "WindowRect"
                            err = New clsAMIMessage(String.Format("Can't write to a {0}", elementType.Name))
                            Return False
                        Case "CheckBox", "RadioButton"
                            sQuery = "setchecked"
                        Case "NetCheckBox"
                            sQuery = "setdotnetcontrolproperty propname=Checked"
                        Case "ListBox"
                            sQuery = "selectitem"
                        Case "JavaCheckBox", "JavaRadioButton", "JavaToggleButton"
                            sQuery = "jabsetchecked"
                        Case "JavaComboBox"
                            sQuery = "jabsettext"
                        Case "AAElement", "AAButton", "AAEdit"
                            sQuery = "aasetvalue"
                        Case "AAListBox", "AAComboBox"
                            sQuery = "AASelectItem"
                        Case "AACheckBox", "AARadioButton"
                            sQuery = "aasetchecked"
                        Case "Java", "JavaEdit"
                            sQuery = "jabsettext"
                        Case "JavaPasswordEdit"
                            sQuery = "jabsetpasswordtext"
                        Case "JavaScrollBar", "JavaUpDown", "JavaTrackBar"
                            sQuery = "jabsettext"
                        Case "HTML", "HTMLEdit"
                            sQuery = "HTMLSetValue"
                        Case "HTMLCheckBox", "HTMLRadioButton"
                            sQuery = "HTMLSetChecked"
                        Case "TrackBar", "UpDown", "ScrollBar"
                            sQuery = "setnumericvalue numericvalue=" & args("NewText")
                        Case "DateTimePicker", "MonthCalPicker"
                            sQuery = "setdatetimevalue"
                        Case "DTPicker"
                            sQuery = "setdtpickerdatetime"
                        Case "HTMLCombo"
                            sQuery = "htmlselectitem"
                        Case "ComboBox"
                            sQuery = "SelectItem"
                        Case "DDEElement"
                            sQuery = "DDESetText"
                        Case "Password"
                            sQuery = "SetPasswordText"
                        Case "UIAEdit"
                            sQuery = "UIASetValue"
                        Case "UIAButton", "UIACheckBox"
                            sQuery = "UIASetToggleState"
                        Case "UIAComboBox"
                            sQuery = "UIASetValue"
                        Case "UIARadio"
                            sQuery = "UIARadioSetChecked"
                        Case "WebElement"
                            sQuery = "WebSetValue"
                        Case "WebCheckBox"
                            sQuery = "WebSetCheckState"
                        Case "WebProgressBar"
                            sQuery = "WebSetValue"
                        Case "WebSlider"
                            sQuery = "WebSetValue"
                        Case "WebTextEdit"
                            sQuery = "WebSetValue"
                        Case Else
                            sQuery = "settext"
                    End Select
                End If

            Else

                Dim outputDictionary = New Dictionary(Of String, String)

                'Handle data driven actions first...
                If elementType.ActionQueries IsNot Nothing AndAlso elementType.ActionQueries.ContainsKey(actionID) Then
                    sQuery = elementType.ActionQueries(actionID)
                    If Not GetFinalDDQuery(sQuery, args, idents, err, externalAppmanTimeout) Then Return False
                End If
                If sQuery Is Nothing AndAlso elementType.ReadQueries IsNot Nothing AndAlso elementType.ReadQueries.ContainsKey(actionID) Then
                    sQuery = elementType.ReadQueries(actionID)
                    If Not GetFinalDDQuery(sQuery, args, idents, err, externalAppmanTimeout) Then Return False
                End If

                ' Formulate query...
                If sQuery Is Nothing Then

                    ' Get the action corresponding to the action ID and ensure that
                    ' we get the appropriate command ID. If there's no action found,
                    ' just leave it to execute as before command IDs came along
                    Dim action As clsActionTypeInfo = Nothing
                    If mActionTypes.TryGetValue(actionID, action) Then _
                     actionID = action.CommandID

                    Select Case actionID
                        Case "ReadCurrentValue"
                            sQuery = GetDefaultReadAction(elementType.ID)

                        Case "Launch"
                            'Use our internal launcher, rather than formulating a query...

                            If Not LaunchApplication(err, args, outputDictionary) Then
                                Return False
                            Else
                                result = FormulateOutputString(outputDictionary)
                                Return True
                            End If
                        Case "LaunchMainframe"

                            Dim mode = GetProcessMode(args)
                            Dim options = GetOptions(args)
                            If Not LaunchMainframe(args, options, mode, err, externalAppmanTimeout) Then
                                Return False
                            Else
                                result = String.Empty
                                Return True
                            End If
                        Case "AttachMainframe"

                            Dim mode = GetProcessMode(args)
                            Dim options = GetOptions(args)
                            If Not AttachMainframe(args, options, mode, err, externalAppmanTimeout) Then
                                Return False
                            Else
                                result = String.Empty
                                Return True
                            End If
                        Case "AttachApplication"
                            If AttachApplication(args, err, outputDictionary) Then
                                result = FormulateOutputString(outputDictionary)
                                Return True
                            Else
                                Return False
                            End If
                        Case "Terminate"
                            Select Case mTargetAppInfo.ID
                                Case clsApplicationTypeInfo.Win32LaunchID, clsApplicationTypeInfo.Win32AttachID, clsApplicationTypeInfo.HTMLLaunchID,
                                     clsApplicationTypeInfo.HTMLAttachID, clsApplicationTypeInfo.JavaLaunchID, clsApplicationTypeInfo.JavaAttachID,
                                     clsApplicationTypeInfo.CitrixAttachID, clsApplicationTypeInfo.CitrixLaunchID, clsApplicationTypeInfo.CitrixJavaAttachID,
                                     clsApplicationTypeInfo.CitrixJavaLaunchID
                                    sQuery = "closeapplication"
                                Case clsApplicationTypeInfo.BrowserLaunchId, clsApplicationTypeInfo.BrowserAttachId, clsApplicationTypeInfo.CitrixBrowserAttachID,
                                     clsApplicationTypeInfo.CitrixBrowserLaunchID
                                    sQuery = "WebCloseApplication"
                                Case Else
                                    If clsApplicationTypeInfo.IsMainframe(mTargetAppInfo) Then
                                        sQuery = "endsession"
                                    Else
                                        err = clsAMIMessage.Parse("ERROR:" & String.Format(My.Resources.ActionTerminateIsNotSupportedForApplicationOfType0, mTargetAppInfo.ID))
                                        Return False
                                    End If
                            End Select
                        Case "IsConnected"
                            result = (mTargetApp IsNot Nothing).ToString()
                            Return True
                        Case "WebIsConnected"
                            If mTargetApp IsNot Nothing AndAlso args.ContainsKey("trackingid") AndAlso Not String.IsNullOrWhiteSpace(args.Item("trackingid")) Then
                                sQuery = "WebIsConnected"
                            Else
                                result = (mTargetApp IsNot Nothing).ToString()
                                Return True
                            End If
                        Case "GetWindowIdentifier"
                            sQuery = "getidentifier idtype=window"
                            TranslateIDName(args)
                        Case "GetAAIdentifier"
                            sQuery = "getidentifier idtype=aa"
                            TranslateIDName(args)
                        Case "GetHTMLIdentifier"
                            sQuery = "getidentifier idtype=html"
                            TranslateIDName(args)
                        Case "GetJABIdentifier"
                            sQuery = "getidentifier idtype=jab"
                            TranslateIDName(args)
                        Case "GetUIAIdentifier"
                            sQuery = "getidentifier idtype=uia"
                            TranslateIDName(args)

                        Case "JABSnapshot"
                            'Backward compatibility...
                            sQuery = "WindowsSnapshot"

                        Case "Click"
                            'TODO: Could also use MouseClick here, correct choice might depend
                            '      on the element or application type.
                            sQuery = "MouseClick"
                        Case "ClickLink"
                            If elementType.ID = "NetLinkLabel" Then
                                sQuery = "NetClickLinkLabel"
                            End If
                        Case "DoJava"
                            sQuery = "jabaction"

                            ' Recognise*Text is here for back-compat only
                            ' ReadChars is the new RecogniseText
                        Case "RecogniseText", "ListRecogniseText", "GridRecogniseText"
                            sQuery = "readtext origalgorithm=true"

                        Case "RecogniseSingleLineText", "ListRecogniseSingleLineText", "GridRecogniseSingleLineText"
                            sQuery = "readtext"

                        Case "ListReadSingleTextInRange"
                            sQuery = "readtextmultielement"

                        Case "RecogniseMultiLineText", "ListRecogniseMultiLineText", "GridRecogniseMultiLineText"
                            sQuery = "readtext multiline=true"

                        Case "ListReadMultiTextInRange"
                            sQuery = "readtextmultielement multiline=true"

                            ' The List and Grid actions aren't actually any different
                            ' They only differ in the parameters they send (Element
                            ' Number, GridSchema, etc).
                        Case "ReadChars"
                            sQuery = "readtext"

                        Case "ListReadCharsInRange"
                            sQuery = "readtextmultielement"

                        Case "GridReadTable"
                            sQuery = "readtextgrid"

                        Case "ReadBitmap", "ListReadBitmap", "GridReadBitmap"
                            sQuery = "readbitmap"

                        Case "ReadTextOCR"
                            sQuery = $"readtextocr engine={ mGlobalInfo.TesseractEngine }"

                            ' Note - this is GetText, ie. wait stage: read multiple
                            ' controls within a region, not GetWindowText in a read
                        Case "GetText", "ListGetText", "GridGetText"
                            sQuery = "gettext"

                        Case "Press"
                            Select Case elementType.ID
                                Case "Java", "JavaCheckBox", "JavaRadioButton", "JavaButton", "JavaToggleButton", "JavaMenuItem"
                                    sQuery = "jabaction action=click"
                                Case "AAButton"
                                    sQuery = "AADefAction"
                                Case "SAPButton"
                                    sQuery = "sapinvokemethod methodname=press"
                                Case Else
                                    sQuery = "pressbutton"
                            End Select
                        Case "SetChecked"
                            Select Case elementType.ID
                                Case "HTML", "HTMLCheckBox", "HTMLRadioButton"
                                    sQuery = "htmlsetchecked"
                                Case "JavaCheckBox", "JavaRadioButton", "JavaToggleButton"
                                    sQuery = "jabsetchecked"
                                Case "NetCheckBox"
                                    sQuery = "setdotnetcontrolproperty propname=Checked"
                                Case Else
                                    sQuery = "setchecked"
                            End Select
                        Case "NetGetChecked"
                            sQuery = "getdotnetcontrolproperty newtext=Checked"
                        Case "CheckExists"
                            Select Case elementType.ID
                                Case "HTML"
                                    sQuery = "HTMLCheckExists"
                                Case "AAElement", "AAButton", "AACheckBox", "AARadioButton", "AAEdit"
                                    sQuery = "AACheckExists"
                                Case "UIAElement"
                                    sQuery = "UIACheckExists"
                                Case "WebElement"
                                    sQuery = "WebCheckExists"
                                Case "Java", "JavaEdit", "JavaButton", "JavaCheckBox", "JavaRadioButton", "JavaScrollBar", "JavaToggleButton", "JavaTabSelector", "JavaProgressBar", "JavaPasswordEdit", "JavaTrackBar", "JavaUpDown"
                                    sQuery = "jabcheckexists"
                                Case "WindowRect"
                                    sQuery = "RegionCheckExists"
                                Case Else
                                    sQuery = "checkwindow"
                            End Select
                        Case "SelectItem"
                            Select Case elementType.ID
                                Case "HTML", "HTMLCombo"
                                    sQuery = "htmlselectitem"
                                Case Else
                                    sQuery = "selectitem"
                            End Select
                        Case "SelectTreeNode"
                            If elementType.ID <> "TreeviewAx" Then
                                err = New clsAMIMessage(My.Resources.ThatActionIsOnlyValidForAnActiveXTreeView)
                                Return False
                            End If
                            sQuery = "axtreeviewselectnode"
                        Case "Focus"
                            Select Case elementType.ID
                                Case "HTML"
                                    sQuery = "HTMLFocus"
                                Case Else
                                    sQuery = actionID
                            End Select
                        Case "Default"
                            sQuery = "AADefAction"
                        Case "DragItem"
                            sQuery = "DragListviewItem"
                        Case "DropOntoItem"
                            sQuery = "DropOntoListviewItem"
                        Case "JABGetSelectedItems"
                            sQuery = "JABGetAllItems selecteditemsonly=true"
                        Case "GetAllItems"
                            Select Case elementType.ID
                                Case "DataGrid", "DataGridView"
                                    sQuery = "getdatagriddata"
                                Case "MSFlexGrid"
                                    sQuery = "getmsflexgridcontents"
                                Case "ApexGrid"
                                    sQuery = "getapexgridcontents"
                                Case "ListViewAx"
                                    sQuery = "getlistviewcontents"
                                Case "StatusBarAx"
                                    sQuery = "getstatusbarcontents"
                                Case "SAPGridView"
                                    sQuery = "sapgetallgriditems"
                                Case Else
                                    sQuery = "getallitems"
                            End Select
                        Case "GetRowOffset"
                            Select Case elementType.ID
                                Case "MSFlexGrid"
                                    sQuery = "msflexgridgetrowoffset"
                                    SubtractOneFromArgs(args, "starty")
                                Case Else
                                    err = New clsAMIMessage(String.Format(My.Resources.GetRowOffsetDoesNotWorkWithElementsOfType0, elementType.ID))
                                    Return False
                            End Select
                        Case "SetTopRow"
                            Select Case elementType.ID
                                Case "MSFlexGrid"
                                    sQuery = "msflexgridsettoprow"
                                    SubtractOneFromArgs(args, "starty")
                                Case Else
                                    err = New clsAMIMessage(String.Format(My.Resources.SetTopRowDoesNotWorkWithElementsOfType0, elementType.ID))
                                    Return False
                            End Select
                        Case "GoToCell"
                            Select Case elementType.ID
                                Case "MSFlexGrid"
                                    sQuery = "msflexgridgoto"
                                    SubtractOneFromArgs(args, "startx", "starty")
                                Case "ApexGrid"
                                    sQuery = "apexgridgoto"
                                    SubtractOneFromArgs(args, "startx", "starty")
                                Case Else
                                    err = New clsAMIMessage(String.Format(My.Resources.GoToCellDoesNotWorkWithElementsOfType0, elementType.ID))
                                    Return False
                            End Select
                        Case "SelectRange"
                            Select Case elementType.ID
                                Case "MSFlexGrid"
                                    sQuery = "msflexgridselect"
                                    SubtractOneFromArgs(args, "startx", "starty", "endx", "endy")
                                Case Else
                                    err = New clsAMIMessage(String.Format(My.Resources.SelectRangeDoesNotWorkWithElementsOfType0, elementType.ID))
                                    Return False
                            End Select
                        Case "GetElementBounds",
                         "GetElementScreenBounds", "MouseClick", "MouseClickCentre"
                            'We need to prepend with one of "JAB", "HTML", "AA", etc
                            Dim prefix As String
                            Select Case elementType.AppType
                                Case AppTypes.AA : prefix = "AA"
                                Case AppTypes.HTML : prefix = "HTML"
                                Case AppTypes.Java : prefix = "JAB"
                                Case AppTypes.SAP : prefix = "SAP"
                                Case Else
                                    ' Anything else (predominantly Win32), we just
                                    ' leave alone. Note that regions now have their
                                    ' own actions and so don't need to be altered
                                    ' here.
                                    prefix = ""
                            End Select
                            sQuery = prefix & actionID
                        Case "GetRegionWindowText"
                            sQuery = "getwindowtext"
                        Case "GetWindowText"
                            Select Case elementType.ID
                                Case "SAPTextBox", "SAPButton"
                                    sQuery = "sapgetproperty propname=text"
                                Case Else
                                    sQuery = "getwindowtext"
                            End Select
                        Case "Verify"
                            Dim pre As String
                            Select Case elementType.AppType
                                Case AppTypes.AA : pre = "aa"
                                Case AppTypes.UIAutomation : pre = "uia"
                                Case AppTypes.HTML : pre = "html"
                                Case AppTypes.Web : pre = "web"
                                Case AppTypes.Java : pre = "jab"
                                Case AppTypes.Mainframe : pre = "terminal"
                                Case Else
                                    Select Case elementType.ID
                                        Case "WindowRect", "Win32ListRegion",
                                         "Win32GridRegion"
                                            pre = "region"
                                        Case Else : pre = ""
                                    End Select
                            End Select
                            sQuery = pre + "verify"
                        Case "DetachApplication"
                            If IsBrowserApplication(mTargetAppInfo.ID) Then
                                sQuery = "WebDetachApplication"
                            Else
                                sQuery = actionID
                            End If
                        Case Else
                            'For anything other than the special cases above, we just
                            'use the action ID as the query command.
                            sQuery = actionID

                    End Select

                End If

            End If

            'It might seem like it would make more sense to do this at the start of
            'this method, before all the other processing, but it doesn't because
            'some of the above actions (launch, isconnected, etc) work when not
            'connected, and don't get to here!
            If mTargetApp Is Nothing Then
                err = New clsAMIMessage(clsAMIMessage.CommonMessages.NotConnected)
                Return False
            End If

            If sQuery Is Nothing Then
                err = New clsAMIMessage(String.Format(My.Resources.InvalidAction0, actionID))
            End If

            AppendArgumentsToQuery(sQuery, args)
            AppendIdentifiersToQuery(sQuery, idents)

            'Execute and return result...
            Dim sResponse As String = Nothing, sResType As String = Nothing
            sResponse = mTargetApp.ProcessQuery(sQuery, externalAppmanTimeout)
            clsQuery.ParseResponse(sResponse, sResType, result)
            Select Case sResType
                Case "ERROR", "WARNING"
                    sErr = String.Format("{0}Query was: {1}", result, sQuery)
                    err = clsAMIMessage.Parse(sResponse)
                    Return False
                Case "OK", "RESULT"
                    Return True
                Case Else
                    sErr = String.Format(My.Resources.UnknownResponseType0, sResType)
                    err = clsAMIMessage.Parse(sErr)
                    Return False
            End Select
        End Function

        Private Shared Function FormulateOutputString(outputs As Dictionary(Of String, String)) As String
            Dim sb As New StringBuilder()

            For Each output In outputs
                sb.Append($"{output.Key}:{output.Value},")
            Next

            Return sb.ToString().TrimEnd(","c)
        End Function


        ''' <summary>
        ''' Performs a diagnostic action.
        ''' </summary>
        ''' <param name="actionid">The action to perform.</param>
        ''' <param name="sResult">The result.</param>
        ''' <param name="Err">An error if the function is unsuccessful.</param>
        ''' <returns>True if successful.</returns>
        Public Function DoDiagnosticAction(
         ByVal actionid As String, ByRef sResult As String,
         ByRef Err As clsAMIMessage) As Boolean

            Dim sQuery As String = actionid

            If mTargetApp Is Nothing Then
                Err = New clsAMIMessage(clsAMIMessage.CommonMessages.NotConnected)
                Return False
            End If

            Dim sResponse As String, sResType As String = Nothing
            sResponse = mTargetApp.ProcessQuery(sQuery)

            clsQuery.ParseResponse(sResponse, sResType, sResult)
            Dim sErr As String
            Select Case sResType
                Case "ERROR", "WARNING"
                    sErr = String.Format("{0}Query was: {1}", sResult, sQuery)
                    Err = clsAMIMessage.Parse(sResponse)
                    Return False
                Case "OK", "RESULT"
                    Return True
                Case Else
                    sErr = String.Format(My.Resources.UnknownResponseType0, sResType)
                    Err = clsAMIMessage.Parse(sErr)
                    Return False
            End Select
        End Function


        ''' <summary>
        ''' Translate the 'idname' argument from a 'friendly name' to an internal
        ''' ID.
        ''' </summary>
        ''' <param name="args">The list of arguments.</param>
        Private Sub TranslateIDName(ByRef args As Dictionary(Of String, String))

            If Not args.ContainsKey("idname") Then Return
            Dim idName As String = args("idname")

            For Each id As clsIdentifierInfo In mIdentifiers.Values
                If id.FullyQualifiedName.Equals(
                  idName, StringComparison.CurrentCultureIgnoreCase) Then
                    args("idname") = id.ID
                    Return
                End If
            Next
            'We'll just leave it alone if we didn't find it - either it's invalid, in
            'which case the appropriate error will appear anyway, or it's already been
            'specified as an ID, which will just work!

        End Sub

        ''' <summary>
        ''' Appends the supplied arguments to the supplied query,
        ''' which is passed by reference.
        ''' </summary>
        ''' <param name="q">The query to which the arguments should be appended.
        ''' </param>
        ''' <param name="args">The arguments to be added.</param>
        Private Sub AppendArgumentsToQuery(ByRef q As String,
         ByVal args As IDictionary(Of String, String))
            If args Is Nothing Then Return
            'Add the arguments...
            For Each argid As String In args.Keys
                'We give out IDs for arguments that match the parameter name used in
                'the query, so we can just write it directly here. This optimisation
                'is hidden from the client though - Automate is just passing back an
                'ID it has been given, via clsArgumentInfo.
                q &= " " & argid & "=" & clsQuery.EncodeValue(args(argid))
            Next
        End Sub

        ''' <summary>
        ''' Appends the supplied identifiers to the supplied query,
        ''' which is passed by reference.
        ''' </summary>
        ''' <param name="q">The query to be appended to.</param>
        ''' <param name="idents">The identifiers to add.</param>
        Private Sub AppendIdentifiersToQuery(
         ByRef q As String, ByVal idents As ICollection(Of clsIdentifierInfo))
            Dim sb As New StringBuilder(q)
            For Each i As clsIdentifierInfo In idents
                sb.Append(" "c).Append(i.ID)
                sb.Append(GetComparisonTypeSymbol(i.ComparisonType))
                sb.Append(clsQuery.EncodeValue(i.Value))
            Next
            q = sb.ToString()
        End Sub


        ''' <summary>
        ''' Gets the build version of this AMI instance.
        ''' </summary>
        Public Shared ReadOnly Property Version() As System.Version
            Get
                Return Assembly.GetExecutingAssembly().GetName().Version
            End Get
        End Property


        ''' <summary>
        ''' The types of comparison available for data, when comparing
        ''' to an expected value (eg in the case of identifiers for elements, and 
        ''' for query return values in wait queries).
        ''' </summary>
        Public Enum ComparisonTypes
            ''' <summary>
            ''' Tests equality.
            ''' </summary>
            ''' <remarks>This is the default comparison type, in 
            ''' the absence of any other specific type being specified.
            ''' </remarks>
            Equal = 0
            LessThan
            LessThanOrEqual
            GreaterThan
            GreaterThanOrEqual
            ''' <summary>
            ''' Tests for inequality of values.
            ''' </summary>
            ''' <remarks>Valid for any data type.</remarks>
            NotEqual
            ''' <summary>
            ''' Enables the use of wildcard characters in a text comparison.
            ''' To intepret these characters literally, use the
            ''' <see cref="ComparisonTypes.Equal">Equal</see> comparison type.
            ''' </summary>
            ''' <remarks>Valid only for text comparisons.</remarks>
            Wildcard
            RegEx
        End Enum


        ''' <summary>
        ''' Gets the friendly name of the specified comparison type.
        ''' </summary>
        ''' <param name="Type">The type whose name is desired.</param>
        ''' <returns>Returns the friendly name of the specefied type,
        ''' suitable for UI display.</returns>
        Public Shared Function GetComparisonTypeFriendlyName(ByVal Type As ComparisonTypes) As String
            Select Case Type
                Case ComparisonTypes.Equal
                    Return My.Resources.Equal
                Case ComparisonTypes.NotEqual
                    Return My.Resources.NotEqual
                Case ComparisonTypes.LessThan
                    Return My.Resources.LessThan
                Case ComparisonTypes.LessThanOrEqual
                    Return My.Resources.LessThanOrEqual
                Case ComparisonTypes.GreaterThan
                    Return My.Resources.GreaterThan
                Case ComparisonTypes.GreaterThanOrEqual
                    Return My.Resources.GreaterThanOrEqual
                Case ComparisonTypes.Wildcard
                    Return My.Resources.Wildcard
                Case ComparisonTypes.RegEx
                    Return My.Resources.RegEx
                Case Else
                    Throw New ArgumentException(String.Format(My.Resources.UnrecognisedComparisonType0, Type.ToString))
            End Select
        End Function


        ''' <summary>
        ''' Gets the symbolic representation of the specified 
        ''' comparison type
        ''' </summary>
        ''' <param name="Type">The comparison type of interest.</param>
        ''' <returns>Returns a string representing the specified
        ''' comparison type, in symbolic form.</returns>
        Public Shared Function GetComparisonTypeSymbol(ByVal Type As ComparisonTypes) As String
            Select Case Type
                Case ComparisonTypes.Equal
                    Return "="
                Case ComparisonTypes.GreaterThan
                    Return ">"
                Case ComparisonTypes.GreaterThanOrEqual
                    Return ">="
                Case ComparisonTypes.LessThan
                    Return "<"
                Case ComparisonTypes.LessThanOrEqual
                    Return "<="
                Case ComparisonTypes.NotEqual
                    Return "<>"
                Case ComparisonTypes.Wildcard
                    Return "%="
                Case ComparisonTypes.RegEx
                    Return "$="
                Case Else
                    Throw New ArgumentException(String.Format(My.Resources.UnrecognisedComparisonType0, Type.ToString))
            End Select
        End Function


        ''' <summary>
        ''' Determines which comparison types are available
        ''' for a reply from application manager of the specified
        ''' data type.
        ''' </summary>
        ''' <param name="tp">The data type of interest. This must be a valid
        ''' Automate data type in internal representation form (eg "text" rather
        ''' than "Text").</param>
        ''' <returns>Returns a collection of the comparison types available. This may
        ''' be empty, but will never be null.</returns>
        Public Shared Function GetAllowedComparisonTypes(ByVal tp As String) As ICollection(Of ComparisonTypes)
            Select Case tp
                Case "number", "date", "time", "datetime", "timespan"
                    Return CollectionUtil.ToCollection(ComparisonTypes.Equal, ComparisonTypes.NotEqual, ComparisonTypes.LessThan, ComparisonTypes.LessThanOrEqual, ComparisonTypes.GreaterThan, ComparisonTypes.GreaterThanOrEqual)
                Case "flag", "image"
                    Return CollectionUtil.ToCollection(ComparisonTypes.Equal, ComparisonTypes.NotEqual)
                Case "text"
                    Return CollectionUtil.ToCollection(ComparisonTypes.Wildcard, ComparisonTypes.Equal, ComparisonTypes.NotEqual, ComparisonTypes.RegEx)
                Case Else
                    'None available
                    Return GetEmpty.ICollection(Of ComparisonTypes)()
            End Select
        End Function

        Private mDisposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not mDisposedValue AndAlso disposing Then
                mGlobalInfo = Nothing
                mTargetAppInfo = Nothing

                ' Don't dispose these shared member variables as it's the instance that's being disposed
                ' not the shared data. Really this should be in 2 separate classes.
                'mActionTypes = Nothing
                'mConditionTypes = Nothing
                'mElementTypes = Nothing
                'mIdentifiers = Nothing
                'mExtDataErrors = Nothing
                'sReqdAttrsListRegion = Nothing
                'sReqdAttrsGridRegion = Nothing
                'sReqdAttrsCharMatching = Nothing

                Try
                    mTargetApp?.Dispose()
                Catch
                    ' This is ok if this throws an error as we're disposing of it anyway.
                End Try
            End If
            mDisposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub


#End Region

    End Class

End Namespace
