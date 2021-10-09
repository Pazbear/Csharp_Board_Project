using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    ///구조체데이터
    struct ConfigData
    {
        public byte[] FREQUENCY;
        public byte PGA;
        public byte TH_LEVEL;
        public byte STOP;
        public byte BURST;
        public byte LNA;
    }
    struct SystemVoltageData
    {
        public byte[] TDTV; //3.3V
        public byte[] FV;   //5V
        public byte[] FHV;  //400V
    }
    struct ChannelUpDownData
    {
        public byte CHANNEL;
        public byte UPDOWN;
        public byte[] UPDOWNVALUE;
    }
    struct TrendLog
    {
        public int CHNO;
        public int YEAR;
        public int MONTH;
        public int DAY;
        public int HOUR;
        public int MINUTE;
        public int SECOND;
        public double UPVALUE;
        public double DOWNVALUE;
    }
    static class Globals
    {
        public const int TASK_ABLE_T = 30;
        public const int ArrayLengthByMeasureTime = 5000;
        public const int QUEUEBUFSIZE = 1024;

        public const string LogsPath = "C:\\Program Files\\BoardApp\\Logs";
    }
}
