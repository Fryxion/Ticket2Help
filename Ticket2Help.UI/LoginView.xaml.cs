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
using Ticket2Help.BLL.Services;

namespace Ticket2Help.UI
{
    public partial class LoginView : Window
    {
        private readonly UserService _userService;

        public LoginView(UserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }

        // Handle the login button click
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            var user = _userService.Login(email, password);

            if (user != null)
            {
                // Successfully logged in, proceed to the main page or next action
                MessageBox.Show("Login successful!");
                this.Close(); // Close the login window
                // Open the main window or another page
            }
            else
            {
                MessageBox.Show("Invalid email or password.");
            }
        }

        // Navigate to the Register page
        private void RegisterTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var registerPage = new RegisterView(_userService);
            registerPage.Show();
            this.Close(); // Close the login window
        }
    }
}
