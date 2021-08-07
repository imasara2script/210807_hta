using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netHTA
{
    class htaファイル解釈
    {
        public static async Task<string> 開始(string path)
        {
            Dictionary<string, string> dicHTA = await JSON.getDic(TextRead.getAll(path));

            ディレクトリ.カレント.設定(System.IO.Path.GetDirectoryName(path));

            if (chkDic(dicHTA, "ページ偏移の有効化", "true"))
            {
                webView2関係.ページ偏移の有効化 = true;
            }

            if (chkDic(dicHTA, "ブラウザの起動許可", "true"))
            {
                webView2関係.ブラウザの起動許可 = true;
            }

            return dicHTA["html"];
        }

        private static bool chkDic(Dictionary<string, string> dic, string 項目名, string 値)
        {
            if (!dic.ContainsKey(項目名) || dic[項目名]!=値) { return false; }
            return true;
        }
    }
}
