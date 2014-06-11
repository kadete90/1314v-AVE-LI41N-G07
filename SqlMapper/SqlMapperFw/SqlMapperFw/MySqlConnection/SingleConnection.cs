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
            MySql = new SqlConnection(connString.ConnectionString);
            MyCmdBuilder = new CmdBuilderDataMapper<T>(MySql, bindMembers);
            OpenConnection();
        }

        public override Object Execute(String typeCommand, Object elem)
        {
            return ExecuteSwitch(typeCommand, elem);
        }
    }
}
