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
    public partial class RegisterView : Window
    {
        private readonly UserService _userService;

        public RegisterView(UserService userService)
        {
            InitializeComponent();
            _userService = userService;  // Injected UserService from BLL
        }

        // Handle the register button click
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            // Send data to UserService (BLL) for user creation
            _userService.RegisterUser(name, email, password);

            MessageBox.Show("Registration successful!");
            this.Close(); // Close the register window
            var loginPage = new LoginView(_userService);
            loginPage.Show();
        }
    }
}
