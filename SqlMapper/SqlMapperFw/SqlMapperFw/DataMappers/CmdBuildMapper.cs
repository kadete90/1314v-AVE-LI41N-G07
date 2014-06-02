using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using SqlMapperFw.BuildMapper;

namespace SqlMapperFw.DataMappers
{
    public class CmdBuilder<T> : IDataMapper<T>
    {
        private readonly SqlConnection _conSql;
        public string TableName;
        public List<String> DBFieldNamesList = new List<string>();
        public string PKPropName;

        public CmdBuilder(SqlConnection strBuilder, Type type)
        {
            _conSql = strBuilder;
            TableNameAttribute tableNameAttribute = (TableNameAttribute)Attribute.GetCustomAttribute(type, typeof(TableNameAttribute));
            TableName = tableNameAttribute != null ? tableNameAttribute.Name : type.Name;

            foreach (MemberInfo pi in type.GetMembers())
            {
                if (pi == null) continue;
                DBFieldNameAttribute propNameAttribute = (DBFieldNameAttribute)Attribute.GetCustomAttribute(type, typeof(DBFieldNameAttribute));
                var propName = propNameAttribute != null ? propNameAttribute.Name : pi.Name;
                DBFieldNamesList.Add(propName);

                PropPKAttribute pkAttribute = (PropPKAttribute)Attribute.GetCustomAttribute(type, typeof(PropPKAttribute));
                if (pkAttribute != null)
                {
                    PKPropName = propName;
                }
            }

        }

        public IEnumerable<T> GetAll()
        {
            //1. utilizar reflexão para obter valor dos custtom attributes e propriedades
            //2. criar comando adicionando os parametros obtidos no ponto anterior
            //3. executar comando e obter o dataReader
            //4. fazer bind (utilizando novamente a reflexao) do dataReader para Ienumerable
            //5. retornar ienumerable

            List<T> list = new List<T>();

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
                    DBfields = DBfields.Substring(0, DBfields.Length - 2);
                    cmd.CommandText = "SELECT " + DBfields  + " FROM @tableName";
                    cmd.Parameters.Add(TableName);
                    SqlDataReader rd = cmd.ExecuteReader();
                    // TO DO
                }
                catch (Exception exception)
                {
                    Console.WriteLine("And Error Occur on GetAll()");
                    sqlTransaction.Rollback();
                }
                sqlTransaction.Commit();
            }

            IEnumerable<T> iEnumerable = list.ToList();
            foreach (T t in iEnumerable)
            {
                yield return t;
            }
        }

        public void Update(T val)
        {
            const string strUpdate = "UPDATE Products SET ProductName = @name WHERE ProductID = @id";

            SqlParameter name = new SqlParameter("@name", SqlDbType.NVarChar);
            SqlParameter id = new SqlParameter("@id", SqlDbType.Int);

            SqlCommand cmd = _conSql.CreateCommand();

            cmd.Parameters.Add(name);
            cmd.Parameters.Add(id);

            cmd.CommandText = strUpdate;

        }

        public void Delete(T val)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(T val)
        {
            const string strUpdate = "INSERT INTO Products (ProductName, UnitPrice, UnitsInStock) OUTPUT INSERTED.ProductID VALUES (@name, @price, @stock)";

            SqlCommand cmd = _conSql.CreateCommand();

            cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Money));
            cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.SmallInt));

            cmd.CommandText = strUpdate;

        }
    }
}
