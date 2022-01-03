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
                        P1 = r.Phase2.ToString();
                    if (r.Phase3 >= 0)
                        P1 = r.Phase3.ToString();
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
    }
}