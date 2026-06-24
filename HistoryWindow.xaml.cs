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
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using System.Data;
using LiveCharts;
using LiveCharts.Wpf;

namespace SultanStore
{
    public partial class HistoryWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";

        public HistoryWindow()
        {
            InitializeComponent();
            LoadHistory();
            LoadWeekChart();
        }

        // 📦 ЗАГРУЗКА
        private void LoadHistory()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                dayProfit.Text = $"День: {GetSum(conn, "date(SaleDate)=date('now')"):N0} ₽";
                weekProfit.Text = $"Неделя: {GetSum(conn, "date(SaleDate)>=date('now','-7 day')"):N0} ₽";
                monthProfit.Text = $"Месяц: {GetSum(conn, "strftime('%Y-%m',SaleDate)=strftime('%Y-%m','now')"):N0} ₽";

                var cmd = new SqliteCommand("SELECT SaleID, SaleDate, TotalAmount FROM Sales ORDER BY SaleDate DESC", conn);

                DataTable table = new DataTable();
                table.Load(cmd.ExecuteReader());

                gridHistory.ItemsSource = table.DefaultView;
            }
        }

        private decimal GetSum(SqliteConnection conn, string condition)
        {
            var cmd = new SqliteCommand($"SELECT IFNULL(SUM(TotalAmount),0) FROM Sales WHERE {condition}", conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        // 📈 ГРАФИКИ
        private void LoadWeekChart(object sender = null, RoutedEventArgs e = null)
        {
            LoadChart("-6 day");
        }

        private void LoadMonthChart(object sender, RoutedEventArgs e)
        {
            LoadChart("-30 day");
        }

        private void LoadYearChart(object sender, RoutedEventArgs e)
        {
            LoadChart("-365 day");
        }

        private void LoadChart(string period)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand($@"
                SELECT date(SaleDate) as Period,
                       IFNULL(SUM(TotalAmount),0) as Total
                FROM Sales
                WHERE date(SaleDate) >= date('now','{period}')
                GROUP BY Period
                ORDER BY Period", conn);

                var reader = cmd.ExecuteReader();

                var labels = new List<string>();
                var values = new ChartValues<decimal>();

                while (reader.Read())
                {
                    labels.Add(DateTime.Parse(reader["Period"].ToString()).ToString("dd.MM"));
                    values.Add(Convert.ToDecimal(reader["Total"]));
                }

                DrawChart(labels, values);
            }
        }

        // 📅 ФИЛЬТР ПО ДАТАМ
        private void FilterByDate(object sender, RoutedEventArgs e)
        {
            if (dateFrom.SelectedDate == null || dateTo.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты");
                return;
            }

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand(@"
                SELECT date(SaleDate) as Period,
                       IFNULL(SUM(TotalAmount),0) as Total
                FROM Sales
                WHERE date(SaleDate) BETWEEN @from AND @to
                GROUP BY Period
                ORDER BY Period", conn);

                cmd.Parameters.AddWithValue("@from", dateFrom.SelectedDate.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@to", dateTo.SelectedDate.Value.ToString("yyyy-MM-dd"));

                var reader = cmd.ExecuteReader();

                var labels = new List<string>();
                var values = new ChartValues<decimal>();

                while (reader.Read())
                {
                    labels.Add(DateTime.Parse(reader["Period"].ToString()).ToString("dd.MM"));
                    values.Add(Convert.ToDecimal(reader["Total"]));
                }

                DrawChart(labels, values);
            }
        }

        // 🎯 ОТРИСОВКА
        private void DrawChart(List<string> labels, ChartValues<decimal> values)
        {
            salesChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Выручка",
                    Values = values,
                    LineSmoothness = 0.8,
                    PointGeometrySize = 10
                }
            };

            salesChart.AxisX.Clear();
            salesChart.AxisX.Add(new Axis { Labels = labels });

            salesChart.AxisY.Clear();
            salesChart.AxisY.Add(new Axis { Title = "₽" });
        }

        // 🧾 ЧЕК
        private void gridHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridHistory.SelectedItem == null) return;

            var row = (DataRowView)gridHistory.SelectedItem;
            int saleId = Convert.ToInt32(row["SaleID"]);

            ShowSaleItems(saleId);
        }


private void ShowSaleItems(int saleId)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand(@"
        SELECT p.Name, si.Quantity, si.Price
        FROM SaleItems si
        LEFT JOIN Products p ON si.ProductID=p.ProductID
        WHERE si.SaleID=@id", conn);

                cmd.Parameters.AddWithValue("@id", saleId);

                var reader = cmd.ExecuteReader();

                // 🧾 ОКНО
                Window win = new Window
                {
                    Title = $"Чек №{saleId}",
                    Width = 350,
                    Height = 500,
                    Background = new SolidColorBrush(Color.FromRgb(18, 18, 18)),
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var main = new StackPanel
                {
                    Margin = new Thickness(15)
                };

                // 🔥 Заголовок
                main.Children.Add(new TextBlock
                {
                    Text = $"Чек №{saleId}",
                    Foreground = Brushes.Gold,
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 15)
                });

                // 📦 Линия
                main.Children.Add(new System.Windows.Controls.Separator { Margin = new Thickness(0, 5, 0, 10) });

                decimal total = 0;

                // 📦 ТОВАРЫ
                while (reader.Read())
                {
                    string name = reader["Name"].ToString();
                    int qty = Convert.ToInt32(reader["Quantity"]);
                    decimal price = Convert.ToDecimal(reader["Price"]);
                    decimal sum = qty * price;

                    total += sum;

                    var row = new Grid();

                    row.ColumnDefinitions.Add(new ColumnDefinition());
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    // название
                    var nameText = new TextBlock
                    {
                        Text = name,
                        Foreground = Brushes.White,
                        FontSize = 14
                    };

                    // сумма
                    var sumText = new TextBlock
                    {
                        Text = $"{qty} x {price:N0} = {sum:N0} ₽",
                        Foreground = Brushes.White,
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };

                    Grid.SetColumn(nameText, 0);
                    Grid.SetColumn(sumText, 1);

                    row.Children.Add(nameText);
                    row.Children.Add(sumText);

                    main.Children.Add(row);
                }

                // 📦 линия
                main.Children.Add(new System.Windows.Controls.Separator { Margin = new Thickness(0, 5, 0, 10) });

                // 💰 ИТОГ
                main.Children.Add(new TextBlock
                {
                    Text = $"ИТОГО: {total:N0} ₽",
                    Foreground = Brushes.Gold,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold
                });

                // 🙏
                main.Children.Add(new TextBlock
                {
                    Text = "Спасибо за покупку!",
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 15, 0, 0)
                });

                win.Content = main;
                win.ShowDialog();
            }
        }
    }
}