using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ConsoleApp2
{
    class ConsoleGui
    {
        private DbManager db;
        public ConsoleGui()
        {
            db = new DbManager();
        }

        public void Start()
        {
            Console.WriteLine("Welcome in XML processing application\n");
            Console.WriteLine("Enter command:");

            String input = Console.ReadLine();
            while(input.ToLower() != "exit"  && input.ToLower() != "quit" && input.ToLower() != "q")
            {
                handleEvent(input);
                Console.WriteLine("Enter command:");
                input = Console.ReadLine();
            }
        }

        private void writeHelp()
        {
            String s = "\n";
            s += "Available command: \n";
            s += "list - print list of all documents in database\n";
            s += "read - print selected document\n";
            s += "insert - add local document to database\n";
            s += "delete - remove selected document from database\n";
            s += "find - print node if  element match requirements\n";
            s += "modify - change value of node, text or attribute in selected document\n";
            Console.WriteLine(s);
        }

        private void handleEvent(String s)
        {
            String[] input  = s.Split(' ');
            if(input.Length > 0)
            {
                switch(input[0].ToLower())
                {
                    case "help":
                        writeHelp();
                        break;
                    case "list":
                        Console.Write(db.getAllDocuments().content);
                        break;
                    case "read":
                        if (input.Length > 1)
                            Console.Write(db.readXmlDocument(input[1]).content);
                        else
                            Console.Write("Enter 'insert + name' of document to read from database!\n\n");
                        break;
                    case "insert":
                        if (input.Length > 2)
                        {
                            FileReader reader = new FileReader();
                            reader.readFile(input[2]);
                            XmlDocument xmlObject = new XmlDocument();
                            xmlObject.LoadXml(reader.Content);
                            Console.Write(db.insertXmlDocument(input[1], ref xmlObject).content);
                        }
                        else
                            Console.Write("Enter 'insert nameOfDocument documentLocalPath' to insert document!\n\n");
                        break;
                    case "delete":
                        if (input.Length > 1)
                            Console.Write(db.deleteXMLDocument(input[1]).content);
                        else
                            Console.Write("Enter 'get + name' of document to read from database!\n\n");
                        break;
                    case "find":
                        if (input.Length > 1)
                            Console.Write(db.findAttribute(input[1], input[2]).content);
                        else
                            Console.Write("Enter 'find documentName xpath' of document to find node or attribute!\n\n");
                        break;
                    case "modify":
                        if(input.Length > 1)
                        {
                            if(input[1].ToLower() == "text")
                            {
                                if(input.Length > 4)
                                {
                                    Console.Write(db.modifyContent(input[2], input[3], input[4]).content);
                                }
                                else
                                {
                                    Console.Write("Enter 'modify text documentName xPath newValue' to modify text!\n\n");
                                }
                            }
                            else if(input[1].ToLower() == "attribute")
                            {
                                if (input.Length > 4)
                                {
                                    Console.Write(db.modifyAttribute(input[2], input[3], input[4]).content);
                                }
                                else
                                {
                                    Console.Write("Enter 'modify attribute documentName xPath newValue' to modify attribute!\n\n");
                                }
                            }
                            else if (input[1].ToLower() == "element")
                            {
                                if (input.Length > 4)
                                {
                                    Console.Write(db.modifyElement(input[2], input[3], input[4]).content);
                                }
                                else
                                {
                                    Console.Write("Enter 'modify element documentName xPath newValue' to modify element name!\n\n");
                                }
                            }
                        }
                        else
                        {
                            Console.Write("Specify what you want to modify: attribute, node or text");
                        }
                        break;
                    default:
                        Console.Write("Command not recognized!\n\n");
                        break;
                }
            }
        }
    }
}
