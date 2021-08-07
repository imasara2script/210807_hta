using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netHTA
{
    class シェル
    {
        public static string 実行(string path実行ファイル, string パラメータ = @"/c dir c:\ /w")
        {

            // 参考：https://dobon.net/vb/dotnet/process/standardoutput.html

            System.Diagnostics.Process プロセス = new System.Diagnostics.Process();

            // comspec = cmd.exeのパス
            // プロセス.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
            プロセス.StartInfo.FileName = path実行ファイル;

            // 出力を読み取れるようにする。
            プロセス.StartInfo.UseShellExecute = false;
            プロセス.StartInfo.RedirectStandardInput = false;
            プロセス.StartInfo.RedirectStandardOutput = true;

            // ウィンドウを表示しないようにする。
            プロセス.StartInfo.CreateNoWindow = true;

            プロセス.StartInfo.Arguments = パラメータ;

            // 実行
            プロセス.Start();

            // 出力を読み取る。
            string results = プロセス.StandardOutput.ReadToEnd();

            // プロセス終了まで待機する。
            //WaitForExitはReadToEndの後である必要がある
            //(親プロセス、子プロセスでブロック防止のため)
            プロセス.WaitForExit();
            プロセス.Close();

            return results;
        }

        public static void 非同期実行(string path実行ファイル, string パラメータ, Func<string, string> callBack)
        {
            System.Diagnostics.Process プロセス = new System.Diagnostics.Process();

            // comspec = cmd.exeのパス
            // プロセス.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
            プロセス.StartInfo.FileName = path実行ファイル;

            // 出力を読み取れるようにする。
            プロセス.StartInfo.UseShellExecute = false;
            プロセス.StartInfo.RedirectStandardOutput = true;

            // 参考：https://www.fenet.jp/dotnet/column/language/5722/

            string 文字列 = "";
            プロセス.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
            {
                文字列 += e.Data + "\r\n";
            };

            プロセス.EnableRaisingEvents = true;
            プロセス.Exited += new EventHandler((object sender, EventArgs e)=> {
                callBack(文字列);
            });

            // ウィンドウを表示しないようにする。
            プロセス.StartInfo.CreateNoWindow = true;

            プロセス.StartInfo.Arguments = パラメータ;

            // 実行
            プロセス.Start();

            //非同期で出力の読み取りを開始
            プロセス.BeginOutputReadLine();
        }
    }
}
