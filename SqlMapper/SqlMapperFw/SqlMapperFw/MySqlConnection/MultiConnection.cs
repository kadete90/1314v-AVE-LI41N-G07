using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.MySqlConnection
{
    public class MultiConnection<T> : AbstractMapperSqlConnection<T>
    {
        public MultiConnection(SqlConnectionStringBuilder connString, IEnumerable<Type> bindMembers)
        {
            Connection = new SqlConnection(connString.ConnectionString);
            MyCmdBuilder = new CmdBuilderDataMapper<T>(this, bindMembers);
        }

        public override void Rollback()
        {
            if (SqlTransaction == null || !ActiveConnection())
                throw new Exception("Cannot Rollback! Transaction doesn't have a active connection or is null!");

            SqlTransaction.Dispose();
            SqlTransaction.Rollback();
            SqlTransaction = null;
            CloseConnection();
        }

        public override void Commit()
        {
            if (SqlTransaction == null || !ActiveConnection())
                throw new Exception("Cannot Commit! Transaction doesn't have a active connection or is null!");

            SqlTransaction.Commit();
            SqlTransaction = null;
        }

        protected override void BeforeCommandExecuted()
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
