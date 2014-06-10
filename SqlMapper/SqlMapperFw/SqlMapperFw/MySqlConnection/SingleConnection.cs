using System;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.MySqlConnection
{
    public class SingleConnection<T> : AbstractMapperSqlConnection<T>
    {
        public SingleConnection(SqlConnectionStringBuilder connString)
        {
            MySql = new SqlConnection(connString.ConnectionString);
            MyDataMapper = new CmdBuilder<T>(MySql);
            OpenConnection();
        }

        public override Object Execute(String typeCommand, Object elem)
        {
            return ExecuteSwitch(typeCommand, elem);
        }
    }
}
