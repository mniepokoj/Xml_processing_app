using System;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Project_test
{
    [TestClass]
    public class DeleteDocumentTest
    {

        [TestInitialize()]
        public void TestInit()
        {
            
            SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
            String sqlcommand = "TRUNCATE TABLE XML_Account";
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlcommand, connection);
                command.ExecuteScalar();

                sqlcommand = "DELETE FROM XMLTable WHERE xmlID > 0";
                command.CommandText = sqlcommand;
                command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
            finally { connection.Close(); }

            var db = new Project_app.DbManager();
            XmlDocument xmlObject = new XmlDocument();
            String xmlDoc = "<node></node>";
            xmlObject.LoadXml(xmlDoc);
            Assert.AreEqual(1, db.InsertXmlDocument(1, "testowyDokument", ref xmlObject).status);
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            
            SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
            String sqlcommand = "TRUNCATE TABLE XML_Account";
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlcommand, connection);
                command.ExecuteScalar();

                sqlcommand = "DELETE FROM XMLTable WHERE xmlID > 0";
                command.CommandText = sqlcommand;
                command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
            finally { connection.Close(); }
        }


        [TestMethod]
        public void DeleteExistingDocument_OnCall_ShouldPass()
        {
            try
            {
                var db = new Project_app.DbManager();
                Assert.AreEqual(1, db.DeleteXMLDocument(1, "testowyDokument").status);
                db = null;




                
                SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
                String sqlcommand = "SELECT * FROM XMLTable";
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlcommand, connection);
                    Assert.AreEqual(command.ExecuteScalar(), null);
                }
                catch (SqlException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }


        [TestMethod]
        public void DeleteDocumentWithoutAccess_OnCall_ShouldFail()
        {
            try
            {
                var db = new Project_app.DbManager();
                Assert.AreEqual(0, db.DeleteXMLDocument(2, "testowyDokument").status);
                
                SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
                String sqlcommand = "SELECT * FROM XMLTable";
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlcommand, connection);
                    Assert.AreNotEqual(command.ExecuteScalar(), null);
                }
                catch (SqlException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }
    }
}
