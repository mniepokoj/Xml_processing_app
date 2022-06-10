using System;

using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Project_app
{
    public class DbManager
    {
        public SqlConnection connection;
        public DbManager()
        {
            //
            
            connection = new SqlConnection(Project_app.Connection.Sqlconnection);
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

        public Message Login(String login, String password)
        {
            
            SqlCommand command = new SqlCommand("SELECT accountID from account WHERE AccountName=@login AND password=@pass", connection);
            command.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
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


        int GetXMLid(String docName)
        {
            SqlCommand command = new SqlCommand("SELECT xmlID from XMLTable WHERE name=@name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            Object o = command.ExecuteScalar();
            if (o == null)
                return -1;
            return (int)o;
        }


        public int CheckNodeExist(String docName, String xpath)
        {
            SqlCommand command = new SqlCommand("SELECT XmlColumn.exist('"+xpath+"') FROM xmlTable WHERE name='@docName'", connection);
            command.Parameters.Add("@docName", SqlDbType.VarChar).Value = docName;
            Object o = command.ExecuteScalar();
            if (o == null)
                return -1;
            return (int)o;
        }

        int GetDocumentId(String docName)
        {
            SqlCommand command = new SqlCommand("SELECT XmlId from XmlTable WHERE name=@name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = docName;
            Object o = command.ExecuteScalar();
            if (o == null)
                return -1;
            return (int)o;
        }

        int GetAccountId(String accountName)
        {
            SqlCommand command = new SqlCommand("SELECT accountId from Account WHERE AccountName=@name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = accountName;
            Object o = command.ExecuteScalar();
            if (o == null)
                return -1;
            return (int)o;
        }

        int ReplaceElement(String docName, String xpath, String newValue)
        {
            SqlCommand command = new SqlCommand("UPDATE XMLTable " +
                                                "SET XmlColumn.modify('replace value of "+xpath+" with "+ newValue + "') " +
                                                "FROM xmlTable WHERE Name=@docName;", connection);
            command.Parameters.Add("@docName", SqlDbType.VarChar).Value = docName;
            Object o = command.ExecuteScalar();
            if (o == null)
                return -1;
            return (int)o;
        }

        private bool CheckAccess(int accountId, String docName)
        {
            int xmlId = GetXMLid(docName);
            if(xmlId != -1)
            {
                SqlCommand command = new SqlCommand("SELECT accountID FROM XML_account WHERE accountID=@account_id AND xmlID=@xml_id", connection);
                command.Parameters.Add("@account_id", SqlDbType.Int).Value = accountId;
                command.Parameters.Add("@xml_id", SqlDbType.Int).Value = xmlId;
                Object o = command.ExecuteScalar();
                return o != null;
            }

            return false;
        }
   

        /*
         * function whitch insert XMLdocument into dataBase
         * @param xmlDoc document to be inserted
        */
        public Message InsertXmlDocument(int accountId, String name, ref XmlDocument xmlDoc)
        {
            SqlCommand command = new SqlCommand("EXEC insert_xml_doc_proc @name, @xmlDoc, @accountId", connection);
            command.Parameters.Add("@xmlDoc", SqlDbType.Xml).Value = xmlDoc.OuterXml;
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            command.Parameters.Add("@accountId", SqlDbType.Int).Value = accountId;
            try
            {
                Object o = command.ExecuteScalar();
                return new Message(1, "Document has been written to database!\n\n");
            }
            catch(SqlException e)
            {
                return new Message(0, e.Message + "\n\n");
            }
        }
        private String PrintElement(XmlNode xn)
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
        public Message ReadXmlDocument(int accountId ,String name) 
        {
            if(!CheckAccess(accountId, name))
            {
                return new Message(0, "You don't have acces to this document or the document doesn't exist!\n");
            }

            SqlCommand command = new SqlCommand("SELECT XMLColumn FROM XMLTable WHERE name = @name", connection);
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
            XmlReader reader = command.ExecuteXmlReader();
            if(reader.Read())
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(reader);

                XmlNode xn = xd.FirstChild;
                reader.Close();
                return new Message(1, ReadNodes(xn, "") + "\n\n");
            }
            else
            {
                reader.Close();
                return new Message(0, "Document '" + name + "' can not been found!\n");
            }
        }
        public Message DeleteXMLDocument(int accountId, String docName)
        {
            SqlCommand command = new SqlCommand("EXEC delete_xml_doc_proc @docname, @accountid", connection);
            command.Parameters.Add("@docname", SqlDbType.VarChar).Value = docName;
            command.Parameters.Add("@accountId", SqlDbType.Int).Value = accountId;
            try
            {
                Object o = command.ExecuteScalar();
                return new Message(1, "Document has been deleted from database!\n\n");
            }
            catch (SqlException e)
            {
                return new Message(0, e.Message + "\n\n");
            }
        }
        private String ReadNodes(XmlNode xn, String depth="")
        {
            String s = "";
            s += "\n" + depth;
            switch (xn.NodeType)
            {
                case XmlNodeType.Element:
                    s+=PrintElement(xn);
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
                s+= ReadNodes(x, depth);
            }
            if (xn.Name != "#text")
            {
                depth = depth.Remove(depth.Length - 1);
                s += String.Format("\n" + depth + "</{0}>", xn.Name);
            }
            return s;
        }
        public Message FindElement(int accountId ,String docName, String xpath)
        {
            if (!CheckAccess(accountId, docName))
            {
                return new Message(0, "You don't have acces to this document or the document doesn't exist!\n");
            }

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
                    s += ReadNodes(xn);
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
        public Message GetAllDocuments(int accountId)
        {
            String s = "";
            SqlCommand command = new SqlCommand("SELECT name FROM XMLTable " +
                "JOIN xml_account on xml_account.xmlId=xmlTable.xmlID WHERE xml_account.accountID = @accountID", connection);
            command.Parameters.Add("@accountID", SqlDbType.Int).Value = accountId;
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
        public Message ModifyElement(int accountId, String docName, String xpath, String newValue)
        {
            if (!CheckAccess(accountId, docName))
            {
                return new Message(0, "You don't have acces to this document or the document doesn't exist!\n");
            }
            if(CheckNodeExist(docName, xpath) == 0)
            {
                return new Message(0, "There is no node which match xpath query!\n");
            }
            else
            {
                if(ReplaceElement(docName, xpath, newValue) < 1)
                {
                    return new Message(0, "Unexpected error occured!\n");
                }
                else
                {
                    return new Message(1, "Value have been modified succesfully!\n");
                }
            }
            

        }

        public Message AddAccountAccess(int accountId, String docName, String accountName)
        {
            if (!CheckAccess(accountId, docName))
            {
                return new Message(0, "You don't have acces to this document or the document doesn't exist!\n");
            }
            int xmlId = GetXMLid(docName);
            int newAccountId = GetAccountId(accountName);

            if(accountId == newAccountId)
            {
                return new Message(0, "You can't give access to yourself!\n");
            }
            if(xmlId == -1 || newAccountId == -1)
            {
                return new Message(0, "Given accountName or document name doesn't exist.");
            }
            else
            {
                SqlCommand command = new SqlCommand("INSERT INTO Xml_Account(AccountId, XmlId) VALUES (@account_id, @xml_id)", connection);
                command.Parameters.Add("@account_id", SqlDbType.Int).Value = newAccountId;
                command.Parameters.Add("@xml_id", SqlDbType.Int).Value = xmlId;
                command.ExecuteNonQuery();
                return new Message(1, "Account " + accountName+" has got access to "+docName+".\n\n");
            }

        }
        ~DbManager()
        {
        }
    }
}
