#If UNITTESTS Then

Imports System.Diagnostics.CodeAnalysis
Imports System.Text
Imports BluePrism.Common.Security

''' <summary>
''' Helper functions for the unit tests - i.e. bits and bobs that are used for
''' various tests.
''' </summary>
<ExcludeFromCodeCoverage()>
Public Class TestHelper

    ''' <summary>
    ''' Returns a dictionary of clsProcessValue instances, designed to be used
    ''' across several round trip tests.
    ''' </summary>
    ''' <param name="includePopulatedCollection">
    ''' Default is False. When this is turned on then the process values returned
    ''' will contain a collection containing rows. Note: Fluent Assertions
    ''' cannot deal with clsCollection containing rows, so we can't use a populated
    ''' collection for any tests using this for assertions.
    ''' </param>
    ''' <returns>A dictionary containing different clsProcessValue instances for 
    ''' testing where the key is the description of clsProcessValue, and the value
    ''' is the instance of the clsProcessValue.</returns>
    ''' <remarks>As clsProcessValue uses ISerializable but is used within chains of 
    ''' classes that use DataContracts, it seems like a good idea to always check
    ''' the full range of process values in each of these tests</remarks>
    Public Shared Function CreateProcessValueDictionary(Optional includePopulatedCollection As Boolean = False) _
                                                            As Dictionary(Of String, clsProcessValue)

        Dim values As New Dictionary(Of String, clsProcessValue)

        With values
            .Add("Text", New clsProcessValue("sample text") _
                       With {.Description = "Description 1"})
            .Add("Password", New clsProcessValue("sample text".AsSecureString()))
            .Add("Flag", New clsProcessValue(False))
            .Add("Time", New clsProcessValue(DataType.time, Now()))
            .Add("Datetime", New clsProcessValue(DataType.datetime, Now()))
            .Add("Date", New clsProcessValue(DataType.date, Now()))
            .Add("TimeSpan", New clsProcessValue(New TimeSpan(1, 2, 3)))
            .Add("Number", New clsProcessValue(100))
            .Add("Bytes", New clsProcessValue(Encoding.UTF8.GetBytes("sample text")))
            .Add("Empty Collection", New clsProcessValue(New clsCollection))
            Dim nullCollection As clsCollection = Nothing
            .Add("Null Collection", New clsProcessValue(nullCollection))
        End With

        If includePopulatedCollection Then
            Dim collection = CreatePopulatedCollection(1, False)
            values.Add("Populated Collection", New clsProcessValue(collection))
        End If

        Return values

    End Function

    ''' <summary>
    ''' Create a collection with data for use in round trip unit tests. The collection will contain 
    ''' every data type, including an empty collection, null collection and populated collection.
    ''' </summary>
    ''' <param name="numOfRows">The number of rows of data to populate the collection with</param>
    ''' <returns>A collection with a definition defined and populated with data</returns>
    Public Shared Function CreateCollectionWithData(numOfRows As Integer) As clsCollection
        Return CreatePopulatedCollection(numOfRows, True)
    End Function

    ''' <summary>
    ''' Creates a collection populated with data. The collection will contain every data
    ''' type, including an empty collection, null collection and optionally a populated collection.
    ''' </summary>
    ''' <param name="numOfRows">The number of rows of data to populate the collection with</param>
    ''' <param name="includePopulatedCollection">Whether to include a populated collection</param>
    ''' <returns>A collection with a definition defined and populated with data</returns>
    Private Shared Function CreatePopulatedCollection(numOfRows As Integer,
                                                      includePopulatedCollection As Boolean) As clsCollection

        Dim coll As New clsCollection()

        ' Get a dictionary of process values that cover every data type including an empty collection, 
        ' null collection and a populated collection
        Dim values As Dictionary(Of String, clsProcessValue) =
            CreateProcessValueDictionary(includePopulatedCollection)

        ' Loop through the process values and build up the collection definition
        ' based on the data types
        For Each p In values
            coll.Definition.AddField(p.Key, p.Value.DataType)
        Next

        ' Populate the collection with data based on the process values
        For i = 1 To numOfRows Step 1
            Dim row As New clsCollectionRow()
            For Each p In values
                row(p.Key) = p.Value
            Next
            coll.Add(row)
        Next

        Return coll

    End Function

End Class

#End If