using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using SqlMapperFw.DataMappers;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Reflection;
using SqlMapperFw.Reflection.Binder;

namespace SqlMapperFw.BuildMapper
{
    public struct PairInfoBind
    {
        public MemberInfo MemberInfo;
        public AbstractBindMember BindMember;

        public PairInfoBind(MemberInfo memberInfo, AbstractBindMember bindMember)
        {
            MemberInfo = memberInfo;
            BindMember = bindMember;
        }
    }

    class MyDictionary : Dictionary<String, PairInfoBind>
    {
        public void Add(String key, MemberInfo memberInfo, AbstractBindMember bindMember)
        {
            Add(key, new PairInfoBind(memberInfo, bindMember));
        }
    }

    public class CmdBuilderDataMapper<T> : IDataMapper<T>
    {
        readonly SqlConnection _sqlConnection;
        readonly AbstractMapperSqlConnection<T> _mapperSqlConnection;

        readonly String _tableName;
        readonly KeyValuePair<String, PairInfoBind> _pkKeyValuePair; //DB_PK_Name, <DE_PK_Info, DE_Binder>
        readonly MyDictionary _fieldsMatchDictionary = new MyDictionary(); //DB_Field_Name, <DE_Field_Info, DE_Binder>

        readonly Dictionary<String, SqlCommand> _commandsDictionary = new Dictionary<String, SqlCommand>(); //TypeCommand, Command

        public CmdBuilderDataMapper(AbstractMapperSqlConnection<T> abstractMapperSqlConnection, IEnumerable<Type> bindMembersTypes)
        {

            _mapperSqlConnection = abstractMapperSqlConnection;
            _sqlConnection = _mapperSqlConnection.Connection;

            Type type = typeof(T);
            _tableName = type.getTableName();

            List<AbstractBindMember> _bindMembersAux = new List<AbstractBindMember>();
            foreach (Type bmType in bindMembersTypes)
            {
                if (bmType == null)
                    throw new ArgumentNullException("bindMembersTypes");
                if (!typeof(AbstractBindMember).IsAssignableFrom(bmType))
                    throw new Exception("This type of binder doesn't extends AbstractBindMember");
                _bindMembersAux.Add((AbstractBindMember)Activator.CreateInstance(bmType));
            }

            foreach (MemberInfo mi in type.GetMembers())
            {
                MemberInfo validMemberInfo = null;
                AbstractBindMember binder = 
                    _bindMembersAux.FirstOrDefault(bm => (validMemberInfo = bm.GetMemberInfoValid(mi)) != null);

                if (binder == null)
                    continue;

                var DEFieldName = validMemberInfo.getDBFieldName();
                if (_fieldsMatchDictionary.ContainsKey(DEFieldName) && _pkKeyValuePair.Key != null)  //TODO opcional: chave composta
                    continue;

                if (validMemberInfo.isPrimaryKey())
                    _pkKeyValuePair = new KeyValuePair<string, PairInfoBind>(DEFieldName,new PairInfoBind(validMemberInfo, binder));
                else
                    _fieldsMatchDictionary.Add(DEFieldName, mi, binder);
            }

            if (_fieldsMatchDictionary.Count == 0 || _pkKeyValuePair.Key == null)
                throw new Exception("No domain entity fields recognized or no PK defined!!");

            CreateCommands();
        }

