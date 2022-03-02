Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data

Public Interface ISqlHelper
    Sub SelectMultipleIds(Of TId)(connection As IDatabaseConnection, ids As IEnumerable(Of TId), selector As Action(Of IDataProvider), query As String)
    Sub SelectMultipleIds(Of TId)(connection As IDatabaseConnection, command As IDbCommand, ids As IEnumerable(Of TId), selector As Action(Of IDataProvider), query As String)
End Interface
