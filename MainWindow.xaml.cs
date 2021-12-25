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
                //Mercury230.TestLink();
                //Mercury230.OpenConnection();
                List<TTF> TTFs = new List<TTF>();
                TTFs.Add(new TTF(Rates.Rate2, 0));
                TTFs.Add(new TTF(Rates.Rate1, 7));
                TTFs.Add(new TTF(Rates.Rate3, 9));
                TTFs.Add(new TTF(Rates.Rate1, 11));
                TTFs.Add(new TTF(Rates.Rate3, 18));
                TTFs.Add(new TTF(Rates.Rate1, 20));
                TTFs.Add(new TTF(Rates.Rate2, 22));
                TTFs.Add(new TTF(Rates.Rate1, 24));
                MMSKH mm = new MMSKH();
                mm.October = true;
                WDPM dm = new WDPM();
                dm.Tuesday = true;
                TRECORDH recs = new TRECORDH(TTFs);
                Mercury230.WriteRateSchedule(mm, dm, recs);

                //Mercury230.CloseConnection();
            }
            catch (Exception exc)
            {
                string message = $"Во время выполнения возникла следующая ошибка:\n{exc.Message}";
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
