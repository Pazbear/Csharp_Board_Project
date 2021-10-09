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
    /// <summary>
    /// SerialNoItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SerialNoItem : UserControl
    {
        public SerialNoItem()
        {
            InitializeComponent();
        }
        public string SerialNo
        {
            get { return Txt_serial_no.Text; }
            set { Txt_serial_no.Text = value; }
        }
    }
}
