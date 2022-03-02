
Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports System.Globalization
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports NLog

''' Project  : Automate
''' Class    : clsDBConnection
''' 
''' <summary>
''' A database interface class providing the data layer access to Automate.
''' </summary>
Public Class clsDBConnection
    Implements IDisposable, IDatabaseConnection

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger() 

    ''' <summary>
    ''' The maximum number of retries performed if a command is made a deadlock
    ''' victim on execution.
    ''' This can only have an effect if we're not in a transaction, since we don't
    ''' have the necessary context to know whether other statements are required to
    ''' be run as part of the resubmission of the transaction.
    ''' </summary>
    Private Const MaxDeadlockRetries As Integer = 10

    Private mDisposed As Boolean
    Private mConnection As SqlConnection
    Private mTransaction As SqlTransaction
    Private mDataAdapter As SqlDataAdapter

#Region " Constructors / Destructors / Dispose methods "

    ''' <summary>
    ''' Constructor - get a new connection.
    ''' </summary>
    ''' <param name="cons">The database connection setting to use</param>
    Public Sub New(ByVal cons As IDatabaseConnectionSetting)
        mConnection = cons.CreateSqlConnection()
        mDisposed = False
    End Sub

    ''' <summary>
    ''' Finalizes this connection, ensuring it is (implicitly) disposed of
    ''' </summary>
    Protected Overrides Sub Finalize()
        Dispose(False)
    End Sub

    ''' <summary>
    ''' Disposes of this DB connection - in practice, this just closes the connection
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Disposes of this DB connection, including the managed objects as specified
    ''' </summary>
    Protected Overridable Sub Dispose(ByVal explicit As Boolean)
        If Not mDisposed Then
            Try
                Close()
            Catch ' Ignore any errors - Dispose() shouldn't throw exceptions
            End Try
        End If
        mDisposed = True
    End Sub

#End Region

    ''' <summary>
    ''' Open the connection if it isn't already open.
    ''' </summary>
    Private Sub Open() Implements IDatabaseConnection.Open
        If mConnection.State = ConnectionState.Closed Then mConnection.Open()
    End Sub

    ''' <summary>
    ''' Determines if a transaction is currently in progress - i.e. BeginTransaction
    ''' has been called, but either CommitTransaction or RollbackTransaction has not.
    ''' </summary>
    Public ReadOnly Property InTransaction() As Boolean Implements IDatabaseConnection.InTransaction
        Get
            Return mTransaction IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Begins a transaction with default transaction isolation level, opening the
    ''' underlying connection if it is not already open.
    ''' </summary>
    Public Overridable Sub BeginTransaction() Implements IDatabaseConnection.BeginTransaction
        BeginTransaction(IsolationLevel.Unspecified, Nothing)
    End Sub
    ''' <summary>
    ''' Begins a transaction, opening the underlying connection if it's not already
    ''' open. The transaction created will operate at the given isolation level or
    ''' the default level if <see cref="IsolationLevel.Unspecified"/> is given.
    ''' </summary>
    ''' <param name="iso">The isolation level required for the transaction which is
    ''' initiated by this method. <see cref="IsolationLevel.Unspecified"/> to use
    ''' the default isolation level (which is, at the time of going to press,
    ''' <see cref="IsolationLevel.ReadCommitted"/>).</param>
    Public Overridable Sub BeginTransaction(iso As IsolationLevel) Implements IDatabaseConnection.BeginTransaction
        BeginTransaction(iso, Nothing)
    End Sub

    ''' <summary>
    ''' Begins a transaction, opening the underlying connection if it's not already
    ''' open. The transaction created will operate at the default isolation level.
    ''' </summary>
    ''' <param name="name">The name to given the transaction, or null to given it
    ''' no defined name.</param>
    Public Overridable Sub BeginTransaction(name As String) Implements IDatabaseConnection.BeginTransaction
        BeginTransaction(IsolationLevel.Unspecified, name)
    End Sub

    ''' <summary>
    ''' Begins a transaction, opening the underlying connection if it's not already
    ''' open. The transaction created will operate at the given isolation level or
    ''' the default level if <see cref="IsolationLevel.Unspecified"/> is given.
    ''' </summary>
    ''' <param name="iso">The isolation level required for the transaction which is
    ''' initiated by this method. <see cref="IsolationLevel.Unspecified"/> to use
    ''' the default isolation level (which is, at the time of going to press,
    ''' <see cref="IsolationLevel.ReadCommitted"/>).</param>
    ''' <param name="name">The name to given the transaction, or null to given it
    ''' no defined name.</param>
    Public Overridable Sub BeginTransaction(iso As IsolationLevel, name As String) Implements IDatabaseConnection.BeginTransaction
        Open()
        mTransaction = mConnection.BeginTransaction(iso, name)
    End Sub

    ''' <summary>
    ''' Commits the transaction.
    ''' </summary>
    Public Overridable Sub CommitTransaction() Implements IDatabaseConnection.CommitTransaction
        Try
            If InTransaction Then mTransaction.Commit()
        Finally
            mTransaction = Nothing
        End Try
    End Sub

    ''' <summary>
    ''' Rolls back the transaction.
    ''' </summary>
    Public Overridable Sub RollbackTransaction() Implements IDatabaseConnection.RollbackTransaction
        Try
            If InTransaction Then mTransaction.Rollback()
        Finally
            mTransaction = Nothing
        End Try
    End Sub

    ''' <summary>
    ''' Adds a 'savepoint' in the current transaction, providing a place where the
    ''' work can be rolled back to if necessary.
    ''' </summary>
    ''' <param name="name">The name to provide to the savepoint</param>
    ''' <exception cref="InvalidStateException">If this connection does not currently
    ''' <see cref="InTransaction">have an active transaction</see></exception>
    ''' <exception cref="ArgumentNullException">If <paramref name="name"/> is null.
    ''' </exception>
    ''' <exception cref="ArgumentException">If <paramref name="name"/> is empty.
    ''' </exception>
    Public Overridable Sub Save(name As String) Implements IDatabaseConnection.Save
        If Not InTransaction Then Throw New InvalidStateException(
            "Cannot save a savepoint when not inside a transaction")
        If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))
        name = name.Trim()
        If name = "" Then Throw New ArgumentException(
            "A savepoint name must be provided", NameOf(name))
        mTransaction.Save(name)
    End Sub

    ''' <summary>
    ''' Rolls back the current transaction to the specified save point.
    ''' </summary>
    ''' <param name="name">The name of the save point to roll back to</param>
    ''' <exception cref="InvalidStateException">If this connection does not currently
    ''' <see cref="InTransaction">have an active transaction</see></exception>
    ''' <exception cref="ArgumentNullException">If <paramref name="name"/> is null.
    ''' </exception>
    ''' <exception cref="ArgumentException">If <paramref name="name"/> is empty.
    ''' </exception>
    Public Overridable Sub RollbackTo(name As String) Implements IDatabaseConnection.RollbackTo
        If Not InTransaction Then Throw New InvalidStateException(
            "Cannot roll back to a savepoint when not inside a transaction")
        If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))
        name = name.Trim()
        If name = "" Then Throw New ArgumentException(
            "No savepoint name provided to rollback to", NameOf(name))

        mTransaction.Rollback(name)
    End Sub

    ''' <summary>
    ''' Closes the connection.
    ''' </summary>
    Public Overridable Sub Close() Implements IDatabaseConnection.Close

        ' Any data adapters? Get rid
        If mDataAdapter IsNot Nothing Then mDataAdapter.Dispose()
        mDataAdapter = Nothing

        ' If there's a transaction here, dispose of it. This will ensure that
        ' the transaction is rolled back if necessary.
        If mTransaction IsNot Nothing Then mTransaction.Dispose()
        mTransaction = Nothing

        ' Finally the connection proper.
        If mConnection IsNot Nothing Then mConnection.Dispose()
        mConnection = Nothing
    End Sub

    ''' <summary>
    ''' Executes an SQL query.
    ''' </summary>
    ''' <param name="cmd">The query</param>
    Public Overridable Sub Execute(ByVal cmd As IDbCommand) Implements IDatabaseConnection.Execute
        ExecuteReturnRecordsAffected(cmd)
    End Sub

    ''' <summary>
    ''' Sets up the given SQL Command with the state in this connection object
    ''' and returns it.
    ''' After returning from this call, the connection is open and assigned to the
    ''' returned command object, and any current transaction is set into it.
    ''' </summary>
    ''' <param name="cmd">The command to setup</param>
    ''' <returns>The SQL Command, set up with the parameters in this connection.
    ''' </returns>
    Protected Overridable Function SetupCommand(ByVal cmd As IDbCommand) As IDbCommand Implements IDatabaseConnection.SetupCommand
        Open()
        cmd.Connection = mConnection

        ' Change the default timeout from 30s to that set in the config. If the
        ' caller has already changed the default, leave it as it is.
        If cmd.CommandTimeout = 30 Then _
         cmd.CommandTimeout = Options.Instance.SqlCommandTimeout

        ' If the transaction object has been instantiated then BeginTransaction() 
        ' has been called. As such, the command should be made part of the transaction.
        If mTransaction IsNot Nothing Then cmd.Transaction = mTransaction
        Return cmd
    End Function

    ''' <summary>
    ''' Reports an error which occurred for a particular command
    ''' </summary>
    ''' <param name="cmd">The command for which the error occurred</param>
    ''' <param name="ex">The exception which was thrown</param>
    Protected Overridable Sub ReportError(
     ByVal cmd As IDbCommand, ByVal ex As Exception) Implements IDatabaseConnection.ReportError
        ' We log stack trace separately because by default an exception's stack
        ' trace winds back to the context of the try block in which it was caught
        ' and not beyond, which is generally not much use to us for DB errors.
        Log.Error(ex,
         "A database error occurred while executing the statement:{0}" &
         "{1}{0}{0}" &
         "Error Details: {2}: {3}",
         vbCrLf, cmd.CommandText, ex.GetType(), New StackTrace(0, True))
    End Sub

