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

namespace SqlMapperTests.WithoutAssociations.MultipleConnection
{
    [TestClass]
    public class ProductTests
    {
        Builder _builder;
        IDataMapper<Product> _productDataMapper;
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
            _builder = new Builder(_connectionStringBuilder, typeof(MultiConnection<>), bindMemberList);

            _productDataMapper = _builder.Build<Product>();
            CleanToDefault();
            Console.WriteLine("=====================================================");
            Console.WriteLine("\t BEGINING MULTIPLE CONNECTION TEST");
            Console.WriteLine("=====================================================");
        }

        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
            {
                conSql.Open();
                    new SqlCommand("DELETE FROM Products WHERE ProductId > 78", conSql).ExecuteNonQuery();
                conSql.Close();
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            Console.WriteLine("=====================================================");
            Console.WriteLine("\t  ENDING MULTIPLE CONNECTION TEST");
            Console.WriteLine("=====================================================");
        }

        [TestMethod]
        public void TestReadAllProducts()
        {
            int count = _productDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllProducts Count = {0} <--", count);
            Assert.AreEqual(77, count);
        }

        [TestMethod]
        public void TestCommandsOnProduct()
        {
            int productId = InsertProduct();
            Console.WriteLine("-----------------------------------------------------");
            //UpdateProduct(productId);
            Console.WriteLine("-----------------------------------------------------");
            DeleteProduct(productId);
        }

        private int InsertProduct()
        {
            Product product = new Product
            {
                ProductName = "ProductName",
                QuantityPerUnit = "100",
                UnitPrice = 100,
                UnitsInStock = 50,
                UnitsOnOrder = 30
            };
            _productDataMapper.Insert(product);
            Assert.IsNotNull(product.id);
            Console.WriteLine("    --> Inserted new product with id = {0} <--", product.id);
            return product.id;
        }

        private void UpdateProduct(int productId)
        {

            Product product = new Product
            {
                id = productId,
                ProductName = "NewProductname",
                QuantityPerUnit = "100",
                UnitPrice = 100,
                UnitsInStock = 50,
                UnitsOnOrder = 30
            };
            _productDataMapper.Update(product);
            Assert.Equals("NewProductname", product.ProductName);
            Console.WriteLine("    --> Updated the product with id = {0} <--", productId);
            
        }

        private void DeleteProduct(int productId)
        {
            if (productId == 0)
                return;
            Product product = new Product { id = productId };
            _productDataMapper.Delete(product);
            int count = _productDataMapper.GetAll().Count();
            Assert.AreEqual(77, count);
            Console.WriteLine("    --> Removed the product with id = {0} <--", productId);
        }
    }
}
