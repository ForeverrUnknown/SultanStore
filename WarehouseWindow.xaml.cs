using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace SultanStore
{
    public partial class WarehouseWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";

        public WarehouseWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand("SELECT * FROM Products", conn);
                var reader = cmd.ExecuteReader();

                DataTable table = new DataTable();
                table.Load(reader);

                // 💰 ФОРМАТ ЦЕНЫ
                gridProducts.ItemsSource = table.DefaultView;

                // 💰 формат цены (ТОЛЬКО отображение)
                if (gridProducts.Columns.Count > 2)
                {
                    var priceColumn = gridProducts.Columns[2] as DataGridTextColumn;

                    if (priceColumn != null)
                    {
                        priceColumn.Binding = new Binding("Price")
                        {
                            StringFormat = "{0:N0} ₽"
                        };
                    }
                }

                gridProducts.ItemsSource = table.DefaultView;
            }
        }

        private void SearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearch();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySearch();
        }

        private void ApplySearch()
        {
            if (gridProducts.ItemsSource is DataView dv)
            {
                string text = searchBox.Text.Replace("'", "''");

                dv.RowFilter =
                    $"Name LIKE '%{text}%' OR Convert(ProductID, 'System.String') LIKE '%{text}%'";
            }
        }

        private void SearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (gridProducts.ItemsSource is DataView dv)
            {
                dv.RowFilter =
                    $"Name LIKE '%{searchBox.Text}%'";
                dv.RowFilter =
    $"Name LIKE '%{searchBox.Text}%' OR Convert(ProductID, 'System.String') LIKE '%{searchBox.Text}%'";
            }
        }

        private void AddProduct(object sender, RoutedEventArgs e)
        {
            AddProductWindow window = new AddProductWindow();
            window.ShowDialog();

            LoadProducts(); // обновляем таблицу
        }
        private void DeleteProduct(object sender, RoutedEventArgs e)
        {
            if (gridProducts.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            var row = (gridProducts.SelectedItem as System.Data.DataRowView);

            int id = Convert.ToInt32(row["ProductID"]);

            var result = MessageBox.Show("Удалить товар?", "Подтверждение",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                using (var conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
                {
                    conn.Open();

                    var cmd = new Microsoft.Data.Sqlite.SqliteCommand(
                        "DELETE FROM Products WHERE ProductID=@id", conn);

                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                LoadProducts();
            }
        }
            private void EditProduct(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (gridProducts.SelectedItem == null) return;

            var row = (gridProducts.SelectedItem as System.Data.DataRowView);

            int id = Convert.ToInt32(row["ProductID"]);
            string name = row["Name"].ToString();
            string price = row["Price"].ToString();
            string qty = row["Quantity"].ToString();
           

            
            EditProductWindow window = new EditProductWindow(id, name, price, qty);
            window.ShowDialog();

            LoadProducts();
        }
    }
    
}
