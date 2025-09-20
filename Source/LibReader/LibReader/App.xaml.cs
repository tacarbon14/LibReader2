using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace LibReader
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        protected override void OnStartup(StartupEventArgs e)
        {
            string processName = "LibReader"; // exe名（拡張子なし）

            // 自分自身のプロセスIDを取得
            int currentProcessId = Process.GetCurrentProcess().Id;

            // 既に起動中のプロセスがあるかチェック（自分自身は除外）
            var existingProcess = Process.GetProcessesByName(processName)
                                         .FirstOrDefault(p => p.Id != currentProcessId);

            if (existingProcess != null)
            {
                // 既存のウィンドウを前面に出す
                IntPtr hWnd = existingProcess.MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE); // 最小化されていたら元に戻す
                    SetForegroundWindow(hWnd); // フォーカスを当てる
                }

                // 新しいプロセスを終了
                Shutdown();
                return;
            }

            // 通常のアプリ起動処理
            base.OnStartup(e);
        }
    }
}

