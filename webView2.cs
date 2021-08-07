using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// CoreWebView2NavigationCompletedEventArgsの参照に使用
using Microsoft.Web.WebView2.Core;

namespace netHTA
{
    class webView2関係
    {
        public static bool ページ偏移の有効化 = false;
        public static bool ブラウザの起動許可 = false;
        public static bool pathが無効 = false;

        public static void 初期化(string path)
        {
            CoreWebView2 cwv2 = MainWindow.cwv2;

            cwv2.NavigationCompleted += Web_NavigationCompleted;
            cwv2.WebMessageReceived  += MessageReceived;
            cwv2.NewWindowRequested  += NewWindowRequested;

            if (!System.IO.File.Exists(path))
            {
                // 指定されたpathのファイルが存在しない場合は
                // ページ偏移を許可し、画面内へのドラッグ＆ドロップなど出来るようにしておく。
                // 初回偏移時にページ偏移を無効化する。
                ページ偏移の有効化 = true;
                pathが無効 = true;
                デフォルトページを表示();
                return;
            }

            nav(path);
        }

        private static void デフォルトページを表示()
        {
            string ソース = リソース.テキスト読み込み("netHTA.リソース.default.index.html");
            if (false)
            {
                // 以下だとブラウザ側のデベロッパーツールにソースが表示されないため、デバッグ困難。
                MainWindow.cwv2.NavigateToString(ソース);
                return;
            }

            string path = MainWindow.実行ファイルのフォルダ + "/リソース/default/index.html";
            ディレクトリ.作成(ディレクトリ.get親フォルダ(path));
            TextWrite.上書き(path, ソース);
            MainWindow.cwv2.Navigate(path);
        }

        private static void nav(string path)
        {
            // ページ内からの相対Pathが正しく機能するようにカレントディレクトリを変更する。
            ディレクトリ.カレント.設定(System.IO.Path.GetDirectoryName(path));
            MainWindow.cwv2.Navigate(path);
        }

        private static void Web_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // 起動後初回のNavigateが完了してから、それ以降のNavigateを監視する。
            MainWindow.cwv2.NavigationStarting += navigationStatind;
        }

        // C#だと処理が難しい、複数階層のJSONをJS側で処理したりするのに使うメソッド。
        public static async Task<string> JS実行(string 実行可能文字列)
        {
            return await MainWindow.cwv2.ExecuteScriptAsync(実行可能文字列);
        }

        //JavaScriptからメッセージを受信したときに実行します。
        private static async void MessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            String text = args.TryGetWebMessageAsString();

            if (await 正規表現.マッチしたら実行(text, "^hta=([A-Za-z0-9]+)$", async (string[] 配列) => {
                string 文字列;
                文字列 = JSON.シリアライズ(TextRead.getAll(MainWindow.実行ファイルのフォルダ + "/リソース/hta.js"));
                JS実行(配列[0] + "(" + 文字列 + ")");
                return true;
            })) { return; }

            if (await 正規表現.マッチしたら実行(text, "^hta\\((.+)\\)$", async (string[] 配列) => {
                await hta_jsファイルをブラウザ内ページに追加(配列[0]);
                // 追加した直後はscriptファイルのローディングが完了していない。
                // ローディング完了した事を通知する仕組みを作る必要がある。
                // JS実行(配列[0] + "()");
                return true;
            })) { return; }

