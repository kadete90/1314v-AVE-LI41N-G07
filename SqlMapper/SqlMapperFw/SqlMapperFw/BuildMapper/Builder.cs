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
        readonly Type _typeConnection;
        readonly SqlConnectionStringBuilder _connectionStringBuilder;
        static IMapperSqlConnection _mapperSqlConnection;  

        internal class MyInterceptor : IInvokeWrapper
        {
            public void BeforeInvoke(InvocationInfo info)
            {

            }

            public object DoInvoke(InvocationInfo info)
            {
                try
                {
                    return _mapperSqlConnection.Execute(info.TargetMethod.Name, info.Arguments);
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
            _mapperSqlConnection.CloseConnection();
        }

        public Builder(SqlConnectionStringBuilder connectionStringBuilderBuilder, Type typeConnection)
        {
            _typeConnection = typeConnection;
            _connectionStringBuilder = connectionStringBuilderBuilder;
        }

        public IDataMapper<T> Build<T>()
        {
            if (!_typeConnection.ImplementsInterface(typeof(IMapperSqlConnection)))
                throw new Exception("This type of connection doesn't implements IMapperSqlConnection");

            _mapperSqlConnection = (IMapperSqlConnection)
                Activator.CreateInstance(_typeConnection.MakeGenericType(typeof(T)), _connectionStringBuilder);

            return new ProxyFactory().CreateProxy<IDataMapper<T>>(new MyInterceptor());
        }
    }
}
