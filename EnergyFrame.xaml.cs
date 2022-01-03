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
    /// Interaction logic for EnergyFrame.xaml
    /// </summary>
    public partial class EnergyFrame : Page
    {
        MainWindow MW = (MainWindow)Application.Current.MainWindow;
        class DataGridRow
        {
            public string Rate { get; private set; }
            public string Ap { get; private set; } = "";
            public string An { get; private set; } = "";
            public string Rp { get; private set; } = "";
            public string Rn { get; private set; } = "";
            public string P1 { get; private set; } = "";
            public string P2 { get; private set; } = "";
            public string P3 { get; private set; } = "";

            public DataGridRow(string r)
            {
                Rate = r;
            }
            public DataGridRow(ReadStoredEnergyResponse r)
            {
                switch (r.Rate)
                {
                    case Rates.Sum:
                        Rate = "Сумма";
                        break;
                    case Rates.Rate1:
                        Rate = "Тариф 1";
                        break;
                    case Rates.Rate2:
                        Rate = "Тариф 2";
                        break;
                    case Rates.Rate3:
                        Rate = "Тариф 3";
                        break;
                }
                if (r.IsPerPhase)
                {
                    if (r.Phase1 >= 0)
                        P1 = r.Phase1.ToString();
                    if (r.Phase2 >= 0)
                        P2 = r.Phase2.ToString();
                    if (r.Phase3 >= 0)
                        P3 = r.Phase3.ToString();
                }
                else
                {
                    if (r.ActivePositive >= 0)
                        Ap = r.ActivePositive.ToString();
                    if (r.ActiveNegative >= 0)
                        An = r.ActiveNegative.ToString();
                    if (r.ReactivePositive >= 0)
                        Rp = r.ReactivePositive.ToString();
                    if (r.ReactiveNegative >= 0)
                        Rn = r.ReactiveNegative.ToString();
                }
            }
        }
        public EnergyFrame()
        {
            InitializeComponent();

            TotalEnergyDG.Items.Add(new DataGridRow("Тариф 1"));
            TotalEnergyDG.Items.Add(new DataGridRow("Тариф 2"));
            TotalEnergyDG.Items.Add(new DataGridRow("Тариф 3"));
            TotalEnergyDG.Items.Add(new DataGridRow("Сумма"));

            PerPhaseDG.Items.Add(new DataGridRow("Тариф 1"));
            PerPhaseDG.Items.Add(new DataGridRow("Тариф 2"));
            PerPhaseDG.Items.Add(new DataGridRow("Тариф 3"));
            PerPhaseDG.Items.Add(new DataGridRow("Сумма"));
            
            string[] stringMonths = new string[]
            {
                "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };
            MonthsCB.ItemsSource = stringMonths;
            MonthsCB.SelectedIndex = 0;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MonthsCB.IsEnabled = true;
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MonthsCB.IsEnabled = false;
        }

        private void ReadTotalBTN_Click(object sender, RoutedEventArgs e)
        {
            MW.UpdateStatusBar("Чтение данных...");
            Mouse.OverrideCursor = Cursors.Wait;
            Meter Mercury230 = (Meter)App.Current.Properties["Meter"];

            DataArrays da = DataArrays.FromReset;
            Months m = Months.None;
            if ((bool)ResetRB.IsChecked)
                da = DataArrays.FromReset;
            if ((bool)CurrentYearRB.IsChecked)
                da = DataArrays.CurrentYear;
            if ((bool)LastYearRB.IsChecked)
                da = DataArrays.PastYear;
            if ((bool)MonthsRB.IsChecked)
            {
                da = DataArrays.Month;
                if (MonthsCB.Text == "Январь")
                    m = Months.January;
                if (MonthsCB.Text == "Февраль")
                    m = Months.February;
                if (MonthsCB.Text == "Март")
                    m = Months.March;
                if (MonthsCB.Text == "Апрель")
                    m = Months.April;
                if (MonthsCB.Text == "Май")
                    m = Months.May;
                if (MonthsCB.Text == "Июнь")
                    m = Months.June;
                if (MonthsCB.Text == "Июль")
                    m = Months.July;
                if (MonthsCB.Text == "Август")
                    m = Months.August;
                if (MonthsCB.Text == "Сентябрь")
                    m = Months.September;
                if (MonthsCB.Text == "Октябрь")
                    m = Months.October;
                if (MonthsCB.Text == "Ноябрь")
                    m = Months.November;
                if (MonthsCB.Text == "Декабрь")
                    m = Months.December;
            }
            if ((bool)CurrentDayRB.IsChecked)
                da = DataArrays.CurrentDay;
            if ((bool)LastDayRB.IsChecked)
                da = DataArrays.PastDay;

            try
            {
                ReadStoredEnergyResponse rate1 = Mercury230.ReadStoredEnergy(da, m, Rates.Rate1);
                ReadStoredEnergyResponse rate2 = Mercury230.ReadStoredEnergy(da, m, Rates.Rate2);
                ReadStoredEnergyResponse rate3 = Mercury230.ReadStoredEnergy(da, m, Rates.Rate3);
                ReadStoredEnergyResponse sum = Mercury230.ReadStoredEnergy(da, m, Rates.Sum);

                TotalEnergyDG.Items.Clear();
                TotalEnergyDG.Items.Add(new DataGridRow(rate1));
                TotalEnergyDG.Items.Add(new DataGridRow(rate2));
                TotalEnergyDG.Items.Add(new DataGridRow(rate3));
                TotalEnergyDG.Items.Add(new DataGridRow(sum));
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

        private void ReadPerPhaseBTN_Click(object sender, RoutedEventArgs e)
        {
            MW.UpdateStatusBar("Чтение данных...");
            Mouse.OverrideCursor = Cursors.Wait;
            Meter Mercury230 = (Meter)App.Current.Properties["Meter"];

            DataArrays da = DataArrays.PerPhase;
            Months m = Months.None;
            try
            {
                ReadStoredEnergyResponse phase1 = Mercury230.ReadStoredEnergy(da, m, Rates.Rate1);
                ReadStoredEnergyResponse phase2 = Mercury230.ReadStoredEnergy(da, m, Rates.Rate2);
                ReadStoredEnergyResponse phase3 = Mercury230.ReadStoredEnergy(da, m, Rates.Rate3);
                ReadStoredEnergyResponse sum = Mercury230.ReadStoredEnergy(da, m, Rates.Sum);

                PerPhaseDG.Items.Clear();
                PerPhaseDG.Items.Add(new DataGridRow(phase1));
                PerPhaseDG.Items.Add(new DataGridRow(phase2));
                PerPhaseDG.Items.Add(new DataGridRow(phase3));
                PerPhaseDG.Items.Add(new DataGridRow(sum));
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