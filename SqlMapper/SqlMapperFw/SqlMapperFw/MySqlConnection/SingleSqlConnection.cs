using System;
using System.Data.SqlClient;

namespace SqlMapperFw.MySqlConnection
{
    public class SingleSqlConnection : AbstractSqlConnection
    {
        public SingleSqlConnection(SqlConnectionStringBuilder connString)
        {
            Connection = new SqlConnection(connString.ConnectionString);
            OpenConnection();
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
        }

        public override void Commit()
        {
            if (SqlTransaction == null || !IsActiveConnection())
            {
                Console.WriteLine("Cannot Commit! Transaction doesn't have a active connection or is null!");
                return;
            }
            SqlTransaction.Commit();
            SqlTransaction = null;
        }

        protected internal override void BeforeCommandExecuted()
        {
        }

        internal override void AfterCommandExecuted()
        {
        }
    }
}
