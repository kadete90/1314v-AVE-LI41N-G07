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


    static SqlCommand MakeUpdateCmd(SqlConnection conSql)
    {
        string strUpdate = "UPDATE Products SET ProductName = @name WHERE ProductID = @id";

        SqlParameter name = new SqlParameter("@name", SqlDbType.NVarChar);
        SqlParameter id = new SqlParameter("@id", SqlDbType.Int);

        SqlCommand cmd = conSql.CreateCommand();

        cmd.Parameters.Add(name);
        cmd.Parameters.Add(id);

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
                    cmdUpdate = MakeUpdateCmd(conSql))
                {
                    cmdRead.Transaction = tran;
                    cmdUpdate.Transaction = tran;

                    cmdRead.Parameters["@id"].Value = 9;
                    
                    SqlDataReader product = cmdRead.ExecuteReader();
                    product.Read();
                    string prodName = product["ProductName"].ToString();
                    Console.WriteLine(prodName);
                    product.Close();

                    cmdUpdate.Parameters["@id"].Value = 9;
                    cmdUpdate.Parameters["@name"].Value = "Casa de Cafe Bastos";
                    cmdUpdate.ExecuteNonQuery();

                    product = cmdRead.ExecuteReader();
                    product.Read();
                    Console.WriteLine(product["ProductName"]);
                    product.Close();
                    
                } // Dispose commands
                tran.Rollback();
            }// Dispose transaction
        }// Dispose connection
    }
}