using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryOptionAutoTrade
{
    static class Constants
    {
        #region "固定定数"

        ///<summary>LINE(拡張機能)実行URL</summary>
        public const string URL_LINE_Index = "chrome-extension://ophjlpahpchlmihnnnihgmmeilfjmjjc/index.html";

#if DEBUG
        /// <summary>slnのフォルダ場所</summary>
        public const string PATH_Base_Project = @"C:\Users\pspst\source\boat\BinaryOptionAutoTrade\";
#else
        /// <summary>プログラム実行フォルダの場所</summary>
        public static string PATH_Base_Project;
#endif
        /// <summary>BinaryOption履歴のテキストファイル場所</summary>
        public const string PATH_BO_HISTORY_TEXT = @"C:\Users\pspst\Desktop\BO履歴";

        /// <summary>デモトレードURL</summary>
        public const string URL_HIGH_LOW_DEMO = "https://demotrade.highlow.com/";

        /// <summary>本番 ハイロー URL</summary>
        public const string URL_HIGH_LOW = "https://trade.highlow.com/";

        /// <summary> Googleスプレッドシートに送る用URL MEMO:末尾の？つけるのを忘れない</summary>
        //public const string URL_GAS_POST_MIYU = "https://script.google.com/macros/s/AKfycbyE8dXFLd69C-EvU-a1dVv46jF6S5iYdEJZ8bygOZLrmi8UYw97/exec?";
        //public const string URL_GAS_POST_AUXSIS = "https://script.google.com/macros/s/AKfycby0hFZpK7moSwmY2YKc-4ht6sZcWPqWWc7hQpMpTChaiimeEgo/exec?";
        //public const string URL_GAS_POST_COMMON = "https://script.google.com/macros/s/AKfycbw0fAEhoOIIHM4VnYg05kp9REU9TIEacSy2tQUsAA/exec?";
        public const string URL_GAS_POST_COMMON = "https://script.google.com/macros/s/AKfycbw-D5vR1EbPFPhWZiZDC_81VJtKjEjH9Mxzw05T6UoBvW0qJX30/exec?";

        /// <summary> 経済指標URL </summary>
        public const string URL_INVESTING = "https://jp.investing.com/economic-calendar/";

        /// <summary> LINE プログラム内部専用改行マーク </summary>
        public const string LINE_CRLF = "＜＠CRLF＠＞";

        #endregion

        #region "INIファイルからの 設定あり定数"

        ///<summary>ChromeDriverパス</summary>
        public static string PATH_ChromeDriver;

        ///<summary>Line拡張機能パス</summary>
        public static string PATH_CRX_LINE;

        ///<summary>Profileディレクトリパス</summary>
        public static string PATH_ProfileDir;

        ///<summary>使用するProfile名</summary>
        public static string UsingProfileName;

        /// <summary>ラインにログインする一番最初かどうか</summary>
        public static bool Login_Line_First;

        /// <summary>ログインに使用するアドレス</summary>
        public static string Login_Line_Address;

        /// <summary>ログインに使用するパスワード</summary>
        public static string Login_Line_PassWord;

        /// <summary>ハイロー　ログインに使用するID</summary>
        public static string Login_HL_ID;

        /// <summary>ハイロー　ログインに使用するパスワード</summary>
        public static string Login_HL_PassWord;

        /// <summary>親機からラインによる受信を受ける側かどうか？（子機か？）
        /// TRUE 子機 ／ FALSE 親機
        /// </summary>
        public static bool LINE_RECEIVER_SIDE;

        ///// <summary> 時間帯毎の デモか本番かを 判断するスイッチ ON OFF </summary>
        //public static List<bool> JUDGE_TIME ;

        #endregion

        #region "XPATH関係"

        /// <summary>LINE 左側にある　部屋名の要素</summary>
        public const string XPATH_LINE_ROOM = "//*[@id='_chat_list_body']/li[@title='{0}']";

        /// <summary>LINE 新着数を表示する要素</summary>
        public const string XPATH_LINE_NewArrivals = "//*[@id='_chat_list_body']/li[contains(@title,'{0}')]/div/div[4]/div[2]";

        /// <summary>LINE チャット メッセージ要素 を取得する(他人のメッセージのみ)</summary>
        public const string XPATH_LINE_ChatRoomMsgsOther = "//*[@id='_chat_room_msg_list']/div[@class='MdRGT07Cont mdRGT07Other']";

        /// <summary>LINE チャット 最新の メッセージ要素 を取得する(他人のメッセージのみ)</summary>
        public const string XPATH_LINE_ChatRoomMsgsOtherLatest = "//*[@id='_chat_room_msg_list']/div[@class='MdRGT07Cont mdRGT07Other'][last()]";

        /// <summary>LINE チャット 一覧取得用 メッセージ要素を取得する(自分のメッセージ)</summary>
        public const string XPATH_LINE_ChatRoomMsgsOwn = "//*[@id='_chat_room_msg_list']/div[@class='MdRGT07Cont mdRGT07Own']";

        /// <summary>LINE チャット 最新の メッセージ要素を取得する(自分のメッセージ)</summary>
        public const string XPATH_LINE_ChatRoomMsgsOwnLatest = "//*[@id='_chat_room_msg_list']/div[@class='MdRGT07Cont mdRGT07Own'][last()]";


        /// <summary>HighLowのデモ時に最初に現れるようこそボタン</summary>
        public const string XPATH_HL_Welcome_Button = "//*[@id='account-balance']/div[2]/div/div[1]/a";

        /// <summary>HighLow ブランドを選択する</summary>
        public const string XPATH_HL_BrandSelect = "//*[@id='filterCategoryBox']//li[@data-asset='{0}']";

        /// <summary>HighLow Highボタン</summary>
        public const string XPATH_HL_High_Button = "//*[@id='up_button']";

        /// <summary>HighLow Highボタン</summary>
        public const string XPATH_HL_Low_Button = "//*[@id='down_button']";

        /// <summary>HighLow 残り時刻の表示領域</summary>
        public const string XPATH_HL_Remaining_Time = "//*[@id='timeRemaining']/span";

        #endregion

        #region "HTML ID"

        /// <summary>LINE チャット のリストDIV要素ID</summary>
        public const string HTML_ID_LINE_ChatRoomDivId = "_chat_list_scroll";

        /// <summary>HighLow 残高のID</summary>
        public const string HTML_ID_HL_Balance = "balance";

        /// <summary>HighLow 投資額　入力テキストボックス</summary>
        public const string HTML_ID_HL_Amount = "amount";

        /// <summary>HighLow 注意メッセージ</summary>
        public const string HTML_ID_HL_NoticeMsg = "notification_text";

        #endregion

        #region "入力を受け付ける部屋名"

        /// <summary>[LINE部屋名] RISE BOトレードグループ極</summary>
        public const string ROOM_NAME_RISE = "RISE BOトレードグループ極";

        /// <summary>[LINE部屋名] AUXESIS</summary>
        public const string ROOM_NAME_AUXESIS = "AUXESIS";

        /// <summary>[LINE部屋名] CmdLine</summary>
        public const string ROOM_NAME_CmdLine = "CmdLine";

        /// <summary>[LINE部屋名] </summary>
        public const string ROOM_NAME_SignalMeijin = "無料シグナル名人公式シグナルの独り言";

        /// <summary>[LINE部屋名] デバッグ用部屋名</summary>
        public const string ROOM_NAME_DebugRoomName = "リニアス";

        /// <summary>[LINE部屋名] MIYU 用部屋名</summary>
        //public const string ROOM_NAME_MIYU = "Miyu signal";
        public const string ROOM_NAME_MIYU = "Bainaly signal";

        /// <summary>[LINE部屋名] 親機が配信する先のグループ
        /// 子機が受けるグループ
        /// </summary>
        public const string ROOM_NAME_BoSignal = "BOシグナル";

        #endregion

        #region "グローバル変数"

        /// <summary>[ハンドル] ライン</summary>
        public static string CHROME_HANDLE_LINE = string.Empty;

        ///// <summary>[ハンドル] ハイロー 操作対象ハンドラ？</summary>
        //public static string CHROME_HANDLE_HIGH_LOW = string.Empty;           // 複数インスタンスなのでグローバル化は無理

        /// <summary>[ハンドル] ハイロー 本番環境 Production</summary>
        public static string CHROME_HANDLE_HIGH_LOW_PRDCT = string.Empty;

        /// <summary>[ハンドル] ハイロー デモ環境 DEMO </summary>
        public static string CHROME_HANDLE_HIGH_LOW_DEMO = string.Empty;

        /// <summary>本番／デモを切り替える判断をする情報を持つ辞書　シグナル名＠時間(HH)</summary>
        public static Dictionary<string, bool> rdDic = new Dictionary<string, bool>();

        #endregion

        /// <summary>
        /// 初期化
        /// テキストファイルから設定を読み込む
        /// </summary>
        public static void Initialize()
        {
#if DEBUG
            // 宣言時に表記
#else
            PATH_Base_Project = Environment.CurrentDirectory + "\\";
#endif
            string parameterTextPath = PATH_Base_Project + "parameters.ini";

            PATH_CRX_LINE = PATH_Base_Project + "packages\\Extensions\\LINE_kaku\\2.1.5_0.crx";
            PATH_ProfileDir = PATH_Base_Project + "packages\\profile";
            PATH_ChromeDriver = PATH_Base_Project + "driver\\chromedriver_win32";

            UsingProfileName = "BinaryOptionProfile1";

            // テキストから読み込む パラメータ
            ArrayList textAry = TextControl.GetTextRowArray(parameterTextPath);

            foreach (string rTxt in textAry)
            {
                // =が含まれているもののみ
                // パラメータ設定値の有効行として見なす
                if (rTxt.Contains("="))
                {
                    string[] parames = rTxt.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    string paraName = parames[0];
                    string paraVal = parames[1];

                    switch (paraName)
                    {
                        case "LINE_LOGIN_FIRST":
                            Login_Line_First = bool.Parse(paraVal);
                            break;
                        case "LINE_LOGIN_ADDRESS":
                            Login_Line_Address = paraVal;
                            break;
                        case "LINE_LOGIN_PASSWORD":
                            Login_Line_PassWord = paraVal;
                            break;
                        case "LINE_RECEIVER_SIDE":
                            LINE_RECEIVER_SIDE = bool.Parse(paraVal);
                            break;
                        case "HIGHLOW_LOGIN_ID":
                            Login_HL_ID = paraVal;
                            break;
                        case "HIGHLOW_LOGIN_PASSWORD":
                            Login_HL_PassWord = paraVal;
                            break;
                    }
                }
            }

            // シグナル切り替え情報を保持
            string[] sigAry = new string[2] { ROOM_NAME_AUXESIS, ROOM_NAME_SignalMeijin };
            foreach (string sName in sigAry)
            {
                textAry = TextControl.GetTextRowArray(PATH_Base_Project + "\\Signal\\" + sName + ".txt");

                foreach (string rdInfo in textAry)
                {
                    string[] parames = rdInfo.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    string paraTime = parames[0];
                    string paraVal = parames[1];

                    bool ret = false;
                    bool.TryParse(paraVal, out ret);

                    rdDic.Add(sName + "@" + paraTime, ret);            // Key:シグナル名@時間HH , Value: Real True / Demo False

                }

            }
        }
    }
}
