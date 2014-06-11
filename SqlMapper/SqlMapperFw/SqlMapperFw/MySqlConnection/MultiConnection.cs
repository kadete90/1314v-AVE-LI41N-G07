using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.MySqlConnection
{
    public class MultiConnection<T> : AbstractMapperSqlConnection<T>
    {
        public MultiConnection(SqlConnectionStringBuilder connString, IEnumerable<Type> bindMembers)
        {
            MySql = new SqlConnection(connString.ConnectionString);
            MyCmdBuilder = new CmdBuilderDataMapper<T>(MySql, bindMembers);
        }

        public override Object Execute(String typeCommand, Object elem)
        {
            OpenConnection();
            Object aux;
            while ((aux = ExecuteSwitch(typeCommand, elem)) != null)
                return aux;
            CloseConnection();
            return null;
        }
    }
}
