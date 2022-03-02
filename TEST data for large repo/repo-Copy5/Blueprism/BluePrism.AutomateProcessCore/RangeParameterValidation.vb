Imports System.Xml


Public Class RangeParameterValidation
    Implements IParameterValidation

    Private mLowThreshold As Integer
    Private mHighThreshold As Integer

    Public Sub New()

    End Sub

    Public Sub New(low As Integer, high As Integer)
        mLowThreshold = low
        mHighThreshold = high

    End Sub


    Public Function Clone() As RangeParameterValidation Implements IParameterValidation.Clone
        Return New RangeParameterValidation(mLowThreshold, mHighThreshold)
    End Function


    Public Property Parameter As String Implements IParameterValidation.Parameter
        Get
            Return $"{mLowThreshold}:{mHighThreshold}"
        End Get
        Set(value As String)

            If Not value.Contains(":") Then Throw New ArgumentException("Invalid parameter format")

            Dim split = value.Split(CType(":", Char()))
            mLowThreshold = Integer.Parse(split(0))
            mHighThreshold = Integer.Parse(split(1))

        End Set
    End Property

    Public Function Validate(map As String) As Boolean Implements IParameterValidation.Validate
        If String.IsNullOrEmpty(map) Then
            Return True  'no value is valid and should pass validation for the range check
        End If

        Dim checkReult = True
        Dim number As Long
        Long.TryParse(map, number)
        If number < mLowThreshold Or number > mHighThreshold Then
            checkReult = False
        End If
        Return checkReult
    End Function

    Public Function Message() As String Implements IParameterValidation.Message
        Return String.Format(My.Resources.Resources.RangeParameterValidationMessage, mLowThreshold, mHighThreshold)
    End Function

    Public Function ToXML(ByVal parentDocument As XmlDocument) As XmlElement Implements IParameterValidation.ToXML
        If parentDocument Is Nothing Then
            Throw New ArgumentNullException(NameOf(parentDocument))
        End If

        Dim retval As XmlElement
        retval = parentDocument.CreateElement("validator")
        retval.SetAttribute("type", GetType(RangeParameterValidation).FullName)
        retval.SetAttribute("parameter", Parameter)
        Return retval
    End Function
End Class

