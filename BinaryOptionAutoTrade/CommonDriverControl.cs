using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;

namespace BinaryOptionAutoTrade
{
    /// <summary>
    /// ドライバ用操作
    /// 継承外の操作をする時ようの処理を書く
    /// </summary>
    class CommonDriverControl
    {
        /// <summary>
        /// ドライバを作成する
        /// </summary>
        /// <returns></returns>
        public static ChromeDriverEx CreateNewChromeDriver()
        {
            ChromeDriverEx chrome;

            try
            {
                // Chromeに入れるオプションの作成
                ChromeOptions chromeOptions = new ChromeOptions();

                // LINEセット
                chromeOptions.AddExtension(Constants.PATH_CRX_LINE);

                // プロファイルセット 2段階認証 Skip用
                chromeOptions.AddArgument("user-data-dir=" + Constants.PATH_ProfileDir);
                chromeOptions.AddArgument("--profile-directory=" + Constants.UsingProfileName);

                chrome = new ChromeDriverEx(Constants.PATH_ChromeDriver, chromeOptions);

                // エレメント待機の設定
                IWait<IWebDriver> wait = new WebDriverWait(chrome, TimeSpan.FromSeconds(30));   // 最大30秒待つ

                // ブラウザを最大化
                chrome.Manage().Window.Maximize();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw new Exception("ブラウザの起動に失敗しました。\r\n 原因" + e.Message);
            }
            finally
            {
                // Nothing
            }

            return chrome;
        }


        /// <summary>
        /// 新しいタブで対象のURLを開く
        /// </summary>
        /// <param name="chrome"></param>
        /// <param name="argURL"></param>
        /// <returns>新しく開いたURLのハンドル</returns>
        public static string OpenNewWindowTab(ChromeDriverEx chrome, string argURL)
        {
            string newWindow = string.Empty;

            try
            {
                // 新規タブを開く
                chrome.ExecuteScript("window.open()");
                //chrome.ExecuteScript("window.open(arguments[0], 'newtab')", argURL);

                // 開いたウィンドウのハンドルを取得
                newWindow = chrome.WindowHandles[chrome.WindowHandles.Count - 1];

                // 作業対象を切り替え
                chrome.SwitchTo().Window(newWindow);

                chrome.Url = argURL;

                // 指定のURLを開く
                //chrome.Url = argURL;

            }
            catch (Exception e)
            {
                newWindow = string.Empty;
                //throw;
            }

            return newWindow;
        }

        /// <summary>
        /// 【XPATH版】
        /// 対象の要素が取得できる状態になってから
        /// 安全に取得する
        /// </summary>
        /// <param name="chrome">ドライバ</param>
        /// <param name="xPath">XPATH</param>
        /// <returns></returns>
        public static IWebElement GetSafeWebElementByXPath(ChromeDriverEx chrome, string xPath)
        {
            try
            {
                IWebElement element;

                WaitElementArrivalByXPath(chrome, xPath);
                element = chrome.FindElementByXPath(xPath);

                return element;
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e);
            }

            // error
            return null;
        }

        /// <summary>
        /// 【ID版】
        /// 対象の要素が取得できる状態になってから
        /// 安全に取得する
        /// </summary>
        /// <param name="chrome">ドライバ</param>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public static IWebElement GetSafeWebElementById(ChromeDriverEx chrome, string elementId)
        {
            try
            {
                IWebElement element;

                WaitElementArrivalById(chrome, elementId);
                element = chrome.FindElementById(elementId);

                return element;
            }
            catch (Exception e)
            {
                CommonControl.DebugPrint(e);
            }

            // error
            return null;
        }

        /// <summary>
        /// ターゲット要素が出現するまで待機する
        /// </summary>
        /// <param name="chrome">対象ドライバ</param>
        /// <param name="elementId">対象要素</param>
        public static Boolean WaitElementArrivalById(ChromeDriverEx chrome, String elementId)
        {
            return WaitElementArrivalBy(chrome, By.Id(elementId));
        }

        /// <summary>
        /// ターゲット要素が出現するまで待機する
        /// XPath
        /// </summary>
        /// <param name="chrome">対象ドライバ</param>
        /// <param name="XPath">対象要素</param>
        public static Boolean WaitElementArrivalByXPath(ChromeDriverEx chrome, String XPath)
        {
            //対象のXパスの要素が現れるまで待つ
            return WaitElementArrivalBy(chrome, By.XPath(XPath));
        }

        /// <summary>
        /// 対象のエレメントが現れるまで待つ
        /// </summary>
        /// <param name="chrome">ドライバ</param>
        /// <param name="by">対象</param>
        private static Boolean WaitElementArrivalBy(ChromeDriverEx chrome, By by)
        {
            //const int waitTimeLimit = 60000;        // 待機限界
            //int waitTime = 0 ;                      // カウンタ
            bool ret = false;                       // 返り値

            try
            {
                // 旧仕様
                // 最大3分待つ
                IWait<IWebDriver> wait = new WebDriverWait(chrome, TimeSpan.FromMinutes(1));
                // 旧方式でつくる
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(by));

                //// 見つかるまでループ
                //while (!ret)
                //{
                //    ReadOnlyCollection<IWebElement> target = chrome.FindElements(by);

                //    if (target.Count > 0)
                //    {
                //        //  見つかった
                //        ret = true;
                //    }
                //    else
                //    {
                //        //  見つからなかった
                //        ret = false;
                //    }

                //    CommonControl.SleepWait(500);
                //    waitTime += 500;

                //    if (waitTime > waitTimeLimit)
                //        break;
                //}

                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 対象のID要素のテキストボックスを全選択状態にする
        /// </summary>
        /// <param name="chrome">ドライバ</param>
        /// <param name="argId">要素ID</param>
        public static void selectAllTextBox(ChromeDriverEx chrome, string argId)
        {
            // 使えない
            chrome.ExecuteScript("return document.getElementById('{0}').select()", argId);
        }

        /// <summary>
        /// 対象の要素が見える位置にまでスクロールを行う
        /// </summary>
        /// <param name="element">表示させたい要素(エレメント)</param>
        public static void ScrollTargetElementView(IWebElement element)
        {
            RemoteWebElement remote = (RemoteWebElement)element;
            var view = remote.LocationOnScreenOnceScrolledIntoView;
        }

    }
}
