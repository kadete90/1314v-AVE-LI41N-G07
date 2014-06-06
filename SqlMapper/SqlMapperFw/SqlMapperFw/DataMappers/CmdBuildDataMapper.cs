using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using SqlMapperFw.Reflection;

namespace SqlMapperFw.DataMappers
{
    public class CmdBuilder<T> : IDataMapper<T>
    {
        public String TableName;
        public String PKPropName;
        private readonly BindFields<T> BindFields;
        private readonly SqlConnection _conSql;
        readonly Dictionary<String, MemberInfo> FieldsNamesDictionary = new Dictionary<String, MemberInfo>();

        public CmdBuilder(SqlConnection strBuilder)
        {
            _conSql = strBuilder;
            Type type = typeof(T);

            DBTableNameAttribute tableNameAttribute = (DBTableNameAttribute)type.GetCustomAttribute(typeof(DBTableNameAttribute));
            TableName = tableNameAttribute != null ? tableNameAttribute.Name : type.Name;

            foreach (MemberInfo mi in type.GetMembers())
            {
                MemberInfo realTypeInfo = mi.GetValidType();
                if (realTypeInfo == null)
                    continue;

                DBFieldNameAttribute fieldNameAttribute = (DBFieldNameAttribute)realTypeInfo.GetCustomAttribute(typeof(DBFieldNameAttribute));
                var EDFieldName = (fieldNameAttribute != null)
                    ? fieldNameAttribute.Name : realTypeInfo.Name;

                if (realTypeInfo.MemberType == MemberTypes.Property)
                    EDFieldName = EDFieldName.Replace("set_", "").Replace("get_", ""); //melhorar

                if (!FieldsNamesDictionary.ContainsKey(EDFieldName))
                    FieldsNamesDictionary.Add(EDFieldName, mi); //key: bd_fieldname, value: ed_fieldname

                //não é aceite mais que uma PK
                if (PKPropName != null) //TODO: alterar quando tiver chave composta
                    continue;

                PropPKAttribute pkAttribute = (PropPKAttribute)realTypeInfo.GetCustomAttribute(typeof(PropPKAttribute));
                if (pkAttribute != null)
                    PKPropName = EDFieldName;
            }

            BindFields = new BindFields<T>(FieldsNamesDictionary);
        }

        //minimizar reflexão neste método
        public IEnumerable<T> GetAll()
        {
            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction())
            {
                String DBfields = "";
                SqlDataReader rd;
                try
                {
                    SqlCommand cmd = _conSql.CreateCommand();
                    if (FieldsNamesDictionary.Count == 0)
                        throw new Exception("List of fields empty!!");
                    foreach (String fieldName in FieldsNamesDictionary.Keys)
                    {
                        DBfields += fieldName + ", ";
                    }
                    if(DBfields != "")
                        DBfields = DBfields.Substring(0, DBfields.Length - 2); //remove última vírgula
                    cmd.CommandText = "SELECT " + DBfields  + " FROM " + TableName;
                    cmd.Transaction = sqlTransaction;

                    rd = cmd.ExecuteReader();
                    
                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occur on GetAll(): \n" + exception);
                    Console.WriteLine("Rollback from this transaction...");
                    sqlTransaction.Rollback();
                    yield break;
                }

                foreach (var DBRowValues in rd.AsEnumerable())
                {
                    //TODO: bind (tentar minimizar o uso da reflexão)
                    yield return BindFields.bind(DBRowValues);
                }
                rd.Close();
                sqlTransaction.Commit();
            }
        }

        //minimizar reflexão neste método
        public void Update(T val)
        {
            throw new NotImplementedException();
            //const string strUpdate = "UPDATE Products SET ProductName = @name WHERE ProductID = @id";

            //SqlParameter name = new SqlParameter("@name", SqlDbType.NVarChar);
            //SqlParameter id = new SqlParameter("@id", SqlDbType.Int);

            //SqlCommand cmd = _conSql.CreateCommand();

            //cmd.Parameters.Add(name);
            //cmd.Parameters.Add(id);

            //cmd.CommandText = strUpdate;

        }

        //minimizar reflexão neste método
        public void Delete(T val)
        {
            throw new NotImplementedException();
        }

        //minimizar reflexão neste método
        public void Insert(T instance)
        {
            throw new NotImplementedException();

            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction())
            {
                String DBfields = "";
                SqlDataReader rd;
                try
                {
                    SqlCommand cmd = _conSql.CreateCommand();
                    if (FieldsNamesDictionary.Count == 0)
                        throw new Exception("List of fields empty!!");
                    foreach (String fieldName in FieldsNamesDictionary.Keys)
                    {
                        if (PKPropName != fieldName) //identity
                            DBfields += fieldName + ", ";
                    }

                    //const string strUpdate = "INSERT INTO Products (ProductName, UnitPrice, UnitsInStock) VALUES (@name, @price, @stock)";


                    if (DBfields != "")
                        DBfields = DBfields.Substring(0, DBfields.Length - 2); //remove última vírgula
                    cmd.CommandText = "INSERT INTO " + TableName + " ("+ DBfields + ") VALUES (" + getTypeValues(instance) + ")";
                    cmd.Transaction = sqlTransaction;

                    rd = cmd.ExecuteReader();

                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occur on Insert(): \n" + exception);
                    Console.WriteLine("Rollback from this transaction...");
                    sqlTransaction.Rollback();
                    return;
                }
                rd.Close();
                sqlTransaction.Commit();
            }
            //const string strUpdate = "INSERT INTO Products (ProductName, UnitPrice, UnitsInStock) OUTPUT INSERTED.ProductID VALUES (@name, @price, @stock)";

            //SqlCommand cmd = _conSql.CreateCommand();

            //cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
            //cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Money));
            //cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.SmallInt));

            //cmd.CommandText = strUpdate;

        }
    }
}
