Imports BluePrism.Core
Imports System.Linq
Imports System.Text
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports HtmlAgilityPack
Imports BluePrism.Core.Utility

Public Class ChromiumHtmlDocument
    Private Const TextNodeName As String = "#TEXT"

    Private ReadOnly mDocument As HtmlDocument = New HtmlDocument()

    Public Sub New(htmlString As String)
        mDocument.LoadHtml(htmlString)
    End Sub

    Public Function GenerateNavigatorString() As String
        Dim sb = New StringBuilder()

        NavigatorStringForNode(mDocument.DocumentNode, sb, 0, 2, " "c, "WEB:")

        Return sb.ToString()
    End Function

    Private Shared Sub NavigatorStringForNode(node As HtmlNode, ByRef sb As StringBuilder, indentLevel As Integer, indentInc As Integer, indentChar As Char, prefix As String)

        If node.Name.ToUpperInvariant = TextNodeName Then
            Return
        End If

        sb.Append(indentChar, indentLevel)
        sb.Append(prefix)
        AppendIdentifiers(sb, node)
        sb.AppendLine()
        For Each childNode In node.ChildNodes
            NavigatorStringForNode(childNode, sb, indentLevel + indentInc, indentInc, indentChar, prefix)
            sb.AppendLine()
        Next

    End Sub

    Private Shared Sub AppendIdentifiers(ByRef sb As StringBuilder, node As HtmlNode)
        sb.Append("+wXPath=").Append(clsQuery.EncodeValue(node.XPath.ToUpperInvariant)).
            Append(" wClass=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "class"))).
            Append(" +wId=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "id"))).
            Append(" wElementType=").Append(clsQuery.EncodeValue(node.Name.ToUpperInvariant())).
            Append(" wInputType=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "type"))).
            Append(" wChildCount=").Append(clsQuery.EncodeValue(GetChildCount(node))).
            Append(" wTargetAddress=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "href"))).
            Append(" wAlt=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "alt"))).
            Append(" wPattern=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "pattern"))).
            Append(" wRel=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "rel"))).
            Append(" wLinkTarget=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "target"))).
            Append(" wPlaceholder=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "placeholder"))).
            Append(" wSource=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "src"))).
            Append(" wValue=").Append(clsQuery.EncodeValue(GetAttributeValue(node, "value"))).
            Append(" MatchIndex=1")
    End Sub

    Private Shared Function GetAttributeValue(node As HtmlNode, attributeName As String) As String
        If Not node.HasAttributes Then
            Return String.Empty
        End If

        Return If(node.Attributes.FirstOrDefault(Function(a) String.Equals(a.Name, attributeName, StringComparison.CurrentCultureIgnoreCase))?.Value, String.Empty)

    End Function

    Private Shared Function GetChildCount(node As HtmlNode) As Integer

        Return node.ChildNodes.Count - node.ChildNodes.Where(Function(n) n.Name.ToUpperInvariant = TextNodeName).Count

    End Function

End Class
