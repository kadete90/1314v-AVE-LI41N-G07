using System.Data;

namespace SqlMapperFw.MySqlConnection
{
    public interface IMySqlConnection
    {
        void CloseConnection();
        void BeginTransaction(IsolationLevel isolationLevel);
        void Rollback();
        void Commit();
    }
}