            JSへの応答.msg解釈(text);
        }

        private static async Task<string> hta_jsファイルをブラウザ内ページに追加(string loadハンドラー="null")
        {
            if (loadハンドラー.Length == 0) { loadハンドラー = "null"; }
            return await JS実行(@"
                !(()=>{
                    const el = document.createElement('script');
                    let path = '-path-'
                    path = path.slice(1, path.length-1)
                    el.src = path
                    el.onload = -loaded-
                    document.body.appendChild(el)
                })()
            ".Replace("-path-", JSON.シリアライズ(MainWindow.実行ファイルのフォルダ + "/リソース/hta.js"))
             .Replace("-loaded-", loadハンドラー));
        }

        private static class JSへの応答
        {
            public static void 値を返す(Dictionary<string, string> dic, string 戻り値){
                if (!dic.ContainsKey("callBack")) { return; }
                string str = "hta.WPF.callBack(" + dic["callBack"] + ", " + 戻り値 + ")";
                MainWindow.別スレッドから要素を操作(()=> {
                //    dynamic res = MainWindow.cwv2.ExecuteScriptAsync(str);
                    MainWindow.cwv2.ExecuteScriptAsync(str);
                    return "";
                });
            }

            private static void 完了通知(Dictionary<string, string> dic)
            {
                // 値は返さなくて良いが処理が完了した事はJS側に通知して欲しいケースで使用する。
                値を返す(dic, "null");
            }

            public static async void msg解釈(string msg)
            {
                Dictionary<string, string> dic = await JSON.getDic(msg);
                Dictionary<string, string> dic引数;
                MainWindow ウィンドウ = MainWindow.ウィンドウ;
                string 文字列;
                Int32 int値;
                Type プロパティの型;

                switch (dic["機能の名前"])
                {
                    case "AppActivate":
                        return;
                    case "icon":
                        var getBF = new getBitmapFrame();
                        ウィンドウ.ウィンドウのプロパティを変更("Icon", getBF.fromString(dic["引数"]));
                        完了通知(dic);
                        return;
                    case "Close":
                        return;
                    case "CMD同期":
                        dic引数 = await JSON.getDic(dic["引数"]);
                        文字列 = シェル.実行(dic引数["path"], dic引数["パラメータ"]);
                        文字列 = JSON.シリアライズ(文字列);
                        値を返す(dic, 文字列);
                        return;
                    case "CMD非同期":
                        dic引数 = await JSON.getDic(dic["引数"]);
                        シェル.非同期実行(dic引数["path"], dic引数["パラメータ"], (string str) => {
                            str = JSON.シリアライズ(str);
                            値を返す(dic, str);
                            return str;
                        });
                        return;
                    case "TextRead":
                        文字列 = TextRead.getAll(dic["引数"]);
                        文字列 = JSON.シリアライズ(文字列);
                        値を返す(dic, 文字列);
                        return;
                    case "TextWrite":
                        dic引数 = await JSON.getDic(dic["引数"]);
                        TextWrite.上書き(dic引数["path"], dic引数["value"]);
                        完了通知(dic);
                        return;
                    case "カレントディレクトリ":
                        ディレクトリ.カレント.設定(dic["引数"]);
                        return;
                    case "ページ偏移の有効化":
                        switch (dic["引数"])
                        {
                            case "true":
                                ページ偏移の有効化 = true;
                                文字列 = "true";
                                break;
                            case "false":
                                ページ偏移の有効化 = false;
                                文字列 = "true";
                                break;
                            default:
                                文字列 = "値が不正です。true or false。大文字小文字は区別します。";
                                break;
                        }
                        値を返す(dic, 文字列);
                        return;
                    case "ブラウザの起動許可":
                        switch (dic["引数"])
                        {
                            case "true":
                                ブラウザの起動許可 = true;
                                文字列 = "true";
                                break;
                            case "false":
                                ブラウザの起動許可 = false;
                                文字列 = "true";
                                break;
                            default:
                                文字列 = "値が不正です。true or false。大文字小文字は区別します。";
                                break;
                        }
                        値を返す(dic, 文字列);
                        return;
                    default:
                        if (dic["機能の名前"].IndexOf("get") == 0)
                        {
                            string 項目 = dic["機能の名前"].Substring(3);
                            switch (項目)
                            {
                                case "Args":
                                    値を返す(dic, "'" + string.Join("','", ウィンドウ.get引数()).Replace("\\", "/") + "'");
                                    return;
                                    /*
                                case "Height":
                                case "Left":
                                case "Top":
                                case "Width":
                                    値を返す(dic, ウィンドウ.ウィンドウのプロパティ値を取得<Double>(項目).ToString());
                                    return;
                                    */
                                case "カレントディレクトリ":
                                    値を返す(dic, ディレクトリ.カレント.取得());
                                    return;
                                default:
                                    プロパティの型 = ウィンドウ.ウィンドウのプロパティの型を取得(項目);
                                    switch (プロパティの型.Name) {
                                        case "String":
                                            値を返す(dic, ウィンドウ.ウィンドウのプロパティ値を取得<String>(項目));
                                            return;
                                        case "Double":
                                            値を返す(dic, ウィンドウ.ウィンドウのプロパティ値を取得<Double>(項目).ToString());
                                            return;
                                        default:
                                            // 任意の型のプロパティを読み出せるようにしたいが
                                            // 動的メソッドはデメリットもそこそこありそうなので、とりあえず対応できる型は限定する。
                                            値を返す(dic, "["+項目+"]プロパティは["+プロパティの型.Name+"]型で、この型の読み込みはサポートしていません。");
                                            return;
                                    }
                            }
                        }

                        string プロパティ名 = dic["機能の名前"];
                        プロパティの型 = ウィンドウ.ウィンドウのプロパティの型を取得(プロパティ名);
                        switch (プロパティの型.Name)
                        {
                            case "null":
                                値を返す(dic, "[" + プロパティ名 + "]という名前のプロパティはありません");
                                return;
                            case "String":
                                ウィンドウ.ウィンドウのプロパティを変更(dic["機能の名前"], dic["引数"]);
                                break;
                            default:
                                if (int.TryParse(dic["引数"], out int値))
                                {
                                    ウィンドウ.ウィンドウのプロパティを変更(dic["機能の名前"], int値);
                                }
                                break;
                        }
                        完了通知(dic);
                        return;
                }
            }
        }

        // https://teratail.com/questions/320460
        private static void navigationStatind(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if(e.Uri == MainWindow.cwv2.Source)
            {
                // F5キーなどでの更新は許可する。
                return;
            }

            if (!ページ偏移の有効化) {
                e.Cancel = true;
                return;
            }

            if (pathが無効) {
                ページ偏移の有効化 = false;
                pathが無効         = false;
            }
        }

        private static async void NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true; // NewWindowのキャンセル
            if (ブラウザの起動許可)
            {
                シェル.実行("cmd", "start " + e.Uri);
            }
            else
            {
                string path = e.Uri;
                if (System.IO.Path.GetExtension(path) == ".hta+")
                {
                    path = await htaファイル解釈.開始((new Uri(path)).LocalPath);
                    if (System.IO.File.Exists(path))
                    {
                        // 元から絶対Pathだったとしても無視して絶対Pathへの変換を実行する。
                        path = System.IO.Path.GetFullPath(path);
                    }
                }
                nav(path);
            }
        }
    }
}
