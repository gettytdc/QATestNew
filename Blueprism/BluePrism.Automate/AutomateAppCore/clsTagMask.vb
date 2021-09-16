Imports System.Text.RegularExpressions

Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization

''' Project  : AutomateAppCore
''' Class    : clsTagMask
''' 
''' <summary>
''' <para>Class to represent a mask of tags - either tags which are to be applied (ie. 
''' switched ON), or masked/filtered (ie. switched off).</para>
''' 
''' <para>The context that instances of this class are used will add meaning... eg.
''' for filtering work queue items, applied tags will represent tags which items
''' must have in order to be returned and masked tags will represent tags which items
''' must <em>not</em> have in order to be returned, whereas for applying tags to an
''' individual item, applied tags will represent tags which should be applied to the
''' item, whereas masked tags will represent tags which should be removed from the
''' item.</para>
''' 
''' <para>The semantics within the class are as generic as possible, ie. the mask
''' contains ON tags (tags which are to be set ON / applied), and OFF tags (tags
''' which are to be set OFF / masked).</para>
''' </summary>
<Serializable, DataContract([Namespace]:="bp"),
 KnownType(GetType(clsSortedSet(Of String)))>
Public Class clsTagMask

#Region "Variables (Constants & Members)"

    ''' <summary>
    ''' Regular expression to match the application of a tag to a work queue item,
    '''  any of "+[tag name]", "-[tag name]" or "[tag name]" with surrounding
    ''' whitespace removed.
    ''' </summary>
    Private Shared ReadOnly RegexTag As New Regex("^(\+|\-)?\s*(.*)\s*$", RegexOptions.Compiled)

    ''' <summary>
    ''' The set of tags which are applied (ie. set ON) in this tag mask
    ''' </summary>
    <DataMember>
    Private mOnTags As IBPSet(Of String)

    ''' <summary>
    ''' The set of tags which are masked (ie. set OFF) in this tag mask
    ''' </summary>
    <DataMember>
    Private mOffTags As IBPSet(Of String)

#End Region

