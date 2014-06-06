using System;
using System.Data;
using System.Data.SqlClient;
using SqlMapperFw.DataMappers;

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
            switch (typeCommand)
            {
                case "GetAll":
                    return MyDataMapper.GetAll();
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
            return null;
        }
    }
}
