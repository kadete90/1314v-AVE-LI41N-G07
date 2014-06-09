using System;
using System.Data;
using System.Data.SqlClient;
using SqlMapperFw.DataMappers;

namespace SqlMapperFw.MySqlConnection
{
    public abstract class AbstractMapperSqlConnection<T> : IMapperSqlConnection
    {
        protected SqlConnection MySql;
        public IDataMapper<T> MyDataMapper;

        public void OpenConnection()
        {
            if (MySql.State == ConnectionState.Open)
                return;
            MySql.Open();
            if (MySql.State != ConnectionState.Open)
            {
                throw new Exception("Could not open a new connection!");
            }
        }

        public void CloseConnection()
        {
            if (MySql.State != ConnectionState.Closed)
                MySql.Close();
            
        }

        public Object ExecuteSwitch(string typeCommand, Object elem)
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

        public abstract Object Execute(string typeCommand, Object elem);
    
    }
}
