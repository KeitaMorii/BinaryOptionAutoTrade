using OpenQA.Selenium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BinaryOptionAutoTrade
{

    /// <summary>
    /// 経済指標の情報をもつクラス
    /// </summary>
    public class invCalender
    {
        /// <summary>
        /// イベント時間
        /// </summary>
        public DateTime evTime;

        /// <summary>
        /// 銘柄
        /// </summary>
        public string brand = string.Empty;

        /// <summary>
        /// 重要度
        /// </summary>
        public int importance = 0;

        /// <summary>
        /// 指標内容
        /// </summary>
        public string detail = string.Empty;

    }

    /// <summary>
    /// 経済指標についての処理を行うクラス
    /// </summary>
    class InvestingCalender
    {
        // 操作するドライバ
        private ChromeDriverEx chrome;
        /// <summary>経済指標の重要度の回避リスト</summary>
        private List<invCalender> EconomicList;

        public InvestingCalender(ref ChromeDriverEx chrome)
        {
            this.chrome = chrome;
        }

        /// <summary>
        /// 経済指標のイベントを取得する
        /// </summary>
        /// <returns></returns>
        public void GetInvestingCalender()
        {
            EconomicList = new List<invCalender>();

            try
            {
                // 経済指標にアクセス
                while (true)
                {
                    this.chrome.Url = Constants.URL_INVESTING;
                    this.chrome.WaitCompleteDelay(1000);
                    if (this.chrome.Url.Equals(Constants.URL_INVESTING)) break;
                }

                //指標一覧リスト　の要素取得
                IReadOnlyCollection<IWebElement> Events;
                Events = this.chrome.FindElementsByXPath("//tr[contains(@id, 'eventRowId_')]");

                foreach (IWebElement ele in Events)
                {
                    //重要度のチェック
                    IWebElement impTextEle = this.chrome.FindElementByXPath("//*[@id='" + ele.GetAttribute("id") + "']/td[3]");
                    if (impTextEle.GetAttribute("title").Equals("高い重要性"))
                    {
                        try
                        {
                            // 重要度が高い場合イベントとして記録
                            invCalender inv = new invCalender();
                            string[] eveList = ele.Text.Split(new string[] { " " }, StringSplitOptions.None);

                            inv.importance = 3;
                            inv.evTime = DateTime.Parse(eveList[0]);                // 時間
                            inv.brand = eveList[3];                                 // イベント銘柄
                            inv.detail = eveList[4] + eveList[5] + eveList[6];      // 内容

                            EconomicList.Add(inv);
                        }
                        catch (Exception e)
                        {
                            // エラーが出ても継続する
                            CommonControl.DebugPrint(e, "指標の型変換に失敗");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e, "経済指標の情報取得に失敗した");
            }


            // debug
            //invCalender invs = new invCalender
            //{
            //    importance = 3,
            //    evTime = DateTime.Now,                // 時間
            //    brand = "USD",                                 // イベント銘柄
            //    detail = "テスト指標"      // 内容
            //};
            //EconomicList.Add(invs);

        }

        /// <summary>
        /// エントリーする時間が高い重要度の時間帯でないかどうかチェックする
        /// 重要度前後30分以内であればスルーする
        /// </summary>
        /// <returns>True 高い重要度 / False それ以外の重要度または重要でない</returns>
        public bool JudgeEnvironmentSignalZone(ref SignalOrder sig, ref LINEControl cLine)
        {
            bool ret = false;

            //// 指定時間帯の時トレードを通す
            //DateTime dudgeTime = sig.tradeEntryTime.AddMinutes(5);

            //// 指定の時間帯であれば取引を行う
            //if ( DateTime.Parse("11:00") <= dudgeTime && dudgeTime < DateTime.Parse("12:00") )
            //{
            //    ret = false;
            //} else
            //{
            //    ret = true;
            //}

            // TODO:今は無条件でテスト
            sig.Environment = EnvState.esTest;

            return ret;

            //TODO:必要になったらコメントを外す

            // IsCheckImportanceTimeZone
            //try
            //{
            //    foreach (invCalender inv in EconomicList)
            //    {
            //        DateTime sTime = inv.evTime.AddMinutes(-30);
            //        DateTime eTime = inv.evTime.AddMinutes(30);

            //        // 間に入っているか？
            //        if (sTime <= sig.tradeEntryTime && sig.tradeEntryTime <= eTime)
            //        {
            //            ret = true;
            //            // 回避になった場合メッセージを送る
            //            cLine.putSignalMsgToRoom(Constants.ROOM_NAME_CmdLine
            //                                        , sig.signalRoomName
            //                                        , "次の経済指標に引っかかった為エントリーしませんでした。"
            //                                            + "経済指標 【 "+ inv.evTime.ToString("HH:mm:ss") + " " + inv.detail + " 】 " 
            //                                            + "シグナル【" + sig.rawOriginalMessage + "】" );

            //            break;              // 一つでも見つかったら終了
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    CommonControl.DebugPrint(e, "経済指標　時間比較失敗");
            //}

            //return ret;
        }
    }


}