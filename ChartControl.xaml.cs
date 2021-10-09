using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace project
{
    
    /// <summary>
    /// ChartControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChartControl : UserControl
    {
        public static readonly DependencyProperty TpProperty =
            DependencyProperty.Register("Tp", typeof(string), typeof(ChartControl));

        private int data_index =0;

        public double[] datas =null;
        public double[] timeDatas=null;

        public string Tp
        {
            get { return (string)GetValue(TpProperty); }
            set { SetValue(TpProperty, value); }
        }

        

        public ChartControl()
        {
            InitializeComponent();
        }

        public void InitDataArr(int dataCount)
        {
            datas = new double[dataCount];
            timeDatas = new double[dataCount];
        }

        public bool CheckTrendExist()
        {
            return data_index > 0 ? true : false;
        }

        public void PutData(double data, double timeData)
        {
            datas[data_index] = data;
            timeDatas[data_index] = timeData;
            data_index++;
        }

        public void RenderTrend()
        {
            trendChart.Reset();
            trendChart.plt.Style(ScottPlot.Style.Light1);
            trendChart.Plot.Title(Tp);
            trendChart.Plot.PlotScatter(timeDatas, datas);
            trendChart.Plot.XAxis.DateTimeFormat(true);
            trendChart.Configuration.Zoom = true;
            trendChart.Configuration.ScrollWheelZoom = true;
            trendChart.Configuration.LockVerticalAxis = false;
            trendChart.Render();

            this.DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

        }

    }
}
