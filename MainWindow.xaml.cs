using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ApplyBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Meter Mercury230 = new Meter(89, MeterAccessLevel.User, "111111");
                Mercury230.TestLink();
                Mercury230.OpenConnection();
                
                
                Mercury230.ReadStoredEnergy(DataArrays.FromReset, Months.None, Rates.Sum);
                Mercury230.ReadSerialNumberAndReleaseDate();
                Mercury230.ReadSoftwareVersion();
                
                
                Mercury230.CloseConnection();
            }
            catch (Exception exc)
            {
                string message = $"Во время выполнения возникла следующая ошибка:\n{exc.Message}";
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
