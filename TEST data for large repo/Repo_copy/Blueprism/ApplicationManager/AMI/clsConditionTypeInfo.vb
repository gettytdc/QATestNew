Imports BluePrism.BPCoreLib.Collections

Namespace BluePrism.ApplicationManager.AMI
    ''' Project  : AMI
    ''' Class    : clsConditionTypeInfo
    ''' 
    ''' <summary>
    ''' This class is used to return information to the client about a particular
    ''' condition Type.
    ''' 
    ''' Note that this class is functionally immutable - ie. once an instance has
    ''' been created it cannot be externally modified.
    ''' </summary>
    Public Class clsConditionTypeInfo
        Inherits clsActionTypeInfo

        Private mDefaultValue As String
        Private mDataType As String

        ''' <summary>
        ''' Creates a new non-focusing condition with no default value and the given
        ''' arguments.
        ''' </summary>
        ''' <param name="sID">The ID of the condition</param>
        ''' <param name="sName">The name of the condition</param>
        ''' <param name="sDataType">The datatype tested within the condition</param>
        ''' <param name="sHelpText">A description of the condition</param>
        ''' <param name="args">The arguments required for the condition</param>
        Friend Sub New(ByVal sID As String, ByVal sName As String, ByVal sDataType As String, ByVal sHelpText As String, ByVal ParamArray args() As clsArgumentInfo)
            Me.New(sID, sName, sDataType, sHelpText, False, "", args)
        End Sub

        ''' <summary>
        ''' Creates a new non-focusing condition with the given arguments.
        ''' </summary>
        ''' <param name="sID">The ID of the condition</param>
        ''' <param name="sName">The name of the condition</param>
        ''' <param name="sDataType">The datatype tested within the condition</param>
        ''' <param name="sHelpText">A description of the condition</param>
        ''' <param name="defaultVal">The default test value for the condition, rendered
        ''' as a string</param>
        ''' <param name="args">The arguments required for the condition</param>
        Friend Sub New(ByVal sID As String, ByVal sName As String, ByVal sDataType As String, ByVal sHelpText As String, ByVal defaultVal As String, ByVal ParamArray args() As clsArgumentInfo)
            Me.New(sID, sName, sDataType, sHelpText, False, defaultVal, args)
        End Sub

        ''' <summary>
        ''' Creates a new condition with the given arguments.
        ''' </summary>
        ''' <param name="sID">The ID of the condition</param>
        ''' <param name="sName">The name of the condition</param>
        ''' <param name="sDataType">The datatype tested within the condition</param>
        ''' <param name="sHelpText">A description of the condition</param>
        ''' <param name="defaultVal">The default test value for the condition, rendered
        ''' as a string</param>
        ''' <param name="requiresFocus">True if the action requires the target
        ''' application to be focussed.</param>
        ''' <param name="args">The arguments required for the condition</param>
        Friend Sub New(ByVal sID As String, ByVal sName As String,
     ByVal sDataType As String, ByVal sHelpText As String,
     ByVal requiresFocus As Boolean, ByVal defaultVal As String,
     ByVal ParamArray args() As clsArgumentInfo)
            MyBase.New(sID, Nothing, sName, sHelpText, requiresFocus, "",
         GetEmpty.ICollection(Of String), args)
            mDataType = sDataType
            mDefaultValue = defaultVal
        End Sub

        ''' <summary>
        ''' The data type of the argument - i.e. the internal Automate datatype
        ''' identifier -  (e.g. "text", "number").
        ''' </summary>
        Public ReadOnly Property DataType() As String
            Get
                Return mDataType
            End Get
        End Property

        ''' <summary>
        ''' The default value, that is likely to be checked against this condition - e.g.
        ''' "True" for "CheckExists". Used to pre-populate the UI.
        ''' </summary>
        Public ReadOnly Property DefaultValue() As String
            Get
                Return mDefaultValue
            End Get
        End Property

    End Class
End Namespace