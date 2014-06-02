using System;
using System.Data;
using System.Data.SqlClient;

class App1 { 
    static void Main(){
        SqlConnection con = new SqlConnection();
        try
        {
            con.ConnectionString = @"
                Data Source=DRAGAO\SQLEXPRESS;
				Initial Catalog=Northwind;
                Integrated Security=True";
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT ProductID, ProductName, UnitPrice, UnitsInStock FROM Products";
            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
                Console.WriteLine(dr["ProductName"]);
        }
        finally
        {
            if (con.State != ConnectionState.Closed)
                con.Dispose();
        }
    }
}