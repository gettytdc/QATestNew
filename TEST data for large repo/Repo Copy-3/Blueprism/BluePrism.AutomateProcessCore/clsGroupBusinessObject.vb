Imports System.Xml
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.WebApis

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsGroupBusinessObjects
''' 
''' <summary>
''' A class representing all available Business Objects, along with
''' an encapsulated instance of each.
''' 
''' The Dispose() method must ALWAYS be used when the Business Objects
''' are no longer required.
''' </summary>
Public Class clsGroupBusinessObject : Inherits clsBusinessObject

    Private mbCleanedUp As Boolean

    ''' <summary>
    ''' Determines whether the group is internal.
    ''' </summary>
    Public Property InternalGroup As Boolean

    ''' <summary>
    ''' Provides access to the group details
    ''' </summary>
    Public ReadOnly Property Details As IGroupObjectDetails

    ''' <summary>
    ''' Provides access to the children of the group
    ''' </summary>
    Public Property Children As ICollection(Of clsBusinessObject)

    Private Sub New(name As String, internalGroup As Boolean, details As IGroupObjectDetails, Optional ByVal friendlyName As String = Nothing)
        mName = name
        mFriendlyName = CStr(IIf(friendlyName Is Nothing, name, friendlyName))
        mValid = True
        Children = New List(Of clsBusinessObject)
        Me.InternalGroup = internalGroup
        Me.Details = details
    End Sub

    ''' <summary>
    ''' Creates a new set of business objects representing the objects specified in
    ''' this environment, including those in the given external objects info.
    ''' This does not provide a parent process or session to the generated objects,
    ''' thus it should only be used when not intending to execute the objects.
    ''' </summary>
    ''' <param name="objectDetails">The object representing the external objects
    ''' to create within this instance.</param>
    Public Sub New(objectDetails As IGroupObjectDetails)
        Me.New(objectDetails, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Constructor for the clsBusinessObjects class. Creating an instance of this
    ''' object immediately creates an instance of each Business Object specified in
    ''' the parameters, and these Business Objects are accessible for the lifetime of
    ''' the clsBusinessObjects instance itself.</summary>
    ''' <param name="objectDetails">An IGroupObjectDetails instance
    ''' containing information about the available set of objects.</param>
    ''' <param name="process">The parent process for all the business objects
    ''' created. Can be Nothing if unknown or not relevant, but some functionality
    ''' may not be available in this case - the differences being specific to the
    ''' type of Business Object.</param>
    ''' <param name="session">The session we are running under. As with the
    ''' process, this can be Nothing in non-running circumstances - the lack of
    ''' an session will just make some object features unavailable.</param>
    ''' <param name="usedobjects">Optionally, a list of business object names that
    ''' we actually want to load. Any available objects not in this list will be
    ''' ignored. If this parameter is not specified, all objects are loaded.</param>
    Public Sub New(objectDetails As IGroupObjectDetails, process As clsProcess,
                   session As clsSession, Optional includeParent As Boolean = False,
                   Optional usedobjects As List(Of String) = Nothing)

        Details = objectDetails

        mbCleanedUp = False

        'Create an empty collection to store all our object references. We will then add
        'each object type in turn...
        Children = New List(Of clsBusinessObject)

        Dim obr As clsBusinessObject

        Dim legacyGroup As New clsGroupBusinessObject("Legacy COM Objects", False, Nothing, My.Resources.Resources.clsGroupBusinessObject_LegacyCOMObjects)
        Dim webServicesGroup As New clsGroupBusinessObject("SOAP Web Services", False, Nothing, My.Resources.Resources.clsGroupBusinessObject_SOAPWebServices)
        Dim webApiServicesGroup As New clsGroupBusinessObject("Web API Services", False, Nothing, My.Resources.Resources.clsGroupBusinessObject_WebAPIServices)
        Dim VBOGroup As New clsGroupBusinessObject("Visual Business Objects", False, Nothing, My.Resources.Resources.clsGroupBusinessObject_VisualBusinessObjects)
        Dim internalGroup As New clsGroupBusinessObject("Internal Business Objects", True, Nothing, My.Resources.Resources.clsGroupBusinessObject_InternalBusinessObjects)
        For Each details As IObjectDetails In objectDetails.Children
            'Add legacy COM Business Objects...
            Dim extObj = TryCast(details, clsCOMObjectDetails)
            If Not extObj Is Nothing Then
                obr = New clsCOMBusinessObject(extObj.ProgID, extObj.Config)
                If usedobjects Is Nothing OrElse usedobjects.Contains(obr.Name) Then
                    legacyGroup.Children.Add(obr)
                End If
            End If

            'Add web services...
            Dim objwebService = TryCast(details, clsWebServiceDetails)
            If objwebService IsNot Nothing Then
                Dim objInvestigate As New clsWSDLProcess
                obr = objInvestigate.Load(objwebService)
                If usedobjects Is Nothing OrElse usedobjects.Contains(obr.Name) Then
                    webServicesGroup.Children.Add(obr)
                End If
            End If

            'Add web api services
            Dim webApiService = TryCast(details, WebApi)
            If webApiService IsNot Nothing Then
                obr = New WebApiBusinessObject(webApiService)
                webApiServicesGroup.Children.Add(obr)
            End If

            DescendChildren(VBOGroup, details, objectDetails, process, includeParent, usedobjects)
        Next

        'Add internal business objects...
        obr = New clsCollectionBusinessObject(process, session)
        If usedobjects Is Nothing OrElse usedobjects.Contains(obr.Name) Then
            internalGroup.Children.Add(obr)
        Else
            obr.Dispose()
        End If
        'Add also add-on internal business objects, i.e. those defined outside of APC...
        If clsAPC.ObjectLoader IsNot Nothing Then
            For Each iobj As clsInternalBusinessObject In clsAPC.ObjectLoader.CreateAll(process, session)
                If iobj.CheckLicense() AndAlso
                    (usedobjects Is Nothing OrElse usedobjects.Contains(iobj.Name)) Then
                    internalGroup.Children.Add(iobj)
                Else
                    iobj.Dispose()
                End If
            Next
        End If

        If legacyGroup.Children.Count > 0 Then
            Children.Add(legacyGroup)
        End If
        If webApiServicesGroup.Children.Count > 0 Then
            Children.Add(webApiServicesGroup)
        End If
        If webServicesGroup.Children.Count > 0 Then
            Children.Add(webServicesGroup)
        End If
        If VBOGroup.Children.Count > 0 Then
            Children.Add(VBOGroup)
        End If

        'There will always be at least one internal bo, see above.
        Children.Add(internalGroup)
    End Sub

    Private Sub DescendChildren(group As clsGroupBusinessObject, details As IObjectDetails,
                                externalObjectsInfo As IGroupObjectDetails, process As clsProcess,
                                includeParent As Boolean, usedobjects As ICollection(Of String))
        Dim groupDetails = TryCast(details, IGroupObjectDetails)
        If groupDetails IsNot Nothing Then
            Dim childGroup As New clsGroupBusinessObject(groupDetails.FriendlyName, False, groupDetails)
            For Each childDetail In groupDetails.Children
                DescendChildren(childGroup, childDetail, externalObjectsInfo, process, includeParent, usedobjects)
            Next
            group.Children.Add(childGroup)
        End If
        Dim vboDetails = TryCast(details, clsVBODetails)
        If vboDetails IsNot Nothing Then
            'The optimisation here is that we can check if we want the object or not
            'before we actually create the instance, which involves retrieving the XML
            'and loading it...
            If usedobjects Is Nothing OrElse usedobjects.Contains(details.FriendlyName) Then
                'Don't add parent to this list for root objects
                If Not includeParent AndAlso process IsNot Nothing AndAlso process.ParentProcess Is Nothing _
                 AndAlso process.ParentObject = details.FriendlyName Then Return

                group.Children.Add(New clsVBO(vboDetails, externalObjectsInfo, process))
            End If
        End If
    End Sub

    'Cleanup code...
    Private mbDisposed As Boolean = False
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not mbDisposed Then
            mbDisposed = True
            Dim res = DoCleanup()
            If Not res.Success Then
                'Not much we can do with an error from DoCleanup at this point, but
                'it should have already been cleaned up, so the error should be
                '"Already cleaned up"!!!
                Debug.Assert(res.ExceptionDetail = My.Resources.Resources.clsGroupBusinessObject_AlreadyCleanedUp)
            End If
            'Dispose the business objects...
            For Each obr As clsBusinessObject In Children
                obr.Dispose()
            Next
        End If
    End Sub
    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub


    ''' <summary>
    ''' Clean up all business objects after running.
    ''' </summary>
    ''' <returns>True if successful, False otherwise</returns>
    Public Overrides Function DoCleanup() As StageResult
        If mbCleanedUp Then
            Return StageResult.InternalError(My.Resources.Resources.clsGroupBusinessObject_AlreadyCleanedUp)
        End If

        'Call the cleanup method on each valid object.
        For Each obr As clsBusinessObject In Children
            If obr.Valid And obr.Lifecycle Then
                If obr.mInited Then
                    Dim res As StageResult = obr.CleanUp()
                    If Not res.Success Then
                        'Mark the object as invalid if it fails to clean up.
                        obr.Valid = False
                        obr.ErrorMessage = res.GetText()
                    End If
                End If
            End If
        Next

        mbCleanedUp = True
        Return StageResult.OK
    End Function

    ''' <summary>
    ''' Find the information for a specific object.
    ''' </summary>
    ''' <param name="name">The object name</param>
    ''' <returns>A reference to a clsBusinessObject object, containing
    ''' information about the object. Returns Nothing if the
    ''' object doesn't exist.</returns>
    Public Function FindObjectReference(ByVal name As String) As clsBusinessObject
        For Each ref As clsBusinessObject In Children
            Dim found = DescendChildren(ref, name)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Checks this node and any descendents of this node for matches
    ''' </summary>
    ''' <param name="ref">The node to check</param>
    ''' <param name="name">The Object name to look for</param>
    ''' <returns>A reference to a clsBusinessObject object, containing
    ''' information about the object. Returns Nothing if the
    ''' object doesn't exist.</returns>
    Private Function DescendChildren(ref As clsBusinessObject, name As String) As clsBusinessObject
        ' If this child is an Object then check if we have a match, otherwise
        ' it must be a group so continue to descend it's children
        Dim group = TryCast(ref, clsGroupBusinessObject)
        If group Is Nothing Then
            If ref.Name = name Then Return ref
        Else
            For Each childObj As clsBusinessObject In group.Children
                Dim found = DescendChildren(childObj, name)
                If found IsNot Nothing Then Return found
            Next
        End If
        Return Nothing
    End Function

    Public Function GetNonVBORunModes() As Dictionary(Of String, BusinessObjectRunMode)
        Dim runModes As New Dictionary(Of String, BusinessObjectRunMode)

        'Return a list of objects and run mode for each non VBO object (so internal/COM
        'objects as Web Services don't have a run mode - Neither of these are groupable)
        For Each grp As clsGroupBusinessObject In Children
            If grp.Name = "Legacy COM Objects" OrElse grp.Name = "Internal Business Objects" Then
                For Each obj As clsBusinessObject In grp.Children
                    runModes.Add(obj.Name, obj.RunMode)
                Next
            End If
        Next

        Return runModes
    End Function

    Public Overrides Sub DisposeTasks()
    End Sub

    Protected Overrides Function DoDoAction(actionName As String, scopeStage As clsProcessStage, inputs As clsArgumentList, ByRef outputs As clsArgumentList) As StageResult
        Return StageResult.InternalError(My.Resources.Resources.clsGroupBusinessObject_CannotExecuteAGroup)
    End Function

    Public Overrides Function DoInit() As StageResult
        For Each child In Children
            Dim res = child.DoInit()
            If Not res.Success Then Return res
        Next
        Return StageResult.OK
    End Function

    Public Overrides Function GetConfig(ByRef sErr As String) As String
        Return String.Empty
    End Function

    Protected Overrides Sub GetHTMLPreamble(xr As XmlTextWriter)
    End Sub

    Public Overrides Function ShowConfigUI(ByRef sErr As String) As Boolean
    End Function
End Class
