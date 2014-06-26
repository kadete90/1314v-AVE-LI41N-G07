using System;
using System.Data;

namespace SqlMapperFw.MySqlConnection
{
    public interface IMapperSqlConnection
    {
        Object Execute(String typeCommand, Object elem);
        void CloseConnection();
        void BeginTransaction(IsolationLevel isolationLevel);
        void Rollback();
        void Commit();
    }
}
