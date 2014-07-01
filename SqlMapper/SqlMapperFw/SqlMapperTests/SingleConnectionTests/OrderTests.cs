using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlMapperClient.Entities;
using SqlMapperFw.Binder;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.BuildMapper.DataMapper;
using SqlMapperFw.MySqlConnection;

namespace SqlMapperTests.SingleConnectionTests
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
            _builder = new Builder(_connectionStringBuilder, typeof(SingleSqlConnection), bindMemberList);

            _orderDataMapper = _builder.Build<Order>();
            CleanToDefault();
        }


        public static void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Orders WHERE OrderID > 11077", conSql);
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
            _builder.BeginTransaction(IsolationLevel.ReadUncommitted);
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
            _builder.BeginTransaction(IsolationLevel.ReadUncommitted);
            IEnumerable<Order> orders = _orderDataMapper.GetAll().Where("EmployeeID = 5").Where("Freight > 255.5").Where("ShipVia = 3");
            Order order = null;
            int countOrders = 0;
            foreach (Order ord in orders)
            {
                order = ord;
                countOrders++;
            }
            _builder.Commit();
            Assert.IsNotNull(order);
            Assert.AreEqual(1, countOrders);
            Assert.AreEqual(10359, order.ID);
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("    --> OrderID: {0}, EmployeeID: {1} <--", order.ID, order.Employee.ID);
            Console.WriteLine("---------------------------------------------");

        }

        [TestMethod]
        public void TestCommandsOnOrder()
        {
            _builder.BeginTransaction(IsolationLevel.ReadCommitted);
            Order order = InsertOrder();
            Assert.AreEqual(831, _orderDataMapper.GetAll().Count());
            Console.WriteLine("    --> Inserted new order with ID = {0} <--\n", order.ID);
            UpdateOrder(order);
            Console.WriteLine("    --> Updated the order with ID = {0} <--\n", order.ID);
            DeleteOrder(order);
            Assert.AreEqual(830, _orderDataMapper.GetAll().Count());
            Console.WriteLine("    --> Deleted the order with ID = {0} <--", order.ID);
            _builder.Rollback();
        }

        private Order InsertOrder()
        {
            Customer customer = new Customer { ID = "VINET" };
            Employee employee = new Employee { ID = 4 };
            DateTime dt = DateTime.Now;
            Order order = new Order
            {
                Customer = customer,
                Employee = employee,
                OrderDate = dt,
                RequiredDate = dt.AddDays(20),
                ShippedDate = dt.AddDays(10),
                ShipVia = 2,
                Freight = (decimal)10.2,
                ShipName = "KadeteShip",
                ShipAddress = "PevidesAdress",
                ShipCity = "Kadete Town",
                ShipRegion = "RL",
                ShipPostalCode = "2640",
                ShipCountry = "Portugal"
            };
            _orderDataMapper.Insert(order);
            Assert.IsNotNull(order.ID);
            Assert.AreNotEqual(0, order.ID);
            Assert.AreEqual(4, order.Employee.ID);
            Assert.AreEqual("VINET", order.Customer.ID);
            return order;
        }

        public void UpdateOrder(Order order)
        {
            order.Customer.ID = "TOMSP";
            order.Employee.ID = 6;
            order.ShipName = "KadeteShip";

            _orderDataMapper.Update(order);
            
            Assert.AreEqual("TOMSP", order.Customer.ID);
            Assert.AreEqual(6, order.Employee.ID);
            Assert.AreEqual("KadeteShip", order.ShipName);
            Assert.AreEqual("PevidesAdress", order.ShipAddress);
            
        }

        private void DeleteOrder(Order orderToDel)
        {
            _orderDataMapper.Delete(orderToDel);
        }
    }
}
