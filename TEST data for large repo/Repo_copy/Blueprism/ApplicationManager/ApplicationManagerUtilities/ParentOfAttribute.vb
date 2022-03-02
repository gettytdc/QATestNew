
Imports BluePrism.BPCoreLib
Imports IdentifierTypes =
 BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.IdentifierTypes

''' <summary>
''' Attribute applied to an identifier type which indicates that it is a parent
''' attribute, and giving the 'child' type that it is a parent equivalent of.
''' </summary>
Public Class ParentOfAttribute : Inherits Attribute

    ' The child equivalent of the attribute
    Private mChildType As IdentifierTypes

    ''' <summary>
    ''' Gets the child equivalent of the enum value tagged with this attribute.
    ''' </summary>
    Public ReadOnly Property ChildType As IdentifierTypes
        Get
            Return mChildType
        End Get
    End Property

    ''' <summary>
    ''' Creates a new parent of attribute, referring to the given child id.
    ''' </summary>
    ''' <param name="childIdType">The equivalent 'child' ID to the identifier tagged
    ''' with this attribute</param>
    Public Sub New(childIdType As IdentifierTypes)
        mChildType = childIdType
    End Sub

    ''' <summary>
    ''' Gets the child equivalent to a given identifier type. Note that there is no
    ''' 'None' identifier type, so if there is no registered child equivalent of the
    ''' given type, this will return the type provided.
    ''' </summary>
    ''' <param name="idType">The ID type for which the child equivalent is required.
    ''' </param>
    ''' <returns>The child type equivalent to the given type, if it is decorated with
    ''' a 'ParentOf' attribute providing such a value. This returns
    ''' <paramref name="idType"/> if it no child type is registered on the given ID
    ''' type.</returns>
    Public Shared Function GetChildType(idType As IdentifierTypes) As IdentifierTypes
        Dim attr = BPUtil.GetAttributeValue(Of ParentOfAttribute)(idType)
        Return If(attr IsNot Nothing, attr.ChildType, idType)
    End Function

End Class
