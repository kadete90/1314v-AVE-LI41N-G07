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
    public class EmployeeTests
    {
        Builder builder;
        IDataMapper<Employee> employeeDataMapper;
       
        [TestInitialize]
        public void Setup()
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };
            builder = new Builder(connectionStringBuilder, typeof(SingleConnection<>));
            employeeDataMapper = builder.Build<Employee>();
        }

        [TestCleanup]
        public void TearDown()
        {
           builder.CloseConnection();
        }

        [TestMethod]
        public void TestReadAllEmployees()
        { 
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\t\tSINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
            int count = employeeDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllEmployees Count: " + count);
            Assert.AreEqual(9, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
