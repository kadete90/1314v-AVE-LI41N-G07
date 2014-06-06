using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.MySqlConnection;
using SqlMapperTests.Entities;

namespace SqlMapperTests
{

    class Program
    {

        //eliminar código e dependência do projecto SqlMapperClient
        public static void Main()
        {

            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = @"(local)",
                IntegratedSecurity = true,
                InitialCatalog = "Northwind"
            };

            Builder b = new Builder(connectionStringBuilder, typeof(SingleConnection<>));

            //IDataMapper<Employee> emplyeeMapper = b.Build<Employee>(); //1ªparte 1.
            //IEnumerable<Employee> employees = emplyeeMapper.GetAll();
            //foreach (Employee employee in employees)
            //{
            //    Console.WriteLine(employee.EmployeeId);
            //}
            //IDataMapper<Customer> customerMapper = b.Build<Customer>(); //1ªparte 1.
            IDataMapper<Product> productMapper = b.Build<Product>(); //1ªparte 1.

            //customerMapper.Insert(new Customer());
            //productMapper.Update(new Product());
            //productMapper.Delete(new Product());
            foreach (Product p in productMapper.GetAll())
            {
                Console.WriteLine(p.id +", "+ p.ProductName +", "+ p.UnitPrice 
                                       +", "+ p.QuantityPerUnit +", "+ p.UnitsInStock +", "+ p.UnitsOnOrder);
            }
            ////--------------------------------------------------------------------------
            //Builder b2 = new Builder(connectionStringBuilder, typeof(MultiConnection<>));
            //IDataMapper<Customer> customerMapper2 = b2.Build<Customer>(); //1ªparte 1.
            //customerMapper2.Update(new Customer());
            //customerMapper2.Delete(new Customer());

            //IDataMapper<Customer> custMapper = b.Build<Customer>(); //1ªparte 1.
            //IDataMapper<Employee> empMapper = b.Build<Employee>(); //1ªparte 1.


            //IDataMapper<Product> prodMapper = b.Build<Product>(); //1ªparte 1.
            //IEnumerable<Product> prods = prodMapper.GetAll();
            //prods.Where("CategoryID = 7").Where("UnitsinStock > 30"); //1ªparte 2.
        }
    }
}
