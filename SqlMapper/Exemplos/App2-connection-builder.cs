using System;
using System.Data;
using System.Data.SqlClient;

class App2 { 
    static void Main(){
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.DataSource = @"DRAGAO\SQLEXPRESS";
        builder.IntegratedSecurity = true;
        builder.InitialCatalog = "Northwind";
        SqlConnection conSql = new SqlConnection(builder.ConnectionString);
        try
        {
            conSql.Open();
            if (conSql.State == System.Data.ConnectionState.Open)
                Console.WriteLine("Ligação Aberta:\n{0}", builder.ConnectionString);
            else
                Console.WriteLine("Hum.... Algo está errado:\n{0}", builder.ConnectionString);
        }
        finally
        {
            if (conSql.State != ConnectionState.Closed)
                conSql.Close();
        }

    }
}