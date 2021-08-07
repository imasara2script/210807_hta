using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace netHTA
{
    class 正規表現
    {
        public static string 変換(string 対象, string code, string 変換後)
        {
            return Regex.Replace(対象, code, 変換後);
        }

        public static bool match(string 対象, string code)
        {
            return Regex.IsMatch(対象, code);
        }

        public static string[] matches(string 対象, string code)
        {
            // System.Text.RegularExpressions.MatchCollection
            System.Text.RegularExpressions.MatchCollection c = Regex.Matches(対象, code);
            string[] arr = new string[c.Count];

            int i = 0;
            foreach (System.Text.RegularExpressions.Match m in c)
            {
                arr[i] = m.Groups[i+1].Value;
                i++;
            }

            return arr;
        }

        public static async Task<bool> マッチしたら実行(string 対象, string code, Func<string[], Task<bool>> func)
        {
            string[] 配列 = matches(対象, code);
            if (配列.Length==0) { return false; }
            return await func(配列);
        }
    }
}
