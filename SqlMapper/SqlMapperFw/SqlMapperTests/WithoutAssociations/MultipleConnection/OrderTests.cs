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
    public class OrderTests
    {
        Builder builder;
        IDataMapper<Order> orderDataMapper;

        [TestInitialize]
        public void Setup()
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };
            builder = new Builder(connectionStringBuilder, typeof(MultiConnection<>));
            orderDataMapper = builder.Build<Order>();
        }

        [TestCleanup]
        public void TearDown()
        {
            builder.CloseConnection();
        }

        [TestMethod]
        public void TestReadAllOrders()
        {
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\t\tSINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
            int count = orderDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllOrders Count: " + count);
            Assert.AreEqual(830, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
