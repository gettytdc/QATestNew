Imports System.Xml
Imports System.Linq

Imports BluePrism.AMI

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsApplicationElement
''' 
''' <summary>
''' Represents a feature of the target application to be automated. This may be
''' a button, a menu item etc or even the application itself!
''' </summary>
<DebuggerDisplay("Element: {FullPath}", Name:="{mName}")> _
Public Class clsApplicationElement : Inherits clsApplicationMember

#Region " Member Variables "

    ' The description of this element
    Private mDescription As String

    ' Narrative (notes) for this element
    Private mNarrative As String

    ' The list of attributes set in this element
    Private mAttributes As New List(Of clsApplicationAttribute)

    ' Flag indicating diagnostic action should be taken for this element on a mismatch
    Private mDiagnose As Boolean

    ' The base (ie. system detected) element type that this object represents.
    Private mBaseType As clsElementTypeInfo

    ' The user specified element type that this object represents
    Private mType As clsElementTypeInfo

    ' The data type that this element naturally deals in
    Private mDataType As DataType

    ' The ID of the element which hosts this element - typically set on a region
    ' which defines an area on another control - the other control is represented
    ' by the 'host' element.
    Private mHostId As Guid

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty application element with no name
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new application element with the given name.
    ''' </summary>
    ''' <param name="name">The name of the new element</param>
    Public Sub New(ByVal name As String)
        MyBase.New(name)
        mDataType = DataType.unknown
        mDescription = ""
        mNarrative = ""
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The datatype of this application element; may be none.
    ''' </summary>
    ''' <value></value>
    Public Property DataType() As DataType
        Get
            Return mDataType
        End Get
        Set(ByVal value As DataType)
            mDataType = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the default data type for this application element, if known. Otherwise,
    ''' returns <see cref="AutomateProcessCore.DataType.unknown"/>
    ''' </summary>
    Public ReadOnly Property DefaultDataType As DataType
        Get
            Dim dt As DataType
            If mType IsNot Nothing AndAlso
             clsProcessDataTypes.TryParse(mType.DefaultDataType, dt) Then
                Return dt
            End If
            Return DataType.unknown
        End Get
    End Property

    ''' <summary>
    ''' The type of this application element - eg combobox, textbox, menuitem etc.
    ''' This is the type as selected by the user, which can differ from the BaseType
    ''' it was originally spied as.
    ''' </summary>
    ''' <value></value>
    Public Property Type() As clsElementTypeInfo
        Get
            Return mType
        End Get
        Set(ByVal value As clsElementTypeInfo)
            mType = value
        End Set
    End Property

    ''' <summary>
    ''' The type of this application element - eg combobox, textbox, menuitem etc.
    ''' This is the type it was originally spied as, and determines what other types
    ''' may be selectable by the user.
    ''' </summary>
    Public Property BaseType() As clsElementTypeInfo
        Get
            Return mBaseType
        End Get
        Set(ByVal value As clsElementTypeInfo)
            mBaseType = value
        End Set
    End Property

    ''' <summary>
    ''' The element type name, used only for the debugger display property to
    ''' ensure no null reference exceptions when debugging
    ''' </summary>
    Private ReadOnly Property ElementTypeName() As String
        Get
            If mType IsNot Nothing Then Return mType.Name
            Return "<Unknown>"
        End Get
    End Property

    ''' <summary>
    ''' Value indicating whether to take diagnostic actions on this element
    ''' if a mismatch occurrs.
    ''' </summary>
    Public Property Diagnose() As Boolean
        Get
            Return mDiagnose
        End Get
        Set(ByVal value As Boolean)
            mDiagnose = value
        End Set
    End Property


    ''' <summary>
    ''' The attributes of this ApplicationElement.
    ''' </summary>
    Public ReadOnly Property Attributes() As ICollection(Of clsApplicationAttribute)
        Get
            Return mAttributes
        End Get
    End Property

    ''' <summary>
    ''' The active (ie. InUse) attributes of this ApplicationElement.
    ''' </summary>
    ''' <remarks>The list returned by this property is read-only. It cannot be used
    ''' to maintain the active attributes - it is simply a filtered view of the
    ''' <see cref="Attributes"/> property.</remarks>
    Public ReadOnly Property ActiveAttributes() As IList(Of clsApplicationAttribute)
        Get
            Dim list As New List(Of clsApplicationAttribute)
            For Each attr As clsApplicationAttribute In mAttributes
                If attr.InUse Then list.Add(attr)
            Next
            Return GetReadOnly.IList(list)
        End Get
    End Property

    ''' <summary>
    ''' Gets the active attributes in this element as their equivalent identifiers.
    ''' </summary>
    Public ReadOnly Property ActiveIdentifiers() As List(Of clsIdentifierInfo)
        Get
            Dim ids As New List(Of clsIdentifierInfo)
            For Each attr As clsApplicationAttribute In ActiveAttributes
                ids.Add(attr.ToIdentifierInfo())
            Next
            Return ids
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the attributes in this application element from a collection
    ''' of Identifiers. Note that this property is dynamically generated - ie. any
    ''' modifications made to the collection after getting or setting will have no
    ''' effect on this application element.
    ''' Also note that identifiers have no concept of being 'dynamic' so any
    ''' conversion between an attribute and an identifier will lose that information.
    ''' The other comparison types are retained, however.
    ''' </summary>
    ''' <remarks>Identifiers which support multiple values as created as system
    ''' attributes, indicating that they cannot be modified in any way.</remarks>
    Public Property AllIdentifiers() As ICollection(Of clsIdentifierInfo)
        Get
            Dim ids As New List(Of clsIdentifierInfo)
            For Each attr As clsApplicationAttribute In mAttributes
                ids.Add(attr.ToIdentifierInfo())
            Next
            Return ids
        End Get
        Set(ByVal value As ICollection(Of clsIdentifierInfo))
            mAttributes.Clear()
            For Each i As clsIdentifierInfo In value
                For Each val As String In i.Values
                    Dim a As clsApplicationAttribute = New clsApplicationAttribute( _
                     i.ID, clsProcessValue.Decode(i.DataType, val), _
                     i.EnableByDefault, i.SupportsMultiple)
                    a.ComparisonType = i.ComparisonType
                    mAttributes.Add(a)
                Next
            Next
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether any of the attributes in the Attributes
    ''' property are dynamic.
    ''' </summary>
    Public ReadOnly Property HasDynamicAttributes() As Boolean
        Get
            For Each a As clsApplicationAttribute In Me.Attributes
                If a.Dynamic Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Provides access to the description of the element.
    ''' </summary>
    Public Property Description() As String
        Get
            Return mDescription
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then
                mDescription = ""
            Else
                mDescription = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the narrative of the element which can be used for notes.
    ''' </summary>
    Public Property Narrative() As String
        Get
            Return mNarrative
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then
                mNarrative = ""
            Else
                mNarrative = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the XML element name for application members of this type.
    ''' </summary>
    Friend Overrides ReadOnly Property XmlName() As String
        Get
            Return "element"
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Takes the details from the given app member into this element. If the
    ''' supplied value is not an element, only the member details will be copied,
    ''' otherwise all the detail on the element is copied into this element.
    ''' </summary>
    ''' <param name="target">The target application member from which the details
    ''' are to be taken.</param>
    Public Overrides Sub ReplaceMember(ByVal target As clsApplicationMember)
        MyBase.ReplaceMember(target)

        Dim el As clsApplicationElement = TryCast(target, clsApplicationElement)
        ' If the target is not an element, then we can glean nothing further
        ' from it. Just return now.
        If el Is Nothing Then Return

        mDescription = el.mDescription
        mNarrative = el.mNarrative
        mAttributes.Clear()
        For Each attr As clsApplicationAttribute In el.mAttributes
            mAttributes.Add(attr.Copy())
        Next
        mDiagnose = el.mDiagnose
        mBaseType = el.mBaseType
        mType = el.mType
        mDataType = el.mDataType

    End Sub

    ''' <summary>
    ''' Gets the attribute within this element with the specified name.
    ''' </summary>
    ''' <param name="name">The name of the attribute required.</param>
    ''' <returns>This element's attribute with the specified name, or null if it
    ''' had no such attribute.</returns>
    Public Function GetAttribute(ByVal name As String) As clsApplicationAttribute
        For Each attr As clsApplicationAttribute In mAttributes
            If attr.Name = name Then Return attr
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the process value associated with the attribute within this element
    ''' which has the specified name.
    ''' </summary>
    ''' <param name="name">The name of the attribute for which the value is required.
    ''' </param>
    ''' <returns>The process value corresponding to the given name, or null if there
    ''' was no such attribute within this element.</returns>
    Public Function GetValue(ByVal name As String) As clsProcessValue
        Dim attr As clsApplicationAttribute = GetAttribute(name)
        If attr Is Nothing Then Return Nothing Else Return attr.Value
    End Function

    ''' <summary>
    ''' Loads the data from the given XML element into this element object
    ''' </summary>
    ''' <param name="el">The XML element from which to load the data</param>
    Public Overrides Sub FromXml(ByVal el As XmlElement)

        MyBase.FromXml(el)

        'load xml attributes
        Name = el.GetAttribute("name")

        'load xml child elements
        ' clsApplicationMember handles its own data (ID, Name, Children), but
        ' we must handle everything specific to elements.
        For Each xe As XmlElement In el.ChildNodes
            Select Case xe.Name
                Case "type"
                    mType = clsAMI.GetElementTypeInfo(xe.InnerText)
                    If mType Is Nothing Then Throw New BluePrismException(
                     My.Resources.Resources.clsApplicationElement_AMIDidNotRecogniseTheElementType0CarriedByTheElement1,
                     xe.InnerText, Name)

                Case "basetype"
                    mBaseType = clsAMI.GetElementTypeInfo(xe.InnerText)

                Case "attributes"
                    For Each attEl As XmlElement In xe.ChildNodes
                        mAttributes.Add(clsApplicationAttribute.FromXML(attEl))
                    Next

                Case "datatype"
                    mDataType = clsProcessDataTypes.Parse(xe.InnerText)

                Case "diagnose"
                    mDiagnose = Boolean.Parse(xe.InnerText)

                Case "description"
                    mDescription = xe.InnerText

                Case "narrative"
                    mNarrative = xe.InnerText

            End Select
        Next

        ' If we didn't load a base type, it must be a definition from before
        ' these were introduced, but we can simply copy the selected type...
        If mBaseType Is Nothing Then mBaseType = mType

    End Sub


    ''' <summary>
    ''' Generates an xml element representing the current instance.
    ''' </summary>
    ''' <param name="doc">The parent document to use when generating new elements.
    ''' </param>
    ''' <returns>An XML element representing the current instance.</returns>
    Public Overrides Function ToXML(ByVal doc As XmlDocument) As XmlElement
        Dim e As XmlElement = MyBase.ToXml(doc)

        ' Add all the child elements which aren't handled by ApplicationMember
        ' (ie. everything except ID, Name and Children)
        If mType IsNot Nothing Then _
         BPUtil.AppendTextElement(e, "type", mType.ID)

        If mBaseType IsNot Nothing Then _
         BPUtil.AppendTextElement(e, "basetype", mBaseType.ID)

        BPUtil.AppendTextElement(e, "datatype", mDataType.ToString())
        BPUtil.AppendTextElement(e, "diagnose", mDiagnose.ToString())

        If mDescription <> "" Then _
         BPUtil.AppendTextElement(e, "description", mDescription)
        If mNarrative <> "" Then _
         BPUtil.AppendTextElement(e, "narrative", mNarrative)

        If mAttributes.Count > 0 Then
            Dim attrs As XmlElement = doc.CreateElement("attributes")
            For Each attr As clsApplicationAttribute In mAttributes
                attrs.AppendChild(attr.ToXML(doc))
            Next
            e.AppendChild(attrs)
        End If

        Return e
    End Function

    ''' <summary>
    ''' Clones this application element.
    ''' </summary>
    ''' <returns>A clone of this application element.</returns>
    Protected Overrides Function InnerClone() _
     As clsApplicationMember
        Dim e As clsApplicationElement = _
         DirectCast(MyBase.InnerClone(), clsApplicationElement)

        e.mAttributes = New List(Of clsApplicationAttribute)
        CollectionUtil.CloneInto(mAttributes, e.mAttributes)

        Return e
    End Function

    ''' <summary>
    ''' Checks if this application member matches the given string in a search /
    ''' filter operation. For an application element, this means that the name,
    ''' description and <em>active</em> attribute values are checked for a match.
    ''' </summary>
    ''' <param name="text">The text to check for in this member.</param>
    ''' <param name="partialMatch">True to indicate a partial match should count
    ''' as a match. False indicates that the whole text should match.</param>
    ''' <param name="caseSensitive">True to indicate that the search should be
    ''' case sensitive, False to indicate that the case should not be taken into
    ''' account when checking for a match.</param>
    ''' <returns>True if the given string matches this member; False otherwise.
    ''' </returns>
    Public Overrides Function Matches(ByVal text As String, _
     ByVal partialMatch As Boolean, ByVal caseSensitive As Boolean) As Boolean

        ' If there's a match on the member, might as well return true here and now
        If MyBase.Matches(text, partialMatch, caseSensitive) Then Return True

        ' Otherwise, quick check of the description, if there is one.
        If mDescription <> "" _
         AndAlso BPUtil.IsMatch(mDescription, text, partialMatch, caseSensitive) Then
            Return True
        End If

        ' Not a match on name, not a match on description. Check the active
        ' attribute *values*
        For Each attr As clsApplicationAttribute In ActiveAttributes
            If attr.Value.Matches(text, partialMatch, caseSensitive) Then Return True
        Next

        ' We've nothing left to check - not a match
        Return False

    End Function

    ''' <summary>
    ''' Gets the resultant datatype for this element when the override configured
    ''' in the element is compared to the type specified by AMI.
    ''' </summary>
    ''' <param name="amiDataType">The datatype returned by AMI</param>
    ''' <returns>The datatype which should be used for this element. If AMI specifies
    ''' a text datatype, it can be overridden by this element, otherwise, the AMI
    ''' type is deferred to, regardless of any settings in this element.</returns>
    Public Function GetResultantDataType(ByVal amiDataType As DataType) As DataType
        ' Note - all this functionality was taken from clsReadStage.DecideReadActionReturnType
        ' Some of the design decisions are a bit odd, but the functionality should be identical

        ' If the ami datatype is text, it may be overridden by the
        ' datatype set on the target element (if one is set)
        If amiDataType = DataType.text AndAlso mDataType <> DataType.unknown Then _
         Return mDataType

        ' Can't be overridden then - return AMI's data type if it is set
        If amiDataType <> DataType.unknown Then Return amiDataType

        ' ...and text if it is not
        Return DataType.text

    End Function

    ''' <summary>
    ''' Creates additional identifiers used to identify this application element that are included
    ''' with the payload sent to process the command. This can be overriden by subclasses of 
    ''' clsApplicationElement that require additional attributes to be sent for the type of element.
    ''' </summary>
    ''' <returns>A sequence of <see cref="clsIdentifierInfo" /> objects</returns>
    Public Overridable Function GetSupplementaryIdentifiers() _
        As IEnumerable(Of clsIdentifierInfo)
        Return Enumerable.Empty(Of clsIdentifierInfo)()
    End Function

#End Region

End Class
