using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace project
{
    public class SerialComm
    {
        private static SerialPort serialPort=null;
        private static bool isReceived = false;

        public delegate void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e);

        public static void SerialCommInit(string port)
        {
                serialPort = new SerialPort();
                serialPort.PortName = port;
                serialPort.BaudRate = 115000;
                serialPort.DataBits = (int)8;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.ReadTimeout = (int)500;
                serialPort.WriteTimeout = (int)500;
        }
        public static void UnInit()
        {
            serialPort.Dispose();
            serialPort = null;
        }
        public static void SetReceivedHandler(SerialPort_DataReceived ReceivedHandler)
        {
            if(serialPort != null)
                serialPort.DataReceived += new SerialDataReceivedEventHandler(ReceivedHandler);
        }
        public static void SetReceived()
        {
            isReceived = true;
        }

        public static bool Write(byte[] dataToSend, int cnt = 3)
        {
            serialPort.Write(dataToSend, 0, dataToSend.Length);
            Utils.Delay(1500);
            if (isReceived) {
                isReceived = false;
                return true;
            }
            else
            {
                cnt--;
                if (cnt == 0)
                    return false;
                else
                    return Write(dataToSend, cnt);
            }
        }

        public static bool WriteClassical(byte[] dataToSend)
        {
            serialPort.Write(dataToSend, 0, dataToSend.Length);
            return true;
        }
        public static bool IsInitialized()
        {
            return serialPort == null ? false : true;
        }



        public static bool IsOpened()
        {
            return serialPort.IsOpen;
        }
        public static void Open()
        {
            serialPort.Open();
        }

        public static void Close()
        {
            serialPort.Close();
        }
    }
}
