using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;

namespace project
{
    /// <summary>
    /// ShootWindow.xaml에 대한 상호 작용 논리
    /// </summary>

    public partial class MeasurementWindow : Window
    {
        private DispatcherTimer taskTimer;
        private StreamWriter trendWriter, logWriter;
        private ListBox[] chListBoxes;

        int receivedDataCnt = 0;
        int timerCnt = 0;
        int timeToStop = 0;
        bool isRunning = false;

        private Nullable<DateTime> lastReceivedTime = null;
        byte[] downData, upData;
        byte[] tdtv, fv, fhv;


        SolidColorBrush logColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#007ACC");
        SolidColorBrush errColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF7E7E");
        SolidColorBrush btnEnableColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#C4FDFF");
        SolidColorBrush btnDisableColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#E8FFFF");

        public MeasurementWindow()
        {
            InitializeComponent();
            
            DataBindingInit();
            TimerInit();
            taskTimer.Stop();
        }

        public void ReceivedAction_ValueData(byte[] dataBuf)
        {
            if (!isRunning)
            {
                StopMeasurement();
                return;
            }
            try
            {
                lastReceivedTime = DateTime.Now;
                string currTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string currTimeToSave = DateTime.Now.ToString("yyyy,MM,dd,HH,mm,ss");
                /*if (Convert.ToString(ConvertUtils.byteArrayToFloat_bigEndian(upData)).Length > 9) MessageBox.Show(ConvertUtils.byteArrayToString(dataBuf, dataBuf.Length));*/
                for (int j = 0; j < 5; j++)
                {
                    Array.Copy(dataBuf, j * 8, downData, 0, 4);
                    Array.Copy(dataBuf, j * 8 + 4, upData, 0, 4);
                    AddCHData(
                        j,
                        currTime,
                        currTimeToSave,
                        Convert.ToString(ConvertUtils.byteArrayToFloat_bigEndian(upData)),
                        Convert.ToString(ConvertUtils.byteArrayToFloat_bigEndian(downData))
                        );
                }
                receivedDataCnt++;
            }catch(Exception e) { MessageBox.Show("비정상 에러"+e.Message); }
        }

        public void ReceivedAction_SystemVoltage(byte[] dataBuf)
        {
            if (!isRunning)
            {
                StopMeasurement();
                return;
            }
            try
            {
                lastReceivedTime = DateTime.Now;
                Array.Copy(dataBuf, 0, tdtv, 0, 4);
                Array.Copy(dataBuf, 4, fv, 0, 4);
                Array.Copy(dataBuf, 8, fhv, 0, 4);
                TB_TDTV.Text = Convert.ToString(ConvertUtils.byteArrayToFloat_bigEndian(tdtv));
                TB_FV.Text = Convert.ToString(ConvertUtils.byteArrayToFloat_bigEndian(fv));
                TB_FHV.Text = Convert.ToString(ConvertUtils.byteArrayToFloat_bigEndian(fhv));
            }
            catch (Exception e1) { MessageBox.Show("비정상 에러1" + e1.Message); }
        }
        private void AddCHData(int idx, string currTime, string timeToSave, string upVal, string downVal)
        {
            ValueItem val = new ValueItem();
            val.CurrentTime = currTime;
            val.UpVal = upVal;
            val.DownVal = downVal;
            chListBoxes[idx].Items.Add(val);
            try
            {
                logWriter.WriteLine(Utils.ToSaveLog(idx, currTime, upVal, downVal));
                trendWriter.WriteLine(Utils.ToTrendData(idx, timeToSave, upVal, downVal));
            }
            catch (Exception e2) { MessageBox.Show("비정상 에러2" + e2.Message); }
            //마지막 인덱스로 자동 스크롤
            chListBoxes[idx].SelectedIndex = chListBoxes[idx].Items.Count - 1;
            chListBoxes[idx].ScrollIntoView(chListBoxes[idx].SelectedItem);
        }

        private void TB_Measure_Time_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((e.Text) == null || !(e.Text).All(char.IsDigit))
            {
                    e.Handled = true;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timerCnt++;
                TB_Measurement_Timer.Text = TimeSpan.FromMilliseconds(timeToStop*1000 - 1000 * timerCnt).ToString(@"hh\:mm\:ss");
                if (timerCnt >= timeToStop)
                {
                    StopMeasurement();
                    isRunning = false;
                    toggleTaskBtn();
                    return;
                }

                if (lastReceivedTime != null)
                {
                    TimeSpan diff = DateTime.Now - (DateTime)lastReceivedTime;
                    if (diff.TotalSeconds > 10)
                    {
                        MessageBox.Show("Response 없음으로 인한 종료");
                        StopMeasurement();
                        isRunning = false;
                        toggleTaskBtn();
                    }
                }
                if (timerCnt % 60 == 0) //1분마다 한번
                {
                    if(logWriter != null)
                    {
                        logWriter.Flush();
                    }
                    if(trendWriter != null)
                    {
                        trendWriter.Flush();
                    }
                }
            }catch(Exception e3)
            {
                MessageBox.Show("비정상 에러3" + e3.Message);
            }
        }



