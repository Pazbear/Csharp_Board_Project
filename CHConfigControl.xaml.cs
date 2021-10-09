using System;
using System.Collections.Generic;
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
    public delegate void BtnReadConfigClickHandler(byte ChNo);
    public delegate void BtnWriteConfigClickHandler(byte ChNo);
    /// <summary>
    /// ChConfigControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChConfigControl : UserControl
    {
        public event BtnReadConfigClickHandler BtnReadConfigClick;
        public event BtnWriteConfigClickHandler BtnWriteConfigClick;

        private byte ChNo;
        public ChConfigControl()
        {
            InitializeComponent();
        }

        private void Btn_Read_Config_Click(object sender, RoutedEventArgs e)
        {
            BtnReadConfigClick(ChNo);
        }

        private void Btn_Write_Config_Click(object sender, RoutedEventArgs e)
        {
            BtnWriteConfigClick(ChNo);
        }

        public byte ChannelNo
        {
            get { return ChNo; }
            set { this.ChNo = value; }
        }
    }
}
