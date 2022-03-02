Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Reflection

Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.Server.Domain.Models
Imports BluePrism.BrowserAutomation
Imports BluePrism.BrowserAutomation.Events
Imports BluePrism.Utilities.Functional

Imports IdentifierTypes =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.IdentifierTypes

Namespace BrowserAutomation
    Public Class BrowserAutomationIdentifierHelper
        Implements IBrowserAutomationIdentifierHelper

        Private Shared ReadOnly DefaultIdentifierTypes As New HashSet(Of IdentifierTypes) From {
            IdentifierTypes.wName,
            IdentifierTypes.wId,
            IdentifierTypes.wElementType,
            IdentifierTypes.wPageAddress,
            IdentifierTypes.wXPath,
            IdentifierTypes.wAccessKey
            }

        Private Shared ReadOnly FilterFunctions As IReadOnlyDictionary(Of IdentifierTypes, Func(Of IReadOnlyCollection(Of IWebElement), clsIdentifierMatchTarget, IReadOnlyCollection(Of IWebPage), IReadOnlyCollection(Of IWebElement)))
        Private Shared ReadOnly OrderedIdentifierTypes As IReadOnlyCollection(Of IdentifierTypes)

        Friend WithEvents MyWebPageProvider As IWebPageProvider

        Public Event OnWebPageCreated As WebPageCreatedDelegate Implements IBrowserAutomationIdentifierHelper.OnWebPageCreated

        Shared Sub New()

            Dim filterMethodInfos =
                    GetType(IdentifierTypes).
                    Map(AddressOf [Enum].GetValues).
                    Cast(Of IdentifierTypes).
                    Select(Function(x) New With {.Type = x, .Method = GetFilterMethodForIdentifierType(x)}).
                    Where(Function(x) x.Method IsNot Nothing).
                    ToList()

            FilterFunctions =
                filterMethodInfos.
                    ToDictionary(Function(k) k.Type, Function(v) GetFilterCallFunction(v.Method))

            OrderedIdentifierTypes =
                filterMethodInfos.
                    Select(Function(x) New With {x.Type, .OrderAttribute = x.Method.GetCustomAttribute(Of FilterOrderAttribute)()}).
                    Select(Function(x) New With {x.Type, .Order = If(x.OrderAttribute?.Order, Integer.MaxValue)}).
                    OrderBy(Function(x) x.Order).
                    Select(Function(x) x.Type).
                    ToList()

        End Sub

        Public Sub New(webPageProvider As IWebPageProvider)
            MyWebPageProvider = webPageProvider
        End Sub

        Public Function GetIdentifiers(element As IWebElement) _
            As IReadOnlyCollection(Of String) _
            Implements IBrowserAutomationIdentifierHelper.GetIdentifiers

            Dim identifiers = New List(Of String)()

            AddIdentifiersIfBoundsValid(identifiers, element.GetBounds(), IdentifierTypes.wX, IdentifierTypes.wY, IdentifierTypes.wWidth, IdentifierTypes.wHeight)
            AddIdentifiersIfBoundsValid(identifiers, element.GetClientBounds(), IdentifierTypes.wClientX, IdentifierTypes.wClientY, IdentifierTypes.wClientWidth, IdentifierTypes.wClientHeight)
            AddIdentifiersIfBoundsValid(identifiers, element.GetOffsetBounds(), IdentifierTypes.wOffsetX, IdentifierTypes.wOffsetY, IdentifierTypes.wOffsetWidth, IdentifierTypes.wOffsetHeight)
            AddIdentifiersIfBoundsValid(identifiers, element.GetScrollBounds(), IdentifierTypes.wScrollX, IdentifierTypes.wScrollY, IdentifierTypes.wScrollWidth, IdentifierTypes.wScrollHeight)

            Dim addIfNotEmpty =
                Sub(identifier As IdentifierTypes, value As String)
                    If Not String.IsNullOrEmpty(value) Then
                        identifiers.Add(GetIdentifier(identifier, value))
                    End If
                End Sub

            addIfNotEmpty(IdentifierTypes.wId, element.GetElementId())
            addIfNotEmpty(IdentifierTypes.wName, element.GetName())

            identifiers.AddRange({
                GetIdentifier(IdentifierTypes.wElementType, element.GetElementType()),
                GetIdentifier(IdentifierTypes.wXPath, element.GetPath()),
                GetIdentifier(IdentifierTypes.wCssSelector, String.Empty),
                GetIdentifier(IdentifierTypes.wValue, element.GetValue()),
                GetIdentifier(IdentifierTypes.wPageAddress, element.Page.GetAddress()),
                GetIdentifier(IdentifierTypes.wClass, element.GetClass()),
                GetIdentifier(IdentifierTypes.wChildCount, element.GetChildCount()),
                GetIdentifier(IdentifierTypes.wIsEditable, element.GetIsEditable()),
                GetIdentifier(IdentifierTypes.wStyle, element.GetStyle()),
                GetIdentifier(IdentifierTypes.wTabIndex, element.GetTabIndex()),
                GetIdentifier(IdentifierTypes.wInputType, element.GetAttribute("type")),
                GetIdentifier(IdentifierTypes.wAccessKey, element.GetAccessKey()),
                GetIdentifier(IdentifierTypes.wInnerText, element.GetText()),
                GetIdentifier(IdentifierTypes.wSource, element.GetAttribute("src")),
                GetIdentifier(IdentifierTypes.wTargetAddress, element.GetAttribute("href")),
                GetIdentifier(IdentifierTypes.wAlt, element.GetAttribute("alt")),
                GetIdentifier(IdentifierTypes.wPattern, element.GetAttribute("pattern")),
                GetIdentifier(IdentifierTypes.wRel, element.GetAttribute("rel")),
                GetIdentifier(IdentifierTypes.wLinkTarget, element.GetAttribute("target")),
                GetIdentifier(IdentifierTypes.wPlaceholder, element.GetAttribute("placeholder"))
            })

            Return identifiers

        End Function

        Public Function FindSingleElement(query As clsQuery) As IWebElement Implements IBrowserAutomationIdentifierHelper.FindSingleElement

            Dim matches = FindElements(query)

            If Not matches.Any() Then Throw New NoSuchElementException(
                My.Resources.NoElementMatchedTheQueryTerms)

            If matches.Count > 1 Then Throw New TooManyElementsException(
                My.Resources.MoreThanOneElementMatchedTheQueryTerms)

            Return matches.Single()

        End Function

        Public Function GetActiveWebPages() As IReadOnlyCollection(Of IWebPage) Implements IBrowserAutomationIdentifierHelper.GetActiveWebPages
            Return MyWebPageProvider.GetActiveWebPages(String.Empty)
        End Function

        Public Sub SetProcHandle(procHandle As IntPtr) Implements IBrowserAutomationIdentifierHelper.SetProcHandle
            MyWebPageProvider.SetProcHandle(procHandle)
        End Sub

        Public Function FindElements(query As clsQuery) As IReadOnlyCollection(Of IWebElement) _
            Implements IBrowserAutomationIdentifierHelper.FindElements

            Dim pages = GetWebPages(query)

            Dim elements =
                query.Identifiers.
                    Select(Function(x) x.Identifier).
                    Map(AddressOf OrderedIdentifierTypes.Intersect).
                    Select(AddressOf query.GetIdentifier).
                    Aggregate(Nothing, GetFilterFunction(pages))

            If elements.Any() Then
                'a find may return empty elements from related pages (plugin returns as empty guid - uuidZero)
                Dim uuidZero = Guid.Empty.ToString
                Dim result = From x In elements Where x.Id.ToString <> uuidZero
                If result.Count > 0 Then
                    elements = CType(result.ToList, IReadOnlyCollection(Of IWebElement))
                End If
            End If

            Dim matchIndex = query.GetIntParam(clsQuery.ParameterNames.MatchIndex)

            If matchIndex <= 0 Then Return elements

            If matchIndex > elements.Count Then
                Return New IWebElement() {}
            End If

            Return {elements.ElementAt(matchIndex - 1)}

        End Function

        Public Function CheckParentDocumentLoaded() As Boolean Implements IBrowserAutomationIdentifierHelper.CheckParentDocumentLoaded
            Return True
        End Function

        Private Shared Function GetFilterCallFunction(method As MethodInfo) As Func(Of IReadOnlyCollection(Of IWebElement), clsIdentifierMatchTarget, IReadOnlyCollection(Of IWebPage), IReadOnlyCollection(Of IWebElement))

            Return _
                Function(elements, matchTarget, pages) _
                    DirectCast(method.Invoke(Nothing, {elements, matchTarget, pages}), IReadOnlyCollection(Of IWebElement))

        End Function

        Private Shared Function GetFilterMethodForIdentifierType(identifierType As IdentifierTypes) As MethodInfo

            Return _
                GetType(BrowserAutomationIdentifierHelper).
                    GetMethod(
                        $"Filter_{identifierType.ToString()}",
                        BindingFlags.Static Or BindingFlags.NonPublic,
                        Nothing,
                        {GetType(IReadOnlyCollection(Of IWebElement)), GetType(clsIdentifierMatchTarget), GetType(IReadOnlyCollection(Of IWebPage))},
                        Nothing)

        End Function

        Private Shared Function GetIdentifier(
            identifierType As IdentifierTypes,
            value As Object) _
            As String

            Return _
                If(DefaultIdentifierTypes.Contains(identifierType), "+", "") &
                $"{identifierType}={clsQuery.EncodeValue(value)}"

        End Function


        Private Shared Function GetFilterFunction(pages As IReadOnlyCollection(Of IWebPage)) As Func(Of IReadOnlyCollection(Of IWebElement), clsIdentifierMatchTarget, IReadOnlyCollection(Of IWebElement))

            Return _
                Function(elements, matchTarget) _
                    If(FilterFunctions.ContainsKey(matchTarget.Identifier),
                        FilterFunctions(matchTarget.Identifier)(elements, matchTarget, pages),
                        elements)

        End Function

        Private Shared Sub AddIdentifiersIfBoundsValid(
            identifiers As List(Of String),
            bounds As Rectangle,
            xIdentifier As IdentifierTypes,
            yIdentifier As IdentifierTypes,
            widthIdentifier As IdentifierTypes,
            heightIdentifier As IdentifierTypes)

            If bounds.X <> -1 OrElse bounds.Y <> -1 Then
                identifiers.AddRange({
                    GetIdentifier(xIdentifier, bounds.Left),
                    GetIdentifier(yIdentifier, bounds.Top),
                    GetIdentifier(widthIdentifier, bounds.Width),
                    GetIdentifier(heightIdentifier, bounds.Height)
                })
            End If

        End Sub

        Public Function GetWebPages(query As clsQuery) As IReadOnlyCollection(Of IWebPage) Implements IBrowserAutomationIdentifierHelper.GetWebPages
            Dim tryGetAddress =
                    Function(page As IWebPage)
                        Try
                            Return New With {page, .Address = page.GetAddress()}
                        Catch ex As TimeoutException
                            Return Nothing
                        End Try
                    End Function

            Dim tryGetTrackingId = If(query.GetParameter("trackingid"), "")
            Return _
                MyWebPageProvider.GetActiveWebPages(tryGetTrackingId).
                    AsParallel().
                    Select(Function(x) tryGetAddress(x)).
                    Where(Function(x) x IsNot Nothing).
                    Where(Function(x) If(query.Identifier(IdentifierTypes.wPageAddress)?.IsMatch(x.Address), True)).
                    Select(Function(x) x.page).
                    ToList()
        End Function

        Public Sub DetachTrackedWebPage(trackingId As String) Implements IBrowserAutomationIdentifierHelper.DetachTrackedWebPage
            MyWebPageProvider.DetachTrackingId(trackingId)
        End Sub

        Public Sub DetachAllTrackedPages() Implements IBrowserAutomationIdentifierHelper.DetachAllTrackedPages
            MyWebPageProvider.DetachAllTrackedPages()
        End Sub
        Public Function IsTracking() As Boolean Implements IBrowserAutomationIdentifierHelper.IsTracking
            Return MyWebPageProvider.IsTracking()
        End Function

        Public Function IsTracking(trackingId As String) As Boolean Implements IBrowserAutomationIdentifierHelper.IsTracking
            Return MyWebPageProvider.IsTracking(trackingId)
        End Function

        Public Sub RemoveWebPage(pageId As Guid) Implements IBrowserAutomationIdentifierHelper.RemoveWebPage
            MyWebPageProvider.DisconnectPage(pageId)
        End Sub

        Private Sub TrackedPagesDetached(sender As Object, args As TrackingIdDetachedEventArgs) Handles MyWebPageProvider.OnTrackingIdDetached
            If args?.TrackingId IsNot Nothing Then
                ActiveMessagingHost()?.Detach(args.TrackingId)
            End If
        End Sub
        ' ReSharper disable UnusedMember.Local
        <FilterOrder(0)>
        Private Shared Function Filter_wXPath(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)

            If elements IsNot Nothing Then
                Throw New InvalidOperationException(My.Resources.NoElementsShouldBePassedWhenLookingForElementsByXPath)
            End If

            Return _
                pages.
                SelectMany(GetElementsByXPathForPage(matchTarget)).
                Where(Function(x) x IsNot Nothing).
                ToList()

        End Function

        Private Shared Function GetElementsByXPathForPage(matchTarget As clsMatchTarget) As Func(Of IWebPage, IReadOnlyCollection(Of IWebElement))

            Return _
                Function(page)
                    If matchTarget.ComparisonType = clsMatchTarget.ComparisonTypes.Equal Then
                        Return {page.GetElementByPath(matchTarget.MatchValue)}
                    ElseIf matchTarget.ComparisonType = clsMatchTarget.ComparisonTypes.Wildcard Then
                        Return GetElementsByWildcardXPath(page, matchTarget)
                    Else
                        Return GetElementsByXPathMatch(page, matchTarget)
                    End If
                End Function

        End Function

        Private Shared Function GetElementsByWildcardXPath(page As IWebPage, matchTarget As clsMatchTarget) As IReadOnlyCollection(Of IWebElement)

            Dim xPath = matchTarget.MatchValue

            If Not xPath.Contains("*"c) Then
                Return {page.GetElementByPath(xPath)}
            End If

            xPath = xPath.Substring(0, xPath.IndexOf("*"c))

            If xPath.Contains("/"c) Then
                xPath = xPath.Substring(0, xPath.LastIndexOf("/"c))
            End If

            Return If(
                page.GetElementByPath(xPath)?.
                    GetDescendants().
                    Where(Function(x) matchTarget.IsMatch(x.GetPath())).
                    ToList(),
                New List(Of IWebElement)())

        End Function

        Private Shared Function GetElementsByXPathMatch(page As IWebPage, matchTarget As clsMatchTarget) As IReadOnlyCollection(Of IWebElement)
            Return _
                page.GetRootElement().
                    GetDescendants().
                    Where(Function(x) matchTarget.IsMatch(x.GetPath())).
                    ToList()
        End Function

        ' ReSharper disable UnusedMember.Local
        <FilterOrder(0)>
        Private Shared Function Filter_wCssSelector(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)

            If elements IsNot Nothing Then
                Throw New InvalidOperationException(My.Resources.NoElementsShouldBePassedWhenLookingForElementsByCssSelector)
            End If

            Return pages.
                SelectMany(GetElementsByCssSelectorForPage(matchTarget)).
                Where(Function(x) x IsNot Nothing).
                ToList()

        End Function

        Private Shared Function GetElementsByCssSelectorForPage(matchTarget As clsMatchTarget) As Func(Of IWebPage, IReadOnlyCollection(Of IWebElement))

            Return Function(page)
                       Return {page.GetElementByCssSelector(matchTarget.MatchValue)}
                   End Function

        End Function

        <FilterOrder(1)>
        Private Shared Function Filter_wId(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetElementId(), Function(x) x.GetElementById(matchTarget.MatchValue))
        End Function

        <FilterOrder(2)>
        Private Shared Function Filter_wClass(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetClass(), Function(x) x.GetElementsByClass(matchTarget.MatchValue))
        End Function

        Private Shared Function Filter_wName(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetName(), Function(x) x.GetElementsByName(matchTarget.MatchValue))
        End Function

        Private Shared Function Filter_wElementType(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetElementType(), Function(x) x.GetElementsByType(matchTarget.MatchValue))
        End Function

        Private Shared Function Filter_wX(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetBounds().Left)
        End Function

        Private Shared Function Filter_wY(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetBounds().Top)
        End Function

        Private Shared Function Filter_wWidth(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetBounds().Width)
        End Function

        Private Shared Function Filter_wHeight(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetBounds().Height)
        End Function

        Private Shared Function Filter_wClientX(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetClientBounds().Left)
        End Function

        Private Shared Function Filter_wClientY(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetClientBounds().Top)
        End Function

        Private Shared Function Filter_wClientWidth(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetClientBounds().Width)
        End Function

        Private Shared Function Filter_wClientHeight(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetClientBounds().Height)
        End Function

        Private Shared Function Filter_wOffsetX(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetOffsetBounds().Left)
        End Function

        Private Shared Function Filter_wOffsetY(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetOffsetBounds().Top)
        End Function

        Private Shared Function Filter_wOffsetWidth(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetOffsetBounds().Width)
        End Function

        Private Shared Function Filter_wOffsetHeight(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetOffsetBounds().Height)
        End Function

        Private Shared Function Filter_wScrollX(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetScrollBounds().Left)
        End Function

        Private Shared Function Filter_wScrollY(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetScrollBounds().Top)
        End Function

        Private Shared Function Filter_wScrollWidth(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetScrollBounds().Width)
        End Function

        Private Shared Function Filter_wScrollHeight(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetScrollBounds().Height)
        End Function

        Private Shared Function Filter_wChildCount(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetChildCount())
        End Function

        Private Shared Function Filter_wIsEditable(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetIsEditable())
        End Function

        Private Shared Function Filter_wStyle(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetStyle())
        End Function

        Private Shared Function Filter_wTabIndex(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetTabIndex())
        End Function

        Private Shared Function Filter_wAccessKey(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAccessKey())
        End Function

        Private Shared Function Filter_wInnerText(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetText())
        End Function

        Private Shared Function Filter_wSource(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAttribute("src"))
        End Function

        Private Shared Function Filter_wTargetAddress(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAttribute("href"))
        End Function

        Private Shared Function Filter_wAlt(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAttribute("alt"))
        End Function

        Private Shared Function Filter_wPattern(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAttribute("pattern"))
        End Function

        Private Shared Function Filter_wRel(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAttribute("rel"))
        End Function

        Private Shared Function Filter_wLinkTarget(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAttribute("target"))
        End Function

        Private Shared Function Filter_wPlaceholder(elements As IReadOnlyCollection(Of IWebElement), matchTarget As clsMatchTarget, pages As IReadOnlyCollection(Of IWebPage)) As IReadOnlyCollection(Of IWebElement)
            Return FilterElements(elements, pages, matchTarget, Function(x) x.GetAttribute("placeholder"))
        End Function
        ' ReSharper restore UnusedMember.Local

        Private Shared Function FilterElements(
                    elements As IReadOnlyCollection(Of IWebElement),
                    pages As IReadOnlyCollection(Of IWebPage),
                    matchTarget As clsMatchTarget,
                    filterWith As Func(Of IWebElement, IConvertible)) _
                As IReadOnlyCollection(Of IWebElement)

            Dim searchElements = If(elements, pages.SelectMany(Function(x) x.GetRootElement().GetDescendants()))
            Return searchElements.Where(IsMatch(matchTarget, filterWith)).ToList()

        End Function

        Private Shared Function IsMatch(matchTarget As clsMatchTarget, filterWith As Func(Of IWebElement, IConvertible)) As Func(Of IWebElement, Boolean)

            Return Function(element) _
                If(element.Map(filterWith)?.Map(AddressOf matchTarget.IsMatch), False)

        End Function

        Private Shared Function FilterElements(
                    elements As IReadOnlyCollection(Of IWebElement),
                    pages As IReadOnlyCollection(Of IWebPage),
                    matchTarget As clsMatchTarget,
                    filterWith As Func(Of IWebElement, IConvertible),
                    retrieveFromOnEqualsComparison As Func(Of IWebPage, IWebElement)) _
                As IReadOnlyCollection(Of IWebElement)

            Return FilterElements(elements, pages, matchTarget, filterWith, Function(x) {retrieveFromOnEqualsComparison(x)})

        End Function

        Private Shared Function FilterElements(
                    elements As IReadOnlyCollection(Of IWebElement),
                    pages As IReadOnlyCollection(Of IWebPage),
                    matchTarget As clsMatchTarget,
                    filterWith As Func(Of IWebElement, IConvertible),
                    retrieveFromOnEqualsComparison As Func(Of IWebPage, IEnumerable(Of IWebElement))) _
                As IReadOnlyCollection(Of IWebElement)

            If elements IsNot Nothing Then
                Return elements.Where(IsMatch(matchTarget, filterWith)).ToList()
            End If

            If matchTarget.ComparisonType = clsMatchTarget.ComparisonTypes.Equal Then
                Return pages.SelectMany(retrieveFromOnEqualsComparison).ToList()
            End If

            Return _
                pages.
                    SelectMany(Function(x) x.GetRootElement().GetDescendants()).
                    Where(IsMatch(matchTarget, filterWith)).
                    ToList()

        End Function

        Private Sub OnWebPageProviderWebPageCreated(sender As Object, e As WebPageCreatedEventArgs) Handles MyWebPageProvider.OnWebPageCreated
            RaiseEvent OnWebPageCreated(sender, e)
        End Sub

        Public Function ActiveMessagingHost() As IWebPage Implements IBrowserAutomationIdentifierHelper.ActiveMessagingHost
            Return MyWebPageProvider.ActiveMessagingHost
        End Function

        Public Sub CloseActivePages() Implements IBrowserAutomationIdentifierHelper.CloseActivePages
            MyWebPageProvider.CloseActivePages()
        End Sub

        Private Class FilterOrderAttribute
            Inherits Attribute

            Public ReadOnly Property Order As Integer

            Public Sub New(order As Integer)
                Me.Order = order
            End Sub

        End Class
    End Class
End Namespace
