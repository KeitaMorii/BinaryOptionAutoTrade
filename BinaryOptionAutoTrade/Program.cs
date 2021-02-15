using System.Collections.Generic;
using System.Threading.Tasks;
using static BinaryOptionAutoTrade.CommonDriverControl;
using System.Windows;
using System.Windows.Forms;
using System;
using System.Collections;

/*
 * 今問題になっている現象 
 * 一時間以上寝かせるとシグナルが来ても反応しない？　→ 原因不明のまま解決(要注意)
 * 
 * 2019/07/26
 * BO結果がどのシグナルでエントリーしたものかを判断できるようにする
 * 
 * 2019/09/25
 * 祝 シグナル判断できるようになる
 * 
 * シグナル単位で、出力先シート(スプレッドシート)を変更する
 * 
 * デモとホントレ用にクラスを分けてprogramモジュールで管理する方向にする
 * 
 */

/*
 * メインコード
 */
namespace BinaryOptionAutoTrade
{
    class Program
    {
        /// <summary>
        /// クリックアクショントリガ
        /// </summary>
        static private int ClickActionTrigger = 0;

        static void Main(string[] args)
        {
            //ForDebugOnly();       // デバッグ用関数
            //return;

            try
            {
                // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
                // 起動準備　＆　引数用意
                // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

                // パラメータ設定の初期化（取得）
                Constants.Initialize();

                // ブラウザ立ち上げ
                ChromeDriverEx chrome = CreateNewChromeDriver();

                // 経済指標を取得
                //InvestingCalender InvC = new InvestingCalender(ref chrome);
                //InvC.GetInvestingCalender();

                //// LINE用
                LINEControl cLINE = new LINEControl(ref chrome);

                // ログイン ライン
                cLINE.LoginLINE();

                //// 初回ログイン時はログイン情報だけを記憶させて終了
                //if (Constants.Login_Line_First) return;

                //// highlow 本トレ
                HighLowControl cHighLow_Real = new HighLowControl(ref chrome);
                cHighLow_Real.Open(false);          // 本番OPEN
                cHighLow_Real.Login();              // 本トレログイン

                ///// highlowデモ
                HighLowControl cHighLow_Demo = new HighLowControl(ref chrome);
                cHighLow_Demo.Open(true);           // デモOPEN
                cHighLow_Demo.demoLogin();          // デモログイン

                CommonControl.SleepWait(500);

                // ユーザーによるコマンド受付　情報保持用
                string userCommandStatus = string.Empty;

                //ハイロー入札履歴
                List<BoHistroyContainer> HL_History = new List<BoHistroyContainer>();

#if DEBUG
                //ForDebugOnly(chrome);
                //return;
#endif

                // シグナルが来るまで待つか、[コマンド]終了を受け付けたら終了
                while (true)
                {
                    //CommonControl.DebugPrint("待機開始・・・");

                    // メッセージを待つ
                    string signalRoomName = WaitLineGroupsNewArraival(cLINE);

                    // なければ、終了定期処理へ
                    if (string.IsNullOrEmpty(signalRoomName))
                    {
                        CommonControl.DebugPrint("この時間来ませんでした" + DateTime.Now);
                        goto finallyProcess;        // 終了まで飛ばす
                    }

                    // 以下、シグナル受信していたら
                    // メッセージが来た時

                    // 新着対象の部屋のメッセージ群を取得する
                    ArrayList newMsgList = cLINE.getNewMsgText(signalRoomName);

                    //　メッセージがあるか？（通知に気づいたときには古いメッセージしかなくて新着メッセージないときは抜ける）
                    if (newMsgList.Count == 0)
                    {
                        // 新着に気づいたが、メッセージが古い時間のものしか無かった時
                        CommonControl.DebugPrint("気づくのが遅かった・・・部屋名：" + signalRoomName);
                        goto finallyProcess;        // 終了まで飛ばす
                    }

                    string newMsg = "";
                    if (!userCommandStatus.Equals("stop"))
                    {
                        // エントリーの情報1つのみに絞り込むか又は命令メッセージ(最初に見つかったエントリー メッセージを取得)
                        newMsg = HighLowControl.SelectSignalMsg(signalRoomName, newMsgList);
                    }
                    else
                    {
                        newMsg = (string)newMsgList[newMsgList.Count - 1];
                    }

                    if (Constants.LINE_RECEIVER_SIDE)
                    {
                        // 子機の場合は冒頭についているどの部屋から来たか情報を取り出し
                        // メッセージの冒頭の親からのヘッダパラメータをカットしてシグナル文のみにする
                        signalRoomName = HighLowControl.AnalysisReceiverSide(ref newMsg);
                    }

                    // ※Stop時以外
                    // 命令かバイナリー配信かを判断する 　
                    if (IsSignalMsg(signalRoomName, newMsg))
                    {
                        // バイナリー配信の時

                        // 親機の時のみ
                        if (Constants.LINE_RECEIVER_SIDE == false)
                        {
                            // 対象の部屋(グループ)へメッセージをコピー
                            cLINE.putSignalMsgToRoom(Constants.ROOM_NAME_BoSignal, signalRoomName, newMsg);
                        }

                        // シグナル解析
                        SignalOrder sig = HighLowControl.AnalysisWord(signalRoomName, newMsg);

                        // 指標の高い重要度の時間帯でないか\
                        // 又は、指定した時間帯
                        //if (!InvC.JudgeEnvironmentSignalZone(ref sig, ref cLINE))
                        //sig.Environment = EnvState.esTest;      // 無条件デモ
                        HighLowControl.JudgeRealDemo(ref sig);

                        // エントリー結果
                        bool entryResult = false;

                        if (sig.Environment.Equals(EnvState.esReal))
                        {
                            // リアルトレード
                            entryResult = cHighLow_Real.EntryHighLow(ref sig);
                        }
                        else
                        {
                            // デモトレード
                            entryResult = cHighLow_Demo.EntryHighLow(ref sig);
                        }

                        // 結果をラインに表示
                        if (entryResult)
                        {
                            //成功時
                            cLINE.putSignalMsgToRoom(Constants.ROOM_NAME_CmdLine, signalRoomName, "エントリーに成功。" + Constants.LINE_CRLF + sig.getEntryNoticeMsg());

                            // シグナル履歴にストック 
                            BoHistroyContainer boc = new BoHistroyContainer(HistoryState.hsWaitResult, sig);
                            HL_History.Add(boc);
                        }
                        else
                        {
                            CommonControl.DebugPrint("エントリーに失敗。" + sig.rawOriginalMessage);
                            // デバッグ用
                            //cLINE.putSignalMsgToRoom(Constants.ROOM_NAME_CmdLine, signalRoomName, "エントリーに失敗。" + sig.rawOriginalMessage);
                        }

                        // 待機に戻る
                    }
                    else
                    {
                        // バイナリー配信ではなくて、命令の時　ステータスにセット
                        if (signalRoomName.Equals(Constants.ROOM_NAME_CmdLine))     // 命令は指定のルームからのメッセージのみ受け取る
                        {

                            // 命令の抽出
                            string comResult = CommandProc(newMsg, userCommandStatus, out userCommandStatus);

                            // エラーがあった場合
                            if (comResult.Length != 0)
                            {
                                cLINE.putSignalMsgToRoom(Constants.ROOM_NAME_CmdLine, "", comResult);
                            }

                            // 命令実行 TODO:今は仮
                            if (userCommandStatus.IndexOf("end") >= 0)      // 終了時
                            {
                                cLINE.putSignalMsgToRoom(Constants.ROOM_NAME_CmdLine, signalRoomName, "プログラムを終了します。" + newMsg);
                                break;
                            }
                        }
                    }

                // １通り終了時　定期的に行う処理
                finallyProcess:

                    // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
                    // 溜まったタスクを消化
                    // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
                    System.Windows.Forms.Application.DoEvents();
                    ClickActionTrigger += 1;

                    // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
                    // 結果を見に行き、新着があればそれを部屋に出力
                    // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- 
                    cHighLow_Real.getSignalResult(ref HL_History);      // 本トレ
                    cHighLow_Demo.getSignalResult(ref HL_History);      // デモ　それぞれに見に行く
                    for (int i = 0; i < HL_History.Count; i++)
                    {
                        if (HL_History[i].State == HistoryState.hsDone) continue;   // 全て完了済みのものは飛ばす

                        bool ret = false;
                        HL_History[i].PostResultData = CommonControl.PostBoResult(HL_History[i]);               // Google送信
                        // postデータが作られていたら成功
                        HL_History[i].State = string.IsNullOrEmpty(HL_History[i].PostResultData) ? HL_History[i].State : HistoryState.hsPostDataComplete;      // 成功したかどうか

                        ret = CommonControl.OutPutTextBoResult(HL_History[i]);                                  // テキスト吐き出し
                        // テキスト掃き出しできたらすべて完了
                        HL_History[i].State = ret ? HistoryState.hsDone : HL_History[i].State;
                    }
                    // TODO: コンプリートしたBOコンテナ要素はここで削除しても良いかも

                    // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
                    // ２minに一回何らかのアクションを起こしてメッセージの受信頻度を上げる
                    // スリープ(LINE受信しなくなる)対策
                    // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----
                    if (ClickActionTrigger % 2 == 0)
                    {
                        CommonControl.ClickAction();
                        ClickActionTrigger = 0;

                        // 受信が行われるようにActiveWindowsの切り替えを行う　
                        //TODO:実験成功？ 画面のライトがONOFFしなくても受信できるかチェックする
                        SendKeys.SendWait("%{TAB}");
                        CommonControl.SleepWait(500);
                        SendKeys.SendWait("%{TAB}");

                        // ネットワークが切れていれば 再ログイン 効果未確認
                        cHighLow_Real.RetryLogin();

                    }
                    // debug用
                    //break;
                }

                // main end

                // ハイローログアウト
                //cHighLow_Demo.Logout();
                //cHighLow_Real.Logout();

                chrome.Quit();

            }
            catch (Exception e)
            {
                // 想定外エラー発生時
                // 原因調査用
                CommonControl.DebugPrint(e, "Main Exception");
            }
            finally
            {
                CommonControl.DebugPrint("プログラムを終了しました");

            }
        }

