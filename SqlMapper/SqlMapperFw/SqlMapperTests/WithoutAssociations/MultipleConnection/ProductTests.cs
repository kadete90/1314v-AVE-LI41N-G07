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
            _builder = new Builder(_connectionStringBuilder, typeof(MultiConnection<>), bindMemberList);

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
        }

        // alterar para testes separados quando fôr implementado rollback e commit da transação
        // fazer rollback depois de cada test para não estragar a BD -> autoclosable(false)
        [TestMethod]
        public void TestCommandsOnProduct()
        {
            int productId = InsertProduct();
            Console.WriteLine("-----------------------------------------------------");
            //UpdateProduct(productId);
            Console.WriteLine("-----------------------------------------------------");
            DeleteProduct(productId);
        }

        [TestMethod]
        public void TestWhereOnReadAllProduct()
        {
            IEnumerable<Product> prods = _productDataMapper.GetAll().Where("UnitsInStock > 30").Where("CategoryID = 7");
            IEnumerator<Product> iterator = prods.GetEnumerator();
            Product product = null;
            while (iterator.MoveNext())
            {
                product = iterator.Current;
                Console.WriteLine("ProductName: {0}, UnitsInStock: {1}", product.ProductName, product.UnitsInStock);
            }
            Assert.IsNotNull(product);
            Assert.AreEqual("Tofu", product.ProductName);
            Assert.AreEqual(35, product.UnitsInStock);
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
