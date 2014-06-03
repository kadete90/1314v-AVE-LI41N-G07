using System;
using System.Data;
using System.Data.SqlClient;
using SqlMapperFw.DataMappers;

namespace SqlMapperFw.MySqlConnection
{
    public class SingleConnection<T> : IMapperSqlConnection<T>
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

        public Object Execute(String typeCommand, T elem)
        {
            switch (typeCommand)
            {
                case "GetAll":
                    MyDataMapper.GetAll();
                    break;
                case "Delete":
                    MyDataMapper.Delete(elem);
                    break;
                case "Insert":
                    MyDataMapper.Insert(elem);
                    break;
                case "Update":
                    MyDataMapper.Update(elem);
                    break;
                default:
                    throw new Exception("This command doesn't exist");
            }
            return null; // alterar!!!!!!
        }
    }
}
