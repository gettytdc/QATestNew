Imports System.Text
Imports System.Xml
Imports BluePrism.BPCoreLib
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsApplicationAttribute
''' 
''' <summary>
''' Represents an attribute of an application element - i.e. a defining feature.
''' Examples include window class, location, size etc.
''' 
''' The values contained in this class are not understood by the APC but simply
''' passed on as strings to the Application Manager where they are understood.
''' </summary>
<DebuggerDisplay("{ToString()}")> _
Public Class clsApplicationAttribute : Implements ICloneable

#Region " Member Variables "

    ' The name of the attribute
    Private mName As String

    ' The value of the attribute
    Private mValue As clsProcessValue

    ' Flag indicating if this attribute is in use
    Private mInUse As Boolean

    ' Flag indicating if this attribute is a system attribute
    Private mSystem As Boolean

    ' Flag indicating if this attribute is dynamic
    Private mDynamic As Boolean

    ' The comparison type configured for this attribute
    Private mComparisonType As clsAMI.ComparisonTypes

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="name">The name of this attribute.</param>
    ''' <param name="value">The value of this attribute.</param>
    ''' <param name="inUse">Whether this attribute is InUse.</param>
    ''' <param name="systemAttr">True to indicate that this is a 'system' attribute
    ''' (ie. it is usually hidden from the user) or not.</param>
    Public Sub New(ByVal name As String, ByVal value As clsProcessValue, _
     ByVal inUse As Boolean, ByVal systemAttr As Boolean)
        mName = name
        mValue = value
        mInUse = inUse
        mSystem = systemAttr
        mDynamic = False
        mComparisonType = clsAMI.ComparisonTypes.Equal
    End Sub


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="name">The name of this attribute.</param>
    ''' <param name="value">The value of this attribute.</param>
    ''' <param name="inUse">Whether this attribute is InUse.</param>
    Public Sub New( _
     ByVal name As String, ByVal value As clsProcessValue, ByVal inUse As Boolean)
        Me.New(name, value, inUse, False)
    End Sub

    ''' <summary>
    ''' Private constructor, useful in cloning methods, where we don't have all
    ''' information to hand.
    ''' </summary>
    Private Sub New()
        Me.New("", Nothing, False)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The name of this attribute. Eg LocationX, Text, ParentSizeY etc
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
    ''' The value of this attribute, e.g. 25 for LocationX
    ''' </summary>
    Public Property Value() As clsProcessValue
        Get
            Return mValue
        End Get
        Set(ByVal value As clsProcessValue)
            mValue = value
        End Set
    End Property


    ''' <summary>
    ''' Determines if this attribute is in use by its owning Application Element.
    ''' </summary>
    Public Property InUse() As Boolean
        Get
            Return mInUse
        End Get
        Set(ByVal value As Boolean)
            mInUse = value
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating if this attribute is a 'system' attribute. A system
    ''' attribute is always sent as part of a query.
    ''' </summary>
    Public Property IsSystem() As Boolean
        Get
            Return mSystem
        End Get
        Set(ByVal value As Boolean)
            mSystem = value
        End Set
    End Property

    ''' <summary>
    ''' String describing the active state of this attribute. Here mainly for the
    ''' DebuggerDisplayAttribute
    ''' </summary>
    Private ReadOnly Property ActiveString() As String
        Get
            If InUse Then Return "[ON]"
            Return "[OFF]"
        End Get
    End Property

    ''' <summary>
    ''' Determines if this attribute is dynamic in the sense that its value is 
    ''' populated at runtime as a parameter within the business object.
    ''' 
    ''' When set to True, the data type must also be populated, specifying
    ''' which data type is expected as an argument. This data type is entirely
    ''' the user's choice.
    ''' </summary>
    Public Property Dynamic() As Boolean
        Get
            Return mDynamic
        End Get
        Set(ByVal value As Boolean)
            mDynamic = value
        End Set
    End Property

    ''' <summary>
    ''' Determines the type of comparison to be used when matching the attribute's
    ''' value.
    ''' </summary>
    Public Property ComparisonType() As clsAMI.ComparisonTypes
        Get
            Return mComparisonType
        End Get
        Set(ByVal value As clsAMI.ComparisonTypes)
            mComparisonType = value
        End Set
    End Property

    ''' <summary>
    ''' A string indicating the comparison type for this attribute. If it is dynamic,
    ''' that overrides any other comparison type - otherwise a string representation
    ''' of the comparison type set in this attribute is returned.
    ''' </summary>
    Private ReadOnly Property ComparisonTypeString() As String
        Get
            If mDynamic Then Return "dynamic"
            Return mComparisonType.ToString()
        End Get
    End Property

#End Region

