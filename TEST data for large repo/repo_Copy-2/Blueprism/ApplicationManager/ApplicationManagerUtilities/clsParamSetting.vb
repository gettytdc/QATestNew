
''' <summary>
''' Class to represent a parameter setting (argument?). It is used to override params
''' in subqueries so that the original query remains unchanged if a subquery is
''' required.
''' <seealso cref="clsQuery.WithParams"/>
''' </summary>
Public Class clsParamSetting

    ' The type of the parameter
    Private mParamType As String

    ' The value of the parameter
    Private mVal As String

    ''' <summary>
    ''' Creates a new parameter setting representing the given type and value.
    ''' </summary>
    ''' <param name="paramType">The type of the parameter</param>
    ''' <param name="val">The value of the parameter</param>
    Public Sub New(ByVal paramType As String, ByVal val As String)
        mParamType = paramType
        mVal = val
    End Sub

    ''' <summary>
    ''' The type of the parameter
    ''' </summary>
    Public ReadOnly Property ParamType() As String
        Get
            Return mParamType
        End Get
    End Property

    ''' <summary>
    ''' The value of the parameter
    ''' </summary>
    Public ReadOnly Property Value() As String
        Get
            Return mVal
        End Get
    End Property

End Class
