using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using LinFu.DynamicProxy;
using SqlMapperFw.Binder;
using SqlMapperFw.BuildMapper.DataMapper;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Utils;

namespace SqlMapperFw.BuildMapper
{
    public class Builder
    {
        readonly Type _typeConnection;
        readonly SqlConnectionStringBuilder _connectionStringBuilder;
        readonly IEnumerable<Type> _bindMembers;
        private readonly Dictionary<String, KeyValuePair<IDataMapper, AbstractMapperSqlConnection>> mapEDMapper 
            = new Dictionary<string, KeyValuePair<IDataMapper, AbstractMapperSqlConnection>>();
        private static KeyValuePair<IDataMapper, AbstractMapperSqlConnection> _activePair;
        readonly MyDictionary _fieldsMatchDictionary = new MyDictionary(); //DB_Field_Name, <DE_Field_Info, DE_Binder>
        
        internal class MyInterceptor : IInvokeWrapper
        {
            public void BeforeInvoke(InvocationInfo info)
            {

            }

            public object DoInvoke(InvocationInfo info)
            {
                try
                {
                    return _activePair.Key.Execute(info.TargetMethod.Name,
                        (info.Arguments.Length > 0) ? info.Arguments.GetValue(0) : info.Arguments);
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
            if (mapEDMapper.ContainsKey(typeof (T).Name)) //testar optimização
            {
                KeyValuePair<IDataMapper, AbstractMapperSqlConnection> pair;
                mapEDMapper.TryGetValue(typeof (T).Name, out pair);
                _activePair = new KeyValuePair<IDataMapper, AbstractMapperSqlConnection>(pair.Key, pair.Value);
                return new ProxyFactory().CreateProxy<IDataMapper<T>>(new MyInterceptor());
            }

            if (!_typeConnection.ImplementsInterface(typeof(IMySqlConnection)))
                throw new Exception("This Type of connection doesn't implements IMapperSqlConnection");

            List<AbstractBindMember> bindMembersAux = new List<AbstractBindMember>();
            foreach (Type bmType in _bindMembers)
            {
                if (bmType == null)
                    throw new Exception("Type member cannot be null");
                if (!typeof(AbstractBindMember).IsAssignableFrom(bmType))
                    throw new Exception("This TypeAux of binder doesn't extends AbstractBindMember");
                bindMembersAux.Add((AbstractBindMember)Activator.CreateInstance(bmType));
            }

            Type type = typeof(T);
            string _tableName = type.GetTableName();
            KeyValuePair<String, PairInfoBind> _pkKeyValuePair = new KeyValuePair<string, PairInfoBind>(); //DB_PK_Name, <DE_PK_Info, DE_Binder>
            foreach (MemberInfo mi in type.GetMembers())
            {

                MemberInfo validMemberInfo = null;
                AbstractBindMember binder = bindMembersAux.FirstOrDefault(bm =>
                    (validMemberInfo = bm.GetMemberInfo(mi)) != null);

                if (binder == null)
                    continue;
                if (validMemberInfo.IsForeignKey())
                    validMemberInfo = new FkMemberInfo(validMemberInfo, type);

                var DEFieldName = validMemberInfo.GetDBFieldName();

                if (_fieldsMatchDictionary.ContainsKey(DEFieldName))
                    continue;

                //TODO OPCIONAL: chave composta
                bool isPK = validMemberInfo.IsPrimaryKey();
            
                if (isPK && _pkKeyValuePair.Key != null)
                    continue;

                if (isPK)
                    _pkKeyValuePair = new KeyValuePair<string, PairInfoBind>(DEFieldName, new PairInfoBind(validMemberInfo, binder));
                else
                    _fieldsMatchDictionary.Add(DEFieldName, validMemberInfo, binder);
            }
            
            AbstractMapperSqlConnection _mapperSqlConnection = (AbstractMapperSqlConnection) Activator.CreateInstance(_typeConnection, _connectionStringBuilder);
            CmdBuilderDataMapper<T> MyCmdBuilder = new CmdBuilderDataMapper<T>(_mapperSqlConnection, _tableName, _pkKeyValuePair, _fieldsMatchDictionary);

            _activePair = new KeyValuePair<IDataMapper, AbstractMapperSqlConnection>(MyCmdBuilder, _mapperSqlConnection);
            mapEDMapper.Add(typeof(T).Name, _activePair);
           
            return new ProxyFactory().CreateProxy<IDataMapper<T>>(new MyInterceptor());
        }

        public void CloseConnection()
        {
            _activePair.Value.CloseConnection();
        }

        public void Rollback()
        {
            _activePair.Value.Rollback();
        }

        public void Commit()
        {
            _activePair.Value.Commit();
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _activePair.Value.BeginTransaction(isolationLevel);
        }

    }
}
