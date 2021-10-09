using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
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
    /// BoardConfigurationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BoardConfigurationWindow : Window
    {
        private ChConfigControl[] chConfigControls = new ChConfigControl[5];
        byte channelSent = 0X00;


        SolidColorBrush logColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#007ACC");
        SolidColorBrush errColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF7E7E");

        public BoardConfigurationWindow()
        {
            InitializeComponent();

            DataBindingInit();
        }
        public void ReceivedAction(int ChNo, Object data)
        {
            ConfigData configData = (ConfigData)data;
            Dispatcher.Invoke(new Action(delegate
            {
                chConfigControls[ChNo - 1].TB_FREQUENCY.Text =
                                    Convert.ToString(ConvertUtils.ByteArrayToUInt32_bigEndian(configData.FREQUENCY));
                chConfigControls[ChNo - 1].TB_PGA.Text = Convert.ToString(configData.PGA);
                chConfigControls[ChNo - 1].TB_THLEVEL.Text = Convert.ToString(configData.TH_LEVEL);
                chConfigControls[ChNo - 1].TB_STOP.Text = Convert.ToString(configData.STOP);
                chConfigControls[ChNo - 1].TB_BURST.Text = Convert.ToString(configData.BURST);
                chConfigControls[ChNo - 1].TB_LNA.Text = Convert.ToString(configData.LNA);
            }));
        }
        private void ReadConfigurationData(byte ChNo)
        {
            Logcat_log("BOARD CONFIGURATION 읽기");
            string str = string.Empty;
            byte[] dataBuf = new byte[9];
            int i = 0;
            dataBuf[i++] = SerialCommands.STX;
            dataBuf[i++] = SerialCommands.READ_CONFIG_1;
            dataBuf[i++] = SerialCommands.READ_CONFIG_2;
            dataBuf[i++] = ChNo;
            dataBuf[i++] = 0x00;
            dataBuf[i++] = 0x01;
            dataBuf[i++] = 0x00;
            dataBuf[i++] = SerialCommands.ETX;
            dataBuf[i++] = Utils.GetBCC(dataBuf,0, i);
            if (!SerialComm.WriteClassical(dataBuf))
            {
                Logcat_err("응답이 없습니다.");
            }
        }

        private void WriteConfigurationData(byte ChNo)
        {
            //Data
            ConfigData configData = new ConfigData();
            configData.FREQUENCY = new byte[4];
            UInt32 f_frequency;
            if (UInt32.TryParse(chConfigControls[(int)ChNo - 1].TB_FREQUENCY.Text, out f_frequency))
            {
                byte[] byteArray = ConvertUtils.Uint32ToByteArray(f_frequency);
                Array.Clear(configData.FREQUENCY, 0, 4);
                Array.Copy(byteArray, 0, configData.FREQUENCY, 0, 4);
                configData.PGA = (byte)Convert.ToInt32(chConfigControls[(int)ChNo - 1].TB_PGA.Text);
                configData.TH_LEVEL = (byte)Convert.ToInt32(chConfigControls[(int)ChNo - 1].TB_THLEVEL.Text);
                configData.STOP = (byte)Convert.ToInt32(chConfigControls[(int)ChNo - 1].TB_STOP.Text);
                configData.BURST = (byte)Convert.ToInt32(chConfigControls[(int)ChNo - 1].TB_BURST.Text);
                configData.LNA = (byte)Convert.ToInt32(chConfigControls[(int)ChNo - 1].TB_LNA.Text);
            }
            else
            {
                MessageBox.Show("Frequency 값의 형식이 올바르지 않습니다.");
            }

            byte[] dataBuf = new byte[17];
            int i = 0;
            dataBuf[i++] = SerialCommands.STX;
            dataBuf[i++] = SerialCommands.WRITE_CONFIG_1;
            dataBuf[i++] = SerialCommands.WRITE_CONFIG_2;
            dataBuf[i++] = ChNo;
            channelSent = ChNo;
            dataBuf[i++] = 0x00;
            dataBuf[i++] = 0x09;
            Array.Copy(configData.FREQUENCY, 0, dataBuf, i, 4);
            i += 4;
            dataBuf[i++] = configData.PGA;
            dataBuf[i++] = configData.TH_LEVEL;
            dataBuf[i++] = configData.STOP;
            dataBuf[i++] = configData.BURST;
            dataBuf[i++] = configData.LNA;
            dataBuf[i++] = SerialCommands.ETX;
            dataBuf[i] = Utils.GetBCC(dataBuf,0, i);
            SerialComm.WriteClassical(dataBuf);
        }

        private void DataBindingInit()
        {
            chConfigControls[0] = CH1Config;
            chConfigControls[1] = CH2Config;
            chConfigControls[2] = CH3Config;
            chConfigControls[3] = CH4Config;
            chConfigControls[4] = CH5Config;

            for (int i = 0; i < 5; i++)
            {
                chConfigControls[i].ChannelNo = BitConverter.GetBytes(i + 1)[0];
                chConfigControls[i].BtnReadConfigClick += new BtnReadConfigClickHandler(ReadConfigurationData);
                chConfigControls[i].BtnWriteConfigClick += new BtnWriteConfigClickHandler(WriteConfigurationData);
            }
        }

        private void LoadConfigurationData()
        {
            for(int i=1; i<=5; i++)
            {
                ReadConfigurationData((byte)i);
                Utils.Delay(500);
            }
        }

        private void CH1Config_Loaded(object sender, RoutedEventArgs e)
        {

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfigurationData();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
