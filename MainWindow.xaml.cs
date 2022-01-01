using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
        private static readonly Action EmptyDelegate = delegate { };
        public MainWindow()
        {
            App.Current.Properties["Meter"] = new Meter();
            InitializeComponent();
            MainFrame.Content = new ConnectionFrame();
        }
        public void UpdateStatusBar(string message)
        {
            StatusBar.Text = message;
            StatusBar.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
}
