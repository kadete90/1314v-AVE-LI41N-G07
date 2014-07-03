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
    public class EmployeeTests
    {
        static Builder _builder;
        static IDataMapper<Employee> _employeeDataMapper;
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

            _employeeDataMapper = _builder.Build<Employee>();
            CleanToDefault();
        }

        public static void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Employees WHERE EmployeeID > 9", conSql);
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
        public void TestReadAllEmployees()
        {
            _builder.BeginTransaction();
            Console.WriteLine("-----------------------------------------------------");
            int count = _employeeDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllEmployees Count = {0} <--", count);
            Assert.AreEqual(9, count);
            Console.WriteLine("-----------------------------------------------------");
            _builder.Commit();
        }

        [TestMethod]
        public void TestWhereOnReadAllEmployee()
        {
            _builder.BeginTransaction();
            IEnumerable<Employee> prods = _employeeDataMapper.GetAll().Where("Country = 'UK'").Where("Extension = 465");

            IEnumerator<Employee> iterator = prods.GetEnumerator();
            Employee Employee = null;
            int countProds = 0;
            while (iterator.MoveNext())
            {
                countProds++;
                Employee = iterator.Current;
                Console.WriteLine("EmployeeID: {0}, FirstName: {1}, LastName: {2}, ReportsTo : {3}",
                            Employee.ID, Employee.FirstName, Employee.LastName, Employee.ReportsTo.ID);
            }
            Assert.IsNotNull(Employee);
            Assert.AreEqual(1, countProds);
            Assert.AreEqual(7, Employee.ID);
            _builder.Commit();
        }

        [TestMethod]
        public void TestCommandsOnEmployee()
        {
            _builder.BeginTransaction(IsolationLevel.RepeatableRead);
            Employee prod = InsertEmployee();
            Assert.AreEqual(10, _employeeDataMapper.GetAll().Count());
            Console.WriteLine("    --> Inserted new Employee with ID = {0} <--\n", prod.ID);
            UpdateEmployee(prod);
            Console.WriteLine("    --> Updated the Employee with ID = {0} <--\n", prod.ID);
            DeleteEmployee(prod);
            Assert.AreEqual(9, _employeeDataMapper.GetAll().Count());
            Console.WriteLine("    --> Deleted the Employee with ID = {0} <--", prod.ID);
            _builder.Rollback();
        }

        private Employee InsertEmployee()
        {
            Employee boss = new Employee { ID = 6 };
  
            Employee Employee = new Employee
            {
                FirstName = "Flávio",
                LastName = "Cadete",
                Address = "Mafra",
                BirthDate = new DateTime(1990, 09,05),
                City = "Lisbon",
                Country = "Portugal",
                Title = "Manage",
                ReportsTo = boss
            };
            _employeeDataMapper.Insert(Employee);

            Assert.IsNotNull(Employee.ID);
            Assert.AreNotEqual(0, Employee.ID);
            return Employee;
        }

        public void UpdateEmployee(Employee Employee)
        {
            Employee.LastName = "El Boss";

            _employeeDataMapper.Update(Employee);
            //TODO _EmployeeDataMapper.getById()
            IEnumerator<Employee> enumerator = _employeeDataMapper.GetAll().Where("EmployeeID =" + Employee.ID).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Employee emp = enumerator.Current;
            Assert.AreEqual("El Boss", emp.LastName);
            Assert.AreEqual(6, emp.ReportsTo.ID);
            while (enumerator.MoveNext()) { }
        }

        private void DeleteEmployee(Employee Employee)
        {
            _employeeDataMapper.Delete(Employee);
        }

    }
}
