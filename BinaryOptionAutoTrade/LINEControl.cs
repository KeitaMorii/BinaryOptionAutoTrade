using OpenQA.Selenium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ライン画面を操作する　メソッド群
namespace BinaryOptionAutoTrade
{
    /// <summary>
    /// LINEブラウザ操作用クラス
    /// </summary>
    class LINEControl
    {
        /// <summary>操作対象ドライバ</summary>
        private ChromeDriverEx chrome;

        ///// <summary>新着を受けたルーム名</summary>
        //public string NewArraivalRoomName
        //{
        //    get { return NewArraivalRoomName; }
        //    private set { NewArraivalRoomName = value; }
        //}

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="chromeDriver"></param>
        public LINEControl(ref ChromeDriverEx chromeDriver)
        {
            this.chrome = chromeDriver;
        }

        /// <summary>
        /// Lineのウインドウに切り替える
        /// </summary>
        public void IsActivate()
        {
            this.chrome.SwitchWindowLine();
        }

        /// <summary>
        /// defaultのラインログイン
        /// </summary>
        public void LoginLINE()
        {
            LoginLINE(Constants.Login_Line_Address,
                        Constants.Login_Line_PassWord);
        }

        /// <summary>
        /// ラインにログインする
        /// </summary>
        public void LoginLINE(String address, String passWord)
        {
            try
            {
                this.chrome.Url = Constants.URL_LINE_Index;

                // TODO:空白ぺージを完全と誤って判断している？ 読み込み待ち用
                chrome.WaitCompleteDelay(1000);

                // ラインのハンドルをキープ
                Constants.CHROME_HANDLE_LINE = chrome.CurrentWindowHandle;

                // アドレス 一回入力すれば、プロファイルから省略可
                //IWebElement eleAdrs = this.chrome.FindElementById("line_login_email");
                //eleAdrs.SendKeys(address);

                // パス
                IWebElement eleLoginPwd = CommonDriverControl.GetSafeWebElementById(this.chrome, "line_login_pwd");
                eleLoginPwd.SendKeys(passWord);

                Thread.Sleep(500);

                // ログイン
                IWebElement eleLoginBtn = CommonDriverControl.GetSafeWebElementById(this.chrome, "login_btn");
                eleLoginBtn.Click();

                // 部屋名が現れるまで待つ
                CommonControl.SleepWait(1000);
                CommonDriverControl.WaitElementArrivalById(this.chrome, Constants.HTML_ID_LINE_ChatRoomDivId);

                // TODO:LINEの読み込み処理を完全に待つ方法がわからない
                this.chrome.WaitCompleteDelay(2000);

                // 部屋リストを最後までスクロールして部屋名リストを全部読みこむ
                ChatRoomListScrollEnd();
                CommonControl.SleepWait(300);
                ChatRoomListScrollTop();

                CommonControl.SleepWait(500);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        /// <summary>
        /// 対象の部屋名に新着が本当に来ているか確認する
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns>True 来てる / False 来てない</returns>
        public Boolean ConfirmIsRoomsNewArrivals(string roomName)
        {
            try
            {
                // 新着数が0より上
                return getRoomNewArrivalsMsgNumber(roomName) > 0;
            }
            catch (Exception)
            {
                // 来てない
                return false;
                //throw;
            }
        }

        /// <summary>
        /// 新着メッセージの数を取得する
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        public int getRoomNewArrivalsMsgNumber(string roomName)
        {
            // 対象のルームの新着番号　XPATH
            string targetRoomXPATH = string.Format(Constants.XPATH_LINE_NewArrivals, roomName);
            IWebElement ele = null;

            try
            {
                IsActivate();

                // 要素が見つかったら
                if (chrome.FindElementsByXPath(targetRoomXPATH).Count > 0)
                {
                    ele = chrome.FindElementByXPath(targetRoomXPATH);

                    // 新着数 取得
                    if (string.IsNullOrEmpty(ele.Text.Trim(' ')))
                    {
                        return 0;
                    }
                    else
                    {
                        return int.Parse(ele.Text.Trim(' '));
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                if (ele != null)
                {
                    CommonControl.DebugPrint(e, ele.Text);      // 原因究明用
                }
                else
                {
                    CommonControl.DebugPrint(e);
                }
                return 0;
            }
        }

        /// <summary>
        /// 対象のルームの新着が来るのを待つ<br></br>
        /// 新着が来たらその部屋名を取得
        /// </summary>
        /// <param name="roomName">部屋名</param>
        /// <returns></returns>
        public Boolean IsRoomsNewArrivals(string roomName)
        {
            string targetRoomXPATH = string.Format(Constants.XPATH_LINE_NewArrivals, roomName);

            CommonDriverControl.WaitElementArrivalByXPath(this.chrome, targetRoomXPATH);

            return true;
        }

        /// <summary>
        /// 対象のルームの新着が来るのを待つ
        /// 新着が来たらその部屋名を返す
        /// </summary>
        /// <param name="roomName">部屋名</param>
        /// <returns></returns>
        public string WaitRoomNewArrivals(string roomName)
        {
            string targetRoomXPATH = string.Format(Constants.XPATH_LINE_NewArrivals, roomName);

            Boolean ret = CommonDriverControl.WaitElementArrivalByXPath(this.chrome, targetRoomXPATH);

            if (ret)
            {
                return roomName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 対象の部屋名の要素取得
        /// </summary>
        /// <param name="roomName">部屋名</param>
        /// <returns></returns>
        private IWebElement FindElementRoomTitleLiByXPath(string roomName)
        {
            try
            {
                IWebElement element = this.chrome.FindElementByXPath(string.Format(Constants.XPATH_LINE_ROOM, roomName));
                return element;

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 対象のルームをアクティブにする
        /// </summary>
        /// <param name="roomName">部屋名</param>
        public void RoomAvtive(string roomName)
        {
            try
            {
                bool successFlag = false;
                int tryTimes = 0;           // 試行回数

                while (successFlag == false)
                {
                    // 部屋を画面内に
                    IWebElement eleRoom = FindElementRoomTitleLiByXPath(roomName);
                    CommonDriverControl.ScrollTargetElementView(eleRoom);

                    // 部屋をクリックする
                    eleRoom.Click();
                    this.chrome.WaitCompleteDelay(500);

                    // 本当にクリック成功したか？
                    IWebElement eleRoomHeader = this.chrome.FindElementByXPath("//*[@id='_chat_header_area']/div/div[2]/h1");
                    if (eleRoomHeader.Text.Equals(roomName))
                    {
                        successFlag = true;
                    }
                    else
                    {
                        tryTimes += 1;
                    }

                    if (tryTimes >= 3)
                    {
                        throw new Exception("LINEの対象の部屋をアクティブにする既定の試行回数をこえました");
                    }
                }

            }
            catch (NoSuchElementException)
            {
                CommonControl.DebugPrint("対象の部屋をアクティブにしようとして失敗しました。　対象の部屋：" + roomName);
                throw;
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e);
                //throw e;
            }
        }

        /// <summary>
        /// 部屋リストをトップにスクロールする
        /// </summary>
        /// <returns></returns>
        public Boolean ChatRoomListScrollTop()
        {
            try
            {
                // スクロール
                this.chrome.ExecuteScript(string.Format("document.getElementById('{0}').scrollTop = 0;", Constants.HTML_ID_LINE_ChatRoomDivId));
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "ChatRoomListScrollEnd");
                throw e;
            }

            return true;
        }

        /// <summary>
        /// 部屋リストを最後までスクロールする
        /// </summary>
        /// <returns></returns>
        public Boolean ChatRoomListScrollEnd()
        {
            try
            {
                // スクロール
                this.chrome.ExecuteScript(string.Format("document.getElementById('{0}').scrollTop = document.getElementById('{0}').scrollHeight;", Constants.HTML_ID_LINE_ChatRoomDivId));
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "ChatRoomListScrollEnd");
                throw e;
            }

            return true;
        }




        /// <summary>
        /// 対象の部屋のメッセージの最新テキストを取得
        /// </summary>
        /// <param name="roomName">部屋名</param>
        /// <returns></returns>
        public ArrayList getNewMsgText(string roomName)
        {
            ArrayList rceMsgs = new ArrayList();

            IReadOnlyCollection<IWebElement> MsgElements;

            try
            {

                // LINE タブをアクティブにする
                this.chrome.SwitchWindowLine();

                //  アクティブ前に新着メッセージ数を取得
                int msgNumbder = getRoomNewArrivalsMsgNumber(roomName);

                // 取得する部屋をアクティブに
                RoomAvtive(roomName);

                // メッセージルームの一番最後のをメッセージを取得
                MsgElements = chrome.FindElementsByXPath(Constants.XPATH_LINE_ChatRoomMsgsOther);

                // メッセージ受信の起点となる時間
                DateTime RceivedTime = DateTime.Now;

                // 受信から3分以内のメッセージを取得
                // 最後から見ていく
                for (int i = MsgElements.Count - 1; (MsgElements.Count - 1) - msgNumbder < i; i--)
                {
                    // メッセージリスト
                    IWebElement msgEleBlock = MsgElements.ElementAt(i);

                    // 受信メッセージ　
                    string tmpMsg = msgEleBlock.Text;
                    // 改行で区切ってリスト化する
                    string[] tmpAry = tmpMsg.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    // メッセージの受信時間 　（配列の一番末尾が時間）
                    DateTime msgTime = DateTime.Parse(tmpAry[tmpAry.Length - 1]);

                    if (msgTime > RceivedTime.AddMinutes(-3))
                    {
                        // 受信してから3分以内 又は 指定した数まで取得
                        // メッセージ要素を取得
                        //IWebElement msgElement = msgEleBlock.FindElement(By.XPath("//span[@class='mdRGT07MsgTextInner']"));
                        //rceMsgs.Add(msgElement.Text);

                        // デバッグモード　受信したラインメッセージをテキスト出力
                        //System.IO.File.AppendAllText(Constants.PATH_BO_HISTORY_TEXT + @"\" + DateTime.Now.ToString("yyyyMMdd") + "LineMessage.txt", roomName + Environment.NewLine);
                        //System.IO.File.AppendAllText(Constants.PATH_BO_HISTORY_TEXT + @"\" + DateTime.Now.ToString("yyyyMMdd") + "LineMessage.txt", msgEleBlock.Text + Environment.NewLine);
                        //System.IO.File.AppendAllText(Constants.PATH_BO_HISTORY_TEXT + @"\" + DateTime.Now.ToString("yyyyMMdd") + "LineMessage.txt", " ===== ===== ===== ===== ===== ===== ===== ===== ===== =====" + Environment.NewLine);

                        string rawMsg = string.Empty;       // 格納するワーク用文字列

                        // もし、部屋がLine@だった場合はメッセージ送信者が最初の文字列に入らないので格納する必要がある
                        switch (roomName)
                        {
                            case Constants.ROOM_NAME_SignalMeijin:
                                // Line@だったので飛ばしたメッセージ配列[0]を入れる(Line@の部屋での1行目)
                                rawMsg = rawMsg + tmpAry[0] + Constants.LINE_CRLF;
                                break;
                            default:
                                // Line@以外の部屋は配列[0]は送信者の名前なので飛ばす
                                break;
                        }

                        // 最初と最後以外を連結して格納
                        // 改行でSplitをした最初の配列にはメッセージ送信者の名前が入っているのでFor文は1から入れる（Line@以外)
                        for (int n = 1; n < tmpAry.Count() - 1; n++)    
                        {
                            rawMsg = rawMsg + tmpAry[n] + Constants.LINE_CRLF;
                        }

                        //System.IO.File.AppendAllText(Constants.PATH_BO_HISTORY_TEXT + @"\" + DateTime.Now.ToString("yyyyMMdd") + "LineMessage.txt", rawMsg.Replace(Constants.LINE_CRLF, "/CRLF/") + Environment.NewLine);
                        //System.IO.File.AppendAllText(Constants.PATH_BO_HISTORY_TEXT + @"\" + DateTime.Now.ToString("yyyyMMdd") + "LineMessage.txt", "------------------------------------------------------" + Environment.NewLine);

                        rceMsgs.Add(rawMsg);

                    }
                    else
                    {
                        break;        // 3分以上前なら抜ける
                    }
                }

                return rceMsgs;
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "getSignalOrder");
                return rceMsgs;
                //throw e;f
            }

        }

        /// <summary>
        /// 【シグナルメッセージ専用】
        /// 対象の部屋にメッセージを書き込む
        /// </summary>
        /// <param name="toWriteRoomName">書き込む対象の部屋名</param>
        /// <param name="signalRoomName">実際に来たシグナルの部屋</param>
        /// <param name="argMsg">メッセージ</param>
        public void putSignalMsgToRoom(string toWriteRoomName, string signalRoomName, string argMsg)
        {
            try
            {
                // LINEをアクティブに
                this.chrome.SwitchWindowLine();

                // 対象の部屋をアクティブに
                RoomAvtive(toWriteRoomName);

                // メッセージを書き込む
                IWebElement msgInputTxtBox = this.chrome.FindElementById("_chat_room_input");
                msgInputTxtBox.Click();                 // フォーカスあてる代わり

                //CommonControl.SleepWait(2000);
                msgInputTxtBox.SendKeys(signalRoomName);
                System.Windows.Forms.SendKeys.SendWait("+~");               // Shift + Enter

                string[] aryMsg = argMsg.Split(new string[] { Constants.LINE_CRLF }, StringSplitOptions.None);
                for (int i = 0; i < aryMsg.Length; i++)
                {
                    msgInputTxtBox.SendKeys(aryMsg[i]);
                    System.Windows.Forms.SendKeys.SendWait("+~");           // Shift + Enter
                }

                CommonControl.SleepWait(500);
                msgInputTxtBox.SendKeys(Keys.Enter);                        // 送信

            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "メッセージの送信に失敗しました");
            }
        }

    }
}
