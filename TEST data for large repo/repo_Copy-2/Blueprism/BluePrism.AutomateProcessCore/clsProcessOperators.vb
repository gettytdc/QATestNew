Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore.clsProcessDataTypes

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessOperators
''' 
''' <summary>
''' A class with shared members only, providing information about the
''' available operators.
''' </summary>
Public Class clsProcessOperators

    ''' <summary>
    ''' A set of all the binary operators supported by this class, in order of their
    ''' relative priority.
    ''' </summary>
    Private Shared ReadOnly sAllOperators As IBPSet(Of String) = _
     GetReadOnly.ISet(New clsOrderedSet(Of String)(New String() { _
      "AND", "and", "OR", "or", _
      "=", "<>", ">", ">=", "<", "<=", _
      "&", "+", "-", "*", "/", "^"}))

    ''' <summary>
    ''' The full set of binary operator symbols supported by this class
    ''' </summary>
    Public Shared ReadOnly Property All() As ICollection(Of String)
        Get
            Return sAllOperators
        End Get
    End Property

    ''' <summary>
    ''' Determines if the supplied value exists among the set of permitted operators
    ''' by searching the list defined in clsProcessOperators.GetAll()
    ''' </summary>
    ''' <param name="candidate">The string representation of the operator of 
    ''' interest (eg "+" or "&lt;=").</param>
    ''' <returns>Returns true if the string exactly matches one of the operators
    ''' in the collection returned by clsProcessParameter.GetAll(); otherwise
    ''' returns false.</returns>
    Public Shared Function IsAnOperator(ByVal candidate As String) As Boolean
        Return All.Contains(candidate)
    End Function

    ''' <summary>
    ''' Performs the specified operation.
    ''' </summary>
    ''' <param name="op">The operator to use</param>
    ''' <param name="val1">The value to the left of the operator.
    ''' This can be Nothing to specify a unary operation with a
    ''' value on the right-hand side only.</param>
    ''' <param name="val2">The value to the right of the operator</param>
    ''' <param name="justValidate">If True, the expression is validated
    ''' only, no calculations or data lookups will be done. The result
    ''' will therefore have an undetermined value, but the data type
    ''' will be correct.</param>
    ''' <returns>The result of the operation</returns>
    ''' <exception cref="InvalidOperationException">If any errors occur while
    ''' attempting to perform the operation.</exception>
    Public Shared Function DoOperation(ByVal op As String, _
     ByVal val1 As clsProcessValue, ByVal val2 As clsProcessValue, _
     ByVal justValidate As Boolean) As clsProcessValue
        Dim sErr As String = Nothing
        Dim resp As clsProcessValue = Nothing
        If Not DoOperation(op, val1, val2, resp, justValidate, sErr) Then _
         Throw New InvalidOperationException(sErr)
        Return resp
    End Function

    ''' <summary>
    ''' Performs the specified operation.
    ''' </summary>
    ''' <param name="op">The operator to use</param>
    ''' <param name="val1">The value to the left of the operator.
    ''' This can be Nothing to specify a unary operation with a
    ''' value on the right-hand side only.</param>
    ''' <param name="val2">The value to the right of the operator</param>
    ''' <param name="resp">On success, the result of the operation
    ''' is stored here</param>
    ''' <param name="justValidate">If True, the expression is validated
    ''' only, no calculations or data lookups will be done. The result
    ''' will therefore have an undetermined value, but the data type
    ''' will be correct.</param>
    ''' <param name="sErr">If an error occurs, this holds an error
    ''' message.</param>
    ''' <returns>True if successful, False in case of error.</returns>
    Public Shared Function DoOperation(ByVal op As String, _
     ByVal val1 As clsProcessValue, ByVal val2 As clsProcessValue, _
     ByRef resp As clsProcessValue, ByVal justValidate As Boolean, _
     ByRef sErr As String) As Boolean
        Try

            'Determine if this is a unary or binary operation...
            If val1 Is Nothing Then

                'None of the unary operations can be performed on a null value...
                If Not justValidate AndAlso val2.IsNull Then
                    sErr = String.Format(My.Resources.Resources.clsProcessOperators_CannotPerform0OperationOnAnEmptyValue, op)
                    Return False
                End If

                'Unary operation...
                Select Case op
                    Case "+"
                        'Rather a pointless operation, and shouldn't
                        'be technically allowed on some data types,
                        'but we will let it pass...
                        If justValidate _
                         Then resp = New clsProcessValue(val2.DataType) _
                         Else resp = val2.Clone()

                    Case "-"
                        Select Case val2.DataType
                            Case DataType.number
                                If justValidate _
                                 Then resp = New clsProcessValue(DataType.number) _
                                 Else resp = -CDec(val2)

                            Case DataType.timespan
                                If justValidate _
                                 Then resp = New clsProcessValue(DataType.timespan) _
                                 Else resp = CType(val2, TimeSpan).Negate()

                            Case Else
                                sErr = My.Resources.Resources.clsProcessOperators_InvalidDataTypeForUnaryMinus
                                Return False
                        End Select
                    Case Else
                        sErr = String.Format(My.Resources.Resources.clsProcessOperators_The0OperationMustHaveTwoOperands, op)
                        Return False
                End Select
            Else

                ' None of the binary operations can be performed if either of values
                ' is null unless they are binary values because of reasons
                If Not justValidate AndAlso val1.DataType <> DataType.binary Then
                    If val1.IsNull Then
                        sErr = String.Format(My.Resources.Resources.clsProcessOperators_CannotPerform0OperationWhenTheLeftHandValueIsEmpty, op)
                        Return False
                    End If
                    If val2.IsNull Then
                        sErr = String.Format(My.Resources.Resources.clsProcessOperators_CannotPerform0OperationWhenTheRightHandValueIsEmpty, op)
                        Return False
                    End If
                End If

                'Binary operation....
                Select Case op
                    Case "&"
                        Return DoOp_Concatenation(val1, val2, resp, justValidate, sErr)

                    Case "+"
                        Return DoOp_Addition(val1, val2, resp, justValidate, sErr)

                    Case "-"
                        Return DoOp_Subtraction(val1, val2, resp, justValidate, sErr)

                    Case "*"
                        Return DoOp_Multiplication(val1, val2, resp, justValidate, sErr)

                    Case "/"
                        Return DoOp_Division(val1, val2, resp, justValidate, sErr)

                    Case "^"
                        Return DoOp_Exponentiation(val1, val2, resp, justValidate, sErr)

                    Case "="
                        Return DoOp_Equality(val1, val2, resp, justValidate, sErr)

                    Case "<>"
                        Return DoOp_NonEquality(val1, val2, resp, justValidate, sErr)

                    Case ">"
                        Return DoOp_Greater(val1, val2, resp, justValidate, sErr)

                    Case ">="
                        Return DoOp_GreaterEq(val1, val2, resp, justValidate, sErr)

                    Case "<"
                        Return DoOp_Less(val1, val2, resp, justValidate, sErr)

                    Case "<="
                        Return DoOp_LessEq(val1, val2, resp, justValidate, sErr)

                    Case "and", "AND"
                        If val1.DataType <> DataType.flag OrElse val2.DataType <> DataType.flag Then
                            sErr = My.Resources.Resources.clsProcessOperators_BothOperandsForANDMustBeFlags
                            Return False
                        End If
                        resp = CBool(val1) AndAlso CBool(val2)

                    Case "or", "OR"
                        If val1.DataType <> DataType.flag OrElse val2.DataType <> DataType.flag Then
                            sErr = My.Resources.Resources.clsProcessOperators_BothOperandsForORMustBeFlag
                            Return False
                        End If
                        resp = CBool(val1) OrElse CBool(val2)

                    Case Else
                        sErr = String.Format(My.Resources.Resources.clsProcessOperators_UnknownOperator0, op)
                        Return False
                End Select
            End If
            Return True

        Catch e As Exception
            sErr = String.Format(My.Resources.Resources.clsProcessOperators_ExceptionOccurred0, e.ToString())
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Performs the addition operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Addition(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType
            Case DataType.number
                ' If we're actually doing the operation, or the rhs operand datatype
                ' is known at this point (it may not be while validating), ensure
                ' that it is a timespan value
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyAddANumberToANumber
                    Return False
                End If

                If justValidate Then
                    result = New clsProcessValue(DataType.number)
                Else
                    result = CDec(lhs) + CDec(rhs)
                End If

            Case DataType.timespan
                ' If we're actually doing the operation, or the rhs operand datatype
                ' is known at this point (it may not be while validating), ensure
                ' that it is a timespan value
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.timespan Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyAddATimespanToATimespan
                    Return False
                End If

                If justValidate Then
                    result = New clsProcessValue(DataType.timespan)
                Else
                    result = CType(lhs, TimeSpan) + CType(rhs, TimeSpan)
                End If

            Case DataType.date, DataType.datetime, DataType.time
                ' If we're actually doing the operation, or the rhs operand datatype
                ' is known at this point (it may not be while validating), ensure
                ' that it is a timespan value
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.timespan Then
                    sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanOnlyAddATimespanToA0, GetFriendlyName(lhs.DataType))
                    Return False
                End If
                If justValidate Then
                    result = New clsProcessValue(lhs.DataType)
                Else
                    result = New clsProcessValue(lhs.DataType, CDate(lhs) + CType(rhs, TimeSpan))
                End If

                ' Addition for a binary is concatenation of two binary values.
            Case DataType.binary
                ' If we're actually doing the operation, or the rhs operand datatype
                ' is known at this point (it may not be while validating), ensure
                ' that it is a binary value
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.binary Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyAddABinaryToABinary
                    Return False
                End If
                If justValidate Then
                    result = New clsProcessValue(DataType.binary)

                Else
                    Dim b As Byte() = CType(lhs, Byte())
                    If b Is Nothing Then b = New Byte() {}
                    Dim b2 As Byte() = CType(rhs, Byte())
                    If b2 Is Nothing Then b2 = New Byte() {}
                    Dim len As Integer = b.Length
                    Array.Resize(b, len + b2.Length)
                    b2.CopyTo(b, len)
                    result = b

                End If

            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTAddTo0, GetFriendlyName(lhs.DataType))
                Return False
        End Select
        Return True

    End Function

    ''' <summary>
    ''' Performs the subtraction operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Subtraction(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType
            Case DataType.number
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlySubtractANumberFromANumber
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.number) _
                 Else result = CDec(lhs) - CDec(rhs)

            Case DataType.date, DataType.datetime, DataType.time
                If rhs.DataType = DataType.timespan Then
                    If justValidate _
                     Then result = New clsProcessValue(lhs.DataType) _
                     Else result = New clsProcessValue(
                      lhs.DataType, CDate(lhs) - CType(rhs, TimeSpan))

                ElseIf rhs.DataType = lhs.DataType Then
                    If justValidate _
                     Then result = New clsProcessValue(DataType.timespan) _
                     Else result = CDate(lhs) - CDate(rhs)

                Else
                    If Not justValidate OrElse rhs.DataType <> DataType.unknown Then
                        sErr = String.Format(
                         My.Resources.Resources.clsProcessOperators_CanOnlySubtractATimespanOrA0FromA0,
                         GetFriendlyName(lhs.DataType))
                        Return False
                    End If
                    result = New clsProcessValue(DataType.unknown)
                End If

            Case DataType.timespan
                If (justValidate = False OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.timespan Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlySubtractATimespanFromATimespan
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.timespan) _
                 Else result = CType(lhs, TimeSpan) - CType(rhs, TimeSpan)

            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTSubtractFrom0, GetFriendlyName(lhs.DataType))
                Return False

        End Select
        Return True

    End Function

    ''' <summary>
    ''' Performs the concatenation operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Concatenation(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        ' Can get unknown values during validation, from undefined collection fields
        Static validTypesWhenValidating As _
         New clsSet(Of DataType)(DataType.text, DataType.unknown)

        If justValidate _
         AndAlso validTypesWhenValidating.Contains(lhs.DataType) _
         AndAlso validTypesWhenValidating.Contains(rhs.DataType) _
         Then result = "" : Return True

        Try
            result =
             CStr(lhs.CastInto(DataType.text)) & CStr(rhs.CastInto(DataType.text))

            Return True

        Catch ex As Exception
            sErr = ex.Message
            Return False

        End Try
    End Function

    ''' <summary>
    ''' Performs the multiplication operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Multiplication(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType()
            Case DataType.number
                Select Case rhs.DataType
                    Case DataType.number
                        If justValidate _
                         Then result = New clsProcessValue(DataType.number) _
                         Else result = CDec(lhs) * CDec(rhs)

                    Case DataType.timespan
                        If justValidate _
                         Then result = New clsProcessValue(DataType.timespan) _
                         Else result = New TimeSpan(CLng(CDec(lhs) * CType(rhs, TimeSpan).Ticks))

                    Case Else
                        sErr = My.Resources.Resources.clsProcessOperators_YouCanOnlyMultiplyANumberByAnotherNumberOrATimespan
                        Return False

                End Select
                Return True

            Case DataType.timespan
                Select Case rhs.DataType
                    Case DataType.number
                        If justValidate _
                         Then result = New clsProcessValue(DataType.timespan) _
                         Else result = New TimeSpan(CLng(CDec(rhs) * CType(lhs, TimeSpan).Ticks))

                        Return True
                    Case Else
                        sErr = My.Resources.Resources.clsProcessOperators_CanOnlyMultiplyTimespanByANumber
                        Return False
                End Select
            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTMultiplyA0, GetFriendlyName(lhs.DataType))
                Return False
        End Select

    End Function

    ''' <summary>
    ''' Performs the division operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Division(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType
            Case DataType.number
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyDivideANumberByANumber
                    Return False
                End If
                If justValidate Then
                    result = New clsProcessValue(DataType.number)
                Else
                    Dim divisor As Decimal = CDec(rhs)
                    If divisor = 0 Then sErr = My.Resources.Resources.clsProcessOperators_DivisionByZero : Return False
                    result = (CDec(lhs) / divisor)
                End If
                Return True
            Case DataType.timespan
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyDivideATimespanByANumber
                    Return False
                End If
                If justValidate Then
                    result = New clsProcessValue(DataType.timespan)
                Else
                    Dim divisor As Decimal = CDec(rhs)
                    If divisor = 0 Then sErr = My.Resources.Resources.clsProcessOperators_DivisionByZero : Return False
                    result = New TimeSpan(CLng(CType(lhs, TimeSpan).Ticks / divisor))
                End If
                Return True

            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTDivideA0, GetFriendlyName(lhs.DataType))
                Return False

        End Select

    End Function

    ''' <summary>
    ''' Performs the exponent operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Exponentiation(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        If (Not justValidate OrElse rhs.DataType <> DataType.unknown) AndAlso
         (lhs.DataType <> DataType.number OrElse rhs.DataType <> DataType.number) Then
            sErr = My.Resources.Resources.clsProcessOperators_CanOnlyPerformExponentiationUsingTwoNumbers
            Return False
        End If

        If justValidate Then
            result = New clsProcessValue(DataType.number)
        Else
            Dim dec1 As Decimal = CDec(lhs)
            Dim dec2 As Decimal = CDec(rhs)

            If dec1 = 0 AndAlso dec2 < 0 Then
                sErr = My.Resources.Resources.clsProcessOperators_DivisionByZeroCannotEvaluate0ToThePowerOfANegativeValue
                Return False
            End If

            result = CDec(Math.Pow(dec1, dec2))
        End If

        Return True
    End Function

    ''' <summary>
    ''' Performs the equality operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Equality(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean
        result = (justValidate OrElse lhs.Equals(rhs))
        Return True
    End Function

    ''' <summary>
    ''' Performs the non-equality operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_NonEquality(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean
        result = (justValidate OrElse Not lhs.Equals(rhs))
        Return True
    End Function

    ''' <summary>
    ''' Performs the greater than operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Greater(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType
            Case DataType.number
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyCompareANumberToANumber
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDec(lhs) > CDec(rhs))

            Case DataType.date, DataType.datetime, DataType.time
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso lhs.DataType <> rhs.DataType Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanTCompareDifferentDateTypes
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDate(lhs) > CDate(rhs))

            Case DataType.timespan
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.timespan Then
                    sErr = My.Resources.Resources.clsProcessOperators_DoOp_Greater_ATimespanCanOnlyBeComparedToAnotherTimespan
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CType(lhs, TimeSpan) > CType(rhs, TimeSpan))

            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTCompare0, GetFriendlyName(lhs.DataType))
                Return False
        End Select
        Return True

    End Function

    ''' <summary>
    ''' Performs the greater than or equal operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_GreaterEq(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType
            Case DataType.number
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyCompareANumberToANumber
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDec(lhs) >= CDec(rhs))

            Case DataType.date, DataType.datetime, DataType.time
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso lhs.DataType <> rhs.DataType Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanTCompareDifferentDateTypes
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDate(lhs) >= CDate(rhs))

            Case DataType.timespan
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.timespan Then
                    sErr = My.Resources.Resources.clsProcessOperators_DoOp_GreaterEq_ATimespanCanOnlyBeComparedToAnotherTimespan
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CType(lhs, TimeSpan) >= CType(rhs, TimeSpan))

            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTCompare0, GetFriendlyName(lhs.DataType))
                Return False
        End Select
        Return True

    End Function

    ''' <summary>
    ''' Performs the less than operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_Less(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType
            Case DataType.number
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyCompareANumberToANumber
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDec(lhs) < CDec(rhs))

            Case DataType.date, DataType.datetime, DataType.time
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso lhs.DataType <> rhs.DataType Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanTCompareDifferentDateTypes
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDate(lhs) < CDate(rhs))

            Case DataType.timespan
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.timespan Then
                    sErr = My.Resources.Resources.clsProcessOperators_DoOp_Less_ATimespanCanOnlyBeComparedToAnotherTimespan
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CType(lhs, TimeSpan) < CType(rhs, TimeSpan))

            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTCompare0, GetFriendlyName(lhs.DataType))
                Return False
        End Select
        Return True

    End Function

    ''' <summary>
    ''' Performs the less than or equal operator on the given arguments
    ''' </summary>
    ''' <param name="lhs">The left hand side value for the operation</param>
    ''' <param name="rhs">The right hand side value for the operation</param>
    ''' <param name="result">The result of the operation</param>
    ''' <param name="justValidate">True to just validate the arguments to the
    ''' operation. When set to true, the result will typically just be a null value
    ''' of the correct output type since the actual operation is not performed.
    ''' </param>
    ''' <param name="sErr">Any error message which may have occurred</param>
    ''' <returns>True on a successful call of the operation; False if the operation
    ''' failed.</returns>
    Private Shared Function DoOp_LessEq(
     ByVal lhs As clsProcessValue, ByVal rhs As clsProcessValue,
     ByRef result As clsProcessValue, ByVal justValidate As Boolean,
     ByRef sErr As String) As Boolean

        Select Case lhs.DataType
            Case DataType.number
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.number Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanOnlyCompareANumberToANumber
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDec(lhs) <= CDec(rhs))

            Case DataType.date, DataType.datetime, DataType.time
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso lhs.DataType <> rhs.DataType Then
                    sErr = My.Resources.Resources.clsProcessOperators_CanTCompareDifferentDateTypes
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CDate(lhs) <= CDate(rhs))

            Case DataType.timespan
                If (Not justValidate OrElse rhs.DataType <> DataType.unknown) _
                 AndAlso rhs.DataType <> DataType.timespan Then
                    sErr = My.Resources.Resources.clsProcessOperators_ATimeSpanCanOnlyBeComparedToAnotherTimespan
                    Return False
                End If
                If justValidate _
                 Then result = New clsProcessValue(DataType.flag) _
                 Else result = (CType(lhs, TimeSpan) <= CType(rhs, TimeSpan))

            Case Else
                sErr = String.Format(My.Resources.Resources.clsProcessOperators_CanTCompare0, GetFriendlyName(lhs.DataType))
                Return False
        End Select
        Return True

    End Function

    ''' JC Jan 06. Basic operator class definitions brought in from Automate V2. 
    ''' Employed only by calculation and decision properties forms at present but
    ''' potential to amalgamate with shared code above as per clsFunctions. For review.
    Private Enum V2GroupNames
        Text
        Logic
        [Date]
        Number
    End Enum

#Region "clsProcessOperator"

    ''' <summary>
    ''' Base class for defining basic operator details.
    ''' </summary>
    Public MustInherit Class clsProcessOperator

        Public MustOverride ReadOnly Property Symbol() As String

        Public MustOverride ReadOnly Property Name() As String

        Public MustOverride ReadOnly Property GroupName() As String

        Public MustOverride ReadOnly Property HelpText() As String

    End Class
