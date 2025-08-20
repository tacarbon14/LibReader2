// #define DEBUG_MODE

using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using MessageBox = System.Windows.Forms.MessageBox;
using Clipboard = System.Windows.Clipboard;

//using System;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using System.Text;

// 山川増設部
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using System.Data.Odbc;
using System.Diagnostics;
using System.Globalization;
using System.Text;
//using Microsoft.VisualBasic;using System;
using System.Drawing;
using System.Web;
//using System.Web.SessionState;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;


namespace LibReader
{
    public partial class MainWindow : Window
    {
        //SMARTカード関係DLL S
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //APDU増設部
        // 定数定義
        private const uint SCARD_SCOPE_USER = 0;             // スコープ: ユーザー
        private const uint SCARD_SHARE_SHARED = 2;          // 共有モード
        private const uint SCARD_PROTOCOL_T0 = 1;           // プロトコルT0
        private const uint SCARD_PROTOCOL_T1 = 2;           // プロトコルT1

        private const byte TAG_No = 0x66; // 読み取るデータのタグ

        [DllImport("winscard.dll")]
        private static extern int SCardEstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext);

        [DllImport("winscard.dll")]
        private static extern int SCardListReaders(IntPtr hContext, string mszGroups, byte[] mszReaders, ref int pcchReaders);

        [DllImport("winscard.dll")]
        private static extern int SCardConnect(IntPtr hContext, string szReader, uint dwShareMode, uint dwPreferredProtocols, out IntPtr phCard, out IntPtr pdwActiveProtocol);

        [DllImport("winscard.dll")]
        private static extern int SCardTransmit(IntPtr hCard, ref SCARD_IO_REQUEST pioSendPci, byte[] pbSendBuffer, int cbSendLength, ref SCARD_IO_REQUEST pioRecvPci, byte[] pbRecvBuffer, ref int pcbRecvLength);

        [DllImport("winscard.dll")]
        private static extern int SCardDisconnect(IntPtr hCard, int dwDisposition);

        [DllImport("winscard.dll")]
        private static extern int SCardReleaseContext(IntPtr hContext);

        [StructLayout(LayoutKind.Sequential)]
        private struct SCARD_IO_REQUEST
        {
            public uint dwProtocol;  // プロトコル (T0/T1)
            public uint cbPciLength; // PCI構造体のサイズ
        }
        //SMARTCard 関係DLL E

        //キーボード入力処理 S
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentProcessId();

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref PROCESS_BASIC_INFORMATION processInformation, uint processInformationLength, out uint returnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2;
            public IntPtr Reserved3;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId; // 親プロセスのID
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

   //     [DllImport("user32.dll", SetLastError = true)]
  //      static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;

        private const int KEYEVENTF_KEYDOWN = 0x0;          // キーを押す
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;      // 拡張コード
        private const int VK_SHIFT = 0x10;                  // SHIFTキー 
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        //重複送信防止用
        private static string LastLibID = "";

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        //ushort scanCode = (ushort)MapVirtualKey((ushort)key, 0);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern short VkKeyScan(char ch);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
         //キーボード入力処理 E


        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();       //タイマー開始

