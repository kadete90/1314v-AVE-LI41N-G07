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
    public class OrderTests
    {
        Builder _builder;
        IDataMapper<Order> _orderDataMapper;
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

            _orderDataMapper = _builder.Build<Order>();
            CleanToDefault();
            Console.WriteLine("=====================================================");
            Console.WriteLine("\t BEGINING MULTIPLE CONNECTION TEST");
            Console.WriteLine("=====================================================");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
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
            Console.WriteLine("=====================================================");
            Console.WriteLine("\t  ENDING MULTIPLE CONNECTION TEST");
            Console.WriteLine("=====================================================");
        }

        [TestMethod]
        public void TestReadAllOrders()
        {
            Console.WriteLine("-----------------------------------------------------");
            int count = _orderDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllProducts Count = {0} <--", count);
            Assert.AreEqual(830, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