#Region "Constructors"

    ''' <summary>
    ''' Creates a new empty tag mask.
    ''' In its initial state, this will neither apply nor mask any tags.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new tag mask applying the given tag string, skipping tags
    ''' with errors.
    ''' The tags which are currently being filtered on as a string in the format
    ''' When setting, if any tags don't match the required (extremely liberal)
    ''' format, they are skipped.
    ''' </summary>
    ''' <param name="applyTagString">A string of tags to apply in this mask,
    ''' in the format: "+wanted tag;+other wanted tag;-unwanted tag"</param>
    Public Sub New(ByVal applyTagString As String)
        Me.New(applyTagString, False)
    End Sub

    ''' <summary>
    ''' Creates a new tag mask applying the given tag string.
    ''' The tags which are currently being filtered on as a string in the format
    ''' When setting, if any tags don't match the required (extremely liberal)
    ''' format, they are skipped or cause an exception to be thrown if
    ''' <paramref name="failOnError">failOnError</paramref> is True.
    ''' </summary>
    ''' <param name="applyTagString">A string of tags to apply in this mask,
    ''' in the format: "+wanted tag;+other wanted tag;-unwanted tag"</param>
    ''' <param name="failOnError">True to indicate that an exception should
    ''' be thrown if any invalid tags are found.</param>
    ''' <exception cref="ArgumentException">Thrown if <paramref 
    ''' name="failOnError">failOnError</paramref> is true and an invalid
    ''' tag was found within the string</exception>
    ''' <remarks>See the <seealso cref="ApplyTags">ApplyTags</seealso>
    ''' description for fuller detail on validation/interpretation rules.
    ''' </remarks>
    Public Sub New(ByVal applyTagString As String, ByVal failOnError As Boolean)
        ApplyTags(applyTagString, Not failOnError)
    End Sub

#End Region

#Region "Private Helper Methods"

    ''' <summary>
    ''' Ensures that the given tag matches the validation rules for a tag, namely:
    ''' <list>
    ''' <item>Tags cannot be empty (or only contain whitespace)</item>
    ''' <item>Tags cannot start with a '+'(plus) or '-'(minus) character</item>
    ''' <item>Tags cannot contain ';'(semi-colon) characters</item>
    ''' </list>
    ''' If the given tag fails any of these tests an exception is thrown.
    ''' </summary>
    ''' <param name="tag">The tag to be checked for correctness</param>
    Private Sub EnsureCorrect(ByVal tag As String)
        If tag IsNot Nothing Then tag = tag.Trim()
        If String.IsNullOrEmpty(tag) Then
            Throw New ArgumentNullException(NameOf(tag), My.Resources.clsTagMask_CannotSetAnEmptyTag)
        ElseIf "+-".Contains(tag.Chars(0)) Then
            Throw New ArgumentException(My.Resources.clsTagMask_TagsCannotBeginWithAOrCharacter)
        ElseIf tag.Contains(";") Then
            Throw New ArgumentException(My.Resources.clsTagMask_TagsCannotContainSemiColonCharacters)
        End If
        ' Otherwise, okay...
    End Sub

    ''' <summary>
    ''' <para>Apply the given tag, putting it into the given set, and taking it out
    ''' of the other set. Note that if <paramref name="into">into</paramref> is null,
    ''' this method will instantiate it before adding the tag.
    ''' If <paramref name="outFrom">outFrom</paramref> is null, it will be left alone
    ''' </para>
    ''' <para><strong>Note: </strong> this performs no validation checking of the 
    ''' given tag - this must be performed before the tag is applied by this method.
    ''' It does, however, remove any leading or trailing whitespace from the tag
    ''' before applying it.
    ''' </para>
    ''' </summary>
    ''' <param name="tag">The tag to apply, to the specified set, and to remove from
    ''' the other set.</param>
    ''' <param name="into">The set into which the tag should be applied. If the set
    ''' is 'Nothing' then it will be instantiated.</param>
    ''' <param name="outFrom">The set from which the tag should be removed, if it
    ''' exists within that set.</param>
    Private Sub InternalApplyTag(ByVal tag As String,
     ByRef into As IBPSet(Of String), ByVal outFrom As IBPSet(Of String))

        tag = tag.Trim()

        If outFrom IsNot Nothing Then outFrom.Remove(tag)
        If into Is Nothing Then
            into = New clsSortedSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
        End If
        into.Add(tag)

    End Sub

#End Region

#Region "Public Accessors/Mutators"

    ''' <summary>
    ''' Applies the given tag within this mask (ie. sets the tag ON)
    ''' </summary>
    ''' <param name="tag">The tag to set ON in this object.</param>
    ''' <exception cref="ArgumentException">Thrown if the given
    ''' tag fails validation tests, ie. if it is empty, or it starts
    ''' with a '+' or '-' character, or it contains a ';' character.
    ''' </exception>
    Public Sub SetTagOn(ByVal tag As String)
        EnsureCorrect(tag)
        InternalApplyTag(tag, mOnTags, mOffTags)
    End Sub

    ''' <summary>
    ''' Masks the given tag within this mask (ie. sets the tag OFF)
    ''' </summary>
    ''' <param name="tag">The tag to set OFF in this object.</param>
    ''' <exception cref="ArgumentException">Thrown if the given
    ''' tag fails validation tests, ie. if it is empty, or it starts
    ''' with a '+' or '-' character, or it contains a ';' character.
    ''' </exception>
    Public Sub SetTagOff(ByVal tag As String)
        EnsureCorrect(tag)
        InternalApplyTag(tag, mOffTags, mOnTags)
    End Sub

    ''' <summary>
    ''' Applies the given tag, which is optionally prefixed by a '+' or '-' to
    ''' set the tag ON or OFF, respectively.
    ''' </summary>
    ''' <param name="prefixedTag">The tag to apply, optionally prefixed by a '+'
    ''' or '-' character. Default assumed prefix is '+'.</param>
    ''' <exception cref="ArgumentException">Thrown if the given tag is empty or 
    ''' fails validation rules.</exception>
    Public Sub ApplyTag(ByVal prefixedTag As String)
        ' EnsureCorrect(Nothing) forces an 'empty tag' error and means we define the
        ' error message in just one place (and thus change it in just one place)
        If prefixedTag Is Nothing Then EnsureCorrect(Nothing)
        prefixedTag = prefixedTag.Trim()
        If prefixedTag.Length = 0 Then EnsureCorrect(Nothing)
        If prefixedTag.Chars(0) = "-"c Then
            SetTagOff(prefixedTag.Substring(1))
        ElseIf prefixedTag.Chars(0) = "+"c Then
            SetTagOn(prefixedTag.Substring(1))
        Else ' no prefix there... assume a setOn
            SetTagOn(prefixedTag)
        End If
    End Sub

    ''' <summary>
    ''' The collection of tags which are set ON in this mask.
    ''' This will always return a collection, even if no tags are set ON.
    ''' </summary>
    Public ReadOnly Property OnTags() As ICollection(Of String)
        Get
            If mOnTags IsNot Nothing Then Return GetReadOnly.ISet(mOnTags)
            Return GetEmpty.ICollection(Of String)()
        End Get
    End Property

    ''' <summary>
    ''' The collection of tags which are set OFF in this mask.
    ''' This will always contain a collection, even if no tags are set OFF
    ''' </summary>
    Public ReadOnly Property OffTags() As ICollection(Of String)
        Get
            If mOffTags IsNot Nothing Then Return GetReadOnly.ISet(mOffTags)
            Return GetEmpty.ICollection(Of String)()
        End Get
    End Property

    ''' <summary>
    ''' Applies the given tags to this mask.
    ''' The tags should be in a format similar to: 
    ''' "+wanted tag;+other wanted tag;-unwanted tag". Specifically, the following
    ''' rules should be applied:
    ''' <list>
    ''' <item>Tags are separated by ';'(semi-colon) characters. Any such 
    ''' characters in the string will be treated as separators rather than
    ''' as part of any tag.</item>
    ''' <item>Tags cannot be empty.</item>
    ''' <item>Tags can be prefixed with a '+'(plus) or '-'(minus) character to 
    ''' indicate that the tag should be applied (set ON), or masked (set OFF)
    ''' respectively. An implied '+' is assumed if neither are present.</item>
    ''' <item>As a result of the above rule, tags cannot start with '+' or
    ''' '-'.</item>
    ''' </list>
    ''' If any of the above validation errors are found within a tag, that tag
    ''' is either skipped, or an exception is thrown, depending on the value of
    ''' the <paramref name="skipOnError">skipOnError</paramref> parameter.
    ''' Note that attempting to apply an empty string does not cause an error
    ''' to occur. Also note that any valid tags which are found <em>up to
    ''' but not including</em> an invalid tag will be applied to the mask
    ''' before any exception is thrown.
    ''' </summary>
    ''' <param name="tags">A string of tags to apply in this mask,
    ''' in the format: "+wanted tag;+other wanted tag;-unwanted tag"</param>
    ''' <param name="skipOnError">True to skip any tags within the string
    ''' which fail validation. False to force an exception if any tag
    ''' validation errors are found.</param>
    ''' <exception cref="ArgumentException">If skipOnError is set to
    ''' False and a tag is found which fails validation.</exception>
    ''' <remarks></remarks>
    Public Sub ApplyTags(ByVal tags As String, Optional ByVal skipOnError As Boolean = True)

        ' First, deal with empty tag string - not an error
        If tags Is Nothing Then Return
        tags = tags.Trim()
        If tags.Length = 0 Then Return

        ' Okay, we split the string over semi-colons (tag separators)
        ' Then match against the RegexTag which defines the format allowed for a 
        ' single tag. The tag will be in the format:
        ' +tag
        ' tag
        ' -tag
        ' The first two are tags which must be applied, ie. set ON;
        ' The last is a tag which must be masked, ie. set OFF
        ' If the fragment doesn't match this format, it is ignored/errored.
        ' If the tag text is empty (ie. only whitespace / zero-length string), then
        ' it is ignored/errored.
        ' If the tag text is invalid (starts with "+" or "-") it is ignored/errored
        ' The sets are built up ignoring case, as such repetitions are also ignored.
        ' Note that the if a tag is both set on and set off in the same string,
        ' the later occurrence will be the one which is applied.
        Dim frags As String() = tags.Split(";"c)
        For i As Integer = 0 To frags.Length - 1
            ' Get the tag + the operator
            Dim tag As String = frags(i).Trim()
            ' Match it
            Dim m As Match = RegexTag.Match(tag)
            ' Validate the tag - throw exception if not skipping, otherwise just skip
            ' the tag and move onto the next one.
            Try
                ' only thing that wouldn't match is blank - force a blank error
                If Not m.Success Then EnsureCorrect(Nothing)
                ' Split into operator and tagname
                Dim oper As String = m.Groups(1).ToString()
                Dim tagName As String = m.Groups(2).ToString().Trim()
                ' Validate the tag
                EnsureCorrect(tagName)
                ' Apply it to the appropriate set
                If oper = "-" Then ' ie. set OFF
                    InternalApplyTag(tagName, mOffTags, mOnTags)
                Else ' ie. set ON
                    InternalApplyTag(tagName, mOnTags, mOffTags)
                End If
            Catch ex As Exception
                If skipOnError Then Continue For
                Throw New ArgumentException(String.Format(
                 My.Resources.clsTagMask_Error0OccurredWhileApplyingTags1, ex.Message, tags))
            End Try

        Next
    End Sub

    ''' <summary>
    ''' Clears this tag mask, removing any applied or masked tags from it
    ''' </summary>
    Public Sub Clear()
        mOnTags = Nothing
        mOffTags = Nothing
    End Sub

    ''' <summary>
    ''' Checks if this tag mask is empty or not.
    ''' </summary>
    ''' <returns>True if this tag mask has no tags set or masked in it. False if any
    ''' tags are registered with this object.</returns>
    Public Function IsEmpty() As Boolean
        Return (mOnTags Is Nothing OrElse mOnTags.Count = 0) _
         AndAlso (mOffTags Is Nothing OrElse mOffTags.Count = 0)
    End Function

#End Region

#Region "ToString(), ToBuffer() and static overloads"

    ''' <summary>
    ''' Combines the given collection of tags into a semicolon-separated string,
    ''' prefixing each tag with the specified prefix.
    ''' </summary>
    ''' <param name="tags">The collection of tags to combine into a string.</param>
    ''' <param name="prefix">The prefix to add before each tag.</param>
    ''' <returns>A string combining the given set of tags with semicolons, and the
    ''' supplied prefix.</returns>
    Friend Overloads Shared Function ToString( _
     ByVal tags As ICollection(Of String), ByVal prefix As String) As String
        Return ToBuffer(New StringBuilder(), tags, prefix).ToString()
    End Function

    ''' <summary>
    ''' Combines the given collection of tags into the specified string buffer,
    ''' separating them with semicolons and prefixing them with the given prefix.
    ''' </summary>
    ''' <param name="sb">The buffer into which the tags should be appended.</param>
    ''' <param name="tags">The tags to append into the buffer</param>
    ''' <param name="prefix">The prefix to prepend each tag with - empty string if
    ''' no prefix is required.</param>
    ''' <returns>The given stringbuffer with the provided tags appended to it.
    ''' </returns>
    Friend Shared Function ToBuffer( _
     ByVal sb As StringBuilder, ByVal tags As ICollection(Of String), ByVal prefix As String) _
     As StringBuilder

        If tags Is Nothing OrElse tags.Count = 0 Then Return sb

        Dim sep As String = If(sb.Length = 0, "", ";")
        For Each tag As String In tags
            sb.Append(sep).Append(prefix).Append(tag)
            sep = ";"
        Next
        Return sb

    End Function

    ''' <summary>
    ''' Gets the string representation of this tag mask in the format:
    ''' "+wanted tag;+other wanted tag;-unwanted tag".
    ''' Note that the output in this method can be set into another TagMask's
    ''' <see cref="ApplyTags">ApplyTags</see> method, or used to create a new TagMask
    ''' with this masks tags.
    ''' </summary>
    ''' <returns>The contents of this tag mask as a string.</returns>
    Public Overrides Function ToString() As String
        Return ToBuffer(New StringBuilder()).ToString()
    End Function

    ''' <summary>
    ''' Appends the contents of this tag mask to the given buffer and returns it.
    ''' </summary>
    ''' <param name="sb">The buffer to which this mask should be appended
    ''' </param>
    ''' <returns>The given buffer with this mask appended to it.</returns>
    ''' <exception cref="ArgumentNullException">If the given buffer
    ''' is <c>Nothing</c></exception>
    Public Overridable Function ToBuffer(ByVal sb As StringBuilder) As StringBuilder

        If mOnTags IsNot Nothing Then ToBuffer(sb, mOnTags, "+")
        If mOffTags IsNot Nothing Then ToBuffer(sb, mOffTags, "-")
        Return sb

    End Function

#End Region

End Class