#Region " Actual Execute Callers "
    ' These methods all actually execute SQL on the server... separated into a
    ' region so the direct callers are easy to find.
    ' Other 'Execute' methods delegate their work to these methods.

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns
    ''' the number of records affected. Use this method to execute commands 
    ''' such as Transact-SQL INSERT, DELETE, UPDATE, AND SET statements.
    ''' This method wraps the ExecuteNonQuery method of the SqlCommand. 
    ''' </summary>
    ''' <param name="cmd">The Transact-SQL statement to execute.</param>
    ''' <returns>An integer containing the number of records affected.</returns>
    Public Overridable Function ExecuteReturnRecordsAffected(ByVal cmd As IDbCommand) As Integer Implements IDatabaseConnection.ExecuteReturnRecordsAffected
        Dim deadlockCount As Integer = 0
        While True
            Try
                Return SetupCommand(cmd).ExecuteNonQuery()
            Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.DeadlockVictim _
                OrElse sqle.Number = DatabaseErrorCode.UpdateConflict
                ' If there's a wider transaction, then we don't have the scope to
                ' retry a command - there may be other commands which need to be
                ' resent too. Just throw the exception and let the caller deal
                If mTransaction IsNot Nothing Then Throw

                ' Increment the count of deadlocks - if we've run over the max
                ' then pass on the exception
                deadlockCount += 1
                If deadlockCount > MaxDeadlockRetries Then Throw
            End Try
        End While
        ' It really can't get here.. no, really
        Throw New InvalidOperationException()
    End Function


    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns a DataSet. 
    ''' Note:The DataSet can only be used to store the results from a SELECT SQL statement.
    ''' </summary>
    ''' <param name="cmd">The SQL query to execute. The query must be a SELECT SQL statement.</param>
    ''' <param name="table">The name of the source table to use for table mapping. This corresponds
    ''' to the name of the table in the returned DataSet.</param>
    ''' <param name="timeout">The timeout in seconds to wait for the
    ''' excecution to complete. If left unset then the timeout
    ''' for the connection will be used.</param>
    ''' <returns>A DataSet containing the query results.</returns>
    Public Overridable Function ExecuteReturnDataSet(
     ByVal cmd As IDbCommand, ByVal table As String, Optional ByVal timeout As Integer = -1) As DataSet Implements IDatabaseConnection.ExecuteReturnDataSet

        cmd = SetupCommand(cmd)

        If timeout <> -1 Then cmd.CommandTimeout = timeout

        Dim deadlockCount As Integer = 0
        Try
            While True
                Try
                    mDataAdapter = New SqlDataAdapter()
                    mDataAdapter.SelectCommand = DirectCast(cmd, SqlCommand)
                    Dim ds As New DataSet()
                    ds.Locale = CultureInfo.InvariantCulture
                    'Fill and return the data set.
                    If table = "" Then
                        mDataAdapter.Fill(ds)
                    Else
                        mDataAdapter.Fill(ds, table)
                    End If
                    SetUnspecifiedDateTime(ds)
                    Return ds

                Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.DeadlockVictim _
                    OrElse sqle.Number = DatabaseErrorCode.UpdateConflict
                    ' If there's a wider transaction, then we don't have the scope to
                    ' retry a command - there may be other commands which need to be
                    ' re-sent too. Just throw the exception and let the caller deal
                    If mTransaction IsNot Nothing Then Throw

                    ' Increment the count of deadlocks - if we've run over the max
                    ' then pass on the exception
                    deadlockCount += 1
                    If deadlockCount > MaxDeadlockRetries Then Throw
                End Try
            End While

            ' It should never actually get to this point
            Throw New UnreachableException()

        Catch ex As Exception
            ReportError(cmd, ex)
            Throw

        End Try

    End Function

    ''' <summary>
    ''' Loads data into a datatable, and ensures any datetime data is
    ''' set to unspecified (used for incremental loading of data)
    ''' </summary>
    ''' <param name="cmd">The sql command used to load the data</param>
    ''' <param name="dt">The data table to load the data into</param>
    ''' <remarks></remarks>
    Public Sub LoadDataTable(cmd As IDbCommand, dt As DataTable) Implements IDatabaseConnection.LoadDataTable
        dt.Load(ExecuteReturnDataReader(cmd))
        SetUnspecifiedDateTime(dt)
    End Sub

    ''' <summary>
    ''' Sets the DataSet to use unspecified for all datetime columns
    ''' </summary>
    ''' <param name="source">The dataset change</param>
    Private Shared Sub SetUnspecifiedDateTime(source As DataSet)
        For Each dt As DataTable In source.Tables
            SetUnspecifiedDateTime(dt)
        Next
    End Sub

    ''' <summary>
    '''  Sets the DataTable to use unspecified for all datetime columns
    ''' </summary>
    ''' <param name="source">The datatable change</param>
    Friend Shared Sub SetUnspecifiedDateTime(source As DataTable)
        For Each col As DataColumn In source.Columns
            If col.DataType = GetType(DateTime) Then
                col.DateTimeMode = DataSetDateTime.Unspecified
            End If
        Next
    End Sub


    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns
    ''' a SqlDataReader. This method wraps the ExecuteReader method of the SqlCommand.
    ''' Note:Always close the SqlDataReader after use.
    ''' </summary>
    ''' <param name="cmd">The SQL command to execute.</param>
    ''' <param name="behave">The command behaviour to use for this query.</param>
    ''' <returns>A SqlDataReader containing the query results.</returns>
    Public Overridable Function ExecuteReturnDataReader(
     ByVal cmd As IDbCommand, ByVal behave As CommandBehavior) As IDataReader Implements IDatabaseConnection.ExecuteReturnDataReader

        Dim deadlockCount As Integer = 0
        Try
            While True
                Try
                    Return SetupCommand(cmd).ExecuteReader(behave)
                Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.DeadlockVictim _
                    OrElse sqle.Number = DatabaseErrorCode.UpdateConflict
                    ' If there's a wider transaction, then we don't have the scope to
                    ' retry a command - there may be other commands which need to be
                    ' resent too. Just throw the exception and let the caller deal
                    If mTransaction IsNot Nothing Then Throw

                    ' Increment the count of deadlocks - if we've run over the max
                    ' then pass on the exception
                    deadlockCount += 1
                    If deadlockCount > MaxDeadlockRetries Then Throw
                End Try
            End While

            ' It should never actually get to this point
            Throw New UnreachableException()

        Catch ex As Exception
            ReportError(cmd, ex)
            Throw

        End Try

    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement and returns the first column of the first
    ''' row in the resultset returned by the query. Extra columns or rows are ignored. 
    ''' Typically used for queries that return a single value e.g. an aggregate.
    ''' This method wraps the ExecuteScalar method of the SqlCommand.
    ''' </summary>
    ''' <param name="cmd">The SQL Command to execute.</param>
    ''' <returns>An object containing the data from the first column of the first row 
    ''' in the resultset returned from the query, or Nothing if there is no result
    ''' at all.</returns>
    Public Overridable Function ExecuteReturnScalar(ByVal cmd As IDbCommand) As Object Implements IDatabaseConnection.ExecuteReturnScalar
        Try
            Dim deadlockCount As Integer = 0
            While True
                Try
                    Return SetupCommand(cmd).ExecuteScalar()
                Catch sqle As SqlException When sqle.Number = DatabaseErrorCode.DeadlockVictim _
                    OrElse sqle.Number = DatabaseErrorCode.UpdateConflict
                    ' If there's a wider transaction, then we don't have the scope to
                    ' retry a command - there may be other commands which need to be
                    ' resent too. Just throw the exception and let the caller deal
                    If mTransaction IsNot Nothing Then Throw

                    ' Increment the count of deadlocks - if we've run over the max
                    ' then pass on the exception
                    deadlockCount += 1
                    If deadlockCount > MaxDeadlockRetries Then Throw
                End Try
            End While
            ' It should never actually get to this point
            Throw New UnreachableException()
        Catch ex As Exception
            ReportError(cmd, ex)
            Throw
        End Try
    End Function

    Public Overridable Function ExecuteReturnScalar(Of TResult)(command As IDbCommand) As TResult Implements IDatabaseConnection.ExecuteReturnScalar
        Return DirectCast(ExecuteReturnScalar(command), TResult)
    End Function

    Public Overridable Function ExecuteScalarExpectError(command As IDbCommand, errorNumber As Integer) As Object Implements IDatabaseConnection.ExecuteScalarExpectError
        Try
            Return SetupCommand(command).ExecuteScalar()
        Catch sqlException As SqlException When sqlException.Number = errorNumber
            Return Nothing
        End Try
    End Function

