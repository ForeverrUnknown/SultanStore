using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WP = DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using DataTable = System.Data.DataTable;
using IOPath = System.IO.Path;

namespace SultanStore
{
    public partial class OrderWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";

        List<OrderItem> order = new List<OrderItem>();
        DataTable productsTable;

        int supplierId;
        string supplierName;

        public OrderWindow(int supId, string supName)
        {
            InitializeComponent();

            supplierId = supId;
            supplierName = supName;

            LoadProducts();
        }

        private void LoadProducts()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand("SELECT * FROM Products", conn);

                productsTable = new DataTable();
                productsTable.Load(cmd.ExecuteReader());

                gridProducts.ItemsSource = productsTable.DefaultView;
            }
        }

        // 🔍 ПОИСК
        private void SearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filter = searchBox.Text.Replace("'", "''");

            productsTable.DefaultView.RowFilter =
                $"Name LIKE '%{filter}%'";
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                productsTable.DefaultView.RowFilter = "";
                return;
            }
        }

        // ➕ ДОБАВИТЬ В ЗАКАЗ
private void AddToOrder(object sender, RoutedEventArgs e)
        {
            if (gridProducts.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (!int.TryParse(qtyBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите количество");
                return;
            }

            var row = (DataRowView)gridProducts.SelectedItem;

            string name = row["Name"].ToString();

            // 🔥 проверка — если уже есть в корзине
            var existing = order.FirstOrDefault(x => x.Name == name);

            if (existing != null)
                existing.Quantity += qty;
            else
                order.Add(new OrderItem { Name = name, Quantity = qty });

            qtyBox.Clear();

            UpdateOrderUI();
        }


private void RemoveItem(object sender, RoutedEventArgs e)
        {
            if (orderList.SelectedItem == null) return;

            string selected = orderList.SelectedItem.ToString();

            var item = order.FirstOrDefault(x => selected.Contains(x.Name));

            if (item != null)
                order.Remove(item);

            UpdateOrderUI();
        }


private void UpdateOrderUI()
        {
            orderList.Items.Clear();

            foreach (var item in order)
            {
                orderList.Items.Add($"{item.Name} x{item.Quantity}");
            }
        }



private void CreateOrder(object sender, RoutedEventArgs e)
    {
        if (order.Count == 0)
        {
            MessageBox.Show("Добавьте товары");
            return;
        }

        try
        {
            // 📂 ПУТЬ НА РАБОЧИЙ СТОЛ
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                string filePath = IOPath.Combine(
                desktop,
                $"Order_{supplierName}_{DateTime.Now:yyyyMMdd_HHmm}.docx"
            );

            using (var doc = WordprocessingDocument.Create(
                filePath,
                DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                var main = doc.AddMainDocumentPart();
                main.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();

                var body = main.Document.AppendChild(new WP.Body());

                // 🏷 Заголовок
                body.Append(new WP.Paragraph(
                    new WP.Run(new WP.Text("ЗАКАЗ ПОСТАВЩИКУ")))
                );

                body.Append(new WP.Paragraph(
                    new WP.Run(new WP.Text($"Поставщик: {supplierName}")))
                );

                body.Append(new WP.Paragraph(
                    new WP.Run(new WP.Text($"Дата: {DateTime.Now}")))
                );

                body.Append(new WP.Paragraph(new WP.Run(new WP.Text(" "))));

                body.Append(new WP.Paragraph(
                    new WP.Run(new WP.Text("Товары:")))
                );

                // 📦 товары
                foreach (var item in order)
                {
                    body.Append(new WP.Paragraph(
                        new WP.Run(
                            new WP.Text($"{item.Name} — {item.Quantity} шт")
                        )
                    ));
                }
            }

            // ✅ УСПЕХ
            MessageBox.Show($"Заказ создан!\n\n{filePath}");

            // 🔥 ОЧИСТКА
            order.Clear();
            UpdateOrderUI();

            // 🔥 ЗАКРЫВАЕМ ОКНО
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка создания файла:\n" + ex.Message);
        }
    }

        class OrderItem
        {
            public string Name;
            public int Quantity;
        }
    }
}