        private void toggleTaskBtn()
        {
            Btn_Measure_Task_Start.IsHitTestVisible = isRunning ? false:true;
            Btn_Measure_Task_Start.Background = isRunning? btnDisableColor : btnEnableColor;
            Btn_Measure_Task_Stop.IsHitTestVisible = isRunning ? true : false;
            Btn_Measure_Task_Stop.Background = isRunning ? btnEnableColor : btnDisableColor;
        }
        private void StartMeasurement()
        {
            for (int j = 0; j < 5; j++) chListBoxes[j].Items.Clear();
            byte[] dataBuf = new byte[10];
            int i = 0;
            dataBuf[i++] = SerialCommands.STX;
            dataBuf[i++] = SerialCommands.START_MEASUREMENT_1;
            dataBuf[i++] = SerialCommands.START_MEASUREMENT_2;
            dataBuf[i++] = 0x00;
            dataBuf[i++] = 0x00;    //Data Length
            dataBuf[i++] = 0x02;    //Data Length
            dataBuf[i++] = 0x01;
            dataBuf[i++] = (byte)(Convert.ToInt32(TB_Measure_Time.Text));
            dataBuf[i++] = 0x03;
            dataBuf[i++] = Utils.GetBCC(dataBuf,0, i);
            if (!SerialComm.WriteClassical(dataBuf))
            {
                Logcat_err("응답이 없습니다.");
                return;
            }
            SP_Measurement_Timer.Visibility = Visibility.Hidden;
            TB_Measurement_Timer.Visibility = Visibility.Visible;
            taskTimer.Start();
            isRunning = true;
        }

        private void StopMeasurement()
        {
            try
            {
                byte[] dataBuf = new byte[10];
                int i = 0;
                dataBuf[i++] = SerialCommands.STX;
                dataBuf[i++] = SerialCommands.START_MEASUREMENT_1;
                dataBuf[i++] = SerialCommands.START_MEASUREMENT_2;
                dataBuf[i++] = 0x00;
                dataBuf[i++] = 0x00;    //Data Length
                dataBuf[i++] = 0x02;    //Data Length
                dataBuf[i++] = 0X00;
                dataBuf[i++] = 0X00;
                dataBuf[i++] = 0x03;
                dataBuf[i++] = Utils.GetBCC(dataBuf,0, i);
                if (!SerialComm.WriteClassical(dataBuf))
                {
                    Logcat_err("응답이 없습니다.");
                    return;
                }
                taskTimer.Stop();
                SP_Measurement_Timer.Visibility = Visibility.Visible;
                TB_Measurement_Timer.Visibility = Visibility.Hidden;
                MessageBox.Show("측정 완료");
            }
            catch (Exception e) { MessageBox.Show("비정상 종료5" + e.Message); }
            finally
            {
                if (logWriter != null)
                {
                    logWriter.Flush();
                    logWriter.Close();
                    logWriter.Dispose();
                    logWriter = null;
                }
                if (trendWriter != null)
                {
                    trendWriter.Flush();
                    trendWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                    trendWriter.Write("Datas : " + Convert.ToString(receivedDataCnt) + "\n");
                    trendWriter.Close();
                    trendWriter.Dispose();
                    trendWriter = null;
                }
            }
        }

        

        private void DataBindingInit()
        {
            chListBoxes = new ListBox[5];

            chListBoxes[0] = CH1ListBox;
            chListBoxes[1] = CH2ListBox;
            chListBoxes[2] = CH3ListBox;
            chListBoxes[3] = CH4ListBox;
            chListBoxes[4] = CH5ListBox;

            downData = new byte[4];
            upData = new byte[4];

            tdtv = new byte[4];
            fv = new byte[4];
            fhv = new byte[4];
        }

        private void TimerInit() {
            taskTimer = new DispatcherTimer();
            taskTimer.Tick += timer_Tick;
            taskTimer.Interval = TimeSpan.FromMilliseconds(1000);
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

        #region 버튼 클릭
        private void Btn_Measure_Task_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int measureTime;
                string logIdx = AppConfiguration.GetAppConfig("LogIdx");
                string logFileFullPath = new StringBuilder()
                    .AppendFormat("{0}{1}{2}{3}{4}{5}",AppDomain.CurrentDomain.BaseDirectory, "Logg\\",
                    "MT",DateTime.Now.ToString("yyyyMMdd"), logIdx,".log").ToString();
                string trendFileFullPath = new StringBuilder()
                    .AppendFormat("{0}{1}{2}{3}{4}{5}{6}", AppDomain.CurrentDomain.BaseDirectory, "Logg\\",
                    "Trend\\" ,"TD" , DateTime.Now.ToString("yyyyMMdd"),logIdx, ".log").ToString();
                AppConfiguration.SetAppConfig("LogIdx", Convert.ToString(int.Parse(logIdx) + 1));
                if (TB_Measure_Time.Text != "" || TB_Measure_Time.Text != "0")
                {
                    measureTime = Convert.ToInt32(TB_Measure_Time.Text);
                }
                else
                {
                    MessageBox.Show("측정시간을 입력해주세요.");
                    return;
                }
                StartMeasurement();
                timeToStop = 3600 * measureTime;

                logWriter = new StreamWriter(new FileStream(logFileFullPath, FileMode.Create));
                trendWriter = new StreamWriter(new FileStream(trendFileFullPath, FileMode.Create));
                trendWriter.Write("Datas : -------------------------------\n");

                isRunning = true;
                toggleTaskBtn();
            }catch(Exception e4) { MessageBox.Show((new StringBuilder()).AppendFormat("{0} {1}", "비정상종료4", e4.Message).ToString()); }
        }

        private void Btn_Measure_Task_Stop_Click(object sender, RoutedEventArgs e)
        {
            StopMeasurement();
            isRunning = false;
            toggleTaskBtn();
        }

        #endregion 버튼 클릭

        private void Window_Closed(object sender, EventArgs e)
        {
            if(isRunning)
                StopMeasurement();
            
        }
    }
}
