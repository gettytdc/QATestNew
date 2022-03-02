namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Data;

    public class MockDatabaseConnection : IDatabaseConnection
    {
        private readonly IDbConnection _connection;

        public MockDatabaseConnection(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Open() => _connection.Open();

        public void BeginTransaction() => _connection.BeginTransaction();

        public void BeginTransaction(string name) => _connection.BeginTransaction();

        public void CommitTransaction() { }

        public void RollbackTransaction() { }

        public void Save(string name) { }

        public void RollbackTo(string name) { }

        public void Close() => _connection.Close();

        public DatabaseServer GetDatabaseVersion() => DatabaseServer.SqlServer2019;

        public string GetDatabaseName() => "MockedDb";

        public void ResetConnectionDefaultIsolationLevel() { }

        public bool InTransaction => false;

        public int InsertDataTable(DataTable table, string destinationTableName) => throw new NotImplementedException();

        public void LoadDataTable(IDbCommand command, DataTable dataTable) => throw new NotImplementedException();

        public int UpdateDataAdapter(DataTable dataTable, IDbCommand command) => throw new NotImplementedException();

        public int UpdateDataAdapter(DataTable dataTable) => throw new NotImplementedException();

        public DataSet ExecuteReturnDataSet(IDbCommand command) => throw new NotImplementedException();

        public DataTable ExecuteReturnDataTable(IDbCommand command, string table) => throw new NotImplementedException();

        public DataTable ExecuteReturnDataTable(IDbCommand command, int timeout = -1)
        {
            var dataTable = new DataTable();
            var reader = SetupCommand(command).ExecuteReader();

            if (!reader.Read())
                return dataTable;

            dataTable.Columns.AddRange(Enumerable.Range(0, reader.FieldCount).Select(x => (Name: reader.GetName(x), Type: reader.GetFieldType(x))).Select(x => new DataColumn(x.Name, x.Type)).ToArray());
            do
            {
                dataTable.Rows.Add(Enumerable.Range(0, reader.FieldCount).Select(reader.GetValue).ToArray());
            } while (reader.Read());

            return dataTable;
        }

        public object ExecuteScalarExpectError(IDbCommand command, int errorNumber) => throw new NotImplementedException();

        public TResult ExecuteReturnScalar<TResult>(IDbCommand command) => (TResult)SetupCommand(command).ExecuteScalar();

        public object ExecuteReturnScalar(IDbCommand command) => SetupCommand(command).ExecuteScalar();

        public IDataReader ExecuteReturnDataReader(IDbCommand command, CommandBehavior behavior) => SetupCommand(command).ExecuteReader(behavior);

        public IDataReader ExecuteReturnDataReader(IDbCommand command) => SetupCommand(command).ExecuteReader();

        public DataSet ExecuteReturnDataSet(IDbCommand command, string table, int timeout = -1) => throw new NotImplementedException();

        public int ExecuteReturnRecordsAffected(IDbCommand command) => (int)SetupCommand(command).ExecuteScalar();

        public void ReportError(IDbCommand command, Exception ex) => throw new NotImplementedException();

        public IDbCommand SetupCommand(IDbCommand command)
        {
            Open();
            if (command is SqlCommand)
            {
                var newCommand = _connection.CreateCommand();
                newCommand.CommandText = command.CommandText;
                foreach (var parameter in command.Parameters.OfType<IDataParameter>())
                {
                    newCommand.Parameters.AddWithValue(newCommand, parameter.ParameterName, parameter.Value);
                }

                command = newCommand;
            }

            command.Connection = _connection;
            return command;
        }

        public void Execute(IDbCommand command) => SetupCommand(command).ExecuteNonQuery();

        public void BeginTransaction(IsolationLevel isolationLevel, string name) =>
            _connection.BeginTransaction(isolationLevel);

        public void BeginTransaction(IsolationLevel isolationLevel) => _connection.BeginTransaction(isolationLevel);

        public void Dispose() => _connection.Dispose();

    }
}
