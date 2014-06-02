using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LinFu.DynamicProxy;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.Entities;

namespace SqlMapperFw
{

    class Program
    {

        public static string GetConnectionString()
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = @"(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };

            using (SqlConnection connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    connection.Close();
                }
                catch
                {
                    Console.WriteLine("Connection With Problems!!");
                    return null;
                }
            }
            return connectionStringBuilder.ConnectionString;
        }

        static void Main()
        {

            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = @"(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };

            Builder b = new Builder(connectionStringBuilder, true);
            IDataMapper<Product> orderMapper = b.Build<Product>(); //1ªparte 1.
            orderMapper.Update(new Product());
            orderMapper.Insert(new Product());
            orderMapper.Delete(new Product());
            foreach (Product p in orderMapper.GetAll())
            {
                Console.WriteLine("> " + p.ProductID);
            }
            

            //IDataMapper<Customer> custMapper = b.Build<Customer>(); //1ªparte 1.
            //IDataMapper<Employee> empMapper = b.Build<Employee>(); //1ªparte 1.


            //IDataMapper<Product> prodMapper = b.Build<Product>(); //1ªparte 1.
            //IEnumerable<Product> prods = prodMapper.GetAll();
            //prods.Where("CategoryID = 7").Where("UnitsinStock > 30"); //1ªparte 2.
        }
    }
}
