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
    public partial class EditProductWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";
        int productId;

        public EditProductWindow(int id, string name, string price, string qty)
        {
            InitializeComponent();

            productId = id;

           
            nameBox.Text = name;
            priceBox.Text = price;
            qtyBox.Text = qty;
        }

        private void SaveChanges(object sender, RoutedEventArgs e)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand(
                    "UPDATE Products SET Name=@n, Price=@p, Quantity=@q WHERE ProductID=@id", conn);

                cmd.Parameters.AddWithValue("@n", nameBox.Text);
                cmd.Parameters.AddWithValue("@p", priceBox.Text);
                cmd.Parameters.AddWithValue("@q", qtyBox.Text);
                cmd.Parameters.AddWithValue("@id", productId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Изменения сохранены");
            this.Close();
        }
    }
}
