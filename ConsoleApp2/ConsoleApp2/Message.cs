using System;
using System.Collections.Generic;
using System.Text;

namespace Project_app
{
    public class Message
    {
        public int status;
        public string content;
        public Message(int s, string c)
        {
            status = s;
            content = c;
        }
    }
}
