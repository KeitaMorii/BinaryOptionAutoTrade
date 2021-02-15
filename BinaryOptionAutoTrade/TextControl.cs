using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryOptionAutoTrade
{
    /// <summary>
    /// テキスト操作専用クラス
    /// </summary>
    class TextControl
    {
        public static ArrayList GetTextRowArray(string textPath)
        {
            string line = "";
            ArrayList al = new ArrayList();

            using (StreamReader sr = new StreamReader(
                textPath, Encoding.GetEncoding("Shift_JIS")))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    al.Add(line);
                }
            }
            //for (int i = 0; i < al.Count; i++)
            //{
            //    Console.WriteLine(al[i]);
            //}

            return al;
        }
    }
}
