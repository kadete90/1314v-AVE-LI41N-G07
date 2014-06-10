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
    public class CustomerTests
    {
        Builder builder;
        IDataMapper<Customer> customerDataMapper;
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
            customerDataMapper = builder.Build<Customer>();
            //CleanToDefault();

            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\tBEGINING SINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Customers WHERE CustomerId > ??", conSql);
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
        public void TestReadAllCustomers()
        { 
            int count = customerDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllCustomers Count: " + count);
            Assert.AreEqual(91, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
