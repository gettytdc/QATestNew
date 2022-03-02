''' <summary>
''' Generates a document in wiki format. Made overridable so that different
''' implementations can be created.
''' </summary>
''' <remarks>To use this class:
''' 1) Call constructor.
''' 2) Call BeginDocument
''' 3) Call CreateHeader, CreateParagraph, etc as desired
''' 4) Call EndDocument
''' 5) Call ToString to get document text.</remarks>
Public Class clsWikiDocumentBuilder

    ''' <summary>
    ''' The internal mechanism for building the document
    ''' </summary>
    Protected mStringBuilder As System.Text.StringBuilder

    Public Sub New()
        mStringBuilder = New System.Text.StringBuilder
    End Sub

    ''' <summary>
    ''' Begins the document, using the specified title.
    ''' </summary>
    ''' <param name="DocumentTitle">The title of the document.</param>
    ''' <remarks>It is not acceptable to simply use CreateHeader
    ''' instead, since some implementations may have some other initialisation
    ''' to do in this method as well.</remarks>
    Public Overridable Sub BeginDocument(ByVal DocumentTitle As String)
        CreateHeader(DocumentTitle, 1)
    End Sub

    ''' <summary>
    ''' Creates a header with the specified level (ie importance).
    ''' </summary>
    ''' <param name="HeaderText">The text which comprises the header</param>
    ''' <param name="Level">The level as an integer from 1 to 5,
    ''' inclusive. The lower the number the more important (ie larger)
    ''' the header.</param>
    Public Overridable Sub CreateHeader(ByVal HeaderText As String, ByVal Level As Integer, Optional ByVal HeaderID As String = "")

        If Level < 1 OrElse Level > 5 Then
            Throw New ArgumentOutOfRangeException(NameOf(Level), My.Resources.RequiredRangeIsFrom1To5Inclusive)
        End If

        If Level < 3 Then
            CreateNamedAnchor(clsHTMLDocumentBuilder.EspapeHeaderCharacters(HeaderID))
        End If

        Dim HeaderSymbol As String = String.Empty
        For i As Integer = 1 To Level
            HeaderSymbol &= "="
        Next
        mStringBuilder.Append(vbCrLf & HeaderSymbol & HeaderText & HeaderSymbol)
    End Sub

    ''' <summary>
    ''' Creates a named anchor with the specified ID.
    ''' </summary>
    ''' <param name="AnchorID">The ID of the anchor.</param>
    ''' <remarks>Named anchors allow internal links within
    ''' a document in wiki and html documents.</remarks>
    Protected Sub CreateNamedAnchor(ByVal AnchorID As String)
        'Make sure that the anchor id is unique amongst
        'existing IDs
        Static Dim AnchorsList As Dictionary(Of String, String)
        If AnchorsList Is Nothing Then AnchorsList = New Dictionary(Of String, String)
        If AnchorsList.ContainsKey(AnchorID) Then
            Dim i As Integer = 0
            While AnchorsList.ContainsKey(AnchorID & i.ToString)
                i += 1
            End While
            AnchorID = AnchorID & i.ToString
        End If
        If Not String.IsNullOrEmpty(AnchorID) Then AnchorsList.Add(AnchorID, AnchorID)

        'Append the anchor
        If Not String.IsNullOrEmpty(AnchorID) Then
            Dim xdoc As New Xml.XmlDocument
            Dim e As Xml.XmlElement = xdoc.CreateElement("span")
            e.SetAttribute("id", AnchorID)
            e.InnerText = " "
            mStringBuilder.Append(vbCrLf & vbCrLf & e.OuterXml)
        End If
    End Sub

    ''' <summary>
    ''' Creates a paragraph containing the specified text.
    ''' </summary>
    ''' <param name="Text">The text to be appended to the document.</param>
    Public Overridable Sub AppendParagraph(ByVal Text As String)
        mStringBuilder.Append(vbCrLf & Text & vbCrLf)
    End Sub

    ''' <summary>
    ''' Creates a link to another document, or another part of this document.
    ''' </summary>
    ''' <param name="Destination">The destination of the link.</param>
    ''' <param name="LinkText">The text to be displayed in the link.</param>
    Public Overridable Sub CreateLink(ByVal Destination As String, ByVal LinkText As String)
        mStringBuilder.Append("[[" & Destination & "|" & LinkText & "]]")
    End Sub

    ''' <summary>
    ''' Appends the supplied text without any processing at all.
    ''' </summary>
    ''' <param name="Text">The text to insert.</param>
    ''' <remarks>No espaping of special characters will be performed;
    ''' it is up to the caller to ensure that this does not affect
    ''' the validity of the document.</remarks>
    Public Overridable Sub AppendLiteralText(ByVal Text As String)
        mStringBuilder.Append(Text)
    End Sub

    ''' <summary>
    ''' Ends the document.
    ''' </summary>
    ''' <remarks>This method must be called before the document is
    ''' output. No further text can be appended after this call.</remarks>
    Public Overridable Sub EndDocument()
        mStringBuilder.Append(vbCrLf)
    End Sub


    Public Overridable Sub BeginTable(ByVal TableCaption As String)
        mStringBuilder.Append(vbCrLf & "{| border=""1"" cellpadding=""0"" cellspacing=""0"" style=""width: 100%"" class=""wikitable"" |+" & TableCaption)
    End Sub

    Public Overridable Sub BeginTableRow(Optional ByVal ClassName As String = "")
        mStringBuilder.Append(vbCrLf & "|-")
        If Not String.IsNullOrEmpty(ClassName) Then mStringBuilder.Append(" class=""" & ClassName & """")
    End Sub

    Public Overridable Sub EndTableRow()
        'Do nothing
    End Sub

    Public Overridable Sub AppendTableHeader(ByVal HeaderText As String)
        mStringBuilder.Append(vbCrLf & "!" & HeaderText)
    End Sub


    Public Overridable Sub EndTable()
        mStringBuilder.Append(vbCrLf & "|}")
    End Sub


    Public Overridable Sub BeginList()
        'do nothing
    End Sub

    Public Overridable Sub AppendListItem(ByVal ItemText As String)
        mStringBuilder.Append(vbCrLf & "* " & ItemText)
    End Sub

    Public Overridable Sub BeginListItem()
        mStringBuilder.Append(vbCrLf & "*")
    End Sub

    Public Overridable Sub EndListItem()
        'do nothing
    End Sub

    Public Overridable Sub EndList()
        'do nothing
    End Sub

    Public Enum HorizontalAlignment
        Left
        Right
        Center
    End Enum

    Public Overridable Sub AppendTableData(ByVal Data As String, Optional ByVal Alignment As HorizontalAlignment = HorizontalAlignment.Left, Optional ByVal ClassName As String = "")
        mStringBuilder.Append(vbCrLf & "|")
        If Not String.IsNullOrEmpty(ClassName) Then mStringBuilder.Append(" class=""" & ClassName & """""")
        mStringBuilder.Append(" align=""" & Alignment.ToString.ToLower & """| " & Data)
    End Sub

    Public Overridable Sub BeginTableData()
        mStringBuilder.Append("|")
    End Sub

    Public Overridable Sub EndTableData()
        'do nothing
    End Sub

    ''' <summary>
    ''' Gets the text generated for the document.
    ''' </summary>
    Public Overrides Function ToString() As String
        Return mStringBuilder.ToString
    End Function

End Class

''' <summary>
''' Generates an html document.
''' </summary>
Public Class clsHTMLDocumentBuilder
    Inherits clsWikiDocumentBuilder


    Public Overrides Sub BeginDocument(ByVal DocumentTitle As String)
        mStringBuilder.AppendLine("<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN""")
        mStringBuilder.AppendLine(vbTab & """http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">")
        mStringBuilder.AppendLine("<html xmlns=""http://www.w3.org/1999/xhtml"">")
        mStringBuilder.AppendLine("<head>")
        mStringBuilder.AppendLine(vbTab & "<link type=""text/css"" href=""AutomateHelp.css"" rel=""stylesheet"" />")
        mStringBuilder.AppendLine(vbTab & "<meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"" />")
        mStringBuilder.AppendLine(vbTab & "<title>" & DocumentTitle & "</title>")
        mStringBuilder.AppendLine("</head>")
        mStringBuilder.AppendLine("<body>")
        mStringBuilder.AppendLine("<!-- START: Documentation Auto-generated by clsAMI.GetDocumentation -->")

        CreateHeader(DocumentTitle, 1)
    End Sub


    Public Overrides Sub CreateHeader(ByVal HeaderText As String, ByVal Level As Integer, Optional ByVal HeaderID As String = "")

        If Level < 1 OrElse Level > 5 Then
            Throw New ArgumentOutOfRangeException(NameOf(Level), My.Resources.RequiredRangeIsFrom1To5Inclusive)
        End If

        Dim xdoc As New Xml.XmlDocument
        Dim e As Xml.XmlElement

        If Level < 3 Then
            CreateNamedAnchor(EspapeHeaderCharacters(HeaderID))
        End If

        'Create the header itself
        e = xdoc.CreateElement("h" & Level.ToString)
        e.InnerText = HeaderText
        mStringBuilder.AppendLine(e.OuterXml)
    End Sub

    Public Overrides Sub CreateLink(ByVal Destination As String, ByVal LinkText As String)
        Dim xdoc As New Xml.XmlDocument
        Dim e As Xml.XmlElement = xdoc.CreateElement("a")
        e.InnerText = LinkText
        e.SetAttribute("href", Destination)

        mStringBuilder.Append(e.OuterXml)
    End Sub


    Public Overrides Sub AppendParagraph(ByVal Text As String)
        mStringBuilder.AppendLine("<p>")
        mStringBuilder.AppendLine(Text)
        mStringBuilder.Append("</p>")
    End Sub

    Public Overrides Sub EndDocument()
        mStringBuilder.AppendLine(vbCrLf & "<!-- END: Generated block -->")
        mStringBuilder.Append("</body>" & vbCrLf & "</html>")
    End Sub


    Public Overrides Sub BeginTable(ByVal TableCaption As String)
        mStringBuilder.AppendLine(vbCrLf & "<table border=""1"" cellspacing=""0"" cellpadding=""0"" style=""width:100%"">")

        Dim xdoc As New Xml.XmlDocument
        Dim e As Xml.XmlElement = xdoc.CreateElement("caption")
        e.InnerText = TableCaption
        mStringBuilder.Append(e.OuterXml)
    End Sub

    Public Overrides Sub BeginTableRow(Optional ByVal ClassName As String = "")
        Dim ClassString As String = String.Empty
        If Not String.IsNullOrEmpty(ClassName) Then
            ClassString = " class=""" & ClassName & """"
        End If
        mStringBuilder.Append(vbCrLf & "<tr" & ClassString & ">")
    End Sub

    Public Overrides Sub EndTableRow()
        mStringBuilder.Append(vbCrLf & "</tr>")
    End Sub

    Public Overrides Sub AppendTableData(ByVal Data As String, Optional ByVal Alignment As clsWikiDocumentBuilder.HorizontalAlignment = clsWikiDocumentBuilder.HorizontalAlignment.Left, Optional ByVal ClassName As String = "")
        Dim xdoc As New Xml.XmlDocument
        Dim e As Xml.XmlElement = xdoc.CreateElement("td")
        If Not String.IsNullOrEmpty(ClassName) Then e.SetAttribute("class", ClassName)
        e.SetAttribute("align", Alignment.ToString.ToLower)
        e.InnerText = Data
        mStringBuilder.Append(vbCrLf & e.OuterXml)
    End Sub

    Public Overrides Sub BeginTableData()
        mStringBuilder.Append("<td>")
    End Sub

    Public Overrides Sub EndTableData()
        mStringBuilder.Append("</td>")
    End Sub

    Public Overrides Sub AppendTableHeader(ByVal HeaderText As String)
        mStringBuilder.Append(vbCrLf & "<th>" & HeaderText & "</th>")
    End Sub

    Public Overrides Sub EndTable()
        mStringBuilder.Append(vbCrLf & "</table>")
    End Sub

    Public Overrides Sub BeginList()
        mStringBuilder.Append(vbCrLf & "<ul>")
    End Sub

    Public Overrides Sub AppendListItem(ByVal ItemText As String)
        Dim xdoc As New Xml.XmlDocument
        Dim e As Xml.XmlElement = xdoc.CreateElement("li")
        e.InnerText = ItemText
        mStringBuilder.Append(vbCrLf & e.OuterXml)
    End Sub

    Public Overrides Sub BeginListItem()
        mStringBuilder.Append("<li>")
    End Sub

    Public Overrides Sub EndListItem()
        mStringBuilder.Append("</li>")
    End Sub
    Public Overrides Sub EndList()
        mStringBuilder.Append(vbCrLf & "</ul>")
    End Sub

    Public Shared Function EspapeHeaderCharacters(ByVal Text As String) As String
        Return Text.Replace(" ", "_").Replace("(", ".28").Replace(")", ".29").Trim
    End Function
End Class


