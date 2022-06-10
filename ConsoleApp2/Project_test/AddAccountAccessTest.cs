using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Project_test
{
    [TestClass]
    public class AddAccountAccessTest
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
            String xmlDoc = "<node>5</node>";
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
        public void GiveAccessToAccount_OnCall_ShouldGiveAccess()
        {
            try
            {
                
                SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
                var db = new Project_app.DbManager();
                Assert.AreEqual(0, db.ReadXmlDocument(2, "testowyDokument").status);
                db.AddAccountAccess(1, "testowyDokument", "user2");
                Assert.AreEqual(1, db.ReadXmlDocument(2, "testowyDokument").status);
            }
            catch (SqlException ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }
    }
}
