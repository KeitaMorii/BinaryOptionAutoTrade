using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace BinaryOptionAutoTrade
{

    /// <summary>
    /// 条件に縛られない共通処理
    /// </summary>
    class CommonControl
    {

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        /// <summary>
        /// クリックアクションを起こす
        /// </summary>
        public static void ClickAction()
        {
            //ディスプレイの高さ
            int displayHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            //ディスプレイの幅
            int displayWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

            int PointX = displayWidth / 2;
            int PointY = displayHeight / 2;

            SetCursorPos(PointX, PointY);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// 待機する
        /// </summary>
        /// <param name="millisecondsTo">待機するミリ秒</param>
        public static void SleepWait(int millisecondsTo)
        {
            System.Threading.Thread.Sleep(millisecondsTo);
        }


        /// <summary>
        /// デバッグ用
        /// </summary>
        /// <param name="args"></param>
        public static void DebugPrint(params string[] args)
        {
            string msg = string.Empty;

            System.Diagnostics.Trace.WriteLine("●●●Debug ===== ===== ===== ===== ===== TIME: " + DateTime.Now.ToString());
            foreach (string tmp in args) msg += tmp + ",";
            if (!string.IsNullOrEmpty(msg)) System.Diagnostics.Trace.WriteLine(msg);
        }

        /// <summary>
        /// デバッグ用
        /// </summary>
        /// <param name="e">例外</param>
        /// <param name="args">追記用メッセージ</param>
        public static void DebugPrint(Exception e, params string[] args)
        {
            string msg = string.Empty;

            System.Diagnostics.Trace.WriteLine("●●●Debug ===== ===== ===== ===== ===== ");

            foreach (string tmp in args) msg += tmp + ",";
            if (!string.IsNullOrEmpty(msg)) System.Diagnostics.Trace.WriteLine(msg);

            System.Diagnostics.Trace.WriteLine(e.Message);
            System.Diagnostics.Trace.WriteLine(e.StackTrace);
        }


        /// <summary>
        /// 結果用の型（クラス）に変換する
        /// </summary>
        /// <param name="boResultMsg"></param>
        public static SignalResult ConvBoResult(string boResultMsg)
        {
            SignalResult sigR = new SignalResult();     // 返り値用

            try
            {
                sigR.R_RawString = boResultMsg;                             // 生データ

                String[] tmp = boResultMsg.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                String[] tmpSub = tmp[2].Split(new string[] { " " }, StringSplitOptions.None);

                sigR.R_TradeBrand = tmp[0];                                 // ◆通貨ペア
                sigR.R_BidRate = tmp[1];                                    // ◆入札レート

                if (tmp.Count() > 2)
                {
                    if (tmpSub.Count() > 0) sigR.R_BidTime = tmpSub[0];     // ◆入札時間
                    if (tmpSub.Count() > 1) sigR.R_JudgeTime = tmpSub[1];   // ◆判定時間
                    if (tmpSub.Count() > 2) sigR.R_State = tmpSub[2];       // ◆ステータス
                    if (tmpSub.Count() > 3) sigR.R_JudgeRate = tmpSub[3];   // ◆判定レート
                    if (tmpSub.Count() > 4) sigR.R_BidPrice = tmpSub[4];    // ◆入札金額
                    if (tmpSub.Count() > 5) sigR.R_PayOut = tmpSub[5];      // ◆ペイアウト
                }

                // ◆結果判定
                if (sigR.R_PayOut.Length != 0)
                {
                    // ペイアウト結果が取得できているとき
                    // ペイアウトが２文字（\0）以上？
                    sigR.R_Result = sigR.R_PayOut.Length > 2 ? "[WIN]" : "[LOSE]";
                }
                else
                { sigR.R_Result = "[UNKOWN]"; }         // 判定不能

                // ◆HighLow　どっちか
                // HighまたはLowどっちで入れたか True & True 又は False & False の時Highとなる
                //                結果がWINか？             XOR                  入札レート ＜　結果レート
                sigR.R_Arrow = sigR.R_Result.Equals("[WIN]") ^ double.Parse(sigR.R_BidRate) < double.Parse(sigR.R_JudgeRate) ? "Low" : "High";

            }
            catch (Exception e)
            {
                // コンバートに失敗
                DebugPrint(e, "ConvBoResult");
                //throw e;
            }

            return sigR;
        }

        /// <summary>
        /// 取引履歴をスプレッドシートへ送信する
        /// </summary>
        /// <param name="bhc">送信するメッセージコンテナ 1つ</param>
        public static String PostBoResult(BoHistroyContainer bhc)
        {
            string ret = string.Empty;

            try
            {
                // 送信待ちでないものは終了 そのまま返す
                if (bhc.State != HistoryState.hsWaitPost) return bhc.PostResultData;

                SignalResult boResultSig = bhc.ResultSig;
                SignalOrder boEntrySig = bhc.Sig;

                // 送信用パラメータ文字列作成
                string postResultParam = string.Empty;

                postResultParam += "result=" + boResultSig.R_Result;            // 結果
                postResultParam += "&pair=" + boResultSig.R_TradeBrand;         // 通貨ペア
                postResultParam += "&rate=" + boResultSig.R_BidRate;            // 入札レート
                postResultParam += "&bidTime=" + boResultSig.R_BidTime;         // 入札時間
                postResultParam += "&judgeTime=" + boResultSig.R_JudgeTime;     // 判定時間
                postResultParam += "&state=Done";     // + boResultSig.R_State;             // ステータス Done固定
                postResultParam += "&judgeRate=" + boResultSig.R_JudgeRate ;                // 判定レート
                postResultParam += "&bidPrice=" + boResultSig.R_BidPrice.Replace("¥", "");  // 入札金額
                postResultParam += "&payout=" + boResultSig.R_PayOut.Replace("¥", "");      // ペイアウト
                postResultParam += "&signalName=" + bhc.Sig.signalRoomName;                 // シグナル名
                postResultParam += "&environment=" + bhc.Sig.getEnviromentNames();          // 環境の送信
                postResultParam += "&conditionPrice=" + boEntrySig.bidPrice;                // エントリー時の判断した時のレート価格

                // 部屋名のコンバート
                postResultParam += "&SignalName=DATA";

                //送信用コマンドを保持
                ret = bhc.PostURL_SpredSheet + postResultParam;

                Process cmd = new Process();
                cmd.StartInfo.FileName = "PowerShell.exe";
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;                                  //PowerShellのWindowを立ち上げずに実行。
                cmd.StartInfo.Arguments = @"curl '" + bhc.PostURL_SpredSheet + postResultParam + "'";   // 引数optionsをShellのコマンドとして渡す。
                cmd.Start();

                cmd.Close();

                return ret;
            }
            catch (Exception e)
            {
                DebugPrint(e, "postBoResult");

                bhc.PostResultData = string.Empty;

                return ret;
            }

        }

        /// <summary>
        /// 取引履歴をテキストにストックする
        /// </summary>
        /// <param name="bhc">記録するBOコンテナ</param>
        /// <returns> True 成功 / False 失敗 </returns>
        public static bool OutPutTextBoResult(BoHistroyContainer bhc)
        {
            try
            {
                // 送信済みのもののみ受け付け
                if (bhc.State != HistoryState.hsPostDataComplete) return false;

                // 作成するテキストファイル 1日単位 の命名
                string boFullFileName = Constants.PATH_BO_HISTORY_TEXT + @"\BO履歴_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                // 出力 URL付
                System.IO.File.AppendAllText(boFullFileName, bhc.PostResultData + Environment.NewLine);

                return true;
            }
            catch (Exception e)
            {
                DebugPrint(e, "OutPutTextBoResult");
                return false;
            }

        }

    }
}
