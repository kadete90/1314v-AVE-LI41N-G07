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
        static Builder _builder;
        static IDataMapper<Product> _productDataMapper;
        static SqlConnectionStringBuilder _connectionStringBuilder;

        //TODO: apenas fazer no inicio da classe e não em cada teste
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
            _builder = new Builder(_connectionStringBuilder, typeof(MultiConnection<>), bindMemberList, false);

            _productDataMapper = _builder.Build<Product>();
        }

        [TestInitialize]
        public void CleanToDefault()
        {
            using (SqlConnection conSql = new SqlConnection(_connectionStringBuilder.ConnectionString))
            {
                conSql.Open();
                new SqlCommand("DELETE FROM Products WHERE ProductId > 78", conSql).ExecuteNonQuery();
                conSql.Close();
            }
        }

        //TODO: apenas fazer no fim de todos os testes terem sido executados
        [ClassCleanup]
        public static void TearDown()
        {
        }

        [TestMethod]
        public void TestReadAllProducts()
        {
            int count = _productDataMapper.GetAll().Count();
            Console.WriteLine("    --> TestReadAllProducts Count = {0} <--", count);
            Assert.AreEqual(77, count);
            _builder.Commit();
        }

        [TestMethod]
        public void TestWhereOnReadAllProduct()
        {
            IEnumerable<Product> prods = _productDataMapper.GetAll().Where("UnitsInStock > 30").Where("CategoryID = 7");

            IEnumerator<Product> iterator = prods.GetEnumerator();
            Product product = null;
            int countProds = 0;
            while (iterator.MoveNext())
            {
                countProds++;
                product = iterator.Current;
                Console.WriteLine("ProductID: {0}, ProductName: {1}, UnitsInStock: {2}", product.id, product.ProductName, product.UnitsInStock);
            }
            _builder.Commit();
            Assert.IsNotNull(product);
            Assert.AreEqual(1, countProds);
            Assert.AreEqual(14, product.id);
        }

        [TestMethod]
        public void TestCommandsOnProduct()
        {
            Int32 id = InsertProduct();
            Assert.AreEqual(78, _productDataMapper.GetAll().Count());
            _builder.Commit();
            Console.WriteLine("    --> Inserted new product with id = {0} <--\n", id);
            UpdateProduct(id);
            Console.WriteLine("    --> Updated the product with id = {0} <--", id);
            Console.WriteLine("           --> Rollback update <--\n");
            DeleteProduct(id);
            Assert.AreEqual(77, _productDataMapper.GetAll().Count());
            _builder.Commit();
            Console.WriteLine("    --> Deleted the product with id = {0} <--", id);
        }

        private Int32 InsertProduct()
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
            Assert.AreNotEqual(0, product.id);
            _builder.Commit();
            return product.id;
        }

        public void UpdateProduct(Int32 productId)
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
            Assert.AreEqual("NewProductname", product.ProductName);
            _builder.Rollback();
        }

        private void DeleteProduct(Int32 productId)
        {
            Product product = new Product { id = productId };
            _productDataMapper.Delete(product);
            _builder.Commit();


        }
    }
}
