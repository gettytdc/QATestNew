Imports System.Collections.Generic
Imports System.Linq

Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports BluePrism.Utilities.Functional
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Conditions
Imports BluePrism.Core
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Imports ComparisonTypes =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsMatchTarget.ComparisonTypes
Imports IdentifierTypes =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.IdentifierTypes
Imports ParameterNames =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.ParameterNames


Namespace UIAutomation

    Public Class UIAutomationIdentifierHelper : Implements IUIAutomationIdentifierHelper

        ''' <summary>
        ''' The automation properties supported in a search, mapped against the
        ''' <see cref="IdentifierTypes"/> instances which represent them in App Man
        ''' queries
        ''' </summary>
        Private ReadOnly mUiaSearchMap As IDictionary(Of IdentifierTypes, PropertyType) =
            GetReadOnly.IDictionary(New Dictionary(Of IdentifierTypes, PropertyType) From {
                {IdentifierTypes.uAutomationId, PropertyType.AutomationId},
                {IdentifierTypes.uClassName, PropertyType.ClassName},
                {IdentifierTypes.uControlType, PropertyType.ControlType},
                {IdentifierTypes.uLocalizedControlType, PropertyType.LocalizedControlType},
                {IdentifierTypes.uName, PropertyType.Name},
                {IdentifierTypes.uIsPassword, PropertyType.IsPassword},
                {IdentifierTypes.uIsRequiredForForm, PropertyType.IsRequiredForForm},
                {IdentifierTypes.uOrientation, PropertyType.Orientation},
                {IdentifierTypes.uItemStatus, PropertyType.ItemStatus},
                {IdentifierTypes.uItemType, PropertyType.ItemType},
                {IdentifierTypes.uLabeledBy, PropertyType.LabeledBy},
                {IdentifierTypes.uOffscreen, PropertyType.IsOffscreen},
                {IdentifierTypes.uProcessId, PropertyType.ProcessId},
                {IdentifierTypes.uEnabled, PropertyType.IsEnabled},
                {IdentifierTypes.uHelpText, PropertyType.HelpText},
                {IdentifierTypes.uHasKeyboardFocus, PropertyType.HasKeyboardFocus},
                {IdentifierTypes.uAcceleratorKey, PropertyType.AcceleratorKey},
                {IdentifierTypes.uAccessKey, PropertyType.AccessKey}
            })

        Private ReadOnly mAutomationFactory As IAutomationFactory
        Private ReadOnly mAutomationHelper As IAutomationHelper

        Private ReadOnly mComparisonPropertyTypes As IReadOnlyCollection(Of PropertyType) = New List(Of PropertyType) From {
            PropertyType.AcceleratorKey,
            PropertyType.AccessKey,
            PropertyType.AutomationId,
            PropertyType.BoundingRectangle,
            PropertyType.ClassName,
            PropertyType.HasKeyboardFocus,
            PropertyType.HelpText,
            PropertyType.IsEnabled,
            PropertyType.IsOffscreen,
            PropertyType.IsPassword,
            PropertyType.IsRequiredForForm,
            PropertyType.ItemStatus,
            PropertyType.ItemType,
            PropertyType.LabeledBy,
            PropertyType.ControlType,
            PropertyType.LocalizedControlType,
            PropertyType.Name ,
            PropertyType.Orientation _
            }

        Private ReadOnly mComparisonFunctions As IReadOnlyDictionary(Of clsQuery.IdentifierTypes, Func(Of clsIdentifierMatchTarget, Func(Of IAutomationElement, IAutomationCacheRequest, Boolean))) =
            New Dictionary(Of clsQuery.IdentifierTypes, Func(Of clsIdentifierMatchTarget, Func(Of IAutomationElement, IAutomationCacheRequest, Boolean))) From {
            {clsQuery.IdentifierTypes.uX, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedBoundingRectangle.X)},
            {clsQuery.IdentifierTypes.uY, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedBoundingRectangle.Y)},
            {clsQuery.IdentifierTypes.uWidth, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedBoundingRectangle.Width)},
            {clsQuery.IdentifierTypes.uHeight, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedBoundingRectangle.Height)},
            {clsQuery.IdentifierTypes.uClassName, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedClassName)},
            {clsQuery.IdentifierTypes.uAutomationId, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedAutomationId)},
            {clsQuery.IdentifierTypes.uControlType, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedControlType.ToString())},
            {clsQuery.IdentifierTypes.uLocalizedControlType, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedLocalizedControlType)},
            {clsQuery.IdentifierTypes.uName, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedName)},
            {clsQuery.IdentifierTypes.uIsPassword, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedIsPassword)},
            {clsQuery.IdentifierTypes.uIsRequiredForForm, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedIsRequiredForForm)},
            {clsQuery.IdentifierTypes.uOrientation, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedOrientation)},
            {clsQuery.IdentifierTypes.uItemStatus, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedItemStatus)},
            {clsQuery.IdentifierTypes.uItemType, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedItemType)},
            {clsQuery.IdentifierTypes.uLabeledBy, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(If(element.CachedLabeledBy?.CachedAutomationId, ""))},
            {clsQuery.IdentifierTypes.uAcceleratorKey, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedAcceleratorKey)},
            {clsQuery.IdentifierTypes.uAccessKey, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedAccessKey)},
            {clsQuery.IdentifierTypes.uHasKeyboardFocus, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedHasKeyboardFocus)},
            {clsQuery.IdentifierTypes.uHelpText, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedHelpText)},
            {clsQuery.IdentifierTypes.uOffscreen, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedIsOffscreen)},
            {clsQuery.IdentifierTypes.uEnabled, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(element.CachedIsEnabled)},
            {clsQuery.IdentifierTypes.uTopLevelWindowId, Function(constraint) Function(element, cacheRequest) constraint.IsMatch(GetTopLevelWindowAutomationId(element))},
            {clsQuery.IdentifierTypes.puControlType, AddressOf IsMatch_ParentElementControlType},
            {clsQuery.IdentifierTypes.puLocalizedControlType, AddressOf IsMatch_ParentElementLocalizedControlType},
            {clsQuery.IdentifierTypes.puClassName, AddressOf IsMatch_ParentElementClassName},
            {clsQuery.IdentifierTypes.puName, AddressOf IsMatch_ParentElementName},
            {clsQuery.IdentifierTypes.puAutomationId, AddressOf IsMatch_ParentElementAutomationId}
        }

        ''' <summary>
        ''' Returns a function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's class name matches
        ''' the specified match target.
        ''' </summary>
        ''' <param name="constraint">The match target to check against</param>
        ''' <returns>A function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's class name matches
        ''' the specified match target.</returns>
        Private Function IsMatch_ParentElementClassName(constraint As clsIdentifierMatchTarget) _
            As Func(Of IAutomationElement, IAutomationCacheRequest, Boolean)

            Return Function(element As IAutomationElement, cacheRequest As IAutomationCacheRequest) As Boolean
                       Dim parent = element.GetCachedParent(cacheRequest)
                       If parent IsNot Nothing Then
                           Return constraint.IsMatch(If(parent.CachedClassName, ""))
                       Else
                           Return False
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' Returns a function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's name matches
        ''' the specified match target.
        ''' </summary>
        ''' <param name="constraint">The match target to check against</param>
        ''' <returns>A function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's name matches
        ''' the specified match target.</returns>
        Private Function IsMatch_ParentElementName(constraint As clsIdentifierMatchTarget) _
            As Func(Of IAutomationElement, IAutomationCacheRequest, Boolean)

            Return Function(element As IAutomationElement, cacheRequest As IAutomationCacheRequest) As Boolean
                       Dim parent = element.GetCachedParent(cacheRequest)
                       If parent IsNot Nothing Then
                           Return constraint.IsMatch(If(parent.CachedName, ""))
                       Else
                           Return False
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' Returns a function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's automation id matches
        ''' the specified match target.
        ''' </summary>
        ''' <param name="constraint">The match target to check against</param>
        ''' <returns>A function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's automation id matches
        ''' the specified match target.</returns>
        Private Function IsMatch_ParentElementAutomationId(constraint As clsIdentifierMatchTarget) _
            As Func(Of IAutomationElement, IAutomationCacheRequest, Boolean)

            Return Function(element As IAutomationElement, cacheRequest As IAutomationCacheRequest) As Boolean
                       Dim parent = element.GetCachedParent(cacheRequest)
                       If parent IsNot Nothing Then
                           Return constraint.IsMatch(If(parent.CachedAutomationId, ""))
                       Else
                           Return False
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' Returns a function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's localized control type matches
        ''' the specified match target.
        ''' </summary>
        ''' <param name="constraint">The match target to check against</param>
        ''' <returns>A function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's control type matches
        ''' the specified match target.</returns>
        Private Shared Function IsMatch_ParentElementLocalizedControlType(constraint As clsIdentifierMatchTarget) _
            As Func(Of IAutomationElement, IAutomationCacheRequest, Boolean)

            Return Function(element As IAutomationElement, cacheRequest As IAutomationCacheRequest) As Boolean
                Dim parent = element.GetCachedParent(cacheRequest)
                If parent IsNot Nothing Then
                    Return constraint.IsMatch(If(parent.CachedLocalizedControlType, ""))
                Else
                    Return False
                End If
            End Function
        End Function

        ''' <summary>
        ''' Returns a function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's control type matches
        ''' the specified match target.
        ''' </summary>
        ''' <param name="constraint">The match target to check against</param>
        ''' <returns>A function that accepts an Automation Element and a Cache Request
        ''' and returns True if the element's cached parent's control type matches
        ''' the specified match target.</returns>
        Private Shared Function IsMatch_ParentElementControlType(constraint As clsIdentifierMatchTarget) _
            As Func(Of IAutomationElement, IAutomationCacheRequest, Boolean)

            Return Function(element As IAutomationElement, cacheRequest As IAutomationCacheRequest) As Boolean
                       Dim parent = element.GetCachedParent(cacheRequest)
                       If parent IsNot Nothing Then
                           Return constraint.IsMatch(parent.CachedControlType.ToString())
                       Else
                           Return False
                       End If
                   End Function
        End Function

        Public Sub New(
            automationFactory As IAutomationFactory,
            automationHelper As IAutomationHelper)

            mAutomationFactory = automationFactory
            mAutomationHelper = automationHelper
        End Sub


        ''' <inheritDoc />
        Public Function AppendIdentifiers(
         element As IAutomationElement,
         sb As StringBuilder) As StringBuilder _
         Implements IUIAutomationIdentifierHelper.AppendIdentifiers

            Dim bounds = element.CurrentBoundingRectangle

            If bounds <> Nothing Then sb.
                AppendId(IdentifierTypes.uX, bounds.Left).
                AppendId(IdentifierTypes.uY, bounds.Top).
                AppendId(IdentifierTypes.uWidth, bounds.Width).
                AppendId(IdentifierTypes.uHeight, bounds.Height)

            Dim topLevelWindowID As String = GetTopLevelWindowAutomationId(element)

            sb.
                AppendId(IdentifierTypes.uClassName, element.CurrentClassName).
                AppendId(IdentifierTypes.uAutomationId, element.CurrentAutomationId).
                AppendId(IdentifierTypes.uControlType, element.CurrentControlType.ToString()).
                AppendId(IdentifierTypes.uLocalizedControlType, element.CurrentLocalizedControlType).
                AppendId(IdentifierTypes.uIsPassword, element.CurrentIsPassword).
                AppendId(IdentifierTypes.uIsRequiredForForm, element.CurrentIsRequiredForForm).
                AppendId(IdentifierTypes.uName, element.CurrentName).
                AppendId(IdentifierTypes.uOrientation, element.CurrentOrientation).
                AppendId(IdentifierTypes.uItemStatus, element.CurrentItemStatus).
                AppendId(IdentifierTypes.uItemType, element.CurrentItemType).
                AppendId(IdentifierTypes.uOffscreen, element.CurrentIsOffscreen).
                AppendId(IdentifierTypes.uTopLevelWindowId, topLevelWindowID).
                AppendId(IdentifierTypes.uLabeledBy, element.CurrentLabeledBy?.CurrentAutomationId).
                AppendId(IdentifierTypes.uEnabled, element.CurrentIsEnabled).
                AppendId(IdentifierTypes.uAcceleratorKey, element.CurrentAcceleratorKey).
                AppendId(IdentifierTypes.uAccessKey, element.CurrentAccessKey).
                AppendId(IdentifierTypes.uHasKeyboardFocus, element.CurrentHasKeyboardFocus).
                AppendId(IdentifierTypes.uHelpText, element.CurrentHelpText).
                AppendId(IdentifierTypes.uProcessId, element.CurrentProcessId)

            Dim parentElement = mAutomationFactory.GetParentElement(element)
            If parentElement IsNot Nothing Then
                sb.
                    AppendId(IdentifierTypes.puControlType, parentElement.CurrentControlType.ToString()).
                    AppendId(IdentifierTypes.puLocalizedControlType, parentElement.CurrentLocalizedControlType).
                    AppendId(IdentifierTypes.puClassName, parentElement.CurrentClassName).
                    AppendId(IdentifierTypes.puName, parentElement.CurrentName).
                    AppendId(IdentifierTypes.puAutomationId, parentElement.CurrentAutomationId)
            End If

            sb.Append(" MatchIndex=1")

            Return sb
        End Function

        ''' <inheritDoc />
        Public Function Matches(
            element As IAutomationElement,
            identifiers As IEnumerable(Of clsIdentifierMatchTarget),
            cache As IAutomationCacheRequest,
            includeBounds As Boolean) As Boolean _
            Implements IUIAutomationIdentifierHelper.Matches

            For Each constraint As clsIdentifierMatchTarget In identifiers
                ' Anything which requires greater context than we have available, we
                ' can ignore
                If CanIgnoreInBasicCheck(constraint.Identifier) Then Continue For

                ' If we're not including bounds checks and this is a bounds check, we
                ' can ignore that too
                If Not includeBounds AndAlso constraint.Identifier.IsUiaBounds() Then _
                    Continue For

                ' Otherwise do a full match
                If Not Matches(element, cache, includeBounds, constraint) Then _
                    Return False
            Next

            Return True

        End Function


        Public Function Matches(
            element As IAutomationElement,
            cache As IAutomationCacheRequest,
            includeBounds As Boolean,
            constraint As clsIdentifierMatchTarget) As Boolean _
            Implements IUIAutomationIdentifierHelper.Matches

            Dim parentElement As IAutomationElement = mAutomationFactory.GetParentElement(element, cache)
            Dim id = constraint.Identifier

            ' If this is an ancestor ID we can chain it straight to checking that
            ' element using a small recursion
            If id.IsUiaAncestor() Then
                ' If we don't have a parent element, any parent value is empty, by
                ' definition; perhaps the constraint allows for an empty value - if
                ' so we should match on that, otherwise, there's no need to bother
                ' checking any further.
                If parentElement Is Nothing Then Return constraint.IsMatch("")

                ' Otherwise, match on the parent element using the child ID directly
                ' Note that if this is a grandparent match, it will recurse twice,
                ' but this mechanism supports arbitrary levels of depth here.
                Return Matches(parentElement, cache, includeBounds, constraint.GetChildEquivalent())

            End If

            ' Get the cached information to check
            Select Case id
                Case IdentifierTypes.uX
                    If Not constraint.IsMatch(element.CachedBoundingRectangle.X) Then Return False
                Case IdentifierTypes.uY
                    If Not constraint.IsMatch(element.CachedBoundingRectangle.Y) Then Return False
                Case IdentifierTypes.uWidth
                    If Not constraint.IsMatch(element.CachedBoundingRectangle.Width) Then Return False
                Case IdentifierTypes.uHeight
                    If Not constraint.IsMatch(element.CachedBoundingRectangle.Height) Then Return False
                Case IdentifierTypes.uClassName
                    If Not constraint.IsMatch(element.CachedClassName) Then Return False
                Case IdentifierTypes.uAutomationId
                    If Not constraint.IsMatch(element.CachedAutomationId) Then Return False
                Case IdentifierTypes.uControlType
                    If Not constraint.IsMatch(element.CachedControlType.ToString()) Then Return False
                Case IdentifierTypes.uLocalizedControlType
                    If Not constraint.IsMatch(element.CachedLocalizedControlType) Then Return False
                Case IdentifierTypes.uName
                    If Not constraint.IsMatch(element.CachedName) Then Return False
                Case IdentifierTypes.uIsPassword
                    If Not constraint.IsMatch(element.CachedIsPassword) Then Return False
                Case IdentifierTypes.uIsRequiredForForm
                    If Not constraint.IsMatch(element.CachedIsRequiredForForm) Then Return False
                Case IdentifierTypes.uOrientation
                    If Not constraint.IsMatch(element.CachedOrientation) Then Return False
                Case IdentifierTypes.uItemStatus
                    If Not constraint.IsMatch(element.CachedItemStatus) Then Return False
                Case IdentifierTypes.uItemType
                    If Not constraint.IsMatch(element.CachedItemType) Then Return False
                Case IdentifierTypes.uLabeledBy
                    If Not constraint.IsMatch(
                     If(element.CachedLabeledBy?.CachedAutomationId, "")) Then Return False
                Case IdentifierTypes.uAcceleratorKey
                    If Not constraint.IsMatch(element.CachedAcceleratorKey) Then Return False
                Case IdentifierTypes.uAccessKey
                    If Not constraint.IsMatch(element.CachedAccessKey) Then Return False
                Case IdentifierTypes.uHasKeyboardFocus
                    If Not constraint.IsMatch(element.CachedHasKeyboardFocus) Then Return False
                Case IdentifierTypes.uHelpText
                    If Not constraint.IsMatch(element.CachedHelpText) Then Return False
                Case IdentifierTypes.uOffscreen
                    If Not constraint.IsMatch(element.CachedIsOffscreen) Then Return False
                Case IdentifierTypes.uEnabled
                    If Not constraint.IsMatch(element.CachedIsEnabled) Then Return False
                Case IdentifierTypes.uTopLevelWindowId
                    If Not constraint.IsMatch(GetTopLevelWindowAutomationId(element)) _
                     Then Return False

            End Select

            ' Anything which doesn't fail is presumed to match. This includes any
            ' of the above identifiers which *does* match or, indeed, any identifier
            ' not in the above list.
            Return True

        End Function

        ''' <inheritDoc />
        Public Function GetComparisonFunction(constraint As clsIdentifierMatchTarget) As Func(Of IAutomationElement, IAutomationCacheRequest, Boolean) _
            Implements IUIAutomationIdentifierHelper.GetComparisonFunction

            Return _
                If(mComparisonFunctions.ContainsKey(constraint.Identifier),
                    mComparisonFunctions(constraint.Identifier)(constraint),
                    Function(e, c) False)
        End Function

        ''' <inheritDoc />
        Public Function GetCacheRequestForComparison() As IAutomationCacheRequest _
            Implements IUIAutomationIdentifierHelper.GetCacheRequestForComparison

            Dim cacheRequest = mAutomationFactory.CreateCacheRequest()

            For Each propertyType In mComparisonPropertyTypes
                cacheRequest.Add(propertyType)
            Next

            Return cacheRequest
        End Function
    
        ''' <summary>
        ''' Walk back up the UI tree to get the AutomationID of the first window that
        ''' matches the specified processID
        ''' </summary>
        ''' <param name="element">The automation element to get the root from.</param>
        ''' <returns>The automation ID of the top level window which owns 'this'
        ''' automation element</returns>
        Private Function GetTopLevelWindowAutomationId(element As IAutomationElement) _
            As String

            Dim walker = mAutomationFactory.CreateControlTreeWalker()
            Dim pid = element.CurrentProcessId

            ' Hold 'current' and 'parent' elements. When the parent's PID doesn't
            ' match the current PID, the current is the root within this process,
            ' so return that.
            Dim current = element
            Dim parent = walker.GetParent(element)

            While parent IsNot Nothing
                If parent.CurrentProcessId <> pid Then _
                    Return current.CurrentAutomationId

                current = parent
                parent = walker.GetParent(current)
            End While

            Return ""

        End Function

        ''' <summary>
        ''' Checks if this identifier type can be ignored in a basic check. Generally
        ''' anything which requires greater context than a single attribute against
        ''' a single element can provide.
        ''' </summary>
        ''' <param name="identifier">The identifier type to check</param>
        ''' <returns>True if this identifiery type can be ignored in a basic test of
        ''' an element matching a constraint; False if it should be tested.</returns>
        Private Function CanIgnoreInBasicCheck(identifier As IdentifierTypes) As Boolean
            Return {
                IdentifierTypes.uMatchIndex,
                IdentifierTypes.uTopLevelWindowId
            }.Contains(identifier)
        End Function


