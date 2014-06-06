using System;
using System.Data;
using System.Data.SqlClient;
using SqlMapperFw.DataMappers;

namespace SqlMapperFw.MySqlConnection
{
    public class SingleConnection<T> : IMapperSqlConnection
    {
        private readonly SqlConnection _mySql;
        public IDataMapper<T> MyDataMapper;

        public SingleConnection(SqlConnectionStringBuilder connString)
        {
            _mySql = new SqlConnection(connString.ConnectionString);
            MyDataMapper = new CmdBuilder<T>(_mySql);
            OpenConnection();
        }

        public void CloseConnection()
        {
            if (_mySql.State != ConnectionState.Closed)
                _mySql.Close();
        }

        public void OpenConnection()
        {
            if (_mySql.State == ConnectionState.Open)
                return;
            _mySql.Open();
            if (_mySql.State != ConnectionState.Open)
            {
                throw new Exception("Could not open a new connection!");
            }
        }

        public Object Execute(String typeCommand, Object elem)
        {
            switch (typeCommand)
            {
                case "GetAll":
                    return MyDataMapper.GetAll();
                case "Delete":
                    MyDataMapper.Delete((T)elem);
                    return true;
                case "Insert":
                    MyDataMapper.Insert((T)elem);
                    return true;
                case "Update":
                    MyDataMapper.Update((T)elem);
                    return true;
                default:
                    throw new Exception("This command doesn't exist");
            }
        }
    }
}
