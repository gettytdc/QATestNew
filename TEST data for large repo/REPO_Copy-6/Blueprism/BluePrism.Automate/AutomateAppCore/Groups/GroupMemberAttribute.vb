Imports System.Reflection
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

Namespace Groups

    ''' <summary>
    ''' Attribute which defines the associated metadata of a group member value
    ''' </summary>
    Public Class GroupMemberAttribute : Inherits FriendlyNameAttribute

        ' An empty group member attribute to use as a placeholder
        Friend Shared ReadOnly Empty As New GroupMemberAttribute(Nothing)

        ' The type which represents this group member
        Private ReadOnly mType As Type

        ' The view with which data regarding this member can be loaded
        Private ReadOnly mView As String

        Private ReadOnly mGetTreeQueryHeaders As New List(Of String) From { "*" }
        ''' <summary>
        ''' Creates a new group member attribute with the given name.
        ''' </summary>
        ''' <param name="name">The name of a single instance of the group member type
        ''' </param>
        Public Sub New(name As String)
            Me.New(name, Nothing, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new group member attribute with the given properties.
        ''' </summary>
        ''' <param name="name">The name of a single instance of the group member type
        ''' </param>
        ''' <param name="memberType">The .NET type of the object which represents the group
        ''' member type associated with this attribute.</param>
        ''' <param name="viewName">The name of the database view associated with the
        ''' group type, from which data about the member type can be retrieved.
        ''' </param>
        ''' <exception cref="InvalidArgumentException">If the given
        ''' <paramref name="memberType"/> is not a subclass of <see cref="GroupMember"/>
        ''' </exception>
        Public Sub New(name As String, memberType As Type, viewName As String)
            MyBase.New(name)
            If _
                memberType IsNot Nothing AndAlso
                Not GetType(GroupMember).IsAssignableFrom(memberType)

                Throw New InvalidArgumentException(
                    "The member type '{0}' does not inherit from GroupMember", memberType)
            End If

            mType = memberType
            mView = viewName
        End Sub

        Public Sub New(name As String, memberType As Type, viewName As String, 
                       getTreeQueryHeaders As String())
            Me.New(name, memberType, viewName)
            mGetTreeQueryHeaders = mGetTreeQueryHeaders.ToList
        End Sub

        ''' <summary>
        ''' The class which represents this member type
        ''' </summary>
        Public ReadOnly Property MemberType As Type
            Get
                Return mType
            End Get
        End Property

        ''' <summary>
        ''' The name of the view on the database which provides grouping information
        ''' for this type of group member
        ''' </summary>
        Public ReadOnly Property ViewName As String
            Get
                Return If(mView, "")
            End Get
        End Property

        Public ReadOnly Property GetTreeSelect As String
        Get
                Return String.Join(", ", mGetTreeQueryHeaders)
        End Get
        End Property

        ''' <summary>
        ''' Creates an empty group member of the type defined by this attribute
        ''' </summary>
        ''' <returns>The group member, initialised with the specified ID and name.
        ''' </returns>
        ''' <exception cref="InvalidStateException">If no <see cref="MemberType"/> is
        ''' specified in this attribute.</exception>
        ''' <exception cref="ArgumentException">If type is not a RuntimeType. -or-
        ''' type is an open generic type (that is, the
        ''' System.Type.ContainsGenericParameters property returns true).</exception>
        ''' <exception cref="NotSupportedException">If the assembly that contains
        ''' type is a dynamic assembly that was created with
        ''' System.Reflection.Emit.AssemblyBuilderAccess.Save.</exception>
        ''' <exception cref="TargetInvocationException">If the constructor being
        ''' called throws an exception.</exception>
        ''' <exception cref="MethodAccessException">If the caller does not have
        ''' permission to call this constructor.</exception>
        ''' <exception cref="MemberAccessException">If an attempt was made to create
        ''' an instance of an abstract class, or this member was invoked with a
        ''' late-binding mechanism.</exception>
        ''' <exception cref="MissingMethodException">If no matching public
        ''' constructor was found.</exception>
        ''' <exception cref="TypeLoadException">If type is not a valid type.
        ''' </exception>
        Public Function CreateMember() As GroupMember
            If mType Is Nothing Then Throw New InvalidStateException(
                "No type held for this attribute. Cannot create member")
            Return DirectCast(Activator.CreateInstance(mType), GroupMember)
        End Function

        ''' <summary>
        ''' Creates an initialised group member of the type defined by this attribute
        ''' </summary>
        ''' <param name="prov">The data provider used to populate the new group
        ''' member instance. This would typically include "id" (Guid) and "name"
        ''' (String) properties, along with any other type-specific properties.
        ''' </param>
        ''' <returns>The group member, initialised with the specified ID and name.
        ''' </returns>
        ''' <exception cref="InvalidStateException">If no <see cref="MemberType"/> is
        ''' specified in this attribute.</exception>
        ''' <exception cref="ArgumentException">If type is not a RuntimeType. -or-
        ''' type is an open generic type (that is, the
        ''' System.Type.ContainsGenericParameters property returns true).</exception>
        ''' <exception cref="NotSupportedException">If the assembly that contains
        ''' type is a dynamic assembly that was created with
        ''' System.Reflection.Emit.AssemblyBuilderAccess.Save.</exception>
        ''' <exception cref="TargetInvocationException">If the constructor being
        ''' called throws an exception.</exception>
        ''' <exception cref="MethodAccessException">If the caller does not have
        ''' permission to call this constructor.</exception>
        ''' <exception cref="MemberAccessException">If an attempt was made to create
        ''' an instance of an abstract class, or this member was invoked with a
        ''' late-binding mechanism.</exception>
        ''' <exception cref="MissingMethodException">If no matching public
        ''' constructor was found.</exception>
        ''' <exception cref="TypeLoadException">If type is not a valid type.
        ''' </exception>
        Public Function CreateMember(prov As IDataProvider) As GroupMember
            If mType Is Nothing Then Throw New InvalidStateException(
                "No type held for this attribute. Cannot create member")

            Return DirectCast(Activator.CreateInstance(mType, prov), GroupMember)
        End Function

    End Class

End Namespace