#Region " UIAutomation "

        ''' <summary>
        ''' Finds the UIAutomation element which matches the given query for the
        ''' process running with the given ID.
        ''' </summary>
        ''' <param name="q">The query to match against</param>
        ''' <param name="pid">The ID of the process to search.</param>
        ''' <returns>The single automation element which matches the given query.
        ''' </returns>
        ''' <exception cref="NoSuchElementException">If no UIA element could be found
        ''' which matched the constraints set in the query.</exception>
        ''' <exception cref="TooManyElementsException">If more than one UIA element
        ''' was found which matched the constraints set in the query.</exception>
        Friend Function FindUIAutomationElement(
         q As clsQuery, pid As Integer) As IAutomationElement _
         Implements IUIAutomationIdentifierHelper.FindUIAutomationElement
            'Get a list of matching elements.
            Dim matches = FindAllUIAutomationElements(q, pid)

            If Not matches.Any() Then Throw New NoSuchElementException(
             My.Resources.NoElementMatchedTheQueryTerms)

            If matches.Count > 1 Then Throw New TooManyElementsException(
             My.Resources.MoreThanOneElementMatchedTheQueryTerms)

            'We only matched one AAelement - here it is...
            Return matches.First()
        End Function

        ''' <summary>
        ''' Find all the automation elements that match the search query
        ''' </summary>
        ''' <param name="q">Query containing information about what to elements on
        ''' </param>
        ''' <param name="pid">The process id of the application containing the element
        ''' </param>
        ''' <returns>The automation elements that match the query</returns>
        Friend Function FindAllUIAutomationElements(q As clsQuery, pid As Integer) _
         As IReadOnlyCollection(Of IAutomationElement) _
         Implements IUIAutomationIdentifierHelper.FindAllUIAutomationElements

            Dim cacheRequest = GetCacheRequestForComparison()
            ' Find the root window elements
            Dim roots = FindProcessWindowElements(pid, Nothing, cacheRequest)
            If roots.Count = 0 Then Return roots

            Dim matchIndex = q.GetIntParam(ParameterNames.MatchIndex)

            Dim condition = q.Identifiers.
             Select(AddressOf GetComparisonFunction).
             Select(AddressOf mAutomationFactory.CreateCustomCondition).
             Map(AddressOf JoinConditions)

            Dim elements = roots.
             SelectMany(Function(x) x.FindAll(TreeScope.Subtree, condition, cacheRequest)).
             ToList()

            ' If there is a match index, only return the correct match
            If matchIndex > 0 Then
                ' if matchIndex is beyond the number of elements returned, then
                ' it is not a match
                If matchIndex > elements.Count() Then _
                    Return New IAutomationElement() {}

                ' Get the element at matchIndex - 1 (matchIndex is 1-based;
                ' ElementAt is 0-based)
                Return New IAutomationElement() {elements(matchIndex - 1)}
            End If

            Return elements

        End Function

        ''' <summary>
        ''' Find the top level automation element associated with the specified process.
        ''' </summary>
        ''' <param name="pid">The process id to search for</param>
        ''' <returns>The UI Automation element of the process's top level window
        ''' </returns>
        Public Function FindProcessWindowElements(pid As Integer) _
         As IReadOnlyCollection(Of IAutomationElement) _
         Implements IUIAutomationIdentifierHelper.FindProcessWindowElements
            Return FindProcessWindowElements(pid, Nothing,
                                             mAutomationFactory.CreateCacheRequest)
        End Function

        ''' <summary>
        ''' Find the top level automation element associated with the specified
        ''' process.
        ''' </summary>
        ''' <param name="pid">The process id to search for</param>
        ''' <param name="windowId">The AutomationID of the top level window of
        ''' the application or null to look in all windows
        ''' </param>
        ''' <returns>The UI Automation element of the process window</returns>
        Public Function FindProcessWindowElements(pid As Integer, windowId As String) _
            As IReadOnlyCollection(Of IAutomationElement) _
            Implements IUIAutomationIdentifierHelper.FindProcessWindowElements

            Return FindProcessWindowElements(pid, windowId,
                                             mAutomationFactory.CreateCacheRequest)

        End Function

        ''' <summary>
        ''' Find the top level automation element associated with the specified
        ''' process.
        ''' </summary>
        ''' <param name="pid">The process id to search for</param>
        ''' <param name="windowId">The AutomationID of the top level window of
        ''' the application or null to look in all windows
        ''' </param>
        ''' <param name="cache">The cache request to use to retrieve information
        ''' about the automation element.</param>
        ''' <returns>The UI Automation element of the process window</returns>
        Public Function FindProcessWindowElements(pid As Integer, windowId As String,
                                                  cache As IAutomationCacheRequest) _
         As IReadOnlyCollection(Of IAutomationElement) _
         Implements IUIAutomationIdentifierHelper.FindProcessWindowElements

            ' Create the search conditions to find the automation element
            ' representing the process window
            Dim condition As IAutomationCondition =
             mAutomationFactory.CreatePropertyCondition(PropertyType.ProcessId, pid)

            If windowId <> "" Then condition = mAutomationFactory.CreateAndCondition(
                condition, mAutomationFactory.CreatePropertyCondition(
                    PropertyType.AutomationId, windowId))

            'Make a first attempt by searching direct children of desktop first
            Dim matchingChildren = mAutomationFactory.GetRootElement().FindAll(
                TreeScope.Children, condition, cache).ToList()
            If matchingChildren.Any() Then Return matchingChildren

            ' If nothing found, then attempt to find the Internet Explorer_Server
            ' window that matches the pid - this is because each IE tab has a
            ' separate explorer server with a separate processid.
            Dim ieServer = FindInternetExplorerServer(pid, cache)
            If ieServer IsNot Nothing Then
                Return New IAutomationElement() {ieServer}
            End If

            ' Otherwise, try and find the applicationframewindow that is found in
            ' modern apps, and look for the process inside that
            Dim modernAppFrames = FindModernFrames(condition, cache)
            If modernAppFrames.Count > 0 Then Return modernAppFrames

            Return New IAutomationElement() {}

        End Function

        ''' <summary>
        ''' Checks if this identifier match target can be used in a FindAll() call in
        ''' UIAutomation.
        ''' </summary>
        ''' <param name="identifierMatchTarget">The identifier match target to check
        ''' </param>
        ''' <param name="usingIE">Indicates whether the application is Internet
        ''' Explorer</param>
        ''' <returns>True, if the identifier match target can be used in a FindAll()
        ''' call in UIAutomation.</returns>
        ''' <remarks>
        ''' <para>Effectively, this checks that the match target represents an
        ''' equality check (either 'Equals' or 'Not Equals' a value - no wildcards or
        ''' ranges etc) and it is in the whitelist of identifiers that we have a
        ''' property lookup for.</para>
        ''' <para>Note that the bounding rectangle of an element can be included in a
        ''' FindAll() but that is not checked in the context of this test - it can
        ''' only be included if all 4 bounding identifiers are included in the query
        ''' as Equals tests and this method doesn't have the context to determine
        ''' that. Individually, the identifiers cannot be used in a FindAll query, so
        ''' they will return false from this test.</para>
        ''' <para>There is also an edge case where you can't successfully search for an 
        ''' element using Name = "" when the application is IE.</para>
        ''' </remarks>
        Private Function CanBeSearchedInUiaFindAll(
         identifierMatchTarget As clsIdentifierMatchTarget,
         usingIE As Boolean) As Boolean

            Return TryGetValidPropertyType(identifierMatchTarget, usingIE, Nothing)

        End Function

        ''' <summary>
        ''' Try and get the Property Type associated the identifier match target, but only
        ''' if that Property is valid and can be used in UIA FindAll searches.
        ''' </summary>
        ''' <param name="identifierMatchTarget">
        ''' The identifier match target to get the Property Type for
        ''' </param>
        ''' <param name="usingIE">Indicates whether the application is Internet Explorer</param>
        ''' <param name="type">
        ''' When this method returns, contains the Property Value associated with the 
        ''' specified Identifier Match Target, if that Property Value is valid and can be
        ''' used in UIA FindAll searches; otherwise it returns Nothing.</param>
        ''' <returns>True if the identifier match target has a valid Property Type that 
        ''' can be used in UIA FindAll searches. Otherwise False. </returns>
        Private Function TryGetValidPropertyType(
         identifierMatchTarget As clsIdentifierMatchTarget, usingIE As Boolean,
         ByRef type As PropertyType) As Boolean

            type = Nothing

            ' Find All only works for search conditions using equality checks
            If Not identifierMatchTarget.ComparisonType.IsEqualityCheck() Then Return False

            ' Find All only works for a subset of identifiers
            If Not mUiaSearchMap.TryGetValue(identifierMatchTarget.Identifier, type) Then Return False

            ' There seems to be a bug when trying to search for an element using Name = "" in the FindAll function
            ' when using Internet Explorer
            If usingIE AndAlso type = PropertyType.Name AndAlso identifierMatchTarget.MatchValue = "" Then Return False

            Return True

        End Function


        ''' <summary>
        ''' Join a list of conditions together to make a condition
        ''' </summary>
        ''' <param name="conditions">The conditions to join into a single condition.
        ''' </param>
        ''' <returns>A single condition which asserts that all the given conditions are
        ''' matched.</returns>
        ''' <exception cref="EmptyException">If <paramref name="conditions"/> is empty.
        ''' </exception>
        Private Function JoinConditions(
         conditions As IEnumerable(Of IAutomationCondition)) As IAutomationCondition
            If conditions.Count > 1 Then
                Return mAutomationFactory.CreateAndCondition(conditions.ToArray())
            ElseIf conditions.Count = 1 Then
                Return conditions(0)
            Else
                Throw New EmptyException(My.Resources.NoConditionSpecified)
            End If

        End Function

        ''' <summary>
        ''' Adds an automation element search condition that matches on the bounding
        ''' rectangle, if the search query requests equals matchs on the x, y, width
        ''' and height attributes.
        ''' </summary>
        ''' <param name="q">The query containing the search constraints</param>
        ''' <param name="conditions">The collection to which the condition should be
        ''' added if a condition was created.</param>
        ''' <returns>True if a bounds condition was added to the collection; False
        ''' otherwise.</returns>
        Private Function TryAddBoundsCondition(
         q As clsQuery,
         conditions As ICollection(Of IAutomationCondition)) As Boolean

            Dim x = -1.0#, y = -1.0#, width = -1.0#, height = -1.0#

            For Each ident In q.Identifiers.Where(Function(i) i.Identifier.IsUiaBounds())
                ' If we get *any* non-equality comparisons we can't create a single
                ' constraint, so bail immediately
                If ident.ComparisonType <> ComparisonTypes.Equal Then Return False

                ' Otherwise, populate the appropriate rect variable
                Select Case ident.Identifier
                    Case IdentifierTypes.uX
                        x = InternalCulture.Dbl(ident.MatchValue)
                    Case IdentifierTypes.uY
                        y = InternalCulture.Dbl(ident.MatchValue)
                    Case IdentifierTypes.uWidth
                        width = InternalCulture.Dbl(ident.MatchValue)
                    Case IdentifierTypes.uHeight
                        height = InternalCulture.Dbl(ident.MatchValue)
                End Select
            Next

            ' If any of the values are -1, we have no full bounds search; don't add
            ' anything to the conditions
            If {x, y, width, height}.Contains(-1.0#) Then Return False

            conditions.Add(mAutomationFactory.CreatePropertyCondition(
                PropertyType.BoundingRectangle,
                New Windows.Rect(x, y, width, height)
            ))
            Return True

        End Function

        ''' <summary>
        ''' Finds the IE server associated with the given process ID
        ''' </summary>
        ''' <param name="pid">The process ID for which the internet explorer server
        ''' window is required.</param>
        ''' <param name="cache">The cache request to use to retrieve information
        ''' about the automation element.</param>
        ''' <returns>The automation element associated with the Internet Explorer Server
        ''' window owned by the process with the specified ID.</returns>
        Private Function FindInternetExplorerServer(pid As Integer, 
                                                    cache As IAutomationCacheRequest) _
         As IAutomationElement

            Try
                Dim explorerHwnd As IntPtr
                BPUtil.EnumAllWindows(pid,
                    Function(hwnd)
                        'Check whether the window's class name is the explorer server
                        If GetRealWindowClass(hwnd) <> "Internet Explorer_Server" Then _
                         Return True

                        ' We're only interested in visible windows
                        If Not IsWindowVisible(hwnd) Then Return True

                        ' Found it
                        explorerHwnd = hwnd
                        Return False

                    End Function
                )
                If explorerHwnd = Nothing Then Return Nothing
                Return mAutomationFactory.FromHandle(explorerHwnd, cache)

            Catch ex As Exception
                ' Nothing we can do in production; highlight the problem in dev - there
                ' may be something we can do to mitigate errors if we know about them
                Debug.Fail(ex.ToString())
                Return Nothing

            End Try
        End Function

        ''' <summary>
        ''' Finds the modern application window which matches the given search condition
        ''' </summary>
        ''' <param name="cond">The search condition to use to identify the
        ''' required modern app frame</param>
        ''' <param name="cache">The cache request to use to retrieve information
        ''' about the automation element.</param>
        ''' <returns>The automation element representing the first modern app frame found
        ''' meeting the specified search condition.</returns>
        Private Function FindModernFrames(cond As IAutomationCondition, 
                                          cache As IAutomationCacheRequest) _
            As IReadOnlyCollection(Of IAutomationElement)
            Try
                Dim condition  = mAutomationFactory.CreatePropertyCondition(
                    PropertyType.ClassName,
                    "ApplicationFrameWindow")
                Dim modernAppFrames =
                mAutomationFactory _
                .GetRootElement() _
                .FindAll(
                    TreeScope.Children,
                    condition, cache)

                ' Look through each application frame - if we find any children which
                ' match the search condition, add them to the list.
                Dim elems As New List(Of IAutomationElement)
                For Each frame As IAutomationElement In modernAppFrames
                    Dim elements = frame.FindAll(TreeScope.Children, cond, cache)
                    elems.AddRange(elements.Cast(Of IAutomationElement))
                Next

                Return elems

            Catch ex As Exception
                ' Nothing we can do in production; highlight the problem in dev - there
                ' may be something we can do to mitigate errors if we know about them
                Debug.Fail(ex.ToString())
                Return Nothing

            End Try
        End Function

#End Region

    End Class
End Namespace
