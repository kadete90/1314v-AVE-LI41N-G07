using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlMapperClient.Entities;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.MySqlConnection;

namespace SqlMapperTests.WithoutAssociations.MultipleConnection
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
            builder = new Builder(connectionStringBuilder, typeof(MultiConnection<>));
            productDataMapper = builder.Build<Product>();
            CleanToDefault();
        }
        //SqlCommand cmd_id = new SqlCommand("SET IDENTITY_INSERT PRODUCTS ON");
        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd_del = new SqlCommand("DELETE FROM Products WHERE ProductId > 78", conSql);
                conSql.Open();
                cmd_del.ExecuteNonQuery();
                conSql.Close();
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("\tBEGINING MULTIPLE CONNECTION TEST");
                Console.WriteLine("-----------------------------------------------------");
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            builder.CloseConnection();
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\tENDING MULTIPLE CONNECTION TEST");
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
        public void TestInsertProduct()
        {
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\t\tSINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
            Product p = new Product
            {
                ProductName = "OLA",
                QuantityPerUnit = "100",
                UnitPrice = 100,
                UnitsInStock = 50,
                UnitsOnOrder = 30
            };
            productDataMapper.Insert(p);
            Assert.IsNotNull(p.id);
            Console.WriteLine("Inserted new product with id = {0}",p.id);
            Console.WriteLine("-----------------------------------------------------");
        }

        [TestMethod]
        public void TestDeleteProduct()
        {
            Console.WriteLine("-----------------------------------------------------");
            Product product = new Product { id = 78 };
            productDataMapper.Delete(product);
            Console.WriteLine(" productDataMapper.Delete(product)");
            int count = productDataMapper.GetAll().Count();
            Assert.AreEqual(77, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
