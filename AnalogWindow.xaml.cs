using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace project
{
    /// <summary>
    /// AnalogWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AnalogWindow : Window
    {
        float CurrentValueSent = 0;
        UInt16 PulseValueSent = 0;
        int CurrentSuccess = 0, CurrentFailed = 0;
        int PulseSuccess = 0, PulseFailed = 0;

        bool CurrentRunning = false;
        bool PulseRunning = false;


        float f_current_measurement;
        UInt16 i_pulse_measurement;

        SolidColorBrush logColor = (SolidColorBrush) new BrushConverter().ConvertFrom("#007ACC");
        SolidColorBrush errColor = (SolidColorBrush) new BrushConverter().ConvertFrom("#FF7E7E");

        public AnalogWindow()
        {
            InitializeComponent();
        }

        public void ReceivedAction_Current(byte[] dataBuf)
        {
            if (!CurrentRunning)
                CurrentRunning = !CurrentRunning;
            float value = ConvertUtils.byteArrayToFloat_bigEndian(dataBuf);
            TB_CURRENT_RESPONSE.Text = Convert.ToString(value);
        }

        public void ReceivedAction_Pulse(byte[] dataBuf)
        {
            if (!PulseRunning)
                PulseRunning = !PulseRunning;
            UInt32 value = ConvertUtils.ByteArrayToUInt16_bigEndian(dataBuf);
            TB_PULSE_RESPONSE.Text = Convert.ToString(value);
        }

        private void SendCurrentMeasurement(int type) //0: stop, 1: start
        {
            byte[] dataBuf = new byte[13];
            int i = 0;
            dataBuf[i++] = SerialCommands.STX;
            dataBuf[i++] = SerialCommands.ANALOG_CURRENT_MEASUREMENT_1;
            dataBuf[i++] = SerialCommands.ANALOG_CURRENT_MEASUREMENT_2;
            dataBuf[i++] = 0x00;
            dataBuf[i++] = 0x00;    //Data Length
            dataBuf[i++] = 0x05;    //Data Length
            dataBuf[i++] = (byte)type;
            if (type == 1)
            {
                if (float.TryParse(TB_CURRENT_MEASUREMENT.Text, out f_current_measurement))
                {
                    byte[] current_measurement = ConvertUtils.FloatToByteArray(f_current_measurement);
                    Array.Copy(current_measurement, 0, dataBuf, i, 4);
                    i += 4;
                }
                else
                {
                    MessageBox.Show("값을 제대로 입력해주세요");
                    return;
                }
            }
            else
            {
                dataBuf[i++] = 0x00;
                dataBuf[i++] = 0x00;
                dataBuf[i++] = 0x00;
                dataBuf[i++] = 0x00;
            }
            dataBuf[i++] = 0x03;
            dataBuf[i] = Utils.GetBCC(dataBuf,0, i);
            if (type == 1)
            {
                CurrentValueSent = f_current_measurement;
                if (!SerialComm.Write(dataBuf))
                {
                    MessageBox.Show("응답이 없습니다.");
                    return;
                }
            }
            else
                CurrentRunning = false;
            
        }

        private void SendPulseMeasurement(int type)
        {
            byte[] dataBuf = new byte[11];
            int i = 0;
            dataBuf[i++] = SerialCommands.STX;
            dataBuf[i++] = SerialCommands.ANALOG_PULSE_MEASUREMENT_1;
            dataBuf[i++] = SerialCommands.ANALOG_PULSE_MEASUREMENT_2;
            dataBuf[i++] = 0x00;
            dataBuf[i++] = 0x00;    //Data Length
            dataBuf[i++] = 0x03;    //Data Length
            dataBuf[i++] = (byte)type;
            if (type == 1)
            {
                if (UInt16.TryParse(TB_PULSE_MEASUREMENT.Text, out i_pulse_measurement))
                {
                    byte[] barray_pulse = ConvertUtils.Uint16ToByteArray(i_pulse_measurement);
                    Array.Copy(barray_pulse, 0, dataBuf, i, 2);
                    i += 2;
                }
                else
                {
                    MessageBox.Show("값을 제대로 입력해주세요");
                    return;
                }
            }
            else
            {
                dataBuf[i++] = 0x00;
                dataBuf[i++] = 0x00;
            }
            dataBuf[i++] = 0x03;
            dataBuf[i] = Utils.GetBCC(dataBuf,0, i);
            if (type == 1)
            {
                PulseValueSent = i_pulse_measurement;
                if (!SerialComm.Write(dataBuf))
                {
                    MessageBox.Show("응답이 없습니다.");
                    return;
                }
            }
            else
                PulseRunning = false;
        }
     
        private void AddSerialNo()
        {
            SerialNoItem val = new SerialNoItem();
            val.SerialNo = "USFA08000076";
            ListBox_Serial_Manager.Items.Add(val);
        }

        private void Start_Current_Measurement_Click(object sender, EventArgs e)
        {
            SendCurrentMeasurement(1);
        }

        private void Stop_Current_Measurement_Click(object sender, EventArgs e)
        {
            SendCurrentMeasurement(0);
        }

        private void Start_Pulse_Measurement_Click(object sender, EventArgs e)
        {
            SendPulseMeasurement(1);
        }

        private void Stop_Pulse_Measurement_Click(object sender, EventArgs e)
        {
            SendPulseMeasurement(0);
        }

        public void Logcat_log(string log)
        {
            TB_LOGCAT.Text = log;
            TB_LOGCAT.Background = logColor;
        }

        public void Logcat_err(string err)
        {
            TB_LOGCAT.Text = err;
            TB_LOGCAT.Background = errColor;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(CurrentRunning)
                SendCurrentMeasurement(0);
            if (PulseRunning)
                SendPulseMeasurement(0);
        }
    }
}
