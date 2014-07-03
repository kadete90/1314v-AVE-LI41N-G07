using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlMapperClient.Entities;
using SqlMapperFw.Binder;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.BuildMapper.DataMapper;
using SqlMapperFw.MySqlConnection;

namespace SqlMapperTests.MultipleConnectionTests
{
    [TestClass]
    public class ProductTests
    {
        static Builder _builder;
        static IDataMapper<Product> _productDataMapper;
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
            CleanToDefault();
        }

        public static void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
            {
                conSql.Open();
                new SqlCommand("DELETE FROM Products WHERE ProductId > 78", conSql).ExecuteNonQuery();
                conSql.Close();
            }
        }

        [TestMethod]
        public void TestReadAllProducts()
        {
            int count = _productDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllProducts Count = {0} <--", count);
            Assert.AreEqual(77, count);
        }

        [TestMethod]
        public void TestGetProductById()
        {
            Product prod = _productDataMapper.GetById(7);

            Console.WriteLine(prod.ToString());

            Assert.IsNotNull(prod);
            Assert.AreEqual("Uncle Bob's Organic Dried Pears", prod.ProductName);
            Assert.IsFalse(prod.Discontinued);
        }

        [TestMethod]
        public void TestWhereOnReadAllProduct()
        {
            List<Product> prods = _productDataMapper.GetAll().Where("UnitsInStock > 30").Where("CategoryID = 7").ToList();
            Assert.IsNotNull(prods);
            Assert.AreEqual(1, prods.Count);
            Assert.AreEqual(14, prods[0].ID);
            Console.WriteLine("ProductID: {0}, ProductName: {1}, UnitsInStock: {2}", 
                                prods[0].ID, prods[0].ProductName, prods[0].UnitsInStock);
        }

        [TestMethod]
        public void TestCommandsOnProduct()
        {
            Product prod = InsertProduct();
            Assert.AreEqual(78, _productDataMapper.GetAll().Count());
            Console.WriteLine("    --> Inserted new product with ID = {0} <--\n", prod.ID);
            UpdateProduct(prod);
            Console.WriteLine("    --> Updated the product with ID = {0} <--\n", prod.ID);
            DeleteProduct(prod);
            IEnumerable<Product> enumerable = _productDataMapper.GetAll();
            Assert.AreEqual(77, enumerable.Count());
            Console.WriteLine("    --> Deleted the product with ID = {0} <--", prod.ID);
        }

        private Product InsertProduct()
        {
            Supplier sup = new Supplier { ID = 4 };

            Product product = new Product
            {
                ProductName = "ProductName",
                QuantityPerUnit = "100",
                UnitPrice = 100,
                UnitsInStock = 50,
                UnitsOnOrder = 30,
                Supplier = sup
            };
            _productDataMapper.Insert(product);

            Assert.IsNotNull(product.ID);
            Assert.AreNotEqual(0, product.ID);
            return product;
        }

        public void UpdateProduct(Product product)
        {
            product.ProductName = "NewProductname";
            product.Supplier.ID = 3;

            _productDataMapper.Update(product);
            //TODO _productDataMapper.getById()
            IEnumerator<Product> enumerator = _productDataMapper.GetAll().Where("ProductID =" + product.ID).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Product prod = enumerator.Current;
            Assert.AreEqual("NewProductname", prod.ProductName);
            Assert.AreEqual(3, prod.Supplier.ID);
            while (enumerator.MoveNext()) { }
        }

        private void DeleteProduct(Product product)
        {
            _productDataMapper.Delete(product);
        }
    }
   
}
