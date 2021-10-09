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
    /// SerialCommConnectWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SerialCommConnectWindow : Window
    {
        public SerialCommConnectWindow()
        {
            InitializeComponent();
        }

        private void BTN_SERIAL_CONNECT_Click(object sender, RoutedEventArgs e)
        {
            if (CB_SERIALPORT.SelectedItem != null)
            {
                SerialComm.SerialCommInit(CB_SERIALPORT.SelectedItem.ToString());
                this.DialogResult = true;
                Window.GetWindow(this).Close();
            }
        }

        private void CB_SERIALPORT_DropDownOpened(object sender, EventArgs e)
        {
            string[] PortNames = SerialPort.GetPortNames();
            CB_SERIALPORT.Items.Clear();
            foreach (string portName in PortNames)
            {
                CB_SERIALPORT.Items.Add(portName);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] PortNames = SerialPort.GetPortNames();
            if (PortNames.Length == 0)
            {
                this.DialogResult = false;
                Window.GetWindow(this).Close();
            }
        }
    }
}
