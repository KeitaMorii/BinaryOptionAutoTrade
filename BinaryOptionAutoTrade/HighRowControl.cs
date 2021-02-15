using OpenQA.Selenium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryOptionAutoTrade
{

    /// <summary>
    /// シグナルのステータス状態を表す
    /// </summary>
    public enum SignalState
    {
        /// <summary>オーダー：初期値</summary>
        soInitilized = 0,
        /// <summary>オーダー：エントリー</summary>
        soEntry = 1,
        /// <summary>オーダー：待機</summary>
        soStandBy = 2,
        /// <summary>オーダー：マーチン</summary>
        soMartin = 3,
        /// <summary>オーダー：キャンセル</summary>
        soCancel = 4,
        /// <summary>オーダー：結果</summary>
        soResult = 5,
        /// <summary>オーダー：sub ptn20</summary>
        soPtn20 = 6,
        /// <summary>オーダー：sub WIN</summary>
        soWin = 7,
        /// <summary>オーダー：sub LOSE</summary>
        soLose = 8,

    }

    /// <summary>
    /// High か Low を示す
    /// </summary>
    public enum arrowState
    {
        /// <summary> ハイロー初期値 </summary>
        asUnkown = 0,

        /// <summary> ハイロー ハイ High</summary>
        asHigh = 1,

        /// <summary> ハイロー ロー Low</summary>
        asLow = 2,

    }

    /// <summary>
    /// シグナルの環境を表す
    /// </summary>
    public enum EnvState
    {
        /// <summary> 初期値 </summary>
        esUnkown = 0,

        /// <summary> 本番環境 </summary>
        esReal = 1,

        /// <summary> テスト環境 </summary>
        esTest = 2,

    }

    /// <summary>
    /// 履歴シグナルコンテナの状態を示す
    /// BoHistroyContainer用
    /// 0 → 1 → 2 → 3と遷移するのが正常動作
    /// </summary>
    public enum HistoryState
    {
        /// <summary> 何らかの予期しないエラーが起きた </summary>
        hsUnkownError = -1,

        /// <summary> シグナル結果待ち </summary>
        hsWaitResult = 0,

        ///<summary> Googleへの送信待ち </summary>
        hsWaitPost = 1,

        ///<summary> Googleへ結果送信完了 </summary>
        hsPostDataComplete = 2,

        ///<summary> 全ての処理完了 </summary>
        hsDone = 3,
    }


    /// <summary>
    /// HighLow取引１オーダー分のクラス
    /// </summary>
    public class SignalOrder
    {
        /// <summary>環境ステータス 本番 Or テスト </summary>
        public EnvState Environment = EnvState.esUnkown;

        /// <summary>受け取ったシグナルが終わったらTRUE</summary>
        public Boolean signalCompleteFlag = false;

        /// <summary>受け取ったシグナルの部屋名</summary>
        public string signalRoomName = string.Empty;

        /// <summary>メッセージがバイナリー配信か、命令コマンドかを判断する</summary>
        public Boolean msgType = false;

        /// <summary>メインシグナルステータス </summary>
        public SignalState mainState = SignalState.soInitilized;

        /// <summary>サブ　シグナルステータス</summary>
        public SignalState subState = SignalState.soInitilized;

        /// <summary>取引する銘柄</summary>
        public string tradeBrand = string.Empty;

        /// <summary>【未使用】取引する　時間単位</summary>
        public string tradeTimeSpan = string.Empty;

        /// <summary>High又はLow</summary>
        public arrowState arrow = arrowState.asUnkown;

        /// <summary>入札予定価格</summary>
        public string bidPrice = string.Empty;

        /// <summary>入札予定価格 取得元メッセージ</summary>
        public string bidPriceRawMessage = string.Empty;

        /// <summary>シグナルが配信を決めたメッセージの時刻</summary>
        public DateTime signalMsgTimeRceived;

        /// <summary>シグナルをLineで受け取った時刻</summary>
        public DateTime lineSignalTimeRceived;

        /// <summary>エントリーする時刻</summary>
        public DateTime tradeEntryTime;

        /// <summary>
        /// エントリー判定時刻　タイムフレーム（この時間が判定時間の枠でエントリーする）
        /// 時刻形式 HH:MM (00:00)
        /// </summary>
        public string tradeTimeFrame;

        /// <summary>配信された生メッセージを保存</summary>
        public string rawOriginalMessage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SignalOrder()
        {
        }

        /// <summary>
        /// 環境の文字列を返す
        /// </summary>
        /// <returns></returns>
        public string getEnviromentNames()
        {
            string ret = string.Empty;

            switch (this.Environment)                                    // 環境
            {
                case EnvState.esReal:
                    ret = "R";              // Realの「R」
                    break;
                case EnvState.esTest:
                    ret = "D";              // Demo の「D」
                    break;
            }

            return ret;
        }

        /// <summary>
        /// エントリーした事を知らせるメッセージを返す
        /// </summary>
        /// <returns></returns>
        public string getEntryNoticeMsg()
        {

            string ret = string.Empty;
            ret = "シグナル名：" + this.signalRoomName + Constants.LINE_CRLF;
            ret += "環境：" + getEnviromentNames() + Constants.LINE_CRLF;
            ret += "銘柄：" + this.tradeBrand + Constants.LINE_CRLF;
            ret += "方向：" + (this.arrow == arrowState.asLow ? "Low" : "High") + Constants.LINE_CRLF;
            ret += "エントリー時間：" + this.tradeEntryTime.ToShortTimeString() + Constants.LINE_CRLF;      // ToString("HH時MI分");
            ret += "エントリーフレーム：" + this.tradeTimeFrame + Constants.LINE_CRLF;

            return ret;
        }
    }

    /// <summary>
    /// 判定結果を格納するクラス
    /// </summary>
    public class SignalResult
    {
        /// <summary>取引した銘柄</summary>
        public string R_TradeBrand = string.Empty;

        /// <summary>入札したHigh又はLow </summary>
        public string R_Arrow = string.Empty;

        /// <summary> 判定結果 [WIN] Or [LOSE]</summary>
        public string R_Result = string.Empty;

        /// <summary>入札した価格 </summary>
        public string R_BidPrice = string.Empty;

        /// <summary>入札した時間 </summary>
        public string R_BidTime = string.Empty;

        /// <summary>入札レート</summary>
        public string R_BidRate = string.Empty;

        /// <summary> 判定時刻 </summary>
        public string R_JudgeTime;

        /// <summary> 判定レート </summary>
        public string R_JudgeRate;

        /// <summary> 判定時ペイアウト </summary>
        public string R_PayOut = string.Empty;

        /// <summary> シグナルステータス </summary>
        public string R_State = string.Empty;

        /// <summary> 生データ文字列 </summary>
        public string R_RawString = string.Empty;

        /// <summary> コンストラクタ </summary>
        public SignalResult()
        {
        }
    }

    /// <summary>
    /// シグナル情報の履歴を管理するクラス
    /// </summary>
    public class BoHistroyContainer
    {
        ///<summary> コンテナのステータス </summary>
        public HistoryState State;

        /// <summary>入力したシグナル情報</summary>
        public SignalOrder Sig;

        /// <summary>結果情報</summary>
        public SignalResult ResultSig;

        /// <summary> 送信データ文字列 </summary>
        public String PostResultData = string.Empty;

        /// <summary> 送信先URL Google スプレッドシート</summary>
        public String PostURL_SpredSheet = string.Empty;

        /// <summary>コンストラクタ</summary>
        public BoHistroyContainer()
        {
        }

        /// <summary>コンストラクタ Sig付き</summary>
        public BoHistroyContainer(HistoryState State_, SignalOrder Sig_)
        {
            this.State = State_;
            this.Sig = Sig_;

            // 当初はブック単位でシグナルを切り替える予定だったが、1つのブックで管理するように変更
            this.PostURL_SpredSheet = Constants.URL_GAS_POST_COMMON;

            // 切り替える必要が出てきたらここで変える
            //switch (this.Sig.signalRoomName)
            //{
            //    case Constants.ROOM_NAME_CmdLine:
            //    case Constants.ROOM_NAME_SignalMeijin:
            //        this.PostURL_SpredSheet = Constants.URL_GAS_POST_AUXSIS;
            //            break;
            //    case Constants.ROOM_NAME_AUXESIS:
            //        this.PostURL_SpredSheet = Constants.URL_GAS_POST_AUXSIS;
            //        break;
            //    case Constants.ROOM_NAME_DebugRoomName:
            //        this.PostURL_SpredSheet = Constants.URL_GAS_POST_AUXSIS;        // debug　
            //        break;
            //}

        }

    }


    /// <summary>
    /// ハイローオーストラリアの操作を行うクラス
    /// </summary>
    class HighLowControl
    {
        private ChromeDriverEx chrome;

        private string CHROME_HANDLE_HIGH_LOW_ME;       // 自身に切り替えるためのハンドラ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="chrome"></param>
        public HighLowControl(ref ChromeDriverEx chrome)
        {
            this.chrome = chrome;
        }

        /// <summary>
        /// ハイロー画面に切り替える
        /// </summary>
        public void IsActivate()
        {
            this.chrome.SwitchWindowHighLow(CHROME_HANDLE_HIGH_LOW_ME);
        }

        /// <summary>
        /// ハイロー画面を開く
        /// </summary>
        /// <param name="demoFlag">Trueデモ ／ False本トレ</param>
        public void Open(bool demoFlag)
        {
            if (demoFlag)
            {
                // ハイローURLを開く　デモにログイン
                CHROME_HANDLE_HIGH_LOW_ME = CommonDriverControl.OpenNewWindowTab(
                                                                    this.chrome, Constants.URL_HIGH_LOW_DEMO);
                chrome.WaitCompleteDelay(2000);
            }
            else
            {
                // 本番にログイン
                CHROME_HANDLE_HIGH_LOW_ME = CommonDriverControl.OpenNewWindowTab(
                                                                    this.chrome, Constants.URL_HIGH_LOW);
                chrome.WaitComplete(1000);

            }

        }

        /// <summary>
        /// 【正規版】本番にログインを行う
        /// </summary>
        public void Login()
        {
            try
            {
                //// ログインボタンを押す 
                IWebElement loginBtn = CommonDriverControl.GetSafeWebElementByXPath(
                                            this.chrome, "//*[@id='header']/div/div/div/div/div/span/span/a[2]");
                loginBtn.Click();

                //  ログイン後、ロードが走る場合があるのでそれを考慮
                CommonControl.SleepWait(2000);

                // ログインID
                IWebElement eleLoginId = CommonDriverControl.
                                            GetSafeWebElementById(this.chrome, "login-username");
                eleLoginId.SendKeys(Constants.Login_HL_ID);

                CommonControl.SleepWait(500);

                // ログインパスワード
                IWebElement eleLoginPassWord = CommonDriverControl.
                                            GetSafeWebElementById(this.chrome, "login-password");
                eleLoginPassWord.SendKeys(Constants.Login_HL_PassWord);

                CommonControl.SleepWait(500);

                // なんかログインボタンがうまく取れないのでエンターで代用
                eleLoginPassWord.SendKeys("\r\n");

                //// ログインボタン　押下
                //IWebElement eleLoginTriggerBtn = CommonDriverControl.
                //                                    GetSafeWebElementByXPath(this.chrome, "//*[@id='signin - popup']/div[1]/div/div[2]/div[1]/form/div/div[6]");
                //eleLoginTriggerBtn.Click();
            }
            catch (Exception e)
            {
                // 何らかの原因でログインに失敗した
                CommonControl.DebugPrint(e, "HighLow Login");
                throw;
            }
        }

        /// <summary>
        /// 【デモ版】ハイローにログインする
        /// </summary>
        public void demoLogin()
        {

            // デモログインボタン 見つかるまで
            IWebElement demoLoginLink = CommonDriverControl.GetSafeWebElementByXPath(
                                            chrome, "//*[@id='header']/div/div/div/div/div/span/span/a[1]");
            demoLoginLink.Click();
            this.chrome.WaitCompleteDelay(3000);

            // 歓迎ボタンを押す
            IWebElement welcomeBtn = CommonDriverControl.
                                        GetSafeWebElementByXPath(this.chrome, Constants.XPATH_HL_Welcome_Button);
            welcomeBtn.Click();
            CommonControl.SleepWait(500);

        }

        /// <summary>
        /// もし404エラー（接続切れ）になっていたらログインを再度行う
        /// </summary>
        public void RetryLogin()
        {
            try
            {
                // エラーになっていないかどうか確認 見つかった場合は何もしない
                this.IsActivate();
                if (this.chrome.IsFindElementById("header")) return;

                // 見つからない場合はログインしなおし　TODO:デモと本番の切り替えられるようにする
                chrome.Navigate().Refresh();
                chrome.WaitCompleteDelay(1000);
                // 歓迎ボタンを押す
                IWebElement welcomeBtn = CommonDriverControl.
                                            GetSafeWebElementByXPath(this.chrome, Constants.XPATH_HL_Welcome_Button);
                welcomeBtn.Click();
                CommonControl.SleepWait(1000);

                CommonControl.DebugPrint("リフレッシュした。");

            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "RetryLogin");
            }

        }


        /// <summary>
        /// ハイローからログアウトする
        /// </summary>
        /// <returns></returns>
        public void Logout()
        {
            try
            {
                IsActivate();

                IWebElement eleLogoutBtn = CommonDriverControl.GetSafeWebElementById(this.chrome, "Logout");
                eleLogoutBtn.Click();
            }
            catch (Exception)
            {
                // 投げない
                //throw;
            }

        }

        /// <summary>
        /// ハイローにエントリーする
        /// </summary>
        /// <param name="sig">シグナル情報　コンテナ</param>
        /// <returns>True エントリー成功 / False エントリー失敗</returns>
        public Boolean EntryHighLow(ref SignalOrder sig)
        {
            try
            {
                // シグナルのステータスがエントリーのみ実行
                if (sig.mainState != SignalState.soEntry) return false;

                // ハイローをアクティブに
                this.IsActivate();

                // 要素が押せるようにスクロールをトップに
                chrome.ExecuteScript("document.scrollingElement.scrollTop = 140");

                // 対象の取引する銘柄へ移動
                // ===== ===== ===== ===== ===== =====
                // 銘柄リストを開く
                // ===== ===== ===== ===== ===== =====
                IWebElement BrandList = this.chrome.FindElementByXPath("//*[@id='highlow-asset-filter']");
                CommonDriverControl.ScrollTargetElementView(BrandList);
                BrandList.Click();
                CommonControl.SleepWait(500);

                // ===== ===== ===== ===== ===== =====
                // 銘柄を選ぶ
                // ===== ===== ===== ===== ===== =====
                // 検索にかける
                IWebElement eleSearchBox = this.chrome.FindElementById("searchBox");
                eleSearchBox.SendKeys(sig.tradeBrand);
                CommonControl.SleepWait(200);

                // ヒットした一番目を選択（1つしかないはず）
                IWebElement eleResultList = this.chrome.FindElementByXPath("//*[@id='assetsFilteredList']/div[1]");
                eleResultList.Click();

                // ===== ===== ===== ===== ===== =====
                // 15分固定
                // ===== ===== ===== ===== ===== =====
                IWebElement timeTab = this.chrome.FindElementByXPath(
                                        "//*[@id='assetsCategoryFilterZoneRegion']/div/div[2]");
                CommonDriverControl.ScrollTargetElementView(timeTab);
                timeTab.Click();
                CommonControl.SleepWait(500);

                // ===== ===== ===== ===== ===== =====
                // エントリーする判定時刻 フレーム時間のタブを見つける
                // ===== ===== ===== ===== ===== =====
                IReadOnlyCollection<IWebElement> frameTimeDigits =
                                        this.chrome.FindElementsByXPath("//*[@class='time-digits']");

                // 最大3つある　フレームタイムの時間を見に行き、同じ時間を探す
                bool findFrameFlag = false;
                for (int i = 0; i < frameTimeDigits.Count(); i++)
                {
                    //シグナル時間の取得
                    string chkFrameTime = frameTimeDigits.ElementAt(i).GetAttribute("textContent");

                    // 次のシグナルの時間か？
                    if (sig.tradeTimeFrame.Equals(chkFrameTime))
                    {
                        // 表示
                        frameTimeDigits.ElementAt(i).Click();
                        findFrameFlag = true;       // 見つかった
                        break;                      // 1つしかない
                    }
                }

                // 見つからなかったとき
                if (findFrameFlag == false)
                {
                    // エントリー失敗
                    CommonControl.DebugPrint("entryHighLow", "対象のタイムフレーム時間が見つからなかった為、エントリー出来ませんでした");
                    CommonControl.DebugPrint("探そうとしたタイムフレーム" + sig.tradeTimeFrame);
                    return false;
                }

                CommonControl.SleepWait(500);

                // ===== ===== ===== ===== ===== =====
                // エントリー金額を入力する
                // ===== ===== ===== ===== ===== =====
                // TODO:実際は総資金の10分の１入札する　この仕組みは後に作る　今は単純に10分の１でエントリーする
                // 10分１が20万を超えた場合は以下のようにする
                // 例：２２万円の場合、11万*2回エントリーする
                // 30万円の場合、15万円*2回エントリーする

                //  残高 
                // タイムフレームを押した後、読み込み中である可能性があるので取得可能になるまで待つ（GetSafeWebElementById）
                IWebElement eleBalance = CommonDriverControl.GetSafeWebElementById(chrome, Constants.HTML_ID_HL_Balance);
                int balance = int.Parse(eleBalance.GetAttribute("textContent")
                                                            .Replace(@"¥", "").Replace(",", ""));

                //  入力
                IWebElement eleAmount = chrome.FindElementById(Constants.HTML_ID_HL_Amount);
                // TODO:たまに失敗してる？　要調査 念のため2回おこなう
                CommonDriverControl.ScrollTargetElementView(eleAmount);                         // 見えていないとクリアできない
                CommonDriverControl.ScrollTargetElementView(eleAmount);                         // 見えていないとクリアできない
                eleAmount.Clear();                                                              // 既存入力額をクリア
                CommonControl.SleepWait(200);

                int amount = balance / 2000;                // 資産の　20分の1で
                amount = amount * 100;                      // 100の倍数にする

                if (amount < 1000) amount = 1000;       // 1000円未満になった場合は1000円
                if (amount > 200000) amount = 200000;    // 20万を超える場合は20万　（20万以上はエントリーできないため)

                eleAmount.SendKeys(amount.ToString());

                // ===== ===== ===== ===== ===== =====
                // High Or Low 入力
                // ===== ===== ===== ===== ===== =====
                // 要素が押せるようにスクロールトップの調整
                chrome.ExecuteScript("document.scrollingElement.scrollTop = 275");
                IWebElement btn;
                switch (sig.arrow)
                {
                    case arrowState.asLow:
                        btn = this.chrome.FindElementByXPath(Constants.XPATH_HL_Low_Button);
                        break;
                    case arrowState.asHigh:
                        btn = this.chrome.FindElementByXPath(Constants.XPATH_HL_High_Button);
                        break;
                    default:
                        CommonControl.DebugPrint("ハイ、又はローが分かりませんでした。");
                        return false;
                }
                btn.Click();
                CommonControl.SleepWait(200);

                // ===== ===== ===== ===== ===== =====
                // エントリー時間になるまで待つ
                // ===== ===== ===== ===== ===== =====
                IWebElement rTime = this.chrome.FindElementByXPath(Constants.XPATH_HL_Remaining_Time);
                string[] timeTxt = rTime.GetAttribute("textContent").Split(new string[] { ":" }, StringSplitOptions.None);

                // 後どれだけ待てば良いかを求める 5分前まで
                TimeSpan tsReman = new TimeSpan(0, int.Parse(timeTxt[0]), int.Parse(timeTxt[1])) - new TimeSpan(0, 5, 0);

                while (true)
                {
                    // 残り時刻が5分切るまで待機
                    if (tsReman > new TimeSpan(0, 0, 0))
                    {
                        CommonControl.SleepWait((int)tsReman.TotalMilliseconds);
                    }
                    break;
                }

                // 成功するまで　エントリーを行う
                int tryTime = 0;
                bool entryFlag = false;
                while (entryFlag == false)
                {
                    IWebElement entryBtn = this.chrome.FindElementById("invest_now_button");
                    entryBtn.Click();
                    CommonControl.SleepWait(1250);

                    IWebElement eleNotice = this.chrome.FindElementById(Constants.HTML_ID_HL_NoticeMsg);
                    if (eleNotice.Text.Equals("成功"))
                    {
                        //エントリーに成功した
                        entryFlag = true;
                    }
                    tryTime += 1;
                    if (tryTime > 2) break;     // 3回目失敗したら強制終了
                }
#if debug
                if (tryTime == 3) CommonControl.DebugPrint("3回エントリー失敗した");
#endif
                //  処理自体のエントリー結果を返す
                return entryFlag;
            }
            catch (Exception e)
            {
                // エントリーに失敗した
                CommonControl.DebugPrint(e, "EntryHighLow エントリーに失敗しました");
                //throw;// 投げない
                return false;

            }
        }


        /// <summary>
        /// 入札結果を取得し、終了のものが有れば、
        /// エントリーしたシグナル情報と照らし合わせて
        /// どのシグナルでエントリーしたものかをチェックして
        /// 格納する
        /// </summary>
        /// <returns></returns>
        public void getSignalResult(ref List<BoHistroyContainer> boHistroyContainers)
        {
            try
            {
                IsActivate();       // ハイロー画面へ

                // リストを取得
                IReadOnlyCollection<IWebElement> resultList = this.chrome.FindElementsByXPath("//*[@id='tradeActionsTableBody']/tr");

                for (int i = 0; i < resultList.Count(); i++)
                {
                    // 取引終了の結果があった場合
                    if (resultList.ElementAt(i).Text.Contains("取引終了"))
                    {
                        // 比較先　型を取る
                        SignalResult resSig = CommonControl.ConvBoResult(resultList.ElementAt(i).Text);

                        // すべての履歴コンテナと比較
                        for (int n = 0; n < boHistroyContainers.Count; n++)
                        {
                            // 比較元 入力シグナルコンテナ
                            BoHistroyContainer bhc = boHistroyContainers.ElementAt(n);

                            if (bhc.State != HistoryState.hsWaitResult) continue;                       // シグナル結果待ちのもののみ比較対象とする

                            // 入札銘柄と時間が同じなら そのシグナルで入札した結果とし、googleに送信する
                            if (resSig.R_TradeBrand.Equals(bhc.Sig.tradeBrand)
                                && DateTime.Parse(resSig.R_JudgeTime).Equals(DateTime.Parse(bhc.Sig.tradeTimeFrame)))
                            {
                                boHistroyContainers.ElementAt(n).ResultSig = resSig;
                                boHistroyContainers.ElementAt(n).State = HistoryState.hsWaitPost;       // → Google送信待ち
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e);
            }
        }


        /// <summary>
        /// 受け手側(子機)専用の解析とパラメータ設定
        /// </summary>
        /// <param name="argSignalFlatWord">解析するシグナル文章</param>
        /// <returns></returns>
        public static string AnalysisReceiverSide(ref string argSignalFlatWord)
        {
            // 2個以上の場合、　今は1個しかないので決め打ち
            // 親機は子機に受け渡したい値があればパラメータ=値＠パラメータ=値＠・・・＠（シグナル本文）と渡す
            // TODO: 2個以上必要になったら作り直す

            string[] res = argSignalFlatWord.Split(new string[] { "＠" }, StringSplitOptions.None);

            argSignalFlatWord = res[1];

            return res[0];

        }

        /// <summary>
        /// 受信したメッセージの中からエントリー情報だけ抽出する
        /// </summary>
        /// <param name="roomName">受信したルーム名</param>
        /// <param name="ary">メッセージ群</param>
        /// <returns></returns>
        public static string SelectSignalMsg(string roomName, ArrayList ary)
        {
            bool ret = false;
            string chkMsg = string.Empty;

            for (int i = 0; i < ary.Count; i++)
            {
                chkMsg = (string)ary[i];
                switch (roomName)
                {
                    case Constants.ROOM_NAME_AUXESIS:       // AUXESIS用
                        if (chkMsg.Contains("待機")) ret = true;                  // AUXSISは待機で入れる
                        break;
                    case Constants.ROOM_NAME_CmdLine:       // コマンドライン
                    case Constants.ROOM_NAME_SignalMeijin:
                        //  エントリー予告の文字があるかどうかだけ見る
                        if (chkMsg.Contains("エントリー予告")) ret = true;
                        break;
                    case Constants.ROOM_NAME_DebugRoomName: //  デバッグ用
                    case Constants.ROOM_NAME_RISE:          // RISE用未作成
                        // TODO:未定　メッセージ来たら作る
                        break;
                }
                if (ret) break;
            }

            if (ret) return chkMsg;

            // なかったら最後のメッセージだけ返す 命令メッセージ時を考慮
            return (string)ary[ary.Count - 1];
        }

        /// <summary>
        /// デモかリアルかを判断する
        /// </summary>
        /// <param name="sio">判断するシグナル</param>
        public static void JudgeRealDemo(ref SignalOrder sio)
        {
            try
            {
                // デフォルト値
                sio.Environment = EnvState.esTest;

                // 判定時間で判断する　判定に使用する時間
                int Jtime = sio.tradeEntryTime.Hour;

                // Keyから取得
                Constants.rdDic.TryGetValue(sio.signalRoomName + "@" + Jtime, out bool timeKey);

                sio.Environment = timeKey ? EnvState.esReal : EnvState.esTest;                  // True Real / False Demo

            }
            catch (Exception)
            {
                sio.Environment = EnvState.esTest;
            }

        }


        /// <summary>
        /// 平文のシグナル文章を各パラメータに解析する
        /// </summary>
        /// <param name="roomName">受けた部屋名</param>
        /// <param name="argSignalFlatWord">何れかのシグナル文章</param>
        /// <returns></returns>
        public static SignalOrder AnalysisWord(string roomName, string argSignalFlatWord)
        {
            SignalOrder ret = new SignalOrder();

            try
            {
                switch (roomName)
                {
                    case Constants.ROOM_NAME_AUXESIS:       // AUXESIS用
                        ret = AnalysisByAuxesisMsg(argSignalFlatWord);
                        break;
                    case Constants.ROOM_NAME_RISE:          // RISE用未作成
                                                            //ret = new SignalOrder();
                        break;
                    case Constants.ROOM_NAME_CmdLine:
                    case Constants.ROOM_NAME_SignalMeijin:
                        ret = AnalysisBySignalMeijin(argSignalFlatWord);
                        break;

                    case Constants.ROOM_NAME_MIYU:
                        ret = AnalysisByMiyuMsg(argSignalFlatWord);
                        break;

                    case Constants.ROOM_NAME_DebugRoomName:
                        //debug用
                        ret = AnalysisByAuxesisMsg(argSignalFlatWord);
                        break;
                }
            }
            catch (Exception e)
            {
                // シグナル解析に失敗した
                // throwしない
                CommonControl.DebugPrint(e);
            }
            finally
            {
                // 終わった後に格納
                ret.signalRoomName = roomName;                  // 部屋名
                ret.rawOriginalMessage = argSignalFlatWord;     // メッセージ原文
            }

            return ret;
        }

        /// <summary>
        /// シグナル名人のシグナルを解析する
        /// </summary>
        /// <param name="argSignalWord"></param>
        /// <returns></returns>
        private static SignalOrder AnalysisBySignalMeijin(string argSignalWord)
        {
            SignalOrder Sio = new SignalOrder();
            Sio.rawOriginalMessage = argSignalWord;

            string wTime;

            try
            {
                string[] tmp = argSignalWord.Split(new string[] { Constants.LINE_CRLF }, StringSplitOptions.None);

                for (int i = 0; i < tmp.Length; i++)
                {
                    switch (i)
                    {
                        case 2: // 銘柄
                            char[] tmpBrand = tmp[i].Trim().ToCharArray();
                            for (int n = 0; n < tmpBrand.Length; n++)
                            {
                                Sio.tradeBrand += tmpBrand[n];
                                if (n == 2) Sio.tradeBrand += "/";
                            }
                            break;
                        case 3: // 取引時間
                            wTime = tmp[i].Split(new string[] { " " }, StringSplitOptions.None)[1];
                            Sio.tradeEntryTime = DateTime.Parse(wTime);
                            if (wTime.Equals("0時0分")) Sio.tradeEntryTime.AddDays(1);  // 0時0分の時の入札は明日
                            break;

                        case 4: // 判定時間
                            wTime = tmp[i].Split(new string[] { " " }, StringSplitOptions.None)[1];
                            DateTime dt = DateTime.Parse(wTime);
                            wTime = dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00");
                            Sio.tradeTimeFrame = wTime;
                            break;
                        case 5: // ハイロー
                            Sio.arrow = tmp[i].Trim().Equals("Low") ? arrowState.asLow : arrowState.asHigh;
                            break;
                        case 6: // 指標値
                            Sio.bidPriceRawMessage = tmp[i];
                            Sio.bidPrice = tmp[i].Split(new string[] { "　" }, StringSplitOptions.None)[0];
                            break;
                        case 7:
                            if (tmp[i].Equals("エントリー準備です")) Sio.mainState = SignalState.soEntry;       // エントリー行う
                            else Sio.mainState = SignalState.soInitilized;                                      // それ以外は無視
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "想定外の形式メッセージが来た");
            }

            return Sio;
        }


        /// <summary>
        /// MIYU 部屋用
        /// 
        /// メッセージ例：[IFTTT] EURUSD Low Chance ・・・
        /// </summary>
        /// <param name="argSignalWord">解析する文章</param>
        /// <returns></returns>
        private static SignalOrder AnalysisByMiyuMsg(string argSignalWord)
        {

            SignalOrder Sio = new SignalOrder();

            try
            {
                string[] tmp = argSignalWord.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                string[] wword = tmp[0].Split(new string[] { " " }, StringSplitOptions.None);

                // 6文字の場合は間にスラッシュ
                if (wword[1].Trim().Length == 6)
                {
                    char[] tmpBrand = wword[1].Trim().ToCharArray();
                    for (int n = 0; n < tmpBrand.Length; n++)
                    {
                        Sio.tradeBrand += tmpBrand[n];
                        if (2 == n) Sio.tradeBrand += "/";
                    }
                }

                // ハイかローを決める
                Sio.arrow = wword[2].Trim().Equals("High") ? arrowState.asHigh : arrowState.asLow;

                // 現在時間で次のエントリー時間を求める
                Sio.signalMsgTimeRceived = DateTime.Now;
                getNextEntryTime(Sio);

                //無条件 エントリーを指定
                Sio.mainState = SignalState.soEntry;

            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "想定外のメッセージ形式");
                Sio.mainState = SignalState.soInitilized;
                //throw;
            }

            return Sio;
        }


        /// <summary>
        /// アクシス部屋の文章を解析する
        /// 以下は１マーチンメッセージ例 上から下へ受信する
        /// [バイナリー配信] Mon Jan 07 2019 16:59:00 待機,,EURUSD,5分,ロー,数値:1.144
        /// [バイナリー配信] Mon Jan 07 2019 16:59:53 エントリー,ptn20,EURUSD,5分,ロー,数値:1.144
        /// [バイナリー配信] Mon Jan 07 2019 17:04:36 待機,マーチン,EURUSD,5分,ロー,数値:1.144
        /// [バイナリー配信] Mon Jan 07 2019 17:04:59 エントリー,マーチン,EURUSD,5分,ロー,数値:1.144
        /// [バイナリー配信] Mon Jan 07 2019 17:10:00 ProPirot_WIN,EURUSD,5分,ロー,マーチン,CloseValue[1]:1.1435700
        /// </summary>
        /// <param name="argWord">メッセージ平文</param>
        /// <returns></returns>
        private static SignalOrder AnalysisByAuxesisMsg(string argWord)
        {

            SignalOrder Sio = new SignalOrder();

            try
            {
                // 受け取った時間
                Sio.lineSignalTimeRceived = DateTime.Now;

                string[] signalArray = argWord.Split(',');
                string[] tmp;

                for (int i = 0; i < signalArray.Count(); i++)
                {
                    switch (i)
                    {
                        case 0:
                            //  TODO:区切り文字を何にするか　今後の課題
                            string[] rep = { " 2020 " };
                            tmp = signalArray[i].Split(rep, StringSplitOptions.None);

                            // Hawkの前　シグナルを受けた時間 とステータス
                            String[] splitTimeAndState = tmp[1].Split(new string[] { " " }, StringSplitOptions.None);

                            // 時間だけでよい 2019/03/07 受けた時間に修正する
                            Sio.signalMsgTimeRceived = DateTime.Now; // DateTime.Parse(splitTimeAndState[0]);

                            // エントリー時間関係の情報を計算する
                            getNextEntryTime(Sio);

                            // ステータス
                            switch (splitTimeAndState[1].Trim())
                            {
                                case "キャンセル":
                                    Sio.mainState = SignalState.soCancel;
                                    break;
                                case "待機":
                                    Sio.mainState = SignalState.soEntry;        // 待機をエントリーとして扱う
                                    break;
                                case "エントリー":
                                    Sio.mainState = SignalState.soCancel;        // エントリーメッセージが来た時点では既に遅いのでキャンセルにする
                                    break;
                                case "WIN":
                                    Sio.mainState = SignalState.soResult;
                                    Sio.subState = SignalState.soWin;
                                    break;
                                case "LOSE":
                                    Sio.mainState = SignalState.soResult;
                                    Sio.subState = SignalState.soLose;
                                    break;
                            }
                            break;

                        // 1番目以外
                        default:
                            // ptn20
                            if (signalArray[i].Equals("ptn20"))
                            {
                                Sio.subState = SignalState.soPtn20;
                            }
                            // マーチン
                            else if (signalArray[i].Equals("マーチン"))
                            {
                                Sio.subState = SignalState.soMartin;
                            }
                            else
                            {
                                // 銘柄
                                if (string.IsNullOrEmpty(Sio.tradeBrand))
                                {
                                    // 6文字の場合は間にスラッシュ
                                    if (signalArray[i].Trim().Length == 6)
                                    {
                                        char[] tmpBrand = signalArray[i].Trim().ToCharArray();
                                        for (int n = 0; n < tmpBrand.Length; n++)
                                        {
                                            Sio.tradeBrand += tmpBrand[n];
                                            if (n == 2) Sio.tradeBrand += "/";
                                        }
                                    }
                                    else
                                    {
                                        // それ以外
                                        Sio.tradeBrand = signalArray[i].Trim();
                                    }
                                }
                                // 入札時間
                                else if (string.IsNullOrEmpty(Sio.tradeTimeSpan))
                                {
                                    Sio.tradeTimeSpan = signalArray[i].Trim();
                                }
                                // ハイロー
                                else if (Sio.arrow == arrowState.asUnkown)
                                {
                                    Sio.arrow = signalArray[i].Trim().Equals("ハイ") ? arrowState.asHigh : arrowState.asLow;
                                }
                                // 入札数値
                                else if (string.IsNullOrEmpty(Sio.bidPrice))
                                {
                                    // 数値の文字が入ってたら取得する
                                    if (signalArray[i].Contains("数値"))
                                    {
                                        try
                                        {
                                            Sio.bidPrice = signalArray[i].Split(':')[1];
                                        }
                                        catch (Exception)
                                        {
                                            // 無視する
                                            //throw;
                                        }
                                    }
                                }
                            }

                            break;
                    }

                }

                Sio.signalCompleteFlag = true;
            }
            catch (Exception ex)
            {
                CommonControl.DebugPrint(ex, "想定外のメッセージ配列を受信しました。\nメッセージ：" + argWord);
                throw;
            }

            return Sio;
        }


        /// <summary>
        /// 次にエントリーする時間を求める
        /// </summary>
        /// <param name="argSio">シグナル情報</param>
        /// <returns></returns>
        private static void getNextEntryTime(SignalOrder argSio)
        {
            try
            {

                // エントリー時間
                int entryHour = 0;          // 時
                int entryMinute = 0;        // 分

                // シグナルメッセージ時刻
                DateTime sTime = argSio.signalMsgTimeRceived;

                // 次の5分刻みの時刻をもとめる
                entryHour = sTime.Hour;
                entryMinute = sTime.Minute;
                entryMinute = entryMinute + 5 - (entryMinute % 5);

                // 次の時間またいでいるとき Hour繰り上げ
                if (entryMinute >= 60)
                {
                    entryHour = entryHour + 1;
                    entryMinute = entryMinute % 60;
                }

                // エントリー時間　TODO:Parseがミスする？
                argSio.tradeEntryTime =
                    DateTime.Parse(entryHour.ToString() + ":" +
                                    entryMinute.ToString() + ":00");

                // タイムフレーム時間 エントリー時間の5分後
                DateTime fTime = argSio.tradeEntryTime.AddMinutes(5);
                argSio.tradeTimeFrame = fTime.Hour.ToString("00") + ":" + fTime.Minute.ToString("00");

            }
            catch (Exception e)
            {
                // 
                CommonControl.DebugPrint(e, "次のエントリー時間を求めるのに失敗した" + argSio.lineSignalTimeRceived);
                throw;
            }
        }

        /// <summary>
        /// 入札結果をライン用に整える
        /// </summary>
        /// <param name="msg">結果メッセージ</param>
        /// <returns></returns>
        public static string PrepareResultMessage(string msg)
        {
            string ret = string.Empty;

            try
            {
                String[] tmp = msg.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                String[] tmpSub = tmp[2].Split(new string[] { " " }, StringSplitOptions.None);

                // 勝ち負け判定
                string result = "";
                try
                {
                    // ２文字（\0）以上？
                    result = tmpSub[5].Length > 2 ? "[WIN]" : "[LOSE]";
                }
                catch
                {
                    result = "[UNKOWN]";
                }

                ret = "【入札結果】" + result;
                ret += " 通貨ペア：" + tmp[0];
                ret += " / 入札レート：" + tmp[1];
                if (tmp.Count() > 2)
                {
                    if (tmpSub.Count() > 0) ret += " / 入札時間：" + tmpSub[0];
                    if (tmpSub.Count() > 1) ret += " / 判定時間：" + tmpSub[1];
                    if (tmpSub.Count() > 2) ret += " / ステータス：" + tmpSub[2];
                    if (tmpSub.Count() > 3) ret += " / 判定レート：" + tmpSub[3];
                    if (tmpSub.Count() > 4) ret += " / 入札金額：" + tmpSub[4];
                    if (tmpSub.Count() > 5) ret += " / ペイアウト：" + tmpSub[5];
                }
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "結果メッセージの出力に失敗 メッセージ" + msg);
            }

            return ret;
        }

    }
}
