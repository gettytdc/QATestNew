
Imports System.Globalization

Imports BluePrism.Core
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.Server.Domain.Models
Imports System.Linq

''' <summary>
''' Class to represent an expression.
''' 
''' FIXME: Currently this is all but a placeholder, with the same structure as the
''' code which was taken straight out of clsProcess.
''' </summary>
''' <remarks></remarks>
Public Class clsExpression

    ''' <summary>
    ''' Enum to represent the state inside the tokenizer within
    ''' <see cref="EvaluateExpression"/>
    ''' </summary>
    Private Enum TokenState
        NotInToken = 0
        InToken
        InStringLiteral
        InVariable
        InOperator ' Not yet used
    End Enum

    ''' <summary>
    ''' Checks if a string is bookended by a specified pair of characters. ie. if the
    ''' first and last character in the string are equal to the
    ''' <paramref name="begins"/> and <paramref name="ends"/> characters respectively
    ''' </summary>
    ''' <param name="str">The string to test.</param>
    ''' <param name="begins">The character to check for at the beginning of the
    ''' string.</param>
    ''' <param name="ends">The character to check for at the end of the string.
    ''' </param>
    ''' <returns>True if the first character in the string is equal to
    ''' <paramref name="begins"/> and the last character in the string is equal to
    ''' <paramref name="ends"/>; False otherwise.</returns>
    Private Shared Function Surrounds(
     ByVal str As String, ByVal begins As Char, ByVal ends As Char) As Boolean
        Return str IsNot Nothing AndAlso str.Length > 1 _
         AndAlso str(0) = begins AndAlso str(str.Length - 1) = ends
    End Function

    ''' <summary>
    ''' Checks if the strings in the given list are surrounded by a specified pair
    ''' of strings - ie. if the first and last element in the list are equal to the
    ''' <paramref name="begins"/> and <paramref name="ends"/> arguments respectively.
    ''' </summary>
    ''' <param name="strs">The list of strings to test.</param>
    ''' <param name="begins">The string to search for at the beginning of the list.
    ''' </param>
    ''' <param name="ends">The string to search for at the end of the list.</param>
    ''' <returns>True if the first element in the list is equal to
    ''' <paramref name="begins"/> and the last element in the list is equal to
    ''' <paramref name="ends"/>; False otherwise.</returns>
    Private Shared Function Surrounds(
     ByVal strs As IList(Of String),
     ByVal begins As String, ByVal ends As String) As Boolean
        Return strs IsNot Nothing AndAlso strs.Count > 1 _
        AndAlso strs(0) = begins AndAlso strs(strs.Count - 1) = ends
    End Function

    ''' <summary>
    ''' Verifies that the given response value is a valid non-null value, and has a
    ''' known type (unless the expression is merely being validated)
    ''' </summary>
    ''' <param name="resp">The value to verify</param>
    ''' <param name="justValidate">True to indicate that an expression is just being
    ''' validated and the datatype of the value is allowed to be
    ''' <see cref="DataType.unknown"/>; False otherwise.</param>
    ''' <returns>The given process value</returns>
    ''' <exception cref="InvalidExpressionException">If the given value is found to
    ''' be an invalid response from expression evaluation</exception>
    ''' <remarks>This represents the 'exitok' block in the big
    ''' <see cref="EvaluateExpression"/> method, as was.</remarks>
    Private Shared Function Verify(
     ByVal resp As clsProcessValue, ByVal justValidate As Boolean) As clsProcessValue
        'Make sure we have a result...
        If resp Is Nothing Then Throw New InvalidExpressionException(My.Resources.Resources.clsExpression_NoResult)

        'Unless we're validating, the data type of the result must be valid. When
        'validating, we could for example be looking at a Collection field which is
        'defined at runtime, in which case we genuinely don't know the data type
        If Not justValidate AndAlso resp.DataType = DataType.unknown Then _
         Throw New InvalidExpressionException(My.Resources.Resources.clsExpression_InvalidDataType)

        Return resp
    End Function

    ''' <summary>
    ''' Evaluate an expression.
    ''' </summary>
    ''' <param name="expr">The expression to evaluate</param>
    ''' <param name="res">A clsProcessValue reference which receives the result.
    ''' </param>
    ''' <param name="scopeStg">The stage representing the current scope.</param>
    ''' <param name="justValidate">If True, the expression is validated only, no
    ''' calculations or data lookups will be done. The result will therefore have an
    ''' undetermined value, but the data type will be correct.</param>
    ''' <param name="exprInfo">On return, a reference to a new instance of
    ''' clsExpressionInfo, which contains information about the expression. The value
    ''' on input is irrelevant - always pass Nothing.</param>
    ''' <param name="sErr">If an error occurs, this is a description of the error
    ''' </param>
    ''' <returns>True if successful, otherwise False</returns>
    Public Shared Function EvaluateExpression(
      ByVal expr As String,
      ByRef res As clsProcessValue,
      ByVal scopeStg As clsProcessStage,
      ByVal justValidate As Boolean,
      ByRef exprInfo As clsExpressionInfo,
      ByRef sErr As String) As Boolean
        Return EvaluateExpression(BPExpression.FromNormalised(expr),
         res, scopeStg, justValidate, exprInfo, sErr)
    End Function

    ''' <summary>
    ''' Evaluate an expression.
    ''' </summary>
    ''' <param name="expressionObj">The expression to evaluate</param>
    ''' <param name="res">A clsProcessValue reference which receives the result.
    ''' </param>
    ''' <param name="scopeStg">The stage representing the current scope.</param>
    ''' <param name="justValidate">If True, the expression is validated only, no
    ''' calculations or data lookups will be done. The result will therefore have an
    ''' undetermined value, but the data type will be correct.</param>
    ''' <param name="exprInfo">On return, a reference to a new instance of
    ''' clsExpressionInfo, which contains information about the expression. The value
    ''' on input is irrelevant - always pass Nothing.</param>
    ''' <param name="sErr">If an error occurs, this is a description of the error
    ''' </param>
    ''' <returns>True if successful, otherwise False</returns>
    Public Shared Function EvaluateExpression(
     ByVal expressionObj As BPExpression,
     ByRef res As clsProcessValue,
     ByVal scopeStg As clsProcessStage,
     ByVal justValidate As Boolean,
     ByRef exprInfo As clsExpressionInfo,
     ByRef sErr As String) As Boolean

        Dim expr As String = expressionObj.NormalForm

        ' Create the instance of clsExpressionInfo that will be used
        ' throughout to collate information that the caller might want.
        exprInfo = New clsExpressionInfo()

        ' Deal with silly input first
        If expr Is Nothing Then res = "" : Return True

        'Start by tokenising the expression...
        Dim tokens As New List(Of String)
        ' State is determined by TokenState enum values
        Dim state As TokenState = TokenState.NotInToken
        Dim posn As Integer = 0
        Dim currTok As String = ""
        Dim currChar As Char
        Dim nextChar As Char

        While posn < expr.Length
            currChar = expr(posn)

            If posn + 1 < expr.Length _
             Then nextChar = expr(posn + 1) _
             Else nextChar = Char.MinValue

            If state = TokenState.InStringLiteral Then
                If currChar = """"c Then
                    currTok &= """"
                    If nextChar = """"c Then
                        posn += 1
                    Else
                        tokens.Add(currTok)
                        state = TokenState.NotInToken
                    End If
                ElseIf currChar = vbCr Then
                    currTok &= " "
                ElseIf currChar <> vbLf Then
                    currTok &= currChar
                End If
            ElseIf state = TokenState.InVariable Then
                If currChar = "]"c Then
                    tokens.Add(currTok + "]")
                    state = TokenState.NotInToken
                ElseIf currChar = vbCr Then
                    currTok &= " "
                ElseIf currChar <> vbLf Then
                    currTok &= currChar
                End If
            Else
                Select Case currChar
                    Case """"c
                        If state = TokenState.InToken Then
                            sErr = My.Resources.Resources.clsExpression_EvaluateExpression_MisplacedQuote
                            Return False
                        End If
                        currTok = """"
                        state = TokenState.InStringLiteral

                    Case " "c, Chr(10), Chr(13)
                        If state = TokenState.InToken Then
                            tokens.Add(currTok)
                            state = TokenState.NotInToken
                        End If

                    Case "["c
                        Select Case state
                            Case TokenState.InVariable
                                sErr = My.Resources.Resources.clsExpression_EvaluateExpression_MisplacedLeftBracket
                                Return False
                            Case TokenState.InToken
                                tokens.Add(currTok)
                                state = TokenState.NotInToken
                        End Select
                        currTok = "["
                        state = TokenState.InVariable


                    Case "+"c, "-"c, "*"c, "/"c, "^"c, "&"c, "("c, ")"c, ","c, ">"c, "<"c, "="c
                        If state = TokenState.InToken Then
                            tokens.Add(currTok)
                            state = TokenState.NotInToken
                        End If
                        Select Case (currChar & nextChar)
                            ' If the combination of this char and the next char is
                            ' a combination operator, merge them into a single token
                            ' and move the pointer on by one
                            Case ">=", "<=", "<>"
                                tokens.Add(currChar & nextChar)
                                posn += 1
                            Case Else
                                tokens.Add(currChar)
                        End Select

                    Case Else
                        If state = TokenState.NotInToken Then
                            currTok = ""
                            state = TokenState.InToken
                        End If
                        currTok &= currChar
                End Select
            End If
            posn += 1
        End While
        If state = TokenState.InVariable Then
            sErr = My.Resources.Resources.clsExpression_EvaluateExpression_MissingRightBracket
            Return False
        End If
        If state = TokenState.InStringLiteral Then
            sErr = My.Resources.Resources.clsExpression_EvaluateExpression_MissingQuote
            Return False
        End If
        If state = TokenState.InToken Then tokens.Add(currTok)

        If tokens.Count = 0 Then res = "" : Return True

        'Now evaluate the tokenised expression...
        Try
            res = EvaluateExpression(tokens, scopeStg, justValidate, exprInfo)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Evaluate an tokenised expression. Calls itself recursively to deal with the
    ''' full evaluation. Note this is for internal use only, external classes need to
    ''' evaluate expressions use the other version of this method, that takes a
    ''' String for the expression.
    ''' </summary>
    ''' <param name="tokens">A collection of tokens that make up the expression. The
    ''' tokenisation can only be done correctly by the other version of
    ''' EvaluateExpression within this class.</param>
    ''' <param name="scopeStg">The stage representing the current scope.</param>
    ''' <param name="justValidate">If True, the expression is validated only, no
    ''' calculations or data lookups will be done. The result will therefore have an
    ''' undetermined value, but the data type will be correct.</param>
    ''' <param name="exprInfo">A reference to a clsExpressionInfo instance, which
    ''' should be created before this method is called. Information will be added to
    ''' it as evaluation progresses.</param>
    ''' <returns>A clsProcessValue object representing the result.</returns>
    ''' <exception cref="IllegalArgumentException">If no scope stage is given
    ''' </exception>
    ''' <exception cref="InvalidExpressionException">If an error in the expression
    ''' meant that it could not be evaluated.</exception>
    Private Shared Function EvaluateExpression(ByVal tokens As IList(Of String),
     ByVal scopeStg As clsProcessStage,
     ByVal justValidate As Boolean, ByRef exprInfo As clsExpressionInfo) _
     As clsProcessValue

        'Make sure a scope has been specified...
        If scopeStg Is Nothing Then Throw New IllegalArgumentException(
         My.Resources.Resources.clsExpression_CannotEvaluateAnExpressionWithoutAScope)

        Dim proc As clsProcess = scopeStg.Process

        'If the expression is completely parenthesised, drop them
        'and continue. This can happen multiple times - we remain
        'in this Do loop until we are convinced that we don't have
        'a completely parenthesised expression.
        Do
            If tokens.Count < 3 Then Exit Do
            If Not Surrounds(tokens, "(", ")") Then Exit Do
            'Need to make sure the start and end actually match
            'with each other...
            Dim depth As Integer = 0
            For i As Integer = 1 To tokens.Count - 2
                Select Case CStr(tokens(i))
                    Case "(" : depth += 1
                    Case ")" : depth -= 1 : If depth < 0 Then Exit Do
                End Select
            Next
            'Found some, so remove and check again...
            tokens.RemoveAt(tokens.Count - 1)
            tokens.RemoveAt(0)
        Loop

        'Process all binary operators, in order of priority,
        'by recursively evaluating the expression on either
        'side and then performing the operation.

        For Each op As String In clsProcessOperators.All
            Dim level As Integer = 0
            For i As Integer = tokens.Count - 1 To 0 Step -1
                Select Case tokens(i)
                    Case op
                        ' Check parenthesis level before processing the operator
                        ' Operators not at parenthesis level 0 are ignored, as
                        ' they will be processed at a deeper level.
                        If level <> 0 Then Continue For

                        ' Create a collection of tokens from the left hand
                        ' side, and evaluate that expression to get the first
                        ' operand...
                        Dim lhsTokens As New List(Of String)
                        For j As Integer = 0 To i - 1 : lhsTokens.Add(tokens(j)) : Next
                        Dim lhsRes As clsProcessValue = Nothing

                        ' Check for no tokens on the left of the operator, which
                        ' we will allow as a unary operator for now.
                        If lhsTokens.Count > 0 Then
                            ' if the last token to the left is also an operator
                            ' then either the user has made a mistake orelse we
                            ' are looking at a unary operator.
                            If clsProcessOperators.IsAnOperator(lhsTokens(lhsTokens.Count - 1)) Then
                                'we have a unary operator
                                Continue For
                            End If
                            lhsRes = EvaluateExpression(lhsTokens, scopeStg, justValidate, exprInfo)
                        End If

                        ' Create a collection of tokens from the right hand side,
                        ' and evaluate to get second operand...
                        Dim rhsTokens As New List(Of String)
                        For j As Integer = i + 1 To tokens.Count - 1 : rhsTokens.Add(tokens(j)) : Next
                        Dim rhsRes As clsProcessValue = Nothing

                        If rhsTokens.Count = 0 Then Throw New BluePrismException(
                         My.Resources.Resources.clsExpression_The0OperatorRequiresAnExpressionOnTheRightNoSuchExpressionFound, op)

                        rhsRes = EvaluateExpression(rhsTokens, scopeStg, justValidate, exprInfo)


                        'We now have the two operands in
                        'sRes1 and sRes2, so perform the
                        'operation...
                        Return Verify(
                         clsProcessOperators.DoOperation(op, lhsRes, rhsRes, justValidate),
                         justValidate)

                    Case "(" : level += 1
                    Case ")" : level -= 1

                End Select
            Next
        Next

        ' If we reach this point in the function without having already exited, that
        ' means there are no operators left, we just need to get the value for what
        ' is left. Alternatively this can happen if the user has ignored the case of
        ' an operator (eg logical 'OR' versus lower case 'or'). First, we look for a
        ' function, which is any value with more than one token (currently!)...
        If tokens.Count > 1 Then ' ie. it's a function call
            If tokens(1) <> "(" OrElse tokens(tokens.Count - 1) <> ")" Then _
             Throw New InvalidExpressionException(My.Resources.Resources.clsExpression_SyntaxErrorTheToken0IsInvalidPleaseCheckTheCaseOfYourOperatorsEgOrVersusORAndCh,
             tokens(1))

            'Process the parameters...
            Dim parms As IList(Of clsProcessValue) =
             EvaluateParms(tokens, scopeStg, justValidate, exprInfo)

            'Evaluate the function...
            Dim fn As clsFunction = proc.Functions.GetFunction(tokens(0))
            If fn Is Nothing Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_UndefinedFunction0, tokens(0))

            exprInfo.AddFunction(fn)

            If justValidate Then
                ' We try to match parameters with an available signature
                Dim compatibleSignatureFound As Boolean = False
                ' Indicates whether the number of supplied arguments matches the
                ' number of parameters exposed by at least one signature
                Dim countMatch As Boolean
                For Each sig As clsFunctionParm() In fn.Signatures
                    If sig.Length = parms.Count Then
                        countMatch = True
                        Dim isCompatible As Boolean = True
                        For paramIndex As Integer = 0 To parms.Count - 1
                            Dim dataType As DataType = parms(paramIndex).DataType
                            If dataType <> DataType.unknown Then
                                If Not parms(paramIndex).CanCastInto(sig(paramIndex).DataType) Then
                                    isCompatible = False
                                    Exit For
                                End If
                            End If
                        Next
                        If isCompatible Then
                            compatibleSignatureFound = True
                            Exit For
                        End If
                    End If
                Next

                If Not compatibleSignatureFound Then
                    If Not countMatch Then Throw New InvalidExpressionException(
                     My.Resources.Resources.clsExpression_IncorrectNumberOfParametersToFunction0, fn.Name)

                    Throw New InvalidExpressionException(
                     My.Resources.Resources.clsExpression_InvalidValuesSuppliedToParametersOfFunction0, fn.Name)

                End If

                ' When validating, we create a dummy value of the correct type rather
                ' than actually evaluating the function...
                Return New clsProcessValue(fn.DataType)
            Else
                Return Verify(fn.Evaluate(parms, proc), justValidate)
            End If
        End If

        'If we reach this point, we are dealing with a simple
        'raw value, either a data reference or a constant
        'of some sort, so we just figure out which one it
        'is and return that value...
        Dim str As String = CStr(tokens(0))

        If Surrounds(str, """"c, """"c) Then
            'A string constant...
            Return str.Substring(1, str.Length - 2)

        ElseIf str(0) >= "0"c And str(0) <= "9"c Then
            'A number...
            Dim dec As Decimal
            If Decimal.TryParse(str, NumberStyles.AllowDecimalPoint,
             InternalCulture.Instance, dec) Then Return dec

            ' Report the error - ensure that we convert it back into local form for
            ' the error message
            Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_CouldNotConvert0IntoANumber, NormalToLocal(str))

        ElseIf str.IndexOf(".") <> -1 Then
            'If it has a dot in, and wasn't a number, it ought
            'to be a reference to a field in a collection...
            If Not Surrounds(str, "["c, "]"c) Then Throw New _
             InvalidExpressionException(My.Resources.Resources.clsExpression_CollectionNameMustHaveAndAroundIt)

            Dim ind As Integer = str.IndexOf("."c)
            Dim collName As String = str.Substring(1, ind - 1)
            Dim fldName As String = str.Substring(ind + 1)
            fldName = fldName.Substring(0, fldName.Length - 1)

            Dim outOfScope As Boolean
            Dim dataStg As clsProcessStage =
             proc.GetDataStage(collName, scopeStg, outOfScope)
            Dim collStg As clsCollectionStage =
             TryCast(dataStg, clsCollectionStage)

            If dataStg Is Nothing Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_MissingCollection0, collName)

            If outOfScope Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_Collection0IsOutOfScope, collName)

            If collStg Is Nothing Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_Stage0IsNotACollection, dataStg.Name)

            If fldName.Length = 0 Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_NoFieldNameSpecifiedWithinCollection0, collName)

            'Record the fact this data item has been referenced:
            exprInfo.AddDataItem(dataStg.Id)
            exprInfo.AddCollectionField(collStg, fldName)

            If Not justValidate Then
                Dim fieldValue = collStg.GetFieldValue(fldName)
                If fieldValue?.DataType = DataType.collection AndAlso
                 Not proc.PassNestedCollectionsByReference Then
                    fieldValue = fieldValue.Clone()
                End If
                Return Verify(fieldValue, justValidate)
            Else
                'See if we can validate the field name - depends on
                'whether or not the collection is dynamic...
                Dim info As clsCollectionFieldInfo = Nothing

                ' If there is no definition, then of course we can't validate the
                ' field name, so just return an 'unknown' response.
                If collStg.Definition Is Nothing Then Return New clsProcessValue()

                Try
                    ' Find the field with the given name within the collection.
                    ' This will search through the nested collections until
                    ' it finds such a field.
                    info = collStg.Definition.FindField(fldName)
                    Return New clsProcessValue(info.DataType)

                Catch fnfe As FieldNotFoundException
                    ' Check the definition - if it contains any fields, then
                    ' this error stands - there are fields defined and the
                    ' given one is not one of them.
                    If fnfe.Definition IsNot Nothing _
                     AndAlso fnfe.Definition.FieldCount > 0 Then Throw

                    ' Otherwise, it's dynamic, thus it's more accurate to say
                    ' that we don't know the makeup of the field... ie:
                    Return New clsProcessValue()

                End Try

            End If

        Else
            'True and False
            Select Case str
                Case "True" : Return True
                Case "False" : Return False
            End Select

            'If it isn't one of the above, it had better be
            'the name of a data stage...
            If Not Surrounds(str, "["c, "]"c) Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_DataItemsMustBeBetweenAndTextMustBeBetweenAndFunctionsShouldBeInTheFormFunction)

            Dim outOfScope As Boolean
            Dim dataStg As clsDataStage = proc.GetDataStage(
             str.Substring(1, str.Length - 2), scopeStg, outOfScope)

            If dataStg Is Nothing Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_MissingData0, str)

            If outOfScope Then Throw New InvalidExpressionException(
             My.Resources.Resources.clsExpression_DataItem0CannotBeAccessedFromStage1BecauseItHasBeenHiddenFromTheCurrentPage, str, scopeStg.Name)

            'Record the fact this data item has been referenced:
            exprInfo.AddDataItem(dataStg.Id)
            Return Verify(dataStg.GetValue().Clone(), justValidate)

        End If

    End Function

    ''' <summary>
    ''' Evaluate all the parameters to a function.
    ''' </summary>
    ''' <param name="tokens">The tokens that make up the function call</param>
    ''' <param name="justValidate">True when in validation mode - empty strings are
    ''' returned for each parameter</param>
    ''' <param name="exprInfo">A reference to a clsExpressionInfo instance, which
    ''' should be created before this method is called. Information will be added to
    ''' it as evaluation progresses.</param>
    ''' <returns>A collection of strings, one for each parameter</returns>
    ''' <exception cref="IllegalArgumentException">If no scope stage is provided.
    ''' </exception>
    ''' <exception cref="InvalidFormatException">If too many ")" characters were
    ''' found in a parameter</exception>
    ''' <exception cref="InvalidExpressionException">If the expression in the
    ''' given parameters was invalid.</exception>
    Private Shared Function EvaluateParms(
     ByVal tokens As IList(Of String),
     ByVal scopeStg As clsProcessStage,
     ByVal justValidate As Boolean,
     ByRef exprInfo As clsExpressionInfo) As IList(Of clsProcessValue)

        Dim parms As New List(Of clsProcessValue)
        Dim paramTokens As New List(Of String)
        Dim res As clsProcessValue = Nothing
        Dim i As Integer
        Dim level As Integer = 0

        If tokens.Count <> 3 Then
            For i = 2 To tokens.Count - 1
                If (tokens(i) = "," AndAlso level = 0) OrElse i = tokens.Count - 1 Then
                    res = EvaluateExpression(paramTokens, scopeStg, justValidate, exprInfo)
                    parms.Add(res)
                    paramTokens = New List(Of String)
                Else
                    paramTokens.Add(tokens(i))
                    Select Case tokens(i)
                        Case "(" : level += 1
                        Case ")"
                            level -= 1
                            If level < 0 Then Throw New InvalidFormatException(
                             My.Resources.Resources.clsExpression_UnexpectedInParameters)
                    End Select
                End If
            Next
        End If
        Return parms
    End Function

    ''' <summary>
    ''' Checks the supplied expression for errors.
    ''' </summary>
    ''' <param name="expr">The expression to be checked.</param>
    ''' <param name="scopeStg">The stage to be used as a scope stage, if any.</param>
    ''' <param name="targetDataType">The datatype that is expected, or 'unknown' to
    ''' not check that.</param>
    ''' <param name="loc">The location of the error. This can either be an empty
    ''' string, or a description in the form, for example, " in row 1". Note the
    ''' leading space!</param>
    ''' <param name="exprInfo">On return, may contain a clsExpressionInfo with
    ''' further details about the expression. May also contain Nothing if the
    ''' expression validation failed completely.</param>
    ''' <param name="result">On return, may contain a clsProcessValue with the
    ''' 'result' (actually just the datatype) of the expression. May also contain
    ''' Nothing if the expression validation failed completely.</param>
    ''' <returns>Returns a list of errors, which may be empty, but not null</returns>
    Public Shared Function CheckExpressionForErrors(ByVal expr As BPExpression,
      ByVal scopeStg As clsProcessStage, ByVal targetDataType As DataType,
      ByVal loc As String, ByRef exprInfo As clsExpressionInfo,
      ByRef result As clsProcessValue) As ValidationErrorList
        Return CheckExpressionForErrors(
         expr.NormalForm, scopeStg, targetDataType, loc, exprInfo, result)
    End Function

    ''' <summary>
    ''' Checks the supplied expression for errors.
    ''' </summary>
    ''' <param name="expr">The expression to be checked.</param>
    ''' <param name="scopeStg">The stage to be used as a scope stage, if any.</param>
    ''' <param name="targTypes">The datatypes that are expected, or an empty
    ''' enumerable to not check that.</param>
    ''' <param name="loc">The location of the error. This can either be an empty
    ''' string, or a description in the form, for example, " in row 1". Note the
    ''' leading space!</param>
    ''' <param name="exprInfo">On return, may contain a clsExpressionInfo with
    ''' further details about the expression. May also contain Nothing if the
    ''' expression validation failed completely.</param>
    ''' <param name="result">On return, may contain a clsProcessValue with the
    ''' 'result' (actually just the datatype) of the expression. May also contain
    ''' Nothing if the expression validation failed completely.</param>
    ''' <returns>Returns a list of errors, which may be empty, but not null</returns>
    Public Shared Function CheckExpressionForErrors(ByVal expr As BPExpression,
      ByVal scopeStg As clsProcessStage, ByVal targTypes As IEnumerable(Of DataType),
      ByVal loc As String, ByRef exprInfo As clsExpressionInfo,
      ByRef result As clsProcessValue) As ValidationErrorList
        Return CheckExpressionForErrors(
         expr.NormalForm, scopeStg, targTypes, loc, exprInfo, result)
    End Function

    ''' <summary>
    ''' Checks the supplied expression for errors.
    ''' </summary>
    ''' <param name="expr">The expression to be checked.</param>
    ''' <param name="scopeStg">The stage to be used as a scope stage, if any.</param>
    ''' <param name="targetDataType">The datatype that is expected, or 'unknown' to
    ''' not check that.</param>
    ''' <param name="loc">The location of the error. This can either be an empty
    ''' string, or a description in the form, for example, " in row 1". Note the
    ''' leading space!</param>
    ''' <param name="exprInfo">On return, may contain a clsExpressionInfo with
    ''' further details about the expression. May also contain Nothing if the
    ''' expression validation failed completely.</param>
    ''' <param name="result">On return, may contain a clsProcessValue with the
    ''' 'result' (actually just the datatype) of the expression. May also contain
    ''' Nothing if the expression validation failed completely.</param>
    ''' <returns>Returns a list of errors, which may be empty, but not null</returns>
    Public Shared Function CheckExpressionForErrors(
     expr As String,
     scopeStg As clsProcessStage,
     targetDataType As DataType,
     loc As String,
     ByRef exprInfo As clsExpressionInfo,
     ByRef result As clsProcessValue) As ValidationErrorList
        Dim types = If(
            targetDataType = DataType.unknown,
            New DataType() {},
            New DataType() {targetDataType}
        )

        Return CheckExpressionForErrors(expr, scopeStg, types, loc, exprInfo, result)

    End Function
    Public Shared Function useWordDelimiter() As Boolean
        Dim curCulture = Globalization.CultureInfo.CurrentUICulture
        Return CType(IIf(curCulture.TwoLetterISOLanguageName.Equals("ja") OrElse
                         curCulture.TwoLetterISOLanguageName.Equals("zh") OrElse
                         curCulture.TwoLetterISOLanguageName.Equals("ko") OrElse
                         curCulture.TwoLetterISOLanguageName.Equals("th"), False, True), Boolean)
    End Function
    ''' <summary>
    ''' Checks the supplied expression for errors.
    ''' </summary>
    ''' <param name="expr">The expression to be checked.</param>
    ''' <param name="scopeStg">The stage to be used as a scope stage, if any.</param>
    ''' <param name="targTypes">The datatypes that are expected, or an empty
    ''' enumerable to not check that.</param>
    ''' <param name="loc">The location of the error. This can either be an empty
    ''' string, or a description in the form, for example, " in row 1". It will be
    ''' provided to any error description prepended with a space character if it did
    ''' not start with one.</param>
    ''' <param name="exprInfo">On return, may contain a clsExpressionInfo with
    ''' further details about the expression. May also contain Nothing if the
    ''' expression validation failed completely.</param>
    ''' <param name="result">On return, may contain a clsProcessValue with the
    ''' 'result' (actually just the datatype) of the expression. May also contain
    ''' Nothing if the expression validation failed completely.</param>
    ''' <returns>Returns a list of errors, which may be empty, but not null</returns>
    Public Shared Function CheckExpressionForErrors(
     expr As String,
     scopeStg As clsProcessStage,
     targTypes As IEnumerable(Of DataType),
     loc As String,
     ByRef exprInfo As clsExpressionInfo,
     ByRef result As clsProcessValue) As ValidationErrorList

        ' If a location is provided with no leading space, add one.
        If loc <> "" AndAlso loc(0) <> " "c AndAlso useWordDelimiter() Then loc = " " & loc

        Dim errors As New ValidationErrorList()
        Dim proc As clsProcess = scopeStg.Process

        If expr = "" Then
            errors.Add(scopeStg, 24, loc)
        Else
            Dim sErr As String = Nothing
            If EvaluateExpression(expr, result, scopeStg, True, exprInfo, sErr) Then
                'Check that the data type matches
                If targTypes.Any() Then
                    Dim res = result ' Can't use a ByRef in a lambda, hence 'res'
                    If res.DataType <> DataType.unknown Then
                        If Not targTypes.Any(Function(d) res.CanCastInto(d)) Then
                            errors.Add(scopeStg, 21)
                        End If

                    Else
                        'This is probably because the expression uses an undefined
                        'collection field, meaning the final data type cannot be resolved
                        'until runtime. Not necessarily an error, so we just warn about it here
                        errors.Add(scopeStg, 22, loc)

                    End If
                End If

                'Warn against the use of deprecated functions
                Dim recoveryOnly As Boolean = False
                For Each func As clsFunction In exprInfo.Functions.Values
                    If func.Deprecated Then
                        errors.Add(scopeStg, "1064", 45, loc, func.Name)
                        Exit For
                    End If
                    If func.RecoveryOnly Then recoveryOnly = True
                Next
                ' Check if we're using a recovery-only function where we can't...
                ' We only check when mBacklinksLookup is not null - this implies that we have
                ' context for the check - ie. that we're in a full validate. A single
                ' expression validate doesn't have that context - not least since it may
                ' well be operating on a clone of the actual stage, which doesn't actually
                ' fit in the process and thus will never be in a recovery section.
                If recoveryOnly AndAlso scopeStg.Process.IsFullValidateInProgress _
                 AndAlso Not scopeStg.IsInRecoverySection() Then
                    errors.Add(scopeStg, 128, loc)
                End If

            Else
                errors.Add(scopeStg, 23, loc, sErr)
            End If
        End If

        Return errors
    End Function

    Public Shared Function LocalToNormal(ByVal expr As String) As String
        Return BPExpression.FromLocalised(expr).NormalForm
    End Function

    Public Shared Function NormalToLocal(ByVal expr As String) As String
        Return BPExpression.FromNormalised(expr).LocalForm
    End Function



End Class
