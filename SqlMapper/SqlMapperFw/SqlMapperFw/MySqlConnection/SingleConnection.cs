using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.MySqlConnection
{
    public class SingleConnection<T> : AbstractMapperSqlConnection<T>
    {
        public SingleConnection(SqlConnectionStringBuilder connString, IEnumerable<Type> bindMembers)
        {
            Connection = new SqlConnection(connString.ConnectionString);
            MyCmdBuilder = new CmdBuilderDataMapper<T>(this, bindMembers);
            OpenConnection();
        }

        public override void Rollback()
        {
            if (SqlTransaction == null || !ActiveConnection())
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
            if (SqlTransaction == null || !ActiveConnection())
            {
                Console.WriteLine("Cannot Commit! Transaction doesn't have a active connection or is null!");
                return;
            }
            SqlTransaction.Commit();
            SqlTransaction = null;
        }

        protected override void BeforeCommandExecuted()
        {
        }

        internal override void AfterCommandExecuted()
        {
        }

        
    }
}
