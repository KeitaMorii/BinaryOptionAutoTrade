using using OpenQA.Selenium;
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

    }


    public class investingCalender
    {
        /// <summary>
        /// クロームドライバ
        /// </summary>
        private ChromeDriverEx chrome;

        public investingCalender(ref ChromeDriverEx chromeDriver )
        {
            

        }
    }


}