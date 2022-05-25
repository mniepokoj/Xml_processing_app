using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Security;

namespace ConsoleApp2
{
    class DbManager
    {
        public SqlConnection connection;
        public DbManager()
        {
            //string sqlconnection = @"DATA SOURCE=DESKTOP-0C06VAE; INITIAL CATALOG=xml_projectDB; INTEGRATED SECURITY=SSPI;";
            string sqlconnection = @"DATA SOURCE=DESKTOP-0C06VAE; INITIAL CATALOG=xml_projectDB; INTEGRATED SECURITY=SSPI;";
            connection = new SqlConnection(sqlconnection);
            try
            {
                connection.Open();
            }
            catch(SqlException e)
            {
                Console.Error.Write("Unable to connect do database.");
                Console.Error.Write(e.ToString());
                Environment.Exit(-1);
            }
        }

        private void update_log(int user_id, String documentName, String operation, int status)
        {

            SqlCommand command = new SqlCommand("SELECT XMLId from XMLTable WHERE name=@documentname", connection);
            command.Parameters.Add("@documentname", SqlDbType.VarChar).Value = documentName;
            int docID;
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                docID = reader.GetInt32(0);
            }
            else
            {
                docID = -1;
            }
            reader.Close();

            if(docID == -1)
            {
                command = new SqlCommand("INSERT INTO XmlTable_log(UserID, operationDate, operation, statusID)" +
                " VALUES (@UserID, @operationDate, @operation, @statusID)", connection);
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = user_id;
                command.Parameters.Add("@operationDate", SqlDbType.DateTime).Value = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss.fff");
                command.Parameters.Add("@operation", SqlDbType.VarChar).Value = operation;
                command.Parameters.Add("@statusID", SqlDbType.Int).Value = status;
            }
            else
            {
                command = new SqlCommand("INSERT INTO XmlTable_log(UserID, DocumentID, operationDate, operation, statusID)" +
                " VALUES (@UserID, @DocumentID, @operationDate, @operation, @statusID)", connection);
                command.Parameters.Add("@UserID", SqlDbType.Int).Value = user_id;
                if (docID == -1)
                    command.Parameters.Add("@DocumentID", SqlDbType.Int).Value = null;
                else
                    command.Parameters.Add("@DocumentID", SqlDbType.Int).Value = docID;

                command.Parameters.Add("@operationDate", SqlDbType.DateTime).Value = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss.fff");
                command.Parameters.Add("@operation", SqlDbType.VarChar).Value = operation;
                command.Parameters.Add("@statusID", SqlDbType.Int).Value = status;
            }
            command.ExecuteScalar();
        }

        public Message login(String login, String password)
        {
            
            SqlCommand command = new SqlCommand("SELECT userID from account WHERE username=@login AND password=@pass", connection);
            command.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
            String s = password.ToString();
            command.Parameters.Add("@pass", SqlDbType.VarChar).Value = password.ToString();
            SqlDataReader reader = command.ExecuteReader();
            if(reader.Read())
            {
                int id = reader.GetInt32(0);
                reader.Close();
                return new Message(id, "You have logged into the system as " + login + "\n\n");
            }
            else
            {
                reader.Close();
                return new Message(-1, "Login or password are incorrect.\n");
            }
        }