        public void CreateCommands()
        {
            if (_sqlConnection == null)
                throw  new Exception("Connection needed to create commands");

            String DBfields = _fieldsMatchDictionary.Keys.Aggregate("", (current, fieldName) => current + (fieldName + ", "));

            if (DBfields != "")
                DBfields = DBfields.Substring(0, DBfields.Length - 2); //remove última vírgula

            SqlCommand cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "SELECT " + _pkKeyValuePair.Key +", " +DBfields + " FROM " + _tableName;
            _commandsDictionary.Add("SELECTALL", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "SELECT " + _pkKeyValuePair.Key + ", " + DBfields + " FROM " + _tableName + " WHERE "+ _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("SELECTONE", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "INSERT INTO " + _tableName + " (" + DBfields + ")" + " VALUES (" + _fieldsMatchDictionary.Keys.StringBuilder() + ") SET @ID = SCOPE_IDENTITY();";
            _commandsDictionary.Add("INSERT", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "UPDATE " + _tableName + " SET " + _fieldsMatchDictionary.StringBuilder() + " WHERE " + _pkKeyValuePair.Key + "= @ID";
            _commandsDictionary.Add("UPDATE", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "DELETE FROM " + _tableName + " WHERE " + _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("DELETE", cmd);
        }

        public ISqlEnumerable<T> GetAll()
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("SELECTALL", out cmd))
                throw new Exception("This Command doesn't exist!");

            return new SqlEnumerable<T>(cmd, _fieldsMatchDictionary.Values, _pkKeyValuePair.Value, _mapperSqlConnection);
        }

        //public T GetById(object id)
        //{
        //    SqlCommand cmd;
        //    if (!_commandsDictionary.TryGetValue("SELECTONE", out cmd))
        //        throw new Exception("This Command doesn't exist!");
        //    T newInstance = (T)Activator.CreateInstance(typeof(T));
        //    SqlParameter pkSqlParameter = new SqlParameter("@ID", _pkKeyValuePair.Value.MemberInfo.GetSqlDbType(newInstance))
        //    {
        //        Value = id
        //    };
        //    cmd.Parameters.Add(pkSqlParameter);

        //    SqlDataReader _sqlDataReader = _mapperSqlConnection.ReadTransactionAutoClosable(cmd);

        //    foreach (var DBRowValues in _sqlDataReader.AsEnumerable())
        //    {
        //        foreach (AbstractBindMember bm in _bindMembersAux)
        //            if (bm.bind(newInstance, _pkKeyValuePair.Value.MemberInfo, DBRowValues[0]))
        //                break;
        //        int idx = 1;
        //        Dictionary<string, PairInfoBind>.ValueCollection MemberInfos = _fieldsMatchDictionary.Values;
        //        foreach (PairInfoBind pair in MemberInfos)
        //        {
        //            foreach (AbstractBindMember bm in _bindMembersAux)
        //                if (bm.bind(newInstance, pair.MemberInfo, DBRowValues[idx]))
        //                    break;
        //            idx++;
        //        }
        //    }
        //    _sqlDataReader.Close();
        //    return newInstance;
        //}

        //minimizar reflexão neste método
        public void Insert(T val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("INSERT", out cmd))
                throw new Exception("This Command doesn't exist!");

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;
            cmd.Parameters.Add("@ID", mipk.GetSqlDbType(val, bmpk)).Direction = ParameterDirection.Output;
            
            foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchDictionary)
            {
                MemberInfo mi = pair.Value.MemberInfo;
                AbstractBindMember bm = pair.Value.BindMember;

                SqlParameter p = new SqlParameter(pair.Key, mi.GetSqlDbType(val, bm))
                {
                    Value = bm.GetValue(val, mi)
                };
                cmd.Parameters.Add(p);
            }
            _mapperSqlConnection.ExecuteTransaction(cmd);
            bmpk.bind(val, mipk, cmd.Parameters[0].Value);      
        }

        //private Dictionary<String, MemberInfo> filterMemberInfos(T toUpdate)
        //{
        //    T OnDB = GetById(_pkKeyValuePair.Value.MemberInfo.GetValue(toUpdate));

        //    MyDictionary toInsertInfos = new MyDictionary();

        //    foreach (MemberInfo mi in OnDB.GetType().GetMembers())
        //    {
        //        if (_fieldsMatchDictionary.ContainsValue(mi))
        //        {
        //            Object valueOnBD = mi.GetValue(OnDB);
        //            Object valueToUpdate = mi.GetValue(toUpdate);
        //            if (valueOnBD.Equals(valueToUpdate)) continue;
        //            MemberInfo info;
        //            if(_fieldsMatchDictionary.TryGetValue(mi.getDBFieldName(), out info))
        //                toInsertInfos.Add(mi.getDBFieldName(), info);
        //        }
        //    }
        //    return toInsertInfos;
        //} 

        //minimizar reflexão neste método
        public void Update(T val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("UPDATE", out cmd))
                throw new Exception("This Command doesn't exist!");

            //Dictionary<String, MemberInfo> filterInfos = filterMemberInfos(val);
            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;

            SqlParameter pkSqlParameter = new SqlParameter("@ID", mipk.GetSqlDbType(val, bmpk))
            {
                Value = bmpk.GetValue(val, mipk)
            };
            cmd.Parameters.Add(pkSqlParameter);

            foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchDictionary) //_fieldsMatchDictionary
            {
                MemberInfo mi = pair.Value.MemberInfo;
                AbstractBindMember bm = pair.Value.BindMember;

                SqlParameter p = new SqlParameter(pair.Key, mi.GetSqlDbType(val, bm))
                {
                    Value = bm.GetValue(val, mi)
                };
                cmd.Parameters.Add(p);
            }
            _mapperSqlConnection.ExecuteTransaction(cmd);
        }

        //minimizar reflexão neste método
        public void Delete(T val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("DELETE", out cmd))
                throw new Exception("This Command doesn't exist!");

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;

            SqlParameter pkSqlParameter = new SqlParameter("@ID", mipk.GetSqlDbType(val, bmpk))
            {
                Value = bmpk.GetValue(val, mipk)
            };
            cmd.Parameters.Add(pkSqlParameter);
            _mapperSqlConnection.ExecuteTransaction(cmd);
        }
    }
}