        /// <summary>
        /// コマンド命令専用メソッド
        /// ユーザからの送信メッセージを受けとりそれに合わせた命令をこなす
        /// コマンド以外から始まるものはそのまま入れる
        /// </summary>
        /// <param name="msg">ユーザコマンド</param>
        /// <param name="beforeStatus">変更前ユーザステータス</param>
        /// <param name="argUserCommandStatus"></param>
        /// <returns>エラーメッセージ(あれば)</returns>
        private static string CommandProc(string msg, string beforeStatus , out string argUserCommandStatus)
        {
            string ret = string.Empty;
            argUserCommandStatus = beforeStatus;

            try
            {
                // コマンドから始まる場合
                if (msg.IndexOf("コマンド") == 0)
                {

                    string[] tmp = msg.Split(new string[] { Constants.LINE_CRLF }, StringSplitOptions.None);

                    // 改行で上から　あるかぎり処理 既存のものと入れ替える
                    for (int i = 1; i < tmp.Length; i++)        // 1行目(i=0)はコマンド
                    {
                        if (tmp[i].Length == 0) break;

                        string[] msgSplit = tmp[i].Split(new string[] { "@" }, StringSplitOptions.None);

                        string sigName = string.Empty;
                        string dKey = string.Empty;
                        bool dValue = false;

                        switch (msgSplit[0].ToUpper())                  // シグナル名の変換
                        {
                            case "A":
                                sigName = Constants.ROOM_NAME_AUXESIS;
                                break;
                            case "S":
                                sigName = Constants.ROOM_NAME_SignalMeijin;
                                break;
                        }

                        dValue = msgSplit[2].ToUpper().Equals("R") ? true : false;

                        dKey = sigName + "@" + msgSplit[1];

                        if (Constants.rdDic.ContainsKey(dKey)) Constants.rdDic.Remove(dKey);

                        Constants.rdDic.Add(dKey, dValue);

                    }

                    ret = "書き換え 正常終了";
                }
                else if (msg.ToUpper().IndexOf("SHOWALL") == 0)
                {
                    // 出力メッセージを返す

                    string sigName = "";

                    foreach ( KeyValuePair<string, bool> kvp in Constants.rdDic)
                    {
                        string[] sigNameHead = kvp.Key.Split(new string[] { "@" }, StringSplitOptions.None);

                        if (!sigName.Equals(sigNameHead[0]))       // シグナル名が変わる時にだけ入力
                        {
                            ret += "＜" + sigNameHead[0] + "＞" + Constants.LINE_CRLF;
                            sigName = sigNameHead[0];
                        }

                        string rdv = kvp.Value ? "Real" : "Demo";                   // リアルまたはデモ
                        ret += sigNameHead[1] + "@" + rdv + Constants.LINE_CRLF;
                    }

                }
                else
                {
                    argUserCommandStatus = msg;         // コマンド命令以外はそのまま入れる
                }
            }
            catch (Exception)
            {
                // 処理できなかったら返す
                ret = "エラー 無効なコマンド命令が入力されました";
            }

            return ret;
        }


