using System;
using System.Data.SqlClient;
using SqlMapperFw.DataMappers;

namespace SqlMapperFw.MySqlConnection
{
    public class MultiConnection<T> : AbstractMapperSqlConnection<T>
    {
        public MultiConnection(SqlConnectionStringBuilder connString)
        {
            MySql = new SqlConnection(connString.ConnectionString);
            MyDataMapper = new CmdBuilder<T>(MySql);
        }

        public override Object Execute(String typeCommand, Object elem)
        {
            OpenConnection();
            object ret = ExecuteSwitch(typeCommand, elem);
            CloseConnection();
            return ret;
        }
    }
}
