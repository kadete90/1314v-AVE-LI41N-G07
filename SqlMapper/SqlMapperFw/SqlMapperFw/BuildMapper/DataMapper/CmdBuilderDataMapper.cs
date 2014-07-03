using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using SqlMapperFw.Binder;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Utils;

namespace SqlMapperFw.BuildMapper.DataMapper
{
    public class CmdBuilderDataMapper<T> : IDataMapper<T>, ICmdBuilder
    {
        readonly SqlConnection _connection;
        readonly AbstractSqlConnection _mySqlConnection;

        readonly String _tableName;
        readonly KeyValuePair<String, PairInfoBind> _pkKeyValuePair; //DB_PK_Name, <DE_PK_Info, DE_Binder>
        readonly MyMemberDictionary _fieldsMatchMemberDictionary = new MyMemberDictionary(); //DB_Field_Name, <DE_Field_Info, DE_Binder>

        readonly Dictionary<String, SqlCommand> _commandsDictionary = new Dictionary<String, SqlCommand>(); //TypeCommand, Command

        public CmdBuilderDataMapper(AbstractSqlConnection abstractMySqlConnection, string tableName, KeyValuePair<string, PairInfoBind> pkKeyValuePair, MyMemberDictionary fieldsMatchMemberDictionary)
        {
            _mySqlConnection = abstractMySqlConnection;
            _connection = _mySqlConnection.Connection;
            _tableName = tableName;
            _pkKeyValuePair = pkKeyValuePair;           
            _fieldsMatchMemberDictionary = fieldsMatchMemberDictionary;

            if (_fieldsMatchMemberDictionary.Count == 0 || _pkKeyValuePair.Key == null)
                throw new Exception("No domain entity fields recognized or no PK defined!!");

            CreateCommands();
        }

        public void CreateCommands()
        {
            if (_mySqlConnection == null)
                throw  new Exception("Connection needed to create commands");

            String DBfields = _fieldsMatchMemberDictionary.Keys.Aggregate("", (current, fieldName) => current + (fieldName + ", "));

            if (DBfields != "")
                DBfields = DBfields.Substring(0, DBfields.Length - 2); //remove last ', '

            SqlCommand cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT " + _pkKeyValuePair.Key +", " +DBfields + " FROM " + _tableName;
            _commandsDictionary.Add("SELECTALL", cmd);

            cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT " + DBfields + " FROM " + _tableName + " WHERE "+ _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("SELECTONE", cmd);

            cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO " + _tableName + " (" + DBfields + ")" + " VALUES (" + _fieldsMatchMemberDictionary.Keys.StringBuilderKeyCollection() + ") SET @ID = SCOPE_IDENTITY();";
            _commandsDictionary.Add("INSERT", cmd);

            cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE " + _tableName + " SET "+ _fieldsMatchMemberDictionary.StringBuilderDicionary() +" WHERE " + _pkKeyValuePair.Key + "= @ID";
            _commandsDictionary.Add("UPDATE", cmd);

            cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM " + _tableName + " WHERE " + _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("DELETE", cmd);
        }

        public Object Execute(string typeCommand, Object elem)
        {
            _mySqlConnection.BeforeCommandExecuted();
            try
            {
                switch (typeCommand)
                {
                    case "GetAll":
                        return GetAll();
                    case "GetById":
                        return GetById(elem);
                    case "Delete":
                        Delete(elem);
                        break;
                    case "Insert":
                        Insert(elem);
                        break;
                    case "Update":
                        Update(elem);
                        break;
                    default:
                        throw new Exception("This command doesn't exist");
                }
            }
            catch (SqlException ex)
            {
                _mySqlConnection.Rollback();
                Console.WriteLine(" >> Rollback !!\n" + ex.Message);
            }
            _mySqlConnection.AfterCommandExecuted();
            return null;
        }

        public ISqlEnumerable<T> GetAll()
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("SELECTALL", out cmd))
                throw new Exception("This Command doesn't exist!");

