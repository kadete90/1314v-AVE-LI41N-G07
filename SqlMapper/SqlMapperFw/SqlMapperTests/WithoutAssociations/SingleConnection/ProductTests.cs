using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlMapperClient.Entities;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.MySqlConnection;

namespace SqlMapperTests.WithoutAssociations.SingleConnection
{
    [TestClass]
    public class ProductTests
    {
        Builder builder;
        IDataMapper<Product> productDataMapper;
        SqlConnectionStringBuilder connectionStringBuilder;
        [TestInitialize]
        public void Setup()
        {
            connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };
            builder = new Builder(connectionStringBuilder, typeof(SingleConnection<>));
            productDataMapper = builder.Build<Product>();
            CleanToDefault();

            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\tBEGINING SINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd_del = new SqlCommand("DELETE FROM Products WHERE ProductId > 78", conSql);
                conSql.Open();
                cmd_del.ExecuteNonQuery();
                conSql.Close();
            }
        }

        [TestCleanup]
        public void TearDown()
        {
           builder.CloseConnection();
           Console.WriteLine("-----------------------------------------------------");
           Console.WriteLine("\tENDING SINGLE CONNECTION TEST");
           Console.WriteLine("-----------------------------------------------------");
        }

        [TestMethod]
        public void TestReadAllProducts()
        {
            Console.WriteLine("-----------------------------------------------------");
            int count = productDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllProducts Count: " + count);
            Assert.AreEqual(77, count);
            Console.WriteLine("-----------------------------------------------------");
        }

        [TestMethod]
        public void TestDeleteProduct()
        {
            Console.WriteLine("-----------------------------------------------------");
            Product product = new Product {id = 78};
            productDataMapper.Delete(product);
            Console.WriteLine(" productDataMapper.Delete(product)");
            int count = productDataMapper.GetAll().Count();
            Assert.AreEqual(77, count);
            Console.WriteLine("-----------------------------------------------------");
        }

    }
}