#End Region

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns
    ''' a DataTable. 
    ''' Note:The DataSet can only be used to store the results from a SELECT SQL statement.
    ''' </summary>
    ''' <param name="cmd">The SQL query to execute. The query must be a SELECT SQL statement.</param>
    ''' <param name="TimeOut">The timeout in seconds to wait for the excecution to complete. 
    ''' If left unset then the timeout for the connection will be used.</param>
    ''' <returns>A DataTable containing the query results.</returns>
    Public Function ExecuteReturnDataTable(ByVal cmd As IDbCommand, Optional ByVal timeout As Integer = -1) As DataTable Implements IDatabaseConnection.ExecuteReturnDataTable
        Return ExecuteReturnDataSet(cmd, "", timeout).Tables(0)
    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns a DataTable. 
    ''' Note:The DataSet can only be used to store the results from a SELECT SQL statement.
    ''' </summary>
    ''' <param name="cmd">The SQL query to execute. The query must be a SELECT SQL statement.</param>
    ''' <param name="table">The name of the source table to use for table mapping.</param>
    ''' <returns>A DataTable containing the query results.</returns>
    Public Function ExecuteReturnDataTable(ByVal cmd As IDbCommand, ByVal table As String) As DataTable Implements IDatabaseConnection.ExecuteReturnDataTable
        Return ExecuteReturnDataSet(cmd, table).Tables(table)
    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns a DataSet. 
    ''' Note:The DataSet can only be used to store the results from a SELECT SQL statement.
    ''' </summary>
    ''' <param name="cmd">The SQL query to execute. The query must be a SELECT SQL statement.</param>
    ''' <returns>A DataSet containing the query results.</returns>
    Public Function ExecuteReturnDataSet(ByVal cmd As IDbCommand) As DataSet Implements IDatabaseConnection.ExecuteReturnDataSet
        Return ExecuteReturnDataSet(cmd, "")
    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns
    ''' a SqlDataReader. This method wraps the ExecuteReader method of the SqlCommand.
    ''' Note:Always close the SqlDataReader after use.
    ''' </summary>
    ''' <param name="cmd">The SQL command to execute.</param>
    ''' <returns>A SqlDataReader containing the query results.</returns>
    Public Function ExecuteReturnDataReader(ByVal cmd As IDbCommand) As IDataReader Implements IDatabaseConnection.ExecuteReturnDataReader
        Return ExecuteReturnDataReader(cmd, CommandBehavior.Default)
    End Function

    ''' <summary>
    ''' Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, 
    ''' updated, or deleted row in the specified DataSet from a DataTable named "Table".
    ''' </summary>
    ''' <param name="dt">The DataTable</param>
    ''' <returns>The number of rows successfully updated from the DataSet.</returns>
    Public Function UpdateDataAdapter(ByVal dt As DataTable) As Integer Implements IDatabaseConnection.UpdateDataAdapter
        If mDataAdapter Is Nothing Or mTransaction Is Nothing Then
            Return 0
        Else
            Return mDataAdapter.Update(dt)
        End If
    End Function

    Public Function UpdateDataAdapter(ByVal dt As DataTable, ByVal updCommand As IDbCommand) As Integer Implements IDatabaseConnection.UpdateDataAdapter
        If mDataAdapter Is Nothing Then
            Return 0
        Else
            SetupCommand(updCommand)
            mDataAdapter.UpdateCommand = DirectCast(updCommand, SqlCommand)
            mDataAdapter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None
            mDataAdapter.UpdateBatchSize = dt.Rows.Count
            Return mDataAdapter.Update(dt)
        End If
    End Function

    ''' <summary>
    ''' Utility method to convert a system date to an SQL Date, ensuring that any
    ''' boundary values are converted correctly. This will never return a null
    ''' SQL DateTime value.
    ''' </summary>
    ''' <param name="dt">The date to convert</param>
    ''' <returns>The SQL Date value corresponding to the given system date value.
    ''' </returns>
    Friend Shared Function UtilDateToSqlDate(ByVal dt As Date) As SqlDateTime
        Return UtilDateToSqlDate(dt, False, False)
    End Function

    ''' <summary>
    ''' Utility method to convert a system date to an SQL Date, ensuring that any
    ''' boundary values are converted correctly.
    ''' </summary>
    ''' <param name="dt">The date to convert</param>
    ''' <param name="treatMinAsNull">True to return a null Sql Date value for
    ''' Date.MinValue</param>
    ''' <param name="treatMaxAsNull">True to return a null Sql Date value for
    ''' Date.MaxValue</param>
    ''' <returns>The SQL Date value corresponding to the given system date value.
    ''' </returns>
    Friend Shared Function UtilDateToSqlDate( _
     ByVal dt As Date, ByVal treatMinAsNull As Boolean, ByVal treatMaxAsNull As Boolean) _
     As SqlDateTime

        ' If we're hitting the SQL DateTime bounds, assume the system bounds
        ' in order to make the following checks more consistent
        If dt < SqlDateTime.MinValue.Value Then dt = Date.MinValue
        If dt > SqlDateTime.MaxValue.Value Then dt = Date.MaxValue

        If dt = Date.MinValue Then
            If treatMinAsNull Then Return SqlDateTime.Null Else Return SqlDateTime.MinValue
        End If
        If dt = Date.MaxValue Then
            If treatMaxAsNull Then Return SqlDateTime.Null Else Return SqlDateTime.MaxValue
        End If

        Return New SqlDateTime(dt)
    End Function

    ''' <summary>
    ''' Converts the given SQL Date value to a system Date value.
    ''' </summary>
    ''' <param name="sdt">The SQL date value to convert</param>
    ''' <param name="nullValue">The value to use for a NULL SQL Date value
    ''' </param>
    ''' <returns>The system date value corresponding to the given SQL Date value.
    ''' </returns>
    Friend Shared Function UtilSqlDateToDate(ByVal sdt As SqlDateTime, ByVal nullValue As Date) As Date

        If sdt.IsNull Then Return nullValue
        If sdt = SqlDateTime.MaxValue Then Return Date.MaxValue
        If sdt = SqlDateTime.MinValue Then Return Date.MinValue
        Return sdt.Value

    End Function

    ''' <summary>
    ''' Gets the database version of the database that this connection connects to.
    ''' The connection must be opened for this information to be retrieved.
    ''' </summary>
    Public Function GetDatabaseVersion() As DatabaseServer Implements IDatabaseConnection.GetDatabaseVersion

        ' Regular expression for the server version on SqlConnection, from format taken from :
        ' http://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.serverversion.aspx
        Static connRegex As New Regex( _
         "^(?<major>\d+)\.(?<minor>\d+)\.(?<release>\d+)$", RegexOptions.Compiled)

        Try

            Open()

            Dim svr As DatabaseServer = DirectCast(Integer.Parse( _
             connRegex.Match(mConnection.ServerVersion).Groups("major").ToString()),  _
             DatabaseServer)
            Return svr

        Catch ' Just return 'Unknown if there are any errors either opening or parsing.
        End Try

        Return DatabaseServer.Unknown

    End Function

    Public Function GetDatabaseName() As String Implements IDatabaseConnection.GetDatabaseName
        Return Me.mConnection.Database
    End Function

    Public Function InsertDataTable(table As DataTable, destinationTableName As String) As Integer Implements IDatabaseConnection.InsertDataTable
        Using command = New SqlCommand($"SELECT COUNT(*) FROM {destinationTableName}")
            Dim startRows = CInt(ExecuteReturnScalar(command))
            Using bulkCopy = New SqlBulkCopy(mConnection, SqlBulkCopyOptions.CheckConstraints, mTransaction)
                bulkCopy.DestinationTableName = destinationTableName
                AutoMapToColumnsWithSameName(bulkCopy, table)
                bulkCopy.WriteToServer(table)
            End Using

            Dim rowsNow = CInt(ExecuteReturnScalar(command))
            Dim rowsInserted = rowsNow - startRows
            If rowsInserted < table.Rows.Count Then
                Throw New BluePrismException(
                            My.Resources.clsServer_NotAllRowsCorrectlyInserted_Template,
                            destinationTableName)
            End If

            Return rowsInserted
        End Using
    End Function

    Public Sub AutoMapToColumnsWithSameName(bulkCopy As SqlBulkCopy, table As DataTable)
        For Each column As DataColumn In table.Columns
            bulkCopy.ColumnMappings.Add(
                           New SqlBulkCopyColumnMapping(
                               column.ColumnName, column.ColumnName))
        Next
    End Sub

    ''' <summary>
    ''' Restores Read committed status on a connection before it is returned to the pool
    ''' this is to deal with a situation that can arise in SQL Server pre 2014 where 
    ''' the default level is not restored in sp_reset_connection.
    ''' see https://docs.microsoft.com/pl-pl/SQL/database-engine/breaking-changes-to-database-engine-features-in-sql-server-2016?view=sql-server-2014#isolation-level-and-sp_reset_connection
    ''' </summary>
    Public Sub ResetConnectionDefaultIsolationLevel() Implements IDatabaseConnection.ResetConnectionDefaultIsolationLevel
        Using t = mConnection.BeginTransaction(IsolationLevel.ReadCommitted)
            t.Commit()
        End Using
    End Sub
End Class
