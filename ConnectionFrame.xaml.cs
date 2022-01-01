﻿using System;
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
        MainWindow MW = (MainWindow)Application.Current.MainWindow;
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
                string pwd = MeterPasswordTB.Password;
                if (pwd.Length < 1 || pwd.Length > 6)
                    throw new Exception("Пароль не может быть пустым или больше 6 символов.");
                selectedItem = (ComboBoxItem)WaitTimeCB.SelectedItem;
                // Время ожидания ответа
                int waitTime = int.Parse(selectedItem.Content.ToString());
                // Открыть соединение со счётчиком
                Mouse.OverrideCursor = Cursors.Wait;
                MW.UpdateStatusBar("Проверка физического подключения...");
                Mercury230 = new Meter(addr, comPort, accessLevel, pwd, waitTime);
                if (!Mercury230.TestLink())
                {
                    MW.UpdateStatusBar("Ошибка: проверка физического подключения не пройдена");
                    throw new Exception("Проверка физического подключения не пройдена.");
                }
                MW.UpdateStatusBar("Попытка открыть соединение...");
                if (!Mercury230.OpenConnection())
                {
                    MW.UpdateStatusBar("Ошибка: Не удалось установить соединение");
                    throw new Exception("Не удалось установить соединение.");
                }
                MW.UpdateStatusBar("Соединение открыто");
            }
            catch (TimeoutException)
            {
                MW.UpdateStatusBar("Ошибка: невозможно установить соединение, проверьте настройки для подключения.");
                string message = $"Невозможно установить соединение, проверьте настройки для подключения.";
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception exc)
            {
                string message = $"Во время выполнения возникла следующая ошибка:\n{exc.Message}";
                MW.UpdateStatusBar(exc.Message);
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}
