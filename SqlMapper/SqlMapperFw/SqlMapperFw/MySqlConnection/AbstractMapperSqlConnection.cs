﻿using System;
using System.Data;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.MySqlConnection
{
    public abstract class AbstractMapperSqlConnection<T> : IMapperSqlConnection
    {
        internal SqlConnection Connection { get; set; }
        internal CmdBuilderDataMapper<T> MyCmdBuilder;
        internal SqlTransaction SqlTransaction;
        internal bool autoCommit = true;
       

        public abstract void Commit();
        public abstract void Rollback();

        public void OpenConnection()
        {
            if (Connection.State == ConnectionState.Open)
                return;
            Connection.Open();
            if (Connection.State != ConnectionState.Open)
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
            switch (typeCommand)
            {
                case "GetAll":
                    return MyCmdBuilder.GetAll();
                case "GetById":
                    return MyCmdBuilder.GetById(elem);
                case "Delete":
                    MyCmdBuilder.Delete((T)elem);
                    break;
                case "Insert":
                    MyCmdBuilder.Insert((T)elem);
                    break;
                case "Update":
                    MyCmdBuilder.Update((T)elem);
                    break;
                default:
                    throw new Exception("This command doesn't exist");
            }
            return null;
        }

        public SqlDataReader ReadTransaction(SqlCommand sqlCommand)
        {
            OpenConnection();
            SqlTransaction = Connection.BeginTransaction(IsolationLevel.ReadUncommitted);
            sqlCommand.Transaction = SqlTransaction;
            try
            {
                return sqlCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                Rollback();
                throw new Exception("An error occur on ReadTransaction(...): \n\nRollback this transaction...\n" + ex.Message);
            }
        }

        public SqlDataReader ReadTransactionAutoClosable(SqlCommand sqlCommand)
        {
            OpenConnection();
            using (sqlCommand.Transaction = Connection.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    return sqlCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    Rollback();
                    throw new Exception("An error occur on ReadTransactionAutoClosable(...): \n\nRollback this transaction...\n" + ex.Message);
                }
            }
        }

        public void ExecuteTransaction(SqlCommand sqlCommand)
        {
            OpenConnection();
            SqlTransaction = Connection.BeginTransaction(IsolationLevel.Serializable);
            sqlCommand.Transaction = SqlTransaction;
            try
            {
                if (sqlCommand.ExecuteNonQuery() == 0)
                    Console.WriteLine("No row(s) affected!");
            }
            catch (Exception ex)
            {
                Rollback();
                throw new Exception("An error occur on ExecuteTransaction(...): \n\nRollback this transaction...\n" + ex.Message);
            }
            if (autoCommit)
            {
                Commit();
            }
        }
    }
}
