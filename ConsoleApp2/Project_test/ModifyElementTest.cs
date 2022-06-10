using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Project_test
{
    [TestClass]
    public class ModifyElementTest
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
        public void ModifyNodeValue_OnCall_ShouldValueChange()
        {
            try
            {
                
                SqlConnection connection = new SqlConnection(Project_app.Connection.Sqlconnection);
                var db = new Project_app.DbManager();
                db.ModifyElement(1, "testowyDokument", "(/node/text())[1]", "4");
                try
                {
                    connection.Open();
                    String sqlcommand = "SELECT cast(XMLColumn as nvarchar(max)) FROM XMLTable WHERE name = 'testowyDokument'";
                    SqlCommand command = new SqlCommand(sqlcommand, connection);
                    Assert.AreEqual("<node>4</node>", command.ExecuteScalar().ToString());
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
