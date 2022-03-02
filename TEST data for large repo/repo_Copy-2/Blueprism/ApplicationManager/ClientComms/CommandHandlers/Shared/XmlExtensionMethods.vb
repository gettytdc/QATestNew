Imports System.Collections.Generic
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Linq

Namespace CommandHandlers.Shared



    ''' <summary>
    ''' Provides extension methods for generating XML from different objects
    ''' </summary>
    Public Module XmlExtensionMethods

        <Extension>
        Public Function AsCollectionXML(rectangle As Rectangle) As String

            Dim xml =
                <?xml version="1.0"?>
                <collection>
                    <row>
                        <field name="Left" type="number" value=<%= rectangle.Left %>/>
                        <field name="Top" type="number" value=<%= rectangle.Top %>/>
                        <field name="Bottom" type="number" value=<%= rectangle.Bottom %>/>
                        <field name="Right" type="number" value=<%= rectangle.Right %>/>
                        <field name="Width" type="number" value=<%= rectangle.Width %>/>
                        <field name="Height" type="number" value=<%= rectangle.Height %>/>
                    </row>
                </collection>

            Return xml.ToString()
        End Function

        <Extension>
        Public Function AsCollectionXml(values As IEnumerable(Of String)) As String

            Dim xml =
                <?xml version="1.0"?>
                <collection>
                    <%= values.Select(Function(x) _
                        <row>
                            <field name="Value" type="Text" value=<%= x %>/>
                        </row>)
                    %>
                </collection>

            Return xml.ToString()

        End Function
    End Module
End Namespace