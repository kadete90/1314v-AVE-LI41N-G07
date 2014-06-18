using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlMapperClient.Entities;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.MySqlConnection;
using SqlMapperFw.Reflection.Binder;

namespace SqlMapperTests.WithoutAssociations.SingleConnection
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
            _builder = new Builder(_connectionStringBuilder, typeof(SingleConnection<>), bindMemberList, false);

            _orderDataMapper = _builder.Build<Order>();
        }

        [TestInitialize]
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

        [ClassCleanup]
        public static void TearDown()
        {
           _builder.CloseConnection();
        }

        [TestMethod]
        public void TestReadAllOrders()
        {
            Console.WriteLine("-----------------------------------------------------");
            int count = _orderDataMapper.GetAll().Count();
            _builder.Commit();
            Console.WriteLine("    --> TestReadAllOrders Count = {0} <--", count);
            Assert.AreEqual(830, count);
            Console.WriteLine("-----------------------------------------------------");
        }

        [TestMethod]
        public void TestWhereOnReadAllOrder()
        {
            IEnumerable<Order> prods = _orderDataMapper.GetAll().Where("EmployeeID = 5").Where("Freight > 255.5").Where("ShipVia = 3");

            IEnumerator<Order> iterator = prods.GetEnumerator();
            Order order = null;
            int countProds = 0;
            while (iterator.MoveNext())
            {
                countProds++;
                order = iterator.Current;
                Console.WriteLine("OrderId: {0}, EmployeeId: {1}", order.OrderId, order.EmployeeId);
            }
            
            Assert.IsNotNull(order);
            Assert.AreEqual(1, countProds);
            Assert.AreEqual(10359, order.OrderId);
            _builder.Commit();
        }

        [TestMethod]
        public void TestCommandsOnOrder()
        {
            Order order = InsertOrder();
            Assert.AreEqual(831, _orderDataMapper.GetAll().Count());
            _builder.Commit();
            Console.WriteLine("    --> Inserted new order with id = {0} <--\n", order.OrderId);
            UpdateOrder(order);
            Console.WriteLine("    --> Updated the order with id = {0} <--", order.OrderId);
            Console.WriteLine("           --> Rollback update <--\n");
            DeleteOrder(order);
            Assert.AreEqual(830, _orderDataMapper.GetAll().Count());
            _builder.Commit();
            Console.WriteLine("    --> Deleted the order with id = {0} <--", order.OrderId);
        }

        private Order InsertOrder()
        {
            Order order = new Order
            {
                CustomerId = "VINET",
                EmployeeId = 4,
                //OrderDate = dt,
                //RequiredDate = dt,
                //ShippedDate = dt,
                ShipVia = 2,
                Freight = (decimal)10.2,
                ShipName = "KadeteShip",
                ShipAddress = "PevidesAdress",
                //ShipCity = "Kadete Town",
                //ShipRegion = "RL",
                ShipPostalCode = "2640"
                //,ShipCountry = "Portugal"
            };
            _orderDataMapper.Insert(order);

            Assert.IsNotNull(order.OrderId);
            Assert.AreNotEqual(0, order.OrderId);
            _builder.Commit();
            return order;
        }

        public void UpdateOrder(Order order)
        {
            order.CustomerId = "TOMSP";
            order.EmployeeId = 6;
            order.ShipName = "KadeteShip";

            _orderDataMapper.Update(order);
            
            Assert.AreEqual("TOMSP", order.CustomerId);
            Assert.AreEqual(6, order.EmployeeId);
            Assert.AreEqual("KadeteShip", order.ShipName);
            Assert.AreEqual("PevidesAdress", order.ShipAddress);
            _builder.Rollback();
            
        }

        private void DeleteOrder(Order orderToDel)
        {
            Order order = new Order { OrderId = orderToDel.OrderId };
            _orderDataMapper.Delete(order);
            _builder.Commit();
        }
    }
}
