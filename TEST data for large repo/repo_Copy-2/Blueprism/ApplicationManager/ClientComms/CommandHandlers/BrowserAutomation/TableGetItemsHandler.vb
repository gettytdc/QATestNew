Imports System.Xml
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.ApplicationManager.BrowserAutomation
Imports BluePrism.ApplicationManager.CommandHandlers.BrowserAutomation.Shared
Imports BluePrism.ApplicationManager.CommandHandling
Imports BluePrism.BrowserAutomation

Namespace CommandHandlers.BrowserAutomation

    <CommandId("WebGetTableItems")>
    Friend Class TableGetItemsHandler : Inherits BrowserAutomationHandlerBase

        Public Sub New(application As ILocalTargetApp, identifierHelper As IBrowserAutomationIdentifierHelper)
            MyBase.New(application, identifierHelper)
        End Sub

        Protected Overrides Function Execute(element As IWebElement, context As CommandContext) As Reply

            Dim lastRowNumber = element.GetRowCount()

            ' Create XML from rows
            Dim xdoc As New XmlDocument()
            Dim collection As XmlElement = xdoc.CreateElement("collection")
            xdoc.AppendChild(collection)

            For rowIndex = 0 To lastRowNumber - 1
                Dim row = xdoc.CreateElement("row")
                collection.AppendChild(row)

                Dim lastColumnNumber = element.GetColumnCount(rowIndex)

                For columnIndex = 0 To lastColumnNumber - 1

                    Dim cellText = element.GetTableItemText(rowIndex, columnIndex)

                    Dim el As XmlElement = xdoc.CreateElement("field")
                    el.SetAttribute("type", "text")
                    el.SetAttribute("value", cellText)
                    el.SetAttribute("name", "Column" & (columnIndex + 1))

                    row.AppendChild(el)
                Next
            Next

            Return Reply.Result(xdoc.OuterXml)
        End Function

    End Class
End Namespace