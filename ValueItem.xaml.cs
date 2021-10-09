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
    /// ValueItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ValueItem : UserControl
    {
        public ValueItem()
        {
            InitializeComponent();
        }
        public string CurrentTime
        {
            get { return Txt_Current_Time.Text; }
            set { Txt_Current_Time.Text = value; }
        }
        public string UpVal
        {
            get { return Txt_Val_Up.Text; }
            set { Txt_Val_Up.Text = value; }
        }

        public string DownVal
        {
            get { return Txt_Val_Down.Text; }
            set { Txt_Val_Down.Text = value; }
        }
    }
}
