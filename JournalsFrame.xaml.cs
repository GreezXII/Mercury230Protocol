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
    /// Interaction logic for JournalsFrame.xaml
    /// </summary>
    public partial class JournalsFrame : Page
    {
        class DataGridRow
        {
            public string BeginTime { get; private set; }
            public string EndTime { get; private set; }
           
            public DataGridRow()
            {
                BeginTime = "";
                EndTime = "";
            }
            public DataGridRow(DateTime bt, DateTime et)
            {
                if (bt == new DateTime())
                    BeginTime = "";
                else
                    BeginTime = bt.ToString("g");
                if (et == new DateTime())
                    EndTime = "";
                else
                    EndTime = et.ToString("g");
            }
        }
        MainWindow MW = (MainWindow)Application.Current.MainWindow;
        public JournalsFrame()
        {
            InitializeComponent();

            for (int i = 0; i < 10; i++)
            {
                TimeDG.Items.Add(new DataGridRow());
            }
        }

        private void ReadTotalBTN_Click(object sender, RoutedEventArgs e)
        {
            MW.UpdateStatusBar("Чтение данных...");
            Mouse.OverrideCursor = Cursors.Wait;
            Meter Mercury230 = (Meter)App.Current.Properties["Meter"];

            Journals j = Journals.OnOff;
            if ((bool)OnOffRB.IsChecked)
                j = Journals.OnOff;
            if ((bool)OpeningClosingRB.IsChecked)
                j = Journals.OpeningClosing;
            if ((bool)Phase1OnOffRB.IsChecked)
                j = Journals.Phase1OnOff;
            if ((bool)Phase2OnOffRB.IsChecked)
                j = Journals.Phase2OnOff;
            if ((bool)Phase3OnOffRB.IsChecked)
                j = Journals.Phase3OnOff;
            if ((bool)Phase1CurrentOnOffRB.IsChecked)
                j = Journals.Phase1CurrentOnOff;
            if ((bool)Phase2CurrentOnOffRB.IsChecked)
                j = Journals.Phase2CurrentOnOff;
            if ((bool)Phase3CurrentOnOffRB.IsChecked)
                j = Journals.Phase3CurrentOnOff;

            try
            {
                TimeDG.Items.Clear();
                List<DateTime> records = Mercury230.ReadJournal(j);
                for (int i = 0; i < records.Count; i += 2)
                {
                    TimeDG.Items.Add(new DataGridRow(records[i], records[i + 1]));
                }
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
