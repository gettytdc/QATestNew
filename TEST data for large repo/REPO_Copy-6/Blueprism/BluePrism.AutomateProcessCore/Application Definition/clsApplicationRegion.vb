Imports System.Xml
Imports System.Drawing

Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.BPCoreLib

''' <summary>
''' Class which describes a region within an application
''' </summary>
<DebuggerDisplay("Region: {FullPath}", Name:="{mName}")>
Public Class clsApplicationRegion : Inherits clsApplicationElement

#Region " Member Vars "

    ' The host of this region - ie. the ID of the element that this region describes
    ' an area on.
    Private mContainerId As Guid

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty region element with no name or container
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new application element with the given name.
    ''' </summary>
    ''' <param name="name">The name of the new element</param>
    ''' <param name="contId">The ID of the container holding the screenshot on which
    ''' this region describes itself</param>
    Public Sub New(ByVal name As String, ByVal contId As Guid)
        MyBase.New(name)
        mContainerId = contId
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The ID of the region container which hosts this region.
    ''' </summary>
    ''' <remarks>The region does not permanently inherit any properties from its
    ''' container, although its original attribute values should be identical - once
    ''' a region is created it is independent, and the container is retained so that
    ''' the region can be edited in the context of it later.</remarks>
    Public Property ContainerId() As Guid
        Get
            Return mContainerId
        End Get
        Set(ByVal value As Guid)
            mContainerId = value
        End Set
    End Property

    ''' <summary>
    ''' The region container set as the container for this region. Null if the
    ''' container ID is not set, or no container with that ID could be found in
    ''' or below the root of the application model which holds this region.
    ''' </summary>
    Public ReadOnly Property Container() As clsRegionContainer
        Get
            ' Quick shortcut - try the parent to see if that is the container
            Dim cont As clsRegionContainer = TryCast(Parent, clsRegionContainer)

            ' If it's not the parent, then look through the model for a container
            ' with the ID set in this region
            If cont Is Nothing OrElse cont.ID <> mContainerId Then _
             cont = Root.FindMember(Of clsRegionContainer)(mContainerId)

            Return cont

        End Get
    End Property

    ''' <summary>
    ''' Checks if this region is hosted in a container or not. If it has been copied
    ''' or moved around the model, it may become disassociated with its container.
    ''' </summary>
    Public ReadOnly Property IsHosted() As Boolean
        Get
            Return (mContainerId <> Nothing)
        End Get
    End Property

    ''' <summary>
    ''' Gets a rectangle which describes this region
    ''' </summary>
    ''' <exception cref="FormatException">If any of the attributes :- <list>
    ''' <item>StartX</item>
    ''' <item>StartY</item>
    ''' <item>EndX</item>
    ''' <item>EndY</item>
    ''' </list> ... cannot be converted into integers</exception>
    Public ReadOnly Property Rectangle() As Rectangle
        Get
            Dim startX As Integer = CInt(GetValue("StartX"))
            Dim startY As Integer = CInt(GetValue("StartY"))
            Dim endX As Integer = CInt(GetValue("EndX"))
            Dim endY As Integer = CInt(GetValue("EndY"))
            Return New Rectangle(startX, startY, endX - startX, endY - startY)
        End Get
    End Property

    ''' <summary>
    ''' Gets the XML element name for application members of this type.
    ''' </summary>
    Friend Overrides ReadOnly Property XmlName() As String
        Get
            Return "region"
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Creates an XML representation of this region and returns it.
    ''' </summary>
    ''' <param name="doc">The parent document on which the element will reside
    ''' </param>
    ''' <returns>An XML element representing this object.</returns>
    Public Overrides Function ToXML(ByVal doc As XmlDocument) As XmlElement
        ' Populate the Xml element with all the application element guff
        Dim e As XmlElement = MyBase.ToXML(doc)
        ' Prepend the container element, if present
        If mContainerId <> Nothing Then
            Dim contEl As XmlElement = doc.CreateElement("container")
            contEl.InnerText = mContainerId.ToString()
            e.PrependChild(contEl)
        End If
        ' Return the result
        Return e
    End Function

    ''' <summary>
    ''' Loads this element from the given XML element.
    ''' </summary>
    ''' <param name="e">The XML element to load this region's data from</param>
    Public Overrides Sub FromXML(ByVal e As XmlElement)
        MyBase.FromXml(e)
        For Each n As XmlElement In e.GetElementsByTagName("container")
            mContainerId = BPUtil.IfNull(n.InnerText, Guid.Empty)
        Next
    End Sub

    ''' <summary>
    ''' Override to provide attributes used to find this region via relative parents
    ''' </summary>
    ''' <returns>A sequence of <see cref="clsIdentifierInfo" /> objects</returns>
    ''' <remarks>This adds the attributes needed to locate relative parents as
    ''' additional attributes prefixed by a zero-based index value, e.g.
    ''' RelativeParent_0_StartX, RelativeParent_0_StartY, ...,
    ''' RelativeParent_1_StartX, RelativeParent_1_StartY, which allows us to use the
    ''' existing identifier value encoding functionality rather than custom
    ''' delimiters and / or serialization.
    ''' </remarks>
    Public Overrides Function GetSupplementaryIdentifiers() As IEnumerable(Of clsIdentifierInfo)


        Dim attributeNames() As String = {
            "StartX", "StartY",
            "EndX", "EndY",
            "ElementSnapshot",
            "LocationMethod",
            "RegionPosition",
            "ImageSearchPadding",
            "ColourTolerance",
            "Greyscale"
        }

        Dim identifiers As New List(Of clsIdentifierInfo)
        Dim index = 0
        ' Get the above attributes for each of the relative parents in the hierarchy
        ' and encode them into the query, recording the level within the hierarchy
        ' of each parent found
        For Each region In GetRelativeParentHierarchy()
            For Each attributeName In attributeNames
                Dim attribute = region.GetAttribute(attributeName)
                If attribute Is Nothing Then Continue For

                Dim customName = String.Format(
                    "RelativeParent_{0}_{1}", index, attribute.Name)
                identifiers.Add(attribute.ToIdentifierInfo(customName))

            Next
            index += 1
        Next
        Return identifiers

    End Function

    ''' <summary>
    ''' Gets the hierarchy of regions relative to this region, discovered via its 
    ''' "RelativeParentID" attribute, starting with the immediate relative parent and 
    ''' working up through the hierarchy of relative ancestors.
    ''' </summary>
    ''' <returns>A sequence of regions</returns>
    Private Function GetRelativeParentHierarchy() As IEnumerable(Of clsApplicationRegion)

        Dim hierarchy As New List(Of clsApplicationRegion)
        Dim parent = GetRelativeParent()

        While parent IsNot Nothing
            hierarchy.Add(parent)
            parent = parent.GetRelativeParent()
        End While

        Return hierarchy
    End Function

    ''' <summary>
    ''' Gets the region that a region is related to via the "RelativeParentID" attribute
    ''' </summary>
    ''' <returns>An clsApplicationRegion which is parent of this region, or null if no 
    ''' relative parent is found</returns>
    Private Function GetRelativeParent() As clsApplicationRegion

        Dim attribute = GetAttribute("RelativeParentID")
        If attribute Is Nothing Then Return Nothing
        ' The Root application member allows us to find the parent region as it stands  
        ' in the current application definition for this (child) region (including 
        ' Attributes which have not yet been saved to the app model)
        Return Root.FindMember(Of clsApplicationRegion)(CType(attribute.Value, Guid))

    End Function

#End Region

End Class
