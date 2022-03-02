Imports System.Xml
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation

    ''' <summary>
    ''' Gets the currently selected table rows in an xml collection.
    ''' </summary>
    <CommandId("UIATableGetSelectedRows")>
    Friend Class TableGetSelectedRowsHandler : Inherits UIAutomationHandlerBase

        ''' <summary>
        ''' The automation factory
        ''' </summary>
        Private ReadOnly mAutomationFactory As IAutomationFactory

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TableGetSelectedRowsHandler" /> class.
        ''' </summary>
        ''' <param name="application">The application.</param>
        ''' <param name="identifierHelper">The identifier helper.</param>
        ''' <param name="automationFactory">The automation factory.</param>
        Public Sub New(
                      application As ILocalTargetApp,
                      identifierHelper As IUIAutomationIdentifierHelper,
                      automationFactory As IAutomationFactory)
            MyBase.New(application, identifierHelper)

            mAutomationFactory = automationFactory
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Function Execute(element As IAutomationElement,
                                             context As CommandContext) As Reply

            Dim selectedRows = GridHelper.GetSelectedRowElements(element)

            Const EmptySelectionIndex = -1
            If selectedRows Is Nothing Then
                Return Reply.Result(EmptySelectionIndex)
            End If

            Dim gridPattern = element.EnsurePattern(Of IGridPattern)

            Dim columnCount = gridPattern.CurrentColumnCount

            ' Create XML from rows
            Dim xdoc As New XmlDocument()
            Dim collection As XmlElement = xdoc.CreateElement("collection")
            xdoc.AppendChild(collection)

            For Each row In selectedRows
                Dim pattern = row.EnsurePatternFromSubtree(Of IGridItemPattern)(
                             PatternType.GridItemPattern)
                Dim rowNumber = pattern.CurrentRow

                Dim xRow = xdoc.CreateElement("row")
                collection.AppendChild(xRow)
                For columnIndex = 0 To columnCount - 1
                    Dim cell = gridPattern.GetItem(rowNumber, columnIndex)
                    Dim cellText = GetInnerElementsText(cell, mAutomationFactory)

                    Dim el As XmlElement = xdoc.CreateElement("field")
                    el.SetAttribute("type", "text")
                    el.SetAttribute("value", cellText)
                    el.SetAttribute("name", "Column" & columnIndex + 1)

                    xRow.AppendChild(el)
                Next

            Next
            Return Reply.Result(xdoc.OuterXml)

        End Function
    End Class
End Namespace