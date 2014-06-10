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
    public class OrderTests
    {
        Builder builder;
        IDataMapper<Order> orderDataMapper;
        private SqlConnectionStringBuilder connectionStringBuilder;
       
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
            orderDataMapper = builder.Build<Order>();
            CleanToDefault();

            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\tBEGINING SINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Orders WHERE OrderId > 11077", conSql);
                conSql.Open();
                cmd.ExecuteNonQuery();
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
        public void TestReadAllOrders()
        {
            Console.WriteLine("-----------------------------------------------------");
            int count = orderDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllOrders Count: " + count);
            Assert.AreEqual(830, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
