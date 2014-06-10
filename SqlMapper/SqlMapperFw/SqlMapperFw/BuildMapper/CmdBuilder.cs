using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using SqlMapperFw.DataMappers;
using SqlMapperFw.Reflection;

namespace SqlMapperFw.BuildMapper
{
    public class CmdBuilder<T> : IDataMapper<T>
    {
        public String TableName;
        readonly BindFields<T> _bindFields;
        readonly SqlConnection _conSql;
        readonly KeyValuePair<String, MemberInfo> _pkKeyValuePair; //DB_Field_Name
        readonly Dictionary<String, MemberInfo> _fieldsMatchDictionary = new Dictionary<String, MemberInfo>(); //DB_Field_Name, DE_Field_Info
        readonly Dictionary<String, SqlCommand> _commandsDictionary = new Dictionary<String, SqlCommand>(); //TypeCommand, Command

        public CmdBuilder(SqlConnection strBuilder)
        {
            Type type = typeof(T);
            _conSql = strBuilder;

            TableName = type.getTableName();

            foreach (MemberInfo mi in type.GetMembers())
            {
                MemberInfo validMemberInfo = mi.GetValidType();
                if (validMemberInfo == null)
                    continue;

                var DEFieldName = validMemberInfo.getDBFieldName();

                if (!_fieldsMatchDictionary.ContainsKey(DEFieldName) && !validMemberInfo.isPrimaryKey())
                    _fieldsMatchDictionary.Add(DEFieldName, mi);

                //não é aceite mais que uma PK
                if (_pkKeyValuePair.Key != null) //TODO: alterar quando tiver chave composta
                    continue;
                if (validMemberInfo.isPrimaryKey())
                    _pkKeyValuePair = new KeyValuePair<string, MemberInfo>(DEFieldName, validMemberInfo);

            }

            if (_fieldsMatchDictionary.Count == 0 || _pkKeyValuePair.Key == null)
                throw new Exception("No domain entity fields recognized or no PK defined!!");

            //SqlParameter[] sqlParameters = new SqlParameter[_fieldsMatchDictionary.Count];

            //Criação dos comandos
            String DBfields = "";
            foreach (String fieldName in _fieldsMatchDictionary.Keys)
                DBfields += fieldName + ", ";

            if (DBfields != "")
                DBfields = DBfields.Substring(0, DBfields.Length - 2); //remove última vírgula

            SqlCommand cmd = _conSql.CreateCommand();
            cmd.CommandText = "SELECT " + DBfields + " FROM " + TableName;
            _commandsDictionary.Add("SELECT", cmd);

            ////TODO UPDATE
            cmd = _conSql.CreateCommand();
            cmd.CommandText = "UPDATE " + TableName + " SET ProductName = @name WHERE ProductID = @id";
            _commandsDictionary.Add("UPDATE", cmd);

            ////TODO DELETE
            cmd = _conSql.CreateCommand();
            cmd.CommandText = "DELETE FROM " + TableName + " WHERE " + _pkKeyValuePair.Key + " = @ID";
            _commandsDictionary.Add("DELETE", cmd);

            ////TODO INSERT
            cmd = _conSql.CreateCommand();
            cmd.CommandText = "INSERT INTO " + TableName + " (" + DBfields + ")" + " VALUES (" + ReflectionMethods.StringBuilder(_fieldsMatchDictionary.Keys) + ") SET @ID = SCOPE_IDENTITY();";
            _commandsDictionary.Add("INSERT", cmd);

            _bindFields = new BindFields<T>(_fieldsMatchDictionary);
        }

        //minimizar reflexão neste método
        public IEnumerable<T> GetAll()
        {
            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction(IsolationLevel.ReadCommitted))
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
                    yield return _bindFields.bind(DBRowValues);
                }
                rd.Close();
                sqlTransaction.Commit();
            }
        }

        //minimizar reflexão neste método
        public void Insert(T val)
        {
            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction())
            {
                try
                {
                    SqlCommand cmd;
                    if (!_commandsDictionary.TryGetValue("INSERT", out cmd))
                        throw new Exception("This Command doesn't exist!");
                    cmd.Parameters.Add("@ID", _pkKeyValuePair.Value.GetSqlDbType<T>(val)).Direction = ParameterDirection.Output;
                    foreach (var field in _fieldsMatchDictionary)
                    {
                        if (_pkKeyValuePair.Key != field.Key) //identity
                        {
                            SqlParameter p = new SqlParameter(field.Key, field.Value.GetSqlDbType<T>(val));
                            p.Value = field.Value.GetValue(val);
                            cmd.Parameters.Add(p);
                        }
                        
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
            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction(IsolationLevel.Serializable))
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
