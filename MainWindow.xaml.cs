using System;
using System.IO.Ports;
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

namespace Mercury230Protocol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort Port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
        public MainWindow()
        {
            InitializeComponent();
            Port.Open();
        }

        private void ApplyBTN_Click(object sender, RoutedEventArgs e)
        {
            string hexValue = hexTXT.Text;
            if (hexValue.Length % 2 != 0)
                throw new Exception("HEX должен быть четным");

            List<byte> buffer = new List<byte>();

            for (int i = 0; i < hexValue.Length; i += 2)
            {
                string hexByte = string.Concat(hexValue[i], hexValue[i + 1]);
                byte b = Convert.ToByte(hexByte, 16);
                buffer.Add(b);
            }
            Com();
        }

        public void Com()
        {
            Frame f = new Frame();
            f.Address = 89;
            f.Body = new byte[] { 0x0 };

            byte[] buffer = f.Create();

            Port.Write(buffer, 0, buffer.Length);
            Port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[4];
            Port.Read(buffer, 0, 4);

            string response = "";
            foreach (byte b in buffer)
                response += Convert.ToString(b, 16) + " ";
            Dispatcher.Invoke(() => ResultLBL.Content = response);
        }
    }
}
