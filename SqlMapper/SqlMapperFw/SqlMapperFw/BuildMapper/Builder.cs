using System;
using System.Data.SqlClient;
using LinFu.DynamicProxy;
using SqlMapperFw.DataMappers;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Reflection;

namespace SqlMapperFw.BuildMapper
{
    public class Builder
    {
        public static Type TypeConnection;
        public static IMapperSqlConnection Connection;
        public SqlConnectionStringBuilder ConnectionString { get; set; }

        internal class MyInterceptor : IInvokeWrapper
        {
            public void BeforeInvoke(InvocationInfo info)
            {

            }

            public object DoInvoke(InvocationInfo info)
            {
                try
                {
                    return Connection.Execute(info.TargetMethod.Name, info.Arguments);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return null;
            }

            public void AfterInvoke(InvocationInfo info, object returnValue)
            {

            }
        }

        public void CloseConnection()
        {
            Connection.CloseConnection();
        }

        public Builder(SqlConnectionStringBuilder connectionStringBuilder, Type typeConnection)
        {
            TypeConnection = typeConnection;
            ConnectionString = connectionStringBuilder;
        }

        public IDataMapper<T> Build<T>()
        {
            if (!TypeConnection.ImplementsInterface(typeof(IMapperSqlConnection)))
                throw new Exception("This type of connection doesn't implements IMapperSqlConnection");

            Connection = (IMapperSqlConnection)Activator.CreateInstance(TypeConnection.MakeGenericType(typeof(T)), ConnectionString);
            return new ProxyFactory().CreateProxy<IDataMapper<T>>(new MyInterceptor());
        }
    }
}
