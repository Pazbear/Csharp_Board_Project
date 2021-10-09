using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace project
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort sp;
        int recvSize;

        QueueBuff queueBuff;
        int QUEUEBUFSIZE = 1024;
        byte[] buff;
        Thread ThreadSP;
        private bool IsThreadStop = false;

        byte[] dataLen;


        SerialCommConnectWindow SCCW = null;
        MeasurementWindow MW=null;
        AnalogWindow AW = null;
        TrendWindow TW = null;
        BoardConfigurationWindow BCW = null;

        private int PortConnected = 0;
        private int BoardConnected = 0;

        int i = 0;
        #region 시리얼포트 변수

        #endregion 시리얼포트 변수

        public MainWindow()
        {
            InitializeComponent();
            int Measurement_Result_Passed = int.Parse(AppConfiguration.GetAppConfig("Measurement_Result_Passed"));
            int Measurement_Result_Failed = int.Parse(AppConfiguration.GetAppConfig("Measurement_Result_Failed"));
            int Analog_Result_Passed = int.Parse(AppConfiguration.GetAppConfig("Analog_Result_Passed"));
            int Analog_Result_Failed = int.Parse(AppConfiguration.GetAppConfig("Analog_Result_Failed"));
            if (AppConfiguration.GetAppConfig("LogIdxDate") != DateTime.Now.ToString("yyyyMMdd"))
                AppConfiguration.SetAppConfig("LogIdx", "0");
            BoardConnected = int.Parse(AppConfiguration.GetAppConfig("Connected"));

            TB_Measurement_Result_Total.Text = Convert.ToString(Measurement_Result_Passed + Measurement_Result_Failed);
            TB_Measurement_Result_Passed.Text = Convert.ToString(Measurement_Result_Passed);
            TB_Measurement_Result_Failed.Text = Convert.ToString(Measurement_Result_Failed);

            TB_Analog_Result_Total.Text = Convert.ToString(Analog_Result_Passed + Analog_Result_Failed);
            TB_Analog_Result_Passed.Text = Convert.ToString(Analog_Result_Passed);
            TB_Analog_Result_Failed.Text = Convert.ToString(Analog_Result_Failed);
            ToggleBoardConnection(BoardConnected);
            DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Logg\\"+"Trend\\");
            if (!di.Exists)
                di.Create();

            queueBuff = new QueueBuff();
            buff = new byte[1024];
            dataLen = new byte[2];

            try
            {
                ThreadSP = new Thread(StartThreadSP);
                ThreadSP.Start();
            }
            catch { };
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                sp = (SerialPort)sender;
                int recvSize = sp.BytesToRead;
                if (recvSize != 0)
                {
                    byte[] recvBuff = new byte[recvSize];
                    sp.Read(recvBuff, 0, recvSize);
                    queueBuff.Enqueue(recvBuff, recvSize);
                }
            }catch(Exception eR) { MessageBox.Show("DATA RECEIVED" + eR.Message); }
        }
        public void StartThreadSP()
        {
            try
            {
                while (!IsThreadStop)
                {
                    Thread.Sleep(10);
                    while(queueBuff.head != queueBuff.tail)
                    {
                        ReceivedAction();
                    }
                }
            }
            catch (Exception e) {};
        }

        private void ReceivedAction()
        {
            SerialComm.SetReceived();
            i = 0;
            try
            { 
                if (queueBuff.buff[(queueBuff.tail+i)%QUEUEBUFSIZE] == SerialCommands.STX)
                {
                    i++;
                    Array.Clear(buff, 0, buff.Length);
                    #region MainWindow SerialComm
                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.BOARD_CONNECT_1
                        && queueBuff.buff[(queueBuff.tail + i+1) % QUEUEBUFSIZE] == SerialCommands.BOARD_CONNECT_2)
                    {
                        i += 2;
                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x00)
                        {
                            i++;
                            dataLen[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            dataLen[1]= queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            if (dataLen[0] == 0x00 && dataLen[1] == 0x01)
                            {
                                if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x00)
                                {
                                    i++;
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ETX)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff, queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                            BoardConnected = 0;
                                            Dispatcher.Invoke(new Action(delegate
                                            {
                                                ToggleBoardConnection(BoardConnected);
                                            }));
                                        }
                                    }
                                }
                                else if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x01)
                                {
                                    i++;
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ETX)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff, queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                            BoardConnected = 1;
                                            Dispatcher.Invoke(new Action(delegate
                                            {
                                                ToggleBoardConnection(BoardConnected);
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion MainWindow SerialComm
                    
                    #region BoardConfigurationWindow SerialComm
                    else if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.READ_CONFIG_1
                        && queueBuff.buff[(queueBuff.tail + i+1) % QUEUEBUFSIZE] == SerialCommands.READ_CONFIG_2)
                    {
                        i += 2;
                        int ChannelNo = ConvertUtils.ByteToInt(queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE]);
                        i++;
                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x09)
                        {
                            i++;
                            ConfigData configData = new ConfigData();
                            configData.FREQUENCY = new byte[4];

                            configData.FREQUENCY[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.FREQUENCY[1] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.FREQUENCY[2] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.FREQUENCY[3] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.PGA = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.TH_LEVEL = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.STOP = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.BURST = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            configData.LNA = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x03)
                            {
                                i++;
                                if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff,queueBuff.tail, i))
                                {
                                    i++;
                                    queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                    BCW.ReceivedAction(ChannelNo, configData);
                                }
                            }
                        }
                    }
                    else if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.WRITE_CONFIG_1
                        && queueBuff.buff[(queueBuff.tail + i+1) % QUEUEBUFSIZE] == SerialCommands.WRITE_CONFIG_2)
                    {
                        if (BCW == null)
                        {
                            Logcat_err("허용되지 않은 시리얼 데이터 수신");
                            return;
                        }
                        i += 2;
                        if ((int)queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] >= 1 
                            && (int)queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] <= 5)
                        {
                            i++;
                            if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x00 
                                && queueBuff.buff[(queueBuff.tail + i+1) % QUEUEBUFSIZE] == 0x01)
                            {
                                i += 2;
                                byte result = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ETX)
                                {
                                    i++;
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff, queueBuff.tail, i))
                                    {
                                        i++;
                                        queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                        Dispatcher.Invoke(new Action(delegate
                                        {
                                            switch (result)
                                            {
                                                case 0x00:
                                                    BCW.Logcat_err("DATA 쓰기 에러");
                                                    break;
                                                case 0x01:
                                                    BCW.Logcat_log("DATA 쓰기 성공");
                                                    break;
                                                default:
                                                    BCW.Logcat_err("시리얼 데이터 파싱 에러(DATA)");
                                                    break;
                                            }
                                        }));
                                    }
                                }
                            }
                        }
                    }
                    #endregion BoardConfigurationWindow SerialComm
                    
                    #region MeasurementWindow SerialComm
                    else if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.START_MEASUREMENT_1
                        && queueBuff.buff[(queueBuff.tail + i+1) % QUEUEBUFSIZE] == SerialCommands.START_MEASUREMENT_2) //MEASURE DATA
                    {
                        if (MW == null)
                        {
                            return;
                        }
                        try
                        {
                            i += 2;
                            if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x01)
                            {
                                i += 1;
                                dataLen[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                dataLen[1] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                if (dataLen[0] == 0x00 && dataLen[1] == 0x28)
                                {
                                    for(int j=0; j<40; j++)
                                    {
                                        buff[j] = queueBuff.buff[(queueBuff.tail + i+j) % QUEUEBUFSIZE];
                                    }
                                    i += 40;
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x03)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff, queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                            Dispatcher.Invoke(new Action(delegate
                                            {
                                                MW.ReceivedAction_ValueData(buff);
                                            }));
                                        }
                                    }
                                }
                            }
                            else if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x00)
                            {
                                i ++;
                                dataLen[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                dataLen[1] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                if (dataLen[0] == 0x00 && dataLen[1] == (byte)12)
                                {
                                    for(int j = 0; j<12; j++)
                                    {
                                        buff[j] = queueBuff.buff[(queueBuff.tail + i +j) % QUEUEBUFSIZE];
                                    }
                                    i += 12;
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x03)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff, queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                            Dispatcher.Invoke(new Action(delegate
                                            {
                                                MW.ReceivedAction_SystemVoltage(buff);
                                            }));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e) {
                            MessageBox.Show("비정상에러~" + e.Message);
                        }
                    }
                    #endregion MeasurementWindow SerialComm
                    #region AnalogWindow SerialComm
                    else if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ANALOG_CURRENT_MEASUREMENT_1
                        && queueBuff.buff[(queueBuff.tail + i+1) % QUEUEBUFSIZE] == SerialCommands.ANALOG_CURRENT_MEASUREMENT_2)
                    {
                        if (AW == null)
                        {
                            return;
                        }
                        i += 2;
                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x00)
                        {
                            i++;
                            dataLen[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            dataLen[1] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            if (dataLen[0] == 0x00 && dataLen[1] == 0x05)
                            {
                                byte resType = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                byte[] dataBuff = new byte[4];
                                dataBuff[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                dataBuff[1] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                dataBuff[2] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                dataBuff[3] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                if (resType == 0x00) //STOP
                                {
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ETX)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff,queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                        }
                                    }
                                }
                                else if (resType == 0x01) //START
                                {
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ETX)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff, queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                            Dispatcher.Invoke(new Action(delegate
                                            {
                                                AW.ReceivedAction_Current(dataBuff);
                                            }));
                                            
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ANALOG_PULSE_MEASUREMENT_1
                       && queueBuff.buff[(queueBuff.tail + i+1) % QUEUEBUFSIZE] == SerialCommands.ANALOG_PULSE_MEASUREMENT_2)
                    {
                        if (AW == null)
                        {
                            return;
                        }
                        i += 2;
                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == 0x00)
                        {
                            i++;
                            dataLen[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            dataLen[1] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                            i++;
                            if (dataLen[0] == 0x00 && dataLen[1] == 0x03)
                            {
                                byte resType = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                byte[] dataBuff = new byte[2];
                                dataBuff[0] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                dataBuff[1] = queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE];
                                i++;
                                if (resType == 0x00) //STOP
                                {
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ETX)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff,queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                        }
                                    }
                                }
                                else if (resType == 0x01) //START
                                {
                                    if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == SerialCommands.ETX)
                                    {
                                        i++;
                                        if (queueBuff.buff[(queueBuff.tail + i) % QUEUEBUFSIZE] == Utils.GetBCC(queueBuff.buff,queueBuff.tail, i))
                                        {
                                            i++;
                                            queueBuff.tail = (queueBuff.tail + i) % QUEUEBUFSIZE;
                                            
                                            Dispatcher.Invoke(new Action(delegate
                                            {
                                                AW.ReceivedAction_Pulse(dataBuff);
                                            }));
                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion AnalogWindow SerialComm
                }
                else
                {
                    queueBuff.tail = (queueBuff.tail+ 1)% QUEUEBUFSIZE;
                }
            }catch(Exception e)
            {
                MessageBox.Show("비정상 에러------" + e.Message);
            }
        }

        private void ConnectBoard(int type)//0x01 : connect, 0x00 : disconnect
        {
            byte[] dataBuf = new byte[11];
            i = 0;
            dataBuf[i++] = SerialCommands.STX;
            dataBuf[i++] = SerialCommands.BOARD_CONNECT_1;
            dataBuf[i++] = SerialCommands.BOARD_CONNECT_2;
            dataBuf[i++] = 0x00;
            dataBuf[i++] = 0x00;    //Data Length
            dataBuf[i++] = 0x01;    //Data Length
            dataBuf[i++] = (byte)type;
            dataBuf[i++] = 0x03;
            dataBuf[i++] = Utils.GetBCC(dataBuf,0, i);
            if (!SerialComm.Write(dataBuf))
            {
                MessageBox.Show("응답이 없습니다.");
            }
        }

        private void TogglePortConnection(int PortConnected)
        {
            if (PortConnected == 1)
            {
                ELP_PORT_STATUS.Fill = new SolidColorBrush(Colors.GreenYellow);
                TB_PORT_STATUS.Text = "OPENED";
                Logcat_log("PORT OPENED");
            }
            else
            {
                ELP_PORT_STATUS.Fill = new SolidColorBrush(Colors.Red);
                TB_PORT_STATUS.Text = "CLOSED";
                Logcat_log("PORT CLOSED");
            }
        }

        private void ToggleBoardConnection(int Connected)
        {
            if (Connected == 1)
            {
                AppConfiguration.SetAppConfig("Connected", Convert.ToString(Connected));
                ELP_BOARD_STATUS.Fill = new SolidColorBrush(Colors.GreenYellow);
                TB_BOARD_STATUS.Text = "CONNECTED";
                Logcat_log("BOARD CONNECTED");
            }
            else
            {
                AppConfiguration.SetAppConfig("Connected", Convert.ToString(Connected));
                ELP_BOARD_STATUS.Fill = new SolidColorBrush(Colors.Red);
                TB_BOARD_STATUS.Text = "DISCONNECTED";
                Logcat_log("BOARD DISCONNECTED");
            }
        }

        private void Button_Click_View_Shoot(object sender, RoutedEventArgs e)
        {
            if (PortConnected == 0)
            {
                MessageBox.Show("시리얼 포트를 먼저 오픈해주세요");
                return;
            }
            if (BoardConnected == 0)
            {
                MessageBox.Show("보드를 먼저 연결해주세요");
                return;
            }
            MW = new MeasurementWindow();
            try
            {
                MW.ShowDialog();
            }
            catch(Exception e1) { MessageBox.Show("비정상종료 : " + e1.Message); };
            MW = null;

        }
        private void Button_Click_View_Analog(object sender, RoutedEventArgs e)
        {
            if (PortConnected == 0)
            {
                MessageBox.Show("시리얼 포트를 먼저 오픈해주세요");
                return;
            }
            if (BoardConnected == 0)
            {
                MessageBox.Show("보드를 먼저 연결해주세요");
                return;
            }
            AW = new AnalogWindow();
            try
            {
                AW.ShowDialog();
            }
            catch { };
            AW = null;
        }

        private void Button_Click_View_Trend(object sender, RoutedEventArgs e)
        {
            TW = new TrendWindow();
            try
            {
                TW.ShowDialog();
            }
            catch { };
            TW = null;
        }
        private void Button_Click_View_Config(object sender, RoutedEventArgs e)
        {
            if (PortConnected == 0)
            {
                MessageBox.Show("시리얼 포트를 먼저 오픈해주세요");
                return;
            }
            if (BoardConnected == 0)
            {
                MessageBox.Show("보드를 먼저 연결해주세요");
                return;
            }
            try {
                BCW = new BoardConfigurationWindow();
                BCW.ShowDialog();
            }
            catch(Exception e1) { MessageBox.Show("비정상 종료" + e1.Message); };
            BCW = null;
        }

        private void Button_Click_Board_Connect(object sender, RoutedEventArgs e)
        {
            if (SerialComm.IsInitialized() && SerialComm.IsOpened()) {
                if (BoardConnected == 1)
                    ConnectBoard(0);
                else
                    ConnectBoard(1);
            }
            else
            {
                MessageBox.Show("시리얼 포트를 먼저 오픈해주세요");
            }
            
        }

        private void Button_Click_Port_Connect(object sender, RoutedEventArgs e)
        {
            if (PortConnected == 1)
            {
                SerialComm.Close();
                PortConnected = 0;
                TogglePortConnection(PortConnected);
            }
            else
            {
                SCCW = new SerialCommConnectWindow();
                try
                {
                    SCCW.ShowDialog();
                    if (SCCW.DialogResult.HasValue && SCCW.DialogResult.Value)
                    {
                        
                        
                        if (!SerialComm.IsOpened())
                        {
                            SerialComm.SetReceivedHandler(SerialPort_DataReceived);
                            try
                            {
                                SerialComm.Open();
                                PortConnected = 1;
                                TogglePortConnection(PortConnected);
                            }
                            catch
                            {
                                MessageBox.Show("오류! 포트를 확인하세요");
                                SerialComm.UnInit();
                            }
                        }
                    }
                }
                catch { };
            }
        }

        private void Logcat_log(string log)
        {
            TB_LOGCAT.Text = log;
            TB_LOGCAT.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#007ACC");
        }

        private void Logcat_err(string err)
        {
            TB_LOGCAT.Text = err;
            TB_LOGCAT.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF7E7E");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsThreadStop = true;
        }
    }
}
