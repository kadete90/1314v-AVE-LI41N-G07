using System;
using System.Data;
using System.Data.SqlClient;

class App4 {

    static SqlCommand MakeReadCmd(SqlConnection conSql)
    {
        SqlCommand cmd = conSql.CreateCommand();
        cmd.CommandText = "SELECT ProductID, ProductName, UnitPrice, UnitsInStock FROM Products WHERE ProductID = @id";

        SqlParameter id = new SqlParameter("@id", SqlDbType.Int);
        cmd.Parameters.Add(id);

        return cmd;
    }


    static SqlCommand MakeInsert(SqlConnection conSql)
    {
        string strUpdate = "INSERT INTO Products (ProductName, UnitPrice, UnitsInStock) OUTPUT INSERTED.ProductID VALUES (@name, @price, @stock)";

        SqlCommand cmd = conSql.CreateCommand();

        cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar));
        cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Money));
        cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.SmallInt));

        cmd.CommandText = strUpdate;

        return cmd;
    }


    static void Main(){
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.DataSource = @"DRAGAO\SQLEXPRESS";
        builder.IntegratedSecurity = true;
        builder.InitialCatalog = "Northwind";

        using(SqlConnection conSql = new SqlConnection(builder.ConnectionString))
        {
            conSql.Open();
            using (SqlTransaction tran = conSql.BeginTransaction())
            {
                using (
                    SqlCommand cmdRead = MakeReadCmd(conSql),
                    cmdInsert = MakeInsert(conSql))
                {
                    cmdRead.Transaction = tran;
                    cmdInsert.Transaction = tran;

                    cmdInsert.Parameters["@name"].Value = "Cafe da Alcobia";
                    cmdInsert.Parameters["@price"].Value = 100.0;
                    cmdInsert.Parameters["@stock"].Value = 5550;
                    int prodId = (int) cmdInsert.ExecuteScalar();

                    cmdRead.Parameters["@id"].Value = prodId;
                    SqlDataReader product = cmdRead.ExecuteReader();
                    product.Read();
                    Console.WriteLine(product["ProductName"]);
                    product.Close();
                    
                } // Dispose commands
                tran.Rollback();
            }// Dispose transaction
        }// Dispose connection
    }
}