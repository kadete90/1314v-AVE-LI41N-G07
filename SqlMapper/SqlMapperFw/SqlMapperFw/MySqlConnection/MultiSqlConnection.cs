using System;
using System.Data.SqlClient;

namespace SqlMapperFw.MySqlConnection
{
    public class MultiSqlConnection : AbstractSqlConnection
    {
        public MultiSqlConnection(SqlConnectionStringBuilder connString)
        {
            Connection = new SqlConnection(connString.ConnectionString);
        }

        public override void Rollback()
        {
            if (SqlTransaction == null || !IsActiveConnection())
            {
                Console.WriteLine("Cannot Rollback! Transaction doesn't have a active connection or is null!");
                return;
            }
            SqlTransaction.Dispose();
            SqlTransaction.Rollback();
            SqlTransaction = null;
            CloseConnection();
        }

        public override void Commit()
        {
            if (SqlTransaction == null || !IsActiveConnection())
            {
                Console.WriteLine("Cannot Rollback! Transaction doesn't have a active connection or is null!");
                return;
            }

            SqlTransaction.Commit();
            SqlTransaction = null;
        }

        protected internal override void BeforeCommandExecuted()
        {
            OpenConnection();
            BeginTransaction();
        }

        internal override void AfterCommandExecuted()
        {
            Commit();
            CloseConnection();
        }
    }
}
