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
    public class CustomerTests
    {
        Builder builder;
        IDataMapper<Customer> customerDataMapper;

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
            customerDataMapper = builder.Build<Customer>();
        }

        [TestCleanup]
        public void TearDown()
        {
            builder.CloseConnection();
        }

        [TestMethod]
        public void TestReadAllCustomers()
        {
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\t\tSINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
            int count = customerDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllCustomers Count: " + count);
            Assert.AreEqual(91, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
