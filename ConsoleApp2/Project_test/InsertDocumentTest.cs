using System;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Project_test
{
    [TestClass]
    public class InsertDocumentTest
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
        public void InsertOnceDocument_OnCall_ShouldPass()
        {
            try
            {
                var db = new Project_app.DbManager();
                XmlDocument xmlObject = new XmlDocument();
                String xmlDoc = "<node></node>";
                xmlObject.LoadXml(xmlDoc);
                Assert.AreEqual(1, db.InsertXmlDocument(1, "testowyDokument", ref xmlObject).status);

                db = null;


                
                SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
                String sqlcommand = "SELECT cast(XMLColumn as nvarchar(max)) FROM XMLTable WHERE name = 'testowyDokument'";
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlcommand, connection);
                    Assert.AreEqual(command.ExecuteScalar(), "<node/>");
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
        public void InsertTwiceTheSameDocument_OnCall_ShouldNotAllow()
        {
            try
            {
                var db = new Project_app.DbManager();
                XmlDocument xmlObject = new XmlDocument();
                String xmlDoc = "<node></node>";
                xmlObject.LoadXml(xmlDoc);
                Assert.AreEqual(1, db.InsertXmlDocument(1, "testowyDokument", ref xmlObject).status);
                Assert.AreEqual(0, db.InsertXmlDocument(1, "testowyDokument", ref xmlObject).status);
                db = null;
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }

    }
}
