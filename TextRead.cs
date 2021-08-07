using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace netHTA
{
    class TextRead
    {
        public static String getAll(String path)
        {
            StreamReader sr = new StreamReader(path); // , Encoding.GetEncoding("Shift_JIS")
            string str = sr.ReadToEnd();
            sr.Close();
            return str;
        }
    }
}
