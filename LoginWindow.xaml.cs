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
    public partial class LoginWindow : Window
    {
        string cs = "Data Source=SultanStore.db";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            using (var conn = new SqliteConnection(cs))
            {
                conn.Open();

                var cmd = new SqliteCommand(
                    "SELECT COUNT(*) FROM Users WHERE Login=@l AND Password=@p", conn);

                cmd.Parameters.AddWithValue("@l", loginBox.Text);
                cmd.Parameters.AddWithValue("@p", passBox.Password);

                long count = (long)cmd.ExecuteScalar();

                if (count > 0)
                {
                    new MainWindow().Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль");
                }
            }
        }
    }
}