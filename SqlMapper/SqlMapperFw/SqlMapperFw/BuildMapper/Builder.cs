﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using LinFu.DynamicProxy;
using SqlMapperFw.DataMapper;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Reflection;

namespace SqlMapperFw.BuildMapper
{
    public class Builder
    {
        readonly Type _typeConnection;
        readonly SqlConnectionStringBuilder _connectionStringBuilder;
        readonly IEnumerable<Type> _bindMembers;
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
                    return _mapperSqlConnection.Execute(info.TargetMethod.Name, 
                        (info.Arguments.Length > 0 ) ? info.Arguments.GetValue(0) :info.Arguments );
                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void AfterInvoke(InvocationInfo info, object returnValue)
            {

            }
        }

        public Builder(SqlConnectionStringBuilder connectionStringBuilderBuilder, Type typeConnection, IEnumerable<Type> bindMembers)
        {
            if (connectionStringBuilderBuilder == null)
                throw new ArgumentNullException("connectionStringBuilderBuilder");
            if (typeConnection == null)
                throw new ArgumentNullException("typeConnection");
            if (bindMembers == null)
                throw new ArgumentNullException("bindMembers");

            _typeConnection = typeConnection;
            _connectionStringBuilder = connectionStringBuilderBuilder;
            _bindMembers = bindMembers;
        }

        public IDataMapper<T> Build<T>()
        {
            if (!_typeConnection.ImplementsInterface(typeof(IMapperSqlConnection)))
                throw new Exception("This Type of connection doesn't implements IMapperSqlConnection");

            _mapperSqlConnection = (IMapperSqlConnection)
            Activator.CreateInstance(_typeConnection.MakeGenericType(typeof(T)), _connectionStringBuilder, _bindMembers);

            return new ProxyFactory().CreateProxy<IDataMapper<T>>(new MyInterceptor());
        }

        public void CloseConnection()
        {
            _mapperSqlConnection.CloseConnection();
        }

        public void Rollback()
        {
            _mapperSqlConnection.Rollback();
        }

        public void Commit()
        {
            _mapperSqlConnection.Commit();
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _mapperSqlConnection.BeginTransaction(isolationLevel);
        }

    }
}
