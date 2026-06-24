using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using System.Data;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SultanStore
{
    public partial class SuppliersWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";

        private DataTable suppliersTable;

        public SuppliersWindow()
        {
            InitializeComponent();
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand("SELECT * FROM Suppliers", conn);

                suppliersTable = new DataTable();
                suppliersTable.Load(cmd.ExecuteReader());

                gridSuppliers.ItemsSource = suppliersTable.DefaultView;
            }
        }

        // 🔍 ПОИСК
        private void SearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (suppliersTable == null) return;

            string filter = searchBox.Text.Replace("'", "''");

            suppliersTable.DefaultView.RowFilter =
                $"Name LIKE '%{filter}%' OR Phone LIKE '%{filter}%' OR Address LIKE '%{filter}%'";
        }

        // ➕ ДОБАВИТЬ
        private void OpenAddWindow(object sender, RoutedEventArgs e)
        {
            AddSupplierWindow win = new AddSupplierWindow();

            win.ShowDialog();

            LoadSuppliers();
        }


        // ❌ УДАЛИТЬ
        private void DeleteSupplier(object sender, RoutedEventArgs e)
        {
            if (gridSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика");
                return;
            }

            var row = (DataRowView)gridSuppliers.SelectedItem;
            int id = Convert.ToInt32(row["SupplierID"]);

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand(
                    "DELETE FROM Suppliers WHERE SupplierID=@id", conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            LoadSuppliers();
        }

        // 📄 СОЗДАНИЕ WORD ЗАКАЗА
        private void CreateOrder(object sender, RoutedEventArgs e)
        {
            if (gridSuppliers.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика");
                return;
            }

            var row = (DataRowView)gridSuppliers.SelectedItem;

            int id = Convert.ToInt32(row["SupplierID"]);
            string name = row["Name"].ToString();

            OrderWindow win = new OrderWindow(id, name);
            win.ShowDialog();
        }
    }
}