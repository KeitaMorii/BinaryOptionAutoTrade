using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BinaryOptionAutoTrade
{
    /// <summary>
    /// Chromeブラウザを拡張操作する
    /// </summary>
    class ChromeDriverEx : ChromeDriver
    {

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChromeDriverEx(string chromeDriverDirectory, ChromeOptions chromeOptions) : base(chromeDriverDirectory, chromeOptions)
        {
        }

        /// <summary>
        /// ライン画面に切り替える
        /// </summary>
        public void SwitchWindowLine()
        {
            while (true)
            {
                base.SwitchTo().Window(Constants.CHROME_HANDLE_LINE);
                WaitCompleteDelay(500);
                if (base.Url.Equals(Constants.URL_LINE_Index)) break;   // ちゃんと切り替えられているかどうかのチェック
            }
        }

        /// <summary>
        /// ハイロー画面に切り替える
        /// </summary>
        /// <param name="Handler">切り替えるハンドル</param>
        public void SwitchWindowHighLow(string Handler)
        {
            while (true)
            {
                base.SwitchTo().Window(Handler);
                WaitCompleteDelay(500);
                if (base.Url.Equals(Constants.URL_HIGH_LOW)) break;         // ちゃんと切り替えられているかどうかのチェック
                if (base.Url.Equals(Constants.URL_HIGH_LOW_DEMO)) break;
            }
        }

        /// <summary>
        /// 完全読み込み後、指定した時間待つ
        /// </summary>
        /// <param name="afterWaitMilliSeconds">ページが完全になった後に待つ時間(ミリ秒)</param>
        /// <returns></returns>
        public Boolean WaitCompleteDelay(int afterWaitMilliSeconds = 500)
        {
            Boolean ret = false;
            ret = WaitComplete();

            if (ret)
            {
                Thread.Sleep(afterWaitMilliSeconds);
            }

            return ret;
        }

        /// <summary>
        /// ページが完全になるまで待つ
        /// 指定なし10分
        /// </summary>
        /// <param name="waitMilliSeconds">待つ最大ミリ秒数</param>
        /// <returns>True:OK False:指定秒数までに待てなかった Exception:想定外の例外</returns>
        public Boolean WaitComplete(int waitMilliSeconds = 600000)
        {
            Boolean ret = false;
            DateTime endTime = DateTime.Now.AddMilliseconds(waitMilliSeconds);

            try
            {
                // 時間が過ぎるか、ステータスがTRUE
                while (!ret)
                {
                    string state = (string)base.ExecuteScript("return document.readyState");
                    ret = state.Equals("complete");

                    if (!(DateTime.Now > endTime)) throw new TimeoutException();
                };
            }
            catch (TimeoutException e)
            {
                TimeoutException IgnoreException = e;
                //CommonControl.DebugPrint(e, "ページが完全に読み込めませんでした");
            }
            catch (Exception e)
            {
                //タイムアウト以外の例外
                CommonControl.DebugPrint(e, "想定外の例外");
                //throw e;
            }

            return ret;
        }

        /// <summary>
        /// 対象のIDがあるか調査する ID
        /// </summary>
        /// <param name="id">調査対象 ID 名</param>
        /// <returns>True 見つかった / False 見つからなかった</returns>
        public bool IsFindElementById(string id)
        {
            return IsFindElementBy(By.Id(id));
        }

        /// <summary>
        /// 対象の要素があるか調査する BY
        /// </summary>
        /// <param name="by">調査対象 Byオブジェクト</param>
        /// <returns>True 見つかった / False 見つからなかった</returns>
        public bool IsFindElementBy(By by)
        {
            bool ret = false;

            try
            {
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> ele = base.FindElements(by);

                //  結果を返す
                ret = (ele.Count > 0);
            }
            catch (Exception)
            {
                ret = false;
                //throw;
            }

            return ret;
        }

    }
}
