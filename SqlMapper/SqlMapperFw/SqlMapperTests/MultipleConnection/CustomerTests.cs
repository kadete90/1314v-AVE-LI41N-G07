using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlMapperClient.Entities;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Reflection.Binder;

namespace SqlMapperTests.MultipleConnection
{
    [TestClass]
    public class CustomerTests
    {
        static Builder _builder;
        static IDataMapper<Customer> _customerDataMapper;
        static SqlConnectionStringBuilder _connectionStringBuilder;

        [ClassInitialize]
        public static void Setup(TestContext testContext)
        {
            _connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };

            List<Type> bindMemberList = new List<Type> { typeof(BindFields), typeof(BindProperties) };
            _builder = new Builder(_connectionStringBuilder, typeof(MultiConnection<>), bindMemberList, true);

            _customerDataMapper = _builder.Build<Customer>();
            CleanToDefault();
        }

        public static void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Customers WHERE CustomerId = 'xpto'", conSql);
                conSql.Open();
                cmd.ExecuteNonQuery();
                conSql.Close();
            }
        }

        [TestMethod]
        public void TestReadAllCustomers()
        {
            Console.WriteLine("-----------------------------------------------------");
            int count = _customerDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllProducts Count = {0} <--", count);
            Assert.AreEqual(91, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
