Imports System.IO

' Only expose this class if we are in the UNITTESTS configuration - here to ensure
' that we don't inadvertently leave a logging connection enabled in release code
' This was changed from DEBUG solely so that it is excluded from our Veracode builds
#If UNITTESTS Then

''' <summary>
''' A specialised DB Connection which can be used to log timings and queries
''' performed by the connection.
''' </summary>
Friend Class clsLoggingDBConnection : Inherits clsDBConnection

    ' The writer to which the logging is written
    Private mLogger As TextWriter

    ' The last command set up by the SetupCommand() method
    Private mLastCommand As String

    ''' <summary>
    ''' Creates a new connection, logging to stdout
    ''' </summary>
    ''' <param name="cons">The database connection setting to use</param>
    Public Sub New(ByVal cons As clsDBConnectionSetting)
        Me.New(cons, Console.Out)
    End Sub

    ''' <summary>
    ''' Creates a new connection, logging to stdout
    ''' </summary>
    ''' <param name="cons">The database connection setting to use</param>
    ''' <param name="filename">The name of the file to which the logging should be
    ''' written. The name can contain a string-formatting placeholder for a date/time
    ''' which will be formatted when the logger is instantiated. </param>
    Public Sub New(ByVal cons As clsDBConnectionSetting, ByVal filename As String)
        Me.New(cons, New StreamWriter( _
         New FileStream(String.Format(filename, Date.Now), FileMode.OpenOrCreate)) _
        )
    End Sub

    ''' <summary>
    ''' Creates a new connection, logging to the given logger
    ''' </summary>
    ''' <param name="cons">The onnection setting to use</param>
    ''' <param name="logger">The writer to which log entries should be written
    ''' </param>
    Public Sub New(ByVal cons As clsDBConnectionSetting, ByVal logger As TextWriter)
        MyBase.New(cons)
        mLogger = logger
    End Sub

    ''' <summary>
    ''' Disposes of this connection, ensuring that the logger is closed at the same
    ''' time.
    ''' </summary>
    ''' <param name="explicit">True if this connection is being disposed of
    ''' explicitly, false if it is being disposed of by the finalizer thread of the
    ''' garbage collector. The logger is left if it is being disposed of from within
    ''' the finalizer</param>
    Protected Overrides Sub Dispose(ByVal explicit As Boolean)
        MyBase.Dispose(explicit)
        ' Dispose of the logger it this is an explicit dispose and we're not
        ' logging to stdout / stderr
        If explicit AndAlso mLogger IsNot Nothing Then
            If mLogger IsNot Console.Out AndAlso mLogger IsNot Console.Error Then
                mLogger.Dispose()
            End If
        End If
        mLogger = Nothing
    End Sub

    ''' <summary>
    ''' Logs the given parameterised message
    ''' </summary>
    ''' <param name="msg">The message to log with formatting placeholders</param>
    ''' <param name="args">The arguments for the message</param>
    Private Sub Log(ByVal msg As String, ByVal ParamArray args() As Object)
        If mLogger IsNot Nothing Then mLogger.WriteLine("DBConnection: " & msg, args)
    End Sub

    ''' <summary>
    ''' Begins a transaction in the specified isolation level
    ''' </summary>
    ''' <param name="iso">The isolation level to use for the transaction</param>
    Public Overrides Sub BeginTransaction(ByVal iso As IsolationLevel)
        Log("BEGIN TRAN {0}", iso)
        Dim sw As Stopwatch = Stopwatch.StartNew()
        MyBase.BeginTransaction(iso)
        Log("-- BEGIN TRAN: {0}", sw.Elapsed)
        sw.Stop()
    End Sub

    ''' <summary>
    ''' Commits the currently open transaction
    ''' </summary>
    Public Overrides Sub CommitTransaction()
        Log("COMMIT TRAN")
        Dim sw As Stopwatch = Stopwatch.StartNew()
        MyBase.CommitTransaction()
        Log("-- COMMIT TRAN: {0}", sw.Elapsed)
        sw.Stop()
    End Sub

    ''' <summary>
    ''' Rolls back the currently open transaction
    ''' </summary>
    Public Overrides Sub RollbackTransaction()
        Log("ROLLBACK TRAN")
        Dim sw As Stopwatch = Stopwatch.StartNew()
        MyBase.RollbackTransaction()
        Log("-- ROLLBACK TRAN: {0}", sw.Elapsed)
        sw.Stop()
    End Sub

    ''' <summary>
    ''' Sets up the given SQL command, ensuring that the connection and, optionally,
    ''' the transaction is configured in it before it is executed.
    ''' </summary>
    ''' <param name="cmd">The SQL command to setup</param>
    ''' <returns>The command after it has been configured to the requirements of this
    ''' connection</returns>
    Protected Overrides Function SetupCommand(ByVal cmd As IDbCommand) As IDbCommand
        mLastCommand = cmd.CommandText
        Return MyBase.SetupCommand(cmd)
    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns
    ''' a SqlDataReader. This method wraps the ExecuteReader method of the SqlCommand.
    ''' Note:Always close the SqlDataReader after use.
    ''' </summary>
    ''' <param name="cmd">The SQL command to execute.</param>
    ''' <param name="behave">The command behaviour to use for this query.</param>
    ''' <returns>A SqlDataReader containing the query results.</returns>
    Public Overrides Function ExecuteReturnDataReader(
     ByVal cmd As IDbCommand, ByVal behave As CommandBehavior) As IDataReader
        Dim sw As New Stopwatch()
        Try
            Log("-- ExecuteReturnDataReader: {0}{1}", vbCrLf, mLastCommand)
            sw.Start()
            Return MyBase.ExecuteReturnDataReader(cmd, behave)
        Finally
            Log("-- ExecuteReturnDataReader(): {0}", sw.Elapsed)
            sw.Stop()
        End Try
    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns
    ''' a DataSet. Note:The DataSet can only be used to store the results from a
    ''' SELECT SQL statement.
    ''' </summary>
    ''' <param name="cmd">The SQL query to execute. The query must be a SELECT SQL
    ''' statement.</param>
    ''' <param name="table">The name of the source table to use for table mapping.
    ''' This corresponds to the name of the table in the returned DataSet.</param>
    ''' <param name="timeout">The timeout in seconds to wait for the
    ''' excecution to complete. If left unset then the timeout
    ''' for the connection will be used.</param>
    ''' <returns>A DataSet containing the query results.</returns>
    Public Overrides Function ExecuteReturnDataSet(ByVal cmd As IDbCommand,
     ByVal table As String, Optional ByVal timeout As Integer = -1) As DataSet
        Dim sw As New Stopwatch()
        Try
            Log("-- ExecuteReturnDataSet: {0}{1}", vbCrLf, mLastCommand)
            sw.Start()
            Return MyBase.ExecuteReturnDataSet(cmd, table, timeout)
        Finally
            Log("-- ExecuteReturnDataSet(): {0}", sw.Elapsed)
            sw.Stop()
        End Try
    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement and returns the first column of the first
    ''' row in the resultset returned by the query. Extra columns or rows are
    ''' ignored.  Typically used for queries that return a single value e.g. an
    ''' aggregate. This method wraps the ExecuteScalar method of the SqlCommand.
    ''' </summary>
    ''' <param name="cmd">The SQL Command to execute.</param>
    ''' <returns>An object containing the data from the first column of the first row
    ''' in the resultset returned from the query, or Nothing if there is no result
    ''' at all.</returns>
    Public Overrides Function ExecuteReturnScalar(ByVal cmd As IDbCommand) As Object
        Dim sw As New Stopwatch()
        Try
            Log("-- ExecuteReturnScalar: {0}{1}", vbCrLf, mLastCommand)
            sw.Start()
            Return MyBase.ExecuteReturnScalar(cmd)
        Finally
            Log("-- ExecuteReturnScalar(): {0}", sw.Elapsed)
            sw.Stop()
        End Try
    End Function

    ''' <summary>
    ''' Executes a Transact-SQL statement against the specified database and returns
    ''' the number of records affected. Use this method to execute commands
    ''' such as Transact-SQL INSERT, DELETE, UPDATE, AND SET statements.
    ''' This method wraps the ExecuteNonQuery method of the SqlCommand.
    ''' </summary>
    ''' <param name="cmd">The Transact-SQL statement to execute.</param>
    ''' <returns>An integer containing the number of records affected.</returns>
    Public Overrides Function ExecuteReturnRecordsAffected(
     ByVal cmd As IDbCommand) As Integer
        Dim sw As New Stopwatch()
        Try
            Log("-- ExecuteReturnRecordsAffected: {0}{1}", vbCrLf, mLastCommand)
            sw.Start()
            Return MyBase.ExecuteReturnRecordsAffected(cmd)
        Finally
            Log("-- ExecuteReturnRecordsAffected(): {0}", sw.Elapsed)
            sw.Stop()
        End Try
    End Function

End Class

#End If