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

namespace SqlMapperTests.WithoutAssociations.MultipleConnection
{
    [TestClass]
    public class CustomerTests
    {
        Builder _builder;
        IDataMapper<Customer> _customerDataMapper;
        SqlConnectionStringBuilder _connectionStringBuilder;

        [TestInitialize]
        public void Setup()
        {
            _connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };

            List<Type> bindMemberList = new List<Type> { typeof(BindFields), typeof(BindProperties) };
            _builder = new Builder(_connectionStringBuilder, typeof(SingleConnection<>), bindMemberList);

            _customerDataMapper = _builder.Build<Customer>();
            //CleanToDefault();
            Console.WriteLine("=====================================================");
            Console.WriteLine("\t BEGINING MULTIPLE CONNECTION TEST");
            Console.WriteLine("=====================================================");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
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
            Console.WriteLine("=====================================================");
            Console.WriteLine("\t  ENDING MULTIPLE CONNECTION TEST");
            Console.WriteLine("=====================================================");
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
