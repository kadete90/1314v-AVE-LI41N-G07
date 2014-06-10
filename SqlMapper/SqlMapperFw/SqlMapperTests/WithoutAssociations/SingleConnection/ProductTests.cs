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
       
        [TestInitialize]
        public void Setup()
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };
            builder = new Builder(connectionStringBuilder, typeof(SingleConnection<>));
            productDataMapper = builder.Build<Product>();
        }

        [TestCleanup]
        public void TearDown()
        {
           builder.CloseConnection();
        }

        [TestMethod]
        public void TestReadAllProducts()
        { 
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\t\tSINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
            int count = productDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllProducts Count: " + count);
            Assert.AreEqual(77, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
