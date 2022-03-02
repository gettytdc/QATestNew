Imports System.Math
Imports System.IO
Imports System.Globalization
Imports Microsoft.Win32
Imports BluePrism.AutomateProcessCore.clsProcessDataTypes
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Extensions

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsFunctions
''' 
''' <summary>
''' This class provides the main interface to Functions, as used in Expressions. An
''' instance of this class represents a set of available functions. A clsProcess
''' object maintains a persistent instance of a clsFunctions throughout its lifetime.
''' This allows the set of available functions to be supplemented from 'live' sources
''' such as Business Objects and Web Services.
''' </summary>
''' <remarks>
''' <para>
''' At present, clsProcess is only class that creates an instance of clsFunctions.
''' Other code, especially UI code, should obtain a reference to the clsFunctions
''' instance from the clsProcess they are working with.
''' </para>
''' <para>
''' Note that this is a partial class, currently only extended by the unit tests in
''' the UnitTests subfolder of AutomateProcessCore.
''' </para>
''' </remarks>
Partial Public Class clsFunctions

    ' All the following handle individual internal functions. See the base class for
    ' documentation!

    Private Class clsFunction_Round
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Round"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Round_Round
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Number"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms,
             New Signature(DataType.number),
             New Signature(DataType.number, DataType.number))

            Dim dec As Decimal, places As Integer
            Try
                dec = CDec(parms(0))
            Catch
                Throw New FunctionArgException(1, Name, "a valid number")
            End Try
            Try
                If parms.Count < 2 Then places = 0 Else places = CInt(parms(1))
            Catch ex As Exception
                Throw New FunctionArgException(2, Name, "a valid number")
            End Try
            Return Decimal.Round(dec, places)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Public Overrides ReadOnly Property Signatures() As List(Of clsFunctionParm())
            Get
                Dim BaseSigs As List(Of clsFunctionParm()) = MyBase.Signatures
                BaseSigs.Add(New clsFunctionParm() {New clsFunctionParm("Number", My.Resources.Resources.clsFunction_Round_TheNumberToBeRounded, DataType.number)})
                Return BaseSigs
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
          New clsFunctionParm("Number", My.Resources.Resources.clsFunction_Round_TheNumberToBeRounded, DataType.number),
          New clsFunctionParm("Places", My.Resources.Resources.clsFunction_Round_TheNumberOfDecimalPlacesToRoundTo, DataType.number)
         }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Round_GetsANumberRoundOffToANumberOfDecimalPlacesEGRound1471ResultsIn15
            End Get
        End Property
    End Class

    Private Class clsFunction_RndUp
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "RndUp"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_RndUp_RoundUp
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Number"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms,
             New Signature(DataType.number),
             New Signature(DataType.number, DataType.number))

            Dim dec As Decimal, places As Integer
            Try
                dec = CDec(parms(0))
            Catch e As Exception
                Throw New FunctionArgException(1, Name, "a valid number")
            End Try
            Try
                If parms.Count < 2 Then places = 0 Else places = CInt(parms(1))
            Catch ex As Exception
                Throw New FunctionArgException(2, Name, "a valid number")
            End Try
            Dim factor As Integer = CInt(10 ^ places)
            Dim factoredNum As Decimal = Fix(dec * factor)
            dec = (factoredNum +
             CDec(IIf((dec * factor) = factoredNum, 0, Sign(dec)))) / factor
            Return dec
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Number", My.Resources.Resources.clsFunction_RndUp_TheNumberToBeRounded, DataType.number),
         New clsFunctionParm("Places", My.Resources.Resources.clsFunction_RndUp_TheNumberOfDecimalPlacesToRoundTo, DataType.number)
        }
        Public Overrides ReadOnly Property Signatures() As List(Of clsFunctionParm())
            Get
                Dim BaseSigs As List(Of clsFunctionParm()) = MyBase.Signatures
                BaseSigs.Add(New clsFunctionParm() {New clsFunctionParm("Number", My.Resources.Resources.clsFunction_RndUp_TheNumberToBeRounded, DataType.number)})
                Return BaseSigs
            End Get
        End Property
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_RndUp_GetsANumberRoundedUpToANumberOfDecimalPlacesEGRndUp1471ResultsIn15
            End Get
        End Property
    End Class

    Private Class clsFunction_RndDn
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "RndDn"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_RndDn_RoundDown
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Number"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue

            EnsureParams(parms,
             New Signature(DataType.number),
             New Signature(DataType.number, DataType.number))

            Dim num As Decimal, places As Integer
            Try
                num = CDec(parms(0))
            Catch e As Exception
                Throw New FunctionArgException(1, Name, "a valid number")
            End Try
            Try
                If parms.Count < 2 Then places = 0 Else places = CInt(parms(1))
            Catch ex As Exception
                Throw New FunctionArgException(2, Name, "a valid number")
            End Try
            Dim factor As Integer = CInt(10 ^ places)
            num = Fix(num * factor) / factor
            Return num
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Number", My.Resources.Resources.clsFunction_RndDn_TheNumberToBeRounded, DataType.number),
         New clsFunctionParm("Places", My.Resources.Resources.clsFunction_RndDn_TheNumberOfDecimalPlacesToRoundToOptional, DataType.number)
        }
        Public Overrides ReadOnly Property Signatures() As List(Of clsFunctionParm())
            Get
                Dim BaseSigs As List(Of clsFunctionParm()) = MyBase.Signatures
                BaseSigs.Add(New clsFunctionParm() {New clsFunctionParm("Number", My.Resources.Resources.clsFunction_RndDn_Signatures_TheNumberToBeRounded, DataType.number)})
                Return BaseSigs
            End Get
        End Property
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_RndDn_GetsANumberRoundedDownToANumberOfDecimalPlacesEGRndDn1471ResultsIn14RndDnIsSymm
            End Get
        End Property
    End Class

    Private Class clsFunction_DecPad
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "DecPad"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_DecPad_PadDecimals
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Number"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms,
             New Signature(DataType.number, DataType.number))

            ' Negative breaks string constructor's heart.
            Dim decPlaces As Integer = CInt(parms(1))
            If decPlaces < 0 Then decPlaces = 0

            ' Round it first, using the same rounding mechanism as Round().
            Dim num As Decimal = CDec(parms(0))
            ' Decimal.Round() can only round to between 0 and 28 digits of precision
            If decPlaces < 28 Then num = Decimal.Round(num, decPlaces)

            ' Note that this '.' character is treated as "current locale's decimal
            ' point char", not necessarily a dot
            Return num.ToString("0." & New String("0"c, decPlaces))

        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Number", My.Resources.Resources.clsFunction_DecPad_TheNumberToBePadded, DataType.number),
         New clsFunctionParm("Places", My.Resources.Resources.clsFunction_DecPad_TheNumberOfDecimalPlacesToPadOutTo, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_DecPad_GetsTextRepresentingANumberWithDecimalPlacesPaddedOutWithZerosEGDecPad12Results
            End Get
        End Property
    End Class

    Private Class clsFunction_Sqrt : Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Sqrt"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Sqrt_SquareRoot
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Number"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.number))
            Dim num As Double = CDbl(parms(0))
            If num < 0D Then Throw New FunctionArgException(1, Name,
             "a number greater than or equal to zero")
            Return Math.Sqrt(num)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Number",
          My.Resources.Resources.clsFunction_Sqrt_TheSquareRootToBeCalculated, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Sqrt_GetsTheSquareRootOfTheNumberEGSqrt9ResultsIn3
            End Get
        End Property
    End Class


    Private Class clsFunction_Log
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Log"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Log_LogarithmToAnyBase
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Number"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue),
         ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.number, DataType.number))

            Dim num As Decimal = CDec(parms(0))
            Dim base As Decimal = CDec(parms(1))
            If num <= 0 OrElse base <= 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_Log_TheLogFunctionOnlySupportsPositiveNumericArguments)

            Return Math.Log(num, base)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Number",
          My.Resources.Resources.clsFunction_Log_TheInputValueToTheLogarithmFunction, DataType.number),
         New clsFunctionParm("Base",
          My.Resources.Resources.clsFunction_Log_TheBaseOfTheLogarithmFunction, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return _
                 My.Resources.Resources.clsFunction_Log_DeterminesThePowerToWhichTheLogarithmSBaseMustBeRaisedToAchieveTheInputValueEGL
            End Get
        End Property
    End Class

    Private Class clsFunction_Left
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Left"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Left_CharactersFromLeft
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text, DataType.number))
            Return CStr(parms(0)).Left(CInt(parms(1)))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Left_TheTextFromWhichCharactersAreReturned, DataType.text),
         New clsFunctionParm("Length", My.Resources.Resources.clsFunction_Left_TheNumberOfCharactersToReturn, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Left_GetsTextTakenFromTheLeftOfATextExpressionEGLeftExample3WillResultInExa
            End Get
        End Property
    End Class

    Private Class clsFunction_Right
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Right"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Right_CharactersFromRight
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text, DataType.number))
            Return CStr(parms(0)).Right(CInt(parms(1)))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Right_TheTextFromWhichCharactersAreReturned, DataType.text),
         New clsFunctionParm("Length", My.Resources.Resources.clsFunction_Right_TheNumberOfCharactersToReturn, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Right_GetsTextTakenFromTheRightOfTextExpressionEGRightExample3WillResultInPle
            End Get
        End Property
    End Class

    Private Class clsFunction_Mid
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Mid"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Mid_CharactersFromMiddle
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms,
             New Signature(DataType.text, DataType.number, DataType.number))
            Return CStr(parms(0)).Mid(CInt(parms(1)), CInt(parms(2)))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Mid_TheTextFromWhichCharactersAreReturned, DataType.text),
         New clsFunctionParm("Start point", My.Resources.Resources.clsFunction_Mid_ThePositionOfTheFirstCharacterToBeReturned, DataType.number),
         New clsFunctionParm("Length", My.Resources.Resources.clsFunction_Mid_TheNumberOfCharactersToReturn, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Mid_GetsTextTakenFromTheMiddleOfTextExpressionEGMidExample33WillResultInAmp
            End Get
        End Property
    End Class

    Private Class clsFunction_Chr
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Chr"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Chr_GetCharacter
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.number))
            Return New clsProcessValue(DataType.text, Chr(CInt(parms(0))))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Code", My.Resources.Resources.clsFunction_Chr_ANumericCharacterCode, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Chr_GetTheCharacterRepresentedByTheGivenNumericCodeEG65IsA66IsB
            End Get
        End Property
    End Class

    Private Class clsFunction_NewLine
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "NewLine"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_NewLine_GetNewLineCharacters
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_NewLine_NewLineFunctionShouldNotHaveAnyParameters)
            Return vbCrLf
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_NewLine_GetTheNewLineCharactersCarriageReturnFollowedByLineFeed
            End Get
        End Property
        Public Overrides ReadOnly Property TextAppendFunction() As Boolean
            Get
                Return True
            End Get
        End Property
    End Class

    Private Class clsFunction_InStr
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "InStr"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_InStr_FindInString
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text, DataType.text))
            Return InStr(CStr(parms(0)), CStr(parms(1)))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_InStr_TheTextExpressionToBeSearched, DataType.text),
         New clsFunctionParm("Search", My.Resources.Resources.clsFunction_InStr_TheTextExpressionToBeSearchedFor, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_InStr_GetsTheStartingPointOfASubSectionOfTextEGInStrExampleMpResultsIn4TheResultIsAlw
            End Get
        End Property
    End Class

    Private Class clsFunction_Len
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Len"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Len_Length
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms,
             New Signature(DataType.text), New Signature(DataType.binary))

            If parms(0).DataType = DataType.binary Then
                If parms(0).IsNull Then Return 0
                Return CType(parms(0), Byte()).Length
            Else
                Return Len(CStr(parms(0)))
            End If
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Len_TheTextToBeMeasured, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Len_GetsTheNumberOfCharactersInSomeTextEGLenExampleWillResultIn7
            End Get
        End Property
    End Class

    Private Class clsFunction_Bytes
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Bytes"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Bytes_ByteCount
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Data"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 1 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_Bytes_BytesFunctionRequires1Parameter)

            If parms(0).DataType <> DataType.binary Then _
             Throw New FunctionArgException(1, Name, DataType.binary)

            If parms(0).IsNull Then Return 0
            Return CType(parms(0), Byte()).Length

        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Data", My.Resources.Resources.clsFunction_Bytes_TheBinaryDataToBeMeasured, DataType.binary)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Bytes_GetsTheNumberOfBytesInSomeBinaryData
            End Get
        End Property
    End Class


    Private Class clsFunction_TrimStart
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "TrimStart"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_TrimStart_ShortDesc_TrimStart
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text))
            Return CStr(parms(0)).TrimStart()
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_TrimStart_TheTextExpressionToBeConverted, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_TrimStart_GetsTextThatIsTheInputWithAnyWhitespaceTrimmedFromTheStartEGTrimStartExampleRes
            End Get
        End Property
    End Class

    Private Class clsFunction_TrimEnd
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "TrimEnd"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_TrimEnd_ShortDesc_TrimEnd
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text))
            Return CStr(parms(0)).TrimEnd()
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_TrimEnd_TheTextExpressionToBeConverted, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_TrimEnd_GetsTextThatIsTheInputWithAnyWhitespaceTrimmedFromTheEndEGTrimEndExampleResults
            End Get
        End Property
    End Class

    Private Class clsFunction_Trim
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Trim"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Trim_Trim
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text))
            Return CStr(parms(0)).Trim()
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Trim_TheTextExpressionToBeConverted, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Trim_GetsTextThatIsTheInputWithAnyWhitespaceTrimmedFromTheStartAndEndEGTrimExampleRe
            End Get
        End Property
    End Class

    Private Class clsFunction_StartsWith
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "StartsWith"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_StartsWith_ShortDesc_StartsWith
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.flag
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text, DataType.text))
            Return CStr(parms(0)).StartsWith(CStr(parms(1)))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_StartsWith_TheTextExpressionToBeChecked, DataType.text),
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_StartsWith_TheTextToCheckAgainst, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_StartsWith_ChecksIfOnePieceOfTextStartsWithAnotherEGStartsWithExampleExamResultsInTrue
            End Get
        End Property
    End Class

    Private Class clsFunction_EndsWith
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "EndsWith"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_EndsWith_ShortDesc_EndsWith
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.flag
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text, DataType.text))
            Return CStr(parms(0)).EndsWith(CStr(parms(1)))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_EndsWith_TheTextExpressionToBeChecked, DataType.text),
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_EndsWith_TheTextToCheckAgainst, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_EndsWith_ChecksIfOnePieceOfTextEndsWithAnotherEGEndsWithExampleAmpleResultsInTrue
            End Get
        End Property
    End Class

    Private Class clsFunction_Upper
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Upper"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Upper_MakeUpperCase
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text))
            Return CStr(parms(0)).ToUpper()
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Upper_TheTextExpressionToBeConverted, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Upper_GetsTextRepresentingATextExpressionInUpperCaseEGUpperEXAMpleResultsInEXAMPLE
            End Get
        End Property
    End Class

    Private Class clsFunction_Replace
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Replace"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Replace_ReplaceText
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text, DataType.text, DataType.text))
            Return CStr(parms(0)).Replace(CStr(parms(1)), CStr(parms(2)))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Replace_TheTextExpressionToBeConverted, DataType.text),
         New clsFunctionParm("Pattern", My.Resources.Resources.clsFunction_Replace_TheStringPatternToBeReplaced, DataType.text),
         New clsFunctionParm("NewText", My.Resources.Resources.clsFunction_Replace_TheStringToBeWrittenOverTheOldPattern, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Replace_GetsTextWithOnePieceOfTextReplacedWithAnotherEGReplaceExampleAmToResultsInExtop
            End Get
        End Property
    End Class

    Private Class clsFunction_Lower
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Lower"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Lower_MakeLowerCase
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Text"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text))
            Return CStr(parms(0)).ToLower()
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_Lower_TheTextExpressionToBeConverted, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Lower_GetsTextRepresentingATextExpressionInLowerCaseEGLowerEXAMpleResultsInExample
            End Get
        End Property
    End Class

    Private Class clsFunction_Today
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Today"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Today_CurrentDate
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.date
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_Today_TodayFunctionShouldNotHaveAnyParameters)

            Return New clsProcessValue(DataType.date, Date.Today)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Today_GetsTheCurrentDateAsADateValue
            End Get
        End Property
    End Class

    Private Class clsFunction_LocalTime
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "LocalTime"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_LocalTime_CurrentLocalTime
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.time
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_LocalTime_LocalTimeFunctionShouldNotHaveAnyParameters)

            Return New clsProcessValue(DataType.time, DateTime.Now)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_LocalTime_GetsTheCurrentLocalTimeAsATimeValue
            End Get
        End Property
    End Class

    Private Class clsFunction_UTCTime
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "UTCTime"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_UTCTime_CurrentUTCTime
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.time
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_UTCTime_UTCTimeFunctionShouldNotHaveAnyParameters)
            Return New clsProcessValue(DataType.time, DateTime.UtcNow)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_UTCTime_GetsTheCurrentUTCTimeAsATimeValue
            End Get
        End Property
    End Class


    Private Class clsFunction_Now
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Now"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_Now_CurrentDateAndTime
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.datetime
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_Now_NowFunctionShouldNotHaveAnyParameters)
            Return Date.UtcNow
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_Now_GetsTheCurrentDateAndTimeAsADatetimeValue
            End Get
        End Property
    End Class

    Private Class clsFunction_DateAdd
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "DateAdd"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_DateAdd_AddToDate
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.date
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.number, DataType.number, DataType.date))

            Dim interval As DateInterval
            Dim num As Double
            Dim dt As Date

            Try
                Select Case CInt(parms(0))
                    Case 0 : interval = DateInterval.Year
                    Case 1 : interval = DateInterval.WeekOfYear
                    Case 2 : interval = DateInterval.Weekday
                    Case 3 : interval = DateInterval.Second
                    Case 4 : interval = DateInterval.Quarter
                    Case 5 : interval = DateInterval.Month
                    Case 6 : interval = DateInterval.Minute
                    Case 7 : interval = DateInterval.Hour
                    Case 8 : interval = DateInterval.DayOfYear
                    Case 9 : interval = DateInterval.Day
                End Select
            Catch ex As Exception
                Throw New clsFunctionException(My.Resources.Resources.clsFunction_DateAdd_FirstParameterForDateAddMustBeANumberThatRefersToAValidDateInterval)
            End Try

            Try
                num = CInt(parms(1))
            Catch
                Throw New clsFunctionException(My.Resources.Resources.clsFunction_DateAdd_SecondParameterForDateAddMustBeTheNumberToAddOrSubtract)
            End Try

            Try
                dt = CDate(parms(2))
            Catch
                Throw New clsFunctionException(My.Resources.Resources.clsFunction_DateAdd_ThirdParameterForDateAddMustBeTheDate)
            End Try

            ' If there's no time element to the date, assume that it's a date,
            ' otherwise assume that it's a datetime
            If dt.TimeOfDay = TimeSpan.Zero Then
                Return New clsProcessValue(DataType.date, DateAdd(interval, num, dt))
            Else
                Return DateAdd(interval, num, dt)
            End If

        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Interval", My.Resources.Resources.clsFunction_DateAdd_TheTypeOfIntervalRepresentedByANumber, DataType.number),
         New clsFunctionParm("Number", My.Resources.Resources.clsFunction_DateAdd_TheNumberOfIntervalsToBeAdded, DataType.number),
         New clsFunctionParm("Date", My.Resources.Resources.clsFunction_DateAdd_TheDateDatetimeToBeAddedTo, DataType.date)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_DateAdd_GetsADateWithANumberOfIntervalsEGDaysMonthsEtcAddedToItSeeTheHelpDocumentationF
            End Get
        End Property
    End Class

    <Serializable>
    Private Class FunctionArgException : Inherits clsFunctionException
        Public Sub New(
         ByVal argNum As Integer,
         ByVal functionName As String,
         ByVal expectedType As DataType)
            MyBase.New("The {0}{1} parameter for {2} must be of type: {3}",
             argNum, GetOrdinal(argNum), functionName, GetFriendlyName(expectedType))
        End Sub
        Public Sub New(
         ByVal argNum As Integer,
         ByVal functionName As String,
         ByVal expectedString As String)
            MyBase.New("The {0}{1} parameter for {2} must be {3}",
             argNum, GetOrdinal(argNum), functionName, expectedString)
        End Sub
        Private Shared Function GetOrdinal(ByVal num As Integer) As String
            ' If it's between 10-20, it's always "th"
            If ((num Mod 100) \ 10) = 1 Then Return "th"

            ' Otherwise, it depends on the last digit
            Select Case (num Mod 10)
                Case 1 : Return "st"
                Case 2 : Return "nd"
                Case 3 : Return "rd"
                Case Else : Return "th"
            End Select
        End Function
    End Class

    Private Class clsFunction_DateDiff
        Inherits clsFunction

        'DATEDIFF IS DEPRECATED - See bug 3478.
        Public Overrides ReadOnly Property Deprecated() As Boolean
            Get
                Return True
            End Get
        End Property
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "DateDiff"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_DateDiff_DateDifference
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.number, DataType.datetime, DataType.datetime))

            Dim interval As DateInterval
            Dim fromDate As Date
            Dim toDate As Date

            Try
                Select Case CInt(parms(0))
                    Case 0 : interval = DateInterval.Year
                    Case 1 : interval = DateInterval.WeekOfYear
                    Case 2 : interval = DateInterval.Weekday
                    Case 3 : interval = DateInterval.Second
                    Case 4 : interval = DateInterval.Quarter
                    Case 5 : interval = DateInterval.Month
                    Case 6 : interval = DateInterval.Minute
                    Case 7 : interval = DateInterval.Hour
                    Case 8 : interval = DateInterval.DayOfYear
                    Case 9 : interval = DateInterval.Day
                End Select
            Catch
                Throw New FunctionArgException(1, Name,
                 "a number that refers to a valid date interval")
            End Try

            Try
                fromDate = CDate(parms(1))
            Catch
                Throw New FunctionArgException(2, Name,
                 "the date to be subtracted")
            End Try

            Try
                toDate = CDate(parms(2))
            Catch
                Throw New FunctionArgException(3, Name,
                 "the date from which you subtract the other date")
            End Try

            Return DateDiff(interval, fromDate, toDate)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Interval", My.Resources.Resources.clsFunction_DateDiff_TheTypeOfIntervalRepresentedByANumber, DataType.number),
         New clsFunctionParm("Start Date", My.Resources.Resources.clsFunction_DateDiff_TheDateToCalculateFrom, DataType.datetime),
         New clsFunctionParm("End Date", My.Resources.Resources.clsFunction_DateDiff_TheDateToCalculateTo, DataType.datetime)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_DateDiff_GetsTheNumberOfIntervalsEGDaysMonthsEtcBetweenTwoDatesSeeTheHelpDocumentationFo
            End Get
        End Property
    End Class

    Private Class clsFunction_MakeDate
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "MakeDate"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeDate_ShortDesc_MakeDate
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.date
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.number, DataType.number, DataType.number))

            'Deal with a two-digit year being passed in, for backwards compatibility,
            'although this functionality should NEVER be used ideally - it WILL lead
            'to broken processes, because different systems will interpret '30' as
            'either 1930 or 2030 depending on their settings. See bug #4216.
            Dim year As Integer = CInt(parms(2))
            If year <= 29 Then year += 2000
            If year < 100 Then year += 1900

            Try
                Return New clsProcessValue(DataType.date,
                 New Date(year, CInt(parms(1)), CInt(parms(0))))
            Catch ex As Exception
                Throw New clsFunctionException(
                 My.Resources.Resources.clsFunction_MakeDate_UnableToConvertThe3SuppliedParametersToADate)
            End Try
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Day", My.Resources.Resources.clsFunction_MakeDate_TheDayOfTheMonth, DataType.number),
         New clsFunctionParm("Month", My.Resources.Resources.clsFunction_MakeDate_TheMonthOfTheYearEGJan1Feb2, DataType.number),
         New clsFunctionParm("Year", My.Resources.Resources.clsFunction_MakeDate_TheYearDONOTUseATwoDigitYearIEUse1929Not29, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeDate_GetsADateMadeFromTheGivenDayMonthAndYearEGMakeDate122005ResultsIn01022005
            End Get
        End Property
    End Class


    Private Class clsFunction_MakeDateTime
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "MakeDateTime"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeDateTime_ShortDesc_MakeDateTime
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.datetime
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms,
             New Signature(
              DataType.number, DataType.number, DataType.number,
              DataType.number, DataType.number, DataType.number, DataType.flag))
            Dim dDate As Date

            If CInt(parms(2)) < 100 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_MakeDateTime_InvalidYear0TheCenturyMustBeSpecified, CInt(parms(2)))

            Try

                dDate = New Date(
                 CInt(parms(2)), CInt(parms(1)), CInt(parms(0)),
                 CInt(parms(3)), CInt(parms(4)), CInt(parms(5)))
                Return New clsProcessValue(DataType.datetime, dDate, CBool(parms(6)))
            Catch ex As Exception
                Throw New clsFunctionException(
                 My.Resources.Resources.clsFunction_MakeDateTime_UnableToConvertTheSuppliedParametersToADatetime0,
                 ex.Message)
            End Try
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Day", My.Resources.Resources.clsFunction_MakeDateTime_TheDayOfTheMonth, DataType.number),
         New clsFunctionParm("Month", My.Resources.Resources.clsFunction_MakeDateTime_TheMonthOfTheYearEGJan1Feb2, DataType.number),
         New clsFunctionParm("Year", My.Resources.Resources.clsFunction_MakeDateTime_TheYear, DataType.number),
         New clsFunctionParm("Hours", My.Resources.Resources.clsFunction_MakeDateTime_TheHours023, DataType.number),
         New clsFunctionParm("Minutes", My.Resources.Resources.clsFunction_MakeDateTime_TheMinutes059, DataType.number),
         New clsFunctionParm("Seconds", My.Resources.Resources.clsFunction_MakeDateTime_TheSeconds059, DataType.number),
         New clsFunctionParm("Local", My.Resources.Resources.clsFunction_MakeDateTime_TrueIfTheInputValueIsALocalTimeLocalTimeValuesWillBeConvertedToUTCWhenStored, DataType.flag)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeDateTime_GetsADatetimeMadeFromTheGivenDayMonthAndYearHoursMinutesAndSecondsEGMakeDateTim
            End Get
        End Property
    End Class


    Private Class clsFunction_MakeTime
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "MakeTime"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeTime_ShortDesc_MakeTime
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.time
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.number, DataType.number, DataType.number))
            Try
                Return New clsProcessValue(DataType.time,
                 DateTime.MinValue +
                 New TimeSpan(CInt(parms(0)), CInt(parms(1)), CInt(parms(2))))

            Catch
                Throw New clsFunctionException(
                 My.Resources.Resources.clsFunction_MakeTime_UnableToConvertTheSuppliedParametersToATime)
            End Try
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Hours", My.Resources.Resources.clsFunction_MakeTime_TheHours023, DataType.number),
         New clsFunctionParm("Minutes", My.Resources.Resources.clsFunction_MakeTime_TheMinutes059, DataType.number),
         New clsFunctionParm("Seconds", My.Resources.Resources.clsFunction_MakeTime_TheSeconds059, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeTime_GetsATimeMadeFromTheGivenHoursMinutesAndSecondsEGMakeTime13250ResultsIn132500
            End Get
        End Property
    End Class


    Private Class clsFunction_MakeTimeSpan
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "MakeTimeSpan"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeTimeSpan_ShortDesc_MakeTimeSpan
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.timespan
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms,
             New Signature(DataType.number, DataType.number, DataType.number, DataType.number),
             New Signature(DataType.number, DataType.number, DataType.number, DataType.number, DataType.number))

            Try
                Return New TimeSpan(
                 CInt(parms(0)), CInt(parms(1)), CInt(parms(2)), CInt(parms(3)), 0)
            Catch ex As Exception
                Throw New clsFunctionException(
                 My.Resources.Resources.clsFunction_MakeTimeSpan_UnableToConvertTheSuppliedParametersToATime0,
                 ex.Message)
            End Try
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Days", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfDays, DataType.number),
         New clsFunctionParm("Hours", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfHours023, DataType.number),
         New clsFunctionParm("Minutes", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfMinutes059, DataType.number),
         New clsFunctionParm("Seconds", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfSeconds059, DataType.number)
        }
        Public Overrides ReadOnly Property Signatures() As List(Of clsFunctionParm())
            Get
                Dim BaseSigs As List(Of clsFunctionParm()) = MyBase.Signatures
                Dim compatsig As clsFunctionParm() = {
                  New clsFunctionParm("Days", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfDays, DataType.number),
                  New clsFunctionParm("Hours", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfHours023, DataType.number),
                  New clsFunctionParm("Minutes", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfMinutes059, DataType.number),
                  New clsFunctionParm("Seconds", My.Resources.Resources.clsFunction_MakeTimeSpan_TheNumberOfSeconds059, DataType.number),
                  New clsFunctionParm("Ignored", My.Resources.Resources.clsFunction_MakeTimeSpan_ForBackwardsCompatibility, DataType.number)
                 }
                BaseSigs.Add(compatsig)
                Return BaseSigs
            End Get
        End Property
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_MakeTimeSpan_GetsATimespanMadeFromTheGivenDaysHoursMinutesSecondsEGMakeTimeSpan213250Results
            End Get
        End Property
        Public Overrides ReadOnly Property HelpDetailText() As String
            Get
                Return _
                My.Resources.Resources.clsFunction_MakeTimeSpan_ForHoursMinutesAndSecondsValuesOutsideOfTheGivenRangeCanBeSuppliedAndWillBeInte
            End Get
        End Property
    End Class


    Private Class clsFunction_AddMonths
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "AddMonths"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_AddMonths_ShortDesc_AddMonths
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.date
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.date, DataType.number))

            Dim mon As Integer
            Try
                mon = CInt(parms(1))
            Catch
                Throw New FunctionArgException(2, Name, "the number of months to add")
            End Try
            Dim dd As DateTime
            Try
                dd = CDate(parms(0))
            Catch
                Throw New FunctionArgException(1, Name, "a valid date")
            End Try
            Return New clsProcessValue(DataType.date, dd.AddMonths(mon))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Date", My.Resources.Resources.clsFunction_AddMonths_TheDateToAddTo, DataType.date),
         New clsFunctionParm("Months", My.Resources.Resources.clsFunction_AddMonths_maParms_TheNumberOfMonthsToAdd, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_AddMonths_AddsANumberOfMonthsToADateEgAddMonths250120053ResultsInTheDate25042005
            End Get
        End Property
    End Class

    Private Class clsFunction_AddDays
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "AddDays"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_AddDays_ShortDesc_AddDays
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.date
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.date, DataType.number))

            Dim mon As Integer
            Try
                mon = CInt(parms(1))
            Catch
                Throw New FunctionArgException(2, Name, "the number of days to add")
            End Try
            Dim dd As DateTime
            Try
                dd = CDate(parms(0))
            Catch
                Throw New FunctionArgException(1, Name, "a valid date")
            End Try
            Return New clsProcessValue(DataType.date, dd.AddDays(mon))
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Date", My.Resources.Resources.clsFunction_AddDays_TheDateToAddTo, DataType.date),
         New clsFunctionParm("Days", My.Resources.Resources.clsFunction_AddDays_maParms_TheNumberOfDaysToAdd, DataType.number)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_AddDays_AddANumberOfDaysToADateEgAddDays0102200520ResultsInTheDate21022005
            End Get
        End Property
    End Class

#Region "Format Date/Time functions"

    ''' <summary>
    ''' Function to format a local date
    ''' </summary>
    Private Class clsFunction_FormatDate : Inherits clsFunction

        ''' <summary>
        ''' The name of the function.
        ''' </summary>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "FormatDate"
            End Get
        End Property

        ''' <summary>
        ''' The short description of the function.
        ''' </summary>
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_FormatDate_ShortDesc_FormatDate
            End Get
        End Property
        ''' <summary>
        ''' The data type of the result of the function.
        ''' </summary>
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        ''' <summary>
        ''' The group name that this function belongs to.
        ''' </summary>
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Date"
            End Get
        End Property

        ''' <summary>
        ''' Evaluates the function with the given parameters within the specified
        ''' process.
        ''' </summary>
        ''' <param name="parms">The parameters passed into the function.</param>
        ''' <param name="objProcess">The process on which this function is being
        ''' called.</param>
        ''' <returns>The specified date formatted into the given format.</returns>
        ''' <exception cref="clsFunctionException">If any validation errors occur
        ''' while attempting to format the date - ie. wrong number of parameters,
        ''' parameters in wrong datatype or format </exception>
        Protected Overrides Function InnerEvaluate(
         ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            Return Evaluate(parms, objProcess, DateTimeStyles.None)
        End Function

        ''' <summary>
        ''' Evaluates the function using the given parameters, process and date time
        ''' styles value.
        ''' </summary>
        ''' <param name="params">The parameters passed into the function.</param>
        ''' <param name="proc">The process on which this function is being called.
        ''' </param>
        ''' <param name="styles">The date time style setting to use when parsing
        ''' the date. This can govern the type of output (local / UTC) or a hint
        ''' as to the type of input.</param>
        ''' <returns>A process value containing the date formatted to the required
        ''' specification.</returns>
        Protected Overloads Function Evaluate(ByVal params As IList(Of clsProcessValue),
         ByVal proc As clsProcess, ByVal styles As DateTimeStyles) _
         As clsProcessValue
            EnsureParams(params, New Signature(SupportedDataType, DataType.text))
            Try
                Return params(0).FormatDate(CStr(params(1)), styles)

            Catch ex As Exception
                Throw New clsFunctionException(
                 My.Resources.Resources.clsFunction_FormatDate_AnErrorOccurredWhileFormattingTheDate0, ex.Message)
            End Try

        End Function

        ''' <summary>
        ''' Gets the default parameters for this function
        ''' </summary>
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property

        ''' <summary>
        ''' The expected parameters for this functino - namely:
        ''' "Date:date, Format:text"
        ''' </summary>
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Date",
          String.Format(My.Resources.Resources.The0ToBeFormatted, GetFriendlyName(SupportedDataType)), SupportedDataType),
         New clsFunctionParm("Format", My.Resources.Resources.clsFunction_FormatDate_TheDateFormat, DataType.text)
        }

        ''' <summary>
        ''' The help text for this function.
        ''' </summary>
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return String.Format(My.Resources.Resources.clsFunction_FormatDate_GetsTextRepresentingA01InAGivenFormat3Eg201022003DdMMMYy3ResultsIn01Feb03,
                 Me.DateKind, Me.SupportedDataType, Me.Name, vbCrLf)
            End Get
        End Property

        ''' <summary>
        ''' Gets the data type that this function formats. By default, this is
        ''' a date.
        ''' </summary>
        Protected Overridable ReadOnly Property SupportedDataType() As DataType
            Get
                Return DataType.date
            End Get
        End Property

        ''' <summary>
        ''' The type of date - by default, this is 'local'.
        ''' This is used to tailor the help text
        ''' </summary>
        Protected Overridable ReadOnly Property DateKind() As String
            Get
                Return "local"
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Formats the date, treating it as a UTC date.
    ''' </summary>
    Private Class clsFunction_FormatDateTime : Inherits clsFunction_FormatDate
        ''' <summary>
        ''' The name of the function
        ''' </summary>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "FormatDateTime"
            End Get
        End Property
        ''' <summary>
        ''' A short description of the function
        ''' </summary>
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_FormatDateTime_ShortDesc_FormatDateTime
            End Get
        End Property

        ''' <summary>
        ''' Gets the data type that this function formats.
        ''' This function supports datetimes.
        ''' </summary>
        Protected Overrides ReadOnly Property SupportedDataType() As DataType
            Get
                Return DataType.datetime
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Formats the date, treating it as a UTC date.
    ''' </summary>
    Private Class clsFunction_FormatUTCDateTime : Inherits clsFunction_FormatDateTime
        ''' <summary>
        ''' The name of the function
        ''' </summary>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "FormatUTCDateTime"
            End Get
        End Property
        ''' <summary>
        ''' A short description of the function
        ''' </summary>
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_FormatUTCDateTime_ShortDesc_FormatUTCDateTime
            End Get
        End Property
        ''' <summary>
        ''' Evaluates the function using the given parameters.
        ''' </summary>
        ''' <param name="parms">The list of parameters to the function.</param>
        ''' <param name="objProcess">The process that this function is being called
        ''' on.</param>
        ''' <returns>The process value result of the function evaluation.</returns>
        ''' <exception cref="clsFunctionException">If any validation errors occur
        ''' while attempting to evaluate the function.</exception>
        Protected Overrides Function InnerEvaluate(
         ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            Return Evaluate(parms, objProcess, DateTimeStyles.AdjustToUniversal)
        End Function
        ''' <summary>
        ''' The type of date - in this subclass, this is 'UTC'
        ''' </summary>
        Protected Overrides ReadOnly Property DateKind() As String
            Get
                Return "UTC"
            End Get
        End Property

    End Class

#End Region

    Private Class clsFunction_LoadBinaryFile
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "LoadBinaryFile"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_LoadBinaryFile_LoadABinaryFile
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.binary
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "File"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text))
            Try
                Return File.ReadAllBytes(CStr(parms(0)))
            Catch ex As Exception
                Throw New clsFunctionException(String.Format(My.Resources.Resources.FailedToReadFile0, ex.Message))
            End Try
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Filename", My.Resources.Resources.clsFunction_LoadBinaryFile_TheNameOfTheFileToLoad, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_LoadBinaryFile_ReadTheContentsOfAFileAsBinaryData
            End Get
        End Property
    End Class

    Private Class clsFunction_LoadTextFile
        Inherits clsFunction
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "LoadTextFile"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_LoadTextFile_LoadATextFile
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "File"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.text))
            Try
                Return File.ReadAllText(CStr(parms(0)))
            Catch ex As Exception
                Throw New clsFunctionException(String.Format(My.Resources.Resources.FailedToReadFile0, ex.Message))
            End Try
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Filename", My.Resources.Resources.clsFunction_LoadTextFile_TheNameOfTheFileToLoad, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_LoadTextFile_ReadTheContentsOfATextFile
            End Get
        End Property
    End Class


    Private MustInherit Class clsIsFunction
        Inherits clsFunction

        Friend MustOverride ReadOnly Property DataTypeChecked() As DataType

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Is" & GetFriendlyName(DataTypeChecked, False, False)
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return String.Format(My.Resources.Resources.clsIsFunction_IsDataType0, GetFriendlyName(DataTypeChecked))
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.flag
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Logic"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 1 Then Throw New clsFunctionException(
             My.Resources.Resources.clsIsFunction_0FunctionRequires1Parameter, Name)
            ' Try casting it into the requird type. If it works, then it is a valid
            ' instance of that type
            Dim val As clsProcessValue = parms(0)
            Return (val.TryCastInto(DataTypeChecked, val) AndAlso Not val.IsNull)
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsIsFunction_TheTextToBeChecked, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return String.Format(
                 My.Resources.Resources.clsIsFunction_ChecksIfTheGivenTextIsAValid0AccordingToTheCurrentRegionalSettings1EgIs0Example,
                 GetFriendlyName(DataTypeChecked), vbCrLf, $"Is{GetFriendlyName(DataTypeChecked,False,False)}"
                )
            End Get
        End Property

    End Class

    Private Class clsFunction_IsNumber
        Inherits clsIsFunction
        Friend Overrides ReadOnly Property DataTypeChecked() As DataType
            Get
                Return DataType.number
            End Get
        End Property
    End Class

    Private Class clsFunction_IsDate
        Inherits clsIsFunction
        Friend Overrides ReadOnly Property DataTypeChecked() As DataType
            Get
                Return DataType.date
            End Get
        End Property
    End Class

    Private Class clsFunction_IsTime
        Inherits clsIsFunction
        Friend Overrides ReadOnly Property DataTypeChecked() As DataType
            Get
                Return DataType.time
            End Get
        End Property

    End Class

    Private Class clsFunction_IsDateTime
        Inherits clsIsFunction
        Friend Overrides ReadOnly Property DataTypeChecked() As DataType
            Get
                Return DataType.datetime
            End Get
        End Property
    End Class

    Private Class clsFunction_IsTimeSpan
        Inherits clsIsFunction
        Friend Overrides ReadOnly Property DataTypeChecked() As DataType
            Get
                Return DataType.timespan
            End Get
        End Property
    End Class

    Private Class clsFunction_IsFlag
        Inherits clsIsFunction
        Friend Overrides ReadOnly Property DataTypeChecked() As DataType
            Get
                Return DataType.flag
            End Get
        End Property
    End Class




    Private Class clsFunction_ExceptionType
        Inherits clsFunction
        Public Sub New(ByVal proc As clsProcess)
            mProcess = proc
        End Sub
        Private mProcess As clsProcess
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "ExceptionType"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_ExceptionType_CurrentExceptionType
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Exceptions"
            End Get
        End Property
        Public Overrides ReadOnly Property RecoveryOnly() As Boolean
            Get
                Return True
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_ExceptionType_ExceptionTypeFunctionShouldNotHaveAnyParameters)

            If Not mProcess.mRecoveryMode Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_ExceptionType_ExceptionTypeFunctionCanOnlyBeUsedWhenInRecoveryMode)

            Return mProcess.mRecoveryType

        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return mParms
            End Get
        End Property
        Private mParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_ExceptionType_TheTypeOfTheExceptionCurrentlyBeingRecoveredFrom
            End Get
        End Property
    End Class

    Private Class clsFunction_ExceptionDetail
        Inherits clsFunction
        Public Sub New(ByVal proc As clsProcess)
            mProcess = proc
        End Sub
        Private mProcess As clsProcess
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "ExceptionDetail"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_ExceptionDetail_CurrentExceptionDetail
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Exceptions"
            End Get
        End Property
        Public Overrides ReadOnly Property RecoveryOnly() As Boolean
            Get
                Return True
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_ExceptionDetail_ExceptionDetailFunctionShouldNotHaveAnyParameters)

            If Not mProcess.mRecoveryMode Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_ExceptionDetail_ExceptionDetailFunctionCanOnlyBeUsedWhenInRecoveryMode)

            Return mProcess.mRecoveryDetail
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return mParms
            End Get
        End Property
        Private mParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_ExceptionDetail_TheDetailForTheExceptionCurrentlyBeingRecoveredFrom
            End Get
        End Property
    End Class

    Private Class clsFunction_ExceptionStage
        Inherits clsFunction
        Public Sub New(ByVal proc As clsProcess)
            mProcess = proc
        End Sub
        Private mProcess As clsProcess
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "ExceptionStage"
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_ExceptionStage_CurrentExceptionSource
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Exceptions"
            End Get
        End Property
        Public Overrides ReadOnly Property RecoveryOnly() As Boolean
            Get
                Return True
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_ExceptionStage_ExceptionStageFunctionShouldNotHaveAnyParameters)

            If Not mProcess.mRecoveryMode Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_ExceptionStage_ExceptionStageFunctionCanOnlyBeUsedWhenInRecoveryMode)

            Return mProcess.GetStage(mProcess.mRecoverySource).Name
        End Function
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return mParms
            End Get
        End Property
        Private mParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_ExceptionStage_TheNameOfTheStageWhichIsTheSourceOfTheExceptionCurrentlyBeingHandled
            End Get
        End Property
    End Class


#Region " Data Type Conversion "

    ''' <summary>
    ''' Base class for a very basic conversion function, which just uses the data
    ''' casting rules to take one parameter and convert it to a different type.
    ''' The default handling only specifies a single text parameter; in order to
    ''' specify overloads, the <see cref="clsFunction.Signatures"/> property should
    ''' be overridden by subclasses.
    ''' </summary>
    Private MustInherit Class clsFunction_ToOtherType : Inherits clsFunction

        ''' <summary>
        ''' The data type output from this function
        ''' </summary>
        Public MustOverride Overrides ReadOnly Property DataType() As DataType

        ' The set of input params to use in this function
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("Text", My.Resources.Resources.clsFunction_ToOtherType_TheTextToBeConverted, DataType.text)
        }

        ''' <summary>
        ''' The default signature for this function
        ''' </summary>
        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property

        ''' <summary>
        ''' Applies this function to the given parameters within the context of the
        ''' given process
        ''' </summary>
        ''' <param name="parms">The parameters to apply the function to</param>
        ''' <param name="proc">The process within which the function is being
        ''' evaluated</param>
        ''' <returns>The result of applying this function to the given parameters.
        ''' </returns>
        ''' <exception cref="clsFunctionException">If the parameters were invalid or
        ''' the function could not be applied for some reason.</exception>
        Protected Overrides Function InnerEvaluate(
         ByVal parms As IList(Of clsProcessValue),
         ByVal proc As clsProcess) As clsProcessValue
            If parms.Count <> 1 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_ToOtherType_0FunctionRequires1Parameter, Name)

            Try
                Return parms(0).CastInto(Me.DataType)
            Catch ex As Exception
                Throw New clsFunctionException(
                 My.Resources.Resources.clsFunction_ToOtherType_UnableToConvertSuppliedValueToA01,
                 DataTypeLocalizedFormalName, ex.Message)
            End Try


        End Function

        ''' <summary>
        ''' The formal name for the datatype used for this function. This generally
        ''' follows the 'friendly' name of the data type and is used in the name of
        ''' the function (eg. 'ToDate', 'ToNumber', 'ToDateTime') as well as in the
        ''' error and help messages ('Converts a value to a Number' etc).
        ''' </summary>
        Protected ReadOnly Property DataTypeLocalizedFormalName() As String
            Get
                Return GetFriendlyName(Me.DataType)
            End Get
        End Property

        ''' <summary>
        ''' The formal name for the datatype used for this function. This generally
        ''' follows the 'friendly' name of the data type and is used in the name of
        ''' the function (eg. 'ToDate', 'ToNumber', 'ToDateTime') as well as in the
        ''' error and help messages ('Converts a value to a Number' etc).
        ''' </summary>
        Protected ReadOnly Property DataTypeFormalName() As String
            Get
                Return GetFriendlyName(Me.DataType, False, False)
            End Get
        End Property

        ''' <summary>
        ''' The name of the function group that this function falls within
        ''' </summary>
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Conversion"
            End Get
        End Property

        ''' <summary>
        ''' The help text explaining what this function does
        ''' </summary>
        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return String.Format(
                 My.Resources.Resources.clsFunction_ToOtherType_ConvertsAValueToA0FollowingTheStandardBluePrismDataCastingRules,
                 DataTypeLocalizedFormalName)
            End Get
        End Property

        ''' <summary>
        ''' The name of this function
        ''' </summary>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return String.Format("To{0}", DataTypeFormalName)
            End Get
        End Property

        ''' <summary>
        ''' A short description of this function
        ''' </summary>
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return String.Format(My.Resources.Resources.clsFunction_ToOtherType_ConvertsTo0, DataTypeLocalizedFormalName)
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Function to convert a text value to a number
    ''' </summary>
    Private Class clsFunction_ToNumber : Inherits clsFunction_ToOtherType

        ''' <summary>
        ''' The type of output from this function (<see cref="AutomateProcessCore.DataType.number"/>)
        ''' </summary>
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Function to convert a text value to a time
    ''' </summary>
    Private Class clsFunction_ToTime : Inherits clsFunction_ToOtherType

        ''' <summary>
        ''' The type of output from this function (<see cref="AutomateProcessCore.DataType.time"/>)
        ''' </summary>
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.time
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Function to convert a text value to a datetime
    ''' </summary>
    Private Class clsFunction_ToDateTime : Inherits clsFunction_ToOtherType

        ''' <summary>
        ''' The type of output from this function (see "DataType.datetime")
        ''' </summary>
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.datetime
            End Get
        End Property

        ''' <summary>
        ''' The signatures supported by this function. This override can convert date
        ''' or time arguments as well as the standard text argument.
        ''' </summary>
        Public Overrides ReadOnly Property Signatures() As List(Of clsFunctionParm())
            Get
                Dim sigs As List(Of clsFunctionParm()) = MyBase.Signatures
                sigs.Add(New clsFunctionParm() {New clsFunctionParm("Date", My.Resources.Resources.clsFunction_ToDateTime_TheDateToBeConverted, DataType.date)})
                sigs.Add(New clsFunctionParm() {New clsFunctionParm("Time", My.Resources.Resources.clsFunction_ToDateTime_TheTimeToBeConverted, DataType.time)})
                Return sigs
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Function to convert a text value to a date
    ''' </summary>
    Private Class clsFunction_ToDate : Inherits clsFunction_ToOtherType

        ''' <summary>
        ''' The type of output from this function (<see cref="AutomateProcessCore.DataType.date"/>)
        ''' </summary>
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.date
            End Get
        End Property

        ''' <summary>
        ''' The signatures supported by this function. This override can convert
        ''' datetime arguments as well as the standard text argument.
        ''' </summary>
        Public Overrides ReadOnly Property Signatures() As List(Of clsFunctionParm())
            Get
                Dim sigs As List(Of clsFunctionParm()) = MyBase.Signatures
                sigs.Add(New clsFunctionParm() {New clsFunctionParm("DateTime", My.Resources.Resources.clsFunction_ToDate_TheDateTimeToBeConverted, DataType.datetime)})
                Return sigs
            End Get
        End Property
    End Class

#End Region

#Region " Extracting TimeSpan Components "

    Private MustInherit Class clsToTimeSpanComponent
        Inherits clsFunction

        Friend MustOverride ReadOnly Property TSComponent() As String
        Friend MustOverride Function Convert(ByVal ts As TimeSpan) As Integer

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "To" & TSComponent
            End Get
        End Property
        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return String.Format(My.Resources.Resources.clsToTimeSpanComponent_ConvertTo0, TSComponent)
            End Get
        End Property
        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property
        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Conversion"
            End Get
        End Property
        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            EnsureParams(parms, New Signature(DataType.timespan))
            Return Convert(CType(parms(0), TimeSpan))
        End Function
        ''' <summary>
        ''' Gets the localized friendly name For this TSCmponent according To the current culture.
        ''' </summary>
        ''' <param name="key">The string to retrieve </param>
        ''' <returns>The name of the data type for the current culture</returns>
        Public Shared Function GetLocalizedFriendlyName(key As String) As String
            Return My.Resources.Resources.ResourceManager.GetString($"clsToTimeSpanComponent_{key}")
        End Function

        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {
         New clsFunctionParm("TimeSpan", My.Resources.Resources.clsToTimeSpanComponent_TheTimeSpanToBeConverted, DataType.text)
        }
        Public Overrides ReadOnly Property HelpText() As String
            Get
                If TSComponent = "Seconds" Then
                    Return String.Format(My.Resources.Resources.clsToTimeSpanComponent_ConvertsATimeSpanToATotalNumberOf0, GetLocalizedFriendlyName(TSComponent))
                Else
                    Return String.Format(My.Resources.Resources.clsToTimeSpanComponent_ConvertsATimeSpanToAWholeNumberRoundedDownOf0, GetLocalizedFriendlyName(TSComponent))
                End If
            End Get
        End Property
        Public Overrides ReadOnly Property HelpDetailText() As String
            Get
                If TSComponent = "Seconds" Then
                    Return ""
                Else
                    Return My.Resources.Resources.clsToTimeSpanComponent_WhenRoundingTheFractionalComponentIsDiscardedSoForExample90MinutesBecomes1HourF
                End If
            End Get
        End Property

    End Class

    Private Class clsFunction_ToDays
        Inherits clsToTimeSpanComponent
        Friend Overrides ReadOnly Property TSComponent() As String
            Get
                Return "Days"
            End Get
        End Property
        Friend Overrides Function Convert(ByVal ts As TimeSpan) As Integer
            Return CInt(Fix(ts.TotalDays()))
        End Function
    End Class
    Private Class clsFunction_ToHours
        Inherits clsToTimeSpanComponent
        Friend Overrides ReadOnly Property TSComponent() As String
            Get
                Return "Hours"
            End Get
        End Property
        Friend Overrides Function Convert(ByVal ts As TimeSpan) As Integer
            Return CInt(Fix(ts.TotalHours()))
        End Function
    End Class
    Private Class clsFunction_ToMinutes
        Inherits clsToTimeSpanComponent
        Friend Overrides ReadOnly Property TSComponent() As String
            Get
                Return "Minutes"
            End Get
        End Property
        Friend Overrides Function Convert(ByVal ts As TimeSpan) As Integer
            Return CInt(Fix(ts.TotalMinutes()))
        End Function
    End Class
    Private Class clsFunction_ToSeconds
        Inherits clsToTimeSpanComponent
        Friend Overrides ReadOnly Property TSComponent() As String
            Get
                Return "Seconds"
            End Get
        End Property
        Friend Overrides Function Convert(ByVal ts As TimeSpan) As Integer
            Return CInt(Fix(ts.TotalSeconds()))
        End Function
    End Class

#End Region


    Private Class clsFunction_GetOSArchitecture
        Inherits clsFunction

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property

        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal proc As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then
                Throw New clsFunctionException(My.Resources.Resources.clsFunction_GetOSArchitecture_GetOSArchitectureFunctionShouldNotHaveAnyParameters)
            End If
            Dim archtype As String = "32bit"
            If BPUtil.Is64BitOperatingSystem Then
                archtype = "64bit"
            End If
            Return New clsProcessValue(DataType.text, archtype)
        End Function

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Environment"
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSArchitecture_GetTheOperatingSystemArcitecture
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetOSArchitecture"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSArchitecture_GetTheOperatingSystemArchitecture
            End Get
        End Property
    End Class

    Private Class clsFunction_GetOSVersionMajor
        Inherits clsFunction

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As AutomateProcessCore.clsProcess) As AutomateProcessCore.clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_GetOSVersionMajor_GetOSVersionMajorFunctionShouldNotHaveAnyParameters)
            Return BPUtil.GetOSVersion.Major
        End Function

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Environment"
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSVersionMajor_GetTheOperatingSystemMajorVersionNumber
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetOSVersionMajor"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSVersionMajor_GetTheOperatingSystemMajorVersionNumber
            End Get
        End Property
    End Class

    Private Class clsFunction_GetOSVersionMinor
        Inherits clsFunction

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As AutomateProcessCore.clsProcess) As AutomateProcessCore.clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_GetOSVersionMinor_GetOSVersionMinorFunctionShouldNotHaveAnyParameters)
            Return BPUtil.GetOSVersion.Minor
        End Function

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Environment"
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSVersionMinor_GetTheOperatingSystemMinorVersionNumber
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetOSVersionMinor"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSVersionMinor_GetOperatingSystemMinorVersionNumber
            End Get
        End Property
    End Class

    Private Class clsFunction_GetIEVersionMajor
        Inherits clsFunction

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_GetIEVersionMajor_GetIEVersionMajorFunctionShouldNotHaveAnyParameters)
            Return GetIEVersionMajor()
        End Function

        Private Function GetIEVersionMajor() As Integer
            Try
                Using regKey As RegistryKey = Registry.LocalMachine.OpenSubKey(
                 "Software\Microsoft\Internet Explorer", False)
                    Dim ver = CStr(
                     If(regKey.GetValue("svcVersion"), regKey.GetValue("Version")))
                    Return CInt(ver.Split("."c)(0))
                End Using
            Catch ex As Exception
                Throw New clsFunctionException(String.Format(My.Resources.Resources.clsFunction_GetIEVersionMajor_FailedToGetInternetExplorerVersion0, ex.Message))
            End Try
        End Function

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Environment"
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_GetIEVersionMajor_GetInternetExplorerMajorVersionNumber
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetIEVersionMajor"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_GetIEVersionMajor_GetInternetExplorerMajorVersionNumber
            End Get
        End Property
    End Class

    Private Class clsFunction_GetOSVersion
        Inherits clsFunction

        Public Overrides ReadOnly Property Deprecated() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.text
            End Get
        End Property

        Protected Overrides Function InnerEvaluate(ByVal parms As IList(Of clsProcessValue), ByVal objProcess As clsProcess) As clsProcessValue
            If parms.Count <> 0 Then Throw New clsFunctionException(
             My.Resources.Resources.clsFunction_GetOSVersion_GetOSVersionFunctionShouldNotHaveAnyParameters)

            Return GetOSVersion()
        End Function

        ''' <summary>
        ''' This function returns a string containing the OS version name.
        ''' </summary>
        ''' <returns>
        ''' One of the following
        ''' "Windows 95"
        ''' "Windows 98"
        ''' "Windows 98 Second Edition"
        ''' "Windows Me"
        ''' "Windows NT 3.51"
        ''' "Windows NT 4.0"
        ''' "Windows 2000"
        ''' "Windows XP"
        ''' "Windows Server 2003"
        ''' "Windows Vista"
        ''' "Windows 7"
        ''' </returns>
        ''' <remarks>Changes to this will affect the behaviour of the exposed function
        ''' in clsEnvironmentFunctions.</remarks>
        Private Function GetOSVersion() As String
            Dim v As Version = BPUtil.GetOSVersion
            Select Case Environment.OSVersion.Platform
                Case PlatformID.Win32Windows
                    Select Case v.Minor
                        Case 0 : Return "Windows 95"
                        Case 10
                            ' NOTE: This was 'If v.Revision.ToString() = "2222A"'
                            ' which is what MS says in KB304283, but since v.Revision
                            ' is an integer, its ToString() could never return an 'A'
                            ' Since we don't support Win98 anyway, I guess this is
                            ' academic, but hey. Might as well make sense in our
                            ' completely redundant code.
                            If v.Revision = 2222 Then
                                Return "Windows 98 Second Edition"
                            Else
                                Return "Windows 98"
                            End If
                        Case 90 : Return "Windows Me"
                    End Select

                Case PlatformID.Win32NT
                    Select Case v.Major
                        Case 3 : Return "Windows NT 3.51"
                        Case 4 : Return "Windows NT 4.0"
                        Case 5
                            Select Case v.Minor
                                Case 0 : Return "Windows 2000"
                                Case 1 : Return "Windows XP"
                                Case 2 : Return "Windows Server 2003"
                            End Select
                        Case 6
                            Select Case v.Minor
                                Case 0 : Return "Windows Vista"
                                Case 1 : Return "Windows 7"
                                Case 2 : Return "Windows 8"
                                Case 3 : Return "Windows 8.1"
                            End Select
                        Case 10
                            Return "Windows 10"
                        Case Else
                            Return My.Resources.Resources.clsFunction_GetOSVersion_Failed
                    End Select
            End Select

            Return My.Resources.Resources.clsFunction_GetOSVersion_Failed
        End Function

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Environment"
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSVersion_GetTheOperatingSystemVersionForExampleWindows2000OrWindowsXP
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "GetOSVersion"
            End Get
        End Property

        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return maParms
            End Get
        End Property
        Private maParms As clsFunctionParm() = {}

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return My.Resources.Resources.clsFunction_GetOSVersion_GetOperatingSystemVersion
            End Get
        End Property
    End Class


    ''' <summary>
    ''' A list of all defined group names.
    ''' </summary>
    Public ReadOnly Property GroupNames() As String()
        Get
            'If it became a performance issue, the creation of this list could
            'happen when the clsFunctions is constructed.
            Dim ss As New List(Of String)
            For Each s As String In mFunctions.Keys
                Dim f As clsFunction = mFunctions(s)
                If Not ss.Contains(f.GroupName) Then
                    ss.Add(f.GroupName)
                End If
            Next
            Return ss.ToArray()
        End Get
    End Property


    ''' <summary>
    ''' A Dictionary containing objects derived from clsFunction, each
    ''' representing an available Function that can be called. The
    ''' Dictionary is keyed on the Function names.
    ''' </summary>
    Public ReadOnly Property All() As Dictionary(Of String, clsFunction)
        Get
            Return mFunctions
        End Get
    End Property
    Private mFunctions As Dictionary(Of String, clsFunction)

    ''' <summary>
    ''' The process instance this object belongs to.
    ''' </summary>
    ''' <remarks></remarks>
    Private mProcess As clsProcess

    ''' <summary>
    ''' Get the handler for a particular function.
    ''' </summary>
    ''' <param name="sName">The name of the funcion.</param>
    ''' <returns>A reference to a clsFunction-derived handler, or Nothing if the
    ''' function is not defined.</returns>
    Public Function GetFunction(ByVal sName As String) As clsFunction
        Dim fn As clsFunction = Nothing
        mFunctions.TryGetValue(sName, fn)
        Return fn
    End Function

    ''' <summary>
    ''' Generates all the inbuilt functions for the given process.
    ''' </summary>
    ''' <param name="proc">The process for which the functions are required.</param>
    ''' <returns>A collection of clsFunction objects for the given process.</returns>
    Public Shared Function GenerateFunctions(ByVal proc As clsProcess) As ICollection(Of clsFunction)
        Return New clsFunction() {
         New clsFunction_Round(),
         New clsFunction_RndUp(),
         New clsFunction_RndDn(),
         New clsFunction_DecPad(),
         New clsFunction_Sqrt(),
         New clsFunction_Log(),
         New clsFunction_Left(),
         New clsFunction_Right(),
         New clsFunction_Mid(),
         New clsFunction_Chr(),
         New clsFunction_NewLine(),
         New clsFunction_InStr(),
         New clsFunction_Len(),
         New clsFunction_Bytes(),
         New clsFunction_Trim(),
         New clsFunction_TrimStart(),
         New clsFunction_TrimEnd(),
         New clsFunction_Replace(),
         New clsFunction_StartsWith(),
         New clsFunction_EndsWith(),
         New clsFunction_Upper(),
         New clsFunction_Lower(),
         New clsFunction_Today(),
         New clsFunction_Now(),
         New clsFunction_LocalTime(),
         New clsFunction_UTCTime(),
         New clsFunction_DateAdd(),
         New clsFunction_DateDiff(),
         New clsFunction_MakeDate(),
         New clsFunction_MakeDateTime(),
         New clsFunction_MakeTime(),
         New clsFunction_MakeTimeSpan(),
         New clsFunction_AddMonths(),
         New clsFunction_AddDays(),
         New clsFunction_FormatDate(),
         New clsFunction_FormatDateTime(),
         New clsFunction_FormatUTCDateTime(),
         New clsFunction_LoadBinaryFile(),
         New clsFunction_LoadTextFile(),
         New clsFunction_IsNumber(),
         New clsFunction_IsDate(),
         New clsFunction_IsTime(),
         New clsFunction_IsDateTime(),
         New clsFunction_IsTimeSpan(),
         New clsFunction_IsFlag(),
         New clsFunction_ExceptionType(proc),
         New clsFunction_ExceptionDetail(proc),
         New clsFunction_ExceptionStage(proc),
         New clsFunction_ToNumber(),
         New clsFunction_ToDate(),
         New clsFunction_ToDateTime(),
         New clsFunction_ToTime(),
         New clsFunction_ToDays(),
         New clsFunction_ToHours(),
         New clsFunction_ToMinutes(),
         New clsFunction_ToSeconds(),
         New clsFunction_GetOSVersion(),
         New clsFunction_GetOSArchitecture(),
         New clsFunction_GetOSVersionMajor(),
         New clsFunction_GetOSVersionMinor(),
         New clsFunction_GetIEVersionMajor()
        }

    End Function

    ''' <summary>
    ''' Create a new instance of clsFunctions. See main class documentation for
    ''' restrictions on when and where this can be done.
    ''' </summary>
    ''' <param name="proc">The process instance this object belongs to.</param>
    Friend Sub New(ByVal proc As clsProcess)
        mProcess = proc
        mFunctions = New Dictionary(Of String, clsFunction)

        'Add all internal/pre-defined functions...
        For Each f As clsFunction In GenerateFunctions(proc)
            mFunctions.Add(f.Name, f)
        Next

        'Now the add-ons...
        For Each f As clsFunction In clsAPC.AddOnFunctions
            mFunctions.Add(f.Name, f)
        Next

    End Sub



End Class

