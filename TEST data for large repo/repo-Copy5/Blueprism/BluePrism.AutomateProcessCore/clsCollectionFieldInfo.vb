Imports System.Xml
Imports System.Runtime.Serialization

Imports BluePrism.Server.Domain.Models
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionFieldInfo
''' 
''' <summary>
''' Represents a field in a Collection. Not an instance of a field,
''' i.e. with a value, but just the definition of that field.
''' </summary>
<Serializable, DataContract([Namespace]:="bp", IsReference:=True)>
Public Class clsCollectionFieldInfo
    Implements ICloneable
    Implements IDataField

#Region " Member variables / constants "

    ''' <summary>
    ''' String used to indicate that this field has no defined parent (stage or
    ''' field definition) in a fully qualified name.
    ''' </summary>
    Private Const UnknownParent As String = "<?>"

    ''' <summary>
    ''' The collection definition that this field definition appears on
    ''' </summary>
    Private mParent As clsCollectionInfo

    ''' <summary>
    ''' The name of the field.
    ''' </summary>
    <DataMember>
    Private mName As String

    ''' <summary>
    ''' The description of the field
    ''' </summary>
    <DataMember>
    Private mDescription As String


    ''' <summary>
    ''' The display name
    ''' </summary>
    <DataMember>
    Private mDisplayName As String

    ''' <summary>
    ''' The data type of this instance. This is an internal data type
    ''' name, as defined in clsProcessDataTypes. e.g. "text".
    ''' </summary>
    <DataMember>
    Private mDataType As DataType

    ''' <summary>
    ''' The children of this field - only applicable if this field represents a
    ''' collection. Otherwise, this should be null.
    ''' </summary>
    <DataMember>
    Private mChildren As clsCollectionInfo

    ''' <summary>
    ''' The namespace of this field.
    ''' </summary>
    ''' <remarks></remarks>
    <DataMember>
    Private mNamespace As String
#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="sName">The name of this field</param>
    ''' <param name="dtDataType">The data type of this field</param>
    ''' <param name="ns">The namespace of the field</param>
    Public Sub New(ByVal sName As String, ByVal dtDataType As DataType, ns As String)
        Me.New(Nothing, sName, dtDataType, ns)
    End Sub

    ''' <summary>
    ''' Creates a new collection field definition using the data from the given
    ''' collection field definition and the specified parent.
    ''' </summary>
    ''' <param name="parent">The parent collection info that this field is owned by.
    ''' </param>
    ''' <param name="fld">The field from which to draw the data.</param>
    Friend Sub New(ByVal parent As clsCollectionInfo, ByVal fld As clsCollectionFieldInfo)
        Me.New(parent, fld.Name, fld.DataType, fld.Namespace)
        Me.Description = fld.Description
        Me.DisplayName = fld.DisplayName
        CopyChildren(fld.Children)
        Me.Children.NestingElement = fld.Children.NestingElement
    End Sub

    ''' <summary>
    ''' Creates a new collection field definition with the given attributes.
    ''' </summary>
    ''' <param name="parent">The owner of this field definition.</param>
    ''' <param name="name">The name of the field specified by this object.</param>
    ''' <param name="dataType">The datatype of the field specified by this object.
    ''' <param name="namespace">The namespace of the field</param>
    ''' </param>
    Friend Sub New(
     ByVal parent As clsCollectionInfo, ByVal name As String, ByVal dataType As DataType,
     [namespace] As String)
        mParent = parent
        mName = name
        mDataType = dataType
        mNamespace = [namespace]
    End Sub

    ''' <summary>
    ''' Creates a new collection field definition derived from the given XML element.
    ''' The new field has the given collection definition as its parent and its
    ''' data set from the values found in the given XML element. This element is
    ''' expected to be equivalent to that output from the <see cref="ToXml"/>
    ''' method.
    ''' </summary>
    ''' <param name="parent">The collection definition to which this field should
    ''' be set to belong.</param>
    ''' <param name="el">The element from which this field should draw its data.
    ''' </param>
    Friend Sub New(ByVal parent As clsCollectionInfo, ByVal el As XmlElement)
        FromXml(parent, el)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' A reference to the clsCollectionInfo this field belongs to. This is usually
    ''' set only when the field is added to a clsCollectionInfo.
    ''' </summary>
    ''' <remarks>
    ''' If <see cref="clsCollectionInfo"/> is directly serialized using DataContract 
    ''' serialization, Parent will not be deserialized as it does not have a data 
    ''' member attribute. Parent will only be deserialized if it was serialized 
    ''' through its parent <see cref="clsCollectionInfo"/>
    ''' </remarks>
    Public Property Parent() As clsCollectionInfo
        Get
            Return mParent
        End Get
        Friend Set(ByVal Value As clsCollectionInfo)
            mParent = Value
        End Set
    End Property

    ''' <summary>
    ''' The fully qualified name of this field which can be used to recognise it
    ''' within an expression. This is a combination of the parent of this field
    ''' (either another collection field or a collection stage) which this field is
    ''' part of and the name of this field, so for example :-
    ''' "Queue Items.Item ID" is a field "Item ID" in the "Queue Items" collection
    ''' stage. If this field does not have a collection owner, the string "&lt;?&gt;"
    ''' denotes an unknown parent.
    ''' </summary>
    Public ReadOnly Property FullyQualifiedName() As String _
     Implements IDataField.FullyQualifiedName
        Get
            Dim prefix As String = UnknownParent
            If mParent IsNot Nothing Then
                If mParent.ParentField IsNot Nothing Then
                    prefix = mParent.ParentField.FullyQualifiedName

                ElseIf mParent.ParentStage IsNot Nothing Then
                    prefix = mParent.ParentStage.GetName()

                End If
            End If
            Return prefix & "."c & mName
        End Get
    End Property

    ''' <summary>
    ''' The fully qualified name, less the name of the root stage which holds it.
    ''' This simply removes the first element in the name (which should always exist
    ''' even if this field is not ultimately attached to a stage).
    ''' The partially qualified name can be used to look up instances of this field
    ''' in the <see cref="StageCollection"/> property.
    ''' </summary>
    Public ReadOnly Property PartiallyQualifiedName() As String
        Get
            Dim fullName As String = FullyQualifiedName
            Return fullName.Substring(1 + fullName.IndexOf("."c))
        End Get
    End Property

    ''' <summary>
    ''' The data type of this field definition.
    ''' </summary>
    Public Property DataType() As DataType Implements IDataField.DataType
        Get
            Return mDataType
        End Get
        Set(ByVal Value As DataType)
            mDataType = Value
        End Set
    End Property

    ''' <summary>
    ''' The collection of the stage which ultimately holds this collection field.
    ''' This may be null if this field is not attached to a definition, or if the
    ''' definition is not attached to a stage, or if that stage doesn't have a
    ''' collection value.
    ''' </summary>
    Private ReadOnly Property StageCollection() As clsCollection
        Get
            If Parent Is Nothing Then Return Nothing

            Dim stg As clsCollectionStage = Parent.RootStage
            If stg Is Nothing Then Return Nothing

            Return stg.Value.Collection
        End Get
    End Property

    ''' <summary>
    ''' The current value of this field in the stage that it is currently hosted
    ''' within, or null if it has no value, or it is not hosted within a collection
    ''' stage.
    ''' </summary>
    Public Property Value() As clsProcessValue Implements IDataField.Value
        Get
            Dim coll As clsCollection = StageCollection
            If coll Is Nothing Then Return Nothing
            Try
                Return coll.GetField(PartiallyQualifiedName)
            Catch
                Return Nothing
            End Try
        End Get
        Set(ByVal value As clsProcessValue)
            Dim coll As clsCollection = StageCollection
            If coll Is Nothing Then _
             Throw New NoSuchElementException(My.Resources.Resources.NoCollectionFoundForThisField)
            coll.SetField(PartiallyQualifiedName, value)
        End Set
    End Property

    ''' <summary>
    ''' The name of this field definition.
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal Value As String)
            mName = Value
        End Set
    End Property

    ''' <summary>
    ''' The namespace of this field definition
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property [Namespace] As String
        Get
            Return mNamespace
        End Get
    End Property


    ''' <summary>
    ''' The description of this field. This will never be null.
    ''' </summary>
    ''' <remarks>Placeholder for annotated collection fields - not wired in as yet.
    ''' See bug 4675.</remarks>
    Public Property Description() As String
        Get
            If mDescription Is Nothing Then Return ""
            Return mDescription
        End Get
        Set(ByVal value As String)
            mDescription = value
        End Set
    End Property


    Public Property DisplayName As String
        Get
            If mDisplayName Is Nothing Then Return ""
            Return mDisplayName
        End Get
        Set(ByVal value As String)
            mDisplayName = value
        End Set
    End Property

    ''' <summary>
    ''' <para>
    ''' The collection definition representing this field - this is only valid if
    ''' this field represents a <see cref="AutomateProcessCore.DataType.collection" /> field.
    ''' </para><para>
    ''' This is never null - empty definitions indicate that this field definition
    ''' has no children, though the <see cref="HasChildren"/> method should be used
    ''' to determine that in case the internal representation changes.
    ''' </para>
    ''' </summary>
    Public ReadOnly Property Children() As clsCollectionInfo
        Get
            If mChildren Is Nothing Then mChildren = New clsCollectionInfo(Me)
            Return mChildren
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this field definition represents a single row collection.
    ''' </summary>
    Public ReadOnly Property IsSingleRowCollection As Boolean
        Get
            Return (mDataType = DataType.collection AndAlso Children.SingleRow)
        End Get
    End Property

#End Region

#Region " Public methods "

    ''' <summary>
    ''' Checks if this field has any child fields.
    ''' </summary>
    ''' <returns>True if this field has child fields defined on it; False otherwise.
    ''' </returns>
    Public Function HasChildren() As Boolean
        Return (mChildren IsNot Nothing AndAlso mChildren.Count > 0)
    End Function

    ''' <summary>
    ''' Creates a copy of the given child field collection and sets it within this
    ''' field definition.
    ''' </summary>
    ''' <param name="mgr">The collection definition manager which is managing the
    ''' children fields to be set into this collection definition. A null or empty
    ''' definition manager has no effect on the children in this field definition.
    ''' </param>
    Public Sub CopyChildren(ByVal mgr As ICollectionDefinitionManager)
        If mgr Is Nothing Then Return
        ' Create children defn if not already there and set to mimic the given mgr.
        Children.SetFrom(mgr)
    End Sub

    ''' <summary>
    ''' Sets this field definition's attributes (name, description, datatype and
    ''' children) from the given field definition, and propogates the change b
    ''' informing the parent definition that the data on this field has been
    ''' modified.
    ''' </summary>
    ''' <param name="fld">The field from which the attributes should be copied.
    ''' </param>
    Public Sub SetFrom(ByVal fld As clsCollectionFieldInfo)

        If fld.Name = Me.Name _
         AndAlso fld.Description = Me.Description _
         AndAlso fld.DataType = Me.DataType _
         AndAlso fld.Children = Me.Children Then Return ' Already we have the same value...

        Dim oldCopy As clsCollectionFieldInfo = DirectCast(Me.Clone(), clsCollectionFieldInfo)

        With fld
            ' If the previous item was a collection, then we need to remove the children.
            If oldCopy.DataType = DataType.collection AndAlso fld.DataType <> DataType.collection Then
                .Children.Clear()
            End If
            ' We need to update the children first - the stage listens for changes
            ' to its definition (including changes to nested fields) and updates
            ' the corresponding definitions in its initial/current values - if we
            ' change the name first, the values will contain an obsolete name and
            ' the stage won't be able to find the field which should change.
            ' See bug 6475 for the results of doing it the wrong way round.
            Me.Children.SetFrom(.Children)
            Me.Name = .Name
            Me.Description = .Description
            Me.DataType = .DataType
        End With

        Try
            ' Register the change with the parent definition.
            mParent.OnFieldModified(mParent, oldCopy, Me)
        Catch
            ' If there are any errors we need to revert our data back to that which
            ' it was originally
            With oldCopy
                Me.Name = .Name
                Me.Description = .Description
                Me.DataType = .DataType
                Me.Children.SetFrom(.Children)
            End With

            ' And rethrow to let the caller know that the SetFrom has not occurred
            Throw

        End Try

    End Sub

    ''' <summary>
    ''' Creates a new process value which can be held by a field using this
    ''' definition.
    ''' </summary>
    ''' <returns>A newly initialised process value which can be held in fields of
    ''' this definition.</returns>
    Public Function NewValue() As clsProcessValue
        Dim val As New clsProcessValue(DataType)
        If DataType = DataType.collection Then
            val.Collection = New clsCollection()
            val.Collection.Definition.SetFrom(Children)

            ' Add an empty row if the collection definition is "SingleRow" because its assumed that single rows will always have one row 
            If val.Collection.Definition.SingleRow Then
                val.Collection.Insert(0)
            End If
        End If
        Return val
    End Function

    ''' <summary>
    ''' Checks for equality against the given object.
    ''' </summary>
    ''' <param name="obj">The object to compare this field definition to.</param>
    ''' <returns>True if the given object is a non-null clsCollectionFieldInfo
    ''' object representing a field with the same value as this one - ie. the same
    ''' name, description, datatype and children. False otherwise.
    ''' <strong>Note: </strong> The ancestry of the field definition is not checked
    ''' to test equality, only its attributes and its descendant definitions.
    ''' </returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return Equals(TryCast(obj, clsCollectionFieldInfo))
    End Function

    ''' <summary>
    ''' Checks for equality against the given object.
    ''' </summary>
    ''' <param name="fld">The object to compare this field definition to.</param>
    ''' <returns>True if the given object is a non-null clsCollectionFieldInfo
    ''' object representing a field with the same value as this one - ie. the same
    ''' name, description, datatype and children. False otherwise.
    ''' <strong>Note: </strong> The ancestry of the field definition is not checked
    ''' to test equality, only its attributes and its child definitions.</returns>
    Public Overloads Function Equals(ByVal fld As clsCollectionFieldInfo) As Boolean
        If fld Is Nothing Then Return False
        If fld Is Me Then Return True ' Shortcut check...
        Return Me.Name = fld.Name _
         AndAlso Me.Description = fld.Description _
         AndAlso Me.DataType = fld.DataType _
         AndAlso Me.Children = fld.Children
    End Function

    ''' <summary>
    ''' Creates a <em>disassociated</em> clone of this field info object, meaning
    ''' that the clone created will not be associated with a stage. Equally, any
    ''' children associated with this field will also not be associated with a
    ''' stage.
    ''' </summary>
    ''' <returns>A deep-cloned copy of this field info object, with no stage
    ''' associations retained.</returns>
    Public Function Clone() As Object Implements ICloneable.Clone
        Dim fi As clsCollectionFieldInfo = DirectCast(Me.MemberwiseClone(), clsCollectionFieldInfo)

        ' Disassociate from this field's parent
        fi.mParent = Nothing

        ' Currently all the children (and their container) are shared with this field.
        ' This will not do. This will not do at all.

        ' By setting the collection defn to null, it forces a recreate when
        ' copying the children from this object to the clone. 
        fi.mChildren = Nothing

        ' If we have no children in this field, there's no need to do anything
        ' "mChildren = Nothing" is semantically identical to "mChildren = Empty Object"
        ' so the distinction doesn't interest us.

        ' However, if we do have children, copy them across.
        If HasChildren() Then fi.CopyChildren(mChildren)

        Return fi

    End Function

#End Region

#Region " XML Input/Output "

    ''' <summary>
    ''' Serializes this collection field definition to XML in the given document,
    ''' appended to the given element.
    ''' </summary>
    ''' <param name="doc">The document to which this field definition should be
    ''' serialized.</param>
    ''' <param name="parentElement">The element to which the resulting XML element
    ''' should be appended.</param>
    ''' <remarks>On return, this field will be fully represented as a child element
    ''' under the given <paramref name="parentElement"/></remarks>
    Friend Sub ToXml(ByVal doc As XmlDocument, ByVal parentElement As XmlElement)

        Dim el As XmlElement = doc.CreateElement("field")
        el.SetAttribute("name", mName)
        el.SetAttribute("type", mDataType.ToString())
        If mNamespace <> "" Then el.SetAttribute("namespace", mNamespace)
        If mDescription <> "" Then el.SetAttribute("description", mDescription)
        If mDisplayName <> "" Then el.SetAttribute("displayname", mDisplayName)
        ' If this field has any children, append them as children to this field's element.
        If HasChildren() Then
            If mChildren.SingleRow Then el.SetAttribute("singlerow", "1")
            For Each fld As clsCollectionFieldInfo In mChildren
                fld.ToXml(doc, el)
            Next
        End If
        parentElement.AppendChild(el)

    End Sub

    ''' <summary>
    ''' Loads this field definition with data from the given element, using the
    ''' given collection definition as its parent.
    ''' </summary>
    ''' <param name="parent">The parent collection definition which should be set
    ''' within this field definition.</param>
    ''' <param name="el">The element representing the field in XML.</param>
    Friend Sub FromXml(ByVal parent As clsCollectionInfo, ByVal el As XmlElement)
        mParent = parent
        mName = el.GetAttribute("name")
        mDataType = clsProcessDataTypes.DataTypeId(el.GetAttribute("type"))
        If el.HasAttribute("description") Then mDescription = el.GetAttribute("description")
        If el.HasAttribute("displayname") Then mDisplayName = el.GetAttribute("displayname")
        For Each child As XmlElement In el.ChildNodes
            Children.AddField(child)
        Next
        If el.HasAttribute("singlerow") Then Children.SingleRow = True
        If el.HasAttribute("namespace") Then mNamespace = el.GetAttribute("namespace")

    End Sub

#End Region

End Class
