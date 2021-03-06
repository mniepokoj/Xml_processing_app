using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Project_app
{
    class FileReader
    {
        bool good = true;

        public bool Good
        {
            get { return good; }
        }

        String content = null;

        public String Content
        {
            get { return content; }
        }

        public FileReader() {  }
        public void ReadFile(in String filename)
        {
            try
            {
                content = File.ReadAllText(filename);
                good = true;
            }
            catch(Exception)
            {
                content = null;
                good = false;
            }
        }
    }
}
