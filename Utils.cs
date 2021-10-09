using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    class Utils
    {
        public static byte GetBCC(byte[] byteArray,int start, int arrayLength)
        {
            byte bcc = 0x00;
            for (int i = 0; i < arrayLength; i++)
            {
                bcc ^= byteArray[(start+i)%1024];
            }
            return bcc;
        }

        public static string ToTrendData(int channel, string date, string upData, string downData)
        {
            String[] trendData = { Convert.ToString(channel + 1), ",",date, ",", upData, ",", downData };
            return string.Concat(trendData);
        }

        public static string ToSaveLog(int channel, string date, string upData, string downData)
        {
            String[] saveLog = { "[",date,"]   ",Convert.ToString(channel + 1),"    ", upData,"    ",downData};
            return string.Concat(saveLog);
        }

        public static void Delay(int ms)
        {
            DateTime dateTimeNow = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);
            DateTime dateTimeAdd = dateTimeNow.Add(duration);
            while(dateTimeAdd >= dateTimeNow)
            {
                System.Windows.Forms.Application.DoEvents();
                dateTimeNow = DateTime.Now;
            }
            return;
        }
    }
}