#Region " Xml Handling "

    ''' <summary>
    ''' Serialises this object as an xml element, ready to be written out as a
    ''' string.
    ''' </summary>
    ''' <param name="doc">The parent document to use when creating new elements.
    ''' </param>
    ''' <returns>Returns an xml element with root "attribute".</returns>
    Public Function ToXML(ByVal doc As XmlDocument) As XmlElement
        Dim e As XmlElement = doc.CreateElement("attribute")

        e.SetAttribute("name", mName)
        If mDynamic OrElse mComparisonType <> clsAMI.ComparisonTypes.Equal Then _
         e.SetAttribute("comparisontype", ComparisonTypeString)
        If mInUse Then e.SetAttribute("inuse", "True")
        If mSystem Then e.SetAttribute("system", "True")

        'add child node for Value property
        e.AppendChild(mValue.ToXML(doc))

        Return e
    End Function


    ''' <summary>
    ''' Interprets an XmlElement with root node as "Attribute" and returns the
    ''' clsApplicationAttribute object that it represents.
    ''' </summary>
    ''' <param name="e">The xml element to interpret, must have name
    ''' "Attribute".</param>
    ''' <returns>Returns the clsApplicationAttribute represented by the supplied xml
    ''' element, or nothing if a bad argument is passed.</returns>
    Public Shared Function FromXML(ByVal e As XmlElement) As clsApplicationAttribute
        If Not e Is Nothing Then
            If e.Name = "attribute" Then
                Dim att As New clsApplicationAttribute()
                For Each a As XmlAttribute In e.Attributes
                    Select Case a.Name
                        Case "name" : att.Name = a.Value
                        Case "inuse" : att.InUse = Boolean.Parse(a.Value)
                        Case "system" : att.mSystem = Boolean.Parse(a.Value)
                            ' left for backwards compatability with old xml
                        Case "dynamic" : att.mDynamic = Boolean.Parse(a.Value)
                            ' The new way of handling comparisons
                        Case "comparisontype"
                            If a.InnerText = "dynamic" Then
                                att.Dynamic = True
                            Else
                                att.Dynamic = False
                                clsEnum.TryParse(a.InnerText, att.mComparisonType)
                            End If
                    End Select
                Next

                ' Populate Value Property
                Dim valueEl As XmlElement = TryCast(e.ChildNodes(0), XmlElement)
                If valueEl IsNot Nothing AndAlso valueEl.Name = "ProcessValue" Then _
                 att.Value = clsProcessValue.FromXML(valueEl)

                Return att

            End If
        End If

        Return Nothing
    End Function

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Returns a full copy of this attribute.
    ''' </summary>
    ''' <returns>See summary.</returns>
    Public Function Copy() As clsApplicationAttribute
        Dim attr As clsApplicationAttribute = _
         DirectCast(MemberwiseClone(), clsApplicationAttribute)
        attr.Value = mValue.Clone()
        Return attr
    End Function

    ''' <summary>
    ''' Clones this attribute
    ''' </summary>
    ''' <returns>A deep clone of this attribute</returns>
    Private Function Clone() As Object Implements ICloneable.Clone
        Return Copy()
    End Function

    ''' <summary>
    ''' Converts the current attribute to an identifier info object.
    ''' </summary>
    ''' <param name="customName">An optional custom name to use for the identifier 
    ''' info - the attribute's name will be used if not specified</param>
    ''' <returns>Returns the nearest-equivalent identifier info object.</returns>
    Public Function ToIdentifierInfo(Optional customName As String = Nothing) As clsIdentifierInfo
        Dim info As clsIdentifierInfo = _
         clsAMI.GetIdentifierInfo(mName, mValue.EncodedValue, InUse, customName)
        If Not mDynamic Then info.ComparisonType = mComparisonType
        Return info
    End Function


    ''' <summary>
    ''' Gets a string representation of this application attribute.
    ''' </summary>
    ''' <returns>A string representation of this attribute</returns>
    Public Overrides Function ToString() As String
        Dim sb As New StringBuilder()

        If mSystem Then sb.AppendFormat("[{0}]", mName) Else sb.Append(mName)

        If mDynamic Then
            sb.Append(" <Dynamic>")
        Else
            Select Case mComparisonType
                Case clsAMI.ComparisonTypes.Equal : sb.Append(" = ")
                Case clsAMI.ComparisonTypes.GreaterThan : sb.Append(" > ")
                Case clsAMI.ComparisonTypes.GreaterThanOrEqual : sb.Append(" >= ")
                Case clsAMI.ComparisonTypes.LessThan : sb.Append(" < ")
                Case clsAMI.ComparisonTypes.LessThanOrEqual : sb.Append(" <= ")
                Case clsAMI.ComparisonTypes.NotEqual : sb.Append(" <> ")
                Case clsAMI.ComparisonTypes.RegEx : sb.Append(" ~= ")
                Case clsAMI.ComparisonTypes.Wildcard : sb.Append(" *= ")
                Case Else : sb.Append(" <").Append(mComparisonType).Append("> ")
            End Select
            sb.Append(mValue.EncodedValue)
        End If
        sb.Append(" ").Append(ActiveString)
        Return sb.ToString()
    End Function

#End Region

End Class
