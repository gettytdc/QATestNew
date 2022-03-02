namespace BluePrism.Data
{
    using System;
    using System.Data;

    public interface IDatabaseConnection : IDisposable
    {
        bool InTransaction { get; }

        void Open();
        void BeginTransaction();
        void BeginTransaction(IsolationLevel isolationLevel);
        void BeginTransaction(string name);
        void BeginTransaction(IsolationLevel isolationLevel, string name);
        void CommitTransaction();
        void RollbackTransaction();
        void Save(string name);
        void RollbackTo(string name);
        void Close();
        void Execute(IDbCommand command);
        IDbCommand SetupCommand(IDbCommand command);
        void ReportError(IDbCommand command, Exception ex);
        int ExecuteReturnRecordsAffected(IDbCommand command);
        DataSet ExecuteReturnDataSet(IDbCommand command, string table, int timeout = -1);
        IDataReader ExecuteReturnDataReader(IDbCommand command);
        IDataReader ExecuteReturnDataReader(IDbCommand command, CommandBehavior behavior);
        object ExecuteReturnScalar(IDbCommand command);
        TResult ExecuteReturnScalar<TResult>(IDbCommand command);
        object ExecuteScalarExpectError(IDbCommand command, int errorNumber);
        DataTable ExecuteReturnDataTable(IDbCommand command, int timeout = -1);
        DataTable ExecuteReturnDataTable(IDbCommand command, string table);
        DataSet ExecuteReturnDataSet(IDbCommand command);
        int UpdateDataAdapter(DataTable dataTable);
        int UpdateDataAdapter(DataTable dataTable, IDbCommand command);
        DatabaseServer GetDatabaseVersion();
        void LoadDataTable(IDbCommand command, DataTable dataTable);
        string GetDatabaseName();
        int InsertDataTable(DataTable table, string destinationTableName);
        void ResetConnectionDefaultIsolationLevel();
    }
}