using System;

namespace SqlMapperFw.MySqlConnection
{
    public interface IMapperSqlConnection<in T>
    {
        void OpenConnection();
        void CloseConnection();
        Object Execute(String typeCommand, T elem);
    }
}
