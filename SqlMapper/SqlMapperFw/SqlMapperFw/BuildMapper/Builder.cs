using System;
using System.Data;
using System.Data.SqlClient;
using LinFu.DynamicProxy;
using SqlMapperFw.DataMappers;

namespace SqlMapperFw.BuildMapper
{
    class Builder
    {
        private static SqlConnection _mySql;
        public static bool IsSingleConnection { get; private set; }
        public static bool IsOpenConnection { get; private set; }

        public void TryGetConnectionString(SqlConnectionStringBuilder connectionStringBuilder)
        {
            using (SqlConnection connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    connection.Close();
                } 
                catch
                {
                    Console.WriteLine("New Connection With Problems!!");
                    _mySql = null;
                    return;
                }
            }
            _mySql = new SqlConnection(connectionStringBuilder.ConnectionString);
        }

        private static void OpenConnection()
        {
            _mySql.Open();
            if (_mySql.State != ConnectionState.Open)
            {
                Console.WriteLine("Active Connection with Problems!\n");
                IsOpenConnection = false;
            }
            else
            {
                IsOpenConnection = true;
            }

        }

        public void CloseConnection()
        {
            _mySql.Close();
        }

        internal class MyInterceptor : IInvokeWrapper
        {
            public void BeforeInvoke(InvocationInfo info)
            {
                if (!IsSingleConnection)
                    OpenConnection();
            }

            public object DoInvoke(InvocationInfo info)
            {
                if (!IsOpenConnection)
                    return null;

                Console.Write(info.TargetMethod.Name + " invoked with args: ");
                foreach (var a in info.Arguments)
                {
                    Console.Write(a + " ");
                }
                Console.WriteLine();
                return 0;
            }

            public void AfterInvoke(InvocationInfo info, object returnValue)
            {
                if (!IsSingleConnection)
                    _mySql.Close();
            }
        }

        public Builder(SqlConnectionStringBuilder strBuilder, bool isSingleConnection)
        {
            TryGetConnectionString(strBuilder);
            if (_mySql == null)
            {
                return;
            }
            IsSingleConnection = isSingleConnection;
            if(IsSingleConnection)
                OpenConnection();
        }

        public IDataMapper<T> Build<T>()
        {
            return new ProxyFactory().CreateProxy<IDataMapper<T>>(new MyInterceptor());
        }
    }
}
