using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SqlMapperFw.DataMappers;

namespace SqlMapperFw.BuildMapper
{
    public class CmdBuilder <T> : IDataMapper<T>
    {
        private readonly SqlConnection _conSql;

        public CmdBuilder(SqlConnection strBuilder)
        {
            _conSql = strBuilder;
        }

        public IEnumerable<T> GetAll()
        {
            //1. utilizar reflexão para obter valor dos custtom attributes e propriedades
            //2. criar comando adicionando os parametros obtidos no ponto anterior
            //3. executar comando e obter o dataReader
            //4. fazer bind (utilizando novamente a reflexao) do dataReader para Ienumerable
            //5. retornar ienumerable
            T type = Activator.CreateInstance<T>();
            List<T> list = new List<T>();

            using (_conSql.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {

                    SqlDataReader rd = Cmd.ExecuteReader();
                    Object ret = GetSqlDataElement(rd, args);
                    list.Add();
                }
                catch
                (SqlException sqlException)
                {

                }
        }



            IEnumerable<T> iEnumerable = list.ToList();
            foreach (T t in iEnumerable)
            {
                yield return t;
            }

            //SqlCommand cmd = _conSql.CreateCommand();
            //cmd.CommandText = "SELECT ProductID, ProductName, UnitPrice, UnitsInStock FROM Products";

            //SqlParameter id = new SqlParameter("@id", SqlDbType.Int);
            //cmd.Parameters.Add(id);

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

            return cmd;
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

            return cmd;

        }
    }
}
