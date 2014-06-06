using System;
using System.Data;
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

        public override Object Execute(string typeCommand, Object elem)
        {
            OpenConnection();

            switch (typeCommand)
            {
                case "GetAll":
                    MyDataMapper.GetAll();
                    break;
                case "Delete":
                    MyDataMapper.Delete((T)elem);
                    break;
                case "Insert":
                    MyDataMapper.Insert((T)elem);
                    break;
                case "Update":
                    MyDataMapper.Update((T)elem);
                    break;
                default:
                    throw new Exception("This command doesn't exist");
            }
            CloseConnection();
            return null; // alterar!!!!!!
        }
    }
}
