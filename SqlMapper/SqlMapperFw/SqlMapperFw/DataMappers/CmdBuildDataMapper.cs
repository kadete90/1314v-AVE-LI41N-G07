using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using SqlMapperFw.Reflection;

namespace SqlMapperFw.DataMappers
{
    public class CmdBuilder<T> : IDataMapper<T>
    {
        private readonly SqlConnection _conSql;
        public string TableName;
        public List<String> DBFieldNamesList = new List<string>();
        public string PKPropName;
        private readonly T TInstance;

        public CmdBuilder(SqlConnection strBuilder)
        {
            _conSql = strBuilder;
            TInstance = Activator.CreateInstance<T>();
            Type type = typeof(T);
            DBTableNameAttribute tableNameAttribute = (DBTableNameAttribute)type.GetCustomAttribute(typeof(DBTableNameAttribute));
            TableName = tableNameAttribute != null ? tableNameAttribute.Name : type.Name;

            foreach (MemberInfo pi in type.GetMembers(BindingFlags.Public))
            {
                if (pi == null) continue;

                // /!\ Não funciona
                DBFieldNameAttribute fieldNameAttribute = (DBFieldNameAttribute)pi.GetCustomAttribute(typeof(DBFieldNameAttribute));
                var propName = fieldNameAttribute != null
                    ? fieldNameAttribute.Name : pi.Name.Replace("set_", "").Replace("get_", ""); //melhorar
                DBFieldNamesList.Add(propName);

                if (PKPropName != null) //alterar isto quando tiver chave composta
                    continue;

                PropPKAttribute pkAttribute = (PropPKAttribute)pi.GetCustomAttribute(typeof(PropPKAttribute));
                if (pkAttribute != null)
                    PKPropName = propName;
            }

        }

        public IEnumerable<T> GetAll()
        {
            SqlDataReader rd = null;
            using (SqlTransaction sqlTransaction = _conSql.BeginTransaction())
            {
                String DBfields = "";
                try
                {
                    SqlCommand cmd = _conSql.CreateCommand();
                    if (DBFieldNamesList.Count == 0)
                        throw new Exception("List of fields empty!!");
                    foreach (String fieldName in DBFieldNamesList)
                    {
                        DBfields += fieldName + ", ";
                    }
                    if(DBfields != "")
                        DBfields = DBfields.Substring(0, DBfields.Length - 2); //remove última vírgula
                    cmd.CommandText = "SELECT " + DBfields  + " FROM @tableName";
                    cmd.Parameters.Add(TableName);
                    rd = cmd.ExecuteReader();
                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occur on GetAll(): " + exception);
                    Console.WriteLine("Rollback from this transaction...");
                    sqlTransaction.Rollback();
                }
                sqlTransaction.Commit();
            }
            if (rd == null)
            {
                throw new Exception("This Table doesn't have row's!");
            }

            BindFields<T> b = new BindFields<T>();
            foreach (var DBRowValues in rd.AsEnumerable())
            {
                yield return b.bind(TInstance, DBRowValues, DBFieldNamesList);
            }   
            rd.Close();
        }

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

        public void Delete(T val)
        {
            throw new NotImplementedException();
        }

        public void Insert(T val)
        {
            throw new NotImplementedException();
            //const string strUpdate = "INSERT INTO Products (ProductName, UnitPrice, UnitsInStock) OUTPUT INSERTED.ProductID VALUES (@name, @price, @stock)";

            //SqlCommand cmd = _conSql.CreateCommand();

            //cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
            //cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Money));
            //cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.SmallInt));

            //cmd.CommandText = strUpdate;

        }
    }
}
