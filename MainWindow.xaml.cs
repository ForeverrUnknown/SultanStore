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

namespace SultanStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenWarehouse(object sender, RoutedEventArgs e)
        {
            WarehouseWindow window = new WarehouseWindow();
            window.Show();
        }

        private void OpenSales(object sender, RoutedEventArgs e)
        {
            SalesWindow window = new SalesWindow();
            window.Show();
        }

        private void OpenSuppliers(object sender, RoutedEventArgs e)
        {
            SuppliersWindow win = new SuppliersWindow();
            win.ShowDialog();
        }

        private void OpenHistory(object sender, RoutedEventArgs e)
        {
            HistoryWindow window = new HistoryWindow();
            window.Show();
        }
    }
}
