
Imports System.Text.RegularExpressions

''' <summary>
''' Performs matching of subquery values.
''' </summary>
''' <remarks>The client should execute the condition query and call IsMatch on the
''' result.</remarks>
Public Class clsConditionMatchTarget
    Inherits clsMatchTarget

    ''' <summary>
    ''' The Query specifying the condition query. The client should
    ''' execute the condition query and call
    ''' <see cref="IsMatch">IsMatch</see> on the result.
    ''' </summary>
    Public Property ConditionQuery() As clsQuery
        Get
            Return mConditionQuery
        End Get
        Set(ByVal value As clsQuery)
            mConditionQuery = value
        End Set
    End Property
    Private mConditionQuery As clsQuery

End Class

''' <summary>
''' Performs matching of parameter values.
''' </summary>
Public Class clsParameterMatchTarget
    Inherits clsMatchTarget
    ''' <summary>
    ''' The parameter whose value is under consideration.
    ''' </summary>
    Public Parameter As String
    Public Sub New(ByVal Parameter As String)
        Me.Parameter = Parameter
    End Sub
End Class

''' <summary>
''' Performs matching of identifier values.
''' </summary>
Public Class clsIdentifierMatchTarget
    Inherits clsMatchTarget
    ''' <summary>
    ''' The parameter whose value is under consideration.
    ''' </summary>
    Public Identifier As clsQuery.IdentifierTypes
    Public Sub New(ByVal Identifier As clsQuery.IdentifierTypes)
        Me.Identifier = Identifier
    End Sub

    ''' <summary>
    ''' Gets an equivalent identifier match target to this target, which references
    ''' the equivalent child to the identifier in this match target, according to
    ''' the <see cref="ParentOfAttribute"/> assigned to the identifier.
    ''' </summary>
    ''' <returns>An identifier match target with the same match value as this target,
    ''' but which specifies the child equivalent ID to the ID set in this target.
    ''' </returns>
    Public Function GetChildEquivalent() As clsIdentifierMatchTarget
        Return New clsIdentifierMatchTarget(
            ParentOfAttribute.GetChildType(Identifier)) With {
            .MatchValue = Me.MatchValue}

    End Function
End Class

