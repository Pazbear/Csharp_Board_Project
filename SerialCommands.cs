using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    static class SerialCommands
    {
        //Commands
        public const byte STX = 0x02;
        public const byte ETX = 0x03;

        public const byte BOARD_CONNECT_1 = 0X42;
        public const byte BOARD_CONNECT_2 = 0X43;

        public const byte READ_CONFIG_1 = 0x42; //B
        public const byte READ_CONFIG_2 = 0x52; //R

        public const byte WRITE_CONFIG_1 = 0x42; //B
        public const byte WRITE_CONFIG_2 = 0x57; //W

        public const byte START_MEASUREMENT_1 = 0X4D; //M
        public const byte START_MEASUREMENT_2 = 0X53; //S

        public const byte STOP_MEASUREMENT_1 = 0X53;
        public const byte STOP_MEASUREMENT_2 = 0X54;


        public const byte ANALOG_CURRENT_MEASUREMENT_1 = 0X43;  //C
        public const byte ANALOG_CURRENT_MEASUREMENT_2 = 0X54;  //T

        public const byte ANALOG_PULSE_MEASUREMENT_1 = 0X50;    //P
        public const byte ANALOG_PULSE_MEASUREMENT_2 = 0X54;    //T
    }
}
