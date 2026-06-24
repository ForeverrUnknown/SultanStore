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
    public partial class AddSupplierWindow : Window
    {
        string connectionString = "Data Source=SultanStore.db";

        public AddSupplierWindow()
        {
            InitializeComponent();
        }

        private void SaveSupplier(object sender, RoutedEventArgs e)
        {
            // Проверка
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Введите название");
                return;
            }

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                var cmd = new SqliteCommand(@"
                    INSERT INTO Suppliers
                    (Name, Phone, Email, Address)
                    VALUES
                    (@name,@phone,@email,@address)", conn);

                cmd.Parameters.AddWithValue("@name", nameBox.Text);
                cmd.Parameters.AddWithValue("@phone", phoneBox.Text);
                cmd.Parameters.AddWithValue("@email", emailBox.Text);
                cmd.Parameters.AddWithValue("@address", addressBox.Text);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Поставщик добавлен");

            Close();
        }
    }
}