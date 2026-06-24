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

namespace SultanStore
{
    public partial class AddProductWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";

        public AddProductWindow()
        {
            InitializeComponent();
        }

        private void SaveProduct(object sender, RoutedEventArgs e)
        {
            string name = nameBox.Text;
            string price = priceBox.Text;
            string qty = qtyBox.Text;

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                string query = "INSERT INTO Products (Name, Price, Quantity) VALUES (@n,@p,@q)";

                SqliteCommand cmd = new SqliteCommand(query, conn);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@p", price);
                cmd.Parameters.AddWithValue("@q", qty);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Товар добавлен");

            this.Close();
        }
    }
}
