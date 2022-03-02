Imports System.IO
Imports System.Xml
Imports System.Linq
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Stages.clsCollectionStage
Imports BluePrism.Common.Security
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models
Imports System.Text.RegularExpressions
Imports LocaleTools
Imports Newtonsoft.Json

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollection
''' 
''' <summary>
''' This class represents an instance of a Collection (the Blue Prism data type,
''' which has nothing to do with a .NET Collection!). A Collection consists of zero
''' or more rows, each represented by a clsCollectionRow. Each row is a set of
''' name/value pairs, which each value being a clsProcessValue.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsCollection
    Implements ICollectionDefinitionManager

#Region " Statics / Constants "

    ''' <summary>
    ''' Checks if the given collection is null or empty.
    ''' </summary>
    ''' <param name="coll">The collection to check</param>
    ''' <returns>True if the collection is null or has no data. False otherwise.
    ''' </returns>
    Public Shared Function IsNullOrEmpty(ByVal coll As clsCollection) As Boolean
        Return (coll Is Nothing OrElse coll.Count = 0)
    End Function

    ''' <summary>
    ''' Splits the given collection path into top level and remaining path,
    ''' effectively "popping" the top level off the path and returning the head and
    ''' remaining tail elements.
    ''' </summary>
    ''' <param name="path">The collection path for which the top level should be
    ''' split from the rest.</param>
    ''' <returns>A tuple of (head, tail) - ie. the head element from the collection
    ''' path and the remaining elements in dot-separated form</returns>
    ''' -----------------------------------------------------------------------------
    Public Shared Function SplitPath(path As String) As (String, String)
        Dim match = Regex.Match(path, "^([^\.]+)(\.(\s*){1}([^\.,\s]+)([^\.]*))*$")

        If Not match.Success Then Throw New InvalidFormatException(
             My.Resources.Resources.clsCollection_InvalidCollectionPathToSplit0, path)

        Dim topLevel = match.Groups(1).Value

        If (path.Length <= topLevel.Length) Then
            Return (topLevel, String.Empty)
        End If

        Return (topLevel, path.Remove(0, topLevel.Length + 1))
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Checks if the given collections are equal by value, ie. this tests the data
    ''' within the collections for equality, not necessarily the definition of the
    ''' collection except in the case that the definition changes the data inside.
    ''' 
    ''' Essentially, this means that testing 2 empty collections with different
    ''' definitions will find that they are equal (ie. both have no data) rather than
    ''' finding that they differ in the definition.
    ''' </summary>
    ''' <param name="coll1">The first collection to check</param>
    ''' <param name="coll2">The second collection to check</param>
    ''' <returns>True if the 2 collections have the same data - ie. they are both
    ''' empty (null is treated as an empty collection in this context) or they both
    ''' contain the same number of rows with the same field values within;
    ''' False otherwise.</returns>
    Public Overloads Shared Function Equals(
     ByVal coll1 As clsCollection, ByVal coll2 As clsCollection) As Boolean
        If IsNullOrEmpty(coll1) _
         Then Return IsNullOrEmpty(coll2) _
         Else Return coll1.Equals(coll2)
    End Function

    ''' <summary>
    ''' Gets a label depicting info about the given collection. Returns any of :-
    ''' <list>
    ''' <item>"No Data" if there was no collection (ie. it was null)</item>
    ''' <item>"Empty" if the given collection had no data</item>
    ''' <item>"[n] row(s)" if the given collection has data and is not being
    '''       iterated over.</item>
    ''' <item>"Row [m] of [n]" if the given collection has data and is in the
    '''       process of being iterated over.</item>
    ''' </list>
    ''' </summary>
    ''' <param name="coll">The collection for which information is required.
    ''' </param>
    ''' <returns>A label with summary information about the given collection.
    ''' </returns>
    Public Shared Function GetInfoLabel(ByVal coll As clsCollection) As String

        If coll Is Nothing Then
            Return My.Resources.Resources.clsCollection_NoData

        ElseIf coll.Count = 0 Then
            Return My.Resources.Resources.clsCollection_Empty

        ElseIf (coll.CurrentRowIndex < 0) Then
            Return LTools.Format(My.Resources.Resources.clsCollection_plural_rows, "COUNT", coll.Count)

        Else
            Return String.Format(My.Resources.Resources.clsCollection_Row0Of1, coll.CurrentRowIndex + 1, coll.Count)

        End If

    End Function

    ''' <summary>
    ''' Parses the given XML assuming it has no root element. Really, this is here
    ''' for the clsCollectionStage parsing - it stores collection XML within an
    ''' 'initialvalue' element directly, so it needs to be able to parse the contents
    ''' without providing the root 'collection' element.
    ''' </summary>
    ''' <param name="xml">The XML to parse into a collection value, missing the root
    ''' 'collection' element.</param>
    ''' <returns>A clsCollection parsed from the given XML, or null if the given XML
    ''' was empty.</returns>
    ''' <exception cref="InvalidFormatException">If a row failed to be added to the
    ''' the collection, proabably meaning that the data has duplicate field names.
    ''' </exception>
    Friend Shared Function ParseWithoutRoot(ByVal xml As String) As clsCollection
        If xml = "" Then Return Nothing
        Dim coll As New clsCollection()
        coll.Parse("<collection>" & xml & "</collection>")
        Return coll
    End Function

    ''' <summary>
    ''' The 'ExtendedProperty' name which can represent a blue prism type in a
    ''' DataColumn which represents a Blue Prism collection.
    ''' </summary>
    Friend Const BluePrismTypeExtendedPropertyName As String = "bptype"

#End Region


#Region " Member Variables "

    ' The definition describing the structure of this collection
    <DataMember, JsonIgnore>
    Private mDefinition As clsCollectionInfo

    ' Flag indicating if the current row has been marked for deletion
    <DataMember, JsonIgnore>
    Private mCurrentRowDeleted As Boolean

    ' The current row index, default is 0 (so it is initialised to 1st row)
    <DataMember, JsonIgnore>
    Private mCurrentRowIndex As Integer

    ' The rows in this collection
    <DataMember, JsonProperty("Rows")>
    Private mRows As List(Of clsCollectionRow)

#End Region

    ''' <summary>
    ''' The definition (i.e. fields, data types) for this collection. This
    ''' definition should not be modified directly - instead, call the relevant
    ''' methods on the collection itself. This allows the values to be kept
    ''' consistent if the field definitions change.
    ''' This is never Nothing - there is always a Definition, even if there are
    ''' no fields.
    ''' </summary>
    Public ReadOnly Property Definition() As clsCollectionInfo
        Get
            Return mDefinition
        End Get
    End Property

    ''' <summary>
    ''' The index of the current row, or -1 if there is no current row.
    ''' </summary>
    Public Property CurrentRowIndex() As Integer
        Get
            If mDefinition.SingleRow Then Return 0
            If mRows.Count > 0 AndAlso mCurrentRowIndex < mRows.Count AndAlso Not mCurrentRowDeleted Then
                Return mCurrentRowIndex
            Else
                Return -1
            End If
        End Get
        Set(ByVal value As Integer)
            SetCurrentRow(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets whether a current row is currently set in this collection.
    ''' </summary>
    Public ReadOnly Property IsCurrentRowSet As Boolean
        Get
            Return (CurrentRowIndex <> -1)
        End Get
    End Property

    ''' <summary>
    ''' Gets the row at the specified index.
    ''' </summary>
    Public ReadOnly Property Row(ByVal index As Integer) As clsCollectionRow
        Get
            Return mRows(index)
        End Get
    End Property


    ''' <summary>
    ''' The number of rows currently in the collection.
    ''' </summary>
    Public ReadOnly Property Count() As Integer
        Get
            Return mRows.Count
        End Get
    End Property


    ''' <summary>
    ''' The rows currently in the collection.
    ''' </summary>
    Public ReadOnly Property Rows() As ICollection(Of clsCollectionRow)
        Get
            Return mRows
        End Get
    End Property

#Region " Constructors "

    ''' <summary>
    ''' Default constructor.
    ''' </summary>
    Public Sub New()
        mRows = New List(Of clsCollectionRow)
        mDefinition = New clsCollectionInfo()
        mCurrentRowDeleted = False

        'Really there should be no current row unless you do something (e.g. loop)
        'to make there be one, but for backwards compatibility with what I can only
        'assume was a bug, by default the current row is the first one...
        '(This is even more silly, since there may not even be a row there, but the
        'accessors filter this out)
        mCurrentRowIndex = 0
    End Sub

    ''' <summary>
    ''' Creates a collection containing only the given row.
    ''' </summary>
    ''' <param name="row">The row to create in this collection.</param>
    Public Sub New(ByVal row As clsCollectionRow)
        Me.New()
        Add(row)
    End Sub

    ''' <summary>
    ''' Constructor to populate a collection using a data table.
    ''' </summary>
    ''' <param name="dt">The data table</param>
    Public Sub New(ByVal dt As DataTable)

        Me.New()
        ' First, set up the schema
        For Each col As DataColumn In dt.Columns
            Dim d As DataType = clsProcessDataTypes.GetDataTypeFromFrameworkEquivalent(col.DataType)

            ' If the data table indicates what type of data it is, we should use that
            If col.ExtendedProperties.ContainsKey(BluePrismTypeExtendedPropertyName) Then
                Dim prop = col.ExtendedProperties(BluePrismTypeExtendedPropertyName)
                If TypeOf prop Is DataType Then d = DirectCast(prop, DataType)
            End If

            If d = DataType.unknown Then Throw New InvalidTypeException(
                My.Resources.Resources.clsCollection_CanTConvert0ToABluePrismDataType, col.DataType)

            AddField(col.ColumnName, d)
        Next
        ' Then, add the data.
        For Each row As DataRow In dt.Rows
            mRows.Add(New clsCollectionRow(row))
        Next

    End Sub

    ''' <summary>
    ''' Creates a new collection with its initial data populated from the given XML.
    ''' </summary>
    ''' <param name="xml">The XML representing the values in this collection.
    ''' </param>
    Public Sub New(ByVal xml As String)
        Me.New()
        Parse(xml)
    End Sub

#End Region

#Region " Xml Handling "

    ''' <summary>
    ''' Generates XML that represents all the fields rows and values inside the
    ''' collection.
    ''' </summary>
    ''' <param name="loggable">True to return value in loggable format</param>
    ''' <returns>The XML representation of this collection</returns>
    Public Function GenerateXML(Optional ByVal loggable As Boolean = False) As String
        Dim xmlDoc As XmlDocument = GenerateXMLDocument(loggable)

        ' read the contents into a string
        Using xml As New StringWriter()
            Using text As New XmlTextWriter(xml)
                xmlDoc.WriteTo(text)
                Return xml.ToString()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Generates XMLDocument that represents all the fields rows and values inside the
    ''' collection.
    ''' </summary>
    ''' <param name="loggable">True to return value in loggable format</param>    
    ''' <returns>An XmlDocument containing the XML representing this collection.</returns>

    Public Function GenerateXMLDocument(Optional loggable As Boolean = False) As XmlDocument
        Dim xmlDoc As New XmlDocument

        ' Create the framework
        Dim element As XmlElement = xmlDoc.CreateElement("collection")
        xmlDoc.AppendChild(element)

        ' Fill in the contents
        GenerateXMLInternal(xmlDoc, element, loggable)

        Return xmlDoc
    End Function

    ''' <summary>
    ''' Generates the internal XML for a collection into the given text writer. Note
    ''' that this does not generate the root node; this should be generated by the
    ''' caller. Use <see cref="GenerateXML"/> to get a fully formed XML
    ''' representation of this collection including a valid root node.
    ''' </summary>
    ''' <param name="xmlDoc">the xml document to write the xml into</param>
    ''' <param name="parent">the parent xml node</param>
    ''' <param name="loggable">Logability status</param>
    Friend Sub GenerateXMLInternal(
        xmlDoc As XmlDocument, parent As XmlElement, ByVal loggable As Boolean)

        If SingleRow Then parent.AppendChild(xmlDoc.CreateElement("singlerow"))

        For Each row As clsCollectionRow In mRows
            Dim xmlRow As XmlElement = xmlDoc.CreateElement("row")

            For Each fieldname As String In row.FieldNames
                Dim field As XmlElement = xmlDoc.CreateElement("field")

                field.Attributes.Add(xmlDoc, "name", fieldname)

                Dim val As clsProcessValue = row(fieldname)
                field.Attributes.Add(xmlDoc, "type", val.EncodedType)

                If loggable Then
                    field.Attributes.Add(xmlDoc, "value", val.LoggableValue)

                ElseIf val.DataType = DataType.password Then
                    field.Attributes.Add(xmlDoc, "encvalue", val.EncodedValue)

                ElseIf val.DataType = DataType.collection Then
                    If val.HasCollectionData Then
                        val.Collection.GenerateXMLInternal(xmlDoc, field, loggable)
                    End If
                Else
                    Dim [value] As XmlAttribute = xmlDoc.CreateAttribute("value")
                    [value].Value = val.EncodedValue
                    field.Attributes.Append([value])

                End If

                xmlRow.AppendChild(field)
            Next
            parent.AppendChild(xmlRow)
        Next
    End Sub

    ''' <summary>
    ''' Parse an xml string into this collection object.
    ''' </summary>
    ''' <param name="xml">The XML to parse into this collection</param>
    Public Sub Parse(ByVal xml As String)
        'If the xml is empty then leave the collection empty
        If xml = "" Then Exit Sub

        Parse(New ReadableXmlDocument(xml))
    End Sub

    ''' <summary>
    ''' Populates this collection with values collected from XML
    ''' </summary>
    ''' <param name="xml">The XML representing the values in the collection</param>
    ''' <exception cref="InvalidFormatException">If a row failed to be added to the
    ''' this collection, proabably meaning that the data has duplicate field names.
    ''' </exception>
    Public Sub Parse(ByVal xml As XmlDocument)

        Dim collElem = xml.ChildNodes.OfType(Of XmlElement).First(
         Function(e) e.Name = "collection")

        If collElem IsNot Nothing Then ParseInternal(collElem)

    End Sub

    ''' <summary>
    ''' Populates this collection with the data within the given XML element.
    ''' </summary>
    ''' <param name="elem">The element within which the collection data has been
    ''' stored</param>
    Private Sub ParseInternal(elem As XmlElement)
        For Each rowEl As XmlElement In elem.ChildNodes
            Select Case rowEl.Name
                Case "singlerow"
                    mDefinition.SingleRow = True
                Case "row"
                    Dim collRow As New clsCollectionRow()
                    For Each fieldEl As XmlElement In rowEl
                        If fieldEl.Name <> "field" Then Continue For

                        Dim val As clsProcessValue = Nothing
                        Dim dt As DataType =
                             clsProcessDataTypes.Parse(fieldEl.GetAttribute("type"))
                        If dt = DataType.collection Then
                            If fieldEl.HasAttribute("value") Then
                                val = New clsProcessValue(
                                    dt, fieldEl.GetAttribute("value"))

                            Else
                                Dim coll As New clsCollection()
                                coll.ParseInternal(fieldEl)
                                val = New clsProcessValue(coll)
                            End If

                        ElseIf dt = DataType.password Then
                            Dim encval = fieldEl.GetAttribute("encvalue")
                            If encval <> "" Then
                                val = clsProcessValue.Decode(DataType.password, encval)
                            Else
                                val = New clsProcessValue(
                                     New SafeString(fieldEl.GetAttribute("value")))
                            End If
                        Else
                            val = clsArgument.TreatAsTextIfNeeded(fieldEl, dt)

                        End If

                        Try
                            collRow.Add(fieldEl.GetAttribute("name"), val)

                        Catch ex As Exception
                            'If we failed to add to the row, it is most likely that
                            'the data returned has duplicate field names. If we don't trap
                            'it, the user can get stuck in an endless loop.
                            Throw New InvalidFormatException(
                                 My.Resources.Resources.clsCollection_InvalidCollectionDataReturnedFromActionAreFieldNamesDuplicated)
                        End Try
                    Next
                    Add(collRow)
            End Select
        Next

    End Sub

#End Region
    ''' <summary>
    ''' Sets the current row to be the first row, ready for an iteration.
    ''' The current row can then be accessed via GetCurrentRow()
    ''' </summary>
    ''' <returns>Returns true if the operation was successful; false
    ''' otherwise (eg in the event that the collection has no rows).
    ''' </returns>
    Public Function StartIterate() As Boolean

        If mRows.Count > 0 Then
            mCurrentRowIndex = 0
            mCurrentRowDeleted = False
            Return True
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' Advances the current row to the next row in the collection, if there is 
    ''' such a row. The current row can then be accessed via GetCurrentRow()
    ''' </summary>
    ''' <returns>Returns true if successful, false otherwise (eg if there 
    ''' are no more rows, or if there are no rows at all).</returns>
    Public Function ContinueIterate() As Boolean

        If mRows.Count > 0 Then
            If mCurrentRowDeleted Then
                'If the last row was deleted then there is no need to increment the index
            Else
                mCurrentRowIndex += 1
            End If
            mCurrentRowDeleted = False
            Return (mCurrentRowIndex < mRows.Count)
        End If

        Return False

    End Function


    ''' <summary>
    ''' Gets the current row in the collection. Which row is the current row 
    ''' can be changed using the methods StartIterate() and ContinueIterate().
    ''' 
    ''' Note that the current row will be undefined after a call to
    ''' delete it. It will remain undefined until either StartIterate
    ''' or ContinueIterate is called.
    ''' </summary>
    ''' <returns>Returns the current row, if this has been defined using
    ''' the methods StartIterate() and ContinueIterate(). Returns
    ''' Nothing if there is no current row, or if the collection has no
    ''' rows at all, or if the current row has been deleted.</returns>
    Public Function GetCurrentRow() As clsCollectionRow

        'We use the property here, to take advantage of the handling of single
        'row collections...
        Dim row As Integer = CurrentRowIndex
        If (Not mCurrentRowDeleted) AndAlso (row > -1) AndAlso (row < mRows.Count) Then
            Return mRows(row)
        Else
            Return Nothing
        End If

    End Function


    ''' <summary>
    ''' Sets the current row.
    ''' </summary>
    ''' <param name="i"></param>
    Public Sub SetCurrentRow(ByVal i As Integer)
        mCurrentRowIndex = i
        mCurrentRowDeleted = False
    End Sub


    ''' <summary>
    ''' Determines if we are currently in an iteration.
    ''' </summary>
    ''' <returns>True if the current row is defined, false otherwise.</returns>
    Public Function IsInIteration() As Boolean
        Return (mCurrentRowIndex > -1) AndAlso (mCurrentRowIndex < mRows.Count)
    End Function

    ''' <summary>
    ''' Adds a row to the collection. For a single row collection, the current row
    ''' is replaced.
    ''' </summary>
    ''' <param name="row">The collection row to add</param>
    ''' <returns>The index of the point in the collection where the row was added
    ''' </returns>
    ''' <exception cref="EmptyDefinitionException">If this collection has no
    ''' definition and the given row has no definition, and thus the collection 
    ''' cannot contain any rows.</exception>
    Public Function Add(ByVal row As clsCollectionRow) As Integer
        If mDefinition.SingleRow Then mRows.Clear()

        Insert(mRows.Count, row)
        Return mRows.Count - 1
    End Function

    ''' <summary>
    ''' Adds a row to the collection, with blank values for all fields. For a single
    ''' row collection, the current row is replaced.
    ''' </summary>
    ''' <returns>The index of the point in the collection where the row was added
    ''' </returns>
    ''' <exception cref="EmptyDefinitionException">If this collection has no
    ''' definition, and thus cannot contain any rows.</exception>
    Public Function Add() As Integer
        If mDefinition.SingleRow Then mRows.Clear()

        Insert(mRows.Count)
        Return mRows.Count - 1
    End Function


    ''' <summary>
    ''' Inserts a row into the collection, with blank values for all fields.
    ''' </summary>
    ''' <param name="index">The index to insert before.</param>
    Public Sub Insert(ByVal index As Integer)
        ' Really if there are no columns, it doesn't make sense to have any rows,
        ' but if you set a collection to be 'SingleRow' when it has no definition,
        ' it needs to work, so allow it through..
        ' If mDefinition.Count = 0 Then Throw New EmptyDefinitionException()

        Dim row As New clsCollectionRow()
        For Each field As clsCollectionFieldInfo In mDefinition
            row.Add(field.Name, field.NewValue())
        Next
        Insert(index, row)
    End Sub


    ''' <summary>
    ''' Inserts the supplied row at the specified index. If the row contains fields
    ''' that are not in the collection's definition, those fields are added to the
    ''' definition. If the row is missing fields that are in the collection's
    ''' definition, those fields are added to the row with null values.
    ''' </summary>
    ''' <param name="index">The zero-based index at which to insert. Must not exceed
    ''' the number of rows already in the collection.</param>
    ''' <param name="row">The row to be inserted.</param>
    ''' <exception cref="EmptyDefinitionException">If this collection has no
    ''' definition and the given row has no definition, and thus the collection 
    ''' cannot contain any rows.</exception>
    Public Sub Insert(ByVal index As Integer, ByVal row As clsCollectionRow)

        'Update definition if the row contains new fields...
        For Each f As String In row.FieldNames
            If Not mDefinition.Contains(f) Then
                mDefinition.AddField(f, row(f).DataType)
            End If
        Next

        'Insert null values for fields not in the row...
        For Each f As clsCollectionFieldInfo In mDefinition
            If Not row.Contains(f.Name) Then
                row.Add(f.Name, New clsProcessValue(f.DataType))
            End If
        Next

        mRows.Insert(index, row)
        'TODO: should we adjust the current row if we inserted before it?
    End Sub


    ''' <summary>
    ''' Allows us to see if the collection contains a particular value
    ''' </summary>
    ''' <param name="row">The row to look up</param>
    ''' <returns>True if the row is contained within the collection</returns>
    Public Function Contains(ByVal row As clsCollectionRow) As Boolean
        Return mRows.Contains(row)
    End Function

    ''' <summary>
    ''' Deletes the current row. After this method is called, the current row
    ''' will no longer be defined and the method GetCurrentRow will return
    ''' nothing until the current row is redefined via a call to 
    ''' either StartIterate() or ContinueIterate().
    ''' </summary>
    ''' <returns>Returns true if the current row was successfully deleted;
    ''' false otherwise (eg because the collection is not currently in
    ''' an iteration, or the collection is undefined, or is empty).</returns>
    Public Function DeleteCurrentRow() As Boolean
        If mDefinition.SingleRow Then Return False
        Dim row As clsCollectionRow = GetCurrentRow()
        Dim rowIndex As Integer = CurrentRowIndex

        Try
            If (Not row Is Nothing) Then
                mRows.RemoveAt(rowIndex)
                Return True
            End If
        Finally
            'We determine the value of the flag this way
            'because if an exception is thrown we can't
            'know whether the row has been successfully 
            'removed or not.
            mCurrentRowDeleted = Not ContainsByReference(row)
        End Try

        Return False
    End Function


    ''' <summary>
    ''' Checks if this collection contains a field with the given name.
    ''' </summary>
    ''' <param name="name">The name to search this collection definition for.</param>
    ''' <returns>True if there exists a field in the definition managed by this
    ''' object with the given name; False otherwise.</returns>
    Public Function ContainsField(ByVal name As String) As Boolean _
     Implements ICollectionDefinitionManager.ContainsField
        Return (GetFieldDefinition(name) IsNot Nothing)
    End Function

    ''' <summary>
    ''' Checks if this collection contains a given row, by reference rather than by
    ''' value.
    ''' </summary>
    ''' <param name="row">The row to check this collection for</param>
    ''' <returns>True if this collection contains a reference to
    ''' <paramref name="row"/>; False otherwise - notably False if the collection
    ''' contains a row with the same value as <paramref name="row"/> but not
    ''' referring to the same object in memory.</returns>
    Public Function ContainsByReference(row As clsCollectionRow) As Boolean
        For Each r As clsCollectionRow In mRows
            If r Is row Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Gets the collection field definition from this stage with the specified name
    ''' </summary>
    ''' <param name="name">The name of the field definition required.</param>
    ''' <returns>The field corresponding to the given name, or null if no such field
    ''' was found in this stage.</returns>
    Public Function GetFieldDefinition(ByVal name As String) As clsCollectionFieldInfo _
     Implements ICollectionDefinitionManager.GetFieldDefinition
        For Each fldInfo As clsCollectionFieldInfo In mDefinition
            If fldInfo.Name = name Then Return fldInfo
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the collection field definitions which are currently being held by this
    ''' stage.
    ''' </summary>
    Public ReadOnly Property FieldDefinitions() As IEnumerable(Of clsCollectionFieldInfo) _
     Implements ICollectionDefinitionManager.FieldDefinitions
        Get
            Return mDefinition.FieldDefinitions
        End Get
    End Property


    ''' <summary>
    ''' Gets the number of field definitions currently held by this stage.
    ''' </summary>
    Public ReadOnly Property FieldCount() As Integer Implements ICollectionDefinitionManager.FieldCount
        Get
            Return mDefinition.Count
        End Get
    End Property


    ''' <summary>
    ''' Add a new field to the collection. Existing rows have null values added.
    ''' </summary>
    ''' <param name="name">The name of the new field.</param>
    ''' <param name="datatype">The data type of the new field.</param>
    ''' <remarks></remarks>
    Public Sub AddField(ByVal name As String, ByVal datatype As DataType) Implements ICollectionDefinitionManager.AddField
        AddField(New clsCollectionFieldInfo(name, datatype, String.Empty))
    End Sub

    ''' <summary>
    ''' Add a new field to the collection. Existing rows have null values added.
    ''' </summary>
    ''' <param name="name">The name of the new field.</param>
    ''' <param name="datatype">The data type of the new field.</param>
    ''' <remarks></remarks>
    Public Sub AddField(ByVal name As String, ByVal datatype As DataType, ns As String) Implements ICollectionDefinitionManager.AddField
        AddField(New clsCollectionFieldInfo(name, datatype, ns))
    End Sub

    ''' <summary>
    ''' Performs the given nested value operation using the specified parameters
    ''' on the field specified in the parameters, held by this collection or by
    ''' collections nested within this collection.
    ''' </summary>
    ''' <param name="params">The parameters defining the operation and the fields
    ''' and collections on which the operation is to be invoked.</param>
    ''' <param name="op">The delegate called with the owners of the identified
    ''' fields within the parameters.</param>
    Friend Sub PerformNestedValueOperation(
     ByVal params As CollectionDelegateParams, ByVal op As NestedValueOperation)

        If params.LastInPath Then
            ' We've reached our destination.
            params.Owner = Me
            op(params)

        Else
            Try
                ' Otherwise, we have to go deeper
                Dim fieldName As String = params.NextField()

                ' The 'fieldName' represents the name of the nested collection which the
                ' field needs to be added to.
                ' So go through each row, get the collection corresponding to that name
                ' and pass the field onto that collection.
                For Each row As clsCollectionRow In mRows
                    Dim v As clsProcessValue = row(fieldName)
                    If v Is Nothing Then
                        Throw New ArgumentException(String.Format(
                         My.Resources.Resources.clsCollection_UnrecognisedField0InCollection1, fieldName, Me))
                    End If
                    If v.DataType <> DataType.collection Then
                        Throw New ArgumentException(String.Format(
                         My.Resources.Resources.clsCollection_Field0IsDatatype1ShouldBeCollection, fieldName, v.DataType))
                    End If

                    ' If the collection is null, autocreate it as appropriate.
                    ' Then recurse the operation through to the collection
                    If v.IsNull Then v.Collection = New clsCollection()
                    v.Collection.PerformNestedValueOperation(params, op)

                Next

            Finally ' Rewind so that other collections get a shot at this field.
                params.PreviousField()

            End Try
        End If
    End Sub

    ''' <summary>
    ''' Adds a field containing the same data as the given field info to the
    ''' definition managed by this object.
    ''' Note that the actual object given is not added, but an object with the same
    ''' value (ie. name, description, datatype, children) as the given field.
    ''' </summary>
    ''' <param name="fld">The field to add to this definition manager.</param>
    ''' <exception cref="ArgumentException">If a field with the given name already
    ''' exists on this collection and the rows in it and the data cannot be cast to
    ''' the new target type.</exception>
    Public Sub AddField(ByVal fld As clsCollectionFieldInfo) Implements ICollectionDefinitionManager.AddField
        AddField(fld, False)
    End Sub

    ''' <summary>
    ''' Adds the given field to this collection, ensuring that each row is updated
    ''' with the new field.
    ''' If the field already exists in a row, the data is cast into the new datatype.
    ''' If it is not compatible, either an exception is raised or the data is deleted
    ''' in favour of a new blank entry of the required type, dependent on the
    ''' <paramref name="failOnIncompatibleField"/> parameter.
    ''' </summary>
    ''' <param name="fld">The field to add to this collection.</param>
    ''' <param name="failOnIncompatibleField">True to raise an error if this
    ''' collection already holds data in fields with the same name as that specified,
    ''' and it cannot be converted to the new type. False to ignore such 
    ''' incompatibilities and force the new field in.</param>
    ''' <exception cref="ArgumentException">If a field with the given name already
    ''' exists on this collection and the rows in it and the data cannot be cast to
    ''' the new target type. Only thrown if <paramref name="failOnIncompatibleField"/>
    ''' is true.</exception>
    Public Sub AddField(ByVal fld As clsCollectionFieldInfo, ByVal failOnIncompatibleField As Boolean)
        mDefinition.AddField(fld)
        For Each row As clsCollectionRow In mRows
            Dim pv As clsProcessValue = Nothing

            ' If it's there and it's the same type, ignore the add - move onto the next row
            If row.TryGetValue(fld.Name, pv) AndAlso pv.DataType = fld.DataType Then Continue For

            ' If we have a "pv" then it was there, but the type was different.
            ' See if we can cast it
            Dim err As String = Nothing
            If pv IsNot Nothing AndAlso Not pv.TryCastInto(fld.DataType, pv, err) Then
                ' Failed to cast it, error if 'failOnIncompatible' is set
                If failOnIncompatibleField Then
                    Throw New ArgumentException(String.Format(
                     My.Resources.Resources.clsCollection_CollectionRowAlreadyHasA0FieldOfType123,
                     fld.Name, pv.DataType, vbCrLf, err))
                End If
                ' Otherwise, force it in - basically, blank the value.
                pv = Nothing
            End If

            ' If we don't have a value at this point, just provide a blank one.
            If pv Is Nothing Then pv = New clsProcessValue(fld.DataType)
            row.SetField(fld.Name, pv)
        Next
    End Sub

    ''' <summary>
    ''' Delete a field from the collection. Corresponding values are deleted from
    ''' existing rows.
    ''' Note that if the field being deleted is the last field in the collection,
    ''' all rows are deleted - the collection is effectively emptied.
    ''' </summary>
    ''' <param name="name">The name of the field to delete.</param>
    Public Sub DeleteField(ByVal name As String) Implements ICollectionDefinitionManager.DeleteField
        mDefinition.DeleteField(name)
        If mDefinition.Count = 0 Then
            mRows.Clear()
        Else
            For Each row As clsCollectionRow In mRows
                row.Remove(name)
            Next
        End If
    End Sub

    ''' <summary>
    ''' Clears all fields from this collection - by definition, this means that there
    ''' can be no rows in the collection either... thus this effectively clears the
    ''' collection.
    ''' </summary>
    Public Sub ClearFields() Implements ICollectionDefinitionManager.ClearFields
        mDefinition.Clear()
        mRows.Clear()
    End Sub

    ''' <summary>
    ''' Rename a field in the collection. Values are also renamed in existing rows.
    ''' (Note - this should be irrelevant, it's an artefact of historical silliness
    ''' that the rows also contain named items!)
    ''' </summary>
    ''' <param name="oldname">The old (existing) name.</param>
    ''' <param name="newname">The new name.</param>
    Public Sub RenameField(ByVal oldname As String, ByVal newname As String) _
     Implements ICollectionDefinitionManager.RenameField
        mDefinition.RenameField(oldname, newname)
        For Each row As clsCollectionRow In mRows
            Dim v As clsProcessValue = row(oldname)
            row.Remove(oldname)
            row.Add(newname, v)
        Next
    End Sub


    ''' <summary>
    ''' Change the datatype of a field in the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field.</param>
    ''' <param name="newtype">The new datatype.</param>
    Public Sub ChangeFieldDataType(ByVal name As String, ByVal newtype As DataType) _
     Implements ICollectionDefinitionManager.ChangeFieldDataType
        Dim oldtype As DataType = mDefinition.GetField(name).DataType
        mDefinition.ChangeFieldDataType(name, newtype)
        For Each row As clsCollectionRow In mRows
            Dim sErr As String = Nothing
            'Try and cast the data in each of the existing rows to the new type. If
            'we can't do it, we put a null value in instead.
            Try
                row(name) = row(name).CastInto(newtype)
            Catch
                row(name) = New clsProcessValue(newtype)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Set the description of a field in the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field which needs its description
    ''' changing</param>
    ''' <param name="desc">The description of the field to set.</param>
    Public Sub SetFieldDescription(ByVal name As String, ByVal desc As String) _
     Implements ICollectionDefinitionManager.SetFieldDescription
        mDefinition.SetFieldDescription(name, desc)
    End Sub

    ''' <summary>
    ''' True if the collection is a single-row collection. Changing the value of
    ''' this property will result in any underlying collection data being altered!
    ''' </summary>
    Public Property SingleRow() As Boolean Implements ICollectionDefinitionManager.SingleRow
        Get
            Return mDefinition.SingleRow
        End Get
        Set(ByVal value As Boolean)
            mDefinition.SingleRow = value
            If value Then
                If mRows.Count < 1 Then
                    Add()
                ElseIf mRows.Count > 1 Then
                    mRows.RemoveRange(1, mRows.Count - 1)
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Get the value of the given field, from the current row of the collection.
    ''' Handles references to fields inside nested collections.
    ''' </summary>
    ''' <param name="fullName">The fully qualified name of the field, relative to this
    ''' collection.</param>
    ''' <returns>The value found in the field with the given name, using the current
    ''' row of this and any nested collections traversed.</returns>
    ''' <exception cref="InvalidFormatException">If a field was expected to be a
    ''' collection when parsed from <paramref name="fullName"/>, but it turned out not to
    ''' be a collection.</exception>
    ''' <exception cref="EmptyCollectionException">If a referenced field within this
    ''' collection has no row in which data can be set.</exception>
    ''' <exception cref="NoCurrentRowException">If this collection has no current row
    ''' set and thus cannot set a value within it</exception>
    ''' <exception cref="FieldNotFoundException">If no field with the name
    ''' <paramref name="fullName"/> could be found in this collection.</exception>
    Public Function GetField(ByVal fullName As String) As clsProcessValue

        Dim row As clsCollectionRow = GetCurrentRow()
        If row Is Nothing Then Throw New InvalidOperationException(
            My.Resources.Resources.clsCollection_TheCollectionHasNoCurrentRow)

        Dim path As (name As String, subs As String) = SplitPath(fullName)

        Dim fieldDefn = GetFieldDefinition(path.name)
        If fieldDefn Is Nothing Then Throw New FieldNotFoundException(
         mDefinition, path.name)

        ' If the value is a collection and this collection's schema indicates that it
        ' is a single row collection, make sure it is initialised.
        Dim val = InitCollection(row(path.name), fieldDefn)

        If path.subs = "" Then Return val

        If val.DataType <> DataType.collection Then Throw New InvalidFormatException(
         My.Resources.Resources.clsCollection_ReferencedFieldIsNotACollectionButSubFieldNotationWasUsed)
        If Not val.HasCollectionData Then Throw New EmptyCollectionException(
         My.Resources.Resources.clsCollection_ChildCollectionInField0HasNoData, path.name)
        Return val.Collection.GetField(path.subs)
    End Function

    ''' <summary>
    ''' Initialises the collection in the given process value with the definition for
    ''' the field provided
    ''' </summary>
    ''' <param name="value">The value in which the collection that should be
    ''' initialised resides.</param>
    ''' <param name="fieldDefn">The definition of the field in which the value will
    ''' be stored.</param>
    ''' <returns>The process value given, with an initialised collection and schema
    ''' if the value represented a collection.</returns>
    Private Function InitCollection(
     value As clsProcessValue, fieldDefn As clsCollectionFieldInfo) As clsProcessValue
        If value.DataType <> DataType.collection Then Return value
        If value.HasCollectionData Then Return value

        If value.Collection Is Nothing Then value.Collection = New clsCollection()
        value.Collection.CopyDefinition(fieldDefn.Children)

        Return value
    End Function

    ''' <summary>
    ''' Sets the value of the given field, using the current row of the collection.
    ''' Handles references to fields inside nested collections.
    ''' </summary>
    ''' <param name="name">The qualified name of the field to set.</param>
    ''' <param name="pv">The value to set the field to.</param>
    ''' <exception cref="InvalidFormatException">If a field was expected to be a
    ''' collection when parsed from <paramref name="name"/>, but it turned out not to
    ''' be a collection.</exception>
    ''' <exception cref="EmptyCollectionException">If a referenced field within this
    ''' collection has no row in which data can be set.</exception>
    ''' <exception cref="NoCurrentRowException">If this collection has no current row
    ''' set and thus cannot set a value within it</exception>
    ''' <exception cref="FieldNotFoundException">If no field with the name
    ''' <paramref name="name"/> could be found in this collection.</exception>
    ''' <exception cref="InvalidTypeException">If the field identified by
    ''' <paramref name="name"/> was of a different type to <paramref name="pv"/>.
    ''' </exception>

    Public Sub SetField(ByVal name As String, ByVal pv As clsProcessValue)
        SetField(name, pv, False)
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Sets the value of the given field, using the current row of the collection.
    ''' Handles references to fields inside nested collections.
    ''' </summary>
    ''' <param name="fullName">The qualified name of the field to set.</param>
    ''' <param name="pv">The value to set the field to.</param>
    ''' <param name="tryCasting">True to try casting into the target data type, if
    ''' <paramref name="pv"/> is not of the correct type; False to fail immediately
    ''' if the datatype does not match the target field exactly.</param>
    ''' <exception cref="InvalidFormatException">If a field was expected to be a
    ''' collection when parsed from <paramref name="fullName"/>, but it turned out not to
    ''' be a collection.</exception>
    ''' <exception cref="EmptyCollectionException">If a referenced field within this
    ''' collection has no row in which data can be set.</exception>
    ''' <exception cref="NoCurrentRowException">If this collection has no current row
    ''' set and thus cannot set a value within it</exception>
    ''' <exception cref="FieldNotFoundException">If no field with the name
    ''' <paramref name="fullName"/> could be found in this collection.</exception>
    ''' <exception cref="InvalidTypeException">If the field identified by
    ''' <paramref name="fullName"/> was of a different type to <paramref name="pv"/> and
    ''' <paramref name="tryCasting"/> was false.</exception>
    ''' <exception cref="BadCastException">If <paramref name="pv"/> could not be cast
    ''' into the type required by the field identified by <paramref name="fullName"/>.
    ''' </exception>
    ''' -----------------------------------------------------------------------------
    Public Sub SetField(fullName As String, pv As clsProcessValue, tryCasting As Boolean)

        Dim row As clsCollectionRow = GetCurrentRow()
        If row Is Nothing Then Throw New NoCurrentRowException(
         My.Resources.Resources.clsCollection_CannotSet0TheCollectionHasNoCurrentRow, fullName)

        Dim path As (name As String, subs As String) = SplitPath(fullName)
        Dim fieldDefn As clsCollectionFieldInfo = mDefinition.GetField(path.name)
        If fieldDefn Is Nothing Then Throw New FieldNotFoundException(mDefinition, path.name)

        If path.subs = "" Then
            ' We're at the end of the path, so set the value.
            If fieldDefn.DataType <> pv.DataType Then
                If Not tryCasting Then Throw New InvalidTypeException(
                My.Resources.Resources.clsCollection_CannotSetA0ValueIntoTheField1ItsDataTypeIs2,
                 pv.DataType, path.name, fieldDefn.DataType)

                pv = pv.CastInto(fieldDefn.DataType)
            End If
            row(path.name) = pv

        Else
            If row(path.name).DataType <> DataType.collection Then Throw New InvalidFormatException(
                My.Resources.Resources.clsCollection_Field0IsNotACollection1ButSubFieldNotationWasUsed,
                path.name, row(path.name).DataType)

            ' Work our way through the rest of the collection
            Dim val = InitCollection(row(path.name), fieldDefn)

            If val.Collection Is Nothing Then Throw New EmptyCollectionException(
                My.Resources.Resources.clsCollection_ChildCollectionInField0HasNoData, path.name)

            Try
                val.Collection.SetField(path.subs, pv)
            Catch ex As NoCurrentRowException
                Throw New NoCurrentRowException(
                    String.Format(My.Resources.Resources.clsCollection_CannotSet0TheCollection1HasNoCurrentRow, fullName, path.name))
            Catch ex As FieldNotFoundException
                Throw New FieldNotFoundException(mDefinition, path.subs, path.name)
            End Try
        End If
    End Sub


    ''' <summary>
    ''' Update the collection field information to look for missing children.
    ''' </summary>
    ''' <param name="collectionInfo">collection info</param>
    Public Sub UpdateDefinitionsChildren(ByVal collectionInfo As clsCollectionInfo)

        If collectionInfo Is Nothing Then
            Return
        End If

        For Each f As clsCollectionFieldInfo In collectionInfo
            If mDefinition.Contains(f.Name) Then
                Dim definition = mDefinition.GetField(f.Name)
                If f.HasChildren AndAlso Not definition.HasChildren Then
                    definition.CopyChildren(f.Children)
                End If
            Else
                AddField(f)
            End If
        Next
        SingleRow = collectionInfo.SingleRow
    End Sub

    ''' <summary>
    ''' Copy another collection definition into this collection's definition. The
    ''' existing rows will be made to conform to the new schema.
    ''' </summary>
    ''' <param name="src">The definition to copy from - if null or empty, this has
    ''' the effect of clearing the definition in this collection.</param>
    Public Sub CopyDefinition(ByVal src As clsCollectionInfo)

        ' Clear the definition if we are copying an empty definition
        If src Is Nothing OrElse src.Count = 0 Then
            mDefinition.Clear()
            If src Is Nothing Then Return
        Else

            'Delete any fields that aren't in the definition we're copying...
            Dim todelete As New List(Of String)
            For Each f As clsCollectionFieldInfo In mDefinition
                If Not src.Contains(f.Name) Then
                    todelete.Add(f.Name)
                End If
            Next
            For Each s As String In todelete
                DeleteField(s)
            Next

            'Add any fields that we don't have...
            For Each f As clsCollectionFieldInfo In src
                If Not mDefinition.Contains(f.Name) Then AddField(f)
            Next
        End If

        'Copy single-row status. (Setting the property on ourself will
        'automatically make the collection data conform and update the definition
        SingleRow = src.SingleRow
    End Sub

    ''' <summary>
    ''' Tests for equality between this collection and the given object.
    ''' </summary>
    ''' <param name="obj">The object to test against.</param>
    ''' <returns>True if the given object is a non-null collection with the same
    ''' data in its rows as this collection.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim col As clsCollection = TryCast(obj, clsCollection)
        If col Is Nothing OrElse col.Count <> Me.Count Then Return False
        For i As Integer = 0 To mRows.Count - 1
            If Not col.mRows(i).Equals(mRows(i)) Then Return False
        Next
        Return True
    End Function

    ''' <summary>
    ''' Removes the first instance of the given row from this collection
    ''' </summary>
    ''' <param name="value">The row to remove</param>
    Public Sub Remove(ByVal value As clsCollectionRow)
        mRows.Remove(value)
    End Sub

    ''' <summary>
    ''' Clear all rows from this collection.
    ''' </summary>
    Public Sub Clear()
        mRows.Clear()
    End Sub

    ''' <summary>
    ''' Deep clones this collection - data and state.
    ''' </summary>
    ''' <returns>A clone of this collection, with the current row set to the same as
    ''' the current row in this collection.</returns>
    Public Function Clone() As clsCollection

        Dim copy As New clsCollection()
        copy.CopyDefinition(mDefinition)

        If mDefinition.SingleRow Then
            copy.mRows(0) = mRows(0).Clone()
        Else
            For Each row As clsCollectionRow In mRows
                copy.Add(New clsCollectionRow(row))
            Next
        End If

        copy.mCurrentRowIndex = mCurrentRowIndex
        Return copy

    End Function

End Class
