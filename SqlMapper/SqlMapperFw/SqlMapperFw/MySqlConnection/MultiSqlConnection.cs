using System;
using System.Data;
using System.Data.SqlClient;

namespace SqlMapperFw.MySqlConnection
{
    public class MultiSqlConnection : AbstractMapperSqlConnection
    {
        public MultiSqlConnection(SqlConnectionStringBuilder connString)
        {
            Connection = new SqlConnection(connString.ConnectionString);
        }

        public override void Rollback()
        {
            if (SqlTransaction == null || !IsActiveConnection())
                throw new Exception("Cannot Rollback! Transaction doesn't have a active connection or is null!");

            SqlTransaction.Dispose();
            SqlTransaction.Rollback();
            SqlTransaction = null;
            CloseConnection();
        }

        public override void Commit()
        {
            if (SqlTransaction == null || !IsActiveConnection())
                throw new Exception("Cannot Commit! Transaction doesn't have a active connection or is null!");

            SqlTransaction.Commit();
            SqlTransaction = null;
        }

        protected internal override void BeforeCommandExecuted()
        {
            OpenConnection();
            BeginTransaction(IsolationLevel.ReadUncommitted);
        }

        internal override void AfterCommandExecuted()
        {
            Commit();
            CloseConnection();
        }
    }
}
