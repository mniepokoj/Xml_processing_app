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

        public Message login(String login, SecureString password)
        {
            
            SqlCommand command = new SqlCommand("SELECT id from users WHERE login=@login, password=@pass", connection);
            command.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
            command.Parameters.Add("@pass", SqlDbType.VarChar).Value = password.ToString();
            SqlDataReader reader = command.ExecuteReader();
            if(reader.Read())
            {
                return new Message(reader.GetInt32(0), "You have logged into the system as " + login + "\n\n");
            }
            else
            {
                return new Message(-1, "Login or password are incorrect.\n");
            }
        }


        /*
         * function whitch insert XMLdocument into dataBase
         * @param xmlDoc document to be inserted
        */
        public Message insertXmlDocument(String name , ref XmlDocument xmlDoc)
        {
            SqlCommand command = new SqlCommand("INSERT INTO XMLTable(Name, XMLColumn) VALUES (@name, @xmlDoc)", connection);
            command.Parameters.Add("@xmlDoc", SqlDbType.Xml).Value = xmlDoc.OuterXml;
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            Object o = command.ExecuteScalar();
            if(o == null)
            {
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
        public Message deleteXMLDocument(String name)
        {
            SqlCommand command = new SqlCommand("DELETE FROM XMLTable where name = @name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            int o = command.ExecuteNonQuery();
            if (o > 0)
            {
                Message m = new Message(1, "Documents have been deleted from database!\n");
                return m;
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
        public Message getAllDocuments()
        {
            String s = "";
            SqlCommand command = new SqlCommand("SELECT name FROM XMLTable", connection);
            SqlDataReader reader = command.ExecuteReader();

            int i = 1;
            while(reader.Read())
            {
                s += String.Format( "{0}.\t{1}\n", i++,reader.GetString(0));
            }
            reader.Close();
            if (s == "")
            {
                return new Message(0, "There is no documents in database");
            }
            else
            {
                s += "\n";
                return new Message(1, s);
            }
        }
        private Message modifyDocument(String docName, ref XmlDocument xmlDoc)
        {
            SqlCommand command = new SqlCommand("UPDATE XMLTable SET XMLColumn=@XMLDoc WHERE name=@name", connection);
            command.Parameters.Add("@xmlDoc", SqlDbType.Xml).Value = xmlDoc.OuterXml;
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            Object o = command.ExecuteScalar();
            if (o == null)
            {
                return new Message(1, "Document has been updated!\n\n");
            }
            else
            {
                return new Message(0, "Error occured update document!\n" +
                                           "Information: " + o.ToString() + "\n\n");
            }
        }
        public Message modifyAttribute(String docName, String xpath, String newValue)
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
                    this.modifyDocument(docName, ref xml);
                }
                s = String.Format("Operation have been performed. {0} nodes affected.\n\n", i);
                return new Message(1, s);
            }
            else
            {
                return new Message(0, "Document '" + docName + "' can not been found!\n");
            }
        }
        public Message modifyContent(String docName, String xpath, String newValue)
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
                    this.modifyDocument(docName, ref xml);
                    s = String.Format("Operation have been performed. {0} nodes affected.\n\n", i);
                }
                return new Message(1, s);
            }
            else
            {
                return new Message(0, "Document '" + docName + "' can not been found!\n");
            }
        }
        public Message modifyElement(String docName, String xpath, String newValue)
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
                    this.modifyDocument(docName, ref xml);
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
