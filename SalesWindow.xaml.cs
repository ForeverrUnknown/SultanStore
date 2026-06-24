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

namespace SultanStore
{
    public partial class SalesWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";

        List<CartItem> cart = new List<CartItem>();

        public SalesWindow()
        {
            InitializeComponent();
            LoadProducts();
            UpdateTotal();
        }

        // 💰 ИТОГ
        private void UpdateTotal()
        {
            decimal total = cart.Sum(x => x.Price * x.Quantity);
            totalText.Text = $"Итого: {total.ToString("N0")} ₽";
        }
        private void SearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (gridProducts.ItemsSource is DataView view)
            {
                string filter = searchBox.Text.Replace("'", "''");

                if (string.IsNullOrWhiteSpace(filter))
                    view.RowFilter = "";
                else
                    view.RowFilter = $"Name LIKE '%{filter}%'";
            }
        }

        private void RemoveFromCart(object sender, RoutedEventArgs e)
        {
            if (cartList.SelectedItem == null) return;

            string selected = cartList.SelectedItem.ToString();

            var item = cart.FirstOrDefault(x => selected.Contains(x.Name));

            if (item != null)
                cart.Remove(item);

            UpdateCartUI();
        }

        // 📦 ЗАГРУЗКА ТОВАРОВ
        private void LoadProducts()
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand("SELECT * FROM Products", conn);
                var reader = cmd.ExecuteReader();

                DataTable table = new DataTable();
                table.Load(reader);

                gridProducts.ItemsSource = table.DefaultView;
            }

            searchBox.Text = "";
        }

        // ⌨️ ENTER
        private void SearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearch();
            }
        }

        // 🔘 КНОПКА
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySearch();
        }

        // 🧠 ЛОГИКА ПОИСКА
        private void ApplySearch()
        {
            if (gridProducts.ItemsSource is DataView dv)
            {
                string text = searchBox.Text.Replace("'", "''");

                dv.RowFilter =
                    $"Name LIKE '%{text}%' OR Convert(ProductID, 'System.String') LIKE '%{text}%'";
            }
        }

        // ➕ ДОБАВИТЬ В КОРЗИНУ
        private void AddToCart(object sender, RoutedEventArgs e)
        {
            if (gridProducts.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (!int.TryParse(qtyBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество");
                return;
            }

            var row = (DataRowView)gridProducts.SelectedItem;

            int productId = Convert.ToInt32(row["ProductID"]);
            int stockQty = Convert.ToInt32(row["Quantity"]);

            int alreadyInCart = cart
                .Where(x => x.ProductID == productId)
                .Sum(x => x.Quantity);

            if (qty + alreadyInCart > stockQty)
            {
                MessageBox.Show($"Доступно только {stockQty - alreadyInCart} шт.");
                return;
            }

            cart.Add(new CartItem
            {
                ProductID = productId,
                Name = row["Name"].ToString(),
                Price = Convert.ToDecimal(row["Price"]),
                Quantity = qty
            });

            UpdateCartUI();
            qtyBox.Text = "";
            searchBox.Text = "";
        }

        // 🛒 КОРЗИНА
        private void UpdateCartUI()
        {
            cartList.Items.Clear();

            foreach (var item in cart)
            {
                decimal sum = item.Price * item.Quantity;
                cartList.Items.Add($"{item.Name} x{item.Quantity} = {sum.ToString("N0")} ₽");
            }

            UpdateTotal();
        }

        // 💸 ПРОДАЖА
        private void SellAll(object sender, RoutedEventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        decimal totalSum = cart.Sum(x => x.Price * x.Quantity);

                        var saleCmd = new SqliteCommand(
                            @"INSERT INTO Sales (SaleDate, UserID, TotalAmount) 
                              VALUES (datetime('now','localtime'), 1, @total); 
                              SELECT last_insert_rowid();",
                            conn, transaction);

                        saleCmd.Parameters.AddWithValue("@total", totalSum);
                        long saleID = (long)saleCmd.ExecuteScalar();

                        foreach (var item in cart)
                        {
                            // проверка наличия
                            var checkCmd = new SqliteCommand(
                                "SELECT Quantity FROM Products WHERE ProductID=@id",
                                conn, transaction);

                            checkCmd.Parameters.AddWithValue("@id", item.ProductID);

                            int stock = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (stock < item.Quantity)
                            {
                                MessageBox.Show($"Недостаточно товара: {item.Name}");
                                transaction.Rollback();
                                return;
                            }

                            // добавление в чек
                            var itemCmd = new SqliteCommand(
                                @"INSERT INTO SaleItems (SaleID, ProductID, Quantity, Price) 
                                  VALUES (@sid, @pid, @q, @p)",
                                conn, transaction);

                            itemCmd.Parameters.AddWithValue("@sid", saleID);
                            itemCmd.Parameters.AddWithValue("@pid", item.ProductID);
                            itemCmd.Parameters.AddWithValue("@q", item.Quantity);
                            itemCmd.Parameters.AddWithValue("@p", item.Price);

                            itemCmd.ExecuteNonQuery();

                            // списание
                            var update = new SqliteCommand(
                                "UPDATE Products SET Quantity = Quantity - @q WHERE ProductID=@id",
                                conn, transaction);

                            update.Parameters.AddWithValue("@q", item.Quantity);
                            update.Parameters.AddWithValue("@id", item.ProductID);

                            update.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Ошибка: " + ex.Message);
                        return;
                    }
                }
            }

            // 🧾 ЧЕК
            string receipt = "SULTAN STORE\n\n";

            foreach (var item in cart)
            {
                receipt += $"{item.Name} x{item.Quantity} = {(item.Price * item.Quantity).ToString("N0")} ₽\n";
            }

            receipt += $"\n{totalText.Text}";
            receipt += "\n\nСпасибо за покупку!";

            MessageBox.Show(receipt);

            cart.Clear();
            UpdateCartUI();
            LoadProducts();
        }
    }

    public class CartItem
    {
        public int ProductID;
        public string Name;
        public decimal Price;
        public int Quantity;
    }
}