        /// <summary>
        /// 対象のメッセージがシグナルか命令かを判断する
        /// </summary>
        /// <param name="roomName">部屋名</param>
        /// <param name="msg">メッセージ</param>
        /// <returns></returns>
        private static bool IsSignalMsg(string roomName, string msg)
        {
            bool ret = false;

            switch (roomName)
            {
                case Constants.ROOM_NAME_AUXESIS:
                    // 指定文言から始まっているか？
                    ret = msg.IndexOf("[BO配信]") == 0;
                    break;
                case Constants.ROOM_NAME_RISE:                      // RISE用未作成
                    //ret = false;
                    break;
                case Constants.ROOM_NAME_CmdLine:
                    //ret = msg.IndexOf("コマンド") == 0;                 // コマンドからのみ
                    break;
                case Constants.ROOM_NAME_SignalMeijin:                  // エントリー予告が入っているもののみ
                    ret = msg.IndexOf("エントリー予告") > 0;
                    break;
                //case Constants.ROOM_NAME_CmdLine:                     // コマンドラインからきたやつも今はデバッグ中につき同じ処理
                //    ret = msg.IndexOf("[IFTTT]") == 0;
                //    break;
                case Constants.ROOM_NAME_MIYU:                          // 自動配信ヘッダだったらOK
                    ret = msg.IndexOf("[IFTTT]") == 0;
                    break;
                case Constants.ROOM_NAME_DebugRoomName:
                    ret = false;
                    break;
            }

            return ret;

        }


