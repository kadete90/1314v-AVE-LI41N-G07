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
    public class CmdBuilderDataMapper<T> : IDataMapper<T>
    {
        readonly SqlConnection _sqlConnection;
        readonly AbstractMapperSqlConnection<T> _mapperSqlConnection;

        readonly String _tableName;
        readonly KeyValuePair<String, MemberInfo> _pkKeyValuePair; //DB_PK_Name, DE_PK_Info
        readonly Dictionary<String, MemberInfo> _fieldsMatchDictionary = new Dictionary<String, MemberInfo>(); //DB_Field_Name, DE_Field_Info

        readonly List<AbstractBindMember> _bindMembers = new List<AbstractBindMember>();
        readonly Dictionary<String, SqlCommand> _commandsDictionary = new Dictionary<String, SqlCommand>(); //TypeCommand, Command

        public CmdBuilderDataMapper(AbstractMapperSqlConnection<T> abstractMapperSqlConnection, IEnumerable<Type> bindMembersTypes)
        {

            _mapperSqlConnection = abstractMapperSqlConnection;
            _sqlConnection = _mapperSqlConnection.Connection;

            Type type = typeof(T);
            _tableName = type.getTableName();

            foreach (Type bmType in bindMembersTypes)
            {
                if (bmType == null)
                    throw new ArgumentNullException("bindMembersTypes");
                if (!typeof(AbstractBindMember).IsAssignableFrom(bmType))
                    throw new Exception("This type of binder doesn't extends AbstractBindMember");
                _bindMembers.Add((AbstractBindMember)Activator.CreateInstance(bmType));
            }

            foreach (MemberInfo mi in type.GetMembers())
            {
                MemberInfo validMemberInfo = null;
                foreach (AbstractBindMember bm in _bindMembers)
                    if ((validMemberInfo = bm.GetMemberInfoValid(mi)) != null)
                        break;

                if (validMemberInfo == null)
                    continue;

                var DEFieldName = validMemberInfo.getDBFieldName();

                if (!_fieldsMatchDictionary.ContainsKey(DEFieldName) && !validMemberInfo.isPrimaryKey())
                    _fieldsMatchDictionary.Add(DEFieldName, mi);

                if (_pkKeyValuePair.Key != null) //TODO: alterar quando tiver chave composta
                    continue;
                if (validMemberInfo.isPrimaryKey())
                    _pkKeyValuePair = new KeyValuePair<string, MemberInfo>(DEFieldName, validMemberInfo);
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
            _commandsDictionary.Add("SELECT", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "INSERT INTO " + _tableName + " (" + DBfields + ")" + " VALUES (" + _fieldsMatchDictionary.Keys.StringBuilder() + ") SET @ID = SCOPE_IDENTITY();";
            _commandsDictionary.Add("INSERT", cmd);

            cmd = _sqlConnection.CreateCommand();                        
            cmd.CommandText = "UPDATE " + _tableName + " SET " + _fieldsMatchDictionary.StringBuilder() +" WHERE "+ _pkKeyValuePair.Key +"= @ID";
            _commandsDictionary.Add("UPDATE", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "DELETE FROM " + _tableName + " WHERE " + _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("DELETE", cmd);
        }

        public ISqlEnumerable<T> GetAll()
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("SELECT", out cmd))
                throw new Exception("This Command doesn't exist!");

            return new SqlEnumerable<T>(cmd, _bindMembers,
                new List<MemberInfo>(_fieldsMatchDictionary.Values), _pkKeyValuePair.Value, _mapperSqlConnection);
        }

        //minimizar reflexão neste método
        public void Insert(T val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("INSERT", out cmd))
                throw new Exception("This Command doesn't exist!");
            cmd.Parameters.Add("@ID", _pkKeyValuePair.Value.GetSqlDbType(val)).Direction = ParameterDirection.Output;
            foreach (var field in _fieldsMatchDictionary)
            {
                SqlParameter p = new SqlParameter(field.Key, field.Value.GetSqlDbType(val))
                {
                    Value = field.Value.GetValue(val)
                };
                cmd.Parameters.Add(p);
            }
            _mapperSqlConnection.ExecuteTransaction(cmd);
            _pkKeyValuePair.Value.SetValue(val, cmd.Parameters[0].Value);      
        }

        //minimizar reflexão neste método
        public void Update(T val)
        {
            SqlCommand cmd;
            if (!_commandsDictionary.TryGetValue("UPDATE", out cmd))
                throw new Exception("This Command doesn't exist!");
            SqlParameter pkSqlParameter = new SqlParameter("@ID", SqlDbType.Int)
            {
                Value = _pkKeyValuePair.Value.GetValue(val)
            };
            cmd.Parameters.Add(pkSqlParameter);
            foreach (var field in _fieldsMatchDictionary)
            {
                SqlParameter p = new SqlParameter(field.Key, field.Value.GetSqlDbType(val))
                {
                    Value = field.Value.GetValue(val)
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

            SqlParameter pkSqlParameter = new SqlParameter("@ID", SqlDbType.Int)
            {
                Value = _pkKeyValuePair.Value.GetValue(val)
            };
            cmd.Parameters.Add(pkSqlParameter);
            _mapperSqlConnection.ExecuteTransaction(cmd);
        }
    }
}
