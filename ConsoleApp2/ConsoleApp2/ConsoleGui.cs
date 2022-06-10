using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Security;

namespace Project_app
{
    class ConsoleGui
    {
        readonly private DbManager db;
        private int accountId;

        public ConsoleGui()
        {
            db = new DbManager();
        }

        public String GetPassword()
        {
            String pwd = "";
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd = pwd.Remove(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000')
                {
                    pwd += i.KeyChar;
                    Console.Write("*");
                }
            }
            return pwd;
        }

        void Login()
        {
            Console.WriteLine("Log in to the system");
            do
            {
                Console.Write("Login: ");
                String log = Console.ReadLine();
                Console.Write("Password: ");
                String pass = GetPassword();
                Console.WriteLine();
                Message m = db.Login(log, pass);
                Console.WriteLine(m.content);
                accountId = m.status;
            } while (accountId < 0);
        }

        public void Start()
        {
            Console.WriteLine("Welcome in XML processing application\n");

            Login();

            Console.WriteLine("Enter command:");

            String input = Console.ReadLine();
            while(input.ToLower() != "exit"  && input.ToLower() != "quit" && input.ToLower() != "q")
            {
                HandleEvent(input);
                Console.WriteLine("Enter command:");
                input = Console.ReadLine();
            }
        }

        private void WriteHelp()
        {
            String s = "\n";
            s += "Available command: \n";
            s += "list - print list of all documents in database\n";
            s += "read - print selected document\n";
            s += "insert - add local document to database\n";
            s += "delete - remove selected document from database\n";
            s += "find - print node if element match requirements\n";
            s += "modify - change value of node, text or attribute in selected document\n";
            s += "give access - give access on document to other account\n";
            s += "logout - log out user and go back to log in part\n";
            s += "exit - close terminate\n";
            Console.WriteLine(s);
        }

        private void HandleEvent(String s)
        {
            String[] input  = s.Split(' ');
            if(input.Length > 0)
            {
                switch(input[0].ToLower())
                {
                    case "help":
                        WriteHelp();
                        break;
                    case "list":
                        Console.Write(db.GetAllDocuments(accountId).content);
                        break;
                    case "read":
                        if (input.Length > 1)
                            Console.Write(db.ReadXmlDocument(accountId, input[1]).content);
                        else
                            Console.Write("Enter 'insert + name' of document to read from database!\n\n");
                        break;
                    case "insert":
                        if (input.Length > 2)
                        {
                            FileReader reader = new FileReader();
                            reader.ReadFile(input[2]);
                            XmlDocument xmlObject = new XmlDocument();
                            if(reader.Good)
                            {
                                xmlObject.LoadXml(reader.Content);
                                Console.Write(db.InsertXmlDocument(accountId, input[1], ref xmlObject).content);
                            }
                            else
                            {
                                Console.Write("Couldn't find document with this name.");
                            }

                        }
                        else
                            Console.Write("Enter 'insert nameOfDocument documentLocalPath' to insert document!\n\n");
                        break;
                    case "delete":
                        if (input.Length > 1)
                            Console.Write(db.DeleteXMLDocument(accountId, input[1]).content);
                        else
                            Console.Write("Enter 'get + name' of document to read from database!\n\n");
                        break;
                    case "find":
                        if (input.Length > 1)
                            Console.Write(db.FindElement(accountId, input[1], input[2]).content);
                        else
                            Console.Write("Enter 'find documentName xpath' of document to find node or attribute!\n\n");
                        break;
                    case "modify":
                        if(input.Length > 1)
                        {
                            if(input.Length > 3)
                            {
                                Console.Write(db.ModifyElement(accountId, input[1], input[2], input[3]).content);
                            }
                            else
                            {
                                Console.Write("Enter 'modify documentName xPath newValue' to modify text!\n\n");
                            }
                        }
                        else
                        {
                            Console.Write("Specify what you want to modify: attribute, node or text");
                        }
                        break;
                    case "give":
                        if (input.Length > 3)
                        {
                            if (input[1].ToLower() == "access")
                            {
                                Console.Write(db.AddAccountAccess(accountId, input[2], input[3]).content);
                            }
                            else
                            {
                                Console.Write("Enter 'GRANT ACCESS documentName AccountName!\n\n");
                            }
                        }
                        else
                        {
                            Console.Write("Enter 'GIVE ACCESS documentName AccountName!\n\n");
                        }
                        break;
                    case "logout":
                        accountId = -1;
                        Login();
                        break;
                    default:
                        Console.Write("Command not recognized!\n\n");
                        break;
                }
            }
        }
    }
}