        /// <summary>
        /// ラインの部屋で
        /// シグナルを受け取る部屋に新着があるか、
        /// 又はコマンド部屋での新しい入力を受け付けるまで待つ
        /// </summary>
        /// <param name="Line">ラインオブジェクト</param>
        /// <returns></returns>
        private static string WaitLineGroupsNewArraival(LINEControl Line)
        {
            // ラインに切り替える
            Line.IsActivate();

            // 子機かどうか？
            if (Constants.LINE_RECEIVER_SIDE)
            {
                return WaitLineGroupsByReciever(Line);
            }
            else
            {
                // 親機の時
                return WaitLineGroupsByParent(Line);
            }

        }

        /// <summary>
        /// 親機用ライングループ　待機処理
        /// </summary>
        /// <param name="Line">ラインオブジェクト</param>
        /// <returns></returns>
        private static string WaitLineGroupsByParent(LINEControl Line)
        {
            // ライン部屋
            Line.IsActivate();

            // TODO:待機用の部屋をアクティブにして待つ（LINE新着の数字を取得するため）　
            // 今は仮でデバッグ部屋にする
            Line.RoomAvtive(Constants.ROOM_NAME_DebugRoomName);
            Line.ChatRoomListScrollTop();

            //var tasks = new List<Task>();
            string ret = string.Empty;
            string[] roomNames;

            //roomNames = new string[] {
            //                    Constants.ROOM_NAME_RISE,
            //                    Constants.ROOM_NAME_AUXESIS,
            //                    Constants.ROOM_NAME_CmdLine,
            //                    Constants.ROOM_NAME_MIYU };
            roomNames = new string[] {
                                "",
                                Constants.ROOM_NAME_AUXESIS,
                                Constants.ROOM_NAME_CmdLine,
                                Constants.ROOM_NAME_SignalMeijin };

            var task1 = Task.Run(() => Line.WaitRoomNewArrivals(roomNames[0]));
            var task2 = Task.Run(() => Line.WaitRoomNewArrivals(roomNames[1]));
            var task3 = Task.Run(() => Line.WaitRoomNewArrivals(roomNames[2]));
            var task4 = Task.Run(() => Line.WaitRoomNewArrivals(roomNames[3]));

            // 何れかに着信が来るまでまつ
            int taskNum = Task.WaitAny(task1, task2, task3, task4);

            // 帰ってきた値の部屋が本当に新着があるか確認する
            if (Line.ConfirmIsRoomsNewArrivals(roomNames[taskNum]))
            {
                ret = roomNames[taskNum];
            }
            else
            {
                ret = string.Empty;

                // 来ていない、又は来ていたが無しと判断されたとき（TODO:原因不明　要調査）
                for (int i = 0; i < roomNames.Length; i++)
                {
                    // 1つずつ見て本当になかったか確認する
                    if (Line.ConfirmIsRoomsNewArrivals(roomNames[i]))
                    {
                        // debug
                        CommonControl.DebugPrint("WaitLineGroupsByParent", "自身で見に行って発見した", roomNames[i]);

                        // 実は見つかった時
                        ret = roomNames[i];
                        break;
                    }
                }

            }

            // 対象のシグナル1の着信
            return ret;
        }

