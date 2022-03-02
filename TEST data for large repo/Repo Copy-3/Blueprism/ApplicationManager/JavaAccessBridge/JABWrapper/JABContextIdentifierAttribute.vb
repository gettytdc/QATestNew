
Imports IdentifierTypes = _
 BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.IdentifierTypes

''' <summary>
''' Class used in property attributes to match up properties of the JABContext class
''' against parameters in clsQuery.
''' </summary>
Public Class JABContextIdentifierAttribute : Inherits Attribute

    ''' <summary>
    ''' The type of identifier which has been tagged by this attribute
    ''' </summary>
    Public Identifier As IdentifierTypes

    ''' <summary>
    ''' Whether the identifier tagged by this attribute is enabled by default
    ''' </summary>
    Public DefaultEnabled As Boolean

    ''' <summary>
    ''' Creates a new identifier attribute
    ''' </summary>
    ''' <param name="identifier">The type of identifier represented by the property
    ''' </param>
    ''' <param name="defaultEnabled">True to enable the attribute by default in any
    ''' spy queries; False otherwise</param>
    Public Sub New( _
     ByVal identifier As IdentifierTypes, ByVal defaultEnabled As Boolean)
        Me.Identifier = identifier
        Me.DefaultEnabled = defaultEnabled
    End Sub

End Class

