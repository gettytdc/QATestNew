
Imports BluePrism.AMI
Imports System.Xml
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ApplicationManager.AMI

''' Project: AutomateProcessCore
''' Class: clsApplicationDefinition
''' <summary>
''' This class encapsulates the notion of an application definition, collecting
''' together information about the application type, the elements in the
''' application, associated info such as the location of the executable,
''' the session identifier for mainframe apps, etc.
''' </summary>
Public Class clsApplicationDefinition

    ''' <summary>
    ''' Information about the target application, such as  the path of the target
    ''' executable.
    ''' </summary>
    Public Property ApplicationInfo() As clsApplicationTypeInfo
        Get
            Return mApplicationInfo
        End Get
        Set(ByVal value As clsApplicationTypeInfo)
            mApplicationInfo = value
        End Set
    End Property
    Private mApplicationInfo As clsApplicationTypeInfo


    ''' <summary>
    ''' The root application element in this application definition.
    ''' </summary>
    Public Property RootApplicationElement() As clsApplicationElement
        Get
            If mRootApplicationElement Is Nothing Then
                mRootApplicationElement = New clsApplicationElement(My.Resources.Resources.ApplicationRoot)
                mRootApplicationElement.Type = clsAMI.GetElementTypeInfo("Application")
            End If
            Return mRootApplicationElement
        End Get
        Set(ByVal value As clsApplicationElement)
            mRootApplicationElement = value
        End Set
    End Property
    Private mRootApplicationElement As clsApplicationElement

    ''' <summary>
    ''' Finds the member with the supplied name, by traversing the tree from the root
    ''' element.
    ''' </summary>
    ''' <param name="name">The name of member to find.</param>
    ''' <returns>Returns the first member found with a matching name, or Nothing if no
    ''' such match is found.</returns>
    Public Function FindMemberByName(ByVal name As String) As clsApplicationMember
        Return RootApplicationElement.FindMemberByName(name)
    End Function


    ''' <summary>
    ''' Finds the member with the supplied ID, by traversing the tree from the root
    ''' element.
    ''' </summary>
    ''' <param name="ID">The ID of the member to find.</param>
    ''' <returns>Returns the first member found with a matching ID, or Nothing if no
    ''' such match is found.</returns>
    Public Function FindMember(ByVal ID As Guid) As clsApplicationMember
        Return RootApplicationElement.FindMember(ID)
    End Function

    ''' <summary>
    ''' Finds the element with the supplied ID, by traversing the tree from the root
    ''' element.
    ''' </summary>
    ''' <param name="ID">The ID of the element to find.</param>
    ''' <returns>Returns the first element found with a matching ID, or Nothing if no
    ''' such match is found.</returns>
    ''' <remarks>This method should only be used if the callee is sure that the
    ''' application member with the supplied ID is of type clsApplicationElement.
    ''' Otherwise the  method FindMember should be used instead, in order to 
    ''' avoid an invalid cast exception.</remarks>
    Public Function FindElement(ByVal ID As Guid) As clsApplicationElement
        Return RootApplicationElement.FindMember(Of clsApplicationElement)(ID)
    End Function

    ''' <summary>
    ''' Finds the region container with the supplied ID, if present.
    ''' </summary>
    ''' <param name="id">The ID of the required region container</param>
    ''' <returns>The region container in this application definition with the given
    ''' ID, or null if no member with the ID was found in this definition, or if the
    ''' member found was not a region container</returns>
    Public Function FindRegionContainer(ByVal id As Guid) As clsRegionContainer
        Return RootApplicationElement.FindMember(Of clsRegionContainer)(id)
    End Function

    ''' <summary>
    ''' Finds the descendant of the supplied application member, which has the
    ''' requested ID.
    ''' </summary>
    ''' <param name="ChildID">The ID of the descendant to find.</param>
    ''' <param name="objParent">The starting point of the search. All child members
    ''' will be searched recursively.</param>
    ''' <returns>Returns the first descendant found having  a matching ID, or Nothing
    ''' if no such exists.</returns>
    Public Function FindDescendant(ByVal ChildID As Guid, ByVal objParent As clsApplicationMember) As clsApplicationMember
        For Each objMember As clsApplicationMember In objParent.ChildMembers
            If objMember.ID.Equals(ChildID) Then
                Return objMember
            End If
            Dim temp As clsApplicationMember = FindDescendant(ChildID, objMember)
            If Not temp Is Nothing Then Return temp
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Get all the descendents of the application definition keyed by id
    ''' </summary>
    Public Function Descendents() As IDictionary(Of Guid, clsApplicationMember)
        Dim dict As New Dictionary(Of Guid, clsApplicationMember)
        GetDescendents(RootApplicationElement.ChildMembers, dict, False)
        Return dict
    End Function

    ''' <summary>
    ''' Get all the descendents of the child members keyed by id
    ''' </summary>
    ''' <param name="fixDuplicateIds">True to replace duplicate Ids
    ''' with new Ids</param>
    Private Sub GetDescendents(children As ICollection(Of clsApplicationMember),
     descendents As IDictionary(Of Guid, clsApplicationMember), fixDuplicateIds As Boolean)
        For Each element In children

            Dim g = TryCast(element, clsApplicationElementGroup)
            If g IsNot Nothing Then
                HandleDuplicateIds(descendents, fixDuplicateIds, g)
                GetDescendents(g.ChildMembers, descendents, fixDuplicateIds)
            End If

            Dim e = TryCast(element, clsApplicationElement)
            If e IsNot Nothing Then
                HandleDuplicateIds(descendents, fixDuplicateIds, e)
                GetDescendents(e.ChildMembers, descendents, fixDuplicateIds)
            End If
        Next
    End Sub

    Private Sub HandleDuplicateIds(descendents As IDictionary(Of Guid, clsApplicationMember),
                                   fixDuplicateIds As Boolean, m As clsApplicationMember)
        If Not descendents.ContainsKey(m.ID) Then
            descendents.Add(m.ID, m)
        ElseIf fixDuplicateIds Then
            m.ID = Guid.NewGuid()
            descendents.Add(m.ID, m)
        End If
    End Sub

    ''' <summary>
    ''' Diagnostic actions to be taken after an element mismatch.
    ''' </summary>
    Public ReadOnly Property DiagnosticActions() As List(Of clsActionTypeInfo)
        Get
            If mDiagnosticActions Is Nothing Then
                mDiagnosticActions = New List(Of clsActionTypeInfo)
            End If
            Return mDiagnosticActions
        End Get
    End Property
    Private mDiagnosticActions As List(Of clsActionTypeInfo)

    ''' <summary>
    ''' Generates an xml element representing the application definition.
    ''' </summary>
    ''' <param name="doc">The parent document
    ''' to use when generating new elements.</param>
    ''' <returns>As summary.</returns>
    Public Function ToXML(ByVal doc As XmlDocument) As XmlElement
        Dim e As XmlElement
        e = doc.CreateElement("appdef")
        If Not mRootApplicationElement Is Nothing Then
            e.AppendChild(mRootApplicationElement.ToXML(doc))
        End If
        If Not Me.ApplicationInfo Is Nothing Then
            e.AppendChild(Me.ApplicationInfo.ToXML(doc))
        End If
        If Not Me.DiagnosticActions.Count = 0 Then
            Dim e2 As XmlElement = doc.CreateElement("diagnostics")

            Dim e3 As XmlElement = Nothing
            For Each a As clsActionTypeInfo In Me.DiagnosticActions
                e3 = doc.CreateElement("action")
                Dim e4 As XmlElement = doc.CreateElement("id")
                e4.InnerText = a.ID

                e3.AppendChild(e4)
            Next

            e2.AppendChild(e3)

            e.AppendChild(e2)
        End If

        Return e
    End Function

    ''' <summary>
    ''' Create a new clsApplicationDefinition instance using an XML fragment as
    ''' created by the .ToXML method.
    ''' </summary>
    ''' <param name="e">The XmlElement of the "appdef"</param>
    ''' <returns>A new clsApplicationDefinition instance.</returns>
    Public Shared Function FromXML(ByVal e As XmlElement) As clsApplicationDefinition
        Dim ad As New clsApplicationDefinition

        For Each child As XmlElement In e.ChildNodes
            Select Case child.Name
                Case "element", "region-container", "region"
                    ad.RootApplicationElement = DirectCast( _
                     clsApplicationMember.CreateFromXML(child), clsApplicationElement)

                Case "apptypeinfo"
                    ad.ApplicationInfo = clsApplicationTypeInfo.FromXML(child)

                Case "diagnostics"
                    For Each subEl As XmlElement In child.ChildNodes
                        If subEl.Name = "action" Then
                            For Each subSubEl As XmlElement In subEl.ChildNodes
                                If subSubEl.Name = "id" Then
                                    Dim act As clsActionTypeInfo = _
                                     clsAMI.GetActionTypeInfo(subSubEl.InnerText)

                                    Debug.Assert(act IsNot Nothing, _
                                     "The action type: " & subSubEl.InnerText & _
                                     " is not registered in AMI - it will be skipped")

                                    If act IsNot Nothing Then
                                        Dim alternative As String = Nothing, helpMessage As String = Nothing, helpTopic As Integer
                                        If clsAMI.IsValidDiagnosticAction(act.ID, ad.ApplicationInfo, alternative, helpMessage, helpTopic) Then
                                            If alternative IsNot Nothing Then
                                                act = clsAMI.GetActionTypeInfo(alternative)
                                                Debug.Assert(act IsNot Nothing, _
                                                 "The alternative action type " & alternative & _
                                                 "doesn't exist")
                                            End If
                                            ad.DiagnosticActions.Add(act)
                                        Else
                                            Debug.Fail("This diagnostic action is invalid")
                                        End If
                                    End If

                                End If
                            Next
                        End If
                    Next
            End Select
        Next

        ad.FixDuplicateIds()

        ad.RootApplicationElement.ResolveRegionRelationships()

        Return ad
    End Function

    ''' <summary>
    ''' Find duplicate element Id's and replace them with new Ids
    ''' </summary>
    Private Sub FixDuplicateIds()
        Dim dict As New Dictionary(Of Guid, clsApplicationMember)
        GetDescendents(RootApplicationElement.ChildMembers, dict, True)
    End Sub
    ''' <summary>
    ''' Clones this application definition.
    ''' </summary>
    ''' <returns>A deep clone of this application definition</returns>
    Public Function Clone() As clsApplicationDefinition
        Return FromXML(ToXML(New XmlDocument()))
    End Function

    ''' <summary>
    ''' Gets a string representation of this application definition
    ''' </summary>
    ''' <returns>This application definition object as a string - actually returned
    ''' in XML form </returns>
    Public Overrides Function ToString() As String
        Return ToXML(New XmlDocument()).InnerXml
    End Function

    ''' <summary>
    ''' Returns any references to other dependency items.
    ''' </summary>
    ''' <param name="inclInternal">Include internal references</param>
    ''' <returns>List of dependency objects</returns>
    Public Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
        Dim deps As New List(Of clsProcessDependency)()

        Dim parent As clsApplicationMember = RootApplicationElement
        Dim members As New clsSet(Of clsApplicationMember)()
        If parent IsNot Nothing Then
            parent.GetAllDescendents(parent, members)
            For Each mem As clsApplicationMember In members
                Dim reg As clsApplicationRegion = TryCast(mem, clsApplicationRegion)
                If reg IsNot Nothing Then
                    For Each attr As clsApplicationAttribute In reg.Attributes
                        If attr.Name = "FontName" And attr.Value.FormattedValue <> String.Empty Then
                            deps.Add(New clsProcessFontDependency(attr.Value.FormattedValue))
                        End If
                    Next
                End If
            Next
        End If

        Return deps
    End Function

    Public Function IsBrowserAppDefinition() As Boolean
        Return CheckBrowserID(clsApplicationTypeInfo.BrowserLaunchId) OrElse
        CheckBrowserID(clsApplicationTypeInfo.BrowserAttachId) OrElse
        CheckBrowserID(clsApplicationTypeInfo.CitrixBrowserLaunchID) OrElse
        CheckBrowserID(clsApplicationTypeInfo.CitrixBrowserAttachID)
    End Function

    Public Function CheckBrowserID(id As string) As Boolean
       return ApplicationInfo.Id.Equals(id)
    End Function

End Class