#End Region

#Region "Private operator class definitions"

#Region "clsProcessOperator_And"

    Private Class clsProcessOperator_And
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "AND"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "And"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_And_GetsAFlagSayingWhetherTwoConditionsAreSimultaneouslyTrueEG12AND34ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Or"

    Private Class clsProcessOperator_Or
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "OR"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Or"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Or_GetsAFlagSayingWhetherAtLeastOneOfTwoConditionsIsTrueEG12OR34ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Equal"

    Private Class clsProcessOperator_Equal
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "="
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Equal"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Equal_GetsAFlagSayingWhetherOneValueIsEqualToAnotherEG532ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Inequal"

    Private Class clsProcessOperator_Inequal
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "<>"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Not equal"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Inequal_GetsAFlagSayingWhetherOneValueIsNotEqualToAnotherEG53ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_GreaterThan"

    Private Class clsProcessOperator_GreaterThan
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return ">"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Greater than"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_GreaterThan_GetsAFlagSayingWhetherOneValueIsGreaterThanAnotherEG53ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_GreaterThanOrEqual"

    Private Class clsProcessOperator_GreaterThanOrEqual
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return ">="
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Greater than or equal"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_GreaterThanOrEqual_GetsAFlagSayingWhetherOneValueIsGreaterThanOrEqualToAnotherEG53ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_LessThan"

    Private Class clsProcessOperator_LessThan
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "<"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Less than"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_LessThan_GetsAFlagSayingWhetherOneValueIsLessThanAnotherEG35ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_LessThanOrEqual"

    Private Class clsProcessOperator_LessThanOrEqual
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "<="
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Less than or equal"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Logic.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_LessThanOrEqual_GetsAFlagSayingWhetherOneValueIsLessThanOrEqualToAnotherEG35ResultsInTrue
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Concatenate"

    Private Class clsProcessOperator_Concatenate
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "&"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Concatenate"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Text.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Concatenate_GetsATextExpressionByConcatenatingTwoOthersEGABResultsInAB
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Add"

    Private Class clsProcessOperator_Add
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "+"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Add"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Number.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Add_GetsANumberByAddingTwoNumbersTogetherEG12ResultsIn3
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Subtract"

    Private Class clsProcessOperator_Subtract
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "-"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Subtract"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Number.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Subtract_GetsANumberBySubtractingOneNumberFromAnotherEG21ResultsIn1
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Multiply"

    Private Class clsProcessOperator_Multiply
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "*"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Multiply"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Number.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Multiply_GetsANumberByMultiplyingTwoNumbersTogetherEG12ResultsIn2
            End Get
        End Property

    End Class

