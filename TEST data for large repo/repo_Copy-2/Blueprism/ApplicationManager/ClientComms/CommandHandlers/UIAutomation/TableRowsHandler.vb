Imports System.Xml
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Returns a collection containing data from some or all rows in the nearest
    ''' ancestor (or self) grid element.
    ''' Required parameters: Those required to uniquely identify the table plus optional
    ''' parameters 'firstrownumber' and 'lastrownumber' to limit the rows included in
    ''' the collection.
    ''' Result: """RESULT:&lt;xml&gt;"" where &lt;xml&gt; is the collections xml containing the items of the table."
    ''' </summary>
    <CommandId("UIATableRows")>
    Friend Class TableRowsHandler : Inherits UIAutomationHandlerBase

        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <inheritdoc/>
        Public Sub New(application As ILocalTargetApp,
                       identifierHelper As IUIAutomationIdentifierHelper,
                       automationFactory As IAutomationFactory)
            MyBase.New(application, identifierHelper)
            mAutomationFactory = automationFactory
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim gridPattern = element.EnsurePatternFromAncestors(Of IGridPattern)(
                True, PatternType.GridPattern)

            Dim columnCount = gridPattern.CurrentColumnCount
            Dim rowCount = gridPattern.CurrentRowCount
            Dim firstRowNumber = context.Query.GetIntParam(clsQuery.ParameterNames.FirstRowNumber)
            If firstRowNumber = 0 Then firstRowNumber = 1
            Dim lastRowNumber = context.Query.GetIntParam(clsQuery.ParameterNames.LastRowNumber)
            If lastRowNumber = 0 Then lastRowNumber = rowCount

            If lastRowNumber > rowCount Then
                Throw New NoSuchElementException($"The specified rows do not exist. Rows available: {rowCount}.")
            End If

            ' Create XML from rows
            Dim xdoc As New XmlDocument()
            Dim collection As XmlElement = xdoc.CreateElement("collection")
            xdoc.AppendChild(collection)

            For rowIndex = firstRowNumber - 1 To lastRowNumber - 1
                Dim row = xdoc.CreateElement("row")
                collection.AppendChild(row)
                For columnIndex = 0 To columnCount - 1
                    Dim cell = gridPattern.GetItem(rowIndex, columnIndex)
                    Dim cellText = GetInnerElementsText(cell, mAutomationFactory)

                    Dim el As XmlElement = xdoc.CreateElement("field")
                    el.SetAttribute("type", "text")
                    el.SetAttribute("value", cellText)
                    el.SetAttribute("name", "Column" & columnIndex + 1)

                    row.AppendChild(el)
                Next
            Next

            Return Reply.Result(xdoc.OuterXml)
        End Function
    End Class
End Namespace