        int getXMLid(String name)
        {
            SqlCommand command = new SqlCommand("SELECT xmlID from XMLTable WHERE name=@name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            Object o = command.ExecuteScalar();
            if (o == null)
                return -1;
            return (int)o;
        }
   

        /*
         * function whitch insert XMLdocument into dataBase
         * @param xmlDoc document to be inserted
        */
        public Message insertXmlDocument(int user_id, String name, ref XmlDocument xmlDoc)
        {
            SqlCommand command = new SqlCommand("INSERT INTO XMLTable(Name, XMLColumn) VALUES (@name, @xmlDoc)", connection);
            command.Parameters.Add("@xmlDoc", SqlDbType.Xml).Value = xmlDoc.OuterXml;
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            Object o = command.ExecuteScalar();
            if (o == null)
            {
                int xml_id = getXMLid(name);
                command = new SqlCommand("INSERT INTO XML_account(accountId, XMLid) VALUES (@account_id, @xml_id)", connection);
                command.Parameters.Add("@account_id", SqlDbType.Int).Value = user_id;
                command.Parameters.Add("@xml_id", SqlDbType.Int).Value = xml_id;
                command.ExecuteNonQuery();
                update_log(user_id, name, "insert", 1);
                return new Message(1, "Document has been written to database!\n\n");
            }
            else
            {
                return new Message(0, "Error occured during write document to database!\n" +
                                           "Information: " + o.ToString() + "\n\n");
            }
        }
        private String printElement(XmlNode xn)
        {
            if(xn.Attributes.Count == 0)
            {
                return String.Format("<{0}>", xn.Name);
            }
            else
            {
                String s = "";
                s += String.Format("<{0}", xn.Name);
                for(int i = 0; i < xn.Attributes.Count; i++)
                {
                    s += String.Format(" {0}='{1}'", xn.Attributes[i].Name, xn.Attributes[i].Value);
                }
                s += ">";
                return s;
            }
        }
        public Message readXmlDocument(String name) 
        {
            SqlCommand command = new SqlCommand("SELECT XMLColumn FROM XMLTable WHERE name = @name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            XmlReader reader = command.ExecuteXmlReader();
            if(reader.Read())
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(reader);

                XmlNode xn = xd.FirstChild;
                reader.Close();
                return new Message(1, readNodes(xn, "") + "\n\n");
            }
            else
            {
                reader.Close();
                return new Message(0, "Document '" + name + "' can not been found!\n");
            }
        }
        public Message deleteXMLDocument(int user_id, String name)
        {
            int xmlId = getXMLid(name);
            if(xmlId != -1)
            {
                SqlCommand command = new SqlCommand("DELETE FROM XML_account where xmlId=@xmlId", connection);
                command.Parameters.Add("@xmlId", SqlDbType.Int).Value = xmlId;
                command.ExecuteNonQuery();

                command = new SqlCommand("DELETE FROM XMLTable where name = @name", connection);
                command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
                command.ExecuteNonQuery();
                
                update_log(user_id, name, "delete", 1);
                return new Message(1, "Documents have been deleted from database!\n");
            }
            else
            {
                return new Message(0, "Document '" + name + "' can not been found!\n");
            }
        }
        private String readNodes(XmlNode xn, String depth="")
        {
            String s = "";
            s += "\n" + depth;
            switch (xn.NodeType)
            {
                case XmlNodeType.Element:
                    s+=printElement(xn);
                    break;
                case XmlNodeType.Text:
                    s+=xn.Value;
                    break;
                case XmlNodeType.CDATA:
                    s += String.Format("<![CDATA[{0}]]>", xn.Value);
                    break;
                case XmlNodeType.ProcessingInstruction:
                    s += String.Format("<?{0} {1}?>", xn.Name, xn.Value);
                    break;
                case XmlNodeType.Comment:
                    s += String.Format("<!--{0}-->", xn.Value);
                    break;
                case XmlNodeType.XmlDeclaration:
                    s += String.Format("<?xml version='1.0'?>");
                    break;
                case XmlNodeType.Document:
                    break;
                case XmlNodeType.DocumentType:
                    s += String.Format("<!DOCTYPE {0} [{1}]", xn.Name, xn.Value);
                    break;
                case XmlNodeType.EntityReference:
                    s += String.Format(xn.Name);
                    break;
                case XmlNodeType.EndElement:
                    s += String.Format("</{0}>", xn.Name);
                    break;
            }

            XmlNodeList xnList = xn.ChildNodes;
            depth += "\t";
            foreach(XmlNode x in xnList)
            {
                s+= readNodes(x, depth);
            }
            if (xn.Name != "#text")
            {
                depth = depth.Remove(depth.Length - 1);
                s += String.Format("\n" + depth + "</{0}>", xn.Name);
            }
            return s;
        }
        public Message findAttribute(String docName, String xpath)
        {
            String s = "";
            SqlCommand command = new SqlCommand("SELECT XMLColumn FROM XMLTable WHERE name = @name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            XmlReader reader = command.ExecuteXmlReader();
            if (reader != null)
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);

                XmlNodeList xnList = xml.SelectNodes(xpath);
                int i = 1;
                foreach (XmlNode xn in xnList)
                {
                    s += "-------------------------------" + i + "-------------------------------";
                    s += readNodes(xn);
                    s += "\n\n\n";
                    i++;
                }
                reader.Close();
                return new Message(1, s);
            }
            else
            {
                reader.Close();
                return new Message(0, "Document '" + docName + "' can not been found!\n");
            }

        }
        public Message getAllDocuments(int user_id)
        {
            String s = "";
            SqlCommand command = new SqlCommand("SELECT name FROM XMLTable " +
                "JOIN xml_account on xml_account.xmlId=xmlTable.xmlID WHERE xml_account.accountID = @userID", connection);
            command.Parameters.Add("@userID", SqlDbType.Int).Value = user_id;
            SqlDataReader reader = command.ExecuteReader();

            int i = 1;
            while(reader.Read())
            {
                s += String.Format( "{0}.\t{1}\n", i++,reader.GetString(0));
            }
            reader.Close();
            if (s == "")
            {
                return new Message(0, "There is no documents in database\n\n");
            }
            else
            {
                s += "\n";
                return new Message(1, s);
            }
        }
        private Message modifyDocument(int user_id, String docName, ref XmlDocument xmlDoc)
        {
            SqlCommand command = new SqlCommand("UPDATE XMLTable SET XMLColumn=@XMLDoc WHERE name=@name", connection);
            command.Parameters.Add("@xmlDoc", SqlDbType.Xml).Value = xmlDoc.OuterXml;
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            Object o = command.ExecuteScalar();
            if (o == null)
            {
                update_log(user_id, docName, "modify", 1);
                return new Message(1, "Document has been updated!\n\n");
            }
            else
            {
                return new Message(0, "Error occured update document!\n" +
                                           "Information: " + o.ToString() + "\n\n");
            }
        }
        public Message modifyAttribute(int user_id, String docName, String xpath, String newValue)
        {
            String s = "";
            SqlCommand command = new SqlCommand("SELECT XMLColumn FROM XMLTable WHERE name = @name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            XmlReader reader = command.ExecuteXmlReader();

            XmlDocument xml = new XmlDocument();

            if (reader != null)
            {
                xml.Load(reader);

                XmlNodeList xnList = xml.SelectNodes(xpath);
                int i = 1;
                if (xnList.Count == 0)
                {
                    s += "Entered attribute could not been found!\n";
                    return new Message(0, s);
                }
                else
                {
                    int indx1 = xpath.IndexOf('@') + 1;
                    int indx2 = xpath.IndexOf('=', indx1);
                    String attribute = xpath.Substring(indx1, indx2 - indx1);

                    foreach (XmlNode xn in xnList)
                    {
                        if(xn.Attributes[attribute] != null)
                        {
                            xn.Attributes[attribute].Value = newValue;
                            i++;
                        }
                    }
                    this.modifyDocument(user_id, docName, ref xml);
                }
                s = String.Format("Operation have been performed. {0} nodes affected.\n\n", i);
                return new Message(1, s);
            }
            else
            {
                return new Message(0, "Document '" + docName + "' can not been found!\n");
            }
        }
        public Message modifyContent(int user_id, String docName, String xpath, String newValue)
        {
            String s = "";
            SqlCommand command = new SqlCommand("SELECT XMLColumn FROM XMLTable WHERE name = @name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            XmlReader reader = command.ExecuteXmlReader();

            XmlDocument xml = new XmlDocument();

            if (reader != null)
            {
                xml.Load(reader);

                XmlNodeList xnList = xml.SelectNodes(xpath);
                if (xnList.Count == 0)
                {
                    s += "Entered element could not been found!\n";
                    return new Message(0, s);
                }
                else
                {
                    int i = 0;
                    foreach (XmlNode xn in xnList)
                    {
                        foreach(XmlNode xc in xn.ChildNodes)
                        {
                            if(xc.NodeType == XmlNodeType.Text)
                            {
                                xc.InnerText = newValue;
                                i++;
                            }
                        }
                    }
                    this.modifyDocument(user_id, docName, ref xml);
                    s = String.Format("Operation have been performed. {0} nodes affected.\n\n", i);
                }
                return new Message(1, s);
            }
            else
            {
                return new Message(0, "Document '" + docName + "' can not been found!\n");
            }
        }
        public Message modifyElement(int user_id, String docName, String xpath, String newValue)
        {
            String s = "";
            SqlCommand command = new SqlCommand("SELECT XMLColumn FROM XMLTable WHERE name = @name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            XmlReader reader = command.ExecuteXmlReader();

            XmlDocument xml = new XmlDocument();

            if (reader != null)
            {
                xml.Load(reader);

                XmlNodeList xnList = xml.SelectNodes(xpath);
                if (xnList.Count == 0)
                {
                    s += "Entered element could not been found!\n";
                    return new Message(0, s);
                }
                else
                {
                    int i = 0;
                    foreach (XmlNode xn in xnList)
                    {
                        if (xn.NodeType == XmlNodeType.Element)
                        {
                            xn.Value = newValue;
                            i++;
                        }
                    }
                    this.modifyDocument(user_id, docName, ref xml);
                    s = String.Format("Operation have been performed. {0} nodes affected.\n\n", i);
                }
                return new Message(1, s);
            }
            else
            {
                return new Message(0, "Document '" + docName + "' can not been found!\n");
            }
        }
        ~DbManager()
        {
            if(connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }


    }
}
