Imports System.Text

Imports BluePrism.Server.Domain.Models
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports LocaleTools

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsFunctions.clsFunction
''' 
''' <summary>
''' Represents an individual function, whether it be internal or
''' external. Classes for specific functions inherit from this.
''' </summary>
''' <remarks>
''' </remarks>
Public MustInherit Class clsFunction

    ''' <summary>
    ''' Class to encapsulate a function signature
    ''' </summary>
    Public Class Signature : Inherits List(Of DataType)

        ''' <summary>
        ''' Creates a new signature populated by the given data types.
        ''' </summary>
        ''' <param name="types">The data types defined in this signature</param>
        Public Sub New(ByVal ParamArray types() As DataType)
            MyBase.New(types)
        End Sub

        ''' <summary>
        ''' Gets a string representation of this signature. This is a comma-separated
        ''' list of the data types set within this signature, surrounded by brackets.
        ''' eg. "(number,number,text)" represents a signature which takes 2 number
        ''' params and a text param.
        ''' </summary>
        ''' <returns>A string representation of this signature</returns>
        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder(Count * 8)
            sb.Append("(")
            CollectionUtil.JoinInto(Me, ",", sb)
            sb.Append(")")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Tests if this signature will match the given set of values
        ''' </summary>
        ''' <param name="vals">The values to test against this signature</param>
        ''' <param name="percent">A percentage of full match - ie. the number of
        ''' given arguments which matched the types defined in this signature exactly
        ''' rather than requiring casting first. This will be set to zero if the
        ''' method returns false.</param>
        ''' <returns>True if the given signature will match, False if the arguments
        ''' do not match this signature and/or cannot be cast to match it.</returns>
        Public Function WillMatch( _
         ByVal vals As ICollection(Of clsProcessValue), ByRef percent As Double) _
         As Boolean
            ' No params and no args? Full match
            If vals.Count = 0 AndAlso Count = 0 Then percent = 100 : Return True

            ' Different number of params? Not a match at all
            If vals.Count <> Count Then percent = 0 : Return False

            ' Keep track of the number of (full) matches for a percentage match
            Dim matches As Integer = 0

            ' A double loop - enumerator for the arguments; for loop for the params
            Dim enu As IEnumerator(Of clsProcessValue) = vals.GetEnumerator()
            For i As Integer = 0 To Count - 1

                If Not enu.MoveNext() Then Throw New InvalidStateException(
                 My.Resources.Resources.Signature_MismatchWhenComparingParams0ElementsAgainstArgs1ElementsLoopOutOfSyncAtIndex2,
                 Count, vals.Count, i)

                Dim val As clsProcessValue = enu.Current

                ' Null (not sure what that means for args but certainly no match)
                If val Is Nothing Then Continue For

                ' If there's a full match, we don't need to cast it. Increment
                ' the match counter and move onto the next arg
                If val.DataType = Me(i) Then matches += 1 : Continue For

                ' Otherwise, not a full match, but maybe we can cast it.
                ' If we can't then it just doesn't match this signature at all
                If Not val.TryCastInto(Me(i), val) Then percent = 0 : Return False

            Next

            ' At this point we have 'matches' exact datatype matches out of Count,
            ' so let's calculate a percentage from that

            ' First off, deal with 100% explicitly to avoid embarassing rounding errs
            If matches = Count Then percent = 100 : Return True

            ' Otherwise (100*m)/count
            percent = (100.0R * CDbl(matches) / CDbl(Count))

            ' And it "will match", even if it has to cast one or two of the args
            Return True

        End Function

    End Class

    ''' <summary>
    ''' If true then this function is only made available for backwards
    ''' compatibility; it should not be used or made available in any
    ''' new expressions.
    ''' </summary>
    Public Overridable ReadOnly Property Deprecated() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' If True then this function is only valid for use within a Recovery
    ''' section of a process.
    ''' </summary>
    Public Overridable ReadOnly Property RecoveryOnly() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' If True then this function will display a bespoke helper in the user
    ''' interface showing the user how to use this function in conjucntion with
    ''' the concatenate operator to append the result of the function to some text
    ''' (Currently only used for the NewLine() function)
    ''' </summary>
    Public Overridable ReadOnly Property TextAppendFunction() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' The unique name of the function, as used in expressions, for example "Left".
    ''' This must be globally unique.
    ''' </summary>
    ''' <value>A String containing the function name.</value>
    Public MustOverride ReadOnly Property Name() As String

    ''' <summary>
    ''' The short description of the function, e.g. "Characters from Left"
    ''' </summary>
    ''' <value>A String containing the description.</value>
    Public MustOverride ReadOnly Property ShortDesc() As String

    ''' <summary>
    ''' The type of data returned by the function.
    ''' </summary>
    ''' <value>The data type, from DataType</value>
    Public MustOverride ReadOnly Property DataType() As DataType

    ''' <summary>
    ''' The name of the Group this function belongs to. Groups are used to group
    ''' functions in tree views etc for the user.
    ''' </summary>
    ''' <value>The group name as a String</value>
    Public MustOverride ReadOnly Property GroupName() As String

    ''' <summary>
    ''' Help text for the function, for display to the user.
    ''' </summary>
    ''' <value>The help text as a String.</value>
    Public MustOverride ReadOnly Property HelpText() As String

    ''' <summary>
    ''' Additional detailed help text for the function. This will not be shown
    ''' in limited space areas such as the expression builder form, but may be
    ''' included in detailed documentation etc.
    ''' </summary>
    ''' <value>The help detail text as a String.</value>
    Public Overridable ReadOnly Property HelpDetailText() As String
        Get
            Return ""
        End Get
    End Property

    ''' <summary>
    ''' The parameters to the function, as an array of clsFunctionParm objects
    ''' each representing a particular parameter.
    ''' </summary>
    Public MustOverride ReadOnly Property DefaultSignature() As clsFunctionParm()

    ''' <summary>
    ''' Alternative signatures (ie parameter combinations) by which
    ''' the function may be called.
    ''' </summary>
    ''' <returns>All available signatures. The first value will be as for
    ''' the DefaultSignature property.</returns>
    Public Overridable ReadOnly Property Signatures() As List(Of clsFunctionParm())
        Get
            Dim sigs As New List(Of clsFunctionParm())
            sigs.Add(DefaultSignature)
            Return sigs
        End Get
    End Property

    ''' <summary>
    ''' Evaluate the function, given the input parameters.
    ''' </summary>
    ''' <param name="parms">A List of clsProcessValue objects, one for each input
    ''' parameter. These values can be modified by the function!</param>
    ''' <param name="proc">The process that the function is being evaluated
    ''' in, for context.</param>
    ''' <returns>A clsProcessValue containing the result.</returns>
    ''' <exception cref="clsFunctionException">If an error occurs while
    ''' attempting to evaluate this function</exception>
    ''' <remarks>This is a thin wrapper around the actual evaluate call to ensure
    ''' that any function exceptions are wrapped into clsFunctionException if
    ''' a different exception type is thrown by the implementation</remarks>
    Public Function Evaluate(
     ByVal parms As IList(Of clsProcessValue), ByVal proc As clsProcess) _
     As clsProcessValue
        Try
            Return InnerEvaluate(parms, proc)
        Catch fe As clsFunctionException
            Throw
        Catch ex As Exception
            Throw New clsFunctionException(ex,
             My.Resources.Resources.clsFunction_UnhandledErrorOccurredInThe0Function1, Name, ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Evaluate the function, given the input parameters.
    ''' </summary>
    ''' <param name="parms">A List of clsProcessValue objects, one for each input
    ''' parameter. These values can be modified by the function!</param>
    ''' <param name="proc">The process that the function is being evaluated
    ''' in, for context.</param>
    ''' <returns>A clsProcessValue containing the result.</returns>
    ''' <exception cref="clsFunctionException">If an error occurs while
    ''' attempting to evaluate this function</exception>
    Protected MustOverride Function InnerEvaluate(
     ByVal parms As IList(Of clsProcessValue),
     ByVal proc As clsProcess) As clsProcessValue

    ''' <summary>
    ''' Ensures that the parameters match one of the given signatures, casting
    ''' them to the required type if a match is found.
    ''' </summary>
    ''' <param name="parms">The parameters to ensure meet the datatype
    ''' requirements. On successful return, a new list is written to this
    ''' parameter with the argument values cast into their target types within.
    ''' </param>
    ''' <param name="sigs">The signatures available to this function</param>
    ''' <returns>The Signature which was matched</returns>
    Protected Function EnsureParams(
     ByRef parms As IList(Of clsProcessValue),
     ByVal sigs As IList(Of Signature)) As Signature

        ' Keep a map of the signatures and the percentage of a full match that a
        ' successful signature corresponds to.
        ' Basically, if two sigs have equal correspondence, it's arbitrary which one
        ' we pick (we choose the first in the list); but we want the highest value -
        ' hence the reverse comparer, we just get the first in the dictionary as our
        ' target signature to actually convert the params using.
        Dim map As New SortedDictionary(Of Double, Signature)(
         New clsReverseComparer(Of Double))

        ' Go through each of the valid signatures
        For Each s As Signature In sigs
            Dim pc As Double = 0.0R
            ' If it will match, and we don't already have a sig with this level
            ' of compatibility, add it into the map
            If s.WillMatch(parms, pc) AndAlso Not map.TryGetValue(pc, Nothing) Then _
             map(pc) = s

        Next

        ' If we have any matches, go in and cast into the target signature (or just
        ' return the params as is if they are already fully compatible)
        If map.Count > 0 Then
            With CollectionUtil.First(map)

                Dim compat As Double = .Key
                Dim sig As Signature = .Value

                ' If we have a complete match, we don't need to do any casting, so just
                ' return the signature without updating the list at all
                If compat = 100.0R Then Return sig

                ' Otherwise, we cast into a new list and return that as the byref
                Dim newParms As New List(Of clsProcessValue)(parms)
                Dim val As clsProcessValue = Nothing
                For i As Integer = 0 To sig.Count - 1
                    ' It will cast - WillMatch would've returned false if it couldn't
                    newParms(i) = parms(i).CastInto(sig(i))
                Next
                ' We've made it through, we've successfully cast all of the
                ' process values, so set them into the param list and return
                parms = newParms
                Return sig

            End With
        End If

        ' If we get here, we didn't find the number of parameters in any of the
        ' valid signatures required by the function

        If sigs.Count = 0 Then Throw New clsFunctionException(
         My.Resources.Resources.clsFunction_0FunctionTakesNoArguments, Name)

        If sigs.Count = 1 Then Throw New clsFunctionException(LTools.Format(My.Resources.Resources.clsFunction_plural_requires_argument_of_type,
                          "NAME", Name, "COUNT", sigs(0).Count, "ARGS", CollectionUtil.Join(sigs(0), ",")))

        Dim sb As New StringBuilder()
        sb.AppendFormat("{0} function requires any of", Name)
        Dim doneCounts As New clsSet(Of Integer)
        For Each sig As IList(Of DataType) In sigs
            If doneCounts.Add(sig.Count) Then sb.Append(
                LTools.Format(My.Resources.Resources.clsFunction_plural_arguments_of_type, "COUNT", sig.Count, "ARGS", CollectionUtil.Join(sig, ",")))
        Next
        Throw New clsFunctionException(sb.ToString())

    End Function

    ''' <summary>
    ''' Ensures that the parameters match one of the given signatures, casting
    ''' them to the required type if a match is found.
    ''' </summary>
    ''' <param name="parms">The parameters to ensure meet the datatype
    ''' requirements. On successful return, a new list is written to this
    ''' parameter with the argument values cast into their target types within.
    ''' </param>
    ''' <param name="sigs">The signatures available to this function</param>
    ''' <returns>The Signature which was matched</returns>
    Protected Function EnsureParams( _
     ByRef parms As IList(Of clsProcessValue), _
     ByVal ParamArray sigs() As Signature) As Signature
        Return EnsureParams(parms, DirectCast(sigs, IList(Of Signature)))
    End Function

    ''' <summary>
    ''' Ensures that the parameters match one of the signatures defined in this
    ''' function.
    ''' </summary>
    ''' <param name="parms">The arguments to ensure meet the signatures in this
    ''' function.</param>
    Protected Sub EnsureParams(ByVal parms As IList(Of clsProcessValue))
        Dim sigs As New List(Of Signature)
        For Each s() As clsFunctionParm In Me.Signatures
            Dim types(s.Length - 1) As DataType
            For i As Integer = 0 To s.Length - 1 : types(i) = s(i).DataType : Next
            sigs.Add(New Signature(types))
        Next
        EnsureParams(parms, sigs)
    End Sub


End Class
