using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlMapperClient.Entities;
using SqlMapperFw.Binder;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.BuildMapper.DataMapper;
using SqlMapperFw.MySqlConnection;

namespace SqlMapperTests.MultipleConnectionTests
{
    [TestClass]
    public class MultiDataMappersTests
    {
        static Builder _builder;
        static IDataMapper<Product> _productDataMapper;
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
            _builder = new Builder(_connectionStringBuilder, typeof(MultiSqlConnection), bindMemberList);

            _productDataMapper = _builder.Build<Product>();
            _orderDataMapper = _builder.Build<Order>();
        }

        [TestMethod]
        public void TestDataMapper()
        {
            _builder.ActivateDataMapper(_productDataMapper);
            int countProducts = _productDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllProducts Count = {0} <--", countProducts);
            Assert.AreEqual(77, countProducts);

            _builder.ActivateDataMapper(_orderDataMapper);
            int countOrders = _orderDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllOrders Count = {0} <--", countOrders);
            Assert.AreEqual(830, countOrders);
        }

    }
   
}
