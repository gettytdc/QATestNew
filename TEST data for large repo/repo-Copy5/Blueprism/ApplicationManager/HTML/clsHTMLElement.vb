Option Strict On
Imports System.Text
Imports System.IO
Imports BluePrism.BPCoreLib
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities


''' Project  : ApplicationManagerUtilities
''' Class    : clsHTMLElement
''' 
''' <summary>
''' This class represents an HTML element.
''' </summary>
<CLSCompliant(False)> _
Public Class clsHTMLElement

    ''' <summary>
    ''' The mshtml.IHTMLElement that this instance represents.
    ''' </summary>
    Private mElement As mshtml.IHTMLElement

    ''' <summary>
    ''' Holds a reference to the parent document of the element.
    ''' </summary>
    Private mParentDocument As clsHTMLDocument

    ''' <summary>
    ''' Creates a new clsHTMLElement.
    ''' </summary>
    ''' <param name="e">The mshtml.IHTMLElement that the new instance is to
    ''' represent.</param>
    Public Sub New(ByVal e As mshtml.IHTMLElement, ByVal parent As clsHTMLDocument)
        mElement = e
        mParentDocument = parent
    End Sub

    ''' <summary>
    ''' Checks to see if the element has a parent element.
    ''' </summary>
    ''' <returns>True if the element has no parent element.</returns>
    Public Function HasNoParent() As Boolean
        Return mElement.parentElement Is Nothing
    End Function

    ''' <summary>
    ''' Gets a list of clsHTMLElement instances from an IHTMLElementCollection,
    ''' optionally filtering by the tag name.
    ''' </summary>
    ''' <param name="obj">An object which is expected to be  of type
    ''' mshtml.IHTMLElementCollection. If it isn't, the returned list will be
    ''' empty.</param>
    ''' <param name="tagname">Optionally, a tag name to use as a filter. If Nothing,
    ''' no filtering takes place.</param>
    ''' <returns>A List of clsHTMLElement objects.</returns>
    Private Shared Function GetElements(ByVal obj As Object, ByVal parent As clsHTMLDocument, Optional ByVal tagname As String = Nothing) As List(Of clsHTMLElement)
        Dim c As New List(Of clsHTMLElement)
        Dim src As mshtml.IHTMLElementCollection = TryCast(obj, mshtml.IHTMLElementCollection)
        If Not src Is Nothing Then
            For Each el As mshtml.IHTMLElement In src
                If tagname Is Nothing OrElse el.tagName = tagname Then
                    c.Add(New clsHTMLElement(el, parent))
                End If
            Next
        End If
        Return c
    End Function


    ''' <summary>
    ''' Gets all the HTML elements that are of this HTML element
    ''' </summary>
    Public ReadOnly Property All() As Generic.List(Of clsHTMLElement)
        Get
            Return GetElements(mElement.all, mParentDocument)
        End Get
    End Property

    ''' <summary>
    ''' Gets all the HTML elements that are children of this HTML element
    ''' </summary>
    Public ReadOnly Property Children() As Generic.List(Of clsHTMLElement)
        Get
            Return GetElements(mElement.children, mParentDocument)
        End Get
    End Property

    ''' <summary>
    ''' Gets the outer HTML of this element
    ''' </summary>
    Public ReadOnly Property OuterHTML() As String
        Get
            Return mElement.outerHTML
        End Get
    End Property

    ''' <summary>
    ''' Gets all the inner HTML of this element
    ''' </summary>
    Public ReadOnly Property InnerHTML() As String
        Get
            Return mElement.innerHTML
        End Get
    End Property

    ''' <summary>
    ''' Gets the classname of this element or String. Empty if there is none
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.ClassName)> _
    Public Property ClassName() As String
        Get
            Dim s As String = mElement.className
            If s Is Nothing Then Return String.Empty
            Return s
        End Get
        Set(ByVal value As String)
            mElement.className = value
        End Set
    End Property

    ''' <summary>
    ''' Performs the click action on an html element
    ''' </summary>
    Public Sub Click()
        'The order of events here should be onmousedown, onmouseclick, onmouseup
        'note that calling mElement.click fires the onclick event.
        FireEvent("onMouseDown")
        mElement.click()
        FireEvent("onMouseUp")
    End Sub

    ''' <summary>
    ''' Performs a double click action on an html element
    ''' </summary>
    Public Sub DoubleClick()
        'The order of events leading to the ondblclick event is 
        'onmousedown, onclick, onmouseup, and then ondblclick.
        'note that the order described above is implemented however 
        'some of the events are fired in the Click() function
        Click()
        FireEvent("onDblClick")
    End Sub

    ''' <summary>
    ''' Focuses the element.
    ''' </summary>
    Public Sub Focus()
        Dim e As mshtml.IHTMLElement2 = TryCast(mElement, mshtml.IHTMLElement2)
        If e IsNot Nothing Then e.focus()
    End Sub

    ''' <summary>
    ''' Fires an event.
    ''' </summary>
    Public Sub FireEvent(ByVal name As String)
        Try
            Dim e As mshtml.IHTMLElement3 = TryCast(mElement, mshtml.IHTMLElement3)
            e.FireEvent(name)
        Catch
        End Try
    End Sub


    ''' <summary>
    ''' Checks to see if the given clsHTMLElement's underlying element is the same
    ''' element as this instance's underlying element.
    ''' </summary>
    ''' <param name="e">Another clsHTMLElement to compare against.</param>
    ''' <returns>True if the two clsHTMLElements have the same underlying
    ''' mshtml.IHTMLElement.</returns>
    Public Function ElementEquals(ByVal e As clsHTMLElement) As Boolean
        Return mElement Is e.mElement
    End Function

    ''' <summary>
    ''' Dumps this HTML element and its descendents to a string. The string contains
    ''' all of the identifiers for this element. Each level of parentage is indented
    ''' by 2 spaces
    ''' </summary>
    ''' <returns>A full dump of the HTML tree at and below this element</returns>
    Public Function Dump() As String
        Return DumpTo(New StringBuilder()).ToString()
    End Function

    ''' <summary>
    ''' Dumps this HTML element and its descendents to the given buffer. The buffer
    ''' will contain all of the identifiers for this element. Each level of parentage
    ''' is separated by a carriage return and indented by 2 spaces
    ''' </summary>
    ''' <returns>A full dump of the HTML tree at and below this element</returns>
    Public Function DumpTo(ByVal sb As StringBuilder) As StringBuilder
        Return DumpTo(sb, 0, 2, " "c, "HTML:")
    End Function

    ''' <summary>
    ''' Dumps this HTML element and its descendents to the given buffer. The buffer
    ''' will contain all of the identifiers for this element. Each level of parentage
    ''' is separated by a carriage return and indented by the specified number of
    ''' prefix characters.
    ''' </summary>
    ''' <param name="sb">The buffer to append this element tree to</param>
    ''' <param name="indentLevel">The number of indent chars to prepend this
    ''' element's data with</param>
    ''' <param name="indentInc">The number of indent characters to increment each
    ''' level with</param>
    ''' <param name="indentChar">The indent character to use to indent the descendant
    ''' elements with. If this is null (ie. a Char value of Chr(0) / '\0'), then no
    ''' prefix is prepended to the element.</param>
    ''' <param name="prefix">The prefix to add to the element after the indent,
    ''' but before the identifiers.</param>
    ''' <returns>The given string buffer with this element appended to it.</returns>
    Public Function DumpTo(ByVal sb As StringBuilder, _
     ByVal indentLevel As Integer, ByVal indentInc As Integer, _
     ByVal indentChar As Char, ByVal prefix As String) _
     As StringBuilder

        If indentChar <> Nothing Then sb.Append(indentChar, indentLevel)
        sb.Append(prefix)
        AppendIdentifiers(sb).AppendLine()
        For Each el As clsHTMLElement In Children
            el.DumpTo(sb, indentLevel + indentInc, indentInc, indentChar, prefix)
            sb.AppendLine()
        Next
        Return sb

    End Function

    ''' <summary>
    ''' Get the HTML identifiers for this element, in the format returned from
    ''' a spy query.
    ''' </summary>
    ''' <returns>The formatted identifier information.</returns>
    Public Function GetIdentifiers() As String
        Return AppendIdentifiers(New StringBuilder()).ToString()
    End Function

    ''' <summary>
    ''' Appends the HTML identifiers for this element to the given buffer, in the
    ''' format returned from a spy query.
    ''' </summary>
    ''' <param name="sb">The buffer to which this element should be appended</param>
    ''' <returns>The given buffer with this element's identifiers appended to it
    ''' </returns>
    Public Function AppendIdentifiers(ByVal sb As StringBuilder) As StringBuilder
        Dim r As RECT = ClientBounds
        Return sb.Append("X=").Append(r.Left) _
         .Append(" Y=").Append(r.Top) _
         .Append(" Width=").Append(r.Width) _
         .Append(" Height=").Append(r.Height) _
         .Append(" +Path=").Append(clsQuery.EncodeValue(Path)) _
         .Append(" +TagName=").Append(clsQuery.EncodeValue(TagName)) _
         .Append(" +ClassName=").Append(clsQuery.EncodeValue(ClassName)) _
         .Append(" +ID=").Append(clsQuery.EncodeValue(ID)) _
         .Append(" +Title=").Append(clsQuery.EncodeValue(Title)) _
         .Append(" +Link=").Append(clsQuery.EncodeValue(Link)) _
         .Append(" +InputType=").Append(clsQuery.EncodeValue(InputType)) _
         .Append(" +InputIdentifier2=").Append(clsQuery.EncodeValue(InputIdentifier2)) _
         .Append(" AncestorCount=").Append(AncestorCount.ToString()) _
         .Append(" Value=").Append(clsQuery.EncodeValue(Value)) _
         .Append(" +Enabled=").Append(Enabled) _
         .Append(" Checked=").Append(Checked) _
         .Append(" +pURL=").Append(clsQuery.EncodeValue(ParentURL)) _
         .Append(" ScreenBounds=").Append(CType(AbsoluteBounds, RECT)) _
         .Append(" MatchIndex=1") _
         .Append(" MatchReverse=True")
    End Function


    ''' <summary>
    ''' Gets or sets the ID of the element
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.ID)> _
    Public Property ID() As String
        Get
            Dim s As String = mElement.id
            If s Is Nothing Then Return String.Empty
            Return s
        End Get
        Set(ByVal value As String)
            mElement.id = value
        End Set
    End Property


    ''' <summary>
    ''' Gets a string representing the path of the element in a xpath like syntax e.g
    ''' HTML/BODY(1)/P(1) The number indicates the index of the element of that type.
    ''' </summary>
    <HTMLIdentifier(clsQuery.IdentifierTypes.Path)> _
    Public ReadOnly Property Path() As String
        Get
            Dim p As String

            If mElement.parentElement Is Nothing Then
                p = "HTML"
            Else
                p = GetTagNameAndIndex()
            End If

            Dim pef As clsHTMLElement = ParentElementOrFrameElement
            If Not pef Is Nothing Then
                p = pef.Path & "/" & p
            Else
                p = "/" & p
            End If

            Return p
        End Get
    End Property


    ''' <summary>
    ''' Gets the name of the tag and an index number 
    ''' </summary>
    ''' <returns>The tag name, with the index number in parentheses appended to it.
    ''' </returns>
    Private Function GetTagNameAndIndex() As String
        Dim sPath As String
        Dim objParent As mshtml.IHTMLElement = mElement.parentElement
        Dim iIndex As Integer = GetIndex(objParent, mElement)
        If iIndex = -1 AndAlso objParent.parentElement IsNot Nothing Then
            'Find missing child by starting at the parent of the parent, and decending down again
            sPath = FindPath(objParent.parentElement, mElement)
            'The result gives us an extra element because we started at the parent of the parent
            'so trim it off.
            Dim ileft As Integer = sPath.IndexOf("/"c) + 1
            sPath = sPath.Remove(0, ileft)
        Else
            sPath = mElement.tagName & "(" & iIndex + 1 & ")"
        End If
        Return sPath
    End Function


    ''' <summary>
    ''' Helper function to recursively find a path fragment for a child element
    ''' within another element.
    ''' </summary>
    ''' <param name="eElementToSearch">The element to search within.</param>
    ''' <param name="eElementToFind">The element to find.</param>
    ''' <returns>The path fragment that resolves the child's location.</returns>
    Private Shared Function FindPath(ByVal eElementToSearch As mshtml.IHTMLElement, ByVal eElementToFind As mshtml.IHTMLElement) As String
        Dim iIndex As Integer = GetIndex(eElementToSearch, eElementToFind)
        If iIndex = -1 Then
            For Each eChild As mshtml.IHTMLElement In TryCast(eElementToSearch.children, mshtml.IHTMLElementCollection)
                Dim sPath As String = FindPath(eChild, eElementToFind)
                If sPath IsNot Nothing Then
                    Dim iChildIndex As Integer = GetIndex(eChild.parentElement, eChild)
                    Return eChild.tagName & "(" & iChildIndex + 1 & ")/" & sPath
                End If
            Next

            Return Nothing
        Else
            Return eElementToFind.tagName & "(" & iIndex + 1 & ")"
        End If
    End Function


    ''' <summary>
    ''' Gets the index of an element of a particular type
    ''' </summary>
    ''' <returns>The element's 0-based index.</returns>
    Private Shared Function GetIndex(ByVal eElementToSearch As mshtml.IHTMLElement, ByVal eElementToFind As mshtml.IHTMLElement) As Integer
        Dim iIndex As Integer = 0
        For Each eChild As mshtml.IHTMLElement In TryCast(eElementToSearch.children, mshtml.IHTMLElementCollection)
            If eChild Is eElementToFind OrElse eChild.sourceIndex = eElementToFind.sourceIndex Then
                Return iIndex
            End If
            If eChild.tagName = eElementToFind.tagName Then
                iIndex += 1
            End If
        Next
        Return -1
    End Function


    ''' <summary>
    ''' Gets the content linked by an element. Typically a "script" or "link"
    ''' element. eg "&lt;link rel='stylesheet' type='text/css' href='/content/styles/style.css'&gt;" 
    ''' </summary>
    ''' <param name="contentURL">Carries back the URL of the linked content.</param>
    ''' <param name="contentType">Carries back the content type of the linked content.
    ''' Where a MIME type is specified in the document, this value is carried back.
    ''' Otherwise one of "text" or "data" is returned.</param>
    ''' <param name="contentEncoding">Carries back the content encoding of the
    ''' linked content, if any. It will be an empty string if the header was not
    ''' present.</param>
    ''' <returns>Returns the linked content as a string. Anything with a MIME type
    ''' which starts with "text" is encoded as a plain string; all other content is
    ''' encoded as a base64 string. Throws exception in the event of an error (e.g.
    ''' network problem).</returns>
    Public Function GetLinkedContent(ByRef contentURL As String, ByRef contentType As String, ByRef contentEncoding As String) As String

        'This long block determines contenttype and location (url)
        'Because document authors may be lazy and omit MIME types,
        'etc, we sometimes have to work hard to infer the contenttype
        '(based on rel attribute or file extension)
        contentType = Nothing
        contentURL = Nothing
        contentEncoding = ""
        Select Case True
            Case (TypeOf mElement Is mshtml.IHTMLLinkElement)
                Dim linkElement As mshtml.IHTMLLinkElement = CType(mElement, mshtml.IHTMLLinkElement)
                contentURL = linkElement.href
                contentType = linkElement.type
                If String.IsNullOrEmpty(contentType) Then
                    'We have to use a heuristic approach

                    'Try to infer from "rel" attribute
                    If Not String.IsNullOrEmpty(linkElement.rel) Then
                        Select Case linkElement.rel.ToLower
                            Case "appendix", "bookmark", "chapter", "contents", "copyright", "glossary", "help", "home", "index", "next", "prev", "section", "start", "stylesheet", "subsection"
                                contentType = "text"
                            Case "apple-touch-icon"
                                contentType = "image/gif"
                        End Select
                    End If

                    'Try to infer content type from file extension
                    If String.IsNullOrEmpty(contentType) Then
                        If Not String.IsNullOrEmpty(linkElement.href) Then
                            Dim index As Integer = linkElement.href.LastIndexOf(".")
                            If index > -1 Then
                                Dim FileExtension As String = linkElement.href.Substring(index + 1)
                                Select Case FileExtension
                                    Case "css", "js", _
                                    "html", "shtml", "shtm", "htm", "asp", "htc", "ssi", _
                                     "txt", "text", "log", "rtf", _
                                     "xml", "sgm", "sgml", _
                                     "c", "pas", "cpp", "c++", "def", "h", "java", "jav", "pl", "py", "asm"
                                        contentType = "text"
                                    Case Else
                                        contentType = "application"
                                End Select
                            End If
                        End If
                    End If
                End If
            Case (TypeOf mElement Is mshtml.IHTMLScriptElement)
                Dim scriptEl As mshtml.IHTMLScriptElement = CType(mElement, mshtml.IHTMLScriptElement)
                contentURL = scriptEl.src
                contentType = "text"
        End Select

        'Get the content
        If Not String.IsNullOrEmpty(contentURL) Then
            If contentURL.StartsWith("//") Then
                Dim curi As New Uri(ParentURL)
                contentURL = curi.Scheme & ":" & contentURL
            ElseIf contentURL.StartsWith("/") Then
                Dim curi As New Uri(ParentURL)
                contentURL = curi.Scheme & Uri.SchemeDelimiter & curi.Authority & contentURL
            ElseIf Not Uri.IsWellFormedUriString(contentURL, UriKind.Absolute) Then
                If Not Uri.IsWellFormedUriString(contentURL, UriKind.Relative) Then
                    contentURL = Nothing
                Else
                    contentURL = New Uri(New Uri(ParentURL), contentURL).ToString()
                End If
            End If
            If String.IsNullOrEmpty(contentType) Then contentType = "data"
        End If

        If Not String.IsNullOrEmpty(contentURL) Then
            Dim request As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(contentURL), System.Net.HttpWebRequest)
            Dim response As System.Net.HttpWebResponse = CType(request.GetResponse(), System.Net.HttpWebResponse)
            contentEncoding = response.ContentEncoding
            If contentType.StartsWith("text") And contentEncoding.Length = 0 Then
                Dim enc As String = "utf-8"
                If Not String.IsNullOrEmpty(response.CharacterSet) Then
                    enc = response.CharacterSet
                End If
                Dim encode As Encoding = Encoding.GetEncoding(enc)
                Using sr As New StreamReader(response.GetResponseStream(), encode)
                    Return sr.ReadToEnd()
                End Using
            Else
                Using st As System.IO.Stream = response.GetResponseStream
                    Dim Base64Output As New Text.StringBuilder
                    Using br As New System.IO.BinaryReader(st)
                        Const buffersize As Integer = 3 * 1024  'NB this should be a mulitple of three.
                        Dim BytesRead As Integer
                        Do
                            Dim Bytes(buffersize - 1) As Byte
                            BytesRead = br.Read(Bytes, 0, buffersize)
                            Dim s As String = Convert.ToBase64String(Bytes)
                            Base64Output.Append(s)
                        Loop While BytesRead > 0
                    End Using
                    Return Base64Output.ToString
                End Using
            End If
        End If

        Return String.Empty
    End Function

    ''' <summary>
    ''' Gets the Ancestor Count of the element.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.AncestorCount)> _
    Public ReadOnly Property AncestorCount() As Integer
        Get
            Dim count As Integer = 0
            Dim parent As mshtml.IHTMLElement = mElement.parentElement
            While parent IsNot Nothing
                count += 1
                parent = parent.parentElement
            End While
            Return count
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the element.
    ''' </summary>
    Public ReadOnly Property ClientBounds() As RECT
        Get
            Dim left As Integer = mElement.offsetLeft
            Dim top As Integer = mElement.offsetTop
            Dim width As Integer = mElement.offsetWidth
            Dim height As Integer = mElement.offsetHeight

            Return New RECT(left, left + width, top, top + height)
        End Get
    End Property

    ''' <summary>
    ''' Gets the bounds of the element relative to the owning document (or frame).
    ''' </summary>
    Public ReadOnly Property BoundingRectangle() As System.Drawing.Rectangle
        Get
            Dim R As mshtml.IHTMLRect = CType(mElement, mshtml.IHTMLElement2).getBoundingClientRect()
            Return New System.Drawing.Rectangle(R.left, R.top, R.right - R.left, R.bottom - R.top)
        End Get
    End Property

    ''' <summary>
    ''' Gets the Scroll position of the vertical scrollbar.
    ''' </summary>
    Public Property ScrollTop() As Integer
        Get
            Dim e As mshtml.IHTMLElement2 = TryCast(mElement, mshtml.IHTMLElement2)
            If Not e Is Nothing Then
                Return e.scrollTop
            End If
            Return 0
        End Get
        Set(ByVal value As Integer)
            Dim e As mshtml.IHTMLElement2 = TryCast(mElement, mshtml.IHTMLElement2)
            If Not e Is Nothing Then
                e.scrollTop = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the scroll position of the horizontal scrollbar.
    ''' </summary>
    Public Property ScrollLeft() As Integer
        Get
            Dim e As mshtml.IHTMLElement2 = TryCast(mElement, mshtml.IHTMLElement2)
            If Not e Is Nothing Then
                Return e.scrollLeft
            End If
            Return 0
        End Get
        Set(ByVal value As Integer)
            Dim e As mshtml.IHTMLElement2 = TryCast(mElement, mshtml.IHTMLElement2)
            If Not e Is Nothing Then
                e.scrollLeft = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the X coordinate, i.e. the Left edge of the element.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.X)> _
    Public ReadOnly Property Left() As Integer
        Get
            Return Me.ClientBounds.Left
        End Get
    End Property

    ''' <summary>
    ''' Gets the Right edge of the element.
    ''' </summary>
    Public ReadOnly Property Right() As Integer
        Get
            Return Me.ClientBounds.Right
        End Get
    End Property

    ''' <summary>
    ''' Gets the Y coordinate, i.e. the Top edge of the element.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Y)> _
    Public ReadOnly Property Top() As Integer
        Get
            Return Me.ClientBounds.Top
        End Get
    End Property

    ''' <summary>
    ''' Gets the Bottom edge of the element.
    ''' </summary>
    Public ReadOnly Property Bottom() As Integer
        Get
            Return Me.ClientBounds.Bottom
        End Get
    End Property

    ''' <summary>
    ''' Gets the width of the element.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Width)> _
    Public ReadOnly Property Width() As Integer
        Get
            Return Me.ClientBounds.Width
        End Get
    End Property

    ''' <summary>
    ''' Gets the height edge of the element.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Height)> _
    Public ReadOnly Property Height() As Integer
        Get
            Return Me.ClientBounds.Height
        End Get
    End Property


    ''' <summary>
    ''' Gets the parent element, or if the parent element is Nothing gets the
    ''' parent frame element.
    ''' </summary>
    Private ReadOnly Property ParentElementOrFrameElement() As clsHTMLElement
        Get
            Dim objElement As clsHTMLElement = Me.Parent
            If objElement IsNot Nothing Then Return objElement

            Dim objFrame As clsHTMLDocumentFrame = TryCast(mParentDocument, clsHTMLDocumentFrame)
            If Not objFrame Is Nothing Then
                Return objFrame.FrameElement
            End If

            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the parent of this HTML element, or Nothing if it has no parent
    ''' element.
    ''' </summary>
    Public ReadOnly Property Parent() As clsHTMLElement
        Get
            Dim objElement As mshtml.IHTMLElement = mElement.parentElement
            If Not objElement Is Nothing Then
                Return New clsHTMLElement(objElement, mParentDocument)
            End If
            Return Nothing
        End Get
    End Property


    ''' <summary>
    ''' Gets the absolute bounds of the element, that is to say its bounds relative
    ''' to the screen.
    ''' </summary>
    Public ReadOnly Property AbsoluteBounds() As Drawing.Rectangle
        Get
            Return mParentDocument.GetAbsoluteBounds(Me)
        End Get
    End Property


    ''' <summary>
    ''' Gets the name of the tag.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.TagName)> _
    Public ReadOnly Property TagName() As String
        Get
            Dim s As String = mElement.tagName
            If s Is Nothing Then Return String.Empty
            Return s
        End Get
    End Property


    ''' <summary>
    ''' Gets the title of the element.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Title)> _
    Public ReadOnly Property Title() As String
        Get
            Dim s As String = mElement.title
            If s Is Nothing Then Return String.Empty
            Return s
        End Get
    End Property


    ''' <summary>
    ''' Gets or sets the value of the element.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Value)> _
    Public Property Value() As String
        Get
            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            Dim s As String
            If e IsNot Nothing Then
                s = e.value
            Else
                s = mElement.innerText
            End If
            If s Is Nothing Then Return String.Empty
            Return s
        End Get
        Set(ByVal value As String)
            Dim ValueChanged As Boolean = False

            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            If e IsNot Nothing Then
                ValueChanged = value <> e.value
                e.value = value
            Else
                If Writeable Then
                    ValueChanged = mElement.innerText <> value
                    mElement.innerText = value
                Else
                    Throw New InvalidOperationException(My.Resources.TheElementIsReadOnlyAndCanNotBeWrittenTo)
                End If
            End If

            If ValueChanged Then
                FireEvent("onChange")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Determine if this element is writable.
    ''' </summary>
    Public ReadOnly Property Writeable() As Boolean
        Get
            Select Case mElement.tagName.ToUpper

                Case "COL", "COLGROUP", "FRAMESET", "HTML", "HEAD", "STYLE", "TABLE", "TBODY", "TFOOT", "THEAD", "TITLE", "TR"
                    'These elements are not writeable as documented here http://msdn2.microsoft.com/en-us/library/aa752298.aspx
                    Return False
                Case Else
                    Return True
            End Select
        End Get
    End Property

    ''' <summary>
    ''' This function is broken since it returns the value of the InputIdentifier 
    ''' instead of the name. Its left in for backwards compatability with old processes.
    ''' InputIdentifier2 will be used in newly spied elements. 
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.InputIdentifier)> _
    Public ReadOnly Property InputIdentifier() As String
        Get
            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            If Not e Is Nothing Then
                Dim s As String = e.value
                If s IsNot Nothing Then Return s
            End If
            Return String.Empty
        End Get
    End Property

    ''' <summary>
    ''' Gets an ID which is relevent if this element is an input, such as a textbox
    ''' or checkbox.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.InputIdentifier2)> _
    Public ReadOnly Property InputIdentifier2() As String
        Get
            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            If Not e Is Nothing Then
                Dim s As String = e.name
                If s IsNot Nothing Then Return s
            End If
            Return String.Empty
        End Get
    End Property

    ''' <summary>
    ''' Gets the type of the input.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.InputType)> _
    Public ReadOnly Property InputType() As String
        Get
            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            If Not e Is Nothing Then
                Return e.type
            End If
            If TypeOf mElement Is mshtml.IHTMLSelectElement Then
                Return "select"
            End If
            Return String.Empty
        End Get
    End Property

    ''' <summary>
    ''' Gets the link text of the element otherwise known as the href
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Link)> _
    Public ReadOnly Property Link() As String
        Get
            Dim e As mshtml.IHTMLAnchorElement = TryCast(mElement, mshtml.IHTMLAnchorElement)
            If Not e Is Nothing AndAlso Not e.href Is Nothing Then
                Return e.href
            End If
            Return String.Empty
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the check state of the element
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Checked)> _
    Public Property Checked() As Boolean
        Get
            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            If Not e Is Nothing Then
                Return e.checked
            End If
            Return False
        End Get
        Set(ByVal value As Boolean)
            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            If Not e Is Nothing Then
                Dim Changed As Boolean = e.checked <> value
                e.checked = value
                If Changed Then FireEvent("onChange")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the selected state of the element
    ''' </summary>
    Public Property Selected() As Boolean
        Get
            Dim e As mshtml.IHTMLOptionElement = TryCast(mElement, mshtml.IHTMLOptionElement)
            If Not e Is Nothing Then
                Return e.selected
            End If
            Return False
        End Get
        Set(ByVal value As Boolean)
            Dim e As mshtml.IHTMLOptionElement = TryCast(mElement, mshtml.IHTMLOptionElement)
            If Not e Is Nothing Then
                e.selected = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the enabled status of the element
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.Enabled)> _
    Public ReadOnly Property Enabled() As Boolean
        Get
            Dim e As mshtml.IHTMLInputElement = TryCast(mElement, mshtml.IHTMLInputElement)
            If Not e Is Nothing Then
                Return Not e.disabled
            End If
            Return True
        End Get
    End Property


    ''' <summary>
    ''' Gets the first HTML ancestor of this element with the given tag name.
    ''' If no such ancestor was found, this will return nothing.
    ''' </summary>
    ''' <param name="tagName">The tag name of the ancestor required</param>
    ''' <returns>The HTML Element representing the first ancestor in the DOM
    ''' hierarchy with the specified tag name, or Nothing if no such ancestor
    ''' was found.</returns>
    Public Function GetAncestorWithTagName(ByVal tagName As String) As clsHTMLElement

        If Me.TagName = tagName Then Return Me

        Dim parent As clsHTMLElement = Me.Parent
        If parent Is Nothing Then Return Nothing
        Return parent.GetAncestorWithTagName(tagName)

    End Function

    ''' <summary>
    ''' Gets the currently selected item
    ''' </summary>
    Public Function SelectedItem() As clsHTMLElement
        Dim e As mshtml.IHTMLSelectElement = TryCast(mElement, mshtml.IHTMLSelectElement)
        If Not e Is Nothing Then
            Dim i As Integer = e.selectedIndex
            If i >= 0 Then
                Return New clsHTMLElement(TryCast(e.item(i), mshtml.IHTMLElement), mParentDocument)
            End If
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Selects an item in a combo dropdown via its index.
    ''' </summary>
    ''' <param name="i">The index of the item to select.</param>
    Public Function SelectItem(ByVal i As Integer) As Boolean
        Dim e As mshtml.IHTMLSelectElement = TryCast(mElement, mshtml.IHTMLSelectElement)
        If e IsNot Nothing AndAlso i < e.length Then
            e.selectedIndex = i
            FireEvent("onClick")
            FireEvent("onChange")
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Selects an item in a combo box.
    ''' </summary>
    ''' <param name="item">The identifier of the item to select.</param>
    Public Sub SelectItem(ByVal item As String)
        Dim e As mshtml.IHTMLSelectElement = TryCast(mElement, mshtml.IHTMLSelectElement)
        e.value = item
    End Sub

    ''' <summary>
    ''' Selects an item in a combo box by name.
    ''' </summary>
    ''' <param name="name">The name of the item to select, as seen in the combo box.
    ''' </param>
    ''' <returns>True if the item is successfully selected</returns>
    Public Function SelectItemByName(ByVal name As String) As Boolean
        Dim e As mshtml.IHTMLSelectElement = TryCast(mElement, mshtml.IHTMLSelectElement)
        If e IsNot Nothing Then
            For i As Integer = 0 To e.length
                Dim it As mshtml.IHTMLElement = TryCast(e.item(i), mshtml.IHTMLElement)
                If it IsNot Nothing AndAlso it.innerText = name Then
                    Return SelectItem(i)
                End If
            Next
        End If
        Return False
    End Function

    ''' <summary>
    ''' Selects an item in a combo box by value.
    ''' </summary>
    ''' <param name="value">The value of the item to select, as given by the 'value' 
    ''' attribute in the html.</param>
    ''' <returns>True if the item is successfully selected</returns>
    Public Function SelectItemByValue(ByVal value As String) As Boolean
        Dim e As mshtml.IHTMLSelectElement = TryCast(mElement, mshtml.IHTMLSelectElement)
        If e IsNot Nothing Then
            For i As Integer = 0 To e.length
                Dim it As mshtml.IHTMLOptionElement = TryCast(e.item(i), mshtml.IHTMLOptionElement)
                If it IsNot Nothing AndAlso it.value = value Then
                    Return SelectItem(i)
                End If
            Next
        End If
        Return False
    End Function

    ''' <summary>
    ''' Gets the offset left of the element.
    ''' </summary>
    Public ReadOnly Property OffsetLeft() As Integer
        Get
            If mElement IsNot Nothing Then
                Return mElement.offsetLeft
            End If
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' Gets the offset top of the element.
    ''' </summary>
    Public ReadOnly Property OffsetTop() As Integer
        Get
            If mElement IsNot Nothing Then
                Return mElement.offsetTop
            End If
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' Gets the offset width of the element.
    ''' </summary>
    Public ReadOnly Property OffsetWidth() As Integer
        Get
            If mElement IsNot Nothing Then
                Return mElement.offsetWidth
            End If
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' Gets the offset height of the element.
    ''' </summary>
    Public ReadOnly Property OffsetHeight() As Integer
        Get
            If mElement IsNot Nothing Then
                Return mElement.offsetHeight
            End If
            Return 0
        End Get
    End Property

    ''' <summary>
    ''' Gets a child of this element, identified by information from a path.
    ''' </summary>
    ''' <param name="index">The index from the path</param>
    ''' <param name="tagname">The tag name from the path.</param>
    ''' <returns></returns>
    Public Function GetChild(ByVal index As Integer, ByVal tagname As String) As clsHTMLElement

        Dim iCount As Integer = 0
        If Not mElement Is Nothing Then
            Dim children As mshtml.IHTMLElementCollection = TryCast(mElement.children, mshtml.IHTMLElementCollection)
            If Not children Is Nothing Then
                For Each el As mshtml.IHTMLElement In children
                    If tagname Is Nothing OrElse el.tagName = tagname Then
                        If iCount = index Then
                            Return New clsHTMLElement(el, mParentDocument)
                        End If
                        iCount += 1
                    End If
                Next
            End If
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the parent document of this element.
    ''' </summary>
    Public ReadOnly Property ParentDocument() As clsHTMLDocument
        Get
            Return mParentDocument
        End Get
    End Property

    ''' <summary>
    ''' The parent URL of the document.
    ''' </summary>
    <HTMLIdentifierAttribute(clsQuery.IdentifierTypes.pURL)> _
    Public ReadOnly Property ParentURL() As String
        Get
            Return mParentDocument.URL
        End Get
    End Property

    ''' <summary>
    ''' Holds a list of properties that can be indexed quickly for a given Identifier type.
    ''' </summary>
    Private Shared mobjProperties As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)

    ''' <summary>
    ''' Returns a list of properties that can be indexed quickly for a given Identifier type.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetProperties() As Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)
        If mobjProperties Is Nothing Then
            mobjProperties = New Dictionary(Of clsQuery.IdentifierTypes, Reflection.PropertyInfo)
            For Each pi As System.Reflection.PropertyInfo In GetType(clsHTMLElement).GetProperties(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.IgnoreCase)
                Dim Attributes As Object() = pi.GetCustomAttributes(GetType(clsHTMLElement.HTMLIdentifierAttribute), False)
                If Attributes IsNot Nothing AndAlso Attributes.Length > 0 Then
                    mobjProperties.Add(CType(Attributes(0), clsHTMLElement.HTMLIdentifierAttribute).Identifier, pi)
                End If
            Next
        End If

        Return mobjProperties
    End Function

    ''' <summary>
    ''' Class used in property attributes to match up properties of this class
    ''' against parameters in clsQuery.
    ''' </summary>
    Private Class HTMLIdentifierAttribute
        Inherits System.Attribute
        Public Identifier As clsQuery.IdentifierTypes
        Public Sub New(ByVal id As clsQuery.IdentifierTypes)
            Identifier = id
        End Sub
    End Class

End Class
