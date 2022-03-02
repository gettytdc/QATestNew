Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports BluePrism.BrowserAutomation
Imports BluePrism.BrowserAutomation.Data

Namespace CommandHandlers.BrowserAutomation.Shared
    Public Module XmlExtensionMethods
       
        <Extension>
        Public Function ValuesAsCollectionXml(webElements As IEnumerable(Of IWebElement)) As String
            Dim xml =
                <?xml version="1.0"?>
                <collection>
                    <%= webElements.Select(Function(element) _
                        <row>
                            <field name="Value" type="Text" value=<%= element.GetValue() %>/>
                        </row>
                    )%>
                </collection>

            Return xml.ToString()
        End Function

        <Extension>
        Public Function AsCollectionXml(items As IEnumerable(Of ListItem)) As String
            Dim xml = 
                <?xml version="1.0"?>
                <collection>
                    <%= items.Select(Function(x) _
                        <row>
                            <field name="Text" type="Text" value=<%= x.Text %>/>
                            <field name="Value" type="Text" value=<%= x.Value %>/>
                        </row>
                    )%>
                </collection>

            Return xml.ToString()
        End Function

    End Module
End NameSpace