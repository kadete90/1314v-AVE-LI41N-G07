using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public abstract object Execute(string typeCommand, object elem);
    }
}
