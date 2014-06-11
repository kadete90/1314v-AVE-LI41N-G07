using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using SqlMapperFw.DataMappers;
using SqlMapperFw.Reflection;
using SqlMapperFw.Reflection.Binder;

namespace SqlMapperFw.BuildMapper
{
    public class CmdBuilderDataMapper<T> : IDataMapper<T>
    {
        readonly SqlConnection _sqlConnection;
        readonly List<AbstractBindMember> _bindMembers = new List<AbstractBindMember>();

        readonly String _tableName;
        readonly KeyValuePair<String, MemberInfo> _pkKeyValuePair; //DB_PK_Name, DE_PK_Info
        readonly Dictionary<String, MemberInfo> _fieldsMatchDictionary = new Dictionary<String, MemberInfo>(); //DB_Field_Name, DE_Field_Info
        
        readonly Dictionary<String, SqlCommand> _commandsDictionary = new Dictionary<String, SqlCommand>(); //TypeCommand, Command

        public CmdBuilderDataMapper(SqlConnection mySql, IEnumerable<Type> bindMembersTypes)
        {
            _sqlConnection = mySql;

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
            cmd.CommandText = "SELECT " + DBfields + " FROM " + _tableName;
            _commandsDictionary.Add("SELECT", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "INSERT INTO " + _tableName + " (" + DBfields + ")" + " VALUES (" + ReflectionMethods.StringBuilder(_fieldsMatchDictionary.Keys) + ") SET @ID = SCOPE_IDENTITY();";
            _commandsDictionary.Add("INSERT", cmd);

            //TODO : UPDATE
            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "UPDATE " + _tableName + " SET ProductName = @name WHERE ProductID = @id";
            _commandsDictionary.Add("UPDATE", cmd);

            cmd = _sqlConnection.CreateCommand();
            cmd.CommandText = "DELETE FROM " + _tableName + " WHERE " + _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("DELETE", cmd);


        }

        //TODO : minimizar reflexão neste método
        public IEnumerable<T> GetAll()
        {
            if (_sqlConnection == null || _sqlConnection.State == ConnectionState.Closed)
                throw new Exception("Open Connection needed to execute command!!");

            using (SqlTransaction sqlTransaction = _sqlConnection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                SqlDataReader rd;
                try
                {
                    SqlCommand cmd;
                    if(!_commandsDictionary.TryGetValue("SELECT", out cmd))
                        throw new Exception("This Command doesn't exist!");
                    cmd.Transaction = sqlTransaction;
                    rd = cmd.ExecuteReader();
                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occur on GetAll(): \n" + exception + "\nRolling back this transaction...");
                    sqlTransaction.Rollback();
                    yield break;
                }
                foreach (var DBRowValues in rd.AsEnumerable())
                {
                    T newInstance = (T)Activator.CreateInstance(typeof(T));
                    Dictionary<string, MemberInfo>.ValueCollection.Enumerator MemberInfos = _fieldsMatchDictionary.Values.GetEnumerator();
                    foreach (object value in DBRowValues)
                    {
                        if (!MemberInfos.MoveNext())
                                break;
                        foreach (AbstractBindMember bm in _bindMembers)
                            if (bm.bind(newInstance, MemberInfos.Current, value))
                                break;
                    }
                    yield return newInstance;
                }
                rd.Close();
                sqlTransaction.Commit();
            }
        }

        //minimizar reflexão neste método
        public void Insert(T val)
        {
            if (_sqlConnection == null || _sqlConnection.State == ConnectionState.Closed)
                throw new Exception("Open Connection needed to execute command!!");

            using (SqlTransaction sqlTransaction = _sqlConnection.BeginTransaction())
            {
                try
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
                    
                    cmd.Transaction = sqlTransaction;
                    cmd.ExecuteNonQuery();
                    _pkKeyValuePair.Value.SetValue(val, cmd.Parameters[0].Value);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occur on Delete(): \n" + exception + "\nRolling back this transaction...");
                    sqlTransaction.Rollback();
                    return;
                }
                sqlTransaction.Commit();
            }
            
        }

        //minimizar reflexão neste método
        public void Update(T val)
        {
            throw new NotImplementedException();
        }

        //minimizar reflexão neste método
        public void Delete(T val)
        {
            if (_sqlConnection == null || _sqlConnection.State == ConnectionState.Closed)
                throw new Exception("Open Connection needed to execute command!!");

            using (SqlTransaction sqlTransaction = _sqlConnection.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    SqlCommand cmd;
                    if (!_commandsDictionary.TryGetValue("DELETE", out cmd))
                        throw new Exception("This Command doesn't exist!");

                    SqlParameter pkSqlParameter = new SqlParameter("@ID", SqlDbType.Int)
                    {
                        Value = _pkKeyValuePair.Value.GetValue(val)
                    };
                    cmd.Parameters.Add(pkSqlParameter);
                    cmd.Transaction = sqlTransaction;


                    //if (pi.ParameterType == typeof(int))
                    //{
                    //    pm[i] = new SqlParameter("@" + pi.Name, SqlDbType.Int);
                    //}
                    //else if (pi.ParameterType == typeof(double))
                    //{
                    //    pm[i] = new SqlParameter("@" + pi.Name, SqlDbType.Float);
                    //}
                    //else if (pi.ParameterType == typeof(string))
                    //{
                    //    pm[i] = new SqlParameter("@" + pi.Name, SqlDbType.VarChar, 50);
                    //}
                    //cmd.Parameters.Add();

                    if (cmd.ExecuteNonQuery() == 0)
                        Console.WriteLine("No row(s) affected!");

                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occur on Delete(): \n" + exception + "\nRolling back this transaction...");
                    sqlTransaction.Rollback();
                    return;
                }
                sqlTransaction.Commit();
            }
        }
    }
}
