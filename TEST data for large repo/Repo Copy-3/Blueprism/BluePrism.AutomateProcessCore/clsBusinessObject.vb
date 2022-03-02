Imports System.Xml
Imports System.Text
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsBusinessObject
''' 
''' <summary>
''' A class representing a Business Object.
''' </summary>
Public MustInherit Class clsBusinessObject
    Implements IDisposable


    ''' <summary>
    ''' The default constructor just makes sure we have a collection setup
    ''' ready to hold the actions.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new business object with the given name and friendly name.
    ''' </summary>
    ''' <param name="name">The (progid) name of this object.</param>
    ''' <param name="friendlyName">The friendly (user-viewable) name of this object.
    ''' </param>
    Protected Sub New(ByVal name As String, ByVal friendlyName As String)
        Me.New(name, friendlyName, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new business object with the given names and narrative.
    ''' </summary>
    ''' <param name="name">The (progid) name of this object.</param>
    ''' <param name="friendlyName">The friendly (user-viewable) name of this object.
    ''' </param>
    ''' <param name="narrative">The narrative describing the business object.</param>
    Protected Sub New(ByVal name As String, ByVal friendlyName As String, ByVal narrative As String)
        mActions = New List(Of clsBusinessObjectAction)
        mInited = False
        mCleanedUp = False
        mName = name
        mFriendlyName = friendlyName
        mNarrative = narrative
    End Sub


    ''' <summary>
    ''' The available actions in this object.
    ''' </summary>
    Protected mActions As List(Of clsBusinessObjectAction)

    ''' <summary>
    ''' The name (ProgID) of the Business Object, e.g. "CommonAutomation.clsWord"
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal Value As String)
            mName = Value
        End Set
    End Property
    Protected mName As String


    ''' <summary>
    ''' If the Valid property is False, this contains the reason the object
    ''' reference is not valid.
    ''' </summary>
    Public Property ErrorMessage() As String
        Get
            Return mErrorMessage
        End Get
        Set(ByVal Value As String)
            mErrorMessage = Value
        End Set
    End Property
    Protected mErrorMessage As String


    ''' <summary>
    ''' The friendly name of the Business Object, e.g. "Microsoft Office - 
    ''' Word Actions"
    ''' </summary>
    Public Property FriendlyName() As String
        Get
            Return mFriendlyName
        End Get
        Set(ByVal Value As String)
            mFriendlyName = Value
        End Set
    End Property
    Protected mFriendlyName As String


    ''' <summary>
    ''' True if this object is valid. If not, msErrorMessage will contain the reason
    ''' why.
    ''' </summary>
    Public Property Valid() As Boolean
        Get
            Return mValid
        End Get
        Set(ByVal Value As Boolean)
            mValid = Value
        End Set
    End Property
    Protected mValid As Boolean

    ''' <summary>
    ''' The run mode for this object, which must be one of:
    ''' 
    '''     Exclusive
    '''     Foreground
    '''     Background
    ''' 
    ''' See "Resource PC Flow Diagram V1.1.vsd" for more information,
    ''' or the RunModes enumeration.
    ''' </summary>
    Public Overridable Property RunMode() As BusinessObjectRunMode
        Get
            Return mRunMode
        End Get
        Set(ByVal Value As BusinessObjectRunMode)
            mRunMode = Value
        End Set
    End Property
    Protected mRunMode As BusinessObjectRunMode

    ''' <summary>
    ''' True if this Business Object is configurable.
    ''' </summary>
    Public Property Configurable() As Boolean
        Get
            Return mConfigurable
        End Get
        Set(ByVal Value As Boolean)
            mConfigurable = Value
        End Set
    End Property
    Protected mConfigurable As Boolean


    ''' <summary>
    ''' True if this Business Object supports lifecycle management.
    ''' </summary>
    Public Property Lifecycle() As Boolean
        Get
            Return mLifecycle
        End Get
        Set(ByVal Value As Boolean)
            mLifecycle = Value
        End Set
    End Property
    Protected mLifecycle As Boolean


    ''' <summary>
    ''' True when this business object has been initialised for running.
    ''' </summary>
    Friend mInited As Boolean

    ''' <summary>
    ''' True when this business object has been cleaned up.
    ''' </summary>
    Friend mCleanedUp As Boolean

    ''' <summary>
    ''' Get the actions available for this object.
    ''' </summary>
    ''' <returns>A List of clsBusinessObjectAction objects</returns>
    Public Overridable Function GetActions() As IList(Of clsBusinessObjectAction)
        Return mActions.AsReadOnly()
    End Function

    ''' <summary>
    ''' Get the available actions in alphabetically sorted form. Used when
    ''' generating documentation.
    ''' </summary>
    ''' <returns>A List of clsBusinessObjectAction objects</returns>
    Private Function GetSortedActions() As IList(Of clsBusinessObjectAction)
        Dim acts As New List(Of clsBusinessObjectAction)(GetActions())
        acts.Sort()
        Return acts.AsReadOnly()
    End Function

    ''' <summary>
    ''' Get information about a particular action
    ''' </summary>
    ''' <param name="name">The name of the action</param>
    ''' <returns>A clsBusinessObjectAction object describing that action,
    ''' or Nothing if it doesn't exist</returns>
    Public Overridable Function GetAction(ByVal name As String) As clsBusinessObjectAction
        For Each act As clsBusinessObjectAction In mActions
            If act.GetName() = name Then
                Return act
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Add an action
    ''' </summary>
    ''' <param name="act">The action object to add</param>
    Public Sub AddAction(ByVal act As clsBusinessObjectAction)
        mActions.Add(act)
    End Sub


    ''' <summary>
    ''' Initialise the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public MustOverride Function DoInit() As StageResult

    ''' <summary>
    ''' Clean up the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public MustOverride Function DoCleanUp() As StageResult

    ''' <summary>
    ''' Initialise the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Function Init() As StageResult
        Debug.Assert(Not mInited)
        If mLifecycle Then
            Dim res As StageResult = DoInit()
            If Not res.Success Then Return res
        End If
        mInited = True
        Return New StageResult(True)
    End Function

    ''' <summary>
    ''' Clean up the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Function CleanUp() As StageResult
        Debug.Assert(mInited And Not mCleanedUp)
        mCleanedUp = True
        If mLifecycle Then
            Return DoCleanUp()
        Else
            Return New StageResult(True)
        End If
    End Function


    ''' <summary>
    ''' Ask the Business Object to perform an action.
    ''' </summary>
    ''' <param name="actionName">The name of the action to perform</param>
    ''' <param name="scopeStage">The stage used to resolve scope within the business
    ''' object action. Eg for an internal business object which modifies a collection,
    ''' a scope stage is needed. Must not be null.</param>
    ''' <param name="inputs">The inputs</param>
    ''' <param name="outputs">On return, contains the outputs</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Protected MustOverride Function DoDoAction(ByVal actionName As String, ByVal scopeStage As clsProcessStage, ByVal inputs As clsArgumentList, ByRef outputs As clsArgumentList) As StageResult

    ''' <summary>
    ''' Ask the Business Object to perform an action.
    ''' </summary>
    ''' <param name="actionName">The name of the action to perform</param>
    ''' <param name="scopeStage">The stage used to resolve scope within the business
    ''' object action. Eg for an internal business object which modifies a collection,
    ''' a scope stage is needed. Must not be null.</param>
    ''' <param name="inputs">The input arguments</param>
    ''' <param name="outputs">On return, contains the output arguments</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Function DoAction(ByVal actionName As String, ByVal scopeStage As clsProcessStage, ByVal inputs As clsArgumentList, ByRef outputs As clsArgumentList) As StageResult

        'Make sure the business object hasn't already been cleaned up.
        If mCleanedUp Then
            Return New StageResult(False, "Internal", My.Resources.Resources.clsBusinessObject_BusinessObjectAlreadyCleanedUp)
        End If

        'Lazily initialise the business object...
        If Not mInited Then
            Dim res As StageResult = Init()
            If Not res.Success Then Return res
        End If

        Return DoDoAction(actionName, scopeStage, inputs, outputs)
    End Function


    ''' <summary>
    ''' Show Config UI on the Business Object
    ''' </summary>
    ''' <param name="sErr">On failure, an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public MustOverride Function ShowConfigUI(ByRef sErr As String) As Boolean

    ''' <summary>
    ''' Get configuration on the Business Object - a wrapper for the
    ''' Business Object function itself
    ''' </summary>
    ''' <param name="sErr">On failure, an error description, otherwise an
    ''' empty string</param>
    ''' <returns>The ConfigXML of the Businessobject</returns>
    Public MustOverride Function GetConfig(ByRef sErr As String) As String

    ''' <summary>
    ''' Gets the Narrative for the business object.
    ''' </summary>
    Public Property Narrative() As String
        Get
            Return mNarrative
        End Get
        Set(ByVal value As String)
            mNarrative = value
        End Set
    End Property
    Private mNarrative As String


    ''' <summary>
    ''' Generates documentation in wiki format using the object model.
    ''' </summary>
    ''' <returns>Wiki documentation as a String</returns>
    Public Function GetWikiDocumentation() As String
        Dim sb As New StringBuilder()

        'Get the actions first, because due to the lazy-loading of VBOs, nothing
        'will be populated until we've done something like that!
        Dim actions As IList(Of clsBusinessObjectAction) = GetSortedActions()

        sb.AppendLine(String.Format(My.Resources.Resources.clsBusinessObject_BusinessObject0, mFriendlyName))
        sb.AppendLine(mNarrative & vbCrLf)
        sb.AppendLine(String.Format(My.Resources.Resources.clsBusinessObject_TheRunmodeOfThisBusinessObjectIs0, Me.RunMode.ToString.ToLower))

        For Each objAction As clsBusinessObjectAction In actions
            sb.AppendLine("==" & objAction.FriendlyName() & "==")
            sb.AppendLine(objAction.GetNarrative())

            For Each objParam As clsProcessParameter In objAction.GetParameters
                sb.AppendLine("*" & CStr(IIf(objParam.Direction = ParamDirection.In, My.Resources.Resources.clsBusinessObject_Input, My.Resources.Resources.clsBusinessObject_Output)) & String.Format(My.Resources.Resources.clsBusinessObject_Param0DataType1Narrative2, objParam.FriendlyName, clsProcessDataTypes.GetFriendlyName(objParam.GetDataType()), objParam.Narrative))
                'For a collection parameter, include field definitions...
                If objParam.GetDataType() = DataType.collection Then
                    For Each objField As clsCollectionFieldInfo In objParam.CollectionInfo
                        sb.AppendLine(String.Format(My.Resources.Resources.clsBusinessObject_Field0DataType1, objField.Name, clsProcessDataTypes.GetFriendlyName(objField.DataType)))
                    Next
                End If
            Next

            If objAction.GetPreConditions.Count > 0 Then
                sb.AppendLine(My.Resources.Resources.clsBusinessObject_GetWikiDocumentation_Preconditions)
                For Each sCond As String In objAction.GetPreConditions()
                    sb.AppendLine("*" & sCond)
                Next
            End If

            If objAction.GetEndpoint <> "" Then
                sb.AppendLine(My.Resources.Resources.clsBusinessObject_GetWikiDocumentation_Endpoint)
                sb.AppendLine(objAction.GetEndpoint())
            End If

        Next

        Return sb.ToString()

    End Function

    ''' <summary>
    ''' Generates documentation in HTML format using the object model.
    ''' </summary>
    ''' <returns>An HTML document as a String</returns>
    Public Function GetDocumentationHTML() As String
        Using st As New IO.StringWriter
            Using xr As New Xml.XmlTextWriter(st)
                xr.Formatting = Formatting.Indented
                xr.WriteStartElement("html")
                xr.WriteAttributeString("xmlns", "http://www.w3.org/TR/xhtml1/strict")
                xr.WriteStartElement("head")
                xr.WriteStartElement("style")
                xr.WriteAttributeString("type", "text/css")
                xr.WriteString(vbCrLf & "body" & vbCrLf &
                "{" & vbCrLf &
                "font-family: Arial;" & vbCrLf &
                "margin-top: 2.54 cm;" & vbCrLf &
                "margin-left: 3.17 cm;" & vbCrLf &
                "margin-right: 3.17 cm;" & vbCrLf &
                "} " & vbCrLf &
                "h4" & vbCrLf &
                "{" & vbCrLf &
                "margin-bottom: 5px;" & vbCrLf &
                "} " & vbCrLf &
                "table " & vbCrLf &
                "{" & vbCrLf &
                "margin-top: 1em;" & vbCrLf &
                "width: 100%;" & vbCrLf &
                "border:1px black solid;" & vbCrLf &
                "border-collapse: collapse;" & vbCrLf &
                "} " & vbCrLf &
                "th" & vbCrLf &
                "{" & vbCrLf &
                "padding: 5px;" & vbCrLf &
                "background-color: #003366;" & vbCrLf &
                "color: white;" & vbCrLf &
                "border-bottom:2px black solid;" & vbCrLf &
                "border-right:1px black solid;" & vbCrLf &
                "white-space: nowrap;" & vbCrLf &
                "} " & vbCrLf &
                "td" & vbCrLf &
                "{" & vbCrLf &
                "padding: 5px;" & vbCrLf &
                "border-top:1px black solid;" & vbCrLf &
                "border-right:1px black solid;" & vbCrLf &
                "}")
                xr.WriteEndElement()         'style
                xr.WriteEndElement()         'head

                xr.WriteStartElement("body")

                GetHTMLPreamble(xr)

                Dim i As Integer = 0
                xr.WriteElementString("h2", String.Format(My.Resources.Resources.clsBusinessObject_GetDocumentationHTML_VBOFriendlyName10, i, Me.mFriendlyName))
                xr.WriteElementString("div", Me.mNarrative)
                xr.WriteElementString("p", String.Format(My.Resources.Resources.clsBusinessObject_TheRunmodeOfThisBusinessObjectIs0, Me.RunMode.ToString.ToLower))

                For Each objAction As clsBusinessObjectAction In GetSortedActions()
                    i += 1
                    xr.WriteElementString("h3", String.Format(My.Resources.Resources.clsBusinessObject_GetDocumentationHTML_ActionName10, i, objAction.FriendlyName))
                    xr.WriteElementString("div", objAction.GetNarrative)

                    If objAction.GetPreConditions.Count > 0 Then
                        xr.WriteElementString("h4", My.Resources.Resources.clsBusinessObject_GetDocumentationHTML_Preconditions)
                        For Each sCond As String In objAction.GetPreConditions
                            xr.WriteElementString("div", sCond)
                        Next
                    End If

                    If objAction.GetEndpoint <> "" Then
                        xr.WriteElementString("h4", My.Resources.Resources.clsBusinessObject_GetDocumentationHTML_Endpoint)
                        xr.WriteElementString("div", objAction.GetEndpoint)
                    End If

                    If objAction.GetParameters.Count > 0 Then
                        xr.WriteStartElement("table")
                        xr.WriteStartElement("tr")
                        xr.WriteElementString("th", My.Resources.Resources.clsBusinessObject_Parameter)
                        xr.WriteElementString("th", My.Resources.Resources.clsBusinessObject_Direction)
                        xr.WriteElementString("th", My.Resources.Resources.clsBusinessObject_DataType)
                        xr.WriteElementString("th", My.Resources.Resources.clsBusinessObject_Description)
                        xr.WriteEndElement()                  'tr

                        For Each objParam As clsProcessParameter In objAction.GetParameters
                            xr.WriteStartElement("tr")
                            xr.WriteElementString("td", objParam.FriendlyName)
                            xr.WriteElementString("td", clsProcessParameter.GetLocalizedDirectionFriendlyName(objParam.Direction))
                            xr.WriteElementString("td", clsProcessDataTypes.GetFriendlyName(objParam.GetDataType))
                            xr.WriteElementString("td", objParam.Narrative)
                            xr.WriteEndElement()      'tr
                            'If its a collection list each field
                            If objParam.HasDefinedCollection() Then
                                For Each objField As clsCollectionFieldInfo In objParam.CollectionInfo
                                    xr.WriteStartElement("tr")
                                    xr.WriteStartElement("td")
                                    xr.WriteElementString("i", String.Format(My.Resources.Resources.clsBusinessObject_0CollectionField, objField.Name))
                                    xr.WriteEndElement()
                                    xr.WriteStartElement("td")
                                    xr.WriteElementString("i", clsProcessParameter.GetLocalizedDirectionFriendlyName(objParam.Direction))
                                    xr.WriteEndElement()
                                    xr.WriteStartElement("td")
                                    xr.WriteElementString("i", clsProcessDataTypes.GetFriendlyName(objField.DataType))
                                    xr.WriteEndElement()
                                    xr.WriteStartElement("td")
                                    xr.WriteElementString("i", objField.Description)
                                    xr.WriteEndElement()
                                    xr.WriteEndElement()
                                Next
                            End If
                        Next
                        xr.WriteEndElement()                'table
                    End If
                Next

                xr.WriteEndElement()          'body
                xr.WriteEndElement()          'html

                Return st.ToString()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Subclasses of clsBusinessObject must provide an HTML preamble that appears at
    ''' the top of the business object documentation.
    ''' </summary>
    ''' <param name="xr">An xmltextwriter to write the preamble into</param>
    Protected MustOverride Sub GetHTMLPreamble(ByVal xr As XmlTextWriter)


    ''' <summary>
    ''' Handles anything that must be done to dispose the object.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustOverride Sub DisposeTasks()

    'Cleanup code...
    Private mbDisposed As Boolean = False
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not mbDisposed Then
            mbDisposed = True
            Try
                If disposing Then
                    DisposeTasks()
                End If
            Catch e As Exception
            End Try
        End If
    End Sub
    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub


End Class
