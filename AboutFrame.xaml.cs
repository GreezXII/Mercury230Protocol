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

namespace Mercury230Protocol
{
    /// <summary>
    /// Interaction logic for AboutFrame.xaml
    /// </summary>
    public partial class AboutFrame : Page
    {
        MainWindow MW = (MainWindow)Application.Current.MainWindow;
        public AboutFrame()
        {
            InitializeComponent();
        }

        private void ReadInfoBTN_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            MW.UpdateStatusBar("Чтение...");
            try
            {
                Meter Mercury230 = (Meter)App.Current.Properties["Meter"];
                SerialNumberAndReleaseDateResponse response = Mercury230.ReadSerialNumberAndReleaseDate();
                SerialNumberLBL.Content = response.SerialNumber;
                ReleaseDateLBL.Content = response.ReleaseDate.ToString("d");
                SoftVersionLBL.Content = Mercury230.ReadSoftwareVersion();
                MW.UpdateStatusBar("Запрос выполнен");
            }
            catch (ArgumentNullException)
            {
                string message = $"Соединение не установлено или было сброшено, установите соединение заново";
                MW.UpdateStatusBar(message);
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

        private void ReadLocationBTN_Click(object sender, RoutedEventArgs e)
        {
            MW.UpdateStatusBar("Чтение...");
            Mouse.OverrideCursor = Cursors.Wait;
            MW.UpdateStatusBar("Чтение...");
            try
            {
                Meter Mercury230 = (Meter)App.Current.Properties["Meter"];
                LocationTB.Text = Mercury230.GetLocation();
                MW.UpdateStatusBar("Запрос выполнен");
            }
            catch (ArgumentNullException)
            {
                string message = $"Соединение не установлено или было сброшено, установите соединение заново";
                MW.UpdateStatusBar(message);
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

        private void WriteLocationBTN_Click(object sender, RoutedEventArgs e)
        {
            MW.UpdateStatusBar("Чтение...");
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                Meter Mercury230 = (Meter)App.Current.Properties["Meter"];
                Mercury230.SetLocation(LocationTB.Text.Trim());
                MW.UpdateStatusBar("Запрос выполнен");
            }
            catch (ArgumentNullException)
            {
                string message = $"Соединение не установлено или было сброшено, установите соединение заново";
                MW.UpdateStatusBar(message);
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
