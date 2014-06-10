using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using SqlMapperFw.DataMappers;
using SqlMapperFw.Reflection;

namespace SqlMapperFw.BuildMapper
{
    public class CmdBuilder<T> : IDataMapper<T>
    {
        public String TableName;
        public String PKPropName;
        readonly BindFields<T> _bindFields;
        readonly SqlConnection _conSql;
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

                if (!_fieldsMatchDictionary.ContainsKey(DEFieldName))
                    _fieldsMatchDictionary.Add(DEFieldName, mi);

                //não é aceite mais que uma PK
                if (PKPropName != null) //TODO: alterar quando tiver chave composta
                    continue;
                if (validMemberInfo.isPrimaryKey())
                    PKPropName = DEFieldName;

            }
           
            if (_fieldsMatchDictionary.Count == 0)
                throw new Exception("No domain entity fields recognized!!");

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
            cmd.CommandText = "DELETE FROM " + TableName + " WHERE "+ PKPropName +" = ";
            _commandsDictionary.Add("DELETE", cmd);

            ////TODO INSERT
            cmd = _conSql.CreateCommand();
            cmd.CommandText = "INSERT INTO" + TableName + " VALUES (" + DBfields + ")";
            _commandsDictionary.Add("INSERT", cmd);

            _bindFields = new BindFields<T>(_fieldsMatchDictionary);
        }

        //minimizar reflexão neste método
        public IEnumerable<T> GetAll()
        {
            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction())
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
                    Console.WriteLine("An error occur on GetAll(): \n" + exception+ "\nRolling back this transaction...");
                    sqlTransaction.Rollback();
                    yield break;
                }

                foreach (var DBRowValues in rd.AsEnumerable())
                {
                    //TODO: bind (tentar minimizar o uso da reflexão)
                    yield return _bindFields.bind(DBRowValues);
                }

                rd.Close();
                sqlTransaction.Commit();
            }
        }

        //minimizar reflexão neste método
        public void Insert(T instance)
        {
            throw new NotImplementedException();
        }

        //minimizar reflexão neste método
        public void Update(T val)
        {
            throw new NotImplementedException();
        }

        //minimizar reflexão neste método
        public void Delete(T val)
        {
            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction())
            {
                SqlDataReader rd;
                try
                {
                    SqlCommand cmd;
                    if (!_commandsDictionary.TryGetValue("DELETE", out cmd))
                        throw new Exception("This Command doesn't exist!");

                    cmd.CommandText += val.GetType();
                    cmd.Transaction = sqlTransaction;
                    rd = cmd.ExecuteReader();
                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occur on Delete(): \n" + exception + "\nRolling back this transaction...");
                    sqlTransaction.Rollback();
                    return;
                }

                rd.Close();
                sqlTransaction.Commit();
            }
        }
    }
}