''' <summary>
''' Encapsulates the notion of a value that needs to be matched.
''' The means of matching may vary according to the comparison type
''' (eg equals, not equal, wildcard match, less than, greater than, etc
''' All of this implementation is centralised in this class, and allows
''' clients to treat all comparisons uniformly, regardless of the
''' comparison type required.
''' </summary>
Public Class clsMatchTarget
    ''' <summary>
    ''' The value to be matched against.
    ''' </summary>
    ''' <remarks> This may be a wildcard pattern when the appropriate
    ''' <see cref="ComparisonType">ComparisonType</see> is set.</remarks>
    Public Property MatchValue() As String
        Get
            Return msMatchValue
        End Get
        Set(ByVal value As String)
            msMatchValue = value
        End Set
    End Property
    Private msMatchValue As String = String.Empty

    ''' <summary>
    ''' The type of comparison to be applied to the
    ''' msMatchValue.
    ''' </summary>
    Public ComparisonType As ComparisonTypes

    ''' <summary>
    ''' The data type of the msMatchValue. Test matches
    ''' will also be treated as this type in calls to Match
    ''' </summary>
    ''' <remarks>This value is ignored if a comparison type
    ''' of wildcard is chosen.</remarks>
    Public DataType As DataTypes = DataTypes.Text

    ''' <summary>
    ''' The types of comparison available for the msMatchValue.
    ''' </summary>
    Public Enum ComparisonTypes
        Equal = 0
        LessThan
        LessThanOrEqual
        GreaterThanOrEqual
        GreaterThan
        NotEqual
        Range
        Wildcard
        RegEx
    End Enum

    ''' <summary>
    ''' These data types correspond to Automate data types
    ''' </summary>
    ''' <remarks>Not all are included - eg password is an inappropriate data type
    ''' outside of Automate itself.</remarks>
    Public Enum DataTypes
        ''' <summary>
        ''' The default data type
        ''' </summary>
        ''' <remarks></remarks>
        Text = 0
        Number
        Flag
        [Date]
        [DateTime]
        Time
        [TimeSpan]
    End Enum

    ''' <summary>
    ''' Tests the supplied convertable value for a match against the stored
    ''' <see cref="MatchValue"/>, using the current <see cref="ComparisonType"/>,
    ''' treating the data according to the <see cref="DataType"/> member.
    ''' </summary>
    ''' <param name="testObj">The value to be tested for a match. This will be
    ''' converted into a string using the <see cref="IConvertible.ToString"/>
    ''' method of the object under the current culture before calling IsMatch with
    ''' a string overload.</param>
    ''' <returns>Returns true on a match, false otherwise.</returns>
    <CLSCompliant(False)>
    Public Function IsMatch(ByVal testObj As IConvertible) As Boolean
        Return IsMatch(testObj.ToString(Nothing))
    End Function

    ''' <summary>
    ''' Tests the supplied string for a match against the stored
    ''' <see cref="msMatchValue">msMatchValue</see>, using the
    ''' current <see cref="ComparisonType">ComparisonType</see>,
    ''' treating the data according to the <see cref="DataType">DataType</see>
    ''' member.
    ''' </summary>
    ''' <param name="TestString">The string to be tested for a match.
    ''' This will be treated according to the <see cref="DataType">DataType</see>
    ''' member.</param>
    ''' <returns>Returns true on a match, false otherwise.</returns>
    ''' <remarks>Throws exceptions in unforeseen circumstances.</remarks>
    Public Function IsMatch(ByVal TestString As String) As Boolean

        Select Case Me.ComparisonType
            Case ComparisonTypes.Equal
                Return String.Compare(TestString, Me.msMatchValue) = 0
            Case ComparisonTypes.NotEqual
                Return String.Compare(TestString, Me.msMatchValue) <> 0
            Case ComparisonTypes.Wildcard
                'We treat as text - ignore data type
                Return clsWildcardMatcher.IsMatch(TestString, msMatchValue)
            Case ComparisonTypes.RegEx
                'We treat as text - ignore data type
                Return Regex.IsMatch(TestString, msMatchValue)
            Case ComparisonTypes.Range
                If Me.msMatchValue.Contains("..") Then
                    Dim split() As String = Me.msMatchValue.Split(New String() {".."}, StringSplitOptions.None)
                    Dim lowerRange As String = split(0)
                    Dim upperRange As String = split(1)
                    Dim icMatch As IComparable = Nothing
                    Dim icReply As IComparable = Nothing
                    ConvertTextToComparible(Me.DataType, TestString, lowerRange, icMatch, icReply)
                    If icReply.CompareTo(icMatch) >= 0 Then
                        ConvertTextToComparible(Me.DataType, TestString, upperRange, icMatch, icReply)
                        Return icReply.CompareTo(icMatch) <= 0
                    End If
                Else
                    Throw New InvalidOperationException(My.Resources.RangeValueDoesNotContain)
                End If
            Case ComparisonTypes.GreaterThan, ComparisonTypes.LessThan, ComparisonTypes.LessThanOrEqual, ComparisonTypes.GreaterThanOrEqual
                Dim icMatch As IComparable = Nothing
                Dim icReply As IComparable = Nothing
                ConvertTextToComparible(Me.DataType, TestString, Me.msMatchValue, icMatch, icReply)

                Select Case Me.ComparisonType
                    Case ComparisonTypes.LessThan
                        Return icReply.CompareTo(icMatch) < 0
                    Case ComparisonTypes.LessThanOrEqual
                        Return icReply.CompareTo(icMatch) <= 0
                    Case ComparisonTypes.GreaterThan
                        Return icReply.CompareTo(icMatch) > 0
                    Case ComparisonTypes.GreaterThanOrEqual
                        Return icReply.CompareTo(icMatch) >= 0
                    Case Else
                        Throw New InvalidOperationException(String.Format(My.Resources.UnexpectedComparisonType0, Me.ComparisonType.ToString))
                End Select
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.UnsupportedComparisonTypeOf0, Me.ComparisonType.ToString))
        End Select
    End Function

    Private Sub ConvertTextToComparible(ByVal dataType As DataTypes, ByVal testString As String, ByVal MatchValue As String, ByRef icMatch As IComparable, ByRef icReply As IComparable)
        Select Case dataType
            Case DataTypes.Number
                Try
                    icMatch = Decimal.Parse(MatchValue)
                    icReply = Decimal.Parse(testString)
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValuesAsNumber0, ex.Message))
                End Try
            Case DataTypes.DateTime
                Try
                    icMatch = DateTime.Parse(MatchValue)
                    icReply = DateTime.Parse(testString)
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValuesAsNumber0, ex.Message))
                End Try
            Case DataTypes.Date
                Try
                    icMatch = Date.Parse(MatchValue).Date
                    icReply = Date.Parse(testString).Date
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValuesAsNumber0, ex.Message))
                End Try
            Case DataTypes.Time
                Try
                    icMatch = Date.Parse(MatchValue).TimeOfDay
                    icReply = Date.Parse(testString).TimeOfDay
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValuesAsNumber0, ex.Message))
                End Try
            Case DataTypes.TimeSpan
                Try
                    icMatch = TimeSpan.Parse(MatchValue)
                    icReply = TimeSpan.Parse(testString)
                Catch ex As Exception
                    Throw New InvalidOperationException(String.Format(My.Resources.FailedToInterpretValuesAsNumber0, ex.Message))
                End Try
            Case DataTypes.Flag, DataTypes.Text
                Throw New InvalidOperationException(String.Format(My.Resources.OnlyTestsOfEqualityCanBePerformedForDataOfType0, dataType.ToString))
            Case Else
                Throw New InvalidOperationException(String.Format(My.Resources.UnsupportedDataTypeValue0, dataType.ToString))
        End Select
    End Sub
End Class
