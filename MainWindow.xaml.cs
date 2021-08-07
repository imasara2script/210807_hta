using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Web.WebView2.Core;

namespace netHTA
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow ウィンドウ;
        public static CoreWebView2 cwv2;
        public static string 実行ファイルのフォルダ = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();

            // ウィンドウのプロパティを変更("Title", "abccc");
        }

        async void InitializeAsync()
        {
            await web.EnsureCoreWebView2Async(null);

            ウィンドウ = this;
            cwv2 = web.CoreWebView2;

            string[] 引数 = get引数();
            if (引数.Length == 1) { 引数 = new string[] { 引数[0], Environment.CurrentDirectory + "/index.html" }; }

            string path = 引数[1];
            if (System.IO.Path.GetExtension(path) == ".hta+")
            {
                path = await htaファイル解釈.開始(path);
            }

            if (System.IO.File.Exists(path))
            {
                // 元から絶対Pathだったとしても無視して絶対Pathへの変換を実行する。
                path = System.IO.Path.GetFullPath(path);
            }

            webView2関係.初期化(path);
        }

        public string[] get引数()
        {
            string[] args = Environment.GetCommandLineArgs();
            return args;
        }

        public static void 別スレッドから要素を操作(Func<string> func)
        {
            // 非同期にすると別スレッドになり、別スレッドになるとメインスレッドが所有するコントロールにアクセスできないので
            // メインスレッドのDispatcher(キューを管理するクラス)に実行を依頼する。
            /*
            ウィンドウ.Dispatcher.Invoke(async () =>
            {
                var webTitle = await wv2.ExecuteScriptAsync("document.title");
                ウィンドウ.Title = webTitle;
            });
            */
            ウィンドウ.Dispatcher.Invoke(func);
        }

        public void ウィンドウのプロパティを変更<Type>(string プロパティ名, Type 値)
        {
            this.GetType().GetProperty(プロパティ名).SetValue(this, 値, null);
        }

        public T ウィンドウのプロパティ値を取得<T>(string プロパティ名)
        {
            var prop = this.GetType().GetProperty(プロパティ名);
            dynamic 値 = prop.GetValue(this);
            return 値;
        }

        public Type ウィンドウのプロパティの型を取得(string プロパティ名)
        {
            var prop = this.GetType().GetProperty(プロパティ名);

            // 指定された名前のプロパティが存在しない場合、propにはnullが入っている。
            if (prop==null) { return null; }

            Type 型 = prop.PropertyType;
            return 型;
        }

    }
}