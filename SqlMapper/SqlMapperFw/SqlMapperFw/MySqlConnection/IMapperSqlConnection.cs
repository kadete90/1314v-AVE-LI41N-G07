using System;

namespace SqlMapperFw.MySqlConnection
{
    public interface IMapperSqlConnection
    {
        Object Execute(String typeCommand, Object elem);
        void CloseConnection();
        void Rollback();
        void Commit();
    }
}
