using System.Data;
using System.Data.SqlClient;

namespace SqlMapperFw.BuildMapper
{
    public class ProductsCmdBuilder
    {
        public static SqlCommand MakeReadCmd(SqlConnection conSql)
        {
            SqlCommand cmd = conSql.CreateCommand();
            cmd.CommandText = "SELECT ProductID, ProductName, UnitPrice, UnitsInStock FROM Products WHERE ProductID = @id";

            SqlParameter id = new SqlParameter("@id", SqlDbType.Int);
            cmd.Parameters.Add(id);

            return cmd;
        }


        public static SqlCommand MakeInsert(SqlConnection conSql)
        {
            const string strUpdate = "INSERT INTO Products (ProductName, UnitPrice, UnitsInStock) OUTPUT INSERTED.ProductID VALUES (@name, @price, @stock)";

            SqlCommand cmd = conSql.CreateCommand();

            cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Money));
            cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.SmallInt));

            cmd.CommandText = strUpdate;

            return cmd;
        }

        public static SqlCommand MakeUpdateCmd(SqlConnection conSql)
        {
            const string strUpdate = "UPDATE Products SET ProductName = @name WHERE ProductID = @id";

            SqlParameter name = new SqlParameter("@name", SqlDbType.NVarChar);
            SqlParameter id = new SqlParameter("@id", SqlDbType.Int);

            SqlCommand cmd = conSql.CreateCommand();

            cmd.Parameters.Add(name);
            cmd.Parameters.Add(id);

            cmd.CommandText = strUpdate;

            return cmd;
        }
    }
}
