using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Services
{
    public static class DateConvert
    {
        /// <summary>
        /// 中文周日~周六
        /// </summary>
        /// <param name="date"></param>
        public static string TWDayOfWeek(DateTime date)
        {
            var tw_Num = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            var dw_Num = (int)date.DayOfWeek;
            return tw_Num[dw_Num];
        }
    }
}
