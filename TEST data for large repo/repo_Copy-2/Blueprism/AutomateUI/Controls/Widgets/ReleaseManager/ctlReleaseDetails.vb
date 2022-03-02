Imports BluePrism.AutomateAppCore

''' <summary>
''' Control for viewing the details of a selected release
''' </summary>
Public Class ctlReleaseDetails : Inherits ctlOwnerComponent

    ''' <summary>
    ''' The release which is being viewed in this control
    ''' </summary>
    Public Property Release() As clsRelease
        Get
            Return DirectCast(Me.Component, clsRelease)
        End Get
        Set(ByVal value As clsRelease)
            Me.Component = value
            ' Set the user action label to the appropriate value
            If value Is Nothing Then UserActionLabel = My.Resources.ctlReleaseDetails_Hyphen : Return
            If value.Local Then UserActionLabel = My.Resources.ctlReleaseDetails_Created Else UserActionLabel = My.Resources.Imported
        End Set
    End Property

    ''' <summary>
    ''' Gets the package that the currently viewing release was created from, or
    ''' null if there is no current release.
    ''' </summary>
    Public ReadOnly Property Package() As clsPackage
        Get
            Dim rel As clsRelease = Me.Release
            If rel Is Nothing Then Return Nothing
            Return rel.Package
        End Get
    End Property

End Class
