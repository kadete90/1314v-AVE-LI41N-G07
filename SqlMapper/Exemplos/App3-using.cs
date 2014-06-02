using System;
using System.Data;
using System.Data.SqlClient;

class App2 { 
    static void Main(){
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.DataSource = @"DRAGAO\SQLEXPRESS";
        builder.IntegratedSecurity = true;
        builder.InitialCatalog = "Northwind";
        using(SqlConnection conSql = new SqlConnection(builder.ConnectionString))
        {
            using (SqlCommand cmd = conSql.CreateCommand())
            {
                cmd.CommandText = "SELECT ProductID, ProductName, UnitPrice, UnitsInStock FROM Products";
                conSql.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    Console.WriteLine(dr["ProductName"]);
            }
        }
    }
}