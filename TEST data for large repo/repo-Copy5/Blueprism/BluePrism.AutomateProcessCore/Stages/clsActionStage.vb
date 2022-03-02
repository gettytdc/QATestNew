Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.Utilities.Functional

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsActionStage
    ''' 
    ''' <summary>
    ''' This represents a class of objects that represent stages that are able call 
    ''' Business objects. The business object that is called is referred to by a 
    ''' resource name, a resource action, and inputs/outputs to that action. The 
    ''' inputs and outputs are kept in the base class as they are also used for some
    ''' other stages.
    ''' The mention of "resource" is a piece of antiquated terminology and is 
    ''' nothing to do with with the current terminology refering to PCs.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsActionStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' A string representation of the Business Object referenced in this stage
        ''' </summary>
        <DataMember>
        Private msResourceObject As String

        ''' <summary>
        ''' A string representation of the Action used within the business object
        ''' specified in msResourceObject (relevant only for action stages and
        ''' collections).
        ''' </summary>
        ''' <remarks></remarks>
        <DataMember>
        Private msResourceAction As String

        ''' <summary>
        ''' The name of the business object that this stage references.
        ''' </summary>
        Public ReadOnly Property ObjectName() As String
            Get
                Return msResourceObject
            End Get
        End Property

        ''' <summary>
        ''' The name of the action on the business object that this stage references
        ''' </summary>
        Public ReadOnly Property ActionName() As String
            Get
                Return msResourceAction
            End Get
        End Property

        ''' <summary>
        ''' Sets the Business Object and Action referenced by this stage.
        ''' </summary>
        ''' <param name="sObject">The business object referenced by this stage.</param>
        ''' <param name="sAction">The action within the above business object
        ''' referenced by this stage.</param>
        Public Sub SetResource(ByVal sObject As String, ByVal sAction As String)
            msResourceObject = sObject
            msResourceAction = sAction
        End Sub

        ''' <summary>
        ''' Gets the Business Object and Action referenced by this stage.
        ''' </summary>
        ''' <param name="sObject"></param>
        ''' <param name="sAction"></param>
        Public Sub GetResource(ByRef sObject As String, ByRef sAction As String)
            sObject = msResourceObject
            sAction = msResourceAction
        End Sub

        ''' <summary>
        ''' Creates a new instance of the clsActionStage class and sets its parent
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an action stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsActionStage(mParent)
        End Function

        ''' <summary>
        ''' Creates a deep copy of the action stage.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsActionStage = CType(MyBase.Clone, clsActionStage)
            Dim sObject As String = Nothing
            Dim sAction As String = Nothing
            Me.GetResource(sObject, sAction)
            copy.SetResource(sObject, sAction)
            Return copy
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Action
            End Get
        End Property

        'These are all the actions on the Work Queues Internal Business Object
        'that make reference to a queue. They always do so via an input
        'parameter called "Queue Name"
        Private Shared mQueueNameActions As New List(Of String)({"Add To Queue",
                                                          "Get Completed Items",
                                                          "Get Exception Items",
                                                          "Get Locked Items",
                                                          "Get Next Item",
                                                          "Get Pending Items",
                                                          "Get Report Data",
                                                          "Is Item In Queue"})
        'And the same for the Credentials Internal Business Object (but the
        'parameter is always "Credentials Name"
        Private Shared mCredentialsNameActions As New List(Of String)({"Generate And Set",
                                                                "Get",
                                                                "Get Property",
                                                                "Set"})

        'And the same for the Calendar Internal Business Object (but the
        'parameter is always "Calendar Name"
        Private Shared mCalendarNameActions As New List(Of String)({"Get Working Days In Range",
                                                             "Count Working Days In Range",
                                                             "Is Working Day",
                                                             "Add Working Days",
                                                             "Is Weekend",
                                                             "Is Public Holiday",
                                                             "Is Other Holiday",
                                                             "Get Public Holidays In Range",
                                                             "Get Other Holidays In Range"})

        ''' <summary>
        ''' Returns items referred to by this stage, so externally defined things
        ''' (such as Objects (and actions), Web Services, Credentials, Work Queues)
        ''' and when required things defined within the process (e.g. data items,
        ''' page references).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            'We need mObjRefs populated so we can distinguish between different kinds of
            'objects as we look at references. There ought to be no side effects from
            'doing this. Note that we're deliberately using the 'Running' mode.
            Dim obj As clsBusinessObject = mParent.GetBusinessObjects(True).FindObjectReference(ObjectName)
            If obj Is Nothing AndAlso ObjectName <> String.Empty Then
                mParent.AddBusinessObject(mParent, ObjectName)
                obj = mParent.GetBusinessObjects(True).FindObjectReference(ObjectName)
            End If

            'So basically, unknown references get included here, and VBOs do. We
            'don't include Internal Business Objects at all. And Web Services are
            'separate.
            If obj Is Nothing OrElse TypeOf (obj) Is clsVBO Then
                If ObjectName <> String.Empty AndAlso (ObjectName <> mParent.Name OrElse
                 (inclInternal AndAlso ObjectName = mParent.Name)) Then
                    deps.Add(New clsProcessNameDependency(ObjectName))
                    If ActionName <> String.Empty Then
                        deps.Add(New clsProcessActionDependency(ObjectName, ActionName))
                    End If
                End If
            ElseIf TypeOf (obj) Is clsCOMBusinessObject Then
                deps.Add(New clsProcessNameDependency(obj.Name))
            ElseIf TypeOf (obj) Is clsWebService Then
                If ObjectName <> String.Empty Then
                    deps.Add(New clsProcessWebServiceDependency(ObjectName))
                End If
            ElseIf TypeOf (obj) Is WebApiBusinessObject Then
                If ObjectName <> String.Empty Then
                    deps.Add(New clsProcessWebApiDependency(ObjectName))
                End If

                Dim credentialDependency = GetWebApiActionCredentialDependency(DirectCast(obj, WebApiBusinessObject), inclInternal)
                If credentialDependency IsNot Nothing Then deps.Add(credentialDependency)

            ElseIf TypeOf (obj) Is clsInternalBusinessObject Then
                deps.Add(New clsProcessNameDependency(obj.Name))
                'We're interested in some internal business objects - specifically
                'the work queues and credentials ones at this stage...
                If obj.Name = "Blueprism.Automate.clsWorkQueuesActions" Then
                    If mQueueNameActions.Contains(ActionName) Then
                        Dim processQueueDependency = GetDependencyFromInputParameter("Queue Name", inclInternal,
                                                                                     Function(x) New clsProcessQueueDependency(x))
                        If processQueueDependency IsNot Nothing Then deps.Add(processQueueDependency)
                    End If
                ElseIf obj.Name = "Blueprism.Automate.clsCredentialsActions" Then
                    If mCredentialsNameActions.Contains(ActionName) Then
                        Dim processCredentialsDependency = GetDependencyFromInputParameter("Credentials Name", inclInternal,
                                                                                     Function(x) New clsProcessCredentialsDependency(x))
                        If processCredentialsDependency IsNot Nothing Then deps.Add(processCredentialsDependency)
                    End If
                ElseIf obj.Name = "clsCalendarsBusinessObject" Then
                    If mCalendarNameActions.Contains(ActionName) Then
                        Dim processCalendarDependency = GetDependencyFromInputParameter("Calendar Name", inclInternal,
                                                                                     Function(x) New clsProcessCalendarDependency(x))
                        If processCalendarDependency IsNot Nothing Then deps.Add(processCalendarDependency)
                    End If
                End If
            End If

            Return deps
        End Function

        Private Function GetDependencyFromInputParameter(parameterName As String, includeInternal As Boolean, dependencyFactory As Func(Of String, clsProcessDependency)) As clsProcessDependency
            Dim p = GetParameter(parameterName, ParamDirection.In)
            If p IsNot Nothing AndAlso Not String.IsNullOrEmpty(p.GetMap()) Then
                If p.GetMapType() = MapType.Expr AndAlso p.Expression.GetDataItems().Count > 0 Then
                    If includeInternal Then Return dependencyFactory("")
                Else
                    Dim val As clsProcessValue = Nothing
                    Dim sErr As String = Nothing
                    If clsExpression.EvaluateExpression(p.GetMap(), val, Me, False, Nothing, sErr) Then
                        If p.GetDataType() = DataType.text AndAlso val.EncodedValue <> String.Empty Then
                            Return dependencyFactory(val.EncodedValue)
                        End If
                    End If
                End If
            End If
            Return Nothing
        End Function

        Private Function GetWebApiActionCredentialDependency(webApiAction As WebApiBusinessObject, includeInternal As Boolean) As clsProcessCredentialsDependency

            Dim authenticationWithCredential = TryCast(webApiAction.WebApi.Configuration.CommonAuthentication, ICredentialAuthentication)
            If authenticationWithCredential Is Nothing Then Return Nothing

            Dim defaultCredentialName = authenticationWithCredential.Credential.CredentialName
            Dim dependencyFromDefaultCredential = If(String.IsNullOrEmpty(defaultCredentialName), Nothing,
                                                     New clsProcessCredentialsDependency(defaultCredentialName))

            If Not authenticationWithCredential.Credential.ExposeToProcess Then Return dependencyFromDefaultCredential

            Dim credentialParameterName = authenticationWithCredential.Credential.InputParameterName
            Dim credentialParameterValue = GetParameter(credentialParameterName, ParamDirection.In)?.GetMap()

            If String.IsNullOrEmpty(credentialParameterValue) Then
                Return dependencyFromDefaultCredential
            Else
                Return GetDependencyFromInputParameter(credentialParameterName, includeInternal, Function(x) New clsProcessCredentialsDependency(x)).
                            Map(Function(x) DirectCast(x, clsProcessCredentialsDependency))
            End If

        End Function

        Private Sub LogWebServiceDiags(logger As CompoundLoggingEngine, ByVal sMessage As String)
            Dim info = GetLogInfo()
            logger.LogDiagnostic(info, Me, sMessage)
        End Sub


        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing

            'Perform the action for this stage...
            Dim obr As clsBusinessObject
            Dim obj As String = Nothing, act As String = Nothing
            GetResource(obj, act)
            obr = mParent.GetBusinessObjectRef(obj)
            If obr Is Nothing Then
                If obj = "" Then
                    Return StageResult.InternalError(My.Resources.Resources.clsActionStage_NoResourceSpecifiedForAction)
                Else
                    Return StageResult.InternalError(My.Resources.Resources.clsActionStage_TheBusinessObject0IsNotAvailablePleaseVisitSystemManagerToCheckThatThisObjectIs, obj)
                End If
            End If

            Dim aa As clsBusinessObjectAction = obr.GetAction(act)
            If aa Is Nothing Then
                ' First check if the problem is with the business object...
                If Not obr.Valid Then Return StageResult.InternalError(
                 My.Resources.Resources.clsActionStage_TheObject0IsNotValidTheLastErrorMessageRecordedWas1,
                 obj, obr.ErrorMessage)

                ' Otherwise, it must be the action itself.
                Return StageResult.InternalError(
                 My.Resources.Resources.clsActionStage_TheBusinessObject0DoesNotSupportTheAction1, obj, act)

            End If

            Dim inputs As clsArgumentList = Nothing
            Dim outputs As clsArgumentList = Nothing
            Dim vbo As clsVBO = TryCast(obr, clsVBO)

            If mParent.mChildWaitProcess Is Nothing Then
                'Get the inputsXML for the action
                If Not mParent.ReadDataItemsFromParameters(GetParameters(), Me, inputs, sErr, False, False) Then
                    Return New StageResult(False, "Internal", sErr)
                End If

                'Log the action before doing it, not after, in
                'case the business object fails to return...
                ActionPrologue(logger, obj, aa.GetName, inputs)

                If vbo IsNot Nothing Then
                    'Check for recursion with shared objects. If the vbo has an action in progress
                    'or the root object (i.e. one being debugged in studio or called as a web service)
                    'is being called again
                    If (obj = Process.RootProcess.Name AndAlso Process.HasProcessLevelScope) OrElse
                     vbo.ActionStack.Count > 0 Then Return New StageResult(False, "Internal",
                        String.Format(My.Resources.Resources.clsActionStage_CannotRecursivelyCallActionsInASharedObject0, obj))
                    vbo.ActionStack.Push(act)
                End If
            End If

            Dim logWebServiceDiag = Sub(message As String) LogWebServiceDiags(logger, message)
            Dim wsHandlerAdded As Boolean = False
            If vbo IsNot Nothing Then
                vbo.Logger.Union(logger)
            ElseIf TypeOf obr Is IDiagnosticEmitter Then
                If (clsAPC.Diagnostics And clsAPC.Diags.LogWebServices) <> 0 Then
                    'If web service logging is on, temporarily add a handler for it,
                    'which will be removed straight after the call.
                    AddHandler CType(obr, IDiagnosticEmitter).Diags, logWebServiceDiag
                    wsHandlerAdded = True
                End If
            End If

            Dim res As StageResult = obr.DoAction(act, Me, inputs, outputs)
            If wsHandlerAdded Then
                RemoveHandler CType(obr, IDiagnosticEmitter).Diags, logWebServiceDiag
            End If

            If Not res.Success Then
                If vbo IsNot Nothing Then vbo.ActionStack.Pop()
                mParent.mChildWaitProcess = Nothing
                Return res
            End If

            If outputs.IsRunning Then
                'The action is not yet complete. We return True as if successful, but
                'we don't move on to the next stage.
                'Note that the object MUST currently be a Visual Business Object to
                'return this value.
                mParent.mChildWaitProcess = vbo.Process
                mParent.mChildWaitProcessOwned = False
            Else
                If vbo IsNot Nothing Then vbo.ActionStack.Pop()

                'Log that the action is complete.
                ActionEpilogue(logger, obj, aa.GetName, outputs)


                'Cancel child waiting...
                mParent.mChildWaitProcess = Nothing

                'Store outputs in the appropriate places...
                If Not outputs Is Nothing Then
                    If Not mParent.StoreParametersInDataItems(Me, outputs, sErr) Then
                        Return New StageResult(False, "Internal", sErr)
                    End If
                End If

                'Move on to next stage...
                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If

            End If

            Return New StageResult(True)

        End Function

        Private Sub ActionPrologue(logger As CompoundLoggingEngine, ByVal sResourceName As String, ByVal sActionName As String, ByVal inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ActionPrologue(info, Me, sResourceName, sActionName, inputs)
        End Sub

        Private Sub ActionEpilogue(logger As CompoundLoggingEngine, ByVal sResourceName As String, ByVal sActionName As String, ByVal outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ActionEpiLogue(info, Me, sResourceName, sActionName, outputs)
        End Sub

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            For Each e3 As Xml.XmlElement In e2.ChildNodes
                Select Case e3.Name
                    Case "resource"
                        Me.SetResource(e3.GetAttribute("object"), e3.GetAttribute("action"))
                End Select
            Next

            GetParamFriendlyName()

            'Correct malformed input params - see bug 2433
            For Each p As clsProcessParameter In Me.GetInputs
                If p.GetMapType = MapType.Stage Then
                    p.SetMapType(MapType.Expr)
                    Dim Mapping As String = p.GetMap
                    If Not Mapping.StartsWith("[") Then
                        Mapping = "[" & Mapping
                    End If
                    If Not Mapping.EndsWith("]") Then
                        Mapping &= "]"
                    End If
                    p.SetMap(Mapping)
                End If
            Next
        End Sub

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)
            Dim e2 As Xml.XmlElement
            Dim sResObject As String = Nothing
            Dim sResAction As String = Nothing
            Me.GetResource(sResObject, sResAction)
            e2 = ParentDocument.CreateElement("resource")
            e2.SetAttribute("object", sResObject)
            e2.SetAttribute("action", sResAction)
            StageElement.AppendChild(e2)
        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList

            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            'check for action stages with no business object set
            'and objects that haven't been installed
            Dim sObject As String = Nothing, sAction As String = Nothing
            GetResource(sObject, sAction)
            If sObject = "" OrElse sAction = "" Then
                'no object or action set
                errors.Add(New ValidateProcessResult(Me, 97))
            Else
                If Not SkipObjects Then
                    'object and action set, but do they exist?
                    Dim obr As clsBusinessObject
                    obr = mParent.GetBusinessObjects.FindObjectReference(sObject)
                    If obr Is Nothing Then
                        errors.Add(New ValidateProcessResult(Me, "helpBusinessObjects.htm", 98, sObject))
                    Else
                        'Is this business object retired?
                        If TypeOf obr Is clsVBO Then
                            Dim sErr As String = Nothing
                            Dim A As ProcessAttributes
                            Dim BusinessObjectID As Guid = CType(obr, clsVBO).ProcessID
                            If clsAPC.ProcessLoader.GetProcessAtrributes(BusinessObjectID, A, sErr) Then
                                Dim ProcessIsRetired As Boolean = (A And ProcessAttributes.Retired) > 0
                                If ProcessIsRetired Then
                                    'referenced subprocess has been retired
                                    errors.Add(New ValidateProcessResult(Me, "helpSystemManagerBusinessObjects.htm", 99))
                                End If
                            End If
                        Else
                            If TypeOf obr Is clsInternalBusinessObject Then
                                Dim objParm As clsProcessParameter
                                Dim objParm2 As clsProcessParameter
                                Dim obrAction = obr.GetAction(sAction)
                                Dim bFound As Boolean
                                bFound = False
                                If obrAction IsNot Nothing Then
                                    Dim obrNarrative = obrAction.GetNarrative()
                                    Narrative = CStr(IIf(obrNarrative IsNot Nothing, obrNarrative, Narrative))
                                    For Each objParm In obrAction.GetParameters()
                                        Dim mt As MapType
                                        If objParm.Direction = ParamDirection.In Then
                                            mt = MapType.Expr
                                        Else
                                            mt = MapType.Stage
                                        End If
                                        For ii = 0 To GetNumParameters() - 1
                                            objParm2 = GetParameter(ii)
                                            If objParm2.Name = objParm.Name And objParm2.Direction = objParm.Direction Then
                                                'Update the data type if it changed...
                                                If objParm2.GetDataType() <> objParm.GetDataType() Then
                                                    objParm2.SetDataType(objParm.GetDataType())
                                                End If
                                                If objParm2.Narrative <> objParm.Narrative Then
                                                    objParm2.Narrative = objParm.Narrative
                                                End If
                                                If objParm2.FriendlyName <> objParm.FriendlyName Then
                                                    objParm2.FriendlyName = objParm.FriendlyName
                                                End If
                                                bFound = True
                                            End If
                                        Next
                                        If Not bFound Then
                                            Dim CollectionInfo As clsCollectionInfo = Nothing
                                            If objParm.GetDataType = DataType.collection Then
                                                CollectionInfo = objParm.CollectionInfo
                                                AddParameter(objParm.Direction, objParm.GetDataType(), objParm.Name, objParm.Narrative, mt, "", CollectionInfo, objParm.FriendlyName)
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If

                        'Does the requested action exist within the target object?
                        If obr.GetAction(sAction) Is Nothing Then
                            errors.Add(New ValidateProcessResult(Me, "helpBusinessObjects.htm", 100, sAction, sObject))
                        End If
                    End If
                End If
            End If

            'Check for missing narrative...
            If GetNarrative().Length = 0 Then
                errors.Add(New ValidateProcessResult(Me, 129))
            End If

            Return errors
        End Function

        Public Sub GetParamFriendlyName()
            'check for action stages with no business object set
            'and objects that haven't been installed
            Dim sObject As String = Nothing, sAction As String = Nothing
            GetResource(sObject, sAction)
            If Not String.IsNullOrEmpty(sObject) AndAlso Not String.IsNullOrEmpty(sAction) Then
                'object and action set, but do they exist?
                Dim obr As clsBusinessObject
                obr = mParent.GetAllBusinessObjects().FindObjectReference(sObject)
                If obr IsNot Nothing AndAlso TypeOf obr Is clsInternalBusinessObject Then
                    Dim objParm As clsProcessParameter
                    Dim objParm2 As clsProcessParameter
                    Dim obrAction = obr.GetAction(sAction)
                    Dim bFound As Boolean
                    bFound = False
                    If obrAction IsNot Nothing Then
                        Dim obrNarrative = obrAction.GetNarrative()
                        Narrative = CStr(IIf(obrNarrative IsNot Nothing, obrNarrative, Narrative))
                        For Each objParm In obrAction.GetParameters()
                            Dim mt As MapType
                            If objParm.Direction = ParamDirection.In Then
                                mt = MapType.Expr
                            Else
                                mt = MapType.Stage
                            End If
                            For ii = 0 To GetNumParameters() - 1
                                objParm2 = GetParameter(ii)
                                If objParm2.Name = objParm.Name And objParm2.Direction = objParm.Direction Then
                                    'Update the data type if it changed...
                                    If objParm2.GetDataType() <> objParm.GetDataType() Then
                                        objParm2.SetDataType(objParm.GetDataType())
                                    End If
                                    If objParm2.Narrative <> objParm.Narrative Then
                                        objParm2.Narrative = objParm.Narrative
                                    End If
                                    If objParm2.FriendlyName <> objParm.FriendlyName Then
                                        objParm2.FriendlyName = objParm.FriendlyName
                                    End If
                                    bFound = True
                                End If
                            Next
                            If Not bFound Then
                                Dim CollectionInfo As clsCollectionInfo = Nothing
                                If objParm.GetDataType = DataType.collection Then
                                    CollectionInfo = objParm.CollectionInfo
                                    AddParameter(objParm.Direction, objParm.GetDataType(), objParm.Name, objParm.Narrative, mt, "", CollectionInfo, objParm.FriendlyName)
                                End If
                            End If
                        Next
                    End If
                End If
            End If
        End Sub

        Public Function GetActionFriendlyName(ByVal sObjectName As String, ByVal sActionName As String) As String

            Dim obr As clsBusinessObject

            obr = mParent.GetBusinessObjects.FindObjectReference(sObjectName)
            If obr IsNot Nothing Then
                If TypeOf obr Is clsInternalBusinessObject Then
                    Dim obrAction = obr.GetAction(sActionName)
                    If obrAction IsNot Nothing Then
                        Return obrAction.FriendlyName
                    End If
                End If
            End If

            Return sActionName

        End Function
    End Class

End Namespace