        /// <summary>
        /// 子機用ライングループ　待機処理
        /// </summary>
        /// <param name="Line"></param>
        /// <returns></returns>
        private static string WaitLineGroupsByReciever(LINEControl Line)
        {
            Line.IsActivate();
            Line.ChatRoomListScrollTop();

            //var tasks = new List<Task>();
            string ret = string.Empty;
            string[] roomNames;

            roomNames = new string[] {
                                Constants.ROOM_NAME_BoSignal};

            var task1 = Task.Run(() => Line.WaitRoomNewArrivals(roomNames[0]));

            // 着信が来るまでまつ
            int taskNum = Task.WaitAny(task1);

            // 対象のシグナル、又は入力された部屋
            return roomNames[taskNum];
        }


        /// <summary>
        /// メッセージ
        /// デバッグ用
        /// </summary>
        public static void ForDebugOnly()
        {
            Constants.Initialize();

            try
            {
                //CommonControl.OutPutTextBoResult("ABC" + Environment.NewLine + " @ " + Environment.NewLine + "D E F G H I J K ");

                string str = "";
                str = str + "";


                SignalOrder sio = HighLowControl.AnalysisWord(Constants.ROOM_NAME_MIYU, "[IFTTT] EURUSD High Chance");


                //List<string> res = new List<string>();
                //hlc.getSignalResult(ref res);
            }
            catch (Exception e)
            {
                throw e;
            }

            // メッセージ調査用
            //// 失敗する
            //string testword = "[バイナリー配信] Mon Jan 07 2019 17:04:59 エントリー,マーチン,EURUSD,5分,ロー,数値:1.144";

            ////CommonControl.DebugPrint(testword.IndexOf("[バイナリー配信]") == 0);
            //SignalOrder sig = HighLowControl.AnalysisWord(Constants.ROOM_NAME_AUXESIS, testword);



            //Console.WriteLine("今開いているハンドル：" + driver.CurrentWindowHandle);

            //foreach (String hndl in driver.WindowHandles)
            //{
            //    driver.SwitchTo().Window(hndl);
            //    System.Threading.Thread.Sleep(2000);
            //    Console.WriteLine("Title:" + driver.Title);
            //    Console.WriteLine("URL:" + driver.Url);
            //}

        }

    }
}
