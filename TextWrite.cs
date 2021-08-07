using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace netHTA
{
    class TextWrite
    {
        public static void 上書き(string path, string value)
        {
            File.WriteAllText(path, value);
        }

        public static void 追記(string path, string value)
        {
            File.AppendAllText(path, value);
        }
    }
}
