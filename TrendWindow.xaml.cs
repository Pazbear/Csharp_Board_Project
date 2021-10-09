using System;
using System.Collections.Generic;
using System.IO;
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
    /// TrendWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TrendWindow : Window
    {
        private ChartControl[] chChartControls = new ChartControl[5];
        private int[] chTrendResults = new int[5]; //-1: 없음, 0: 있음, 1: PASS, 2: FAILED
        private int trendCnt = 0;
        private bool isRendered=false;
        private StreamReader streamReader;

        public TrendWindow()
        {
            InitializeComponent();
            /*AddSerialNo();
            AddSerialNo();*/
        }

        
        private void AddSerialNo()
        {
            SerialNoItem val = new SerialNoItem();
            val.SerialNo = "USFA08000076";
            ListBox_Serial_Number.Items.Add(val);
        }

        private void DataBindingInit(int dataCount)
        {
            chChartControls[0] = CH1_TREND;
            chChartControls[0].InitDataArr(dataCount);
            chChartControls[1] = CH2_TREND;
            chChartControls[1].InitDataArr(dataCount);
            chChartControls[2] = CH3_TREND;
            chChartControls[2].InitDataArr(dataCount);
            chChartControls[3] = CH4_TREND;
            chChartControls[3].InitDataArr(dataCount);
            chChartControls[4] = CH5_TREND;
            chChartControls[4].InitDataArr(dataCount);

            for(int i=0; i<5; i++)
            {
                chTrendResults[i] = -1;
            }
        }

        private void RenderTrendControl(string filePath)
        {
            streamReader = new StreamReader(new FileStream(filePath, FileMode.Open));
            int dataCount = 0;
            try
            {
                dataCount = Int32.Parse(streamReader.ReadLine().Substring(8));
            }
            catch (FormatException)
            {
                MessageBox.Show($"Unable to parse '{streamReader.ReadLine().Substring(8)}'");
            }
            DataBindingInit(dataCount);
            streamReader.ReadLine();//무시
            while (streamReader.EndOfStream == false)
            {
                TrendLog trendLog = ParseTrendLog(streamReader.ReadLine());
                chChartControls[trendLog.CHNO - 1].PutData(trendLog.UPVALUE - trendLog.DOWNVALUE,
                    new DateTime(trendLog.YEAR, trendLog.MONTH, trendLog.DAY, trendLog.HOUR, trendLog.MINUTE, trendLog.SECOND).ToOADate());
            }
            for(int i=0; i<5; i++)
            {
                if (chChartControls[i].CheckTrendExist())
                {
                    chChartControls[i].RenderTrend();
                    chTrendResults[i] = 0;
                }
            }
            isRendered = true;
            streamReader.Close();
        }

        private TrendLog ParseTrendLog(string log)
        {
            TrendLog trendLog = new TrendLog();
            string[] result = log.Split(new char[] { ',' });
            try
            {
                trendLog.CHNO = Int32.Parse(result[0]);
                trendLog.YEAR = Int32.Parse(result[1]);
                trendLog.MONTH = Int32.Parse(result[2]);
                trendLog.DAY = Int32.Parse(result[3]);
                trendLog.HOUR = Int32.Parse(result[4]);
                trendLog.MINUTE = Int32.Parse(result[5]);
                trendLog.SECOND = Int32.Parse(result[6]);
                trendLog.UPVALUE = double.Parse(result[7]);
                trendLog.DOWNVALUE = double.Parse(result[8]);
            }
            catch(FormatException e)
            {
                MessageBox.Show("로그의 형식에 문제가 있습니다.");
                streamReader.Close();
                Close();
            }
            return trendLog;
        }

        private void Button_Select_File_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ShowFileSelectDialog();

            if(fileName != "")
            {
                MessageBox.Show(fileName);
                RenderTrendControl(fileName);
            }
        }

        private string ShowFileSelectDialog()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "파일 선택";
            ofd.Filter = "로그 파일 (*.log)|*.log";

            ofd.InitialDirectory= AppDomain.CurrentDomain.BaseDirectory + "Logg\\Trend\\";

            System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
            if(dr == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = ofd.SafeFileName;
                string fileFullName = ofd.FileName;
                string filePath = fileFullName.Replace(fileName, "");

                return fileFullName;
            }
            else if(dr == System.Windows.Forms.DialogResult.Cancel)
            {
                return "";
            }
            return "";
        }


        private void BtnResultClicked(Button Clicked, Button UnClicked)
        {
            Clicked.Style = Application.Current.Resources["Clicked"] as Style;
            UnClicked.Style = Application.Current.Resources["UnClicked"] as Style;
        }

        private int CheckAllResultsClicked()
        {
            for(int i=0; i<5; i++)
            {
                if (chTrendResults[i] != -1)
                    switch (chTrendResults[i])
                    {
                        case 0:
                            return 0;
                        case 1:
                            continue;
                        case 2:
                            return 2;
                        default:
                            return 0;
                    }
            }
            return 1;
        }

        private void ProcessLogFile()
        {
            switch (CheckAllResultsClicked())
            {
                case 0:
                    return;
                case 1:
                    //All Pass



                    break;
                case 2:
                    //Failed



                    break;
                default:
                    return;
            }
        }


        private void BTN_CH1_PASS_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if(chTrendResults[0] != -1)
                {
                    chTrendResults[0] = 1;
                }
                BtnResultClicked(BTN_CH1_PASS, BTN_CH1_FAILED);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH1_FAILED_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[0] != -1)
                {
                    chTrendResults[0] = 2;
                }
                BtnResultClicked(BTN_CH1_FAILED, BTN_CH1_PASS);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH2_PASS_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[1] != -1)
                {
                    chTrendResults[1] = 1;
                }
                BtnResultClicked(BTN_CH2_PASS, BTN_CH2_FAILED);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
}

        private void BTN_CH2_FAILED_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[1] != -1)
                {
                    chTrendResults[1] = 2;
                }
                BtnResultClicked(BTN_CH2_FAILED, BTN_CH2_PASS);
                ProcessLogFile();
            }

            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH3_PASS_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[2] != -1)
                {
                    chTrendResults[2] = 1;
                }
                BtnResultClicked(BTN_CH3_PASS, BTN_CH3_FAILED);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH3_FAILED_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[2] != -1)
                {
                    chTrendResults[2] = 2;
                }
                BtnResultClicked(BTN_CH3_FAILED, BTN_CH3_PASS);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH4_PASS_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[3] != -1)
                {
                    chTrendResults[3] = 1;
                }
                BtnResultClicked(BTN_CH4_PASS, BTN_CH4_FAILED);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH4_FAILED_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[3] != -1)
                {
                    chTrendResults[3] = 2;
                }
                BtnResultClicked(BTN_CH4_FAILED, BTN_CH4_PASS);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH5_PASS_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[4] != -1)
                {
                    chTrendResults[4] = 1;
                }
                BtnResultClicked(BTN_CH5_PASS, BTN_CH5_FAILED);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void BTN_CH5_FAILED_Click(object sender, RoutedEventArgs e)
        {
            if (isRendered)
            {
                if (chTrendResults[4] != -1)
                {
                    chTrendResults[4] = 2;
                }
                BtnResultClicked(BTN_CH5_FAILED, BTN_CH5_PASS);
                ProcessLogFile();
            }
            else
                MessageBox.Show("파일을 선택해주세요");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }
}
