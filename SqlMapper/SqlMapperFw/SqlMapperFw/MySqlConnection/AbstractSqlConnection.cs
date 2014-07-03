using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlMapperFw.MySqlConnection
{
    public abstract class AbstractSqlConnection : IMySqlConnection
    {
        internal SqlConnection Connection { get; set; }
        internal SqlTransaction SqlTransaction;

        public abstract void Commit();
        public abstract void Rollback();
        internal abstract void AfterCommandExecuted();
        protected internal abstract void BeforeCommandExecuted();
       
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (SqlTransaction != null) throw new InvalidOperationException("Transaction already initialized!");
            SqlTransaction = Connection.BeginTransaction(isolationLevel);
        }

        public bool IsActiveConnection()
        {
            return Connection.State == ConnectionState.Open;
        }

        public void OpenConnection()
        {
            if (IsActiveConnection())
                return;
            Connection.Open();
            if (!IsActiveConnection())
            {
                throw new Exception("Could not open a new connection!");
            }
        }

        public void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }
    }
}
