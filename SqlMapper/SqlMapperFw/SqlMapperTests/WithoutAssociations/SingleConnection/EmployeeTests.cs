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
            employeeDataMapper = builder.Build<Employee>();
            CleanToDefault();

            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("\tBEGINING SINGLE CONNECTION TEST");
            Console.WriteLine("-----------------------------------------------------");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Employees WHERE EmployeeId > 9", conSql);
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
        public void TestReadAllEmployees()
        { 
            Console.WriteLine("-----------------------------------------------------");
            int count = employeeDataMapper.GetAll().Count();
            Console.WriteLine(" TestReadAllEmployees Count: " + count);
            Assert.AreEqual(9, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
