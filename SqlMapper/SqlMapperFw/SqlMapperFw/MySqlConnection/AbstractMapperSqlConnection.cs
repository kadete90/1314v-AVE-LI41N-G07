using System;
using System.Data;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.MySqlConnection
{
    public abstract class AbstractMapperSqlConnection<T> : IMapperSqlConnection
    {
       
        internal CmdBuilderDataMapper<T> MyCmdBuilder;
        internal SqlConnection Connection { get; set; }
        internal SqlTransaction SqlTransaction;

        public abstract void Commit();
        public abstract void Rollback();
        internal abstract void AfterCommandExecuted();
        protected abstract void BeforeCommandExecuted();
       
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (SqlTransaction != null) throw new InvalidOperationException("Transaction already initialized!");
            SqlTransaction = Connection.BeginTransaction(isolationLevel);
        }

        public bool ActiveConnection()
        {
            return Connection.State == ConnectionState.Open;
        }

        public void OpenConnection()
        {
            if (ActiveConnection())
                return;
            Connection.Open();
            if (!ActiveConnection())
            {
                throw new Exception("Could not open a new connection!");
            }
        }

        public void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        public Object Execute(string typeCommand, Object elem)
        {   
            BeforeCommandExecuted();
            try
            {
                switch (typeCommand)
                {
                    case "GetAll":
                        return MyCmdBuilder.GetAll();
                    case "GetById":
                        return MyCmdBuilder.GetById(elem);
                    case "Delete":
                        MyCmdBuilder.Delete((T) elem);
                        break;
                    case "Insert":
                        MyCmdBuilder.Insert((T) elem);
                        break;
                    case "Update":
                        MyCmdBuilder.Update((T) elem);
                        break;
                    default:
                        throw new Exception("This command doesn't exist");
                }
            }
            catch (Exception ex)
            {
                Rollback();
                CloseConnection();
                Console.WriteLine(" >> Rollback !!\n" + ex.Message);
            }
            AfterCommandExecuted();
            return null;
        }
    }
}