            //
        }

        ////タイマー処理関数

        // タイマメソッド
        private void MyTimerMethod(object sender, EventArgs e)
        {

            buttonMuID_Click(this, new RoutedEventArgs()); //←ここをボタン押下の関数に差し替える

        }

        // タイマのインスタンス
        private DispatcherTimer _timer;


        // タイマを設定する
        private void SetupTimer()
        {
            // タイマのインスタンスを生成
            _timer = new DispatcherTimer(); // 優先度はDispatcherPriority.Background
                                            // インターバルを設定
                                            //_timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            // タイマメソッドを設定
            _timer.Tick += new EventHandler(MyTimerMethod);
            // タイマを開始
            _timer.Start();

            // 画面が閉じられるときに、タイマを停止
            this.Closing += new CancelEventHandler(StopTimer);
        }

        // タイマを停止
        private void StopTimer(object sender, CancelEventArgs e)
        {
            _timer.Stop();
        }



        //■■■■■MuID呼び出しボタン押下処理
        private void buttonMuID_Click(object sender, RoutedEventArgs e)
        {

            IntPtr hContext = IntPtr.Zero;
            IntPtr hCard = IntPtr.Zero;
            IntPtr activeProtocol = IntPtr.Zero;
            //String TatenaNo = "";
            LJyotai.Visibility = Visibility.Hidden;

#if DEBUG_MODE
            //DebugModeの時
            String utf8String = "1521756789012";    //13桁に変更 2025/8/19
            TBMuIDText.Text = utf8String;
            SendKeysToApp(utf8String);
            // Debug用に固定値を代入

#else
            //DebugModeでない時
            try
            {
                // コンテキストを作成
                int result = SCardEstablishContext(SCARD_SCOPE_USER, IntPtr.Zero, IntPtr.Zero, out hContext);
                if (result != 0)
                {
                    TextBoxclear();     //テキストボックス等初期化
                    throw new Exception($"Failed to establish context: {result}");
                }

                // 利用可能なリーダーを取得
                byte[] readersBuffer = new byte[2048];
                int readersBufferLength = readersBuffer.Length;
                result = SCardListReaders(hContext, null, readersBuffer, ref readersBufferLength);
                if (result != 0)
                {
                    TextBoxclear();     //テキストボックス等初期化
                    throw new Exception($"Failed to list readers: {result}");
                }

                // リーダー名を取得（最初のリーダーを使用）
                string readerName = Encoding.ASCII.GetString(readersBuffer, 0, readersBufferLength).Split('\0')[0];
                Console.WriteLine("Using reader: " + readerName);

                // カードに接続
                result = SCardConnect(hContext, readerName, SCARD_SHARE_SHARED, SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T1, out hCard, out activeProtocol);
                if (result != 0)
                {
                    TextBoxclear();     //テキストボックス等初期化
                    throw new Exception($"Failed to connect to card: {result}");

                }

                // APDUコマンド送信: SELECT AP (例: 00 A4 04 0C 0D D3 92 F0 00 24 EE 00 01 52 17 04 00 01)
                byte[] selectAPDU = { 0x00, 0xA4, 0x04, 0x0C, 0x0D, 0xD3, 0x92, 0xF0, 0x00, 0x24, 0xEE, 0x00, 0x01, 0x52, 0x17, 0x04, 0x00, 0x01 }; // AP名は例
                byte[] responseBuffer = new byte[256];
                int responseLength = responseBuffer.Length;

                SCARD_IO_REQUEST ioRequest = new SCARD_IO_REQUEST
                {
                    dwProtocol = (uint)activeProtocol,
                    cbPciLength = (uint)Marshal.SizeOf(typeof(SCARD_IO_REQUEST))
                };

                result = SCardTransmit(hCard, ref ioRequest, selectAPDU, selectAPDU.Length, ref ioRequest, responseBuffer, ref responseLength);
                if (result != 0)
                    Console.WriteLine("SELECT APDU Response: " + BitConverter.ToString(responseBuffer, 0, responseLength));
                //MessageBox.Show("SELECT APDU Response: " + BitConverter.ToString(responseBuffer, 0, responseLength));


                // APDUコマンド送信: GET DATA (例: 00 CA 66 00 00)
                //                byte[] getDataAPDU = { 0x00, 0xCA, 0x66, 0x00, 0x00 }; // GET DATAコマンド ChatGPT 0x66の位置おかしい
                byte[] getDataAPDU = { 0x00, 0xCA, 0x00, TAG_No, 0x00 }; // GET DATAコマンド 0x66 妙高市タグ
                responseLength = responseBuffer.Length;

                //MessageBox.Show($"Response Buffer Length: {responseBuffer.Length}", "Buffer Length");
                //test


                // レスポンス全体の表示 (デバッグ用)
                Console.WriteLine("Full response (including SW1, SW2): " + BitConverter.ToString(responseBuffer, 0, responseLength));
                //MessageBox.Show("Full response (including SW1, SW2): " + BitConverter.ToString(responseBuffer, 0, responseLength));

                result = SCardTransmit(hCard, ref ioRequest, getDataAPDU, getDataAPDU.Length, ref ioRequest, responseBuffer, ref responseLength);
                if (result != 0)
                {
                    TextBoxclear();     //テキストボックス等初期化
                    throw new Exception($"Failed to send GET DATA APDU: {result}");
                }
                //MessageBox.Show($"GET DATA Response: " + BitConverter.ToString(responseBuffer, 0, responseLength));

                //以下で値を返している
                // レスポンスデータの処理
                if (responseLength > 2)
                {

                    // 末尾の2バイト (SW1, SW2) を除外し、データ部分を抽出
                    byte[] data = new byte[responseLength - 2];
                    byte[] RID = new byte[13];
                    byte[] UIDdata = new byte[20];
                    byte[] AtenaNo = new byte[18];

                    //読みだした値を上記3値に複写(AtenaNoデコード未処理)
                    //Array.Copy(responseBuffer, 0, data, 0, data.Length);
                    Array.Copy(responseBuffer, 0, data, 0, responseLength - 2);
                    Array.Copy(responseBuffer, 2, RID, 0, 13);
                    Array.Copy(responseBuffer, 17, UIDdata, 0, 20); //上位2文字を無視TLV2のヘッダ)
                    //Array.Copy(responseBuffer, 28, AtenaNo, 0, 9); //下位9文字のみ複写

                    byte[] cleanData2 = UIDdata.Where(b => b != 0x00).ToArray();
                    String TNAtenaNo = AtenaDec(Encoding.ASCII.GetString(cleanData2)); //UIDをデコード・整形
                    //UID(宛名番号)から、8文字切り出し→符号化前のLibID
                    String LibID = TNAtenaNo.Length >= 8 ? TNAtenaNo.Substring(TNAtenaNo.Length - 8, 8) : TNAtenaNo;
                    LibID = LibIDEnc(LibID); //LibIDに符号化を施す
                    LibID = "15217" + LibID;    //J-LIS標準ID化

//                    string utf8String = Encoding.UTF8.GetString(RID);
//                    SendKeysToApp(utf8String);

                    // データを利用する例
                    //Console.WriteLine("Data (hex): " + BitConverter.ToString(data));
                    //Console.WriteLine("Data (ASCII): " + Encoding.ASCII.GetString(data));
                    byte[] cleanData0 = UIDdata.Where(b => b != 0x00).ToArray();
                    //TBMuIDText.Text = Encoding.ASCII.GetString(cleanData0);
                    TBMuIDText.Text = LibID;    
                    // NULL終端文字を削除
                    byte[] cleanData1 = RID.Where(b => b != 0x00).ToArray();
                    TBRID.Text = Encoding.ASCII.GetString(cleanData1);
                    //TatenaNo = Encoding.ASCII.GetString(cleanData1);
                    //TBMyKeyIDText.Text = BitConverter.ToString(data);

                    if (LibID == LastLibID)
                    {

                    }
                    else
                    { 
                        SendKeysToApp(LibID);   //フォアグラウンドプロセスにLibID(符号化後)を戻す
                        //Clipboard.SetText(Encoding.ASCII.GetString(RID)); 
                        Clipboard.SetText(LibID);   // クリップボードへLibIDをコピー
                        LastLibID = LibID;
                    }

                    //// 例: 必要に応じて整数として処理
                    //if (data.Length >= 1)
                    //{
                    //    int value = data[0]; // 先頭バイトを整数値として扱う例
                    //    Console.WriteLine("First byte as integer: " + value);
                    //}
                }
                else
                {

                    TextBoxclear();     //テキストボックス等初期化
                    Console.WriteLine("No data returned or invalid response.");

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // リソース解放
                if (hCard != IntPtr.Zero)
                {
                    SCardDisconnect(hCard, 0);
                }
                if (hContext != IntPtr.Zero)
                {
                    SCardReleaseContext(hContext);
                }
            }
#endif
        }

        // ■■■■■■■■■■　キーボード入力処理関係

        public static IntPtr GetForegroundWindowHandle()
        {
            // ForegroundWindowのハンドルをAPI関数を使って返す            
            return GetForegroundWindow();
        }

        public static IntPtr GetParentProcessMainWindow()
        {
            //この関数を呼び出すと、戻り値に親プロセスのウィンドウハンドルがセットされる。
            int parentProcessId = GetParentProcessId();
            if (parentProcessId == 0) return IntPtr.Zero;

            IntPtr foundWindow = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                if (processId == parentProcessId)
                {
                    foundWindow = hWnd;
                    return false; // 目的のウィンドウを見つけたので列挙を停止
                }
                return true;
            }, IntPtr.Zero);

            return foundWindow;
        }

        private static int GetParentProcessId()
        {
            //GetParentProsseMainWindowが呼び出す下位関数
            int currentProcessId = GetCurrentProcessId();
            Process currentProcess = Process.GetProcessById(currentProcessId);

            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            uint returnLength;
            int status = NtQueryInformationProcess(currentProcess.Handle, 0, ref pbi, (uint)Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION)), out returnLength);

            if (status == 0) // 成功
            {
                return (int)pbi.InheritedFromUniqueProcessId;
            }
            return 0;
        }

        private string GetSmartCardData()
        {
            return "SmartCard123"; // 実際はカードリーダーから取得
        }


        //■これが本体関数
        private void SendKeysToApp(string text)
        {
//            IntPtr hWnd = GetParentProcessMainWindow();     //呼び出し元のWindowハンドルを取得
            
            IntPtr hWnd = GetForegroundWindowHandle();     //フォアグラウンドウィンドウのWindowハンドルを取得
            //           IntPtr hWnd = FindWindow(null, "無題 - メモ帳");
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("指定されたアプリが見つかりません");
                return;
            }

            //MessageBox.Show("ここまで動作");

            //BringWindowToFront(hWnd);
            //SetForegroundWindow(hWnd);
            //Thread.Sleep(100); // フォーカス移動を確実にするために少し待つ '遅い原因
            //SendKeys.SendWait(text);
            //SendTextWithSendInput(hWnd,text);

            SendTextWithPostMessage(hWnd,text);

        }

        //inputversion
        static void SendTextWithSendInput(IntPtr hWnd,string text)
        {
            //うまく動作しないため、いったん保留(この関数は使っていない)
            SetForegroundWindow(hWnd);
            Thread.Sleep(2000);

            foreach (char c in text)
            {
                short vKey = VkKeyScan(c);
                if (vKey == -1) continue; // 変換失敗時はスキップ

                INPUT[] inputs = new INPUT[2];

                // キーダウン
                inputs[0].type = 1; // Keyboard
                inputs[0].u.ki.wVk = (ushort)Keys.A;
                inputs[0].u.ki.wScan = (ushort)MapVirtualKey(inputs[0].u.ki.wVk, 0);  // 仮想キーコードを使う場合はスキャンコードを設定しない
                inputs[0].u.ki.dwFlags = KEYEVENTF_KEYDOWN;  // 押下
                inputs[0].u.ki.dwExtraInfo = (IntPtr)0;
                inputs[0].u.ki.time = 0;

                // キーアップ
                inputs[1].type = 1; // Keyboard
                inputs[1].u.ki.wVk = (ushort)Keys.A;
                inputs[1].u.ki.wScan = (ushort)MapVirtualKey(inputs[1].u.ki.wVk, 0);
                inputs[1].u.ki.dwFlags = KEYEVENTF_KEYUP;  // 放す
                inputs[1].u.ki.dwExtraInfo = (IntPtr)0;
                inputs[1].u.ki.time = 0;

                // SendInput を実行し、成功した回数を取得
                uint result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
                //uint result = SendInput((uint)2, inputs, Marshal.SizeOf(inputs[0]));

                Console.WriteLine($"送信: {c} (VK={vKey & 0xFF}), SendInput結果: {result}");

                Thread.Sleep(100);
            }
        }

        static void SendTextWithPostMessage(IntPtr hWnd,string text)
        {
            foreach (char c in text)
            {
                short vKey = VkKeyScan(c);
                if (vKey == -1) continue; // 変換失敗時はスキップ

                PostMessage(hWnd, 0x0100, VkKeyScan(c), 0);

                //Console.WriteLine($"送信: {c} (VK={vKey & 0xFF}), SendInput結果: {result}");

                Thread.Sleep(70);
            }


        }
        //iputversion end           

        static void BringWindowToFront(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
            for (int i = 0; i < 10; i++) // 最大10回チェック
            {
                if (GetForegroundWindow() == hWnd)
                {
                    Console.WriteLine("フォーカス成功");
                    return;
                }
                Thread.Sleep(500); // 少し待機
            }
            Console.WriteLine("フォーカス移動失敗の可能性あり");
        }

        public void TextBoxclear()
        {
            TBRID.Text = ""; //カードがないときはクリア、終了
            TBMuIDText.Text = "";
            LastLibID = "";
        }

        public static string AtenaDec(string TMuID)
        {
            string TempMuIDA = "";
            string TempMuIDB = "";

            // Null または空文字チェック
            if (string.IsNullOrEmpty(TMuID))
            {
                return "0";
            }

            char mode = TMuID[0];

            if (mode == '0') // 0モード
            {
                TempMuIDB = TMuID.Substring(2, 18); // Mid(3,18) 相当（0始まりなので2から）
            }
            else if (mode == '1') // 1モード
            {
                // 補数を戻す
                for (int i = 0; i < 18; i++)
                {
                    int digit = int.Parse(TMuID.Substring(i + 2, 1));
                    TempMuIDA += (9 - digit).ToString();
                }

                // 並び替え
                TempMuIDB =
                    TempMuIDA.Substring(17, 1) + // No1
                    TempMuIDA.Substring(1, 1) +  // No2
                    TempMuIDA.Substring(15, 1) + // No3
                    TempMuIDA.Substring(3, 1) +  // No4
                    TempMuIDA.Substring(13, 1) + // No5
                    TempMuIDA.Substring(5, 1) +  // No6
                    TempMuIDA.Substring(11, 1) + // No7
                    TempMuIDA.Substring(7, 1) +  // No8
                    TempMuIDA.Substring(8, 1) +  // No9
                    TempMuIDA.Substring(9, 1) +  // No10
                    TempMuIDA.Substring(10, 1) + // No11
                    TempMuIDA.Substring(6, 1) +  // No12
                    TempMuIDA.Substring(12, 1) + // No13
                    TempMuIDA.Substring(4, 1) +  // No14
                    TempMuIDA.Substring(14, 1) + // No15
                    TempMuIDA.Substring(2, 1) +  // No16
                    TempMuIDA.Substring(16, 1) + // No17
                    TempMuIDA.Substring(0, 1);   // No18
            }
            else if (mode == '2') // 2モード
            {
                // (10 + i - digit) % 10 を戻す
                for (int i = 0; i < 18; i++)
                {
                    int digit = int.Parse(TMuID.Substring(i + 2, 1));
                    TempMuIDA += ((10 + (i + 1) - digit) % 10).ToString();
                }

                // 並び替え
                TempMuIDB =
                    TempMuIDA.Substring(17, 1) + // No1
                    TempMuIDA.Substring(1, 1) +  // No2
                    TempMuIDA.Substring(15, 1) + // No3
                    TempMuIDA.Substring(3, 1) +  // No4
                    TempMuIDA.Substring(13, 1) + // No5
                    TempMuIDA.Substring(5, 1) +  // No6
                    TempMuIDA.Substring(11, 1) + // No7
                    TempMuIDA.Substring(7, 1) +  // No8
                    TempMuIDA.Substring(8, 1) +  // No9
                    TempMuIDA.Substring(9, 1) +  // No10
                    TempMuIDA.Substring(10, 1) + // No11
                    TempMuIDA.Substring(6, 1) +  // No12
                    TempMuIDA.Substring(12, 1) + // No13
                    TempMuIDA.Substring(4, 1) +  // No14
                    TempMuIDA.Substring(14, 1) + // No15
                    TempMuIDA.Substring(2, 1) +  // No16
                    TempMuIDA.Substring(16, 1) + // No17
                    TempMuIDA.Substring(0, 1);   // No18
            }
            else
            {
                Console.WriteLine("未知の暗号化方式です");
                TempMuIDB = "0";
            }

            return TempMuIDB;
        }

        public static string LibIDEnc(string LibID)
        {
            string TempLibIDA = "";
            //string TempLibIDB = "";

            // Null または空文字チェック
            if (string.IsNullOrEmpty(LibID))
            {
                return "0";
            }

            char mode = LibID[0];

            if (mode == '0') // 0モード
            {
                for(int i = 1;i<= 8;i++){
                    TempLibIDA += ((15 - int.Parse(LibID.Substring(i - 1, 1))) % 10).ToString("0");
                } //5ずらす
            }
            else
            {
                Console.WriteLine("未知の暗号化方式です");
                TempLibIDA = "0";
            }

            return TempLibIDA;
        }
    }

}