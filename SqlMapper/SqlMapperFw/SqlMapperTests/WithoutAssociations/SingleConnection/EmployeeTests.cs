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

namespace SqlMapperTests.WithoutAssociations.SingleConnection
{
    [TestClass]
    public class EmployeeTests
    {
        Builder _builder;
        IDataMapper<Employee> _employeeDataMapper;
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

            _employeeDataMapper = _builder.Build<Employee>();
            CleanToDefault();
            Console.WriteLine("=====================================================");
            Console.WriteLine("\t BEGINING MULTIPLE CONNECTION TEST");
            Console.WriteLine("=====================================================");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
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
           _builder.CloseConnection();
           Console.WriteLine("=====================================================");
           Console.WriteLine("\t  ENDING MULTIPLE CONNECTION TEST");
           Console.WriteLine("=====================================================");
        }

        [TestMethod]
        public void TestReadAllEmployees()
        { 
            Console.WriteLine("-----------------------------------------------------");
            int count = _employeeDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllProducts Count = {0} <--", count);
            Assert.AreEqual(9, count);
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
