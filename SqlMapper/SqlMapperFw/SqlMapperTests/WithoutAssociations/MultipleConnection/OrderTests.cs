﻿using System;
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
        static Builder _builder;
        static IDataMapper<Order> _orderDataMapper;
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
            _builder = new Builder(_connectionStringBuilder, typeof(MultiConnection<>), bindMemberList);

            _orderDataMapper = _builder.Build<Order>();
            CleanToDefault();
        }

        public static void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Orders WHERE OrderId > 11077", conSql);
                conSql.Open();
                cmd.ExecuteNonQuery();
                conSql.Close();
            }
        }

        [ClassCleanup]
        public static void TearDown()
        {

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
