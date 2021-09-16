Imports System.Runtime.Serialization

Namespace BluePrism.ApplicationManager.AMI
    ''' Project  : AMI
    ''' Class    : clsArgumentInfo
    ''' 
    ''' <summary>
    ''' This class is used to return information to the client about a particular
    ''' Argument to an Action.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsArgumentInfo

        <DataMember>
        Private mId As String
        <DataMember>
        Private mName As String
        <DataMember>
        Private mDescription As String
        <DataMember>
        Private mDataType As String
        <DataMember>
        Private mIsOptional As Boolean

        ''' <summary>
        ''' Identifier for this argument eg "targx". This is the name used internally
        ''' and corresponds to that used in an actual query.
        ''' </summary>
        Public ReadOnly Property ID() As String
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The name of this argument. (This is the name as presented to the Automate
        ''' user)
        ''' </summary>
        Public ReadOnly Property Name() As String
            Get
                Return mName
            End Get
        End Property

        ''' <summary>
        ''' A description of the argument, and its valid
        ''' values.
        ''' </summary>
        Public ReadOnly Property Description() As String
            Get
                Return mDescription
            End Get
        End Property

        ''' <summary>
        ''' The data type of the argument, as corresponds to the data types
        ''' written in xml documents (eg "text", "number").
        ''' </summary>
        Public ReadOnly Property DataType() As String
            Get
                Return mDataType
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether the parameter is optional. An optional
        ''' parameter may receive an empty value (such as an empty string)
        ''' but never a null value.
        ''' </summary>
        Public ReadOnly Property IsOptional() As Boolean
            Get
                Return mIsOptional
            End Get
        End Property

        ''' <summary>
        ''' Constructor, to be used only within AMI itself. Clients of AMI receive
        ''' instances of this class only by calling the relevant clsAMI methods.
        ''' </summary>
        ''' <param name="id">The ID for the required argument</param>
        ''' <param name="name">The (display) name of the argument</param>
        ''' <param name="datatype">The datatype of the argument</param>
        ''' <param name="desc">The description of the argument</param>
        ''' <param name="isOptional">True to indicate that the argument is optional,
        ''' false to indicate that it is mandatory</param>
        Friend Sub New(ByVal id As String, ByVal name As String, ByVal datatype As String, ByVal desc As String, ByVal isOptional As Boolean)
            mId = id
            mName = name
            mDataType = datatype
            mDescription = desc
            mIsOptional = isOptional
        End Sub

        ''' <summary>
        ''' Creates a new <em>mandatory</em> argument info object with the given values.
        ''' </summary>
        ''' <param name="id">The ID for the required argument</param>
        ''' <param name="name">The (display) name of the argument</param>
        ''' <param name="datatype">The datatype of the argument</param>
        ''' <param name="desc">The description of the argument</param>
        Friend Sub New(ByVal id As String, ByVal name As String, ByVal datatype As String, ByVal desc As String)
            Me.New(id, name, datatype, desc, False)
        End Sub

        ''' <summary>
        ''' Clones this argument object.
        ''' </summary>
        ''' <returns>A new argument info object with the same value as this one.</returns>
        Public Function Clone() As clsArgumentInfo
            Return New clsArgumentInfo(mId, mName, mDataType, mDescription, mIsOptional)
        End Function

    End Class
End Namespace
