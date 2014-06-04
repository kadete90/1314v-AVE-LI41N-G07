using System;

namespace SqlMapperFw.MySqlConnection
{
    public interface IMapperSqlConnection
    {
        void OpenConnection();
        void CloseConnection();
        Object Execute(String typeCommand, Object elem);
    }
}
