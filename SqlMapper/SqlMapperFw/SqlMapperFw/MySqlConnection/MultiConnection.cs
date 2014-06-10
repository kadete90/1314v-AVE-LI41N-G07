using System;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

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
            Object aux;
            //TODO: confirmar se existe uma solução melhor
            while ((aux = ExecuteSwitch(typeCommand, elem)) != null)
                return aux;
            CloseConnection();
            return null;
        }
    }
}
