Imports System.Linq
Imports System.Text.RegularExpressions

Namespace WebApis.TemplateProcessing

    ''' <summary>
    ''' Separates a string containing parameter placeholders into a sequence of 
    ''' elements. 
    ''' </summary>
    Friend Class ContentElementLexer
    
        Private ReadOnly mReadFuncs As New List(Of Func(Of String, Integer, ContentElement)) From {
            AddressOf ReadEscapedDelimiter, 
            AddressOf ReadParameter,
            AddressOf ReadLiteral
        }

        Private ReadOnly mOpeningParameterDelimiter As String

        Private ReadOnly mClosingParameterDelimiter As String

        Private ReadOnly mOpeningParameterDelimiterEscaped As String

        Private ReadOnly mParameterRegex As Regex

        ''' <summary>
        ''' Creates a new ContentElementTokeniser
        ''' </summary>
        ''' <param name="openingParameterDelimiter">The delimiter used at the start of
        ''' a parameter token</param>
        ''' <param name="closingParameterDelimiter">The delimiter used at the end of 
        ''' a parameter token</param>
        Sub New(openingParameterDelimiter As String, closingParameterDelimiter As String)

            Me.mOpeningParameterDelimiter = openingParameterDelimiter
            Me.mClosingParameterDelimiter = closingParameterDelimiter
            Me.mOpeningParameterDelimiterEscaped = openingParameterDelimiter + 
                openingParameterDelimiter

            ' \G anchor means the match must start at the position where the previous 
            ' match ended but also applies when specifying the position at which to 
            ' begin searching via the startat parameter in Regex.Match
            mParameterRegex = New Regex($"\G\{openingParameterDelimiter}(?<name>(?!\s)[\w-,. ]*(?<!\s))\{closingParameterDelimiter}")

        End Sub
        
        ''' <summary>
        ''' Separates the supplied string into a sequence of elements combining the 
        ''' static text and parameter placeholder elements within the string.
        ''' </summary>
        ''' <param name="content">The content to split into element</param>
        ''' <returns>A sequence of elements identified within the string</returns>
        Friend Iterator Function Tokenise(content As String) As IEnumerable(Of ContentElement)

            Dim index = 0
            Dim element = ReadNext(content, index)
            While(element IsNot Nothing)
                Yield element
                index += element.Raw.Length
                element = ReadNext(content, index)
            End While
            
        End Function

        ''' <summary>
        ''' Reads the next element from the content at the index specified
        ''' </summary>
        Private Function ReadNext(content As String, index As Integer) As ContentElement
           
            ' Cycle through each potential element in turn using ReadX functions
            Return mReadFuncs.
                Select(Function(f)f(content, index)).
                Where(Function(e)e IsNot Nothing).
                FirstOrDefault
            
        End Function

        ''' <summary>
        ''' Returns an escaped delimiter element from the content if one is found at 
        ''' the index specified
        ''' </summary>
        Private Function ReadEscapedDelimiter(content As String, index As Integer) As ContentElement

            If GetText(content, index, mOpeningParameterDelimiterEscaped) IsNot Nothing Then
                Return New EscapedOpeningDelimiterElement(mOpeningParameterDelimiterEscaped, mOpeningParameterDelimiter)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' Returns a parameter element from the content if one is found at the index 
        ''' specified
        ''' </summary>
        Private Function ReadParameter(content As String, nextIndex As Integer) As ContentElement

            Dim match = mParameterRegex.Match(content, nextIndex)
            If match.Success
                Dim raw = match.Value
                Dim name = match.Groups("name").Value
                Return New ParameterElement(raw, name)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' Returns a static content element from the content if one is found at the index 
        ''' specified
        ''' </summary>
        Private Function ReadLiteral(content As String, index As Integer) As ContentElement

            ' If the next character is an opening parameter delimiter, then we 
            ' treat it as literal content. ReadParameter has already attempted a 
            ' match at this position so it can't be the start of a valid parameter 
            ' token.
            Dim raw = GetText(content, 
                               index, 
                               Function(i, c) i = index OrElse c <> mOpeningParameterDelimiter)
            Return If(raw IsNot Nothing, New LiteralElement(raw), Nothing)

        End Function

        ''' <summary>
        ''' Returns the specified text from the content if found at the index specified
        ''' </summary>
        Private Shared Function GetText(content As String, nextIndex As Integer, text As String) As String

            If nextIndex + text.Length > content.Length
                Return Nothing
            End If
            
            Dim index = 0
            While index < text.Length
                If content(nextIndex + index) <> text(index)
                    Return Nothing
                End If
                index += 1
            End While
            Return text

        End Function

        ''' <summary>
        ''' Returns characters from the content at the index specified while the 
        ''' condition is met
        ''' </summary>
        Private Shared Function GetText(content As String, 
                                         nextIndex As Integer, 
                                         test As Func(Of Integer, Char, Boolean)) As String

            Dim startIndex = nextIndex
            Dim index = startIndex
            While index < content.Length AndAlso test(index, content(index))
                index += 1
            End While
            Return If(index > startIndex, content.Substring(startIndex, index - startIndex), Nothing)
            
        End Function
        
    End Class
End NameSpace