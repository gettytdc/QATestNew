
''' <summary>
''' Represents a parameter to a function.
''' </summary>
Public Class clsFunctionParm

    ''' <summary>
    ''' Constant to refer to an empty signature.
    ''' </summary>
    Public Shared EmptySignature() As clsFunctionParm = {}

    ''' <summary>
    ''' Creates a new function parameter instance
    ''' </summary>
    ''' <param name="name">The name of the function parameter</param>
    ''' <param name="helpTxt">The help text for the parameter</param>
    ''' <param name="dtype">The data type of the parameter</param>
    Public Sub New( _
     ByVal name As String, ByVal helpTxt As String, ByVal dtype As DataType)
        mName = name
        mHelpText = helpTxt
        mDataType = dtype
    End Sub

    ''' <summary>
    ''' The name of the parameter.
    ''' </summary>
    Public ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property
    Private mName As String

    ''' <summary>
    ''' The help text for the parameter.
    ''' </summary>
    Public ReadOnly Property HelpText() As String
        Get
            Return mHelpText
        End Get
    End Property
    Private mHelpText As String

    ''' <summary>
    ''' The data type for the parameter.
    ''' </summary>
    Public ReadOnly Property DataType() As DataType
        Get
            Return mDataType
        End Get
    End Property
    Private mDataType As DataType

End Class

