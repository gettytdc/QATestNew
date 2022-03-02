Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices

Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation.Shared
    ''' <summary>
    ''' Provides extension methods for generating XML from different objects
    ''' </summary>
    Friend Module XmlExtensionMethods

        ''' <summary>
        ''' Converts a collection of <see cref="IAutomationElement"/> objects to an XML collection.
        ''' </summary>
        ''' <param name="elements">The elements to convert.</param>
        ''' <returns>An XML representation of the given elements</returns>
        <Extension>
        Public Function AsCollectionXml(elements As IEnumerable(Of IAutomationElement)) As String

            Dim getElementValue =
                Function(e As IAutomationElement) _
                    If(If(e.GetCurrentPattern(Of IValuePattern)?.CurrentValue, e.CurrentName), String.Empty)

            Dim xml =
                <?xml version="1.0"?>
                <collection>
                    <%=
                        elements.Select(Function(element) _
                            <row>
                                <field name="Item Name" type="Text" value=<%= getElementValue(element) %>/>
                            </row>
                        )
                    %>
                </collection>

            Return xml.ToString()

        End Function

    End Module
End Namespace