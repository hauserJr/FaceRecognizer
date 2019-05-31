using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Helper
{
    public static class ProSerizeNum
    {
        /// <summary>
        /// 產生不重複的的流水號當作個人的Key
        /// </summary>
        /// <param name="shardField"></param>
        /// <returns></returns>
        public static int GetCode(string shardField)
        {
            int code = 0;
            shardField = shardField.Trim();
            for (int i = 0; i < shardField.Length; i += 2)
            {
                code *= 16777619;
                code ^= shardField[i];
            }
            var result = code;

            return result;
        }
    }
}
