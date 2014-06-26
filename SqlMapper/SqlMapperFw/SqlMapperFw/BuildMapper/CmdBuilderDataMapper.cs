using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using SqlMapperFw.DataMapper;
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

    public class CmdBuilderDataMapper<T> : IDataMapper
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
            _tableName = type.GetTableName();

            List<AbstractBindMember> _bindMembersAux = new List<AbstractBindMember>();
            foreach (Type bmType in bindMembersTypes)
            {
                if (bmType == null)
                    throw new ArgumentNullException("bindMembersTypes");
                if (!typeof(AbstractBindMember).IsAssignableFrom(bmType))
                    throw new Exception("This TypeAux of binder doesn't extends AbstractBindMember");
                _bindMembersAux.Add((AbstractBindMember)Activator.CreateInstance(bmType));
            }

            foreach (MemberInfo mi in type.GetMembers())
            {

                MemberInfo validMemberInfo = null;
                AbstractBindMember binder = _bindMembersAux.FirstOrDefault(bm =>
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
                    _pkKeyValuePair = new KeyValuePair<string, PairInfoBind>(DEFieldName,new PairInfoBind(validMemberInfo, binder));
                else
                    _fieldsMatchDictionary.Add(DEFieldName, validMemberInfo, binder);
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
            cmd.CommandText = "SELECT " + DBfields + " FROM " + _tableName + " WHERE "+ _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("SELECTONE", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "INSERT INTO " + _tableName + " (" + DBfields + ")" + " VALUES (" + _fieldsMatchDictionary.Keys.StringBuilderKeyCollection() + ") SET @ID = SCOPE_IDENTITY();";
            _commandsDictionary.Add("INSERT", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "UPDATE " + _tableName + " SET @TO_REPLACE WHERE " + _pkKeyValuePair.Key + "= @ID";
            //cmd.CommandText = "UPDATE " + _tableName + " SET "+ _fieldsMatchDictionary.StringBuilderDicionary() +" WHERE " + _pkKeyValuePair.Key + "= @ID";
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

            //TODO tentar resolver problema de outra forma
            int idx = cmd.CommandText.IndexOf(" WHERE", StringComparison.Ordinal);
            if (idx > 0)
                cmd.CommandText = cmd.CommandText.Remove(idx);
            cmd.Transaction = _mapperSqlConnection.SqlTransaction;
            return new SqlEnumerable<T>(cmd, _fieldsMatchDictionary.Values, _pkKeyValuePair.Value, _mapperSqlConnection.AfterCommandExecuted);
        }

        //2.2 TODO OPCIONAL
        SqlEnumerable IDataMapper.GetAll()
        {
            throw new NotImplementedException();
        }

        public T GetById(object id)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("SELECTONE", out cmd))
                throw new Exception("This Command doesn't exist!");

            T newInstance = (T)Activator.CreateInstance(typeof(T));

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;
            bmpk.bind(newInstance, mipk, id);

            SqlParameter pkSqlParameter = new SqlParameter("@ID", mipk.GetSqlDbType(newInstance, bmpk))
            {
                Value = id
            };
            cmd.Parameters.Add(pkSqlParameter);
            cmd.Transaction = _mapperSqlConnection.SqlTransaction;
            try
            {
                SqlDataReader _sqlDataReader = cmd.ExecuteReader();

                if (!_sqlDataReader.HasRows)
                    throw new Exception("No element reference for this id");

                _sqlDataReader.Read();
                Object[] DBRowValues = new Object[_sqlDataReader.FieldCount];
                int i = 0;

                foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchDictionary)
                {
                    MemberInfo mi = pair.Value.MemberInfo;
                    AbstractBindMember bm = pair.Value.BindMember;
                    bm.bind(newInstance, mi, DBRowValues[i++]);
                }
                _sqlDataReader.Close();
            }
            catch (Exception ex)
            {
                _mapperSqlConnection.Rollback();
                throw new Exception("Problem occur on getById:\n" + ex.Message);
            }
            _mapperSqlConnection.AfterCommandExecuted();
            return newInstance;
        }

        public void Insert(object val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("INSERT", out cmd))
                throw new Exception("This Command doesn't exist!");

            //Dictionary<String, PairInfoBind> filterInfos = filterMemberInfos(val);

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;
            cmd.Parameters.Add("@ID", mipk.GetSqlDbType(val, bmpk)).Direction = ParameterDirection.Output;

            foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchDictionary)
            {
                MemberInfo mi = pair.Value.MemberInfo;
                AbstractBindMember bm = pair.Value.BindMember;

                Object fieldValue = val.GetEDFieldValue(mi, bm);

                SqlParameter p = new SqlParameter(pair.Key, ReflectionMethods.GetSqlDbType(fieldValue))
                {
                    Value = fieldValue
                };

                cmd.Parameters.Add(p);
            }
            cmd.Transaction = _mapperSqlConnection.SqlTransaction;
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    Console.WriteLine("No row(s) affected!");
               
            }
            catch (Exception ex)
            {
                throw new Exception("Exception on Insert\n" + ex.Message);
            }
            val.BindEDFieldValue(mipk, bmpk, cmd.Parameters[0].Value);
        }

        public void Update(object val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("UPDATE", out cmd))
                throw new Exception("This Command doesn't exist!");

            Dictionary<String, PairInfoBind> filterInfos = filterMemberInfos(val);

            cmd.CommandText = cmd.CommandText.Replace("@TO_REPLACE", filterInfos.StringBuilderDicionary());

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;

            Object pkValue = val.GetEDFieldValue(mipk, bmpk);
            SqlParameter pkSqlParameter = new SqlParameter("@ID", ReflectionMethods.GetSqlDbType(pkValue))
            {
                Value = pkValue
            };
            cmd.Parameters.Add(pkSqlParameter);

            foreach (KeyValuePair<string, PairInfoBind> pair in filterInfos)
            {
                MemberInfo mi = pair.Value.MemberInfo;
                AbstractBindMember bm = pair.Value.BindMember;

                Object fieldValue = val.GetEDFieldValue(mi, bm);
                SqlParameter p = new SqlParameter(pair.Key, fieldValue)
                {
                    Value = fieldValue
                };
                cmd.Parameters.Add(p);
                val.BindEDFieldValue(mi, bm, fieldValue);
            }
            cmd.Transaction = _mapperSqlConnection.SqlTransaction;
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    Console.WriteLine("No row(s) affected!");
                
            }
            catch (Exception ex)
            {
                throw new Exception("Exception on Update\n" + ex.Message);
            }
        }

        public void Delete(object val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("DELETE", out cmd))
                throw new Exception("This Command doesn't exist!");

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;

            Object pkValue = val.GetEDFieldValue(mipk, bmpk);
            SqlParameter pkSqlParameter = new SqlParameter("@ID", ReflectionMethods.GetSqlDbType(pkValue))
            {
                Value = pkValue
            };
            cmd.Parameters.Add(pkSqlParameter);
            cmd.Transaction = _mapperSqlConnection.SqlTransaction;
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    Console.WriteLine("No row(s) affected!");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception on Delete\n" + ex.Message);
            }
        }

        // Objectivo: Apenas fazer update dos fields com valores non-default em toUpdate
        //TODO melhorar
        private Dictionary<String, PairInfoBind> filterMemberInfos(object toUpdate)
        {
            MyDictionary toUpdateInfos = new MyDictionary();
            foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchDictionary)
            {
                MemberInfo mi = pair.Value.MemberInfo;
                AbstractBindMember bm = pair.Value.BindMember;
                if (toUpdate.GetEDFieldValue(mi, bm) == null)
                    continue;
                toUpdateInfos.Add(pair.Key, mi, bm);
            }
            return toUpdateInfos;
        } 
    }
}
