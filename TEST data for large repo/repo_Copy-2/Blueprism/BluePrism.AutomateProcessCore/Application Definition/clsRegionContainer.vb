Imports System.Drawing

Imports BluePrism.AMI
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Class which describes an application element which may contain some regions.
''' </summary>
''' <remarks>This class doesn't need to override ToXml or FromXml - the regions
''' themselves maintain a link to their container, and the relationships are
''' resolved in the application definition once it has fully loaded (before then,
''' there is no guarantee that an element exists / has been loaded)</remarks>
<DebuggerDisplay("RegionContainer: {FullPath}", Name:="{mName}")> _
Public Class clsRegionContainer : Inherits clsApplicationElement

#Region " Member Vars "

    ' Event firing collection which contains the regions contained by this element
    Private WithEvents mRegions As clsEventFiringCollection(Of clsApplicationRegion)

    ''' <summary>
    ''' The name of the attribute which defines the screenshot held in a region container.
    ''' </summary>
    Public Const ScreenshotAttributeName As String = clsAMI.ScreenshotIdentifierId

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty application region container 
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new application region container with the given name.
    ''' </summary>
    ''' <param name="name">The name of the new element</param>
    Public Sub New(ByVal name As String)
        MyBase.New(name)
        mRegions = New clsEventFiringCollection(Of clsApplicationRegion)( _
         New clsOrderedSet(Of clsApplicationRegion))
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The regions contained by this element.
    ''' </summary>
    Public ReadOnly Property Regions() As ICollection(Of clsApplicationRegion)
        Get
            Return mRegions
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the screenshot image for this region container. This will return
    ''' null if there is no screenshot attribute registered on it. Setting to null
    ''' will have the effect of removing the screenshot attribute from this element.
    ''' </summary>
    Public Property ScreenshotImage() As Bitmap
        Get
            Return CType(GetValue(ScreenshotAttributeName), Bitmap)
        End Get
        Set(ByVal value As Bitmap)
            Dim attr As clsApplicationAttribute = GetAttribute(ScreenshotAttributeName)

            If attr IsNot Nothing Then ' if we've found a screenshot attribute...
                ' If we're setting to null, remove it from the attrs
                ' Otherwise, set the existing attribute value to the new bitmap
                If value Is Nothing _
                 Then Attributes.Remove(attr) _
                 Else attr.Value = New clsProcessValue(value)
            Else ' otherwise, if we don't currently have a screenshot attribute
                ' If we're setting to null, do nothing - it's already null, in effect
                If value IsNot Nothing Then _
                 Attributes.Add(New clsApplicationAttribute( _
                 ScreenshotAttributeName, New clsProcessValue(value), False))
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets whether this container has a screenshot.
    ''' </summary>
    Public ReadOnly Property HasScreenshot() As Boolean
        Get
            Dim pv As clsProcessValue = GetValue(ScreenshotAttributeName)
            Return (pv IsNot Nothing AndAlso Not pv.IsNull)
        End Get
    End Property

    ''' <summary>
    ''' Gets the XML element name for application members of this type.
    ''' </summary>
    Friend Overrides ReadOnly Property XmlName() As String
        Get
            Return "region-container"
        End Get
    End Property

#End Region

#Region " Regions Collection Event Handlers "

    ''' <summary>
    ''' Handles an item being added to the regions collection
    ''' </summary>
    Private Sub HandleItemAdded(ByVal sender As ICollection(Of clsApplicationRegion), _
     ByVal reg As clsApplicationRegion) Handles mRegions.ItemAdded
        reg.ContainerId = Me.ID
    End Sub

    ''' <summary>
    ''' Handles an item being removed from the regions collection
    ''' </summary>
    Private Sub HandleItemRemoved(ByVal sender As ICollection(Of clsApplicationRegion), _
     ByVal reg As clsApplicationRegion) Handles mRegions.ItemRemoved
        If reg.ContainerId = Me.ID Then reg.ContainerId = Nothing
    End Sub

    ''' <summary>
    ''' Handles the regions collection being cleared
    ''' </summary>
    Private Sub HandleRegionsCleared( _
     ByVal sender As ICollection(Of clsApplicationRegion), _
     ByVal itemsCleared As ICollection(Of clsApplicationRegion)) _
     Handles mRegions.CollectionCleared
        For Each reg As clsApplicationRegion In itemsCleared
            If reg.ContainerId = Me.ID Then reg.ContainerId = Nothing
        Next
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Gets a clone of this region container, with no regions defined in it.
    ''' </summary>
    ''' <returns>A region container with the same values as this object, but with
    ''' no regions registered within it</returns>
    Protected Overrides Function InnerClone() As clsApplicationMember
        Dim rc As clsRegionContainer = _
         DirectCast(MyBase.InnerClone(), clsRegionContainer)
        ' We can't actually clone the regions, per se - we need to reference those
        ' regions within the newly cloned model which we can't do yet since the
        ' model might not have cloned those regions yet. So for now we just clear
        ' the regions without firing any events and having this object accidentally
        ' disassociate the source regions from their region container
        rc.mRegions = New clsEventFiringCollection(Of clsApplicationRegion)( _
         New clsOrderedSet(Of clsApplicationRegion))

        Return rc

    End Function

#End Region

End Class
