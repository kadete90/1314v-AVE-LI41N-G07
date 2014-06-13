using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.MySqlConnection
{
    public class SingleConnection<T> : AbstractMapperSqlConnection<T>
    {
        public SingleConnection(SqlConnectionStringBuilder connString, IEnumerable<Type> bindMembers, bool autoCommit)
        {
            base.autoCommit = autoCommit;
            Connection = new SqlConnection(connString.ConnectionString);
            MyCmdBuilder = new CmdBuilderDataMapper<T>(this, bindMembers);
        }

        public override void Rollback()
        {
            SqlTransaction.Dispose();
            SqlTransaction.Rollback();
        }

        public override void Commit()
        {
            SqlTransaction.Commit();
        }
    }
}
