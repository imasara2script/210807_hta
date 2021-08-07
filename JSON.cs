using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

namespace netHTA
{
    class JSON
    {
        public static Dictionary<string, string> デシリアライズ(string コード)
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(コード);
            return json;
        }

        public static string シリアライズ<TValue>(TValue 引数)
        {
            var 文字列 = JsonSerializer.Serialize(引数);
            return 文字列;
        }

        public static async Task<Dictionary<string, string>> getDic(string 文字列)
        {
            // 階層構造のobjectをそのままデシリアライズするのは
            // 受け入れ先の型宣言が面倒臭い感じになるので
            // 階層構造の場合は子階層はJS側でstring型にシリアライズして
            // 常にDictionary<string, string>の形になるようにするメソッド。

            文字列 = await webView2関係.JS実行(@"
                '' + ((obj)=>{
                    for(const name in obj){
                        const 値 = obj[name]
                        switch(typeof(値)){
                            case 'object':
                            case 'array':
                                obj[name] = JSON.stringify(値)
                                break
                            default:
                                obj[name] = 値.toString()
                        }
                    }
                    return JSON.stringify(obj)
                })
            " + "(" + 文字列 + ")");

            文字列 = 正規表現.変換(文字列, "^\"|\"$", "").Replace("\\\"", "\"").Replace("\\\\", "\\");
            return JSON.デシリアライズ(文字列);
        }

    }
}
