using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Project_test
{
    [TestClass]
    public class FindElemenentTest
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
            }
            finally { connection.Close(); }

            var db = new Project_app.DbManager();
            XmlDocument xmlObject = new XmlDocument();
            String xmlDoc = "<animals><animal><name>Czarek</name><type>Dog</type></animal>" +
                            "<animal><name>Czarek</name><type>Dog</type></animal></animals>";
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
            }
            finally { connection.Close(); }
        }


        [TestMethod]
        public void FindNode_OnCall_ShouldReturnNode()
        {
            try
            {
                
                SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
                var db = new Project_app.DbManager();
                String actual = db.FindElement(1, "testowyDokument", "animals/animal[1]/name").content;
                connection.Open();
                String sqlcommand = "SELECT cast(XMLColumn as nvarchar(max)) FROM XMLTable WHERE name = 'testowyDokument'";
                SqlCommand command = new SqlCommand(sqlcommand, connection);
                Assert.IsTrue(actual.Contains("Czarek"));
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }

        [TestMethod]
        public void FindNodeWithoutAccess_OnCall_ShouldNotReturnNode()
        {
            try
            {
                
                SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
                var db = new Project_app.DbManager();
                Assert.AreEqual(0, db.FindElement(2, "testowyDokument", "animals/animal[1]/name").status);
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }
    }
}