            cmd.Transaction = _mySqlConnection.SqlTransaction;
            return new SqlEnumerable<T>(cmd, _tableName, _fieldsMatchMemberDictionary.Values, _pkKeyValuePair.Value, _mySqlConnection.AfterCommandExecuted);
        }

        //2.2 TODO OPCIONAL
        SqlEnumerable IDataMapper.GetAll()
        {
            throw new NotImplementedException();
        }

        void IDataMapper<T>.Insert(T val)
        {
            Insert(val);
        }

        void IDataMapper<T>.Update(T val)
        {
            Update(val);
        }

        void IDataMapper<T>.Delete(T val)
        {
            Delete(val);
        }

        public T GetById(object id)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("SELECTONE", out cmd))
                throw new Exception("This Command doesn't exist!");

            T newInstance = (T)Activator.CreateInstance(typeof(T));

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;
            if(!bmpk.bind(newInstance, mipk, id))
                throw new Exception(String.Format("Could not bind this id [{0}] on ED [{1}]",id,typeof(T).Name));

            SqlParameter pkSqlParameter = new SqlParameter("@ID", mipk.GetSqlDbType(newInstance, bmpk))
            {
                Value = id
            };
            cmd.Parameters.Add(pkSqlParameter);
            cmd.Transaction = _mySqlConnection.SqlTransaction;
            try
            {
                SqlDataReader _sqlDataReader = cmd.ExecuteReader();

                if (!_sqlDataReader.HasRows)
                    throw new Exception("No element reference for this id");

                _sqlDataReader.Read();
                
                Object[] DBRowValues = new Object[_sqlDataReader.FieldCount];
                int i = 0;
                
                if (_sqlDataReader.GetValues(DBRowValues) == 0)
                    throw new Exception("Couldn't get DataBase row values from SqlDataReader");

                foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchMemberDictionary)
                {
                    if (DBRowValues[i] == null)
                    {
                        i++;
                        continue;
                    }
                    MemberInfo mi = pair.Value.MemberInfo;
                    AbstractBindMember bm = pair.Value.BindMember;

                    if(!bm.bind(newInstance, mi, DBRowValues[i++]))
                        throw new Exception(String.Format("Could not bind this field [{0}] on ED [{1}]", mi.Name, typeof(T).Name));
                }
                _sqlDataReader.Close();
            }
            catch (Exception ex)
            {
                _mySqlConnection.Rollback();
                throw new Exception("Problem occur on getById:\n" + ex.Message);
            }
            _mySqlConnection.AfterCommandExecuted();
            return newInstance;
        }

        public void Insert(object val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("INSERT", out cmd))
                throw new Exception("This Command doesn't exist!");

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;
            cmd.Parameters.Add("@ID", mipk.GetSqlDbType(val, bmpk)).Direction = ParameterDirection.Output;

            foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchMemberDictionary)
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
            cmd.Transaction = _mySqlConnection.SqlTransaction;
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

            MemberInfo mipk = _pkKeyValuePair.Value.MemberInfo;
            AbstractBindMember bmpk = _pkKeyValuePair.Value.BindMember;

            Object pkValue = val.GetEDFieldValue(mipk, bmpk);
            SqlParameter pkSqlParameter = new SqlParameter("@ID", ReflectionMethods.GetSqlDbType(pkValue))
            {
                Value = pkValue
            };
            cmd.Parameters.Add(pkSqlParameter);

            foreach (KeyValuePair<string, PairInfoBind> pair in _fieldsMatchMemberDictionary)
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
            cmd.Transaction = _mySqlConnection.SqlTransaction;
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
            cmd.Transaction = _mySqlConnection.SqlTransaction;
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

        public void CloseConnection()
        {
            _mySqlConnection.CloseConnection();
        }

        public void Rollback()
        {
            _mySqlConnection.Rollback();
        }

        public void Commit()
        {
            _mySqlConnection.Commit();
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _mySqlConnection.BeginTransaction(isolationLevel);
        }
    }
}
