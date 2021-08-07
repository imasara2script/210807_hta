using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO; // directoryInfo

namespace netHTA
{
    class ディレクトリ
    {
        public static class カレント
        {
            public static void 設定(string path)
            {
                System.IO.Directory.SetCurrentDirectory(path);
            }
            public static string 取得()
            {
                return System.IO.Directory.GetCurrentDirectory();
            }
        }

        public static void 作成(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            di.Create();
        }

        public static string get親フォルダ(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            return di.Parent.FullName;
        }
    }
}
