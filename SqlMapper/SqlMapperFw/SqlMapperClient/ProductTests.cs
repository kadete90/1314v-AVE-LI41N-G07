//using System;
//using System.Data;
//using System.Data.SqlClient;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SqlMapperFw.DataMappers;

//namespace SqlMapperClient
//{
//    [TestClass]
//    public class ProductTests
//    {

//        SqlConnection conSql;

//        [TestInitialize]
//        public void Setup()
//        {
//            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
//            {
//                DataSource = "(local)",
//                IntegratedSecurity = true,
//                InitialCatalog = "Northwind"
//            };
//            conSql = new SqlConnection(builder.ConnectionString);
//        }

//        [TestCleanup]
//        public void TearDown()
//        {
//            if (conSql.State == ConnectionState.Closed) return;
//            conSql.Close();
//            conSql = null;
//        }

//        [TestMethod]
//        public void TestReadAllProducts()
//        {
//            using (SqlCommand cmd = conSql.CreateCommand())
//            {
//                cmd.CommandText = "SELECT ProductID, ProductName, UnitPrice, UnitsInStock FROM Products";
//                conSql.Open();
//                SqlDataReader dr = cmd.ExecuteReader();
//                int count = 0;
//                while (dr.Read())
//                {
//                    Console.WriteLine(dr["ProductName"]);
//                    count++;
//                }
//                Assert.AreEqual(77, count);
//            }
//        }

//        [TestMethod]
//        public void InsertProduct()
//        {
//            conSql.Open();
//            using (SqlTransaction tran = conSql.BeginTransaction())
//            {
//                using (
//                    SqlCommand cmdRead = CmdBuilder.MakeReadCmd(conSql),
//                    cmdInsert = CmdBuilder.(conSql))
//                {
//                    cmdRead.Transaction = tran;
//                    cmdInsert.Transaction = tran;

//                    cmdInsert.Parameters["@name"].Value = "Cafe da Alcobia";
//                    cmdInsert.Parameters["@price"].Value = 100.0;
//                    cmdInsert.Parameters["@stock"].Value = 5550;
//                    int prodId = (int)cmdInsert.ExecuteScalar();
                    
//                    cmdRead.Parameters["@id"].Value = prodId;
//                    using (SqlDataReader product = cmdRead.ExecuteReader())
//                    {
//                        product.Read();
//                        Assert.AreEqual(prodId, product["ProductID"]);
//                        Assert.AreEqual("Cafe da Alcobia", product["ProductName"]);
//                        Assert.AreEqual((decimal)100.0, product["UnitPrice"]);
//                        Assert.AreEqual((Int16) 5550, product["UnitsInStock"]);
//                    }

//                } // Dispose commands
//                tran.Rollback();
//            }// Dispose transaction
//        }

//        [TestMethod]
//        public void UppdateProduct()
//        {
//            conSql.Open();
//            using (SqlTransaction tran = conSql.BeginTransaction())
//            {
//                using (
//                    SqlCommand cmdRead = CmdBuilder<Product>.MakeReadCmd(conSql),
//                    cmdUpdate = CmdBuilder.MakeUpdateCmd(conSql))
//                {
//                    cmdRead.Transaction = tran;
//                    cmdUpdate.Transaction = tran;

//                    cmdRead.Parameters["@id"].Value = 9;

//                    using (SqlDataReader product = cmdRead.ExecuteReader()) { 
//                        product.Read();
//                        string prodName = product["ProductName"].ToString();
//                        Console.WriteLine(prodName);
//                    }

//                    cmdUpdate.Parameters["@id"].Value = 9;
//                    cmdUpdate.Parameters["@name"].Value = "Casa de Cafe Bastos";
//                    cmdUpdate.ExecuteNonQuery();

//                    using (SqlDataReader product = cmdRead.ExecuteReader())
//                    { 
//                        product.Read();
//                        Console.WriteLine(product["ProductName"]);
//                        Assert.AreEqual("Casa de Cafe Bastos", product["ProductName"]);
//                    }

//                } // Dispose commands
//                tran.Rollback();
//            }// Dispose transaction
//        }
//    }
//}
