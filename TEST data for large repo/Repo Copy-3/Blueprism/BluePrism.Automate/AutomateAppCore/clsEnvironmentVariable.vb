Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data

''' <summary>
''' Class to represent an environment variable.
''' </summary>
<Serializable, DataContract([Namespace]:="bp", Name:="ev")>
Public Class clsEnvironmentVariable

#Region " Member Variables "

    ' The old name of this variable after a rename
    <DataMember(Name:="a")>
    Private mOldName As String

    ' The name of this variable
    <DataMember(Name:="b")>
    Private mName As String

    ' The description of this variable
    <DataMember(Name:="c")>
    Private mDescription As String

    ' The current value of this variable - this may be null if the value has not been
    ' initialised
    <DataMember(Name:="d")>
    Private mValue As clsProcessValue

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty environment variable
    ''' </summary>
    Public Sub New()
        Me.New(EmptyDataProvider.Instance)
    End Sub

    ''' 
    ''' <summary>
    ''' Creates a new environment variable using data from the given provider.
    ''' This expects the following attributes :- <list>
    ''' <item>name: String</item>
    ''' <item>description: String</item>
    ''' <item>datatype: String</item>
    ''' <item>value: String</item></list>
    ''' </summary>
    ''' <param name="prov">The provider giving the data for the variable</param>
    Public Sub New(ByVal prov As IDataProvider)
        Me.New(prov.GetString("name"), New clsProcessValue(prov),
         prov.GetString("description"))
    End Sub

    ''' <summary>
    ''' Creates a new environment variable with the given properties.
    ''' </summary>
    ''' <param name="name">The name of the variable</param>
    ''' <param name="val">The value of the variable</param>
    ''' <param name="desc">The description of the variable</param>
    Public Sub New(name As String,
                    val As clsProcessValue,
                    desc As String)
        mName = name
        ' Default the old name at construction time to the current name
        ' After this point, it's the caller's responsibility.
        mOldName = mName
        mValue = val
        mDescription = desc
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The current name of this variable
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' The 'old' name of this variable for renames.
    ''' </summary>
    ''' <remarks>This is completely reliant on external classes setting this at the
    ''' correct time and maintaining any required context, and thus it's not ideal.
    ''' I'll revisit this when we get time -- Stu</remarks>
    Public Property OldName() As String
        Get
            Return mOldName
        End Get
        Set(ByVal value As String)
            mOldName = value
        End Set
    End Property

    ''' <summary>
    ''' The description of this variable
    ''' </summary>
    Public Property Description() As String
        Get
            Return mDescription
        End Get
        Set(ByVal value As String)
            mDescription = value
        End Set
    End Property

    ''' <summary>
    ''' The current value of this variable - this will never be null.
    ''' </summary>
    Public Property Value() As clsProcessValue
        Get
            If mValue Is Nothing Then mValue = New clsProcessValue()
            Return mValue
        End Get
        Set(ByVal value As clsProcessValue)
            mValue = value
        End Set
    End Property

    ''' <summary>
    ''' The datatype of this environment variable.
    ''' </summary>
    Public ReadOnly Property DataType() As DataType
        Get
            If mValue Is Nothing Then Return DataType.unknown
            Return mValue.DataType
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Validates the current state of this env var object, returning an appropriate
    ''' error message if it is currently in an invalid state.
    ''' </summary>
    ''' <returns>An error message if this env var is in an invalid state; null
    ''' otherwise.</returns>
    Public Function Validate() As String
        If mName = "" Then Return My.Resources.clsEnvironmentVariable_EnvironmentVariablesMustHaveAName
        Return Nothing
    End Function

    ''' <summary>
    ''' Checks if this env var is equal in value to the given object.
    ''' To be considered equal, the supplied object must be an environment variable
    ''' with the same name, description, datatype and value.
    ''' </summary>
    ''' <param name="obj">The object to test for equality against.</param>
    ''' <returns>True if the given object is an env var with the same value as this
    ''' env var; False otherwise.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim ev As clsEnvironmentVariable = TryCast(obj, clsEnvironmentVariable)
        Return (SameVariable(ev) AndAlso SameValue(ev))
    End Function

    ''' <summary>
    ''' Checks if the given env var represents the same variable as this variable,
    ''' ignoring the current value - ie. tests if the given env var has the same
    ''' name, description and data type as this env var.
    ''' </summary>
    ''' <param name="ev">The env var to test.</param>
    ''' <returns>True if the given env var has the same configuration as this env
    ''' var, disregarding the current value.</returns>
    Public Function SameVariable(ByVal ev As clsEnvironmentVariable) As Boolean
        Return (ev IsNot Nothing AndAlso _
         ev.Name = Name AndAlso ev.Description = Description AndAlso ev.DataType = Me.DataType)
    End Function

    ''' <summary>
    ''' Checks if the given env var has the same value as this variable, disregarding
    ''' the name and description.
    ''' </summary>
    ''' <param name="ev">The env var to check its value against.</param>
    ''' <returns>True if the value in the given env var matches the value in this
    ''' env var (according to <see cref="clsProcessValue.Equals"/>; False otherwise.
    ''' </returns>
    Public Function SameValue(ByVal ev As clsEnvironmentVariable) As Boolean
        If ev Is Nothing Then Return False
        Dim val As clsProcessValue = ev.mValue

        ' Check empty values first. For the purposes of an env var, a null value
        ' is the same as a value of unknown type.
        If val Is Nothing OrElse val.DataType = DataType.unknown Then _
         Return (mValue Is Nothing OrElse mValue.DataType = DataType.unknown)

        ' We know that val has an actual value so if we don't, it's not the same value.
        If mValue Is Nothing OrElse mValue.DataType = DataType.unknown Then Return False

        Return mValue.Equals(ev.mValue)
    End Function

    ''' <summary>
    ''' Create an instance of <see cref="clsArgument"/> from this environment variable
    ''' </summary>
    ''' <param name="argumentName">The name of argument</param>
    ''' <returns>A new <see cref="clsArgument"/> instance</returns>
    Friend Function CreateArgument(argumentName As String) As clsArgument
        Return New clsArgument(argumentName, Value)
    End Function

#End Region

End Class