#End Region

#Region "clsProcessOperator_Divide"

    Private Class clsProcessOperator_Divide
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "/"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Divide"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Number.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Divide_GetsANumberByDividingOneNumberByAnotherEG12ResultsIn05
            End Get
        End Property

    End Class

#End Region


#Region "clsProcessOperator_Power"

    Private Class clsProcessOperator_Power
        Inherits clsProcessOperator

        Public Overrides ReadOnly Property Symbol() As String
            Get
                Return "^"
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Power"
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return V2GroupNames.Number.ToString()
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsProcessOperator_Power_RaisesANumberToThePowerOfAnotherEg34333381
            End Get
        End Property

    End Class

#End Region

#End Region

#Region "GetOperators"

    Public Shared Function GetOperators() As clsProcessOperator()
        Return New clsProcessOperator() { _
         New clsProcessOperator_And, _
         New clsProcessOperator_Or, _
         New clsProcessOperator_Equal, _
         New clsProcessOperator_Inequal, _
         New clsProcessOperator_GreaterThan, _
         New clsProcessOperator_GreaterThanOrEqual, _
         New clsProcessOperator_LessThan, _
         New clsProcessOperator_LessThanOrEqual, _
         New clsProcessOperator_Concatenate, _
         New clsProcessOperator_Add, _
         New clsProcessOperator_Subtract, _
         New clsProcessOperator_Multiply, _
         New clsProcessOperator_Power, _
         New clsProcessOperator_Divide}
    End Function

#End Region


End Class
