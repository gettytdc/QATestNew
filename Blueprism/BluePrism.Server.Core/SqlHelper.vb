Imports System.Text
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data

Public Class SqlHelper
    Implements ISqlHelper

    Public Const MaxSqlParams As Integer = 2000

    Private ReadOnly mGetDbCommand As Func(Of String, IDbCommand)

    Public Sub New(getDbCommand As Func(Of String, IDbCommand))
        mGetDbCommand = getDbCommand
    End Sub

    ''' <summary>
    ''' Selects data from the database using multiple (possibly more than the SQL
    ''' parameter limit) IDs, calling a specified <paramref name="selector"/> action
    ''' for each row returned.
    ''' </summary>
    ''' <typeparam name="TId">The type of the ID to set in the query</typeparam>
    ''' <param name="connection">The connection to the database to use</param>
    ''' <param name="ids">The IDs to retrieve the rows on the database for.</param>
    ''' <param name="selector">The action called for each row returned from the
    ''' database, passed in a 'data provider' instance with which the returned data
    ''' can be retrieved.</param>
    ''' <param name="query">The query to use for the select, including a placeholder
    ''' string for the ids, eg.
    ''' "select * from BPATable where tableid in ({multiple-ids}) order by thing"
    ''' The placeholder marks where the multiple IDs should go. There is no escape
    ''' for this placeholder, so care should be taken that the string
    ''' "{multiple-ids}" cannot occur in the rest of the query.
    ''' </param>
    Public Sub SelectMultipleIds(Of TId)(
        ByVal connection As IDatabaseConnection,
        ByVal ids As IEnumerable(Of TId),
        ByVal selector As Action(Of IDataProvider),
        ByVal query As String) _
        Implements ISqlHelper.SelectMultipleIds

        SelectMultipleIds(connection, mGetDbCommand(""), ids, selector, query)
    End Sub

    ''' <summary>
    ''' Selects data from the database using multiple (possibly more than the SQL
    ''' parameter limit) IDs, calling a specified <paramref name="selector"/> action
    ''' for each row returned.
    ''' </summary>
    ''' <typeparam name="TId">The type of the ID to set in the query</typeparam>
    ''' <param name="connection">The connection to the database to use</param>
    ''' <param name="command">The SQL command, pre-initialised with any non-ID parameters
    ''' required by the query. On leaving this method normally, the command text and
    ''' parameters will be reset to the state they were on entering.</param>
    ''' <param name="ids">The IDs to retrieve the rows on the database for.</param>
    ''' <param name="selector">The action called for each row returned from the
    ''' database, passed in a 'data provider' instance with which the returned data
    ''' can be retrieved.</param>
    ''' <param name="query">The query to use for the select, including a placeholder
    ''' string for the ids, eg.
    ''' "select * from BPATable where tableid in ({multiple-ids}) order by thing"
    ''' The placeholder marks where the multiple IDs should go. There is no escape
    ''' for this placeholder, so care should be taken that the string
    ''' "{multiple-ids}" cannot occur in the rest of the query.
    ''' </param>
    Public Sub SelectMultipleIds(Of TId)(
        ByVal connection As IDatabaseConnection,
        ByVal command As IDbCommand,
        ByVal ids As IEnumerable(Of TId),
        ByVal selector As Action(Of IDataProvider),
        ByVal query As String) _
        Implements ISqlHelper.SelectMultipleIds

        ' Deal with no IDs given first
        If ids Is Nothing Then Return
        If ids.Count() = 0 Then Return

        ' If the query doesn't include a placeholder, that's an error
        If query.IndexOf("{multiple-ids}") = -1 Then Throw New ArgumentException(
         "The query does not contain a {multiple-ids} placeholder")

        Dim parameters = command.Parameters

        ' Save the initial text of the command so that we can set it back later
        Dim initText As String = command.CommandText

        ' Save the initial parameters so that we can reinstate them after clearing
        ' the command's params after each iteration of the loop.
        Dim initialParameters(parameters.Count - 1) As IDbDataParameter
        For parameterIndex As Integer = 0 To parameters.Count - 1
            initialParameters(parameterIndex) = parameters(parameterIndex)
        Next

        ' Go through all the items, updating each 'window' of items in turn
        ' until all of the collection has been processed.
        Dim idEnumerator As IEnumerator(Of TId) = ids.GetEnumerator()
        ' The builder into which the query will be built up, starting from
        ' the initial query from the caller (up to "{multiple-ids}" ).
        Dim queryBuilder As New StringBuilder(
            query.Length + (6 * Math.Min(ids.Count(), MaxSqlParams)))

        While True
            ' Reset the stringbuilder (doesn't alter capacity / reallocate memory)
            queryBuilder.Length = 0

            ' Get the index within the query of the placeholder string
            Dim placeholderInd As Integer = query.IndexOf("{multiple-ids}")

            ' Append up to the placeholder into the query.
            queryBuilder.Append(query.Substring(0, placeholderInd))

            Dim parameterCount As Integer = 0
            While idEnumerator.MoveNext() AndAlso parameterCount <= MaxSqlParams
                If parameterCount > 0 Then queryBuilder.Append(","c)
                queryBuilder.AppendFormat("@id{0}", parameterCount)

                parameters.AddWithValue(command, "@id" & parameterCount, idEnumerator.Current)
                parameterCount += 1
                ' If we've reached our maximum number of params, exit the ID loop
                If parameterCount >= MaxSqlParams Then Exit While
            End While

            ' If we didn't add any IDs then we have nothing to process...
            If parameterCount = 0 Then Exit While

            ' Continue with the rest of the query (if there's any left)
            queryBuilder.Append(query.Substring(placeholderInd + "{multiple-ids}".Length))

            ' That's all, run the query.
            command.CommandText = queryBuilder.ToString()
            Using reader = connection.ExecuteReturnDataReader(command)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read() : selector(prov) : End While
            End Using

            ' Clear the params we've added and restore the initial parameters that
            ' the command had when it entered the method.
            parameters.Clear()
            For Each parameter In initialParameters
                parameters.Add(parameter)
            Next

        End While

        ' The params are already reset (within the while loop), reset the text too
        command.CommandText = initText

    End Sub
End Class
