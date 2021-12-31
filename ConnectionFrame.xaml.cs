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
    /// Interaction logic for ConnectionFrame.xaml
    /// </summary>
    public partial class ConnectionFrame : Page
    {
        public ConnectionFrame()
        {
            InitializeComponent();
        }

        private void SetComPortRB_Click(object sender, RoutedEventArgs e)
        {
            ConnectionTypeLBL.Content = "Com-порт";
            TCPIPDockPanel.Visibility = Visibility.Hidden;
            ComPortCB.Visibility = Visibility.Visible;
        }

        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            ConnectionTypeLBL.Content = "IP Адресс и порт";
            ComPortCB.Visibility = Visibility.Hidden;
            TCPIPDockPanel.Visibility = Visibility.Visible;
        }

        private void ComPortCB_Loaded(object sender, RoutedEventArgs e)
        {
            string[] comPorts = SerialPort.GetPortNames();
            Array.Sort(comPorts);
            foreach (string s in comPorts)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = s;
                ComPortCB.Items.Add(item);
            }
            ComPortCB.SelectedIndex = 0;
        }
        private void MeterNetworkAddressTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Validators.IsNumber(e))
                e.Handled = true;
        }

        private void IPAddressTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Validators.IsIPAddress(e))
                e.Handled = true;
        }

        private void PortTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Validators.IsNumber(e))
                e.Handled = true;
        }

        private void ConnectBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Meter Mercury230 = (Meter)App.Current.Properties["Meter"];
                // Адрес
                string addrString = MeterNetworkAddressTB.Text;
                if (addrString.Length == 0)
                    throw new Exception("Поле адреса не может быть пустым.");
                byte addr = byte.Parse(MeterNetworkAddressTB.Text);
                // Com порт
                ComboBoxItem selectedItem = (ComboBoxItem)ComPortCB.SelectedItem;
                string comPort = selectedItem.Content.ToString();
                // Уровень доступа
                MeterAccessLevels accessLevel = (MeterAccessLevels)(AccessLevelsCB.SelectedIndex + 1);
                // Пароль
                string pwd = MeterPasswordTB.Text;
                selectedItem = (ComboBoxItem)WaitTimeCB.SelectedItem;
                // Время ожидания ответа
                int waitTime = int.Parse(selectedItem.Content.ToString());
                // Открыть соединение со счётчиком
                Mercury230 = new Meter(addr, comPort, accessLevel, pwd, waitTime);
                Mercury230.TestLink();
                Mercury230.OpenConnection();
            }
            catch (Exception exc)
            {
                string message = $"Во время выполнения возникла следующая ошибка:\n{exc.Message}";
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
