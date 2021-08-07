using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netHTA
{
    class リソース
    {
        public static string テキスト読み込み(string path)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.StreamReader sr = new System.IO.StreamReader(asm.GetManifestResourceStream(path)); // path 例 "Project1.TextFile1.txt"
            string 文字列 = sr.ReadToEnd();
            sr.Close();

            return 文字列;
        }
    }
}
