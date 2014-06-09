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
            
            //IDataMapper<Product> productMapper = b.Build<Product>(); //1ªparte 1.
            //foreach (Product p in productMapper.GetAll())
            //{
            //    Console.WriteLine(p.id +", "+ p.ProductName +", "+ p.UnitPrice 
            //                           +", "+ p.QuantityPerUnit +", "+ p.UnitsInStock +", "+ p.UnitsOnOrder);
            //}

            IDataMapper<Employee> emplyeeMapper = b.Build<Employee>(); //1ªparte 1.
            foreach (Employee e in emplyeeMapper.GetAll())
            {
                Console.WriteLine(e.EmployeeId + ", " + e.BirthDate + ", " + e.City);
            }

            b.CloseConnection();

            ////--------------------------------------------------------------------------
            //Builder b2 = new Builder(connectionStringBuilder, typeof(MultiConnection<>));
            //IDataMapper<Order> orderMapper = b2.Build<Order>(); //1ªparte 1.
            //foreach (Order o in orderMapper.GetAll())
            //{
            //    Console.WriteLine(o.OrderId + ", " + o.OrderDate + ", " + o.ShipAddress);
            //}
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
